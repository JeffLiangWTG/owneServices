using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class CommodityProviderTest : Customs.Business.Testing.DataProviderTestCase<CommodityProvider>
{
	public void TestDescriptionOfGoods()
	{
		invoiceLine.JI_Description = "Description";
		AssertEquals("Description", Provider.DescriptionOfGoods);
	}

	public void TestCusCode()
	{
		invoiceLine.ZG_CusNumber = "CusNumber";
		AssertEquals("CusNumber", Provider.CusCode);
	}

	public void TestHarmonizedSystemSubHeadingCode()
	{
		AssertNullOrEmpty(provider.HarmonizedSystemSubHeadingCode);
	}

	public void TestCombinedNomenclatureCode()
	{
		AssertNullOrEmpty(provider.CombinedNomenclatureCode);
	}

	public void TestDangerousGoods()
	{
		invoiceLine.UNDGs.AddNew();
		AssertEquals(1, provider.DangerousGoods.Count);
	}

	public void TestGrossMass()
	{
		AssertEquals(0m, provider.GrossMass);
	}

	public void TestNetMass()
	{
		AssertNull(provider.NetMass);
	}

	public void TestSupplementaryQty()
	{
		AssertEquals(0m, provider.SupplementaryQty);
	}

	public void TestCalculationOfTaxes()
	{
		AssertNotNull(provider.CalculationOfTaxes);
	}

	public void TestCommodityCode()
	{
		AssertNotNull(provider.CommodityCode);
	}

	public void TestGoodsMeasure()
	{
		AssertNotNull(provider.GoodsMeasure);
	}

	public void TestInvoiceLine()
	{
		invoiceLine.JI_LinePrice = 123m;
		AssertEquals(123m, provider.InvoiceLine);
	}

	public void TestQuotaOrderNumber()
	{
		invoiceLine.JI_ConcessionOrder = "Quota";
		AssertEquals("Quota", provider.QuotaOrderNumber);
	}

	public void TestTypeOfGoods()
	{
		AssertNull(provider.TypeOfGoods);
	}

	public void TestInvoiceLines()
	{
		invoiceLine.UNDGs.AddNew();
		AssertNull(provider.TypeOfGoods);
	}

	protected override CommodityProvider GetProvider() => provider;

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
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		entryInstruction.CEI_ClusterKey = 1;
		entryInstruction.CEI_JE = declaration.PK;

		var lineMerger = new EU.Business.Declaration.LineMerger(declaration);
		lineMerger.DoMerge();

		provider = new CommodityProvider(invoiceLine);
	}

	JobComInvoiceLine invoiceLine;
	CommodityProvider provider;
}
