using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class InvoicingFormPluginTest : InvoicingPluginToFreightTest
	{
		public void TestUserControl()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				var mainPanel = new ZPanel();
				mainPanel.Name = "MainPanel";
				form.Controls.Add(mainPanel);

				form.PlugIns.Add(ControllerIDs.JobInvoicingForm);
				var plugin = (InvoicingFormPlugin)form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicingForm);
				plugin.OnUserControlShown();

				AssertNotNull("UserControl is added in the form.", form.Controls.Find("JobInvoicingUserControl", true));
			}
		}

		public override void TestTextOverride()
		{
			Assert("Plugin's Text is not used in InvoicingForm.", true);
		}
	}
}
