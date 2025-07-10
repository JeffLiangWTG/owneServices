using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ScimApiTokenAuthenticationRegistryItemEditor))]
	internal class ScimApiTokenAuthenticationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new ScimApiTokenAuthenticationRegistryItemEditor(RegistryItem.DataType);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ScimApiTokenAuthenticationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ScimApiTokenAuthenticationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var item = new StringRegistryItem("", null, null, null, RegistryStorageFlags.System, String.Empty);
			item.DataType = new ScimApiTokenAuthenticationDataType();

			return item;
		}

		public void TestCustomValidation()
		{
			using (var form = new ZForm())
			{
				var editorPane = (ScimApiTokenAuthenticationControl)Editor.NewWinFormsEditorPane();
				form.Controls.Add(editorPane);
				form.Show();

				Editor.SetValueFromEditorPane(editorPane, string.Empty);
				AssertEquals("ErrorMessage", "Cannot save an empty API Token. Please generate a new token before saving.", Editor.GetCustomValidation(editorPane));

				Editor.SetValueFromEditorPane(editorPane, Guid.NewGuid().ToString());
				AssertEquals("ErrorMessage", "", Editor.GetCustomValidation(editorPane));
			}
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { String.Empty };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
