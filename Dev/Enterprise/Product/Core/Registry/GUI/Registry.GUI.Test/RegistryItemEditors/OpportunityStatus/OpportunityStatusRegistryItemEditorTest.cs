using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(OpportunityStatusRegistryItemEditor))]
	sealed class OpportunityStatusRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new OpportunityStatusRegistryItemEditor(new Enterprise.Registry.Business.OpportunityStatusRegistryDataType(new OpportunityStatusCollection()), null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((OpportunityStatusRegistryControl)editorPane).ReadOnly;
		}

		protected override System.Type GetExpectedEditorPaneType()
		{
			return typeof(OpportunityStatusRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new OpportunityStatusRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, new OpportunityStatusCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new OpportunityStatusCollection();
			collection.Add("AAA", (NoResString)"AAA Description", true);
			collection.Add("ZZZ", (NoResString)"ZZZ Description", false);
			return new[] { collection };
		}
	}
}
