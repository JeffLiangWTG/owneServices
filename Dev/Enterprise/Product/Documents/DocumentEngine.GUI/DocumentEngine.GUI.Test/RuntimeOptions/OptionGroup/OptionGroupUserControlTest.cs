using System.Windows.Forms;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(OptionGroupUserControl))]
	sealed class OptionGroupUserControlTest : RuntimeOptionUserControlBaseTest<OptionGroupUserControl>
	{
		const int PixelsPerLineInCheckedListBox = 15;
		public void TestCheckedListBoxHeight()
		{
			using (var optionGroupUserControl = new OptionGroupUserControl())
			{
				var optionGroup = new OptionGroup(null)
				{
					DisplayName = "1"
				};
				optionGroup.AddOption("1", "1");
				optionGroup.AddOption("2", "2");
				optionGroup.AddOption("3", "3");

				optionGroupUserControl.SetFilter(optionGroup);
				Assert("should be 3 lines", optionGroupUserControl.Height > 3 * PixelsPerLineInCheckedListBox);
			}

			using (var optionGroupUserControl = new OptionGroupUserControl())
			{
				var optionGroup = new OptionGroup(null)
				{
					DisplayName = "more than 1 line, more than 1 line, more than 1 line, more than 1 line"
				};
				optionGroup.AddOption("1", "1");

				optionGroupUserControl.SetFilter(optionGroup);
				Assert("should be more than 1 line", optionGroupUserControl.Height > 2 * PixelsPerLineInCheckedListBox);
			}
		}

		public void TestOptionGroupIsRadioButton()
		{
			using (var form = new ZForm())
			using (var optionGroupUserControl = new OptionGroupUserControl())
			{
				form.Controls.Add(optionGroupUserControl);
				form.Show();
				Application.DoEvents();

				var optionGroup = new OptionGroup(Factory)
				{
					DisplayName = "1",
					IsRadioButton = true
				};
				optionGroup.AddOption("1", "1");
				optionGroup.AddOption("2", "2");
				optionGroup.AddOption("3", "3");

				optionGroupUserControl.SetFilter(optionGroup);
				var checkedListBox = GUITestHelper.FindControl<ZCheckedListBox>(optionGroupUserControl.Controls, "CheckedListBox");
				AssertEquals(false, checkedListBox.GetItemChecked(0));
				AssertEquals(false, checkedListBox.GetItemChecked(1));

				optionGroup.DescriptionCodePairList[0].Value = true;
				AssertEquals(true, checkedListBox.GetItemChecked(0));
				AssertEquals(false, checkedListBox.GetItemChecked(1));

				optionGroup.DescriptionCodePairList[1].Value = true;
				AssertEquals(false, checkedListBox.GetItemChecked(0));
				AssertEquals(true, checkedListBox.GetItemChecked(1));
			}
		}
	}
}
