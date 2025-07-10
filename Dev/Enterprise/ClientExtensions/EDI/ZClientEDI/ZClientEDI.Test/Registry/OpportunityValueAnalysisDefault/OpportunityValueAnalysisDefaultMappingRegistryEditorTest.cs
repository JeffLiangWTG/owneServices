using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(OpportunityValueAnalysisDefaultRegistryEditor))]
	public class OpportunityValueAnalysisDefaultMappingRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new OpportunityValueAnalysisDefaultRegistryItem("");
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new OpportunityValueAnalysisDefaultRegistryEditor(new OpportunityValueAnalysisDefaultDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(OpportunityValueAnalysisDefaultControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			OpportunityValueAnalysisDefaultCollection[] items = new OpportunityValueAnalysisDefaultCollection[1];
			items[0] = new OpportunityValueAnalysisDefaultCollection();
			OpportunityValueAnalysisDefault item = items[0].AddNew();
			item.Code = "AUD";
			item.ValueInUSD = 0.20m;
			return items;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.All;
			}
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((OpportunityValueAnalysisDefaultControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
