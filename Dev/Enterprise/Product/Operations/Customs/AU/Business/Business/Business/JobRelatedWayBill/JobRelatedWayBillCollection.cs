using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobRelatedWayBillCollection : BusinessObjectCollection<JobRelatedWayBill>
	{
		public JobRelatedWayBillCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
