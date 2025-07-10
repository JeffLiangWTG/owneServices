using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework.TestHelper;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	abstract class DirectTransactionHeaderBaseValidationTest : TransactionHeaderWithLinesValidationTest
	{
		public virtual void TestCheckAH_PostDateNotInFuture()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			var directTransactionHeader = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());
			directTransactionHeader.AH_PostDate = ZDateTime.Now.AddDays(1);
			AssertHasError("AH_PostDate", directTransactionHeader.AH_PostDateInfo, AccountingConstants.FuturePostingErrorMessages.RegistryIsNotEnabled);

			directTransactionHeader.AH_PostDate = ZDateTime.Now;
			AssertNoErrors("AH_PostDate", directTransactionHeader.AH_PostDateInfo);

			AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.ResetSecurityCore();
			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = false;

			directTransactionHeader.AH_PostDate = ZDateTime.Now.AddDays(2);
			AssertHasError("AH_PostDate", directTransactionHeader.AH_PostDateInfo, AccountingConstants.FuturePostingErrorMessages.UserHasNoSecurity);

			Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowed = true;
			directTransactionHeader.AH_PostDate = ZDateTime.Now.AddDays(3);
			AssertNoErrors("AH_PostDate", directTransactionHeader.AH_PostDateInfo);
		}

		public virtual void TestCheckAH_Desc()
		{
			TestBizO.AH_Desc = string.Empty;
			AssertEquals(true, TestBizO.AH_DescInfo.HasErrors());

			TestBizO.AH_Desc = "Test";
			AssertEquals(false, TestBizO.AH_DescInfo.HasErrors());
		}

		public override void TestCheckAH_ReceiptType()
		{
			base.TestCheckAH_ReceiptType();
			TestBizO.AH_ReceiptType = string.Empty;
			AssertEquals(true, TestBizO.AH_ReceiptTypeInfo.HasErrors());
		}

		public void TestCheckAH_ReceiptTypeIsCashAccount()
		{
			var testBank = TestObjectCreator.CreateBankAccount("TST-BANK", "TEST BANK", TestObjectCreator.AUD, null, AccountTypeCodeDescriptionPairList.Codes.BNK);
			testBank.AB_IsActive = true;
			var testCash = TestObjectCreator.CreateBankAccount("TST_CASH", "TEST CASH", TestObjectCreator.AUD, null, AccountTypeCodeDescriptionPairList.Codes.CSH);
			testCash.AB_IsActive = true;

			var testBizO = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());

			AssertNoErrors("No validation error in the beginning", testBizO.AH_ReceiptTypeInfo);

			testBizO.AH_AB = testBank.PK;
			AssertNoErrors("No validation error when Bank Account is selected",
				testBizO.AH_ReceiptTypeInfo);

			testBizO.AH_AB = testCash.PK;
			AssertNoErrors("Since Cash Account selection sets Receipt Type to Cash, there is no validation error",
				testBizO.AH_ReceiptTypeInfo);

			testBizO.AH_ReceiptType = ReceiptTypes.Cheque;
			AssertHasError("Should have error if Receipt Type is not Cash",
				testBizO.AH_ReceiptTypeInfo, $"For Cash Account, please select CSH - Cash {testBizO.AH_ReceiptTypeInfo.HumanReadableName}.");

			testBizO.AH_ReceiptType = ReceiptTypes.Cash;
			AssertNoErrors("No validation error when Cash Receipt Type is selected",
				testBizO.AH_ReceiptTypeInfo);
		}

		public virtual void TestCheckAH_OSTotalAmount()
		{
			TestBizO.AH_OSTotalAmount = -1m;
			((DirectTransactionHeaderBaseValidation)TestBizO.Validation).ValidateAH_OSTotalAmount();
			AssertHasError(TestBizO.AH_OSTotalAmountInfo, "Total cannot be negative.");

			TestBizO.AH_OSTotalAmount = 0m;
			((DirectTransactionHeaderBaseValidation)TestBizO.Validation).ValidateAH_OSTotalAmount();
			AssertHasError(TestBizO.AH_OSTotalAmountInfo, "Total cannot be zero.");

			TestBizO.AH_OSTotalAmount = 10m;
			((DirectTransactionHeaderBaseValidation)TestBizO.Validation).ValidateAH_OSTotalAmount();
			AssertNoErrors(TestBizO.AH_OSTotalAmountInfo);

			((DirectTransactionLineBase)TestBizO.Lines.AddNew()).AL_OSExTaxAmount = -1;
			AssertHasError(TestBizO.AH_OSTotalAmountInfo, "Total cannot be negative.");

			TestBizO.Lines.RemoveAndDeleteAll();
			AssertHasError(TestBizO.AH_OSTotalAmountInfo, "Total cannot be zero.");
		}

		public override void TestCheckAH_AB()
		{
			base.TestCheckAH_AB();
			TestBizO.AH_AB = ZGuid.Empty;
			AssertEquals(true, TestBizO.AH_ABInfo.HasErrors());

			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			TestBizO.AH_AB = testBank.PK;
			AssertEquals(false, TestBizO.AH_ABInfo.HasErrors());

			testBank.AB_IsActive = false;
			TestBizO.AH_AB = testBank.PK;
			AssertHasError(TestBizO.AH_ABInfo, "Enter a valid Bank Account.");
		}

		protected virtual bool IsChequeDrawerAlwaysEditable => true;

		public virtual void TestCheckAH_ChequeDrawer()
		{
			AssertEquals("Percondition", false, TestBizO.AH_ChequeDrawerInfo.ReadOnly);

			TestBizO.AH_ChequeDrawer = string.Empty;
			AssertEquals(true, TestBizO.AH_ChequeDrawerInfo.HasError("Please enter a Check Drawer."));

			TestBizO.AH_ChequeDrawer = "1234";
			AssertEquals(false, TestBizO.AH_ChequeDrawerInfo.HasErrors());

			TestBizO.AH_ReceiptType = ReceiptTypes.Cash;
			AssertEquals("Percondition", !IsChequeDrawerAlwaysEditable, TestBizO.AH_ChequeDrawerInfo.ReadOnly);

			TestBizO.AH_ChequeDrawer = string.Empty;
			AssertEquals(IsChequeDrawerAlwaysEditable, TestBizO.AH_ChequeDrawerInfo.HasError("Please enter a Check Drawer."));
		}

		public virtual void TestChequeNumberNumeric()
		{
			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestBizO.AH_ChequeOrReference = "12345678901234567890";
			AssertEquals("Should have NO error", false, TestBizO.AH_ChequeOrReferenceInfo.HasErrors());

			TestBizO.AH_ChequeOrReference = "abc";
			AssertEquals("Should have error", true, TestBizO.AH_ChequeOrReferenceInfo.HasError("Only numbers are allowed in this field."));

			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			TestBizO.AH_ChequeOrReference = "abc";
			AssertEquals("Should have NO error", false, TestBizO.AH_ChequeOrReferenceInfo.HasErrors());
		}

		public virtual void TestValidateAll_ValidateAH_OSTotalAmount()
		{
			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			TestBizO.RunPreSaveValidation();
			AssertEquals(true, TestBizO.AH_OSTotalAmountInfo.HasErrors());
			AssertHasErrorContaining(TestBizO.AH_OSTotalAmountInfo, "Total cannot be zero.");

			DirectTransactionLineBase testLine = (DirectTransactionLineBase)TestBizO.Lines.AddNew();
			testLine.AL_OSExTaxAmount = 10m;

			TestBizO.RunPreSaveValidation();
			AssertEquals(false, TestBizO.AH_OSTotalAmountInfo.HasErrors());

			DirectTransactionLineBase testLine2 = (DirectTransactionLineBase)TestBizO.Lines.AddNew();
			testLine2.AL_OSExTaxAmount = -100m;

			TestBizO.RunPreSaveValidation();
			AssertHasErrorContaining(TestBizO.AH_OSTotalAmountInfo, "Total cannot be negative.");
		}

		public virtual void TestCheckAH_ChequeOrReference()
		{
			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			TestBizO.AH_ChequeOrReference = "";
			AssertHasError(TestBizO.AH_ChequeOrReferenceInfo, "Please enter a Check No..");

			TestBizO.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			AssertHasError(TestBizO.AH_ChequeOrReferenceInfo, "Please enter a Reference No..");

			TestBizO.AH_ChequeOrReference = "12345";
			AssertNoErrors(TestBizO.AH_ChequeOrReferenceInfo);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestBizO = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());
		}

		protected Type GetExpectedBusinessObjectType() =>
			TestedTypeHelper.GetTestedType(GetType());

		protected override Type HeaderType => GetExpectedBusinessObjectType();

		protected DirectTransactionHeaderBase TestBizO;

		#endregion
	}
}
