using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class CMRACAStandAloneUserControlTest : TestCaseWithFactory
	{
		public void TestChildControl()
		{
			using (CMRACAStandAloneUserControl control = new CMRACAStandAloneUserControl())
			{
				AssertEquals(control.houseDetails, control.ChildControl);
			}
		}

		public void TestVisibility()
		{
			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			var mawb = Factory.New<CusMAWB>();
			using (ZForm form = new ZForm(mawb))
			{
				using (CMRACAStandAloneUserControl control = new CMRACAStandAloneUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(mawb, "");
					Assert(!control.deferredScheduleDateTextBox.Visible);
					mawb.AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage);
					Assert(control.deferredScheduleDateTextBox.Visible);
					mawb.CancelDeferredScheduledMessageLogs(CusMAWBBase.AirCargoReportLogReference);
					Assert(!control.deferredScheduleDateTextBox.Visible);
					Assert(!control.AltPartShipModelCheckBox.Visible);
				}
			}

			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			using (ZForm form = new ZForm(mawb))
			{
				using (CMRACAStandAloneUserControl control = new CMRACAStandAloneUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(mawb, "");
					Assert(control.AltPartShipModelCheckBox.Visible);
				}
			}
		}

		public void TestHousebillsGridExportToCSVMenu()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			using (ZForm form = new ZForm(mAWB))
			{
				using (CMRACAStandAloneUserControlForTest control = new CMRACAStandAloneUserControlForTest())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(mAWB, "");
					MenuItem contingencyMenuItem = null;
					foreach (MenuItem item in control.HouseBillsModuleButtonGrid.ContextMenu.MenuItems)
					{
						if (item.Text == "Create Contingency Data")
						{
							contingencyMenuItem = item;
							break;
						}
					}

					AssertNotNull("Context menu contains Contingency Data menu found", contingencyMenuItem);
					contingencyMenuItem.PerformClick();
					AssertEquals("Please select a Housebill before attempting to create Contingency Data.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Export not called", false, control.ExportCalled);
					mAWB.FilteredChildBills.AddNew();
					contingencyMenuItem.PerformClick();
					AssertEquals("Export called", true, control.ExportCalled);
				}
			}
		}

		sealed class CMRACAStandAloneUserControlForTest : CMRACAStandAloneUserControl
		{
			public override void ExportContingencyData(CusHAWB hAWB)
			{
				ExportCalled = true;
			}

			public bool ExportCalled;
		}
	}
}
