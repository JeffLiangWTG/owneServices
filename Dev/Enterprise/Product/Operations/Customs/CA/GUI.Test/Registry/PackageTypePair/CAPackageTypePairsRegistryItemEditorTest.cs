using System;
using Enterprise.Customs.CA.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(CAPackageTypePairsRegistryItemEditor))]
	sealed class CAPackageTypePairsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem) => new CARegistryFormForTest(registryItem);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new CAPackageTypePairsRegistryItem("", null, null, null, RegistryStorageFlags.Company, new CAPackageTypePairCollection());

		protected override RegistryItemEditor GetEditor() => new CAPackageTypePairsRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override Type GetExpectedEditorPaneType() => typeof(PackageTypePairsControl);

		protected override object[] GetValidRegistryValues() => new object[] { new CAPackageTypePairCollection() };

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane) => !((PackageTypePairsControl)editorPane).ReadOnly;

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		sealed class CARegistryFormForTest : RegistryFormForTest
		{
			public CARegistryFormForTest(IRegistryItem registryItem)
				: base(registryItem)
			{
			}

			public override void DisplayRegistryItem()
			{
				RegistriesTreeView.SelectedNode = RegistriesTreeView.Nodes[0];
				FallbackTreeView.SelectedNode = FallbackTreeView.Nodes[0].Nodes[0];
			}
		}
	}
}
