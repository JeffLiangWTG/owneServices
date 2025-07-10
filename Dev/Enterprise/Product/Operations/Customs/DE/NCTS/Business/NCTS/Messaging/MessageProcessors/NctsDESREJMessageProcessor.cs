using System.Text;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsDESREJMessageProcessor : NctsMessageProcessor<AtlasInboundEDIMessage<IDESREJ>, IDESREJ>
	{
		public NctsDESREJMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("89DEC0BB-68AA-4646-9604-E7402B2AEBDC", "NCTS DESREJ Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IDESREJ> message) => GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IDESREJ> message)
		{
			var provider = message.DataProvider;
			var movementReferenceNumber = provider.MovementReferenceNumber;
			var header = (NctsHeader)message.EM_LinkedObject;

			message.EM_Status = EDIMessage.Status.ProcessedOK;
			message.SetLogbookRegistrationNumber(movementReferenceNumber);

			header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Error;

			emailSubjectSuffixProvider = new NCTSEmailSubjectSuffixProvider(header.ArrivalMovementHeader);

			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory, header
					, Res.GetString("127CD1FB-0C84-4640-8FA4-A2214481EADC", "NCTS Arrival Rejection Notification")
					, GetEmailBody()
					, false
					, message.Branch
					, header
					, provider.ReferencedMessageIdentifier);

			string GetEmailBody()
			{
				var htmlBody = new StringBuilder();
				htmlBody.Append(Res.GetString("84094B37-EEA3-4759-9F7C-BD82264DE82B", "Your NCTS Arrival Declaration for Job {0} has received a Rejection Notification. For details please follow the Link to the Job.", header.BH_JobReference));
				htmlBody.Append("<br /><br />");

				var tableCreator = new HtmlTableCreator();
				var rejectionType = provider.RejectionType;
				tableCreator.WriteRow(Res.GetString("87BF5102-1EB4-4D23-AF9B-8F78092ADBCD", "MRN"), movementReferenceNumber);
				tableCreator.WriteRow(Res.GetString("9380F900-B038-46D3-8F58-0D8C594DF8BF", "Rejection Type"), rejectionType + " - " + new S0570RejectionTypeCodeList().GetDescriptionFromCode(rejectionType));
				htmlBody.Append(tableCreator.ToHtml());

				return htmlBody.ToString();
			}
		}

		NCTSEmailSubjectSuffixProvider emailSubjectSuffixProvider;

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo)
		{
			var email = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			emailSubjectSuffixProvider.SetEmailSubjectSuffix(email);
			return email;
		}
	}
}
