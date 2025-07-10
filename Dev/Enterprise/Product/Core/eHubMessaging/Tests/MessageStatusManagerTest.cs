using System;
using CargoWise.Application;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.eHubMessaging.Tests
{
	public class MessageStatusManagerTest : TestCaseWithFactory
	{
		public void TestFailedInterchangeAndMessages()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			var message1 = interchange.ContainedMessages.AddNew();
			var message2 = interchange.ContainedMessages.AddNew();

			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			message1.EM_Status = EDIMessageStatusList.Codes.Sent;
			message2.EM_Status = EDIMessageStatusList.Codes.Sent;

			var manager = new MessageStatusManager(interchange);
			manager.Fail("Some Error");

			AssertEquals(EDIInterchangeStatusList.Codes.Failed, interchange.EI_Status);
			AssertEquals(EDIMessageStatusList.Codes.Failed, message1.EM_Status);
			AssertEquals(EDIMessageStatusList.Codes.Failed, message2.EM_Status);
			AssertCollectionNotContains("Message handlers do not log", interchange.Logs.GetAllLogs(), log => true);
			AssertCollectionNotContains("Message handlers do not log", message1.Logs.GetAllLogs(), log => true);
			AssertCollectionNotContains("Message handlers do not log", message2.Logs.GetAllLogs(), log => true);
		}

		public void TestUpdateInterchangeStatus()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			var message1 = interchange.ContainedMessages.AddNew();
			var message2 = interchange.ContainedMessages.AddNew();

			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;

			interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			message1.EM_Status = EDIMessageStatusList.Codes.Sent;
			message2.EM_Status = EDIMessageStatusList.Codes.Sent;

			var manager = new MessageStatusManager(interchange);
			manager.Update(EDIInterchangeStatusList.Codes.Received);

			AssertEquals(EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
			AssertEquals(EDIMessageStatusList.Codes.Sent, message1.EM_Status);
			AssertEquals(EDIMessageStatusList.Codes.Sent, message2.EM_Status);
		}

		public void TestUpdateInterchangeAndMessagesStatus()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			var message1 = interchange.ContainedMessages.AddNew();
			var message2 = interchange.ContainedMessages.AddNew();

			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;

			interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			message1.EM_Status = EDIMessageStatusList.Codes.Sent;
			message2.EM_Status = EDIMessageStatusList.Codes.Sent;

			var manager = new MessageStatusManager(interchange);
			manager.Update(EDIInterchangeStatusList.Codes.Received, EDIMessageStatusList.Codes.Received);

			AssertEquals(EDIInterchangeStatusList.Codes.Received, interchange.EI_Status);
			AssertEquals(EDIMessageStatusList.Codes.Received, message1.EM_Status);
			AssertEquals(EDIMessageStatusList.Codes.Received, message2.EM_Status);
		}

		public void TestNoExceptionWhenUpdateFiledApplicationMessageAndHandlerIsNull()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			var message1 = interchange.ContainedMessages.AddNew();

			interchange.EI_ApplicationCode = "QQQ";
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;

			var manager = new MessageStatusManager(interchange);
			AssertNoExceptionThrown(() => manager.Fail("Some Error"));
		}

		public void TestNoExceptionWhenUpdateFiledApplicationMessageAndCompanyIsNull()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_GB = ZGuid.Empty;
			interchange.EI_ApplicationCode = "QQQ";
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
			AssertNull("Precondition", interchange.Company);

			var manager = new MessageStatusManager(interchange);
			AssertNoExceptionThrown(() => manager.Fail("Some Error"));
		}

		public void TestFailApplicationMessage()
		{
			foreach (var testCountry in Country.LicenceKeyBuilderSupportedCountryCodes)
			{
				TestFailApplicationMessage(testCountry);
			}

			void TestFailApplicationMessage(string testCountry)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(testCountry))
				{
					var defaultHandlerMock = new Mock<IFailedMessageHandler>();
					var countrySpecificalhandlerMock = new Mock<IFailedMessageHandler>();

					var objectHandleMock1 = new Mock<ObjectHandle>();
					objectHandleMock1.Setup(m => m.GetObject()).Returns(defaultHandlerMock.Object);
					var objectHandleMock2 = new Mock<ObjectHandle>();
					objectHandleMock2.Setup(m => m.GetObject()).Returns(countrySpecificalhandlerMock.Object);

					var handlers = new KeyObjectHandleDictionaryObject
					{
						{ "Default", objectHandleMock1.Object },
						{ testCountry, objectHandleMock2.Object }
					};

					using (ObjectFactory.Substitute("GEIFailedMessageHandlers", handlers))
					{
						var interchange = Factory.NewWithValidTestData<EDIInterchange>();
						interchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
						AssertEquals("Precondition", testCountry, interchange.Company.GC_RN_NKCountryCode);

						var manager = new MessageStatusManager(interchange);

						interchange.EI_ApplicationCode = "QQQ";
						AssertNoExceptionThrown(() => manager.Fail("Some Error"));
						defaultHandlerMock.Verify(x => x.UpdateFailedMessage(It.IsAny<EDIInterchange>()), Times.Never);
						countrySpecificalhandlerMock.Verify(x => x.UpdateFailedMessage(It.IsAny<EDIInterchange>()), Times.Never);

						interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GlobalElectronicInvoice;
						AssertNoExceptionThrown(() => manager.Fail("Some Error"));
						defaultHandlerMock.Verify(x => x.UpdateFailedMessage(It.IsAny<EDIInterchange>()), Times.Never);
						countrySpecificalhandlerMock.Verify(x => x.UpdateFailedMessage(It.IsAny<EDIInterchange>()), Times.Once);

						defaultHandlerMock.Reset();
						countrySpecificalhandlerMock.Reset();

						interchange.Company.GC_RN_NKCountryCode = "??";
						AssertNoExceptionThrown(() => manager.Fail("Some Error"));
						defaultHandlerMock.Verify(x => x.UpdateFailedMessage(It.IsAny<EDIInterchange>()), Times.Once);
						countrySpecificalhandlerMock.Verify(x => x.UpdateFailedMessage(It.IsAny<EDIInterchange>()), Times.Never);
					}
				}
			}
		}

		class TestMessageStatusHandler : MessageStatusHandler
		{
			public TestMessageStatusHandler(IeHubMessage inputMessage)
			{
				Message = inputMessage;
			}

			protected override string BestMatchOutgoingInterchangeStatus => throw new NotImplementedException();

			protected override string[] SupportedMatchOutgoingInterchangeStatuses => throw new NotImplementedException();

			protected override string[] SupportedLogOnlyMatchOutgoingInterchangeStatuses => throw new NotImplementedException();

			protected override string ResultStatus => throw new NotImplementedException();

			protected override void UpdateInterchangeStatus(EDIInterchange interchange)
			{
				throw new NotImplementedException();
			}
		}
	}
}
