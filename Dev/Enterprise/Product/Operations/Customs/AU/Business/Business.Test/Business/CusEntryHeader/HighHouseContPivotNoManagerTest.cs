using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class HighHouseContPivotNoManagerTest : TestCaseWithFactory
	{
		public void TestAssignLineNumbers()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			declaration.JE_HouseBill = "1";

			var sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;
			declaration.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var manager = entryHeader.HighHouseContPivotNoManager;

			manager.AssignLineNumbers();
			CombineAssertions(() =>
			{
				AssertEquals("HighHouseContPivotNo unchanged", ZShort.Zero, entryHeader.HighHouseContPivotNo);
				AssertEquals("CR_HouseContainerNumber", (ZShort)1, declaration.PackingGroups[0].CR_HouseContainerNumber);
			});

			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill2.CU_HouseBill = "2";

			AssertEquals("PackingGroups - House 2 has Container Number 0 (collection is ordered by Container Number)", "0, 1", GetHouseContainerNumbers(entryHeader.PackingGroups));
			manager.AssignLineNumbers();
			CombineAssertions(() =>
			{
				AssertEquals("HighHouseContPivotNo unchanged", ZShort.Zero, entryHeader.HighHouseContPivotNo);
				AssertEquals("PackingGroup Container Numbers re-assigned", "1, 2", GetHouseContainerNumbers(entryHeader.PackingGroups));
			});

			var houseBill3 = declaration.Bills.AddNew();
			houseBill3.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill3.CU_HouseBill = "3";

			AssertEquals("PackingGroups - House 3 has Container Number 0", "0, 1, 2", GetHouseContainerNumbers(entryHeader.PackingGroups));
			entryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden = 5;
			manager.AssignLineNumbers();
			CombineAssertions(() =>
			{
				AssertEquals("HighHouseContPivotNo unchanged", (ZShort)5, entryHeader.HighHouseContPivotNo);
				AssertEquals("PackingGroup Container Numbers re-assigned", "1, 2, 6", GetHouseContainerNumbers(entryHeader.PackingGroups));
			});

			var houseBill4 = declaration.Bills.AddNew();
			houseBill4.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill4.CU_HouseBill = "4";

			AssertEquals("PackingGroups - House 4 has Container Number 0", "0, 1, 2, 6", GetHouseContainerNumbers(entryHeader.PackingGroups));
			entryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden = 2;
			manager.AssignLineNumbers();
			CombineAssertions(() =>
			{
				AssertEquals("HighHouseContPivotNo unchanged", (ZShort)2, entryHeader.HighHouseContPivotNo);
				AssertEquals("PackingGroup Container Numbers re-assigned", "1, 2, 3, 4", GetHouseContainerNumbers(entryHeader.PackingGroups));
			});

			entryHeader.PackingGroups[2].CR_HouseContainerNumber = 5;
			entryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden = 0;
			manager.AssignLineNumbers();
			CombineAssertions(() =>
			{
				AssertEquals("HighHouseContPivotNo unchanged", ZShort.Zero, entryHeader.HighHouseContPivotNo);
				AssertEquals("PackingGroup Container Numbers unchanged", "1, 2, 3, 4", GetHouseContainerNumbers(entryHeader.PackingGroups));
			});
		}

		string GetHouseContainerNumbers(PackingGroupCollection packingGroups) => string.Join(", ", packingGroups.Cast<PackingGroup>().Select(pg => pg.CR_HouseContainerNumber));

		[TestDate(2023, 05, 10, 01, 00, 00)]
		public void TestRecalculateIfNeeded()
		{
			CMRStatusRecalculationSuspender.SuspendStatusRecalculation(Factory);  // Prevent Factory.Save() triggering the Status Calculator

			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			declaration.JE_HouseBill = "1";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var manager = entryHeader.HighHouseContPivotNoManager;
			manager.ReCalculateIfNeeded();
			AssertEquals("HighHouseContPivotNo unchanged", ZShort.Zero, entryHeader.HighHouseContPivotNo);

			var originalMessage = (CMRIMDMessage)entryHeader.Messages.AddNew(typeof(CMRIMDMessage));
			originalMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			originalMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			originalMessage.EM_MessageText = iMDWithOnePackingLineText;
			entryHeader.Logs.AddNew(Events.StatusChange, CustomsEntryStatus.AwaitingFormalLodge.Code);
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			var clearMessage = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			clearMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			clearMessage.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Clear;
			entryHeader.Logs.AddNew(Events.StatusChange, CustomsEntryStatus.ClearFormalLodge.Code);
			entryHeader.CH_Status = "CFL";
			entryHeader.CH_EntryStatus = "CLR";
			entryHeader.EntryNumber = "123";
			Factory.Save();

			manager.ReCalculateIfNeeded();
			AssertEquals("HighHouseContPivotNo recalculated", (ZShort)1, entryHeader.HighHouseContPivotNo);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			var amendmentMessage = (CMRIMDMessage)entryHeader.Messages.AddNew(typeof(CMRIMDMessage));
			amendmentMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			amendmentMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
			amendmentMessage.EM_MessageText = iMDWithOnePackingLineText.Replace("LIN+1+I", "LIN+2+A");
			entryHeader.Logs.AddNew(Events.StatusChange, CustomsEntryStatus.AwaitingAmendment.Code);
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			var clearMessage2 = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			clearMessage2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			clearMessage2.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Clear;
			entryHeader.Logs.AddNew(Events.StatusChange, CustomsEntryStatus.ClearAmendment.Code);
			Factory.Save();

			manager.ReCalculateIfNeeded();
			AssertEquals("HighHouseContPivotNo recalculated", (ZShort)2, entryHeader.HighHouseContPivotNo);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			var amendmentMessage2 = (CMRIMDMessage)entryHeader.Messages.AddNew(typeof(CMRIMDMessage));
			amendmentMessage2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			amendmentMessage2.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
			amendmentMessage2.EM_MessageText = iMDWithOnePackingLineText.Replace("LIN+2+A", "LIN+3+A");
			entryHeader.Logs.AddNew(Events.StatusChange, CustomsEntryStatus.AwaitingAmendment.Code);
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			var errorResponse = entryHeader.Messages.AddNew(typeof(CMRIMDRMessage));
			errorResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			errorResponse.EM_MessageSubType = EDIMessage.Status.Rejected;
			errorResponse.EM_MessageText = imdrErrorResponse;
			entryHeader.Logs.AddNew(Events.StatusChange, CustomsEntryStatus.FailAmendment.Code);
			Factory.Save();

			manager.ReCalculateIfNeeded();
			AssertEquals("HighHouseContPivotNo unchanged", (ZShort)2, entryHeader.HighHouseContPivotNo);

			var logEntries = string.Join(", ", entryHeader.Logs.Find(c => c.SL_SE_NKEvent == Events.EditedARecordCode).Select(c => c.SL_Reference));
			AssertEquals(@"HighHouseContPivotNo updated from 0 to 0, HighHouseContPivotNo recalculated from 0 to 1. Status:CFL EntryStatus:CLR, HighHouseContPivotNo recalculated from 1 to 2. Status:CFL EntryStatus:CLR", logEntries);
		}

		public void TestReCalculateFromMessages()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();

			declaration.JE_HouseBill = "1";

			var sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;
			declaration.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];

			var now = ZDateTime.UtcNow;

			var packingGroup = entryHeader.PackingGroups[0];
			packingGroup.CR_HouseContainerNumber = 5;
			packingGroup.CR_SystemCreateTimeUtc = now.AddHours(-1);

			entryHeader.Logs.AddNew(Events.StatusChange, "TEST");
			entryHeader.Logs.AddNew(Events.StatusChange, "WFL");
			entryHeader.Logs.AddNew(Events.StatusChange, "CFL");

			Factory.Save();

			var originalMessage = (CMRIMDMessage)entryHeader.Messages.AddNew(typeof(CMRIMDMessage));
			originalMessage.EM_LinkedObject = entryHeader;
			originalMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			originalMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			originalMessage.EM_MessageText = iMDWithTwoPackingLinesText.Replace("LIN+2+I", "LIN+8+I");
			originalMessage.EM_SystemCreateTimeUtc = now.AddHours(-3);
			entryHeader.EntryNumber = "123";

			var amendmentMessage = (CMRIMDMessage)entryHeader.Messages.AddNew(typeof(CMRIMDMessage));
			amendmentMessage.EM_LinkedObject = entryHeader;
			amendmentMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			amendmentMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
			amendmentMessage.EM_MessageText = iMDWithTwoPackingLinesText;
			amendmentMessage.EM_SystemCreateTimeUtc = now.AddHours(-2);

			var clearMessage = entryHeader.Messages.AddNew(typeof(CMRIMDMessage));
			clearMessage.EM_LinkedObject = entryHeader;
			clearMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			clearMessage.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Clear;
			clearMessage.EM_SystemCreateTimeUtc = now.AddHours(-1);

			AssertEquals("Precondition - originalMessage", (ZShort)8, originalMessage.MaxLineNumber);
			AssertEquals("Precondition - amendmentMessage", (ZShort)2, amendmentMessage.MaxLineNumber);

			var manager = entryHeader.HighHouseContPivotNoManager;
			manager.ReCalculateIfNeeded();

			CombineAssertions(() =>
			{
				AssertEquals("HighHouseContPivotNo", (ZShort)2, entryHeader.HighHouseContPivotNo);

				var logEntries = string.Join(", ", entryHeader.Logs.Find(c => c.SL_SE_NKEvent == Events.EditedARecordCode).Select(c => c.SL_Reference));
				AssertEquals("EDT Log", @"HighHouseContPivotNo recalculated from 0 to 2. Status: EntryStatus:", logEntries);
			});
		}

		public void TestReCalculateFromMessages_Held()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();

			declaration.JE_HouseBill = "1";

			var sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;
			declaration.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];

			var now = ZDateTime.UtcNow;

			var packingGroup = entryHeader.PackingGroups[0];
			packingGroup.CR_HouseContainerNumber = 5;
			packingGroup.CR_SystemCreateTimeUtc = now.AddHours(-1);

			entryHeader.Logs.AddNew(Events.StatusChange, "TEST");
			entryHeader.Logs.AddNew(Events.StatusChange, "WFL");
			entryHeader.Logs.AddNew(Events.StatusChange, "CFL");

			Factory.Save();

			var originalMessage = (CMRIMDMessage)entryHeader.Messages.AddNew(typeof(CMRIMDMessage));
			originalMessage.EM_LinkedObject = entryHeader;
			originalMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			originalMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			originalMessage.EM_MessageText = iMDWithTwoPackingLinesText.Replace("LIN+2+I", "LIN+8+I");
			originalMessage.EM_SystemCreateTimeUtc = now.AddHours(-3);
			entryHeader.EntryNumber = "123";

			var amendmentMessage = (CMRIMDMessage)entryHeader.Messages.AddNew(typeof(CMRIMDMessage));
			amendmentMessage.EM_LinkedObject = entryHeader;
			amendmentMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			amendmentMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Amendment;
			amendmentMessage.EM_MessageText = iMDWithTwoPackingLinesText;
			amendmentMessage.EM_SystemCreateTimeUtc = now.AddHours(-2);

			var heldMessage = entryHeader.Messages.AddNew(typeof(CMRIMDMessage));
			heldMessage.EM_LinkedObject = entryHeader;
			heldMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			heldMessage.EM_MessageSubType = CMRMessage.CMRMessageTypes.IMD;
			heldMessage.EM_SystemCreateTimeUtc = now.AddHours(-1);

			AssertEquals("Precondition - originalMessage", (ZShort)8, originalMessage.MaxLineNumber);
			AssertEquals("Precondition - amendmentMessage", (ZShort)2, amendmentMessage.MaxLineNumber);

			var manager = entryHeader.HighHouseContPivotNoManager;
			manager.ReCalculateIfNeeded();

			CombineAssertions(() =>
			{
				AssertEquals("HighHouseContPivotNo", (ZShort)2, entryHeader.HighHouseContPivotNo);
				var logEntries = string.Join(", ", entryHeader.Logs.Find(c => c.SL_SE_NKEvent == Events.EditedARecordCode).Select(c => c.SL_Reference));
				AssertEquals("EDT Log", @"HighHouseContPivotNo recalculated from 0 to 2. Status: EntryStatus:", logEntries);
			});
		}

		public void TestReAssignHouseContainerNumberIfNeeded()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();

			declaration.JE_HouseBill = "1";

			var sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;
			declaration.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];

			var now = ZDateTime.UtcNow;

			var packingGroup1 = entryHeader.PackingGroups[0];
			packingGroup1.CR_HouseContainerNumber = 0;
			packingGroup1.CR_SystemCreateTimeUtc = now.AddHours(-8);

			var packingGroup2 = entryHeader.PackingGroups.AddNew();
			packingGroup2.CR_HouseContainerNumber = 0;
			packingGroup2.CR_CU_HouseBill = declaration.Bills.AddNew().PK;
			packingGroup2.CR_SystemCreateTimeUtc = now.AddHours(-7);

			entryHeader.Logs.AddNew(Events.StatusChange, "TEST");
			entryHeader.Logs.AddNew(Events.StatusChange, "WFL");
			entryHeader.Logs.AddNew(Events.StatusChange, "CFL");

			Factory.Save();

			var originalMessage = (CMRIMDMessage)entryHeader.Messages.AddNew(typeof(CMRIMDMessage));
			originalMessage.EM_LinkedObject = entryHeader;
			originalMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			originalMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			originalMessage.EM_MessageText = iMDWithTwoPackingLinesText;
			originalMessage.EM_SystemCreateTimeUtc = now.AddHours(-2);
			entryHeader.EntryNumber = "123";

			var clearMessage = entryHeader.Messages.AddNew(typeof(CMRIMDMessage));
			clearMessage.EM_LinkedObject = entryHeader;
			clearMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			clearMessage.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Clear;
			clearMessage.EM_SystemCreateTimeUtc = now.AddHours(-1);

			AssertEquals("Precondition - originalMessage", (ZShort)2, originalMessage.MaxLineNumber);

			var manager = entryHeader.HighHouseContPivotNoManager;
			manager.ReCalculateIfNeeded();

			CombineAssertions(() =>
			{
				AssertEquals("HouseContainerNumber reset to 1", (ZShort)1, packingGroup1.CR_HouseContainerNumber);
				AssertEquals("HouseContainerNumber reset to 2", (ZShort)2, packingGroup2.CR_HouseContainerNumber);
			});
		}

		public void TestUpdate()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();

			declaration.JE_HouseBill = "1";

			var sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;
			declaration.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];

			var packingGroup = entryHeader.PackingGroups[0];
			packingGroup.CR_HouseContainerNumber = 0;

			var manager = entryHeader.HighHouseContPivotNoManager;
			manager.Update();

			AssertEquals("HighHouseContPivotNo", ZShort.Zero, entryHeader.HighHouseContPivotNo);

			packingGroup.CR_HouseContainerNumber = 5;
			manager.Update();

			CombineAssertions(() =>
			{
				AssertEquals("HighHouseContPivotNo", (ZShort)5, entryHeader.HighHouseContPivotNo);

				var logEntries = string.Join(", ", entryHeader.Logs.Find(c => c.SL_SE_NKEvent == Events.EditedARecordCode).Select(c => c.SL_Reference));
				AssertEquals("EDT Log", @"HighHouseContPivotNo updated from 0 to 0, HighHouseContPivotNo updated from 0 to 5", logEntries);
			});
		}

		#region Message Text

		readonly string iMDWithOnePackingLineText = @"UNH+1+CUSDEC:D:99B:UN'BGM+929:::IMD+S3IAKL100001904/1/CMT1:2+4'CST++N10::95'LOC+8+AUSYD::6'LOC+12+AUSYD::6'LOC+9+NZAKL::6'LOC+79+AUSYD::6'DTM+178:20230316:102'DTM+260:20230316:102'DTM+252:20230316:102'GIS+Y:153:95'GIS+POR:109:95'GIS+LLB:109:95'GIS+TLB:109:95'MEA+AAE+G+KG:1000.00000'FTX+DEL+++AMY TEST'FTX+CHG+++YFUJKH'RFF+ABQ:ADGHFDGJN'RFF+ADU:S3IAKL100001904/1'RFF+APH:FOB'RFF+AMG:0011'RFF+ADP:0008::Y'RFF+ADP:0009::Y'RFF+ABT:AAAGC3H7F'TDT+20+00001+S+++++9175793::11'NAD+AT+41065894724::95'NAD+WP+001::95'NAD+VT+AA33HF::95'NAD+DP++MASCOT++UNIT 23 635 GARDENERS ROAD++:::NSW+2020+AU'MOA+63:2000.00:AUD'MOA+141:2044.91:AUD'MOA+39:2000.00:AUD'MOA+313:20.00:USD'MOA+71:10.00:USD'UNS+D'DMS+1'LIN+1+I'PAC+++LCL:67:95'PAC+1+1'PCI+1'RFF+AAQ:MAEU5481202'PCI+1'RFF+MB:OBOL123'PCI+1'RFF+BH:JGHTFTKJGFH'CST+1+A::95+N10::95'FTX+AAA+++MOTORS OF AN OUTPUT NOT EXCEEDING 37.5 W'LOC+27+NZ::5'MEA+AAA++NO:1.00000'NAD+VN+123456789012::95'NAD+SU+AAA3336647M::95'MOA+38:1000.00:AUD'RFF+ABD:85011000'RFF+AED:32'RFF+AGW:GEN'RFF+AWA:001'RFF+AAQ:MAEU5481202'RFF+AFV:TV'CST+2+A::95+N10::95'FTX+AAA+++MOTORS OF AN OUTPUT NOT EXCEEDING 37.5 W'LOC+27+NZ::5'MEA+AAA++NO:1.00000'NAD+VN+123456789012::95'NAD+SU+AAA3336647M::95'MOA+38:400.00:AUD'RFF+ABD:85011000'RFF+AED:32'RFF+AGW:GEN'RFF+AWA:001'RFF+AAQ:MAEU5481202'RFF+AFV:TV'CST+3+A::95+N10::95'FTX+AAA+++OF AN OUTPUT NOT EXCEEDING 75 KVA'LOC+27+NZ::5'MEA+AAA++NO:1.00000'NAD+VN+123456789012::95'NAD+SU+AAA3336647M::95'MOA+38:600.00:AUD'RFF+ABD:85021100'RFF+AED:36'RFF+AGW:GEN'RFF+AWA:001'RFF+AAQ:MAEU5481202'RFF+AFV:TV'UNS+S'UNT+95+1'";
		readonly string iMDWithTwoPackingLinesText = @"UNH+1+CUSDEC:D:99B:UN'BGM+929:::IMD+S3IAKL100001904/1/CMT1:2+4'CST++N10::95'LOC+8+AUSYD::6'LOC+12+AUSYD::6'LOC+9+NZAKL::6'LOC+79+AUSYD::6'DTM+178:20230316:102'DTM+260:20230316:102'DTM+252:20230316:102'GIS+Y:153:95'GIS+POR:109:95'GIS+LLB:109:95'GIS+TLB:109:95'MEA+AAE+G+KG:1000.00000'FTX+DEL+++AMY TEST'FTX+CHG+++YFUJKH'RFF+ABQ:ADGHFDGJN'RFF+ADU:S3IAKL100001904/1'RFF+APH:FOB'RFF+AMG:0011'RFF+ADP:0008::Y'RFF+ADP:0009::Y'RFF+ABT:AAAGC3H7F'TDT+20+00001+S+++++9175793::11'NAD+AT+41065894724::95'NAD+WP+001::95'NAD+VT+AA33HF::95'NAD+DP++MASCOT++UNIT 23 635 GARDENERS ROAD++:::NSW+2020+AU'MOA+63:2000.00:AUD'MOA+141:2044.91:AUD'MOA+39:2000.00:AUD'MOA+313:20.00:USD'MOA+71:10.00:USD'UNS+D'DMS+1'LIN+1+A'PAC+++LCL:67:95'PAC+1+1'PCI+1'RFF+AAQ:MAEU5481202'PCI+1'RFF+MB:OBOL123'PCI+1'RFF+BH:JGHTFTKJGFH'LIN+2+I'PAC+++LCL:67:95'PAC+2+1'PCI+1'RFF+AAQ:MAEU5481202'PCI+1'RFF+MB:OBOL123'PCI+1'RFF+BH:JFGHJKMHJG'CST+1+A::95+N10::95'FTX+AAA+++MOTORS OF AN OUTPUT NOT EXCEEDING 37.5 W'LOC+27+NZ::5'MEA+AAA++NO:1.00000'NAD+VN+123456789012::95'NAD+SU+AAA3336647M::95'MOA+38:1000.00:AUD'RFF+ABD:85011000'RFF+AED:32'RFF+AGW:GEN'RFF+AWA:001'RFF+AAQ:MAEU5481202'RFF+AFV:TV'CST+2+A::95+N10::95'FTX+AAA+++MOTORS OF AN OUTPUT NOT EXCEEDING 37.5 W'LOC+27+NZ::5'MEA+AAA++NO:1.00000'NAD+VN+123456789012::95'NAD+SU+AAA3336647M::95'MOA+38:400.00:AUD'RFF+ABD:85011000'RFF+AED:32'RFF+AGW:GEN'RFF+AWA:001'RFF+AAQ:MAEU5481202'RFF+AFV:TV'CST+3+A::95+N10::95'FTX+AAA+++OF AN OUTPUT NOT EXCEEDING 75 KVA'LOC+27+NZ::5'MEA+AAA++NO:1.00000'NAD+VN+123456789012::95'NAD+SU+AAA3336647M::95'MOA+38:600.00:AUD'RFF+ABD:85021100'RFF+AED:36'RFF+AGW:GEN'RFF+AWA:001'RFF+AAQ:MAEU5481202'RFF+AFV:TV'UNS+S'UNT+95+1'";
		readonly string imdrErrorResponse = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::IMDR+CCF_AAA374M_1_FID_1:1+11'FTX+AHN+++REJECTED:The transaction has been rejected due to errors.  Please correct and re-send the message.'NAD+MR+AAA374M::95'RFF+ABO:B00227128/1/CMT1::1'ERP+::1'ERC+15::95'FTX+AAO+++The mandatory field IMPORTERREF is missing from BODY'CNT+55:1'CNT+5:1'UNT+11+000001'";

		#endregion
	}
}
