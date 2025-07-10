using System.Collections.Generic;
using CargoWise.Customs.BR.MessageDefinitions.Export.Incoming;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class BRCSecondReturnSuccessMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCSecondReturnSuccessMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("2C1661FB-4542-47F4-BF53-7344915AE916", "Second Return Success");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.CDE };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.Success };

		protected override BusinessObject GetLinkedObject(EDIMessage message) => GetLinkedObjectFromOutgoingMessage(message);

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			if (message.EM_LinkedObject is CusEntryHeader entryHeader)
			{
				var messageObject = XmlObjectSerializer.Deserialize<pucomexReturn>(message.EM_MessageText);

				if (messageObject.due == null && messageObject.ruc == null && messageObject.chaveDeAcesso == null)
				{
					message.EM_Status = EDIMessageStatusList.Codes.Failed;

					Logger.LogError($"Message #{message.EM_MessageNum}: Message deserialization was failed.");
				}
				else
				{
					if (!ZDateTime.TryParseExact((ZString)messageObject.date.Split('.')[0], out var issueDate, (NoResString)"yyyy-MM-dd HH:mm:ss"))
					{
						issueDate = ZDateTime.Empty;
					}

					if (!string.IsNullOrEmpty(messageObject.due))
					{
						if (entryHeader.MovementReferenceNumber.IsEmpty)
						{
							entryHeader.MovementReferenceNumberSetter(messageObject.due, issueDate);
						}
						else if (entryHeader.MovementReferenceNumber != messageObject.due)
						{
							Logger.LogError($"The MRN number {entryHeader.MovementReferenceNumber} must be the same as Due number");
							return;
						}
					}

					if (!string.IsNullOrEmpty(messageObject.ruc))
					{
						if (entryHeader.UniqueConsignmentReference.IsEmpty)
						{
							entryHeader.UniqueConsignmentReference = messageObject.ruc;
						}
						else if (entryHeader.UniqueConsignmentReference != messageObject.ruc)
						{
							Logger.LogError($"The UCR number {entryHeader.UniqueConsignmentReference} must be the same as RUC number");
						}
					}

					if (!string.IsNullOrEmpty(messageObject.chaveDeAcesso))
					{
						if (entryHeader.EntryAccessKey.IsEmpty)
						{
							entryHeader.EntryAccessKey = messageObject.chaveDeAcesso;
						}
						else if (entryHeader.EntryAccessKey != messageObject.chaveDeAcesso)
						{
							Logger.LogError($"The Entry Access Key {entryHeader.EntryAccessKey} must be the same as Access Key");
						}
					}

					message.EM_MessageInterpretation = new ExportSuccessMessagePrettyFormatter(entryHeader, messageObject).GetFormattedMessageText();
					entryHeader.CH_Status = BRMessageStatusList.Codes.Accepted;
					entryHeader.CH_EntryStatus = Constants.EntryStatus.Registered;

					entryHeader.Logs.AddNew(AutoEvents.CustomsEntryStatus, entryHeader.CH_EntryStatus, issueDate.ToOffset());
				}
			}
		}
	}
}
