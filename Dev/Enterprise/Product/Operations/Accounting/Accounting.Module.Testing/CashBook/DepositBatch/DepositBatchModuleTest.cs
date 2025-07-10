using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(DepositBatchModule))]
	public class DepositBatchModuleTest : FilterGridModuleWithMultipleReversingTest
	{
		public void TestGetNewGridCollection()
		{
			using (DepositBatchModule testModule = new DepositBatchModule())
			{
				ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
				ARReceipt testReceipt2 = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 300M, TestObjectCreator.AUDBankAccount2.PK);
				Factory.Save();

				DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
				AssertEquals(2, testDepositBatchParent.DepositBatchLines.Count);

				ARInvoice aRInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;

				Factory.Save();

				BusinessObjectCollection dDRCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
				dDRCollection.Load();
				AssertEquals(2, dDRCollection.Count);
			}
		}

		public void TestMenuItems()
		{
			using (DepositBatchModule testModule = new DepositBatchModule())
			{
				MenuItem[] testMenuItems = testModule.GetNewActionMenuItems_ForTestOnly();

				AssertEquals(3, testMenuItems.Length);
				AssertEquals("D&ata Transfer", testMenuItems[0].Text);
				AssertEquals("&Print", testMenuItems[2].Text);
			}
		}

		public void TestPrintSecurity()
		{
			bool oldValue = Env.Security.PrintDepositBatch.IsAllowed;

			try
			{
				Env.Security.PrintDepositBatch.IsAllowed = false;

				using (DepositBatchModule testModule = new DepositBatchModule())
				{
					ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
					Factory.Save();

					DepositBatchParent testDepositBatchParent = new DepositBatchParent(Factory);
					AssertEquals(1, testDepositBatchParent.DepositBatchLines.Count);
					Factory.Save();

					BusinessObjectCollection dDRCollection = (BusinessObjectCollection)testModule.GetNewGridCollection_ForTestOnly();
					dDRCollection.Load();
					AssertEquals(1, dDRCollection.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testModule.HandlePrint_ForTestOnly(null, new EventArgs());
					AssertEquals(Env.Security.PrintDepositBatch.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				Env.Security.PrintDepositBatch.IsAllowed = oldValue;
			}
		}

		public void TestDeleteMenuItemText()
		{
			using (DepositBatchModule module = new DepositBatchModule())
			{
				AssertEquals("Cancel", module.GetDeleteMenuItemText_ForTestOnly().Caption);
				AssertEquals("Cancels the selected item after viewing its details read-only (shortcut Del)", module.GetDeleteMenuItemText_ForTestOnly().FullDescription);
			}
		}

		#region Implementations

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DepositBatch;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			collection.Add(Factory.NewWithValidTestData<DepositBatch>());
		}

		protected override BusinessObject[] GetBusinessObjectsToGetControllersFor()
		{
			ARReceipt receipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatch depositBatch = Factory.New<DepositBatch>();
			depositBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			depositBatch.LoadTransactions(ZGuid.Empty);

			return new BusinessObject[] { depositBatch };
		}

		#endregion
	}
}
