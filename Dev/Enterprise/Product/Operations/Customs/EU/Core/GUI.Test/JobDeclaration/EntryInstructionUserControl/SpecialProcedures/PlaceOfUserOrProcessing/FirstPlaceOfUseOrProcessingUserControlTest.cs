using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class FirstPlaceOfUseOrProcessingUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
		}

		public void TesFirstPlaceOfUseOrProcessingGroupBox()
		{
			var groupBox = control.FirstPlaceOfUseOrProcessingGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), groupBox.Location);
				AssertEquals("Caption", "First Place of Use or Processing", groupBox.CaptionResourceString.Caption);
				AssertEquals("FullDescription", "[Annex A 4/5] Dates, Times, Periods and Places > First Place of Use or Processing", groupBox.CaptionResourceString.FullDescription);
			});
		}

		public void TestFirstPlaceOfUseOrProcessingControl()
		{
			var firstPlaceOfUseOrProcessingUserControl = control.FirstPlaceOfUseOrProcessingControl;
			CombineAssertions(() =>
			{
				AssertEquals("Visible", true, firstPlaceOfUseOrProcessingUserControl.Visible);
				AssertType<PlaceOfUseOrProcessingControl>("Type", firstPlaceOfUseOrProcessingUserControl);
				AssertEquals("BindTo", typeof(IFirstPlaceOfUseOrProcessingProvider), firstPlaceOfUseOrProcessingUserControl.BindingSource.DataSourceType);
			});
		}

		FirstPlaceOfUseOrProcessingUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new FirstPlaceOfUseOrProcessingUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
