using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing;

public class EntryLineVatCalculatorTest : TestCaseWithFactory
{
	public void TestZeroVATCalculated_JI_CessionFlagIs02()
	{
		invoiceLine.JI_CessionFlag = "02";
		var calculatedVat = vatCalculator.CalculateVatFee();

		AssertEquals(ZDecimal.Zero, calculatedVat.Amount);
	}

	public void TestVATCalculated_JI_CessionFlagIs01()
	{
		invoiceLine.JI_CessionFlag = "01";
		var calculatedVat = vatCalculator.CalculateVatFee();

		AssertEquals(220.0m, calculatedVat.Amount);
	}

	public void TestVATCalculated_JI_CessionFlagIs03()
	{
		invoiceLine.JI_CessionFlag = "03";
		var calculatedVat = vatCalculator.CalculateVatFee();

		AssertEquals(220.0m, calculatedVat.Amount);
	}

	protected override void SetUp()
	{
		base.SetUp();

		ordVat = SetUpAndSaveRefData(Factory);

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.MergedLines.AddNew();
		entryLine.CL_ValueForVAT = 1000.0m;

		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		invoiceLine.JI_ZZF_NKTaxType = ordVat.ZZF_Code;

		vatCalculator = (EntryLineVatCalculator)entryLine.GetEntryLineVatCalculator();
	}

	EntryLineVatCalculator vatCalculator;
	CusEntryLine entryLine;
	JobComInvoiceLine invoiceLine;

	Universal.RefCusTaxOrFee ordVat;

	public static Universal.RefCusTaxOrFee SetUpAndSaveRefData(BusinessObjectFactory factory)
	{
		var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		var refDataHelper = new UniversalReferenceTestDataHelper(factory);
		var startDate = ZDateTime.Today.AddYears(-1);
		var endDate = ZDateTime.Today.AddYears(1);
		var ordVat = refDataHelper.CreateTaxOrFee("ORD", 0.22m, currentCountryCode, startDate, endDate);
		var eunDataGrouping = refDataHelper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
		refDataHelper.CreateNewOrGetExistingDataGrouping(currentCountryCode, "", parent: eunDataGrouping);
		factory.Save();
		return ordVat;
	}
}
