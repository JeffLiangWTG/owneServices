using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class ActivitiesAndProceduresUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
		}

		public void TestActivitiesAndProceduresGroupBox()
		{
			var groupBox = control.ActivitiesAndProceduresGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), groupBox.Location);
				AssertEquals("Caption", "Activities and Procedures", groupBox.CaptionResourceString.Caption);
			});
		}

		public void TestPeriodOfDischargeDetailsTextBox()
		{
			var textBox = control.DetailsOfPlannedActivitiesTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", textBox);
				AssertEquals("BindTo", "CustomsEntryInstructions.DetailsOfPlannedActivities", textBox.BindTo);
				AssertCollectionContains("Within ActivitiesAndProceduresGroupBox", textBox, control.ActivitiesAndProceduresGroupBox.Controls);
			});
		}

		ActivitiesAndProceduresUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new ActivitiesAndProceduresUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
