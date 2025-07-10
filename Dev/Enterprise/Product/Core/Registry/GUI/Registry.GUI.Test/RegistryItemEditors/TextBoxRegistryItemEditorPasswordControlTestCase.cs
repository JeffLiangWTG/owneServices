using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(TextBoxRegistryItemEditor))]
	sealed class TextBoxRegistryItemEditorPasswordControlTestCase : TextBoxRegistryItemEditorTestCase
	{
		#region Implementation

		protected override TextEditorType TextEditorTypeForEditor
		{
			get { return TextEditorType.Password; }
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !editorPane.GetReadOnly();
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(PasswordControl);
		}

		protected override Enterprise.Registry.GUI.RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeftRight; }
		}

		#endregion
	}
}
