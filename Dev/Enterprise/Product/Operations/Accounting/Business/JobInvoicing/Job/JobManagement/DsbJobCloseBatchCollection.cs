using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class DsbJobCloseBatchCollection : ActiveBusinessObjectCollection<DsbJobCloseBatch>
	{
		public DsbJobCloseBatchCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DsbJobCloseBatchCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(DsbJobCloseBatchSchema.JBB_GC, GlbCompany.CurrentCompany.PK);
			return result;
		}
	}
}
