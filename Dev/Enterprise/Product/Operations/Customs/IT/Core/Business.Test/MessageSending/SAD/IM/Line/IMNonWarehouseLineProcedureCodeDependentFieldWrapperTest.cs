using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IMNonWarehouseLineProcedureCodeDependentFieldWrapperTest : IMLineWrapperTest
{
	public override void TestPreferences()
	{
		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		entryInstruction.CEI_Procedure = "40";
		invoiceLine1.JI_PrimaryPreference = "100";
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Preferences should be", "100", sadLineWrapper.Preferences);
	}

	public override void TestQuotas()
	{
		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		entryInstruction.CEI_Procedure = "40";
		invoiceLine1.JI_ConcessionOrder = "123456";

		sadLineWrapper = GetLineWrapper(entryLine);
		AssertArrayEqualsByElements("Quotas should be", new ZString[] { "123456" }, sadLineWrapper.Quotas.ToArray());

		invoiceLine1.JI_ConcessionOrder = "";
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertArrayEqualsByElements("Quotas should be", System.Array.Empty<ZString>(), sadLineWrapper.Quotas.ToArray());
	}

	public override void TestItemPriceInEuro()
	{
		jobDeclaration.JE_ExportDate = ZDate.Today;
		var invoice1 = jobDeclaration.Invoices.AddNew();
		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine1.JI_JZ = invoice1.PK;
		invoiceLine1.JI_LinePrice = 100m;
		var invoiceLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine2.JI_JZ = invoice1.PK;
		invoiceLine2.JI_LinePrice = 200.14m;

		entryInstruction.CEI_Procedure = "40";
		invoice1.JZ_RX_NKInvoice_Currency = "AUD";
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Item Price EURO should be", 147.13m, sadLineWrapper.ItemPriceEuro);//2.04 exchange rate

		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Item Price EURO should be", 300.14m, sadLineWrapper.ItemPriceEuro);
	}

	public override void TestAdjustmentInEuro()
	{
		entryInstruction.CEI_Procedure = "40";
		entryLine.ZG_AdjustmentAmount = 100.12m;
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Adjusment in EURO should be", 100.12m, sadLineWrapper.AdjustmentInEuro);
	}

	protected override IMLineWrapper GetLineWrapper(CusEntryLine entryLine)
	{
		return new IMNonWarehouseLineProcedureCodeDependentFieldWrapper(entryLine);
	}

	protected override void SetUp()
	{
		base.SetUp();
		SetUpRefData();
	}
}
