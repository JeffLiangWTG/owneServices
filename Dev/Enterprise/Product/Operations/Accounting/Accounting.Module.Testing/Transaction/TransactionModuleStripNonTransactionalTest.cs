using System.Windows.Forms;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Module.TransactionModuleStrip;

namespace Enterprise.Accounting.Module.Testing
{
	[UseSnapshotProtection]
	abstract class TransactionModuleStripNonTransactionalTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2009, 01, 15)]
		public void TestAutoAllocationAndPrintChequesFails()
		{
			AssertAutoAllocationAndPrintCheques(false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2009, 01, 15)]
		public void TestAutoAllocationAndPrintChequesSucceeds()
		{
			AssertAutoAllocationAndPrintCheques(true);
		}

		public void AssertAutoAllocationAndPrintCheques(bool isValidChequeBook)
		{
			var sydBranch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SYD");
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sydBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				TestObjectCreator.CreateTestPeriods(new ZDateTime(2009, 01, 01));
				TestObjectCreator.ABIGAS.OH_IsCreditor = true;
				TestObjectCreator.AALSHI.OH_IsDebtor = true;

				TestObjectCreator.AUDBankAccount.AB_Code = "AUD";
				TestObjectCreator.AUDChequeBook.AK_Code = "AUDC";
				TestObjectCreator.AUDChequeBook.AK_GB = GlbBranch.CurrentBranch.PK;
				TestObjectCreator.USDBankAccount.AB_Code = "USD";

				InvoicingBase invoice1 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "APINV123456", TestObjectCreator.AUD, 1M);
				invoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
				TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.AUD, 1M, 100M, 0M, 0M);
				InvoicingBase invoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "APINV987654", TestObjectCreator.AUD, 1M);
				invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
				TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.AUD, 1M, 300M, 0M, 0M);
				InvoicingBase invoice3 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "APINV2", TestObjectCreator.AUD, 1M);
				invoice3.AH_OH = TestObjectCreator.ABIGAS.PK;
				TestObjectCreator.CreateInvoiceLine(invoice3, TestObjectCreator.AUD, 1M, 100M, 0M, 0M);

				InvoicingBase invoice4 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "00001000", TestObjectCreator.USD, 0.67M);
				invoice4.AH_OH = TestObjectCreator.AALSHI.PK;
				TestObjectCreator.CreateInvoiceLine(invoice4, TestObjectCreator.USD, 0.67M, 1500M, 0M, 0M);
				InvoicingBase invoice5 = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "00001000", TestObjectCreator.USD, 0.67M);
				invoice5.AH_OH = TestObjectCreator.ABIGAS.PK;
				TestObjectCreator.CreateInvoiceLine(invoice5, TestObjectCreator.USD, 0.67M, 800M, 0M, 0M);
				InvoicingBase invoice6 = TestObjectCreator.CreateInvoice(typeof(APCreditNote), "apcredit", TestObjectCreator.USD, 0.67M);
				invoice6.AH_OH = TestObjectCreator.AALSHI.PK;
				TestObjectCreator.CreateInvoiceLine(invoice6, TestObjectCreator.USD, 0.67M, 300M, 0M, 0M);

				TestObjectCreator.SetupAutoPrintChequeBook(TestObjectCreator.AUDBankAccount, TestObjectCreator.AUDChequeBook, Factory);
				Factory.Save();

				var paymentQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, new[] { TransactionTypes.Payment });
				paymentQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, invoice1.AH_GC);
				var receiptQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, new[] { TransactionTypes.Receipt });
				receiptQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, invoice1.AH_GC);

				string pathToTestFile = BaseSourcePath +
					@"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\PaymentReceiptRemittanceFile\Testing\PayRecRemittance.csv";

				bool isImportRun;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(dialog =>
				{
					if (dialog is IDataImporterFormForTestOnly importForm)
					{
						importForm.ImportFromFileExposed(pathToTestFile);
						isImportRun = true;
					}
				});

				MenuItem[] actionMenu = TestModule.FormActionMenu;
				MenuItem importRemittanceFileMenuItem = actionMenu.FindByText("Actions").MenuItems.FindByText("Data Transfer").MenuItems.FindByText("Import Remittance File");
				AssertNotNull("Menu item should exist", importRemittanceFileMenuItem);

				TestObjectCreator.AUDChequeBook.AK_Desc = isValidChequeBook
					? "ValidCheque"
					: "TestAutoAllocationAndPrintCheques";
				Factory.Save();

				isImportRun = false;
				importRemittanceFileMenuItem.PerformClick();
				Assert(nameof(isImportRun), isImportRun);
				var payments = new BusinessObjectFactory().Load<TransactionHeader>(paymentQuery);
				var receipts = new BusinessObjectFactory().Load<TransactionHeader>(receiptQuery);

				if (isValidChequeBook)
				{
					AssertEquals("Payment is saved when auto cheque is printed.", 1, payments.Length);
					AssertEquals("Receipt should be saved.", 1, receipts.Length);
				}
				else
				{
					AssertEquals("Payment must not be saved when auto cheque printing has failed.", 0, payments.Length);
					AssertEquals("Receipt should be saved even when payment saving fails.", 1, receipts.Length);
				}
			}
		}

		protected abstract ModuleIdentifier GetModuleID();

		protected override void TearDown()
		{
			if (testModule != null)
			{
				testModule.Dispose();
			}
			base.TearDown();
		}

		protected TransactionModuleStrip TestModule => testModule ?? (testModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()));
		TransactionModuleStrip testModule;

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		protected BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;
	}
}
