using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceModule))]
	sealed class CommercialInvoiceModuleTest : Customs.Module.Testing.CommercialInvoiceModuleTest
	{
		public void TestLVSFormControllerID()
		{
			var invoice1 = Factory.New<JobComInvoiceHeader>();
			invoice1.JZ_InvoiceNumber = "INV0001";
			Factory.Save();
			using (var module = new CommercialInvoiceModule())
			using (var form = new ZForm())
			{
				var menuItem = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Create New CLVS Shipment");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var filterControl = (CommercialInvoiceFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();
				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.UnSelectAll();
				grid.SelectSingleElementByPK(invoice1.PK);
				AssertEquals(1, module.GetSelectedBusinessObjects().Length);
				menuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				using (var lastDialogForm = module.lvxController.LastShownForm as JobDeclarationForm)
				{
					AssertEquals(ControllerIDs.Customs.CA.CALVXJobs, lastDialogForm.ControllerID);
				}
			}
		}

		public void TestHandleCreateNewLVSShipment()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice1 = Factory.New<JobComInvoiceHeader>();
			invoice1.JZ_InvoiceNumber = "INV0001";
			var invoice2 = Factory.New<JobComInvoiceHeader>();
			invoice2.JZ_InvoiceNumber = "INV0002";
			Factory.Save();
			using (var module = new CommercialInvoiceModule())
			using (var form = new ZForm())
			{
				var menuItem = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Create New CLVS Shipment");
				AssertNotNull(menuItem);

				AssertEquals(0, module.GetSelectedBusinessObjects().Length);
				menuItem.PerformClick();
				AssertEquals("Please select one Commercial Invoice.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var filterControl = (CommercialInvoiceFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();
				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.SelectAllElements();
				AssertEquals(2, module.GetSelectedBusinessObjects().Length);
				menuItem.PerformClick();
				AssertEquals("Please select one Commercial Invoice.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				grid.UnSelectAll();
				grid.SelectSingleElementByPK(invoice1.PK);
				AssertEquals(1, module.GetSelectedBusinessObjects().Length);
				menuItem.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				using (var lastDialogForm = module.lvxController.LastShownForm as JobDeclarationForm)
				{
					AssertEquals(ControllerIDs.Customs.CA.CALVXJobs, lastDialogForm.ControllerID);
				}

				var newFactory = new BusinessObjectFactory();
				var newInvoice2 = newFactory.Load<JobComInvoiceHeader>(invoice2.PK);
				newInvoice2.JZ_JE = dec.PK;
				newFactory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				grid.UnSelectAll();
				grid.SelectSingleElementByPK(invoice2.PK);
				AssertEquals(1, module.GetSelectedBusinessObjects().Length);
				menuItem.PerformClick();
				AssertEquals("Please find Commercial Invoice not attached to any job.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
