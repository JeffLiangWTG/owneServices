using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IExport5ACEntry : IExtendedOfficeHoursEntry
	{
		ZString SupplierName { get; }
	}
}
