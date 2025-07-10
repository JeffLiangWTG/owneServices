using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EES.Business.Testing
{
	public class EESForwardingShipmentTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertEquals("Type of New() return value", typeof(EESForwardingShipment), EESForwardingShipment.New(Factory).GetType());
		}

		public void TestDocumentSupporter()
		{
			EESForwardingShipment shipment = Factory.New<EESForwardingShipment>();
			AssertEquals("Type of DocumentSupporter", typeof(EESShipmentDocumentSupporter), shipment.DocumentSupporter.GetType());
		}
	}
}
