using CargoWise.EntityFramework;
using Enterprise.CommissionManagement.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocViewCommissionLineGroupingCollection : DocBaseWrapperCollection<DocViewCommissionLineGrouping>
	{
		public DocViewCommissionLineGroupingCollection(ViewCommissionLineGroupingCollection collectionToWrap, BusinessObjectFactory factory)
			: base(collectionToWrap, factory)
		{
		}
	}
}
