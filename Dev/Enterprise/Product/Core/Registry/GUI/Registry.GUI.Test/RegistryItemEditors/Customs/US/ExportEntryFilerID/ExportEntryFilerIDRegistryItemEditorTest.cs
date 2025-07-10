using System;
using Enterprise.Integration;
using Enterprise.Registry.Business.Customs.US;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ExportEntryFilerIDRegistryItemEditor))]
	sealed class ExportEntryFilerIDRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		new class RegistryFormForTest : RegistryItemEditorTestCase.RegistryFormForTest
		{
			public RegistryFormForTest(IRegistryItem registryItem)
				: base(registryItem)
			{
			}

			public override void DisplayRegistryItem()
			{
				RegistriesTreeView.SelectedNode = RegistriesTreeView.Nodes[0];
				FallbackTreeView.SelectedNode = FallbackTreeView.Nodes[0].Nodes[0];
			}
		}

		protected override RegistryItemEditorTestCase.RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem)
		{
			return new RegistryFormForTest(registryItem);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ExportEntryFilerIDRegistryItem("", null, null, null);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new ExportEntryFilerIDRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ExportEntryFilerIDControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var filer = new ExportEntryFilerID();

			filer.EntryFilerID = "12-1234567";
			filer.EntryFilerIDType = "E";
			return new object[] { filer };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((ExportEntryFilerIDControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
