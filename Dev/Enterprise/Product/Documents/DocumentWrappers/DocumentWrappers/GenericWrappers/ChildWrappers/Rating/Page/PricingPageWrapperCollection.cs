using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PricingPageWrapperCollection : GenericWrapperCollection<PricingPageWrapper>
	{
		public PricingPageWrapperCollection(BusinessObjectFactory factory)
			: base(factory) { }

		protected override DocumentWrapper WrapObject(object objectToWrap)
		{
			return new PricingPageWrapper((PricingPage)objectToWrap, 0, Factory);
		}
	}
}
