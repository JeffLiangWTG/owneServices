using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using MessageStatusList = Enterprise.Customs.CA.Business.MessageStatusList;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class LVSSubHeadersGridTest : TestCaseWithFactory
	{
		public void TestDetach()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_DeclarationReference = "B00000001";
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration1 = Factory.Load<JobDeclaration>(declaration1.PK);
			var invoice1 = declaration1.Invoices.AddNew();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_DeclarationReference = "B00000002";
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice2 = declaration2.Invoices.AddNew();
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(invoice2, declaration1);
			var invoice3 = declaration2.Invoices.AddNew();
			invoice3.JZ_JE = declaration2.PK;
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(invoice3, declaration1);
			var entryHeader = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			AssertCollectionContains("invoice2 should be attached to declaration1", invoice2, declaration1.Invoices);
			AssertCollectionContains("invoice2.GroupHeader should be attached to declaration1", invoice2.GroupHeader, declaration1.JobComInvoiceGroupHeaders);
			AssertCollectionContains("invoice3 should be attached to declaration1", invoice3, declaration1.Invoices);

			Factory.Save();
			using (var form = new JobDeclarationForm(declaration1))
			{
				form.Show();
				var grid = (LVSSubHeadersGrid)form.Controls.Find("LVSSubHeadersGrid", true)[0];
				var detachMenuItem = grid.ContextMenu.MenuItems.FindByText("Detach");
				var deleteMenuItem = grid.ContextMenu.MenuItems.FindByText("Delete");
				AssertNotNull(detachMenuItem);
				AssertEquals("Detach menu item should be added after Delete menu item", 1, detachMenuItem.Index - deleteMenuItem.Index);

				grid.SelectAllElements();
				detachMenuItem.PerformClick();
				var messages = UnitTestUserNotification.Instance.PreviousMessages;
				AssertEquals("Shipments that have not been consolidated from the Courier LVS Declarations module may not be detached. They may be deleted.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("B00000002 's additional declaration B00000001 has had a CAD accepted or is waiting for a response, it cannot be detached.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
				AssertCollectionContains("invoice1 should not be detached from declaration1", invoice1, declaration1.Invoices);
				AssertCollectionContains("invoice2 should not be detached from declaration1", invoice2, declaration1.Invoices);
				AssertCollectionContains("invoice3 should not be detached from declaration1", invoice3, declaration1.Invoices);

				entryHeader.Delete();

				UnitTestUserNotification.Instance.ClearMessages();
				grid.SelectAllElements();
				detachMenuItem.PerformClick();
				messages = UnitTestUserNotification.Instance.PreviousMessages;
				AssertEquals("Shipments that have not been consolidated from the Courier LVS Declarations module may not be detached. They may be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertCollectionContains("invoice1 should not be detached from declaration1", invoice1, declaration1.Invoices);
				AssertCollectionNotContains("invoice2 should be detached from declaration1", invoice2, declaration1.Invoices);
				AssertCollectionNotContains("invoice2.GroupHeader should be detached from declaration1", invoice2.GroupHeader, declaration1.JobComInvoiceGroupHeaders);
				AssertCollectionNotContains("invoice3 should be detached from declaration1", invoice3, declaration1.Invoices);
				AssertCollectionNotContains("invoice3.GroupHeader should be detached from declaration1", invoice3.GroupHeader, declaration1.JobComInvoiceGroupHeaders);
			}
		}

		public void TestDelete()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration1 = Factory.Load<JobDeclaration>(declaration1.PK);
			var invoice1 = declaration1.Invoices.AddNew();
			invoice1.InvoiceLines.AddNew();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice2 = declaration2.Invoices.AddNew();
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(invoice2, declaration1);
			invoice2.InvoiceLines.AddNew();
			AssertCollectionContains("invoice2 should be attached to declaration1", invoice2, declaration1.Invoices);

			using (var form = new JobDeclarationForm(declaration1))
			{
				form.Show();
				var grid = (LVSSubHeadersGrid)form.Controls.Find("LVSSubHeadersGrid", true)[0];
				var deleteMenuItem = grid.ContextMenu.MenuItems.FindByText("Delete");

				grid.SelectAllElements();
				deleteMenuItem.PerformClick();
				AssertEquals("Shipments which have been consolidated from the Courier LVS Declarations module may not be deleted. They may be detached.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertCollectionNotContains("invoice1 should be detached from declaration1", invoice1, declaration1.Invoices);
				Assert("invoice1 should be deleted", invoice1.IsDeleted);
				AssertCollectionContains("invoice2 should not be detached from declaration1", invoice2, declaration1.Invoices);
				Assert("invoice2 should not be deleted", !invoice2.IsDeleted);
			}
		}

		public void TestEdit()
		{
			var lvsJob = Factory.New<JobDeclaration>();
			lvsJob.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var invoice = lvsJob.Invoices.AddNew();
			var lvxJob = Factory.New<JobDeclaration>();
			lvxJob.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(lvxJob.LVXInvoiceHeader, lvsJob);

			var editMenuItemVisible = false;
			var editMenuItemEnable = false;

			using (var form = new JobDeclarationForm(lvsJob))
			{
				form.Show();
				var grid = (LVSSubHeadersGrid)form.Controls.Find("LVSSubHeadersGrid", true)[0];
				grid.MousePositionForTesting = grid.PointToScreen(new System.Drawing.Point(50, 25));

				grid.ContextMenu.Popup += delegate
				{
					var editMenuItem = grid.ContextMenu.MenuItems.FindByText("Edit");
					if (editMenuItem != null)
					{
						editMenuItemVisible = editMenuItem.Visible;
						editMenuItemEnable = editMenuItem.Enabled;
					}

					grid.ContextMenu.Dispose(); // cannot use SendKeys.Send("{ESC}") because it fails on DAT due to locked machine.
				};

				grid.SelectSingleElement(invoice);
				grid.ContextMenu.Show(grid, new System.Drawing.Point(0, 0));
				Assert("Visible", editMenuItemVisible);
				Assert("Enable", !editMenuItemEnable);
			}

			Factory.Save();
			editMenuItemVisible = false;
			using (var form = new JobDeclarationForm(lvsJob))
			{
				form.Show();
				var grid = (LVSSubHeadersGrid)form.Controls.Find("LVSSubHeadersGrid", true)[0];
				grid.MousePositionForTesting = grid.PointToScreen(new System.Drawing.Point(50, 40));

				grid.ContextMenu.Popup += delegate
				{
					var editMenuItem = grid.ContextMenu.MenuItems.FindByText("Edit");
					if (editMenuItem != null)
					{
						editMenuItemVisible = editMenuItem.Visible;
						editMenuItemEnable = editMenuItem.Enabled;
						if (editMenuItemVisible && editMenuItemEnable)
						{
							editMenuItem.PerformClick();
						}
					}
					grid.ContextMenu.Dispose(); // cannot use SendKeys.Send("{ESC}") because it fails on DAT due to locked machine.
				};

				grid.SelectSingleElement(lvxJob.LVXInvoiceHeader);
				grid.ContextMenu.Show(grid, new System.Drawing.Point(0, 0));
				Assert("Visible", editMenuItemVisible);
				Assert("Enable", editMenuItemEnable);

				var lastShownForm = ZFormModaliser.LastFormShownForTest as JobDeclarationForm;
				AssertEquals(ControllerIDs.Customs.CA.CALVXJobs, lastShownForm.ControllerID);
				AssertEquals(form, ZFormModaliser.GetParentFormForModalForm(lastShownForm));
			}
		}
	}
}
