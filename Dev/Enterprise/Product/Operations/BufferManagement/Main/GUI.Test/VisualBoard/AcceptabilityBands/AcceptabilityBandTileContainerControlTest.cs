using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	class AcceptabilityBandTileContainerControlTest : BMSTestCaseWithFactory
	{
		public void TestPopulateTiles()
		{
			var viewModel = GetViewModel();
			var results = viewModel.GetAcceptabilityBandResults(Factory);

			using (var control = new AcceptabilityBandTileContainerControl(viewModel))
			{
				control.PopulateTiles(results);
				var tiles = control.FindAll<AcceptabilityBandTileControl>().OrderBy(c => c.Left).ToArray();
				AssertEquals(2, tiles.Length);

				AssertEquals("Should order according to display sequence", band2.PK, tiles[0].ViewModel.AcceptabilityBandPK);
				AssertEquals("Should order according to display sequence", band1.PK, tiles[1].ViewModel.AcceptabilityBandPK);

				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(0, 3), tiles[0].Location);
				AssertEquals(new Point(tiles[0].Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), ControlDpiScalingHelper.ScaleToCurrentDpiY(3)), tiles[1].Location);
				AssertGreaterThan(tiles[0].Width, tiles[0].Controls[0].Width);
			}
		}

		public void TestPopulateTiles_ShouldPopulateTiles_ExceptInactiveBands()
		{
			var inactiveBand = CreateAcceptabilityBand_WorkflowsInComponent(section.Component, 0, 0, 0, 0, 0, 0);
			inactiveBand.BAB_Name = "Banned";
			inactiveBand.BAB_IsActive = false;

			BMSTestHelper.AddAcceptabilityBandToSection(section, inactiveBand);

			Factory.Save();

			var viewModel = GetViewModel();
			var results = viewModel.GetAcceptabilityBandResults(Factory);

			using (var control = new AcceptabilityBandTileContainerControl(viewModel))
			{
				control.PopulateTiles(results);
				var tiles = control.FindAll<AcceptabilityBandTileControl>().OrderBy(c => c.Left).ToArray();
				AssertEquals(2, tiles.Length);

				var tile = tiles.SingleOrDefault(t => t.ViewModel.AcceptabilityBandPK == inactiveBand.PK);

				AssertNull(tile);
			}
		}

		public void TestPopulateTiles_ShouldPopulateTiles_WithBandsFromMultipleComponents()
		{
			var anotherBucket = CreateBucket(section.Component.System, "bucket2");
			section.SectionConfiguration.AdditionalComponents.AddNew().BSA_FC_Component = anotherBucket.PK;

			var anotherBand = CreateAcceptabilityBand(anotherBucket, 0, 0, 0, 0, 0, 0, "Radiohead", sql: "SELECT 1 Value, NEWID() Component, NEWID() ReleaseGroup");
			var anotherSectionBand = section.SectionConfiguration.AcceptabilityBands.AddNew();
			anotherSectionBand.AcceptabilityBandPK = anotherBand.PK;
			anotherSectionBand.DisplayName = "Of all the bands, by far the greatest.";
			anotherSectionBand.DisplaySequence = -1;

			Factory.Save();

			var viewModel = GetViewModel();
			var results = viewModel.GetAcceptabilityBandResults(Factory);

			using (var control = new AcceptabilityBandTileContainerControl(viewModel))
			{
				control.PopulateTiles(results);
				var tiles = control.FindAll<AcceptabilityBandTileControl>().OrderBy(c => c.Left).ToArray();
				AssertEquals(3, tiles.Length);

				AssertEquals("Should order according to display sequence", anotherBand.PK, tiles[0].ViewModel.AcceptabilityBandPK);
				AssertEquals("Should order according to display sequence", band2.PK, tiles[1].ViewModel.AcceptabilityBandPK);
				AssertEquals("Should order according to display sequence", band1.PK, tiles[2].ViewModel.AcceptabilityBandPK);

				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(0, 3), tiles[0].Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(tiles[0].Width + 5, 3), tiles[1].Location);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(tiles[1].Left + tiles[1].Width + 5, 3), tiles[2].Location);
			}
		}

		#region Implementation

		BMBoardSection section;
		BMComponentAcceptabilityBand band1, band2;
		DisposableList disposables;

		BMBoardSectionViewModel GetViewModel()
		{
			return BMSTestHelper.CreateViewModel(section);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.DisableAcceptabilityBandResultCache();

			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			section = CreateBoardSection(bucket);
			band1 = CreateAcceptabilityBand_WorkflowsInComponent(bucket, 0, 0, 0, 0, 0, 0);
			band2 = CreateAcceptabilityBand_AverageNumberOfTasksPerWorkflow(bucket, 0, 0, 0, 0, 0, 0);

			section.SectionConfiguration.AcceptabilityBands.AddNew().AcceptabilityBandPK = band2.PK;
			section.SectionConfiguration.AcceptabilityBands.AddNew().AcceptabilityBandPK = band1.PK;

			CreateWorkflows(bucket, 10, 1);

			Factory.Save();

			disposables = new DisposableList(new[] { DisableAsyncBehaviour() });
		}

		protected override void TearDown()
		{
			base.TearDown();

			disposables.Dispose();
		}

		#endregion
	}
}
