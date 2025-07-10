using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface INotifUnloadingNCTSMessageDataProvider : INCTSCommonDataProvider
	{
		INotifUnloadingNCTSTransitOperation TransitOperation { get; }
		ZString CustomsOfficeOfDestinationActual { get; }
		IPartyIdProvider TraderAtDestination { get; }
		IPartyIdProvider RepresentativeAtDestination { get; }
		IUnloadingRemarksNCTS UnloadingRemarks { get; }
		INotifUnloadingConsignment Consignment { get; }
	}

	public interface INotifUnloadingNCTSTransitOperation : INCTSCommonTransitOperationMRN
	{
		ZString OtherThingsToReport { get; }
	}

	public interface IUnloadingRemarksNCTS
	{
		ZBool Conform { get; }
		ZDateTime UnloadingDate { get; }
		ZBool StateOfSeals { get; }
		ZBool StateOfSealsSpecified { get; }
		ZString UnloadingRemark { get; }
	}

	public interface INotifUnloadingConsignment : INCTSCommonConsignment
	{
		ZDecimal GrossMass { get; }
		ZBool GrossMassSpecified { get; }
		IReadOnlyCollection<INCTSCommonDocumentWithInfo> SupportingDocument { get; }
		IReadOnlyCollection<ICommonDocumentSequenceNumber> TransportDocument { get; }
		IReadOnlyCollection<ICommonDocumentSequenceNumber> AdditionalReference { get; }
		IReadOnlyCollection<INotifUnloadingNCTSHouseConsignment> HouseConsignment { get; }
	}

	public interface INotifUnloadingNCTSHouseConsignment : INCTSCommonHouseConsignment
	{
		ZBool GrossMassSpecified { get; }
		IReadOnlyCollection<ICommonDepartureTransportMeans> DepartureTransportMeans { get; }
		IReadOnlyCollection<INCTSCommonDocumentWithInfo> SupportingDocument { get; }
		IReadOnlyCollection<INotifUnloadingNCTSConsignmentItem> ConsignmentItem { get; }
	}

	public interface INotifUnloadingNCTSConsignmentItem : INCTSCommonConsignmentItem
	{
		INotifUnloadingCommodity Commodity { get; }
		IReadOnlyCollection<INCTSCommonDocumentWithInfo> SupportingDocument { get; }
	}

	public interface INotifUnloadingCommodity : INCTSCommonCommodityWithCusCode
	{
		INCTSCommonGoodsMeasure GoodsMeasure { get; }
	}
}
