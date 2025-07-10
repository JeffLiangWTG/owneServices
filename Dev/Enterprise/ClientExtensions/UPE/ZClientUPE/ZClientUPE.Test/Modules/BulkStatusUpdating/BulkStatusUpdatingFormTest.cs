using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(BulkStatusUpdatingForm))]
	public class BulkStatusUpdatingFormTest : ZFormBasherTest
	{
		public void TestFormHeader()
		{
			CusHAWB hAWB = Factory.New<CusHAWB>();
			BulkStatusUpdating businessEntity = BulkStatusUpdating.New(FilterBizObj, new IProcessQueueParent[] { hAWB });
			using (BulkStatusUpdatingForm form = new BulkStatusUpdatingForm(businessEntity))
			{
				form.Show();
				AssertEquals("Form.Text should be correct", "Bulk Status Update", form.FormHeading);
			}
		}

		public void TestShow_ErrorShownWhenNoItemsToUpdate()
		{
			using (BulkStatusUpdatingForm form = new BulkStatusUpdatingForm(FilterBizObj, System.Array.Empty<BusinessObject>()))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("A warning should be shown to the user because no items were selected to bulk update", true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("Form should not be made visible after the warning is shown", false, form.Visible);
			}
		}

		#region Button Event Handlers
		public void TestBulkUpdateButton()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CurrentQueue.P4_CustomsReason = "OriginalRemarks";
			Factory.Save();
			BusinessObject[] itemsToBulkUpdate = new BusinessObject[] { hAWB };
			using (TestBulkStatusUpdatingForm form = new TestBulkStatusUpdatingForm(FilterBizObj, itemsToBulkUpdate))
			{
				form.Show();
				Application.DoEvents();
				form.BusinessEntity.NonPersistentQueue.Reason = "NewRemarks";
				form.BulkUpdateButton.PerformClick();
				AssertEquals("Remarks should be updated if the user clicks yes", "NewRemarks", hAWB.CurrentQueue.P4_CustomsReason);
				AssertEquals("Form should be closed after bulk update has completed", false, form.Visible);
				AssertEquals("Success message should be shown to the user", true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Success message should be shown to the user", "1 item successfully updated", UnitTestUserNotification.Instance.LastMessage.Text.Trim());
			}
		}

		public void TestBulkUpdateButton_WhenNoItemsActuallyUpdated()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CurrentQueue.P4_CustomsReason = "SameRemarks";
			Factory.Save();
			BusinessObject[] itemsToBulkUpdate = new BusinessObject[] { hAWB };
			using (TestBulkStatusUpdatingForm form = new TestBulkStatusUpdatingForm(FilterBizObj, itemsToBulkUpdate))
			{
				form.Show();
				Application.DoEvents();
				form.BusinessEntity.NonPersistentQueue.Reason = "SameRemarks";
				form.BulkUpdateButton.PerformClick();
				AssertEquals("When no items need to be updated, an appropriate message should be shown", "No items were updated", UnitTestUserNotification.Instance.LastMessage.Text.Trim());
			}
		}

		public void TestBulkUpdateButton_ErrorMessageShownToUser()
		{
			UPECusHAWB hAWB = Factory.New<UPECusHAWB>();
			IProcessQueueParent[] itemsToBulkUpdate = new IProcessQueueParent[] { hAWB };
			using (TestBulkStatusUpdatingForm form = new TestBulkStatusUpdatingForm(FilterBizObj, itemsToBulkUpdate))
			{
				form.Show();
				Application.DoEvents();
				form.BulkUpdateButton.PerformClick();
				AssertEquals("Error message should be shown to the user (because no data was entered to bulk update)", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Error message should be shown to the user (because no data was entered to bulk update)", true, UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("Error: You must enter some data"));
				AssertEquals("Form should still be visible", true, form.Visible);
			}
		}

		public void TestCloseButton()
		{
			UPECusHAWB hAWB = Factory.New<UPECusHAWB>();
			IProcessQueueParent[] itemsToBulkUpdate = new IProcessQueueParent[] { hAWB };
			using (TestBulkStatusUpdatingForm form = new TestBulkStatusUpdatingForm(FilterBizObj, itemsToBulkUpdate))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Form should be visible for the test", true, form.Visible);
				form.CloseButton.PerformClick();
				Application.DoEvents();
				AssertEquals("Form should be closed when cancel is clicked", false, form.Visible);
			}
		}

		#endregion
		#region Implementation
		UPEAirCargoFilterBusinessObject FilterBizObj;
		protected override Form GetFormToBashCore()
		{
			return new BulkStatusUpdatingForm(FilterBizObj, System.Array.Empty<BusinessObject>());
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			FilterBizObj = new UPEAirCargoFilterBusinessObject();
		}
		#endregion
	}
}
