using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(JobInvoiceDescriptionRegistryItemEditor))]
	public class JobInvoiceDescriptionRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new JobInvoiceDescriptionRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((JobInvoiceDescriptionControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(JobInvoiceDescriptionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new JobInvoiceDescriptionRegistryItem("", null, null, null, RegistryStorageFlags.System, new JobInvoiceDescriptionCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new JobInvoiceDescriptionCollection();
			JobInvoiceDescription copy = collection.AddNew();

			copy.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			copy.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			copy.Mode = Constants.TransportModes.Rail;
			copy.InvoiceDescription = "This is a test Invoice Description";

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
