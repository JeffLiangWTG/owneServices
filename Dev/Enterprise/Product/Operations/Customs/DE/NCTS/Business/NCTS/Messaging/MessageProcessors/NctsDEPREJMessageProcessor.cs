using System.Text;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsDEPREJMessageProcessor : NctsMessageProcessor<AtlasInboundEDIMessage<IDEPREJ>, IDEPREJ>
	{
		public NctsDEPREJMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("782490C4-592B-4BF5-87EE-AAF8EAC674A2", "NCTS DEPREJ Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IDEPREJ> message) => GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IDEPREJ> message)
		{
			var dataProvider = message.DataProvider;
			var localReferenceNumber = dataProvider.LocalReferenceNumber;
			var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;

			var header = movementHeader.Header;
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit;
			movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Error;

			message.EM_Status = EDIMessage.Status.ProcessedOK;
			message.SetLogbookRegistrationNumber(dataProvider.MovementReferenceNumber);
			message.SetLogbookLocalReferenceNumber(localReferenceNumber);

			emailSubjectSuffixProvider = new NCTSEmailSubjectSuffixProvider(movementHeader);

			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory, header
					, Res.GetString("703E561C-8346-4B53-8842-8730D27AA62B", "NCTS Departure Rejection Notification")
					, GetEmailBody()
					, false
					, message.Branch
					, header
					, dataProvider.ReferencedMessageIdentifier);

			header.CancelWarehouseIfNeeded(r => Logger.LogWarning(r.ErrorMessage));

			movementHeader.GuaranteeTransactionCoordinator.DeleteTransactions();

			string GetEmailBody()
			{
				var htmlBody = new StringBuilder();
				htmlBody.Append(Res.GetString("7A357E4C-F0B3-412D-A3EF-87B3719B4505", "Your NCTS Departure Declaration for Job {0} has received a Rejection Notification. For details please follow the Link to the Job.", header.BH_JobReference));
				htmlBody.Append("<br /><br />");

				var tableCreator = new HtmlTableCreator();
				var rejectionType = dataProvider.RejectionType;
				tableCreator.WriteRow(Res.GetString("0C201047-8FC9-4CDB-BEAA-A674932E4B08", "LRN"), localReferenceNumber);
				tableCreator.WriteRow(Res.GetString("DCCBA800-67B9-438B-80FD-385B79D1CD46", "Rejection Type"), rejectionType + " - " + new NctsMessageRejectionTypeList().GetDescriptionFromCode(rejectionType));
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
