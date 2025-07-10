using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.eHub;
using Enterprise.ZArchitecture;

namespace Enterprise.eHubMessaging.ServiceTasks.Outbound.Scavenging
{
	abstract class ScavengingSubmissionJob<TOutboundItem, TLightweightOutboundItem> : OutboundServiceTaskJob<TOutboundItem, TLightweightOutboundItem>
		where TOutboundItem : BusinessObject
		where TLightweightOutboundItem : LightweightOutboundItem<TOutboundItem>
	{
		protected ScavengingSubmissionJob(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier)
			: base(serviceTaskSupport, notifier)
		{
		}

		protected ScavengingSubmissionJob(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier, IAdaptorFactory adaptorFactory)
			: base(serviceTaskSupport, notifier, adaptorFactory)
		{
		}

		internal virtual ScavengingTaskSettingsManager CreateSettingsManager()
		{
			return new ScavengingTaskSettingsManager();
		}

		internal override void ExecuteInternal()
		{
			try
			{
				var company = base.CompaniesThatShouldBeServiced.OrderBy(c => c.GC_Code).FirstOrDefault();
				if (company != null)
				{
					CurrentCompany = company;
					NotifyExecutingForCompany();
					ProcessMessages();
				}
			}
			catch (Exception ex) when (HandleCompanyLevelException(ex))
			{
			}
		}

		internal override void ProcessMessagesCore()
		{
			var pendingItems = GetPendingItems();
			NotifyCandidateCount(pendingItems.Count);
			if (pendingItems.Count <= 0)
			{
				return;
			}

			var items = ValidatePendingItems(pendingItems);
			NotifyOutboundCount(items.Count);
			if (items.Count > 0)
			{
				SendBatch(items);
			}

			OnAtLeastOneMessageProcessed();
		}

		internal abstract IReadOnlyCollection<TLightweightOutboundItem> GetPendingItems();

		internal sealed override IReadOnlyList<KeyValuePair<TLightweightOutboundItem, IeHubMessage>> CreateMessages(IReadOnlyCollection<TLightweightOutboundItem> items)
		{
			var messages = new List<KeyValuePair<TLightweightOutboundItem, IeHubMessage>>();

			foreach (var item in items)
			{
				var stream = SerializeToStream(item);
				if (stream != null)
				{
					var message = CreateMessage(stream, item);
					messages.Add(new KeyValuePair<TLightweightOutboundItem, IeHubMessage>(item, message));
				}
			}

			return messages.AsReadOnly();
		}

		protected override void AddMessageToOutbox(IMessageOutbox outbox, KeyValuePair<TLightweightOutboundItem, IeHubMessage> messagePair)
		{
			base.AddMessageToOutbox(outbox, messagePair);
			messageToItemMap.Add(messagePair.Value.TrackingID, messagePair.Key.FullItem);
		}

		protected override void SaveSendCore(IEnumerable<KeyValuePair<TLightweightOutboundItem, IeHubMessage>> messages)
		{
			foreach (var fault in Faults)
			{
				if (messageToItemMap.TryGetValue(fault.Key, out var item))
				{
					HandleFaultyItem(item, fault.Value);
				}
				else
				{
					ErrorReporter.ReportOnce("Invalid message tracking ID", string.Format("Could not find an item corresponding to the error '{0}'", fault.Value));
				}
			}

			var itemsToDelete = messageToItemMap.Where(kv => !Faults.ContainsKey(kv.Key)).Select(kv => kv.Value);
			itemsToDelete.ForEach(item => item.Delete());
			Factory.Save();
		}

		protected override void FinalizeSend(IeHubAdapter adapter)
		{
			messageToItemMap.Clear();
			base.FinalizeSend(adapter);
		}

		internal override void CallAdapterToSendMessages(IeHubAdapter adapter)
		{
			try
			{
				var messageCount = adapter.Outbox.Count;
				adapter.SendMessages();
				Notifier.Notify(new InfoNotification(Res.GetString("29379e4e-0667-4fa8-98fa-3c2f58408626", "{0} message(s) sent.", messageCount)));
			}
			catch (eHubAdapterException e)
			{
				var messageExceptionDictionary = e.GetMessageExceptionDictionary();
				if (messageExceptionDictionary != null)
				{
					messageExceptionDictionary.ForEach(x => Faults.Add(x.Key, x.Value));
				}
				else
				{
					string errorMessage = string.Empty;
					if (IsIndividualMessageExceedingMaxReceiveError(e, adapter.Outbox, ref errorMessage))
					{
						var ehubMessage = adapter.Outbox.First();
						Faults.Add(ehubMessage.TrackingID, string.Join(" ", messageToItemMap[ehubMessage.TrackingID].GetType().Name, messageToItemMap[ehubMessage.TrackingID].PK, errorMessage));
					}
					else
					{
						throw;
					}
				}
			}
		}

		protected ScavengingSetting LoadSettings()
		{
			SettingsManager.Load();
			return SettingsManager.GetScavengingTaskSettings(JobName);
		}

		protected void SaveSettings()
		{
			SettingsManager.Save();
		}

		protected BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = CreateFactory()); }
		}

		protected abstract string JobName { get; }

		protected abstract string GetSchemaName(TLightweightOutboundItem item);

		internal virtual BusinessObjectFactory CreateFactory()
		{
			return new BusinessObjectFactory(DbConnection);
		}

		protected virtual void HandleFaultyItem(TOutboundItem item, string error)
		{
		}

		protected abstract Stream SerializeToStream(TLightweightOutboundItem item);

		ScavengingTaskSettingsManager SettingsManager
		{
			get { return settingsManager ?? (settingsManager = CreateSettingsManager()); }
		}

		eHubMessage CreateMessage(Stream stream, TLightweightOutboundItem item)
		{
			return new eHubMessage(Guid.NewGuid(), CurrentCompany.LicenceKeyIdentifier, RecipientId, MessageSchemaType.Xml, ApplicationCode, GetSchemaName(item), stream);
		}

		protected const string ApplicationCode = "SCV";
		internal const string RecipientId = "eHubASService";

		ScavengingTaskSettingsManager settingsManager;
		BusinessObjectFactory factory;
		readonly Dictionary<Guid, TOutboundItem> messageToItemMap = new Dictionary<Guid, TOutboundItem>();
	}
}
