using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class RadioButtonControlTest : NUnit.Framework.TestCase
	{
		internal class RadioButtonControlTestClass : RadioButtonControl
		{
			public RadioButtonControlTestClass()
			: base()
			{
			}

			public RadioButtonControlTestClass(string customCaption)
			: base(customCaption)
			{
			}

			public ZRadioButton YesRadioButton
			{
				get { return yesRadioButton; }
			}

			public ZRadioButton NoRadioButton
			{
				get { return noRadioButton; }
			}

			public KGroupBox OptionsGroupBox
			{
				get { return optionGroupBox; }
			}
		}

		public void TestConstructor()
		{
			using (var control = new RadioButtonControlTestClass())
			{
				AssertEquals("OptionGroupBox.Text", "Option", control.OptionsGroupBox.Text);
			}
			using (var control = new RadioButtonControlTestClass(null))
			{
				AssertEquals("OptionGroupBox.Text", "Option", control.OptionsGroupBox.Text);
			}
			using (var control = new RadioButtonControlTestClass("x"))
			{
				AssertEquals("OptionGroupBox.Text", "x", control.OptionsGroupBox.Text);
			}
		}

		public void TestValue()
		{
			using (var control = new RadioButtonControlTestClass())
			{
				AssertEquals("YesRadioButton.Checked", false, control.YesRadioButton.Checked);
				AssertEquals("NoRadioButton.Checked", false, control.NoRadioButton.Checked);

				control.Value = true;
				AssertEquals("YesRadioButton.Checked", true, control.YesRadioButton.Checked);
				AssertEquals("NoRadioButton.Checked", false, control.NoRadioButton.Checked);
				AssertEquals("Value", true, control.Value);

				control.Value = false;
				AssertEquals("YesRadioButton.Checked", false, control.YesRadioButton.Checked);
				AssertEquals("NoRadioButton.Checked", true, control.NoRadioButton.Checked);
				AssertEquals("Value", false, control.Value);
			}
		}

		public void TestCurrentDataItemIsBusinessObject()
		{
			using (var control = new RadioButtonControlTestClass())
			{
				Assert("The CurrentDataItem for the RadioButtonControl should be a BusinessObject", control.CurrentDataItem is BusinessObject);
			}
		}
	}
}
