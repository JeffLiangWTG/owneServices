using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.Shared;
using Enterprise.xTMessaging.Shared.Test;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Xware.Xt.Grpc.Application;
using static Enterprise.xTMessaging.Shared.Test.TestUtils;

namespace Enterprise.RemotePrinting.Client.Tests
{
	sealed class JPNACCSMessageSenderControllerTest : TestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1171:Do Not Use MimeMessage.ToString Rule")]
		public void TestSend()
		{
			var logs = new StringBuilder();
			Dictionary<ulong, MsgStatusCommand> statusTracker = null;
			var messages = new List<string>();

			using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
			{
				var times = 0;

				ISendMessageClient BuildMessageClient()
				{
					var mockClient = new Mock<ISendMessageClient>();
					mockClient.Setup(c => c.IsConnected).Returns(true);
					mockClient.Setup(c => c.Interval).Returns(0.1);
					mockClient.Setup(c => c.ConnectMaxRetries).Returns(2);
					mockClient.Setup(c => c.Connect(It.IsAny<string>())).Callback<string>((host) => logs.AppendLine($"Connect Server: {host}"));
					mockClient.Setup(c => c.Send(It.IsAny<MimeMessage>())).Callback<MimeMessage>((message) =>
					{
						messages.Add(message.ToString());
						logs.AppendLine($"Send Message: {message.Subject}");
					});

					mockClient.Setup(c => c.Disconnect(It.IsAny<bool>())).Callback<bool>((quit) =>
					{
						times++;
						logs.AppendLine($"Disconnect: {quit}");

						if (times == 2)
						{
							cancellationTokenSource.Cancel();
						}
					});

					return mockClient.Object;
				}

				var (controller, ackTracker) = GetMessageSenderControllerAndTracker(BuildMessageClient, cancellationTokenSource.Token);
				controller.ShowInformation += (o, e) => { logs.AppendLine(e.Message); };
				controller.ProcessForTesting();

				statusTracker = ackTracker;
			}

			var fullLog = logs.ToString();

			CombineAssertions($"Should receive 2 messages from the DxT then send them to the NACCS@MAIL.TEST.NACCS6.{System.Environment.NewLine}Logs:{System.Environment.NewLine}{logs}", () =>
			{
				AssertContains("Connect Server: localhost", fullLog);
				AssertContainsExactElementsInExactOrder("xT Message Statuses set", new[] { MsgStatusCommand.StatusOk, MsgStatusCommand.StatusOk }, statusTracker.Values);

				AssertEquals("Messages count", 2, messages.Count);

				var fromAddresses = new List<string>();

				foreach (var message in messages)
				{
					using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(message)))
					using (var mimeMessage = MimeMessage.Load(stream))
					{
						fromAddresses.Add(mimeMessage.From.ToString());

						AssertEquals("SMTP To", "NACCS@MAIL.TEST.NACCS6", mimeMessage.To.Single().ToString());
						AssertEquals("MIME Version", "1.0", mimeMessage.MimeVersion.ToString());
						AssertEquals("Content Type", "text/plain", mimeMessage.Body.ContentType.MimeType);
						AssertEquals("Content Charset", "EUC-JP", mimeMessage.Body.ContentType.Charset);
						AssertEquals("Content TransferEncoding", ContentEncoding.EightBit, ((MimePart)mimeMessage.Body).ContentTransferEncoding);
					}
				}

				AssertContainsExactElementsInAnyOrder("SMTP From", new[] { "xxx@MAIL.TEST.NACCS6", "ddd@MAIL.TEST.NACCS6" }, fromAddresses);
			});
		}

		public void TestSendingWithConnectError()
		{
			using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
			{
				ISendMessageClient BuildMessageClient()
				{
					var mockClient = new Mock<ISendMessageClient>();
					mockClient.Setup(c => c.IsConnected).Returns(false);
					mockClient.Setup(c => c.Interval).Returns(0.1);
					mockClient.Setup(c => c.ConnectMaxRetries).Returns(2);
					mockClient.Setup(c => c.Connect(It.IsAny<string>())).Callback(() =>
					{
						cancellationTokenSource.Cancel();
						throw new Exception("Connect Error");
					});

					return mockClient.Object;
				}

				var prefixTips = @"SMTP connection fails. Please check the followings: 
	1.You are connected to the NACCS internet via a NACCS router. 
	2.The domain WebPrint.WisetechGlobal.com is valid. To update this value, visit CargoWise > Maintain > System > Registry > Customs > Country or Region Specific > Japan > NACCS Messaging > Remote WebPrint Client Configurations. 
	3.The mailbox xxx@MAIL.TEST.NACCS6 is valid. To update this value, visit CargoWise > Maintain > User Admin > Companies >  > Brokerage > NACCS Mailbox. 
If the issue persists. Please raise an eRequest (Product: CargoWise; Module: Customs; Country: Japan) and provide the following exception message to the support. You can raise an eRequest by pressing F1 in CargoWise.";

				var logs = new []
				{
					"[NACCSMessageSenderHandler] An exception [Exception] happen, pause 0.1 second(s).",
					"[NACCSMessageSenderHandler] Clear Cached Setting.",
					"[NACCSMessageSenderHandler] Load Setting.",
					"[NACCSMessageSenderHandler] SMTP Connect Failed. Mailbox Domain: localhost Local Domain: WebPrint.WisetechGlobal.com",
					"Msg:100 has been acknowledged with status StatusFailed/0",
					"Msg:200 has not been acknowledged and would retry in next run/0"
				};

				ProcessAndAssertSubmitMessage<Exception>(BuildMessageClient, cancellationTokenSource.Token, "WebPrint_WTGCO-LTST-1", "Connect Error", logs, prefixTips + "\n\n");
			}
		}

		public void TestConnectWithCommandError()
		{
			using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
			{
				ISendMessageClient BuildMessageClient()
				{
					var mockClient = new Mock<ISendMessageClient>();
					mockClient.Setup(c => c.IsConnected).Returns(false);
					mockClient.Setup(c => c.ConnectMaxRetries).Returns(2);
					mockClient.Setup(c => c.Connect(It.IsAny<string>())).Callback(() =>
					{
						cancellationTokenSource.Cancel();
						throw new SmtpCommandException(SmtpErrorCode.MessageNotAccepted, SmtpStatusCode.SyntaxError, "Connect SmtpCommandException");
					});

					return mockClient.Object;
				}

				var logs = new[]
				{
					"[NACCSMessageSenderHandler] Load Setting.",
					"[NACCSMessageSenderHandler] SMTP Execute Command Failed. Domain: localhost MsgId: 100",
					"Msg:100 has been acknowledged with status StatusFailed/0",
					"Msg:200 has not been acknowledged and would retry in next run/0"
				};

				ProcessAndAssertSubmitMessage<SmtpCommandException>(BuildMessageClient, cancellationTokenSource.Token, "NACCS", "Connect SmtpCommandException", logs);
				AssertEquals("[NACCSMessageSenderHandler] Can't connect Domain: localhost.", ErrorReporter.LastMessageReported);
			}
		}

		public void TestSendingWithCommandError()
		{
			using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
			{
				ISendMessageClient BuildMessageClient()
				{
					var mockClient = new Mock<ISendMessageClient>();
					mockClient.Setup(c => c.IsConnected).Returns(true);
					mockClient.Setup(c => c.Interval).Returns(0.1);
					mockClient.Setup(c => c.Send(It.IsAny<MimeMessage>())).Callback(() =>
					{
						cancellationTokenSource.Cancel();
						throw new SmtpCommandException(SmtpErrorCode.MessageNotAccepted, SmtpStatusCode.SyntaxError, "Send SmtpCommandException");
					});

					return mockClient.Object;
				}

				var logs = new []
				{
					"[NACCSMessageSenderHandler] Load Setting.",
					"[NACCSMessageSenderHandler] SMTP Execute Command Failed. Mailbox: NACCS@localhost MsgId: 100",
					"Msg:100 has been acknowledged with status StatusFailed/0",
					"Msg:200 has not been acknowledged and would retry in next run/0"
				};

				ProcessAndAssertSubmitMessage<SmtpCommandException>(BuildMessageClient, cancellationTokenSource.Token, "NACCS", "Send SmtpCommandException", logs);
				AssertEquals("[NACCSMessageSenderHandler] Can't send message 100.", ErrorReporter.LastMessageReported);
			}
		}

		public void TestSendingWithTimeoutError()
		{
			using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
			{
				ISendMessageClient BuildMessageClient()
				{
					var mockClient = new Mock<ISendMessageClient>();
					mockClient.Setup(c => c.IsConnected).Returns(true);
					mockClient.Setup(c => c.Interval).Returns(0.1);
					mockClient.Setup(c => c.CommandMaxRetries).Returns(2);
					mockClient.Setup(c => c.Send(It.IsAny<MimeMessage>())).Callback(() =>
					{
						cancellationTokenSource.Cancel();
						throw new TimeoutException("Send TimeoutException");
					});

					return mockClient.Object;
				}

				var logs = new []
				{
					"[NACCSMessageSenderHandler] Load Setting.",
					"[NACCSMessageSenderHandler] An exception [TimeoutException] happen, pause 0.1 second(s).",
					"[NACCSMessageSenderHandler] SMTP Send Message Failed. Mailbox: NACCS@localhost MsgId: 100",
					"Msg:100 has been acknowledged with status StatusFailed/0",
					"Msg:200 has not been acknowledged and would retry in next run/0"
				};

				ProcessAndAssertSubmitMessage<TimeoutException>(BuildMessageClient, cancellationTokenSource.Token, "WebPrint_WTGCO-LTST-1", "Send TimeoutException", logs);
			}
		}

		public void TestSendingWithServiceNotConnectedError()
		{
			using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
			{
				ISendMessageClient BuildMessageClient()
				{
					var mockClient = new Mock<ISendMessageClient>();
					mockClient.Setup(c => c.IsConnected).Returns(true);
					mockClient.Setup(c => c.Interval).Returns(0.1);
					mockClient.Setup(c => c.CommandMaxRetries).Returns(2);
					mockClient.Setup(c => c.Send(It.IsAny<MimeMessage>())).Callback(() =>
					{
						cancellationTokenSource.Cancel();
						throw new ServiceNotConnectedException("Send ServiceNotConnectedException");
					});

					return mockClient.Object;
				}

				var logs = new []
				{
					"[NACCSMessageSenderHandler] Load Setting.",
					"[NACCSMessageSenderHandler] An exception [ServiceNotConnectedException] happen, pause 0.1 second(s).",
					"[NACCSMessageSenderHandler] SMTP Send Message Failed. Mailbox: NACCS@localhost MsgId: 100",
					"Msg:100 has been acknowledged with status StatusFailed/0",
					"Msg:200 has not been acknowledged and would retry in next run/0"
				};

				ProcessAndAssertSubmitMessage<ServiceNotConnectedException>(BuildMessageClient, cancellationTokenSource.Token, "NACCS", "Send ServiceNotConnectedException", logs);
			}
		}

		void ProcessAndAssertSubmitMessage<T>(Func<ISendMessageClient> clientFunc, CancellationToken cancellationToken, string sender, string errorMsg, string[] logs, string errorTitle = "") where T : Exception
		{
			var fullLogs = new StringBuilder();

			var controller = GetMessageSenderControllerAndTracker(clientFunc, cancellationToken).Controller;
			controller.ShowInformation += (o, e) => { fullLogs.AppendLine(e.Message); };
			controller.ProcessForTesting();

			CombineAssertions(() =>
			{
				var fullLogText = fullLogs.ToString();

				foreach (var log in logs)
				{
					AssertContains("Should contains the expected log.", log, fullLogText);
				}

				var index = 0;

				foreach (var submitMessage in controller.SubmitMessages)
				{
					index++;

					AssertContains($"[{index}] Sender: {sender}", submitMessage);
					AssertContains($"[{index}] ErrorType: TransmissionError", submitMessage);
					AssertContains($"[{index}] Error: {errorTitle}[1] {typeof(T).FullName}: {errorMsg}", submitMessage);

					if (typeof(T) == typeof(SmtpCommandException))
					{
						AssertNotContains($"[2] {typeof(T).FullName}: {errorMsg}", submitMessage);
						AssertNotContains($"[3] {typeof(T).FullName}: {errorMsg}", submitMessage);
					}
					else
					{
						AssertContains($"[2] {typeof(T).FullName}: {errorMsg}", submitMessage);
						AssertContains($"[3] {typeof(T).FullName}: {errorMsg}", submitMessage);
					}

					AssertContains($"[{index}] custom.NACCS.Domain: WebPrint.WisetechGlobal.com", submitMessage);
					AssertContains($"[{index}] custom.NACCS.ServerMailbox: NACCS@localhost", submitMessage);
					AssertContains($"[{index}] custom.NACCS.MessageId: {index}00", submitMessage);
				}
			});
		}

		(JPNACCSMessageSenderControllerForTest Controller, Dictionary<ulong, MsgStatusCommand> AckTracker) GetMessageSenderControllerAndTracker(Func<ISendMessageClient> clientFunc, CancellationToken cancellationToken)
		{
			var incomingMessageList = new Dictionary<ulong, ITestIncomingMessage>();
			incomingMessageList[100u] = new TestIncomingBinaryMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = BuildNACCSDummyMessage("xxx@MAIL.TEST.NACCS6", "ABC", "user1", "123"), AckResponse = ErrorCode.ErrOk };
			incomingMessageList[200u] = new TestIncomingBinaryMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = BuildNACCSDummyMessage("ddd@MAIL.TEST.NACCS6", "XYZ", "user2", "456"), AckResponse = ErrorCode.ErrOk };

			var (msgClientProvider, ackTracker) = TestUtils.GetMockedMsgClientProviderWithIncomingMessages(incomingMessageList);
			return (new JPNACCSMessageSenderControllerForTest("WTGCO-LTST-1", cancellationToken, clientFunc, msgClientProvider), ackTracker);
		}

		sealed class JPNACCSMessageSenderControllerForTest : JPNACCSMessageSenderController
		{
			public JPNACCSMessageSenderControllerForTest(string name, CancellationToken cancellationToken, Func<ISendMessageClient> clientFunc, IMsgClientProvider msgClientProvider)
				: base(name, cancellationToken)
			{
				submitMessages = new List<string>();

				this.clientFunc = clientFunc;
				this.msgClientProvider = msgClientProvider;
				this.cancellationToken = cancellationToken;
			}

			readonly List<string> submitMessages;
			readonly Func<ISendMessageClient> clientFunc;
			readonly IMsgClientProvider msgClientProvider;
			readonly CancellationToken cancellationToken;

			protected override CancellationToken GetCancellationTokenCore() => cancellationToken;

			protected override IMsgClientProvider MsgClientProvider => msgClientProvider;

			protected override CustomseHubClientSettingManager CreateNewSettingManager(string machineName)
			{
				var mockSetting = new Mock<IJPNACCSClientApplicationSetting>();
				mockSetting.Setup(x => x.Verbose).Returns(true);
				mockSetting.Setup(x => x.MachineName).Returns("WTGCO-LTST-1");
				mockSetting.Setup(x => x.DomainName).Returns("WebPrint.WisetechGlobal.com");
				mockSetting.Setup(x => x.NACCSMailbox).Returns("NACCS@localhost");
				mockSetting.Setup(x => x.IsValid).Returns(true);
				mockSetting.Setup(x => x.DirectxTMessagingConfig).Returns(new Cw1DirectxTMessagingConfig());
				mockSetting.Setup(x => x.xTApplicationNode).Returns("WTLTST_JPC");
				mockSetting.Setup(x => x.RetryInterval).Returns(0.1);
				mockSetting.Setup(x => x.MaxRetries).Returns(3);

				var mockSettingManager = new Mock<JPNACCSClientApplicationSettingManager>("", null);
				mockSettingManager.Protected().Setup<Func<WebClient, ICustomseHubClientSetting>>("GetEhubClientSetting").Returns(_ => mockSetting.Object);

				return mockSettingManager.Object;
			}

			protected override WebClientConfiguration GetNewConfigSetting(string configName) => new ();

			public string[] SubmitMessages => submitMessages.ToArray();

			protected override ISendMessageClient GetMessageClientCore() => clientFunc?.Invoke() ?? base.GetMessageClientCore();

			public void ProcessForTesting() => ProcessCore(SettingManager.CurrentSetting);

			protected override INACCSErrorSender GetErrorSenderCore()
			{
				var mockErrorSender = new Mock<INACCSErrorSender>();
				mockErrorSender.Setup(c => c.SettingManager).Returns(SettingManager as JPNACCSClientApplicationSettingManager);
				mockErrorSender.Setup(c => c.Send(It.IsAny<ErrorInfo>(), It.IsAny<MsgIdUri>(), It.IsAny<GetMessageMetaDataForHandling>()))
					.Callback<ErrorInfo, MsgIdUri, GetMessageMetaDataForHandling>((errorInfo, msgId, handler) =>
					{
						var index = submitMessages.Count + 1;
						var log = new StringBuilder()
							.AppendLine($"[{index}] Sender: {errorInfo.SenderID}")
							.AppendLine($"[{index}] ErrorType: {errorInfo.ErrorType}")
							.AppendLine($"[{index}] Error: {errorInfo.ErrorDescription}");

						foreach (var parameter in errorInfo.Parameters)
						{
							log.AppendLine($"[{index}] {parameter.Key}: {parameter.Value}");
						}

						submitMessages.Add(log.ToString());
					});

				return mockErrorSender.Object;
			}
		}

		byte[] BuildNACCSDummyMessage(string mailAddress, string businessCode, string userID, string messageReference)
		{
			var mimeHeader = $@"Date: Tue, 02 Jan 2018 10:50:28 +0900
From: {mailAddress}
To: NACCS@MAIL.TEST.NACCS6
Message-ID: <xxxx@MAIL.TEST.NACCS6>
Mime-Version: 1.0
Content-Type: text/plain;charset=""EUC-JP""
Content-Transfer-Encoding: 8bit

";

			var result = string.Empty.PadRight(3);
			result += businessCode.PadRight(5);
			result += string.Empty.PadRight(21);
			result += userID.PadRight(8);
			result += string.Empty.PadRight(182);
			result += messageReference.PadRight(26);

			var messageContent = mimeHeader + result.PadRight(398);
			return Encoding.GetEncoding(51932).GetBytes(messageContent);
		}
	}
}
