using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CommunicationStatusRegistryItemEditor))]
	sealed class CommunicationStatusRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new CommunicationStatusRegistryItemEditor(new CommunicationStatusRegistryDataType(new CommunicationStatusCollection()), new CommunicationStatusRegistryEditorInfo((NoResString)"Enabled"), null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CommunicationStatusRegistryControl)editorPane).ReadOnly;
		}

		protected override System.Type GetExpectedEditorPaneType()
		{
			return typeof(CommunicationStatusRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CommunicationStatusRegistryItem("TEST_REGISTRY_ITEM", null, null, null, RegistryStorageFlags.System, new CommunicationStatusRegistryEditorInfo((NoResString)"Enabled"), new CommunicationStatusCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new CommunicationStatusCollection();
			collection.Add("AAA", (NoResString)"DESC A", true, false);
			collection.Add("BBB", (NoResString)"DESC B", false, false);
			collection.Add("CCC", (NoResString)"DESC C", true, true);
			return new[] { collection };
		}
	}
}
