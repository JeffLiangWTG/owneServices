using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(SalesRelationRulesRegistryItemEditor))]
	sealed class SalesRelationRulesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new SalesRelationDirectionRulesRegistryItem("", null, null, null, RegistryStorageFlags.System, new SalesRelationDirectionRuleCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new SalesRelationRulesRegistryItemEditor(null, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(SalesRelationDirectionRulesRegistryControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var rules = new SalesRelationDirectionRuleCollection();
			rules.AddNewRule(RelatableActivityTypeList.Codes.Communication, RelatableActivityTypeList.Codes.OpportunityManager);
			rules.AddNewRule(RelatableActivityTypeList.Codes.Communication, RelatableActivityTypeList.Codes.InquiryManager);

			return new[] { rules };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((SalesRelationDirectionRulesRegistryControl)editorPane).ReadOnly;
		}
	}
}
