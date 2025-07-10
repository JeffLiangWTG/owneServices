using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(AcceptabilityBandTileViewModel))]
	class AcceptabilityBandTileViewModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var group = Factory.New<GlbGroup>();
			sectionBand.BoardSection.Board.MB_GG_ReleaseGroup = group.PK;
			Factory.Save();

			var results = sectionViewModel.GetAcceptabilityBandResults(Factory);
			var viewModel = new AcceptabilityBandTileViewModel(sectionBand, sectionViewModel, results.Single(x => x.SectionBand.AcceptabilityBandPK == band.PK).Result);

			AssertEquals(sectionBand.AcceptabilityBandPK, viewModel.AcceptabilityBandPK);
			AssertEquals(group.PK, viewModel.ReleaseGroupPK);
			AssertEquals("Dat Band", viewModel.DisplayName);
			AssertEquals("rains in Spain", viewModel.ResultUnits);

			AssertEquals(0, viewModel.BoundaryValues.CautionMin);
			AssertEquals(1, viewModel.BoundaryValues.GoodMin);
			AssertEquals(2, viewModel.BoundaryValues.ExcellentMin);
			AssertEquals(3, viewModel.BoundaryValues.ExcellentMax);
			AssertEquals(4, viewModel.BoundaryValues.GoodMax);
			AssertEquals(5, viewModel.BoundaryValues.CautionMax);
		}

		public void TestCalculate()
		{
			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false).ProcessHeaders.AddNew();
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			var results = sectionViewModel.GetAcceptabilityBandResults(Factory);
			var viewModel = new AcceptabilityBandTileViewModel(sectionBand, sectionViewModel, results.Single(x => x.SectionBand.AcceptabilityBandPK == band.PK).Result);

			AssertEquals("1 rains in Spain", viewModel.Result);
			AssertEquals("Status: Good", viewModel.StatusText);
			AssertEquals(ComponentAcceptabilityStatus.Good, viewModel.Status);
		}

		public void TestFiltersByReleaseGroup()
		{
			var releaseGroup1 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup2 = Factory.NewWithValidTestData<GlbGroup>();
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_GG_ReleaseGroup = releaseGroup1.PK;
			workflow2.FH_GG_ReleaseGroup = releaseGroup2.PK;
			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			section.MS_GG_ReleaseGroup = releaseGroup1.PK;
			band.BAB_FiltersByReleaseGroup = false;

			Factory.Save();

			var results = sectionViewModel.GetAcceptabilityBandResults(Factory);
			var viewModel = new AcceptabilityBandTileViewModel(sectionBand, sectionViewModel, results.Single().Result);

			AssertEquals("2 rains in Spain", viewModel.Result);
			AssertEquals("Status: Excellent", viewModel.StatusText);
			AssertEquals(ComponentAcceptabilityStatus.Excellent, viewModel.Status);

			band.BAB_FiltersByReleaseGroup = true;
			Factory.Save();

			sectionViewModel = BMSTestHelper.CreateViewModel(section);
			results = sectionViewModel.GetAcceptabilityBandResults(Factory);
			viewModel = new AcceptabilityBandTileViewModel(sectionBand, sectionViewModel, results.Single().Result);

			AssertEquals("The band should now filter by release group, and yet...", "1 rains in Spain", viewModel.Result);
			AssertEquals("Status: Good", viewModel.StatusText);
			AssertEquals(ComponentAcceptabilityStatus.Good, viewModel.Status);
		}

		BMComponentAcceptabilityBand band;
		BoardSectionAcceptabilityBand sectionBand;
		BMBoardSectionViewModel sectionViewModel;
		BMBoardSection section;

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			BMSTestHelper.DisableAcceptabilityBandResultCache();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			section = BMSTestHelper.CreateBoardSection(bucket);

			band = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(bucket, 0, 1, 2, 3, 4, 5);
			sectionBand = section.SectionConfiguration.AcceptabilityBands.AddNew();
			sectionBand.AcceptabilityBandPK = band.PK;
			sectionBand.DisplayName = "Dat Band";
			sectionBand.DisplayUnits = "rains in Spain";

			sectionViewModel = BMSTestHelper.CreateViewModel(section);
			disposables = new DisposableList(new[] { DummySecondaryServerConnectionProvider.TemporarilyEnableDummyProvider() });
		}
		DisposableList disposables;

		#region TearDown

		protected override void TearDown()
		{
			base.TearDown();

			disposables.Dispose();
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			var bucket = BMSTestHelper.CreateBucket(BMSTestHelper.CreateSystem(Factory));
			var section = BMSTestHelper.CreateBoardSection(bucket);
			var band = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(bucket, 0, 0, 0, 0, 0, 0, name: ZGuid.NewZGuid().ToString());
			var sectionBand = section.SectionConfiguration.AcceptabilityBands.AddNew();
			sectionBand.AcceptabilityBandPK = band.PK;
			Factory.Save();

			var sectionViewModel = BMSTestHelper.CreateViewModel(section);
			var results = sectionViewModel.GetAcceptabilityBandResults(Factory);

			return new AcceptabilityBandTileViewModel(sectionBand, sectionViewModel, results.Single(x => x.SectionBand.AcceptabilityBandPK == band.PK).Result);
		}
	}
}
