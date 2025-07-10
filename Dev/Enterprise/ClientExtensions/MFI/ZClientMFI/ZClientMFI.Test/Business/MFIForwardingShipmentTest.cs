using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.MFI.Testing
{
	public class MFIForwardingShipmentTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertEquals("Type of New() return value", typeof(MFIForwardingShipment), MFIForwardingShipment.New(Factory).GetType());
		}

		public void TestDocumentSupporter()
		{
			MFIForwardingShipment shipment = Factory.New<MFIForwardingShipment>();
			AssertEquals("Type of DocumentSupporter", typeof(MFIForwardingShipmentDocumentSupporter), shipment.DocumentSupporter.GetType());
		}

		public void TestConsol()
		{
			MFIForwardingShipment shipment = Factory.NewWithValidTestData<MFIForwardingShipment>();
			Factory.Save();
			AssertNull(shipment.Consol);
			ForwardingConsol consol = shipment.Consols.AddNew();
			consol.MasterBillMAWB = "Master";
			Factory.Save();
			AssertNotNull(shipment.Consol);
			AssertEquals("Master", shipment.Consol.JK_MasterBillNum);
		}
	}
}
