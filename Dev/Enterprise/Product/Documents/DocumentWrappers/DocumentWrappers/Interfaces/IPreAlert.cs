
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public interface IPreAlert : ITrackingBusinessObject
	{
		ZString ReportName { get; }

		//Pre-Alert Document Header Section
		ZString PreAlertDocumentHeader { get; }
		ZString JobNumberHeading { get; }
		ZString JobNumber { get; }
		ZString SecondJobNumberHeading { get; }
		ZString SecondJobNumber { get; }
		ZString AvailableDateHeading { get; }
		ZDateTime AvailableDate { get; }
		ZString StorageStartsHeading { get; }
		ZString StorageStartsDate { get; }
		ZString ContainerMode { get; }
		ZString TransportMode { get; }
		ZString TransportModeDescription { get; }

		//Document Body Section
		ZString UltimateNotification { get; }
		ZString OrderNumbers { get; }
		ZString BrokerName { get; }
		ZString TransportHeading { get; }
		ZString TransportInfo { get; }
		ZString PreAlertReferenceHeading { get; }
		ZString PreAlertReference { get; }
		ZString MasterBillHeading { get; }
		ZString MasterBillAndIssueHeading { get; }
		ZString MasterBillNum { get; }
		ZString MasterBillAndIssueDate { get; }
		ZString HouseBillHeading { get; }
		ZString HouseBillAndIssueHeading { get; }
		ZString HouseBill { get; }
		ZString HouseBillAndIssueDate { get; }
		ZString GoodsDescription { get; }
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
		ZString ArrivalReference { get; }
		ZString HazCat { get; }
		ZString Context { get; }
		ZString MarksAndNumbers { get; }
		ZBool IsUnattachedOrder { get; }

		DocOrganisation Consignor { get; }
		DocOrganisation Consignee { get; }
		ZInt NotifyPartyCount { get; }
		DocContacts NotifyParty { get; }
		DocContacts NotifyParty2 { get; }
		DocContacts NotifyParty3 { get; }
		DocDocAddress GoodsAvailableAt { get; }
		DocOrganisation ShippingLine { get; }

		DocUNLOCO OriginLoco { get; }
		DocUNLOCO DestinationLoco { get; }
		DocUNLOCO PortOfLoading { get; }
		DocUNLOCO PortOfDischarge { get; }

		DocTransportCollection CompleteRouting { get; }

		//Shipment and Orders Only
		DocCommodityCollection Commodity { get; }
		ZString ReleaseType { get; }
		ZString AlertText { get; }
		ZString PortDisplayMode { get; }
		ZBool ShowChargesOnArrivalNotice { get; }
		ZBool ShowExchangeRatesOnArrivalNotice { get; }

		//MY specific
		ZString KANumber { get; }
		ZString CTOArrivalBerth { get; }
		DocDocAddress UnpackAt { get; }
	}
}
