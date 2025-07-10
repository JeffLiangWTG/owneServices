using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.GDM.Testing
{
	sealed class GuidedDecisionMakingSingleInvoiceLineTargetTest : TestCaseWithFactory
	{
		public void TestRegionOrTerritoryOfDestination()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceLine);
			guidedDecisionMakingTarget.RegionOrTerritoryOfDestination = "BBB";

			AssertEquals("RegionOrTerritoryOfDestination of declaration can be modified", "BBB", declaration.JE_RegionOrTerritoryOfDestination);
		}

		public void TestCountryOfDestination_NotAutomatedFilling()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var guidedDecisionMakingTarget = new GuidedDecisionMakingSingleInvoiceLineTarget(invoiceLine);

			using (var context = new DeclarationValidationDeciderTestContext(declaration, isUCC6: false))
			{
				declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
				invoiceLine.ZG_CountryOfDestination = "CN";
				guidedDecisionMakingTarget.CountryOfDestination = "FR";
				AssertEquals("In UCC5, the GDM should automatically fill the ZG_CountryOfDestination field in FR", "FR", invoiceLine.ZG_CountryOfDestination);
				declaration.JE_GoodsDestination = ZString.Empty;
				invoiceLine.ZG_CountryOfDestination = "CN";
				guidedDecisionMakingTarget.CountryOfDestination = "FR";
				AssertEquals("In UCC5, the GDM should automatically fill the ZG_CountryOfDestination field in FR", "FR", invoiceLine.ZG_CountryOfDestination);
			}

			using (var context = new DeclarationValidationDeciderTestContext(declaration, isUCC6: true))
			{
				context.EnableRule(r => r.IsRuleC0002Active);

				declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
				invoiceLine.ZG_CountryOfDestination = "CN";
				guidedDecisionMakingTarget.CountryOfDestination = "FR";
				AssertEquals("If UCC6 is specified and JE_GoodsDestination is not empty, GDM should not automatically fill the ZG_CountryOfDestination in FR", "CN", invoiceLine.ZG_CountryOfDestination);
				declaration.JE_GoodsDestination = ZString.Empty;
				invoiceLine.ZG_CountryOfDestination = "CN";
				guidedDecisionMakingTarget.CountryOfDestination = "FR";
				AssertEquals("If UCC6 is specified and JE_GoodsDestination is empty, GDM should automatically fill the ZG_CountryOfDestination in FR", "FR", invoiceLine.ZG_CountryOfDestination);

				context.DisableRule(r => r.IsRuleC0002Active);
				declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
				invoiceLine.ZG_CountryOfDestination = "CN";
				guidedDecisionMakingTarget.CountryOfDestination = "FR";
				AssertEquals("When C0002 is not active, the GDM should automatically fill the ZG_CountryOfDestination field in FR", "FR", invoiceLine.ZG_CountryOfDestination);
				declaration.JE_GoodsDestination = ZString.Empty;
				invoiceLine.ZG_CountryOfDestination = "CN";
				guidedDecisionMakingTarget.CountryOfDestination = "FR";
				AssertEquals("When C0002 is not active, the GDM should automatically fill the ZG_CountryOfDestination field in FR", "FR", invoiceLine.ZG_CountryOfDestination);
			}
		}
	}
}
