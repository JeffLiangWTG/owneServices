using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.AWB.Testing
{
	[TestedType(typeof(JASShipmentExportAWBHeader))]
	public class JASShipmentExportAWBRateLineTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("implemented in a 'special' way on base class", true);
		}

		public void TestHumanReadableName()
		{
			JASShipmentExportAWBHeader awbHeader = Factory.New<JASShipmentExportAWBHeader>();
			JASShipmentExportAWBRateLine rateLine = (JASShipmentExportAWBRateLine)awbHeader.AWBRateLines.AddNew();
			AssertEquals("House Air Waybill Freight Breakdown", rateLine.HumanReadableName);
		}

		public void TestNatureAndQtyOfGoodsText()
		{
			JASShipmentExportAWBHeader awbHeader = Factory.New<JASShipmentExportAWBHeader>();
			JASShipmentExportAWBRateLine rateLine = (JASShipmentExportAWBRateLine)awbHeader.AWBRateLines.AddNew();
			AssertEquals(typeof(JASNatureAndQtyOfGoods), rateLine.NatureAndQtyOfGoodsText.GetType());
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return Factory.New<JASShipmentExportAWBHeader>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			Env.Registry.Freight.AirWaybill.AllowAutoCalculationOfTax = false;
			var header = factory.New<JASShipmentExportAWBHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OverrideWaybillDefaults = ZBool.False;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUBNE";
			header.EH_ParentID = shipment.PK;
			return header;
		}
	}
}
