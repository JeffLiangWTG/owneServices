using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PricingPageSetWrapperCollection))]
	sealed class PricingPageSetWrapperCollectionTest : GenericWrapperCollectionTest<PricingPageSetWrapperCollection>
	{
		public void TestViewAgentRates_PricingPage()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
			var tariff = Factory.NewWithValidTestData<CompanyTariff>();
			var fclLine = tariff.AddRateEntry("FCL", "SEA", "AUBNE", "NLAMS", "STD", "20GP").RateLines[0];
			fclLine.GetCalculator<UnitCalculator>().PerUnit = 150m;

			var lclLine = tariff.AddRateEntry("LCL", "LCL", "AUBNE", "NLAMS", "STD", "20GP").RateLines[0];
			lclLine.GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 10m;

			var collection = new PricingPageSetWrapperCollection(tariff, "Pricing Page", Factory);

			var standardPageWrappers = collection["ForwardingStandard"].PricingPages.Cast<PricingPageWrapper>().ToArray();
			AssertEquals("Forwarding Standard Pricing Pages should generate a page per forwarding category", 2, standardPageWrappers.Length);
			AssertEquals(false, GetPricingPage(standardPageWrappers[0]).ViewAgentRates);
			AssertEquals(false, GetPricingPage(standardPageWrappers[1]).ViewAgentRates);

			var landscapePageWrappers = collection["ForwardingLandscapeSimple"].PricingPages.Cast<PricingPageWrapper>().ToArray();
			AssertEquals("Forwarding Landscape Pricing Pages should generate a single page", 1, landscapePageWrappers.Length);
			AssertEquals(false, GetPricingPage(landscapePageWrappers[0]).ViewAgentRates);
		}

		public void TestViewAgentRates_AgentPricingPage()
		{
			var tariff = Factory.NewWithValidTestData<CompanyTariff>();
			var fclLine = tariff.AddRateEntry("FCL", "SEA", "AUBNE", "NLAMS", "STD", "20GP").RateLines[0];
			fclLine.ViewAgentRates = true;
			fclLine.GetCalculator<UnitCalculator>().PerUnit = 150m;

			var lclLine = tariff.AddRateEntry("LCL", "LCL", "AUBNE", "NLAMS", "STD", "20GP").RateLines[0];
			lclLine.ViewAgentRates = true;
			lclLine.GetCalculator<MinimumOrPerUnitCalculator>().PerUnit = 10m;

			var collection = new PricingPageSetWrapperCollection(tariff, "Agent Pricing Page", Factory);

			var standardPageWrappers = collection["ForwardingStandard"].PricingPages.Cast<PricingPageWrapper>().ToArray();
			AssertEquals(true, GetPricingPage(standardPageWrappers[0]).ViewAgentRates);
			AssertEquals(true, GetPricingPage(standardPageWrappers[1]).ViewAgentRates);

			var landscapePageWrappers = collection["ForwardingLandscapeSimple"].PricingPages.Cast<PricingPageWrapper>().ToArray();
			AssertEquals(true, GetPricingPage(landscapePageWrappers[0]).ViewAgentRates);
		}

		#region Implementation

		PricingPage GetPricingPage(PricingPageWrapper wrapper) { return (PricingPage)wrapper.WrappedObject; }

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			var tariff = Factory.New<CompanyTariff>();
			var entry = tariff.AddRateEntryWithFlatRateLine("LCL", "LCL", "AUBNE", "NLAMS", "FRT", 150m);

			return new PricingPageSetWrapper(tariff, PricingPaginationStrategy.ShippingCategoryFilter | PricingPaginationStrategy.StandardStyle, Factory);
		}

		protected override PricingPageSetWrapperCollection GetNewDocumentWrapperCollection()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();

			return new PricingPageSetWrapperCollection(tariff, "Pricing Page", Factory);
		}

		#endregion
	}
}
