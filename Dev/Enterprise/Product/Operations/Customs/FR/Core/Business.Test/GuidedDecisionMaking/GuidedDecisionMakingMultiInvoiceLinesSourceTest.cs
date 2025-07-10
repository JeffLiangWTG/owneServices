using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.GDM.Testing;

public class GuidedDecisionMakingMultiInvoiceLinesSourceTest : TestCaseWithFactory
{
	public void TestRegionOrTerritoryOfDestination()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_RegionOrTerritoryOfDestination = "REG";
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var gdmWrapper = new GuidedDecisionMakingMultiInvoiceLinesSource(invoiceLine);
		AssertEquals("GDM Source Wrapper can get the JE_RegionOrTerritoryOfDestination from declaration", "REG", gdmWrapper.RegionOrTerritoryOfDestination);

		var invoiceLineWithoutDeclaration = Factory.New<JobComInvoiceLine>();
		gdmWrapper = new GuidedDecisionMakingMultiInvoiceLinesSource(invoiceLineWithoutDeclaration);
		AssertEquals("RegionOrTerritoryOfDestination of GDM Source Wrapper is empty if invoice line has no declaration", ZString.Empty, gdmWrapper.RegionOrTerritoryOfDestination);
	}
}
