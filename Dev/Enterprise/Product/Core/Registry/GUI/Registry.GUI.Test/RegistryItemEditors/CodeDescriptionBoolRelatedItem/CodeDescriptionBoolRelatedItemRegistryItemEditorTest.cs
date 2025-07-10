using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CodeDescriptionBoolRelatedItemRegistryItemEditor))]
	sealed class CodeDescriptionBoolRelatedItemRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var editorInfo = new CodeDescriptionBoolRelatedItemRegistryEditorInfo((NoResString)"Bool");
			return new CodeDescriptionBoolRelatedItemRegistryItem("TEST_REGISTRY_ITEM", null, null, null, RegistryStorageFlags.System, 3, editorInfo, new CodeDescriptionBoolRelatedItemCollection(), null);
		}

		protected override RegistryItemEditor GetEditor()
		{
			var editorInfo = new CodeDescriptionBoolRelatedItemRegistryEditorInfo((NoResString)"Bool");
			return new CodeDescriptionBoolRelatedItemRegistryItemEditor(new CodeDescriptionBoolRelatedItemDataType(3), editorInfo, null);
		}

		protected override System.Type GetExpectedEditorPaneType()
		{
			return typeof(CodeDescriptionBoolRelatedItemRegistryControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new CodeDescriptionBoolRelatedItemCollection(OpportunitySourceRelatedItemProvider.SecondaryList);
			collection.Add("AAA", (NoResString)"DESC A", true, "CL2");
			collection.Add("BBB", (NoResString)"DESC B", false, "CL2");
			collection.Add("CCC", (NoResString)"DESC C", true);
			return new[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CodeDescriptionBoolRelatedItemRegistryControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
