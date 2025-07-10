using System.Drawing;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;

namespace Enterprise.BufferManagement.Business.Test
{
	public class AsyncBMBoardSectionViewModelTest : BMSTestCaseWithFactory
	{
		public void TestWithAsyncDisabledWorksAsExpected()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var acceptabilityBand = CreateAcceptabilityBand_WorkflowsInComponent(bucket, 10, 12, 14, 16, 18, 21);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup1 = CreateReleaseGroup(system, group1);

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup2 = CreateReleaseGroup(system, group2);

			var section = CreateBoardSection(bucket);
			BMSTestHelper.AddAcceptabilityBandToSection(section, acceptabilityBand, AcceptabilityBandShowOnOption.Heading);

			Factory.Save();

			var boardViewModel = BMSTestHelper.CreateBoardViewModel(section.Board);

			using (DisableAsyncBehaviour())
			{
				var viewModel = new BMBoardSectionViewModel(section, boardViewModel);

				AssertAcceptabilityBandSubheadingDetails(viewModel, section, "Skip loading when threads are disabled.", "Status: High Risk",
	@"High Risk: Number of Workflows: 0 (target is between 10 and 21)", BMConstants.HighRiskBoardColor);
			}
		}

		public void TestStepThroughAsync()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var acceptabilityBand = CreateAcceptabilityBand_WorkflowsInComponent(bucket, 10, 12, 14, 16, 18, 21);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup1 = CreateReleaseGroup(system, group1);

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup2 = CreateReleaseGroup(system, group2);

			var section = CreateBoardSection(bucket);
			BMSTestHelper.AddAcceptabilityBandToSection(section, acceptabilityBand, AcceptabilityBandShowOnOption.Heading);

			Factory.Save();

			var boardViewModel = BMSTestHelper.CreateBoardViewModel(section.Board);
			var asyncTrigger = new TriggerableAsyncStrategy();
			using (ApplyAsyncStrategy(asyncTrigger))
			{
				var viewModel = new BMBoardSectionViewModel(section, boardViewModel);

				AssertAcceptabilityBandSubheadingDetails(viewModel.SubHeadingAppearance, "Loading mode!", " ",
	@"Loading acceptability bands in the background.", Color.AliceBlue);

				BMSTestHelper.CreateAndPopulateBoardSectionPropertyCacheWithEmptyTasks(viewModel, section);
				var results = viewModel.GetAcceptabilityBandResults(Factory);
				viewModel.RefreshAcceptabilityBandSubHeading(results);
				asyncTrigger.DoAllActions();

				AssertAcceptabilityBandSubheadingDetails(viewModel.SubHeadingAppearance, "Finally working.", "Status: High Risk",
	@"High Risk: Number of Workflows: 0 (target is between 10 and 21)", BMConstants.HighRiskBoardColor);
			}
		}
	}
}
