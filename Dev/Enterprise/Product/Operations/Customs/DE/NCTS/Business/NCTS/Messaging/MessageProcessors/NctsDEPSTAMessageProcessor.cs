using System.Text;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class NctsDEPSTAMessageProcessor : NctsMessageProcessor<AtlasInboundEDIMessage<IDEPSTA>, IDEPSTA>
	{
		public NctsDEPSTAMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("74900B49-3A9C-40D7-839D-259D64626EF7", "NCTS DEPSTA Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IDEPSTA> message) => GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IDEPSTA> message)
		{
			var dataProvider = message.DataProvider;
			var movementReferenceNumber = dataProvider.MovementReferenceNumber;
			var localReferenceNumber = dataProvider.LocalReferenceNumber;
			var departureStatus = dataProvider.DepartureStatus;
			var movementHeader = (NctsDepartureMovementHeader)message.EM_LinkedObject;
			var header = movementHeader.Header;

			message.EM_Status = EDIMessage.Status.ProcessedOK;
			message.SetLogbookRegistrationNumber(movementReferenceNumber);
			message.SetLogbookLocalReferenceNumber(localReferenceNumber);

			header.MovementReferenceEntryNumber.CE_EntryNum = movementReferenceNumber;

			header.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;
			var phaseStatus = MapDepartureStatusToPhaseStatus(departureStatus);
			if (!string.IsNullOrEmpty(phaseStatus))
			{
				movementHeader.BM_Phase = phaseStatus;
			}

			var customsStatus = MapDepartureStatusToCustomsStatus(departureStatus);

			if (!string.IsNullOrEmpty(customsStatus))
			{
				movementHeader.BM_CustomsStatus = customsStatus;
			}

			if (departureStatus == A0115DepartureStatusCodeList.Codes._510)
			{
				movementHeader.Logs.AddNew(Events.CustomsEntryStatus, DeNctsConstants.DepartureCustomsStatus.GuaranteeWrittenOff);
				movementHeader.GuaranteeTransactionCoordinator.CounterBalanceConfirmedTransactions(message.EM_MessageNum);
			}

			emailSubjectSuffixProvider = new NCTSEmailSubjectSuffixProvider(movementHeader);

			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
				, header
				, Res.GetString("AF3D9D26-A2E1-4620-A5A8-D343E652A7AA", "NCTS Departure Status Update")
				, GetEmailBody()
				, false
				, message.Branch
				, header
				, dataProvider.ReferencedMessageIdentifier);

			if (movementHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.Cancelled)
			{
				header.CancelWarehouseIfNeeded(r => Logger.LogWarning(r.ErrorMessage));
			}

			ZString GetEmailBody()
			{
				var departureStatusForMail = $"{departureStatus} - {new A0115DepartureStatusCodeList().GetDescriptionFromCode(departureStatus)}";

				var htmlBody = new StringBuilder();

				htmlBody.Append(Res.GetString("016AEB56-DEEC-4F35-BEEA-D39330BFE199", "Your NCTS Departure Declaration for Job {0} has received a Status Update. For details please follow the Link to the Job.", header.BH_JobReference));

				htmlBody.Append("<br />");
				htmlBody.Append("<br />");

				var tableCreator = new HtmlTableCreator();
				tableCreator.WriteRow(Res.GetString("8967BE4D-31BE-47E6-B248-B3CFD156EAD4", "LRN:"), localReferenceNumber);
				tableCreator.WriteRow(Res.GetString("44ACFAFD-B187-41FC-981F-76F64BE07F73", "MRN:"), movementReferenceNumber);
				tableCreator.WriteRow(Res.GetString("48F8EFD3-4D84-4094-AD75-929BA40FECB0", "Status Update:"), departureStatusForMail);
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

		public static string MapDepartureStatusToCustomsStatus(string departureStatus) =>
			departureStatus switch
			{
				A0115DepartureStatusCodeList.Codes._110 => NCTS5DepartureCustomsStatusList.Codes.Acknowledged,
				A0115DepartureStatusCodeList.Codes._130 => NCTS5DepartureCustomsStatusList.Codes.MrnAllocated,
				A0115DepartureStatusCodeList.Codes._191 => NCTS5DepartureCustomsStatusList.Codes.Cancelled,
				A0115DepartureStatusCodeList.Codes._520 => NCTS5DepartureCustomsStatusList.Codes.Cancelled,
				A0115DepartureStatusCodeList.Codes._510 => ZString.Empty,
				A0115DepartureStatusCodeList.Codes._570 => NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed,
				A0115DepartureStatusCodeList.Codes._571 => NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed,
				A0115DepartureStatusCodeList.Codes._590 => NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed,
				_ => string.Empty
			};

		public static string MapDepartureStatusToPhaseStatus(string departureStatus)
		{
			switch (departureStatus)
			{
				case A0115DepartureStatusCodeList.Codes._110:
				case A0115DepartureStatusCodeList.Codes._130:
				case A0115DepartureStatusCodeList.Codes._510:
				case A0115DepartureStatusCodeList.Codes._570:
				case A0115DepartureStatusCodeList.Codes._571:
				case A0115DepartureStatusCodeList.Codes._590:
					return NctsMovementHeaderTransactionStatusList.Codes.Declaration;
				case A0115DepartureStatusCodeList.Codes._191:
				case A0115DepartureStatusCodeList.Codes._520:
					return NctsMovementHeaderTransactionStatusList.Codes.Cancellation;
				default:
					return string.Empty;
			}
		}
	}
}
