using System;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestDate]
	sealed class AUCInterchangeSenderTest : Customs.Business.Testing.DeclarationsAndShipmentsCreatedCancelledTestCase
	{
		public void TestExecuteCannotTakeLongerThanSpecified()
		{
			BatchAUCInterchangeSenderTimed timedSender = new BatchAUCInterchangeSenderTimed(certConfig);
			timedSender.ExecuteBatch();
			AssertEquals(1, timedSender.SendOutboundCMRInterchangeCallCount);
		}

		public void TestSendAcknowledgementMessage()
		{
			EDIMessage message = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, "CTL", false);

			batchProcessor.ExecuteBatch();
			AssertNotNull("InterchangeReadyEventOccurred", message.Logs.MostRecentLogByEventTime(Events.InterchangeReady));
			AssertNotNull("InterchangeInProgressEventOccurred", message.Logs.MostRecentLogByEventTime(Events.InterchangeInProgress));
		}

		public void TestResendAfterTimePeriod()
		{
			var message = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, false);
			batchProcessor.ExecuteBatch();

			var interchangeFilter = new ZQuery();
			interchangeFilter.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.CMR);
			interchangeFilter.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, "TRX");
			interchangeFilter.AddToFilter(EDIInterchangeSchema.EI_To, CMRInterchangeProvider.CMRRecipientID);
			interchangeFilter.AddToFilter(EDIInterchangeSchema.EI_From, CMRInterchangeProvider.GetCustomsRegNumber(message));

			var interchange = Factory.LoadTop1<EDIInterchange>(interchangeFilter);
			Assert("InterchangeCreated", interchange != null);
			AssertEquals("Interchange Status", EDIInterchange.Status.eHubQueued, interchange.EI_Status);

			batchProcessor.ExecuteBatch();

			//Manipulate to re-queue
			interchange.EI_Status = EDIInterchange.Status.SendPending;
			Factory.Save();
			batchProcessor.ExecuteBatch();

			AssertContains("Interchange #1 being re-sent for 2nd time via eHub.", batchProcessor.Logger.DebugLogStrings[0]);
		}

		public void TestDontResendAfterReachMax()
		{
			Env.Registry.InterchangeMaxSends = 1;
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var message = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, false);

			batchProcessor.ExecuteBatch();
			AssertContains("Interchange #1 being sent for first time", batchProcessor.Logger.DebugLogStrings[1]);

			ZQuery interchangeFilter = new ZQuery();
			interchangeFilter.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, EDIInterchange.ApplicationCodes.CMR);
			interchangeFilter.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, "TRX");
			interchangeFilter.AddToFilter(EDIInterchangeSchema.EI_To, CMRInterchangeProvider.CMRRecipientID);
			interchangeFilter.AddToFilter(EDIInterchangeSchema.EI_From, CMRInterchangeProvider.GetCustomsRegNumber(message));
			var interchange = Factory.LoadTop1<EDIInterchange>(interchangeFilter);

			Assert("InterchangeCreated", interchange != null);
			AssertEquals("Interchange Status", EDIInterchange.Status.eHubQueued, interchange.EI_Status);

			//Manipulate to re-queue
			interchange.EI_Status = EDIInterchange.Status.SendPending;
			Factory.Save();

			batchProcessor.ExecuteBatch();
			AssertContains("Interchange #1, sent via eHub, has exceeded its maximum resend count.", batchProcessor.Logger.DebugLogStrings[0]);

			interchange.Reload();
			AssertEquals(EDIMessage.Status.Failed, interchange.EI_Status);
			message.Reload();
			AssertEquals(EDIMessage.Status.Failed, message.EM_Status);
		}

		public void TestInterchangeStatusUpdatedWhenFailed()
		{
			Env.Registry.InterchangeMaxSends = 1;
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var message = Factory.New<CMRIMDMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			Factory.Save();

			var interchange = Factory.New<CMRInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_To = CMRInterchangeProvider.CMRRecipientID;
			interchange.EI_From = CMRInterchangeProvider.GetCustomsRegNumber(message);
			interchange.EI_InterchangeNum = "1";
			interchange.ContainedMessages.Add(message);

			//Manipulate to re-queue
			interchange.EI_Status = EDIInterchange.Status.SendPending;
			interchange.EI_RetryCount = 1;

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(-1);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			batchProcessor.ExecuteBatch();
			AssertContains("Interchange #1, sent via eHub, has exceeded its maximum resend count.", batchProcessor.Logger.DebugLogStrings[0]);

			interchange.Reload();
			AssertEquals(EDIMessage.Status.Failed, interchange.EI_Status);
			var interchangeLogs = interchange.Logs;
			AssertContains("Interchange StatusUpdated log entry", "|NEW=FAL|OLD=PND", interchangeLogs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.StatusUpdated).First().SL_Reference);
			AssertEquals("Interchange has InterchangeFailedToBeSent log entry", "Max Resends exceeded", interchangeLogs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.InterchangeFailedToBeSent).First().SL_Reference);

			message.Reload();
			AssertEquals(EDIMessage.Status.Failed, message.EM_Status);
			var messageLogs = message.Logs;
			AssertContains("Message StatusUpdated log entry", "|NEW=FAL|OLD=SNT", messageLogs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.StatusUpdated).First().SL_Reference);
			AssertNotNull("Message has InterchangeFailedToBeSent log entry", messageLogs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.InterchangeFailedToBeSent).FirstOrDefault());
		}

		public void TestSendOneStopMessage()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			InsertMessageIntoDatabase(EDIMessage.ApplicationCodes.OneStop, false);
			batchProcessor.ExecuteBatch();
			AssertEquals(2, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		public void TestSendExdocMessage()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			InsertMessageIntoDatabase(EDIMessage.ApplicationCodes.EXDOC, false);
			batchProcessor.ExecuteBatch();
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		public void TestDontSendMessageFromADifferentCompany()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			EDIMessage message = InsertMessageIntoDatabase(EDIMessage.ApplicationCodes.OneStop, true);
			batchProcessor.ExecuteBatch();
			message.Reload();
			AssertEquals("EM_EI", ZGuid.Empty, message.EM_EI);
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		public void TestSendOneStopInterchange()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			InsertInterchangeIntoDatabase(EDIInterchange.ApplicationCodes.OneStop, "CSXTEST", false);
			batchProcessor.ExecuteBatch();
			AssertEquals(2, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		public void TestSendCMRInterchange()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			var interchange = InsertInterchangeIntoDatabase(EDIInterchange.ApplicationCodes.CMR, "TO", false, status: EDIInterchange.Status.SendPending);
			batchProcessor.ExecuteBatch();
			interchange.Reload();
			AssertEquals(EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
		}

		public void TestSendingBackgroundCargoMessagesSendsAirWithSeaInCorrectOrder()
		{
			AUCustomsDataRegistry.Instance.BackgroundCargoReportMaxMsg.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 2);
			AUCustomsDataRegistry.Instance.BackgroundCargoReportSubThrottleWindow.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5);
			AUCustomsDataRegistry.Instance.CreatedTimeOFLastHeldAirCargoMessage.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.MinValue);
			AUCustomsDataRegistry.Instance.CreatedTimeOFLastHeldSeaCargoMessage.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.MinValue);
			Env.Registry.MessagesPerInterchange = 2;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var airMessage1 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIRCR, false);
			airMessage1.EM_MessageText = OutgoingAirCargoMessageText("S00001663");
			airMessage1.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			airMessage1.EM_HeldUntilDate = ZDateTime.UtcNow;
			airMessage1.EM_MessageNum = "M001001";
			var seaMessage1 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.SEACR, false);
			seaMessage1.EM_MessageText = OutgoingSeaCargoMessageText("S00001664");
			seaMessage1.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			seaMessage1.EM_HeldUntilDate = ZDateTime.UtcNow;
			seaMessage1.EM_MessageNum = "M001002";
			var seaMessage2 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.SEACR, false);
			seaMessage2.EM_MessageText = OutgoingSeaCargoMessageText("S00001665");
			seaMessage2.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			seaMessage2.EM_HeldUntilDate = ZDateTime.UtcNow;
			seaMessage2.EM_MessageNum = "M001003";
			var airMessage2 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIRCR, false);
			airMessage2.EM_MessageText = OutgoingAirCargoMessageText("S00001666");
			airMessage2.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			airMessage2.EM_HeldUntilDate = ZDateTime.UtcNow;
			airMessage2.EM_MessageNum = "M001004";
			Factory.Save();

			batchProcessor.ExecuteBatch();
			airMessage1.Reload();
			AssertEquals("airMessage1 sent because it has the lowest message number", EDIMessage.Status.Sent, airMessage1.EM_Status);
			AssertEquals("airMessage1 interchange created and queued", EDIInterchangeStatusList.Codes.eHubQueued, airMessage1.Interchange.EI_Status);

			seaMessage1.Reload();
			AssertEquals("seaMessage1 sent with airMessage1 because it has the next lowest message number", EDIMessage.Status.Sent, seaMessage1.EM_Status);
			AssertEquals("seaMessage1 interchange created and queued", EDIInterchangeStatusList.Codes.eHubQueued, seaMessage1.Interchange.EI_Status);

			seaMessage2.Reload();
			AssertEquals("seaMessage2 was ignored because limit allows only 2 messages", EDIMessage.Status.Queued, seaMessage2.EM_Status);
			AssertNull("seaMessage2 not in an interchange", seaMessage2.Interchange);

			airMessage2.Reload();
			AssertEquals("airMessage2 was ignored because limit allows only 2 messages", EDIMessage.Status.Queued, airMessage2.EM_Status);
			AssertNull("airMessage2 not in an interchange", airMessage2.Interchange);

			batchProcessor.ExecuteBatch();
			seaMessage2.Reload();
			AssertEquals("seaMessage2 was ignored in next cycle because throttle window is waiting for 5 minutes", EDIMessage.Status.Queued, seaMessage2.EM_Status);
			airMessage2.Reload();
			AssertEquals("airMessage2 was ignored in next cycle because throttle window is waiting for 5 minutes", EDIMessage.Status.Queued, airMessage2.EM_Status);

			airMessage1.Interchange.EI_SystemCreateTimeUtc = airMessage1.Interchange.EI_SystemCreateTimeUtc.AddMinutes(-10);
			seaMessage1.Interchange.EI_SystemCreateTimeUtc = seaMessage1.Interchange.EI_SystemCreateTimeUtc.AddMinutes(-10);
			Factory.Save();

			batchProcessor.ExecuteBatch();
			seaMessage2.Reload();
			AssertEquals("seaMessage2 sent in following cycle", EDIMessage.Status.Sent, seaMessage2.EM_Status);
			AssertEquals("seaMessage2 interchange created and queued", EDIInterchangeStatusList.Codes.eHubQueued, seaMessage2.Interchange.EI_Status);

			airMessage2.Reload();
			AssertEquals("airMessage2 sent in following cycle", EDIMessage.Status.Sent, airMessage2.EM_Status);
			AssertEquals("airMessage2 interchange created and queued", EDIInterchangeStatusList.Codes.eHubQueued, airMessage2.Interchange.EI_Status);
		}

		public void TestAirCargoMessageSendingHonoursHeldSetting()
		{
			AUCustomsDataRegistry.Instance.BackgroundCargoReportMaxMsg.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 2);
			AUCustomsDataRegistry.Instance.BackgroundCargoReportSubThrottleWindow.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 3);
			AUCustomsDataRegistry.Instance.CreatedTimeOFLastHeldAirCargoMessage.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.MinValue); // Might need to alter or duplicate this registry entry !
			Env.Registry.MessagesPerInterchange = 2;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var message1 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIRCR, false);
			message1.EM_MessageText = OutgoingAirCargoMessageText("S00001663");
			message1.EM_HeldUntilDate = ZDateTime.UtcNow;
			message1.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var message2 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIRCR, false);
			message2.EM_MessageText = OutgoingAirCargoMessageText("S00001664");
			message2.EM_HeldUntilDate = ZDateTime.UtcNow;
			message2.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var message3 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIRCR, false);
			message3.EM_MessageText = OutgoingAirCargoMessageText("S00001665");
			message3.EM_HeldUntilDate = ZDateTime.UtcNow;
			message3.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var message4 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIRCR, false);
			message4.EM_MessageText = OutgoingAirCargoMessageText("S00001666");
			message4.EM_HeldUntilDate = ZDateTime.UtcNow;
			message4.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var message5 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIRCR, false);
			message5.EM_MessageText = OutgoingAirCargoMessageText("S00001667");
			message5.EM_HeldUntilDate = ZDateTime.UtcNow;
			message5.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_RL_NKDischargePort = "AUSYD";
			mawb.CM_ArrivalDate = ZDateTime.UtcNow.AddDays(-1);
			var hawb = mawb.ChildBills.AddNew();
			message5.EM_LinkUniqueID = hawb.PK;
			message5.EM_LinkTable = hawb.TableName;
			var message6 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIRCR, false);
			message6.EM_MessageText = OutgoingAirCargoMessageText("S00001668");
			message6.EM_HeldUntilDate = ZDateTime.Empty;
			message6.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var message7 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIRCR, false);
			message7.EM_MessageText = OutgoingAirCargoMessageText("S00001669");
			message7.EM_HeldUntilDate = ZDateTime.Empty;
			message7.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var message8 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIRCR, false);
			message8.EM_MessageText = OutgoingAirCargoMessageText("S00001670");
			message8.EM_HeldUntilDate = ZDateTime.UtcNow;
			message8.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			Factory.Save();

			batchProcessor.ExecuteBatch();
			message1.Reload();
			AssertNull("message1 should have been excluded", message1.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message1.EM_Status);

			message2.Reload();
			AssertNull("message2 should have been excluded", message2.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message2.EM_Status);

			message3.Reload();
			AssertNull("message3 should have been excluded", message3.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message3.EM_Status);

			message4.Reload();
			AssertNull("message4 should have been excluded", message4.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message4.EM_Status);

			message5.Reload();
			var interchange = message5.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::AIRCR+S00001667/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message6.Reload();
			interchange = message6.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::AIRCR+S00001668/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message7.Reload();
			AssertNull("message7 should have been excluded", message7.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message7.EM_Status);
			AssertEquals(ZDateTime.Empty, message7.EM_HeldUntilDate);

			message8.Reload();
			AssertNull("message8 should have been excluded", message8.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message8.EM_Status);
			AssertEquals(DateTime.MinValue, AUCustomsDataRegistry.Instance.CreatedTimeOFLastHeldAirCargoMessage.Value);

			batchProcessor.ExecuteBatch();
			message1.Reload();
			interchange = message1.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::AIRCR+S00001663/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message2.Reload();
			AssertNull("message2 should have been excluded", message2.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message2.EM_Status);

			message3.Reload();
			AssertNull("message3 should have been excluded", message3.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message3.EM_Status);

			message4.Reload();
			AssertNull("message4 should have been excluded", message4.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message4.EM_Status);

			message7.Reload();
			interchange = message7.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::AIRCR+S00001669/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message8.Reload();
			AssertNull("message8 should have been excluded", message8.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message8.EM_Status);
			AssertEquals(message1.EM_SystemCreateTimeUtc.ToDateTime(), AUCustomsDataRegistry.Instance.CreatedTimeOFLastHeldAirCargoMessage.Value);

			Env.Registry.MessagesPerInterchange = 5;
			AUCustomsDataRegistry.Instance.BackgroundCargoReportMaxMsg.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 4);
			batchProcessor.ExecuteBatch();

			message2.Reload();
			interchange = message2.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::AIRCR+S00001664/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message3.Reload();
			interchange = message3.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::AIRCR+S00001665/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message4.Reload();
			interchange = message4.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::AIRCR+S00001666/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message8.Reload();
			AssertNull("message8 should have been excluded", message8.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message8.EM_Status);
			AssertEquals(message4.EM_SystemCreateTimeUtc.ToDateTime(), AUCustomsDataRegistry.Instance.CreatedTimeOFLastHeldAirCargoMessage.Value);

			batchProcessor.ExecuteBatch();
			message8.Reload();
			AssertNull("message8 should have been excluded as limit has been reached for this timeframe", message8.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message8.EM_Status);
			AssertEquals(message4.EM_SystemCreateTimeUtc.ToDateTime(), AUCustomsDataRegistry.Instance.CreatedTimeOFLastHeldAirCargoMessage.Value);

			AUCustomsDataRegistry.Instance.BackgroundCargoReportMaxMsg.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5);
			batchProcessor.ExecuteBatch();
			message8.Reload();
			interchange = message8.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::AIRCR+S00001670/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));
			AssertEquals(message8.EM_SystemCreateTimeUtc.ToDateTime(), AUCustomsDataRegistry.Instance.CreatedTimeOFLastHeldAirCargoMessage.Value);
		}

		public void TestSendAirCargoReportEndToEnd()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var message = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIRCR, false);
			message.EM_MessageText = OutgoingAirCargoMessageText("S00001663");
			Factory.Save();

			batchProcessor.ExecuteBatch();
			AssertEquals("Should not be an ERouter copy", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			message.Reload();
			var interchange = message.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("EHub queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertEquals("Retry count is zero", 0, interchange.EI_RetryCount);
			AssertContains("Body is Base64 encoded", "Content-Transfer-Encoding: base64", interchange.EI_BodyText);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::AIRCR+S00001663/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));
		}

		public void TestAirOutturnMessageSendingHonoursHeldSetting()
		{
			AUCustomsDataRegistry.Instance.BackgroundCargoReportMaxMsg.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 2);
			AUCustomsDataRegistry.Instance.BackgroundCargoReportSubThrottleWindow.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 10);
			Env.Registry.MessagesPerInterchange = 2;

			var message1 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIROUT, false);
			message1.EM_MessageText = OutgoingAirCargoMessageText("S00001663");
			message1.EM_HeldUntilDate = ZDateTime.UtcNow;
			message1.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var message2 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIROUT, false);
			message2.EM_MessageText = OutgoingAirCargoMessageText("S00001664");
			message2.EM_HeldUntilDate = ZDateTime.UtcNow;
			message2.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var message3 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIROUT, false);
			message3.EM_MessageText = OutgoingAirCargoMessageText("S00001665");
			message3.EM_HeldUntilDate = ZDateTime.UtcNow;
			message3.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var message4 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIROUT, false);
			message4.EM_MessageText = OutgoingAirCargoMessageText("S00001666");
			message4.EM_HeldUntilDate = ZDateTime.UtcNow;
			message4.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var message5 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIROUT, false);
			message5.EM_MessageText = OutgoingAirCargoMessageText("S00001667");
			message5.EM_HeldUntilDate = ZDateTime.Empty;
			message5.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var message6 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIROUT, false);
			message6.EM_MessageText = OutgoingAirCargoMessageText("S00001668");
			message6.EM_HeldUntilDate = ZDateTime.Empty;
			message6.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var message7 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIROUT, false);
			message7.EM_MessageText = OutgoingAirCargoMessageText("S00001669");
			message7.EM_HeldUntilDate = ZDateTime.Empty;
			message7.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var message8 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIROUT, false);
			message8.EM_MessageText = OutgoingAirCargoMessageText("S00001670");
			message8.EM_HeldUntilDate = ZDateTime.UtcNow;
			message8.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			Factory.Save();

			batchProcessor.ExecuteBatch();
			message1.Reload();
			AssertNull("message1 should have been excluded", message1.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message1.EM_Status);

			message2.Reload();
			AssertNull("message2 should have been excluded", message2.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message2.EM_Status);

			message3.Reload();
			AssertNull("message3 should have been excluded", message3.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message3.EM_Status);

			message4.Reload();
			AssertNull("message4 should have been excluded", message4.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message4.EM_Status);

			message5.Reload();
			var interchange = message5.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::AIRCR+S00001667/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message6.Reload();
			interchange = message6.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::AIRCR+S00001668/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message7.Reload();
			AssertNull("message7 should have been excluded", message7.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message7.EM_Status);
			AssertEquals(ZDateTime.Empty, message7.EM_HeldUntilDate);

			message8.Reload();
			AssertNull("message8 should have been excluded", message8.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message8.EM_Status);

			batchProcessor.ExecuteBatch();
			message1.Reload();
			interchange = message1.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::AIRCR+S00001663/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message2.Reload();
			AssertNull("message2 should have been excluded", message2.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message2.EM_Status);

			message3.Reload();
			AssertNull("message3 should have been excluded", message3.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message3.EM_Status);

			message4.Reload();
			AssertNull("message4 should have been excluded", message4.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message4.EM_Status);

			message7.Reload();
			interchange = message7.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::AIRCR+S00001669/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message8.Reload();
			AssertNull("message8 should have been excluded", message8.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message8.EM_Status);

			Env.Registry.MessagesPerInterchange = 5;
			AUCustomsDataRegistry.Instance.BackgroundCargoReportMaxMsg.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 4);
			batchProcessor.ExecuteBatch();

			message2.Reload();
			interchange = message2.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::AIRCR+S00001664/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message3.Reload();
			interchange = message3.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::AIRCR+S00001665/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message4.Reload();
			interchange = message4.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::AIRCR+S00001666/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message8.Reload();
			AssertNull("message8 should have been excluded", message8.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message8.EM_Status);

			batchProcessor.ExecuteBatch();
			message8.Reload();
			AssertNull("message8 should have been excluded as limit has been reached for this timeframe", message8.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message8.EM_Status);

			AUCustomsDataRegistry.Instance.BackgroundCargoReportMaxMsg.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5);
			batchProcessor.ExecuteBatch();
			message8.Reload();
			interchange = message8.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
		}

		public void TestSeaCargoMessageSendingHonoursHeldSetting()
		{
			AUCustomsDataRegistry.Instance.BackgroundCargoReportMaxMsg.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 2);
			AUCustomsDataRegistry.Instance.BackgroundCargoReportSubThrottleWindow.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 3);
			AUCustomsDataRegistry.Instance.CreatedTimeOFLastHeldSeaCargoMessage.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.MinValue); // Might need to alter or duplicate this registry entry !
			Env.Registry.MessagesPerInterchange = 2;

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var message1 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.SEACR, false);
			message1.EM_MessageText = OutgoingSeaCargoMessageText("S00001663");
			message1.EM_HeldUntilDate = ZDateTime.UtcNow;
			message1.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var message2 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.SEACR, false);
			message2.EM_MessageText = OutgoingSeaCargoMessageText("S00001664");
			message2.EM_HeldUntilDate = ZDateTime.UtcNow;
			message2.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var message3 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.SEACR, false);
			message3.EM_MessageText = OutgoingSeaCargoMessageText("S00001665");
			message3.EM_HeldUntilDate = ZDateTime.UtcNow;
			message3.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var message4 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.SEACR, false);
			message4.EM_MessageText = OutgoingSeaCargoMessageText("S00001666");
			message4.EM_HeldUntilDate = ZDateTime.UtcNow;
			message4.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;

			var message5 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.SEACR, false);
			message5.EM_MessageText = OutgoingSeaCargoMessageText("S00001667");
			message5.EM_HeldUntilDate = ZDateTime.UtcNow;
			message5.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			oceanBill.CB_DateOfArrival = ZDateTime.UtcNow.AddDays(-1);
			var houseBill = oceanBill.HouseBills.AddNew();
			message5.EM_LinkUniqueID = houseBill.PK;
			message5.EM_LinkTable = houseBill.TableName;

			var message6 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.SEACR, false);
			message6.EM_MessageText = OutgoingSeaCargoMessageText("S00001668");
			message6.EM_HeldUntilDate = ZDateTime.Empty;
			message6.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var message7 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.SEACR, false);
			message7.EM_MessageText = OutgoingSeaCargoMessageText("S00001669");
			message7.EM_HeldUntilDate = ZDateTime.Empty;
			message7.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			var message8 = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.SEACR, false);
			message8.EM_MessageText = OutgoingSeaCargoMessageText("S00001670");
			message8.EM_HeldUntilDate = ZDateTime.UtcNow;
			message8.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
			Factory.Save();

			batchProcessor.ExecuteBatch();
			message1.Reload();
			AssertNull("message1 should have been excluded", message1.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message1.EM_Status);

			message2.Reload();
			AssertNull("message2 should have been excluded", message2.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message2.EM_Status);

			message3.Reload();
			AssertNull("message3 should have been excluded", message3.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message3.EM_Status);

			message4.Reload();
			AssertNull("message4 should have been excluded", message4.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message4.EM_Status);

			message5.Reload();
			var interchange = message5.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::SEACR+S00001667/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message6.Reload();
			interchange = message6.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::SEACR+S00001668/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message7.Reload();
			AssertNull("message7 should have been excluded", message7.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message7.EM_Status);
			AssertEquals(ZDateTime.Empty, message7.EM_HeldUntilDate);

			message8.Reload();
			AssertNull("message8 should have been excluded", message8.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message8.EM_Status);
			AssertEquals(DateTime.MinValue, AUCustomsDataRegistry.Instance.CreatedTimeOFLastHeldSeaCargoMessage.Value);

			batchProcessor.ExecuteBatch();
			message1.Reload();
			interchange = message1.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::SEACR+S00001663/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message2.Reload();
			AssertNull("message2 should have been excluded", message2.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message2.EM_Status);

			message3.Reload();
			AssertNull("message3 should have been excluded", message3.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message3.EM_Status);

			message4.Reload();
			AssertNull("message4 should have been excluded", message4.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message4.EM_Status);

			message7.Reload();
			interchange = message7.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::SEACR+S00001669/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message8.Reload();
			AssertNull("message8 should have been excluded", message8.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message8.EM_Status);
			AssertEquals(message1.EM_SystemCreateTimeUtc.ToDateTime(), AUCustomsDataRegistry.Instance.CreatedTimeOFLastHeldSeaCargoMessage.Value);

			Env.Registry.MessagesPerInterchange = 5;
			AUCustomsDataRegistry.Instance.BackgroundCargoReportMaxMsg.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 4);
			batchProcessor.ExecuteBatch();

			message2.Reload();
			interchange = message2.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::SEACR+S00001664/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message3.Reload();
			interchange = message3.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::SEACR+S00001665/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message4.Reload();
			interchange = message4.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::SEACR+S00001666/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));

			message8.Reload();
			AssertNull("message8 should have been excluded", message8.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message8.EM_Status);
			AssertEquals(message4.EM_SystemCreateTimeUtc.ToDateTime(), AUCustomsDataRegistry.Instance.CreatedTimeOFLastHeldSeaCargoMessage.Value);

			batchProcessor.ExecuteBatch();
			message8.Reload();
			AssertNull("message8 should have been excluded as limit has been reached for this timeframe", message8.Interchange);
			AssertEquals(EDIMessage.Status.Queued, message8.EM_Status);
			AssertEquals(message4.EM_SystemCreateTimeUtc.ToDateTime(), AUCustomsDataRegistry.Instance.CreatedTimeOFLastHeldSeaCargoMessage.Value);

			AUCustomsDataRegistry.Instance.BackgroundCargoReportMaxMsg.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5);
			batchProcessor.ExecuteBatch();
			message8.Reload();
			interchange = message8.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("Queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::SEACR+S00001670/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));
			AssertEquals(message8.EM_SystemCreateTimeUtc.ToDateTime(), AUCustomsDataRegistry.Instance.CreatedTimeOFLastHeldSeaCargoMessage.Value);
		}

		public void TestSendSeaCargoReportEndToEnd()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var message = InsertMessageIntoDatabase(EDIInterchange.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.AIRCR, false);
			message.EM_MessageText = OutgoingSeaCargoMessageText("S00001663");
			Factory.Save();

			batchProcessor.ExecuteBatch();
			AssertEquals("Should not be an ERouter copy", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			message.Reload();
			var interchange = message.Interchange;
			AssertNotNull("Interchange created", interchange);
			AssertEquals("EHub queued status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertEquals("Retry count is zero", 0, interchange.EI_RetryCount);
			AssertContains("Body is Base64 encoded", "Content-Transfer-Encoding: base64", interchange.EI_BodyText);
			AssertContains("Expected body contents", "CUSCAR:D:99B:UN'BGM+933:::SEACR+S00001663/1:1+9'", UnpackInterchangeBody(interchange.EI_BodyText));
		}

		public void TestSendNEXDOCS()
		{
			var message = InsertMessageIntoDatabase(EDIMessage.ApplicationCodes.NEXDOCS, "NEX", false);
			message.EM_MessageText = @"<RexAcknowledgeOwnership xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
    <identification>
      <rexNumber>REX0000028829</rexNumber>
    </identification>
    <isAccepted>true</isAccepted>
  </RexAcknowledgeOwnership>";
			Factory.Save();
			batchProcessor.ExecuteBatch();

			message.Reload();
			var interchange = message.Interchange;
			AssertNotNull("Should have created Interchange.", interchange);
		}

		public void TestSendCOLSInterchange()
		{
			var message = InsertMessageIntoDatabase(EDIMessage.ApplicationCodes.COLS, "CNL", false);
			Factory.Save();
			batchProcessor.ExecuteBatch();

			message.Reload();
			var interchange = message.Interchange;
			AssertNotNull("Should have created Interchange.", interchange);
		}

		public void TestConcurrencyErrorDoesNotCauseException()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_InterchangeType = "CTL";
			interchange.EI_To = "TO";
			interchange.EI_From = "FROM";
			interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			AUCInterchangeSenderTestHelper batchProcessorHelper = new AUCInterchangeSenderTestHelper(certConfig);

			batchProcessorHelper.ForceConcurrencyError = true;
			batchProcessorHelper.ExecuteBatch();
			interchange.Reload();
			AssertContains("Log contains Concurrency Error", "Save failed for Interchange 1: **CONCURRENCY Error Saving Record **", batchProcessorHelper.Logger.UserLogStrings[1].Replace("\r", "").Replace("\n", ""));

			batchProcessorHelper.ForceConcurrencyError = false;
			batchProcessorHelper.ExecuteBatch();
			interchange.Reload();
			AssertContains("Now sent", "1 new interchange(s) sent.", batchProcessorHelper.Logger.UserLogStrings[1]);
		}

		public void TestNumberToBatch()
		{
			AssertEquals("Pre-condition - default registry value", 50, Env.Registry.MessagesPerInterchange);

			var batchProcessor = new AUCInterchangeSenderForTest(certConfig);
			AssertEquals(50, batchProcessor.NumberToBatch);

			Env.Registry.MessagesPerInterchange = 200;
			AssertEquals("Messages per Interchange is Capped at 100", 100, batchProcessor.NumberToBatch);
		}

		public void TestDontSendCMRInterchangeIfWeAreMissingCompanyCertificate()
		{
			Env.Registry.AUCCompanyCertificateData = Array.Empty<byte>();
			InsertInterchangeIntoDatabase(EDIInterchange.ApplicationCodes.CMR, "TO", false);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			certConfig.Dispose();
			certConfig = new CertificateManager(Factory);
			batchProcessor = new AUCInterchangeSenderForTest(certConfig);

			batchProcessor.ExecuteBatch();
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		public void TestDontSendCMRInterchangeIfWeAreMissingCompanyCertificatePassword()
		{
			Env.Registry.AUCCompanyCertificatePassword = ZString.Empty;
			InsertInterchangeIntoDatabase(EDIInterchange.ApplicationCodes.CMR, "TO", false);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			certConfig.Dispose();
			certConfig = new CertificateManager(Factory);
			batchProcessor = new AUCInterchangeSenderForTest(certConfig);

			batchProcessor.ExecuteBatch();
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			AssertNotNull(ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestDontSendInterchangeFromADifferentCompany()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			InsertInterchangeIntoDatabase(EDIInterchange.ApplicationCodes.OneStop, "CSXTEST", true);
			batchProcessor.ExecuteBatch();
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		public void TestDontSendCMRInterchangeFromADifferentCompany()
		{
			InsertInterchangeIntoDatabase(EDIInterchange.ApplicationCodes.CMR, "TO", true);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			batchProcessor.ExecuteBatch();
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		[ExpectNoExceptions()]
		public void TestSendOneStopInterchangeWithFunnyToDoesntCreateEndlessLoop()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			InsertInterchangeIntoDatabase(EDIInterchange.ApplicationCodes.OneStop, "LALALA", false);
			batchProcessor.ExecuteBatch();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var certificatesHelper = ObjectFactory.New<ICertificateManagerHelper>(Factory);
			certificatesHelper.SetupValidCompanyCertificatesForTest(out var _, out var _);
			certificatesHelper.CreateCustomsCertificates2023();
			Factory.Save();

			batchProcessor = new AUCInterchangeSender();
			certConfig = new CertificateManager(Factory);

			StoreTimeZoneOffsets();
		}
		CertificateManager certConfig;
		AUCInterchangeSender batchProcessor;

		protected override void TearDown()
		{
			base.TearDown();
			certConfig?.Dispose();
			batchProcessor.Dispose();

			CleanUpTimeZoneOffsets();
		}

		void StoreTimeZoneOffsets()
		{
			var sql = $@"
INSERT INTO RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES (NEWID(), 'AUSYD', '2016-10-02 03:00:00.000', '2017-04-02 02:00:00.000', 660);
INSERT INTO RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES (NEWID(), 'AUSYD', '2017-10-01 03:00:00.000', '2018-04-01 02:00:00.000', 660);
INSERT INTO RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES (NEWID(), 'AUSYD', '2018-04-01 02:00:00.000', '2018-10-07 03:00:00.000', 600);
INSERT INTO RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES (NEWID(), 'AUSYD', '2018-10-07 03:00:00.000', '2019-04-07 02:00:00.000', 660);
INSERT INTO RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES (NEWID(), 'AUSYD', '2017-04-02 02:00:00.000', '2017-10-01 03:00:00.000', 600);
INSERT INTO RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES (NEWID(), 'AUSYD', '2023-04-02 02:00:00.000', '2023-10-07 03:00:00.000', 600);
INSERT INTO RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES (NEWID(), 'AUSYD', '2023-10-07 03:00:00.000', '2024-04-02 02:00:00.000', 660);
";
			Db.Connection.Command(sql).ExecuteNonQuery();
		}

		void CleanUpTimeZoneOffsets()
		{
			var sql = $@"
DELETE FROM dbo.RefDatabase_RefUNLOCOUtcOffset WHERE RLO_RL_NKCode = 'AUSYD';
";
			Db.Connection.Command(sql).ExecuteNonQuery();
		}

		ZString UnpackInterchangeBody(ZString signedText)
		{
			string signedContent = signedText.Substring(signedText.IndexOf("\r\n\r\n") + 4);
			var verifiedContent = certConfig.TrustPointCertificate.VerifyBytes(signedContent, certConfig.CompanyCertificate.Name);
			return BatchProcessorSupporter.DecodeMIMEText(verifiedContent.Content);
		}

		string OutgoingAirCargoMessageText(string reference) => ZString.Format(outgoingAirCargoMessageText, reference);
		const string outgoingAirCargoMessageText = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'BGM+933:::AIRCR+{0}/1:1+9'RFF+PQ:CC'RFF+HWB:CuckooSqueaker'RFF+MWB:61855037172'NAD+CN+++ZF AUSTRALIA PTY LTD+LOCKED BAG 6305+BLACKTOWN++2147+AU'NAD+CZ+++LEMFORDER+INTERNATIONAL AG & CO. KG.:BORGWARDSTRASSE 16+BREMEN++28279+DE'NAD+VW+66015286036::95'TDT+20+221++6+SQ::3'LOC+8+AUSYD::6'LOC+76+DEFRA::6'LOC+12+AUSYD::6'LOC+91+DEFRA::6'DTM+178:20041216:102'CNI+1'RFF+UCN:S00001663'MOA+96:NDV'GID+1'PAC+1'FTX+AAA+++AUTOMOTIVE SPARE PART'MEA+AAE+G+KG:144.00'UNT+22+<<MSGNO PLACEHOLDER>>'";

		string OutgoingSeaCargoMessageText(string reference) => ZString.Format(outgoingSeaCargoMessageText, reference);
		const string outgoingSeaCargoMessageText = @"UNH+<<MSGNO PLACEHOLDER>>+CUSCAR:D:99B:UN'BGM+933:::SEACR+{0}/1:1+9'RFF+PQ:CC'RFF+BH:SHA11'RFF+MB:SHA1'NAD+CN++AMY TEST::123 TEST STREET  SYDNEY 2000 AU'NAD+CZ++ASUS COMPUTER CO LTD::8,168 MEISHENG ROAD  WAI GOA QIAO 2:00131 CN'NAD+VW+41065894724::95'NAD+AH+41065894724::95'TDT+20+2PW++11++++9044748::11'LOC+8+AUSYD::6'LOC+76+CNSHA::6'LOC+12+AUSYD::6'LOC+73+CNSHA::6'LOC+27+CN::5'CNI++:::I'RFF+AAQ:PWSU5577994'GID+1'RFF+ACC:2008'GID+1'RFF+SN:G'GID+1'PAC+1'PAC+++FCL:67:95'PAC+++GENN:121:95'PAC+++PF:185:95'FTX+AAA+++STUFF'MEA+AAE+AAL+KG:1.00'MEA+AAE+G+KG:1.00'MEA+AAE+ABJ+CU:1.00'PCI+28+NM'UNT+32+1'";

		EDIMessage InsertMessageIntoDatabase(string applicationCode, bool insertForOtherCompany) => InsertMessageIntoDatabase(applicationCode, ZString.Empty, insertForOtherCompany);

		EDIMessage InsertMessageIntoDatabase(string applicationCode, string messageType, bool insertForOtherCompany)
		{
			var message = Factory.New<TestEdiMessage>();
			message.EM_ApplicationCode = applicationCode;
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			if (insertForOtherCompany)
			{
				message.EM_GB = BranchFromOtherCompany.PK;
			}

			Factory.Save();
			return message;
		}

		EDIInterchange InsertInterchangeIntoDatabase(string applicationCode, string toParty, bool insertForOtherCompany, string status = EDIInterchange.Status.Queued)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = applicationCode;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_Status = status;
			interchange.EI_To = toParty;
			interchange.EI_From = "FROM";
			if (insertForOtherCompany)
			{
				interchange.EI_GB = BranchFromOtherCompany.PK;
			}
			else
			{
				interchange.EI_GB = GlbBranch.CurrentBranch.PK;
			}

			Factory.Save();
			return interchange;
		}

		GlbBranch BranchFromOtherCompany
		{
			get
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
				return Factory.LoadTop1<GlbBranch>(filter);
			}
		}

		sealed class BatchAUCInterchangeSenderTimed : AUCInterchangeSenderForTest
		{
			public BatchAUCInterchangeSenderTimed(CertificateManager certConfig)
				: base(certConfig)
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				EDIInterchange interchange0 = factory.New<EDIInterchange>();
				EDIInterchange interchange1 = factory.New<EDIInterchange>();

				SendableInterchanges = new BusinessObject[] { interchange0, interchange1 };
			}

			public override TimeSpan TimeToSpendProcessing => new TimeSpan(0, 0, 0, 45);

			protected override void SendOutboundCMRInterchange()
			{
				SendOutboundCMRInterchangeCallCount++;
				Thread.Sleep(new TimeSpan(TimeSpan.TicksPerMinute));
			}

			public int SendOutboundCMRInterchangeCallCount;
		}

		sealed class TestEdiMessage : EDIMessage
		{
			public TestEdiMessage(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override string GetMessageReferenceNumber()
			{
				string sender = Env.Registry.AUCustomsEdificeSenderID;
				string receiver = EDIInterchange.InterchangePartyIDs.CMRMailbox;
				return Env.NumberFountains.EDIFACTNumberFountain("M", sender, receiver).GetNextFormatted(Factory);
			}
		}

		class AUCInterchangeSenderForTest : AUCInterchangeSender
		{
			public AUCInterchangeSenderForTest(CertificateManager certConfig)
				: base()
			{
				certManager = certConfig;
			}
			readonly CertificateManager certManager;

			protected override CertificateManager CertManager => certManager;

			new public int NumberToBatch => base.NumberToBatch;
		}

		sealed class AUCInterchangeSenderTestHelper : AUCInterchangeSenderForTest
		{
			public AUCInterchangeSenderTestHelper(CertificateManager certConfig)
				: base(certConfig)
			{
			}

			public bool ForceConcurrencyError;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
			protected override bool SendCMRInterchange(CMRInterchange interchange, bool iseHubInterchangeBeingReprocessed)
			{
				if (ForceConcurrencyError)
				{
					BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
					EDIInterchange interchangeInAnotherFactory = anotherFactory.Load<EDIInterchange>(interchange.PK);
					interchangeInAnotherFactory.EI_Priority = "A";
					anotherFactory.Save();
					interchange.EI_Priority = "B";
				}
				return base.SendCMRInterchange(interchange, iseHubInterchangeBeingReprocessed);
			}

			public override TimeSpan TimeToSpendProcessing => new TimeSpan(0, 0, 0);
		}
	}
}
