using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IExtendedOfficeHoursEntry
	{
		ZString ReferenceNumber { get; }
		ZDecimal TotalCustomsValueInUSD { get; }
		ZDecimal TotalPackQty { get; }
		ZDecimal TotalGrossWeightInKG { get; }
	}
}
