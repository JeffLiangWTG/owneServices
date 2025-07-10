using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ISeaOutturnReportLineInformation : IOutturnReportLineInformation
	{
		ZString ContainerNumber { get; }
		ZString HouseBillOfLading { get; }
		ZString OceanBillOfLading { get; }
		ZString SealNumber { get; }
		bool SealIntactIndicator { get; }
		bool VesselDischargeUnderbondIndicator { get; }
		bool UnpackIndicator { get; }
		ZDateTime DateTimeOfCargoReceiptUnload { get; }
		ZDateTime DateTimeOfOutturn { get; }
		ZInt Quantity { get; }
		ZString QuantityUnit { get; }
		ZString ImportCargoType { get; }
		ZString PackageType { get; }
		ZString MarksAndNumbers { get; }
		ZString OutturnStatus { get; }
	}
}
