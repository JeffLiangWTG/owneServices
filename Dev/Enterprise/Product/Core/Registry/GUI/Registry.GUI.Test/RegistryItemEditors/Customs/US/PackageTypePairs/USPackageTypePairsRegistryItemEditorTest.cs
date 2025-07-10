using System;
using Enterprise.Integration;
using Enterprise.Registry.Business.Customs.US;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(USPackageTypePairsRegistryItemEditor))]
	sealed class USPackageTypePairsRegistryItemEditorTest : RegistryItemEditorTestCase
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
			return new USPackageTypePairsRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.Company, new USPackageTypePairCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new USPackageTypePairsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(PackageTypePairsControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new USPackageTypePairCollection() };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((PackageTypePairsControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
