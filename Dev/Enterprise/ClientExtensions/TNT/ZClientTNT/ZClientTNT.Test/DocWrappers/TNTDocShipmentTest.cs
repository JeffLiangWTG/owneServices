using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TNT.DocWrappers.Testing
{
	[TestedType(typeof(TNTDocForwardingShipment))]
	public class TNTDocShipmentTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			CreateTNTDocForwardingShipment();
			return new DocumentWrapper[] { DocShipment };
		}

		public void TestGoodsValue()
		{
			AssertEquals("Goods Value should be 943.35", 943.35m, DocShipment.GoodsValue);
			Shipment.JS_GoodsValue = 2m;
			AssertEquals("Goods Value should be 943.35", 2.00m, DocShipment.GoodsValue);
			Shipment.JS_GoodsValue = 1.1m;
			AssertEquals("Goods Value should be 943.35", 1.10m, DocShipment.GoodsValue);
		}

		ForwardingShipment Shipment;
		TNTDocForwardingShipment DocShipment;
		void CreateTNTDocForwardingShipment()
		{
			Shipment = Factory.NewWithValidTestData<ForwardingShipment>(TestBusinessObjectKind.MinimumRequiredToSave);
			Shipment.JS_GoodsValue = 943.3534m;
			DocShipment = TNTDocForwardingShipment.New(Shipment, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CreateTNTDocForwardingShipment();
		}
	}
}
