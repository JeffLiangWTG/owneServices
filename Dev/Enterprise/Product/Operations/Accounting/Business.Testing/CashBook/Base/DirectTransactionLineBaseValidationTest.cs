using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	abstract class DirectTransactionLineBaseValidationTest : DependentTransactionLineValidationTest
	{
		protected override (bool isNeedTestNoTaxMessage, bool isUseAPRegistry) GetIsNeedTestNoTaxMessageAndIsAP(TransactionHeaderWithLines header) => (true, header.AH_TransactionType != TransactionTypes.DirectReceipt);

		public virtual void TestCheckAL_Desc()
		{
			TestBizO.AL_Desc = string.Empty;
			AssertEquals("Should have error", true, TestBizO.AL_DescInfo.HasErrors());

			TestBizO.AL_Desc = "Test";
			AssertEquals("Should have NO error", false, TestBizO.AL_DescInfo.HasErrors());
		}

		public override void TestCheckAL_AG()
		{
			base.TestCheckAL_AG();

			ZString expectedError = new TransactionLineValidation(TestBizO).InvalidGLAccountError;

			AccGLHeader validGLHeader = TestObjectCreator.CreateGLHeader();
			validGLHeader.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;

			AccGLHeader invalidGLHeader = TestObjectCreator.CreateGLHeader();
			invalidGLHeader.AG_AccountType = AccountTypeComboBoxConstants.Header;

			TestBizO.AL_AG = validGLHeader.PK;
			AssertEquals(false, TestBizO.AL_AGInfo.HasErrors());

			TestBizO.AL_AG = ZGuid.Empty;
			AssertEquals(true, TestBizO.AL_AGInfo.HasErrors());

			TestBizO.AL_AG = invalidGLHeader.PK;
			AssertEquals("HDR GL account, should have error", true, TestBizO.AL_AGInfo.HasError(expectedError));

			invalidGLHeader.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			invalidGLHeader.AG_ControlAccount = true;
			TestBizO.AL_AG = invalidGLHeader.PK;
			if (TestBizO.TransactionHeader.AH_TransactionType == ZArchitecture.Core.TransactionTypes.DirectPayment || TestBizO.TransactionHeader.AH_TransactionType == ZArchitecture.Core.TransactionTypes.DirectReceipt)
			{
				AssertHasError("HDR GL account, should have error", TestBizO.AL_AGInfo, "You cannot post to control accounts from this screen");
			}
			else
			{
				AssertNoErrors(TestBizO.AL_AGInfo);
			}

			invalidGLHeader.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			TestBizO.AL_AG = invalidGLHeader.PK;
			AssertEquals("P&L GL account, should NOT have error", false, TestBizO.AL_AGInfo.HasErrors());

			invalidGLHeader.AG_AccountType = AccountTypeComboBoxConstants.BalanceSheetAccount;
			TestBizO.AL_AG = invalidGLHeader.PK;
			AssertEquals("BSH GL account, should NOT have error", false, TestBizO.AL_AGInfo.HasErrors());
		}

		public virtual void TestCheckAL_AT()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			TestBizO.AL_AT = ZGuid.Empty;
			AssertEquals("Should have error", true, TestBizO.AL_ATInfo.HasErrors());

			TestBizO.AL_AT = TestObjectCreator.CreateTaxRate("AAA", "TEST", 5).PK;
			AssertEquals("Should have NO error", false, TestBizO.AL_ATInfo.HasErrors());

			TestBizO.AL_AT = TestObjectCreator.CreateTaxRate("RAAA", "Reverse VAT Rate", AccTaxRate.Types.ReverseRated, 5, string.Empty, 0, 1).PK;
			AssertHasError(TestBizO.AL_ATInfo, "Reverse tax rates cannot be used in cashbook transactions");

			TestBizO.AL_AT = TestObjectCreator.SVAT1.PK;
			AssertHasError(TestBizO.AL_ATInfo, "Suspended tax rates cannot be used in cashbook transactions");

			TestBizO.AL_AT = TestObjectCreator.CreateTaxRate("RETTST", "VAT Retention", AccTaxRate.Types.Rated, 16, AccTaxRate.ExtraTypes.VATRetention, 4, 1).PK;
			AssertHasError(TestBizO.AL_ATInfo, "VAT Withholding tax rates cannot be used in cashbook transactions");
		}

		public virtual void TestCheckAL_OSExTaxAmount()
		{
			TestBizO.AL_OSTaxAmount = 0m;
			AssertEquals("Should have NO error", false, TestBizO.AL_OSTaxAmountInfo.HasErrors());

			TestBizO.AL_OSExTaxAmount = 10m;
			AssertEquals("Should have NO error", false, TestBizO.AL_OSExTaxAmountInfo.HasErrors());

			TestBizO.AL_OSExTaxAmount = 0m;
			AssertHasError("Should have error", TestBizO.AL_OSExTaxAmountInfo, "Amount cannot be zero.");
		}

		public override void TestCheckAL_LocalExTaxAmount_WhenExRateIsOneAndLocalAndForeignAmountDoNotMatch()
		{
			Assert("Does not apply", true);
		}

		public override void TestCheckAL_LocalTaxAmount()
		{
			Assert("Does not apply", true);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestBizO = (DirectTransactionLineBase)Factory.New(GetExpectedBusinessObjectType());
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;

		#endregion
	}
}
