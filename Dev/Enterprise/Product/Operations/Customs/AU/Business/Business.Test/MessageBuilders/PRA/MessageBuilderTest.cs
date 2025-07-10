using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class MessageBuilderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestNullContainerThrowsNoExceptions()
		{
			var container = GetNewContainer();
			var messagingData = new FreightDataLayer(container);
			messageBuilder = new MessageBuilder(messagingData, Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			_ = messageBuilder.GetMessageText();
		}

		public void TestEmptyContainerHasMandatoryFields()
		{
			var container = GetNewContainer();
			var messagingData = new FreightDataLayer(container);
			messagingData.IsEmptyContainer = true;
			messagingData.DateTimeStringForMessage = "20040402092831";
			messagingData.SenderID = "TSTSYD";
			messagingData.ConsignorName = "CONSIGNOR";
			messageBuilder = new MessageBuilder(messagingData, Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			var actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			AssertMultilineASCIIMessagesEquals("MessageResult", PRAMessageEmptyContainer, actualMessage);
		}

		public void TestMinimumRequirements()
		{
			var container = GetNewContainer();
			var messagingData = new FreightDataLayer(container);
			messagingData.MessageReference = "001";
			messagingData.DateTimeStringForMessage = "20040402092831";
			messagingData.SenderID = "TSTSYD";
			messagingData.ConsignorName = "CONSIGNOR";
			messagingData.ShippingLineBookingReference = "BOOKING123";
			messagingData.ECNorCRN = "CUSTOMS ECN";
			messagingData.ShippingLine1StopCode = "ANL";
			messagingData.VesselName = "MSC MARTINA";
			messagingData.Voyage = "123N";
			messagingData.LloydsNumber = "9060637";
			messagingData.PortOfLoading = "AUSYD";
			messagingData.LoadTerminal1StopCode = "ASLPB";
			messagingData.PortOfFinalDischarge = "IDJKT";
			messagingData.ContainerNumber = "TESU1234567";
			messagingData.ISOContainerType = "22G0";
			messagingData.Commodity1StopCode = "GENL";
			messagingData.ContainerGrossWeight = 11600m;
			messagingData.SealNumber = "289184";

			messageBuilder = new MessageBuilder(messagingData, Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			var actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			AssertMultilineASCIIMessagesEquals("MessageResult", PRAMessageMinimum, actualMessage);
		}

		public void TestCartageABN()
		{
			var container = GetNewContainer();
			var messagingData = new FreightDataLayer(container);
			messagingData.MessageReference = "001";
			messagingData.DateTimeStringForMessage = "20040402092831";
			messagingData.SenderID = "TSTSYD";
			messagingData.ConsignorName = "CONSIGNOR";
			messagingData.ShippingLineBookingReference = "BOOKING123";
			messagingData.ECNorCRN = "CUSTOMS ECN";
			messagingData.ShippingLine1StopCode = "ANL";
			messagingData.VesselName = "MSC MARTINA";
			messagingData.Voyage = "123N";
			messagingData.LloydsNumber = "9060637";
			messagingData.PortOfLoading = "AUSYD";
			messagingData.LoadTerminal1StopCode = "ASLPB";
			messagingData.PortOfFinalDischarge = "IDJKT";
			messagingData.ContainerNumber = "TESU1234567";
			messagingData.ISOContainerType = "22G0";
			messagingData.Commodity1StopCode = "GENL";
			messagingData.ContainerGrossWeight = 11600m;
			messagingData.SealNumber = "289184";
			messagingData.CartageCompanyABN = "123987432";

			messageBuilder = new MessageBuilder(messagingData, Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			var actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			AssertMultilineASCIIMessagesEquals("MessageResult", PRAMessageCartageABN, actualMessage);
		}

		public void TestTypicalRoad()
		{
			var container = GetNewContainer();
			var messagingData = new FreightDataLayer(container);
			messagingData.MessageReference = "001";
			messagingData.DateTimeStringForMessage = "20040402092831";

			messagingData.SenderID = "TSTSYD";
			messagingData.ConsignorName = "CONSIGNOR";
			messagingData.ShippingLineBookingReference = "BOOKING123";
			messagingData.ECNorCRN = "CUSTOMS ECN";
			messagingData.TerminalVBSBooking = "VBS BOOKING";

			messagingData.CartageBookingReference = "CN123";
			messagingData.CartageCompanyABN = "CARRIER ABN";
			messagingData.TruckRegoNumber = "TRUCK REGO";
			messagingData.RoadOrig1StopCode = "DEPOT";
			messagingData.RoadDest1StopCode = "TERMINAL";
			messagingData.RoadScheduledArrival = new ZDateTime(2004, 4, 2, 11, 30, 0);

			messagingData.ShippingLine1StopCode = "ANL";
			messagingData.VesselName = "MSC MARTINA";
			messagingData.Voyage = "123N";
			messagingData.LloydsNumber = "9060637";
			messagingData.PortOfLoading = "AUSYD";
			messagingData.LoadTerminal1StopCode = "ASLPB";
			messagingData.PortOfDischarge = "IDJKT";
			messagingData.PortOfFinalDischarge = "SGSIN";

			messagingData.ContainerNumber = "TESU1234567";
			messagingData.ISOContainerType = "22G0";
			messagingData.Commodity1StopCode = "GENL";
			messagingData.ContainerGrossWeight = 11600m;
			messagingData.ContainerNetWeight = 1600m;
			messagingData.SealNumber = "289184";

			messagingData.GoodsDescription = "GENERAL CARGO";

			messageBuilder = new MessageBuilder(messagingData, Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			var actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			const string expectedMessage = @"
UNH+<<MSGNO PLACEHOLDER>>+IFTERA:D:98B:RT:ENET54'
BGM+ERA+TSTSYD+9+AQ'
DTM+137:20040402092831:204'
NAD+MR+ASLPB'
NAD+MS+TSTSYD'
CTA+IC+:DEVELOPER'
COM+1-STOPPRA@EDI.NET.AU:EM'
RFF+BN:BOOKING123'
RFF+ERN:001'
TDT+10+CN123+3++CARRIER ABN+++TRUCK REGO'
LOC+88++DEPOT'
LOC+7++TERMINAL'
DTM+232:200404021130:203'
TDT+20+123N+1++ANL+++9060637:::MSC MARTINA'
LOC+9+AUSYD+ASLPB'
LOC+11+IDJKT'
LOC+7+SGSIN'
EQD+CN+TESU1234567+22G0++2+5'
HAN+:::GENL'
NAD+CZ+CONSIGNOR'
MEA+AAE+G+KGM:11600'
SEL+289184+AB+1'
FTX+ZO1+++GENERAL CARGO'
RFF+AAE:CUSTOMS ECN'
RFF+CN:VBS BOOKING'
UNT+26+<<MSGNO PLACEHOLDER>>'";
			AssertMultilineASCIIMessagesEquals("MessageResult", expectedMessage, actualMessage);
		}

		public void TestTypicalRail()
		{
			var container = GetNewContainer();
			var messagingData = new FreightDataLayer(container);
			messagingData.MessageReference = "001";
			messagingData.DateTimeStringForMessage = "20040402092831";

			messagingData.SenderID = "TSTSYD";
			messagingData.ConsignorName = "CONSIGNOR";
			messagingData.ShippingLineBookingReference = "BOOKING123";
			messagingData.ECNorCRN = "CUSTOMS ECN";
			messagingData.TerminalVBSBooking = "VBS BOOKING";

			messagingData.ArrivingAtCTOByRail = true;
			messagingData.CartageBookingReference = "CN123";
			messagingData.CartageCompanyABN = "CARRIER ABN";
			messagingData.TruckRegoNumber = "TRUCK REGO";
			messagingData.RoadOrig1StopCode = "DEPOT";
			messagingData.RoadDest1StopCode = "TERMINAL";
			messagingData.RoadScheduledArrival = new ZDateTime(2004, 4, 2, 11, 30, 0);

			messagingData.ShippingLine1StopCode = "ANL";
			messagingData.VesselName = "MSC MARTINA";
			messagingData.Voyage = "123N";
			messagingData.LloydsNumber = "9060637";
			messagingData.PortOfLoading = "AUSYD";
			messagingData.LoadTerminal1StopCode = "ASLPB";
			messagingData.PortOfDischarge = "IDJKT";
			messagingData.PortOfFinalDischarge = "SGSIN";

			messagingData.ContainerNumber = "TESU1234567";
			messagingData.ISOContainerType = "22G0";
			messagingData.Commodity1StopCode = "GENL";
			messagingData.ContainerGrossWeight = 11600m;
			messagingData.ContainerNetWeight = 1600m;
			messagingData.SealNumber = "289184";

			messagingData.GoodsDescription = "GENERAL CARGO";

			messageBuilder = new MessageBuilder(messagingData, Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			var actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			const string expectedMessage = @"
UNH+<<MSGNO PLACEHOLDER>>+IFTERA:D:98B:RT:ENET54'
BGM+ERA+TSTSYD+9+AQ'
DTM+137:20040402092831:204'
NAD+MR+ASLPB'
NAD+MS+TSTSYD'
CTA+IC+:DEVELOPER'
COM+1-STOPPRA@EDI.NET.AU:EM'
RFF+BN:BOOKING123'
RFF+ERN:001'
TDT+10+CN123+2++CARRIER ABN+++TRUCK REGO'
LOC+88++DEPOT'
LOC+7++TERMINAL'
DTM+232:200404021130:203'
TDT+20+123N+1++ANL+++9060637:::MSC MARTINA'
LOC+9+AUSYD+ASLPB'
LOC+11+IDJKT'
LOC+7+SGSIN'
EQD+CN+TESU1234567+22G0++2+5'
HAN+:::GENL'
NAD+CZ+CONSIGNOR'
MEA+AAE+G+KGM:11600'
SEL+289184+AB+1'
FTX+ZO1+++GENERAL CARGO'
RFF+AAE:CUSTOMS ECN'
RFF+CN:VBS BOOKING'
UNT+26+<<MSGNO PLACEHOLDER>>'";
			AssertMultilineASCIIMessagesEquals("MessageResult", expectedMessage, actualMessage);
		}

		public void TestRefrigeratedContainer()
		{
			var container = GetNewContainer();
			var messagingData = new FreightDataLayer(container);
			messagingData.MessageReference = "001";
			messagingData.DateTimeStringForMessage = "20040402092831";

			messagingData.SenderID = "TSTSYD";
			messagingData.ConsignorName = "CONSIGNOR";
			messagingData.ShippingLineBookingReference = "BOOKING123";
			messagingData.ECNorCRN = "CUSTOMS ECN";

			messagingData.ShippingLine1StopCode = "ANL";
			messagingData.VesselName = "MSC MARTINA";
			messagingData.Voyage = "123N";
			messagingData.LloydsNumber = "9060637";
			messagingData.PortOfLoading = "AUSYD";
			messagingData.LoadTerminal1StopCode = "ASLPB";
			messagingData.PortOfDischarge = "IDJKT";
			messagingData.PortOfFinalDischarge = "SGSIN";

			messagingData.ContainerNumber = "TESU1234567";
			messagingData.ISOContainerType = "22R0";
			messagingData.Commodity1StopCode = "REEF";
			messagingData.ContainerGrossWeight = 14550m;
			messagingData.ContainerTareWeight = 2950m;
			messagingData.ContainerNetWeight = 11600m;
			messagingData.SealNumber = "289184";

			messagingData.IsTempControlled = true;
			messagingData.TemperatureSettingFormatted = "-18.0";
			messagingData.HumidityPercentage = 50;
			messagingData.AirVentSetting = 90;
			messagingData.AirVentSettingUnit = "P1";

			messageBuilder = new MessageBuilder(messagingData, Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			var actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			AssertMultilineASCIIMessagesEquals("MessageResult", PRAMessageReefer, actualMessage);
		}

		public void TestEmptyContainer()
		{
			var container = GetNewContainer();
			var messagingData = new FreightDataLayer(container);
			messagingData.MessageReference = "001";
			messagingData.DateTimeStringForMessage = "20040402092831";

			messagingData.SenderID = "TSTSYD";
			messagingData.ConsignorName = "CONSIGNOR";

			messagingData.ContainerNumber = "TESU1234567";
			messagingData.ContainerGrossWeight = 0;

			messagingData.IsEmptyContainer = true;

			messageBuilder = new MessageBuilder(messagingData, Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			var actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			AssertMultilineASCIIMessagesEquals("MessageResult", PRAMessageEmptyContainer, actualMessage);
		}

		public void TestEmptyContainerWithHazardousResidue()
		{
			var container = GetNewContainer();
			var messagingData = new FreightDataLayer(container);
			messagingData.MessageReference = "001";
			messagingData.DateTimeStringForMessage = "20040402092831";

			messagingData.SenderID = "TSTSYD";
			messagingData.ConsignorName = "CONSIGNOR";

			messagingData.ContainerNumber = "MHAZ9999995";
			messagingData.ContainerTareWeight = 5m;
			messagingData.ContainerGrossWeight = 6m;
			messagingData.GoodsDescription = "EMPTY - HAZARDOUS";
			messagingData.Commodity1StopCode = "MTHZ";
			messagingData.IsEmptyContainer = true;

			var dangerousGoodsList = messagingData.DangerousGoodsList.AddNew();
			dangerousGoodsList.UNDGNumber = "1021";
			dangerousGoodsList.IMDGClass = "2.1";
			dangerousGoodsList.FlashpointTemperatureInCelcius = "00.0";
			dangerousGoodsList.ContactName = "Joanna Hill";
			dangerousGoodsList.ContactEmailAddress = "johill@interchem.com.au";
			dangerousGoodsList.ContactPhoneNumber = "61 (3) 9270-9607";

			messageBuilder = new MessageBuilder(messagingData, Common.AU.PRAMessageTypeConstants.MessageType.Submit);

			var errorMessage = "Expected to have sent a PRA message with blank Carriers Reference Number as 1Stop requires it for empty containers with hazardous residue";
			var actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			AssertMultilineASCIIMessagesEquals(errorMessage, PRAMessageEmptyHazContainer, actualMessage);

			dangerousGoodsList.FlashpointTemperatureInCelcius = "33.3";
			messageBuilder = new MessageBuilder(messagingData, Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			AssertMultilineASCIIMessagesEquals(errorMessage, PRAMessageEmptyHazContainerWithFlashPoint, actualMessage);
		}

		public void TestContainerWithDangerousGoodsWeightRoundUp()
		{
			var container = GetNewContainer();
			var messagingData = new FreightDataLayer(container);
			messagingData.MessageReference = "001";
			messagingData.DateTimeStringForMessage = "20040402092831";

			messagingData.SenderID = "TSTSYD";
			messagingData.ConsignorName = "CONSIGNOR";

			messagingData.ContainerNumber = "MHAZ9999995";
			messagingData.ContainerTareWeight = 5m;
			messagingData.ContainerGrossWeight = 6m;
			messagingData.GoodsDescription = "EMPTY - HAZARDOUS";
			messagingData.Commodity1StopCode = "MTHZ";
			messagingData.IsEmptyContainer = true;

			var dangerousGoodsList = messagingData.DangerousGoodsList.AddNew();
			dangerousGoodsList.UNDGNumber = "1021";
			dangerousGoodsList.IMDGClass = "2.1";
			dangerousGoodsList.FlashpointTemperatureInCelcius = "33.3";
			dangerousGoodsList.ContactName = "Joanna Hill";
			dangerousGoodsList.ContactEmailAddress = "johill@interchem.com.au";
			dangerousGoodsList.ContactPhoneNumber = "61 (3) 9270-9607";
			dangerousGoodsList.Weight = 0.11;

			messageBuilder = new MessageBuilder(messagingData, Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			var actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			AssertMultilineASCIIMessagesEquals("Round to 1, because < 1 and > 0", GetPRAMessageWeightRoundUp("1"), actualMessage);

			dangerousGoodsList.Weight = 4.55;
			messageBuilder = new MessageBuilder(messagingData, Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			AssertMultilineASCIIMessagesEquals("Round to 5, because > 1", GetPRAMessageWeightRoundUp("5"), actualMessage);

			dangerousGoodsList.Weight = 0;
			messageBuilder = new MessageBuilder(messagingData, Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			AssertMultilineASCIIMessagesEquals("Don't round up 0", GetPRAMessageWeightRoundUp("0"), actualMessage);
		}

		public void TestOverhangContainer()
		{
			var container = GetNewContainer();
			var messagingData = new FreightDataLayer(container);
			messagingData.MessageReference = "001";
			messagingData.DateTimeStringForMessage = "20040402092831";

			messagingData.SenderID = "TSTSYD";
			messagingData.ConsignorName = "CONSIGNOR";
			messagingData.ShippingLineBookingReference = "BOOKING123";
			messagingData.ECNorCRN = "CUSTOMS ECN";

			messagingData.ShippingLine1StopCode = "ANL";
			messagingData.VesselName = "MSC MARTINA";
			messagingData.Voyage = "123N";
			messagingData.LloydsNumber = "9060637";
			messagingData.PortOfLoading = "AUSYD";
			messagingData.LoadTerminal1StopCode = "ASLPB";
			messagingData.PortOfDischarge = "IDJKT";
			messagingData.PortOfFinalDischarge = "SGSIN";

			messagingData.ContainerNumber = "TESU1234567";
			messagingData.ISOContainerType = "22G0";
			messagingData.Commodity1StopCode = "GENL";
			messagingData.ContainerGrossWeight = 11600m;
			messagingData.SealNumber = "289184";

			messagingData.OverhangFrontInCM = 300;
			messagingData.OverhangBackInCM = 20;
			messagingData.OverhangLeftInCM = 60;
			messagingData.OverhangRightInCM = 30;
			messagingData.OverhangHeightInCM = 10;

			messageBuilder = new MessageBuilder(messagingData, Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			var actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			AssertMultilineASCIIMessagesEquals("MessageResult", PRAMessageOverhangContainer, actualMessage);
		}

		public void TestAttachedEquipment()
		{
			var container = GetNewContainer();
			var messagingData = new FreightDataLayer(container);
			messagingData.MessageReference = "001";
			messagingData.DateTimeStringForMessage = "20040402092831";

			messagingData.SenderID = "TSTSYD";
			messagingData.ConsignorName = "CONSIGNOR";
			messagingData.ShippingLineBookingReference = "BOOKING123";
			messagingData.ECNorCRN = "CUSTOMS ECN";

			messagingData.ShippingLine1StopCode = "ANL";
			messagingData.VesselName = "MSC MARTINA";
			messagingData.Voyage = "123N";
			messagingData.LloydsNumber = "9060637";
			messagingData.PortOfLoading = "AUSYD";
			messagingData.LoadTerminal1StopCode = "ASLPB";
			messagingData.PortOfDischarge = "IDJKT";
			messagingData.PortOfFinalDischarge = "SGSIN";

			messagingData.ContainerNumber = "TESU1234567";
			messagingData.ISOContainerType = "22G0";
			messagingData.Commodity1StopCode = "GENL";
			messagingData.ContainerGrossWeight = 11600m;
			messagingData.SealNumber = "289184";

			messagingData.HasTynes = true;
			messagingData.FlatRackID = "FR789";
			messagingData.ReeferGeneratorID = "RG123";

			messageBuilder = new MessageBuilder(messagingData, Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			var actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			AssertMultilineASCIIMessagesEquals("MessageResult", PRAMessageAttachedEqiupment, actualMessage);
		}

		public void TestPostMessage()
		{
			var container = GetNewContainer();
			var messagingData = new FreightDataLayer(container);
			messagingData.MessageReference = "001";
			messagingData.DateTimeStringForMessage = "20040402092831";
			messagingData.SenderID = "TSTSYD";
			messagingData.ConsignorName = "CONSIGNOR";
			messagingData.ShippingLineBookingReference = "BOOKING123";
			messagingData.ECNorCRN = "CUSTOMS ECN";
			messagingData.ShippingLine1StopCode = "ANL";
			messagingData.VesselName = "MSC MARTINA";
			messagingData.Voyage = "123N";
			messagingData.LloydsNumber = "9060637";
			messagingData.PortOfLoading = "AUSYD";
			messagingData.LoadTerminal1StopCode = "ASLPB";
			messagingData.PortOfDischarge = "IDJKT";
			messagingData.ContainerNumber = "TESU1234567";
			messagingData.ISOContainerType = "22G0";
			messagingData.Commodity1StopCode = "GENL";
			messagingData.ContainerGrossWeight = 11600m;
			messagingData.SealNumber = "289184";

			messageBuilder = new MessageBuilder(messagingData, Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			messageBuilder.PostMessage();
			Factory.Save();

			AssertEquals(1, container.Messages.Count);
			AssertEquals("SSM", container.Messages[0].EM_MessageSubType);
		}

		public void TestReSendMessage()
		{
			var container = GetBasicMinimumFreightContainer();

			messageBuilder = new MessageBuilder(new FreightDataLayer(container), Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			var actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			AssertMultilineASCIIMessagesEquals("MessageResult", PRAMessageMinimum, actualMessage);
		}

		public void TestCancellation()
		{
			var container = GetNewContainer();
			var messagingData = new FreightDataLayer(container);
			messagingData.MessageReference = "001";
			messagingData.DateTimeStringForMessage = "20040402092831";

			messagingData.SenderID = "TSTSYD";
			messagingData.ConsignorName = "CONSIGNOR";
			messagingData.ShippingLineBookingReference = "BOOKING123";
			messagingData.ECNorCRN = "CUSTOMS ECN";
			messagingData.TerminalVBSBooking = "VBS BOOKING";

			messagingData.CartageBookingReference = "CN123";
			messagingData.CartageCompanyABN = "CARRIER ABN";
			messagingData.TruckRegoNumber = "TRUCK REGO";
			messagingData.RoadOrig1StopCode = "DEPOT";
			messagingData.RoadDest1StopCode = "TERMINAL";
			messagingData.RoadScheduledArrival = new ZDateTime(2004, 4, 2, 11, 30, 0);

			messagingData.ShippingLine1StopCode = "ANL";
			messagingData.VesselName = "MSC MARTINA";
			messagingData.Voyage = "123N";
			messagingData.LloydsNumber = "9060637";
			messagingData.PortOfLoading = "AUSYD";
			messagingData.LoadTerminal1StopCode = "ASLPB";
			messagingData.PortOfDischarge = "IDJKT";
			messagingData.PortOfFinalDischarge = "SGSIN";

			messagingData.ContainerNumber = "TESU1234567";
			messagingData.ISOContainerType = "22G0";
			messagingData.Commodity1StopCode = "GENL";
			messagingData.ContainerGrossWeight = 11600m;
			messagingData.ContainerNetWeight = 1600m;
			messagingData.SealNumber = "289184";

			messagingData.GoodsDescription = "GENERAL CARGO";

			messageBuilder = new MessageBuilder(messagingData, Common.AU.PRAMessageTypeConstants.MessageType.Cancel);
			var actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			const string expectedMessage = @"
UNH+<<MSGNO PLACEHOLDER>>+IFTERA:D:98B:RT:ENET54'
BGM+ERA+TSTSYD+1+AQ'
DTM+137:20040402092831:204'
NAD+MR+ASLPB'
NAD+MS+TSTSYD'
CTA+IC+:DEVELOPER'
COM+1-STOPPRA@EDI.NET.AU:EM'
RFF+BN:BOOKING123'
RFF+ERN:001'
TDT+10+CN123+3++CARRIER ABN+++TRUCK REGO'
LOC+88++DEPOT'
LOC+7++TERMINAL'
DTM+232:200404021130:203'
TDT+20+123N+1++ANL+++9060637:::MSC MARTINA'
LOC+9+AUSYD+ASLPB'
LOC+11+IDJKT'
LOC+7+SGSIN'
EQD+CN+TESU1234567+22G0++2+5'
HAN+:::GENL'
NAD+CZ+CONSIGNOR'
MEA+AAE+G+KGM:11600'
SEL+289184+AB+1'
FTX+ZO1+++GENERAL CARGO'
RFF+AAE:CUSTOMS ECN'
RFF+CN:VBS BOOKING'
UNT+26+<<MSGNO PLACEHOLDER>>'";
			AssertMultilineASCIIMessagesEquals("MessageResult", expectedMessage, actualMessage);
		}

		public void TestFreightBOMinimum()
		{
			var container = GetBasicMinimumFreightContainer();

			messageBuilder = new MessageBuilder(new FreightDataLayer(container), Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			var actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			AssertMultilineASCIIMessagesEquals("MessageResult", PRAMessageMinimum, actualMessage);
		}

		public void TestFreightMinimumWithAtmosphere()
		{
			var container = GetBasicMinimumFreightContainer();
			container.JC_IsControlledAtmosphere = true;
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container.JC_RC = refContainer.PK;

			messageBuilder = new MessageBuilder(new FreightDataLayer(container), Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			var actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			AssertMultilineASCIIMessagesEquals("MessageResult", PRAMessageMinimum, actualMessage);
		}

		public void TestFreightBOReeferContainer()
		{
			var container = GetBasicMinimumFreightContainer();
			var onForwardingLeg = container.Consol.Transports.AddNew();
			onForwardingLeg.JW_TransportType = Enterprise.Core.Constants.TransportPlanningType.OnForwarding;
			onForwardingLeg.JW_RL_NKDiscPort = "SGSIN";
			onForwardingLeg.JW_RL_NKLoadPort = "IDJKT";
			container.Consol.JK_RL_NKDischargePort = "SGSIN";

			AssertEquals("Container.Consol.Transports should contain Main and Onforwarding Legs", 2, container.Consol.Transports.Count);
			var commodityCode = CreateOrLoadCommodityCode("REEF", "");
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE");
			container.JC_RC = refContainer.PK;
			container.JC_RH_NKContainerCommodityCode = commodityCode.RH_Code;
			container.JC_IsControlledAtmosphere = true;
			container.JC_SetPointTemp = -18.0m;
			container.JC_SetPointTempUnit = "C";
			container.JC_HumidityPercent = 50;
			container.JC_AirVentFlow = 90m;
			container.JC_AirVentFlowRateUnit = AirFlowRateUnits.Codes.Percent;

			messageBuilder = new MessageBuilder(new FreightDataLayer(container), Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			var actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			AssertMultilineASCIIMessagesEquals("MessageResult", PRAMessageReefer, actualMessage);
		}

		public void TestFreightBOAttachedEquipment()
		{
			var container = GetNewContainer();
			var dataLayer = new FreightDataLayer(container);
			dataLayer.MessageReference = "001";
			dataLayer.DateTimeStringForMessage = "20040402092831";

			dataLayer.SenderID = "TSTSYD";
			dataLayer.ConsignorName = "CONSIGNOR";
			dataLayer.ShippingLineBookingReference = "BOOKING123";
			dataLayer.ECNorCRN = "CUSTOMS ECN";

			dataLayer.ShippingLine1StopCode = "ANL";
			dataLayer.VesselName = "MSC MARTINA";
			dataLayer.Voyage = "123N";
			dataLayer.LloydsNumber = "9060637";
			dataLayer.PortOfLoading = "AUSYD";
			dataLayer.LoadTerminal1StopCode = "ASLPB";
			dataLayer.PortOfDischarge = "IDJKT";
			dataLayer.PortOfFinalDischarge = "SGSIN";

			dataLayer.ContainerNumber = "TESU1234567";
			dataLayer.ISOContainerType = "22G0";
			dataLayer.Commodity1StopCode = "GENL";
			dataLayer.ContainerGrossWeight = 11600m;
			dataLayer.SealNumber = "289184";

			dataLayer.HasTynes = true;
			dataLayer.FlatRackID = "FR789";
			dataLayer.ReeferGeneratorID = "RG123";

			messageBuilder = new MessageBuilder(dataLayer, Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			var actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			AssertMultilineASCIIMessagesEquals("MessageResult", PRAMessageAttachedEqiupment, actualMessage);
		}

		public void TestFreightBOOverhangContainer()
		{
			var container = GetNewContainer();
			var dataLayer = new FreightDataLayer(container);
			dataLayer.MessageReference = "001";
			dataLayer.DateTimeStringForMessage = "20040402092831";

			dataLayer.SenderID = "TSTSYD";
			dataLayer.ConsignorName = "CONSIGNOR";
			dataLayer.ShippingLineBookingReference = "BOOKING123";
			dataLayer.ECNorCRN = "CUSTOMS ECN";

			dataLayer.ShippingLine1StopCode = "ANL";
			dataLayer.VesselName = "MSC MARTINA";
			dataLayer.Voyage = "123N";
			dataLayer.LloydsNumber = "9060637";
			dataLayer.PortOfLoading = "AUSYD";
			dataLayer.LoadTerminal1StopCode = "ASLPB";
			dataLayer.PortOfDischarge = "IDJKT";
			dataLayer.PortOfFinalDischarge = "SGSIN";

			dataLayer.ContainerNumber = "TESU1234567";
			dataLayer.ISOContainerType = "22G0";
			dataLayer.Commodity1StopCode = "GENL";
			dataLayer.ContainerGrossWeight = 11600m;
			dataLayer.SealNumber = "289184";

			dataLayer.OverhangFrontInCM = 300;
			dataLayer.OverhangBackInCM = 20;
			dataLayer.OverhangLeftInCM = 60;
			dataLayer.OverhangRightInCM = 30;
			dataLayer.OverhangHeightInCM = 10;

			messageBuilder = new MessageBuilder(dataLayer, Common.AU.PRAMessageTypeConstants.MessageType.Submit);
			var actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
			AssertMultilineASCIIMessagesEquals("MessageResult", PRAMessageOverhangContainer, actualMessage);
		}

		public void TestFreightVerifiedGrossWeight()
		{
			CreateNewCompanyBranch();

			AssertPRAMessage("Test: VGM Method 1, Container FULL", Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container, null, PRAMessageVGMMethod1);
			AssertPRAMessage("Test: VGM Method 2, Container FULL", Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages, null, PRAMessageVGMMethod2);
			AssertPRAMessage("Test: VGM Method 3, Container FULL", Core.Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal, null, PRAMessageVGMMethod3);

			var emptyCommodityCode = CreateOrLoadCommodityCode("MT", "");
			AssertPRAMessage("Test: VGM Method 2, Container Empty", Core.Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages, emptyCommodityCode, PRAMessageVGMMethod2EmptyContainer);
		}

		MessageBuilder messageBuilder;
		GlbBranch newBranch;
		GlbStaff newStaff;

		protected override void SetUp()
		{
			base.SetUp();
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "TST";
			registrationKey.ServerCodeForTest = "SYD";
			GlbStaff.CurrentUser.GS_FullName = "Developer";
		}

		void AssertPRAMessage(string message, string verificationGrossWeight, RefCommodityCode commodityCode, string expectedOutput)
		{
			using (Env.SetTemporaryUserContext(newStaff.GS_LoginName, newBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var container = GetBasicMinimumFreightContainer(verificationGrossWeight);

				if (commodityCode != null)
				{
					container.JC_RH_NKContainerCommodityCode = commodityCode.RH_Code;
				}

				messageBuilder = new MessageBuilder(new FreightDataLayer(container), Common.AU.PRAMessageTypeConstants.MessageType.Submit);
				var actualMessage = "\r\n" + messageBuilder.GetMessageText().Replace("'", "'\r\n");
				AssertMultilineASCIIMessagesEquals(message, expectedOutput, actualMessage);
			}
		}

		void CreateNewCompanyBranch()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "TTT";
			orgProxy.OH_FullName = "DEMO ORGANISATION";
			orgProxy.MainAddress.OA_Address1 = "111 Demo St";

			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = "XXY";
			newCompany.GC_Name = "XXY Company";
			newCompany.GC_RN_NKCountryCode = "AU";
			newCompany.GC_RX_NKLocalCurrency = "AUD";
			newCompany.GC_OH_OrgProxy = orgProxy.PK;

			newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "XYZ";
			newBranch.GB_BranchName = "XYZ Branch";
			newBranch.GB_RL_NKHomePort = "AUSYD";
			newBranch.GB_GC = newCompany.PK;

			newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_LoginName = "DEV";
			newStaff.GS_Code = "DEV";
			newStaff.GS_FullName = "Developer";
			newStaff.GS_EmailAddress = "Developer@edi.com.au";
			newStaff.GS_GB_HomeBranch = newBranch.PK;

			Factory.Save();
		}

		JobSailing CreateSailing(string vesselName, string voyageNumber, string lloydsNumber, string portOfLoading, string portOfDischarge)
		{
			var vessel = GetOrCreateVessel(vesselName, lloydsNumber);
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vesselName;
			voyage.JV_VoyageFlight = voyageNumber;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = portOfLoading;
			origin.JA_E_DEP = ZDateTime.Now;
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = portOfDischarge;
			destination.JB_E_ARV = ZDateTime.Now.AddDays(14);
			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		RefCommodityCode CreateOrLoadCommodityCode(ZString code, ZString description)
		{
			RefCommodityCode result;
			result = Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, code);
			if (result == null)
			{
				result = Factory.New<RefCommodityCode>();
				result.RH_Code = code;
			}
			result.RH_Description = description;
			//TODO: Refactor to use List.
			result.RH_IsForwarding = true;
			result.RH_IsShipping = true;
			result.RH_IsLandTransport = false;
			return result;
		}

		ForwardingContainer GetBasicMinimumFreightContainer(string verificationGrossWeight = "")
		{
			var consignor = CreateOrgForPRA("CONSIGNOR");

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "IDJKT";
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.SetDefaultSendingForwarderAddress(consignor);

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TESU1234567";
			consol.JK_BookingReference = "BOOKING123";

			var can = consol.CusEntryNums.AddNew();
			can.CE_EntryNum = "CUSTOMS ECN";
			can.CE_EntryType = CusEntryNumberTypes.Australia.CAN;

			var shippingLine = CreateOrgForPRA("ANL");
			consol.SetDefaultShippingLineAddress(shippingLine);

			var transport = consol.Transports[0];
			transport.JW_JX = CreateSailing("MSC MARTINA", "123N", "9060637", "AUSYD", "IDJKT").PK;

			var cto = CreateOrgForPRA("ASLPB");
			consol.JK_OA_DepartureCTOAddress = cto.MainAddress.PK;

			var commodityCode = CreateOrLoadCommodityCode("GENL", "");
			container.JC_RC = GetContainerType("22G0").PK;
			container.RefContainer.RC_TareWeight = 0m;
			container.JC_TareWeight = 0m;
			container.JC_RH_NKContainerCommodityCode = commodityCode.RH_Code;
			container.JC_GrossWeight = 11600m;
			container.JC_SealNum = "289184";

			if (verificationGrossWeight.IsNullOrEmpty())
			{
				container.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			}
			else
			{
				container.JC_GrossWeightVerificationType = verificationGrossWeight;
				container.JC_GrossWeightVerificationDateTime = new ZDateTime(2016, 4, 1, 13, 59, 0);
				container.GrossWeightVerifiedByAddress.E2_AddressOverride = true;
				container.GrossWeightVerifiedByAddress.E2_CompanyName = "CONTAINER WEIGHTING LTD";
				container.GrossWeightVerifiedByAddress.E2_Address1 = "Address 1";
				container.GrossWeightVerifiedByAddress.E2_Address2 = "Address 2";
				container.GrossWeightVerifiedByAddress.E2_City = "Mascot";
				container.GrossWeightVerifiedByAddress.E2_Postcode = "2025";
				container.GrossWeightVerifiedByAddress.E2_RN_NKCountryCode = "AU";
			}

			var shipment = consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 20;
			shipment.OuterPackLines[0].JL_ActualWeight = 11600m;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsignorDocumentaryAddress.ContactPK = consignor.Contacts[0].PK;

			return container;
		}

		RefVessel GetOrCreateVessel(string vesselName, string lloydsNumber)
		{
			var refVesselFilter = new ZQuery(RefVesselSchema.RV_Code, vesselName);
			var result = Factory.LoadTop1<RefVessel>(refVesselFilter);
			if (result == null)
			{
				result = Factory.New<RefVessel>();
				result.RV_Code = vesselName;
			}
			result.RV_LloydsNumber = lloydsNumber;
			return result;
		}

		RefContainer GetContainerType(string iSOContainerType)
		{
			var result = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_ISOType, iSOContainerType));
			return result;
		}

		void AssertMultilineASCIIMessagesEquals(string message, string expectedMessage, string actualMessage)
		{
			var noDateExpectedMessage = RemoveDate137AndRFFERN(expectedMessage);
			var noDateActualMessage = RemoveDate137AndRFFERN(actualMessage);
			AssertMultilineASCIIEquals(message, noDateExpectedMessage, noDateActualMessage);
		}

		string RemoveDate137AndRFFERN(string message)
		{
			const string EndIndexString = ":204'";
			var dTMStartIndex = message.IndexOf("DTM+137:");
			var dTMEndIndex = message.IndexOf(EndIndexString, dTMStartIndex);
			var result = message.Remove(dTMStartIndex, dTMEndIndex - dTMStartIndex + EndIndexString.Length);
			var rFFERNStartIndex = result.IndexOf("RFF+ERN");
			var rFFERNEndIndex = result.IndexOf("'", rFFERNStartIndex);
			var rFFLength = rFFERNEndIndex - rFFERNStartIndex;
			Assert("Missing References Number", rFFLength > 8);
			result = result.Remove(rFFERNStartIndex, rFFLength);
			return result;
		}

		OrgHeader CreateOrgForPRA(string orgCode)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = orgCode;
			org.MainAddress.OA_Address1 = "72 ORiordan St";
			org.MainAddress.OA_City = "Alexandria";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2015";
			org.OH_RL_NKClosestPort = "AUSYD";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = orgCode + "_Contact";
			contact.OC_Phone = "(613) 1111 2222";
			contact.OC_Email = "contact@edi.com.au";

			var customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_CustomsRegNo = orgCode;
			customsCode.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			customsCode.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Australia;

			return org;
		}

		ForwardingContainer GetNewContainer()
		{
			var container = Factory.New<ForwardingContainer>();
			container.JC_ContainerNum = "TESU1234567";
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_GrossWeightVerificationType = Core.Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;

			return container;
		}

		const string PRAMessageMinimum = @"
UNH+<<MSGNO PLACEHOLDER>>+IFTERA:D:98B:RT:ENET54'
BGM+ERA+TSTSYD+9+AQ'
DTM+137:20040402092831:204'
NAD+MR+ASLPB'
NAD+MS+TSTSYD'
CTA+IC+:DEVELOPER'
COM+1-STOPPRA@EDI.NET.AU:EM'
RFF+BN:BOOKING123'
RFF+ERN:001'
TDT+10++3'
TDT+20+123N+1++ANL+++9060637:::MSC MARTINA'
LOC+9+AUSYD+ASLPB'
LOC+11+IDJKT'
LOC+7+IDJKT'
EQD+CN+TESU1234567+22G0++2+5'
HAN+:::GENL'
NAD+CZ+CONSIGNOR'
MEA+AAE+G+KGM:11600'
SEL+289184+AB+1'
RFF+AAE:CUSTOMS ECN'
UNT+21+<<MSGNO PLACEHOLDER>>'";

		const string PRAMessageCartageABN = @"
UNH+<<MSGNO PLACEHOLDER>>+IFTERA:D:98B:RT:ENET54'
BGM+ERA+TSTSYD+9+AQ'
DTM+137:20040402092831:204'
NAD+MR+ASLPB'
NAD+MS+TSTSYD'
CTA+IC+:DEVELOPER'
COM+1-STOPPRA@EDI.NET.AU:EM'
RFF+BN:BOOKING123'
RFF+ERN:001'
TDT+10++3++123987432'
TDT+20+123N+1++ANL+++9060637:::MSC MARTINA'
LOC+9+AUSYD+ASLPB'
LOC+11+IDJKT'
LOC+7+IDJKT'
EQD+CN+TESU1234567+22G0++2+5'
HAN+:::GENL'
NAD+CZ+CONSIGNOR'
MEA+AAE+G+KGM:11600'
SEL+289184+AB+1'
RFF+AAE:CUSTOMS ECN'
UNT+21+<<MSGNO PLACEHOLDER>>'";

		const string PRAMessageReefer = @"
UNH+<<MSGNO PLACEHOLDER>>+IFTERA:D:98B:RT:ENET54'
BGM+ERA+TSTSYD+9+AQ'
DTM+137:20040402092831:204'
NAD+MR+ASLPB'
NAD+MS+TSTSYD'
CTA+IC+:DEVELOPER'
COM+1-STOPPRA@EDI.NET.AU:EM'
RFF+BN:BOOKING123'
RFF+ERN:001'
TDT+10++3'
TDT+20+123N+1++ANL+++9060637:::MSC MARTINA'
LOC+9+AUSYD+ASLPB'
LOC+11+IDJKT'
LOC+7+SGSIN'
EQD+CN+TESU1234567+22R0++2+5'
HAN+:::REEF'
NAD+CZ+CONSIGNOR'
MEA+AAE+G+KGM:14550'
MEA+AAE+AAO+P1:50'
MEA+AAE+AAS+P1:90'
SEL+289184+AB+1'
RFF+AAE:CUSTOMS ECN'
TMP+5+-18.0:CEL'
UNT+24+<<MSGNO PLACEHOLDER>>'";

		const string PRAMessageAttachedEqiupment = @"
UNH+<<MSGNO PLACEHOLDER>>+IFTERA:D:98B:RT:ENET54'
BGM+ERA+TSTSYD+9+AQ'
DTM+137:20040402092831:204'
NAD+MR+ASLPB'
NAD+MS+TSTSYD'
CTA+IC+:DEVELOPER'
COM+1-STOPPRA@EDI.NET.AU:EM'
RFF+BN:BOOKING123'
RFF+ERN:001'
TDT+10++3'
TDT+20+123N+1++ANL+++9060637:::MSC MARTINA'
LOC+9+AUSYD+ASLPB'
LOC+11+IDJKT'
LOC+7+SGSIN'
EQD+CN+TESU1234567+22G0++2+5'
HAN+:::GENL'
NAD+CZ+CONSIGNOR'
MEA+AAE+G+KGM:11600'
SEL+289184+AB+1'
RFF+AAE:CUSTOMS ECN'
EQA+FSU'
EQA+AJ+FR789'
EQA+RG+RG123'
UNT+24+<<MSGNO PLACEHOLDER>>'";

		const string PRAMessageEmptyHazContainer = @"
UNH+<<MSGNO PLACEHOLDER>>+IFTERA:D:98B:RT:ENET54'
BGM+ERA+TSTSYD+9+AQ'
DTM+137:20040402092831:204'
NAD+MS+TSTSYD'
CTA+IC+:DEVELOPER'
COM+1-STOPPRA@EDI.NET.AU:EM'
RFF+ERN:CON--MHAZ9999995'
TDT+10++3'
TDT+20++1'
LOC+9'
LOC+11'
LOC+7'
EQD+CN+MHAZ9999995+++2+4'
HAN+:::MTHZ'
NAD+CZ+CONSIGNOR'
MEA+AAE+G+KGM:6'
SEL++AB+1'
FTX+ZO1+++EMPTY - HAZARDOUS'
RFF+CN'
DGS+IMD+2.1+1021+00.0:CEL'
FTX+AAD'
CTA+HG+:JOANNA HILL'
COM+JOHILL@INTERCHEM.COM.AU:EM'
COM+61 (3) 9270-9607:TE'
MEA+AAE+AAL+KGM:0'
UNT+26+<<MSGNO PLACEHOLDER>>'";

		const string PRAMessageEmptyHazContainerWithFlashPoint = @"
UNH+<<MSGNO PLACEHOLDER>>+IFTERA:D:98B:RT:ENET54'
BGM+ERA+TSTSYD+9+AQ'
DTM+137:20040402092831:204'
NAD+MS+TSTSYD'
CTA+IC+:DEVELOPER'
COM+1-STOPPRA@EDI.NET.AU:EM'
RFF+ERN:CON--MHAZ9999995'
TDT+10++3'
TDT+20++1'
LOC+9'
LOC+11'
LOC+7'
EQD+CN+MHAZ9999995+++2+4'
HAN+:::MTHZ'
NAD+CZ+CONSIGNOR'
MEA+AAE+G+KGM:6'
SEL++AB+1'
FTX+ZO1+++EMPTY - HAZARDOUS'
RFF+CN'
DGS+IMD+2.1+1021+33.3:CEL'
FTX+AAD'
CTA+HG+:JOANNA HILL'
COM+JOHILL@INTERCHEM.COM.AU:EM'
COM+61 (3) 9270-9607:TE'
MEA+AAE+AAL+KGM:0'
UNT+26+<<MSGNO PLACEHOLDER>>'";

		string GetPRAMessageWeightRoundUp(string weight) => $@"
UNH+<<MSGNO PLACEHOLDER>>+IFTERA:D:98B:RT:ENET54'
BGM+ERA+TSTSYD+9+AQ'
DTM+137:20040402092831:204'
NAD+MS+TSTSYD'
CTA+IC+:DEVELOPER'
COM+1-STOPPRA@EDI.NET.AU:EM'
RFF+ERN:CON--MHAZ9999995'
TDT+10++3'
TDT+20++1'
LOC+9'
LOC+11'
LOC+7'
EQD+CN+MHAZ9999995+++2+4'
HAN+:::MTHZ'
NAD+CZ+CONSIGNOR'
MEA+AAE+G+KGM:6'
SEL++AB+1'
FTX+ZO1+++EMPTY - HAZARDOUS'
RFF+CN'
DGS+IMD+2.1+1021+33.3:CEL'
FTX+AAD'
CTA+HG+:JOANNA HILL'
COM+JOHILL@INTERCHEM.COM.AU:EM'
COM+61 (3) 9270-9607:TE'
MEA+AAE+AAL+KGM:{weight}'
UNT+26+<<MSGNO PLACEHOLDER>>'";

		const string PRAMessageEmptyContainer = @"
UNH+<<MSGNO PLACEHOLDER>>+IFTERA:D:98B:RT:ENET54'
BGM+ERA+TSTSYD+9+AQ'
DTM+137:20040402092831:204'
NAD+MS+TSTSYD'
CTA+IC+:DEVELOPER'
COM+1-STOPPRA@EDI.NET.AU:EM'
RFF+ERN:CON--TESU1234567'
TDT+10++3'
TDT+20++1'
LOC+9'
LOC+11'
LOC+7'
EQD+CN+TESU1234567+++2+4'
HAN'
NAD+CZ+CONSIGNOR'
MEA+AAE+G+KGM:0'
SEL++AB+1'
UNT+18+<<MSGNO PLACEHOLDER>>'";

		const string PRAMessageOverhangContainer = @"
UNH+<<MSGNO PLACEHOLDER>>+IFTERA:D:98B:RT:ENET54'
BGM+ERA+TSTSYD+9+AQ'
DTM+137:20040402092831:204'
NAD+MR+ASLPB'
NAD+MS+TSTSYD'
CTA+IC+:DEVELOPER'
COM+1-STOPPRA@EDI.NET.AU:EM'
RFF+BN:BOOKING123'
RFF+ERN:001'
TDT+10++3'
TDT+20+123N+1++ANL+++9060637:::MSC MARTINA'
LOC+9+AUSYD+ASLPB'
LOC+11+IDJKT'
LOC+7+SGSIN'
EQD+CN+TESU1234567+22G0++2+5'
HAN+:::GENL'
NAD+CZ+CONSIGNOR'
MEA+AAE+G+KGM:11600'
DIM+9+CMT:300:60:10:20:30'
SEL+289184+AB+1'
RFF+AAE:CUSTOMS ECN'
UNT+22+<<MSGNO PLACEHOLDER>>'";

		const string PRAMessageVGMMethod1 = @"
UNH+<<MSGNO PLACEHOLDER>>+IFTERA:D:98B:RT:ENET54'
BGM+ERA+TSTSYD+9+AQ'
DTM+137:20040402092831:204'
NAD+MR+ASLPB'
NAD+MS+TSTSYD'
CTA+IC+:DEVELOPER'
COM+1-STOPPRA@EDI.NET.AU:EM'
RFF+BN:BOOKING123'
RFF+ERN:001'
TDT+10++3'
TDT+20+123N+1++ANL+++9060637:::MSC MARTINA'
LOC+9+AUSYD+ASLPB'
LOC+11+IDJKT'
LOC+7+IDJKT'
EQD+CN+TESU1234567+22G0++2+5'
HAN+:::GENL'
NAD+CZ+CONSIGNOR'
MEA+AAE+G+KGM:11600'
MEA+AAE+VGM+KGM:11600'
SEL+289184+AB+1'
FTX+AAY++SM1:VGM+201604010259UTC:CONTAINER WEIGHTING LTD;ADDRESS 1 ADDRESS 2;MASCOT;AU:CONSIGNOR_CONTACT;CONSIGNOR;(613) 1111 2222;CONTACT@EDI.COM.AU:CONSIGNOR_CONTACT'
RFF+AAE:CUSTOMS ECN'
UNT+23+<<MSGNO PLACEHOLDER>>'";

		const string PRAMessageVGMMethod2 = @"
UNH+<<MSGNO PLACEHOLDER>>+IFTERA:D:98B:RT:ENET54'
BGM+ERA+TSTSYD+9+AQ'
DTM+137:20040402092831:204'
NAD+MR+ASLPB'
NAD+MS+TSTSYD'
CTA+IC+:DEVELOPER'
COM+1-STOPPRA@EDI.NET.AU:EM'
RFF+BN:BOOKING123'
RFF+ERN:001'
TDT+10++3'
TDT+20+123N+1++ANL+++9060637:::MSC MARTINA'
LOC+9+AUSYD+ASLPB'
LOC+11+IDJKT'
LOC+7+IDJKT'
EQD+CN+TESU1234567+22G0++2+5'
HAN+:::GENL'
NAD+CZ+CONSIGNOR'
MEA+AAE+G+KGM:11600'
MEA+AAE+VGM+KGM:11600'
SEL+289184+AB+1'
FTX+AAY++SM2:VGM+201604010259UTC:CONTAINER WEIGHTING LTD;ADDRESS 1 ADDRESS 2;MASCOT;AU:CONSIGNOR_CONTACT;CONSIGNOR;(613) 1111 2222;CONTACT@EDI.COM.AU:CONSIGNOR_CONTACT'
RFF+AAE:CUSTOMS ECN'
UNT+23+<<MSGNO PLACEHOLDER>>'";

		const string PRAMessageVGMMethod3 = @"
UNH+<<MSGNO PLACEHOLDER>>+IFTERA:D:98B:RT:ENET54'
BGM+ERA+TSTSYD+9+AQ'
DTM+137:20040402092831:204'
NAD+MR+ASLPB'
NAD+MS+TSTSYD'
CTA+IC+:DEVELOPER'
COM+1-STOPPRA@EDI.NET.AU:EM'
RFF+BN:BOOKING123'
RFF+ERN:001'
TDT+10++3'
TDT+20+123N+1++ANL+++9060637:::MSC MARTINA'
LOC+9+AUSYD+ASLPB'
LOC+11+IDJKT'
LOC+7+IDJKT'
EQD+CN+TESU1234567+22G0++2+5'
HAN+:::GENL'
NAD+CZ+CONSIGNOR'
MEA+AAE+G+KGM:11600'
SEL+289184+AB+1'
FTX+AAY++WAT:VGM+:CONTAINER WEIGHTING LTD;ADDRESS 1 ADDRESS 2;MASCOT;AU:CONSIGNOR_CONTACT;CONSIGNOR;(613) 1111 2222;CONTACT@EDI.COM.AU:CONSIGNOR_CONTACT'
RFF+AAE:CUSTOMS ECN'
UNT+22+<<MSGNO PLACEHOLDER>>'";

		const string PRAMessageVGMMethod2EmptyContainer = @"
UNH+<<MSGNO PLACEHOLDER>>+IFTERA:D:98B:RT:ENET54'
BGM+ERA+TSTSYD+9+AQ'
DTM+137:20040402092831:204'
NAD+MR+ASLPB'
NAD+MS+TSTSYD'
CTA+IC+:DEVELOPER'
COM+1-STOPPRA@EDI.NET.AU:EM'
RFF+BN:BOOKING123'
RFF+ERN:001'
TDT+10++3'
TDT+20+123N+1++ANL+++9060637:::MSC MARTINA'
LOC+9+AUSYD+ASLPB'
LOC+11+IDJKT'
LOC+7+IDJKT'
EQD+CN+TESU1234567+22G0++2+4'
HAN+:::MT'
NAD+CZ+CONSIGNOR'
MEA+AAE+G+KGM:11600'
SEL+289184+AB+1'
RFF+AAE:CUSTOMS ECN'
UNT+21+<<MSGNO PLACEHOLDER>>'";
	}
}
