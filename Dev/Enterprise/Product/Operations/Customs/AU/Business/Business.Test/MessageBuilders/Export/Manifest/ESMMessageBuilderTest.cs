using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ESMMessageBuilderTest : ManifestMessageBuilderAbstractTest
	{
		public void TestSegmentLOC()
		{
			var testConsol = CreateSimpleESMConsol();
			var org = OrgHeader.New(Factory);
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;
			org.OH_RL_NKClosestPort = "AUSYD";
			var address1 = org.Addresses[0];
			address1.OA_Address1 = "test1";
			address1.OA_Address2 = "test2";
			testConsol.JK_OA_PackDepotAddress = address1.PK;
			testConsol.PackDepotAddress.LocalControlledPremisesID = "ABC123";
			var transport = testConsol.Transports[0];
			transport.JW_ATD = DateTime.Now.AddDays(-3);
			var wrapper = new FreightConsolWrapper(testConsol);
			wrapper.ContingencyCAN = "12345678901234";
			var shipment = testConsol.Shipments.AddNew();
			AddExemptionCode(shipment, CANType.Exemptions.EXLV.Code);
			shipment.JS_OuterPacks = 10;
			var builder = new ESMMessageBuilder(testConsol, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertContains("LOC+107+ABC123::95'", builder.GeneratedMessageStrings[0]);
		}

		public void TestContainerCount()
		{
			var consol = CreateSimpleESMConsol();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "C1";

			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "C2";

			CommonShipment shipment1 = consol.Shipments.AddNew();
			CommonShipment shipment2 = consol.Shipments.AddNew();
			CommonShipment shipment3 = consol.Shipments.AddNew();

			var packLine1 = shipment1.OuterPackLines.AddNew();
			var packLine2 = shipment2.OuterPackLines.AddNew();
			var packLine3 = shipment2.OuterPackLines.AddNew();
			var packLine4 = shipment3.OuterPackLines.AddNew();

			UnpackAllContainers(shipment1);
			UnpackAllContainers(shipment2);
			UnpackAllContainers(shipment3);

			packLine1.Containers.Add(container1);
			packLine2.Containers.Add(container1);
			packLine3.Containers.Add(container2);
			packLine4.Containers.Add(container2);

			AssertEquals(1, shipment1.Containers.Count());
			AssertEquals(2, shipment2.Containers.Count());
			AssertEquals(1, shipment3.Containers.Count());

			var builder = new ESMMessageBuilder(consol, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("NumberOfMessagesGenerated", 1, builder.GeneratedMessageStrings.Length);
			Assert("MessageTextIncludesCTN", builder.GeneratedMessageStrings[0].IndexOf("GIS+C:121:95'CNT+36:2'UNT") != -1);
		}

		public void TestSimpleESMMessage()
		{
			var testConsol = CreateSimpleESMConsol();
			AddShipmentAndDeclarationToConsol(testConsol);
			var builder = new ESMMessageBuilder(testConsol, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("NumberOfMessagesGenerated", 1, builder.GeneratedMessageStrings.Length);
			var expectedString = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("SimpleESM.txt"));
			AssertMultilineASCIIEquals("MessageText", expectedString, builder.GeneratedMessageStrings[0].Replace("'", "'\r\n"));
		}

		public void TestDontSendLinesIfNoLinesFlag()
		{
			var testConsol = CreateSimpleESMConsol();
			AddShipmentAndDeclarationToConsol(testConsol);
			var builder = new ESMMessageBuilder(testConsol, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Replace;
			builder.DontSendAnyLines = true;
			Assert("No Lines Included", builder.GeneratedMessageStrings[0].IndexOf("CNI") == -1);
		}

		public void TestSetStatusAsPendingIfPendingFlagSet()
		{
			var testConsol = CreateSimpleESMConsol();
			AddShipmentAndDeclarationToConsol(testConsol);
			var builder = new ESMMessageBuilder(testConsol, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Replace;
			builder.SetStatusToPending = true;
			AssertEquals("No Lines Included", EDIMessage.Status.Pending, builder.PopulateMessagesReturningResult().EM_Status);
		}

		public void TestECNShipmentSentAsContingency()
		{
			var testConsol = CreateSimpleESMConsol();
			AddShipmentAndDeclarationToConsol(testConsol, CusEntryNumberTypes.Australia.ECN);
			var builder = new ESMMessageBuilder(testConsol, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("NumberOfMessagesGenerated", 1, builder.GeneratedMessageStrings.Length);
			Assert("MessageTextIncludesCCAN", builder.GeneratedMessageStrings[0].IndexOf("RFF+AHV:") != -1);
		}

		public void TestSimpleESMMessageStandalone()
		{
			var header = CreateSimpleESMExportCustomsManifestHeader();
			AddLine(header, "AAAAAAMP7");
			var builder = new ESMMessageBuilder(header, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("NumberOfMessagesGenerated", 1, builder.GeneratedMessageStrings.Length);
			var expectedString = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("SimpleESM.txt"));
			AssertMultilineASCIIEquals("MessageText", expectedString, builder.GeneratedMessageStrings[0].Replace("'", "'\r\n"));
		}

		public void TestGetMessageSubType()
		{
			var testConsol = CreateSimpleESMConsol();
			var messageType = ESMMessageBuilder.GetMessageSubType(testConsol);
			AssertEquals("Message Type", Common.MessageBuilders.MessageSubTypes.Create, messageType);

			var wrapper = new FreightConsolWrapper(testConsol);

			var permit = wrapper.CreateCusEntryNumber();
			permit.CE_EntryNum = "12345";

			messageType = ESMMessageBuilder.GetMessageSubType(testConsol);
			AssertEquals("Message Type", Common.MessageBuilders.MessageSubTypes.Replace, messageType);
		}

		public void TestESMChangeFunctionCode()
		{
			var testConsol = CreateSimpleESMConsol();
			AddShipmentAndDeclarationToConsol(testConsol);
			var builder = new ESMMessageBuilder(testConsol, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			AssertEquals("NumberOfMessagesGenerated", 1, builder.GeneratedMessageStrings.Length);
			Assert("Message Function Code for change message", builder.MessageText.Contains("BGM+87:::ESM+<<SENDERS REFERENCE PLACE HOLDER>>/DAT0:1+4'"));
		}

		public void TestOnlySendManditoryLineData()
		{
			var header = CreateSimpleESMExportCustomsManifestHeader();
			AddLine(header, "AAAAAAMP7");
			var builder = new ESMMessageBuilder(header, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("NumberOfMessagesGenerated", 1, builder.GeneratedMessageStrings.Length);
			Assert("Description not Included", builder.GeneratedMessageStrings[0].IndexOf("FISH LIVERS AND ROES, FRESH") == -1);
			Assert("Goods Owner not Included", builder.GeneratedMessageStrings[0].IndexOf("JACK JONES") == -1);
			Assert("Goods Owner ID not Included", builder.GeneratedMessageStrings[0].IndexOf("1234567890") == -1);
			Assert("Country of Destination not Included", builder.GeneratedMessageStrings[0].IndexOf("NZ") == -1);
		}

		public void TestDontIncludeGoodsOwnerIDCodeTypeIfNoGoodsOwnerID()
		{
			var header = CreateSimpleESMExportCustomsManifestHeader();
			AddLine(header, "AAAAAAMP7");
			header.Lines[0].EL_TypeOfCAN = CANType.Exemptions.EXLV.Code;
			header.Lines[0].EL_CAN = ZString.Empty;
			header.Lines[0].EL_GoodsOwnerPartyID = ZString.Empty;
			var builder = new ESMMessageBuilder(header, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("NumberOfMessagesGenerated", 1, builder.GeneratedMessageStrings.Length);
			Assert("Goods Owner ID Code Type not Included", builder.GeneratedMessageStrings[0].IndexOf("NAD+GO+::95") == -1);
		}

		public void TestDontIncludeABN()
		{
			var header = CreateSimpleESMExportCustomsManifestHeader();
			AddLine(header, "AAAAAAMP7");
			header.Lines[0].EL_TypeOfCAN = CANType.Exemptions.EXLV.Code;
			header.Lines[0].EL_CAN = ZString.Empty;
			header.Lines[0].EL_GoodsOwner = "NICK";
			header.Lines[0].EL_GoodsOwnerPartyID = "12345";
			var builder = new ESMMessageBuilder(header, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("NumberOfMessagesGenerated", 1, builder.GeneratedMessageStrings.Length);
			Assert("Goods Owner ID Code not Included", builder.GeneratedMessageStrings[0].IndexOf("12345") == -1);
		}

		public void TestContingencyCANIncludedInESM()
		{
			var testConsol = CreateSimpleESMConsol();

			CommonShipment shipment = testConsol.Shipments.AddNew();
			AddContingencyCAN(shipment, "CCAN1234567890");
			var builder = new ESMMessageBuilder(testConsol, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("NumberOfMessagesGenerated", 1, builder.GeneratedMessageStrings.Length);
			Assert("MessageTextIncludesCCAN", builder.GeneratedMessageStrings[0].IndexOf("RFF+AHV:CCAN1234567890'") != -1);
		}

		public void TestExit2ExemptionCodeGetsMapped()
		{
			var testConsol = CreateSimpleESMConsol();

			CommonShipment shipment = testConsol.Shipments.AddNew();
			shipment.JS_GoodsDescription = "GOODS DESCRIPTION";
			shipment.JS_RL_NKDestination = "HKHKG";
			shipment.ConsignorPK = Factory.New(typeof(OrgHeader)).PK;
			shipment.Consignor.OH_FullName = "NICK";
			AddExemptionCode(shipment, "EX1");
			var builder = new ESMMessageBuilder(testConsol, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("NumberOfMessagesGenerated", 1, builder.GeneratedMessageStrings.Length);
			Assert("MessageTextIncludesMappedExemptionCode", builder.GeneratedMessageStrings[0].IndexOf("RFF+TL:EXPE'") != -1);
			Assert("DescriptionIncluded", builder.GeneratedMessageStrings[0].IndexOf("FTX+AAA+++GOODS DESCRIPTION'") != -1);
			Assert("CountryOfOriginIncluded", builder.GeneratedMessageStrings[0].IndexOf("LOC+28+HK::5'") != -1);
			Assert("OwnerNameIncluded", builder.GeneratedMessageStrings[0].IndexOf("NAD+GO+++NICK'") != -1);
		}

		public void TestCMRExemptionCodeGetsPutInMessage()
		{
			var testConsol = CreateSimpleESMConsol();

			CommonShipment shipment = testConsol.Shipments.AddNew();
			AddExemptionCode(shipment, CANType.Exemptions.EXLV.Code);
			var builder = new ESMMessageBuilder(testConsol, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("NumberOfMessagesGenerated", 1, builder.GeneratedMessageStrings.Length);
			Assert("MessageTextIncludesExemptionCode", builder.GeneratedMessageStrings[0].IndexOf("RFF+TL:EXLV'") != -1);
		}

		public void TestCCANForStandAlone()
		{
			var header = CreateSimpleESMExportCustomsManifestHeader();
			AddLine(header, "AAAAAAMP7");
			header.Lines[0].EL_TypeOfCAN = CANType.ContingencyCustomsAuthorityNumber.Code;
			var builder = new ESMMessageBuilder(header, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("NumberOfMessagesGenerated", 1, builder.GeneratedMessageStrings.Length);
			ZString expectedResult = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ContingencyLineESM.txt")).Replace("\r\n", "");
			AssertEquals("MessageText", expectedResult, builder.GeneratedMessageStrings[0]);
		}

		public void TestESMStandAloneCCRNWriteOff()
		{
			var header = CreateSimpleESMExportCustomsManifestHeader();
			header.ED_CCAN = "12345678901234";
			header.ED_DepartureDate = DateTime.Now.AddDays(-3);
			AddLine(header, "AAAAAAMP7");

			var builder = new ESMMessageBuilder(header, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertContingencyWriteOffConditions(builder);
		}

		public void TestESMNonStandAloneCCRNWriteOff()
		{
			var testConsol = CreateSimpleESMConsol();
			var transport = testConsol.Transports[0];
			transport.JW_ATD = DateTime.Now.AddDays(-3);
			var wrapper = new FreightConsolWrapper(testConsol);
			wrapper.ContingencyCAN = "12345678901234";
			CommonShipment shipment = testConsol.Shipments.AddNew();
			AddExemptionCode(shipment, CANType.Exemptions.EXLV.Code);
			shipment.JS_OuterPacks = 10;
			var builder = new ESMMessageBuilder(testConsol, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;

			AssertContingencyWriteOffConditions(builder);
		}

		public void TestSendShipmentsWithAnAirCargoStatusOfP300AsTransshipment()
		{
			var testConsol = CreateSimpleESMConsol();

			CommonShipment shipment = testConsol.Shipments.AddNew();
			AddAirCargoStatus(shipment, "P300");
			var builder = new ESMMessageBuilder(testConsol, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("NumberOfMessagesGenerated", 1, builder.GeneratedMessageStrings.Length);
			Assert("MessageTextIncludesEXTIExemption", builder.GeneratedMessageStrings[0].IndexOf("RFF+TL:EXTI'") != -1);
		}

		public void TestDontSendContainersForAirManifest()
		{
			var header = CreateSimpleESMExportCustomsManifestHeader();
			AddLine(header, "AAAAAAMP7");
			header.ED_NoOfContainer = 999;
			header.Lines[0].EL_NumberOfContainers = 999;
			var builder = new ESMMessageBuilder(header, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			Assert("NoContainerCountIncluded", !builder.MessageText.Contains("CNT+36:"));
		}

		public void TestSendShipmentsInFixedOrder()
		{
			var testConsol = CreateSimpleESMConsol();
			var wrapper = new FreightConsolWrapper(testConsol);
			CommonShipment shipment1 = testConsol.Shipments.AddNew();
			CommonShipment shipment2 = testConsol.Shipments.AddNew();
			AddExemptionCode(shipment1, CANType.Exemptions.EXLV.Code);
			AddExemptionCode(shipment2, CANType.Exemptions.EXPE.Code);
			shipment1.JS_UniqueConsignRef = "B";
			shipment2.JS_UniqueConsignRef = "A";
			var builder = new ESMMessageBuilder(testConsol, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			var indexOfEXLV = builder.GeneratedMessageStrings[0].IndexOf("EXLV");
			var indexOfEXPE = builder.GeneratedMessageStrings[0].IndexOf("EXPE");
			Assert("EXPE line appears before EXLV", indexOfEXPE < indexOfEXLV);
		}

		public void TestESMChangeMessage()
		{
			var testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_UniqueConsignRef = "C00026869";
			testConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			testConsol.JK_RL_NKDischargePort = "CATOR";
			testConsol.JK_RL_NKLoadPort = "AUSYD";
			testConsol.JK_MasterBillNum = "081-34234745";
			EnsureQantasAirLineCodeSet();
			var transport = testConsol.Transports[0];
			transport.JW_VoyageFlight = "QF112";
			transport.JW_ATD = new ZDateTime(new DateTime(2014, 10, 15));

			var shipment1 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047238", 1, true);

			var builder = new ESMMessageBuilder(testConsol, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("NumberOfMessagesGenerated", 1, builder.GeneratedMessageStrings.Length);
			var expectedString = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+87:::ESM+<<SENDERS REFERENCE PLACE HOLDER>>/DAT1:1+9'
TDT+20+++6'
DTM+136:20141015:102'
GIS+C:121:95'
CNT+11:10'
CNI+1'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00047238'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
UNT+15+<<MSGNO PLACEHOLDER>>'";
			AssertMultilineASCIIEquals("MessageText", expectedString, builder.GeneratedMessageStrings[0].Replace("'", "'\r\n"));

			var permit = testConsol.CusEntryNums.AddNew();
			permit.CE_EntryType = "CRN";
			permit.CE_EntryNum = "AAAC3CX6C";
			permit.CE_ParentID = testConsol.PK;
			permit.CE_ParentTable = testConsol.TableName;
			Factory.Save();

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

			var shipment2 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047239", 2, true);
			var shipment3 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047249", 3, true);
			var shipment4 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047250", 4, false);
			testConsol.Shipments.Remove(shipment2);

			var changeBuilder = new ESMMessageBuilder(testConsol, false);
			changeBuilder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			AssertEquals("NumberOfMessagesGenerated", 1, changeBuilder.GeneratedMessageStrings.Length);
			var expectedChangeMsgString = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+87:::ESM+C00026869/CMT2:7+4'
RFF+AIZ:AAAC3CX6C'
TDT+20+++6'
DTM+136:20141015:102'
GIS+C:121:95'
CNT+11:30'
CNI+1+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00047238'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+2+:::D'
CNI+3+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00047249'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+4+:::I'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00047250'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
UNT+33+<<MSGNO PLACEHOLDER>>'";
			AssertMultilineASCIIEquals("MessageText should be a change message with 4 HWBs: S00047238 Amended, S00047239 Deleted, S00047249	Amended, S00047250 Inserted", expectedChangeMsgString, changeBuilder.GeneratedMessageStrings[0].Replace("'", "'\r\n"));
		}

		public void TestESMChangeMessageDeletions()
		{
			var testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_UniqueConsignRef = "C00026895";
			testConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			testConsol.JK_RL_NKDischargePort = "CATOR";
			testConsol.JK_RL_NKLoadPort = "AUSYD";
			testConsol.JK_MasterBillNum = "081-42424292";
			EnsureQantasAirLineCodeSet();
			var transport = testConsol.Transports[0];
			transport.JW_VoyageFlight = "QF112";
			transport.JW_ATD = new ZDateTime(new DateTime(2014, 10, 24));

			var permit = testConsol.CusEntryNums.AddNew();
			permit.CE_EntryType = "CRN";
			permit.CE_EntryNum = "AAAC3EKWF";
			permit.CE_ParentID = testConsol.PK;
			permit.CE_ParentTable = testConsol.TableName;
			Factory.Save();

			AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047289", 1, true);
			var shipment2 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047290", 2, true);
			var shipment3 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047291", 3, true);

			var outgoingMessage1ORG = Factory.New<CMRMessage>();
			outgoingMessage1ORG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1ORG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1ORG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1ORG.EM_MessageSubType = "ORG";
			outgoingMessage1ORG.EM_Status = "CLR";
			outgoingMessage1ORG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 24, 06, 40, 00);
			outgoingMessage1ORG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026895/CMT1:1+9'TDT+20+++6'DTM+136:20141024:102'GIS+C:121:95'CNT+11:12'CNI+1'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047289'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++GOODS'CNI+2'CNT+11:4'RFF+TL:EXLV'GID+1'RFF+HWB:S00047290'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++MAGAZINES'CNI+3'CNT+11:5'RFF+TL:EXLV'GID+1'RFF+HWB:S00047291'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++NEWSPAPER PRINT'UNT+34+1'";
			testConsol.Messages.Add(outgoingMessage1ORG);

			var responseMessage1CLR = Factory.New<CMRMessage>();
			responseMessage1CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1CLR.EM_MessageSubType = "CLR";
			responseMessage1CLR.EM_Status = "RCV";
			responseMessage1CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 24, 06, 41, 00);
			responseMessage1CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+331D G3I5 9EG4:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026895/CMT1::001'RFF+ACW:ESM'RFF+AFM:9'RFF+AIZ:AAAC3EKWF'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0003'FTX+AHN+++CLEAR'CNT+5:0003'UNT+22+000001'";
			testConsol.Messages.Add(responseMessage1CLR);

			var shipment4 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047292", 4, false);
			var shipment5 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047293", 5, true);
			AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047294", 6, false);

			var outgoingMessage2CHG = Factory.New<CMRMessage>();
			outgoingMessage2CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage2CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage2CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2CHG.EM_MessageSubType = "ORG";
			outgoingMessage2CHG.EM_Status = "SNT";
			outgoingMessage2CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 24, 06, 46, 00);
			outgoingMessage2CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026895/CMT1:2+4'RFF+AIZ:AAAC3EKWF'TDT+20+++6'DTM+136:20141024:102'GIS+C:121:95'CNT+11:22'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047289'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++GOODS'CNI+2+:::A'CNT+11:4'RFF+TL:EXLV'GID+1'RFF+HWB:S00047290'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++MAGAZINES'CNI+3+:::A'CNT+11:5'RFF+TL:EXLV'GID+1'RFF+HWB:S00047291'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++NEWSPAPER PRINT'CNI+4+:::I'CNT+11:2'RFF+TL:EXLV'GID+1'RFF+HWB:S00047292'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++MATERIAL'CNI+5+:::I'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047293'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF'CNI+6+:::I'CNT+11:7'RFF+TL:EXLV'GID+1'RFF+HWB:S00047294'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PRINTED DOCUMENTS'UNT+62+1'";
			testConsol.Messages.Add(outgoingMessage2CHG);

			var responseMessage2CLR = Factory.New<CMRMessage>();
			responseMessage2CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage2CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage2CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage2CLR.EM_MessageSubType = "CLR";
			responseMessage2CLR.EM_Status = "RCV";
			responseMessage2CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 24, 06, 47, 00);
			responseMessage2CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+3533 77F2 JEG4:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026895/CMT1::002'RFF+ACW:ESM'RFF+AFM:4'RFF+AIZ:AAAC3EKWF'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0003'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0004'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0005'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0006'FTX+AHN+++CLEAR'CNT+5:0006'UNT+34+000001'";
			testConsol.Messages.Add(responseMessage2CLR);

			AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047299", 7, false);
			testConsol.Shipments.Remove(shipment2);
			var shipment = new FreightShipmentWrapper(shipment2, testConsol);
			shipment.ShipmentManifestLineSequence.CY_Code = CustomsManifestLineSequence.DataCodes.DeletedLineNumber;
			testConsol.Shipments.Remove(shipment3);
			shipment = new FreightShipmentWrapper(shipment3, testConsol);
			shipment.ShipmentManifestLineSequence.CY_Code = CustomsManifestLineSequence.DataCodes.DeletedLineNumber;
			testConsol.Shipments.Remove(shipment4);
			shipment = new FreightShipmentWrapper(shipment4, testConsol);
			shipment.ShipmentManifestLineSequence.CY_Code = CustomsManifestLineSequence.DataCodes.DeletedLineNumber;

			var outgoingMessage3CHG = Factory.New<CMRMessage>();
			outgoingMessage3CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage3CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage3CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage3CHG.EM_MessageSubType = "CHG";
			outgoingMessage3CHG.EM_Status = "SNT";
			outgoingMessage3CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 27, 05, 48, 00);
			outgoingMessage3CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026895/CMT1:3+4'RFF+AIZ:AAAC3EKWF'TDT+20+++6'DTM+136:20141024:102'GIS+C:121:95'CNT+11:15'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047289'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++GOODS'CNI+5+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047293'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF'CNI+6+:::A'CNT+11:7'RFF+TL:EXLV'GID+1'RFF+HWB:S00047294'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PRINTED DOCUMENTS'CNI+7+:::I'CNT+11:4'RFF+TL:EXLV'GID+1'RFF+HWB:S00047299'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++TESTING CHANGE MSSG DELETIONS'CNI+2+:::D'CNI+3+:::D'CNI+4+:::D'UNT+47+1'";
			testConsol.Messages.Add(outgoingMessage3CHG);

			var responseMessage3CLR = Factory.New<CMRMessage>();
			responseMessage3CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage3CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage3CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage3CLR.EM_MessageSubType = "CLR";
			responseMessage3CLR.EM_Status = "RCV";
			responseMessage3CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 27, 05, 49, 00);
			responseMessage3CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+4245 D4JA 7A1E:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026895/CMT1::003'RFF+ACW:ESM'RFF+AFM:4'RFF+AIZ:AAAC3EKWF'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0005'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0006'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0007'FTX+AHN+++CLEAR'CNT+5:0004'UNT+26+000001'";
			testConsol.Messages.Add(responseMessage3CLR);

			testConsol.Shipments.Remove(shipment5);

			var changeBuilder = new ESMMessageBuilder(testConsol, false);
			changeBuilder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			AssertEquals("NumberOfMessagesGenerated", 1, changeBuilder.GeneratedMessageStrings.Length);
			var expectedChangeMsgString = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+87:::ESM+C00026895/CMT1:4+4'
RFF+AIZ:AAAC3EKWF'
TDT+20+++6'
DTM+136:20141024:102'
GIS+C:121:95'
CNT+11:30'
CNI+1+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00047289'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+5+:::D'
CNI+6+:::I'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00047294'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+7+:::I'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00047299'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
UNT+33+<<MSGNO PLACEHOLDER>>'";
			AssertMultilineASCIIEquals("Deletion change message testing - MessageText should be a change message with 3 remaining HWBs and 1 deleted HWB - line 5", expectedChangeMsgString, changeBuilder.GeneratedMessageStrings[0].Replace("'", "'\r\n"));
		}

		public void TestESMChangeWhenHBNumbersAreNotSameAsJobUniqueIDs()
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

			var permit = testConsol.CusEntryNums.AddNew();
			permit.CE_EntryType = "CRN";
			permit.CE_EntryNum = "AAAC3EMXR";
			permit.CE_ParentID = testConsol.PK;
			permit.CE_ParentTable = testConsol.TableName;
			Factory.Save();

			var shipment1 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047302", 1, true);
			shipment1.JS_HouseBill = "58247-1";
			var shipment2 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047304", 2, true);
			shipment2.JS_HouseBill = "45423-2";
			var shipment3 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047305", 3, true);
			shipment3.JS_HouseBill = "85923-3";

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
			testConsol.Messages.Add(outgoingMessage3CHG);

			var outgoingMessage4CHG = Factory.New<CMRMessage>();
			outgoingMessage4CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage4CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage4CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage4CHG.EM_MessageSubType = "CHG";
			outgoingMessage4CHG.EM_Status = "SNT";
			outgoingMessage4CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 28, 01, 33, 00);
			outgoingMessage4CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026899/CMT1:4+4'RFF+AIZ:AAAC3EMXR'TDT+20+++6'DTM+136:20141028:102'GIS+C:121:95'CNT+11:22'CNI+1+:::A'CNT+11:2'RFF+TL:EXLV'GID+1'RFF+HWB:58247-1'LOC+28+NZ::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++GOODS'CNI+2+:::A'CNT+11:4'RFF+TL:EXLV'GID+1'RFF+HWB:45423-2'LOC+28+NZ::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++TESTING CHANGE MSSG DELETIONS'CNI+3+:::A'CNT+11:7'RFF+TL:EXLV'GID+1'RFF+HWB:85923-3'LOC+28+NZ::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PRINTED DOCUMENTS'CNI+4+:::I'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:49238-4'LOC+28+NZ::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PAPER'CNI+5+:::I'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:68478-5'LOC+28+NZ::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++NEWSPRINT'CNI+6+:::I'CNT+11:5'RFF+TL:EXLV'GID+1'RFF+HWB:43523-6'LOC+28+NZ::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PAPER REAMS'UNT+62+1'";
			testConsol.Messages.Add(outgoingMessage4CHG);

			var shipment4 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047306", 4, true);
			shipment4.JS_HouseBill = "49238-4";
			var shipment5 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047307", 5, true);
			shipment5.JS_HouseBill = "68478-5";
			var shipment6 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047308", 6, true);
			shipment6.JS_HouseBill = "43523-6";

			var responseMessage4CLR = Factory.New<CMRMessage>();
			responseMessage4CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage4CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage4CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage4CLR.EM_MessageSubType = "CLR";
			responseMessage4CLR.EM_Status = "RCV";
			responseMessage4CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 28, 01, 34, 00);
			responseMessage4CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+441D 948H 02BE:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026899/CMT1::004'RFF+ACW:ESM'RFF+AFM:4'RFF+AIZ:AAAC3EMXR'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0003'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0004'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0005'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0006'FTX+AHN+++CLEAR'CNT+5:0006'UNT+34+000001'";
			testConsol.Messages.Add(responseMessage4CLR);

			testConsol.Shipments.Remove(shipment2);
			testConsol.Shipments.Remove(shipment3);
			testConsol.Shipments.Remove(shipment4);
			testConsol.Shipments.Remove(shipment5);
			testConsol.Shipments.Remove(shipment6);

			var changeBuilder = new ESMMessageBuilder(testConsol, false);
			changeBuilder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			AssertEquals("NumberOfMessagesGenerated", 1, changeBuilder.GeneratedMessageStrings.Length);
			var expectedChangeMsgString = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+87:::ESM+C00026899/CMT1:5+4'
RFF+AIZ:AAAC3EMXR'
TDT+20+++6'
DTM+136:20141028:102'
GIS+C:121:95'
CNT+11:10'
CNI+1+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:58247-1'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+2+:::D'
CNI+3+:::D'
CNI+4+:::D'
CNI+5+:::D'
CNI+6+:::D'
UNT+21+<<MSGNO PLACEHOLDER>>'";
			AssertMultilineASCIIEquals("Change Message testing - MessageText should be a change message with only 1 existing HWBs remaining, the other 5 being deleted HWBs - (Lines 2, 3, 4, 5 & 6)", expectedChangeMsgString, changeBuilder.GeneratedMessageStrings[0].Replace("'", "'\r\n"));
		}

		public void TestESMChangeMessageMoreThan10Lines()
		{
			var testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_UniqueConsignRef = "C00026895";
			testConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			testConsol.JK_RL_NKDischargePort = "CATOR";
			testConsol.JK_RL_NKLoadPort = "AUSYD";
			testConsol.JK_MasterBillNum = "081-42424292";
			EnsureQantasAirLineCodeSet();
			var transport = testConsol.Transports[0];
			transport.JW_VoyageFlight = "QF112";
			transport.JW_ATD = new ZDateTime(new DateTime(2014, 10, 24));

			var shipment1 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00040001", 1, true);
			var shipment2 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00040002", 2, true);
			var shipment3 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00040003", 3, true);
			var shipment4 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00040004", 4, true);
			var shipment5 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00040005", 5, true);
			var shipment6 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00040006", 6, true);
			var shipment7 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00040007", 7, true);
			var shipment8 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00040008", 8, true);
			var shipment9 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00040009", 9, true);
			var shipment10 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00040010", 10, true);
			var shipment11 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00040011", 11, true);
			var shipment12 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00040012", 12, true);
			var shipment13 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00040013", 13, true);
			var shipment14 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00040014", 14, true);

			var builder = new ESMMessageBuilder(testConsol, false);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("NumberOfMessagesGenerated", 1, builder.GeneratedMessageStrings.Length);
			var expectedString = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+87:::ESM+<<SENDERS REFERENCE PLACE HOLDER>>/DAT1:1+9'
TDT+20+++6'
DTM+136:20141024:102'
GIS+C:121:95'
CNT+11:140'
CNI+1'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040001'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+2'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040002'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+3'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040003'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+4'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040004'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+5'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040005'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+6'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040006'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+7'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040007'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+8'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040008'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+9'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040009'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+10'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040010'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+11'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040011'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+12'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040012'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+13'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040013'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+14'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040014'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
UNT+119+<<MSGNO PLACEHOLDER>>'";
			AssertMultilineASCIIEquals("MessageText", expectedString, builder.GeneratedMessageStrings[0].Replace("'", "'\r\n"));

			var permit = testConsol.CusEntryNums.AddNew();
			permit.CE_EntryType = "CRN";
			permit.CE_EntryNum = "AAAC3EKWF";
			permit.CE_ParentID = testConsol.PK;
			permit.CE_ParentTable = testConsol.TableName;
			Factory.Save();

			var outgoingMessage1ORG = Factory.New<CMRMessage>();
			outgoingMessage1ORG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1ORG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1ORG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1ORG.EM_MessageSubType = "ORG";
			outgoingMessage1ORG.EM_Status = "CLR";
			outgoingMessage1ORG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 24, 06, 40, 00);
			outgoingMessage1ORG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026895/CMT1:1+9'TDT+20+++6'DTM+136:20141024:102'GIS+C:121:95'CNT+11:140'CNI+1'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040001'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+2'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040002'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+3'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040003'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+4'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040004'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+5'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040005'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+6'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040006'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+7'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040007'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+8'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040008'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+9'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040009'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+10'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040010'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+11'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040011'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+12'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040012'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+13'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040013'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+14'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040014'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'UNT+119+1'";
			testConsol.Messages.Add(outgoingMessage1ORG);

			var responseMessage1CLR = Factory.New<CMRMessage>();
			responseMessage1CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1CLR.EM_MessageSubType = "CLR";
			responseMessage1CLR.EM_Status = "RCV";
			responseMessage1CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 24, 06, 41, 00);
			responseMessage1CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+331D G3I5 9EG4:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026895/CMT1::001'RFF+ACW:ESM'RFF+AFM:9'RFF+AIZ:AAAC3EKWF'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0003'FTX+AHN+++CLEAR'CNT+5:0003'UNT+22+000001'";
			testConsol.Messages.Add(responseMessage1CLR);

			var shipment15 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S000400015", 15, false);
			var shipment16 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S000400016", 16, false);

			var changeBuilder = new ESMMessageBuilder(testConsol, false);
			changeBuilder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			AssertEquals("NumberOfMessagesGenerated", 1, changeBuilder.GeneratedMessageStrings.Length);
			var expectedChangeMsgString = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+87:::ESM+C00026895/CMT1:2+4'
RFF+AIZ:AAAC3EKWF'
TDT+20+++6'
DTM+136:20141024:102'
GIS+C:121:95'
CNT+11:160'
CNI+1+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040001'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+2+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040002'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+3+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040003'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+4+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040004'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+5+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040005'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+6+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040006'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+7+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040007'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+8+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040008'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+9+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040009'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+10+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040010'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+11+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040011'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+12+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040012'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+13+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040013'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+14+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00040014'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+15+:::I'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S000400015'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+16+:::I'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S000400016'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
UNT+136+<<MSGNO PLACEHOLDER>>'";
			AssertMultilineASCIIEquals("Deletion change message testing - MessageText should be a change message with 3 remaining HWBs and 1 deleted HWB - line 5", expectedChangeMsgString, changeBuilder.GeneratedMessageStrings[0].Replace("'", "'\r\n"));
		}

		public void TestESMChangeMessageDeletionOfLastLine()
		{
			var testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_UniqueConsignRef = "C00026895";
			testConsol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			testConsol.JK_RL_NKDischargePort = "CATOR";
			testConsol.JK_RL_NKLoadPort = "AUSYD";
			testConsol.JK_MasterBillNum = "081-42424292";
			EnsureQantasAirLineCodeSet();
			var transport = testConsol.Transports[0];
			transport.JW_VoyageFlight = "QF112";
			transport.JW_ATD = new ZDateTime(new DateTime(2014, 10, 24));

			var permit = testConsol.CusEntryNums.AddNew();
			permit.CE_EntryType = "CRN";
			permit.CE_EntryNum = "AAAC3EKWF";
			permit.CE_ParentID = testConsol.PK;
			permit.CE_ParentTable = testConsol.TableName;
			Factory.Save();

			AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047289", 1, true);
			var shipment2 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047290", 2, true);
			var shipment3 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047291", 3, true);

			var outgoingMessage1ORG = Factory.New<CMRMessage>();
			outgoingMessage1ORG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1ORG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1ORG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1ORG.EM_MessageSubType = "ORG";
			outgoingMessage1ORG.EM_Status = "CLR";
			outgoingMessage1ORG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 24, 06, 40, 00);
			outgoingMessage1ORG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026895/CMT1:1+9'TDT+20+++6'DTM+136:20141024:102'GIS+C:121:95'CNT+11:12'CNI+1'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047289'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++GOODS'CNI+2'CNT+11:4'RFF+TL:EXLV'GID+1'RFF+HWB:S00047290'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++MAGAZINES'CNI+3'CNT+11:5'RFF+TL:EXLV'GID+1'RFF+HWB:S00047291'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++NEWSPAPER PRINT'UNT+34+1'";
			testConsol.Messages.Add(outgoingMessage1ORG);

			var responseMessage1CLR = Factory.New<CMRMessage>();
			responseMessage1CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1CLR.EM_MessageSubType = "CLR";
			responseMessage1CLR.EM_Status = "RCV";
			responseMessage1CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 24, 06, 41, 00);
			responseMessage1CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+331D G3I5 9EG4:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026895/CMT1::001'RFF+ACW:ESM'RFF+AFM:9'RFF+AIZ:AAAC3EKWF'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0003'FTX+AHN+++CLEAR'CNT+5:0003'UNT+22+000001'";
			testConsol.Messages.Add(responseMessage1CLR);

			var shipment4 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047292", 4, false);
			var shipment5 = AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047293", 5, true);
			AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047294", 6, true);

			var outgoingMessage2CHG = Factory.New<CMRMessage>();
			outgoingMessage2CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage2CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage2CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2CHG.EM_MessageSubType = "ORG";
			outgoingMessage2CHG.EM_Status = "SNT";
			outgoingMessage2CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 24, 06, 46, 00);
			outgoingMessage2CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026895/CMT1:2+4'RFF+AIZ:AAAC3EKWF'TDT+20+++6'DTM+136:20141024:102'GIS+C:121:95'CNT+11:22'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047289'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++GOODS'CNI+2+:::A'CNT+11:4'RFF+TL:EXLV'GID+1'RFF+HWB:S00047290'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++MAGAZINES'CNI+3+:::A'CNT+11:5'RFF+TL:EXLV'GID+1'RFF+HWB:S00047291'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++NEWSPAPER PRINT'CNI+4+:::I'CNT+11:2'RFF+TL:EXLV'GID+1'RFF+HWB:S00047292'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++MATERIAL'CNI+5+:::I'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047293'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF'CNI+6+:::I'CNT+11:7'RFF+TL:EXLV'GID+1'RFF+HWB:S00047294'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PRINTED DOCUMENTS'UNT+62+1'";
			testConsol.Messages.Add(outgoingMessage2CHG);

			var responseMessage2CLR = Factory.New<CMRMessage>();
			responseMessage2CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage2CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage2CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage2CLR.EM_MessageSubType = "CLR";
			responseMessage2CLR.EM_Status = "RCV";
			responseMessage2CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 24, 06, 47, 00);
			responseMessage2CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+3533 77F2 JEG4:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026895/CMT1::002'RFF+ACW:ESM'RFF+AFM:4'RFF+AIZ:AAAC3EKWF'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0002'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0003'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0004'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0005'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0006'FTX+AHN+++CLEAR'CNT+5:0006'UNT+34+000001'";
			testConsol.Messages.Add(responseMessage2CLR);

			AddAdditionalShipmentAndDeclarationToConsol(testConsol, "S00047299", 7, true);
			testConsol.Shipments.Remove(shipment2);
			testConsol.Shipments.Remove(shipment3);
			testConsol.Shipments.Remove(shipment4);

			var outgoingMessage3CHG = Factory.New<CMRMessage>();
			outgoingMessage3CHG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage3CHG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage3CHG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage3CHG.EM_MessageSubType = "CHG";
			outgoingMessage3CHG.EM_Status = "SNT";
			outgoingMessage3CHG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 27, 05, 48, 00);
			outgoingMessage3CHG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026895/CMT1:3+4'RFF+AIZ:AAAC3EKWF'TDT+20+++6'DTM+136:20141024:102'GIS+C:121:95'CNT+11:15'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047289'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++GOODS'CNI+5+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047293'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF'CNI+6+:::A'CNT+11:7'RFF+TL:EXLV'GID+1'RFF+HWB:S00047294'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++PRINTED DOCUMENTS'CNI+7+:::I'CNT+11:4'RFF+TL:EXLV'GID+1'RFF+HWB:S00047299'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++TESTING CHANGE MSSG DELETIONS'CNI+2+:::D'CNI+3+:::D'CNI+4+:::D'UNT+47+1'";
			testConsol.Messages.Add(outgoingMessage3CHG);

			var responseMessage3CLR = Factory.New<CMRMessage>();
			responseMessage3CLR.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage3CLR.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage3CLR.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage3CLR.EM_MessageSubType = "CLR";
			responseMessage3CLR.EM_Status = "RCV";
			responseMessage3CLR.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 27, 05, 49, 00);
			responseMessage3CLR.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::ESMR+4245 D4JA 7A1E:001+11'FTX+AHN+++CLEAR:THE GOODS COVERED BY THE DOCUMENT ARE AUTHORISED FOR EXPORT AND MAY BE DEALT WITH.'NAD+MR+41065894724::95'RFF+ABO:C00026895/CMT1::003'RFF+ACW:ESM'RFF+AFM:4'RFF+AIZ:AAAC3EKWF'DOC+1'RFF+TL:EXLV'CST+0001'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0005'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0006'FTX+AHN+++CLEAR'DOC+1'RFF+TL:EXLV'CST+0007'FTX+AHN+++CLEAR'CNT+5:0004'UNT+26+000001'";
			testConsol.Messages.Add(responseMessage3CLR);

			var shipment = new FreightShipmentWrapper(shipment2, testConsol);
			shipment.ShipmentManifestLineSequence.CY_Code = CustomsManifestLineSequence.DataCodes.DeletedLineNumber;
			shipment = new FreightShipmentWrapper(shipment3, testConsol);
			shipment.ShipmentManifestLineSequence.CY_Code = CustomsManifestLineSequence.DataCodes.DeletedLineNumber;
			testConsol.Shipments.Remove(shipment5);

			var changeBuilder = new ESMMessageBuilder(testConsol, false);
			changeBuilder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			AssertEquals("NumberOfMessagesGenerated", 1, changeBuilder.GeneratedMessageStrings.Length);
			var expectedChangeMsgString = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'
BGM+87:::ESM+C00026895/CMT1:4+4'
RFF+AIZ:AAAC3EKWF'
TDT+20+++6'
DTM+136:20141024:102'
GIS+C:121:95'
CNT+11:30'
CNI+1+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00047289'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+5+:::D'
CNI+6+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00047294'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
CNI+7+:::A'
CNT+11:10'
RFF+TL:EXPE'
GID+1'
RFF+HWB:S00047299'
NAD+GO+++JACK JONES'
GID+1'
FTX+AAA+++FISH LIVERS AND ROES, FRESH'
UNT+33+<<MSGNO PLACEHOLDER>>'";
			AssertMultilineASCIIEquals("Deletion change message testing - MessageText should be a change message with 3 remaining HWBs and 1 deleted HWB - line 5", expectedChangeMsgString, changeBuilder.GeneratedMessageStrings[0].Replace("'", "'\r\n"));
		}

		public void TestCheckESMConsignmentCount()
		{
			ZArchitecture.Environment.DataRegistry.Instance.MaximumConsignmentsLines = 10;
			var expectedError = "The total consignment lines for this Consol exceeds the maximum allowed consignments for an Export Sub Manifest Message.\r\n";

			var consol = CreateSimpleESMConsol();
			consol.Shipments.AddNew();

			var hvlShipmentWithConsignments = consol.Shipments.AddNew();
			var header = Factory.New<IHVLVConsignmentHeader>() as BusinessObject;
			header.FillWithValidTestData();
			header[HVLVConsignmentHeaderSchema.HCH_JS_Shipment.Name] = hvlShipmentWithConsignments.PK;
			hvlShipmentWithConsignments.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;

			var hvlShipmentWithoutConsignments = consol.Shipments.AddNew();
			hvlShipmentWithoutConsignments.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;

			var hvmlShipment = consol.Shipments.AddNew();
			hvmlShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueMaster;

			AddTestConsignments(1, header);
			var builder = new ESMMessageBuilder(consol, false);
			var messages = builder.GeneratedMessageStrings;
			Assert("Expect CheckESMConsignmentCount passes when consignment count is smaller than MaximumConsignmentsLines", !builder.Errors.Contains(expectedError));

			AddTestConsignments(9, header);
			builder = new ESMMessageBuilder(consol, false);
			messages = builder.GeneratedMessageStrings;
			Assert("Expect CheckESMConsignmentCount passes when consignment count equals to MaximumConsignmentsLines", !builder.Errors.Contains(expectedError));

			AddTestConsignments(1, header);
			builder = new ESMMessageBuilder(consol, false);
			messages = builder.GeneratedMessageStrings;
			Assert("Expect CheckESMConsignmentCount gives error when consignment count is greater than MaximumConsignmentsLines", builder.Errors.Contains(expectedError));
		}

		void AddTestConsignments(int numberToAdd, BusinessObject header)
		{
			while (numberToAdd > 0)
			{
				var consignment = Factory.New<IHVLVConsignment>() as BusinessObject;
				consignment[HVLVConsignmentSchema.HVC_HCH_Header] = header.PK;

				numberToAdd--;
			}
			Factory.Save();
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestErrorFreeConsol()
		{
			var consol = GetErrorFreeConsolForValidationTesting();
			AssertNumberOfErrors(consol, 0);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestCoLoadMasterWithoutCANIsValid()
		{
			var consol = CreateSimpleESMConsol();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			var shipment3 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment2.JS_JS_ColoadMasterShipment = shipment1.PK;
			shipment3.JS_JS_ColoadMasterShipment = shipment1.PK;
			shipment2.CustomsEntryNumberType = "CAN";
			shipment2.CustomsEntryNumber = "AAAAAAMP7";
			shipment2.JS_OuterPacks = 10;
			shipment3.CustomsEntryNumberType = CANType.Exemptions.EXDD.Code;
			shipment3.JS_OuterPacks = 10;
			AssertNumberOfErrors(consol, 0);

			AssertEquals("NumberOfLines", 2, new FreightConsolManifestHeaderWrapper(consol).Lines.Length);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestCoLoadsWithoutCANIsValid()
		{
			var consol = CreateSimpleESMConsol();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			var shipment3 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment1.JS_OuterPacks = 10;
			shipment1.CustomsEntryNumberType = "CAN";
			shipment1.CustomsEntryNumber = "AAAAAAMP7";

			shipment2.JS_JS_ColoadMasterShipment = shipment1.PK;
			shipment3.JS_JS_ColoadMasterShipment = shipment1.PK;
			shipment2.JS_OuterPacks = 10;
			shipment3.CustomsEntryNumberType = CANType.Exemptions.EXDD.Code;
			shipment3.JS_OuterPacks = 10;
			AssertNumberOfErrors(consol, 0);

			AssertEquals("NumberOfLines", 1, new FreightConsolManifestHeaderWrapper(consol).Lines.Length);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestCoLoadMasterWithoutCANIsInvalidWhenChildShipmentHasNoCAN()
		{
			var consol = CreateSimpleESMConsol();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			var shipment3 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment2.JS_JS_ColoadMasterShipment = shipment1.PK;
			shipment3.JS_JS_ColoadMasterShipment = shipment1.PK;
			shipment2.CustomsEntryNumberType = "CAN";
			shipment2.CustomsEntryNumber = "AAAAAAMP7";
			shipment2.JS_OuterPacks = 10;
			shipment3.JS_OuterPacks = 10;
			AssertNumberOfErrors(consol, 1);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestErrorReceivedIfCertificateProblems()
		{
			var consol = GetErrorFreeConsolForValidationTesting();
			Env.Registry.AUCCompanyCertificatePassword = ZString.Empty;
			AssertNumberOfErrors(consol, 1);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestErrorIfDateOfDepartureMissing()
		{
			var consol = GetErrorFreeConsolForValidationTesting();

			var transport = consol.Transports[0];
			transport.JW_ETD = ZDateTime.Empty;
			transport.JW_ATD = ZDateTime.Empty;
			AssertNumberOfErrors(consol, 1);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestErrorIfTransportModeMissing()
		{
			var consol = GetErrorFreeConsolForValidationTesting();
			consol.JK_TransportMode = ZString.Empty;
			AssertNumberOfErrors(consol, 1);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestInvalidLengthCAN()
		{
			var consol = GetErrorFreeConsolForValidationTesting();
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryType = CANType.CustomsAuthorityNumber.Code;
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryNum = "AAAA";
			AssertNumberOfErrors(consol, 1);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestInvalidCharacterCAN()
		{
			var consol = GetErrorFreeConsolForValidationTesting();
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryType = CANType.CustomsAuthorityNumber.Code;
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryNum = "111111111";
			AssertNumberOfErrors(consol, 1);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestMissingCANAndExemption()
		{
			var consol = GetErrorFreeConsolForValidationTesting();
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryNum = ZString.Empty;
			AssertNumberOfErrors(consol, 1);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestInvalidCheckDigitCAN()
		{
			var consol = GetErrorFreeConsolForValidationTesting();
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryType = CANType.CustomsAuthorityNumber.Code;
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryNum = "AAAAJML6A";
			AssertNumberOfErrors(consol, 1);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestTooLongCCAN()
		{
			var consol = GetErrorFreeConsolForValidationTesting();
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryType = CANType.ContingencyCustomsAuthorityNumber.Code;
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryNum = "123456789012345";
			AssertNumberOfErrors(consol, 1);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestTooShortCCAN()
		{
			var consol = GetErrorFreeConsolForValidationTesting();
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryType = CANType.ContingencyCustomsAuthorityNumber.Code;
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryNum = "1234567890123";
			AssertNumberOfErrors(consol, 1);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestRepeatedCAN()
		{
			var consol = GetErrorFreeConsolForValidationTesting();
			AddShipmentAndDeclarationToConsol(consol);
			AssertNumberOfErrors(consol, 1);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestSeaLineContainerCountMissing()
		{
			var consol = GetErrorFreeConsolForValidationTesting();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			UnpackContainers(consol.Shipments[0]);
			consol.Shipments[0].JS_OuterPacks = 0;
			AssertNumberOfErrors(consol, 1);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestAirLinePackageCountMissing()
		{
			var consol = GetErrorFreeConsolForValidationTesting();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			UnpackContainers(consol.Shipments[0]);
			consol.Shipments[0].JS_OuterPacks = 0;
			AssertNumberOfErrors(consol, 1);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestAirContainerCountPositiveDontError()
		{
			var consol = GetErrorFreeConsolForValidationTesting();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.Shipments[0].JS_OuterPacks = 1;
			AssertNumberOfErrors(consol, 0);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestLineExemptionOriginMissing()
		{
			var consol = GetErrorFreeConsolForValidationTesting();
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryType = CANType.Exemptions.EXLV.Code;
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryNum = ZString.Empty;
			consol.Shipments[0].JS_RL_NKDestination = ZString.Empty;
			AssertNumberOfErrors(consol, 1);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestLineExemptionConsignorMissing()
		{
			var consol = GetErrorFreeConsolForValidationTesting();
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryType = CANType.Exemptions.EXLV.Code;
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryNum = ZString.Empty;
			consol.Shipments[0].ConsignorPK = ZGuid.Empty;
			AssertNumberOfErrors(consol, 1);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestLineExemptionDescriptionMissing()
		{
			var consol = GetErrorFreeConsolForValidationTesting();
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryType = CANType.Exemptions.EXLV.Code;
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryNum = ZString.Empty;
			consol.Shipments[0].JS_GoodsDescription = ZString.Empty;
			AssertNumberOfErrors(consol, 1);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestLineExemptionOriginMissingForExit2()
		{
			var consol = GetErrorFreeConsolForValidationTesting();
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryType = CusEntryNumberTypes.Australia.EX1;
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryNum = ZString.Empty;
			consol.Shipments[0].JS_RL_NKDestination = ZString.Empty;
			AssertNumberOfErrors(consol, 1);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestLineExemptionConsignorMissingForExit2()
		{
			var consol = GetErrorFreeConsolForValidationTesting();
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryType = CusEntryNumberTypes.Australia.EX1;
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryNum = ZString.Empty;
			consol.Shipments[0].ConsignorPK = ZGuid.Empty;
			AssertNumberOfErrors(consol, 1);
		}

		//			[NUnit.Framework.TestDate(2005, 10, 1)]
		public void TestLineExemptionDescriptionMissingForExit2()
		{
			var consol = GetErrorFreeConsolForValidationTesting();
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryType = CusEntryNumberTypes.Australia.EX2;
			new FreightShipmentWrapper(consol.Shipments[0], consol).GetExistingPermit().CE_EntryNum = ZString.Empty;
			consol.Shipments[0].JS_GoodsDescription = ZString.Empty;
			AssertNumberOfErrors(consol, 1);
		}

		protected override IManifestMessageBuilder NewMessageBuilder(Common.MessageBuilders.MessageSubTypes messageSubType, object data)
		{
			var result = new ESMMessageBuilder((ForwardingConsol)data, false);
			result.MessageSubType = messageSubType;
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;

		void UnpackContainers(CommonShipment shipment)
		{
			foreach (var container in shipment.Containers.ToArray())
			{
				shipment.UnpackFromContainer(container);
			}
		}

		void AssertContingencyWriteOffConditions(ESMMessageBuilder builder)
		{
			AssertEquals("NumberOfMessagesGenerated", 1, builder.GeneratedMessageStrings.Length);
			Assert("GeneratedTextContainsCCANWriteOffLine", builder.GeneratedMessageStrings[0].IndexOf("CNI+1'CNT+11:10'RFF+TL:EXCC'GID+1'RFF+AHV:12345678901234'GID+1'") != -1);
			Assert("2 Lines in Total", builder.GeneratedMessageStrings[0].IndexOf("CNI+2'") != -1);
			Assert("TodaysDateSent", builder.GeneratedMessageStrings[0].IndexOf("DTM+136:" + ZDateTime.Now.ToString("yyyyMMdd") + ":102'") != -1);
		}

		string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.MessageBuilders.Export.Manifest.TestFiles." + fileName;

		void AssertNumberOfErrors(ForwardingConsol consol, int expectedErrors)
		{
			var builder = new ESMMessageBuilder(consol, false);
			builder.MessageSubType = ESMMessageBuilder.GetMessageSubType(consol);
			AssertEquals("NumberOfErrors", expectedErrors, builder.ErrorCount);
		}

		ForwardingConsol GetErrorFreeConsolForValidationTesting()
		{
			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var testConsol = CreateSimpleESMConsol();
			AddShipmentAndDeclarationToConsol(testConsol);
			return testConsol;
		}

		void AddExemptionCode(CommonShipment shipment, ZString exemptionCode)
		{
			var cCANEntryNum = Factory.New<AUCusEntryNumber>();
			cCANEntryNum.CE_EntryType = exemptionCode;
			cCANEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cCANEntryNum.CE_ParentID = shipment.PK;
			cCANEntryNum.CE_ParentTable = shipment.TableName;
		}

		void AddAirCargoStatus(CommonShipment shipment, ZString status)
		{
			var entryNum = Factory.New<AUCusEntryNumber>();
			entryNum.CE_EntryType = CusEntryNumber.EntryType.ImportManifestStatus;
			entryNum.CE_EntryNum = status;
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_ParentID = shipment.PK;
			entryNum.CE_ParentTable = shipment.TableName;
		}

		void AddContingencyCAN(CommonShipment shipment, ZString cCAN)
		{
			var cCANEntryNum = Factory.New<AUCusEntryNumber>();
			cCANEntryNum.CE_EntryType = CANType.ContingencyCustomsAuthorityNumber.Code;
			cCANEntryNum.CE_EntryNum = cCAN;
			cCANEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			cCANEntryNum.CE_ParentID = shipment.PK;
			cCANEntryNum.CE_ParentTable = shipment.TableName;
		}

		ExportCustomsManifestHeader CreateSimpleESMExportCustomsManifestHeader()
		{
			var result = Factory.New<ExportCustomsManifestHeader>();
			result.ED_BGMReference = "200401270010";
			result.ED_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			result.ED_RN_NKCountryOfDestination = "NZ";
			result.ED_RL_NKPortOfDeparture = "AUSYD";
			//EnsureQantasAirLineCodeSet();
			result.ED_FlightNumber = "QF112";
			result.ED_AirWayBill = "08100000011";
			result.ED_NoOfPacks = 10;

			result.ED_DepartureDate = new ZDateTime(new DateTime(2004, 1, 28));

			return result;
		}

		ExportCustomsManifestLines AddLine(ExportCustomsManifestHeader header, ZString cAN)
		{
			var line = header.Lines.AddNew();
			line.EL_NumberOfPackages = 10;
			line.EL_GoodsDescription = "FISH LIVERS AND ROES, FRESH";
			line.EL_CAN = cAN;
			line.EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
			line.EL_RN_NKCountryOfDestination = "NZ";

			line.EL_GoodsOwner = "JACK JONES";
			line.EL_GoodsOwnerPartyID = "1234567890";
			return line;
		}

		ForwardingConsol CreateSimpleESMConsol()
		{
			var result = Factory.New<ForwardingConsol>();
			result.JK_UniqueConsignRef = "200401270010";
			result.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			result.JK_RL_NKDischargePort = "NZAKL";
			result.JK_RL_NKLoadPort = "AUSYD";
			EnsureQantasAirLineCodeSet();
			var transport = result.Transports[0];
			transport.JW_VoyageFlight = "QF112";
			result.JK_MasterBillNum = "081-00000011";

			transport.JW_ATD = new ZDateTime(new DateTime(2004, 1, 28));
			return result;
		}

		CommonShipment AddShipmentAndDeclarationToConsol(ForwardingConsol consol) => AddShipmentAndDeclarationToConsol(consol, CANType.CustomsAuthorityNumber.Code);

		CommonShipment AddShipmentAndDeclarationToConsol(ForwardingConsol consol, ZString entryNumType)
		{
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 10;
			shipment.JS_GoodsDescription = "FISH LIVERS AND ROES, FRESH";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_JS = shipment.PK;
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentTable = declaration.TableName;
			entryNumber.CE_ParentID = declaration.PK;

			entryNumber.CE_EntryNum = "AAAAAAMP7";
			entryNumber.CE_EntryType = entryNumType;
			entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "JACK JONES";
			consignor.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, "1234567890");
			shipment.ConsignorPK = consignor.PK;
			return shipment;
		}

		CommonShipment AddAdditionalShipmentAndDeclarationToConsol(ForwardingConsol consol, ZString hawbNo, int consignmentNo, bool updateToManifested)
		{
			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = hawbNo;
			shipment.JS_HouseBill = hawbNo;
			shipment.JS_OuterPacks = 10;
			shipment.JS_GoodsDescription = "FISH LIVERS AND ROES, FRESH";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_JS = shipment.PK;
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentTable = declaration.TableName;
			entryNumber.CE_ParentID = declaration.PK;

			entryNumber.CE_EntryType = CusEntryNumberTypes.Australia.EX1;
			entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var consignor = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "JACJONSYD");
			if (consignor == null)
			{
				consignor = Factory.New<OrgHeader>();
				consignor.OH_FullName = "JACK JONES";
				consignor.OH_Code = "JACJONSYD";
				consignor.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, "1234567890");
			}
			shipment.ConsignorPK = consignor.PK;

			var shipmentWrapper = new FreightShipmentWrapper(shipment, consol);
			shipmentWrapper.CreatePreliminaryManifestLineNumber(consignmentNo);
			if (updateToManifested)
			{
				shipmentWrapper.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			return shipment;
		}

		void UnpackAllContainers(CommonShipment shipment)
		{
			foreach (var container in shipment.Containers.ToArray())
			{
				shipment.UnpackFromContainer(container);
			}
		}
	}
}
