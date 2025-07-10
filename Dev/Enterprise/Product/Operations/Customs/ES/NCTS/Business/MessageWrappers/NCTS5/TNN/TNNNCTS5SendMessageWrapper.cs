using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class TNNNCTS5SendMessageWrapper : NCTS5CommonSendMessageWrapper, ITNNNCTSMessageDataProvider
	{
		public TNNNCTS5SendMessageWrapper(NctsHeader header, ICertificateProvider certificateData) : base(header, certificateData)
		{
			arrivalMovementHeader = Argument.NotNull(nctsHeader.ArrivalMovementHeader, nameof(nctsHeader.ArrivalMovementHeader));
			nctsHeaderDepartureTNN = Argument.NotNull(arrivalMovementHeader.HeaderTNN, nameof(arrivalMovementHeader.HeaderTNN));
		}
		protected NctsArrivalMovementHeader arrivalMovementHeader;
		protected NctsHeader nctsHeaderDepartureTNN;

		public ITNNNCTSTransitOperation TransitOperation => transitOperation ?? (transitOperation = new TNNNCTS5TransitOperationWrapper(nctsHeaderDepartureTNN));
		TNNNCTS5TransitOperationWrapper transitOperation;

		public ZString CustomsOfficeOfDeparture => nctsHeaderDepartureTNN.MovementHeader.CustomsOffices.Where(x => x.IsOfficeDeparture).FirstOrDefault()?.CY_Data ?? ZString.Empty;

		public ZString CustomsOfficeOfDestinationDeclared => nctsHeaderDepartureTNN.MovementHeader.CustomsOffices.Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination).FirstOrDefault()?.CY_Data ?? ZString.Empty;

		public ZString CustomsOfficeOfDestinationActual => arrivalMovementHeader.DestinationCustomsOfficeCodeForArrival;

		public INCTSCommonHolderOfTheTransitProcedureWithAddress HolderOfTheTransitProcedure => holderOfTheTransitProcedure ?? (holderOfTheTransitProcedure = NCTS5CommonHolderOfTheTransitProcedureWithAddressWrapper.New(nctsHeaderDepartureTNN, nctsHeaderDepartureTNN.IsInPhase5TransitionPeriod));
		NCTS5CommonHolderOfTheTransitProcedureWithAddressWrapper holderOfTheTransitProcedure;

		public IPartyIdProvider TraderAtDestination => traderAtDestination ?? (traderAtDestination = NCTS5CommonTraderAtDestinationWrapper.New(nctsHeader.DestinationTrader));
		NCTS5CommonTraderAtDestinationWrapper traderAtDestination;

		public IPartyIdProvider RepresentativeAtDestination => representativeAtDestination ?? (representativeAtDestination = nctsHeaderDepartureTNN?.Principal?.Address?.Header != arrivalMovementHeader.Representative?.Address?.Header ? NCTS5CommonTraderAtDestinationWrapper.New(arrivalMovementHeader.Representative) : null);
		NCTS5CommonTraderAtDestinationWrapper representativeAtDestination;

		public ITNNNCTSDigitizedDocument DigitizedDocument
		{
			get
			{
				if (document == null)
				{
					var annexDoc = nctsHeaderDepartureTNN.GetAllSendableEDocPivots().FirstOrDefault();

					document = annexDoc == null ? null : new TNNNCTS5DigitizedDocumentWrapper(nctsHeaderDepartureTNN, arrivalMovementHeader, annexDoc.Document, annexDoc.CSD_Description);
				}
				return document;
			}
		}
		TNNNCTS5DigitizedDocumentWrapper document;

		public ITNNNCTSConsignment Consignment => consignment ?? (consignment = new TNNNCTS5ConsignmentWrapper(nctsHeaderDepartureTNN));
		TNNNCTS5ConsignmentWrapper consignment;
	}
}
