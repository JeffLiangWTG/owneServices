using CargoWise.Types;
using Enterprise.Client.JAS.Business.JXC;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.AWB.Testing
{
	[TestedType(typeof(JASConsolExportAWBHeader))]
	internal class JASConsolExportAWBHeaderTest : ConsolExportAWBHeaderTest
	{
		public void TestRightTypeIsReturnedFromParentBizO()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(typeof(JASConsolExportAWBHeader), consol.AWBHeader.GetType());
		}

		public void TestShipper()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			JASConsolExportAWBHeader aWBHeader = (JASConsolExportAWBHeader)consol.AWBHeader;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.SetDefaultSendingForwarderAddress(Factory.New<OrgHeader>());
			IJASExportAWBHeader jASAWBHeader = aWBHeader;
			AssertEquals(consol.SendingForwarder.PK, jASAWBHeader.Shipper.PK);
		}

		public void TestConsignee()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			JASConsolExportAWBHeader aWBHeader = (JASConsolExportAWBHeader)consol.AWBHeader;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.SetDefaultReceivingForwarderAddress(Factory.New<OrgHeader>());
			IJASExportAWBHeader jASAWBHeader = aWBHeader;
			AssertEquals(consol.ReceivingForwarder.PK, jASAWBHeader.Consignee.PK);
		}

		public void TestShipperAccountForJXC()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			JASConsolExportAWBHeader aWBHeader = (JASConsolExportAWBHeader)consol.AWBHeader;
			IJASExportAWBHeader jASAWBHeader = aWBHeader;
			aWBHeader.EH_ShipperAccount = "";
			consol.SetDefaultSendingForwarderAddress(ZGuid.Empty);
			AssertEquals("No shipper specified yet", JXCConstants.NotAvailable, jASAWBHeader.ShipperAccountForJXC);
			aWBHeader.EH_ShipperAccount = "TEST";
			AssertEquals("TEST", jASAWBHeader.ShipperAccountForJXC);
			aWBHeader.EH_ShipperAccount = "";
			consol.SetDefaultSendingForwarderAddress(Factory.New<JASOrgHeader>());
			consol.SendingForwarder.OH_Code = "HAHA";
			AssertEquals("HAHA", jASAWBHeader.ShipperAccountForJXC);
		}

		public void TestConsigneeAccountForJXC()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			JASConsolExportAWBHeader aWBHeader = (JASConsolExportAWBHeader)consol.AWBHeader;
			IJASExportAWBHeader jASAWBHeader = aWBHeader;
			aWBHeader.EH_ConsigneeAccount = "";
			consol.SetDefaultReceivingForwarderAddress(ZGuid.Empty);
			AssertEquals("No shipper specified yet", JXCConstants.NotAvailable, jASAWBHeader.ConsigneeAccountForJXC);
			aWBHeader.EH_ConsigneeAccount = "TEST";
			AssertEquals("TEST", jASAWBHeader.ConsigneeAccountForJXC);
			aWBHeader.EH_ConsigneeAccount = "";
			consol.SetDefaultReceivingForwarderAddress(Factory.New<JASOrgHeader>());
			consol.ReceivingForwarder.OH_Code = "HAHA";
			AssertEquals("HAHA", jASAWBHeader.ConsigneeAccountForJXC);
		}

		public void TestAWBRateLines()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			JASConsolExportAWBHeader awbHeader = (JASConsolExportAWBHeader)consol.AWBHeader;
			AssertEquals(typeof(JASConsolExportAWBRateLineCollection), awbHeader.AWBRateLines.GetType());
		}
	}
}
