using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.GDM.Testing;

public class GuidedDecisionMakingMultiInvoiceLinesTargetTest : TestCaseWithFactory
{
	public void TestRegionOrTerritoryOfDestination()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		var guidedDecisionMakingTarget = new GuidedDecisionMakingMultiInvoiceLinesTarget(new List<EU.Business.Declaration.JobComInvoiceLine> { invoiceLine1, invoiceLine2 });
		guidedDecisionMakingTarget.RegionOrTerritoryOfDestination = "BBB";

		AssertEquals("RegionOrTerritoryOfDestination of declaration can be modified", "BBB", declaration.JE_RegionOrTerritoryOfDestination);
	}

	public void TestCountryOfDestination_NotAutomatedFilling()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = declaration.InvoiceLines.AddNew();
		var invoiceLine2 = declaration.InvoiceLines.AddNew();
		var guidedDecisionMakingTarget = new GuidedDecisionMakingMultiInvoiceLinesTarget(new List<EU.Business.Declaration.JobComInvoiceLine> { invoiceLine1, invoiceLine2 });

		using (var context = new DeclarationValidationDeciderTestContext(declaration, isUCC6: false))
		{
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
			invoiceLine1.ZG_CountryOfDestination = "CN";
			invoiceLine2.ZG_CountryOfDestination = "IT";
			guidedDecisionMakingTarget.CountryOfDestination = "FR";
			AssertEquals("Line 1 - In UCC5, the GDM should automatically fill the ZG_CountryOfDestination field in FR", "FR", invoiceLine1.ZG_CountryOfDestination);
			AssertEquals("Line 2 - In UCC5, the GDM should automatically fill the ZG_CountryOfDestination field in FR", "FR", invoiceLine2.ZG_CountryOfDestination);
			declaration.JE_GoodsDestination = ZString.Empty;
			invoiceLine1.ZG_CountryOfDestination = "CN";
			invoiceLine2.ZG_CountryOfDestination = "IT";
			guidedDecisionMakingTarget.CountryOfDestination = "FR";
			AssertEquals("Line 1 - In UCC5, the GDM should automatically fill the ZG_CountryOfDestination field in FR", "FR", invoiceLine1.ZG_CountryOfDestination);
			AssertEquals("Line 2 - In UCC5, the GDM should automatically fill the ZG_CountryOfDestination field in FR", "FR", invoiceLine2.ZG_CountryOfDestination);
		}

		using (var context = new DeclarationValidationDeciderTestContext(declaration, isUCC6: true))
		{
			context.EnableRule(r => r.IsRuleC0002Active);

			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
			invoiceLine1.ZG_CountryOfDestination = "CN";
			invoiceLine2.ZG_CountryOfDestination = "IT";
			guidedDecisionMakingTarget.CountryOfDestination = "FR";
			AssertEquals("Line 1 - If UCC6 is specified and JE_GoodsDestination is not empty, GDM should not automatically fill the ZG_CountryOfDestination in FR", "CN", invoiceLine1.ZG_CountryOfDestination);
			AssertEquals("Line 2 - If UCC6 is specified and JE_GoodsDestination is not empty, GDM should not automatically fill the ZG_CountryOfDestination in FR", "IT", invoiceLine2.ZG_CountryOfDestination);
			declaration.JE_GoodsDestination = ZString.Empty;
			invoiceLine1.ZG_CountryOfDestination = "CN";
			invoiceLine2.ZG_CountryOfDestination = "IT";
			guidedDecisionMakingTarget.CountryOfDestination = "FR";
			AssertEquals("Line 1 - If UCC6 is specified and JE_GoodsDestination is empty, GDM should automatically fill the ZG_CountryOfDestination in FR", "FR", invoiceLine1.ZG_CountryOfDestination);
			AssertEquals("Line 2 - If UCC6 is specified and JE_GoodsDestination is empty, GDM should automatically fill the ZG_CountryOfDestination in FR", "FR", invoiceLine2.ZG_CountryOfDestination);

			context.DisableRule(r => r.IsRuleC0002Active);
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
			invoiceLine1.ZG_CountryOfDestination = "CN";
			invoiceLine2.ZG_CountryOfDestination = "IT";
			guidedDecisionMakingTarget.CountryOfDestination = "FR";
			AssertEquals("Line 1 - When C0002 is not active, the GDM should automatically fill the ZG_CountryOfDestination field in FR", "FR", invoiceLine1.ZG_CountryOfDestination);
			AssertEquals("Line 2 - When C0002 is not active, the GDM should automatically fill the ZG_CountryOfDestination field in FR", "FR", invoiceLine2.ZG_CountryOfDestination);
			declaration.JE_GoodsDestination = ZString.Empty;
			invoiceLine1.ZG_CountryOfDestination = "CN";
			invoiceLine2.ZG_CountryOfDestination = "IT";
			guidedDecisionMakingTarget.CountryOfDestination = "FR";
			AssertEquals("Line 1 - When C0002 is not active, the GDM should automatically fill the ZG_CountryOfDestination field in FR", "FR", invoiceLine1.ZG_CountryOfDestination);
			AssertEquals("Line 2 - When C0002 is not active, the GDM should automatically fill the ZG_CountryOfDestination field in FR", "FR", invoiceLine2.ZG_CountryOfDestination);
		}
	}
}
