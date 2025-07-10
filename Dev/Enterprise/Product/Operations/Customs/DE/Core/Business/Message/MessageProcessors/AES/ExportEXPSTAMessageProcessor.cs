using System.Linq;
using System.Text;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ExportEXPSTAMessageProcessor : ExportMessageProcessor<AesInboundEDIMessage<IEXPSTA>, IEXPSTA>
	{
		public ExportEXPSTAMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("C89F4BB3-5FB7-4A68-9057-F8FD1F7F0CE6", "Export EXPSTA Message Processor");

		protected override BusinessObject GetLinkedObject(AesInboundEDIMessage<IEXPSTA> message)
		{
			BusinessObject linkedObject = null;
			var factory = message.Factory;
			var dataProvider = message.DataProvider;
			if (dataProvider != null)
			{
				linkedObject = GetLinkedObjectFromOriginalMessage(factory, dataProvider.ReferencedMessageIdentifier)
					?? GetEntryHeaderFromMRN(factory, dataProvider.MovementReferenceNumber)
					?? GetEntryHeaderFromOriginalMessageViaLRNAndCustomsOfficeOfExport(factory, dataProvider.LocalReferenceNumber, dataProvider.CustomsOfficeOfExport);
			}
			return linkedObject;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AesInboundEDIMessage<IEXPSTA> message)
		{
			var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			var dataProvider = message.DataProvider;

			if (dataProvider.ExportStatus == "520" && entryHeader.CH_ExitedStatus != ExportExitStatus.Codes.ExitedSatisfactorily)
			{
				entryHeader.CH_ExitedStatus = ExportExitStatus.Codes.UnknownOrNotReported;
			}

			entryHeader.MovementReferenceNumberSetter(dataProvider.MovementReferenceNumber);
			entryHeader.CH_EntryStatus = dataProvider.ExportStatus;
			entryHeader.AddCustomsEntryStatusLog(entryHeader.CH_EntryStatus);

			message.SetLogbookRegistrationNumber(dataProvider.MovementReferenceNumber);
			message.SetLogbookLocalReferenceNumber(dataProvider.LocalReferenceNumber);

			SendEmailIfNeeded(message, entryHeader, dataProvider.ReferencedMessageIdentifier);

			DoCancelWarehouseIfNeeded(entryHeader);
		}

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo)
		{
			var email = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			emailSubjectSuffixProvider.SetEmailSubjectSuffix(email);
			return email;
		}

		DeclarationEmailSubjectSuffixProvider emailSubjectSuffixProvider;

		void SendEmailIfNeeded(AesInboundEDIMessage<IEXPSTA> message, CusEntryHeader entryHeader, ZString referencedMessageIdentifier)
		{
			var declaration = entryHeader.Declaration;
			if (declaration != null)
			{
				emailSubjectSuffixProvider = new DeclarationEmailSubjectSuffixProvider(entryHeader);
				GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory,
					declaration
					, Res.GetString("E2741657-9F44-46F2-BE26-54917989863D", "AES EXP Status Message")
					, GetEmailBody(entryHeader, declaration)
					, false
					, message.Branch
					, declaration
					, referencedMessageIdentifier);
			}
		}

		ZString GetEmailBody(CusEntryHeader entryHeader, JobDeclaration declaration)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("9AF81087-8DA0-42F3-8AAF-C360C9CC1F79",
				"Your Export Declaration Message for {0} has a Status Message. For details please follow the Link to the Job",
				declaration.JE_DeclarationReference));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("6A258D0B-9DB9-4603-9102-848E4BBF4DF5", "MRN:"), entryHeader.MovementReferenceNumber);
			tableCreator.WriteRow(Res.GetString("A14487D0-9BCB-43B0-8A75-B740385895B2", "Status:"), entryHeader.CH_EntryStatus);
			tableCreator.WriteRow(Res.GetString("DB1F6FA9-B91A-44E0-9877-2AF39FDFDB7F", "Status Text:"), entryHeader.EntryHeaderStatusDescription);
			htmlBody.Append(tableCreator.ToHtml());

			return htmlBody.ToString();
		}

		void DoCancelWarehouseIfNeeded(CusEntryHeader entryHeader)
		{
			if ((entryHeader.CH_EntryStatus == "119" || entryHeader.CH_EntryStatus == "191" || entryHeader.CH_EntryStatus == "520") && entryHeader.SupportsBondedWarehousing)
			{
				if (entryHeader.IsExport && entryHeader.IsOutOfWarehouseWarehousing)
				{
					entryHeader.Factory.Saved -= CancelWarehouse;
					entryHeader.Factory.Saved += CancelWarehouse;
				}
			}

			void CancelWarehouse(BusinessObjectFactory factory, bool savedSuccessfully)
			{
				if (savedSuccessfully)
				{
					factory.Saved -= CancelWarehouse;
				}

				if (entryHeader != null)
				{
					var result = entryHeader.PublishCancelEventForWHSOutwardAndSaveIfNeeded();
					if (result.ResultType == UniversalResult.HadErrors)
					{
						Logger.LogWarning(result.ErrorMessage);
					}
				}
				factory.Save();
			}
		}

		CusEntryHeader GetEntryHeaderFromOriginalMessageViaLRNAndCustomsOfficeOfExport(BusinessObjectFactory factory, string lrn, string customsOfficeOfExport)
		{
			CusEntryHeader result = null;
			var groupedRelatedMessages = Enumerable.Empty<IGrouping<ZGuid, EDIMessage>>();
			if (!string.IsNullOrWhiteSpace(lrn) && !string.IsNullOrWhiteSpace(customsOfficeOfExport))
			{
				var query = new ZDBOnlyQuery(typeof(EDIMessage));
				query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.DECustomsAesSystem);
				query.AddToFilter(EDIMessageSchema.EM_ApplicationReference, SQLComparisonOperator.StartsWith, nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDF).Substring(0, 5));
				query.AddToFilter(EDIMessageSchema.EM_MessageType, Messaging.EDIMessageTypeList.Codes.AES);
				query.AddToFilter(EDIMessageSchema.EM_MessageSubType, MessageTypeList.Codes.Export);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit,ReceiveTransmitList.Codes.Transmit);
				query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddMonths(-1));

				var subQueryEntryHeader = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.PK);
				subQueryEntryHeader.AddToFilter(CusEntryHeaderSchema.CH_Status, Common.Shared.MessageStatusList.Codes.Sent);
				var subQueryDeclaration = new ZDBOnlySubQuery(typeof(JobDeclaration), JobDeclarationSchema.PK);
				subQueryDeclaration.AddToFilter(JobDeclarationSchema.JE_CustomsOffice, customsOfficeOfExport);
				subQueryEntryHeader.AddSubQuery(CusEntryHeaderSchema.CH_JE, subQueryDeclaration, JoinCondition.And);
				query.AddSubQuery(EDIMessageSchema.EM_LinkUniqueID, subQueryEntryHeader, JoinCondition.And);

				var subQueryEntryNum = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				subQueryEntryNum.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.LocalReferenceNumber);
				subQueryEntryNum.AddToFilter(CusEntryNumSchema.CE_EntryNum, lrn);
				query.AddSubQuery(EDIMessageSchema.EM_LinkUniqueID, subQueryEntryNum, JoinCondition.And);

				groupedRelatedMessages = factory.Load<EDIMessage>(query)?.GroupBy(x => x.EM_LinkUniqueID);
			}

			var groupedRelatedMessagesCount = groupedRelatedMessages.Count();
			if (groupedRelatedMessagesCount == 1)
			{
				result = factory.Load<CusEntryHeader>(groupedRelatedMessages.Single().Key);
			}
			else
			{
				Logger.LogWarning($"Matched {groupedRelatedMessagesCount} CusEntryHeaders searched by LRN '{lrn}' and CustomsOfficeOfExport '{customsOfficeOfExport}'.");
			}

			return result;
		}
	}
}
