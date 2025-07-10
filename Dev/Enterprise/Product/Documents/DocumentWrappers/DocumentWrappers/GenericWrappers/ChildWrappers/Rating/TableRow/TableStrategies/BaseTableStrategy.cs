using CargoWise.EntityFramework;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	abstract class BaseTableStrategy
	{
		protected BaseTableStrategy(BusinessObjectFactory factory, PricingPageRateLineFactory lsFactory)
		{
			Factory = factory;
			PricingPageRateLineFactory = lsFactory;
		}

		public abstract PricingPageTableRowWrapperCollection Extract(PricingPage pricingPage);

		public BusinessObjectFactory Factory { get; }
		public PricingPageRateLineFactory PricingPageRateLineFactory { get; }
	}
}
