using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Test.RegistryItemEditors.WebServices
{
	[TestedType(typeof(WebServicesRegistryGridItemEditor))]
	sealed class WebServicesRegistryGridItemEditorTest : RegistryItemEditorTestCase
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

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new WebServicesRegistryGridItemEditor(new WebServicesConfigRegistryDataType(new WebServicesConfigCollection()), null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((WebServicesRegistryGridControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(WebServicesRegistryGridControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new WebServicesConfigRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.Company, RegistryOptions.Default, new WebServicesConfigCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new WebServicesConfigCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		#endregion
	}
}
