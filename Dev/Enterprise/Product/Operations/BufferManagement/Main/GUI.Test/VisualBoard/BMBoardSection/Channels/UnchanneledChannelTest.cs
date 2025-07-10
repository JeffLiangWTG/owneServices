using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	class UnchanneledChannelTest : BMSTestCaseWithFactory
	{
		public void TestChannel_UnchanneledGetCapabilityAssignmentButtons()
		{
			var currentChannel = new UnchanneledChannel(ChannelTypeList.Codes.NotChanneled);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var task = BMSTestHelper.CreateTask(jobHeader.ProcessHeaders[0], GlbStaff.CurrentUser.GS_Code);

			var system = BMSTestHelper.CreateSystem(Factory);

			var compontent = system.Components.AddNew();
			compontent.FC_Name = "Mai Component";
			compontent.FC_Type = BMComponentTypeList.Codes.Bucket;

			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(compontent, board);
			var sectionViewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			const string message = "Would you like to assign tasks related to 'XXX' to 'XXX'?";
			var dialogProvider = new MultiActionButtonDialogWrapper<CrossChannelTaskAssignments>();
			ChannelReassignmentHandler.GetCrossChannelTaskAssignmentType(task, currentChannel, sectionViewModel, dialogProvider, message, t => HandleTaskAssignmentCore(t, currentChannel), currentChannel);

			AssertEquals(message, dialogProvider.LastMessage);
		}

		static void HandleTaskAssignmentCore(ProcessTask processTask, IVisualBoardChannel currentChannel)
		{
			var staffCode = currentChannel != null && currentChannel.EntityType == ChannelTypeList.Codes.Resource ? currentChannel.ChannelEntityCode : GlbStaff.CurrentUser.GS_Code;
			processTask.P9_GS_NKAssignedStaffMember = staffCode;
		}
	}
}
