using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(JobInvoicingDefaultDepartmentsRegistryItemEditor))]
	public class JobInvoicingDefaultDepartmentsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new JobInvoicingDefaultDepartmentsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((JobInvoicingDefaultDepartmentsControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(JobInvoicingDefaultDepartmentsControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new JobInvoicingDefaultDepartmentsRegistryItem("", null, null, null, RegistryStorageFlags.System, new JobInvoicingDefaultDepartmentsCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			JobInvoicingDefaultDepartmentsCollection collection = new JobInvoicingDefaultDepartmentsCollection();
			JobInvoicingDefaultDepartments entry = collection.AddNew();
			entry.ConsolType = "ALL";
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
