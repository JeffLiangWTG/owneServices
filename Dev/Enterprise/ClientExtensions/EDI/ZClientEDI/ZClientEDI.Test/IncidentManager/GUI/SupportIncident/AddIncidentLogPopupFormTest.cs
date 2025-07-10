using System.Linq;
using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(AddIncidentLogPopupForm))]
	public class AddIncidentLogPopupFormTest : BaseIncidentPopupFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentLogCommentAction(incident);
			return new AddIncidentLogPopupForm(action);
		}

		public void TestAddInternalLogNotifyInternal()
		{
			var incident = Factory.New<SupportIncident>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "SCW";
			staff.GS_FullName = "Samuel";
			staff.GS_EmailAddress = "samuel@wisetechglobal.com";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "staff2@wisetechglobal.com";
			var participantStaff2 = incident.EConversation.Conversation.Participants.AddNewParticipant(staff2);
			participantStaff2.JCP_IsSubscribed = true;
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var incidentLogCommentAction = new SupportIncidentLogCommentAction(incident);

				using (var form = new AddIncidentLogPopupForm(incidentLogCommentAction))
				{
					form.Show();
					incidentLogCommentAction.Comment = "not send to internal subscribers";
					(form.Controls.Find("CloseButton", true).Single() as ZButton).PerformClick();
					Assert(incident.HasChanges);
					Factory.Save();
					AssertEquals("No outgoing emails should be created", 0,
						Env.OutgoingMailManager.EmailsCreated.Count);
					form.Dispose();
				}

				incidentLogCommentAction = new SupportIncidentLogCommentAction(incident);
				using (var form = new AddIncidentLogPopupForm(incidentLogCommentAction))
				{
					form.Show();
					incidentLogCommentAction.Comment = " send to internal subscribers";
					(form.Controls.Find("notifyInternalSubscribersCheckBox", true).Single() as ZCheckBox).Checked = true;
					(form.Controls.Find("CloseButton", true).Single() as ZButton).PerformClick();
					Assert(incident.HasChanges);
					Factory.Save();
					AssertEquals("emails should be created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
					var email = Env.OutgoingMailManager.EmailsCreated[0];
					AssertContains("send to internal subscribers", email.Body);
					form.Dispose();
				}

				incidentLogCommentAction = new SupportIncidentLogCommentAction(incident);
				using (var form = new AddIncidentLogPopupForm(incidentLogCommentAction))
				{
					form.Show();
					incidentLogCommentAction.Comment = "not send to internal subscribers";
					(form.Controls.Find("CloseButton", true).Single() as ZButton).PerformClick();
					Assert(incident.HasChanges);
					incident.AddStaffMessageToCustomer("Thanks for your reply");
					Factory.Save();
					AssertEquals("emails should be created", 2, Env.OutgoingMailManager.EmailsCreated.Count);
					var email2 = Env.OutgoingMailManager.EmailsCreated[1];
					AssertContains("Thanks for your reply", email2.Body);
					form.Dispose();
				}
				incident.AddStaffMessageToCustomer("Thanks for your reply");
				Factory.Save();
				AssertEquals("emails should be created", 3, Env.OutgoingMailManager.EmailsCreated.Count);
				var email3 = Env.OutgoingMailManager.EmailsCreated[2];
				AssertContains("Thanks for your reply", email3.Body);
			}
		}
	}
}
