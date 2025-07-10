using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Money
{
	public class CostWrapperCollection : GenericWrapperCollection<CostWrapper>
	{
		public CostWrapperCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public CostWrapperCollection(JobConsolCostCollection costCollection, BusinessObjectFactory factory)
			: base(factory)
		{
			costCollection.Load();
			foreach (JobConsolCost cost in costCollection)
			{
				Add(new CostWrapper(cost, factory));
			}
		}
	}
}
