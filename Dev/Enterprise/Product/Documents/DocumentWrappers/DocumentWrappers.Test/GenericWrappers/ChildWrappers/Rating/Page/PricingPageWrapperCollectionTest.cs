using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PricingPageWrapperCollection))]
	sealed class PricingPageWrapperCollectionTest : GenericWrapperCollectionTest<PricingPageWrapperCollection>
	{
		#region Implementation

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			RateEntry entry = tariff.AddRateEntry("FCL", "ALL", "AUBNE", "NLAMS", "STD", "20GP");

			return new PricingPageWrapper(new PricingPage(entry, Factory, PricingPageStyle.Standard), 1, Factory);
		}

		protected override PricingPageWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new PricingPageWrapperCollection(Factory);
		}

		#endregion
	}
}
