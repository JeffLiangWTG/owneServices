using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.YAS.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Client.YAS.Testing.Business
{
	[TestedType(typeof(DocYASBillOfLading))]
	public class DocYASBillOfLadingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBadCasting()
		{
			AssertNoExceptionThrown(YASShipmentIsExporting);
		}

		void YASShipmentIsExporting()
		{
			var yASShipment = new DocYASForwardingShipment(Factory.New<ForwardingModuleShipment>(), Factory);
			bool isExport = yASShipment.IsExportingYASBillOfLading;
		}

		public void TestShowTCImage()
		{
			Shipment.IsExportingYASBillOfLading = false;
			AssertEquals("ShowTCImage", true, Wrapper.ShowTCImage);

			Shipment.IsExportingYASBillOfLading = true;
			AssertEquals("ShowTCImage", false, Wrapper.ShowTCImage);
		}

		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
			Shipment = (YASForwardingShipment)TestHelper.GetExportShipment(Core.Constants.TransportModes.Sea);
			Shipment.JS_HouseBillOfLadingType = "IAU";
			ShipmentWrapper = new DocYASForwardingShipment(Shipment, Factory);
			Wrapper = new DocYASBillOfLading(ShipmentWrapper);
			base.SetUp();
		}

		YASForwardingShipment Shipment;
		DocYASForwardingShipment ShipmentWrapper;
		DocYASBillOfLading Wrapper;

		YASTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new YASTestHelper()); }
		}

		YASTestHelper testHelper;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocYASBillOfLading(ShipmentWrapper);
		}
	}
}
