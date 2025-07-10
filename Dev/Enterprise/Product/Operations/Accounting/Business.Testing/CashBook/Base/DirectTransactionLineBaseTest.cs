using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	abstract class DirectTransactionLineBaseTest : DependentTransactionLineTest
	{
		public override void TestAL_JHValidationOnAL_RevRecognitionType()
		{
			Assert("Test is not applicable as revenue recognition validation is not run for this line type", true);
		}

		public void TestAG_Description()
		{
			AccGLHeader testGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			testGLHeader.AG_Description = "test";

			TestBizO.AL_AG = testGLHeader.PK;
			AssertEquals("test", testGLHeader.AG_Description);
		}

		public void TestAL_ExchangeRate()
		{
			TestBizO.AL_OSExTaxAmount = 10m;
			TestBizO.AL_AT = TestObjectCreator.GST1.PK;
			TestBizO.AL_ExchangeRate = 0.5m;

			AssertEquals(20m, TestBizO.AL_LocalExTaxAmount);
			AssertEquals(2m, TestBizO.AL_LocalTaxAmount);
			AssertEquals(1m, TestBizO.AL_OSTaxAmount);
		}

		public void TestAL_OverseasTotalInfo()
		{
			AssertEquals(true, TestBizO.AL_OverseasTotalInfo.ReadOnly);
		}

		public void TestAL_AT()
		{
			TestBizO.AL_OSExTaxAmount = 10m;
			TestBizO.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			TestObjectCreator.GST1.AT_A9_DefaultVatClass = TestObjectCreator.TaxMsg1.PK;
			TestBizO.AL_AT = TestObjectCreator.GST1.PK;

			AssertEquals(1m, TestBizO.AL_OSTaxAmount);
			AssertEquals(TestObjectCreator.TaxMsg1.PK, TestBizO.AL_A9_VATClass);
		}

		public void TestAL_GovtChargeCode_ReadOnly()
		{
			var line = (DirectTransactionLineBase)CreateNewLine();

			AssertEquals(false, line.AL_GovtChargeCode_ReadOnly_ForTestOnly);
		}

		public void TestTaxCalculationOnAL_TaxDateChange()
		{
			TestBizO.AL_OSExTaxAmount = 8M;
			var taxRate = CreateTaxRate();
			TestBizO.AL_AT = taxRate.PK;
			AssertEquals(ZDate.Today, TestBizO.AL_TaxDate);
			AssertAmounts(8M, 1.2M, 9.2M);

			TestBizO.AL_TaxDate = ZDate.Today.AddDays(1);
			AssertEquals(ZDate.Today.AddDays(1), TestBizO.AL_TaxDate);
			AssertAmounts(8M, 0.24M, 8.24M);
		}

		public void TestTaxCalculationOnAL_ATChange()
		{
			TestBizO.AL_OSExTaxAmount = 8M;
			TestBizO.AL_AT = CreateTaxRate().PK;
			AssertAmounts(8M, 1.2M, 9.2M);

			TestBizO.AL_AT = TestObjectCreator.GST1WithDates.PK;
			AssertAmounts(8M, 0.8M, 8.8M);
		}

		public void TestTaxCalculationOnAL_TaxRateNumeratorChange()
		{
			TestBizO.AL_OSExTaxAmount = 8M;
			TestBizO.AL_AT = CreateTaxRate().PK;
			AssertAmounts(8M, 1.2M, 9.2M);

			TestBizO.AL_TaxRateNumerator = 3;
			AssertAmounts(8M, 0.24M, 8.24M);

			TestBizO.AL_TaxDate = ZDate.Today.AddDays(1);
			AssertAmounts(8M, 0.24M, 8.24M);
		}

		public void TestTaxCalculationOnAL_TaxRateDenominatorChange()
		{
			TestBizO.AL_OSExTaxAmount = 8M;
			TestBizO.AL_AT = CreateTaxRate().PK;
			AssertAmounts(8M, 1.2M, 9.2M);

			TestBizO.AL_TaxRateDenominator = 3;
			AssertAmounts(8M, 0.4M, 8.4M);

			TestBizO.AL_TaxDate = ZDate.Today.AddDays(1);
			AssertAmounts(8M, 0.24M, 8.24M);
		}

		public void TestTaxCalculationOnAL_TaxExtraRateNumeratorChange()
		{
			TestBizO.AL_OSExTaxAmount = 8M;
			TestBizO.AL_AT = CreateTaxRate().PK;
			AssertAmounts(8M, 1.2M, 9.2M);

			TestBizO.AL_TaxExtraRateNumerator = 3;
			AssertAmounts(8M, 1.48M, 9.48M);

			TestBizO.AL_TaxDate = ZDate.Today.AddDays(1);
			AssertAmounts(8M, 0.24M, 8.24M);
		}

		public void TestTaxCalculationOnAL_TaxExtraRateDenominatorChange()
		{
			TestBizO.AL_OSExTaxAmount = 8M;
			TestBizO.AL_AT = CreateTaxRate().PK;
			AssertAmounts(8M, 1.2M, 9.2M);

			TestBizO.AL_TaxExtraRateNumerator = 5;
			TestBizO.AL_TaxExtraRateDenominator = 1;
			AssertAmounts(8M, 1.66M, 9.66M);

			TestBizO.AL_TaxExtraRateDenominator = 5;
			TestBizO.AL_TaxExtraRateDenominator = 5;
			AssertAmounts(8M, 1.29M, 9.29M);

			TestBizO.AL_TaxDate = ZDate.Today.AddDays(1);
			AssertAmounts(8M, 0.24M, 8.24M);
		}

		void AssertAmounts(decimal lineAmount, decimal taxAmount, decimal totalAmount)
		{
			AssertEquals(lineAmount, TestBizO.AL_OSExTaxAmount);
			AssertEquals(taxAmount, TestBizO.AL_OSTaxAmount);
			AssertEquals(totalAmount, TestBizO.AL_OverseasTotal);
		}

		AccTaxRate CreateTaxRate()
		{
			var rate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			rate.SetRate_ForTestOnly(15, 1, ZDate.Today.AddDays(-1), ZDate.Today);
			rate.SetRate_ForTestOnly(18, 6, ZDate.Today.AddDays(1), ZDate.Today.AddDays(2));
			return rate;
		}

		protected override bool IsExpectMultiSubAccountsSupported => true;

		#region Implementation

		public override void TestAL_AGIsReadOnly()
		{
			TestBizO.ReadOnlyIfAL_AGIsValidAndNotMisc = false;
			Assert("Line's GL Account field shouldn't be readonly", !TestBizO.AL_AGInfo.ReadOnly);

			TestBizO.ReadOnlyIfAL_AGIsValidAndNotMisc = true;
			TestBizO.AL_AG = ZGuid.NewZGuid();
			TestBizO.IsParentReceiptTypeMisc = false;
			Assert("Line's GL Account field should be readonly", TestBizO.AL_AGInfo.ReadOnly);

			TestBizO.IsParentReceiptTypeMisc = true;
			Assert("Line's GL Account field shouldn't be readonly", !TestBizO.AL_AGInfo.ReadOnly);

			TestBizO.IsParentReceiptTypeMisc = false;
			Assert("Line's GL Account field should be readonly", TestBizO.AL_AGInfo.ReadOnly);

			TestBizO.AL_AG = ZGuid.Invalid;
			Assert("Line's GL Account field shouldn't be readonly", !TestBizO.AL_AGInfo.ReadOnly);
		}

		protected override bool LineCanHaveTaxComponent
		{
			get { return false; }
		}

		protected override bool LineCanHaveForeignCurrency
		{
			get { return true; }
		}

		protected abstract Type GetExpectedParentBusinessObjectType();

		protected override Type MasterHeaderType
		{
			get { return GetExpectedParentBusinessObjectType(); }
		}

		protected override bool AcceptAL_AC
		{
			get { return false; }
		}

		protected override string ExpectedEmptyAL_AGErrorMessage
		{
			get { return "Please enter a GL Post To Account."; }
		}

		protected DirectTransactionLineBase TestBizO
		{
			get { return (DirectTransactionLineBase)base.Line; }
		}

		#endregion
	}
}
