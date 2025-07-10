using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(BaseIncidentPopupForm))]
	public class BaseIncidentPopupFormTest : ZFormBasherTest
	{
		public void TestFormVerb()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentActionForTest(incident);
			using (BaseIncidentPopupForm form = new BaseIncidentPopupForm(action))
			{
				AssertEquals("", form.FormVerb);
			}
		}

		public void TestCloseButton()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentAssignStaffAction(incident);
			action.StaffPK = ZGuid.Invalid;
			using (BaseIncidentPopupForm form = new BaseIncidentPopupForm(action))
			{
				form.Show();
				form.AcceptButton.PerformClick();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(DialogResult.None, form.DialogResult);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				action.StaffPK = ZGuid.Empty;
				form.AcceptButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestCancelButton()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentAssignStaffAction(incident);
			action.StaffPK = ZGuid.Invalid;
			using (BaseIncidentPopupForm form = new BaseIncidentPopupForm(action))
			{
				form.Show();
				form.CancelButton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestOnSuccessfulClose()
		{
			var action = new SupportIncidentAssignStaffAction(Factory.New<SupportIncident>());
			using (BaseIncidentPopupFormForTesting form = new BaseIncidentPopupFormForTesting(action))
			{
				form.Show();
				Assert(!form.OnSuccessfulCloseWasCalled);
				form.CancelButton.PerformClick();
				Assert(!form.OnSuccessfulCloseWasCalled);
			}

			action.StaffPK = ZGuid.Empty;
			using (BaseIncidentPopupFormForTesting form = new BaseIncidentPopupFormForTesting(action))
			{
				form.Show();
				Assert(!form.OnSuccessfulCloseWasCalled);
				form.AcceptButton.PerformClick();
				Assert(form.OnSuccessfulCloseWasCalled);
			}
		}

		protected override Form GetFormToBashCore()
		{
			SupportIncident incident = Factory.New<SupportIncident>();
			SupportIncidentAction action = new SupportIncidentActionForTest(incident);
			return new BaseIncidentPopupForm(action);
		}
	}
}
