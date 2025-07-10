using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Shared;
using Enterprise.xTMessaging.Shared.Test;
using Grpc.Core;
using Moq;
using Xware.Xt.Grpc.Application;
using SharedUtils = Enterprise.xTMessaging.Shared.Utils;

namespace Enterprise.xTMessaging.Business.Test
{
	sealed class MsgClientWithDeadlineTest : TestCaseWithFactory
	{
		public void TestClientShouldNotNeNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new MsgClientWithDeadline(null, new CancellationToken(), TimeSpan.FromSeconds(10)));
		}

		public void TestSubmitMsgAsync()
		{
			// Arrange
			var client = TestUtils.GetMoqMsgClient(TestUtils.MsgMethod.SubmitMsgAsync);
			var msgClient = new MsgClientWithDeadline(client, new CancellationToken(), TimeSpan.FromSeconds(10));

			// Act & Assert
			var ex = AssertExceptionThrown<MsgServerConnectionException>("SubmitMsgMessage deadline error",
				"Direct xT Client - DeadlineExceeded Error",
				() => msgClient.SubmitMsgAsync(new SubmitMsgMessage()).GetAwaiter().GetResult());

			var errorMsg = SharedUtils.GetDeadlineExceededErrorMessage(TimeSpan.FromSeconds(10));
			AssertEquals("ErrorDetail", $"SubmitMsgAsync{errorMsg}", ex.ErrorDetail);
			AssertEquals(typeof(RpcException), ex.InnerException.GetType());
			AssertEquals("Status(StatusCode=\"DeadlineExceeded\", Detail=\"SubmitMsgAsync Deadline Exceeded\")", ex.InnerException.Message);
		}

		public void TestWriteMsgDataStream()
		{
			var client = TestUtils.GetMoqMsgClient(TestUtils.MsgMethod.WriteMsgDataStream);
			var msgClient = new MsgClientWithDeadline(client, new CancellationToken(), TimeSpan.FromSeconds(10));

			CombineAssertions(() =>
			{
				var ex = AssertExceptionThrown<AggregateException>("WriteMsgDataStream deadline error",
					"One or more errors occurred.",
					() => _ = msgClient.WriteMsgDataStream().ResponseAsync.Result.Ref);

				AssertEquals(typeof(RpcException), ex.InnerException.GetType());
				AssertEquals("Status(StatusCode=\"DeadlineExceeded\", Detail=\"WriteMsgDataStream Deadline Exceeded\")", ex.InnerException.Message);
			});
		}

		public void TestWaitMsgAsync()
		{
			// Arrange
			var client = TestUtils.GetMoqMsgClient(TestUtils.MsgMethod.WaitMsgAsync);
			var msgClient = new MsgClientWithDeadline(client, new CancellationToken(), TimeSpan.FromSeconds(10));

			// Act & Assert
			var ex = AssertExceptionThrown<MsgServerConnectionException>("WaitMsgMessage deadline error",
				"Direct xT Client - DeadlineExceeded Error",
				() => msgClient.WaitMsgAsync(new WaitMsgMessage()).GetAwaiter().GetResult());

			var errorMsg = SharedUtils.GetDeadlineExceededErrorMessage(TimeSpan.FromSeconds(10));
			AssertEquals("ErrorDetail", $"WaitMsgAsync{errorMsg}", ex.ErrorDetail);
			AssertEquals(typeof(RpcException), ex.InnerException.GetType());
			AssertEquals("Status(StatusCode=\"DeadlineExceeded\", Detail=\"WaitMsgAsync Deadline Exceeded\")", ex.InnerException.Message);
		}

		public void TestGetMsgAttributes()
		{
			var client = TestUtils.GetMoqMsgClient(TestUtils.MsgMethod.GetMsgAttributes);
			var msgClient = new MsgClientWithDeadline(client, new CancellationToken(), TimeSpan.FromSeconds(10));
			var ex = AssertExceptionThrown<MsgServerConnectionException>("GetMsgAttributes deadline error",
				"Direct xT Client - DeadlineExceeded Error",
				() => msgClient.GetMsgAttributes(new GetMsgAttributesMessage()));
			var errorMsg = SharedUtils.GetDeadlineExceededErrorMessage(TimeSpan.FromSeconds(10));
			AssertEquals("ErrorDetail", $"GetMsgAttributes{errorMsg}", ex.ErrorDetail);
			AssertEquals(typeof(RpcException), ex.InnerException.GetType());
			AssertEquals("Status(StatusCode=\"DeadlineExceeded\", Detail=\"GetMsgAttributes Deadline Exceeded\")", ex.InnerException.Message);
		}

		public void TestGetMsgAttributesList()
		{
			var client = TestUtils.GetMoqMsgClient(TestUtils.MsgMethod.GetMsgAttributesList);
			var msgClient = new MsgClientWithDeadline(client, new CancellationToken(), TimeSpan.FromSeconds(10));
			var ex = AssertExceptionThrown<MsgServerConnectionException>("GetMsgAttributesList deadline error",
				"Direct xT Client - DeadlineExceeded Error",
				() => msgClient.GetMsgAttributesList(new GetMsgAttributesListMessage()));
			var errorMsg = SharedUtils.GetDeadlineExceededErrorMessage(TimeSpan.FromSeconds(10));
			AssertEquals("ErrorDetail", $"GetMsgAttributesList{errorMsg}", ex.ErrorDetail);
			AssertEquals(typeof(RpcException), ex.InnerException.GetType());
			AssertEquals("Status(StatusCode=\"DeadlineExceeded\", Detail=\"GetMsgAttributesList Deadline Exceeded\")", ex.InnerException.Message);
		}

		public void TestGetMsgData()
		{
			var client = TestUtils.GetMoqMsgClient(TestUtils.MsgMethod.GetMsgData);
			var msgClient = new MsgClientWithDeadline(client, new CancellationToken(), TimeSpan.FromSeconds(10));

			CombineAssertions(() =>
			{
				var ex = AssertExceptionThrown<AggregateException>("GetMsgData deadline error",
					"One or more errors occurred.",
					() =>
					{
						var responseStream = msgClient.GetMsgData(new GetMsgDataMessage()).ResponseStream;
						var result = responseStream.MoveNext().Result;
					});
				AssertEquals(typeof(RpcException), ex.InnerException.GetType());
				AssertEquals("ErrorDetail", "Status(StatusCode=\"DeadlineExceeded\", Detail=\"GetMsgData Deadline Exceeded\")", ex.InnerException.Message);
			});
		}

		public void TestMsgSetStatus()
		{
			var client = TestUtils.GetMoqMsgClient(TestUtils.MsgMethod.MsgSetStatus);
			var msgClient = new MsgClientWithDeadline(client, new CancellationToken(), TimeSpan.FromSeconds(10));
			var ex = AssertExceptionThrown<MsgServerConnectionException>("MsgSetStatus deadline error",
				"Direct xT Client - DeadlineExceeded Error",
				() => msgClient.MsgSetStatus(new MsgSetStatusMessage()));
			var errorMsg = SharedUtils.GetDeadlineExceededErrorMessage(TimeSpan.FromSeconds(10));
			AssertEquals("ErrorDetail", $"MsgSetStatus{errorMsg}", ex.ErrorDetail);
			AssertEquals(typeof(RpcException), ex.InnerException.GetType());
			AssertEquals("Status(StatusCode=\"DeadlineExceeded\", Detail=\"MsgSetStatus Deadline Exceeded\")", ex.InnerException.Message);
		}

		public void TestGetRelatedMessageId()
		{
			var mockClient = new Mock<Msg.MsgClient>();
			var reply = new GetMsgAttributesReply
			{
				Errorcode = 0,
				Id = new MsgIdUri { Msgid = 23456 }
			};
			reply.Msgattr.Add("refcreated", "101");
			reply.Msgattr.Add("refcreator", "102");
			reply.Msgattr.Add("reftocontract", "103");
			reply.Msgattr.Add("refinternalsender", "104");
			reply.Msgattr.Add("refexternal", "105");
			reply.Msgattr.Add("refreply", "107");
			mockClient.Setup(m => m.GetMsgAttributesList(It.IsAny<GetMsgAttributesListMessage>(), It.IsAny<Metadata>(),
					It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
				.Returns(
					new AsyncServerStreamingCall<GetMsgAttributesReply>(
						new TestGetMsgAttributesReplyStreamReader(new List<GetMsgAttributesReply> { reply }), null, null, null, null)
				);

			var result = new MsgClientWithDeadline(mockClient.Object, CancellationToken.None, TimeSpan.FromSeconds(10)).GetRelatedMessageIdIncludeSelf(new HashSet<ulong> { 23456 });
			AssertContainsExactElementsInAnyOrder(new ulong[] { 23456, 101, 102, 103, 104, 105, 107 }, result);
		}

		public void TestGetRelatedMessageId_Error()
		{
			var mockClient = new Mock<Msg.MsgClient>();
			var reply = new GetMsgAttributesReply
			{
				Errorcode = 16,
				Id = new MsgIdUri { Msgid = 23456 },
			};						  
			mockClient.Setup(m => m.GetMsgAttributesList(It.IsAny<GetMsgAttributesListMessage>(), It.IsAny<Metadata>(),
					It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
				.Returns(
					new AsyncServerStreamingCall<GetMsgAttributesReply>(
						new TestGetMsgAttributesReplyStreamReader(new List<GetMsgAttributesReply> { reply }), null, null, null, null)
				);

			var result = new MsgClientWithDeadline(mockClient.Object, CancellationToken.None, TimeSpan.FromSeconds(10)).GetRelatedMessageIdIncludeSelf(new HashSet<ulong> { 23456 });
			AssertEquals(1, result.Count);
		}

		public void TestGetAllRelatedMsgEvents_Recursive()
		{
			var mockClient = CreateMockedClient();
			var result = new MsgClientWithDeadline(mockClient, CancellationToken.None, TimeSpan.FromSeconds(10)).GetMsgEvents(new HashSet<ulong> { 100 }, true);

			AssertEquals(5, result.Count);
			AssertXtMessageEvent(result,
				new[]
				{
					new XtMessageEventData(1, 100, "{\"logevent\":\"1\",\"time\":\"2023-10-20T12:32:21\",\"logtext\":\"\"}"),
					new XtMessageEventData(2, 100, "{\"logevent\":\"2\",\"time\":\"2023-10-20T12:32:21\",\"logtext\":\"logtext2\"}")
				});

			AssertXtMessageEvent(result,
				new[] { new XtMessageEventData(1, 101, "{\"logevent\":\"3\",\"time\":\"2023-10-20T12:32:21\",\"logtext\":\"logtext3\"}") });

			AssertXtMessageEvent(result,
				new[] { new XtMessageEventData(1, 102, "{\"logevent\":\"0\",\"time\":\"2023-10-20T12:32:21\",\"logtext\":\"Permission denied\"}") });

			AssertXtMessageEvent(result,
				new[] { new XtMessageEventData(1, 103, "{\"logevent\":\"0\",\"time\":\"2023-10-20T12:32:21\",\"logtext\":\"Permission denied\"}") });
		}

		public void TestGetAllRelatedMsgEvents_NoRecursive()
		{
			var mockClient = CreateMockedClient();
			var result = new MsgClientWithDeadline(mockClient, CancellationToken.None, TimeSpan.FromSeconds(10)).GetMsgEvents(new HashSet<ulong> { 100 }, false);

			AssertEquals(2, result.Count);
			AssertXtMessageEvent(result,
				new[]
				{
					new XtMessageEventData(1, 100, "{\"logevent\":\"1\",\"time\":\"2023-10-20T12:32:21\",\"logtext\":\"\"}"),
					new XtMessageEventData(2, 100, "{\"logevent\":\"2\",\"time\":\"2023-10-20T12:32:21\",\"logtext\":\"logtext2\"}")
				});
		}

		public void TestGetAllRelatedMsgEventsForMultipleMsg_NoRecursive()
		{
			var mockClient = CreateMockedClient();
			var result = new MsgClientWithDeadline(mockClient, CancellationToken.None, TimeSpan.FromSeconds(10)).GetMsgEvents(new HashSet<ulong> { 100, 101 }, false);

			AssertEquals(3, result.Count);
			AssertXtMessageEvent(result,
				new[]
				{
					new XtMessageEventData(1, 100, "{\"logevent\":\"1\",\"time\":\"2023-10-20T12:32:21\",\"logtext\":\"\"}"),
					new XtMessageEventData(2, 100, "{\"logevent\":\"2\",\"time\":\"2023-10-20T12:32:21\",\"logtext\":\"logtext2\"}")
				});

			AssertXtMessageEvent(result,
				new[]
				{
					new XtMessageEventData(1, 101, "{\"logevent\":\"3\",\"time\":\"2023-10-20T12:32:21\",\"logtext\":\"logtext3\"}")
				});
		}

		public void TestGetAllRelatedMsgEventsForMultipleMsg_Recursive()
		{
			var mockClient = CreateMockedClient();
			var result = new MsgClientWithDeadline(mockClient, CancellationToken.None, TimeSpan.FromSeconds(10)).GetMsgEvents(new HashSet<ulong> { 100, 105 }, true);

			AssertEquals(6, result.Count);

			AssertXtMessageEvent(result,
				new[]
				{
					new XtMessageEventData(1, 100, "{\"logevent\":\"1\",\"time\":\"2023-10-20T12:32:21\",\"logtext\":\"\"}"),
					new XtMessageEventData(2, 100, "{\"logevent\":\"2\",\"time\":\"2023-10-20T12:32:21\",\"logtext\":\"logtext2\"}")
				});

			AssertXtMessageEvent(result,
				new[] { new XtMessageEventData(1, 101, "{\"logevent\":\"3\",\"time\":\"2023-10-20T12:32:21\",\"logtext\":\"logtext3\"}") });
			AssertXtMessageEvent(result,
				new[] { new XtMessageEventData(1, 102, "{\"logevent\":\"0\",\"time\":\"2023-10-20T12:32:21\",\"logtext\":\"Permission denied\"}") });
			AssertXtMessageEvent(result,
				new[] { new XtMessageEventData(1, 103, "{\"logevent\":\"0\",\"time\":\"2023-10-20T12:32:21\",\"logtext\":\"Permission denied\"}") });
			AssertXtMessageEvent(result,
				new[] { new XtMessageEventData(1, 105, "{\"logevent\":\"0\",\"time\":\"2023-10-20T12:32:21\",\"logtext\":\"Permission denied\"}") });
		}

		Msg.MsgClient CreateMockedClient()
		{
			var mockClient = new Mock<Msg.MsgClient>();
			var msgAttributesReply1 = new GetMsgAttributesReply
			{
				Errorcode = 0,
				Id = new MsgIdUri { Msgid = 100 }
			};
			msgAttributesReply1.Msgattr.Add("refcreated", "101");
			msgAttributesReply1.Msgattr.Add("refexternal", "102");
			var msgAttributesReply2 = new GetMsgAttributesReply
			{
				Errorcode = 0,
				Id = new MsgIdUri { Msgid = 101 }
			};
			msgAttributesReply2.Msgattr.Add("reftocontract", "103");
			msgAttributesReply2.Msgattr.Add("refcreator", "100");
			var msgAttributesReply3 = new GetMsgAttributesReply
			{
				Errorcode = 16,
				Id = new MsgIdUri { Msgid = 102 }
			};
			var msgAttributesReply4 = new GetMsgAttributesReply
			{
				Errorcode = 16,
				Id = new MsgIdUri { Msgid = 103 }
			};
			var msgAttributesReply5 = new GetMsgAttributesReply
			{
				Errorcode = 16,
				Id = new MsgIdUri { Msgid = 105 }
			};

			mockClient.Setup(m => m.GetMsgAttributesList(It.IsAny<GetMsgAttributesListMessage>(), It.IsAny<Metadata>(),
					It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
				.Returns((GetMsgAttributesListMessage getMsgAttributesListMessage, Metadata _, DateTime? _, CancellationToken _) =>
					{
						var list = new List<GetMsgAttributesReply>();
						if (getMsgAttributesListMessage.Ids.Any(id => id.Msgid == 100))
						{
							list.Add(msgAttributesReply1);
						}
						if (getMsgAttributesListMessage.Ids.Any(id => id.Msgid == 101))
						{
							list.Add(msgAttributesReply2);
						}
						if (getMsgAttributesListMessage.Ids.Any(id => id.Msgid == 102))
						{
							list.Add(msgAttributesReply3);
						}
						if (getMsgAttributesListMessage.Ids.Any(id => id.Msgid == 103))
						{
							list.Add(msgAttributesReply4);
						}
						if (getMsgAttributesListMessage.Ids.Any(id => id.Msgid == 104))
						{
							list.Add(msgAttributesReply5);
						}

						return new AsyncServerStreamingCall<GetMsgAttributesReply>(new TestGetMsgAttributesReplyStreamReader(list), null, null,
							null, null);
					}
				);

			var e1 = new MsgEvent();
			e1.Eventattrs.Add("logevent", "1");
			e1.Eventattrs.Add("logtext", "");
			e1.Eventattrs.Add("time", ZDateTime.Now.AddMinutes(-1).ToISO8601String());
			var e2 = new MsgEvent();
			e2.Eventattrs.Add("logevent", "2");
			e2.Eventattrs.Add("logtext", "logtext2");
			e2.Eventattrs.Add("time", ZDateTime.Now.ToISO8601String());
			var msgEventsReply1 = new GetMsgEventsReply
			{
				Errorcode = 0,
				Id = new MsgIdUri { Msgid = 100 },
				Events = { e1, e2 }
			};
			var e3 = new MsgEvent();
			e3.Eventattrs.Add("logevent", "3");
			e3.Eventattrs.Add("logtext", "logtext3");
			e3.Eventattrs.Add("time", ZDateTime.Now.AddMinutes(1).ToISO8601String());
			var msgEventsReply2 = new GetMsgEventsReply
			{
				Errorcode = 0,
				Id = new MsgIdUri { Msgid = 101 },
				Events = { e3 }
			};
			var msgEventsReply3 = new GetMsgEventsReply
			{
				Errorcode = 24,
				Id = new MsgIdUri { Msgid = 102 },
				Events = { e3 }
			};
			mockClient.Setup(m => m.GetMsgEvents(It.IsAny<GetMsgEventsMessage>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
				.Returns((GetMsgEventsMessage msgAttributesMessage, Metadata header, DateTime? deadline, CancellationToken cancellationToken) =>
				{
					var id = msgAttributesMessage.Id.Msgid;
					switch (id)
					{
						case 100:
							return msgEventsReply1;
						case 101:
							return msgEventsReply2;
						default:
							return msgEventsReply3;
					}
				});

			return mockClient.Object;
		}

		void AssertXtMessageEvent(IReadOnlyList<IXtMessageEventData> msgEvents, XtMessageEventData[] expectedList)
		{
			var result = msgEvents.Where(x => x.XtMsgId == expectedList.FirstOrDefault().XtMsgId).ToArray();
			for (var i = 0; i < expectedList.Length; i++)
			{
				AssertEquals($"msgEvents[{i}] EventIndex", expectedList[i].EventIndex, result[i].EventIndex);
				AssertEquals($"msgEvents[{i}] LogEvent", expectedList[i].LogEvent, result[i].LogEvent);
				AssertEquals($"msgEvents[{i}] LogText", expectedList[i].LogText, result[i].LogText);
			}
		}

		public void TestGetMsgEvents_Success()
		{
			var mockClient = new Mock<Msg.MsgClient>();
			var e1 = new MsgEvent();
			e1.Eventattrs.Add("logevent", "1");
			e1.Eventattrs.Add("logtext", "");
			e1.Eventattrs.Add("time", ZDateTime.Now.AddMinutes(-1).ToISO8601String());
			var e2 = new MsgEvent();
			e2.Eventattrs.Add("logevent", "2");
			e2.Eventattrs.Add("logtext", "logtext2");
			e2.Eventattrs.Add("time", ZDateTime.Now.ToISO8601String());
			var e3 = new MsgEvent();
			e3.Eventattrs.Add("logevent", "3");
			e3.Eventattrs.Add("logtext", "logtext3");
			e3.Eventattrs.Add("time", ZDateTime.Now.AddMinutes(1).ToISO8601String());
			var reply = new GetMsgEventsReply
			{
				Errorcode = 0,
				Id = new MsgIdUri { Msgid = 23456 },
				Events = { e1, e2, e3 }
			};
			mockClient.Setup(m => m.GetMsgEvents(It.IsAny<GetMsgEventsMessage>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
				.Returns(reply);

			var result = new MsgClientWithDeadline(mockClient.Object, CancellationToken.None, TimeSpan.FromSeconds(10)).GetMsgEvents(0);

			AssertEquals(3, result.Count);
			AssertEquals("Acknowledged by application: logtext3", result[2].LogText);
		}

		public void TestGetMsgEvents_ErrorCode()
		{
			var mockClient = new Mock<Msg.MsgClient>();
			var reply = new GetMsgEventsReply
			{
				Errorcode = 16,
				Id = new MsgIdUri { Msgid = 23456 },
			};
			mockClient.Setup(m => m.GetMsgEvents(It.IsAny<GetMsgEventsMessage>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
				.Returns(reply);

			var result = new MsgClientWithDeadline(mockClient.Object, CancellationToken.None, TimeSpan.FromSeconds(10)).GetMsgEvents(0);

			AssertEquals(1, result.Count);
			AssertEquals("Sequence error is unlocked", result[0].LogText);
		}

		public void TestGetMsgEvents_RpcException()
		{
			AssertGetMsgEvents_Error(new RpcException(new Status(StatusCode.DeadlineExceeded, "DeadlineExceeded.")), "DeadlineExceeded:DeadlineExceeded.");
		}

		public void TestGetMsgEvents_OtherError()
		{
			AssertGetMsgEvents_Error(new Exception("Random exception."), "Random exception.");
		}

		void AssertGetMsgEvents_Error(Exception ex, string expectationReason)
		{
			var mockClient = new Mock<Msg.MsgClient>();
			mockClient.Setup(m => m.GetMsgEvents(It.IsAny<GetMsgEventsMessage>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
				.Throws(ex);

			var result = new MsgClientWithDeadline(mockClient.Object, CancellationToken.None, TimeSpan.FromSeconds(10)).GetMsgEvents(0);

			AssertEquals(expectationReason, result[0].LogText);
		}
	}
	public class TestGetMsgAttributesReplyStreamReader : IAsyncStreamReader<GetMsgAttributesReply>
	{
		readonly IEnumerator<GetMsgAttributesReply> enumerator;

		public TestGetMsgAttributesReplyStreamReader(IEnumerable<GetMsgAttributesReply> lst)
		{
			enumerator = lst.GetEnumerator();
		}

		public GetMsgAttributesReply Current => enumerator.Current;

		public Task<bool> MoveNext(CancellationToken cancellationToken)
		{
			return Task.FromResult(enumerator.MoveNext());
		}
	}
}
