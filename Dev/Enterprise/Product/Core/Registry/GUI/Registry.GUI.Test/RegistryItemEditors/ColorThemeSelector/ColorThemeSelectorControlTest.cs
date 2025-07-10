using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ColorThemeSelectorControl))]
	sealed class ColorThemeSelectorControlTest : RegistryZUserControlTestCase
	{
		public void TestDropEditChanging()
		{
			ColorThemeSelector selector = new ColorThemeSelector();
			using (ColorThemeSelectorControl selectorControl = new ColorThemeSelectorControl())
			{
				selectorControl.FieldValue = selector;
				AssertEquals(0, selectorControl.RowLayoutPanel.Controls.Count);

				selector.ChosenThemeName = DefinedColorThemes.ClassicColorTheme.Name;
				selectorControl.ThemeDropEdit_SelectedIndexChanged(null, EventArgs.Empty);
				Assert(selectorControl.RowLayoutPanel.Controls.Count > 0);

				selector.ChosenThemeName = "Crap";
				selectorControl.ThemeDropEdit_SelectedIndexChanged(null, EventArgs.Empty);
				AssertEquals(0, selectorControl.RowLayoutPanel.Controls.Count);
			}
		}

		public void TestButtonBackColor()
		{
			ColorThemeSelector selector = new ColorThemeSelector();
			using (ColorThemeSelectorControl selectorControl = new ColorThemeSelectorControl())
			{
				selectorControl.FieldValue = selector;

				selector.ChosenThemeName = DefinedColorThemes.BeachColorTheme.Name;
				selectorControl.ThemeDropEdit_SelectedIndexChanged(null, EventArgs.Empty);
				AssertNotEquals(Color.Empty.ToArgb(), ((Button)selectorControl.RowLayoutPanel.Controls[0]).BackColor.ToArgb());

				selector.ChosenThemeName = DefinedColorThemes.ClassicColorTheme.Name;
				selectorControl.ThemeDropEdit_SelectedIndexChanged(null, EventArgs.Empty);
				AssertEquals(SystemColors.ControlDark, ((Button)selectorControl.RowLayoutPanel.Controls[0]).BackColor);
			}
		}

		public void TestButtonClick()
		{
			ColorThemeSelector selector = new ColorThemeSelector();
			using (ColorThemeSelectorControl selectorControl = new ColorThemeSelectorControl())
			{
				selectorControl.FieldValue = selector;

				selector.ChosenThemeName = DefinedColorThemes.BeachColorTheme.Name;
				selectorControl.ThemeDropEdit_SelectedIndexChanged(null, EventArgs.Empty);
				((Button)selectorControl.RowLayoutPanel.Controls[0]).PerformClick();
				AssertEquals("The " + selector.ChosenThemeName + " theme is system defined and cannot be changed. Please pick a Custom theme if you wish to set colors yourself.", UnitTestUserNotification.Instance.LastMessage.Text);

				selector.ChosenThemeName = "Blah";
				((Button)selectorControl.RowLayoutPanel.Controls[0]).PerformClick();
				AssertEquals("Please choose a valid theme.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestButtonClick_Custom()
		{
			ColorThemeSelector selector = new ColorThemeSelector();
			using (ColorThemeSelectorControlForTest selectorControl = new ColorThemeSelectorControlForTest())
			{
				selectorControl.FieldValue = selector;

				selector.ChosenThemeName = "Custom 1";
				selectorControl.ThemeDropEdit_SelectedIndexChanged(null, EventArgs.Empty);
				((Button)selectorControl.RowLayoutPanel.Controls[0]).PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(Color.Blue, selectorControl.LastChosenColor);

				AssertEquals(Color.Blue, ((Button)selectorControl.RowLayoutPanel.Controls[0]).BackColor);
			}
		}

		public void TestColorSettingIsShownAfterFieldValueInitialized()
		{
			ColorThemeSelector selector = new ColorThemeSelector();
			using (ColorThemeSelectorControlForTest selectorControl = new ColorThemeSelectorControlForTest())
			{
				selector.ChosenThemeName = "Custom 1";
				selectorControl.FieldValue = selector;

				Assert(selectorControl.RowLayoutPanel.Controls.Count > 0);
			}
		}

		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new ColorThemeSelector();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return !((ColorThemeSelectorControl)control).ThemeDropEdit.Enabled;
		}

		class ColorThemeSelectorControlForTest : ColorThemeSelectorControl
		{
			protected override Color GetColorDialogResponse(Color initialColor)
			{
				LastChosenColor = Color.Blue;
				return LastChosenColor;
			}

			public Color LastChosenColor;
		}

		#endregion
	}
}
