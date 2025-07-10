using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(OpportunityClosedReasonsRegistryItemEditor))]
	sealed class OpportunityClosedReasonsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new OpportunityClosedReasonsRegistryItemEditor(new Enterprise.Registry.Business.OpportunityClosedReasonsRegistryDataType(new OpportunityClosedReasonsCollection()), null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((OpportunityClosedReasonsControl)editorPane).ReadOnly;
		}

		protected override System.Type GetExpectedEditorPaneType()
		{
			return typeof(OpportunityClosedReasonsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new OpportunityClosedReasonsRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, RegistryOptions.PreserveTestValue, new OpportunityClosedReasonsCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new OpportunityClosedReasonsCollection();
			collection.Add("AAA", (NoResString)"AAA Description");
			collection.Add("ZZZ", (NoResString)"ZZZ Description");
			return new[] { collection };
		}
	}
}
