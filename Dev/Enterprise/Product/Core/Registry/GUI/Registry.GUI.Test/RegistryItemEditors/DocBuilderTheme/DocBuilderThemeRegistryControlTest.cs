using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Registry.GUI.HotSpotPictureBox;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DocBuilderThemeRegistryControl))]
	sealed class DocBuilderThemeRegistryControlTest : RegistryZUserControlTestCase
	{
		public void TestRemoveButton()
		{
			var registry = RegistryControl.ThemeRegistry;
			int themeCount = registry.Themes.Count;
			AssertEquals("Pre-condition: Themes.Count", themeCount, registry.Themes.Count);

			var newThemeName = "New Theme To Remove";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddUserResponse(newThemeName);
			UnitTestUserNotification.Instance.AddOKAnswer();
			RegistryControl.NewButton.PerformClick();

			AssertEquals("Themes.Count", themeCount + 1, registry.Themes.Count);
			var actual = registry.FindTheme(newThemeName);
			AssertNotNull("New Theme should be added.", actual);
			AssertEquals("New Theme should be selected.", RegistryControl.ThemeComboBox.SelectedItem, actual);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			RegistryControl.RemoveButton.PerformClick();
			AssertEquals("Themes.Count", themeCount + 1, registry.Themes.Count);
			AssertNotNull("New Theme should not be removed when remove is cancelled.", actual);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddOKAnswer();
			RegistryControl.RemoveButton.PerformClick();
			AssertEquals("Themes.Count", themeCount, registry.Themes.Count);
			AssertNull("New Theme should be removed.", registry.FindTheme(newThemeName));
		}

		public void TestCopyButton()
		{
			var registry = RegistryControl.ThemeRegistry;
			int themeCount = registry.Themes.Count;
			AssertEquals("Pre-condition: Themes.Count", themeCount, registry.Themes.Count);

			RegistryControl.ThemeComboBox.SelectedIndex = 2;
			var expected = (DocBuilderTheme)RegistryControl.ThemeComboBox.SelectedItem;

			var copiedThemeName = "Copied Theme";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddUserResponse(copiedThemeName);
			UnitTestUserNotification.Instance.AddOKAnswer();
			RegistryControl.CopyButton.PerformClick();

			AssertEquals("Themes.Count", themeCount + 1, registry.Themes.Count);
			var actual = registry.FindTheme(copiedThemeName);
			AssertNotNull("Copied Theme should be added.", actual);
			AssertThemeItemEquals(expected.DocumentHeading, actual.DocumentHeading);
			AssertThemeItemEquals(expected.PageNumberHeading, actual.PageNumberHeading);
			AssertThemeItemEquals(expected.PrimaryHeading, actual.PrimaryHeading);
			AssertThemeItemEquals(expected.PrimaryBody, actual.PrimaryBody);
			AssertThemeItemEquals(expected.SecondaryHeading, actual.SecondaryHeading);
			AssertThemeItemEquals(expected.SecondaryBody, actual.SecondaryBody);

			AssertEquals("Copied Theme should be selected.", RegistryControl.ThemeComboBox.SelectedItem, actual);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			RegistryControl.CopyButton.PerformClick();

			AssertEquals("Themes.Count", themeCount + 1, registry.Themes.Count);
		}

		public void TestNewButton()
		{
			var registry = RegistryControl.ThemeRegistry;
			int themeCount = registry.Themes.Count;
			AssertEquals("Pre-condition: Themes.Count", themeCount, registry.Themes.Count);

			var newThemeName = "New Theme";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddUserResponse(newThemeName);
			UnitTestUserNotification.Instance.AddOKAnswer();
			RegistryControl.NewButton.PerformClick();

			AssertEquals("Themes.Count", themeCount + 1, registry.Themes.Count);
			var actual = registry.FindTheme(newThemeName);
			AssertNotNull("New Theme should be added.", actual);
			AssertEquals("New Theme", actual.Name);
			AssertEquals((NoResString)"New Theme", actual.NameMultilingual);
			AssertEquals("New Theme should be selected.", RegistryControl.ThemeComboBox.SelectedItem, actual);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			RegistryControl.NewButton.PerformClick();

			AssertEquals("Themes.Count", themeCount + 1, registry.Themes.Count);
		}

		public void TestPreviewHotSpotClick()
		{
			RegistryControl.PreviewPictureBox.PerformMouseClickForTesting(new MouseEventArgs(MouseButtons.Left, 1, 139, 15, 0));
			AssertEquals("ThemeItemComboBox.SelectedItem.Name", DocBuilderThemeItemList.Codes.DocumentHeading, ((DocBuilderThemeItem)RegistryControl.ThemeItemComboBox.SelectedItem).Name);

			RegistryControl.PreviewPictureBox.PerformMouseClickForTesting(new MouseEventArgs(MouseButtons.Left, 1, 317, 15, 0));
			AssertEquals("ThemeItemComboBox.SelectedItem.Name", DocBuilderThemeItemList.Codes.PageNumberHeading, ((DocBuilderThemeItem)RegistryControl.ThemeItemComboBox.SelectedItem).Name);

			RegistryControl.PreviewPictureBox.PerformMouseClickForTesting(new MouseEventArgs(MouseButtons.Left, 1, 186, 34, 0));
			AssertEquals("ThemeItemComboBox.SelectedItem.Name", DocBuilderThemeItemList.Codes.PrimaryHeading, ((DocBuilderThemeItem)RegistryControl.ThemeItemComboBox.SelectedItem).Name);

			RegistryControl.PreviewPictureBox.PerformMouseClickForTesting(new MouseEventArgs(MouseButtons.Left, 1, 314, 72, 0));
			AssertEquals("ThemeItemComboBox.SelectedItem.Name", DocBuilderThemeItemList.Codes.PrimaryBody, ((DocBuilderThemeItem)RegistryControl.ThemeItemComboBox.SelectedItem).Name);

			RegistryControl.PreviewPictureBox.PerformMouseClickForTesting(new MouseEventArgs(MouseButtons.Left, 1, 188, 104, 0));
			AssertEquals("ThemeItemComboBox.SelectedItem.Name", DocBuilderThemeItemList.Codes.SecondaryHeading, ((DocBuilderThemeItem)RegistryControl.ThemeItemComboBox.SelectedItem).Name);

			RegistryControl.PreviewPictureBox.PerformMouseClickForTesting(new MouseEventArgs(MouseButtons.Left, 1, 186, 141, 0));
			AssertEquals("ThemeItemComboBox.SelectedItem.Name", DocBuilderThemeItemList.Codes.SecondaryBody, ((DocBuilderThemeItem)RegistryControl.ThemeItemComboBox.SelectedItem).Name);
		}

		protected override RegistryZUserControl GetNewControl()
		{
			var control = new DocBuilderThemeRegistryControl();
			TypeDescriptor.AddAttributes(control.FillButton, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(control.FontColorButton, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(control.BorderButton, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(control.ItalicCheckBox, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(control.BoldCheckBox, new SuppressFormsLocalizedTestAttribute());
			return control;
		}

		DocBuilderThemeRegistryControl RegistryControl;

		protected override void SetUp()
		{
			base.SetUp();
			RegistryControl = new DocBuilderThemeRegistryControl();
			RegistryControl.ThemeRegistry = new DocBuilderThemeRegistry();
		}

		protected override void TearDown()
		{
			RegistryControl.Dispose();
			base.TearDown();
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new DocBuilderThemeRegistry();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return !((DocBuilderThemeRegistryControl)control).ThemeComboBox.Enabled;
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			if (control is PictureBoxWithHotSpots ||
				new[] { "FontSizeNumericUpDown", "ThemeItemComboBox", "ItalicCheckBox", "BoldCheckBox", "FontFamilyComboBox", "ThemeComboBox" }.Contains(control.Name))
			{
				return true;
			}
			return base.ShouldIgnoreMissingBindingMember(control);
		}

		void AssertThemeItemEquals(DocBuilderThemeItem expected, DocBuilderThemeItem actual)
		{
			AssertEquals("Name", expected.Name, actual.Name);
			AssertEquals("Color1Argb", expected.Color1Argb, actual.Color1Argb);
			AssertEquals("Color2Argb", expected.Color2Argb, actual.Color2Argb);
			AssertEquals("FontName", expected.FontName, actual.FontName);
			AssertEquals("FontSize", expected.FontSize, actual.FontSize);
			AssertEquals("FontColorArgb", expected.FontColorArgb, actual.FontColorArgb);
			AssertEquals("FontBold", expected.FontBold, actual.FontBold);
			AssertEquals("FontItalic", expected.FontItalic, actual.FontItalic);
		}
	}
}
