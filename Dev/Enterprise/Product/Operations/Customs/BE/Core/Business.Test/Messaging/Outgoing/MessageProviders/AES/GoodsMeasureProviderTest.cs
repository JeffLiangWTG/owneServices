using Enterprise.Customs.BE.Business.Declaration;
using CusEntryLine = Enterprise.Customs.BE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class GoodsMeasureProviderTest : Customs.Business.Testing.DataProviderTestCase<GoodsMeasureProvider>
{
	public void TestGrossMass()
	{
		var invoiceLine1 = entryLine.InvoiceLines[0];
		var invoiceLine2 = entryLine.InvoiceLines[1];
		invoiceLine1.JI_Weight = 1512;
		invoiceLine2.JI_Weight = 18;
		invoiceLine1.JI_WeightUQ = "KG";
		invoiceLine2.JI_WeightUQ = "KG";

		AssertEquals(1530m, provider.GrossMass);
	}

	public void TestNetMass()
	{
		var invoiceLine1 = entryLine.InvoiceLines[0];
		var invoiceLine2 = entryLine.InvoiceLines[1];
		invoiceLine1.JI_NetWeight = 1512;
		invoiceLine2.JI_NetWeight = 18;
		invoiceLine1.JI_NetWeightUQ = "KG";
		invoiceLine2.JI_NetWeightUQ = "KG";

		AssertEquals(1530m, provider.NetMass);
	}

	public void TestSupplementaryUnits()
	{
		var invoiceLine1 = entryLine.InvoiceLines[0];
		var invoiceLine2 = entryLine.InvoiceLines[1];
		invoiceLine1.JI_CustomsSecondQuantity = 1512;
		invoiceLine2.JI_CustomsSecondQuantity = 18;

		AssertEquals(1530m, provider.SupplementaryUnits);
	}

	public void TestSupplementaryUnitsCode()
	{
		var invoiceLine1 = entryLine.InvoiceLines[0];
		invoiceLine1.JI_CustomsSecondUnitQty = "UT";

		AssertEquals("UT", provider.SupplementaryUnitsCode);
	}

	protected override GoodsMeasureProvider GetProvider() => provider;
	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_TransportModeInland = "SEA";
		declaration.CusContainers.AddNew();
		declaration.InlandTransports.AddNew();

		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_InvoiceNumber = "ABC123";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = "H1";
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceHeader.InvoiceLines.AddNew();

		entryInstruction.CEI_ClusterKey = 1;
		entryInstruction.CEI_JE = declaration.PK;

		var lineMerger = new EU.Business.Declaration.LineMerger(declaration);
		lineMerger.DoMerge();

		entryLine = (CusEntryLine)invoiceLine.CusEntryLine;

		provider = new GoodsMeasureProvider(entryLine);
	}

	CusEntryLine entryLine;
	GoodsMeasureProvider provider;
}
