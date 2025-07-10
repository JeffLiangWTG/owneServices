using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class FreightConsolWrapperTest : Customs.Business.Testing.FreightConsolWrapperTest
	{
		public void TestAreAnyDeclarationsWaitingForAResponse()
		{
			Assert("AreAnyDeclarationsWaitingForAResponse", !wrapper.AreAnyDeclarationsWaitingForAResponse);
			declaration1.JE_EntryStatus = CustomsEntryStatus.AwaitingOriginal.Code;
			Assert("AreAnyDeclarationsWaitingForAResponse", wrapper.AreAnyDeclarationsWaitingForAResponse);
			declaration1.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			Assert("AreAnyDeclarationsWaitingForAResponse", !wrapper.AreAnyDeclarationsWaitingForAResponse);
			declaration3.JE_EntryStatus = CustomsEntryStatus.AwaitingReplacement.Code;
			Assert("AreAnyDeclarationsWaitingForAResponse", wrapper.AreAnyDeclarationsWaitingForAResponse);
			declaration3.JE_EntryStatus = CustomsEntryStatus.ClearOriginal.Code;
			Assert("AreAnyDeclarationsWaitingForAResponse", !wrapper.AreAnyDeclarationsWaitingForAResponse);
			declaration1.JE_MessageStatus = CustomsEntryStatus.AwaitingPreLodge.Code;
			Assert("AreAnyDeclarationsWaitingForAResponse", wrapper.AreAnyDeclarationsWaitingForAResponse);
			declaration1.JE_MessageStatus = CustomsEntryStatus.ClearPreLodge.Code;
			Assert("AreAnyDeclarationsWaitingForAResponse", !wrapper.AreAnyDeclarationsWaitingForAResponse);
			declaration1.JE_MessageStatus = CustomsEntryStatus.AwaitingWARRELOriginal.Code;
			Assert("AreAnyDeclarationsWaitingForAResponse", wrapper.AreAnyDeclarationsWaitingForAResponse);
			declaration1.JE_MessageStatus = CustomsEntryStatus.ClearWARRELOriginal.Code;
			Assert("AreAnyDeclarationsWaitingForAResponse", !wrapper.AreAnyDeclarationsWaitingForAResponse);
		}

		public void TestLoadAJobDeclaration()
		{
			JobDeclaration loadedDec = wrapper.LoadArbitraryJobDeclaration();
			Assert("DecLoaded", loadedDec == declaration1 || loadedDec == declaration2 || loadedDec == declaration3);
		}

		public void TestShortDescription()
		{
			consol.JK_UniqueConsignRef = "C007438237";
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "SGSIN";
			AssertEquals("ShortDescription", "Consol #: C007438237", wrapper.ShortDescription);
		}

		public void TestConsolDetails()
		{
			consol.JK_UniqueConsignRef = "NUMBER";
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "SGSIN";

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_VoyageFlight = "QF123";
			transport.JW_ATD = new ZDateTime(2004, 4, 23);
			transport.JW_ETD = new ZDateTime(2004, 4, 23);
			string expectedDateString = new ZDateTime(2004, 4, 23).ToDateTime().ToLongDateString();

			AssertEquals("ConsolDetails", @"Consol #: NUMBER
Mode: AIR
Port of 1st Load: AUBNE
Port of Loading: AUSYD
Port of Discharge: NZAKL
Port of Final Discharge: SGSIN
Flight #: QF123
Estimated Departure Date: " + expectedDateString + @"
Departure Date: " + expectedDateString + @"
Masterbill #: " + consol.JK_MasterBillNum + "\r\n", wrapper.Details);
		}

		public void TestGetCMRStatus()
		{
			ZString messageType = CMRMessage.CMRMessageTypes.ESM;
			EDIMessage message = consol.Messages.AddNew();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Clear;
			AssertEquals("Status", Constants.CMRConsolStatus.Clear, wrapper.GetCMRStatus(messageType));
			message.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Error;
			AssertEquals("Status", Constants.CMRConsolStatus.Errors, wrapper.GetCMRStatus(messageType));
			message.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Rejected;
			AssertEquals("Status", Constants.CMRConsolStatus.Rejected, wrapper.GetCMRStatus(messageType));
			message.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Revoked;
			AssertEquals("Status", Constants.CMRConsolStatus.Revoked, wrapper.GetCMRStatus(messageType));
			message.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Withdrawn;
			AssertEquals("Status", Constants.CMRConsolStatus.Withdrawn, wrapper.GetCMRStatus(messageType));
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertEquals("Status", Constants.CMRConsolStatus.WaitingForResponse, wrapper.GetCMRStatus(messageType));
		}

		public void TestGetESMStatus()
		{
			AssertNotNull(wrapper.ESMStatus);
		}

		public void TestGetEMMStatus()
		{
			AssertNotNull(wrapper.EMMStatus);
		}

		public void TestContingencyCAN()
		{
			AssertEquals(ZString.Empty, wrapper.ContingencyCAN);
			wrapper.ContingencyCAN = "123";
			AssertEquals("123", wrapper.ContingencyCAN);
			wrapper.ContingencyCAN = "456";
			AssertEquals("456", wrapper.ContingencyCAN);
			wrapper.ContingencyCAN = ZString.Empty;
			AssertEquals(ZString.Empty, wrapper.ContingencyCAN);
		}

		public void TestContingencyCANWillNotGetCUSEntryNumber()
		{
			consol.CusEntryNums.RemoveAndDeleteAll();

			var entry = Factory.New<AUCusEntryNumber>();
			entry.CE_ParentTable = consol.TableName;
			entry.CE_EntryNum = "123";
			entry.CE_ParentID = consol.PK;
			entry.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entry.CE_EntryType = CANType.ContingencyCustomsAuthorityNumber.Code;
			entry.CE_Category = "OTH";
			Factory.Save();

			AssertEquals(ZString.Empty, wrapper.ContingencyCAN);
			entry.CE_Category = "CUS";
			Factory.Save();
			AssertEquals("123", wrapper.ContingencyCAN);
		}

		public void TestChangingTheContingencyCANSetsHasChanges()
		{
			Factory.Save();
			Assert("ConsolDoesntHaveChanges", !consol.HasChanges);
			wrapper.ContingencyCAN = "123";
			Assert("ConsolHasChanges", consol.HasChanges);
		}

		public void TestHasExitEntryNumber()
		{
			AUCusEntryNumber entryNum = wrapper.CreateCusEntryNumber();
			entryNum.CE_EntryNum = "12345678901234";
			Assert(wrapper.HasExitEntryNumber);
			entryNum.CE_EntryNum = "123456789";
			Assert(!wrapper.HasExitEntryNumber);
		}

		public void TestAreAnyCusUnderbondsWaitingForAResponse()
		{
			HAWB.CS_JS = shipment1.PK;
			ICusUnderbondDependentCollectionParent hAWBUnder = HAWB;
			AssertEquals("AreAnyCusUnderbondsWaitingForAResponse", false, wrapper.AreAnyCusUnderbondsWaitingForAResponse);
			hAWBUnder.Underbonds.AddNew();
			AssertEquals("AreAnyCusUnderbondsWaitingForAResponse", false, wrapper.AreAnyCusUnderbondsWaitingForAResponse);
			hAWBUnder.Underbonds[0].Messages.AddNew();
			hAWBUnder.Underbonds[0].Messages[0].EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			AssertEquals("AreAnyCusUnderbondsWaitingForAResponse", true, wrapper.AreAnyCusUnderbondsWaitingForAResponse);
			hAWBUnder.Underbonds[0].Messages[0].EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertEquals("AreAnyCusUnderbondsWaitingForAResponse", false, wrapper.AreAnyCusUnderbondsWaitingForAResponse);
		}

		public void TestTypeOfMessageResponse()
		{
			var originalMessage = Factory.New<CMRMessage>();
			originalMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
			originalMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			originalMessage.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			originalMessage.EM_Status = "SNT";
			originalMessage.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:1+9'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+6+1'";
			consol.Messages.Add(originalMessage);

			var responseMessage1 = Factory.New<CMRMessage>();
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2).AddMinutes(20);
			responseMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageSubType = "CLR";
			responseMessage1.EM_Status = "RCV";
			consol.Messages.Add(responseMessage1);

			var replacementMessage = Factory.New<CMRMessage>();
			replacementMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-50);
			replacementMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			replacementMessage.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			replacementMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			replacementMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
			replacementMessage.EM_Status = "SNT";
			replacementMessage.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:2+5'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+26+1'";
			consol.Messages.Add(replacementMessage);

			var responseMessage2 = Factory.New<CMRMessage>();
			responseMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-30);
			responseMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage2.EM_MessageSubType = "CLR";
			responseMessage2.EM_Status = "RCV";
			consol.Messages.Add(responseMessage2);

			var changeMessage = Factory.New<CMRMessage>();
			changeMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-20);
			changeMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			changeMessage.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			changeMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			changeMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;
			changeMessage.EM_Status = "SNT";
			changeMessage.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+263:::AIROUT+U00003312/CMT3:6+4'DTM+570:20150630:102'DTM+570:0144:401'NAD+VW+41065894724::95'TDT+20+318++6+QF::3'LOC+59+9914N::95'DTM+132:20150630:102'CNI++:::A'RFF+ACU:NIL'GID+1'RFF+MWB:08142348235'GID+1'RFF+HWB:1785D'GID+1'PAC+10'UNT+17+1'";
			consol.Messages.Add(changeMessage);

			var responseMessage3 = Factory.New<CMRMessage>();
			responseMessage3.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-15);
			responseMessage3.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage3.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage3.EM_MessageSubType = "CLR";
			responseMessage3.EM_Status = "RCV";
			consol.Messages.Add(responseMessage3);

			var withdrawalMessage = Factory.New<CMRMessage>();
			withdrawalMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-10);
			withdrawalMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			withdrawalMessage.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			withdrawalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			withdrawalMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;
			withdrawalMessage.EM_Status = "SNT";
			withdrawalMessage.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+263:::AIROUT+U00003313/CMT1:3+50'DTM+570:20150630:102'DTM+570:0408:401'NAD+VW+41065894724::95'TDT+20+005++6+QF::3'LOC+59+9914N::95'DTM+132:20150630:102'UNT+9+1'";
			consol.Messages.Add(withdrawalMessage);

			var responseMessage4 = Factory.New<CMRMessage>();
			responseMessage4.EM_SystemCreateTimeUtc = ZDateTime.Now;
			responseMessage4.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage4.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage4.EM_MessageSubType = "CLR";
			responseMessage4.EM_Status = "RCV";
			consol.Messages.Add(responseMessage4);

			var consolWrapper = new FreightConsolWrapper(consol, responseMessage1);
			AssertEquals("IsOriginalResponse", true, consolWrapper.IsOriginalResponse);
			AssertEquals("IsReplacementResponse", false, consolWrapper.IsReplacementResponse);
			AssertEquals("IsChangeResponse", false, consolWrapper.IsChangeResponse);
			AssertEquals("IsWithdrawalResponse", false, consolWrapper.IsWithdrawalResponse);

			consolWrapper = new FreightConsolWrapper(consol, responseMessage2);
			AssertEquals("IsOriginalResponse", false, consolWrapper.IsOriginalResponse);
			AssertEquals("IsReplacementResponse", true, consolWrapper.IsReplacementResponse);
			AssertEquals("IsChangeResponse", false, consolWrapper.IsChangeResponse);
			AssertEquals("IsWithdrawalResponse", false, consolWrapper.IsWithdrawalResponse);

			consolWrapper = new FreightConsolWrapper(consol, responseMessage3);
			AssertEquals("IsOriginalResponse", false, consolWrapper.IsOriginalResponse);
			AssertEquals("IsReplacementResponse", false, consolWrapper.IsReplacementResponse);
			AssertEquals("IsChangeResponse", true, consolWrapper.IsChangeResponse);
			AssertEquals("IsWithdrawalResponse", false, consolWrapper.IsWithdrawalResponse);

			consolWrapper = new FreightConsolWrapper(consol, responseMessage4);
			AssertEquals("IsOriginalResponse", false, consolWrapper.IsOriginalResponse);
			AssertEquals("IsReplacementResponse", false, consolWrapper.IsReplacementResponse);
			AssertEquals("IsChangeResponse", false, consolWrapper.IsChangeResponse);
			AssertEquals("IsWithdrawalResponse", true, consolWrapper.IsWithdrawalResponse);
		}

		public void TestUpdatePreliminaryLinesToManifestedLines()
		{
			shipment1.JS_UniqueConsignRef = shipment1.JS_HouseBill = "S00047238";
			shipment2.JS_UniqueConsignRef = shipment2.JS_HouseBill = "S00047239";
			shipment3.JS_UniqueConsignRef = shipment3.JS_HouseBill = "S00047240";
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
			}

			wrapper = new FreightConsolWrapper(consol);
			wrapper.UpdatePreliminaryLinesToManifestedLines();
			var wrappedShipment = new FreightShipmentWrapper(consol.Shipments[0], consol);
			AssertEquals("Shipment 1 - HasSubManifestLineNumber", true, wrappedShipment.HasSubManifestLineNumber);
			wrappedShipment = new FreightShipmentWrapper(consol.Shipments[1], consol);
			AssertEquals("Shipment 2 - HasSubManifestLineNumber", true, wrappedShipment.HasSubManifestLineNumber);
			wrappedShipment = new FreightShipmentWrapper(consol.Shipments[2], consol);
			AssertEquals("Shipment 3 - HasSubManifestLineNumber", true, wrappedShipment.HasSubManifestLineNumber);
		}

		public void TestRemovePreliminaryLineNumbers()
		{
			shipment1.JS_UniqueConsignRef = shipment1.JS_HouseBill = "S00047238";
			shipment2.JS_UniqueConsignRef = shipment2.JS_HouseBill = "S00047239";
			shipment3.JS_UniqueConsignRef = shipment3.JS_HouseBill = "S00047240";
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
			}

			wrapper = new FreightConsolWrapper(consol);
			var wrappedShipment = new FreightShipmentWrapper(consol.Shipments[0], consol);
			AssertEquals("Shipment 1", true, wrappedShipment.IsPreliminaryNumberType);
			wrappedShipment = new FreightShipmentWrapper(consol.Shipments[1], consol);
			AssertEquals("Shipment 2", true, wrappedShipment.IsPreliminaryNumberType);
			wrappedShipment = new FreightShipmentWrapper(consol.Shipments[2], consol);
			AssertEquals("Shipment 3", true, wrappedShipment.IsPreliminaryNumberType);

			wrapper.RemovePreliminaryLineNumbers();
			wrappedShipment = new FreightShipmentWrapper(consol.Shipments[0], consol);
			AssertEquals("Shipment 1 - Should not have ESMPreliminaryLineSequence data", null, wrappedShipment.ESMPreliminaryLineSequence);
			wrappedShipment = new FreightShipmentWrapper(consol.Shipments[1], consol);
			AssertEquals("Shipment 2 - Should not have ESMPreliminaryLineSequence data", null, wrappedShipment.ESMPreliminaryLineSequence);
			wrappedShipment = new FreightShipmentWrapper(consol.Shipments[2], consol);
			AssertEquals("Shipment 3 - Should not have ESMPreliminaryLineSequence data", null, wrappedShipment.ESMPreliminaryLineSequence);
		}

		public void TestRemoveAllESMLineNumbers()
		{
			shipment1.JS_UniqueConsignRef = shipment1.JS_HouseBill = "S00047238";
			shipment2.JS_UniqueConsignRef = shipment2.JS_HouseBill = "S00047239";
			shipment3.JS_UniqueConsignRef = shipment3.JS_HouseBill = "S00047240";
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
			}

			lineCount++;
			var shipment4 = consol.Shipments.AddNew();
			var declaration4 = CreateExportDeclarationForShipment(shipment4);
			shipment4.JS_UniqueConsignRef = "S00047241";
			shipment4.JS_HouseBill = "S00047241";
			var shipment4Wrapper = new FreightShipmentWrapper(shipment4, consol);
			shipment4Wrapper.CreatePreliminaryManifestLineNumber(lineCount);

			wrapper = new FreightConsolWrapper(consol);
			var wrappedShipment = new FreightShipmentWrapper(consol.Shipments[0], consol);
			AssertEquals("Shipment 1", true, wrappedShipment.IsPreliminaryNumberType);
			wrappedShipment = new FreightShipmentWrapper(consol.Shipments[1], consol);
			AssertEquals("Shipment 2", true, wrappedShipment.IsPreliminaryNumberType);
			wrappedShipment = new FreightShipmentWrapper(consol.Shipments[2], consol);
			AssertEquals("Shipment 3", true, wrappedShipment.IsPreliminaryNumberType);
			wrappedShipment = new FreightShipmentWrapper(consol.Shipments[3], consol);
			AssertEquals("Shipment 4", true, wrappedShipment.IsPreliminaryNumberType);

			wrapper.RemoveAllESMLineNumbers();
			wrappedShipment = new FreightShipmentWrapper(consol.Shipments[0], consol);
			AssertEquals("Shipment 1 - Should not have SubManifestLineNumber data", 0, wrappedShipment.LineSequenceCollection.Count);
			wrappedShipment = new FreightShipmentWrapper(consol.Shipments[1], consol);
			AssertEquals("Shipment 2 - Should not have SubManifestLineNumber data", 0, wrappedShipment.LineSequenceCollection.Count);
			wrappedShipment = new FreightShipmentWrapper(consol.Shipments[2], consol);
			AssertEquals("Shipment 3 - Should not have SubManifestLineNumber data", 0, wrappedShipment.LineSequenceCollection.Count);
			wrappedShipment = new FreightShipmentWrapper(consol.Shipments[3], consol);
			AssertEquals("Shipment 4 - Should not have SubManifestLineNumber data", 0, wrappedShipment.LineSequenceCollection.Count);
		}

		public void TestHasSameShipments()
		{
			shipment1.JS_UniqueConsignRef = shipment1.JS_HouseBill = "S00047238";
			shipment2.JS_UniqueConsignRef = shipment2.JS_HouseBill = "S00047239";
			shipment3.Delete();
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			var outgoingMessage1 = Factory.New<CMRMessage>();
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
			outgoingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageSubType = "ORG";
			outgoingMessage1.EM_Status = "SNT";
			outgoingMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:1+9'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+6+1'";
			consol.Messages.Add(outgoingMessage1);

			var outgoingMessage2 = Factory.New<CMRMessage>();
			outgoingMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-20);
			outgoingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_MessageSubType = "AMD";
			outgoingMessage2.EM_Status = "SNT";
			outgoingMessage2.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:2+5'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+26+1'";
			consol.Messages.Add(outgoingMessage2);

			var responseMessage1 = Factory.New<CMRMessage>();
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now;
			responseMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageSubType = "CLR";
			responseMessage1.EM_Status = "RCV";
			consol.Messages.Add(responseMessage1);
			Factory.Save();
			AssertEquals("HasSameShipments", true, wrapper.HasSameShipments);

			shipment3 = consol.Shipments.AddNew();
			declaration3 = CreateExportDeclarationForShipment(shipment3);
			shipment3.JS_UniqueConsignRef = shipment3.JS_HouseBill = "S00047240";
			Factory.Save();
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("HasSameShipments? - No, an additional shipment has just been added", false, wrapper.HasSameShipments);
		}

		public void TestHasSameShipmentsWithMultipleHVLShipments()
		{
			shipment1.JS_UniqueConsignRef = shipment1.JS_HouseBill = "S00047238";
			shipment2.JS_UniqueConsignRef = shipment2.JS_HouseBill = "S00047239";
			shipment3.Delete();
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			var outgoingMessage1 = Factory.New<CMRMessage>();
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
			outgoingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageSubType = "ORG";
			outgoingMessage1.EM_Status = "SNT";
			outgoingMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:1+9'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+6+1'";
			consol.Messages.Add(outgoingMessage1);

			var outgoingMessage2 = Factory.New<CMRMessage>();
			outgoingMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-20);
			outgoingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_MessageSubType = "AMD";
			outgoingMessage2.EM_Status = "SNT";
			outgoingMessage2.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:2+5'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+26+1'";
			consol.Messages.Add(outgoingMessage2);

			var responseMessage1 = Factory.New<CMRMessage>();
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now;
			responseMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageSubType = "CLR";
			responseMessage1.EM_Status = "RCV";
			consol.Messages.Add(responseMessage1);
			Factory.Save();
			AssertEquals("HasSameShipments", true, wrapper.HasSameShipments);

			int consignmentCount = consol.ShipmentCount;
			var shipment4 = AddHVLShipmentToConsol(consol, "HVL-001", consignmentCount);
			var shipment5 = AddHVLShipmentToConsol(consol, "HVL-002", consignmentCount + 4);
			Factory.Save();
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("HasSameShipments? - No, two HVL shipments have just been added", false, wrapper.HasSameShipments);

			UpdateLinesToManifested(consol);
			Factory.Save();
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("HasSameShipments? - Yes, HVL shipment consignments & Std Shipments have not changed", true, wrapper.HasSameShipments);

			ForwardingShipment hvlvShipment = (ForwardingShipment)shipment4;
			foreach (IHVLVConsignment consignmentLine in hvlvShipment.HVLVConsignments)
			{
				if (consignmentLine.HVC_WaybillNumber == "HVLV2")
				{
					consignmentLine.Delete();
				}
			}

			Factory.Save();
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("HasSameShipments? - No, HVL shipment 1 consignments have changed", false, wrapper.HasSameShipments);
		}

		[ExpectNoExceptions]
		public void TestHandlesZeroLineSequenceRecordsFromCoLoadShipment()
		{
			shipment1.JS_UniqueConsignRef = shipment1.JS_HouseBill = "S00047238";
			shipment2.JS_UniqueConsignRef = shipment2.JS_HouseBill = "S00047239";
			shipment3.Delete();

			var outgoingMessage1 = Factory.New<CMRMessage>();
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
			outgoingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageSubType = "ORG";
			outgoingMessage1.EM_Status = "SNT";
			outgoingMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:1+9'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+6+1'";
			consol.Messages.Add(outgoingMessage1);

			var outgoingMessage2 = Factory.New<CMRMessage>();
			outgoingMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-20);
			outgoingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_MessageSubType = "AMD";
			outgoingMessage2.EM_Status = "SNT";
			outgoingMessage2.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:2+5'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+26+1'";
			consol.Messages.Add(outgoingMessage2);

			var responseMessage1 = Factory.New<CMRMessage>();
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now;
			responseMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageSubType = "CLR";
			responseMessage1.EM_Status = "RCV";
			consol.Messages.Add(responseMessage1);
			Factory.Save();
			AssertEquals("HasSameShipments", true, wrapper.HasSameShipments);

			shipment3 = consol.Shipments.AddNew();
			declaration3 = CreateExportDeclarationForShipment(shipment3);
			shipment3.JS_UniqueConsignRef = shipment3.JS_HouseBill = "S00047240";
			UpdateManifestData(shipment1, 1);
			UpdateManifestData(shipment2, 0);
			UpdateManifestData(shipment3, 0);
			Factory.Save();
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("HasSameShipments? - No, an additional shipment has just been added", false, wrapper.HasSameShipments);
		}

		public void TestHasSameShipmentsChangeMessage()
		{
			shipment1.JS_UniqueConsignRef = shipment1.JS_HouseBill = "S00047238";
			shipment2.JS_UniqueConsignRef = shipment2.JS_HouseBill = "S00047239";
			shipment3.Delete();
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			var outgoingMessage1 = Factory.New<CMRMessage>();
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
			outgoingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageSubType = "CHG";
			outgoingMessage1.EM_Status = "SNT";
			outgoingMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:4+4'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+6+1'";
			consol.Messages.Add(outgoingMessage1);

			var outgoingMessage2 = Factory.New<CMRMessage>();
			outgoingMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-20);
			outgoingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_MessageSubType = "CHG";
			outgoingMessage2.EM_Status = "SNT";
			outgoingMessage2.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:5+4'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+26+1'";
			consol.Messages.Add(outgoingMessage2);

			var responseMessage1 = Factory.New<CMRMessage>();
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now;
			responseMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageSubType = "CLR";
			responseMessage1.EM_Status = "RCV";
			consol.Messages.Add(responseMessage1);
			AssertEquals("HasSameShipments - no, previous cleared message was a change message", false, wrapper.HasSameShipments);

			shipment3 = consol.Shipments.AddNew();
			declaration3 = CreateExportDeclarationForShipment(shipment3);
			shipment3.JS_UniqueConsignRef = shipment3.JS_HouseBill = "S00047240";
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("HasSameShipments - No, an additional shipment has just been added", false, wrapper.HasSameShipments);
		}

		public void TestHasSameShipmentsWhenHBNotUniqueID()
		{
			shipment1.JS_UniqueConsignRef = "S00047238";
			shipment1.JS_HouseBill = "HWB0001";
			shipment2.JS_UniqueConsignRef = "S00047239";
			shipment2.JS_HouseBill = "68349924";
			shipment3.Delete();
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			var outgoingMessage1 = Factory.New<CMRMessage>();
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now;
			outgoingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageSubType = "ORG";
			outgoingMessage1.EM_Status = "SNT";
			outgoingMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:1+9'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:HWB0001'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:68349924'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+6+1'";
			consol.Messages.Add(outgoingMessage1);

			var outgoingMessage2 = Factory.New<CMRMessage>();
			outgoingMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(2);
			outgoingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_MessageSubType = "AMD";
			outgoingMessage2.EM_Status = "SNT";
			outgoingMessage2.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:5+5'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:HWB0001'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:68349924'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+26+1'";
			consol.Messages.Add(outgoingMessage2);

			var responseMessage1 = Factory.New<CMRMessage>();
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(5);
			responseMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageSubType = "CLR";
			responseMessage1.EM_Status = "RCV";
			consol.Messages.Add(responseMessage1);
			AssertEquals("HasSameShipments", true, wrapper.HasSameShipments);

			shipment3 = consol.Shipments.AddNew();
			declaration3 = CreateExportDeclarationForShipment(shipment3);
			shipment3.JS_UniqueConsignRef = "S00047240";
			shipment3.JS_HouseBill = "HB042438";
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("HasSameShipments - No, an additional shipment has been added", false, wrapper.HasSameShipments);
		}

		public void TestNotShipmentsWhenHBNotUniqueIDChangeMessage()
		{
			shipment1.JS_UniqueConsignRef = "S00047238";
			shipment1.JS_HouseBill = "HWB0001";
			shipment2.JS_UniqueConsignRef = "S00047239";
			shipment2.JS_HouseBill = "68349924";
			shipment3.Delete();
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			var outgoingMessage1 = Factory.New<CMRMessage>();
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now;
			outgoingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageSubType = "CHG";
			outgoingMessage1.EM_Status = "SNT";
			outgoingMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:4+4'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:HWB0001'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:68349924'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+6+1'";
			consol.Messages.Add(outgoingMessage1);

			var outgoingMessage2 = Factory.New<CMRMessage>();
			outgoingMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(2);
			outgoingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_MessageSubType = "CHG";
			outgoingMessage2.EM_Status = "SNT";
			outgoingMessage2.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:5+4'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:HWB0001'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:68349924'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+26+1'";
			consol.Messages.Add(outgoingMessage2);

			var responseMessage1 = Factory.New<CMRMessage>();
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(5);
			responseMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageSubType = "CLR";
			responseMessage1.EM_Status = "RCV";
			consol.Messages.Add(responseMessage1);
			AssertEquals("HasSameShipments - no, previous cleared message was a change message", false, wrapper.HasSameShipments);

			shipment3 = consol.Shipments.AddNew();
			declaration3 = CreateExportDeclarationForShipment(shipment3);
			shipment3.JS_UniqueConsignRef = "S00047240";
			shipment3.JS_HouseBill = "HB042438";
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("HasSameShipments - No, an additional shipment has been added", false, wrapper.HasSameShipments);
		}

		public void TestHasSameShipmentsWhenBlankHBsUsed()
		{
			shipment1.JS_UniqueConsignRef = "S00047238";
			shipment1.JS_HouseBill = ZString.Empty;
			var shipment1Entry = shipment1.CusEntryNumbers.AddNew();
			shipment1Entry.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
			shipment1Entry.CE_EntryNum = "ACKPYWF93";

			shipment2.JS_UniqueConsignRef = "S00047239";
			shipment2.JS_HouseBill = ZString.Empty;
			var shipment2Entry = shipment2.CusEntryNumbers.AddNew();
			shipment2Entry.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
			shipment2Entry.CE_EntryNum = "AAAC3RPC6";

			shipment3.JS_UniqueConsignRef = "S00047240";
			shipment3.JS_HouseBill = "S00047240";
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			var outgoingMessage1 = Factory.New<CMRMessage>();
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now;
			outgoingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageSubType = "CHG";
			outgoingMessage1.EM_Status = "SNT";
			outgoingMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:4+4'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+TN:ACKPYWF93'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TN:AAAC3RPC6'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'CNI+3+:::I'CNT+11:5'RFF+TL:EXLV'GID+1'RFF+HWB:S00047240'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++NEWSPAPER PRINT'UNT+32+1'";
			consol.Messages.Add(outgoingMessage1);

			var responseMessage1 = Factory.New<CMRMessage>();
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(5);
			responseMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageSubType = "CLR";
			responseMessage1.EM_Status = "RCV";
			consol.Messages.Add(responseMessage1);

			shipment1.JS_HouseBill = "NewHBVal";
			AssertEquals("HasSameShipments - HB value added to line1 still same shipment/EDN, but previous message was a change message", false, wrapper.HasSameShipments);

			var shipment4 = consol.Shipments.AddNew();
			var declaration4 = CreateExportDeclarationForShipment(shipment4);
			shipment4.JS_UniqueConsignRef = "S00047241";
			shipment4.JS_HouseBill = "S00047241";
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("HasSameShipments - No, an additional shipment has been added", false, wrapper.HasSameShipments);
		}

		public void TestHasSameShipmentsAlwaysFalseOnceChangeMessageHasBeenUsed()
		{
			shipment1.JS_UniqueConsignRef = shipment1.JS_HouseBill = "S00047238";
			shipment2.JS_UniqueConsignRef = shipment2.JS_HouseBill = "S00047239";
			shipment3.Delete();
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			var outgoingMessage1 = Factory.New<CMRMessage>();
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
			outgoingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageSubType = "CHG";
			outgoingMessage1.EM_Status = "SNT";
			outgoingMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:4+4'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+6+1'";
			consol.Messages.Add(outgoingMessage1);

			var outgoingMessage2 = Factory.New<CMRMessage>();
			outgoingMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-20);
			outgoingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_MessageSubType = "CHG";
			outgoingMessage2.EM_Status = "SNT";
			outgoingMessage2.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:5+4'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+26+1'";
			consol.Messages.Add(outgoingMessage2);

			var responseMessage1 = Factory.New<CMRMessage>();
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now;
			responseMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageSubType = "CLR";
			responseMessage1.EM_Status = "RCV";
			consol.Messages.Add(responseMessage1);
			AssertEquals("HasSameShipments - No, a change message has been used", false, wrapper.HasSameShipments);
		}

		public void TestHasSameShipmentsWhenNoConsignmentSequenceData()
		{
			shipment1.JS_UniqueConsignRef = "S00047238";
			shipment1.JS_HouseBill = ZString.Empty;
			var shipment1Entry = shipment1.CusEntryNumbers.AddNew();
			shipment1Entry.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
			shipment1Entry.CE_EntryNum = "ACKPYWF93";

			shipment2.JS_UniqueConsignRef = "S00047239";
			shipment2.JS_HouseBill = ZString.Empty;
			var shipment2Entry = shipment2.CusEntryNumbers.AddNew();
			shipment2Entry.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
			shipment2Entry.CE_EntryNum = "AAAC3RPC6";

			shipment3.JS_UniqueConsignRef = "S00047240";
			shipment3.JS_HouseBill = "S00047240";

			var outgoingMessage1 = Factory.New<CMRMessage>();
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now;
			outgoingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageSubType = "ORG";
			outgoingMessage1.EM_Status = "SNT";
			outgoingMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:1+9'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+TN:ACKPYWF93'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TN:AAAC3RPC6'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'CNI+3+:::I'CNT+11:5'RFF+TL:EXLV'GID+1'RFF+HWB:S00047240'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++NEWSPAPER PRINT'UNT+32+1'";
			consol.Messages.Add(outgoingMessage1);

			var responseMessage1 = Factory.New<CMRMessage>();
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(5);
			responseMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageSubType = "CLR";
			responseMessage1.EM_Status = "RCV";
			consol.Messages.Add(responseMessage1);

			shipment1.JS_HouseBill = "NewHBVal";
			AssertEquals("HasSameShipments - HB value added to line1 still same shipment/EDN", true, wrapper.HasSameShipments);

			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			var shipment4 = consol.Shipments.AddNew();
			var declaration4 = CreateExportDeclarationForShipment(shipment4);
			shipment4.JS_UniqueConsignRef = "S00047241";
			shipment4.JS_HouseBill = "S00047241";
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("HasSameShipments - No, an additional shipment has been added", false, wrapper.HasSameShipments);
		}

		public void TestHasSameShipmentsWithHVL()
		{
			shipment1.JS_UniqueConsignRef = shipment1.JS_HouseBill = "S00047238";
			shipment2.JS_UniqueConsignRef = shipment2.JS_HouseBill = "S00047239";
			shipment3.Delete();
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			var outgoingMessage1 = Factory.New<CMRMessage>();
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
			outgoingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageSubType = "ORG";
			outgoingMessage1.EM_Status = "SNT";
			outgoingMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:1+9'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+6+1'";
			consol.Messages.Add(outgoingMessage1);

			var outgoingMessage2 = Factory.New<CMRMessage>();
			outgoingMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-20);
			outgoingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_MessageSubType = "AMD";
			outgoingMessage2.EM_Status = "SNT";
			outgoingMessage2.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:2+5'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+26+1'";
			consol.Messages.Add(outgoingMessage2);

			var responseMessage1 = Factory.New<CMRMessage>();
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now;
			responseMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageSubType = "CLR";
			responseMessage1.EM_Status = "RCV";
			consol.Messages.Add(responseMessage1);
			Factory.Save();
			AssertEquals("HasSameShipments", true, wrapper.HasSameShipments);

			var shipment4 = AddHVLShipmentToConsol(consol, "HVL-001", consol.ShipmentCount);
			Factory.Save();
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("HasSameShipments? - No, a HVL shipment has just been added", false, wrapper.HasSameShipments);

			UpdateLinesToManifested(consol);
			Factory.Save();
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("HasSameShipments? - Yes, HVL shipment consignments & Std Shipments have not changed", true, wrapper.HasSameShipments);

			ForwardingShipment hvlvShipment = (ForwardingShipment)shipment4;
			foreach (IHVLVConsignment consignmentLine in hvlvShipment.HVLVConsignments)
			{
				if (consignmentLine.HVC_WaybillNumber == "HVLV2")
				{
					consignmentLine.Delete();
				}
			}

			Factory.Save();
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("HasSameShipments? - No, HVL shipment consignments have changed", false, wrapper.HasSameShipments);
		}

		public void TestGetLastLineNoUsed()
		{
			shipment1.JS_UniqueConsignRef = "S00047238";
			shipment2.JS_UniqueConsignRef = "S00047239";
			shipment3.Delete();
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			EDIMessage outgoingMessage1 = Factory.New<CMRMessage>();
			outgoingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageSubType = "CHG";
			outgoingMessage1.EM_Status = "SNT";
			outgoingMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:4+4'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+6+1'";
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-5);
			consol.Messages.Add(outgoingMessage1);

			EDIMessage outgoingMessage2 = Factory.New<CMRMessage>();
			outgoingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_MessageSubType = "CHG";
			outgoingMessage2.EM_Status = "SNT";
			outgoingMessage2.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:6+4'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:9'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'CNI+3+:::I'CNT+11:5'RFF+TL:EXLV'GID+1'RFF+HWB:S00047249'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++NEWSPAPER PRINT'UNT+35+1'";
			outgoingMessage2.EM_SystemCreateTimeUtc = ZDateTime.Today;
			consol.Messages.Add(outgoingMessage2);

			EDIMessage responseMessage1 = Factory.New<CMRMessage>();
			responseMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageSubType = "CLR";
			responseMessage1.EM_Status = "RCV";
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddMinutes(5);
			consol.Messages.Add(responseMessage1);

			shipment3 = consol.Shipments.AddNew();
			declaration3 = CreateExportDeclarationForShipment(shipment3);
			shipment3.JS_UniqueConsignRef = "S00047249";
			var shipmentWrapper = new FreightShipmentWrapper(shipment3, consol);
			shipmentWrapper.CreatePreliminaryManifestLineNumber(3);
			shipmentWrapper.UpdatePreliminaryLineNumberToManifestedLineNumber();
			AssertEquals("GetLastLineNoUsed", 3, wrapper.LastLineNoUsed);
		}

		public void TestGetLastLineNoUsedWhen10OrMoreLines()
		{
			var lineCount = 0;
			this.shipment1.Delete();
			this.shipment2.Delete();
			this.shipment3.Delete();

			var shipment1 = AddAdditionalShipmentAndDeclarationToConsol(consol, "S00040001");
			var shipment2 = AddAdditionalShipmentAndDeclarationToConsol(consol, "S00040002");
			var shipment3 = AddAdditionalShipmentAndDeclarationToConsol(consol, "S00040003");
			var shipment4 = AddAdditionalShipmentAndDeclarationToConsol(consol, "S00040004");
			var shipment5 = AddAdditionalShipmentAndDeclarationToConsol(consol, "S00040005");
			var shipment6 = AddAdditionalShipmentAndDeclarationToConsol(consol, "S00040006");
			var shipment7 = AddAdditionalShipmentAndDeclarationToConsol(consol, "S00040007");
			var shipment8 = AddAdditionalShipmentAndDeclarationToConsol(consol, "S00040008");
			var shipment9 = AddAdditionalShipmentAndDeclarationToConsol(consol, "S00040009");
			var shipment10 = AddAdditionalShipmentAndDeclarationToConsol(consol, "S00040010");
			var shipment11 = AddAdditionalShipmentAndDeclarationToConsol(consol, "S00040011");
			var shipment12 = AddAdditionalShipmentAndDeclarationToConsol(consol, "S00040012");
			var shipment13 = AddAdditionalShipmentAndDeclarationToConsol(consol, "S00040013");
			var shipment14 = AddAdditionalShipmentAndDeclarationToConsol(consol, "S00040014");
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			var outgoingMessageORG = Factory.New<CMRMessage>();
			outgoingMessageORG.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessageORG.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessageORG.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessageORG.EM_MessageSubType = "ORG";
			outgoingMessageORG.EM_Status = "CLR";
			outgoingMessageORG.EM_SystemCreateTimeUtc = new ZDateTime(2014, 10, 24, 06, 40, 00);
			outgoingMessageORG.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026895/CMT1:1+9'TDT+20+++6'DTM+136:20141024:102'GIS+C:121:95'CNT+11:140'CNI+1'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040001'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+2'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040002'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+3'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040003'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+4'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040004'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+5'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040005'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+6'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040006'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+7'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040007'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+8'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040008'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+9'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040009'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+10'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040010'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+11'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040011'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+12'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040012'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+13'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040013'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'CNI+14'CNT+11:10'RFF+TL:EXPE'GID+1'RFF+HWB:S00040014'NAD+GO+++JACK JONES'GID+1'FTX+AAA+++FISH LIVERS AND ROES, FRESH'UNT+119+1'";
			consol.Messages.Add(outgoingMessageORG);

			EDIMessage responseMessage = Factory.New<CMRMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageSubType = "CLR";
			responseMessage.EM_Status = "RCV";
			responseMessage.EM_SystemCreateTimeUtc = ZDateTime.Today.AddMinutes(5);
			consol.Messages.Add(responseMessage);

			AssertEquals("GetLastLineNoUsed should return 14 - the line number value of the last line in the message", 14, wrapper.LastLineNoUsed);
		}

		public void TestGetRemovedConsignmentsLines()
		{
			shipment1.JS_UniqueConsignRef = shipment1.JS_HouseBill = "S00047238";
			shipment2.JS_UniqueConsignRef = shipment2.JS_HouseBill = "S00047239";
			shipment3.Delete();
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			var outgoingMessage1 = Factory.New<CMRMessage>();
			outgoingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			outgoingMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageSubType = "CHG";
			outgoingMessage1.EM_Status = "SNT";
			outgoingMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:4+4'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+6+1'";
			consol.Messages.Add(outgoingMessage1);

			var responseMessage1 = Factory.New<CMRMessage>();
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Today;
			responseMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageSubType = "CLR";
			responseMessage1.EM_Status = "RCV";
			consol.Messages.Add(responseMessage1);
			AssertEquals("HasSameShipments? - no, previous cleared message was a change message", false, wrapper.HasSameShipments);

			shipment3 = consol.Shipments.AddNew();
			declaration3 = CreateExportDeclarationForShipment(shipment3);
			shipment3.JS_UniqueConsignRef = shipment3.JS_HouseBill = "S00047249";
			UpdateManifestData(shipment3, 3);

			consol.Shipments.Remove(shipment2);
			consol.Shipments.Remove(shipment1);
			Factory.Save();
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("HasSameShipments", false, wrapper.HasSameShipments);
			IEnumerable<ZInt> removedLines = wrapper.GetRemovedConsignmentsLineNo();
			AssertEquals("detachedLines", 2, removedLines.Count());
			bool expectedResult;
			foreach (ZInt consignment in removedLines)
			{
				expectedResult = false;
				if (consignment == 1)
				{
					expectedResult = true;
				}

				if (consignment == 2)
				{
					expectedResult = true;
				}

				Assert(expectedResult);
			}
		}

		public void TestGetRemovedConsignmentsLinesWhenNoHB()
		{
			shipment1.JS_UniqueConsignRef = "S00047238";
			shipment1.JS_HouseBill = ZString.Empty;
			var shipment1Entry = shipment1.CusEntryNumbers.AddNew();
			shipment1Entry.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
			shipment1Entry.CE_EntryNum = "ACKPYWF93";
			shipment1Entry.CE_ParentTable = shipment1.TableName;

			shipment2.JS_UniqueConsignRef = "S00047239";
			shipment2.JS_HouseBill = ZString.Empty;
			var shipment2Entry = shipment2.CusEntryNumbers.AddNew();
			shipment2Entry.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
			shipment2Entry.CE_EntryNum = "AAAC3RPC6";
			shipment2Entry.CE_ParentTable = shipment2.TableName;

			consol.Shipments.Remove(shipment3);
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			var outgoingMessage1 = Factory.New<CMRMessage>();
			outgoingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			outgoingMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageSubType = "CHG";
			outgoingMessage1.EM_Status = "SNT";
			outgoingMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:4+4'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:2'CNI+1+:::A'CNT+11:3'RFF+TN:ACKPYWF93'GID+1'CNI+2+:::A'CNT+11:1'RFF+TN:AAAC3RPC6'GID+1'UNT+15+1'";
			consol.Messages.Add(outgoingMessage1);

			var responseMessage1 = Factory.New<CMRMessage>();
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Today;
			responseMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageSubType = "CLR";
			responseMessage1.EM_Status = "RCV";
			consol.Messages.Add(responseMessage1);
			AssertEquals("HasSameShipments - no, previous cleared message was a change message", false, wrapper.HasSameShipments);

			shipment3 = consol.Shipments.AddNew();
			declaration3 = CreateExportDeclarationForShipment(shipment3);
			shipment3.JS_UniqueConsignRef = shipment3.JS_HouseBill = "S00047249";

			consol.Shipments.Remove(shipment2);
			consol.Shipments.Remove(shipment1);
			Factory.Save();
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("HasSameShipments", false, wrapper.HasSameShipments);
			IEnumerable<ZInt> removedLines = wrapper.GetRemovedConsignmentsLineNo();
			AssertEquals("detachedLines", 2, removedLines.Count());
			bool expectedResult;
			foreach (ZInt consignment in removedLines)
			{
				expectedResult = false;
				if (consignment == 1)
				{
					expectedResult = true;
				}

				if (consignment == 2)
				{
					expectedResult = true;
				}

				Assert(expectedResult);
			}
		}

		public void TestGetRemovedConsignmentsLinesWhenHBRemovedButLineHasEDN()
		{
			shipment1.JS_UniqueConsignRef = "S00047238";
			shipment1.JS_HouseBill = "HB7";
			var shipment1Entry = shipment1.CusEntryNumbers.AddNew();
			shipment1Entry.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
			shipment1Entry.CE_EntryNum = "AAAC3R4HM";
			shipment1Entry.CE_ParentTable = shipment1.TableName;

			consol.Shipments.Remove(shipment2);
			consol.Shipments.Remove(shipment3);
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			var outgoingMessage1 = Factory.New<CMRMessage>();
			outgoingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			outgoingMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageSubType = "ORG";
			outgoingMessage1.EM_Status = "SNT";
			outgoingMessage1.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:1+9'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:2'CNI+1'CNT+11:3'RFF+TN:AAAC3R4HM'GID+1'RFF+HWB:HB7'GID+1'UNT+15+1'";
			consol.Messages.Add(outgoingMessage1);

			var responseMessage1 = Factory.New<CMRMessage>();
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Today;
			responseMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageSubType = "CLR";
			responseMessage1.EM_Status = "RCV";
			consol.Messages.Add(responseMessage1);
			AssertEquals("HasSameShipments", true, wrapper.HasSameShipments);

			shipment1.JS_HouseBill = "";
			Factory.Save();

			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("HasSameShipments", true, wrapper.HasSameShipments);
			IEnumerable<ZInt> removedLines = wrapper.GetRemovedConsignmentsLineNo();
			AssertEquals("detachedLines - should be none as line should be found by CusCodeManifestData even though HB has been removed from line", 0, removedLines.Count());
		}

		public void TestGetRemovedConsignmentsLinesDoesNotProduceDuplicates()
		{
			shipment1.JS_UniqueConsignRef = shipment1.JS_HouseBill = "S00047654";
			shipment1.JS_HouseBill = "H1";
			var entryNum1 = Factory.New<AUCusEntryNumber>();
			entryNum1.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
			entryNum1.CE_EntryNum = "AAAC3R3WL";
			entryNum1.CE_ParentID = shipment1.PK;
			entryNum1.CE_ParentTable = shipment1.TableName;

			shipment2.JS_UniqueConsignRef = shipment2.JS_HouseBill = "S00047656";
			shipment2.JS_HouseBill = "BIL3";
			var entryNum2 = Factory.New<AUCusEntryNumber>();
			entryNum2.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
			entryNum2.CE_EntryNum = "AAAC3R9G6";
			entryNum2.CE_ParentID = shipment2.PK;
			entryNum2.CE_ParentTable = shipment2.TableName;

			shipment3.JS_UniqueConsignRef = shipment2.JS_HouseBill = "S00047657";
			shipment3.JS_HouseBill = "BZ";
			var entryNum3 = Factory.New<AUCusEntryNumber>();
			entryNum3.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
			entryNum3.CE_EntryNum = "AAAC3R333";
			entryNum3.CE_ParentID = shipment3.PK;
			entryNum3.CE_ParentTable = shipment3.TableName;

			var shipment4 = consol.Shipments.AddNew();
			var declaration4 = CreateExportDeclarationForShipment(shipment4);
			shipment4.JS_UniqueConsignRef = "S00047660";
			shipment4.JS_HouseBill = "JJJJJJ";
			var entryNum4 = Factory.New<AUCusEntryNumber>();
			entryNum4.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
			entryNum4.CE_EntryNum = "AAAC3R4HM";
			entryNum4.CE_ParentID = shipment4.PK;
			entryNum4.CE_ParentTable = shipment4.TableName;

			var shipment5 = consol.Shipments.AddNew();
			var declaration5 = CreateExportDeclarationForShipment(shipment5);
			shipment5.JS_UniqueConsignRef = "S00047661";
			shipment5.JS_HouseBill = "GEE2";
			var entryNum5 = Factory.New<AUCusEntryNumber>();
			entryNum5.CE_EntryType = CMRExportExemptionCodes.EXLV.Code;
			entryNum5.CE_ParentID = shipment5.PK;
			entryNum5.CE_ParentTable = shipment5.TableName;

			var shipment6 = consol.Shipments.AddNew();
			var declaration6 = CreateExportDeclarationForShipment(shipment6);
			shipment6.JS_UniqueConsignRef = "S00047662";
			shipment6.JS_HouseBill = "HJKL";
			var entryNum6 = Factory.New<AUCusEntryNumber>();
			entryNum6.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
			entryNum6.CE_EntryNum = "AAAC3R4EW";
			entryNum6.CE_ParentID = shipment6.PK;
			entryNum6.CE_ParentTable = shipment6.TableName;

			var shipment7 = consol.Shipments.AddNew();
			var declaration7 = CreateExportDeclarationForShipment(shipment7);
			shipment7.JS_UniqueConsignRef = "S00047663";
			shipment7.JS_HouseBill = "912";
			var entryNum7 = Factory.New<AUCusEntryNumber>();
			entryNum7.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
			entryNum7.CE_EntryNum = "AAAC3R4F4";
			entryNum7.CE_ParentID = shipment7.PK;
			entryNum7.CE_ParentTable = shipment7.TableName;

			var shipment8 = consol.Shipments.AddNew();
			var declaration8 = CreateExportDeclarationForShipment(shipment8);
			shipment8.JS_UniqueConsignRef = "S00047664";
			shipment8.JS_HouseBill = "LAST";
			var entryNum8 = Factory.New<AUCusEntryNumber>();
			entryNum8.CE_EntryType = CMRExportExemptionCodes.EXLV.Code;
			entryNum8.CE_ParentID = shipment8.PK;
			entryNum8.CE_ParentTable = shipment8.TableName;

			var shipment9 = consol.Shipments.AddNew();
			var declaration9 = CreateExportDeclarationForShipment(shipment9);
			shipment9.JS_UniqueConsignRef = "S00047658";
			shipment9.JS_HouseBill = "JK";
			var entryNum9 = Factory.New<AUCusEntryNumber>();
			entryNum9.CE_EntryType = CusEntryNumberTypes.Australia.CAN;
			entryNum9.CE_EntryNum = "AAAC3R4CS";
			entryNum9.CE_ParentID = shipment9.PK;
			entryNum9.CE_ParentTable = shipment9.TableName;

			var shipment10 = consol.Shipments.AddNew();
			var declaration10 = CreateExportDeclarationForShipment(shipment10);
			shipment10.JS_UniqueConsignRef = "S00047659";
			shipment10.JS_HouseBill = "BIL3";
			var entryNum10 = Factory.New<AUCusEntryNumber>();
			entryNum10.CE_EntryType = CMRExportExemptionCodes.EXLV.Code;
			entryNum10.CE_ParentID = shipment10.PK;
			entryNum10.CE_ParentTable = shipment10.TableName;

			var shipment11 = consol.Shipments.AddNew();
			var declaration11 = CreateExportDeclarationForShipment(shipment11);
			shipment11.JS_UniqueConsignRef = "S00047675";
			shipment11.JS_HouseBill = "BIL1";
			var entryNum11 = Factory.New<AUCusEntryNumber>();
			entryNum11.CE_EntryType = CMRExportExemptionCodes.EXLV.Code;
			entryNum11.CE_ParentID = shipment11.PK;
			entryNum11.CE_ParentTable = shipment11.TableName;

			var shipment12 = consol.Shipments.AddNew();
			var declaration12 = CreateExportDeclarationForShipment(shipment12);
			shipment12.JS_UniqueConsignRef = "S00047682";
			shipment12.JS_HouseBill = "BIL3";
			var entryNum12 = Factory.New<AUCusEntryNumber>();
			entryNum12.CE_EntryType = CMRExportExemptionCodes.EXLV.Code;
			entryNum12.CE_ParentID = shipment12.PK;
			entryNum12.CE_ParentTable = shipment12.TableName;

			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			var outgoingMessage = Factory.New<CMRMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			outgoingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageSubType = "CHG";
			outgoingMessage.EM_Status = "SNT";
			outgoingMessage.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00027040/CMT1:5+4'RFF+AIZ:AAAC3R9CG'TDT+20+++6'DTM+136:20150324:102'GIS+C:121:95'CNT+11:11'CNI+1+:::A'CNT+11:1'RFF+TN:AAAC3R3WL'GID+1'RFF+HWB:H1'GID+1'CNI+4+:::A'CNT+11:1'RFF+TN:AAAC3R333'GID+1'RFF+HWB:BZ'GID+1'CNI+7+:::A'CNT+11:1'RFF+TN:AAAC3R4HM'GID+1'RFF+HWB:JJJJJJ'GID+1'CNI+8+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:GEE2'LOC+28+NZ::5'NAD+GO+++CARGOWISE AUSTRALIA'GID+1'FTX+AAA+++KKG'CNI+9+:::A'CNT+11:1'RFF+TN:AAAC3R4EW'GID+1'RFF+HWB:HJKL'GID+1'CNI+10+:::A'CNT+11:1'RFF+TN:AAAC3R4F4'GID+1'RFF+HWB:912'GID+1'CNI+11+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:LAST'LOC+28+NZ::5'NAD+GO+++CARGOWISE AUSTRALIA'GID+1'FTX+AAA+++JJJJJ'CNI+12+:::A'CNT+11:1'RFF+TN:AAAC3R4CS'GID+1'RFF+HWB:JK'GID+1'CNI+14+:::A'CNT+11:1'RFF+TN:AAAC3R9G6'GID+1'RFF+HWB:BIL3'GID+1'CNI+15+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:BIL1'LOC+28+NZ::5'NAD+GO+++CARGOWISE AUSTRALIA'GID+1'FTX+AAA+++BITS'CNI+16+:::A'CNT+11:1'RFF+TL:EXPE'GID+1'RFF+HWB:BIL2'LOC+28+NZ::5'NAD+GO+++CARGOWISE AUSTRALIA'GID+1'FTX+AAA+++BITS'CNI+17+:::D'UNT+87+1'";
			consol.Messages.Add(outgoingMessage);

			var responseMessage = Factory.New<CMRMessage>();
			responseMessage.EM_SystemCreateTimeUtc = ZDateTime.Today;
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageSubType = "CLR";
			responseMessage.EM_Status = "RCV";
			consol.Messages.Add(responseMessage);
			AssertEquals("HasSameShipments - no, previous cleared message was a change message", false, wrapper.HasSameShipments);

			consol.Shipments.Remove(shipment9);
			consol.Shipments.Remove(shipment11);
			consol.Shipments.Remove(shipment12);
			Factory.Save();

			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("HasSameShipments", false, wrapper.HasSameShipments);
			IEnumerable<ZInt> removedLines = wrapper.GetRemovedConsignmentsLineNo();
			AssertEquals("detachedLines - should be 3 detached consignments", 3, removedLines.Count());
			bool expectedResult;
			foreach (ZInt consignment in removedLines)
			{
				expectedResult = false;
				if (consignment == 9)
				{
					expectedResult = true;
				}

				if (consignment == 11)
				{
					expectedResult = true;
				}

				if (consignment == 12)
				{
					expectedResult = true;
				}

				Assert(expectedResult);
			}
		}

		public void TestRemovedHVLLineConsignments()
		{
			var outgoingMessage2 = Factory.New<CMRMessage>();
			outgoingMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-20);
			outgoingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_MessageSubType = "AMD";
			outgoingMessage2.EM_Status = "SNT";
			outgoingMessage2.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:2+5'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+26+1'";
			consol.Messages.Add(outgoingMessage2);

			var responseMessage1 = Factory.New<CMRMessage>();
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now;
			responseMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageSubType = "CLR";
			responseMessage1.EM_Status = "RCV";
			consol.Messages.Add(responseMessage1);

			shipment1.JS_UniqueConsignRef = shipment1.JS_HouseBill = "S00047238";
			shipment2.JS_UniqueConsignRef = shipment2.JS_HouseBill = "S00047239";
			consol.Shipments.Remove(shipment3);
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			var shipment4 = AddHVLShipmentToConsol(consol, "HVL-001", consol.ShipmentCount);
			Factory.Save();
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("RemovedConsignmentsLines", 0, wrapper.RemovedConsignmentsLineNo.Count);

			UpdateLinesToManifested(consol);
			Factory.Save();
			ForwardingShipment hvlvShipment = (ForwardingShipment)shipment4;
			foreach (IHVLVConsignment consignmentLine in hvlvShipment.HVLVConsignments)
			{
				if (consignmentLine.HVC_WaybillNumber == "HVLV2" || consignmentLine.HVC_WaybillNumber == "HVLV3")
				{
					consignmentLine.Delete();
				}
			}

			Factory.Save();
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("RemovedConsignmentsLines", 2, wrapper.RemovedConsignmentsLineNo.Count);
		}

		public void TestRemovedLineConsignmentsWhenMultipleHVLShipments()
		{
			var outgoingMessage2 = Factory.New<CMRMessage>();
			outgoingMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-20);
			outgoingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_MessageSubType = "AMD";
			outgoingMessage2.EM_Status = "SNT";
			outgoingMessage2.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:2+5'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+26+1'";
			consol.Messages.Add(outgoingMessage2);

			var responseMessage1 = Factory.New<CMRMessage>();
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now;
			responseMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageSubType = "CLR";
			responseMessage1.EM_Status = "RCV";
			consol.Messages.Add(responseMessage1);

			shipment1.JS_UniqueConsignRef = shipment1.JS_HouseBill = "S00047238";
			shipment2.JS_UniqueConsignRef = shipment2.JS_HouseBill = "S00047239";
			consol.Shipments.Remove(shipment3);
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			int consignmentCount = consol.ShipmentCount;
			var shipment4 = AddHVLShipmentToConsol(consol, "HVL-001", consignmentCount);
			var shipment5 = AddHVLShipmentToConsol(consol, "HVL-002", consignmentCount + 4);
			Factory.Save();
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("RemovedConsignmentsLines", 0, wrapper.RemovedConsignmentsLineNo.Count);

			UpdateLinesToManifested(consol);
			Factory.Save();
			var hvlvShipment1 = (ForwardingShipment)shipment4;
			foreach (IHVLVConsignment consignmentLine in hvlvShipment1.HVLVConsignments)
			{
				if (consignmentLine.HVC_WaybillNumber == "HVLV2" || consignmentLine.HVC_WaybillNumber == "HVLV3")
				{
					consignmentLine.Delete();
				}
			}

			var hvlvShipment2 = (ForwardingShipment)shipment5;
			foreach (IHVLVConsignment consignmentLine in hvlvShipment2.HVLVConsignments)
			{
				if (consignmentLine.HVC_WaybillNumber == "HVLV4")
				{
					consignmentLine.Delete();
				}
			}

			Factory.Save();
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("RemovedConsignmentsLines - 2 removed from first HVL Shipment + 1 removed from second HVL shipment", 3, wrapper.RemovedConsignmentsLineNo.Count);

			consol.Shipments.Remove(shipment5);
			Factory.Save();
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("RemovedConsignmentsLines - now second HVL shipment with 3 remaining lines detached also", 6, wrapper.RemovedConsignmentsLineNo.Count);

			consol.Shipments.Remove(shipment4);
			Factory.Save();
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("RemovedConsignmentsLines - now first HVL shipment with its 2 remaining lines also detached", 8, wrapper.RemovedConsignmentsLineNo.Count);
		}

		public void TestRemovedConsignmentsLinesAllHVLShipmentsDetached()
		{
			var outgoingMessage2 = Factory.New<CMRMessage>();
			outgoingMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-20);
			outgoingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage2.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_MessageSubType = "AMD";
			outgoingMessage2.EM_Status = "SNT";
			outgoingMessage2.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026869/CMT2:2+5'RFF+AIZ:AAAC3CX6C'TDT+20+++6'DTM+136:20141015:102'GIS+C:121:95'CNT+11:4'CNI+1+:::A'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047238'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++STUFF D'CNI+2+:::A'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047239'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++WISETECH'UNT+26+1'";
			consol.Messages.Add(outgoingMessage2);

			var responseMessage1 = Factory.New<CMRMessage>();
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now;
			responseMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage1.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage1.EM_MessageSubType = "CLR";
			responseMessage1.EM_Status = "RCV";
			consol.Messages.Add(responseMessage1);

			shipment1.JS_UniqueConsignRef = shipment1.JS_HouseBill = "S00047238";
			shipment2.JS_UniqueConsignRef = shipment2.JS_HouseBill = "S00047239";
			consol.Shipments.Remove(shipment3);
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			int consignmentCount = consol.ShipmentCount;
			var shipment4 = AddHVLShipmentToConsol(consol, "HVL-001", consignmentCount);
			var shipment5 = AddHVLShipmentToConsol(consol, "HVL-002", consignmentCount + 4);
			Factory.Save();
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("RemovedConsignmentsLines", 0, wrapper.RemovedConsignmentsLineNo.Count);

			UpdateLinesToManifested(consol);
			Factory.Save();
			var hvlvShipment1 = (ForwardingShipment)shipment4;
			foreach (IHVLVConsignment consignmentLine in hvlvShipment1.HVLVConsignments)
			{
				if (consignmentLine.HVC_WaybillNumber == "HVLV2" || consignmentLine.HVC_WaybillNumber == "HVLV3")
				{
					consignmentLine.Delete();
				}
			}

			var hvlvShipment2 = (ForwardingShipment)shipment5;
			foreach (IHVLVConsignment consignmentLine in hvlvShipment2.HVLVConsignments)
			{
				if (consignmentLine.HVC_WaybillNumber == "HVLV4")
				{
					consignmentLine.Delete();
				}
			}

			consol.Shipments.Remove(shipment4);
			consol.Shipments.Remove(shipment5);
			Factory.Save();
			wrapper = new FreightConsolWrapper(consol);
			AssertEquals("RemovedConsignmentsLines - 8 - both HVL shipments (4 lines each) detached", 8, wrapper.RemovedConsignmentsLineNo.Count);
		}

		public void TestLineConsignments()
		{
			shipment1.JS_UniqueConsignRef = "S00047289";
			shipment2.JS_UniqueConsignRef = "S00047290";
			shipment3.JS_UniqueConsignRef = "S00047291";
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			var outgoingMessage = Factory.New<CMRMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_Status = "SNT";
			outgoingMessage.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00026895/CMT1:1+9'TDT+20+++6'DTM+136:20141024:102'GIS+C:121:95'CNT+11:12'CNI+1'CNT+11:3'RFF+TL:EXLV'GID+1'RFF+HWB:S00047289'LOC+28+CA::5'NAD+GO+++JANDS PTY LTD'GID+1'FTX+AAA+++GOODS'CNI+2'CNT+11:4'RFF+TL:EXLV'GID+1'RFF+HWB:S00047290'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++MAGAZINES'CNI+3'CNT+11:5'RFF+TL:EXLV'GID+1'RFF+HWB:S00047291'LOC+28+CA::5'NAD+GO+++TREETOYS PTY LTD'GID+1'FTX+AAA+++NEWSPAPER PRINT'UNT+34+1'";
			outgoingMessage.EM_SystemCreateTimeUtc = ZDateTime.Today;
			consol.Messages.Add(outgoingMessage);

			var responseMessage = Factory.New<CMRMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageSubType = "CLR";
			responseMessage.EM_Status = "RCV";
			responseMessage.EM_SystemCreateTimeUtc = ZDateTime.Today.AddMinutes(5);
			consol.Messages.Add(responseMessage);
			Factory.Save();

			AssertEquals("LineConsignments", 3, wrapper.LineShipmentConsignments.Count);
			bool expectedResult;
			foreach (KeyValuePair<ZInt, ZGuid> consignment in wrapper.LineShipmentConsignments)
			{
				expectedResult = false;
				if (consignment.Key == 1)
				{
					AssertEquals("LineConsignment 1 - Shipment1", shipment1.PK, consignment.Value);
					expectedResult = true;
				}
				else if (consignment.Key == 2)
				{
					AssertEquals("LineConsignment 2 - Shipment2", shipment2.PK, consignment.Value);
					expectedResult = true;
				}
				else if (consignment.Key == 3)
				{
					AssertEquals("LineConsignment 3 - Shipment3", shipment3.PK, consignment.Value);
					expectedResult = true;
				}

				Assert(expectedResult);
			}
		}

		public void TestLineConsignmentsUseEDNIfNoHB()
		{
			shipment1.JS_UniqueConsignRef = "S00047556";
			shipment1.JS_HouseBill = ZString.Empty;
			shipment2.JS_UniqueConsignRef = "S00047557";
			shipment2.JS_HouseBill = ZString.Empty;
			shipment3.JS_UniqueConsignRef = "S00047558";
			var lineCount = 0;
			foreach (CommonShipment freightShipment in consol.Shipments)
			{
				lineCount++;
				var shipment = new FreightShipmentWrapper(freightShipment, consol);
				shipment.CreatePreliminaryManifestLineNumber(lineCount);
				shipment.UpdatePreliminaryLineNumberToManifestedLineNumber();
			}

			var outgoingMessage = Factory.New<CMRMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			outgoingMessage.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_Status = "SNT";
			outgoingMessage.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN'BGM+87:::ESM+C00027027/CMT1:1+9'TDT+20+++6'DTM+136:20150316:102'GIS+C:121:95'CNT+11:3'CNI+1'CNT+11:1'RFF+TN:AAAC3RN9X'GID+1'CNI+2'CNT+11:1'RFF+TN:AAAC3RPC6'GID+1'CNI+3'CNT+11:1'RFF+TL:EXLV'GID+1'RFF+HWB:S00047558'LOC+28+NZ::5'NAD+GO+++CARGOWISE AUSTRALIA'GID+1'FTX+AAA+++STUFF'UNT+24+1'";
			outgoingMessage.EM_SystemCreateTimeUtc = ZDateTime.Today;
			consol.Messages.Add(outgoingMessage);

			var responseMessage = Factory.New<CMRMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			responseMessage.EM_MessageType = CMRMessage.CMRMessageTypes.ESM;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageSubType = "CLR";
			responseMessage.EM_Status = "RCV";
			responseMessage.EM_SystemCreateTimeUtc = ZDateTime.Today.AddMinutes(5);
			consol.Messages.Add(responseMessage);
			Factory.Save();

			AssertEquals("LineConsignments", 3, wrapper.LineShipmentConsignments.Count);
			bool expectedResult;
			foreach (KeyValuePair<ZInt, ZGuid> consignment in wrapper.LineShipmentConsignments)
			{
				expectedResult = false;
				if (consignment.Key == 1)
				{
					AssertEquals("LineConsignment 1 - Shipment1", shipment1.PK, consignment.Value);
					expectedResult = true;
				}
				else if (consignment.Key == 2)
				{
					AssertEquals("LineConsignment 2 - Shipment2", shipment2.PK, consignment.Value);
					expectedResult = true;
				}
				else if (consignment.Key == 3)
				{
					AssertEquals("LineConsignment 3 - Shipment3", shipment3.PK, consignment.Value);
					expectedResult = true;
				}

				Assert(expectedResult);
			}
		}

		#region Implementation

		CusMAWB fMAWB;
		CusMAWB MAWB
		{
			get
			{
				if (fMAWB == null)
				{
					fMAWB = Factory.New<CusMAWB>();
				}
				return fMAWB;
			}
		}

		CusHAWB fHAWB;
		CusHAWB HAWB
		{
			get
			{
				if (fHAWB == null)
				{
					fHAWB = MAWB.ChildBills.AddNew();
				}
				return fHAWB;
			}
		}

		protected override Customs.Business.FreightConsolWrapper NewFreightConsolWrapper(ForwardingConsol consol)
		{
			return new FreightConsolWrapper(consol);
		}

		protected JobDeclaration CreateExportDeclarationForShipment(CommonShipment shipment)
		{
			JobDeclaration result = shipment.Factory.New<JobDeclaration>();
			result.JE_JS = shipment.PK;
			result.JE_GB = GlbBranch.CurrentBranch.PK;
			result.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			return result;
		}

		void UpdateManifestData(CommonShipment shipment, ZInt lineNo)
		{
			var shipmentWrapper = new FreightShipmentWrapper(shipment, consol);
			shipmentWrapper.CreatePreliminaryManifestLineNumber(lineNo);
			shipmentWrapper.UpdatePreliminaryLineNumberToManifestedLineNumber();
		}

		protected CommonShipment AddAdditionalShipmentAndDeclarationToConsol(ForwardingConsol consol, ZString hawbNo)
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
			return shipment;
		}

		protected CommonShipment AddHVLShipmentToConsol(ForwardingConsol consol, ZString hawbNo, ZInt consignmentCount)
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
			var consignmentWrapper1 = new FreightShipmentWrapper(shipment, consol, consignment1);
			consignmentWrapper1.CreatePreliminaryManifestLineNumber(consignmentCount + 1);

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
			var consignmentWrapper2 = new FreightShipmentWrapper(shipment, consol, consignment2);
			consignmentWrapper2.CreatePreliminaryManifestLineNumber(consignmentCount + 2);

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
			var consignmentWrapper3 = new FreightShipmentWrapper(shipment, consol, consignment3);
			consignmentWrapper3.CreatePreliminaryManifestLineNumber(consignmentCount + 3);

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
			var consignmentWrapper4 = new FreightShipmentWrapper(shipment, consol, consignment4);
			consignmentWrapper4.CreatePreliminaryManifestLineNumber(consignmentCount + 4);

			return shipment;
		}

		protected void UpdateLinesToManifested(ForwardingConsol consol)
		{
			foreach (ForwardingShipment shipment in consol.Shipments)
			{
				if (shipment.IsHighVolumeLowValue)
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

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C000592001";
			shipment1 = consol.Shipments.AddNew();
			shipment2 = consol.Shipments.AddNew();
			shipment3 = consol.Shipments.AddNew();
			declaration1 = CreateExportDeclarationForShipment(shipment1);
			declaration2 = CreateExportDeclarationForShipment(shipment2);
			declaration3 = CreateExportDeclarationForShipment(shipment3);
			wrapper = new FreightConsolWrapper(consol);
		}

		protected FreightConsolWrapper wrapper;

		protected ForwardingConsol consol;
		protected CommonShipment shipment1;
		protected CommonShipment shipment2;
		protected CommonShipment shipment3;
		protected JobDeclaration declaration1;
		protected JobDeclaration declaration2;
		protected JobDeclaration declaration3;

		#endregion
	}
}
