using CargoWise.Types;
using Enterprise.Client.YAS.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.YAS.Business.Testing
{
	[TestedType(typeof(DocYASForwardingShipment))]
	public class DocYASForwardingShipmentTest : DocumentWrapperTestCase
	{
		public void TestIsExpotingYASBillOfLading()
		{
			Shipment.IsExportingYASBillOfLading = false;
			AssertEquals("IsExportingYASBillOfLading ", false, ShipmentWrapper.IsExportingYASBillOfLading);
			Shipment.IsExportingYASBillOfLading = true;
			AssertEquals("IsExportingYASBillOfLading ", true, ShipmentWrapper.IsExportingYASBillOfLading);
		}

		public void TestEFreightIndicator()
		{
			AssertEquals("EFreightIndicator should be an empty string when custom flag is not set", ZString.Empty, ShipmentWrapper.EFreightIndicator);
			Shipment.DocsAndCartage.JP_CustomFlag1 = true;
			AssertEquals("EFreightIndicator should be an empty string when custom flag is set", DocYASForwardingShipment.EFreightIndicatorStr, ShipmentWrapper.EFreightIndicator);
		}

		protected override void SetUp()
		{
			Shipment = (YASForwardingShipment)TestHelper.GetExportShipment(Core.Constants.TransportModes.Sea);
			ShipmentWrapper = new DocYASForwardingShipment(Shipment, Factory);
			base.SetUp();
		}

		YASForwardingShipment Shipment;
		DocYASForwardingShipment ShipmentWrapper;

		YASTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new YASTestHelper()); }
		}

		YASTestHelper testHelper;

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { ShipmentWrapper };
		}
	}
}
