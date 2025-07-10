using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.AFR.GUI.Testing
{
	class SailingUserControlTest : TestCaseWithFactory
	{
		public void TestEditSailingButton()
		{
			const string sailingScheduleNotCreatedMessage = "There is no Sailing Schedule to edit.";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = true;
			using (var form = new JPAFRForm(header))
			{
				CombineAssertions(() =>
				{
					form.Show();
					var editSailingBtn = form.Controls.Find("EditSailingButton", true)[0] as ZButton;
					var sailingUserControl = form.Controls.Find("sailingUserControl", true)[0] as SailingUserControl;
					AssertNotNull(sailingUserControl);
					editSailingBtn.PerformClick();
					Application.DoEvents();
					AssertEquals(sailingScheduleNotCreatedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					var sailing = Factory.NewWithValidTestData<JobSailing>();
					header.ChangeSailing(sailing.PK);
					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					editSailingBtn.PerformClick();
					Application.DoEvents();
					AssertNotEquals(sailingScheduleNotCreatedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertType(typeof(ZJobVoyageForm), sailingUserControl.LastOpenedFormForTest);
					sailingUserControl.LastOpenedFormForTest.Dispose();
				});
			}
		}

		public void TestClearSailingButton()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = true;
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			header.ChangeSailing(sailing.PK);
			AssertNotNull(header.Sailing);
			using (var form = new JPAFRForm(header))
			{
				form.Show();
				var clearSailingBtn = form.Controls.Find("ClearSailingButton", true)[0] as ZButton;
				clearSailingBtn.PerformClick();
				Application.DoEvents();
				AssertNull(header.Sailing);
			}
		}

		public void TestCanChangeSailing()
		{
			const string cannotChangeSailingMessage = "Messages are sent to customs or in progress, you cannot change the sailing.";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = true;
			var bill = header.Bills.AddNew();
			bill.JPB_MessageStatus = MessageStatusList.Codes.AwaitingMasterBillRegistration;
			var sailing = Factory.NewWithValidTestData<JobSailing>();
			header.ChangeSailing(sailing.PK);
			using (var form = new JPAFRForm(header))
			{
				form.Show();
				var selectSailingBtn = form.Controls.Find("SelectSailingButton", true)[0] as ZButton;
				selectSailingBtn.PerformClick();
				Application.DoEvents();
				AssertEquals(cannotChangeSailingMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var clearSailingBtn = form.Controls.Find("ClearSailingButton", true)[0] as ZButton;
				clearSailingBtn.PerformClick();
				Application.DoEvents();
				AssertEquals(cannotChangeSailingMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
