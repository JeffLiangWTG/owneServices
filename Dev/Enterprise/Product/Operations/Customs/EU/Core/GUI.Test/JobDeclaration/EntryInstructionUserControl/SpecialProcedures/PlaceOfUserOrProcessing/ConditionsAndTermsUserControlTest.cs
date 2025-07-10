using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class ConditionsAndTermsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("CaptionRenderingEnabled", true, control.CaptionRenderingEnabled);
		}

		public void TestProcessingProcedureCodeDropEdit()
		{
			var dropEdit = control.ProcessingProcedureCodeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", dropEdit);
				AssertEquals("BindTo", "CustomsEntryInstructions.ZG_ProcessingProcedureCode", dropEdit.BindTo);
				AssertCollectionContains("Within ConditionsAndTermsGroupBox", dropEdit, control.ConditionsAndTermsGroupBox.Controls);
			});
		}

		public void TestProcessingProcedureDetailsTextBox()
		{
			var textBox = control.ProcessingProcedureDetailsTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", textBox);
				AssertEquals("BindTo", "CustomsEntryInstructions.ProcessingProcedureDetails", textBox.BindTo);
				AssertCollectionContains("Within ConditionsAndTermsGroupBox", textBox, control.ConditionsAndTermsGroupBox.Controls);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ConditionsAndTermsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ConditionsAndTermsUserControl control;
	}
}
