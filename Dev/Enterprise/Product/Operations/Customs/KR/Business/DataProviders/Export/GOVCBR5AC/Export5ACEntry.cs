using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Export5ACEntry : ExtendedOfficeHoursEntry, IExport5ACEntry
	{
		public string SupplierName { get; set; }

		ZString IExport5ACEntry.SupplierName => SupplierName;
	}
}
