using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class SpecialProceduresOthersUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
		}

		public void TestOthersGroupBox()
		{
			var groupBox = control.OthersGroupBox;
			CombineAssertions(() =>
			{
				AssertEquals("Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), groupBox.Location);
				AssertEquals("Caption", "Others", groupBox.CaptionResourceString.Caption);
			});
		}

		public void TestCalculateDutyDropEdit()
		{
			var dropEdit = control.CalculateDutyDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", dropEdit);
				AssertEquals("BindTo", "CustomsEntryInstructions.ZG_Article86_3_UCC", dropEdit.BindTo);
				AssertCollectionContains("Within OthersGroupBox", dropEdit, control.OthersGroupBox.Controls);
			});
		}

		public void TestAdditionalInformationTextBox()
		{
			var textBox = control.AdditionalInformationTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", textBox);
				AssertEquals("BindTo", "CustomsEntryInstructions.AdditionalInformation", textBox.BindTo);
				AssertCollectionContains("Within OthersGroupBox", textBox, control.OthersGroupBox.Controls);
			});
		}

		SpecialProceduresOthersUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new SpecialProceduresOthersUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
