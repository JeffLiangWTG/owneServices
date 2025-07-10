using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class CommonGoodsMeasureWithSupUnitsAndSpecifiedWrapperTest : WrapperHelperTest<CommonGoodsMeasureWithSupUnitsAndSpecifiedWrapper>
{
	public void TestSupplementaryUnits()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_CustomsSecondQuantity = 1.1234m;
			AssertEquals("Expected filled SupplementaryUnits with 1 invoice line", 1.1234m, wrapper.SupplementaryUnits);

			var invLine2 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine2.JI_CustomsSecondQuantity = 2.3211m;
			AssertEquals("Expected filled SupplementaryUnits with 2 invoice lines", 3.4445m, wrapper.SupplementaryUnits);

			var invLine3 = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLine3.JI_CustomsSecondQuantity = 1.0000m;
			AssertEquals("Expected filled SupplementaryUnits with 3 invoice lines", 4.4445m, wrapper.SupplementaryUnits);
		});
	}
	public void TestSupplementaryUnitsSpecified()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_CustomsSecondQuantity = 1.1234m;
			AssertEquals("Expected false SupplementaryUnitsSpecified when no UOM", false, wrapper.SupplementaryUnitsSpecified);

			invoiceLine.JI_CustomsSecondUnitQty = "KGM";
			AssertEquals("Expected true SupplementaryUnitsSpecified when UOM declared", true, wrapper.SupplementaryUnitsSpecified);

			invoiceLine.JI_CustomsSecondQuantity = 0m;
			AssertEquals("Expected false SupplementaryUnitsSpecified when UOM declared but amount is 0", false, wrapper.SupplementaryUnitsSpecified);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		var invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();

		AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		wrapper = new CommonGoodsMeasureWithSupUnitsAndSpecifiedWrapper(entryLine);
	}

	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
	CommonGoodsMeasureWithSupUnitsAndSpecifiedWrapper wrapper;

	protected override CommonGoodsMeasureWithSupUnitsAndSpecifiedWrapper GetProvider() => wrapper;
}
