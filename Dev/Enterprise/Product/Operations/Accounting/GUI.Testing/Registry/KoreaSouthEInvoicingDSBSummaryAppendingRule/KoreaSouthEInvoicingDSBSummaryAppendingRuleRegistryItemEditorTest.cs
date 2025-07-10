using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Registry.GUI;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingDSBSummaryAppendingRuleRegistryItemEditor))]
	class KoreaSouthEInvoicingDSBSummaryAppendingRuleRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new KoreaSouthEInvoicingDSBSummaryAppendingRuleRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((KoreaSouthEInvoicingDSBSummaryAppendingRuleControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(KoreaSouthEInvoicingDSBSummaryAppendingRuleControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new KoreaSouthEInvoicingDSBSummaryAppendingRuleRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { GetRegistryItemWithSystemStorageLevel().DefaultValue };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		#endregion
	}
}
