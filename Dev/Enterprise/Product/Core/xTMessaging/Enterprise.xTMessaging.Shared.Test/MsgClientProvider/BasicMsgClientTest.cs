using System;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using Grpc.Core;
using Moq;
using Xware.Xt.Grpc.Application;

namespace Enterprise.xTMessaging.Shared.Test
{
	sealed class BasicMsgClientTest : TestCaseWithFactory
	{
		public void TestClientShouldNotNeNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new BasicMsgClient(null, TimeSpan.FromSeconds(10), new CancellationToken()));
		}

		public void TestSubmitMsgAsync()
		{
			// Arrange
			var client = TestUtils.GetMoqMsgClient(TestUtils.MsgMethod.SubmitMsgAsync);
			var msgClient = new BasicMsgClient(client, TimeSpan.FromSeconds(10), new CancellationToken());

			// Act & Assert
			var ex = AssertExceptionThrown<MsgServerConnectionException>("SubmitMsgMessage deadline error",
				"Direct xT Client - DeadlineExceeded Error",
				 () => msgClient.SubmitMsgAsync(new SubmitMsgMessage()).GetAwaiter().GetResult());

			var errorMsg = Utils.GetDeadlineExceededErrorMessage(TimeSpan.FromSeconds(10));
			AssertEquals("ErrorDetail", $"SubmitMsgAsync{errorMsg}", ex.ErrorDetail);
			AssertEquals(typeof(RpcException), ex.InnerException.GetType());
			AssertEquals("Status(StatusCode=\"DeadlineExceeded\", Detail=\"SubmitMsgAsync Deadline Exceeded\")", ex.InnerException.Message);
		}

		public void TestWriteMsgDataStream()
		{
			var client = TestUtils.GetMoqMsgClient(TestUtils.MsgMethod.WriteMsgDataStream);
			var msgClient = new BasicMsgClient(client, TimeSpan.FromSeconds(10), new CancellationToken());

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
			var msgClient = new BasicMsgClient(client, TimeSpan.FromSeconds(10), new CancellationToken());

			// Act & Assert
			var ex = AssertExceptionThrown<MsgServerConnectionException>("WaitMsgMessage deadline error",
				"Direct xT Client - DeadlineExceeded Error",
				() => msgClient.WaitMsgAsync(new WaitMsgMessage()).GetAwaiter().GetResult());

			var errorMsg = Utils.GetDeadlineExceededErrorMessage(TimeSpan.FromSeconds(10));
			AssertEquals("ErrorDetail", $"WaitMsgAsync{errorMsg}", ex.ErrorDetail);
			AssertEquals(typeof(RpcException), ex.InnerException.GetType());
			AssertEquals("Status(StatusCode=\"DeadlineExceeded\", Detail=\"WaitMsgAsync Deadline Exceeded\")", ex.InnerException.Message);
		}

		public void TestGetMsgAttributes()
		{
			var client = TestUtils.GetMoqMsgClient(TestUtils.MsgMethod.GetMsgAttributes);
			var msgClient = new BasicMsgClient(client, TimeSpan.FromSeconds(10), new CancellationToken());
			var ex = AssertExceptionThrown<MsgServerConnectionException>("GetMsgAttributes deadline error",
				"Direct xT Client - DeadlineExceeded Error",
				() => msgClient.GetMsgAttributes(new GetMsgAttributesMessage()));
			var errorMsg = Utils.GetDeadlineExceededErrorMessage(TimeSpan.FromSeconds(10));
			AssertEquals("ErrorDetail", $"GetMsgAttributes{errorMsg}", ex.ErrorDetail);
			AssertEquals(typeof(RpcException), ex.InnerException.GetType());
			AssertEquals("Status(StatusCode=\"DeadlineExceeded\", Detail=\"GetMsgAttributes Deadline Exceeded\")", ex.InnerException.Message);
		}

		public void TestGetMsgAttributesList()
		{
			var client = TestUtils.GetMoqMsgClient(TestUtils.MsgMethod.GetMsgAttributesList);
			var msgClient = new BasicMsgClient(client, TimeSpan.FromSeconds(10), new CancellationToken());
			var ex = AssertExceptionThrown<MsgServerConnectionException>("GetMsgAttributesList deadline error",
				"Direct xT Client - DeadlineExceeded Error",
				() => msgClient.GetMsgAttributesList(new GetMsgAttributesListMessage()));
			var errorMsg = Utils.GetDeadlineExceededErrorMessage(TimeSpan.FromSeconds(10));
			AssertEquals("ErrorDetail", $"GetMsgAttributesList{errorMsg}", ex.ErrorDetail);
			AssertEquals(typeof(RpcException), ex.InnerException.GetType());
			AssertEquals("Status(StatusCode=\"DeadlineExceeded\", Detail=\"GetMsgAttributesList Deadline Exceeded\")", ex.InnerException.Message);
		}

		public void TestGetMsgData()
		{
			var client = TestUtils.GetMoqMsgClient(TestUtils.MsgMethod.GetMsgData);
			var msgClient = new BasicMsgClient(client, TimeSpan.FromSeconds(10), new CancellationToken());

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
			var msgClient = new BasicMsgClient(client, TimeSpan.FromSeconds(10), new CancellationToken());
			var ex = AssertExceptionThrown<MsgServerConnectionException>("MsgSetStatus deadline error",
				"Direct xT Client - DeadlineExceeded Error",
				() => msgClient.MsgSetStatus(new MsgSetStatusMessage()));
			var errorMsg = Utils.GetDeadlineExceededErrorMessage(TimeSpan.FromSeconds(10));
			AssertEquals("ErrorDetail", $"MsgSetStatus{errorMsg}", ex.ErrorDetail);
			AssertEquals(typeof(RpcException), ex.InnerException.GetType());
			AssertEquals("Status(StatusCode=\"DeadlineExceeded\", Detail=\"MsgSetStatus Deadline Exceeded\")", ex.InnerException.Message);
		}

		public void TestGetMsgState_Success()
		{
			var mockClient = new Mock<Msg.MsgClient>();
			var reply = new GetMsgAttributesReply
			{
				Errorcode = 0,
				Id = new MsgIdUri { Msgid = 23456 }
			};
			reply.Msgattr.Add("state", "1026");
			reply.Msgattr.Add("statename", "ST_TO_APP");
			mockClient.Setup(m => m.GetMsgAttributes(It.IsAny<GetMsgAttributesMessage>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
				.Returns(reply);

			var result = new BasicMsgClient(mockClient.Object, TimeSpan.FromSeconds(10), CancellationToken.None).GetMsgState(0);

			AssertEquals("ST_TO_APP", result.stateName);
			AssertEquals("1026", result.stateCode);
		}

		public void TestGetMsgState_ErrorCode()
		{
			var mockClient = new Mock<Msg.MsgClient>();
			var reply = new GetMsgAttributesReply
			{
				Errorcode = 16,
				Id = new MsgIdUri { Msgid = 23456 },
			};
			mockClient.Setup(m => m.GetMsgAttributes(It.IsAny<GetMsgAttributesMessage>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
				.Returns(reply);

			var result = new BasicMsgClient(mockClient.Object, TimeSpan.FromSeconds(10), CancellationToken.None).GetMsgState(0);

			AssertEquals("Unexpected Error code returned", result.stateName);
			AssertEquals("0", result.stateCode);
		}
	}
}
