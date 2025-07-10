using System;
using System.Windows.Forms;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.GUI.Registry.Testing
{
	[TestedType(typeof(EmailFormatRegistryItemEditor))]
	sealed class EmailFormatRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new EmailFormatRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((EmailFormatControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(EmailFormatControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new EmailFormatRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			EmailFormat emailFormat = new EmailFormat();
			emailFormat.Separator = "#";

			return new object[] { emailFormat };
		}

		#endregion
	}
}
