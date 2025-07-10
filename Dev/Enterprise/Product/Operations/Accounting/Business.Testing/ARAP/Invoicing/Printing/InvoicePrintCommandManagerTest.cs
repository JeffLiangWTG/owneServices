using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing.Testing
{
	class InvoicePrintCommandManagerTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestGetInvoicePrintCommandSetParentIfMenuPKExists()
		{
			StmMenuItem newMenu = Factory.NewWithValidTestData<StmMenuItem>();
			newMenu.SU_MenuName = "a new menu";
			newMenu.SU_BusinessContext = "ARInvoice";
			newMenu.SU_MenuPath = "";
			newMenu.SU_IsSystemDefined = true;
			newMenu.SU_IsPublished = false;
			newMenu.SU_GS_NKStaffCode = "";
			Factory.Save();

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			DocumentCommand command = InvoicePrintCommandManager.New(invoice, newMenu.PK).Command;
			AssertEquals("document command should set its parent to the invoice", invoice, command.Parent);
		}

		public void TestGetInvoicePrintCommandUsesMenuPKIfItExists()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();

			ZQuery query = new ZQuery(StmMenuItemSchema.SU_MenuName, "DocBuilder Invoice");
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, "ARInvoice");
			StmMenuItem docBuilderMenu = Factory.LoadTop1<StmMenuItem>(query);

			StmMenuItem anotherMenu = Factory.NewWithValidTestData<StmMenuItem>();
			anotherMenu.SU_MenuName = "a new menu";
			anotherMenu.SU_BusinessContext = "ARInvoice";
			anotherMenu.SU_MenuPath = "";
			anotherMenu.SU_IsSystemDefined = true;
			anotherMenu.SU_IsPublished = false;
			anotherMenu.SU_GS_NKStaffCode = "";
			Factory.Save();

			DocumentCommand command = InvoicePrintCommandManager.New(aRInv, anotherMenu.PK).Command;
			AssertEquals("should use menu PK if it's not empty", anotherMenu.PK, command.PK);

			command = InvoicePrintCommandManager.New(aRInv, "DocBuilder Invoice").Command;
			AssertEquals("should use menu name to find the command only if menu pk is empty", docBuilderMenu.PK, command.PK);
		}

		public void TestGetInvoicePrintCommandForPeriodicInvoiceWithJobHeaderSet()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			foreach (var invoiceType in InvoiceTypeCalculationProvider.DeferredInvoiceTypes)
			{
				ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
				aRInv.AH_TransactionCategory = invoiceType;

				ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				jobHeader.JH_ParentID = shipment.PK;
				jobHeader.JH_ParentTableCode = "JS";
				aRInv.AH_JH = jobHeader.PK;
				Factory.Save();
				InvoicePrintTask printTask = GetPrintTask(aRInv.PK);
				DocumentCommand command = InvoicePrintCommandManager.New(aRInv, "DocBuilder Invoice").Command;
				AssertNotNull("There should be a command for DocBuilder Invoice", command);
				AssertEquals("Should choose the ARInvoice's DocBuilder Invoice menu", "ARInvoice", command.SU_BusinessContext);
			}
		}

		public void TestGetInvoicePrintCommandCoreWithOutUsingDocStrips()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			InvoicePrintTask printTask = GetPrintTask(aRInv.PK);
			DocumentCommand command1 = InvoicePrintCommandManager.New(aRInv, "Class A Invoice Preprinted").Command;
			AssertNotNull("There should be a command for Class A Invoice Preprinted", command1);
			try
			{
				DocumentCommand command = InvoicePrintCommandManager.New(aRInv, "blah").Command;
				Fail("UnableToFindInvoiceDocumentCommandException should have been thrown when Menu Item not found at all.");
			}
			catch (UnableToFindInvoiceDocumentCommandException e)
			{
				Assert("Exception should say that command can't be found", e.Message.StartsWith("Unable to find Invoice document command for transaction"));
			}
		}

		public void TestGetInvoicePrintCommandWhenMultipleMenusAreFound()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			InvoicePrintTask printTask = GetPrintTask(aRInv.PK);
			DocumentCommand command = InvoicePrintCommandManager.New(aRInv, "DocBuilder Invoice").Command;
			AssertNotNull("There should be a command for DocBuilder Invoice", command);

			StmMenuItem anotherMenu = Factory.NewWithValidTestData<StmMenuItem>();
			anotherMenu.SU_MenuName = "DocBuilder Invoice";
			anotherMenu.SU_BusinessContext = "ARInvoice";
			anotherMenu.SU_MenuPath = "";
			anotherMenu.SU_IsSystemDefined = true;
			anotherMenu.SU_IsPublished = false;
			anotherMenu.SU_GS_NKStaffCode = "";
			Factory.Save();

			try
			{
				command = InvoicePrintCommandManager.New(aRInv, "DocBuilder Invoice").Command;
				Fail("UnableToFindInvoiceDocumentCommandException should have been thrown when Menu Item not found at all.");
			}
			catch (UnableToFindInvoiceDocumentCommandException e)
			{
				Assert("Exception should say that command can't be found", e.Message.StartsWith("Unable to find Invoice document command for transaction"));
			}
		}

		public void TestGetCorrectCommandBasedOnIsLegacyDocumentOrNot()
		{
			StmMenuItem menuItem1 = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem1.SU_MenuName = "My Test Menu";
			menuItem1.SU_BusinessContext = "ARInvoice";
			menuItem1.SU_IsSystemDefined = true;
			menuItem1.SU_MenuPath = "";
			menuItem1.SU_GS_NKStaffCode = "";

			StmMenuItem menuItem2 = Factory.NewWithValidTestData<StmMenuItem>();
			menuItem2.SU_MenuName = "My Test Menu";
			menuItem2.SU_BusinessContext = "ARInvoice";
			menuItem2.SU_IsSystemDefined = true;
			menuItem2.SU_GS_NKStaffCode = "";
			menuItem2.SU_MenuPath = "Legacy Documents";

			Factory.Save();

			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			InvoicePrintTask printTask = GetPrintTask(aRInv.PK);
			DocumentCommand command = InvoicePrintCommandManager.New(aRInv, "My Test Menu").Command;
			AssertNotNull("There should be a command for My Test Menu", command);
			AssertEquals("SU_MenuPath should be blank because it's not a legacy document", string.Empty, command.SU_MenuPath);
			command = InvoicePrintCommandManager.New(aRInv, "My Test Menu", true).Command;
			AssertEquals("SU_MenuPath should be 'Legacy Documents' because it is a legacy document", "Legacy Documents", command.SU_MenuPath);
		}

		public void TestGetInvoicePrintCommandCoreUsingDocStrips()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			InvoicePrintTask printTask = GetPrintTask(aRInv.PK);

			DocumentCommand command = InvoicePrintCommandManager.New(aRInv, "Invoice").Command;
			AssertNotNull("There should be a command for Invoice", command);

			JobHeader jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = "12345";
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			aRInv.AH_JH = jobHeader.PK;
			Factory.Save();

			DocumentCommand commandWithARInvoiceBusinessContext = GetDocumentCommand("DocBuilder Invoice", "ARInvoice");
			Factory.Save();
			command = InvoicePrintCommandManager.New(aRInv, "DocBuilder Invoice").Command;

			AssertEquals("There should be a command with ARInvoice business context", "ARInvoice", command.SU_BusinessContext);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "12345";
			jobHeader.JH_ParentID = shipment.PK;
			DocumentCommand commandWithShipmentBusinessContext = GetDocumentCommand("DocBuilder Invoice", "Shipment");
			Factory.Save();
			command = InvoicePrintCommandManager.New(aRInv, "DocBuilder Invoice").Command;

			AssertEquals("There should be a command with Shipment business context.", "Shipment", command.SU_BusinessContext);
		}

		public void TestGetSystemCommandFallingBackToNonSystem()
		{
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			InvoicePrintTask printTask = GetPrintTask(aRInv.PK);
			DocumentCommand command = InvoicePrintCommandManager.New(aRInv, "Invoice").Command;
			AssertNotNull(command);

			DocumentCommand commandWithARInvoiceBusinessContext = GetDocumentCommand("DocBuilder Invoice", "ARInvoice");
			commandWithARInvoiceBusinessContext.SU_IsPublished = false;
			Factory.Save();
			command = InvoicePrintCommandManager.New(aRInv, "Invoice").Command;
			AssertNotNull(command);
		}

		DocumentCommand GetDocumentCommand(string menuItemName, string businessContext)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(StmMenuItemSchema.SU_MenuName, menuItemName);
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, businessContext);
			DocumentCommand command = Factory.LoadTop1<DocumentCommand>(query);
			command = command ?? (command = Factory.New<DocumentCommand>());
			command.SU_MenuName = menuItemName;
			command.SU_BusinessContext = businessContext;
			return command;
		}

		protected virtual InvoicePrintTask GetPrintTask(ZGuid invoicePK)
		{
			return new InvoicePrintTask(new InvoicePrintTask.Configuration(invoicePK));
		}
	}
}
