using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.Rohlig.Testing
{
	public class RohForwardingShipmentTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertEquals("Type of instance created by static New", typeof(RohForwardingShipment), RohForwardingShipment.New(Factory).GetType());
		}

		public void TestDocumentSupporter()
		{
			AssertEquals("Type of Document Supporter", typeof(RohForwardingShipmentDocumentSupporter), RohForwardingShipment.New(Factory).DocumentSupporter.GetType());
		}
	}
}
