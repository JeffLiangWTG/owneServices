using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class AccCommissionHeaderCollection : ActiveBusinessObjectCollection<AccCommissionHeader>
	{
		public AccCommissionHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
