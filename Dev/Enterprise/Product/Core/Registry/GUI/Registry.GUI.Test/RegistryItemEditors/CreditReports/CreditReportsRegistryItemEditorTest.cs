using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CreditReportsRegistryItemEditor))]
	sealed class CreditReportsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new CreditReportsRegistryItemEditor(new CreditReportItemDataType(), null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CreditReportsRegistryControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CreditReportsRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CreditReportItemCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new CreditReportItemCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new CreditReportItemCollection() };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		#endregion
	}
}
