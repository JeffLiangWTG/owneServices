using System.Text;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ExportERRNCKMessageProcessor : ExportMessageProcessor<AesInboundEDIMessage<IERRNCK>, IERRNCK>
	{
		public ExportERRNCKMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("2ADB6CB8-B10E-4DC2-B947-FB7DC9E3652A", "Export EXPERR Message Processor");

		protected override BusinessObject GetLinkedObject(AesInboundEDIMessage<IERRNCK> message) => GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AesInboundEDIMessage<IERRNCK> message)
		{
			var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			var dataProvider = message.DataProvider;

			var logbookRegistrationNumber = !dataProvider.ReferenceNumber.IsNullOrEmpty() ? dataProvider.ReferenceNumber : dataProvider.ReferencedMessageIdentifier;
			message.SetLogbookRegistrationNumber(logbookRegistrationNumber);
			message.SetLogbookLocalReferenceNumber(dataProvider.LocalReferenceNumber);

			if (dataProvider.MessageGroup == ExportMessageSubTypeList.Codes.EXP)
			{
				entryHeader.CH_Status = EDIMessageStatusList.Codes.Rejected;
				entryHeader.AddCustomsEntryStatusLog(UniversalReferenceConstants.EntryStatus.ERR);
				SendEmailIfNeeded(message, dataProvider, entryHeader);
			}
		}

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo)
		{
			var email = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			emailSubjectSuffixProvider.SetEmailSubjectSuffix(email);
			return email;
		}

		DeclarationEmailSubjectSuffixProvider emailSubjectSuffixProvider;

		void SendEmailIfNeeded(AesInboundEDIMessage<IERRNCK> message, IERRNCK dataProvider, CusEntryHeader entryHeader)
		{
			var declaration = entryHeader.Declaration;
			if (declaration != null)
			{
				emailSubjectSuffixProvider = new DeclarationEmailSubjectSuffixProvider(entryHeader);
				GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
					, declaration
					, Res.GetString("452B7C95-5D17-4B0A-9639-B3495FA863EB", "Export Declaration Message Status")
					, GetEmailBody(declaration, dataProvider)
					, false
					, message.Branch
					, declaration
					, dataProvider.ReferencedMessageIdentifier);
			}
		}

		ZString GetEmailBody(JobDeclaration declaration, IERRNCK dataProvider)
		{
			var htmlBody = new StringBuilder();
			htmlBody.Append(Res.GetString("988DCDBA-3753-4CE3-9818-6F273B74DE8A", @"Your Export Declaration Message for Job {0} has been rejected. For details please follow the Link to the Job.
Shown below is a summary of relevant information received in the message:", declaration.JE_DeclarationReference));
			htmlBody.Append("<br /><br />");
			htmlBody.Append(GetErrorsEmailTable(dataProvider.Errors));
			return htmlBody.ToString();
		}
	}
}
