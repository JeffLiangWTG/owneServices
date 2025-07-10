using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ExportEXPRELMessageProcessor : ExportMessageProcessor<AesInboundEDIMessage<IEXPREL>, IEXPREL>
	{
		public ExportEXPRELMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("EC6B3F6F-F9E6-43AE-9CB1-1337AE34BA8B", "Export EXPREL Message Processor");

		protected override BusinessObject GetLinkedObject(AesInboundEDIMessage<IEXPREL> message) => GetLinkedObjectFromOriginalMessageOrMrn(message.Factory, message.DataProvider?.ReferencedMessageIdentifier, message.DataProvider?.MovementReferenceNumber);

		protected override bool UpdateWarehouseCore
		{
			get
			{
				var result = false;
				if (CurrentMessage?.EM_LinkedObject is CusEntryHeader cusEntryHeader)
				{
					result = (cusEntryHeader.IsOutOfWarehouseWarehousing && WarehouseTransactionStatusList.IsPendingOutward(cusEntryHeader.CH_WarehouseTransactionStatus) && (cusEntryHeader.CH_EntryStatus == "501" || cusEntryHeader.CH_EntryStatus == "502"));
				}
				return result;
			}
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AesInboundEDIMessage<IEXPREL> message)
		{
			var status = AesEDIMessage.Status.ProcessedOK;
			var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
			var dataProvider = message.DataProvider;

			if (IsEntryStatusValidToBeUpdated())
			{
				var releaseDateTimeLocal = ((ZDateTime)dataProvider.IssuingDateTime).ToLocalBranchTime(factory);
				entryHeader.MovementReferenceNumberSetter(dataProvider.MovementReferenceNumber, releaseDateTimeLocal);
				UpdateEntryStatus();
				entryHeader.CH_EntryReleaseDate = releaseDateTimeLocal;
				SendEmailIfNeeded(message, entryHeader, dataProvider.ReferencedMessageIdentifier);
			}
			else
			{
				status = AesEDIMessage.Status.Discarded;
				factory.CreateStmNoteForEdiMessage(message.PK, Res.GetString("242592C4-418E-429A-9671-821E11285B59", "Message discarded: Entry Status '131', '132', '141' or '142' expected."));
			}

			message.EM_Status = status;
			message.SetLogbookRegistrationNumber(dataProvider.MovementReferenceNumber);
			message.SetLogbookLocalReferenceNumber(dataProvider.LocalReferenceNumber);

			bool IsEntryStatusValidToBeUpdated() => entryHeader.CH_EntryStatus.In(new ZString[] { "131", "132", "141", "142" });

			void UpdateEntryStatus()
			{
				if (entryHeader.CH_EntryStatus == "131" || entryHeader.CH_EntryStatus == "141")
				{
					entryHeader.CH_EntryStatus = "501";
				}
				else if (entryHeader.CH_EntryStatus == "132" || entryHeader.CH_EntryStatus == "142")
				{
					entryHeader.CH_EntryStatus = "502";
				}
				entryHeader.AddCustomsEntryStatusLog(entryHeader.CH_EntryStatus);
			}
		}

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure,
			IGlbBranch branchForEmailLogo)
		{
			var email = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			AttachDocumentsToEmail(email, attachedDocumentsCached);
			emailSubjectSuffixProvider.SetEmailSubjectSuffix(email);
			return email;
		}

		DeclarationEmailSubjectSuffixProvider emailSubjectSuffixProvider;

		void SendEmailIfNeeded(AesInboundEDIMessage<IEXPREL> message, CusEntryHeader entryHeader, ZString referencedMessageIdentifier)
		{
			attachedDocumentsCached = message.AttachedDocuments;

			var declaration = entryHeader.Declaration;
			if (declaration != null)
			{
				emailSubjectSuffixProvider = new DeclarationEmailSubjectSuffixProvider(entryHeader);
				GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory,
					declaration
					, Res.GetString("4E4ED4E1-6FEA-490D-B45B-4A28D9CECE28", "AES EXP Release Message")
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

			htmlBody.Append(Res.GetString("1F477E05-6EE5-47AF-8028-5290C56B7DF9",
				"Your Export Declaration Message for {0} has a Release Message. For details please follow the Link to the Job.",
				declaration.JE_DeclarationReference));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("9965109A-1AEF-4BA5-A01F-E90843FD2C8E", "MRN:"), entryHeader.MovementReferenceNumber);
			htmlBody.Append(tableCreator.ToHtml());

			return htmlBody.ToString();
		}

		IReadOnlyCollection<AttachedDocument> attachedDocumentsCached;
	}
}
