using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	[TestedType(typeof(BankTransferCharge))]
	public class BankTransferChargeTest : DirectPaymentTest
	{
		protected override Type TypeOfValidation
		{
			get { return typeof(BankTransferChargeValidation); }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			BankTransferCharge businessObject = (BankTransferCharge)Factory.New(GetExpectedBusinessObjectType());
			businessObject.EnableFinanceCharge = true;
			return businessObject;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Header.Validation.ValidateAH_PostDate();
		}

		public override void TestAH_ReceiptTypeDefaultValue()
		{
			AssertEquals(ReceiptTypes.EFT, TestBizO.AH_ReceiptType);
		}

		public override void TestGenerateReverseTransaction()
		{
			BankTransferCharge testCharge = Factory.New<BankTransferCharge>();

			BankTransferChargeLine testLine;
			if (testCharge.Lines.Count == 0)
			{
				testLine = (BankTransferChargeLine)testCharge.Lines.AddNew();
			}
			else
			{
				testLine = (BankTransferChargeLine)testCharge.Lines[0];
			}

			testCharge.EnableFinanceCharge = true;
			testCharge.AH_ChequeDrawer = "a";
			testCharge.AH_DrawerBank = "b";
			testCharge.AH_DrawerBranch = "c";

			testCharge.GenerateReverseTransaction(true);

			AssertEquals(testCharge.FReverseTransaction_ForTestOnly.AH_ChequeDrawer, testCharge.AH_ChequeDrawer);
			AssertEquals(testCharge.FReverseTransaction_ForTestOnly.AH_DrawerBank, testCharge.AH_DrawerBank);
			AssertEquals(testCharge.FReverseTransaction_ForTestOnly.AH_DrawerBranch, testCharge.AH_DrawerBranch);
		}

		protected override void CancelTransactionHeaderToBeAbleToSave(TransactionHeader header)
		{
			var testCharge = header as BankTransferCharge;
			AssertNotNull("BankTransferCharge", testCharge);

			if (testCharge.Lines.Count == 0)
			{
				testCharge.Lines.AddNew();
			}

			testCharge.EnableFinanceCharge = true;
			testCharge.AH_ChequeDrawer = "a";
			testCharge.AH_DrawerBank = "b";
			testCharge.AH_DrawerBranch = "c";

			var reversingFactory = new Base.Reversing.ReversingFactory();
			var reversing = reversingFactory.NewReversing(testCharge);
			if (reversing != null && reversing.CanReverseTransaction)
			{
				reversing.Reverse();
				var reverseTransaction = reversing.ReverseTransaction as BankTransferCharge;
				AssertNotNull("ReverseTransaction", reverseTransaction);
				reverseTransaction.EnableFinanceCharge = true;
				if (reverseTransaction.AH_TransactionNum.IsEmpty)
				{
					reverseTransaction.AH_TransactionNum = "_1";
				}
			}
		}

		public override void TestAllLinesHaveSameBranch()
		{
			var transaction = (TransactionHeaderWithLines)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());

			AssertEquals(1, transaction.Lines.Count);

			var line1 = transaction.Lines[0];

			var line2 = transaction.Lines.AddNew();
			line2.AL_GB = line1.AL_GB;

			AssertEquals("All Lines Have Same Branch", true, transaction.LinesHaveSameBranch);

			var branch = TestObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);

			var line3 = transaction.Lines.AddNew();
			line3.AL_GB = branch.PK;

			AssertEquals("The 3 lines have 2 different branches", false, transaction.LinesHaveSameBranch);
		}

		public override void TestDirectTransactionAH_GB_BranchLevelPostingEnabled()
		{
			var header = (BankTransferCharge)Factory.New(GetExpectedBusinessObjectType());
			header.AH_GB = GlbBranch.CurrentBranch.PK;
			header.EnableFinanceCharge = true;

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			header.EnforceBranchLevelPostingRegistryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			var line = header.Lines[0];
			line.AL_GB = TestObjectCreator.NonCurrentBranch.PK;

			Factory.Save();

			AssertEquals("Header and Line branch is the same after saving, since branch level posting is enabled", header.AH_GB, line.AL_GB);
		}

		public override void TestDirectTransactionAH_GB_BranchLevelPostingEnabled_Reversed()
		{
			var header = (DirectTransactionHeaderBase)Factory.New(GetExpectedBusinessObjectType());
			header.AH_GB = GlbBranch.CurrentBranch.PK;

			if (header.EnforceBranchLevelPostingRegistryItem != null)
			{
				var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = false };
				header.EnforceBranchLevelPostingRegistryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

				var line = header.Lines[0];
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				line.AL_GB = TestObjectCreator.NonCurrentBranch.PK;

				Factory.Save();

				AssertNotEquals("Header and Line branch is not the same, since Branch level posting is not enabled", header.AH_GB, line.AL_GB);

				branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
				header.EnforceBranchLevelPostingRegistryItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

				header.GenerateReverseTransaction(true);

				var reversedTransaction = (DirectTransactionHeaderBase)header.ReverseTransaction;

				var reversedLine = reversedTransaction.Lines.Cast<DependentTransactionLine>().FirstOrDefault(x => x.AL_AG == TestObjectCreator.GLHeader1.PK);
				AssertNotEquals("Header and Line branch is not the same, though branch level posting is enabled. This is because reversed transaction should follow the original invoice", reversedTransaction.AH_GB, reversedLine.AL_GB);
			}
			else
			{
				Assert("This test is not applicable to this transaction", true);
			}
		}

		public void TestSetBankAndCurrencyDetailsWhenFinanceChargeEnabled()
		{
			BankTransfer testTransfer = new BankTransfer(Factory, null);

			testTransfer.BankTransferFromPK = TestObjectCreator.USDBankAccount.PK;
			testTransfer.BankTransferToPK = TestObjectCreator.AUDBankAccount.PK;
			testTransfer.SellExchangeRate = 0.7541m;

			Assert(!testTransfer.FinanceChargeZExchangeRate.IsCurrencyRequired);
			Assert(!testTransfer.FinanceChargeZExchangeRate.IsRateRequired);

			testTransfer.EnableFinanceCharge = true;
			AssertEquals(TestObjectCreator.USDBankAccount.PK, testTransfer.FinanceChargeBankPK);
			AssertEquals(TestObjectCreator.USDBankAccount.AB_RX_NKAccountCurrency, testTransfer.FinanceChargeZExchangeRate.Currency);
			AssertEquals(0.7541m, testTransfer.FinanceChargeZExchangeRate.Rate);
			Assert(testTransfer.FinanceChargeZExchangeRate.IsCurrencyRequired);
			Assert(testTransfer.FinanceChargeZExchangeRate.IsRateRequired);
		}

		public void TestRateType()
		{
			BankTransferCharge testCharge = Factory.New<BankTransferCharge>();
			AssertEquals(ExchangeRateType.Buy, testCharge.RateType);
		}

		public void TestDefaultValues()
		{
			BankTransferCharge testCharge = Factory.New<BankTransferCharge>();
			AssertEquals(1, testCharge.Lines.Count);
			AssertEquals(testCharge.PK, ((BankTransferChargeLine)testCharge.Lines[0]).BankTransferCharge.PK);
			AssertEquals((Guid)AccountingConfigurationRegistry.Instance.FinanceChargesAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty), testCharge.Lines[0].AL_AG.ToGuid());
			AssertEquals(ReceiptTypes.EFT, testCharge.AH_ReceiptType);
			AssertEquals((ZByte)3, testCharge.AH_TransactionCount);
			Assert(!testCharge.ExchangeRate.IsCurrencyRequired);
			Assert(!testCharge.ExchangeRate.IsRateRequired);
		}

		public void TestIsSavedByFactory()
		{
			BankTransferCharge testCharge = Factory.New<BankTransferCharge>();
			testCharge.EnableFinanceCharge = false;
			AssertEquals(false, testCharge.IsSavedByFactory);

			testCharge.EnableFinanceCharge = true;
			AssertEquals(true, testCharge.IsSavedByFactory);

			Factory.Save();

			testCharge.EnableFinanceCharge = true;
			testCharge.AH_IsCancelled = true;
			AssertEquals(true, testCharge.IsSavedByFactory);
		}

		public override void TestLoadedLinesOrderedBySequence()
		{
			Assert(true); //N/A BankTransferCharge should only have 1 charge line.
		}

		//Copy of TransactionHeaderWithLinesTest.TestIsChequeNumberAutoAllocated
		public override void TestIsChequeNumberAutoAllocated()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook autoPrintChequeBook = GetAutoPrintChequeBook(testBank, 1, 3, 2);
			BankTransferCharge testHeader = Factory.New<BankTransferCharge>();
			testHeader.AH_AB = testBank.PK;
			testHeader.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testHeader.ChequeBookPK = autoPrintChequeBook.PK;
			Assert("Auto allocation mode should not be enabled by default", !testHeader.IsChequeNumberAutoAllocated_ForTestOnly);
			AssertEquals("current cheque number should be set", "2", testHeader.AH_ChequeOrReference);
			Assert("ChequeOrReference field should not be read only", !testHeader.AH_ChequeOrReferenceInfo.ReadOnly);
		}

		protected override void ForceSavingByFactoryIfApplicable(AccTransactionHeader header)
		{
			((BankTransferCharge)header).EnableFinanceCharge = true;
			base.ForceSavingByFactoryIfApplicable(header);
		}
	}
}
