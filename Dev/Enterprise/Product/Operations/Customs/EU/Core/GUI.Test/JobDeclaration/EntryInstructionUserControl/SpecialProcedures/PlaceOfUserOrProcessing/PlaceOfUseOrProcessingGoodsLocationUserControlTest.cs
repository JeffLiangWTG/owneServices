using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class PlaceOfUseOrProcessingGoodsLocationUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
		}

		public void TesPlacesOfUseOrProcessingGridGroupBox()
		{
			var groupBox = control.PlacesOfUseOrProcessingGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), groupBox.Location);
				AssertEquals("Caption", "Places of Use or Processing", groupBox.CaptionResourceString.Caption);
			});
		}

		public void TestPlacesOfUseOrProcessingGrid()
		{
			var grid = control.PlacesOfUseOrProcessingGrid;
			CombineAssertions(() =>
			{
				AssertType<ZGrid>("Type", grid);
				AssertEquals("BindTo", "CustomsEntryInstructions.PlaceOfUseOrProcessingCollection", grid.BindTo);
				AssertEquals("Columns", 1, grid.ColumnStyles.Count);
				AssertEquals("DisplayText", 100, grid.GetColumnStyle("DisplayText").Width);
				var columnStyleInfo = grid.ColumnStyles[0];
				AssertType<PlaceOfUseOrProcessingColumnStyleInfo>("PlaceOfUseOrProcessingColumnStyle", columnStyleInfo);
				AssertEquals("Caption", "Goods Location", ((PlaceOfUseOrProcessingColumnStyleInfo)columnStyleInfo).CaptionResourceString.Caption);
				AssertEquals("FullDescription", "[Annex A 4/9] Dates, Times, Periods and Places > Place(s) of Use or Processing", ((PlaceOfUseOrProcessingColumnStyleInfo)columnStyleInfo).CaptionResourceString.FullDescription);
			});
		}

		PlaceOfUseOrProcessingGoodsLocationUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new PlaceOfUseOrProcessingGoodsLocationUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
