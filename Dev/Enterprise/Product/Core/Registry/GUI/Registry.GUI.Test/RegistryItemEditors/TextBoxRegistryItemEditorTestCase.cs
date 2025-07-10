using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(TextBoxRegistryItemEditor))]
	abstract class TextBoxRegistryItemEditorTestCase : RegistryItemEditorTestCase
	{
		public void TestAWBCustomTextControl()
		{
			TextRegistryEditorInfo info = new TextRegistryEditorInfo(TextEditorType.AWBCustomisableMultilineText);
			TextBoxRegistryItemEditor newEditor = new TextBoxRegistryItemEditor(new StringRegistryDataType(), info);

			using (var ctrl = (ZUserControl)newEditor.NewWinFormsEditorPane())
			{
				Assert("Precondition: Box.Width should not be 300", ctrl.Width != 300);
				Assert("Precondition: Box.Height should not be 200", ctrl.Height != 200);

				newEditor.SetEditorPaneLayout(ctrl, 300, 200);
				AssertEquals("ctrl.Width", 300, ctrl.Width);
				AssertEquals("ctrl.Height", 200, ctrl.Height);
				AssertEquals("ctrl.Anchor", AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, ctrl.Anchor);
			}
		}

		public void TestMaxLength()
		{
			using (Control editorPane = Editor.NewWinFormsEditorPane())
			{
				PropertyDescriptor maxLengthProperty = BindableComponentMetaDataPropertyLocator.GetDefaultMetaDataProperty(editorPane.Controls[0].GetType(), MetaDataTypes.MaxLength);
				AssertEquals("MaxLength", 2, maxLengthProperty.GetValue(editorPane.Controls[0]));
			}
		}

		public void TestSetValueFromEditorPaneUsesGetUnresolvedStringForResString()
		{
			using (Control editorPane = Editor.NewWinFormsEditorPane())
			{
				var resString = ResString.GetMultilingualString("TEST_ONLY", "Test Only");
				using (var mockResStringData = Res.UseMockData())
				{
					mockResStringData.SetResourceGetter((s) => new ResourceStringData("TEST_ONLY", "Test Test"));
					Editor.SetValueFromEditorPane(editorPane, resString);
					AssertEquals("UnresolvedString", resString.GetUnresolvedString(), Editor.GetValueFromEditorPane(editorPane));
					AssertNotEquals("ToString", resString.ToString(), editorPane.Text);
				}
			}
		}

		public void TestGetValueFromEditorPaneWhenDataTypeIsReadOnly()
		{
			var info = new TextRegistryEditorInfo(TextEditorType.TextBox);
			var newEditor = new TextBoxRegistryItemEditor(new StringRegistryDataType(true, "DefaultOverrideValue"), info);
			using (var editorPane = newEditor.NewWinFormsEditorPane())
			{
				var overrideValue = newEditor.GetValueFromEditorPane(editorPane);
				AssertEquals("DefaultOverrideValue", overrideValue);
			}
		}

		public void TestSetValueFromEditorPaneUseOverrideValueFromDataTypeWhichIsReadOnly()
		{
			var info = new TextRegistryEditorInfo(TextEditorType.TextBox);
			var newEditor = new TextBoxRegistryItemEditor(new StringRegistryDataType(true, "DefaultOverrideValue"), info);
			using (var form = new ZForm())
			using (Control editorPane = newEditor.NewWinFormsEditorPane())
			{
				form.Controls.Add(editorPane);
				newEditor.EnableEditorPane(editorPane, true);
				newEditor.SetValueFromEditorPane(editorPane, "currentFallbackValue");

				form.Show();
				var textBox = (ZTextBox)editorPane.Controls[0];
				AssertEquals("DefaultOverrideValue", textBox.Text);

				newEditor.EnableEditorPane(editorPane, false);
				newEditor.SetValueFromEditorPane(editorPane, "currentFallbackValue");
				AssertEquals("currentFallbackValue", textBox.Text);
			}
		}

		public void TestControlIsReadOnlyWhenDataTypeIsReadOnly()
		{
			var info = new TextRegistryEditorInfo(TextEditorType.TextBox);
			var newEditor = new TextBoxRegistryItemEditor(new StringRegistryDataType(true, "DefaultOverrideValue"), info);
			using (var form = new ZForm())
			using (var editorPane = (ZUserControl)newEditor.NewWinFormsEditorPane())
			{
				form.Controls.Add(editorPane);
				form.Show();

				newEditor.EnableEditorPane(editorPane, true);
				var box = (ZTextBox)editorPane.Controls[0];
				AssertEquals(true, box.ReadOnly);
			}
		}

		#region Implementation

		protected sealed override RegistryItemEditor GetEditor()
		{
			TextRegistryEditorInfo info = new TextRegistryEditorInfo(TextEditorTypeForEditor);

			return new TextBoxRegistryItemEditor(new StringRegistryDataType(1, 2), info);
		}

		protected sealed override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new StringRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected sealed override object[] GetValidRegistryValues()
		{
			return new string[]
			{
				"AllYourBase",
				"12345"
			};
		}

		protected virtual TextEditorType TextEditorTypeForEditor
		{
			get { return TextEditorType.TextBox; }
		}

		#endregion
	}
}
