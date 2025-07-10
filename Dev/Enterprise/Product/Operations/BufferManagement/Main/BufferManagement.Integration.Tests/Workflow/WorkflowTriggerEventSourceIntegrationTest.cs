using System.Linq;
using System.Xml.Linq;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Integration.Tests
{
	class WorkflowTriggerEventSourceIntegrationTest : BMSTestCaseWithFactory
	{
		public void TestManuallyAddedEventToStaffAssignment_ShouldExposeTriggeringEventToNotificationMacro()
		{
			var system = BMSTestHelper.CreateSystemAndRelatedWorkflowType(Factory, "ORG", isActive: true);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "SAM", "Samwise the Brave");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");

			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			trigger.P9_Description = "Arrrival";

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			triggerAction.PQ_EmailAddr = "this@daveeast.com";
			triggerAction.PQ_EmailText =
@"Something arrived in (*OH_Code*):<br />
<br />
The thing which arrived was:<br />
(*TriggeringEvent.SL_TableFriendlyName*)<br />
<br />
It arrived under the watch of:<br />
(*TriggeringEvent.User.GS_FullName*)<br />
";

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "NTHWRNG";

			var staffAssignment = job.StaffAssignments.AddNew();
			staffAssignment.O8_GS_NKPersonResponsible = resource.GS_Code;

			Factory.Save();

			using (Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				staffAssignment.Logs.AddNew(Events.Arrival);
			}

			Factory.Save();

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			BMSTestHelper.RunLogWalker();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Organization - Arrrival", sentEmail.Subject);

			var body = XDocument.Parse(sentEmail.Body);
			var contentElement = (
				from element in body.Root.DescendantNodes().OfType<XElement>()
				where element.Name.LocalName == "td"
				from attribute in element.Attributes()
				where attribute.Name.LocalName == "class"
				where attribute.Value == "content"
				select element
				).FirstOrDefault();

			AssertNotNull(contentElement);
			AssertMultilineASCIIEquals("", @"
Something arrived in NTHWRNG:
The thing which arrived was:
This Staff Assignment
It arrived under the watch of:
Samwise the Brave
", contentElement.Value);
		}
	}
}
