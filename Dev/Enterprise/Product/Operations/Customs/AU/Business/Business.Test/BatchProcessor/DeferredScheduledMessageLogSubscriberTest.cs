using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DeferredScheduledMessageLogSubscriber))]
	sealed class DeferredScheduledMessageLogSubscriberTest : LogSubscriberTest<DeferredScheduledMessageLogSubscriber>
	{
		[ExpectNoExceptions]
		[TestDate(2022, 7, 6, 12, 00, 00, 000)]
		public void TestDeferredScheduledMessageLogSubscriberWithNoErrors()
		{
			var mAWB = Factory.NewWithValidTestData<CusMAWB>();
			mAWB.CM_MAWB = "08111111111";
			mAWB.CM_RL_NKLoadPort = "GBLHR";
			mAWB.CM_RL_NKDischargePort = "AUSYD";
			mAWB.CM_ArrivalDate = ZDateTime.Now.AddDays(-2);
			mAWB.CM_ResponsiblePartyID = "87003014042";
			mAWB.CM_FlightNo = "QF123";

			var hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_HAWB = "HBL1";
			hAWB1.CS_ConsigneeName = "CONSIGNEE NAME";
			hAWB1.CS_ConsigneeCity = "CONSIGNEE CITY";
			hAWB1.CS_ConsignorName = "CONSIGNOR NAME";
			hAWB1.CS_ConsignorCity = "CONSIGNOR CITY";
			hAWB1.CS_RL_NKDestination = "AUSYD";
			hAWB1.CS_PiecesManifested = 10;
			hAWB1.CS_GoodsDescription = "DESC";
			hAWB1.CS_Weight = 100m;

			var hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_HAWB = "HBL2";
			hAWB2.CS_ConsigneeName = "CONSIGNEE NAME";
			hAWB2.CS_ConsigneeCity = "CONSIGNEE CITY";
			hAWB2.CS_ConsignorName = "CONSIGNOR NAME";
			hAWB2.CS_ConsignorCity = "CONSIGNOR CITY";
			hAWB2.CS_RL_NKDestination = "AUSYD";
			hAWB2.CS_PiecesManifested = 10;
			hAWB2.CS_GoodsDescription = "DESC";
			hAWB2.CS_Weight = 100m;

			mAWB.AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage);
			Factory.Save();

			RunLogWalkerCycleForTest();
			Assert(NotifiedEventList.Contains("[DSM Log Subscriber] Generating Cargo messages for Job 08111111111"));
			Assert(NotifiedEventList.Contains("[DSM Log Subscriber] 2 messages sucessfully created."));
			hAWB1.Messages.Load();
			AssertEquals("Message should have been generated", 1, hAWB1.Messages.Count);
			AssertEquals(ZDateTime.Empty, hAWB1.Messages[0].EM_HeldUntilDate);
			hAWB2.Messages.Load();
			AssertEquals("Message should have been generated", 1, hAWB2.Messages.Count);
			AssertEquals(ZDateTime.Empty, hAWB2.Messages[0].EM_HeldUntilDate);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "Deferred AIR CARGO Send errors or warnings"));
			AssertNull("Failure email not sent", email);

			hAWB1.Reload();
			hAWB1.Messages.RemoveAndDeleteAllFromTest();
			hAWB2.Reload();
			hAWB2.Messages.RemoveAndDeleteAllFromTest();
			mAWB.Reload();
			mAWB.CM_ArrivalDate = EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("AUSYD", ZDateTime.UtcNow.AddHours(5).ToDateTime());
			mAWB.AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			NotifiedEventList.Clear();
			RunLogWalkerCycleForTest();
			Assert(NotifiedEventList.Contains("[DSM Log Subscriber] Generating Cargo messages for Job 08111111111"));
			Assert(NotifiedEventList.Contains("[DSM Log Subscriber] 2 messages sucessfully created."));
			hAWB1.Messages.Load();
			AssertEquals("Message should have been generated", 1, hAWB1.Messages.Count);
			var hawb1Message = hAWB1.Messages[0];
			AssertNotEquals(ZDateTime.Empty, hawb1Message.EM_HeldUntilDate);
			Assert("Message is saved", hawb1Message.IsInDatabase);
			hAWB2.Messages.Load();
			AssertEquals("Message should have been generated", 1, hAWB2.Messages.Count);
			var hawb2Message = hAWB2.Messages[0];
			AssertNotEquals(ZDateTime.Empty, hawb2Message.EM_HeldUntilDate);
			Assert("Message is saved", hawb2Message.IsInDatabase);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "Deferred AIR CARGO Send errors or warnings"));
			AssertNull("Failure email not sent", email);

			mAWB.Reload();
			mAWB.AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage);
			var underbond1 = mAWB.Underbonds.AddNew();
			var underbond2 = mAWB.Underbonds.AddNew();
			GlbCompany.GetCurrentCompany(Factory).OrgProxy.Addresses[0].LocalControlledPremisesID = "1234D";
			underbond1.C4_OriginPremiseID = "1234D";
			underbond1.C4_DestinationPremiseID = "1234D";
			underbond2.C4_OriginPremiseID = "1234D";
			underbond2.C4_DestinationPremiseID = "1234D";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			NotifiedEventList.Clear();
			RunLogWalkerCycleForTest();
			Assert(NotifiedEventList.Contains("[DSM Log Subscriber] Generating Cargo messages for Job 08111111111"));
			Assert(NotifiedEventList.Contains("[DSM Log Subscriber] 2 messages sucessfully created."));
			underbond1.Messages.Load();
			AssertEquals("Message should have been generated", 1, underbond1.Messages.Count);
			var underbond1Message = underbond1.Messages[0];
			AssertNotEquals(ZDateTime.Empty, underbond1Message.EM_HeldUntilDate);
			Assert("Message is saved", underbond1Message.IsInDatabase);
			underbond2.Messages.Load();
			AssertEquals("Message should have been generated", 1, underbond2.Messages.Count);
			var underbond2Message = underbond2.Messages[0];
			AssertNotEquals(ZDateTime.Empty, underbond2Message.EM_HeldUntilDate);
			Assert("Message is saved", underbond2Message.IsInDatabase);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "Deferred AIR CARGO Send errors or warnings"));
			AssertNull("Failure email not sent", email);

			NotifiedEventList.Clear();
			var outturnHeader = Factory.NewWithValidTestData<CusOutturnHeader>();
			var outturn = outturnHeader.Outturns.AddNew();
			outturn.C5_CargoReceiptDate = ZDateTime.Now;
			outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			outturn.C5_ContainerNumber = "1234";

			var underbond = outturnHeader.Underbonds.AddNew();
			underbond.Outturns.Add(outturn);

			outturnHeader.AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent();
			Factory.Save();

			RunLogWalkerCycleForTest();
			Assert(NotifiedEventList.Contains("[DSM Log Subscriber] Generating Cargo messages for Job " + outturnHeader.C6_SendersMessageReference));
			Assert(NotifiedEventList.Contains("[DSM Log Subscriber] 1 messages sucessfully created."));
			outturnHeader.Messages.Load();
			AssertEquals("Message should have been generated", 1, outturnHeader.Messages.Count);
			AssertEquals(ZDateTime.Empty, outturnHeader.Messages[0].EM_HeldUntilDate);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "AU Cargo Send errors or warnings"));
			AssertNull("Failure email not sent", email);
		}

		[ExpectNoExceptions]
		public void TestDeferredScheduledMessageLogSubscriberWithSomeErrors()
		{
			var mAWB = Factory.NewWithValidTestData<CusMAWB>();
			mAWB.CM_MAWB = "08111111111";
			mAWB.CM_RL_NKLoadPort = "GBLHR";
			mAWB.CM_RL_NKDischargePort = "AUSYD";
			mAWB.CM_ArrivalDate = ZDateTime.Now;
			mAWB.CM_ResponsiblePartyID = "87003014042";
			mAWB.CM_FlightNo = "QF123";

			var hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_HAWB = "HBL1";
			hAWB1.CS_ConsigneeName = "CONSIGNEE NAME";
			hAWB1.CS_ConsigneeCity = "CONSIGNEE CITY";
			hAWB1.CS_ConsignorName = "CONSIGNOR NAME";
			hAWB1.CS_ConsignorCity = "CONSIGNOR CITY";
			hAWB1.CS_RL_NKDestination = "AUSYD";
			hAWB1.CS_PiecesManifested = 10;
			hAWB1.CS_GoodsDescription = "DESC";
			hAWB1.CS_Weight = 100m;

			var hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_HAWB = "HBL2";
			hAWB2.CS_ConsigneeName = "CONSIGNEE NAME";
			hAWB2.CS_ConsigneeCity = "CONSIGNEE CITY";
			hAWB2.CS_ConsignorName = "CONSIGNOR NAME";
			hAWB2.CS_ConsignorCity = "CONSIGNOR CITY";
			hAWB2.CS_RL_NKDestination = "AUSYD";
			hAWB2.CS_PiecesManifested = 10;
			hAWB2.CS_GoodsDescription = "DESC";

			mAWB.AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage);
			Factory.Save();

			RunLogWalkerCycleForTest();
			Assert(NotifiedEventList.Contains("[DSM Log Subscriber] Generating Cargo messages for Job 08111111111"));
			Assert(NotifiedEventList.Contains(@"[DSM Log Subscriber] 1 messages sucessfully created, but errors or warnings occured on others:
 Mawb 08111111111, Hawb HBL1 created and message prepared for sending to customs.
Error: Mawb 08111111111, Hawb HBL2:
Weight: Weight is required for air cargo messaging.
Messages successfully sent.
") || NotifiedEventList.Contains(@"[DSM Log Subscriber] 1 messages sucessfully created, but errors or warnings occured on others:
 Error: Mawb 08111111111, Hawb HBL2:
Weight: Weight is required for air cargo messaging.
House Bill (Mawb 08111111111, Hawb HBL1) created and message prepared for sending to customs.
Messages successfully sent.
"));

			hAWB1.Messages.Load();
			AssertEquals("Message should have been generated", 1, hAWB1.Messages.Count);
			Assert("Message is saved", hAWB1.Messages[0].IsInDatabase);

			hAWB2.Messages.Load();
			AssertEquals("No message should have been generated", 0, hAWB2.Messages.Count);
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "AU Cargo Send errors or warnings"));
			AssertNotNull("Failure email sent", email);

			NotifiedEventList.Clear();
			var outturnHeader = Factory.NewWithValidTestData<CusOutturnHeader>();
			var outturn = outturnHeader.Outturns.AddNew();
			outturn.C5_CargoReceiptDate = ZDateTime.Now;
			outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			outturnHeader.AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent();
			Factory.Save();

			RunLogWalkerCycleForTest();
			Assert(NotifiedEventList.Contains("[DSM Log Subscriber] Generating Cargo messages for Job O00000001"));
			Assert(NotifiedEventList.Contains(@"[DSM Log Subscriber] 0 messages sucessfully created, but errors or warnings occured on others:
 Error: O00000001:
Container Number: Container Number is required if not Bulk or Break Bulk.
"));
			outturnHeader.Messages.Load();
			AssertEquals("No message should have been generated", 0, outturnHeader.Messages.Count);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "AU Cargo Send errors or warnings"));
			AssertNotNull("Failure email not sent", email);
		}

		[ExpectNoExceptions]
		public void TestDeferredScheduledMessageLogSubscriberWithNoMessagesToSend()
		{
			var mAWB = Factory.NewWithValidTestData<CusMAWB>();
			mAWB.CM_MAWB = "08111111111";
			mAWB.CM_RL_NKLoadPort = "GBLHR";
			mAWB.CM_RL_NKDischargePort = "AUSYD";
			mAWB.CM_ArrivalDate = ZDateTime.Now;
			mAWB.CM_ResponsiblePartyID = "87003014042";
			mAWB.CM_FlightNo = "QF123";

			var hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_HAWB = "HBL1";
			hAWB1.CS_ConsigneeName = "CONSIGNEE NAME";
			hAWB1.CS_ConsigneeCity = "CONSIGNEE CITY";
			hAWB1.CS_ConsignorName = "CONSIGNOR NAME";
			hAWB1.CS_ConsignorCity = "CONSIGNOR CITY";
			hAWB1.CS_RL_NKDestination = "AUSYD";
			hAWB1.CS_PiecesManifested = 10;
			hAWB1.CS_GoodsDescription = "DESC";
			hAWB1.CS_Weight = 100m;
			hAWB1.CS_MsgStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;

			var hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_HAWB = "HBL2";
			hAWB2.CS_ConsigneeName = "CONSIGNEE NAME";
			hAWB2.CS_ConsigneeCity = "CONSIGNEE CITY";
			hAWB2.CS_ConsignorName = "CONSIGNOR NAME";
			hAWB2.CS_ConsignorCity = "CONSIGNOR CITY";
			hAWB2.CS_RL_NKDestination = "AUSYD";
			hAWB2.CS_PiecesManifested = 10;
			hAWB2.CS_GoodsDescription = "DESC";
			hAWB2.CS_Weight = 100m;
			hAWB2.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalAccepted;

			mAWB.AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage);
			Factory.Save();

			RunLogWalkerCycleForTest();
			Assert(NotifiedEventList.Contains("[DSM Log Subscriber] Generating Cargo messages for Job 08111111111"));
			Assert(NotifiedEventList.Contains("[DSM Log Subscriber] No messages were created."));
			hAWB1.Messages.Load();
			AssertEquals("No message should have been generated", 0, hAWB1.Messages.Count);
			hAWB2.Messages.Load();
			AssertEquals("No message should have been generated", 0, hAWB2.Messages.Count);
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "Deferred AIR CARGO Send errors or warnings"));
			AssertNull("Failure email not sent", email);

			NotifiedEventList.Clear();
			var outturnHeader = Factory.NewWithValidTestData<CusOutturnHeader>();
			outturnHeader.C6_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			var outturn = outturnHeader.Outturns.AddNew();
			outturn.C5_CargoReceiptDate = ZDateTime.Now;
			outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			outturn.C5_ContainerNumber = "1234";
			outturnHeader.AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent();
			Factory.Save();

			RunLogWalkerCycleForTest();
			Assert(NotifiedEventList.Contains("[DSM Log Subscriber] Generating Cargo messages for Job O00000001"));
			Assert(NotifiedEventList.Contains("[DSM Log Subscriber] No messages were created."));
			outturnHeader.Messages.Load();
			AssertEquals("No message should have been generated", 0, outturnHeader.Messages.Count);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "AU Cargo Send errors or warnings"));
			AssertNull("Failure email not sent", email);
		}

		[ExpectNoExceptions]
		public void TestDeferredScheduledMessageLogSubscriberWithMutex()
		{
			var mAWB = Factory.NewWithValidTestData<CusMAWB>();
			mAWB.CM_MAWB = "08111111111";
			mAWB.CM_RL_NKLoadPort = "GBLHR";
			mAWB.CM_RL_NKDischargePort = "AUSYD";
			mAWB.CM_ArrivalDate = ZDateTime.Now;
			mAWB.CM_ResponsiblePartyID = "87003014042";

			var hAWB1 = mAWB.ChildBills.AddNew();
			hAWB1.CS_HAWB = "HBL1";
			hAWB1.CS_ConsigneeName = "CONSIGNEE NAME";
			hAWB1.CS_ConsigneeCity = "CONSIGNEE CITY";
			hAWB1.CS_ConsignorName = "CONSIGNOR NAME";
			hAWB1.CS_ConsignorCity = "CONSIGNOR CITY";
			hAWB1.CS_RL_NKDestination = "AUSYD";
			hAWB1.CS_PiecesManifested = 10;
			hAWB1.CS_GoodsDescription = "DESC";
			hAWB1.CS_Weight = 100m;

			var hAWB2 = mAWB.ChildBills.AddNew();
			hAWB2.CS_HAWB = "HBL2";
			hAWB2.CS_ConsigneeName = "CONSIGNEE NAME";
			hAWB2.CS_ConsigneeCity = "CONSIGNEE CITY";
			hAWB2.CS_ConsignorName = "CONSIGNOR NAME";
			hAWB2.CS_ConsignorCity = "CONSIGNOR CITY";
			hAWB2.CS_RL_NKDestination = "AUSYD";
			hAWB2.CS_PiecesManifested = 10;
			hAWB2.CS_GoodsDescription = "DESC";
			hAWB2.CS_Weight = 100m;

			mAWB.AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var mAWBInNewFactory = Factory.Load<CusMAWB>(mAWB.PK);
			mAWBInNewFactory.SendAIRCRMutex.Lock();
			try
			{
				RunLogWalkerCycleForTest();
				Assert(!NotifiedEventList.Contains("[DSM Log Subscriber] Generating Cargo messages for Job 08111111111"));
				hAWB1.Messages.Load();
				AssertEquals("No message should have been generated", 0, hAWB1.Messages.Count);
				hAWB2.Messages.Load();
				AssertEquals("No message should have been generated", 0, hAWB2.Messages.Count);
				EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "AU CARGO Message not sent"));
				AssertNotNull("Failure email sent", email);
			}
			finally
			{
				mAWBInNewFactory.SendAIRCRMutex.Unlock();
			}
		}

		[TestDate(2022, 7, 6, 12, 00, 00, 000)]
		public void TestDeferredScheduledMessageLogSubscriberWithNoErrorsForSCR()
		{
			using (AUCustomsDataRegistry.Instance.SeaMandatoryLatestCargoReportingTimeframe.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 48))
			{
				var oceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
				oceanBill.CB_OceanBill = "OB004288";
				oceanBill.CB_RL_NKPortOfLoading = "GBLHR";
				oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
				oceanBill.CB_DateOfArrival = EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("AUSYD", ZDateTime.UtcNow.AddHours(47).ToDateTime());
				oceanBill.CB_ResponsiblePartyID = "87003014042";
				var container1 = oceanBill.Containers.AddNew();
				container1.CN_ContainerNumber = "MSKU0454586";
				container1.CN_ContainerMode = "LCL";
				container1.CN_RC_NKContainerType = "20GP";

				var hb1 = oceanBill.HouseBills.AddNew();
				hb1.CA_HouseBill = "HBL1";
				hb1.CA_ConsigneeName = "TRITEC PTY LTD";
				hb1.CA_ConsigneeAddress1 = "CO NEW CENTURY PACKING";
				hb1.CA_ConsigneeAddress2 = "UNIT 5 370 NUDGEE ROAD";
				hb1.CA_ConsigneeSuburb = "HENDRA QLD AUSTRALIA";
				hb1.CA_RN_NKConsigneeCountryCode = "AU";
				hb1.CA_ConsigneePostcode = "4011";
				hb1.CA_ConsignorName = "HUHTAMAKI VAN LEER NZ LTD";
				hb1.CA_ConsignorAddress1 = "FLEXIBLE PACKAGING DIVISION";
				hb1.CA_ConsignorAddress2 = "BAG 93 002";
				hb1.CA_ConsignorSuburb = "NEW LYNN AUCKLAND";
				hb1.CA_RN_NKConsignorCountryCode = "NZ";
				hb1.CA_PrepaidCollectOther = CMRMethodsOfPayment.Codes.PrepaidOnly;
				hb1.CA_RL_NK_PortOfDestination = "AUSYD";
				hb1.CA_GoodsDescription = "DESC";
				var hb1Packages = hb1.Pivot.AddNew();
				hb1Packages.CV_AssociatedContainer = "MSKU0454586";
				hb1Packages.CV_PackageCount = 10;
				hb1Packages.CV_PackageType = "BX";
				hb1Packages.CV_NetWeight = 200;
				hb1Packages.CV_Volume = 2.3m;
				hb1Packages.CV_GoodsDescription = "BOOKS";
				hb1Packages.CV_MarksAndNumbers = "NM";

				var hb2 = oceanBill.HouseBills.AddNew();
				hb2.CA_HouseBill = "HBL2";
				hb2.CA_ConsigneeName = "TRITEC PTY LTD";
				hb2.CA_ConsigneeAddress1 = "CO NEW CENTURY PACKING";
				hb2.CA_ConsigneeAddress2 = "UNIT 5 370 NUDGEE ROAD";
				hb2.CA_ConsigneeSuburb = "HENDRA QLD AUSTRALIA";
				hb2.CA_RN_NKConsigneeCountryCode = "AU";
				hb2.CA_ConsigneePostcode = "4011";
				hb2.CA_ConsignorName = "HUHTAMAKI VAN LEER NZ LTD";
				hb2.CA_ConsignorAddress1 = "FLEXIBLE PACKAGING DIVISION";
				hb2.CA_ConsignorAddress2 = "BAG 93 002";
				hb2.CA_ConsignorSuburb = "NEW LYNN AUCKLAND";
				hb2.CA_RN_NKConsignorCountryCode = "NZ";
				hb2.CA_PrepaidCollectOther = CMRMethodsOfPayment.Codes.PrepaidOnly;
				hb2.CA_RL_NK_PortOfDestination = "AUSYD";
				hb2.CA_GoodsDescription = "DESC";
				var hb2Packages = hb2.Pivot.AddNew();
				hb2Packages.CV_AssociatedContainer = "MSKU0454586";
				hb2Packages.CV_PackageCount = 20;
				hb2Packages.CV_PackageType = "CT";
				hb2Packages.CV_NetWeight = 80;
				hb2Packages.CV_Volume = 1m;
				hb2Packages.CV_GoodsDescription = "MAGAZINES";
				hb2Packages.CV_MarksAndNumbers = "No Marks";

				oceanBill.AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent();
				Factory.Save();

				RunLogWalkerCycleForTest();
				Assert(NotifiedEventList.Contains("[DSM Log Subscriber] Generating Cargo messages for Job OB004288"));
				Assert(NotifiedEventList.Contains("[DSM Log Subscriber] 2 messages sucessfully created."));
				hb1.Messages.Load();
				AssertEquals("Message should have been generated", 1, hb1.Messages.Count);
				AssertEquals(ZDateTime.Empty, hb1.Messages[0].EM_HeldUntilDate);
				hb2.Messages.Load();
				AssertEquals("Message should have been generated", 1, hb2.Messages.Count);
				AssertEquals(ZDateTime.Empty, hb2.Messages[0].EM_HeldUntilDate);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "Deferred SEA CARGO Send errors or warnings"));
				AssertNull("Failure email not sent", email);

				hb1.Reload();
				hb1.Messages.RemoveAndDeleteAllFromTest();
				hb2.Reload();
				hb2.Messages.RemoveAndDeleteAllFromTest();
				oceanBill.Reload();
				oceanBill.CB_DateOfArrival = EnvProxy.Instance.Time.GetUnlocoTimeFromUtc("AUSYD", ZDateTime.UtcNow.AddHours(49).ToDateTime());
				oceanBill.AddDeferredScheduledMessagesEventAndConsolCargoMessagingEvent();
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				NotifiedEventList.Clear();
				RunLogWalkerCycleForTest();
				Assert(NotifiedEventList.Contains("[DSM Log Subscriber] Generating Cargo messages for Job OB004288"));
				Assert(NotifiedEventList.Contains("[DSM Log Subscriber] 2 messages sucessfully created."));

				long oneHundredSeconds = 10000000000;
				var expectedHeldUntilDate = ZDateTime.UtcNow.Ticks / oneHundredSeconds;

				hb1.Messages.Load();
				AssertEquals("Message should have been generated", 1, hb1.Messages.Count);
				var hawb1Message = hb1.Messages[0];
				Assert("Message is saved", hawb1Message.IsInDatabase);
				AssertEquals("HeldUntilDate is Now with seconds truncated", expectedHeldUntilDate, hawb1Message.EM_HeldUntilDate.Ticks / oneHundredSeconds);
				hb2.Messages.Load();
				AssertEquals("Message should have been generated", 1, hb2.Messages.Count);
				var hawb2Message = hb2.Messages[0];
				Assert("Message is saved", hawb2Message.IsInDatabase);
				AssertEquals("HeldUntilDate is Now with seconds truncated", expectedHeldUntilDate, hawb2Message.EM_HeldUntilDate.Ticks / oneHundredSeconds);
				email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "Deferred SEA CARGO Send errors or warnings"));
				AssertNull("Failure email not sent", email);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbGroup postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			if (postMasters.Staff.Count == 0)
			{
				postMasters.Staff.AddNew();
			}

			postMasters.Staff[0].GS_EmailAddress = "blah@blah.com";
			Factory.Save();
		}
	}
}
