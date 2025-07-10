using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class FreightConsolManifestHeaderWrapperTest : TestCaseWithFactory
	{
		public void TestAUExportManifestMessage_HVMShipment_Excluded()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00026899";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_MasterBillNum = "081-45238340";
			var transport = consol.Transports[0];
			transport.JW_VoyageFlight = "QF118";
			transport.JW_ATD = new ZDateTime(new DateTime(2016, 06, 28));

			var hvlShipment = AddHVLShipmentToConsol(consol, "HLV-001");
			var hvmShipment = AddHVMShipmentToConsol(consol, "HVM-001");

			var cusEntryNumber = CusEntryNumber.New(hvmShipment, CANType.CustomsAuthorityNumber.Code, Core.Constants.CountryCodes.Australia);
			cusEntryNumber.CE_EntryNum = "IUsedToSkipHVMFromCustomsMessaging";
			cusEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;

			hvlShipment.JS_JS_ColoadMasterShipment = hvmShipment.PK;

			Factory.Save();

			var lines = new FreightConsolManifestHeaderWrapper(consol).Lines;

			AssertCollectionNotContains("HVM House bill \"HVM - 001\" should not be in the message", "HVM-001", lines.Select(l => l.HouseBillNumber));
		}

		public void TestAUExportManifestMessage_HVLShipmentHavingHVMParent_Included()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00026899";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_MasterBillNum = "081-45238340";
			var transport = consol.Transports[0];
			transport.JW_VoyageFlight = "QF118";
			transport.JW_ATD = new ZDateTime(new DateTime(2016, 06, 28));

			var hvlShipment = AddHVLShipmentToConsol(consol, "HLV-001");
			var hvmShipment = AddHVMShipmentToConsol(consol, "HVM-001");

			var cusEntryNumber = CusEntryNumber.New(hvmShipment, CANType.CustomsAuthorityNumber.Code, Core.Constants.CountryCodes.Australia);
			cusEntryNumber.CE_EntryNum = "IUsedToSkipHVMFromCustomsMessaging";
			cusEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;

			hvlShipment.JS_JS_ColoadMasterShipment = hvmShipment.PK;

			Factory.Save();

			var lines = new FreightConsolManifestHeaderWrapper(consol).Lines;

			CombineAssertions(() =>
			{
				AssertEquals("Wrapper should have 4 lines from HVLVConsignment", 4, lines.Length);
				AssertContainsExactElementsInAnyOrder("All HVC_WaybillNumbers should be in the message",
					new[] { "HVLV1", "HVLV2", "HVLV3", "HVLV4" },
					lines.Select(line => line.HouseBillNumber));
			});
		}

		public void TestDepotPremiseID()
		{
			OrgHeader org = OrgHeader.New(Factory);
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;
			org.OH_RL_NKClosestPort = "AUSYD";
			OrgAddress address1 = org.Addresses[0];
			address1.OA_Address1 = "test1";
			address1.OA_Address2 = "test2";
			consol.JK_OA_PackDepotAddress = address1.PK;
			consol.PackDepotAddress.LocalControlledPremisesID = "123987";
			AssertEquals(consol.PackDepotAddress.LocalControlledPremisesID, wrapper.DepotPremiseID);
		}

		public void TestCCAN()
		{
			new FreightConsolWrapper(consol).ContingencyCAN = "12345";
			AssertEquals(new FreightConsolWrapper(consol).ContingencyCAN, wrapper.CCAN);
		}

		public void TestShouldReplaceMessageIfNoSequencingData()
		{
			var testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_UniqueConsignRef = "C00027113";
			testConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			testConsol.JK_RL_NKDischargePort = "USPHL";
			testConsol.JK_RL_NKLoadPort = "AUSYD";
			testConsol.JK_MasterBillNum = "APLUMST052215";
			var transport = testConsol.Transports[0];
			transport.JW_VoyageFlight = "05225";
			transport.JW_ATD = new ZDateTime(new DateTime(2015, 07, 28));

			var permit = testConsol.CusEntryNums.AddNew();
			permit.CE_EntryType = "CRN";
			permit.CE_EntryNum = "AAAC3CX6C";
			permit.CE_ParentID = testConsol.PK;
			permit.CE_ParentTable = testConsol.TableName;
			Factory.Save();

			var shipment1 = AddAShipmentAndDeclarationToConsol(testConsol, "SHCRHOUSE1", 1, true);
			var consolWrapper = new FreightConsolWrapper(testConsol);
			Factory.Save();
			consolWrapper.RemoveAllESMLineNumbers();

			var outgoingMessage1ORG = Factory.New<CMRMessage>();
			outgoingMessage1ORG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1ORG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1ORG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1ORG.EM_MessageSubType = "ORG";
			outgoingMessage1ORG.EM_Status = "CLR";
			outgoingMessage1ORG.EM_SystemCreateTimeUtc = new ZDateTime(2015, 07, 28, 11, 22, 00);
			outgoingMessage1ORG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00027113/CMT2:2+9'LOC+107+9914N::95'TDT+20+++11'DTM+136:20150728:102'GIS+C:121:95'CNT+11:300'CNT+36:2'CNI+1'CNT+11:100'CNT+36:1'RFF+TL:EXLV'GID+1'RFF+HWB:SHCRHOUSE1'LOC+28+US::5'NAD+GO+++ACE TEST SUPPLIER HK'GID+1'FTX+AAA+++SHIP 1'UNT+19+1'";
			testConsol.Messages.Add(outgoingMessage1ORG);

			var responseMessage2CLR = Factory.New<CMRMessage>();
			responseMessage2CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage2CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage2CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage2CLR.EM_MessageSubType = "CLR";
			responseMessage2CLR.EM_Status = "RCV";
			responseMessage2CLR.EM_SystemCreateTimeUtc = new ZDateTime(2015, 07, 28, 11, 35, 00);
			responseMessage2CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+1BJ6 005F EHB5:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00027113/CMT2::002'RFF+ACW:ESM'RFF+AFM:9'RFF+AIZ:AAAC4CPR4'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'CNT+5:0001'UNT+14+000001'";
			testConsol.Messages.Add(responseMessage2CLR);

			wrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			AssertEquals("Wrapper.Lines", 1, wrapper.Lines.Length);
			Factory.Save();
			var line1 = wrapper.Lines[0];
			AssertEquals("LineActionCode will be blank by default for original or replacement message", "", line1.LineActionCode);
			AssertEquals("LineNumber", 1, line1.LineNumber);

			var outgoingMessage3CHG = Factory.New<CMRMessage>();
			outgoingMessage3CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage3CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage3CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage3CHG.EM_MessageSubType = "AMD";
			outgoingMessage3CHG.EM_Status = "SNT";
			outgoingMessage3CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 14, 39, 00);
			outgoingMessage3CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:5'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+26+1'";
			testConsol.Messages.Add(outgoingMessage3CHG);

			wrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			AssertEquals("Wrapper.Lines", 1, wrapper.Lines.Length);
			AssertEquals("LineActionCode should be '' for replace message", "", line1.LineActionCode);
		}

		public void TestLinesWhenChangeMessageIsRequired()
		{
			var testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_UniqueConsignRef = "C00026869";
			testConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			testConsol.JK_RL_NKDischargePort = "CATOR";
			testConsol.JK_RL_NKLoadPort = "AUSYD";
			testConsol.JK_MasterBillNum = "081-34234745";
			var transport = testConsol.Transports[0];
			transport.JW_VoyageFlight = "QF112";
			transport.JW_ATD = new ZDateTime(new DateTime(2014, 10, 15));

			var shipment1 = AddAShipmentAndDeclarationToConsol(testConsol, "S00047238", 1, true);
			var shipment2 = AddAShipmentAndDeclarationToConsol(testConsol, "S00047239", 2, true);

			var outgoingMessage1ORG = Factory.New<CMRMessage>();
			outgoingMessage1ORG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1ORG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1ORG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1ORG.EM_MessageSubType = "ORG";
			outgoingMessage1ORG.EM_Status = "REJ";
			outgoingMessage1ORG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 15, 11, 22, 00);
			outgoingMessage1ORG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT1:1+9'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+5+1'";
			testConsol.Messages.Add(outgoingMessage1ORG);

			var responseMessage1REJ = Factory.New<CMRMessage>();
			responseMessage1REJ.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1REJ.EM_MessageType = CMRMessage.CMRMessageTypes.CONTRL;
			responseMessage1REJ.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1REJ.EM_MessageSubType = "REJ";
			responseMessage1REJ.EM_Status = "RCV";
			responseMessage1REJ.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 15, 11, 23, 00);
			responseMessage1REJ.EM_MessageText = @"UNH+000001+CONTRL:D:3:UN'UCI+369314+AAA374M::AAA374M+AAA336C+4'UCM+1+CUSCAR:D:99B:UN+4+29'UNT+4+000001'";
			testConsol.Messages.Add(responseMessage1REJ);

			var outgoingMessage2ORG = Factory.New<CMRMessage>();
			outgoingMessage2ORG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage2ORG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage2ORG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2ORG.EM_MessageSubType = "ORG";
			outgoingMessage2ORG.EM_Status = "SNT";
			outgoingMessage2ORG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 15, 11, 34, 00);
			outgoingMessage2ORG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:2+9'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+25+1'";
			testConsol.Messages.Add(outgoingMessage2ORG);

			var responseMessage2CLR = Factory.New<CMRMessage>();
			responseMessage2CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage2CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage2CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage2CLR.EM_MessageSubType = "CLR";
			responseMessage2CLR.EM_Status = "RCV";
			responseMessage2CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 15, 11, 35, 00);
			responseMessage2CLR.EM_MessageText = @"UNH+000002+CUSRES:D:99B:UN'BGM+961:::ESMR+17A0 6I10 G214:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026869/CMT2::002'RFF+ACW:ESM'RFF+AFM:9'RFF+AIZ:AAAC3CX6C'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'CNT+5:0002'UNT+18+000002'";
			testConsol.Messages.Add(responseMessage2CLR);

			wrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			AssertEquals("Wrapper.Lines", 2, wrapper.Lines.Length);
			Factory.Save();
			var line1 = wrapper.Lines[0];
			var line2 = wrapper.Lines[1];
			AssertEquals("LineActionCode will be blank by default for original or replacement message", "", line1.LineActionCode);
			AssertEquals("LineNumber", 1, line1.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original or replacement message", "", line2.LineActionCode);
			AssertEquals("LineNumber", 2, line2.LineNumber);

			var permit = testConsol.CusEntryNums.AddNew();
			permit.CE_EntryType = "CRN";
			permit.CE_EntryNum = "AAAC3CX6C";
			permit.CE_ParentID = testConsol.PK;
			permit.CE_ParentTable = testConsol.TableName;
			Factory.Save();

			var outgoingMessage3CHG = Factory.New<CMRMessage>();
			outgoingMessage3CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage3CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage3CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage3CHG.EM_MessageSubType = "CHG";
			outgoingMessage3CHG.EM_Status = "SNT";
			outgoingMessage3CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 14, 39, 00);
			outgoingMessage3CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:3'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+26+1'";
			testConsol.Messages.Add(outgoingMessage3CHG);

			var responseMessage3REJ = Factory.New<CMRMessage>();
			responseMessage3REJ.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage3REJ.EM_MessageType = CMRMessage.CMRMessageTypes.CONTRL;
			responseMessage3REJ.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage3REJ.EM_MessageSubType = "REJ";
			responseMessage3REJ.EM_Status = "RCV";
			responseMessage3REJ.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 14, 39, 00);
			responseMessage3REJ.EM_MessageText = @"UNH+000001+CONTRL:D:3:UN'UCI+369358+AAA374M::AAA374M+AAA336C+4'UCM+1+CUSCAR:D:99B:UN+4+18'UCS+2+13'UNT+5+000001'";
			testConsol.Messages.Add(responseMessage3REJ);

			var outgoingMessage4CHG = Factory.New<CMRMessage>();
			outgoingMessage4CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage4CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage4CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage4CHG.EM_MessageSubType = "CHG";
			outgoingMessage4CHG.EM_Status = "SNT";
			outgoingMessage4CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 14, 59, 00);
			outgoingMessage4CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:4+4'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+6+1'";
			testConsol.Messages.Add(outgoingMessage4CHG);

			var responseMessage4REJ = Factory.New<CMRMessage>();
			responseMessage4REJ.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage4REJ.EM_MessageType = CMRMessage.CMRMessageTypes.CONTRL;
			responseMessage4REJ.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage4REJ.EM_MessageSubType = "REJ";
			responseMessage4REJ.EM_Status = "RCV";
			responseMessage4REJ.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 15, 00, 00);
			responseMessage4REJ.EM_MessageText = @"UNH+000001+CONTRL:D:3:UN'UCI+369359+AAA374M::AAA374M+AAA336C+4'UCM+1+CUSCAR:D:99B:UN+4+29'UNT+4+000001'";
			testConsol.Messages.Add(responseMessage4REJ);

			var outgoingMessage5CHG = Factory.New<CMRMessage>();
			outgoingMessage5CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage5CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage5CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage5CHG.EM_MessageSubType = "CHG";
			outgoingMessage5CHG.EM_Status = "SNT";
			outgoingMessage5CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 15, 07, 00);
			outgoingMessage5CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:5+4'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+26+1'";
			testConsol.Messages.Add(outgoingMessage5CHG);

			var responseMessage5CLR = Factory.New<CMRMessage>();
			responseMessage5CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage5CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage5CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage5CLR.EM_MessageSubType = "CLR";
			responseMessage5CLR.EM_Status = "RCV";
			responseMessage5CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 15, 08, 00);
			responseMessage5CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+1H49 13E7 GEG4:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026869/CMT2::005'RFF+ACW:ESM'RFF+AFM:4'RFF+AIZ:AAAC3CX6C'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'CNT+5:0002'UNT+18+000001'";
			testConsol.Messages.Add(responseMessage5CLR);

			var shipment3 = AddAShipmentAndDeclarationToConsol(testConsol, "S00047249", 3, false);
			wrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			Factory.Save();

			AssertEquals("Wrapper.Lines", 3, wrapper.Lines.Length);
			line1 = wrapper.Lines[0];
			line2 = wrapper.Lines[1];
			var line3 = wrapper.Lines[2];
			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line1.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 1, line1.LineNumber);

			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line2.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 2, line2.LineNumber);

			AssertEquals("LineActionCode should be 'I' for change message when line was not in the previous message, ie for new lines", "I", line3.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 3, line3.LineNumber);
		}

		public void TestLinesWhenChangeMessageDeleteAndInsertionIsRequired()
		{
			var testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_UniqueConsignRef = "C00026869";
			testConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			testConsol.JK_RL_NKDischargePort = "CATOR";
			testConsol.JK_RL_NKLoadPort = "AUSYD";
			testConsol.JK_MasterBillNum = "081-34234745";
			var transport = testConsol.Transports[0];
			transport.JW_VoyageFlight = "QF112";
			transport.JW_ATD = new ZDateTime(new DateTime(2014, 10, 15));

			var permit = testConsol.CusEntryNums.AddNew();
			permit.CE_EntryType = "CRN";
			permit.CE_EntryNum = "AAAC3CX6C";
			permit.CE_ParentID = testConsol.PK;
			permit.CE_ParentTable = testConsol.TableName;
			Factory.Save();

			var shipment1 = AddAShipmentAndDeclarationToConsol(testConsol, "S00047238", 1, true);
			var shipment2 = AddAShipmentAndDeclarationToConsol(testConsol, "S00047239", 2, true);
			var shipment3 = AddAShipmentAndDeclarationToConsol(testConsol, "S00047249", 3, true);

			var outgoingMessage1ORG = Factory.New<CMRMessage>();
			outgoingMessage1ORG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1ORG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1ORG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1ORG.EM_MessageSubType = "ORG";
			outgoingMessage1ORG.EM_Status = "REJ";
			outgoingMessage1ORG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 15, 11, 22, 00);
			outgoingMessage1ORG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT1:1+9'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+5+1'";
			testConsol.Messages.Add(outgoingMessage1ORG);

			var responseMessage1REJ = Factory.New<CMRMessage>();
			responseMessage1REJ.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1REJ.EM_MessageType = CMRMessage.CMRMessageTypes.CONTRL;
			responseMessage1REJ.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1REJ.EM_MessageSubType = "REJ";
			responseMessage1REJ.EM_Status = "RCV";
			responseMessage1REJ.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 15, 11, 23, 00);
			responseMessage1REJ.EM_MessageText = @"UNH+000001+CONTRL:D:3:UN'UCI+369314+AAA374M::AAA374M+AAA336C+4'UCM+1+CUSCAR:D:99B:UN+4+29'UNT+4+000001'";
			testConsol.Messages.Add(responseMessage1REJ);

			var outgoingMessage2ORG = Factory.New<CMRMessage>();
			outgoingMessage2ORG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage2ORG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage2ORG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2ORG.EM_MessageSubType = "ORG";
			outgoingMessage2ORG.EM_Status = "SNT";
			outgoingMessage2ORG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 15, 11, 34, 00);
			outgoingMessage2ORG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:2+9'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+25+1'";
			testConsol.Messages.Add(outgoingMessage2ORG);

			var responseMessage2CLR = Factory.New<CMRMessage>();
			responseMessage2CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage2CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage2CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage2CLR.EM_MessageSubType = "CLR";
			responseMessage2CLR.EM_Status = "RCV";
			responseMessage2CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 15, 11, 35, 00);
			responseMessage2CLR.EM_MessageText = @"UNH+000002+CUSRES:D:99B:UN'BGM+961:::ESMR+17A0 6I10 G214:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026869/CMT2::002'RFF+ACW:ESM'RFF+AFM:9'RFF+AIZ:AAAC3CX6C'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'CNT+5:0002'UNT+18+000002'";
			testConsol.Messages.Add(responseMessage2CLR);

			var outgoingMessage3CHG = Factory.New<CMRMessage>();
			outgoingMessage3CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage3CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage3CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage3CHG.EM_MessageSubType = "CHG";
			outgoingMessage3CHG.EM_Status = "SNT";
			outgoingMessage3CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 14, 39, 00);
			outgoingMessage3CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:3'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+26+1'";
			testConsol.Messages.Add(outgoingMessage3CHG);

			var responseMessage3REJ = Factory.New<CMRMessage>();
			responseMessage3REJ.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage3REJ.EM_MessageType = CMRMessage.CMRMessageTypes.CONTRL;
			responseMessage3REJ.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage3REJ.EM_MessageSubType = "REJ";
			responseMessage3REJ.EM_Status = "RCV";
			responseMessage3REJ.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 14, 39, 00);
			responseMessage3REJ.EM_MessageText = @"UNH+000001+CONTRL:D:3:UN'UCI+369358+AAA374M::AAA374M+AAA336C+4'UCM+1+CUSCAR:D:99B:UN+4+18'UCS+2+13'UNT+5+000001'";
			testConsol.Messages.Add(responseMessage3REJ);

			var outgoingMessage4CHG = Factory.New<CMRMessage>();
			outgoingMessage4CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage4CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage4CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage4CHG.EM_MessageSubType = "CHG";
			outgoingMessage4CHG.EM_Status = "SNT";
			outgoingMessage4CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 14, 59, 00);
			outgoingMessage4CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:4+4'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+6+1'";
			testConsol.Messages.Add(outgoingMessage4CHG);

			var responseMessage4REJ = Factory.New<CMRMessage>();
			responseMessage4REJ.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage4REJ.EM_MessageType = CMRMessage.CMRMessageTypes.CONTRL;
			responseMessage4REJ.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage4REJ.EM_MessageSubType = "REJ";
			responseMessage4REJ.EM_Status = "RCV";
			responseMessage4REJ.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 15, 00, 00);
			responseMessage4REJ.EM_MessageText = @"UNH+000001+CONTRL:D:3:UN'UCI+369359+AAA374M::AAA374M+AAA336C+4'UCM+1+CUSCAR:D:99B:UN+4+29'UNT+4+000001'";
			testConsol.Messages.Add(responseMessage4REJ);

			var outgoingMessage5CHG = Factory.New<CMRMessage>();
			outgoingMessage5CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage5CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage5CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage5CHG.EM_MessageSubType = "CHG";
			outgoingMessage5CHG.EM_Status = "SNT";
			outgoingMessage5CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 15, 07, 00);
			outgoingMessage5CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:5+4'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+26+1'";
			testConsol.Messages.Add(outgoingMessage5CHG);

			var responseMessage5CLR = Factory.New<CMRMessage>();
			responseMessage5CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage5CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage5CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage5CLR.EM_MessageSubType = "CLR";
			responseMessage5CLR.EM_Status = "RCV";
			responseMessage5CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 15, 08, 00);
			responseMessage5CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+1H49 13E7 GEG4:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026869/CMT2::005'RFF+ACW:ESM'RFF+AFM:4'RFF+AIZ:AAAC3CX6C'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'CNT+5:0002'UNT+18+000001'";
			testConsol.Messages.Add(responseMessage5CLR);

			var outgoingMessage6CHG = Factory.New<CMRMessage>();
			outgoingMessage6CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage6CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage6CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage6CHG.EM_MessageSubType = "CHG";
			outgoingMessage6CHG.EM_Status = "SNT";
			outgoingMessage6CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 15, 28, 00);
			outgoingMessage6CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:6+4'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:9'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'CNI+3+:::I'CNT+11:5'RFF+TL:EXLV'GID+1'RFF+HWB:S00047249'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++NEWSPAPER PRINT'UNT+35+1'";
			testConsol.Messages.Add(outgoingMessage6CHG);

			var responseMessage6CLR = Factory.New<CMRMessage>();
			responseMessage6CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage6CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage6CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage6CLR.EM_MessageSubType = "CLR";
			responseMessage6CLR.EM_Status = "RCV";
			responseMessage6CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 15, 30, 00);
			responseMessage6CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+2E6I 2442 GEG4:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026869/CMT2::006'RFF+ACW:ESM'RFF+AFM:4'RFF+AIZ:AAAC3CX6C'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0003'FTX+AHN+++CLEAR'CNT+5:0003'UNT+22+000001'";
			testConsol.Messages.Add(responseMessage6CLR);

			//Delete 1 shipment, Add three new shipments
			testConsol.Shipments.Remove(shipment2);
			var shipment4 = AddAShipmentAndDeclarationToConsol(testConsol, "S00047250", 4, false);
			var shipment5 = AddAShipmentAndDeclarationToConsol(testConsol, "S00047255", 5, false);
			var shipment6 = AddAShipmentAndDeclarationToConsol(testConsol, "S00047258", 6, false);
			Factory.Save();

			wrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			AssertEquals("Wrapper.Lines", 6, wrapper.Lines.Length);
			var wrappedLine1 = wrapper.Lines[0];
			var wrappedLine2 = wrapper.Lines[1];
			var wrappedLine3 = wrapper.Lines[2];
			var wrappedLine4 = wrapper.Lines[3];
			var wrappedLine5 = wrapper.Lines[4];
			var wrappedLine6 = wrapper.Lines[5];
			AssertEquals("First wrapped Line should be for Housebill S00047238", "S00047238", wrappedLine1.HouseBillNumber);
			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", wrappedLine1.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 1, wrappedLine1.LineNumber);

			AssertEquals("Second wrapped Line should be for the deleted Housebill S00047239", "", wrappedLine2.HouseBillNumber);
			AssertEquals("LineActionCode should be 'D' for change message when line was in the previous message but has now been removed, ie for deleted lines", "D", wrappedLine2.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 2, wrappedLine2.LineNumber);

			AssertEquals("Third wrapped Line should be for Housebill S00047249", "S00047249", wrappedLine3.HouseBillNumber);
			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", wrappedLine3.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 3, wrappedLine3.LineNumber);

			AssertEquals("Fourth wrapped Line should be for Housebill S00047250", "S00047250", wrappedLine4.HouseBillNumber);
			AssertEquals("LineActionCode should be 'I' for change message when line was not in the previous message, ie for new lines", "I", wrappedLine4.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 4, wrappedLine4.LineNumber);

			AssertEquals("Fifth wrapped Line should be for Housebill S00047255", "S00047255", wrappedLine5.HouseBillNumber);
			AssertEquals("LineActionCode should be 'I' for change message when line was not in the previous message, ie for new lines", "I", wrappedLine5.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 5, wrappedLine5.LineNumber);

			AssertEquals("Sixth wrapped Line should be for Housebill S00047258", "S00047258", wrappedLine6.HouseBillNumber);
			AssertEquals("LineActionCode should be 'I' for change message when line was not in the previous message, ie for new lines", "I", wrappedLine6.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 6, wrappedLine6.LineNumber);
		}

		public void TestLinesWhenHBNumbersAreNotSameAsJobUniqueIDs()
		{
			var testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_UniqueConsignRef = "C00026899";
			testConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			testConsol.JK_RL_NKDischargePort = "NZAKL";
			testConsol.JK_RL_NKLoadPort = "AUSYD";
			testConsol.JK_MasterBillNum = "081-45238340";
			var transport = testConsol.Transports[0];
			transport.JW_VoyageFlight = "QF118";
			transport.JW_ATD = new ZDateTime(new DateTime(2014, 10, 28));

			var shipment1 = AddAShipmentAndDeclarationToConsol(testConsol, "58247-1", 1, true);
			var shipment2 = AddAShipmentAndDeclarationToConsol(testConsol, "45423-2", 2, true);
			var shipment3 = AddAShipmentAndDeclarationToConsol(testConsol, "85923-3", 3, true);

			var outgoingMessage1ORG = Factory.New<CMRMessage>();
			outgoingMessage1ORG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1ORG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1ORG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1ORG.EM_MessageSubType = "ORG";
			outgoingMessage1ORG.EM_Status = "CLR";
			outgoingMessage1ORG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 28, 00, 15, 00);
			outgoingMessage1ORG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026899/CMT1:1+9'TDT+20+++6'DTM+136:20141028:102'GIS+C:121:95'CNT+11:13'CNI+1'CNT+11:2'RFF+TL:EXLV'GID+1'RFF+HWB:58247-1'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++GOODS'CNI+2'CNT+11:4'RFF+TL:EXLV'GID+1'RFF+HWB:45423-2'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++TESTING CHANGE MSSG DELETIONS'CNI+3'CNT+11:7'RFF+TL:EXLV'GID+1'RFF+HWB:85923-3'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PRINTED DOCUMENTS'UNT+34+1'";
			testConsol.Messages.Add(outgoingMessage1ORG);

			var responseMessage1CLR = Factory.New<CMRMessage>();
			responseMessage1CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1CLR.EM_MessageSubType = "CLR";
			responseMessage1CLR.EM_Status = "RCV";
			responseMessage1CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 28, 00, 16, 00);
			responseMessage1CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+10CD DF50 DHBE:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026899/CMT1::001'RFF+ACW:ESM'RFF+AFM:9'RFF+AIZ:AAAC3EMXR'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0003'FTX+AHN+++CLEAR'CNT+5:0003'UNT+22+000001'";
			testConsol.Messages.Add(responseMessage1CLR);

			wrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			AssertEquals("Wrapper.Lines", 3, wrapper.Lines.Length);
			var line1 = wrapper.Lines[0];
			var line2 = wrapper.Lines[1];
			var line3 = wrapper.Lines[2];
			AssertEquals("LineActionCode will be blank by default for original or replacement message", "", line1.LineActionCode);
			AssertEquals("LineNumber", 1, line1.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original or replacement message", "", line2.LineActionCode);
			AssertEquals("LineNumber", 2, line2.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original or replacement message", "", line3.LineActionCode);
			AssertEquals("LineNumber", 3, line3.LineNumber);

			var permit = testConsol.CusEntryNums.AddNew();
			permit.CE_EntryType = "CRN";
			permit.CE_EntryNum = "AAAC3EMXR";
			permit.CE_ParentID = testConsol.PK;
			permit.CE_ParentTable = testConsol.TableName;
			Factory.Save();

			var outgoingMessage2CHG = Factory.New<CMRMessage>();
			outgoingMessage2CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage2CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage2CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2CHG.EM_MessageSubType = "CHG";
			outgoingMessage2CHG.EM_Status = "SNT";
			outgoingMessage2CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 28, 00, 40, 00);
			outgoingMessage2CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026899/CMT1:2+4'RFF+AIZ:AAAC3EMXR'TDT+20+++6'DTM+136:20141028:102'GIS+C:121:95'CNT+11:22'CNI+1+:::I'CNT+11:2'RFF+TL:EXLV'GID+1'RFF+HWB:58247-1'LOC+28+NZ::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++GOODS'CNI+2+:::I'CNT+11:4'RFF+TL:EXLV'GID+1'RFF+HWB:45423-2'LOC+28+NZ::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++TESTING CHANGE MSSG DELETIONS'CNI+3+:::I'CNT+11:7'RFF+TL:EXLV'GID+1'RFF+HWB:85923-3'LOC+28+NZ::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PRINTED DOCUMENTS'CNI+4+:::I'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:49238-4'LOC+28+NZ::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PAPER'CNI+5+:::I'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:68478-5'LOC+28+NZ::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++NEWSPRINT'CNI+6+:::I'CNT+11:5'RFF+TL:EXLV'GID+1'RFF+HWB:43523-6'LOC+28+NZ::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PAPER REAMS'CNI+1+:::D'CNI+2+:::D'CNI+3+:::D'UNT+65+1'";
			testConsol.Messages.Add(outgoingMessage2CHG);

			var responseMessage2REJ = Factory.New<CMRMessage>();
			responseMessage2REJ.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage2REJ.EM_MessageType = CMRMessage.CMRMessageTypes.CONTRL;
			responseMessage2REJ.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage2REJ.EM_MessageSubType = "REJ";
			responseMessage2REJ.EM_Status = "RCV";
			responseMessage2REJ.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 28, 00, 41, 00);
			responseMessage2REJ.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+17EE F008 G7BE:001+11'FTX+AHN+++REJECTED:THE TRANSACTION HAS BEEN REJECTED DUE TO ERRORS. PLEASE CORRECT AND RE-SEND THE MESSAGE.'NAD+MR+41065894724::95'RFF+ABO:C00026899/CMT1::002'RFF+ACW:ESM'RFF+AFM:4'ERP+::0001'ERC+XM1047::95'FTX+AAO+++LINE NO =000000000000001 ?: ATTEMPT TO INSERT A LINE THAT ALREADY EXISTS'CNT+55:01'UNT+12+000001'";
			testConsol.Messages.Add(responseMessage2REJ);

			var outgoingMessage3CHG = Factory.New<CMRMessage>();
			outgoingMessage3CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage3CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage3CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage3CHG.EM_MessageSubType = "CHG";
			outgoingMessage3CHG.EM_Status = "SNT";
			outgoingMessage3CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 28, 01, 11, 00);
			outgoingMessage3CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026899/CMT1:3+4'RFF+AIZ:AAAC3EMXR'TDT+20+++6'DTM+136:20141028:102'GIS+C:121:95'CNT+11:22'CNI+1+:::I'CNT+11:2'RFF+TL:EXLV'GID+1'RFF+HWB:58247-1'LOC+28+NZ::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++GOODS'CNI+2+:::I'CNT+11:4'RFF+TL:EXLV'GID+1'RFF+HWB:45423-2'LOC+28+NZ::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++TESTING CHANGE MSSG DELETIONS'CNI+3+:::I'CNT+11:7'RFF+TL:EXLV'GID+1'RFF+HWB:85923-3'LOC+28+NZ::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PRINTED DOCUMENTS'CNI+4+:::I'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:49238-4'LOC+28+NZ::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PAPER'CNI+5+:::I'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:68478-5'LOC+28+NZ::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++NEWSPRINT'CNI+6+:::I'CNT+11:5'RFF+TL:EXLV'GID+1'RFF+HWB:43523-6'LOC+28+NZ::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PAPER REAMS'UNT+62+1'";
			testConsol.Messages.Add(outgoingMessage3CHG);

			var responseMessage3REJ = Factory.New<CMRMessage>();
			responseMessage3REJ.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage3REJ.EM_MessageType = CMRMessage.CMRMessageTypes.CONTRL;
			responseMessage3REJ.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage3REJ.EM_MessageSubType = "REJ";
			responseMessage3REJ.EM_Status = "RCV";
			responseMessage3REJ.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 28, 01, 12, 00);
			responseMessage3REJ.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+3JG0 B8I9 A2BE:001+11'FTX+AHN+++REJECTED:THE TRANSACTION HAS BEEN REJECTED DUE TO ERRORS. PLEASE CORRECT AND RE-SEND THE MESSAGE.'NAD+MR+41065894724::95'RFF+ABO:C00026899/CMT1::003'RFF+ACW:ESM'RFF+AFM:4'ERP+::0001'ERC+XM1047::95'FTX+AAO+++LINE NO =000000000000001 ?: ATTEMPT TO INSERT A LINE THAT ALREADY EXISTS'CNT+55:01'UNT+12+000001'";
			testConsol.Messages.Add(responseMessage3REJ);

			var shipment4 = AddAShipmentAndDeclarationToConsol(testConsol, "49238-4", 4, false);
			var shipment5 = AddAShipmentAndDeclarationToConsol(testConsol, "68478-5", 5, false);
			var shipment6 = AddAShipmentAndDeclarationToConsol(testConsol, "43523-6", 6, false);
			Factory.Save();

			wrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			AssertEquals("Wrapper.Lines", 6, wrapper.Lines.Length);
			line1 = wrapper.Lines[0];
			line2 = wrapper.Lines[1];
			line3 = wrapper.Lines[2];
			var line4 = wrapper.Lines[3];
			var line5 = wrapper.Lines[4];
			var line6 = wrapper.Lines[5];
			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line1.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 1, line1.LineNumber);

			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line2.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 2, line2.LineNumber);

			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line3.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 3, line3.LineNumber);

			AssertEquals("LineActionCode should be 'I' for change message when line was not in the previous message, ie for new lines", "I", line4.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 4, line4.LineNumber);

			AssertEquals("LineActionCode should be 'I' for change message when line was not in the previous message, ie for new lines", "I", line5.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 5, line5.LineNumber);

			AssertEquals("LineActionCode should be 'I' for change message when line was not in the previous message, ie for new lines", "I", line6.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 6, line6.LineNumber);

			var outgoingMessage4CHG = Factory.New<CMRMessage>();
			outgoingMessage4CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage4CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage4CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage4CHG.EM_MessageSubType = "CHG";
			outgoingMessage4CHG.EM_Status = "SNT";
			outgoingMessage4CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 28, 01, 33, 00);
			outgoingMessage4CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026899/CMT1:4+4'RFF+AIZ:AAAC3EMXR'TDT+20+++6'DTM+136:20141028:102'GIS+C:121:95'CNT+11:22'CNI+1+:::A'CNT+11:2'RFF+TL:EXLV'GID+1'RFF+HWB:58247-1'LOC+28+NZ::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++GOODS'CNI+2+:::A'CNT+11:4'RFF+TL:EXLV'GID+1'RFF+HWB:45423-2'LOC+28+NZ::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++TESTING CHANGE MSSG DELETIONS'CNI+3+:::A'CNT+11:7'RFF+TL:EXLV'GID+1'RFF+HWB:85923-3'LOC+28+NZ::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PRINTED DOCUMENTS'CNI+4+:::I'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:49238-4'LOC+28+NZ::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PAPER'CNI+5+:::I'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:68478-5'LOC+28+NZ::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++NEWSPRINT'CNI+6+:::I'CNT+11:5'RFF+TL:EXLV'GID+1'RFF+HWB:43523-6'LOC+28+NZ::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PAPER REAMS'UNT+62+1'";
			testConsol.Messages.Add(outgoingMessage4CHG);

			var responseMessage4CLR = Factory.New<CMRMessage>();
			responseMessage4CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage4CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage4CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage4CLR.EM_MessageSubType = "CLR";
			responseMessage4CLR.EM_Status = "RCV";
			responseMessage4CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 28, 01, 34, 00);
			responseMessage4CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+441D 948H 02BE:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026899/CMT1::004'RFF+ACW:ESM'RFF+AFM:4'RFF+AIZ:AAAC3EMXR'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0003'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0004'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0005'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0006'FTX+AHN+++CLEAR'CNT+5:0006'UNT+34+000001'";
			testConsol.Messages.Add(responseMessage4CLR);

			var shipment = new FreightShipmentWrapper(shipment4, testConsol);
			shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			shipment = new FreightShipmentWrapper(shipment5, testConsol);
			shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			shipment = new FreightShipmentWrapper(shipment6, testConsol);
			shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();

			var shipment7 = AddAShipmentAndDeclarationToConsol(testConsol, "54923-7", 7, false);
			testConsol.Shipments.Remove(shipment2);
			testConsol.Shipments.Remove(shipment3);
			testConsol.Shipments.Remove(shipment4);
			Factory.Save();

			wrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			AssertEquals("Wrapper.Lines", 7, wrapper.Lines.Length);
			line1 = wrapper.Lines[0];
			line2 = wrapper.Lines[1];
			line3 = wrapper.Lines[2];
			line4 = wrapper.Lines[3];
			line5 = wrapper.Lines[4];
			line6 = wrapper.Lines[5];
			var line7 = wrapper.Lines[6];

			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line1.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 1, line1.LineNumber);

			AssertEquals("LineActionCode should be 'D' for change message when line was in the previous message but is no longer to be sent, ie for deleted lines", "D", line2.LineActionCode);
			AssertEquals("LineNumber should match the line number of the consignment being deleted must match line number recorded at Customs", 2, line2.LineNumber);

			AssertEquals("LineActionCode should be 'D' for change message when line was in the previous message but is no longer to be sent, ie for deleted lines", "D", line3.LineActionCode);
			AssertEquals("LineNumber should match the line number of the consignment being deleted must match line number recorded at Customs", 3, line3.LineNumber);

			AssertEquals("LineActionCode should be 'D' for change message when line was in the previous message but is no longer to be sent, ie for deleted lines", "D", line4.LineActionCode);
			AssertEquals("LineNumber should match the line number of the consignment being deleted must match line number recorded at Customs", 4, line4.LineNumber);

			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line5.LineActionCode);
			AssertEquals("LineNumber for consignment must match line number recorded at Customs", 5, line5.LineNumber);

			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line6.LineActionCode);
			AssertEquals("LineNumber for consignment must match line number recorded at Customs", 6, line6.LineNumber);

			AssertEquals("LineActionCode should be 'I' for change message when line was not in the previous message, ie for new lines", "I", line7.LineActionCode);
			AssertEquals("LineNumber for consignment must match line number recorded at Customs", 7, line7.LineNumber);

			shipment = new FreightShipmentWrapper(shipment2, testConsol);
			shipment.ShipmentManifestLineSequence.CY_Code = CustomsManifestLineSequence.DataCodes.DeletedLineNumber;
			shipment = new FreightShipmentWrapper(shipment3, testConsol);
			shipment.ShipmentManifestLineSequence.CY_Code = CustomsManifestLineSequence.DataCodes.DeletedLineNumber;
			shipment = new FreightShipmentWrapper(shipment4, testConsol);
			shipment.ShipmentManifestLineSequence.CY_Code = CustomsManifestLineSequence.DataCodes.DeletedLineNumber;
			Factory.Save();

			var outgoingMessage5CHG = Factory.New<CMRMessage>();
			outgoingMessage5CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage5CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage5CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage5CHG.EM_MessageSubType = "CHG";
			outgoingMessage5CHG.EM_Status = "SNT";
			outgoingMessage5CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 28, 01, 45, 00);
			outgoingMessage5CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026899/CMT1:5+4'RFF+AIZ:AAAC3EMXR'TDT+20+++6'DTM+136:20141028:102'GIS+C:121:95'CNT+11:14'CNI+1+:::A'CNT+11:2'RFF+TL:EXLV'GID+1'RFF+HWB:58247-1'LOC+28+NZ::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++GOODS'CNI+5+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:68478-5'LOC+28+NZ::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++NEWSPRINT'CNI+6+:::A'CNT+11:5'RFF+TL:EXLV'GID+1'RFF+HWB:43523-6'LOC+28+NZ::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PAPER REAMS'CNI+7+:::I'CNT+11:4'RFF+TL:EXLV'GID+1'RFF+HWB:54923-7'LOC+28+NZ::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PAPER PRODUCTS'CNI+2+:::D'CNI+3+:::D'CNI+4+:::D'UNT+47+1'";
			testConsol.Messages.Add(outgoingMessage5CHG);

			var responseMessage5CLR = Factory.New<CMRMessage>();
			responseMessage5CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage5CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage5CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage5CLR.EM_MessageSubType = "CLR";
			responseMessage5CLR.EM_Status = "RCV";
			responseMessage5CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 28, 01, 46, 00);
			responseMessage5CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+36H9 B3AF 2CBE:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026899/CMT1::005'RFF+ACW:ESM'RFF+AFM:4'RFF+AIZ:AAAC3EMXR'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0005'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0006'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0007'FTX+AHN+++CLEAR'CNT+5:0004'UNT+26+000001'";
			testConsol.Messages.Add(responseMessage5CLR);

			testConsol.Shipments.Remove(shipment5);
			shipment = new FreightShipmentWrapper(shipment7, testConsol);
			shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			Factory.Save();

			wrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			AssertEquals("Wrapper.Lines", 4, wrapper.Lines.Length);
			line1 = wrapper.Lines[0];
			line2 = wrapper.Lines[1];
			line3 = wrapper.Lines[2];
			line4 = wrapper.Lines[3];
			AssertEquals("LineActionCode should be 'A' for change message when line was in previous cleared message", "A", line1.LineActionCode);
			AssertEquals("LineNumber for consignment must match line number recorded at Customs", 1, line1.LineNumber);

			AssertEquals("LineActionCode should be 'D' for change message when line was in the previous message but is no longer to be sent, ie for deleted lines", "D", line2.LineActionCode);
			AssertEquals("LineNumber should match the line number of the consignment being deleted must match line number recorded at Customs", 5, line2.LineNumber);

			AssertEquals("LineActionCode should be 'A' for change message when line was in previous cleared message", "A", line3.LineActionCode);
			AssertEquals("LineNumber for consignment must match line number recorded at Customs", 6, line3.LineNumber);

			AssertEquals("LineActionCode should be 'A' for change message when line was in previous cleared message", "A", line4.LineActionCode);
			AssertEquals("LineNumber for consignment must match line number recorded at Customs", 7, line4.LineNumber);
		}

		public void TestLinesWhenColoadHousebillsUsed()
		{
			var testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_UniqueConsignRef = "C00026869";
			testConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			testConsol.JK_RL_NKDischargePort = "CATOR";
			testConsol.JK_RL_NKLoadPort = "AUSYD";
			testConsol.JK_MasterBillNum = "081-34234745";
			var transport = testConsol.Transports[0];
			transport.JW_VoyageFlight = "QF112";
			transport.JW_ATD = new ZDateTime(new DateTime(2014, 10, 15));

			var permit = testConsol.CusEntryNums.AddNew();
			permit.CE_EntryType = "CRN";
			permit.CE_EntryNum = "AAAC3CX6C";
			permit.CE_ParentID = testConsol.PK;
			permit.CE_ParentTable = testConsol.TableName;
			Factory.Save();

			var shipment1 = AddAShipmentAndDeclarationToConsol(testConsol, "S00047238", 1, true);
			var shipment2 = AddAShipmentAndDeclarationToConsol(testConsol, "S00047239", 2, true);
			var shipment3 = AddAColoadShipmentAndDeclarationToConsol(testConsol, "S00047246", 3, true);

			var outgoingMessage1ORG = Factory.New<CMRMessage>();
			outgoingMessage1ORG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1ORG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1ORG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1ORG.EM_MessageSubType = "ORG";
			outgoingMessage1ORG.EM_Status = "REJ";
			outgoingMessage1ORG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 15, 11, 22, 00);
			outgoingMessage1ORG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT1:1+9'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+5+1'";
			testConsol.Messages.Add(outgoingMessage1ORG);

			var responseMessage1REJ = Factory.New<CMRMessage>();
			responseMessage1REJ.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1REJ.EM_MessageType = CMRMessage.CMRMessageTypes.CONTRL;
			responseMessage1REJ.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1REJ.EM_MessageSubType = "REJ";
			responseMessage1REJ.EM_Status = "RCV";
			responseMessage1REJ.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 15, 11, 23, 00);
			responseMessage1REJ.EM_MessageText = @"UNH+000001+CONTRL:D:3:UN'UCI+369314+AAA374M::AAA374M+AAA336C+4'UCM+1+CUSCAR:D:99B:UN+4+29'UNT+4+000001'";
			testConsol.Messages.Add(responseMessage1REJ);

			var outgoingMessage2ORG = Factory.New<CMRMessage>();
			outgoingMessage2ORG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage2ORG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage2ORG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2ORG.EM_MessageSubType = "ORG";
			outgoingMessage2ORG.EM_Status = "SNT";
			outgoingMessage2ORG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 15, 11, 34, 00);
			outgoingMessage2ORG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:2+9'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+25+1'";
			testConsol.Messages.Add(outgoingMessage2ORG);

			var responseMessage2CLR = Factory.New<CMRMessage>();
			responseMessage2CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage2CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage2CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage2CLR.EM_MessageSubType = "CLR";
			responseMessage2CLR.EM_Status = "RCV";
			responseMessage2CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 15, 11, 35, 00);
			responseMessage2CLR.EM_MessageText = @"UNH+000002+CUSRES:D:99B:UN'BGM+961:::ESMR+17A0 6I10 G214:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026869/CMT2::002'RFF+ACW:ESM'RFF+AFM:9'RFF+AIZ:AAAC3CX6C'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'CNT+5:0002'UNT+18+000002'";
			testConsol.Messages.Add(responseMessage2CLR);

			AssertEquals("Shipments linked to Consol - 3 shipments 1 CoLoad", 4, testConsol.Shipments.Count);
			wrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			AssertEquals("Wrapper.Lines should only have shipments going in the ESM messages", 3, wrapper.Lines.Length);
			Factory.Save();
			var line1 = wrapper.Lines[0];
			var line2 = wrapper.Lines[1];
			var line3 = wrapper.Lines[2];
			AssertEquals("LineActionCode will be 'A' for original line in change message", "A", line1.LineActionCode);
			AssertEquals("LineNumber", 1, line1.LineNumber);

			AssertEquals("LineActionCode will be 'A' for original line in change message", "A", line2.LineActionCode);
			AssertEquals("LineNumber", 2, line2.LineNumber);

			AssertEquals("LineActionCode will be 'A' for original line in change message", "A", line3.LineActionCode);
			AssertEquals("LineNumber", 3, line3.LineNumber);

			var outgoingMessage3CHG = Factory.New<CMRMessage>();
			outgoingMessage3CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage3CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage3CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage3CHG.EM_MessageSubType = "CHG";
			outgoingMessage3CHG.EM_Status = "SNT";
			outgoingMessage3CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 14, 39, 00);
			outgoingMessage3CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:3'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+26+1'";
			testConsol.Messages.Add(outgoingMessage3CHG);

			var responseMessage3REJ = Factory.New<CMRMessage>();
			responseMessage3REJ.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage3REJ.EM_MessageType = CMRMessage.CMRMessageTypes.CONTRL;
			responseMessage3REJ.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage3REJ.EM_MessageSubType = "REJ";
			responseMessage3REJ.EM_Status = "RCV";
			responseMessage3REJ.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 14, 39, 00);
			responseMessage3REJ.EM_MessageText = @"UNH+000001+CONTRL:D:3:UN'UCI+369358+AAA374M::AAA374M+AAA336C+4'UCM+1+CUSCAR:D:99B:UN+4+18'UCS+2+13'UNT+5+000001'";
			testConsol.Messages.Add(responseMessage3REJ);

			var outgoingMessage4CHG = Factory.New<CMRMessage>();
			outgoingMessage4CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage4CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage4CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage4CHG.EM_MessageSubType = "CHG";
			outgoingMessage4CHG.EM_Status = "SNT";
			outgoingMessage4CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 14, 59, 00);
			outgoingMessage4CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:4+4'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+6+1'";
			testConsol.Messages.Add(outgoingMessage4CHG);

			var responseMessage4REJ = Factory.New<CMRMessage>();
			responseMessage4REJ.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage4REJ.EM_MessageType = CMRMessage.CMRMessageTypes.CONTRL;
			responseMessage4REJ.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage4REJ.EM_MessageSubType = "REJ";
			responseMessage4REJ.EM_Status = "RCV";
			responseMessage4REJ.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 15, 00, 00);
			responseMessage4REJ.EM_MessageText = @"UNH+000001+CONTRL:D:3:UN'UCI+369359+AAA374M::AAA374M+AAA336C+4'UCM+1+CUSCAR:D:99B:UN+4+29'UNT+4+000001'";
			testConsol.Messages.Add(responseMessage4REJ);

			var outgoingMessage5CHG = Factory.New<CMRMessage>();
			outgoingMessage5CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage5CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage5CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage5CHG.EM_MessageSubType = "CHG";
			outgoingMessage5CHG.EM_Status = "SNT";
			outgoingMessage5CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 15, 07, 00);
			outgoingMessage5CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:5+4'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+26+1'";
			testConsol.Messages.Add(outgoingMessage5CHG);

			var responseMessage5CLR = Factory.New<CMRMessage>();
			responseMessage5CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage5CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage5CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage5CLR.EM_MessageSubType = "CLR";
			responseMessage5CLR.EM_Status = "RCV";
			responseMessage5CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 20, 15, 08, 00);
			responseMessage5CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+1H49 13E7 GEG4:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026869/CMT2::005'RFF+ACW:ESM'RFF+AFM:4'RFF+AIZ:AAAC3CX6C'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'CNT+5:0002'UNT+18+000001'";
			testConsol.Messages.Add(responseMessage5CLR);

			var shipment4 = AddAShipmentAndDeclarationToConsol(testConsol, "S00047249", 4, false);
			wrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			Factory.Save();

			AssertEquals("Shipments linked to Consol should now be 5 - 4 shipments 1 CoLoad", 5, testConsol.Shipments.Count);
			AssertEquals("Wrapper.Lines", 4, wrapper.Lines.Length);
			line1 = wrapper.Lines[0];
			line2 = wrapper.Lines[1];
			line3 = wrapper.Lines[2];
			var line4 = wrapper.Lines[3];
			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line1.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 1, line1.LineNumber);

			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line2.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 2, line2.LineNumber);

			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line3.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 3, line3.LineNumber);

			AssertEquals("LineActionCode should be 'I' for change message when line was not in the previous message, ie for new lines", "I", line4.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 4, line4.LineNumber);
		}

		public void TestHLVConsignmentLinesIncluded()
		{
			var testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_UniqueConsignRef = "C00026899";
			testConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			testConsol.JK_RL_NKDischargePort = "NZAKL";
			testConsol.JK_RL_NKLoadPort = "AUSYD";
			testConsol.JK_MasterBillNum = "081-45238340";
			var transport = testConsol.Transports[0];
			transport.JW_VoyageFlight = "QF118";
			transport.JW_ATD = new ZDateTime(new DateTime(2014, 10, 28));

			AddAShipmentAndDeclarationToConsol(testConsol, "58247-1", 1, false);
			AddAShipmentAndDeclarationToConsol(testConsol, "45423-2", 2, false);
			AddAShipmentAndDeclarationToConsol(testConsol, "85923-3", 3, false);
			AddHVLShipmentToConsol(testConsol, "HLS-001");

			Factory.Save();

			var outgoingMessage1ORG = Factory.New<CMRMessage>();
			outgoingMessage1ORG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1ORG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1ORG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1ORG.EM_MessageSubType = "ORG";
			outgoingMessage1ORG.EM_Status = "CLR";
			outgoingMessage1ORG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 28, 00, 15, 00);
			outgoingMessage1ORG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026899/CMT1:1+9'TDT+20+++6'DTM+136:20141028:102'GIS+C:121:95'CNT+11:13'CNI+1'CNT+11:2'RFF+TL:EXLV'GID+1'RFF+HWB:58247-1'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++GOODS'CNI+2'CNT+11:4'RFF+TL:EXLV'GID+1'RFF+HWB:45423-2'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++TESTING CHANGE MSSG DELETIONS'CNI+3'CNT+11:7'RFF+TL:EXLV'GID+1'RFF+HWB:85923-3'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PRINTED DOCUMENTS'UNT+34+1'";
			testConsol.Messages.Add(outgoingMessage1ORG);

			var responseMessage1CLR = Factory.New<CMRMessage>();
			responseMessage1CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1CLR.EM_MessageSubType = "CLR";
			responseMessage1CLR.EM_Status = "RCV";
			responseMessage1CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 28, 00, 16, 00);
			responseMessage1CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+10CD DF50 DHBE:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026899/CMT1::001'RFF+ACW:ESM'RFF+AFM:9'RFF+AIZ:AAAC3EMXR'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0003'FTX+AHN+++CLEAR'CNT+5:0003'UNT+22+000001'";
			testConsol.Messages.Add(responseMessage1CLR);

			wrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			AssertEquals("Wrapper.Lines - should be 3 shipments plus 4 HVLV consignments from HLV shipment", 7, wrapper.Lines.Length);
			var line1 = wrapper.Lines[0];
			var line2 = wrapper.Lines[1];
			var line3 = wrapper.Lines[2];
			var line4 = wrapper.Lines[3];
			var line5 = wrapper.Lines[4];
			var line6 = wrapper.Lines[5];
			var line7 = wrapper.Lines[6];
			AssertEquals("LineActionCode will be blank by default for original", "", line1.LineActionCode);
			AssertEquals("ESM LineNumber", 1, line1.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line2.LineActionCode);
			AssertEquals("ESM LineNumber", 2, line2.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line3.LineActionCode);
			AssertEquals("ESM LineNumber", 3, line3.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line4.LineActionCode);
			AssertEquals("ESM LineNumber", 4, line4.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line5.LineActionCode);
			AssertEquals("ESM LineNumber", 5, line5.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line6.LineActionCode);
			AssertEquals("ESM LineNumber", 6, line6.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line7.LineActionCode);
			AssertEquals("ESM LineNumber", 7, line7.LineNumber);

			UpdateLinesToManifested(testConsol);
			var permit = testConsol.CusEntryNums.AddNew();
			permit.CE_EntryType = "CRN";
			permit.CE_EntryNum = "AAAC3EMXR";
			permit.CE_ParentID = testConsol.PK;
			permit.CE_ParentTable = testConsol.TableName;
			Factory.Save();

			AddAShipmentAndDeclarationToConsol(testConsol, "68478-5", 8, false);
			AddAShipmentAndDeclarationToConsol(testConsol, "43523-6", 9, false);
			Factory.Save();

			wrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			AssertEquals("Wrapper.Lines", 9, wrapper.Lines.Length);
			line1 = wrapper.Lines[0];
			line2 = wrapper.Lines[1];
			line3 = wrapper.Lines[2];
			line4 = wrapper.Lines[3];
			line5 = wrapper.Lines[4];
			line6 = wrapper.Lines[5];
			line7 = wrapper.Lines[6];
			var line8 = wrapper.Lines[7];
			var line9 = wrapper.Lines[8];
			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line1.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 1, line1.LineNumber);

			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line2.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 2, line2.LineNumber);

			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line3.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 3, line3.LineNumber);

			AssertEquals("LineActionCode should be 'A' for amend in change message when line was in previous message - HVLV line 1", "A", line4.LineActionCode);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 4, line4.LineNumber);

			AssertEquals("LineActionCode should be 'A' for amend in change message when line was in previous message - HVLV line 2", "A", line5.LineActionCode);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 5, line5.LineNumber);

			AssertEquals("LineActionCode should be 'A' for amend in change message when line was in previous message - HVLV line 3", "A", line6.LineActionCode);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 6, line6.LineNumber);

			AssertEquals("LineActionCode should be 'A' for amend in change message when line was in previous message - HVLV line 4", "A", line7.LineActionCode);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 7, line7.LineNumber);

			AssertEquals("LineActionCode should be 'I' for change message when line was not in the previous message, ie for new lines", "I", line8.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 8, line8.LineNumber);

			AssertEquals("LineActionCode should be 'I' for change message when line was not in the previous message, ie for new lines", "I", line9.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 9, line9.LineNumber);
		}

		public void TestShipmentAndHLVConsignmentLinesDeleted()
		{
			var testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_UniqueConsignRef = "C00026899";
			testConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			testConsol.JK_RL_NKDischargePort = "NZAKL";
			testConsol.JK_RL_NKLoadPort = "AUSYD";
			testConsol.JK_MasterBillNum = "081-45238340";
			var transport = testConsol.Transports[0];
			transport.JW_VoyageFlight = "QF118";
			transport.JW_ATD = new ZDateTime(new DateTime(2014, 10, 28));

			AddAShipmentAndDeclarationToConsol(testConsol, "58247-1", 1, false);
			var shipment2 = AddAShipmentAndDeclarationToConsol(testConsol, "45423-2", 2, false);
			AddAShipmentAndDeclarationToConsol(testConsol, "85923-3", 3, false);
			var shipment4 = AddHVLShipmentToConsol(testConsol, "HLS-001");

			Factory.Save();

			var outgoingMessage1ORG = Factory.New<CMRMessage>();
			outgoingMessage1ORG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1ORG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1ORG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1ORG.EM_MessageSubType = "ORG";
			outgoingMessage1ORG.EM_Status = "CLR";
			outgoingMessage1ORG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 28, 00, 15, 00);
			outgoingMessage1ORG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026899/CMT1:1+9'TDT+20+++6'DTM+136:20141028:102'GIS+C:121:95'CNT+11:13'CNI+1'CNT+11:2'RFF+TL:EXLV'GID+1'RFF+HWB:58247-1'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++GOODS'CNI+2'CNT+11:4'RFF+TL:EXLV'GID+1'RFF+HWB:45423-2'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++TESTING CHANGE MSSG DELETIONS'CNI+3'CNT+11:7'RFF+TL:EXLV'GID+1'RFF+HWB:85923-3'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PRINTED DOCUMENTS'UNT+34+1'";
			testConsol.Messages.Add(outgoingMessage1ORG);

			var responseMessage1CLR = Factory.New<CMRMessage>();
			responseMessage1CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1CLR.EM_MessageSubType = "CLR";
			responseMessage1CLR.EM_Status = "RCV";
			responseMessage1CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 28, 00, 16, 00);
			responseMessage1CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+10CD DF50 DHBE:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026899/CMT1::001'RFF+ACW:ESM'RFF+AFM:9'RFF+AIZ:AAAC3EMXR'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0003'FTX+AHN+++CLEAR'CNT+5:0003'UNT+22+000001'";
			testConsol.Messages.Add(responseMessage1CLR);

			wrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			AssertEquals("Wrapper.Lines - should be 3 shipments plus 4 HVLV consignments from HLV shipment", 7, wrapper.Lines.Length);
			var line1 = wrapper.Lines[0];
			var line2 = wrapper.Lines[1];
			var line3 = wrapper.Lines[2];
			var line4 = wrapper.Lines[3];
			var line5 = wrapper.Lines[4];
			var line6 = wrapper.Lines[5];
			var line7 = wrapper.Lines[6];
			AssertEquals("LineActionCode will be blank by default for original", "", line1.LineActionCode);
			AssertEquals("ESM LineNumber", 1, line1.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line2.LineActionCode);
			AssertEquals("ESM LineNumber", 2, line2.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line3.LineActionCode);
			AssertEquals("ESM LineNumber", 3, line3.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line4.LineActionCode);
			AssertEquals("ESM LineNumber", 4, line4.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line5.LineActionCode);
			AssertEquals("ESM LineNumber", 5, line5.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line6.LineActionCode);
			AssertEquals("ESM LineNumber", 6, line6.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line7.LineActionCode);
			AssertEquals("ESM LineNumber", 7, line7.LineNumber);

			UpdateLinesToManifested(testConsol);
			var permit = testConsol.CusEntryNums.AddNew();
			permit.CE_EntryType = "CRN";
			permit.CE_EntryNum = "AAAC3EMXR";
			permit.CE_ParentID = testConsol.PK;
			permit.CE_ParentTable = testConsol.TableName;

			var shipment5 = AddAShipmentAndDeclarationToConsol(testConsol, "68478-5", 8, false);
			var shipment6 = AddAShipmentAndDeclarationToConsol(testConsol, "43523-6", 9, false);
			Factory.Save();

			wrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			AssertEquals("Wrapper.Lines", 9, wrapper.Lines.Length);
			line1 = wrapper.Lines[0];
			line2 = wrapper.Lines[1];
			line3 = wrapper.Lines[2];
			line4 = wrapper.Lines[3];
			line5 = wrapper.Lines[4];
			line6 = wrapper.Lines[5];
			line7 = wrapper.Lines[6];
			var line8 = wrapper.Lines[7];
			var line9 = wrapper.Lines[8];
			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line1.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 1, line1.LineNumber);

			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line2.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 2, line2.LineNumber);

			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line3.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 3, line3.LineNumber);

			AssertEquals("LineActionCode should be 'A' for amend in change message when line was in previous message - HVLV line 1", "A", line4.LineActionCode);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 4, line4.LineNumber);

			AssertEquals("LineActionCode should be 'A' for amend in change message when line was in previous message - HVLV line 2", "A", line5.LineActionCode);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 5, line5.LineNumber);

			AssertEquals("LineActionCode should be 'A' for amend in change message when line was in previous message - HVLV line 3", "A", line6.LineActionCode);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 6, line6.LineNumber);

			AssertEquals("LineActionCode should be 'A' for amend in change message when line was in previous message - HVLV line 4", "A", line7.LineActionCode);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 7, line7.LineNumber);

			AssertEquals("LineActionCode should be 'I' for change message when line was not in the previous message, ie for new lines", "I", line8.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 8, line8.LineNumber);

			AssertEquals("LineActionCode should be 'I' for change message when line was not in the previous message, ie for new lines", "I", line9.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 9, line9.LineNumber);

			var wrappedShipment = new FreightShipmentWrapper(shipment5, testConsol);
			wrappedShipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			wrappedShipment = new FreightShipmentWrapper(shipment6, testConsol);
			wrappedShipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			Factory.Save();

			//Delete 1 of the HVLV lines
			var header = Factory.LoadTop1<IHVLVConsignmentHeader>(new ZQuery(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, shipment4.PK)) ?? Factory.New<IHVLVConsignmentHeader>();
			var hvlvQuery = new ZQuery(HVLVConsignmentSchema.HVC_HCH_Header, header.PK);
			hvlvQuery.AddToFilter(HVLVConsignmentSchema.HVC_WaybillNumber, "HVLV2");
			var hvlvLine2 = Factory.LoadTop1<IHVLVConsignment>(hvlvQuery);
			wrappedShipment = new FreightShipmentWrapper(shipment4, testConsol, hvlvLine2);
			wrappedShipment.UpdateManifestedLineNumberToPreliminaryDeletedLine();
			hvlvLine2.Delete();
			Factory.Save();
			AssertNotNull("ESMPreliminaryDeletedLineSequence", wrappedShipment.ESMPreliminaryDeletedLineSequence);

			wrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			AssertEquals("Wrapper.Lines", 9, wrapper.Lines.Length);
			line1 = wrapper.Lines[0];
			line2 = wrapper.Lines[1];
			line3 = wrapper.Lines[2];
			line4 = wrapper.Lines[3];
			line5 = wrapper.Lines[4];
			line6 = wrapper.Lines[5];
			line7 = wrapper.Lines[6];
			line8 = wrapper.Lines[7];
			line9 = wrapper.Lines[8];
			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line1.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 1, line1.LineNumber);

			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line2.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 2, line2.LineNumber);

			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line3.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 3, line3.LineNumber);

			AssertEquals("LineActionCode should be 'A' for amend in change message when line was in previous message - HVLV line 1", "A", line4.LineActionCode);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 4, line4.LineNumber);

			AssertEquals("LineActionCode should be 'D' for deletion of this HVLV line 2", "D", line5.LineActionCode);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 5, line5.LineNumber);

			AssertEquals("LineActionCode should be 'A' for amend in change message when line was in previous message - HVLV line 3", "A", line6.LineActionCode);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 6, line6.LineNumber);

			AssertEquals("LineActionCode should be 'A' for amend in change message when line was in previous message - HVLV line 4", "A", line7.LineActionCode);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 7, line7.LineNumber);

			AssertEquals("LineActionCode should be 'A' for amend in change message when line was in previous message", "A", line8.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment from the previous ammend message", 8, line8.LineNumber);

			AssertEquals("LineActionCode should be 'A' for amend in change message when line was in previous message", "A", line9.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment from the previous ammend message", 9, line9.LineNumber);

			//Update Deleted HVLV line
			wrappedShipment = new FreightShipmentWrapper(shipment4, testConsol, hvlvLine2);
			wrappedShipment.UpdateManifestedLineNumberToPreliminaryDeletedLine();
			var consolWrapper = new FreightConsolWrapper(testConsol, responseMessage1CLR);
			consolWrapper.UpdateDeletedManifestLines();
			Factory.Save();

			//Delete 1 shipment, Add three new shipments, Delete 2 HVLV lines
			testConsol.Shipments.Remove(shipment2);
			var shipment7 = AddAShipmentAndDeclarationToConsol(testConsol, "S00047250", 10, false);
			var shipment8 = AddAShipmentAndDeclarationToConsol(testConsol, "S00047255", 11, false);
			var shipment9 = AddAShipmentAndDeclarationToConsol(testConsol, "S00047258", 12, false);
			hvlvQuery = new ZQuery(HVLVConsignmentSchema.HVC_HCH_Header, header.PK);
			hvlvQuery.AddToFilter(HVLVConsignmentSchema.HVC_WaybillNumber, "HVLV1");
			var hvlvLine1 = Factory.LoadTop1<IHVLVConsignment>(hvlvQuery);
			hvlvLine1.Delete();
			hvlvQuery = new ZQuery(HVLVConsignmentSchema.HVC_HCH_Header, header.PK);
			hvlvQuery.AddToFilter(HVLVConsignmentSchema.HVC_WaybillNumber, "HVLV3");
			var hvlvLine3 = Factory.LoadTop1<IHVLVConsignment>(hvlvQuery);
			hvlvLine3.Delete();
			Factory.Save();

			var reLoadedWrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			AssertEquals("Wrapper.Lines", 11, reLoadedWrapper.Lines.Length);
			line1 = reLoadedWrapper.Lines[0];
			line2 = reLoadedWrapper.Lines[1];
			line3 = reLoadedWrapper.Lines[2];
			line4 = reLoadedWrapper.Lines[3];
			line6 = reLoadedWrapper.Lines[4];
			line7 = reLoadedWrapper.Lines[5];
			line8 = reLoadedWrapper.Lines[6];
			line9 = reLoadedWrapper.Lines[7];
			var line10 = reLoadedWrapper.Lines[8];
			var line11 = reLoadedWrapper.Lines[9];
			var line12 = reLoadedWrapper.Lines[10];
			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line1.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 1, line1.LineNumber);

			AssertEquals("LineActionCode should be 'D' for deletion of this shipment from the ESM", "D", line2.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 2, line2.LineNumber);

			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line3.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 3, line3.LineNumber);

			AssertEquals("HVLV line 1 has been deleted", "D", line4.LineActionCode);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 4, line4.LineNumber);

			//Deletion from previous message (Line 5) should no longer be in this new ESM message now.

			AssertEquals("HVLV line 3 has been deleted", "D", line6.LineActionCode);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 6, line6.LineNumber);

			AssertEquals("LineActionCode should be 'A' for amend in change message when line was in previous message - HVLV line 4", "A", line7.LineActionCode);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 7, line7.LineNumber);

			AssertEquals("LineActionCode should be 'A' for amend in change message when line was in previous message", "A", line8.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment from the previous ammend message", 8, line8.LineNumber);

			AssertEquals("LineActionCode should be 'A' for amend in change message when line was in previous message", "A", line9.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment from the previous ammend message", 9, line9.LineNumber);

			AssertEquals("LineActionCode should be 'I' for change message when line was not in the previous message, ie for new lines", "I", line10.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 10, line10.LineNumber);

			AssertEquals("LineActionCode should be 'I' for change message when line was not in the previous message, ie for new lines", "I", line11.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 11, line11.LineNumber);

			AssertEquals("LineActionCode should be 'I' for change message when line was not in the previous message, ie for new lines", "I", line12.LineActionCode);
			AssertEquals("LineNumber should be next line number when inserting a line", 12, line12.LineNumber);
		}

		public void TestHLVShipmentDeleted()
		{
			var testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_UniqueConsignRef = "C00026899";
			testConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			testConsol.JK_RL_NKDischargePort = "NZAKL";
			testConsol.JK_RL_NKLoadPort = "AUSYD";
			testConsol.JK_MasterBillNum = "081-45238340";
			var transport = testConsol.Transports[0];
			transport.JW_VoyageFlight = "QF118";
			transport.JW_ATD = new ZDateTime(new DateTime(2014, 10, 28));

			var shipment1 = AddAShipmentAndDeclarationToConsol(testConsol, "58247-1", 1, false);
			var shipment2 = AddAShipmentAndDeclarationToConsol(testConsol, "45423-2", 2, false);
			var shipment3 = AddAShipmentAndDeclarationToConsol(testConsol, "85923-3", 3, false);
			var hlvShipment4 = AddHVLShipmentToConsol(testConsol, "HLS-001");

			Factory.Save();

			var outgoingMessage1ORG = Factory.New<CMRMessage>();
			outgoingMessage1ORG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1ORG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1ORG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1ORG.EM_MessageSubType = "ORG";
			outgoingMessage1ORG.EM_Status = "CLR";
			outgoingMessage1ORG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 28, 00, 15, 00);
			outgoingMessage1ORG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026899/CMT1:1+9'TDT+20+++6'DTM+136:20141028:102'GIS+C:121:95'CNT+11:13'CNI+1'CNT+11:2'RFF+TL:EXLV'GID+1'RFF+HWB:58247-1'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++GOODS'CNI+2'CNT+11:4'RFF+TL:EXLV'GID+1'RFF+HWB:45423-2'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++TESTING CHANGE MSSG DELETIONS'CNI+3'CNT+11:7'RFF+TL:EXLV'GID+1'RFF+HWB:85923-3'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PRINTED DOCUMENTS'UNT+34+1'";
			testConsol.Messages.Add(outgoingMessage1ORG);

			var responseMessage1CLR = Factory.New<CMRMessage>();
			responseMessage1CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1CLR.EM_MessageSubType = "CLR";
			responseMessage1CLR.EM_Status = "RCV";
			responseMessage1CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 28, 00, 16, 00);
			responseMessage1CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+10CD DF50 DHBE:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026899/CMT1::001'RFF+ACW:ESM'RFF+AFM:9'RFF+AIZ:AAAC3EMXR'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0003'FTX+AHN+++CLEAR'CNT+5:0003'UNT+22+000001'";
			testConsol.Messages.Add(responseMessage1CLR);

			wrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			AssertEquals("Wrapper.Lines - should be 3 shipments plus 4 HVLV consignments from HLV shipment", 7, wrapper.Lines.Length);
			var line1 = wrapper.Lines[0];
			var line2 = wrapper.Lines[1];
			var line3 = wrapper.Lines[2];
			var line4 = wrapper.Lines[3];
			var line5 = wrapper.Lines[4];
			var line6 = wrapper.Lines[5];
			var line7 = wrapper.Lines[6];
			AssertEquals("LineActionCode will be blank by default for original", "", line1.LineActionCode);
			AssertEquals("ESM LineNumber", 1, line1.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line2.LineActionCode);
			AssertEquals("ESM LineNumber", 2, line2.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line3.LineActionCode);
			AssertEquals("ESM LineNumber", 3, line3.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line4.LineActionCode);
			AssertEquals("ESM LineNumber", 4, line4.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line5.LineActionCode);
			AssertEquals("ESM LineNumber", 5, line5.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line6.LineActionCode);
			AssertEquals("ESM LineNumber", 6, line6.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line7.LineActionCode);
			AssertEquals("ESM LineNumber", 7, line7.LineNumber);

			UpdateLinesToManifested(testConsol);
			var permit = testConsol.CusEntryNums.AddNew();
			permit.CE_EntryType = "CRN";
			permit.CE_EntryNum = "AAAC3EMXR";
			permit.CE_ParentID = testConsol.PK;
			permit.CE_ParentTable = testConsol.TableName;
			Factory.Save();

			// Delete(detach) whole HLS(HVLV) shipment.
			testConsol.Shipments.Remove(hlvShipment4);
			Factory.Save();

			wrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			AssertEquals("Wrapper.Lines", 7, wrapper.Lines.Length);
			line1 = wrapper.Lines[0];
			line2 = wrapper.Lines[1];
			line3 = wrapper.Lines[2];
			line4 = wrapper.Lines[3];
			line5 = wrapper.Lines[4];
			line6 = wrapper.Lines[5];
			line7 = wrapper.Lines[6];
			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line1.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 1, line1.LineNumber);

			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line2.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 2, line2.LineNumber);

			AssertEquals("LineActionCode should be 'A' for change message when line was in previous message", "A", line3.LineActionCode);
			AssertEquals("LineNumber should match the line number for this shipment on the original message", 3, line3.LineNumber);

			AssertEquals("HVLV line 1 has been deleted from the ESM", "D", line4.LineActionCode);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 4, line4.LineNumber);

			AssertEquals("HVLV line 2 has been deleted from the ESM", "D", line5.LineActionCode);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 5, line5.LineNumber);

			AssertEquals("HVLV line 3 has been deleted from the ESM", "D", line6.LineActionCode);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 6, line6.LineNumber);

			AssertEquals("HVLV line 3 has been deleted from the ESM", "D", line7.LineActionCode);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 7, line7.LineNumber);
		}

		public void TestHLVShipmentAmendmentNoChange()
		{
			var testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_UniqueConsignRef = "C00026899";
			testConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			testConsol.JK_RL_NKDischargePort = "NZAKL";
			testConsol.JK_RL_NKLoadPort = "AUSYD";
			testConsol.JK_MasterBillNum = "081-45238340";
			var transport = testConsol.Transports[0];
			transport.JW_VoyageFlight = "QF118";
			transport.JW_ATD = new ZDateTime(new DateTime(2016, 06, 28));

			var shipment1 = AddHVLShipmentToConsol(testConsol, "HLS-001");

			Factory.Save();

			var outgoingMessage1ORG = Factory.New<CMRMessage>();
			outgoingMessage1ORG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1ORG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1ORG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1ORG.EM_MessageSubType = "ORG";
			outgoingMessage1ORG.EM_Status = "CLR";
			outgoingMessage1ORG.EM_SystemCreateTimeUtc = new ZDateTime(2016, 06, 28, 00, 15, 00);
			outgoingMessage1ORG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026899/CMT1:1+9'TDT+20+++6'DTM+136:20160628:102'GIS+C:121:95'CNT+11:13'CNI+1'CNT+11:2'RFF+TL:EXLV'GID+1'RFF+HWB:58247-1'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++GOODS'CNI+2'CNT+11:4'RFF+TL:EXLV'GID+1'RFF+HWB:45423-2'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++TESTING CHANGE MSSG DELETIONS'CNI+3'CNT+11:7'RFF+TL:EXLV'GID+1'RFF+HWB:85923-3'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PRINTED DOCUMENTS'UNT+34+1'";
			testConsol.Messages.Add(outgoingMessage1ORG);

			var responseMessage1CLR = Factory.New<CMRMessage>();
			responseMessage1CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1CLR.EM_MessageSubType = "CLR";
			responseMessage1CLR.EM_Status = "RCV";
			responseMessage1CLR.EM_SystemCreateTimeUtc = new ZDateTime(2016, 06, 28, 00, 16, 00);
			responseMessage1CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+10CD DF50 DHBE:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026899/CMT1::001'RFF+ACW:ESM'RFF+AFM:9'RFF+AIZ:AAAC3EMXR'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0003'FTX+AHN+++CLEAR'CNT+5:0003'UNT+22+000001'";
			testConsol.Messages.Add(responseMessage1CLR);

			wrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			AssertEquals("Wrapper.Lines - should be 4 HVLV consignments from the HLV shipment", 4, wrapper.Lines.Length);
			var line1 = wrapper.Lines[0];
			var line2 = wrapper.Lines[1];
			var line3 = wrapper.Lines[2];
			var line4 = wrapper.Lines[3];
			AssertEquals("LineActionCode will be blank by default for original", "", line1.LineActionCode);
			AssertEquals("ESM LineNumber", 1, line1.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line2.LineActionCode);
			AssertEquals("ESM LineNumber", 2, line2.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line3.LineActionCode);
			AssertEquals("ESM LineNumber", 3, line3.LineNumber);

			AssertEquals("LineActionCode will be blank by default for original", "", line4.LineActionCode);
			AssertEquals("ESM LineNumber", 4, line4.LineNumber);

			UpdateLinesToManifested(testConsol);

			//Simulate sending amendment with no change to HVLV data.
			var permit = testConsol.CusEntryNums.AddNew();
			permit.CE_EntryType = "CRN";
			permit.CE_EntryNum = "AAAC3EMXR";
			permit.CE_ParentID = testConsol.PK;
			permit.CE_ParentTable = testConsol.TableName;
			Factory.Save();

			var amendmentWrapper = new FreightConsolManifestHeaderWrapper(testConsol);
			AssertEquals("Wrapper.Lines - should remain unchanged at 4 HVLV consignments from the HLB shipment", 4, amendmentWrapper.Lines.Length);
			var amendline1 = amendmentWrapper.Lines[0];
			var amendline2 = amendmentWrapper.Lines[1];
			var amendline3 = amendmentWrapper.Lines[2];
			var amendline4 = amendmentWrapper.Lines[3];

			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 1, amendline1.LineNumber);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 2, amendline2.LineNumber);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 3, amendline3.LineNumber);
			AssertEquals("LineNumber should match the line number for this HVLV consignment on the original message", 4, amendline4.LineNumber);

			// There should be no new Preliminary Manifested Lines added
			bool hasConsignmentlines = false;
			foreach (ForwardingShipment shipment in testConsol.Shipments)
			{
				if (shipment.IsHighVolumeLowValue)
				{
					foreach (IHVLVConsignment consignmentLine in shipment.HVLVConsignments)
					{
						hasConsignmentlines = true;
						var shipmentWrapper = new FreightShipmentWrapper(shipment, testConsol, consignmentLine);
						AssertEquals("consignment line should have a Manifested number type not a Preliminary number type", false, shipmentWrapper.IsPreliminaryNumberType);
						AssertEquals("consignment line should have a Manifested number type not a Preliminary number type", true, shipmentWrapper.HasSubManifestLineNumber);
					}
				}
			}
			AssertEquals("hasConsignmentlines", true, hasConsignmentlines);
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			wrapper = new FreightConsolManifestHeaderWrapper(consol);
		}

		ForwardingConsol consol;
		FreightConsolManifestHeaderWrapper wrapper;

		CommonShipment AddAShipmentAndDeclarationToConsol(ForwardingConsol consol, ZString hawbNo, ZInt consignmentCount, bool consignmentManifested)
		{
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = hawbNo;
			shipment1.JS_OuterPacks = 10;
			shipment1.JS_GoodsDescription = "Paper Goods";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_JS = shipment1.PK;
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentTable = declaration.TableName;
			entryNumber.CE_ParentID = declaration.PK;

			entryNumber.CE_EntryType = CusEntryNumberTypes.Australia.EX1;
			entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var shipment = new FreightShipmentWrapper(shipment1, consol);
			shipment.CreatePreliminaryManifestLineNumber(consignmentCount);
			if (consignmentManifested)
			{
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			return shipment1;
		}

		CommonShipment AddAColoadShipmentAndDeclarationToConsol(ForwardingConsol consol, ZString hawbNo, ZInt consignmentCount, bool consignmentManifested)
		{
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = "CLD";
			shipment.JS_HouseBill = hawbNo;
			shipment.JS_OuterPacks = 20;
			shipment.JS_GoodsDescription = "Magazines";

			var relatedShipment = shipment.CoLoadShipments.AddNew();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_JS = relatedShipment.PK;
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentTable = declaration.TableName;
			entryNumber.CE_ParentID = declaration.PK;

			entryNumber.CE_EntryType = CusEntryNumberTypes.Australia.EX1;
			entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var wrappedShipment = new FreightShipmentWrapper(relatedShipment, consol);
			wrappedShipment.CreatePreliminaryManifestLineNumber(consignmentCount);
			if (consignmentManifested)
			{
				wrappedShipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			return shipment;
		}

		CommonShipment AddHVMShipmentToConsol(ForwardingConsol consol, string housebillNumber)
		{
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = housebillNumber;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueMaster;
			shipment.JS_OuterPacks = 10;
			shipment.JS_GoodsDescription = "My baby can count from 1-10";

			return shipment;
		}

		CommonShipment AddHVLShipmentToConsol(ForwardingConsol consol, ZString hawbNo)
		{
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = hawbNo;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.JS_OuterPacks = 10;
			shipment.JS_GoodsDescription = "Milk Powder";

			var consignment1 = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType<IHVLVConsignment>());
			consignment1.HVC_WaybillNumber = "HVLV1";
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_ManifestedWeight = 1m;
			consignment1.HVC_WeightUQ = "KG";
			consignment1.HVC_GoodsValue = 10m;
			consignment1.HVC_GoodsDescription = "Milk Powder";
			consignment1.HVC_RX_NKGoodsValueCurrency = "AUD";
			consignment1.HVC_ItemCount = 10;
			consignment1.HVC_ConsigneeName = "John Smith";
			consignment1.HVC_RN_NKConsigneeCountryCode = Core.Constants.CountryCodes.Australia;

			var consignment2 = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType<IHVLVConsignment>());
			consignment2.HVC_WaybillNumber = "HVLV2";
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_ManifestedWeight = 2m;
			consignment2.HVC_WeightUQ = "KG";
			consignment2.HVC_GoodsValue = 20m;
			consignment2.HVC_GoodsDescription = "Milk Powder";
			consignment2.HVC_RX_NKGoodsValueCurrency = "AUD";
			consignment2.HVC_ItemCount = 20;
			consignment2.HVC_ConsigneeName = "John Smith";
			consignment2.HVC_RN_NKConsigneeCountryCode = Core.Constants.CountryCodes.Australia;

			var consignment3 = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType<IHVLVConsignment>());
			consignment3.HVC_WaybillNumber = "HVLV3";
			consignment3.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment3.HVC_ManifestedWeight = 1m;
			consignment3.HVC_WeightUQ = "KG";
			consignment3.HVC_GoodsValue = 8m;
			consignment3.HVC_GoodsDescription = "Milk Powder";
			consignment3.HVC_RX_NKGoodsValueCurrency = "AUD";
			consignment3.HVC_ItemCount = 5;
			consignment3.HVC_ConsigneeName = "John Smith";
			consignment3.HVC_RN_NKConsigneeCountryCode = Core.Constants.CountryCodes.Australia;

			var consignment4 = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType<IHVLVConsignment>());
			consignment4.HVC_WaybillNumber = "HVLV4";
			consignment4.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment4.HVC_ManifestedWeight = 2m;
			consignment4.HVC_WeightUQ = "KG";
			consignment4.HVC_GoodsValue = 30m;
			consignment4.HVC_GoodsDescription = "Milk Powder";
			consignment4.HVC_RX_NKGoodsValueCurrency = "AUD";
			consignment4.HVC_ItemCount = 18;
			consignment4.HVC_ConsigneeName = "John Smith";
			consignment4.HVC_RN_NKConsigneeCountryCode = Core.Constants.CountryCodes.Australia;

			return shipment;
		}

		void UpdateLinesToManifested(ForwardingConsol consol)
		{
			foreach (ForwardingShipment shipment in consol.Shipments)
			{
				if (shipment.IsHighVolumeLowValueLegacy || shipment.IsHighVolumeLowValue)
				{
					foreach (var consignmentLine in shipment.GetHVLVConsignmentLines())
					{
						var shipmentWrapper = new FreightShipmentWrapper(shipment, consol, consignmentLine);
						shipmentWrapper.UpdatePreliminaryLineNumberToManifestedLineNumber();
					}
				}
				else
				{
					var wrappedShipment = new FreightShipmentWrapper(shipment, consol);
					wrappedShipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
				}
			}
		}
	}
}
