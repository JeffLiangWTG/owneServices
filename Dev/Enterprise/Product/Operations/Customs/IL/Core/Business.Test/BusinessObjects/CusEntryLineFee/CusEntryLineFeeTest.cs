using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(CusEntryLineFee))]
	class CusEntryLineFeeTest : Customs.Business.Testing.CusEntryLineFeeTest
	{
		/***
		 * !!! Test to be removed in production code !!!
		 * Please remove the whole method TestTypeDecider from the production code once the following test case passes.
		 * It is only meant as an initial completeness check immdiately after the country project is set up.
		 ***/
		public void TestTypeDecider()
		{
			AssertType<CusEntryLineFee>("Update Customs.Business.CusEntryLineFee to include a decider for this class", Factory.New(typeof(Customs.Business.CusEntryLineFee)));
		}

		public void TestLookups()
		{
			AssertType(typeof(CusEntryLineFeeLookups), fee.Lookups);
		}

		public void TestValidation()
		{
			AssertType(typeof(CusEntryLineFeeValidation), fee.Validation);
		}

		public void TestSetDefaultValues()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var fee = entryLine.Fees.AddNew();

			AssertEquals("CF_RateOverrideReasonCode should be set to ADD", ILRateOverrideReasonList.Codes.Additional, fee.CF_RateOverrideReasonCode);
		}

		public void TestChargeTypeDescription()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var fee = entryLine.Fees.AddNew();
			fee.CF_ChargeType = "ADD";

			var description = fee.ChargeTypeDescription;
			AssertNotNull("ChargeTypeDescription should not be null", description);
			AssertEquals("ChargeTypeDescription should match the expected description", "Other Additional Charges", description);
		}

		public void TestIncludeForVatCalculation_Default()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var fee = entryLine.Fees.AddNew();

			Assert("IncludeForVatCalculation should be true by default", fee.IncludeForVatCalculation);
		}

		public void TestIsNationalIndirectTaxationFee()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var fee = entryLine.Fees.AddNew();
			fee.CF_ChargeType = "12345";

			Assert("IsNationalIndirectTaxationFee should be true for charge types starting with a digit", fee.IsNationalIndirectTaxationFee);
			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_ChargeType = "ABCDE";

			Assert("IsNationalIndirectTaxationFee should be false for charge types not starting with a digit", !fee2.IsNationalIndirectTaxationFee);
		}

		public void TestIsSystemAddedVatFee()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var fee = entryLine.Fees.AddNew();
			fee.CF_ChargeType = Constants.EntryLineFee.VATFeeTypeCode;
			fee.CF_RateOverrideReasonCode = ZString.Empty;

			Assert("IsSystemAddedVatFee should be true for VAT charge type with empty rate override reason code", fee.IsSystemAddedVatFee);
			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_ChargeType = "A00";
			fee2.CF_RateOverrideReasonCode = ZString.Empty;

			Assert("IsSystemAddedVatFee should be false for non-VAT charge type", !fee2.IsSystemAddedVatFee);
		}
		public void TestListAttributes()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(CusEntryLineFee), "CF_ChargeType", false, attrib => attrib.ListDataSourceMember == "Lookups.ChargeTypeList");
		}

		public void TestIsActionBlank()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var fee = entryLine.Fees.AddNew();
			fee.CF_RateOverrideReasonCode = ZString.Empty;

			Assert("IsActionBlank should be true when CF_RateOverrideReasonCode is empty", fee.IsActionBlank);

			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_RateOverrideReasonCode = "A00";

			Assert("IsActionBlank should be false when CF_RateOverrideReasonCode is not empty", !fee2.IsActionBlank);
		}
		public void TestDefaultMethodOfPaymentIfEmpty()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var fee = entryLine.Fees.AddNew();

			fee.DefaultMethodOfPaymentIfEmpty();

			AssertEquals("CF_MethodOfPayment should be set to ZString.Empty when it is empty", ZString.Empty, fee.CF_MethodOfPayment);

			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_MethodOfPayment = "A01";

			fee2.DefaultMethodOfPaymentIfEmpty();

			AssertEquals("CF_MethodOfPayment should remain unchanged when it is not empty", "A01", fee2.CF_MethodOfPayment);
		}

		public void TestUserEnteredStashSource_NotNull()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var fee = entryLine.Fees.AddNew();
			var userEnteredStashSource = (CusEntryLineFeeUserEnteredStashSource)fee.UserEnteredStashSource;
			AssertNotNull("(CusEntryLineFeeUserEnteredStashSource)UserEnteredStashSource should not be null", userEnteredStashSource);
		}

		protected override void SetUp()
		{
			fee = NewTestObjects(Factory).fee;
		}
		CusEntryLineFee fee;

		static (JobDeclaration declaration, CusEntryLineFee fee) NewTestObjects(BusinessObjectFactory factory, string messageType = "IMP")
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_PaymentMethod = "A";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var fee = entryLine.Fees.AddNew();
			return (declaration, fee);
		}
	}
}
