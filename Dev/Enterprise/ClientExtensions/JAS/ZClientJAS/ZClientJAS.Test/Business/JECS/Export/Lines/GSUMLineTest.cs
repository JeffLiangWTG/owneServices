using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	class GSUMLineTest : MessageLineTestCase
	{
		[TestDate(2007, 10, 1, 13, 10, 39)]
		public void TestLineAsString1()
		{
			((JASOrgHeader)GlbBranch.CurrentBranch.OrgProxy).OfficeCode = "USNYC";
			AssertEquals("GSUM3100;M;Y;DTO;01/10/2007;13:10;;;;USNYC;A", Line.LineAsString);
		}

		[TestDate(2007, 2, 3, 3, 44, 13)]
		public void TestLineAsString2()
		{
			Line = GetNewGsumLine(Constants.TransportModes.Air, "S123", "HB123", null, JXCConstants.GSUMEventCodes.POD_ProofOfDelivery);
			AssertEquals("GSUM3100;A;Y;POD;03/02/2007;03:44;;S123;HB123;NONET;A", Line.LineAsString);
		}

		[TestDate(2007, 2, 3, 3, 44, 13)]
		public void TestLineAsString3()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			Line = GetNewGsumLine(Constants.TransportModes.Air, "S123", "HB123", null, JXCConstants.GSUMEventCodes.POD_ProofOfDelivery);
			AssertEquals("GSUM3100;A;Y;POD;03/02/2007;03:44;;S123;HB123;NONET;A", Line.LineAsString);
		}

		[TestDate(2006, 1, 4, 2, 2, 0)]
		public void TestLineAsString_ShipmentOriginatedFromNonJASOffice()
		{
			((JASOrgHeader)GlbBranch.CurrentBranch.OrgProxy).OfficeCode = "AUMEL";
			JASOrgHeader header = Factory.New<JASOrgHeader>();
			Line = GetNewGsumLine(Constants.TransportModes.Sea, "S909", "", header, JXCConstants.GSUMEventCodes.PUP_PickedUpFromShipper);
			AssertEquals("GSUM3100;M;Y;PUP;04/01/2006;02:02;;S909;;AUMEL;A", Line.LineAsString);
		}

		[TestDate(2006, 1, 4, 2, 2, 0)]
		public void TestLineAsString_ShipmentOriginatedFromJASOffice()
		{
			JASOrgHeader header = Factory.New<JASOrgHeader>();
			header.OfficeCode = "BLAH";
			Line = GetNewGsumLine(Constants.TransportModes.Sea, "S909", "", header, JXCConstants.GSUMEventCodes.PUP_PickedUpFromShipper);
			AssertEquals("GSUM3100;M;Y;PUP;04/01/2006;02:02;;S909;;BLAH;A", Line.LineAsString);
		}

		protected override MessageLine GetMessageLine()
		{
			return GetNewGsumLine("", "", "", null, JXCConstants.GSUMEventCodes.DTO_DocumentTurnoverToBroker);
		}

		protected override int ExpectedFieldCount
		{
			get
			{
				return 10;
			}
		}

		protected override ZString ExpectedLineType
		{
			get
			{
				return "GSUM";
			}
		}

		GSUMLine GetNewGsumLine(ZString transportMode, ZString shipmentRef, ZString houseBill, JASOrgHeader sendingForwarder, string statusCode)
		{
			GsumShipmentWrapper shipmentWrapper = GetNewGsumShipmentWrapper();
			shipmentWrapper.Shipment.JS_TransportMode = transportMode;
			shipmentWrapper.Shipment.JS_UniqueConsignRef = shipmentRef;
			shipmentWrapper.Shipment.JS_HouseBill = houseBill;
			if (sendingForwarder != null)
			{
				JASForwardingConsol consol = (JASForwardingConsol)shipmentWrapper.Shipment.Consols.AddNew();
				consol.SetDefaultSendingForwarderAddress(sendingForwarder);
			}

			return new GSUMLine(shipmentWrapper, statusCode, ZDateTime.Now);
		}

		GsumShipmentWrapper GetNewGsumShipmentWrapper()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			return new GsumShipmentWrapper(shipment);
		}
	}
}
