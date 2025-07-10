using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusUnderbondOutturnStatusCalculator))]
	sealed class CusUnderbondOutturnStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStatusInfo()
		{
			AssertEquals("StatusInfo.Name", "Code", Calculator.StatusInfo.Name);
		}

		public void TestInterestedMessageTypes()
		{
			var message1 = Underbond.Messages.AddNew(typeof(CMRAIROUTMessage));
			message1.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			AssertEquals("InterestedMessageTypes.Length", 2, Calculator.InterestedMessageTypes.Length);
			AssertEquals("InterestedMessageTypes[0]", CMRMessage.CMRMessageTypes.AIROUT, Calculator.InterestedMessageTypes[0]);
			AssertEquals("InterestedMessageTypes[1]", CMRMessage.CMRMessageTypes.SEAOUT, Calculator.InterestedMessageTypes[1]);
		}

		public void TestInterestedInPendingMessages()
		{
			AssertEquals("Having split messages requires us to ensure we have access to pending messages as well", true, Calculator.InterestedInPendingMessages);
		}

		public void TestResponseStatusUpdatesOutturnLastMessageDate()
		{
			var outturn1 = Underbond.Outturns.AddNew();
			var outturn2 = Underbond.Outturns.AddNew();
			var outturn3 = Underbond.Outturns.AddNew();

			AssertEquals("Pre-condition expected", ZDateTime.Empty, outturn1.C5_LastMessageDate);
			AssertEquals("Pre-condition expected", ZDateTime.Empty, outturn2.C5_LastMessageDate);
			AssertEquals("Pre-condition expected", ZDateTime.Empty, outturn3.C5_LastMessageDate);

			var outgoingMessage = Underbond.Messages.AddNew(typeof(CMRAIROUTMessage));
			AssertEquals("Pre-condition expected", "AUT", outgoingMessage.EM_MessageType);
			outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = "SNT";
			outgoingMessage.EM_MessageText = "UNH+1+CUSCAR:D:99B:UN'BGM+263:::AIROUT+U00001799/CMT1:1+9'DTM+570:20120503:102'DTM+570:0812:401'NAD+VW+41065894724::95'TDT+20+442++6+QF::3'LOC+59+9914N::95'DTM+132:20120503:102'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:08102998329'GID+1'RFF+HWB:HB1'GID+1'PAC+10'FTX+AAA+++LINE 1'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:08102998329'GID+1'RFF+HWB:HB2'GID+1'PAC+20'FTX+AAA+++LINE 2'CNI++:::I'RFF+ACU:NIL'GID+1'RFF+MWB:08102998329'GID+1'RFF+HWB:HB3'GID+1'PAC+30'FTX+AAA+++LINE 3'UNT+36+1'";

			var responseMessage = Underbond.Messages.AddNew(typeof(CMRAIROUTMessage));
			AssertEquals("Pre-condition expected", "AUT", responseMessage.EM_MessageType);
			responseMessage.EM_MessageSubType = "CLR";
			responseMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_Status = "RCV";
			responseMessage.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIROUTR+4DFA JFI9 I0AC:001+11'NAD+MR+AAA374M::95'RFF+ACW:AIROUT'RFF+AFM:9'RFF+ABO:U00001799/CMT1::001'DTM+310:20120503081925:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5203:6:95'FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'CNT+55:000'UNT+13+000001'";

			outturn3.C5_MessageStatus = CMRUnderbondStatuses.Codes.OutturnAcceptedThisOutturnLineRejected;

			Calculator.DeriveStatus();
			AssertEquals("Outturn should have been updated", ZDateTime.Today, outturn1.C5_LastMessageDate);
			AssertEquals("Outturn should have been updated", ZDateTime.Today, outturn2.C5_LastMessageDate);
			AssertEquals("Outturn3 should not have been updated", ZDateTime.Empty, outturn3.C5_LastMessageDate);

			var withdrawalMessage = Underbond.Messages.AddNew(typeof(CMRAIROUTMessage));
			withdrawalMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;
			withdrawalMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			withdrawalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			withdrawalMessage.EM_Status = "SNT";
			withdrawalMessage.EM_MessageText = "UNH+1+CUSCAR:D:99B:UN'BGM+263:::AIROUT+U00001799/CMT1:12+50'DTM+570:20120503:102'DTM+570:0812:401'NAD+VW+41065894724::95'TDT+20+442++6+QF::3'LOC+59+9914N::95'DTM+132:20120503:102'UNT+9+1'";

			var responseMessage2 = Underbond.Messages.AddNew(typeof(CMRAIROUTMessage));
			AssertEquals("Pre-condition expected", "AUT", responseMessage2.EM_MessageType);
			responseMessage2.EM_MessageSubType = "CLR";
			responseMessage2.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			responseMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage2.EM_Status = "RCV";
			responseMessage2.EM_MessageText = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIROUTR+3J42 56JG B30C:001+11'NAD+MR+AAA374M::95'RFF+ACW:AIROUT'RFF+AFM:50'RFF+ABO:U00001799/CMT1::012'DTM+310:20120508044732:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5202:6:95'FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITH ERRORS AND/OR WARNINGS'ERP+1'ERC+ADVICE:80:95'ERC+CG2252:6:95'FTX+AAO+++AWO HAS BEEN SUCCESSFULLY WITHDRAWN'CNT+55:000'UNT+17+000001'";

			outturn3.C5_LastMessageDate = ZDateTime.Today.AddDays(-3);
			Calculator.DeriveStatus();
			AssertEquals("Outturn last message date should have been reset when withdrawn", ZDateTime.Empty, outturn1.C5_LastMessageDate);
			AssertEquals("Outturn last message date should have been reset when withdrawn", ZDateTime.Empty, outturn2.C5_LastMessageDate);
			AssertEquals("Outturn last message date should have been reset when withdrawn", ZDateTime.Empty, outturn3.C5_LastMessageDate);
		}

		public void TestClearAllOutturnLogs()
		{
			var message1 = Underbond.Messages.AddNew(typeof(CMRAIROUTMessage));
			message1.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var splitMessage2 = Underbond.Messages.AddNew(typeof(CMRAIROUTMessage));
			splitMessage2.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;
			var splitMessage3 = Underbond.Messages.AddNew(typeof(CMRAIROUTMessage));
			splitMessage3.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;

			message1.EM_Status = EDIMessage.Status.Pending;
			splitMessage2.EM_Status = EDIMessage.Status.Pending;
			splitMessage3.EM_Status = EDIMessage.Status.Pending;

			Underbond.CusUnderbondOutturnLogManager.AddANewSplitMessageFailedLog("Split Message Failure");
			AssertEquals("HasSplitMessageFailedLog", true, Underbond.CusUnderbondOutturnLogManager.HasSplitMessageFailedLog);

			var message4 = Underbond.Messages.AddNew(typeof(CMRAIROUTMessage));
			message4.EM_MessageSubType = CMRMessage.MessageSubTypes.Withdraw;
			Underbond.OutturnStatusCalculator.DeriveStatusNow();
			AssertEquals("HasSplitMessageFailedLog should be cleared now", false, Underbond.CusUnderbondOutturnLogManager.HasSplitMessageFailedLog);
		}

		public void TestOutturnLogs()
		{
			var message1 = Underbond.Messages.AddNew(typeof(CMRAIROUTMessage));
			message1.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var splitMessage2 = Underbond.Messages.AddNew(typeof(CMRAIROUTMessage));
			splitMessage2.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;
			var splitMessage3 = Underbond.Messages.AddNew(typeof(CMRAIROUTMessage));
			splitMessage3.EM_MessageSubType = CMRMessage.MessageSubTypes.Change;

			message1.EM_Status = EDIMessage.Status.Pending;
			splitMessage2.EM_Status = EDIMessage.Status.Pending;
			splitMessage3.EM_Status = EDIMessage.Status.Pending;

			Underbond.CusUnderbondOutturnLogManager.AddANewSplitMessageOriginalRejectedLog("Split Message Original Rejection");
			AssertEquals("HasSplitMessageOriginalRejectedLog", true, Underbond.CusUnderbondOutturnLogManager.HasSplitMessageOriginalRejectedLog);

			var message4 = Underbond.Messages.AddNew(typeof(CMRAIROUTMessage));
			message4.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			Underbond.OutturnStatusCalculator.DeriveStatusNow();
			AssertEquals("Message sent again, HasSplitMessageOriginalRejectedLog should be cleared now", false, Underbond.CusUnderbondOutturnLogManager.HasSplitMessageOriginalRejectedLog);
		}

		public void TestHasNonExistantLineAtCustoms()
		{
			AssertEquals("Has no outturn HasNonExistantLineAtCustoms", false, Underbond.CusUnderbondOutturnLogManager.HasNonExistantLineAtCustoms);
			Underbond.CusUnderbondOutturnLogManager.AddANewHasNonExistantLineAtCustomsLog("Partial Amendment Received");
			AssertEquals("Has HasNonExistantLineAtCustoms", true, Underbond.CusUnderbondOutturnLogManager.HasNonExistantLineAtCustoms);
		}

		protected override BusinessObject GetNewBusinessObject() => new CusUnderbondOutturnStatusCalculatorForTest(Underbond);

		CusUnderbondOutturnStatusCalculatorForTest calculator;
		CusUnderbondOutturnStatusCalculatorForTest Calculator => calculator ?? (calculator = (CusUnderbondOutturnStatusCalculatorForTest)GetNewBusinessObject());

		CusUnderbond underbond;
		CusUnderbond Underbond => underbond ?? (underbond = CusUnderbond.New(Factory));

		sealed class CusUnderbondOutturnStatusCalculatorForTest : CusUnderbondOutturnStatusCalculator
		{
			public CusUnderbondOutturnStatusCalculatorForTest(CusUnderbond underbond) : base(underbond)
			{
			}

			internal new void DeriveStatus() => base.DeriveStatus();
		}
	}
}
