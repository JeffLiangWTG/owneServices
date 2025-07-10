using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PricingPageTableSubRowWrapperCollection : GenericWrapperCollection<PricingPageTableSubRowWrapper>
	{
		public PricingPageTableSubRowWrapperCollection(BusinessObjectFactory factory)
			: base(factory) { }
	}
}
