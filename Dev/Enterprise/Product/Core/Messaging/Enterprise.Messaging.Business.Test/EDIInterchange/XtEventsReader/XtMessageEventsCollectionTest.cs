using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.Test
{
	[TestedAsNonPersistentBusinessObject]
	[TestedType(typeof(XtMessageEventsCollection))]
	sealed class XtMessageEventsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<XtMessageEventsCollection>
	{
		public void TestLoadEvents_Successfully_ShowAllxTEventsLogForRelatedMsg()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = "KRC";
			interchange.EI_XTInternalMsgID = 101;
			interchange.EI_InterchangeNum = "1111";
			var interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_ApplicationCode = "KRC";
			interchange2.EI_XTInternalMsgID = 102;
			interchange2.EI_InterchangeNum = "1112";
			Factory.Save();

			var mockClient = CreateMockedClient();
			interchange.EI_ShowAllxTEventsLog = true;
			interchange.EI_ShowRelatedxTEventsLog = true;
			var eventCollection = new XtMessageEventsCollection(interchange);
			eventCollection.LoadCollection(mockClient);

			CombineAssertions(() =>
			{
				AssertEquals(5, eventCollection.Count);
				AssertXtMessageEvent(eventCollection[0], 1, 1, "Received from application: test logtext11", "101", "1111", interchange.PK);
				AssertXtMessageEvent(eventCollection[1], 2, 55, "Contract found using routing: test logtext155", "101", "1111", interchange.PK);
				AssertXtMessageEvent(eventCollection[2], 3, 2, "Sent to application: test logtext22", "102", "1112", interchange2.PK);
				AssertXtMessageEvent(eventCollection[3], 4, 23, "Moved to archive database: test logtext123", "101", "1111", interchange.PK);
				AssertXtMessageEvent(eventCollection[4], 5, 23, "Moved to archive database: test logtext223", "102", "1112", interchange2.PK);
			});
		}

		public void TestLoadEvents_Successfully_ShowAllxTEventsLogForCurrentMsg()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = "KRC";
			interchange.EI_XTInternalMsgID = 101;
			interchange.EI_InterchangeNum = "1111";
			var interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_ApplicationCode = "KRC";
			interchange2.EI_XTInternalMsgID = 102;
			interchange2.EI_InterchangeNum = "1112";
			Factory.Save();

			var mockClient = CreateMockedClient();

			interchange.EI_ShowAllxTEventsLog = true;
			interchange.EI_ShowRelatedxTEventsLog = false;
			var eventCollection = new XtMessageEventsCollection(interchange);
			eventCollection.LoadCollection(mockClient);

			CombineAssertions(() =>
			{
				AssertEquals(3, eventCollection.Count);
				AssertXtMessageEvent(eventCollection[0], 1, 1, "Received from application: test logtext11", "101", "1111", interchange.PK);
				AssertXtMessageEvent(eventCollection[1], 2, 55, "Contract found using routing: test logtext155", "101", "1111", interchange.PK);
				AssertXtMessageEvent(eventCollection[2], 3, 23, "Moved to archive database: test logtext123", "101", "1111", interchange.PK);
			});
		}

		public void TestLoadEvents_Successfully_ShowKeyxTEventsLogForRelatedMsg()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = "KRC";
			interchange.EI_XTInternalMsgID = 101;
			interchange.EI_InterchangeNum = "1111";
			var interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_ApplicationCode = "KRC";
			interchange2.EI_XTInternalMsgID = 102;
			interchange2.EI_InterchangeNum = "1112";
			Factory.Save();

			var mockedClient = CreateMockedClient();
			interchange.EI_ShowAllxTEventsLog = false;
			interchange.EI_ShowRelatedxTEventsLog = true;
			var eventCollection = new XtMessageEventsCollection(interchange);
			eventCollection.LoadCollection(mockedClient);

			CombineAssertions(() =>
			{
				AssertEquals(2, eventCollection.Count);
				AssertXtMessageEvent(eventCollection[0], 1, 1, "Received from application: test logtext11", "101", "1111", interchange.PK);
				AssertXtMessageEvent(eventCollection[1], 2, 2, "Sent to application: test logtext22", "102", "1112", interchange2.PK);
			});
		}

		public void TestLoadEvents_Successfully_ShowKeyxTEventsLogForCurrentMsg()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = "KRC";
			interchange.EI_XTInternalMsgID = 101;
			interchange.EI_InterchangeNum = "1111";
			var interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_ApplicationCode = "KRC";
			interchange2.EI_XTInternalMsgID = 102;
			interchange2.EI_InterchangeNum = "1112";
			Factory.Save();

			var mockedClient = CreateMockedClient();
			var eventCollection = new XtMessageEventsCollection(interchange);
			eventCollection.LoadCollection(mockedClient);

			CombineAssertions(() =>
			{
				AssertEquals(1, eventCollection.Count);
				AssertXtMessageEvent(eventCollection[0], 1, 1, "Received from application: test logtext11", "101", "1111", interchange.PK);
			});
		}

		public void TestLoadEvents_Cached()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = "KRC";
			interchange.EI_XTInternalMsgID = 101;
			interchange.EI_InterchangeNum = "1111";
			Factory.Save();

			var mockedClient = CreateMockedClient();
			var eventCollection = new XtMessageEventsCollection(interchange);

			CombineAssertions(() =>
			{
				var isKeyCached = Factory.TryGetValueFromCacheOnly("XtMessageEventsCollection_101_N", out IReadOnlyList<IXtMessageEventData> result1);
				AssertEquals("[Before Caching] KEY cached?", false, isKeyCached);

				eventCollection.LoadCollection(mockedClient, false);

				Factory.TryGetValueFromCacheOnly("XtMessageEventsCollection_101_N", out IReadOnlyList<IXtMessageEventData> result2);
				AssertEquals("Cached events count", 3, result2.Count);
				AssertContainsExactElementsInAnyOrder("All LogEvents for 101 interchange cached", new[] { 1, 55, 23 }, result2.Select(x => x.LogEvent));
			});
		}

		public void TestLoadEvents_RelatedMessagesFromCW1()
		{
			var newGuid = ZGuid.NewZGuid();
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = "KRC";
			interchange.EI_XTInternalMsgID = 101;
			interchange.EI_InterchangeNum = "1111";
			interchange.EI_SessionGUID = newGuid;
			var interchange2 = Factory.New<EDIInterchange>();
			interchange2.EI_ApplicationCode = "KRC";
			interchange2.EI_XTInternalMsgID = 102;
			interchange2.EI_InterchangeNum = "1112";
			interchange2.EI_SessionGUID = newGuid;
			Factory.Save();

			interchange.EI_ShowRelatedxTEventsLog = true;
			var mockedClient = CreateMockedClient(true);
			var eventCollection = new XtMessageEventsCollection(interchange);

			CombineAssertions(() =>
			{
				var isKeyCached = Factory.TryGetValueFromCacheOnly("XtMessageEventsCollection_101_102_Y", out IReadOnlyList<IXtMessageEventData> result1);
				AssertEquals("[Before Caching] KEY cached?", false, isKeyCached);
				AssertEquals("Before LoadCollection", 0, eventCollection.Count);

				eventCollection.LoadCollection(mockedClient, false);

				Factory.TryGetValueFromCacheOnly("XtMessageEventsCollection_101_102_Y", out IReadOnlyList<IXtMessageEventData> result2);
				AssertEquals("Cached all events count", 5, result2.Count);
				AssertContainsExactElementsInAnyOrder("All LogEvents for 101 and 102 interchanges cached", new[] { 1, 55, 23, 2, 23 }, result2.Select(x => x.LogEvent));

				AssertEquals("After LoadCollection: only key events of 101 and 102 interchanges", 2, eventCollection.Count);
				AssertXtMessageEvent(eventCollection[0], 1, 1, "Received from application: test logtext11", "101", "1111", interchange.PK);
				AssertXtMessageEvent(eventCollection[1], 2, 2, "Sent to application: test logtext22", "102", "1112", interchange2.PK);
			});
		}

		public void TestLoadEvents_Empty()
		{
			var eventList = new List<IXtMessageEventData>();
			var mockClient = new Mock<IXtMessageEventsReaderClient>();
			mockClient.Setup(c => c.GetMsgEvents(It.IsAny<HashSet<ulong>>(), It.IsAny<bool>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>())).Returns(eventList);
			var eventCollection = new XtMessageEventsCollection(Factory.New<EDIInterchange>());
			eventCollection.LoadCollection(mockClient.Object);

			CombineAssertions(() =>
			{
				AssertEquals(1, eventCollection.Count);
				AssertEquals("No events found on xT server for the Interchange.", eventCollection[0].LogText);
			});
		}

		public void TestLoadEvents_Error()
		{
			var mockClient = new Mock<IXtMessageEventsReaderClient>();
			mockClient.Setup(c => c.GetMsgEvents(It.IsAny<HashSet<ulong>>(), It.IsAny<bool>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>())).Returns(() => throw new Exception());
			var eventCollection = new XtMessageEventsCollection(Factory.New<EDIInterchange>());
			eventCollection.LoadCollection(mockClient.Object);

			CombineAssertions(() =>
			{
				AssertNotNull(eventCollection);
				AssertEquals(1, eventCollection.Count);
				AssertEquals("Connection Error.", eventCollection[0].LogText);
			});
		}

		public void TestNullClient()
		{
			var eventCollection = new XtMessageEventsCollection(Factory.New<EDIInterchange>());
			eventCollection.LoadCollection(null);

			CombineAssertions(() =>
			{
				AssertNotNull(eventCollection);
				AssertEquals(1, eventCollection.Count);
				AssertEquals("Xt Client is null.", eventCollection[0].LogText);
			});
		}

		public void TestAllowNew()
		{
			var collection = new XtMessageEventsCollection(Factory.New<EDIInterchange>());
			AssertEquals("Allow new is false as users should not be able to add", false, collection.AllowNew);
		}

		protected override XtMessageEventsCollection GetCollectionToTest() => new XtMessageEventsCollection(Factory.New<EDIInterchange>());

		protected override BusinessObject GetNewElementToAddToTheCollection() => new XtMessageEvent(null, new XtMessageEventData(1, 101, "{}"));

		IXtMessageEventsReaderClient CreateMockedClient(bool hasRelatedMsgIdFromCW1 = false)
		{
			var mockClient = new Mock<IXtMessageEventsReaderClient>();
			var eventList = new List<IXtMessageEventData>();
			eventList.Add(new XtMessageEventData(1, 101, "{\"logevent\":\"1\",\"time\":\"2023-10-20T12:32:00\",\"logtext\":\"test logtext11\"}"));
			eventList.Add(new XtMessageEventData(2, 101, "{\"logevent\":\"55\",\"time\":\"2023-10-20T12:32:10\",\"logtext\":\"test logtext155\"}"));
			eventList.Add(new XtMessageEventData(3, 101, "{\"logevent\":\"23\",\"time\":\"2023-10-20T12:32:50\",\"logtext\":\"test logtext123\"}"));
			var eventList2 = new List<IXtMessageEventData>();
			eventList2.Add(new XtMessageEventData(1, 102, "{\"logevent\":\"2\",\"time\":\"2023-10-20T12:32:30\",\"logtext\":\"test logtext22\"}"));
			eventList2.Add(new XtMessageEventData(2, 102, "{\"logevent\":\"23\",\"time\":\"2023-10-20T12:32:51\",\"logtext\":\"test logtext223\"}"));
			var allEventList = eventList.Concat(eventList2);

			mockClient.Setup(c => c.GetMsgEvents(It.IsAny<HashSet<ulong>>(), It.IsAny<bool>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.Returns((HashSet<ulong> xtMsgIds, bool isRecursive, DateTime? deadline, CancellationToken? cancellationToken) =>
				{
					return hasRelatedMsgIdFromCW1 ? allEventList.Where(x => xtMsgIds.Contains(x.XtMsgId)).ToList()
							: isRecursive ? allEventList.ToList() : eventList;
				});

			return mockClient.Object;
		}

		void AssertXtMessageEvent(XtMessageEvent msgEvent, int expectedSequence, int expectedEvent, string expectedText, string expectedMsgId, string expectedInterchangeId, ZGuid expectedInterchangeGuid)
		{
			AssertEquals("Sequence", expectedSequence, msgEvent.Sequence);
			AssertEquals("LogEvent", expectedEvent, msgEvent.LogEvent);
			AssertEquals("LogText", expectedText, msgEvent.LogText);
			AssertEquals("XtMsgId", expectedMsgId, msgEvent.XtMsgId);
			AssertEquals("InterchangeId", expectedInterchangeId, msgEvent.InterchangeId);
			AssertEquals("InterchangeGuid", expectedInterchangeGuid, msgEvent.InterchangeGuid);
		}
	}
}
