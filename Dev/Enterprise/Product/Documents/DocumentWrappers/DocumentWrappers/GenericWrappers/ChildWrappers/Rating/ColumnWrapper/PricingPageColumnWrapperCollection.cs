using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PricingPageColumnWrapperCollection : GenericWrapperCollection<PricingPageColumnWrapper>
	{
		public PricingPageColumnWrapperCollection(BusinessObjectFactory factory)
			: base(factory) { }
	}
}
