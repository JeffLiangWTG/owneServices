using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class DutyTaxFeeWrapperTest : DataProviderTestCase<DutyTaxFeeWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new DutyTaxFeeWrapper(null, 1));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals(1, wrapper.SequenceNumeric);
	}

	public void TestTypeCode()
	{
		cusEntryLineFee.CF_ChargeType = "CT1";
		AssertEquals("CT1", wrapper.TypeCode);
	}

	public void TestCCQualifierCode()
	{
		AssertNull(wrapper.CCQualifierCode);
	}

	public void TestPayment()
	{
		CombineAssertions(() =>
		{
			var instruction = cusEntryLineFee.EntryLine.Header.EntryInstruction;
			cusEntryLineFee.G4_MethodOfPayment = "A";

			AssertNotNull(wrapper.Payment);
			AssertType<PaymentWrapper>(wrapper.Payment);
			AssertEquals("CEI_Style is H1", "A", wrapper.Payment.MethodCode);

			instruction.CEI_Style = DeclarationTypeList.Codes.I1;
			AssertEquals("CEI_Style is I1", "A", wrapper.Payment.MethodCode);

			instruction.CEI_Style = DeclarationTypeList.Codes.H5;
			AssertEquals("CEI_Style is H5", "A", wrapper.Payment.MethodCode);

			cusEntryLineFee.G4_MethodOfPayment = "B";
			instruction.CEI_Style = DeclarationTypeList.Codes.H2;
			AssertEquals("CEI_Style isn't H1 or I1 or H5", "B", wrapper.Payment.MethodCode);

			instruction.CEI_Style = DeclarationTypeList.Codes.H1;
			var cusAuthorizationUsage = instruction.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage.AGC_Code = AuthorizationUsageCodeList.Codes._DPO;
			cusAuthorizationUsage.AGC_Number = "1472583";
			cusAuthorizationUsage.AGC_OH_Owner = Factory.New<OrgHeader>().PK;
			AssertEquals("Has related DPO CusAuthorizationUsage", "B", wrapper.Payment.MethodCode);
		});
	}

	public void TestTaxBases()
	{
		CombineAssertions(() =>
		{
			var taxBase = wrapper.TaxBases.Single();
			AssertType<TaxBaseWrapper>("Type", taxBase);
			AssertEquals("SequenceNumeric", 1, taxBase.SequenceNumeric);
		});
	}

	protected override DutyTaxFeeWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		var shutterUpperer = new SendsMessagesToCustomsShutterUpperer(false);
		declaration.DoMerge(shutterUpperer);

		entryHeader = declaration.CustomsEntryHeaders.Cast<Declaration.CusEntryHeader>().FirstOrDefault();
		cusEntryLine = entryHeader.MergedLines.First();
		cusEntryLineFee = cusEntryLine.Fees.AddOrUpdate("CT1", 1m);
		wrapper = new DutyTaxFeeWrapper(cusEntryLineFee, 1);
	}

	Declaration.CusEntryLineFee cusEntryLineFee;
	JobDeclaration declaration;
	Declaration.CusEntryLine cusEntryLine;
	Declaration.CusEntryHeader entryHeader;
	DutyTaxFeeWrapper wrapper;
}
