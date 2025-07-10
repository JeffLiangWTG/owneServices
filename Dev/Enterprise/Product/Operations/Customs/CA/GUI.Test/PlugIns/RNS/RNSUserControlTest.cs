using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.GUI.PlugIns;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class RNSUserControlTest : TestCaseWithFactory
	{
		public void TestArrivalCertificationVisiblity()
		{
			var shipment1 = Factory.New<CFSShipment>();
			var rnsMessagingBO1 = new RNSMessagingBO(new RNSPlugInSupportShipmentWrapper(shipment1));

			using (var rnsUserControl = new RNSUserControl(rnsMessagingBO1))
			{
				Assert(rnsUserControl.Controls.Find("ArrivalCertificationStatusTextBox", true)[0].Visible);
				Assert(rnsUserControl.Controls.Find("ArrivalCertificationDateEdit", true)[0].Visible);
			}

			var shipment2 = Factory.New<ForwardingShipment>();
			var rnsMessagingBO2 = new RNSMessagingBO(new RNSPlugInSupportShipmentWrapper(shipment2));

			using (var rnsUserControl = new RNSUserControl(rnsMessagingBO2))
			{
				Assert(rnsUserControl.Controls.Find("ArrivalCertificationStatusTextBox", true)[0].Visible);
				Assert(rnsUserControl.Controls.Find("ArrivalCertificationDateEdit", true)[0].Visible);
			}
		}
	}
}
