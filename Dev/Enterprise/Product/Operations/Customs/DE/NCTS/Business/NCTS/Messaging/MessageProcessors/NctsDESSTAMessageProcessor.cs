using System.Text;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsDESSTAMessageProcessor : NctsMessageProcessor<AtlasInboundEDIMessage<IDESSTA>, IDESSTA>
	{
		public NctsDESSTAMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("EB02CBEF-5AE1-462C-ADA0-B450FE517C5B", "NCTS DESSTA Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IDESSTA> message) => GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IDESSTA> message)
		{
			var dataProvider = message.DataProvider;
			var header = (NctsHeader)message.EM_LinkedObject;

			var (mappedSuccessfully, mappedStatusCode) = NctsCustomsStatusMapper.GetCW1StatusFromNctsCustomsStatus(message.DataProvider.TransitOperationDestinationStatus);
			if (mappedSuccessfully)
			{
				header.ArrivalMovementHeader.BM_CustomsStatus = mappedStatusCode;
			}

			if (header.EffectiveMessageStatus == LogicalStatusList.Codes.Sent)
			{
				header.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
			}

			message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			message.SetLogbookRegistrationNumber(dataProvider.MovementReferenceNumber);

			var body = CreateEmailBody(dataProvider, header);
			var subject = Res.GetString("6DEADA9C-CDB4-4142-B81F-97C43BDC2A1C", "NCTS Arrival Status Update.");
			emailSubjectSuffixProvider = new NCTSEmailSubjectSuffixProvider(header.ArrivalMovementHeader);

			GenerateHtmlEmailAndSendToOriginalOrGroup(factory, header, subject, body, false, message.Branch, header, () => header.Messages.LastSentOutgoingMessage);
		}

		string CreateEmailBody(IDESSTA provider, NctsHeader nctsHeader)
		{
			var emailBody = new StringBuilder();

			emailBody.Append(ZString.Format(Res.GetString("7DDE61D6-52A9-4DC1-88E8-381C77C8DC01",
				"Your NCTS Arrival Declaration for Job {0} has received a Status Update. For details please follow the Link to the Job.",
				nctsHeader.BH_JobReference)));

			emailBody.Append((NoResString)@"<br/><br/>");
			var emailTable = new HtmlTableCreator();
			emailTable.WriteRow(Res.GetString("46307A02-70FE-4C1E-A104-18307D80C323", "MRN"), provider.MovementReferenceNumber);
			emailTable.WriteRow(Res.GetString("AA6EF452-95C7-406D-B948-B8CE58F23EFE", "Status Update"), a0116StateOfCompletionCodeList.GetDescriptionFromCode(provider.TransitOperationDestinationStatus));

			emailBody.Append(emailTable.ToHtml());

			return emailBody.ToString();
		}

		NCTSEmailSubjectSuffixProvider emailSubjectSuffixProvider;

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo)
		{
			var email = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			emailSubjectSuffixProvider.SetEmailSubjectSuffix(email);
			return email;
		}

		readonly ReadOnlyCodeDescriptionPairList a0116StateOfCompletionCodeList = new A0116StateOfCompletionCodeList();
	}
}
