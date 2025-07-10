using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class NonConstrainedResourcesChannelTest : BMSTestCaseWithFactory
	{
		public void TestIsInChannel()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);

			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			section.SectionConfiguration.IsReleaseScheduler = true;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			var resource3 = Factory.NewWithValidTestData<GlbStaff>();

			group.Staff.AddRange(resource1, resource2, resource3);

			resource1.DesignateAsCCR(buffer);
			resource2.DesignateAsCCR(buffer);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channels = viewModel.PrimaryChannels.ToArray();
			AssertEquals(3, channels.Length);
			var channel = channels[2];

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var task1_1 = CreateTask(workflow1, resource1.GS_Code, 60);
			var task1_2 = CreateTask(workflow1, resource2.GS_Code, 60);
			var task1_3 = CreateTask(workflow1, resource3.GS_Code, 60);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var task2_1 = CreateTask(workflow2, resource3.GS_Code, 60);

			AssertEquals("Should not be in non-constrained channel since workflow contains tasks for constrained resources", false, channel.IsInChannel(task1_1, false));
			AssertEquals("Should not be in non-constrained channel since workflow contains tasks for constrained resources", false, channel.IsInChannel(task1_2, false));
			AssertEquals("Should not be in non-constrained channel since workflow contains tasks for constrained resources", false, channel.IsInChannel(task1_3, false));
			AssertEquals("Should be in non-constrained channel since workflow contains no tasks for constrained resources", true, channel.IsInChannel(task2_1, false));

			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			viewModel = BMSTestHelper.CreateViewModel(section);
			channels = viewModel.PrimaryChannels.ToArray();
			AssertEquals(3, channels.Length);
			channel = channels[2];

			AssertEquals("Should not be in non-constrained channel since workflow contains tasks for constrained resources", false, channel.IsInChannel(task1_1, true));
			AssertEquals("Should not be in non-constrained channel since workflow contains tasks for constrained resources", false, channel.IsInChannel(task1_2, true));
			AssertEquals("Should not be in non-constrained channel since workflow contains tasks for constrained resources", false, channel.IsInChannel(task1_3, true));
			AssertEquals("Should not be in non-constrained channel since workflow contains tasks for constrained resources", false, channel.IsInChannel(task2_1, true));

			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow2_1 = jobHeader2.ProcessHeaders[0];
			var task1_workflow2_1 = CreateTask(workflow2_1, resource3.GS_Code, 60);

			var workflow2_2 = jobHeader2.ProcessHeaders.AddNew();
			var task2_workflow2_2 = CreateTask(workflow2_2, resource3.GS_Code, 60);

			AssertEquals("Should be in non-constrained channel Release section", true, channel.IsInChannel(task1_workflow2_1, true));
			AssertEquals("Should be in non-constrained channel Release section", true, channel.IsInChannel(task2_workflow2_2, true));

			workflow2_1.CurrentComponent.FC_Type = BMComponentTypeList.Codes.Bucket;
			workflow2_2.CurrentComponent.FC_Type = BMComponentTypeList.Codes.Bucket;

			AssertEquals("Should be in non-constrained channel Un-Release section", true, channel.IsInChannel(task1_workflow2_1, true));
			AssertEquals("Should be in non-constrained channel Un-Release section", true, channel.IsInChannel(task2_workflow2_2, true));
		}
	}
}
