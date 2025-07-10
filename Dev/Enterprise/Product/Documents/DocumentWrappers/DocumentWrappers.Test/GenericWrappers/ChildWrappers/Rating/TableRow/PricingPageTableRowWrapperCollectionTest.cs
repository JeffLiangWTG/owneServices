using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PricingPageTableRowWrapperCollection))]
	sealed class PricingPageTableRowWrapperCollectionTest : GenericWrapperCollectionTest<PricingPageTableRowWrapperCollection>
	{
		#region Implementation

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			var tariff = Factory.New<CompanyTariff>();
			var entry = tariff.AddRateEntry(RatingConstants.RateCategory.FCL);
			return new PricingPageTableRowWrapper(new RateEntry[] { entry }, Factory);
		}

		protected override PricingPageTableRowWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new PricingPageTableRowWrapperCollection(Factory);
		}

		#endregion
	}
}
