using CargoWise.EntityFramework.Testing;
using Enterprise.ResourceStrings.Business;

namespace Enterprise.ResourceStrings.GUI.Testing
{
	sealed class TranslationSearchControlTestCase : TestCaseWithFactory
	{
		public void TestCheckboxes()
		{
			using (var form = new TranslationSearchForm(new TranslationSearchCriteria() { SourceText = "s", TargetText = "t" }))
			{
				form.Show();

				AssertEquals(false, form.translationSearchControl.searchSourceCheckbox.Checked);
				AssertEquals(false, form.translationSearchControl.sourceTextBox.Enabled);
				AssertEquals(true, form.translationSearchControl.searchTargetCheckbox.Checked);
				AssertEquals(true, form.translationSearchControl.targetTextBox.Enabled);
				AssertEquals(true, form.translationSearchControl.replaceCheckbox.Enabled);
				AssertEquals(false, form.translationSearchControl.replaceCheckbox.Checked);
				AssertEquals(false, form.translationSearchControl.replaceTextBox.Enabled);

				form.translationSearchControl.searchSourceCheckbox.Checked = true;
				AssertEquals(true, form.translationSearchControl.sourceTextBox.Enabled);
				AssertEquals(true, form.translationSearchControl.targetTextBox.Enabled);
				AssertEquals(true, form.translationSearchControl.replaceCheckbox.Enabled);
				AssertEquals(false, form.translationSearchControl.replaceTextBox.Enabled);

				form.translationSearchControl.replaceCheckbox.Checked = true;
				AssertEquals(true, form.translationSearchControl.sourceTextBox.Enabled);
				AssertEquals(true, form.translationSearchControl.targetTextBox.Enabled);
				AssertEquals(true, form.translationSearchControl.replaceCheckbox.Enabled);
				AssertEquals(true, form.translationSearchControl.replaceCheckbox.Checked);
				AssertEquals(true, form.translationSearchControl.replaceTextBox.Enabled);

				form.translationSearchControl.searchTargetCheckbox.Checked = false;
				AssertEquals(true, form.translationSearchControl.sourceTextBox.Enabled);
				AssertEquals(false, form.translationSearchControl.targetTextBox.Enabled);
				AssertEquals(false, form.translationSearchControl.replaceCheckbox.Enabled);
				AssertEquals(false, form.translationSearchControl.replaceCheckbox.Checked);
				AssertEquals(false, form.translationSearchControl.replaceTextBox.Enabled);

				form.translationSearchControl.searchSourceCheckbox.Checked = false;
				AssertEquals(false, form.translationSearchControl.sourceTextBox.Enabled);
				AssertEquals(false, form.translationSearchControl.targetTextBox.Enabled);
				AssertEquals(false, form.translationSearchControl.replaceCheckbox.Enabled);
				AssertEquals(false, form.translationSearchControl.replaceCheckbox.Checked);
				AssertEquals(false, form.translationSearchControl.replaceTextBox.Enabled);

				form.translationSearchControl.searchTargetCheckbox.Checked = true;
				AssertEquals(false, form.translationSearchControl.sourceTextBox.Enabled);
				AssertEquals(true, form.translationSearchControl.targetTextBox.Enabled);
				AssertEquals(true, form.translationSearchControl.replaceCheckbox.Enabled);
				AssertEquals(false, form.translationSearchControl.replaceCheckbox.Checked);
				AssertEquals(false, form.translationSearchControl.replaceTextBox.Enabled);
			}
		}
	}
}
