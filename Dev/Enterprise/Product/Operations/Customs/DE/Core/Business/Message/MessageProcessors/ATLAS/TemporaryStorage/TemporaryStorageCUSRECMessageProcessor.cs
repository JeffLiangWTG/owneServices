using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public sealed class TemporaryStorageCUSRECMessageProcessor : TemporaryStorageMessageProcessor<AtlasInboundEDIMessage<ICUSREC>, ICUSREC>
	{
		public TemporaryStorageCUSRECMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("{E55CA6DE-7E31-4DC2-AE9E-5C45284D971A}", "Temporary Storage CUSREC Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ICUSREC> message) => GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier);

		protected override IRegistryItem GetEmailGroupRegistryItem()
			=> linkedObjectCached.STH_MessageStatus == EDIMessageStatusList.Codes.Rejected ? EUCustomsDataRegistry.Instance.SendTemporaryStorageErrors : (IRegistryItem)EUCustomsDataRegistry.Instance.SendTemporaryStorageAcknowledgements;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ICUSREC> message)
		{
			var storageDec = (CusTempStorageDec)message.EM_LinkedObject;
			var dataProvider = message.DataProvider;
			var referenceNumber = dataProvider.ReferenceNumber;
			var mrn = dataProvider.MRN;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			message.SetLogbookRegistrationNumber(new ZString[] { referenceNumber, mrn });
			SetupRegistrationNum(message.EM_MessageSubType, storageDec, referenceNumber ?? mrn);
			var isHeaderError = dataProvider.NotificationSeverity.Any(x => x == NotificationTypeList.Codes.Error);

			if (storageDec.STH_MessageStatus != EDIMessageStatusList.Codes.ProcessedOK)
			{
				storageDec.STH_MessageStatus = isHeaderError ? EDIMessageStatusList.Codes.Rejected : EDIMessageStatusList.Codes.Acknowledged;

				var createTimeUtc = storageDec.Messages.LastSentOutgoingMessage?.EM_SystemCreateTimeUtc ?? ZDateTime.Empty;
				if (createTimeUtc.IsValid && !isHeaderError)
				{
					var missingDecLine = new ZStringBuilder();
					foreach (var goodsItem in dataProvider.GoodsItems)
					{
						var storageLine = storageDec.GetLine(goodsItem.SequenceNumber);
						if (storageLine != null
							&& storageLine.TSL_SystemLastEditTimeUtc <= createTimeUtc
							&& goodsItem.NotificationSeverity == NotificationTypeList.Codes.Information)
						{
							storageLine.TSL_IsModified = ZBool.False;
						}
						else
						{
							missingDecLine.Append(Res.GetString("2de46066-12ea-4ddc-8ccd-10a8be910277", "Sequence Number: {0}", goodsItem.SequenceNumber));
						}
					}
					FinalizeNoteText(missingDecLine, MissingDecLineNoteText);
					factory.CreateStmNoteForEdiMessage(message.PK, missingDecLine.ToStringWithNewLineBetweenAppends());
				}
				SendEmail(message, storageDec);
			}
		}

		void SetupRegistrationNum(ZString messageSubType, CusTempStorageDec dec, ZString referenceNumber)
		{
			if (messageSubType == TemporaryStorageMessageSubTypeList.Codes.ReExport
				|| messageSubType == TemporaryStorageMessageSubTypeList.Codes.SummaryDeclarationAfterPresentation
				|| messageSubType == TemporaryStorageMessageSubTypeList.Codes.PreliminarySummaryDeclaration)
			{
				if (!referenceNumber.IsEmpty)
				{
					if (dec.ReferenceNumber.IsEmpty)
					{
						dec.ReferenceNumber = referenceNumber;
					}
					if (!dec.ReferenceNumberIssueDatePopulated)
					{
						dec.CusEntryNumber.CE_IssueDate = ZDateTime.UtcNow;
					}
				}
			}
		}

		void SendEmail(AtlasInboundEDIMessage<ICUSREC> message, CusTempStorageDec declaration)
		{
			linkedObjectCached = declaration;
			var storageHeader = declaration.StorageHeader;
			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
				, storageHeader
				, Res.GetString("58B3869C-2D35-40DE-81C5-2BAB91F9721F", "SumA CUSREC - Customs Receipt Message")
				, GetEmailBody(storageHeader.SJH_JobReference, message.DataProvider)
				, false
				, message.Branch
				, declaration
				, () => declaration.Messages.LastSentOutgoingMessage);
		}

		static string GetEmailBody(string reference, ICUSREC provider)
		{
			var htmlBody = new StringBuilder();
			htmlBody.Append(Res.GetString("EDAED441-0E26-4362-BDB6-9D1A9773179B", @"Your SumA Declaration for Job {0} received a Customs Response Message. For details please follow the link to the job.", reference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			var localReferenceNumber = provider.LocalReferenceNumber;
			var tableCreator = new HtmlTableCreator();
			var mrn = provider.MRN;
			var referenceNumber = provider.ReferenceNumber;
			if (!string.IsNullOrEmpty(mrn))
			{
				tableCreator.WriteRow(Res.GetString("513AF0AE-B472-4CBE-A0D5-53215DF839A5", "MRN"), mrn);
			}
			if (!string.IsNullOrEmpty(referenceNumber))
			{
				tableCreator.WriteRow(Res.GetString("06DFC5EB-EA48-4CA5-95D7-4BB8B34DBF18", "Registration Number"), referenceNumber);
			}
			if (!localReferenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("1B4D89ED-12A9-444A-BD8A-2259FB1F1379", "Local Reference Number"), localReferenceNumber);
			}
			htmlBody.Append(tableCreator.ToHtml());
			return htmlBody.ToString();
		}

		CusTempStorageDec linkedObjectCached;
	}
}
