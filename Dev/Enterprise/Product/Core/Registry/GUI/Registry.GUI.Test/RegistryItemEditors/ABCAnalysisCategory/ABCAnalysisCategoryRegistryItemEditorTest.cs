using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business.Warehouse;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ABCAnalysisCategoryRegistryItemEditor))]
	sealed class ABCAnalysisCategoryRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ABCAnalysisCategoryRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ABCAnalysisCategoryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ABCAnalysisCategoryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ABCAnalysisCategoryRegistryItem("", null, null, null, RegistryStorageFlags.System, ABCAnalysisCategoryCollection.Default);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { ABCAnalysisCategoryCollection.Default };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
