using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(StaffReportingRoleRegistryItemEditor))]
	sealed class StaffReportingRoleRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new StaffReportingRoleRegistryItemEditor(new Enterprise.Registry.Business.StaffReportingRoleRegistryDataType(new StaffReportingRoleCollection()), null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((StaffReportingRoleRegistryControl)editorPane).ReadOnly;
		}

		protected override System.Type GetExpectedEditorPaneType()
		{
			return typeof(StaffReportingRoleRegistryControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new StaffReportingRoleRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, new StaffReportingRoleCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new StaffReportingRoleCollection();
			collection.Add("AAA", (NoResString)"AAA Description", true);
			collection.Add("ZZZ", (NoResString)"ZZZ Description", false);
			return new[] { collection };
		}
	}
}
