using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class UnchanneledChannelTest : BMSTestCaseWithFactory
	{
		public void TestChannel_Resource()
		{
			var channel = new UnchanneledChannel(ChannelTypeList.Codes.Resource);

			var job = Factory.New<OrgHeader>();
			var task1 = job.WorkflowItems.AddNew();
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			var task2 = job.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = ZString.Empty;

			VisualBoardChannelTest.AssertVisualBoardChannel(channel, "Un-channeled", "Un-channeled", "Un-channeled", ChannelTypeList.Codes.Resource, null, null, new[] { task2 }, new[] { task1 });
		}

		public void TestChannel_Group()
		{
			var channel = new UnchanneledChannel(ChannelTypeList.Codes.Group);
			var group = Factory.New<GlbGroup>();

			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_GG_ReleaseGroup = group.PK;

			var task1 = job.WorkflowItems.AddNew();
			task1.P9_GG_AssignedGroup = group.PK;
			task1.P9_FH_ProcessHeader = workflow1.PK;
			var task2 = job.WorkflowItems.AddNew();
			task2.P9_GG_AssignedGroup = ZGuid.Empty;
			task2.P9_FH_ProcessHeader = workflow1.PK;
			var task3 = job.WorkflowItems.AddNew();
			task3.P9_GG_AssignedGroup = ZGuid.Empty;
			task3.P9_FH_ProcessHeader = workflow2.PK;

			VisualBoardChannelTest.AssertVisualBoardChannel(channel, "Un-channeled", "Un-channeled", "Un-channeled", ChannelTypeList.Codes.Group, null, null, new[] { task2 }, new[] { task1, task3 });
		}

		public void TestChannel_Capability()
		{
			var channel = new UnchanneledChannel(ChannelTypeList.Codes.Capability);

			var job = Factory.New<OrgHeader>();
			var task1 = job.WorkflowItems.AddNew();
			task1.P9_G4_RequiredCapability = Factory.New<GlbCapability>().PK;
			var task2 = job.WorkflowItems.AddNew();
			task2.P9_G4_RequiredCapability = ZGuid.Empty;

			VisualBoardChannelTest.AssertVisualBoardChannel(channel, "Un-channeled", "Un-channeled", "Un-channeled", ChannelTypeList.Codes.Capability, null, null, new[] { task2 }, new[] { task1 });
		}
	}
}
