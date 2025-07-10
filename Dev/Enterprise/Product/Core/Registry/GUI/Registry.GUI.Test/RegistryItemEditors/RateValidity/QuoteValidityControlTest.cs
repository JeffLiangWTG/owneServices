using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class QuoteValidityControlTest : TestCaseWithFactory
	{
		public void TestApplyCheckBox_WhenItIsTickedAfterUntickedBlankValueCheckBox_FieldValueShouldBeResetToZero()
		{
			using (var quoteValidityControl = new QuoteValidityControl())
			{
				quoteValidityControl.BlankValueCheckbox.Checked = true;
				AssertEquals("When Blank Value Checkbox is ticked, the field value should be 2,147,483,637.", QuoteValidityRegistryItem.BlankValue, quoteValidityControl.FieldValue);
				quoteValidityControl.BlankValueCheckbox.Checked = false;
				AssertEquals("When Blank Value Checkbox is unticked, the field value keep 2,147,483,637.",QuoteValidityRegistryItem.BlankValue, quoteValidityControl.FieldValue);
				quoteValidityControl.ApplyCheckBox.Checked = true;
				AssertEquals("When Apply Until End of the Month Checkbox is ticked, the field value should be reset to ZERO after unticking the BlankValue check box.", 0, quoteValidityControl.FieldValue);
			}
		}
	}
}
