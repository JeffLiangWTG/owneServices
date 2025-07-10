using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.FR.Business;
using NctsArrivalMovementHeader = Enterprise.Customs.FR.Business.NCTS.NctsArrivalMovementHeader;
using NctsHeader = Enterprise.Customs.FR.Business.NCTS.NctsHeader;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class CC044CWrapper : ICC044C
	{
		CC044CWrapper(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
			this.arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
		}
		readonly NctsHeader nctsHeader;
		readonly NctsArrivalMovementHeader arrivalMovementHeader;

		public static CC044CWrapper New(NctsHeader nctsHeader) => nctsHeader == null ? null : new CC044CWrapper(nctsHeader);

		public string MessageSender => FRConstants.NCTSMessage.Operator;

		public string MessageRecipient => FRConstants.NCTSMessage.NationalAdministration;

		public DateTime PreparationDateAndTime => ZDateTime.Now.ToDateTime();

		public string CorrelationIdentifier => ZString.Empty;

		public IArrivalTransitOperation TransitOperation => transitOperation ?? (transitOperation = ArrivalTransitOperationWrapper.New(nctsHeader));
		IArrivalTransitOperation transitOperation;

		public ICustomsOffice CustomsOfficeOfDestinationActual => customsOfficeOfDestinationActual ?? (customsOfficeOfDestinationActual = CustomsOfficeWrapper.New(arrivalMovementHeader?.DestinationCustomsOfficeCodeForArrival));
		ICustomsOffice customsOfficeOfDestinationActual;

		public ITrader TraderAtDestination => traderAtDestination ?? (traderAtDestination = TraderWrapper.New(nctsHeader.DestinationTrader));
		ITrader traderAtDestination;

		public IUnloadingRemark UnloadingRemark => unloadingRemark ?? (unloadingRemark = UnloadingRemarkWrapper.New(arrivalMovementHeader));
		IUnloadingRemark unloadingRemark;

		public IConsignment Consignment => consignment ?? (consignment = EmitConsignment ? CC044CConsignmentWrapper.New(arrivalMovementHeader) : null);
		IConsignment consignment;

		public IMessageEnveloppe MessageEnveloppe => messageEnveloppe ?? (messageEnveloppe = MessageEnveloppeWrapper.New(nctsHeader, "IE044"));
		IMessageEnveloppe messageEnveloppe;

		bool EmitConsignment => !emitConsignment ? (emitConsignment = !arrivalMovementHeader.BM_NoChangesToReport && nctsHeader.Bills.Any(x => x.MovementDetail.B9_UnloadedState != NctsUnloadedStateList.Codes.DEC)
							|| (arrivalMovementHeader?.ArrivalTransportInfos.Any(x => x.TPM_TransportState != NctsUnloadedStateList.Codes.DEC) ?? true)
							|| nctsHeader.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>().Any(x => x.BC_UnloadedState != NctsUnloadedStateList.Codes.DEC)
							|| nctsHeader.ArrivalHeaderContainers.Cast<NctsArrivalHeaderContainer>().SelectMany(x => x.Seals).Cast<CusSeal>().Any(x => x.BK_UnloadingState != NctsUnloadedStateList.Codes.DEC)) : emitConsignment;
		bool emitConsignment;
	}
}
