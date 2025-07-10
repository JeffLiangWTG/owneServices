using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	abstract class OutboundServiceTaskJob<TOutboundItem, TLightweightOutboundItem> : ServiceTaskJobWithAdapter
		where TOutboundItem : BusinessObject
		where TLightweightOutboundItem : LightweightOutboundItem<TOutboundItem>
	{
		protected OutboundServiceTaskJob(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier, IAdaptorFactory adaptorFactory = null)
					: base(serviceTaskSupport, notifier, adaptorFactory)
		{
		}

		internal virtual int AdapterOutboxCountLimit { get { return 1; } }

		internal virtual IReadOnlyCollection<TLightweightOutboundItem> ValidatePendingItems(IReadOnlyCollection<TLightweightOutboundItem> itemsToValidate)
		{
			if (!PreProcessingCheckPassed())
			{
				FailInterchanges(itemsToValidate.Select(i => i.FullItem));
				return Array.Empty<TLightweightOutboundItem>();
			}

			return itemsToValidate;
		}

		protected void SaveSend(IEnumerable<KeyValuePair<TLightweightOutboundItem, IeHubMessage>> messages)
		{
			ZExceptionReporting.ProcessWithSaveExceptionHandling(() => SaveSendCore(messages), null, reportErrorsOnly: true);
		}

		protected abstract void SaveSendCore(IEnumerable<KeyValuePair<TLightweightOutboundItem, IeHubMessage>> messages);

		protected virtual void FinalizeSend(IeHubAdapter adapter)
		{
			adapter.Outbox.Clear();
			faults = null;
		}

		internal abstract void CallAdapterToSendMessages(IeHubAdapter adapter);

		protected bool IsExceedingSendingByteLimit(long eHubMessageSize, long batchSize)
		{
			var isOutboxSizeLimitReached = batchSize + eHubMessageSize > AdapterOutboxSizeLimitInBytes;

			if (ServiceAdapterType == AdapterType.GatewayAdapter)
			{
				var isGatewaySizeLimitReached = batchSize + eHubMessageSize > GatewayMaxReceivedMessageLimitInBytes;
				return isOutboxSizeLimitReached || isGatewaySizeLimitReached;
			}

			return isOutboxSizeLimitReached;
		}

		protected bool IsIndividualMessageExceedingMaxReceiveError(eHubAdapterException exception, IMessageOutbox outbox, ref string messageError)
		{
			var regex = new Regex(@"The maximum message size quota for incoming messages \((\d+)\) has been exceeded"); // Not ideal to use exception's message, but no other option.
			if (regex.IsMatch(exception.Message) && outbox.Count == 1)
			{
				var maxReceiveSize = regex.Match(exception.Message).Groups[1].Value;
				messageError = $"prepared outgoing message larger than the maximum receive size for the outbound service ({maxReceiveSize} bytes).";
				return true;
			}

			return false;
		}

		internal virtual int FillOutbox(IMessageOutbox outbox, IReadOnlyList<KeyValuePair<TLightweightOutboundItem, IeHubMessage>> messages, int position)
		{
			long eHubMessageBytesInBatch = 0;
			while (position < messages.Count)
			{
				var messagePair = messages[position];
				ThrowIfCancellationRequested();
				NotifyVerbose(Res.GetString("c8188e7c-3fe9-4a48-9f03-90badee8248f", "Preparing message for item PK {0}", messagePair.Key.PK.ToString()));

				var eHubMessageSize = messagePair.Value.MessageStream.Length;
				if (IsExceedingSendingByteLimit(eHubMessageSize, eHubMessageBytesInBatch))
				{
					if (eHubMessageBytesInBatch == 0)
					{
						AddMessageToOutbox(outbox, messagePair);
						position++;
						break;
					}

					NotifyVerbose(Res.GetString("22cfb266-e6b5-4551-880e-b23d8c9ae158", "'{0}' item has a prepared outgoing message, which causes the current batch({1} bytes) larger than out box size limit({2} bytes) or gateway size limit({3} bytes). This item will be sent in the next batch.", messagePair.Key.PK.ToString(), eHubMessageBytesInBatch, AdapterOutboxSizeLimitInBytes, GatewayMaxReceivedMessageLimitInBytes));
					break;
				}

				AddMessageToOutbox(outbox, messagePair);
				eHubMessageBytesInBatch += eHubMessageSize;
				position++;
			}
			NotifyVerbose(Res.GetString("2f2e4452-a218-4fcd-89a4-6812fd844183", "Prepared messages for {0} items", outbox.Count));
			return position;
		}

		protected virtual void ValidateDynamicProperties()
		{
		}

		protected virtual void AddMessageToOutbox(IMessageOutbox outbox, KeyValuePair<TLightweightOutboundItem, IeHubMessage> messagePair)
		{
			outbox.AddMessage(messagePair.Value);
		}

		internal virtual void SendBatch(IReadOnlyCollection<TLightweightOutboundItem> items)
		{
			ThrowIfCancellationRequested();
			using (var adapter = CreateAdapter())
			{
				ValidateDynamicProperties();

				int startPosition = 0;
				var messages = CreateMessages(items);

				while (startPosition < messages.Count)
				{
					NotifyVerbose(Res.GetString("86a2520f-e691-4452-b8af-01d8507fbcea", "Calling Send Messages ()"));
					var nextStartPosition = FillOutbox(adapter.Outbox, messages, startPosition);
					try
					{
						if (adapter.Outbox.Count > 0)
						{
							ThrowIfCancellationRequested();
							CallAdapterToSendMessages(adapter);
						}

						var messagesToSave = messages.Skip(startPosition).Take(nextStartPosition - startPosition).ToList();
						SaveSend(messagesToSave);
						startPosition = nextStartPosition;
						NotifyVerbose(Res.GetString("d8019686-c85c-4e2c-9252-acd22bba1bc9", "Batch Sent by adapter."));
					}
					finally
					{
						FinalizeSend(adapter);
					}
				}
			}
		}

		internal abstract IReadOnlyList<KeyValuePair<TLightweightOutboundItem, IeHubMessage>> CreateMessages(IReadOnlyCollection<TLightweightOutboundItem> items);

		internal override bool PreProcessingCheckPassed()
		{
			if (!base.PreProcessingCheckPassed())
			{
				return false;
			}

			if (CurrentCompany.IsDemoCompany)
			{
				Notifier.Notify(new WarningNotification(Res.GetString("69878F6F-6B4C-4667-8351-A4047D1B45D6", "Cannot process messages from demo company [{0}]", CurrentCompany.PK)));
				return false;
			}

			if (!CompanyShouldBeServiced(CurrentCompany))
			{
				return false;
			}

			return true;
		}

		protected void NotifyCandidateCount(int candidateCount)
		{
			NotifyVerbose(Res.GetString("3f72e3fd-5595-4a52-8b57-83505855b8ae", "Candidate count: {0}", candidateCount));
		}

		protected void NotifyOutboundCount(int outboundCount)
		{
			NotifyVerbose(Res.GetString("31BE8D83-595B-4215-B9D1-F7A78C332BFB", "Outbound count: {0}", outboundCount));
		}

		internal virtual int AdapterOutboxSizeLimitInBytes { get { return eAdaptorFactory.AdapterOutboxSizeLimit * 1024; } }

		Dictionary<Guid, string> faults;
		protected Dictionary<Guid, string> Faults
		{
			get
			{
				faults = faults ?? new Dictionary<Guid, string>();
				return faults;
			}
		}

		internal virtual long GatewayMaxReceivedMessageLimitInBytes => Constants.GatewayMaxReceivedMessageSize;

		protected virtual void FailInterchanges(IEnumerable<TOutboundItem> items) { }

		internal static class Constants
		{
			internal const long GatewayMaxReceivedMessageSize = 104857600;
		}
	}
}
