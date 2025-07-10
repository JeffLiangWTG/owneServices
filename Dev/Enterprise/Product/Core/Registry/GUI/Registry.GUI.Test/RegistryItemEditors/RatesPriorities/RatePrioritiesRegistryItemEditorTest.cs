using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(RatePrioritiesRegistryItemEditor))]
	sealed class RatePrioritiesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new RatesPrioritiesRegistryItem("", null, null, null, RegistryStorageFlags.System, new RatesPrioritiesCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new RatePrioritiesRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(RatesPrioritiesControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new RatesPrioritiesCollection() };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((RatesPrioritiesControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
