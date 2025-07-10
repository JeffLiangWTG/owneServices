using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Rohlig.DocWrappers.Testing
{
	[TestedType(typeof(DocROHForwardingShipment))]
	public class DocROHForwardingShipmentTest : DocumentWrapperTestCase
	{
		public void TestHouseBillSignedBy()
		{
			RohDataRegistry.Instance.HouseBillSignedBy = "blah blah";
			AssertEquals("BLAH BLAH", wrapper.HouseBillSignedBy);
		}

		#region Implementation
		DocROHForwardingShipment wrapper;
		ForwardingShipment shipment;
		protected override void SetUp()
		{
			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			wrapper = DocROHForwardingShipment.New(shipment, Factory);
			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { wrapper };
		}
		#endregion
	}
}
