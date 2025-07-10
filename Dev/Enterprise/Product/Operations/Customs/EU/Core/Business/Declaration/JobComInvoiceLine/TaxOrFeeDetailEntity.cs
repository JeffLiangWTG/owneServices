using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class TaxOrFeeDetailEntity : NonPersistentBusinessObject, ICodeDescription
	{
		public string Code { get; set; }
		public string Description { get; set; }
		public string VATCode { get; set; }
		public ZString AdditionalCode { get; set; }
		public ZString Category { get; set; }
	}
}
