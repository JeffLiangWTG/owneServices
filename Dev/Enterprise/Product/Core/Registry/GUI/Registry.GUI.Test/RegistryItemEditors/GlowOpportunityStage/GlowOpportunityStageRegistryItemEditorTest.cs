using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(GlowOpportunityStageRegistryItemEditor))]
	sealed class GlowOpportunityStageRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new GlowOpportunityStageRegistryItemEditor(new Enterprise.Registry.Business.GlowOpportunityStageRegistryDataType(new GlowOpportunityStageCollection()), null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((GlowOpportunityStageRegistryControl)editorPane).ReadOnly;
		}

		protected override System.Type GetExpectedEditorPaneType()
		{
			return typeof(GlowOpportunityStageRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new GlowOpportunityStageRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, new GlowOpportunityStageCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new GlowOpportunityStageCollection();
			collection.Add("AAA", (NoResString)"AAA Description", true, 50);
			collection.Add("ZZZ", (NoResString)"ZZZ Description", false);
			return new[] { collection };
		}
	}
}
