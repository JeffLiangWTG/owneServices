using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Business.MessageProcessor
{
	public class ObsoleteMessageProcessorProxy
	{
		public ObsoleteMessageProcessorProxy(OutgoingMessageProcessor originalMessageProcessor)
		{
			originalProcessor = Argument.NotNull(originalMessageProcessor, nameof(originalMessageProcessor));
		}

		readonly OutgoingMessageProcessor originalProcessor;

		public void Process(CancellationToken token, string status, TimeSpan timeSpan)
		{
			Argument.NotNullOrEmpty(status, nameof(status));
			Argument.NotNull(timeSpan, nameof(timeSpan));

			var messagePKsNotSaved = new Dictionary<ZGuid, byte>();
			var queuedMessagesToProcessCount = 0;

			do
			{
				token.ThrowIfCancellationRequested();

				var factory = originalProcessor.CreateFactory();

				var query = originalProcessor.GetMessagesToProcessQuery(false);
				query.MaximumRows = null;

				var createTime = ZDateTime.UtcNow.Add(-timeSpan);
				query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, createTime);

				var legacyMessages = new NonDependentEDIMessageCollection(factory);
				legacyMessages.Load(query);

				var logger = originalProcessor.Logger;
				var messagePKsFailSaving3Times = new List<ZGuid>();

				queuedMessagesToProcessCount = legacyMessages.Count;

				if (queuedMessagesToProcessCount > 0)
				{
					try
					{
						foreach (EDIMessage message in legacyMessages)
						{
							message.EM_Status = status;
							message.Notes.AddNew(true, InterchangeProviderBase.ProcessingLogDescription, Res.GetString("EE16E1CC-8393-4280-B560-F6BE91CB4C91", "It's marked as a legacy message because it was created before {0}.", createTime));
						}

						factory.Save();

						legacyMessages.Cast<EDIMessage>().Select(x => x.PK).ForEach(x => messagePKsNotSaved.Remove(x));
						logger?.Log(queuedMessagesToProcessCount.ToString(CultureInfo.InvariantCulture) + $" legacy message(s) have been processed, the new status is '{status}'.");
					}
					catch (ZSaveException e)
					{
						ZExceptionReporting.HandleSaveException(e);

						legacyMessages.Cast<EDIMessage>().Select(x => x.PK).ForEach(x =>
						{
							if (messagePKsNotSaved.TryGetValue(x, out var value))
							{
								if (++value == 3)
								{
									messagePKsNotSaved.Remove(x);
									messagePKsFailSaving3Times.Add(x);
								}
								else
								{
									messagePKsNotSaved[x] = value;
								}
							}
							else
							{
								value = 1;
								messagePKsNotSaved.Add(x, value);
							}
						});
					}
				}

				MarkMessagesFailToSave(messagePKsFailSaving3Times);
			}
			while (queuedMessagesToProcessCount > 0);
		}

		void MarkMessagesFailToSave(List<ZGuid> messagePKsFailSaving3Times)
		{
			if (messagePKsFailSaving3Times.Count > 0)
			{
				var factory = new BusinessObjectFactory();
				factory.NameForDebugging = "Legacy Outgoing Message Fail Saving";
				factory.RefreshEnabled = false;

				foreach (var message in factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.PK, messagePKsFailSaving3Times)))
				{
					message.EM_Status = EDIMessage.Status.Failed;
					message.Notes.AddNew(true, InterchangeProviderBase.ProcessingLogDescription, Res.GetString("ED906A5D-5A67-43A1-84BE-6F0C7AC52154", "Legacy Message fail to saving 3 times."));
				}

				try
				{
					factory.Save();
				}
				catch (ZSaveException e)
				{
					ZExceptionReporting.HandleSaveException(e);
				}

				messagePKsFailSaving3Times.Clear();
			}
		}
	}
}
