using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public interface ISCRLine
	{
		ZInt NumberOfPackages { get; }
		ZString TypeOfPackages { get; }
		ZString GoodsDescription { get; }
		ZDecimal GrossWeight { get; }
		ZString WeightUnits { get; }
		ZDecimal Volume { get; }
		ZString VolumeUnits { get; }
		ZString ContainerNumber { get; }
		ZString DGCodes { get; }
		ZString ShippingMarks { get; }
		ZString TariffNumbers { get; }
	}
}
