using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.JAS.Business.Utilities.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class OMANRecordTest : JXCRecordTestCase
	{
		public void TestLoadConsol_NullParam()
		{
			OMANRecord record = new OMANRecord("", "");
			AssertNull("Should return null when null factory is passed in", record.LoadConsol(null));
		}

		[ExpectNoExceptions]
		public void TestUpdateConsol_NullParams()
		{
			OMANRecord record = (OMANRecord)RecordFactory.NewRecord("OMAN3100;n;01406005003509;KAPITAN MASLOV;615SB;VANCOUVER, BC, CANAD;SYDNEY, AUS.;USYVR;CAYVR;AUSYD;AUSYD;23/05/2005;23/05/2005;13/06/2005");
			record.UpdateConsol(null);
		}

		public void TestLoadConsol()
		{
			CreateConsolsForTest();
			OMANRecord record = (OMANRecord)RecordFactory.NewRecord("OMAN3100;n;101;TEST VESSEL;VOY001;VANCOUVER, BC, CANAD;SYDNEY, AUS.;USYVR;CAYVR;AUSYD;AUSYD;23/05/2005;12/01/2005;13/06/2005");
			JASForwardingConsol loadedConsol = record.LoadConsol(Factory);
			AssertEquals("has to load the closest match", "2", loadedConsol.JK_UniqueConsignRef);
			record = (OMANRecord)RecordFactory.NewRecord("OMAN3100;n;103;TEST VESSEL;VOY001;VANCOUVER, BC, CANAD;SYDNEY, AUS.;USYVR;CAYVR;AUSYD;AUSYD;23/05/2005;12/01/2005;13/06/2005");
			loadedConsol = record.LoadConsol(Factory);
			AssertNull("Should not match", loadedConsol);
			record = (OMANRecord)RecordFactory.NewRecord("OMAN3100;n;102;TEST VESSEL1;VOY001;VANCOUVER, BC, CANAD;SYDNEY, AUS.;USYVR;CAYVR;AUSYD;AUSYD;23/05/2005;12/01/2005;13/06/2005");
			loadedConsol = record.LoadConsol(Factory);
			AssertEquals("has to load the closest match", "3", loadedConsol.JK_UniqueConsignRef);
		}

		public void TestUpdateConsol()
		{
			ZString validVesselName = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_Code;
			OMANRecord record = (OMANRecord)RecordFactory.NewRecord("OMAN3100;n;101;" + validVesselName + ";VOY001;VANCOUVER, BC, CANAD;SYDNEY, AUS.;USYVR;CAYVR;AUSYD;AUSYD;23/05/2005;12/01/2005;13/06/2005");
			JASForwardingConsol consol = CreateNewConsolForUpdateConsolTest();
			DataImportFlagChanger.LastBizO = null;
			record.UpdateConsol(consol);
			Transport transport = consol.Transports[0];
			AssertEquals("101", consol.JK_AgentsReference);
			AssertEquals(validVesselName, transport.JW_Vessel);
			AssertEquals("VOY001", transport.JW_VoyageFlight);
			AssertEquals("CAYVR", transport.JW_RL_NKLoadPort);
			AssertEquals("AUSYD", transport.JW_RL_NKDiscPort);
			AssertEquals(new ZDateTime(2005, 1, 12), transport.JW_ETD);
			AssertEquals(new ZDateTime(2005, 6, 13), transport.JW_ETA);
			AssertEquals("Should use DataImportFlagChanger", consol, DataImportFlagChanger.LastBizO);
			ZString invalidVesselName = "__NOTAVALIDVESSEL_";
			transport.JW_Vessel = "";
			record = (OMANRecord)RecordFactory.NewRecord("OMAN3100;n;101;" + invalidVesselName + ";VOY001;VANCOUVER, BC, CANAD;SYDNEY, AUS.;USYVR;;AUBNE;;23/05/2005;12/01/2005;13/06/2005");
			record.UpdateConsol(consol);
			AssertEquals("Not a valid vessel code, this should not be populated", "", transport.JW_Vessel);
			AssertEquals("USYVR", transport.JW_RL_NKLoadPort);
			AssertEquals("AUBNE", transport.JW_RL_NKDiscPort);
		}

		[ExpectNoExceptions]
		public void TestUpdateConsol_ExcessivelyLongStringIsTrimmed()
		{
			string excessivelyLongString = new string('p', 1000);
			JASCsvLineForTest fields = new JASCsvLineForTest(JXCConstants.OMANFieldCount);
			fields.SetFieldValue(JXCConstants.OMANFieldPositions.EstimatedArrivalDate, excessivelyLongString);
			fields.SetFieldValue(JXCConstants.OMANFieldPositions.EstimatedShippingDate, excessivelyLongString);
			fields.SetFieldValue(JXCConstants.OMANFieldPositions.ManifestNo, excessivelyLongString);
			fields.SetFieldValue(JXCConstants.OMANFieldPositions.ManifestPrintDate, excessivelyLongString);
			fields.SetFieldValue(JXCConstants.OMANFieldPositions.PortOfDichargeCodeForCustoms, excessivelyLongString);
			fields.SetFieldValue(JXCConstants.OMANFieldPositions.PortOfDischargeCode, excessivelyLongString);
			fields.SetFieldValue(JXCConstants.OMANFieldPositions.PortOfDischargeName, excessivelyLongString);
			fields.SetFieldValue(JXCConstants.OMANFieldPositions.PortOfLoadingCode, excessivelyLongString);
			fields.SetFieldValue(JXCConstants.OMANFieldPositions.PortOfLoadingCodeForCustoms, excessivelyLongString);
			fields.SetFieldValue(JXCConstants.OMANFieldPositions.PortOfLoadingName, excessivelyLongString);
			fields.SetFieldValue(JXCConstants.OMANFieldPositions.TypeOfRecord, excessivelyLongString);
			fields.SetFieldValue(JXCConstants.OMANFieldPositions.VesselName, excessivelyLongString);
			fields.SetFieldValue(JXCConstants.OMANFieldPositions.VoyageNo, excessivelyLongString);
			OMANRecord record = new OMANRecord(JXCConstants.LineTypes.OMAN, fields.ConvertToJXCLine());
			JASForwardingConsol consol = CreateNewConsolForUpdateConsolTest();
			record.UpdateConsol(consol);
		}

		protected override JXCRecord GetNewRecord(ZString lineType, ZString lineContent)
		{
			return new OMANRecord(lineType, lineContent);
		}

		void CreateConsolsForTest()
		{
			CreateConsol("1", "101", Core.Constants.TransportModes.Air, "TEST VESSEL", "VOY001", new ZDateTime(2005, 1, 12));
			CreateConsol("2", "101", Core.Constants.TransportModes.Sea, "TEST VESSEL", "VOY001", new ZDateTime(2005, 1, 12));
			CreateConsol("3", "102", Core.Constants.TransportModes.Sea, "TEST VESSEL1", "VOY001", new ZDateTime(2005, 1, 12));
			CreateConsol("4", "102", Core.Constants.TransportModes.Road, "TEST VESSEL", "VOY002", new ZDateTime(2005, 1, 12));
			CreateConsol("5", "103", Core.Constants.TransportModes.Sea, "TEST VESSEL", "VOY00-1", new ZDateTime(2006, 1, 13));
			CreateConsol("6", "103", Core.Constants.TransportModes.Sea, "TEST VESSEL", "VOY00-2", new ZDateTime(2005, 1, 12));
			CreateConsol("71", "103", Core.Constants.TransportModes.Sea, "TEST VESSE2L", "VOY001", new ZDateTime(2005, 1, 12));
		}

		void CreateConsol(ZString uniqueRef, ZString agentReference, ZString transportMode, ZString vesselName, ZString voyageNo, ZDateTime eTD)
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			consol.JK_UniqueConsignRef = uniqueRef;
			consol.JK_AgentsReference = agentReference;
			consol.JK_TransportMode = transportMode;
			RefVessel vessel = RefVessel.LookupVesselByCode(vesselName, Factory);
			if (vessel == null)
			{
				vessel = Factory.New<RefVessel>();
				vessel.RV_Code = vesselName;
			}

			Transport transport = consol.Transports[0];
			transport.JW_Vessel = vesselName;
			transport.JW_VoyageFlight = voyageNo;
			transport.JW_ATD = eTD;
		}

		JASForwardingConsol CreateNewConsolForUpdateConsolTest()
		{
			JASForwardingConsol result = Factory.New<JASForwardingConsol>();
			result.JK_AgentType = Core.Constants.AgentType.Agent;
			result.JK_TransportMode = Core.Constants.TransportModes.Sea;
			result.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			result.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			return result;
		}
	}
}
