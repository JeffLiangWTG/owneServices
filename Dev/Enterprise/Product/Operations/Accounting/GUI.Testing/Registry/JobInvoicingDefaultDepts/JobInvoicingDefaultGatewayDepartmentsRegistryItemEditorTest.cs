using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(JobInvoicingDefaultGatewayDepartmentsRegistryItemEditor))]
	public class JobInvoicingDefaultGatewayDepartmentsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new JobInvoicingDefaultGatewayDepartmentsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((JobInvoicingDefaultGatewayDepartmentsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(JobInvoicingDefaultGatewayDepartmentsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new JobInvoicingDefaultGatewayDepartmentsRegistryItem("", null, null, null, RegistryStorageFlags.System, new JobInvoicingDefaultGatewayDepartmentsCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new JobInvoicingDefaultGatewayDepartmentsCollection();
			JobInvoicingDefaultGatewayDepartments entry = collection.AddNew();
			entry.Direction = "IMP";
			entry.TransportMode = "AIR";
			entry.ConsolType = "CLD";
			entry.Department = entry.Departments[0].PK;

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
