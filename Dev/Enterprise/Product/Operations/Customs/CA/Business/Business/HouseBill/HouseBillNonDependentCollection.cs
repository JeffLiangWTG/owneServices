using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class BillNonDependentCollection : BusinessObjectCollection<Bill>
	{
		public BillNonDependentCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
