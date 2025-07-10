using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageDefinitions.PushNotification;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class BRCExportPushNotificationMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCExportPushNotificationMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("6E516EC1-EB25-4465-A1B9-8D852D9398E0", "Export Push Notification");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.PUS };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.Export };

		protected override BusinessObject GetLinkedObject(EDIMessage message)
		{
			CusEntryHeader entryHeader = null;

			var dueEvent = BRMessageHelper.DeserializeObject<DueEvent>(message.EM_MessageText);
			var dueNumber = new ZString(dueEvent?.due?.numero);
			if (!dueNumber.IsEmpty)
			{
				entryHeader = new CusEntryHeader.Loader(message.Factory).GetEntryHeaderByMRNQuery(dueNumber, MessageTypeList.Codes.CDE);
				if (entryHeader == null)
				{
					if (message.EM_RetryCount >= 3)
					{
						Logger.LogError($"Message #{message.EM_MessageNum}: Unable to find an Entry with Entry Number '{dueNumber}', retried {message.EM_RetryCount} times.");
					}
					else
					{
						var entryNotFoundMessage = $"Unable to find an Entry with Entry Number '{dueNumber}' for {message.EM_MessageType} message #{message.EM_MessageNum}";
						Logger.LogWarning(entryNotFoundMessage);
						throw new MessageProcessLockException(entryNotFoundMessage);
					}
				}
			}
			else
			{
				Logger.LogError($"Message #{message.EM_MessageNum}: Message deserialization was failed or tag 'due.numero' is empty.");
			}
			return entryHeader;
		}

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			if (message.EM_LinkedObject is CusEntryHeader entryHeader)
			{
				var dueEvent = BRMessageHelper.DeserializeObject<DueEvent>(message.EM_MessageText);
				var dueNumber = new ZString(dueEvent?.due?.numero);
				var pushDate = ZDateTime.TryParseExact(dueEvent.data, out var result, (NoResString)$"{Constants.DataFormat} HH:mm:ss:FFF") ? new ZDateTimeOffset(result) : ZDateTimeOffset.Now;
				entryHeader.AddCustomsUpdateLog(pushDate, dueNumber, description: dueEvent.descricao);

				if (!dueEvent.tipo.IsNullOrEmpty())
				{
					var customsStatus = BRRefCusCodeListTypes.GetCustomsStatusByCustomsCode(message.Factory, dueEvent.tipo, EntryStatusListHelper.ExportEntryStatusPrefix);
					if (!customsStatus.IsEmpty)
					{
						entryHeader.CH_EntryStatus = customsStatus;

						if (customsStatus != Constants.EntryStatus.Registered)
						{
							entryHeader.Logs.AddNew(AutoEvents.CustomsEntryStatus, customsStatus, pushDate);
						}
					}
				}

				new ExportCompleteConsultMessageSender(entryHeader).SendMessage();
			}
		}

		protected override void SetHeldUntilDate(EDIMessage message, IEnumerable<EDIMessage> unprocessedMessages)
		{
			message.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(5);
			message.EM_RetryCount++;
		}
	}
}
