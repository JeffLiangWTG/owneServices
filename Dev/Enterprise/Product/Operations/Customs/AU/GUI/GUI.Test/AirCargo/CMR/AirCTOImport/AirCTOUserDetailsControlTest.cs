using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class AirCTOUserDetailsControlTest : TestCaseWithFactory
	{
		public void TestShowAirCTOHawbImportForm_Success()
		{
			CTOCusMAWB mawb = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hawb1 = mawb.ChildBills.AddNew();
			CTOCusHAWB hawb2 = mawb.ChildBills.AddNew();
			Factory.Save();
			using (ZForm form = new ZForm(mawb))
			{
				AirCTOUserDetailsControl control = new AirCTOUserDetailsControl();
				control.Dock = DockStyle.Fill;
				form.ClientSize = control.Size;
				form.Controls.Add(control);
				control.SetDataBinding(mawb, "");
				form.Show();
				control.lastHawbController = null;
				control.masterBillsGrid.ListManager.Position = control.masterBillsGrid.ListManager.List.IndexOf(hawb2);
				control.masterBillsGrid.PerformDoubleClickForTest();
				AssertNotNull(control.lastHawbController);
				using (ZForm subForm = (ZForm)control.lastHawbController.LastShownForm)
				{
					AssertNotNull("Should have shown a form", subForm);
					AssertEquals("Should have shown the correct form", typeof(AirCTOHawbImportForm), subForm.GetType());
					AssertEquals("Should show the correct BizObj", hawb2.PK, ((BusinessObject)subForm.BusinessEntity).PK);
				}
			}
		}

		public void TestShowAirCTOHawbImportForm_NoHawbSelected()
		{
			CTOCusMAWB mawb = Factory.New<CTOCusMAWB>();
			Factory.Save();
			using (ZForm form = new ZForm(mawb))
			{
				AirCTOUserDetailsControl control = new AirCTOUserDetailsControl();
				control.Dock = DockStyle.Fill;
				form.ClientSize = control.Size;
				form.Controls.Add(control);
				control.SetDataBinding(mawb, "");
				form.Show();
				AssertEquals("precondition: ", -1, control.masterBillsGrid.ListManager.Position);
				control.lastHawbController = null;
				control.masterBillsGrid.PerformDoubleClickForTest();
				AssertNull("Dont show a form if no hawb selected", control.lastHawbController);
			}
		}

		public void TestShowAirCTOHawbImportForm_UnsavedChanges()
		{
			CTOCusMAWB mawb = Factory.New<CTOCusMAWB>();
			mawb.CM_MAWB = "MAWB";
			CTOCusHAWB hawb1 = mawb.ChildBills.AddNew();
			CTOCusHAWB hawb2 = mawb.ChildBills.AddNew();
			AssertEquals("precondition: ", true, mawb.HasChanges);
			using (ZForm form = new ZForm(mawb))
			{
				AirCTOUserDetailsControl control = new AirCTOUserDetailsControl();
				control.Dock = DockStyle.Fill;
				form.ClientSize = control.Size;
				form.Controls.Add(control);
				control.SetDataBinding(mawb, "");
				form.Show();
				control.lastHawbController = null;
				control.masterBillsGrid.ListManager.Position = control.masterBillsGrid.ListManager.List.IndexOf(hawb2);
				control.masterBillsGrid.PerformDoubleClickForTest();
				AssertNull("Form should not be shown", control.lastHawbController);
				AssertEquals("Should have shown an error message", "Error The current form has changes, you will need to save before opening this HAWB.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestMessageUserControlBindTo()
		{
			using (AirCTOUserDetailsControl control = new AirCTOUserDetailsControl())
			{
				AssertEquals("MessagesUserControl.BindingMember", "ChildBills", control.messagesUserControl.GetBindingMember());
				AssertEquals("MessagesGrid.BindTo", "Messages", control.messagesUserControl.MessagesGrid.GetBindingMember());
				AssertEquals("MessageTextTextBox.BindTo", "Messages.EM_FormattedMessageText", control.messagesUserControl.MessageTextTextBox.GetBindingMember());
			}
		}

		public void TestCurrentSelectedHAWB()
		{
			using (AirCTOUserDetailsControl control = new AirCTOUserDetailsControl())
			{
				AssertNull(control.HAWB);
			}

			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();
			mAWB.CM_MAWB = "1234";
			CTOCusHAWB hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_HAWB = "1111111";
			CTOCusHAWB hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_HAWB = "2222222";
			using (ZForm form = new ZForm(mAWB))
			{
				using (AirCTOUserDetailsControl control = new AirCTOUserDetailsControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(mAWB, "");
					AssertEquals(hAWB1, control.HAWB);
					control.masterBillsGrid.ListManager.Position = control.masterBillsGrid.ListManager.List.IndexOf(hAWB2);
					AssertEquals(hAWB2, control.HAWB);
					control.masterBillsGrid.ListManager.Position = control.masterBillsGrid.ListManager.List.IndexOf(hAWB1);
					AssertEquals(hAWB1, control.HAWB);
				}
			}
		}

		public void TestChildForStatuses()
		{
			using (AirCTOUserDetailsControl control = new AirCTOUserDetailsControl())
			{
				AssertEquals(control.ctoHouseDetailsUserControl, control.ChildControl);
			}
		}

		public void TestHousebillsGridExportToCSVMenu()
		{
			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();
			using (ZForm form = new ZForm(mAWB))
			{
				using (AirCTOUserDetailsControlForTest control = new AirCTOUserDetailsControlForTest())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(mAWB, "");
					MenuItem contingencyMenuItem = null;
					foreach (MenuItem item in control.masterBillsGrid.ContextMenu.MenuItems)
					{
						if (item.Text == "Create Contingency Data")
						{
							contingencyMenuItem = item;
							break;
						}
					}

					AssertNotNull("Context menu contains Contingency Data menu found", contingencyMenuItem);
					contingencyMenuItem.PerformClick();
					AssertEquals("Please select a row before attempting to create Contingency Data.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Export not called", false, control.ExportCalled);
					mAWB.ChildBills.AddNew();
					contingencyMenuItem.PerformClick();
					AssertEquals("Export called", true, control.ExportCalled);
				}
			}
		}

		public void TestMessageTabInsertedAsSecondTabPage()
		{
			CTOCusMAWB mAWB = Factory.New<CTOCusMAWB>();
			using (ZForm form = new ZForm(mAWB))
			{
				using (AirCTOUserDetailsControlForTest control = new AirCTOUserDetailsControlForTest())
				{
					form.Controls.Add(control);
					form.Show();
					AssertEquals("Tab pages on HAWBTabControl", 2, control.HAWBTabControl.TabPages.Count);
					AssertEquals("1st tab page", "HouseDetailsTabPage", control.HAWBTabControl.TabPages[0].Name);
					AssertEquals("2nd tab page", "MessagesTabPage", control.HAWBTabControl.TabPages[1].Name);
				}
			}
		}

		sealed class AirCTOUserDetailsControlForTest : AirCTOUserDetailsControl
		{
			protected override void ExportContingencyData(CTOCusHAWB hAWB)
			{
				ExportCalled = true;
			}

			public bool ExportCalled;
		}
	}
}
