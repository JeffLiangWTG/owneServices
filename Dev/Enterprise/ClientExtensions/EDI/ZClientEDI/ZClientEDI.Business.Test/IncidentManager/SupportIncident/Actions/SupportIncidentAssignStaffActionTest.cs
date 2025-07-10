using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SupportIncidentAssignStaffAction))]
	public class SupportIncidentAssignStaffActionTest : SupportIncidentActionTestCase
	{
		public void TestCommentAddingToEConversationMaxLength()
		{
			var action = new SupportIncidentAssignStaffAction(Factory.New<SupportIncident>());
			AssertEquals("MaxLength", 32000, action.CommentInfo.MaxLength);
		}

		public void TestSupportStaffList()
		{
			GlbStaff dev1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff dev2 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff dev3 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff dev4 = Factory.NewWithValidTestData<GlbStaff>();
			dev4.GS_IsActive = ZBool.False;

			GlbGroup supportGroup = Factory.NewWithValidTestData<GlbGroup>();
			supportGroup.GG_Code = "SUPPORT";
			GlbStaff sup1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff sup2 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff sup3 = Factory.NewWithValidTestData<GlbStaff>();
			sup3.GS_IsActive = ZBool.False;
			supportGroup.Staff.Add(sup1);
			supportGroup.Staff.Add(sup2);
			supportGroup.Staff.Add(sup3);

			Factory.Save();

			EDIDataRegistry.Instance.IncidentSupportGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, supportGroup.PK.ToGuid());

			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentAssignStaffAction(incident);
			AssertEquals("2 support staff, excluding inactive one", 2, action.SupportStaffList.Count);

			Assert("more than 5 staff including support", action.AllStaffList.Count > 5);
			Assert("inactive staff should be excluded", !action.AllStaffList.Contains(sup3));
			Assert("inactive staff should be excluded", !action.AllStaffList.Contains(dev4));

			EDIDataRegistry.Instance.IncidentSupportGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);

			action = new SupportIncidentAssignStaffAction(incident);
			Assert("All staff (excluding inactive) loaded as registry blank", action.SupportStaffList.Count > 5);
			Assert("inactive staff should be excluded", !action.SupportStaffList.Contains(sup3));
			Assert("inactive staff should be excluded", !action.SupportStaffList.Contains(dev4));
		}

		public void TestAssignToSelf()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentAssignStaffAction(incident);

			action.AssignToSelf = true;
			AssertEquals(GlbStaff.CurrentUser.PK, action.StaffPK);
			action.Comment = "Some cool comment";
			action.SynchroniseToIncident();
			AssertEquals(GlbStaff.CurrentUser.PK, incident.CustServiceContact.PK);

			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Some cool comment"));
		}

		public void TestAssignToOther()
		{
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentAssignStaffAction(incident);

			action.AssignToOther = true;
			AssertEquals(false, action.AssignToSelf);
			action.StaffPK = staff2.PK;
			action.Comment = "Some other comment";
			action.SynchroniseToIncident();
			AssertEquals(staff2.PK, incident.CustServiceContact.PK);
			AssertEquals("OPN", incident.IM_Status);
			AssertEquals("AUC", incident.IM_ResolutionCode);

			var messageList = incident.EConversation.GetTimeOrderedMessages();
			AssertEquals(true, messageList.Any(msg => msg.Body == "Some other comment"));
		}

		public void TestSelectedStaff()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentAssignStaffAction(incident);
			AssertNull(action.Staff);

			action.StaffPK = GlbStaff.CurrentUser.PK;
			AssertEquals(GlbStaff.CurrentUser.PK, action.Staff.PK);
		}

		public void TestStaffPKReadonly()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentAssignStaffAction(incident);
			AssertEquals(true, action.StaffPKInfo.ReadOnly);

			action.AssignToOther = true;
			AssertEquals(false, action.StaffPKInfo.ReadOnly);

			action.AssignToOther = false;
			AssertEquals(true, action.StaffPKInfo.ReadOnly);
		}

		public void TestAssignToPassesComment()
		{
			var incident = Factory.New<SupportIncident>();
			var action = new SupportIncidentAssignStaffAction(incident);

			action.AssignToOther = true;
			action.StaffPK = GlbStaff.CurrentUser.PK;
			action.Comment = "BLAHBLAHBLAH";

			action.SynchroniseToIncident();
			AssertEquals("BLAHBLAHBLAH", incident.AssignmentComment);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var incident = Factory.New<SupportIncident>();
			return new SupportIncidentAssignStaffAction(incident);
		}
	}
}
