using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface ITNNNCTSMessageDataProvider : INCTSCommonDataProvider
	{
		ITNNNCTSTransitOperation TransitOperation { get; }
		ZString CustomsOfficeOfDeparture { get; }
		ZString CustomsOfficeOfDestinationDeclared { get; }
		ZString CustomsOfficeOfDestinationActual { get; }
		INCTSCommonHolderOfTheTransitProcedureWithAddress HolderOfTheTransitProcedure { get; }
		IPartyIdProvider TraderAtDestination { get; }
		IPartyIdProvider RepresentativeAtDestination { get; }
		ITNNNCTSDigitizedDocument DigitizedDocument { get; }
		ITNNNCTSConsignment Consignment { get; }
	}

	public interface ITNNNCTSTransitOperation : INCTSCommonTransitOperationMRN
	{
		INCTSCommonTransitOperation CommonTransitOperation { get; }
		ZDateTime DeclarationAcceptanceDate { get; }
		ZDateTime ReleaseDate { get; }
	}

	public interface ITNNNCTSDigitizedDocument : IAnnexDocCommon
	{
		ZString DocumentType { get; }
		ZString DocumentLocation { get; }
	}

	public interface ITNNNCTSConsignment : INCTSCommonDepartureConsignment
	{
		INCTSCommonConsignmentDepartureAndAmendmentAndTNN CommonConsignmentData { get; }
		INCTSPartyNameProviderWithAddress Consignor { get; }
		IReadOnlyCollection<INCTSCommonActiveBorderTransportMeans> ActiveBorderTransportMeans { get; }
		IReadOnlyCollection<ITNNNCTSHouseConsignment> HouseConsignment { get; }
	}

	public interface ITNNNCTSHouseConsignment : INCTSCommonHouseConsignmentDepartureAndAmendmentAndTNN
	{
		IReadOnlyCollection<ITNNNCTSConsignmentItem> ConsignmentItem { get; }
	}

	public interface ITNNNCTSConsignmentItem : INCTSCommonConsignmentItemDepartureAndAmendmentAndTNN
	{
		ITNNNCTSCommodity Commodity { get; }
		IReadOnlyCollection<INCTSCommonDocumentWithInfo> SupportingDocument { get; }
	}

	public interface ITNNNCTSCommodity : INCTSCommonCommodity
	{
		IReadOnlyCollection<ICommonDangerousGoods> DangerousGoods { get; }
		INCTSCommonGoodsMeasure GoodsMeasure { get; }
	}
}
