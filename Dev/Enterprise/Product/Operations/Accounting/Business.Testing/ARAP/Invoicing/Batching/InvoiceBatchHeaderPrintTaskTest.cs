using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class InvoiceBatchHeaderPrintTaskTest : TestCaseWithFactory
	{
		public void TestNoInvoices()
		{
			InvoiceBatchHeaderPrintTask task = new InvoiceBatchHeaderPrintTask(ZGuid.NewZGuid());
			AssertEquals(0, task.TaskCount);
		}

		public void TestPrintTaskDeliveryInstructionsPK()
		{
			var header = Factory.New<InvoiceBatchHeader>();
			var task = new InvoiceBatchHeaderPrintTask(header.PK);
			AssertEquals("Task delivery instructions PK", task.DocBuilderInvoiceMenuPK_ForTestOnly, task.Task_ForTestOnly.DeliveryInstructionsDefaultPK);
		}

		public void TestTransactions()
		{
			ARInvoice aRInvoice = Factory.New<ARInvoice>();
			aRInvoice.AH_GB = GlbBranch.CurrentBranch.PK;

			ARInvoice aRInvoice2 = Factory.New<ARInvoice>();
			aRInvoice2.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;

			Factory.Save();

			TransactionHeaderCollection result = InvoiceBatchHeaderPrintTask.Transactions_ForTestOnly(new ZQuery());

			AssertEquals(1, result.Count);
			AssertEquals(aRInvoice.PK, result[0].PK);
		}

		public void TestGetInvoicePrintCommand()
		{
			ZString nameOfMenu = "Test Batch";
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = nameOfMenu;
			menuItem.SU_BusinessContext = "ARBatchInvoice";

			Factory.Save();

			InvoiceBatchHeader header = Factory.New<InvoiceBatchHeader>();
			header.AH_OH = TestObjectCreator.AALSHI.PK;

			InvoiceBatchHeaderPrintTask task = new InvoiceBatchHeaderPrintTask(header.PK);

			DocumentCommand command = task.GetInvoicePrintCommand_ForTestOnly(header, nameOfMenu);
			AssertNotNull(command);
			AssertEquals(nameOfMenu, command.SU_MenuName);
		}

		TestObjectCreator fTestObjectCreator;
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
	}
}
