using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PricingPageTableSubRowWrapperCollection))]
	sealed class PricingPageTableSubRowWrapperCollectionTest : GenericWrapperCollectionTest<PricingPageTableSubRowWrapperCollection>
	{
		#region Implementation

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			RateEntry entry = tariff.AddRateEntry(RatingConstants.RateCategory.FCL);

			PricingPage page = new PricingPage(entry, Factory, PricingPageStyle.Landscape);

			return new PricingPageTableSubRowWrapper(Factory);
		}

		protected override PricingPageTableSubRowWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new PricingPageTableSubRowWrapperCollection(Factory);
		}

		#endregion
	}
}
