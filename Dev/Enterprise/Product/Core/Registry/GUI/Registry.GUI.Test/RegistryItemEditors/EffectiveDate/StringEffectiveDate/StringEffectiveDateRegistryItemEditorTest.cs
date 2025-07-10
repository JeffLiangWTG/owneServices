using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(StringEffectiveDateRegistryItemEditor))]
	sealed class StringEffectiveDateRegistryItemEditorTest : RegistryItemEditorTestCase
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
				FallbackTreeView.SelectedNode = FallbackTreeView.Nodes[0];
			}
		}

		protected override RegistryItemEditorTestCase.RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem)
		{
			return new RegistryFormForTest(registryItem);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new StringEffectiveDateRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new StringEffectiveDateRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(StringEffectiveDateControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			StringEffectiveDate stringEffectiveDate = new StringEffectiveDate();
			stringEffectiveDate.PreviousValue = ZString.Empty;
			stringEffectiveDate.NewValue = ZString.Empty;
			stringEffectiveDate.EffectiveDate = ZDateTime.Today;
			return new object[] { stringEffectiveDate };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((StringEffectiveDateControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
