using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Enterprise.RemotePrinting.Client.RemotePrintServer;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.Shared;
using Enterprise.xTMessaging.Shared.Test;
using MailKit;
using MailKit.Net.Pop3;
using MimeKit;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Xware.Xt.Grpc.Application;

namespace Enterprise.RemotePrinting.Client.Tests
{
	sealed class JPNACCSMessageReceiverControllerTest : TestCase
	{
		public void TestReceive()
		{
			var logs = new StringBuilder();

			var groups = new (string BusinessCode, string UserID, string MessageRef)[]
			{
				("ABC", "user1", "123"),
				("EFG", "user2", "456"),
				("HIJ", "user3", "789"),
				("XYZ", "user4", "012")
			};

			var messageIndex = -1;
			var mimeMessages = groups
				.Select(c => MimeMessage.Load(new MemoryStream(BuildNACCSDummyMessage(c.BusinessCode, c.UserID, c.MessageRef))))
				.ToArray();

			try
			{
				IReceiveMessageClient BuildMessageClient()
				{
					var isAuthenticated = false;

					var mockClient = new Mock<IReceiveMessageClient>();
					mockClient.Setup(c => c.IsConnected).Returns(true);
					mockClient.Setup(c => c.IsAuthenticated).Returns(() => isAuthenticated);
					mockClient.Setup(c => c.Interval).Returns(0.1);
					mockClient.Setup(c => c.Count).Returns(2);
					mockClient.Setup(c => c.ConnectMaxRetries).Returns(1);
					mockClient.Setup(c => c.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Callback((string userName, string password) =>
					{
						logs.AppendLine($"Authenticate - {userName}/{password}.");

						if ((userName == @"test001@user.mail.naccs.com" && password == "@MP012345") || (userName == "test002@user.mail.naccs.com" && password == "@MP678901"))
						{
							isAuthenticated = true;
						}
						else
						{
							throw new ServiceNotAuthenticatedException("Invalid UserName and Password.");
						}
					});
					mockClient.Setup(c => c.GetMessage(It.IsAny<int>())).Returns((int index) =>
					{
						messageIndex++;
						logs.AppendLine($"Get Message - {messageIndex}.");

						return mimeMessages[messageIndex];
					});
					mockClient.Setup(c => c.DeleteMessage(It.IsAny<int>())).Callback((int index) =>
					{
						var currentMessage = mimeMessages[messageIndex];
						currentMessage.Dispose();

						logs.AppendLine($"Delete Message - {messageIndex}.");
					});

					return mockClient.Object;
				}

				(var mockMsgClientProvider, var messages, var byteChunks) = TestUtils.GetMockedMsgClientProviderWithMessageInspection();

				var controller = new JPNACCSMessageReceiverControllerForTest("WTGCO-LTST-1", cts.Token, BuildMessageClient, mockMsgClientProvider);
				controller.ShowInformation += (o, e) => { logs.AppendLine(e.Message); };
				controller.ProcessForTesting();

				CombineAssertions($"Should receive 4 messages from the mailbox then send them to the DxT.{System.Environment.NewLine}Logs:{System.Environment.NewLine}{logs}", () =>
				{
					AssertEquals("Should send 4 messages to the DxT.", 4, messages.Count());

					foreach (var message in messages)
					{
						var messageType = message.Msgattr["custom.MessageType"];
						var group = groups.FirstOrDefault(c => c.BusinessCode == messageType);

						AssertNotNull(group);
						AssertEquals("custom.ApplicationCode", "JPC", message.Msgattr["custom.ApplicationCode"]);
						AssertEquals("custom.SourceParty", "NACCS", message.Msgattr["custom.SourceParty"]);
						AssertEquals("custom.DestinationParty", group.UserID, message.Msgattr["custom.DestinationParty"]);
						AssertEquals("custom.Receiver", "WTLTST", message.Parserattr[(int)StdParserFieldId.PfReceiver]);
					}
				});
			}
			finally
			{
				foreach (var mimeMessage in mimeMessages)
				{
					mimeMessage.Dispose();
				}

				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
		}

		byte[] BuildNACCSDummyMessage(string businessCode, string userID, string messageReference)
		{
			var result = @"Date: Tue, 02 Jan 2018 10:50:28 +0900
From: NACCS@MAIL.TEST.NACCS6
To: xxx@MAIL.TEST.NACCS6
Message-ID: <xxxx@MAIL.TEST.NACCS6>
Mime-Version: 1.0
Content-Type: Text/plain;charset=""EUC-JP""
Content-Transfer-Encoding: 8bit

";

			result += string.Empty.PadRight(3);
			result += businessCode.PadRight(5);
			result += string.Empty.PadRight(21);
			result += userID.PadRight(8);
			result += string.Empty.PadRight(182);
			result += messageReference.PadRight(26);

			return Encoding.GetEncoding(51932).GetBytes(result.PadRight(398));
		}

		public void TestReceivingWithConnectError()
		{
			IReceiveMessageClient BuildMessageClient()
			{
				var mockClient = new Mock<IReceiveMessageClient>();
				mockClient.Setup(c => c.IsConnected).Returns(false);
				mockClient.Setup(c => c.IsAuthenticated).Returns(false);
				mockClient.Setup(c => c.Interval).Returns(0.1);
				mockClient.Setup(c => c.Count).Returns(1);
				mockClient.Setup(c => c.Connect(It.IsAny<string>())).Throws(new Exception("Connect Error"));
				mockClient.Setup(c => c.ConnectMaxRetries).Returns(2);

				return mockClient.Object;
			}

			var prefixTipsLogs = new[]
			{
				@"POP3 connection fails. Please check the followings: 
	1.You are connected to the NACCS internet via a NACCS router. 
	2.The mailbox test001@user.mail.naccs.com is valid. To update this value, visit CargoWise > Maintain > User Admin > Companies > TST > Brokerage > NACCS Mailbox. 
If the issue persists. Please raise an eRequest (Product: CargoWise; Module: Customs; Country: Japan) and provide the following exception message to the support. You can raise an eRequest by pressing F1 in CargoWise.",
				@"POP3 connection fails. Please check the followings: 
	1.You are connected to the NACCS internet via a NACCS router. 
	2.The mailbox test002@user.mail.naccs.com is valid. To update this value, visit CargoWise > Maintain > User Admin > Companies > TST > Brokerage > NACCS Mailbox. 
If the issue persists. Please raise an eRequest (Product: CargoWise; Module: Customs; Country: Japan) and provide the following exception message to the support. You can raise an eRequest by pressing F1 in CargoWise."
			};
			var logs = new[] { "[NACCSMessageReceiverHandler] POP3 Connect Failed. Domain: localhost" };
			var attributes = new[] { "custom.NACCS.Domain: localhost" };

			ProcessAndAssertSubmitMessage<Exception>(BuildMessageClient, "WebPrint_WTGCO-LTST-1", "localhost", "TransmissionError", "Connect Error", logs, attributes, prefixTipsLogs);
		}

		public void TestReceivingWithAuthenticateError()
		{
			IReceiveMessageClient BuildMessageClient()
			{
				var mockClient = new Mock<IReceiveMessageClient>();
				mockClient.Setup(c => c.IsConnected).Returns(true);
				mockClient.Setup(c => c.IsAuthenticated).Returns(false);
				mockClient.Setup(c => c.Interval).Returns(0.1);
				mockClient.Setup(c => c.Count).Returns(1);
				mockClient.Setup(c => c.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Throws(new ServiceNotAuthenticatedException("Authenticate Error"));
				mockClient.Setup(c => c.AuthenticateMaxRetries).Returns(2);

				return mockClient.Object;
			}

			var logs = new[]
			{
				"[NACCSMessageReceiverHandler] POP3 Authenticate Failed. Mailbox: test001@user.mail.naccs.com",
				"[NACCSMessageReceiverHandler] POP3 Authenticate Failed. Mailbox: test002@user.mail.naccs.com"
			};

			var attributes = new[] { "custom.NACCS.CompanyCode: TST", "custom.NACCS.ClientMailbox: test00{0}@user.mail.naccs.com" };

			ProcessAndAssertSubmitMessage<ServiceNotAuthenticatedException>(BuildMessageClient, "NACCS", "test00{0}@user.mail.naccs.com", "Unauthorized", "Authenticate Error", logs, attributes);
		}

		public void TestReceivingWithTransmitError()
		{
			IReceiveMessageClient BuildMessageClient()
			{
				var mockClient = new Mock<IReceiveMessageClient>();
				mockClient.Setup(c => c.IsConnected).Returns(true);
				mockClient.Setup(c => c.IsAuthenticated).Returns(false);
				mockClient.Setup(c => c.Interval).Returns(0.1);
				mockClient.Setup(c => c.Count).Returns(1);
				mockClient.Setup(c => c.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception("Transmit Error"));
				mockClient.Setup(c => c.AuthenticateMaxRetries).Returns(2);

				return mockClient.Object;
			}

			var logs = new[]
			{
				"[NACCSMessageReceiverHandler] POP3 Transmit Failed. Mailbox: test001@user.mail.naccs.com",
				"[NACCSMessageReceiverHandler] POP3 Transmit Failed. Mailbox: test002@user.mail.naccs.com"
			};

			var attributes = new[] { "custom.NACCS.CompanyCode: TST", "custom.NACCS.ClientMailbox: test00{0}@user.mail.naccs.com" };

			ProcessAndAssertSubmitMessage<Exception>(BuildMessageClient, "NACCS", "test00{0}@user.mail.naccs.com", "TransmissionError", "Transmit Error", logs, attributes);
		}

		public void TestConnectWithCommandError()
		{
			IReceiveMessageClient BuildMessageClient()
			{
				var mockClient = new Mock<IReceiveMessageClient>();
				mockClient.Setup(c => c.IsConnected).Returns(false);
				mockClient.Setup(c => c.IsAuthenticated).Returns(false);
				mockClient.Setup(c => c.Interval).Returns(0.1);
				mockClient.Setup(c => c.Count).Returns(1);
				mockClient.Setup(c => c.Connect(It.IsAny<string>())).Throws(new Pop3CommandException("Connect Pop3CommandException"));
				mockClient.Setup(c => c.ConnectMaxRetries).Returns(2);

				return mockClient.Object;
			}

			var logs = new[]
			{
				"[NACCSMessageReceiverHandler] POP3 Execute Command Failed. Mailbox: test001@user.mail.naccs.com Domain: localhost",
				"[NACCSMessageReceiverHandler] POP3 Execute Command Failed. Mailbox: test002@user.mail.naccs.com Domain: localhost"
			};

			var attributes = new[] { "custom.NACCS.CompanyCode: TST", "custom.NACCS.ClientMailbox: test00{0}@user.mail.naccs.com" };

			ProcessAndAssertSubmitMessage<Pop3CommandException>(BuildMessageClient, "NACCS", "test00{0}@user.mail.naccs.com", "TransmissionError", "Connect Pop3CommandException", logs, attributes);
			AssertEquals("[NACCSMessageReceiverHandler] Can't receive messages from Mailbox: test002@user.mail.naccs.com Domain: localhost.", ErrorReporter.LastMessageReported);
		}

		public void TestAuthenticateWithCommandError()
		{
			IReceiveMessageClient BuildMessageClient()
			{
				var mockClient = new Mock<IReceiveMessageClient>();
				mockClient.Setup(c => c.IsConnected).Returns(true);
				mockClient.Setup(c => c.IsAuthenticated).Returns(false);
				mockClient.Setup(c => c.Interval).Returns(0.1);
				mockClient.Setup(c => c.Count).Returns(1);
				mockClient.Setup(c => c.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Throws(new Pop3CommandException("Authenticate Pop3CommandException"));
				mockClient.Setup(c => c.AuthenticateMaxRetries).Returns(2);

				return mockClient.Object;
			}

			var logs = new[]
			{
				"[NACCSMessageReceiverHandler] POP3 Execute Command Failed. Mailbox: test001@user.mail.naccs.com Domain: localhost",
				"[NACCSMessageReceiverHandler] POP3 Execute Command Failed. Mailbox: test002@user.mail.naccs.com Domain: localhost"
			};

			var attributes = new[] { "custom.NACCS.CompanyCode: TST", "custom.NACCS.ClientMailbox: test00{0}@user.mail.naccs.com" };

			ProcessAndAssertSubmitMessage<Pop3CommandException>(BuildMessageClient, "NACCS", "test00{0}@user.mail.naccs.com", "TransmissionError", "Authenticate Pop3CommandException", logs, attributes);
			AssertEquals("[NACCSMessageReceiverHandler] Can't receive messages from Mailbox: test002@user.mail.naccs.com Domain: localhost.", ErrorReporter.LastMessageReported);
		}

		public void TestReceivingWithCommandError()
		{
			IReceiveMessageClient BuildMessageClient()
			{
				var mockClient = new Mock<IReceiveMessageClient>();
				mockClient.Setup(c => c.IsConnected).Returns(true);
				mockClient.Setup(c => c.IsAuthenticated).Returns(true);
				mockClient.Setup(c => c.Interval).Returns(0.1);
				mockClient.Setup(c => c.Count).Returns(1);
				mockClient.Setup(c => c.GetMessage(It.IsAny<int>())).Throws(new Pop3CommandException("GetMessage Pop3CommandException"));

				return mockClient.Object;
			}

			var logs = new []
			{
				"[NACCSMessageReceiverHandler] POP3 Execute Command Failed. Mailbox: test001@user.mail.naccs.com Domain: localhost",
				"[NACCSMessageReceiverHandler] POP3 Execute Command Failed. Mailbox: test002@user.mail.naccs.com Domain: localhost"
			};

			var attributes = new[] { "custom.NACCS.CompanyCode: TST", "custom.NACCS.ClientMailbox: test00{0}@user.mail.naccs.com" };

			ProcessAndAssertSubmitMessage<Pop3CommandException>(BuildMessageClient, "NACCS", "test00{0}@user.mail.naccs.com", "TransmissionError", "GetMessage Pop3CommandException", logs, attributes);
			AssertEquals("[NACCSMessageReceiverHandler] Can't receive messages from Mailbox: test002@user.mail.naccs.com Domain: localhost.", ErrorReporter.LastMessageReported);
		}

		public void TestReceivingWithTimeoutError()
		{
			IReceiveMessageClient BuildMessageClient()
			{
				var mockClient = new Mock<IReceiveMessageClient>();
				mockClient.Setup(c => c.IsConnected).Returns(true);
				mockClient.Setup(c => c.IsAuthenticated).Returns(true);
				mockClient.Setup(c => c.Interval).Returns(0.1);
				mockClient.Setup(c => c.Count).Returns(1);
				mockClient.Setup(c => c.GetMessage(It.IsAny<int>())).Throws(new TimeoutException("GetMessage TimeoutException"));
				mockClient.Setup(c => c.CommandMaxRetries).Returns(2);

				return mockClient.Object;
			}

			var logs = new[]
			{
				"[NACCSMessageReceiverHandler] POP3 Get Message Failed. Mailbox: test001@user.mail.naccs.com",
				"[NACCSMessageReceiverHandler] POP3 Get Message Failed. Mailbox: test002@user.mail.naccs.com"
			};

			var attributes = new[] { "custom.NACCS.CompanyCode: TST", "custom.NACCS.ClientMailbox: test00{0}@user.mail.naccs.com" };

			ProcessAndAssertSubmitMessage<TimeoutException>(BuildMessageClient, "WebPrint_WTGCO-LTST-1", "test00{0}@user.mail.naccs.com", "TransmissionError", "GetMessage TimeoutException", logs, attributes);
		}

		public void TestReceivingWithServiceNotConnectedError()
		{
			IReceiveMessageClient BuildMessageClient()
			{
				var mockClient = new Mock<IReceiveMessageClient>();
				mockClient.Setup(c => c.IsConnected).Returns(true);
				mockClient.Setup(c => c.IsAuthenticated).Returns(true);
				mockClient.Setup(c => c.Interval).Returns(0.1);
				mockClient.Setup(c => c.Count).Returns(1);
				mockClient.Setup(c => c.GetMessage(It.IsAny<int>())).Throws(new ServiceNotConnectedException("GetMessage ServiceNotConnectedException"));
				mockClient.Setup(c => c.CommandMaxRetries).Returns(2);

				return mockClient.Object;
			}

			var logs = new[]
			{
				"[NACCSMessageReceiverHandler] POP3 Get Message Failed. Mailbox: test001@user.mail.naccs.com",
				"[NACCSMessageReceiverHandler] POP3 Get Message Failed. Mailbox: test002@user.mail.naccs.com"
			};

			var attributes = new[] { "custom.NACCS.ClientMailbox: test00{0}@user.mail.naccs.com" };

			ProcessAndAssertSubmitMessage<ServiceNotConnectedException>(BuildMessageClient, "NACCS", "test00{0}@user.mail.naccs.com", "TransmissionError", "GetMessage ServiceNotConnectedException", logs, attributes);
		}

		void ProcessAndAssertSubmitMessage<T>(Func<IReceiveMessageClient> clientFunc, string sender, string recipient, string errorType, string errorMsg, string[] logs, string[] attributes, string[] prefixTipLogs = null) where T : Exception
		{
			var mockMsgClientProviderWithMessageInspection = TestUtils.GetMockedMsgClientProviderWithMessageInspection();
			var msgClientProvider = mockMsgClientProviderWithMessageInspection.Item1;

			var fullLogs = new StringBuilder();

			var controller = new JPNACCSMessageReceiverControllerForTest("WTGCO-LTST-1", cts.Token, clientFunc, msgClientProvider);
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
					AssertContains($"[{index}] Recipient: {string.Format(recipient, index)}", submitMessage);
					AssertContains($"[{index}] ErrorType: {errorType}", submitMessage);
					if (prefixTipLogs == null)
					{
						AssertContains($"[{index}] Error: [1] {typeof(T).FullName}: {errorMsg}", submitMessage);
					}
					else
					{
						AssertContains($"[{index}] Error: {prefixTipLogs[index - 1]}\n\n[1] {typeof(T).FullName}: {errorMsg}", submitMessage);
					}

					if (typeof(T) == typeof(Pop3CommandException))
					{
						AssertNotContains($"[2] {typeof(T).FullName}: {errorMsg}", submitMessage);
						AssertNotContains($"[3] {typeof(T).FullName}: {errorMsg}", submitMessage);
					}
					else
					{
						AssertContains($"[2] {typeof(T).FullName}: {errorMsg}", submitMessage);
						AssertContains($"[3] {typeof(T).FullName}: {errorMsg}", submitMessage);
					}

					foreach (var attribute in attributes)
					{
						AssertContains($"[{index}] {string.Format(attribute, index)}", submitMessage);
					}
				}
			});
		}

		CancellationTokenSource cts;

		protected override void SetUp()
		{
			base.SetUp();
			cts = new CancellationTokenSource();
		}

		protected override void TearDown()
		{
			try
			{
				cts.Cancel();
			}
			finally
			{
				cts.Dispose();
			}
			base.TearDown();
		}

		sealed class JPNACCSMessageReceiverControllerForTest : JPNACCSMessageReceiverController
		{
			public JPNACCSMessageReceiverControllerForTest(string name, CancellationToken cancellationToken, Func<IReceiveMessageClient> clientFunc, IMsgClientProvider msgClientProvider)
				: base(name, cancellationToken)
			{
				submitMessages = new List<string>();

				this.clientFunc = clientFunc;
				this.msgClientProvider = msgClientProvider;
			}

			readonly List<string> submitMessages;
			readonly Func<IReceiveMessageClient> clientFunc;
			readonly IMsgClientProvider msgClientProvider;

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
				mockSetting.Setup(x => x.Mailboxes).Returns(new []
				{
					new MailBoxInfo() { CompanyCode = "TST", MailBox = "test001@user.mail.naccs.com", DecryptedMailBoxPassword = "@MP012345" },
					new MailBoxInfo() { CompanyCode = "TST", MailBox = "test002@user.mail.naccs.com", DecryptedMailBoxPassword = "@MP678901" },
				});

				var mockSettingManager = new Mock<JPNACCSClientApplicationSettingManager>("", null);
				mockSettingManager.Protected().Setup<Func<WebClient, ICustomseHubClientSetting>>("GetEhubClientSetting").Returns(_ => mockSetting.Object);

				return mockSettingManager.Object;
			}

			public string[] SubmitMessages => submitMessages.ToArray();

			protected override IReceiveMessageClient GetMessageClientCore() => clientFunc?.Invoke() ?? base.GetMessageClientCore();

			public void ProcessForTesting() => ProcessCore(SettingManager.CurrentSetting);

			protected override WebClientConfiguration GetNewConfigSetting(string configName) => new ();

			protected override INACCSErrorSender GetErrorSenderCore()
			{
				var mockErrorSender = new Mock<INACCSErrorSender>();
				mockErrorSender.Setup(c => c.SettingManager).Returns(SettingManager as JPNACCSClientApplicationSettingManager);
				mockErrorSender.Setup(c => c.Send(It.IsAny<ErrorInfo>()))
					.Callback<ErrorInfo>((errorInfo) =>
					{
						var index = submitMessages.Count + 1;
						var log = new StringBuilder()
							.AppendLine($"[{index}] Sender: {errorInfo.SenderID}")
							.AppendLine($"[{index}] Recipient: {errorInfo.RecipientID}")
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
	}
}
