using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class DutyAndTaxForDisplayCollection : BusinessObjectCollection<DutyAndTax>
	{
		public DutyAndTaxForDisplayCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
