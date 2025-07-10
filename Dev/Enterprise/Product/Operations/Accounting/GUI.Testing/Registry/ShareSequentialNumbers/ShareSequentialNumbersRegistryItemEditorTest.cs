using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ShareSequentialNumbersRegistryItemEditor))]
	public class ShareSequentialNumbersRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override RegistryItemEditor GetEditor()
		{
			return new ShareSequentialNumbersRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ShareSequentialNumbersControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ShareSequentialNumbersControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ShareSequentialReferenceNumbersRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			ShareSequentialReferenceNumbers item = new ShareSequentialReferenceNumbers();

			return new object[] { item };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
