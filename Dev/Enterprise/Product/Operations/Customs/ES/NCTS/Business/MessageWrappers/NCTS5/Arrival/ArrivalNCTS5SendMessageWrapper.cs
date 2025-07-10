using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalNCTS5SendMessageWrapper : NCTS5CommonSendMessageWrapper, IArrivalNCTSMessageDataProvider
	{
		public ArrivalNCTS5SendMessageWrapper(NctsHeader header, ICertificateProvider certificateData)
		: base(header, certificateData)
		{
			arrivalMovementHeader = Argument.NotNull(nctsHeader.ArrivalMovementHeader, nameof(nctsHeader.ArrivalMovementHeader));
		}
		readonly NctsArrivalMovementHeader arrivalMovementHeader;

		public IArrivalNCTSTransitOperation TransitOperation => transitOperation ??= new ArrivalNCTS5TransitOperationWrapper(nctsHeader);
		ArrivalNCTS5TransitOperationWrapper transitOperation;

		public ZString CustomsOfficeOfDestinationActual => nctsHeader.ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival;

		public IPartyIdProvider TraderAtDestination => traderAtDestination ??= NCTS5CommonTraderAtDestinationWrapper.New(nctsHeader.DestinationTrader);
		NCTS5CommonTraderAtDestinationWrapper traderAtDestination;

		public IPartyIdProvider RepresentativeAtDestination => representativeAtDestination ??= NCTS5CommonTraderAtDestinationWrapper.New(arrivalMovementHeader.Representative);
		NCTS5CommonTraderAtDestinationWrapper representativeAtDestination;

		public IArrivalNCTSIndicators Indicators => indicators ??= new ArrivalNCTS5IndicatorsWrapper(nctsHeader);
		IArrivalNCTSIndicators indicators;

		public IArrivalNCTSConsigment Consignment => consignment ??= new ArrivalNCTS5ConsignmentWrapper(nctsHeader);
		IArrivalNCTSConsigment consignment;

		public IReadOnlyCollection<INCTSCommonAuthorisation> Authorisations
		{
			get
			{
				if (authorisations == null)
				{
					var authorisationsList = new List<NCTS5CommonAuthorisationWrapper>();

					ZShort seqNum = 1;
					foreach (var authorization in nctsHeader.CusAuthorizationUsages)
					{
						authorisationsList.Add(new NCTS5CommonAuthorisationWrapper(authorization, seqNum));
						seqNum++;
					}

					authorisations = authorisationsList.AsReadOnly();
				}
				return authorisations;
			}
		}
		IReadOnlyCollection<NCTS5CommonAuthorisationWrapper> authorisations;
	}
}
