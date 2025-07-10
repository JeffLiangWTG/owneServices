using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class EUUniversalCusEntryLineFeeValidationTestBaseOnly : EUUniversalCusEntryLineFeeValidationTest<JobDeclaration, CusEntryLine, CusEntryLineFee>
	{
		protected override void SetUp()
		{
			declarationConfigurationSetup = ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, true);
			base.SetUp();
		}
		IDisposable declarationConfigurationSetup;

		protected override void TearDown()
		{
			base.TearDown();
			declarationConfigurationSetup?.Dispose();
		}

		public void TestCheckCF_BaseValue_MandatoryValidation()
		{
			var (declaration, entryLine, entryLineFee) = SetEntryLineFeeData();
			CombineAssertions(() =>
			{
				entryLineFee.CF_BaseValue = 0;
				AssertHasMessageErrorContaining("Base Amount empty", entryLineFee.CF_BaseValueInfo, MandatoryValidation.YouHaveNotEntered);

				entryLineFee.CF_BaseValue = 1;
				AssertNoMessageErrorContaining("Base Amount not empty", entryLineFee.CF_BaseValueInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}

	public abstract class EUUniversalCusEntryLineFeeValidationTest<TDeclaration, TEntryLine, TEntryLineFee> : BusinessObjectValidationTestCase
		where TDeclaration : JobDeclaration
		where TEntryLine : CusEntryLine
		where TEntryLineFee : CusEntryLineFee
	{
		public void TestCheckNationalFeeTypeCode()
		{
			(TDeclaration declaration, TEntryLine entryLine, TEntryLineFee entryLineFee) = SetEntryLineFeeData();
			if (entryLineFee.Lookups.NationalFeeTypeCodeList.Count > 0)
			{
				entryLineFee.NationalFeeTypeCode = ZString.Empty;
				AssertNoMessageErrorContaining(entryLineFee.NationalFeeTypeCodeInfo, "The code you have selected is not in the list");

				entryLineFee.NationalFeeTypeCode = entryLineFee.Lookups.NationalFeeTypeCodeList[0].Code;
				AssertNoMessageErrorContaining(entryLineFee.NationalFeeTypeCodeInfo, "The code you have selected is not in the list");

				entryLineFee.NationalFeeTypeCode = "XXXX";
				AssertHasMessageErrorContaining(entryLineFee.NationalFeeTypeCodeInfo, "The code you have selected is not in the list");
			}
			else
			{
				entryLineFee.NationalFeeTypeCode = ZString.Empty;
				AssertEquals(false, entryLineFee.NationalFeeTypeCodeInfo.HasNotifications());
				entryLineFee.NationalFeeTypeCode = "XXXX";
				AssertEquals(false, entryLineFee.NationalFeeTypeCodeInfo.HasNotifications());
			}
		}

		public void TestCheckCF_BaseValue_NegativeValidation()
		{
			(TDeclaration declaration, TEntryLine entryLine, TEntryLineFee entryLineFee) = SetEntryLineFeeData();
			AssertNegativeAmountValidation(entryLineFee.CF_BaseValueInfo);
		}

		public virtual void TestCheckCF_ChargeAmount_CannotBeNegative()
		{
			(TDeclaration declaration, TEntryLine entryLine, TEntryLineFee entryLineFee) = SetEntryLineFeeData();
			AssertNegativeAmountValidation(entryLineFee.CF_ChargeAmountInfo);
		}

		void AssertNegativeAmountValidation(ZPropertyInfo amountPtyInfo)
		{
			var fee = (CusEntryLineFee)amountPtyInfo.BizObj;

			fee[amountPtyInfo.Name] = 0;
			AssertNoMessageErrorContaining(amountPtyInfo, MandatoryValidation.ValueCannotBeNegative);

			fee[amountPtyInfo.Name] = -1;
			AssertHasMessageErrorContaining(amountPtyInfo, MandatoryValidation.ValueCannotBeNegative);

			fee[amountPtyInfo.Name] = 1;
			AssertNoMessageErrorContaining(amountPtyInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public virtual void TestCheckCF_ChargeType()
		{
			(TDeclaration declaration, TEntryLine entryLine, TEntryLineFee entryLineFee) = SetEntryLineFeeData();
			entryLineFee.CF_ChargeType = ZString.Empty;
			AssertHasMessageErrorContaining(entryLineFee.CF_ChargeTypeInfo, "You have not entered a Charge Type.");

			entryLineFee.CF_ChargeType = "INV";
			AssertHasMessageErrorContaining(entryLineFee.CF_ChargeTypeInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckCF_RateOverrideReasonCode()
		{
			(TDeclaration declaration, TEntryLine entryLine, TEntryLineFee entryLineFee) = SetEntryLineFeeData();
			entryLineFee.CF_RateOverrideReasonCode = ZString.Empty;
			AssertNoNotifications(entryLineFee.CF_RateOverrideReasonCodeInfo);

			entryLineFee.CF_RateOverrideReasonCode = "INV";
			AssertHasMessageErrorContaining(entryLineFee.CF_RateOverrideReasonCodeInfo, "The code you have selected is not in the list.");

			entryLineFee.CF_RateOverrideReasonCode = "ADD";
			AssertNoMessageErrorContaining(entryLineFee.CF_RateOverrideReasonCodeInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckCF_MethodOfPayment()
		{
			(TDeclaration declaration, TEntryLine entryLine, TEntryLineFee entryLineFee) = SetEntryLineFeeData();
			entryLineFee.CF_MethodOfPayment = ZString.Empty;
			AssertNotificationsForEmptyMethodOfPayment(entryLineFee);

			entryLineFee.CF_MethodOfPayment = InvalidMethodOfPaymentForListValidationTest;
			AssertHasMessageErrorContaining(entryLineFee.CF_MethodOfPaymentInfo, "The code you have selected is not in the list.");
		}

		protected virtual string InvalidMethodOfPaymentForListValidationTest => "ZZ";

		protected virtual void AssertNotificationsForEmptyMethodOfPayment(CusEntryLineFee entryLineFee) => AssertNoNotifications(entryLineFee.CF_MethodOfPaymentInfo);

		public void TestCheckCF_Rate()
		{
			(TDeclaration declaration, TEntryLine entryLine, TEntryLineFee entryLineFee) = SetEntryLineFeeData();
			var expectedMessageError = "Tax Rate allows only 6 decimal places.";
			entryLineFee.CF_Rate = 0m;
			AssertNoMessageErrorContaining(entryLineFee.CF_RateInfo, expectedMessageError);

			entryLineFee.CF_Rate = 0.12345m;
			AssertNoMessageErrorContaining(entryLineFee.CF_RateInfo, expectedMessageError);

			entryLineFee.CF_Rate = 0.1234567m;
			AssertHasMessageErrorContaining(entryLineFee.CF_RateInfo, expectedMessageError);
		}

		protected virtual (TDeclaration declaration, TEntryLine entryLine, TEntryLineFee entryLineFee) SetEntryLineFeeData()
		{
			var declaration = Factory.New<TDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryLine = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().First().MergedLines.Cast<TEntryLine>()
				.First();

			var entryLineFee = (TEntryLineFee)entryLine.Fees.AddNew();
			return (declaration, entryLine, entryLineFee);
		}
	}
}
