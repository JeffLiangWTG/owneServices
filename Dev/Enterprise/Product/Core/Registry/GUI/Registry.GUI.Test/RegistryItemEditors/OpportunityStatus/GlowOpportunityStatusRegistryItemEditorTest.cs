using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(GlowOpportunityStatusRegistryItemEditor))]
	sealed class GlowOpportunityStatusRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new GlowOpportunityStatusRegistryItemEditor(new Enterprise.Registry.Business.GlowOpportunityStatusRegistryDataType(new GlowOpportunityStatusCollection()), null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((GlowOpportunityStatusRegistryControl)editorPane).ReadOnly;
		}

		protected override System.Type GetExpectedEditorPaneType()
		{
			return typeof(GlowOpportunityStatusRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new GlowOpportunityStatusRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, RegistryOptions.Default, new GlowOpportunityStatusCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new GlowOpportunityStatusCollection();
			collection.Add("AAA", (NoResString)"AAA Description", true);
			collection.Add("ZZZ", (NoResString)"ZZZ Description", false);
			return new[] { collection };
		}
	}
}
