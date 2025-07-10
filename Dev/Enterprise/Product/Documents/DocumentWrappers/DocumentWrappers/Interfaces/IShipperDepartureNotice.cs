using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public interface IShipperDepartureNotice
	{
		ZString ReportName { get; }

		//Shipper Departure Notice Header Section
		ZString ShipperDepartureNoticeDocumentHeader { get; }
		ZString JobNumberHeading { get; }
		ZString JobNumber { get; }
		ZString SecondJobNumberHeading { get; }
		ZString SecondJobNumber { get; }

		//Document Body Section
		ZString ReleaseType { get; }
		ZString OrderNumbers { get; }
		ZString AgentsBookingReference { get; }
		ZString GoodsDescription { get; }
		ZString MasterBillHeading { get; }
		ZString MasterBillAndIssueHeading { get; }
		ZString MasterBillNum { get; }
		ZString MasterBillAndIssueDate { get; }
		ZString HouseBillHeading { get; }
		ZString HouseBillAndIssueHeading { get; }
		ZString HouseBill { get; }
		ZString HouseBillAndIssueDate { get; }
		ZString Packages { get; }
		ZString Weight { get; }
		ZString WeightUnit { get; }
		ZString Volume { get; }
		ZString VolumeUnit { get; }
		ZString Chargeable { get; }
		ZString ChargeableUnit { get; }
		ZString CollectedFromETDString { get; }
		ZString DeliveredToETAString { get; }
		ZString LoadingETDString { get; }
		ZString DischargeETAString { get; }
		ZString TransportHeading { get; }
		ZString TransportInfo { get; }
		ZString DepartureReference { get; }
		ZString MarksAndNumbers { get; }
		ZString HazCat { get; }
		ZString TransportMode { get; }
		ZString PackingMode { get; }
		ZString TransportModeAndPackingMode { get; }

		ZInt NoOfOriginalBills { get; }
		ZInt NoOfCopyBills { get; }

		DocOrganisation Consignor { get; }
		DocOrganisation Consignee { get; }
		DocOrganisation ReceivingForwarder { get; }
		DocOrganisation ShippersDeliveryAgent { get; }
		DocOrganisation ShippingLine { get; }
		DocUNLOCO OriginLoco { get; }
		DocUNLOCO DestinationLoco { get; }
		DocUNLOCO PortOfLoading { get; }
		DocUNLOCO PortOfDischarge { get; }

		DocCommodityCollection Commodity { get; }
	}
}
