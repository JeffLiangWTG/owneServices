using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PricingPageCompoundLineWrapperCollection : GenericWrapperCollection<PricingPageCompoundLineWrapper>
	{
		public PricingPageCompoundLineWrapperCollection(BusinessObjectFactory factory)
			: base(factory) { }
	}
}
