using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(TextBoxRegistryItemEditor))]
	sealed class TextBoxRegistryItemEditorZTextBoxTestCase : TextBoxRegistryItemEditorTestCase
	{
		public void TestCharacterCasing()
		{
			using (var control = (ZUserControl)Editor.NewWinFormsEditorPane())
			{
				var box = (ZTextBox)control.Controls[0];
				AssertEquals("CharacterCasing", CharacterCasing.Normal, box.CharacterCasing);
			}
		}

		public void TestNewWinFormsEditorPaneCore_PasswordProperties()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			TextRegistryEditorInfo info = new TextRegistryEditorInfo(TextEditorType.Password);
			TextBoxRegistryItemEditor newEditor = new TextBoxRegistryItemEditor(new StringRegistryDataType(), info);

			using (var control = (ZUserControl)newEditor.NewWinFormsEditorPane())
			{
				var box = (ZTextBox)control.Controls[0];
				AssertEquals("Password type should have normal character casing.", CharacterCasing.Normal, box.CharacterCasing);
				AssertEquals("Password type should have password char '*'.", '*', box.PasswordChar);
			}
		}

		public void TestNewWinFormsEditorPaneCore_HTMLBoxProperties()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			var info = new TextRegistryEditorInfo(TextEditorType.HTML);
			var newEditor = new TextBoxRegistryItemEditor(new StringRegistryDataType(), info);

			using (var control = newEditor.NewWinFormsEditorPane())
			{
				var box = (ZTextBox)control.Controls[0];
				CombineAssertions(() =>
				{
					AssertEquals("HTML markup control should allow multiline text", true, box.Multiline);
					AssertEquals("HTML markup control should show vertical scroll bar", ScrollBars.Vertical, box.ScrollBars);
					AssertEquals("HTML markup control should accept return character", true, box.AcceptsReturn);
				});
			}
		}

		public void TestNewWinFormsEditorPaneCore_PasswordProperties_IsController()
		{
			GlbStaff.CurrentUser.GS_IsController = true;
			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			Factory.Save();

			var item = new StringRegistryItem("TestRegistryItem", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsPasswordVisibleForControllerUser);
			var info = new TextRegistryEditorInfo(TextEditorType.Password);
			var newEditor = new TextBoxRegistryItemEditor(item, new StringRegistryDataType(), info);

			using (var control = (ZUserControl)newEditor.NewWinFormsEditorPane())
			{
				AssertEquals("Control name should be", "PasswordControl", control.Name);
				var box = (ZTextBox)control.Controls[0];
				AssertEquals("Password type should have normal character casing.", CharacterCasing.Normal, box.CharacterCasing);
				AssertEquals("Password type should have password char '*'.", '*', box.PasswordChar);
				var button = (ZButton)control.Controls[1];
				AssertEquals("Button should be visible", true, button.Visible);
			}
		}

		public void TestNewWinFormsEditorPaneCore_MemoProperties()
		{
			TextRegistryEditorInfo info = new TextRegistryEditorInfo(TextEditorType.Memo);
			TextBoxRegistryItemEditor newEditor = new TextBoxRegistryItemEditor(new StringRegistryDataType(CharacterCase.Upper), info);

			using (var control = (ZUserControl)newEditor.NewWinFormsEditorPane())
			{
				var box = (ZTextBox)control.Controls[0];
				AssertEquals("Memo type should have normal character casing.", CharacterCasing.Upper, box.CharacterCasing);
			}
		}

		public void TestNewWinFormsEditorPaneCore_IntRegistryDataType()
		{
			TextRegistryEditorInfo info = new TextRegistryEditorInfo(TextEditorType.Memo);
			TextBoxRegistryItemEditor newEditor = new TextBoxRegistryItemEditor(new IntRegistryDataType(), info);

			using (var control = (ZUserControl)newEditor.NewWinFormsEditorPane())
			{
				var box = (ZTextBox)control.Controls[0];
				AssertEquals("Should return normal without exception", CharacterCasing.Normal, box.CharacterCasing);
			}
		}

		public override void TestEditorPaneLayout()
		{
			TextRegistryEditorInfo info = new TextRegistryEditorInfo(TextEditorType.TextBox);
			TextBoxRegistryItemEditor newEditor = new
				TextBoxRegistryItemEditor(new StringRegistryDataType(), info);

			using (var control = (ZUserControl)newEditor.NewWinFormsEditorPane())
			{
				var box = (ZTextBox)control.Controls[0];
				int originalHeight = box.Height;
				Assert("Precondition: Box.Width should not be 300", box.Width != 300);

				newEditor.SetEditorPaneLayout(box, 300, 200);
				AssertEquals("Box.Width", 300, box.Width);
				AssertEquals("Box.Height", originalHeight, box.Height);
				AssertEquals("Box.Anchor", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, box.Anchor);
			}

			info = new TextRegistryEditorInfo(TextEditorType.Memo);
			newEditor = new TextBoxRegistryItemEditor(new StringRegistryDataType(), info);

			using (var control = (ZUserControl)newEditor.NewWinFormsEditorPane())
			{
				var box = (ZTextBox)control.Controls[0];
				Assert("Precondition: Box.Width should not be 300", box.Width != 300);
				Assert("Precondition: Box.Height should not be 200", box.Height != 200);

				newEditor.SetEditorPaneLayout(box, 300, 200);
				AssertEquals("Box.Width", 300, box.Width);
				AssertEquals("Box.Height", 200, box.Height);
				AssertEquals("Box.Anchor", AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, box.Anchor);
			}
		}

		#region Implementation

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !editorPane.GetReadOnly();
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ZUserControl);
		}

		#endregion
	}
}
