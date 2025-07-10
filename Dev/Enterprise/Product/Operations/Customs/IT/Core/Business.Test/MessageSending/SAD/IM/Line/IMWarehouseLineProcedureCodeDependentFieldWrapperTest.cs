using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IMWarehouseLineProcedureCodeDependentFieldWrapperTest : IMLineWrapperTest
{
	public override void TestPreferences()
	{
		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		entryInstruction.CEI_Procedure = "71";
		invoiceLine1.JI_PrimaryPreference = "100";
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Preferences should be", "", sadLineWrapper.Preferences);
	}

	public override void TestQuotas()
	{
		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		entryInstruction.CEI_Procedure = "71";
		invoiceLine1.JI_ConcessionOrder = "123456";
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertNull(sadLineWrapper.Quotas);
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

		entryInstruction.CEI_Procedure = "71";
		invoice1.JZ_RX_NKInvoice_Currency = "AUD";
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertNull("Item Price EURO should be", sadLineWrapper.ItemPriceEuro);//2.04 exchange rate

		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertNull("Item Price EURO should be", sadLineWrapper.ItemPriceEuro);
	}

	public override void TestAdjustmentInEuro()
	{
		var invoiceLine1 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine1.JI_ValuationCode = "1";
		var invoiceLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
		invoiceLine2.JI_ValuationCode = "2";
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertEquals("Evaluation Method should be", "1", sadLineWrapper.EvaluationMethod);

		entryInstruction.CEI_Procedure = "71";
		entryLine.ZG_AdjustmentAmount = 100.12m;
		sadLineWrapper = GetLineWrapper(entryLine);
		AssertNull("Adjusment in EURO should be", sadLineWrapper.AdjustmentInEuro);
	}

	protected override IMLineWrapper GetLineWrapper(CusEntryLine entryLine)
	{
		return new IMWarehouseLineProcedureCodeDependentFieldWrapper(entryLine);
	}

	protected override void SetUp()
	{
		base.SetUp();
		SetUpRefData();
	}
}
