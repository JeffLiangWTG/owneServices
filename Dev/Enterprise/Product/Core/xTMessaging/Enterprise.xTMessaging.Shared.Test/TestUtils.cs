using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Google.Protobuf;
using Grpc.Core;
using Moq;
using Newtonsoft.Json;
using Xware.Xt.Grpc.Application;
using Xware.Xt.Grpc.Config;

namespace Enterprise.xTMessaging.Shared.Test
{
	public static class TestUtils
	{
		public static Configuration GetTestConfiguration()
		{
			var config = new Configuration()
			{
				Application = new Application { Password = ConfigurationPassword, URI = ConfigurationUri },
				Connect = ConfigurationConnect,
				CA = ConfigurationCa
			};
			ConfigurationUtils.StandardizeConfig(config);
			return config;
		}

		public const string ConfigurationUri = "xt-application:{cfb01c8e-f063-4e3a-bc3e-7e274f84e66r}";
		public const string ConfigurationPassword = "xHGatewayPW";
		public const string ConfigurationConnect = "127.0.0.1:61001";
		public const string ConfigurationCa = "-----BEGIN CERTIFICATE-----\nMIIETjCCAragAwIBAgIJANaqHXch0txmMA0GCSqGSIb3DQEBCwUAMA0xCzAJBgNV\nBAYTAkFVMCAXDTIxMDQyMzAwNTUyN1oYDzIwMjQwNDIzMDAwMDAwWjANMQswCQYD\nVQQGEwJBVTCCAaIwDQYJKoZIhvcNAQEBBQADggGPADCCAYoCggGBAJ0+AyGGGDfU\nbJmCQszgnfJC9MmRm6kRl970hmVgl1UAJ8Oa10T/s+JA/JAhyo48Rvl0Ukh9JD5Z\nC6VayqyP8I1SdREFUXBhjChC6xt+LiA6/ZFs1Z3j5eoWH6S/uS5IWeRp7LIoFrp0\nvMyW8RaCQdOLi1oM6cCGv5c8KQAPFrqcrKwxyBFIk1I8SoUvObpt0QWZPJLcTq37\nuuWxIx//4mOG0Sqgv5aSCbpvYNi2c8FEm+MHTKz6yOVXLoQpHFb4ZBpolI4IxY2E\ncBxiVWYTrLQczoUaMC+0PMUXspoZfE0BWaCd6u+Ddxn1sLoSYF7d1wILMpNogwx5\nl1zKlDCEswZ+5VY8WoEjyojl+r2GYZf7dG86o8Zenhumxr49PE01ZX1WCFadWbZ/\n713CvgJgYnCfumU9tjH0h9VkXMpuomImH1zWKXqFHbHd7pgyewFqMDYESiC++kc4\nGwPhaNrmE98g/TWwLU+kaVdu0DXBe2dROkjlclkLzzgFziWHFNgsYQIDAQABo4Gu\nMIGrMAwGA1UdEwEB/wQCMAAwDgYDVR0PAQH/BAQDAgKkMB0GA1UdJQQWMBQGCCsG\nAQUFBwMCBggrBgEFBQcDATAsBgNVHREEJTAjhwR/AAABhxAAAAAAAAAAAAAAAAAA\nAAABgglsb2NhbGhvc3QwHQYDVR0OBBYEFMq3gb9uyEcOarASQOweAsgbXdC3MB8G\nA1UdIwQYMBaAFMq3gb9uyEcOarASQOweAsgbXdC3MA0GCSqGSIb3DQEBCwUAA4IB\ngQAaLTdlIwhB7kD8bLR9aU3IogeAmUkojF9UeNd9wUpTWVBUE15LItnjbW55hLkJ\n+zgPjCUdG/hroVUDdwiK63c0MzvQ0LH7YDj93rvmfZdiLbQQs57dwsTiFjoqSxNb\nug3P9cqbBPThQ/gzjloIynvAgkrm32p7J5jDGRyx1hbKcXjFINPw+EsrtH8NjPYL\njVC5zckS0IQIfkwCPV90yK3f3gluF6a0zlhhkCxbg1IfXF5l/+4H3kzFMGZpsdU9\nAAwckL2rg3ixd92sIBIBxTIJxbH5BtYlnaLCEmEdNeucWJ33QEjnV/aZE2jxiLk2\ndEh7PhfSIoIQqawxHG8HqOiWgBe25xdGS+2zjZ3jRpy+SZ2lsfzflpg3W7HQUeTU\naDrGqtmiE2tueesZJYtW46TiUEBv0eqhyYOxhRIWSu3aH4VazAL7h09bjShKhvhj\ngp/yyoaVtfrG6lGX7Hw7wt7K48D4leEnH5NaVpw8EboVEwuDm15LIMAO25py09jd\noEs=\n-----END CERTIFICATE-----\n";

		public static IMsgClientProvider GetMoqMsgClientProvider(IEnumerable<SubmitMsgReply> submitReplysSetup = null, Action<string> additionalProcess = null, Action actionOnStartTransactionAsync = null, Action<bool> actionOnEndTransactionAsync = null)
		{
			var callRequestStream = new Mock<IClientStreamWriter<ByteChunk>>();
			callRequestStream.CallBase = true;
			callRequestStream.Setup(m => m.WriteAsync(It.IsAny<ByteChunk>()))
							 .Returns(Task.CompletedTask);
			callRequestStream.Setup(m => m.CompleteAsync())
							 .Returns(Task.CompletedTask);

			var callResponseAsync = Task.FromResult(new WriteMsgDataStreamReply() { Ref = "Ref" });

			var asyncCall = new AsyncClientStreamingCall<ByteChunk, WriteMsgDataStreamReply>(callRequestStream.Object, callResponseAsync, null, null, null, null);

			var msgClientMock = new Mock<IMsgClient>();
			if (submitReplysSetup == null)
			{
				var submitMsgReply = new SubmitMsgReply() { Errorcode = (int)ErrorCode.ErrOk };
				submitMsgReply.Ids.Add(new MsgIdUri() { Msgid = 12345UL });
				msgClientMock.Setup(m => m.SubmitMsgAsync(It.IsAny<SubmitMsgMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
							 .Callback((SubmitMsgMessage message, DateTime? datetime, CancellationToken? token) => additionalProcess?.Invoke(message.Msgattr["msgid"]))
							 .ReturnsAsync(submitMsgReply);
			}
			else
			{
				var returnSequence = msgClientMock.SetupSequence(m => m.SubmitMsgAsync(It.IsAny<SubmitMsgMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()));
				var msgId = 123UL;
				foreach (var reply in submitReplysSetup)
				{
					reply.Ids.Add(new MsgIdUri() { Msgid = msgId });
					msgId++;
					returnSequence.ReturnsAsync(reply);
				}
			}

			msgClientMock.Setup(m => m.WriteMsgDataStream(null, null)).Returns(asyncCall);
			msgClientMock.Setup(m => m.WriteMsgDataStream(It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(asyncCall);

			msgClientMock.Setup(x => x.StartTransactionAsync(It.IsAny<StartTransactionMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
						 .ReturnsAsync(new StartTransactionReply())
						 .Callback(() => actionOnStartTransactionAsync?.Invoke());

			msgClientMock.Setup(x => x.EndTransactionAsync(It.IsAny<EndTransactionMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
						 .ReturnsAsync(new EndTransactionReply())
						 .Callback((EndTransactionMessage endTransactionMessage, DateTime? deadline, CancellationToken? cancellationToken) => actionOnEndTransactionAsync?.Invoke(endTransactionMessage.Commit));

			var clientProvider = new Mock<IMsgClientProvider>();
			clientProvider.Setup(m => m.MsgClient).Returns(msgClientMock.Object);
			return clientProvider.Object;
		}

		public enum MsgClientMethod
		{
			StartTransaction,
			EndTransaction,
			SubmitMsg,
			WaitMsg,
			Default
		}

		public static IMsgClientProvider GetMoqMsgClientProviderWithRunActionWrappersForExceptionHandlingTesting(Exception ex, MsgClientMethod method = MsgClientMethod.Default, IEnumerable<SubmitMsgReply> submitReplysSetup = null, Action<string> additionalProcess = null)
		{
			var callRequestStream = new Mock<IClientStreamWriter<ByteChunk>>();
			callRequestStream.CallBase = true;
			callRequestStream.Setup(m => m.WriteAsync(It.IsAny<ByteChunk>()))
							 .Returns(Task.CompletedTask);
			callRequestStream.Setup(m => m.CompleteAsync())
							 .Returns(Task.CompletedTask);

			var callResponseAsync = Task.FromResult(new WriteMsgDataStreamReply() { Ref = "Ref" });

			var asyncCall = new AsyncClientStreamingCall<ByteChunk, WriteMsgDataStreamReply>(callRequestStream.Object, callResponseAsync, null, null, null, null);

			var msgClientMock = new Mock<IMsgClient>();
			if (submitReplysSetup == null)
			{
				var submitMsgReply = new SubmitMsgReply() { Errorcode = (int)ErrorCode.ErrOk };
				submitMsgReply.Ids.Add(new MsgIdUri() { Msgid = 12345UL });
				msgClientMock.Setup(m => m.SubmitMsgAsync(It.IsAny<SubmitMsgMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
							 .Callback((SubmitMsgMessage message, DateTime? datetime, CancellationToken? token) => additionalProcess?.Invoke(message.Msgattr["msgid"]))
							 .ReturnsAsync(submitMsgReply);
			}
			else
			{
				var returnSequence = msgClientMock.SetupSequence(m => m.SubmitMsgAsync(It.IsAny<SubmitMsgMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()));
				var msgId = 123UL;
				foreach (var reply in submitReplysSetup)
				{
					reply.Ids.Add(new MsgIdUri() { Msgid = msgId });
					msgId++;
					returnSequence.ReturnsAsync(reply);
				}
			}
			msgClientMock.Setup(m => m.WriteMsgDataStream(null, null)).Returns(asyncCall);
			msgClientMock.Setup(m => m.WriteMsgDataStream(It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(asyncCall);

			switch (method)
			{
				case MsgClientMethod.StartTransaction:
					msgClientMock.Setup(x => x.StartTransactionAsync(It.IsAny<StartTransactionMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
						.Returns((StartTransactionMessage msg, DateTime? deadline, CancellationToken? token) =>
							Utils.RunActionAndThrowMsgServerConnectionExceptionIfNeededAsync(
							() => Task.FromException<StartTransactionReply>(ex),
							"StartTransaction",
							TimeSpan.FromSeconds(30)));
					break;
				case MsgClientMethod.EndTransaction:
					msgClientMock.Setup(x => x.StartTransactionAsync(It.IsAny<StartTransactionMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
						.ReturnsAsync(new StartTransactionReply());

					msgClientMock.Setup(x => x.EndTransactionAsync(It.IsAny<EndTransactionMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
						.Returns((EndTransactionMessage msg, DateTime? deadline, CancellationToken? token) =>
							Utils.RunActionAndThrowMsgServerConnectionExceptionIfNeededAsync(
							() => Task.FromException<EndTransactionReply>(ex),
							"EndTransaction",
							TimeSpan.FromSeconds(30)));
					break;
				case MsgClientMethod.SubmitMsg:
					msgClientMock.Setup(x => x.SubmitMsgAsync(It.IsAny<SubmitMsgMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
						.Returns((SubmitMsgMessage msg, DateTime? deadline, CancellationToken? token) =>
							Utils.RunActionAndThrowMsgServerConnectionExceptionIfNeededAsync(
								() => Task.FromException<SubmitMsgReply>(ex),
								"SubmitMessage",
								TimeSpan.FromSeconds(30)));
					break;
				case MsgClientMethod.WaitMsg:
					msgClientMock.Setup(x => x.WaitMsgAsync(It.IsAny<WaitMsgMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
						.Returns((WaitMsgMessage msg, DateTime? deadline, CancellationToken? token) =>
							Utils.RunActionAndThrowMsgServerConnectionExceptionIfNeededAsync(
								() => Task.FromException<WaitMsgReply>(ex),
								"GetMessage",
								TimeSpan.FromSeconds(30)));
					break;
				case MsgClientMethod.Default:
					msgClientMock.Setup(x => x.StartTransactionAsync(It.IsAny<StartTransactionMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
						.ReturnsAsync(new StartTransactionReply());

					msgClientMock.Setup(x => x.EndTransactionAsync(It.IsAny<EndTransactionMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
						.ReturnsAsync(new EndTransactionReply());
					break;
				default:
					break;
			}

			var clientProvider = new Mock<IMsgClientProvider>();
			clientProvider.Setup(m => m.XtToObjFilter).Returns("test");
			clientProvider.Setup(m => m.MsgClient).Returns(msgClientMock.Object);
			return clientProvider.Object;
		}

		public static IMsgClientProvider GetMockedMsgClientProviderChunkSize(List<ByteChunk> chunks)
		{
			var callRequestStream = new Mock<IClientStreamWriter<ByteChunk>>();
			callRequestStream.Setup(m => m.CompleteAsync()).Returns(Task.CompletedTask);
			callRequestStream.Setup(m => m.WriteAsync(It.IsAny<ByteChunk>()))
				.Returns((ByteChunk bytechunkArg) =>
				{
					var bytechunk = bytechunkArg.Clone();
					chunks.Add(bytechunk);
					return Task.CompletedTask;
				});

			var callResponseAsync = Task.FromResult(new WriteMsgDataStreamReply() { Ref = "Ref" });

			var asyncCall = new AsyncClientStreamingCall<ByteChunk, WriteMsgDataStreamReply>(
				callRequestStream.Object,
				callResponseAsync,
				null,
				null,
				null,
				null);

			var msgClientMock = new Mock<IMsgClient>();
			msgClientMock.CallBase = true;
			var submitMsgReply = new SubmitMsgReply() { Errorcode = (int)ErrorCode.ErrOk };
			submitMsgReply.Ids.Add(new MsgIdUri() { Msgid = 12345UL });
			msgClientMock.Setup(m => m.SubmitMsgAsync(It.IsAny<SubmitMsgMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.ReturnsAsync(submitMsgReply);

			msgClientMock.Setup(m => m.WriteMsgDataStream(It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
				.Returns((DateTime? datetime, CancellationToken cancellationToken) =>
				{
					return asyncCall;
				});

			var clientProvider = new Mock<IMsgClientProvider>();
			clientProvider.Setup(m => m.MsgClient).Returns(msgClientMock.Object);

			return clientProvider.Object;
		}

		public static IMsgClientProvider GetMockedMsgClientProviderWithSubmitMsgDelay()
		{
			var callRequestStream = new Mock<IClientStreamWriter<ByteChunk>>();
			callRequestStream.Setup(m => m.CompleteAsync()).Returns(Task.CompletedTask);
			callRequestStream.Setup(m => m.WriteAsync(It.IsAny<ByteChunk>())).Returns(Task.CompletedTask);

			var callResponseAsync = Task.FromResult(new WriteMsgDataStreamReply() { Ref = "Ref" });

			var asyncCall = new AsyncClientStreamingCall<ByteChunk, WriteMsgDataStreamReply>(
				callRequestStream.Object,
				callResponseAsync,
				null,
				null,
				null,
				null);

			var msgClientMock = new Mock<IMsgClient>();
			msgClientMock.CallBase = true;
			msgClientMock
				.Setup(m => m.SubmitMsgAsync(
				It.IsAny<SubmitMsgMessage>(),
				It.IsAny<DateTime>(),
				It.IsAny<CancellationToken>()))
				.Returns((SubmitMsgMessage msg, DateTime dt, CancellationToken token) =>
				{
					return Task.Delay(4000, token).ContinueWith((_) =>
					{
						if (token.IsCancellationRequested)
						{
							throw new OperationCanceledException(token);
						}

						var result = new SubmitMsgReply() { Errorcode = (int)ErrorCode.ErrOk };
						return result;
					}, token);
				});
			msgClientMock
				.Setup(m => m.WriteMsgDataStream(
					It.IsAny<DateTime?>(),
					It.IsAny<CancellationToken>()))
				.Returns(asyncCall);

			var clientProvider = new Mock<IMsgClientProvider>();
			clientProvider.Setup(m => m.MsgClient).Returns(msgClientMock.Object);

			return clientProvider.Object;
		}

		public static (IMsgClientProvider, IEnumerable<SubmitMsgMessage>, IEnumerable<ByteChunk>) GetMockedMsgClientProviderWithMessageInspection(ErrorCode errorCode = ErrorCode.ErrOk, Action actionOnStartTransactionAsync = null, Action<bool> actionOnEndTransactionAsync = null, bool throwExceptionOnStartTransaction = false)
		{
			IList<SubmitMsgMessage> submitMessageList = new List<SubmitMsgMessage>();
			IList<ByteChunk> messagePayloadParts = new List<ByteChunk>();

			var callRequestStream = new Mock<IClientStreamWriter<ByteChunk>>();
			callRequestStream.Setup(m => m.CompleteAsync()).Returns(Task.CompletedTask);
			callRequestStream.Setup(m => m.WriteAsync(It.IsAny<ByteChunk>())).Returns((ByteChunk bytechunkArg) =>
			{
				var bytechunk = bytechunkArg.Clone();
				messagePayloadParts.Add(bytechunk);
				return Task.CompletedTask;
			});

			var callResponseAsync = Task.FromResult(new WriteMsgDataStreamReply() { Ref = "Ref" });

			var asyncCall = new AsyncClientStreamingCall<ByteChunk, WriteMsgDataStreamReply>(callRequestStream.Object, callResponseAsync, null, null, null, null);

			var msgClientMock = new Mock<IMsgClient>();

			msgClientMock
				.Setup(m => m.SubmitMsgAsync(It.IsAny<SubmitMsgMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.Returns((SubmitMsgMessage msg, DateTime? _, CancellationToken? _) =>
				{
					submitMessageList.Add(msg);
					var submitMsgReply = new SubmitMsgReply() { Errorcode = (int)errorCode };
					if (errorCode == ErrorCode.ErrOk)
					{
						submitMsgReply.Ids.Add(new MsgIdUri() { Msgid = 12345UL });
					}
					return Task.FromResult(submitMsgReply);
				});

			if (throwExceptionOnStartTransaction)
			{
				msgClientMock.Setup(m => m.StartTransactionAsync(It.IsAny<StartTransactionMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
					.Callback((StartTransactionMessage _, DateTime? _, CancellationToken? _) =>
					{
						actionOnStartTransactionAsync?.Invoke();
					})
					.Throws(new XtTransactionException());
			}
			else
			{
				msgClientMock.Setup(m => m.StartTransactionAsync(It.IsAny<StartTransactionMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
					.Callback((StartTransactionMessage _, DateTime? _, CancellationToken? _) =>
					{
						actionOnStartTransactionAsync?.Invoke();
					})
					.ReturnsAsync(new StartTransactionReply());
			}

			msgClientMock
				.Setup(m => m.EndTransactionAsync(It.IsAny<EndTransactionMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.Callback((EndTransactionMessage endTransactionMessage, DateTime? _, CancellationToken? _) =>
				{
					actionOnEndTransactionAsync?.Invoke(endTransactionMessage.Commit);
				})
				.ReturnsAsync(new EndTransactionReply());

			msgClientMock.Setup(m => m.WriteMsgDataStream(null, null)).Returns(asyncCall);
			msgClientMock.Setup(m => m.WriteMsgDataStream(It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Returns(asyncCall);

			var clientProvider = new Mock<IMsgClientProvider>();
			clientProvider.Setup(m => m.MsgClient).Returns(msgClientMock.Object);
			return (clientProvider.Object, submitMessageList, messagePayloadParts);
		}

		public static EDIInterchange CreateInterchangeWithLargeBody(BusinessObjectFactory factory, string status = EDIInterchangeStatusList.Codes.Queued, bool isActive = true, string receiveTransmit = EDIInterchange.Direction.Transmit, string transportType = EDIInterchangeTransportTypeList.Codes.xT, string applicationCode = "TST", string from = "TSTSND", string to = "TSTRCV", string interchangeNum = "", bool isEmptyGuid = false)
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var data = resourceRetriever.GetBytes("Enterprise.xTMessaging.Shared.Test.TestFiles.testBody.txt");
				var bodyTest = new MemoryStream(data);
				var interchange = factory.New<EDIInterchange>();
				interchange.EI_Status = status;
				interchange.EI_IsActive = isActive;
				interchange.EI_ReceiveTransmit = receiveTransmit;
				interchange.EI_TransportType = transportType;
				interchange.EI_From = from;
				interchange.EI_To = to;
				interchange.EI_SessionGUID = ZGuid.NewZGuid();
				interchange.SetEI_BodyTextOrDataSource(bodyTest);
				interchange.EI_InterchangeNum = interchangeNum;
				interchange.EI_InterchangeType = "TST";
				interchange.EI_ApplicationCode = applicationCode;
				return interchange;
			}
		}

		public static Stream GetEmbeddedResource(Assembly assembly, string resourceName, string resourcePath)
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(assembly))
			{
				return resourceRetriever.GetStream(resourcePath + resourceName);
			}
		}

		public static EDIInterchange CreateInterchangeForXT(BusinessObjectFactory factory, string status = EDIInterchangeStatusList.Codes.Queued, bool isActive = true, string receiveTransmit = EDIInterchange.Direction.Transmit, string transportType = EDIInterchangeTransportTypeList.Codes.xT, string applicationCode = "TST", string from = "TSTSND", string to = "TSTRCV", string interchangeNum = "", bool isEmptyGuid = false)
		{
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_Status = status;
			interchange.EI_IsActive = isActive;
			interchange.EI_ReceiveTransmit = receiveTransmit;
			interchange.EI_TransportType = transportType;
			interchange.EI_From = "Any";
			interchange.EI_To = "AnyOther";
			interchange.EI_SessionGUID = ZGuid.NewZGuid();
			interchange.EI_ApplicationCode = applicationCode;
			interchange.EI_InterchangeType = "TST";
			interchange.EI_From = from;
			interchange.EI_To = to;
			interchange.EI_BodyText = "TST";
			interchange.EI_InterchangeNum = interchangeNum;
			return interchange;
		}

		public static void InsertRefSysConfig(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateOrGetExistingRefSysConfigType(XTServerCertificateTestKey, "xT Server Certificate Test", "xT Server Certificate Bundle Test");
			helper.CreateOrGetExistingRefSysConfigType(XTServerCertificateProdKey, "xT Server Certificate Prod", "xT Server Certificate Bundle Production");
			helper.CreateOrGetExistingRefSysConfigType(XTServerServerTestKey, "xT Server Address Test", "xT Server Address and Port Test");
			helper.CreateOrGetExistingRefSysConfigType(XTServerServerProdKey, "xT Server Address Prod", "xT Server Address and Port Production");
			helper.CreateOrUpdateExistingRefSysConfig(XTServerCertificateTestKey, XTServerCertificateValue, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddYears(2));
			helper.CreateOrUpdateExistingRefSysConfig(XTServerCertificateProdKey, XTServerCertificateValue, ZDateTime.Today.AddYears(-3), ZDateTime.Today.AddDays(-2));
			helper.CreateOrUpdateExistingRefSysConfig(XTServerServerTestKey, XTServerAddressValue, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddYears(2));
			helper.CreateOrUpdateExistingRefSysConfig(XTServerServerProdKey, XTServerAddressValue, ZDateTime.Today.AddYears(-3), ZDateTime.Today.AddDays(-2));
			factory.Save();
		}

		public static void InsertRefSysConfig(BusinessObjectFactory factory, string configType, string configValue)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateOrGetExistingRefSysConfigType(configType, $"{configType} Desc Test", $"{configType} Long Desc Test");
			helper.CreateOrUpdateExistingRefSysConfig(configType, configValue, ZDateTime.Today.AddYears(-2), ZDateTime.Today.AddYears(2));
			factory.Save();
		}

		const string XTServerCertificateTestKey = "XTCATEST";
		const string XTServerCertificateProdKey = "XTCAPROD";
		const string XTServerServerTestKey = "XTSVRTEST";
		const string XTServerServerProdKey = "XTSVRPROD";

		public const string XTServerAddressValue = "hostname.sand.wtg.zone:1000";
		public const string XTServerCertificateValue = @"-----BEGIN CERTIFICATE-----
MIIETjCCAragAwIBAgIJAPTYdmSO5y+7MA0GCSqGSIb3DQEBCwUAMA0xCzAJBgNV
-----END CERTIFICATE-----";

		#region For Receiving

		public class ResultByteChunkReaderForTest : IAsyncStreamReader<ResultByteChunk>
		{
			public ResultByteChunkReaderForTest(object msgBody)
			{
				this.msgString = msgBody;
				this.retrieved = false;
			}

			readonly object msgString;
			bool retrieved;

			public ResultByteChunk Current
			{
				get
				{
					if (!retrieved)
					{
						var byteChunk = new ResultByteChunk();
						if (this.msgString is string msgStringString)
						{
							byteChunk.Chunk = ByteString.CopyFromUtf8(msgStringString);
							retrieved = true;
						}
						else if (this.msgString is byte[] msgStringBytes)
						{
							byteChunk.Chunk = ByteString.CopyFrom(msgStringBytes);
							retrieved = true;
						}

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

		public class ResultByteChunkReaderForDeadlineExceedTest : IAsyncStreamReader<ResultByteChunk>
		{
			public ResultByteChunkReaderForDeadlineExceedTest(Exception exception)
			{
				this.exception = exception;
			}
			readonly Exception exception;

			public ResultByteChunk Current => new ResultByteChunk();

			public Task<bool> MoveNext(CancellationToken cancellationToken) => AsyncHelper.RunTask<bool>(() => throw exception, new CancellationToken(), "Throw Deadline Exceeded Exception"); //throw new RpcException(new Status(StatusCode.DeadlineExceeded, "GetMsgData Deadline Exceeded"));
		}

		class TestGetMsgAttributesReplyStreamReader(IEnumerable<GetMsgAttributesReply> lst) : IAsyncStreamReader<GetMsgAttributesReply>
		{
			readonly IEnumerator<GetMsgAttributesReply> enumerator = lst.GetEnumerator();

			public GetMsgAttributesReply Current => enumerator.Current;

			public Task<bool> MoveNext(CancellationToken cancellationToken)
			{
				return Task.FromResult(enumerator.MoveNext());
			}
		}

		public interface ITestIncomingMessage
		{
			string TestInstruction { get; set; }
			ErrorCode AckResponse { get; set; }
			Dictionary<string, string> MetaData { get; set; }
			object MsgBody { get; }
		}

		public class TestIncomingMessagePreparation : ITestIncomingMessage
		{
			public TestIncomingMessagePreparation() { }

			public string TestInstruction { get; set; }

			public string MsgBody { get; set; }

			public ErrorCode AckResponse { get; set; }

			public Dictionary<string, string> MetaData { get; set; }

			object ITestIncomingMessage.MsgBody => MsgBody;
		}

		public class TestIncomingBinaryMessagePreparation : ITestIncomingMessage
		{
			public TestIncomingBinaryMessagePreparation() { }

			public string TestInstruction { get; set; }

			public byte[] MsgBody { get; set; }

			public ErrorCode AckResponse { get; set; }

			public Dictionary<string, string> MetaData { get; set; }

			object ITestIncomingMessage.MsgBody => MsgBody;
		}

		internal class TestReceiveHandler : IReceiveHandler
		{
			public TestReceiveHandler(ILogger logger)
			{
				this.messageProcessingResults = new Dictionary<MsgIdUri, MessageHandlingResult>();
				this.receivedContents = new List<string>();
				this.receivedMetaData = new Dictionary<ulong, IDictionary<string, string>>();
				this.logger = logger;
			}

			readonly ILogger logger;

			public IDictionary<MsgIdUri, MessageHandlingResult> MessageProcessingResults => messageProcessingResults;
			readonly IDictionary<MsgIdUri, MessageHandlingResult> messageProcessingResults;

			public List<string> ReceivedContents => receivedContents;
			readonly List<string> receivedContents;

			public IDictionary<ulong, IDictionary<string, string>> ReceivedMetaData => receivedMetaData;
			readonly IDictionary<ulong, IDictionary<string, string>> receivedMetaData;

			public int BatchCount { get; private set; }

			public void HandleReceivedMessageBatch(ICollection<MsgIdUri> msgIds, GetMessageMetaDataForHandling getMessageMetaDataForHandling, GetMsgData getMsgData, LoadReplyIntoMemory loadReplyIntoMemory)
			{
				MessageProcessingResults.Clear();
				BatchCount++;
				foreach (var msgId in msgIds)
				{
					using (var msgData = getMsgData(new GetMsgDataMessage { Id = msgId }))
					using (var resultStream = loadReplyIntoMemory(msgData))
					{
						HandleReceivedMessage(msgId, getMessageMetaDataForHandling(msgId), resultStream, getMessageMetaDataForHandling);
					}
				}
			}

			void HandleReceivedMessage(MsgIdUri msgId, Dictionary<string, string> metaData, Stream payload, GetMessageMetaDataForHandling getMessageMetaDataForHandling)
			{
				var testInst = TestInstructionValues.Key;
				if (!metaData.ContainsKey(Constants.xTMsgAttributes.refexternal))
				{
					_ = metaData.GetOrAdd(Constants.CustomMsgAttributes.MessageTrackingID, () => Guid.NewGuid().ToString());
				}

				var metaDataHelper = new MetaDataHelper(metaData);

				if (!metaDataHelper.IsValidForSavingToEDIInterchange() && metaData.ContainsKey(Constants.xTMsgAttributes.refexternal))
				{
					var orgMsgId = new MsgIdUri() { Msgid = ulong.Parse(metaData[Constants.xTMsgAttributes.refexternal]) };
					metaDataHelper.MergeMessageAttributesFromOriginalInfo(getMessageMetaDataForHandling(orgMsgId));
				}

				if (metaData.ContainsKey(testInst))
				{
					switch (metaData[testInst])
					{
						case TestInstructionValues.SUCCESS:
							MessageProcessingResults[msgId] = new MessageHandlingResult(Constants.MessageHandlingResultOperation.Success, 0);
							ReceivedContents.Add(new StreamReader(payload).ReadToEnd());
							receivedMetaData[msgId.Msgid] = metaData;
							break;
						case TestInstructionValues.ERROR:
							MessageProcessingResults[msgId] = new MessageHandlingResult(Constants.MessageHandlingResultOperation.Error, 0);
							break;
						case TestInstructionValues.EXCEPTION:
							logger.Log(LogType.Error, FormattableString.Invariant($"Error when downloading message MsgId:{msgId.Msgid}: Mocked Error"));
							break;
						default:
							break;
					}
				}
			}
		}

		public static class TestInstructionValues
		{
			public const string Key = "TestInstruction";
			public const string SUCCESS = "SUCCESS";
			public const string ERROR = "ERROR";
			public const string EXCEPTION = "EXCEPTION";
		}

		public static (IMsgClientProvider, Dictionary<ulong, MsgStatusCommand>) GetMockedMsgClientProviderWithIncomingMessages(IDictionary<ulong, ITestIncomingMessage> incomingMessagePreparations, bool throwExceptionOnGetMsgAttributesList = false)
		{
			var ackTracker = new Dictionary<ulong, MsgStatusCommand>();
			var received = new List<ulong>();

			var msgClientMock = new Mock<IMsgClient>();
			msgClientMock
				.Setup(m => m.MsgSetStatus(It.IsAny<MsgSetStatusMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.Returns((MsgSetStatusMessage req, DateTime? datetime, CancellationToken? token) =>
				{
					ackTracker[req.Id.Msgid] = req.Cmd;
					return new MsgSetStatusReply() { Errorcode = (int)incomingMessagePreparations[req.Id.Msgid].AckResponse };
				});

			msgClientMock
				.Setup(m => m.GetMsgData(It.IsAny<GetMsgDataMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.Returns((GetMsgDataMessage req, DateTime? datetime, CancellationToken? token) =>
				{
					var asyncCall = new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(incomingMessagePreparations[req.Id.Msgid].MsgBody), null, null, null, () => { });
					return asyncCall;
				});

			msgClientMock
				.Setup(m => m.GetMsgAttributes(It.IsAny<GetMsgAttributesMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.Returns((GetMsgAttributesMessage req, DateTime? datetime, CancellationToken? token) =>
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
				.Setup(m => m.GetMsgAttributesList(It.IsAny<GetMsgAttributesListMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.Returns((GetMsgAttributesListMessage req, DateTime? datetime, CancellationToken? token) =>
				{
					if (throwExceptionOnGetMsgAttributesList)
					{
						throw new RpcException(new Status(StatusCode.DeadlineExceeded, "GetMsgAttributesList Deadline Exceeded"));
					}
					var getMsgAttributesListReply = new List<GetMsgAttributesReply>();
					foreach (var id in req.Ids)
					{
						var reply = new GetMsgAttributesReply { Id = id };
						var message = incomingMessagePreparations[id.Msgid];
						if (!string.IsNullOrEmpty(message.TestInstruction))
						{
							reply.Msgattr.Add(TestInstructionValues.Key, message.TestInstruction);
						}

						foreach (var kvp in message?.MetaData ?? new Dictionary<string, string>())
						{
							reply.Msgattr.Add(kvp.Key, kvp.Value);
						}
						getMsgAttributesListReply.Add(reply);
					}

					return new AsyncServerStreamingCall<GetMsgAttributesReply>(new TestGetMsgAttributesReplyStreamReader(getMsgAttributesListReply), null, null, null, null);
				});

			msgClientMock
				.Setup(m => m.WaitMsgAsync(It.IsAny<WaitMsgMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.Returns((WaitMsgMessage msg, DateTime? datetime, CancellationToken? token) =>
				{
					var result = new WaitMsgReply();
					var parameters = JsonConvert.DeserializeObject<Dictionary<string, string>>(msg.ToString());

					var toObjMatched = !(parameters?.ContainsKey("toobj") ?? false)
						|| parameters["toobj"].Trim() == GetTestConfiguration().Application.URI;

					if (toObjMatched)
					{
						var thisBatchCount = 0;
						foreach (var key in incomingMessagePreparations.Keys)
						{
							if (incomingMessagePreparations[key].MsgBody != null && !received.Contains(key))
							{
								result.Ids.Add(new MsgIdObj() { Id = new MsgIdUri() { Msgid = key } });
								received.Add(key);
								thisBatchCount++;
								//Added to address xT 1.3-2079 Patch xt513-2103-75396_xtgrpcapi, as xT now sends messages in maximum batches of 4096.
								if (thisBatchCount >= 4096)
								{
									break;
								}
							}
						}
					}

					return Task.FromResult(result);
				});

			var clientProvider = new Mock<IMsgClientProvider>();
			clientProvider.Setup(m => m.MsgClient).Returns(msgClientMock.Object);
			clientProvider.Setup(m => m.XtToObjFilter).Returns(GetTestConfiguration().Application.URI);
			clientProvider.Setup(m => m.ErrorMessage).Returns("");
			clientProvider.Setup(m => m.TearDown()).Callback(() => { });

			return (clientProvider.Object, ackTracker);
		}

		public static Msg.MsgClient GetMoqMsgClient(MsgMethod deadlineExceededMsgMethod)
		{
			var incomingMessage = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body1", AckResponse = ErrorCode.ErrOk };

			var msgClientMock = new Mock<Msg.MsgClient>();
			switch (deadlineExceededMsgMethod)
			{
				case MsgMethod.SubmitMsgAsync:
					msgClientMock.Setup(m => m.SubmitMsgAsync(It.IsAny<SubmitMsgMessage>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
						.Returns((SubmitMsgMessage msg, Metadata metadata, DateTime? datetime, CancellationToken cancellationToken) =>
						{
							var task = Task.FromException<SubmitMsgReply>(new RpcException(new Status(StatusCode.DeadlineExceeded, "SubmitMsgAsync Deadline Exceeded")));
							return new AsyncUnaryCall<SubmitMsgReply>(task, Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
						});
					break;
				case MsgMethod.WriteMsgDataStream:
					msgClientMock.Setup(m => m.WriteMsgDataStream(It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
								.Returns((Metadata metadata, DateTime? datetime, CancellationToken cancellationToken) =>
								{
									var messagePayloadParts = new List<ByteChunk>();
									var callRequestStream = new Mock<IClientStreamWriter<ByteChunk>>();
									callRequestStream.Setup(m => m.CompleteAsync()).Returns(Task.CompletedTask);
									callRequestStream.Setup(m => m.WriteAsync(It.IsAny<ByteChunk>())).Returns((ByteChunk bytechunkArg) =>
									{
										var bytechunk = bytechunkArg.Clone();
										messagePayloadParts.Add(bytechunk);
										return Task.CompletedTask;
									});

									var taskFromTheTesting = AsyncHelper.RunTask<WriteMsgDataStreamReply>(() => throw new RpcException(new Status(StatusCode.DeadlineExceeded, "WriteMsgDataStream Deadline Exceeded")), new CancellationToken(), "Throw Deadline Exceeded Exception");
									return new AsyncClientStreamingCall<ByteChunk, WriteMsgDataStreamReply>(callRequestStream.Object, taskFromTheTesting, null, null, null, null);
								});
					break;
				case MsgMethod.WaitMsgAsync:
					msgClientMock.Setup(m => m.WaitMsgAsync(It.IsAny<WaitMsgMessage>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
						.Returns((WaitMsgMessage msg, Metadata metadata, DateTime? datetime, CancellationToken cancellationToken) =>
						{
							var task = Task.FromException<WaitMsgReply>(new RpcException(new Status(StatusCode.DeadlineExceeded, "WaitMsgAsync Deadline Exceeded")));
							return new AsyncUnaryCall<WaitMsgReply>(task, Task.FromResult(new Metadata()), () => Status.DefaultSuccess, () => new Metadata(), () => { });
						});
					break;
				case MsgMethod.GetMsgAttributes:
					msgClientMock.Setup(m => m.GetMsgAttributes(It.IsAny<GetMsgAttributesMessage>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Throws(new RpcException(new Status(StatusCode.DeadlineExceeded, "GetMsgAttributes Deadline Exceeded")));
					break;
				case MsgMethod.GetMsgAttributesList:
					msgClientMock.Setup(m => m.GetMsgAttributesList(It.IsAny<GetMsgAttributesListMessage>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Throws(new RpcException(new Status(StatusCode.DeadlineExceeded, "GetMsgAttributesList Deadline Exceeded")));
					break;
				case MsgMethod.GetMsgData:
					msgClientMock.Setup(m => m.GetMsgData(It.IsAny<GetMsgDataMessage>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
								.Returns(new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForDeadlineExceedTest(new RpcException(new Status(StatusCode.DeadlineExceeded, "GetMsgData Deadline Exceeded"))), null, null, null, () => { }));
					break;
				case MsgMethod.MsgSetStatus:
					msgClientMock.Setup(m => m.MsgSetStatus(It.IsAny<MsgSetStatusMessage>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>())).Throws(new RpcException(new Status(StatusCode.DeadlineExceeded, "MsgSetStatus Deadline Exceeded")));
					break;
			}
			return msgClientMock.Object;
		}

		public static IMsgClientProvider GetMockMsgClientProviderForDeadlineTesting(MsgMethod deadlineExceededMsgMethod)
		{
			var msgClientMock = new Mock<IMsgClient>();

			var submitMsgReply = new SubmitMsgReply() { Errorcode = (int)ErrorCode.ErrOk };
			submitMsgReply.Ids.Add(new MsgIdUri() { Msgid = 12345UL });
			msgClientMock.Setup(m => m.SubmitMsgAsync(It.IsAny<SubmitMsgMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.ReturnsAsync(submitMsgReply);
			msgClientMock.Setup(m => m.WriteMsgDataStream(It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.Returns(() =>
				{
					IList<ByteChunk> messagePayloadParts = new List<ByteChunk>();
					var callRequestStream = new Mock<IClientStreamWriter<ByteChunk>>();
					callRequestStream.Setup(m => m.CompleteAsync()).Returns(Task.CompletedTask);
					callRequestStream.Setup(m => m.WriteAsync(It.IsAny<ByteChunk>())).Returns((ByteChunk bytechunkArg) =>
					{
						var bytechunk = bytechunkArg.Clone();
						messagePayloadParts.Add(bytechunk);
						return Task.CompletedTask;
					});

					var callResponseAsync = Task.FromResult(new WriteMsgDataStreamReply() { Ref = "Ref" });

					return new AsyncClientStreamingCall<ByteChunk, WriteMsgDataStreamReply>(
						callRequestStream.Object,
						callResponseAsync,
						null,
						null,
						null,
						null);
				});

			var incomingMessage = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body1", AckResponse = ErrorCode.ErrOk };

			msgClientMock.Setup(m => m.WaitMsgAsync(It.IsAny<WaitMsgMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
			 .Returns((WaitMsgMessage msg, DateTime? datetime, CancellationToken? token) =>
			 {
				 var msgReply = new WaitMsgReply();
				 msgReply.Ids.Add(new MsgIdObj() { Id = new MsgIdUri() { Msgid = 1u } });
				 return Task.FromResult(msgReply);
			 });
			msgClientMock.Setup(m => m.GetMsgAttributes(It.IsAny<GetMsgAttributesMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.Returns(() =>
				{
					var msgAttributesReply = new GetMsgAttributesReply();
					msgAttributesReply.Msgattr.Add(TestInstructionValues.Key, incomingMessage.TestInstruction);
					msgAttributesReply.Msgattr.Add(Constants.CustomMsgAttributes.ApplicationCode, "KRC");
					msgAttributesReply.Msgattr.Add(Constants.CustomMsgAttributes.SourceParty, "KR Customs");
					msgAttributesReply.Msgattr.Add(Constants.CustomMsgAttributes.DestinationParty, "WTL");
					msgAttributesReply.Msgattr.Add(Constants.CustomMsgAttributes.MessageTrackingID, "37C1A0A0-7AC6-4BE3-8354-FABDA83F370A");
					msgAttributesReply.Msgattr.Add(Constants.CustomMsgAttributes.MessageType, "MST");
					return msgAttributesReply;
				});
			msgClientMock
				.Setup(m => m.GetMsgAttributesList(It.IsAny<GetMsgAttributesListMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.Returns((GetMsgAttributesListMessage req, DateTime? datetime, CancellationToken? token) =>
				{
					var getMsgAttributesListReply = new List<GetMsgAttributesReply>();
					foreach (var id in req.Ids)
					{
						var reply = new GetMsgAttributesReply { Id = id };
						reply.Msgattr.Add(TestInstructionValues.Key, incomingMessage.TestInstruction);
						reply.Msgattr.Add(Constants.CustomMsgAttributes.ApplicationCode, "KRC");
						reply.Msgattr.Add(Constants.CustomMsgAttributes.SourceParty, "KR Customs");
						reply.Msgattr.Add(Constants.CustomMsgAttributes.DestinationParty, "WTL");
						reply.Msgattr.Add(Constants.CustomMsgAttributes.MessageTrackingID, "37C1A0A0-7AC6-4BE3-8354-FABDA83F370A");
						reply.Msgattr.Add(Constants.CustomMsgAttributes.MessageType, "MST");
						getMsgAttributesListReply.Add(reply);
					}

					return new AsyncServerStreamingCall<GetMsgAttributesReply>(new TestGetMsgAttributesReplyStreamReader(getMsgAttributesListReply), null, null, null, null);
				});
			msgClientMock.Setup(m => m.GetMsgData(It.IsAny<GetMsgDataMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.Returns(new AsyncServerStreamingCall<ResultByteChunk>(new ResultByteChunkReaderForTest(incomingMessage.MsgBody), null, null, null, () => { }));
			msgClientMock.Setup(m => m.MsgSetStatus(It.IsAny<MsgSetStatusMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
				.Returns(new MsgSetStatusReply() { Errorcode = (int)incomingMessage.AckResponse });

			switch (deadlineExceededMsgMethod)
			{
				case MsgMethod.SubmitMsgAsync:
					msgClientMock.Setup(m => m.SubmitMsgAsync(It.IsAny<SubmitMsgMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
						.Throws(new MsgServerConnectionException(Utils.DeadlineExceededErrorReportKey, new RpcException(new Status(StatusCode.DeadlineExceeded, "Deadline Exceeded")), "SubmitMsgAsync Deadline Exceeded"));
					break;
				case MsgMethod.WriteMsgDataStream:
					msgClientMock.Setup(m => m.WriteMsgDataStream(It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
								.Throws(new AggregateException("One or more errors occurred.", new RpcException(new Status(StatusCode.DeadlineExceeded, "Deadline Exceeded"))));
					break;
				case MsgMethod.WaitMsgAsync:
					msgClientMock.Setup(m => m.WaitMsgAsync(It.IsAny<WaitMsgMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
						.Throws(new AggregateException("One or more errors occurred.", new RpcException(new Status(StatusCode.DeadlineExceeded, "Deadline Exceeded"))));

					break;
				case MsgMethod.GetMsgAttributes:
				case MsgMethod.GetMsgAttributesList:
					msgClientMock.Setup(m => m.GetMsgAttributes(It.IsAny<GetMsgAttributesMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
						.Throws(new MsgServerConnectionException(Utils.DeadlineExceededErrorReportKey, new RpcException(new Status(StatusCode.DeadlineExceeded, "Deadline Exceeded")), "GetMsgAttributes Deadline Exceeded"));
					msgClientMock.Setup(m => m.GetMsgAttributesList(It.IsAny<GetMsgAttributesListMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
						.Throws(new MsgServerConnectionException(Utils.DeadlineExceededErrorReportKey, new RpcException(new Status(StatusCode.DeadlineExceeded, "Deadline Exceeded")), "GetMsgAttributesList Deadline Exceeded"));
					break;
				case MsgMethod.GetMsgData:
					msgClientMock.Setup(m => m.GetMsgData(It.IsAny<GetMsgDataMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
						.Throws(new AggregateException("One or more errors occurred.", new RpcException(new Status(StatusCode.DeadlineExceeded, "Deadline Exceeded"))));
					break;
				case MsgMethod.MsgSetStatus:
					msgClientMock.Setup(m => m.MsgSetStatus(It.IsAny<MsgSetStatusMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken?>()))
						.Throws(new MsgServerConnectionException(Utils.DeadlineExceededErrorReportKey, new RpcException(new Status(StatusCode.DeadlineExceeded, "Deadline Exceeded")), "MsgSetStatus Deadline Exceeded"));
					break;
				default:
					break;
			}

			var clientProvider = new Mock<IMsgClientProvider>();
			clientProvider.Setup(m => m.MsgClient).Returns(msgClientMock.Object);
			clientProvider.Setup(m => m.XtToObjFilter).Returns(GetTestConfiguration().Application.URI);
			clientProvider.Setup(m => m.XtServerMessageTimeout).Returns(TimeSpan.FromSeconds(DirectxTMessagingRegistry.Instance.XTServerMessageTimeoutInSeconds.Value));

			return clientProvider.Object;
		}

		public enum MsgMethod
		{
			SubmitMsgAsync,
			WriteMsgDataStream,
			WaitMsgAsync,
			GetMsgAttributes,
			GetMsgAttributesList,
			GetMsgData,
			MsgSetStatus
		}

		public static IMsgClientProvider MockMsgClientProviderForReceivingTesting(MsgBehaviors waitMsgAsyncBehavior)
		{
			var msgClientMock = new Mock<IMsgClient>();

			msgClientMock.Setup(m => m.WaitMsgAsync(It.IsAny<WaitMsgMessage>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
				.Returns((WaitMsgMessage msg, DateTime? datetime, CancellationToken cancellationToken) =>
				{
					Task<WaitMsgReply> taskFromTheTesting;

					switch (waitMsgAsyncBehavior)
					{
						case MsgBehaviors.ThrowException:
							taskFromTheTesting = AsyncHelper.RunTask<WaitMsgReply>(() => throw new Exception("Internal exception occurred"), cancellationToken, "Throw Exception");
							break;
						case MsgBehaviors.ThrowDeadlineException:
							taskFromTheTesting = AsyncHelper.RunTask<WaitMsgReply>(() => throw new RpcException(new Status(StatusCode.DeadlineExceeded, "Deadline Exceeded")), cancellationToken, "Throw Deadline Exceeded Exception");
							break;
						case MsgBehaviors.RunFor5SecWithRespondingToCancel:
							taskFromTheTesting = AsyncHelper.RunTask(() =>
							{
								Task.Delay(5000, cancellationToken).GetAwaiter().GetResult();
								return new WaitMsgReply();
							}, cancellationToken, "Run for 5 sec");
							break;
						case MsgBehaviors.RunFor5SecWithoutRespondingToCancel:
							taskFromTheTesting = AsyncHelper.RunTask(() =>
							{
								Task.Delay(5000, default).GetAwaiter().GetResult();
								return new WaitMsgReply();
							}, cancellationToken, "Run for 5 sec and not responding to CancelToken");
							break;
						case MsgBehaviors.FinishInTime:
						default:
							taskFromTheTesting = AsyncHelper.RunTask(() => new WaitMsgReply(), cancellationToken, "Normal return 0 message");
							break;
					}

					return taskFromTheTesting;
				});

			var clientProvider = new Mock<IMsgClientProvider>();
			clientProvider.Setup(m => m.MsgClient).Returns(msgClientMock.Object);
			clientProvider.Setup(m => m.XtToObjFilter).Returns(GetTestConfiguration().Application.URI);

			return clientProvider.Object;
		}

		public enum MsgBehaviors
		{
			FinishInTime,
			ThrowException,
			ThrowDeadlineException,
			RunFor5SecWithRespondingToCancel,
			RunFor5SecWithoutRespondingToCancel
		}

		#endregion

		public class TestLogger : ILogger
		{
			readonly List<(LogType Type, string Message)> loggedContents = [];

			public List<(LogType Type, string Message)> AllLogs => loggedContents;
			public List<string> InfoLogs => loggedContents.Where(l => l.Type == LogType.Information).Select(l => l.Message).ToList();
			public List<string> DebugLogs => loggedContents.Where(l => l.Type == LogType.Debug).Select(l => l.Message).ToList();
			public List<string> ErrorLogs => loggedContents.Where(l => l.Type == LogType.Error).Select(l => l.Message).ToList();
			public List<string> WarningLogs => loggedContents.Where(l => l.Type == LogType.Warning).Select(l => l.Message).ToList();

			public void Log(LogType type, string message)
			{
				loggedContents.Add((type, message));
			}

			public void Log(LogType type, string message, Exception ex)
			{
				Log(type, $"{message}:{ex.Message}");
			}
		}
	}
}
