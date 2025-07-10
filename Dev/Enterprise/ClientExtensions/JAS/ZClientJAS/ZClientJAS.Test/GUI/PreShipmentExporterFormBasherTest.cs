using System.Windows.Forms;
using Enterprise.Client.JAS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.GUI.Testing
{
	[TestedType(typeof(PreShipmentExporterForm))]
	internal class PreShipmentExporterFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			return new PreShipmentExporterForm(shipment);
		}
	}
}
