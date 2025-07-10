using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PricingPageLineWrapperCollection : GenericWrapperCollection<PricingPageLineWrapper>
	{
		public PricingPageLineWrapperCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public PricingPageLineWrapperCollection(PricingPage pricingPage, PricingPageRateLineFactory lsFactory, BusinessObjectFactory factory)
			: base(factory)
		{
			int setIndex = 0;

			var helper = new QuotationLineListHelper();

			var sets = lsFactory.LoadLineSets(pricingPage, pricingPage.RateEntries);

			foreach (var set in sets)
			{
				var list = helper.GetLines(set, pricingPage.ViewAgentRates);

				if (list.Count > 0)
				{
					setIndex++;
					int lineIndex = 0;

					foreach (var line in list)
					{
						lineIndex++;
						Add(new PricingPageLineWrapper(pricingPage, line, setIndex, lineIndex, factory));
					}
				}
			}
		}
	}
}
