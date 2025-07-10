using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class MAWBToConsolBridgeTest : TestCaseWithFactory
	{
		public void TestUpdateLoadAndDischarge()
		{
			var bridge = new MAWBToConsolBridge(mAWB);
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.SetAirCargoSynchroniserRetriever(() => bridge);
			var transport1 = consol.Transports.AddNew();
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = "ITSPE";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "AUBNE";
			var transport3 = consol.Transports.AddNew();
			transport3.JW_LegOrder = 3;
			transport3.JW_RL_NKLoadPort = "AUBNE";
			transport3.JW_RL_NKDiscPort = "AUSYD";

			AssertEquals("SGSIN", mAWB.CM_RL_NKLoadPort);
			AssertEquals("AUBNE", mAWB.CM_RL_NKDischargePort);
		}

		public void TestFieldsAreSynchedByDefault()
		{
			MAWBToConsolBridge bridge = new MAWBToConsolBridge(mAWB);

			string consolMAWB = "08122222222";
			string consolFlightNumber = "QF321";
			ZDateTime consolDate = new ZDateTime(2005, 11, 25);
			string consolDischargePort = "AUBNE";
			string consolLoadPort = "SGSIN";

			bridge.MAWBNumber = consolMAWB;
			bridge.FlightNumber = consolFlightNumber;
			bridge.ArrivalDate = consolDate;
			bridge.DischargePort = consolDischargePort;
			bridge.LoadPort = consolLoadPort;

			AssertEquals(consolMAWB, mAWB.CM_MAWB);
			AssertEquals(consolFlightNumber, mAWB.CM_FlightNo);
			AssertEquals(consolDate, mAWB.CM_ArrivalDate);
			AssertEquals(consolDischargePort, mAWB.CM_RL_NKDischargePort);
			AssertEquals(consolLoadPort, mAWB.CM_RL_NKLoadPort);
		}

		public void TestFieldsAreNotSynchedWhenMessageSent()
		{
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			MAWBToConsolBridge bridge = new MAWBToConsolBridge(mAWB);

			string consolMAWB = "08122222222";
			string consolFlightNumber = "QF321";
			ZDateTime consolDate = new ZDateTime(2005, 11, 25);
			string consolDischargePort = "AUBNE";
			string consolLoadPort = "SGSIN";

			bridge.MAWBNumber = consolMAWB;
			bridge.FlightNumber = consolFlightNumber;
			bridge.ArrivalDate = consolDate;
			bridge.DischargePort = consolDischargePort;
			bridge.LoadPort = consolLoadPort;

			AssertEquals("08111111111", mAWB.CM_MAWB);
			AssertEquals("QF123", mAWB.CM_FlightNo);
			AssertEquals(new ZDateTime(2005, 11, 12), mAWB.CM_ArrivalDate);
			AssertEquals("AUSYD", mAWB.CM_RL_NKDischargePort);
			AssertEquals("USLAX", mAWB.CM_RL_NKLoadPort);
		}

		public void TestNullNotSupported()
		{
			bool correctExceptionThrown = false;
			try
			{
				MAWBToConsolBridge bridge = new MAWBToConsolBridge(null);
			}
			catch (ArgumentNullException)
			{
				correctExceptionThrown = true;
			}
			Assert("ArgumentNullException should have been thrown because MAWB cannot be null", correctExceptionThrown);
		}

		public void TestShouldWeSynchronise_ShouldUseFetchForMessages()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "MB1";
			mawb.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			for (var i = 1; i < 7; i++)
			{
				var hawb = mawb.ChildBills.AddNew();
			}
			Factory.Save();

			// need to create the messages in a separate factory to stop the CusHAWB from getting a message status
			var newFactory = new BusinessObjectFactory();
			foreach (CusHAWB hawb in mawb.ChildBills)
			{
				var message = newFactory.New<EDIMessage>();
				message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_MessageType = Declaration.Business.CMRMessage.CMRMessageTypes.AIRCR;
				message.EM_LinkUniqueID = hawb.PK;
				message.EM_LinkTable = hawb.TableName;
			}
			newFactory.Save();
			foreach (CusHAWB hawb in mawb.ChildBills)
			{
				hawb.Reload();
				AssertEquals("PreCondition: No message status", ZString.Empty, hawb.CS_MsgStatus);
			}
			var newFactory1 = new BusinessObjectFactory();
			var mawbInNewFactory1 = newFactory1.Load<CusMAWB>(mawb.PK);
			var bridgeInNewFactory1 = new MAWBToConsolBridge(mawbInNewFactory1);
			bridgeInNewFactory1.IsAir = false;
			AssertEquals(false, bridgeInNewFactory1.ShouldWeSynchronise);
			AssertEquals("No need to run fetch hint when it's not air", 0, newFactory1.GetTableHitCount(EDIMessage.Schema.TableName));

			bridgeInNewFactory1.IsAir = true;
			AssertEquals(true, bridgeInNewFactory1.ShouldWeSynchronise);
			AssertEquals("Fetch Hint was called", 1, newFactory1.GetTableHitCount(EDIMessage.Schema.TableName));
			foreach (CusHAWB hawb in mawbInNewFactory1.ChildBills)
			{
				AssertEquals(1, hawb.Messages.Count);
			}
			AssertEquals("Should only be one from fetch hint", 1, newFactory1.GetTableHitCount(EDIMessage.Schema.TableName));
		}

		CusMAWB mAWB;
		protected override void SetUp()
		{
			base.SetUp();
			mAWB = Factory.New<CusMAWB>();
			mAWB.CM_MAWB = "08111111111";
			mAWB.CM_FlightNo = "QF123";
			mAWB.CM_ArrivalDate = new ZDateTime(2005, 11, 12);
			mAWB.CM_RL_NKDischargePort = "AUSYD";
			mAWB.CM_RL_NKLoadPort = "USLAX";
		}
	}
}
