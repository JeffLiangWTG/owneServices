using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PricingPageTableRowWrapperCollection : GenericWrapperCollection<PricingPageTableRowWrapper>
	{
		public PricingPageTableRowWrapperCollection(BusinessObjectFactory factory)
			: base(factory) { }
	}
}
