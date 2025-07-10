using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business.Customs.EU;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class CustomsValuationWrapperTest : DataProviderTestCase<CustomsValuationWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CustomsValuationWrapper(null));
	}

	public void TestMethodCode()
	{
		invLine.JI_ValuationCode = "1";
		AssertEquals("1", wrapper.MethodCode);
	}

	public void TestChargeDeductions()
	{
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invLine.JI_CEI = entryInstruction.PK;
		CombineAssertions(() =>
		{
			var invoiceLineCharge1 = invLine.Charges.AddNew();
			var invoiceLineCharge2 = invLine.Charges.AddNew();
			AssertEquals("EntryInstruction CEI_Style not H1 or H5", 0, wrapper.ChargeDeductions.Count);

			entryInstruction.CEI_Style = DeclarationTypeList.Codes.H1;
			AssertEquals("EntryInstruction CEI_Style is H1 but IsHighValueOvrd is 'N'", 0, wrapper.ChargeDeductions.Count);

			entryInstruction.ZG_IsHighValueOvrd = true;
			AssertEquals("EntryInstruction CEI_Style is H1, IsHighValueOvrd is 'Y' but Valuation Code not 1", 0, wrapper.ChargeDeductions.Count);

			invLine.JI_ValuationCode = ValuationMethodList.Codes._1;
			wrapper = new CustomsValuationWrapper(invLine);
			AssertEquals("EntryInstruction CEI_Style is H1, IsHighValueOvrd is 'Y' and Valuation Code is 1", 2, wrapper.ChargeDeductions.Count);

			var secondChargeDeduction = Provider.ChargeDeductions.ElementAt(1);
			AssertType<ChargeDeductionWrapper>("Type", secondChargeDeduction);
			AssertEquals("SequenceNumeric ", 2, secondChargeDeduction.SequenceNumeric);
		});
	}

	protected override CustomsValuationWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_JE = declaration.PK;
		invLine = invoice.InvoiceLines.AddNew();
		wrapper = new CustomsValuationWrapper(invLine);
	}
	CustomsValuationWrapper wrapper;
	JobComInvoiceLine invLine;
	JobDeclaration declaration;
}
