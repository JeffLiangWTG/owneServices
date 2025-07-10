using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Shared;
using Moq;

namespace Enterprise.Messaging.Business.Test
{
	sealed class XtMessageEventsInfoTest : TestCaseWithFactory
	{
		public void TestInitializeXtMessageEventsInfo()
		{
			var interchange = Factory.New<EDIInterchange>();
			var info = new XtMessageEventsInfoForTest(interchange);

			CombineAssertions(() =>
			{
				AssertEquals("Error", info.MsgState);

				var eventCollection = info.XtMessageEvents;
				AssertEquals(2, eventCollection.Count);
				AssertEquals("Sent to application: test logtext2", eventCollection[0].LogText);
				AssertEquals("Message received from contract: test logtext4", eventCollection[1].LogText);
			});
		}

		public void TestReLoad()
		{
			var interchange = Factory.New<EDIInterchange>();
			var info = new XtMessageEventsInfoForTest(interchange);

			CombineAssertions(() =>
			{
				AssertEquals("First load MsgState", "Error", info.MsgState);
				var eventCollection = info.XtMessageEvents;
				AssertEquals("First load eventCollection", 2, eventCollection.Count);
				AssertEquals("Sent to application: test logtext2", eventCollection[0].LogText);
				AssertEquals("Message received from contract: test logtext4", eventCollection[1].LogText);

				info.ReLoad(true);

				AssertEquals("Reload MsgState", "Finished", info.MsgState);
				eventCollection = info.XtMessageEvents;
				AssertEquals("Reload eventCollection", 1, eventCollection.Count);
				AssertEquals("Message received from contract: test logtext4", eventCollection[0].LogText);
			});
		}

		public void TestClientError()
		{
			var interchange = Factory.New<EDIInterchange>();
			var info = new XtMessageEventsInfoForErrorTest(interchange);

			CombineAssertions(() =>
			{
				AssertEquals(string.Empty, info.MsgState);

				var eventCollection = info.XtMessageEvents;
				AssertEquals(1, eventCollection.Count);
				AssertEquals("Exception thrown.", eventCollection[0].LogText);
			});
		}

		class XtMessageEventsInfoForTest : XtMessageEventsInfo
		{
			public XtMessageEventsInfoForTest(EDIInterchange interchange) : base(interchange)
			{
				visitStateCount = 0;
				visitEventsCount = 0;
			}
			int visitStateCount, visitEventsCount;

			protected override IXtMessageEventsReaderClientProvider GetXtMessageEventsReaderClientProvider()
			{
				var mockClient = new Mock<IXtMessageEventsReaderClient>();

				mockClient.Setup(c => c.GetMsgEvents(It.IsAny<HashSet<ulong>>(), It.IsAny<bool>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
					.Returns((HashSet<ulong> xtMsgIds, bool isRecursive, DateTime? deadline, CancellationToken? cancellationToken) =>
					{
						var eventList = new List<IXtMessageEventData>();
						if (visitEventsCount == 0)
						{
							eventList.Add(new XtMessageEventData(1, 101, "{\"logevent\":\"2\",\"time\":\"2023-10-20T12:32:21\",\"logtext\":\"test logtext2\"}"));
						}
						eventList.Add(new XtMessageEventData(2, 101, "{\"logevent\":\"4\",\"time\":\"2023-10-20T12:32:25\",\"logtext\":\"test logtext4\"}"));
						visitEventsCount++;
						return eventList;
					});

				mockClient.Setup(c => c.GetMsgState(It.IsAny<ulong>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
					.Returns((ulong msgId, DateTime? deadline, CancellationToken? cancellationToken) =>
							{
								var (stateCode, stateName) = visitStateCount > 0 ? ("1023", "Finished") : ("5127", "ConfigurationError");
								visitStateCount++;
								return (stateCode, stateName);
							});
				var mockClientProvider = new Mock<IXtMessageEventsReaderClientProvider>();
				mockClientProvider.Setup(p => p.XtMessageEventsReaderClient).Returns(mockClient.Object);
				mockClientProvider.Setup(p => p.TearDown());
				return mockClientProvider.Object;
			}
		}

		class XtMessageEventsInfoForErrorTest : XtMessageEventsInfo
		{
			public XtMessageEventsInfoForErrorTest(EDIInterchange interchange) : base(interchange)
			{
			}

			protected override IXtMessageEventsReaderClientProvider GetXtMessageEventsReaderClientProvider()
			{
				throw new Exception("Exception thrown.");
			}
		}
	}
}
