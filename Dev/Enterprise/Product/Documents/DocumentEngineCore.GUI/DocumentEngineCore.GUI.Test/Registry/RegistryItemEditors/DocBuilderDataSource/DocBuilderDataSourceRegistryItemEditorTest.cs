using System;
using System.Windows.Forms;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.GUI.Registry.Testing
{
	[TestedType(typeof(DocBuilderDataSourceRegistryItemEditor))]
	sealed class DocBuilderDataSourceRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new DocBuilderDataSourceRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DocBuilderDataSourceControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DocBuilderDataSourceControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DocBuilderDataSourceRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new DocBuilderDataSource());
		}

		protected override object[] GetValidRegistryValues()
		{
			var result = new DocBuilderDataSource();
			result.Freight = true;

			return new object[] { result };
		}

		#endregion
	}
}
