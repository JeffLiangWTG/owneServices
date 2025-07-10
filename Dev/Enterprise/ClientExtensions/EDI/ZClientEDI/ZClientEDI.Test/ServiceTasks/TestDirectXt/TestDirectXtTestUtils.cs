using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.ServiceTasks.TestDirectXt;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.ServiceTasks;
using Enterprise.xTMessaging.Shared;
using Enterprise.xTMessaging.Shared.Test;
using Google.Protobuf;
using Grpc.Core;
using Moq;
using Newtonsoft.Json;
using Xware.Xt.Grpc.Application;
using Xware.Xt.Grpc.Config;

namespace ZClientEDI.Test.ServiceTasks.TestDirectXt
{
	public static class TestDirectXtTestUtils
	{
		public static IMsgClientProvider GetMockedMsgClientProviderOutbound(IEnumerable<SubmitMsgReply> submitReplysSetup = null)
		{
			var callRequestStream = new Mock<IClientStreamWriter<ByteChunk>>();
			callRequestStream.Setup(m => m.WriteAsync(It.IsAny<ByteChunk>())).Returns(Task.CompletedTask);
			callRequestStream.Setup(m => m.CompleteAsync()).Returns(Task.CompletedTask);

			var callResponseAsync = Task.FromResult(new WriteMsgDataStreamReply() { Ref = "Ref" });

			var asyncCall = new AsyncClientStreamingCall<ByteChunk, WriteMsgDataStreamReply>(callRequestStream.Object, callResponseAsync, null, null, null, null);

			var msgClientMock = new Mock<IMsgClient>();
			if (submitReplysSetup == null)
			{
				msgClientMock.Setup(m => m.SubmitMsgAsync(It.IsAny<SubmitMsgMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
					.Returns((SubmitMsgMessage msg, DateTime? datetime, CancellationToken? token) =>
					{
						var submitMsgReply = new SubmitMsgReply() { Errorcode = (int)ErrorCode.ErrOk };
						submitMsgReply.Ids.Add(new MsgIdUri() { Msgid = 12345UL });
						return Task.FromResult(submitMsgReply);
					});
			}
			else
			{
				var msgId = 123UL;
				var returnSequence = msgClientMock.SetupSequence(m => m.SubmitMsgAsync(It.IsAny<SubmitMsgMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()));
				foreach (var reply in submitReplysSetup)
				{
					reply.Ids.Add(new MsgIdUri() { Msgid = msgId });
					msgId++;
					returnSequence.Returns(Task.FromResult(reply));
				}
			}
			msgClientMock.Setup(m => m.WriteMsgDataStream(null, null)).Returns(asyncCall);
			msgClientMock.Setup(m => m.WriteMsgDataStream(It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(asyncCall);

			msgClientMock.Setup(x => x.StartTransactionAsync(It.IsAny<StartTransactionMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.ReturnsAsync(new StartTransactionReply());

			msgClientMock.Setup(x => x.EndTransactionAsync(It.IsAny<EndTransactionMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.ReturnsAsync(new EndTransactionReply());

			var clientProvider = new Mock<IMsgClientProvider>();
			clientProvider.Setup(m => m.MsgClient).Returns(msgClientMock.Object);
			return clientProvider.Object;
		}

		public static (IMsgClientProvider, Dictionary<ulong, MsgStatusCommand>) GetMockedMsgClientProviderInbound(IDictionary<ulong, TestIncomingMessagePreparation> incomingMessagePreparations)
		{
			var ackTracker = new Dictionary<ulong, MsgStatusCommand>();
			var received = new List<ulong>();

			var msgClientMock = new Mock<IMsgClient>();
			msgClientMock
				.Setup(m => m.MsgSetStatus(It.IsAny<MsgSetStatusMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.Returns((MsgSetStatusMessage req, DateTime? datetime, CancellationToken? cancellationToken) =>
				{
					ackTracker[req.Id.Msgid] = req.Cmd;
					return new MsgSetStatusReply() { Errorcode = (int)incomingMessagePreparations[req.Id.Msgid].AckResponse };
				});

			msgClientMock.Setup(m => m.GetMsgData(It.IsAny<GetMsgDataMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.Returns((GetMsgDataMessage req, DateTime? datetime, CancellationToken? cancellationToken) =>
				{
					var asyncCall = new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(incomingMessagePreparations[req.Id.Msgid].MsgBody), null, null, null, () => { });
					return asyncCall;
				});

			msgClientMock
				.Setup(m => m.GetMsgAttributes(It.IsAny<GetMsgAttributesMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.Returns((GetMsgAttributesMessage req, DateTime? datetime, CancellationToken? cancellationToken) =>
				{
					var reply = new GetMsgAttributesReply();
					var message = incomingMessagePreparations[req.Id.Msgid];
					if (!string.IsNullOrEmpty(message.TestInstruction))
					{
						reply.Msgattr.Add(TestInstructionValues.Key, message.TestInstruction);
					}
					foreach (var kvp in message?.MetaData ?? new Dictionary<string, string>())
					{
						reply.Msgattr.Add(kvp.Key, kvp.Value);
					}
					return reply;
				});

			msgClientMock
				.Setup(m => m.WaitMsgAsync(It.IsAny<WaitMsgMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.Returns((WaitMsgMessage msg, DateTime? datetime, CancellationToken? cancellationToken) =>
				{
					var result = new WaitMsgReply();
					var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(msg.ToString());

					var toObjMatched = !(parameters?.ContainsKey("toobj") ?? false)
						|| parameters["toobj"].Trim() == GetTestConfiguration().Application.URI;

					if (toObjMatched)
					{
						foreach (var key in incomingMessagePreparations.Keys)
						{
							if (!string.IsNullOrEmpty(incomingMessagePreparations[key].MsgBody) && !received.Contains(key))
							{
								result.Ids.Add(new MsgIdObj() { Id = new MsgIdUri() { Msgid = key } });
								received.Add(key);
							}
						}
					}

					return Task.FromResult(result);
				});

			var clientProvider = new Mock<IMsgClientProvider>();
			clientProvider.Setup(m => m.MsgClient).Returns(msgClientMock.Object);
			clientProvider.Setup(m => m.XtToObjFilter).Returns(GetTestConfiguration().Application.URI);

			return (clientProvider.Object, ackTracker);
		}

		public static EDIInterchange CreateInterchange(BusinessObjectFactory factory, string status, bool isActive, string receiveTransmit, string transportType)
		{
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = "EDI";
			interchange.EI_Status = status;
			interchange.EI_IsActive = isActive;
			interchange.EI_ReceiveTransmit = receiveTransmit;
			interchange.EI_TransportType = transportType;
			interchange.EI_From = "Any";
			interchange.EI_To = "AnyOther";
			interchange.EI_SessionGUID = ZGuid.NewZGuid();
			return interchange;
		}

		internal static Configuration GetTestConfiguration()
		{
			var config = new Configuration()
			{
				Application = new Application
				{
					Password = "password",
					URI = "xt-application:{cfb01c8e-f063-4e3a-bc3e-7e274f84e66r}"
				},
				Connect = "127.0.0.1:61001",
				CA = "-----BEGIN CERTIFICATE-----\nMIIETjCCAragAwIBAgIJANaqHXch0txmMA0GCSqGSIb3DQEBCwUAMA0xCzAJBgNV\nBAYTAkFVMCAXDTIxMDQyMzAwNTUyN1oYDzIwMjQwNDIzMDAwMDAwWjANMQswCQYD\nVQQGEwJBVTCCAaIwDQYJKoZIhvcNAQEBBQADggGPADCCAYoCggGBAJ0+AyGGGDfU\nbJmCQszgnfJC9MmRm6kRl970hmVgl1UAJ8Oa10T/s+JA/JAhyo48Rvl0Ukh9JD5Z\nC6VayqyP8I1SdREFUXBhjChC6xt+LiA6/ZFs1Z3j5eoWH6S/uS5IWeRp7LIoFrp0\nvMyW8RaCQdOLi1oM6cCGv5c8KQAPFrqcrKwxyBFIk1I8SoUvObpt0QWZPJLcTq37\nuuWxIx//4mOG0Sqgv5aSCbpvYNi2c8FEm+MHTKz6yOVXLoQpHFb4ZBpolI4IxY2E\ncBxiVWYTrLQczoUaMC+0PMUXspoZfE0BWaCd6u+Ddxn1sLoSYF7d1wILMpNogwx5\nl1zKlDCEswZ+5VY8WoEjyojl+r2GYZf7dG86o8Zenhumxr49PE01ZX1WCFadWbZ/\n713CvgJgYnCfumU9tjH0h9VkXMpuomImH1zWKXqFHbHd7pgyewFqMDYESiC++kc4\nGwPhaNrmE98g/TWwLU+kaVdu0DXBe2dROkjlclkLzzgFziWHFNgsYQIDAQABo4Gu\nMIGrMAwGA1UdEwEB/wQCMAAwDgYDVR0PAQH/BAQDAgKkMB0GA1UdJQQWMBQGCCsG\nAQUFBwMCBggrBgEFBQcDATAsBgNVHREEJTAjhwR/AAABhxAAAAAAAAAAAAAAAAAA\nAAABgglsb2NhbGhvc3QwHQYDVR0OBBYEFMq3gb9uyEcOarASQOweAsgbXdC3MB8G\nA1UdIwQYMBaAFMq3gb9uyEcOarASQOweAsgbXdC3MA0GCSqGSIb3DQEBCwUAA4IB\ngQAaLTdlIwhB7kD8bLR9aU3IogeAmUkojF9UeNd9wUpTWVBUE15LItnjbW55hLkJ\n+zgPjCUdG/hroVUDdwiK63c0MzvQ0LH7YDj93rvmfZdiLbQQs57dwsTiFjoqSxNb\nug3P9cqbBPThQ/gzjloIynvAgkrm32p7J5jDGRyx1hbKcXjFINPw+EsrtH8NjPYL\njVC5zckS0IQIfkwCPV90yK3f3gluF6a0zlhhkCxbg1IfXF5l/+4H3kzFMGZpsdU9\nAAwckL2rg3ixd92sIBIBxTIJxbH5BtYlnaLCEmEdNeucWJ33QEjnV/aZE2jxiLk2\ndEh7PhfSIoIQqawxHG8HqOiWgBe25xdGS+2zjZ3jRpy+SZ2lsfzflpg3W7HQUeTU\naDrGqtmiE2tueesZJYtW46TiUEBv0eqhyYOxhRIWSu3aH4VazAL7h09bjShKhvhj\ngp/yyoaVtfrG6lGX7Hw7wt7K48D4leEnH5NaVpw8EboVEwuDm15LIMAO25py09jd\noEs=\n-----END CERTIFICATE-----\n"
			};
			ConfigurationUtils.StandardizeConfig(config);
			return config;
		}
	}

	public class TestIncomingMessagePreparation
	{
		public TestIncomingMessagePreparation() { }

		public string TestInstruction { get; set; }

		public string MsgBody { get; set; }

		public ErrorCode AckResponse { get; set; }

		public Dictionary<string, string> MetaData { get; set; }
	}

	public class ResultByteChunkReaderForTest : IAsyncStreamReader<ResultByteChunk>
	{
		public ResultByteChunkReaderForTest(string msgBody)
		{
			this.msgString = msgBody;
			this.retrieved = false;
		}

		readonly string msgString;
		bool retrieved;

		public ResultByteChunk Current
		{
			get
			{
				if (!retrieved)
				{
					var byteChunk = new ResultByteChunk();
					byteChunk.Chunk = ByteString.CopyFromUtf8(this.msgString);
					retrieved = true;
					return byteChunk;
				}
				else
				{
					return new ResultByteChunk();
				}
			}
		}

		public Task<bool> MoveNext(CancellationToken cancellationToken)
		{
			return Task.FromResult(!retrieved);
		}
	}

	public static class TestInstructionValues
	{
		public const string Key = "TestInstruction";
		public const string SUCCESS = "SUCCESS";
		public const string ERROR = "ERROR";
		public const string EXCEPTION = "EXCEPTION";
	}

	class TestDirectXtOutboundProcessorTestWrapper : TestDirectXtOutboundProcessor
	{
		public TestDirectXtOutboundProcessorTestWrapper(ILogger logger) : base(logger) { }

		internal IMsgClientProvider ClientProvider { get; set; }
		internal ISubmitMsgAttributeModifier AttributeModifier { get; set; }

		protected override IMsgClientProvider GetMsgClientProvider(Configuration config, CancellationToken token)
		{
			return ClientProvider;
		}

		protected override ISubmitMsgAttributeModifier GetSubmitMsgAttributeModifier()
		{
			return AttributeModifier;
		}
	}

	public class TestDirectXtOutboundServiceTaskTestWrapper : TestDirectXtOutboundServiceTask
	{
		public TestDirectXtOutboundServiceTaskTestWrapper() : base()
		{
			ServiceLogger = new TestUtils.TestLogger();
		}

		public IMsgClientProvider ClientProvider { get; set; }
		public ISubmitMsgAttributeModifier AttributeModifier { get; set; }
		public bool IsTest { get; set; }

		protected override IInterchangeProcessor GetMessageProcessor()
		{
			var result = new TestDirectXtOutboundProcessorTestWrapper(ServiceLogger);
			result.ClientProvider = ClientProvider;
			return result;
		}

		internal new bool IsProduction()
		{
			return base.IsProduction();
		}
	}

	class TestDirectXtInboundProcessorTestWrapper : TestDirectXtInboundProcessor
	{
		public TestDirectXtInboundProcessorTestWrapper(ILogger logger) : base(logger) { }

		internal IMsgClientProvider ClientProvider { get; set; }
		internal ISubmitMsgAttributeModifier AttributeModifier { get; set; }

		protected override IMsgClientProvider GetMsgClientProvider(Configuration config, CancellationToken token)
		{
			return ClientProvider;
		}
	}

	public class TestDirectXtInboundServiceTaskTestWrapper : TestDirectXtInboundServiceTask
	{
		public TestDirectXtInboundServiceTaskTestWrapper() : base()
		{
			ServiceLogger = new TestUtils.TestLogger();
		}

		public IMsgClientProvider ClientProvider { get; set; }
		public ISubmitMsgAttributeModifier AttributeModifier { get; set; }
		public bool IsTest { get; set; }

		protected override IInterchangeProcessor GetMessageProcessor()
		{
			var result = new TestDirectXtInboundProcessorTestWrapper(ServiceLogger);
			result.ClientProvider = ClientProvider;
			result.AttributeModifier = AttributeModifier;
			return result;
		}

		internal new bool IsProduction()
		{
			return base.IsProduction();
		}
	}
}
