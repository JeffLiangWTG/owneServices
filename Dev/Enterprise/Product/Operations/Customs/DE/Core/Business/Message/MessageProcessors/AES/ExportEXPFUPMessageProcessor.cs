using System;
using System.Globalization;
using System.Text;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ExportEXPFUPMessageProcessor : ExportMessageProcessor
		<AesInboundEDIMessage<IEXPFUP>, IEXPFUP>
	{
		public ExportEXPFUPMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("4D123E67-1193-4CE5-B8E0-7C50B13AC93E", "Export EXPFUP Message Processor");

		protected override BusinessObject GetLinkedObject(AesInboundEDIMessage<IEXPFUP> message) => GetLinkedObjectFromOriginalMessageOrMrn(message.Factory, message.DataProvider?.ReferencedMessageIdentifier, message.DataProvider?.MovementReferenceNumber);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AesInboundEDIMessage<IEXPFUP> message)
		{
			var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			var dataProvider = message.DataProvider;

			message.SetLogbookRegistrationNumber(dataProvider.MovementReferenceNumber);
			message.SetLogbookLocalReferenceNumber(dataProvider.LocalReferenceNumber);

			entryHeader.AddCustomsEntryStatusLog(UniversalReferenceConstants.EntryStatus.FUP);
			entryHeader.CH_ExitedStatus = ExportExitStatus.Codes.ReminderForNonExitedGoodsReceived;

			SendEmailIfNeeded(message, dataProvider, entryHeader);
		}

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo)
		{
			var email = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			emailSubjectSuffixProvider.SetEmailSubjectSuffix(email);
			return email;
		}

		DeclarationEmailSubjectSuffixProvider emailSubjectSuffixProvider;

		void SendEmailIfNeeded(AesInboundEDIMessage<IEXPFUP> message, IEXPFUP dataProvider, CusEntryHeader entryHeader)
		{
			var declaration = entryHeader.Declaration;
			if (declaration != null)
			{
				emailSubjectSuffixProvider = new DeclarationEmailSubjectSuffixProvider(entryHeader);
				GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
					, declaration
					, Res.GetString("04CD708D-E6FB-42CA-857C-33FE8E708AE4", "AES EXP Follow Up Request")
					, GetEmailBody(declaration, dataProvider)
					, false
					, message.Branch
					, declaration
					, dataProvider.ReferencedMessageIdentifier);
			}
		}

		static ZString GetEmailBody(JobDeclaration declaration, IEXPFUP dataProvider)
		{
			var htmlBody = new StringBuilder();
			htmlBody.Append(Res.GetString("0A3DD856-9922-48AF-8C9D-C864BE0CDC71", @"Your Export Declaration Message for Job {0} has a follow up request. For details please follow the Link to the Job.
Shown below is a summary of relevant information received in the message:", declaration.JE_DeclarationReference));
			htmlBody.Append("<br /><br />");
			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("AE715699-CC24-422D-86EE-3DEA4A9E58F5", "Local Reference No."), dataProvider.LocalReferenceNumber);
			tableCreator.WriteRow(Res.GetString("019A47FF-F10D-46AC-9E50-6D8E3EA65E3A", "MRN"), dataProvider.MovementReferenceNumber);
			tableCreator.WriteRow(Res.GetString("8E88FE4F-3805-4817-9759-623D8A2E77C3", "Request Date"), FormatDate(dataProvider.RequestDate));
			tableCreator.WriteRow(Res.GetString("F8DC8AD5-F34F-4371-9E7A-6AAB26318577", "Response Date"), FormatDate(dataProvider.LatestResponseDate));
			tableCreator.WriteRow(Res.GetString("A0C832F5-EB05-4FE9-B445-553433DC5ED0", "Presentation Date"), FormatDate(dataProvider.LatestPresentationDate));
			htmlBody.Append(tableCreator.ToHtml());

			return htmlBody.ToString();

			string FormatDate(DateTime? date) => date?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
		}
	}
}
