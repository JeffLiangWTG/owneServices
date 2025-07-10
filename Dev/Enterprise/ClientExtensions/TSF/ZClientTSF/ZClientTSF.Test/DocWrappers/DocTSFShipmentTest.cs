using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TSF.DocWrappers.Testing
{
	[TestedType(typeof(DocTSFForwardingShipment))]
	public class DocTSFShipmentTest : DocumentWrapperTestCase
	{
		public void TestFirstContainer()
		{
			AssertNull("First Container of the shipment", Wrapper.FirstContainer);
			CommonContainer c1 = Consol.Containers.AddNew();
			c1.JC_ContainerNum = "abcd1234567";
			CommonContainer c2 = Consol.Containers.AddNew();
			c2.JC_ContainerNum = "wxyz1234567";
			PackLine packLine = Shipment.OuterPackLines.AddNew();
			packLine.Containers.Add(c2);
			Factory.Save();
			Wrapper = DocTSFForwardingShipment.New(Shipment, Factory);
			AssertEquals("First Container of the shipment", "WXYZ1234567", Wrapper.FirstContainer.ContainerNumber);
		}

		#region SetUp & Overrides
		protected override void SetUp()
		{
			Shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Consol = Factory.NewWithValidTestData<ForwardingConsol>();
			Consol.Shipments.Add(Shipment);
			Wrapper = DocTSFForwardingShipment.New(Shipment, Factory);
			base.SetUp();
		}

		ForwardingConsol Consol;
		ForwardingShipment Shipment;
		DocTSFForwardingShipment Wrapper;
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { Wrapper };
		}
		#endregion
	}
}
