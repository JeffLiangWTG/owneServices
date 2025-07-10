using CargoWise.Types;
using Enterprise.Client.YAS.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.YAS.Business.Testing
{
	[TestedType(typeof(DocYASAWB))]
	public class DocYASAWBTest : DocumentWrapperTestCase
	{
		public void TestEFreightIndicator()
		{
			AssertEquals("EFreightIndicator should be an empty string when custom flag is not set", ZString.Empty, AWBWrapper.EFreightIndicator);
			Shipment.DocsAndCartage.JP_CustomFlag1 = true;
			AssertEquals("EFreightIndicator should be an empty string when custom flag is set", DocYASForwardingShipment.EFreightIndicatorStr, AWBWrapper.EFreightIndicator);
		}

		ForwardingShipment Shipment;
		DocYASAWB AWBWrapper;

		protected override void SetUp()
		{
			Shipment = (YASForwardingShipment)TestHelper.GetExportShipment(Core.Constants.TransportModes.Air);
			AWBWrapper = (DocYASAWB)DocYASAWB.New(Shipment.AWBHeader, Factory);
			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { AWBWrapper };
		}

		YASTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new YASTestHelper()); }
		}

		YASTestHelper testHelper;
	}
}
