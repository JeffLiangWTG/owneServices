using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch.Testing
{
	class DepositBatchTransactionLineValidation_InnerTest : TestCaseWithFactory
	{
		public void TestCheckIsSelectedWhenCreateDepositBatch()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			var testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(1, testDepositBatchParent.DepositBatchLines.Count);

			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];
			DepositBatchTransactionLine testLine = testBatch.Transactions[0];

			var testValidation = new DepositBatchTransactionLineValidation(testLine);
			testValidation.CheckIsSelected_ForTestOnly();
			AssertEquals(false, testLine.IsSelectedInfo.HasErrors());

			var newFactory = new BusinessObjectFactory();
			var testReceiptInNewFactory = newFactory.Load<AccTransactionHeader>(testReceipt.PK);

			var testDepositBatchParentInNewFactory = new DepositBatchParent(newFactory);
			AssertEquals(1, testDepositBatchParentInNewFactory.DepositBatchLines.Count);

			DepositBatch testBatchInNewFactory = testDepositBatchParentInNewFactory.DepositBatchLines[0];
			DepositBatchTransactionLine testLineInNewFactory = testBatchInNewFactory.Transactions[0];

			Factory.Save();

			var testValidationInNewFactory = new DepositBatchTransactionLineValidation(testLineInNewFactory);
			testValidationInNewFactory.CheckIsSelected_ForTestOnly();
			AssertHasErrorContaining(testLineInNewFactory.IsSelectedInfo, DepositBatchTransactionLineValidation.TransactionAlreadyBatchedMessage_ForTestOnly);
		}

		public void TestCheckIsSelectedWhenViewDepositBatch()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			var testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(1, testDepositBatchParent.DepositBatchLines.Count);

			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];
			DepositBatchTransactionLine testLine = testBatch.Transactions[0];

			var testValidation = new DepositBatchTransactionLineValidation(testLine);
			testValidation.CheckIsSelected_ForTestOnly();
			AssertEquals(false, testLine.IsSelectedInfo.HasErrors());

			Factory.Save();

			testValidation.CheckIsSelected_ForTestOnly();
			AssertEquals(false, testLine.IsSelectedInfo.HasErrors());

			testValidation.ValidateAll();
			AssertEquals(false, testLine.IsSelectedInfo.HasErrors());
		}

		public void TestCheckIsSelectedWhenCancelDepositBatch()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var testReceiptInNewFactory = newFactory.Load<ARReceipt>(testReceipt.PK);
			var testDepositBatchParent = new DepositBatchParent(newFactory);
			AssertEquals(1, testDepositBatchParent.DepositBatchLines.Count);

			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];
			DepositBatchTransactionLine testLine = testBatch.Transactions[0];

			testReceipt.AH_IsCancelled = true;
			((IMatching)testReceipt).CurrentMatchGroup.AddNew().AP_AH = testReceipt.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(testReceipt);
			Factory.Save();

			var testValidation = new DepositBatchTransactionLineValidation(testLine);
			testValidation.CheckIsSelected_ForTestOnly();
			AssertHasError(testLine.IsSelectedInfo, DepositBatchTransactionLineValidation.TransactionCancelled_ForTestOnly);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestDueDateAndInvoiceDateAndPostDateShouldNotBeRangeChecked()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			ZDateTime testDate = ZDateTime.Today.AddYears(-11);
			Factory.Save();

			testReceipt.AH_DueDate = testDate;
			testReceipt.AH_InvoiceDate = testDate;
			testReceipt.AH_PostDate = testDate;
			testReceipt.Validation.ValidateAll();
			string errorMessage = string.Format("The date '{0}' is more than 10 years old and thus is not valid.", testDate.ToString("dd-MMM-yyyy"));
			Assert("There should be error", testReceipt.AH_DueDateInfo.HasError(errorMessage));
			Assert("There should be error", testReceipt.AH_InvoiceDateInfo.HasError(errorMessage));
			Assert("There should be error", testReceipt.AH_PostDateInfo.HasError(errorMessage));

			DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
			AssertEquals(1, testDepositBatchParent.DepositBatchLines.Count);
			DepositBatch testBatch = testDepositBatchParent.DepositBatchLines[0];

			DepositBatchTransactionLine testLine = testBatch.Transactions[0];

			testLine.Validation.ValidateAll();
			Assert("Validation should be DepositBatchTransactionLine Validation", testLine.Validation.GetType().IsAssignableFrom(typeof(DepositBatchTransactionLineValidation)));

			Assert("There should be no error", !testLine.AH_DueDateInfo.HasErrors());
			Assert("There should be no error", !testLine.AH_InvoiceDateInfo.HasErrors());
			Assert("There should be no error", !testLine.AH_PostDateInfo.HasErrors());
		}

		#region Implementations

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
