using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;

namespace Enterprise.Customs.GB.DocumentWrappers.Ccsuk.Testing
{
	class CcsukWrapperFromCusResTest : TestCaseWithFactory
	{
		public void TestNewFromCusResEdiMessageMawb()
		{
			var mawb = Factory.New<CusMAWB>();
			var message = mawb.Messages.AddNew();
			var inboundMessage = Factory.Load<GbEDIMessage>(message.PK);
			inboundMessage.EM_MessageType = "RES";
			inboundMessage.EM_MessageSubType = "IAR";
			inboundMessage.EM_MessageText = "UNH+999+CUSRES:2:912:UN:109606+448/U00000150'BGM+IAR+06052011002'NAD+CB+CAR+CARGOWISE'LOC+11:LHR:145:3::CAX:129:ZZZ+27:US+84:ATL:145:3'PAC+10'RFF+ABE:00000150'GIS+CA:120:ZZZ'FTX+CAT+++REQUEST TFR LGW'FTX+AAA+++CONSOLIDATION'UNT+10+999'";
			var mocker = Factory.NewMoq<GbEDIMessage>();
			var outboundMessage = mocker.Object;
			outboundMessage.EM_MessageNum = "448";
			outboundMessage.EM_ReceiveTransmit = "TRX";
			outboundMessage.EM_ApplicationCode = "CUK";
			mawb.Messages.Add(outboundMessage);
			var wrapper = CcsukWrapper.New(inboundMessage, Factory);
			AssertNotNull(wrapper);
			AssertEquals("00000150", wrapper.AGENTREF);
			AssertEquals("060-52011002", wrapper.MAWBHAWBSPLIT);
			AssertType(typeof(CcsukWrapperFromCusRes), wrapper);
		}

		public void TestNewFromCusResEdiMessageHawb()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var tsr = hawb.TSRs.AddNew();
			tsr.C4_SendersMessageReference = "U00000104";
			var message = hawb.Messages.AddNew();
			var inboundMessage = Factory.Load<GbEDIMessage>(message.PK);
			inboundMessage.EM_MessageType = "RES";
			inboundMessage.EM_MessageSubType = "TSR";
			inboundMessage.EM_MessageText = "UNH+999+CUSRES:2:912:UN:109606+318/U00000104'BGM+TSR+19042011008+++HWB:HOUSEY'NAD+CB+CAR+CARGOWISE'LOC+11:LHR:145:3::CWE:129:ZZZ+27:US+84:ATL:145:3'PAC+10'RFF+TN:T001579'GIS+CA:120:ZZZ'FTX+CAT+++REQUEST TFR CDG'FTX+AAA+++MORE TOYS'UNT+10+999'";
			var mocker = Factory.NewMoq<GbEDIMessage>();
			var outboundMessage = mocker.Object;
			outboundMessage.EM_MessageNum = "318";
			outboundMessage.EM_ReceiveTransmit = "TRX";
			outboundMessage.EM_ApplicationCode = "CUK";
			hawb.Messages.Add(outboundMessage);
			var wrapper = CcsukWrapper.New(inboundMessage, Factory);
			AssertNotNull(wrapper);
			AssertEquals("00000104", wrapper.AGENTREF);
			AssertEquals("190-42011008-HOUSEY", wrapper.MAWBHAWBSPLIT);
			AssertType(typeof(CcsukWrapperFromCusRes), wrapper);
		}

		public void TestNewFromCusResEdiMessageSplitHawb()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var split = hawb.Splits.AddNew();
			split.SplitReference = "69";
			split.SetCustomsActionCode("DC", ZDateTime.BrettsBirthday);
			split.LatestCustomsActionText = "CAT FROM AWB NOT MSG";
			var tsr = hawb.TSRs.AddNew();
			tsr.C4_SendersMessageReference = "U00000104";
			tsr.SplitReferenceToWhichThisRemovalPertains = "69";
			var message = hawb.Messages.AddNew();
			var inboundMessage = Factory.Load<GbEDIMessage>(message.PK);
			inboundMessage.EM_MessageType = "RES";
			inboundMessage.EM_ApplicationReference = "69";
			inboundMessage.EM_MessageSubType = "TSR";
			inboundMessage.EM_MessageText = "UNH+999+CUSRES:2:912:UN:109606+318/U00000104'BGM+TSR+19042011008+++HWB:HOUSEY:69'NAD+CB+CAR+CARGOWISE'LOC+11:LHR:145:3::CWE:129:ZZZ+27:US+84:ATL:145:3'PAC+10'RFF+TN:T001579'GIS+CA:120:ZZZ'FTX+CAT+++REQUEST TFR CDG'FTX+AAA+++MORE TOYS'UNT+10+999'";
			var mocker = Factory.NewMoq<GbEDIMessage>();
			var outboundMessage = mocker.Object;
			outboundMessage.EM_MessageNum = "318";
			outboundMessage.EM_ReceiveTransmit = "TRX";
			outboundMessage.EM_ApplicationCode = "CUK";
			hawb.Messages.Add(outboundMessage);
			var wrapper = CcsukWrapper.New(inboundMessage, Factory);
			AssertNotNull(wrapper);
			AssertEquals("00000104", wrapper.AGENTREF);
			AssertEquals("190-42011008-HOUSEY/69", wrapper.MAWBHAWBSPLIT);
			AssertEquals("Get CAC/CAT from messasge, not AWB, because a CUSRES does not update the AWB's CAC/CAT", "CA", wrapper.CAC);
			AssertEquals("REQUEST TFR CDG", wrapper.CAT);
			AssertType(typeof(CcsukWrapperFromCusRes), wrapper);
		}

		public void TestVirtualPropertiesForRemovalRequest_IAR()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "06052011002";
			mawb.NumberOfPiecesExpected = 999;
			var underbond = mawb.IARs.AddNew();
			underbond.C4_SendersMessageReference = "U00000150";
			var message = mawb.Messages.AddNew();
			var inboundMessage = Factory.Load<GbEDIMessage>(message.PK);
			inboundMessage.EM_MessageNum = "448";
			inboundMessage.EM_MessageType = "RES";
			inboundMessage.EM_MessageSubType = "IAR";
			inboundMessage.EM_MessageText = "UNH+999+CUSRES:2:912:UN:109606+448/U00000150'BGM+IAR+06052011002+++ACD::69'NAD+CB+CAR+CARGOWISE'LOC+11:LHR:145:3::CAX:129:ZZZ+27:US+84:ATL:145:3'PAC+10'RFF+ABE:00000150'GIS+CA:120:ZZZ'FTX+CAT+++REQUEST TFR LGW'FTX+AAA+++CONSOLIDATION'UNT+10+999'";
			var mocker = Factory.NewMoq<GbEDIMessage>();
			var outboundMessage = mocker.Object;
			outboundMessage.EM_MessageNum = "448";
			outboundMessage.EM_ReceiveTransmit = "TRX";
			outboundMessage.EM_ApplicationCode = "CUK";
			mawb.Messages.Add(outboundMessage);
			var wrapper = (CcsukWrapperFromCusRes)CcsukWrapper.New(inboundMessage, Factory);

			AssertEquals("CARGOWISE", wrapper.AGENTNAME);
			AssertEquals("", wrapper.T1STATEMENT);
			AssertEquals("", wrapper.POS);
			AssertEquals("CAX", wrapper.NEWSHED);
			AssertEquals("", wrapper.TRN);
			AssertEquals("060-52011002/69", wrapper.MAWBHAWBSPLIT);
			AssertEquals("Inter-Airport Removal", wrapper.REMOVALTYPE);
		}

		public void TestVirtualPropertiesForRemovalRequest_TSR()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "190-42011008";
			mawb.NumberOfPiecesExpected = 999;
			var underbond = mawb.TSRs.AddNew();
			underbond.PortOfShipment = "CDG";
			underbond.LicenseRestrictionInd = Enterprise.Customs.Business.YesNoList.Codes.Yes;
			underbond.C4_SendersMessageReference = "U00000104";
			var message = mawb.Messages.AddNew();
			var inboundMessage = Factory.Load<GbEDIMessage>(message.PK);
			inboundMessage.EM_MessageNum = "318";
			inboundMessage.EM_MessageType = "RES";
			inboundMessage.EM_MessageSubType = "TSR";
			inboundMessage.EM_MessageText = "UNH+999+CUSRES:2:912:UN:109606+318/U00000104'BGM+TSR+19042011008+++HWB:HOUSEY:69'NAD+CB+CAR+CARGOWISE'LOC+11:LHR:145:3::CWE:129:ZZZ+27:US+84:ATL:145:3'PAC+10'RFF+TN:T001579'GIS+CA:120:ZZZ'FTX+CAT+++REQUEST TFR CDG'FTX+AAA+++MORE TOYS'UNT+10+999'";
			var mocker = Factory.NewMoq<GbEDIMessage>();
			var outboundMessage = mocker.Object;
			outboundMessage.EM_MessageNum = "318";
			outboundMessage.EM_ReceiveTransmit = "TRX";
			outboundMessage.EM_ApplicationCode = "CUK";
			mawb.Messages.Add(outboundMessage);
			var wrapper = (CcsukWrapperFromCusRes)CcsukWrapper.New(inboundMessage, Factory);
			AssertContains("T1 STATUS", wrapper.T1STATEMENT);
			AssertEquals("CDG", wrapper.POS);
			AssertEquals("T001579", wrapper.TRN);
			AssertContains("RESTRICTED", wrapper.LICENCEINDICATOR1);
			AssertContains("DECLARED", wrapper.LICENCEINDICATOR2);
			underbond.LicenseRestrictionInd = Enterprise.Customs.Business.YesNoList.Codes.No;
			AssertEquals("", wrapper.LICENCEINDICATOR1);
			AssertEquals("", wrapper.LICENCEINDICATOR2);
			AssertEquals("Transhipment Removal", wrapper.REMOVALTYPE);
			AssertEquals("190-42011008-HOUSEY/69", wrapper.MAWBHAWBSPLIT);
		}

		public void TestAllPropertiesForFallbackRequest()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			mawb.AirportOfOrigin = "USLAX";
			mawb.CM_MAWB = "111-77777777";
			mawb.AirportOfDestination = "GBMAN";
			hawb.CS_HAWB = "12345678";
			hawb.AirportOfArrival = "LHR";
			hawb.AirportOfOrigin = "USATL";
			hawb.CargoTerminalOperatorAirport = "LHR";
			hawb.CargoTerminalOperator = "CWE";
			hawb.AgentBadge = "DAN";
			hawb.CS_Weight = 69m;
			hawb.CS_WeightUQ = "KG";
			hawb.CS_GoodsDescription = "STUFF";
			hawb.LatestCustomsActionText = "PUSSY GALORE";
			hawb.CS_RX_NKGoodsCurrency = "GBP";
			hawb.CS_GoodsValue = 69.69m;
			hawb.CS_PiecesManifested = 500;
			hawb.CS_PiecesLanded = 600;

			var underbond = hawb.FBKs.AddNew();
			underbond.C4_SendersMessageReference = "U00000104";
			var message = hawb.Messages.AddNew();
			var inboundMessage = Factory.Load<GbEDIMessage>(message.PK);
			inboundMessage.EM_MessageNum = "999";
			inboundMessage.EM_ReceiveTransmit = "RCV";
			inboundMessage.EM_MessageType = "RES";
			inboundMessage.EM_MessageSubType = "FBK";
			inboundMessage.EM_SystemCreateTimeUtc = new ZDateTime(1987, 12, 11, 1, 2, 3);
			inboundMessage.EM_MessageText = "UNH+999+CUSRES:2:912:UN:109606+318/U00000104'BGM+FBK+11177777777+++HWB:12345678'NAD+CB+FRF+FREDS FORWARDING:0812461234'LOC+11:LGW:145:3::SID:129:ZZZ+27:US+84:LAX:145:3'PAC+80'RFF+ABE:4321'RFF+TN:9876543+141:20101213:102'GIS+CA:120:ZZZ'FTX+CAT+++ENTRY/REQUEST ACCEPTED'FTX+AAA+++HOME BREW'UNT+10+999'";

			var mocker = Factory.NewMoq<GbEDIMessage>();
			var outboundMessage = mocker.Object;
			outboundMessage.EM_MessageNum = "318";
			outboundMessage.EM_MessageType = "FBK";
			outboundMessage.EM_ReceiveTransmit = "TRX";
			outboundMessage.EM_ApplicationCode = "CUK";
			hawb.Messages.Add(outboundMessage);

			var w = (CcsukWrapperFromCusRes)CcsukWrapper.New(inboundMessage, Factory);
			AssertEquals("LHR", w.AOD);
			AssertEquals("CWE", w.SHED);
			AssertEquals("111-77777777-12345678", w.MAWBHAWBSPLIT);
			AssertEquals("500", w.NPX);
			AssertEquals("600", w.NPR);
			AssertEquals("STUFF", w.DESCRIPTION);
			AssertEquals("US", w.COO);
			AssertEquals("ATL", w.AOO);
			AssertEquals("LHR", w.AOD);
			AssertEquals("987-6543", w.ENTRY);
			AssertEquals("13/12/10", w.ENTRYDATE);
			AssertEquals("FREDS FORWARDING", w.AGENTNAME);
			AssertEquals("DAN", w.BADGE);
			AssertEquals("4321", w.AGENTREF);
			AssertEquals("0812461234", w.AGENTPHONE);
			AssertEquals("Fallback", w.REMOVALTYPE);
		}
	}
}
