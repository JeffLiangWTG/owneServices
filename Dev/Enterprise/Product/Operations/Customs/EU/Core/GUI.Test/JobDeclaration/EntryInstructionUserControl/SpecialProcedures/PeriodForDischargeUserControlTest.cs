using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class PeriodForDischargeUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
		}

		public void TestPeriodForDischargeGroupBox()
		{
			var groupBox = control.PeriodForDischargeGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), groupBox.Location);
				AssertEquals("Caption", "Period for Discharge", groupBox.CaptionResourceString.Caption);
			});
		}

		public void TestAutomaticExtensionCheckBox()
		{
			var checkBox = control.AutomaticExtensionCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", checkBox);
				AssertEquals("BindTo", "CustomsEntryInstructions.ZG_PeriodForDischargeAutoExtension", checkBox.BindTo);
				AssertCollectionContains("Within PeriodForDischargeGroupBox", checkBox, control.PeriodForDischargeGroupBox.Controls);
			});
		}

		public void TestPeriodOfDischargePeriodCalcEdit()
		{
			var calcEdit = control.PeriodForDischargePeriodCalcEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcEdit>("Type", calcEdit);
				AssertEquals("BindTo", "CustomsEntryInstructions.ZG_PeriodForDischarge", calcEdit.BindTo);
				AssertCollectionContains("Within PeriodForDischargeGroupBox", calcEdit, control.PeriodForDischargeGroupBox.Controls);
			});
		}

		public void TestPeriodOfDischargeDetailsTextBox()
		{
			var textBox = control.PeriodForDischargeDetailsTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", textBox);
				AssertEquals("BindTo", "CustomsEntryInstructions.PeriodForDischargeDetails", textBox.BindTo);
				AssertCollectionContains("Within PeriodForDischargeGroupBox", textBox, control.PeriodForDischargeGroupBox.Controls);
			});
		}

		PeriodForDischargeUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new PeriodForDischargeUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
