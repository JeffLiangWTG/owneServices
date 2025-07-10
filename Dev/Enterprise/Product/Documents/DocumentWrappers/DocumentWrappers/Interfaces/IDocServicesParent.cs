using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public interface IDocServicesParent
	{
		ZString EmailSubjectNumber { get; }
		ZString ConsolNumber { get; }
		ZString GoodsDescription { get; }
		ZString Packages { get; }
		ZString Weight { get; }
		ZString Volume { get; }
		ZString WeightUnit { get; }
		ZString VolumeUnit { get; }
		ZString MasterBillNum { get; }
		ZString MasterBillHeading { get; }
		ZString HouseBill { get; }
		ZString HouseBillHeading { get; }
		ZString TransportInfo { get; }
		ZDateTime ETD { get; }
		ZDateTime ETA { get; }
		ZString ContainerNumbers { get; }
		ZString Context { get; }
		DocUNLOCO PortOfLoading { get; }
		DocUNLOCO PortOfDischarge { get; }
		ZString OwnerRefAndOrderRef { get; }
		ZString OwnerRefAndOrderRefHeading { get; }
	}
}
