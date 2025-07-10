using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Enterprise.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.Shared;
using Enterprise.xTMessaging.Shared.Test;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Xware.Xt.Grpc.Application;

namespace Enterprise.RemotePrinting.Client.Tests;

sealed class NACCSErrorSenderTest : TestCase
{
	public void TestSendErrorWithoutSourceMessage()
	{
		var logs = new List<string>();

		var parameters = new KeyValuePair<string, string>[] { new(@"custom.TestAttr", "20240820") };

		var errorSenderWithSubmitMessages = GetErrorSenderAndSubmitMessages(logs);

		var errorSender = errorSenderWithSubmitMessages.ErrorSender;
		var errorInfo = new ErrorInfo { SenderID = "NACCS", RecipientID = "TESTParty", ErrorType = NACCSConstants.ErrorTypes.TransmissionError, ErrorDescription = "Test Error", Parameters = parameters };
		errorSender.Send(errorInfo);

		var submitMessages = errorSenderWithSubmitMessages.SubmitMessages.SingleOrDefault();
		var submittedPayload = errorSenderWithSubmitMessages.SubmittedPayload;

		CombineAssertions(() =>
		{
			var expectedAttributes = new Dictionary<string, string>()
			{
				{ xTMessaging.Shared.Constants.CustomMsgAttributes.ApplicationCode, NACCSConstants.ApplicationCode },
				{ xTMessaging.Shared.Constants.CustomMsgAttributes.MessageType, NACCSConstants.ErrorMessageType },
				{ xTMessaging.Shared.Constants.CustomMsgAttributes.MessageTrackingID, NACCSConstants.EmptyTrackingId },
				{ xTMessaging.Shared.Constants.CustomMsgAttributes.SourceParty, NACCSConstants.WebPrintParty },
				{ xTMessaging.Shared.Constants.CustomMsgAttributes.DestinationParty, "WTLTST" },
				{ NACCSConstants.Attributes.ProtocolType, "SMTP" },
				{ "custom.TestAttr", "20240820" }
			};

			AssertContainsExactElementsInAnyOrder("Should export expected attributes", expectedAttributes, submitMessages.Msgattr);

			var expectedMessageData = GetFileContent("MsgWithValidEmptyId.txt");
			var actualMessageData = string.Join(string.Empty, submittedPayload.Select(x => x.Chunk.ToStringUtf8()));
			AssertMultilineEquals("MessageBody", expectedMessageData, actualMessageData, '\n', includeEmptyLines: true);
		});
	}

	public void TestSendErrorWithSourceMessage()
	{
		var logs = new List<string>();

		var parameters = new KeyValuePair<string, string>[] { new("custom.TestAttr", "20240820") };
		var errorSenderWithSubmitMessages = GetErrorSenderAndSubmitMessages(logs);

		var msgTrackingId = Guid.NewGuid().ToString();
		var createMessagId = Guid.NewGuid().ToString();

		Dictionary<string, string> GetMessageMetaDataForHandling(MsgIdUri msgId)
		{
			return new Dictionary<string, string>()
			{
				{ xTMessaging.Shared.Constants.CustomMsgAttributes.ApplicationCode, NACCSConstants.ApplicationCode },
				{ xTMessaging.Shared.Constants.CustomMsgAttributes.MessageType, "EDA" },
				{ xTMessaging.Shared.Constants.CustomMsgAttributes.MessageTrackingID, msgTrackingId },
				{ xTMessaging.Shared.Constants.CustomMsgAttributes.SourceParty, "WTLTST" },
				{ xTMessaging.Shared.Constants.CustomMsgAttributes.DestinationParty, "WTLTST_JPC" },
				{ xTMessaging.Shared.Constants.CustomMsgAttributes.CreateEDIMessage, createMessagId }
			};
		}

		var errorSender = errorSenderWithSubmitMessages.ErrorSender;
		var errorInfo = new ErrorInfo { SenderID = "NACCS", RecipientID = "WTLTST", ErrorType = NACCSConstants.ErrorTypes.TransmissionError, ErrorDescription = "Test Error", Parameters = parameters };
		errorSender.Send(errorInfo, new MsgIdUri { Msgid = 12345 }, GetMessageMetaDataForHandling);

		var submitMessages = errorSenderWithSubmitMessages.SubmitMessages.SingleOrDefault();
		var submittedPayload = errorSenderWithSubmitMessages.SubmittedPayload;

		CombineAssertions(() =>
		{
			var expectedAttributes = new Dictionary<string, string>()
			{
				{ xTMessaging.Shared.Constants.CustomMsgAttributes.ApplicationCode, NACCSConstants.ApplicationCode },
				{ xTMessaging.Shared.Constants.CustomMsgAttributes.MessageType, NACCSConstants.ErrorMessageType },
				{ xTMessaging.Shared.Constants.CustomMsgAttributes.MessageTrackingID, msgTrackingId },
				{ xTMessaging.Shared.Constants.CustomMsgAttributes.SourceParty, NACCSConstants.WebPrintParty },
				{ xTMessaging.Shared.Constants.CustomMsgAttributes.DestinationParty, "WTLTST" },
				{ NACCSConstants.Attributes.ProtocolType, "SMTP" },
				{ "custom.TestAttr", "20240820" }
			};

			AssertContainsExactElementsInAnyOrder("Should export expected attributes", expectedAttributes, submitMessages.Msgattr);

			var expectedMessageData = GetFileContent("MsgWithValidTrackingId.txt");
			var actualMessageData = string.Join(string.Empty, submittedPayload.Select(x => x.Chunk.ToStringUtf8()));
			AssertMultilineEquals("MessageBody", expectedMessageData, actualMessageData, '\n', includeEmptyLines: true);
		});
	}

	string GetFileContent(string fileName)
	{
		using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($@"Enterprise.RemotePrinting.Client.Tests.Customs.TestFiles.NACCS.{fileName}"))
		using (var reader = new StreamReader(stream))
		{
			return reader.ReadToEnd();
		}
	}

	(NACCSErrorSender ErrorSender, IEnumerable<SubmitMsgMessage> SubmitMessages, IEnumerable<ByteChunk> SubmittedPayload) GetErrorSenderAndSubmitMessages(List<string> logs)
	{
		var mockSetting = new Mock<IJPNACCSClientApplicationSetting>();
		mockSetting.Setup(x => x.Verbose).Returns(true);
		mockSetting.Setup(x => x.DomainName).Returns("WebPrint.WisetechGlobal.com");
		mockSetting.Setup(x => x.NACCSMailbox).Returns("NACCS@localhost");
		mockSetting.Setup(x => x.IsValid).Returns(true);
		mockSetting.Setup(x => x.DirectxTMessagingConfig).Returns(new Cw1DirectxTMessagingConfig());
		mockSetting.Setup(x => x.xTApplicationNode).Returns("WTLTST_JPC");

		var mockSettingManager = new Mock<JPNACCSClientApplicationSettingManager>("", null);
		mockSettingManager.Protected().Setup<Func<WebClient, ICustomseHubClientSetting>>("GetEhubClientSetting").Returns(_ => mockSetting.Object);

		var mockLogger = new Mock<ILogger>();
		mockLogger.Setup(c => c.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback<LogType, string>((logType, log) => logs.Add($"[{logType}] {log}"));

		var mockMsgClientProviderWithMessageInspection = TestUtils.GetMockedMsgClientProviderWithMessageInspection();

		var msgClientProvider = mockMsgClientProviderWithMessageInspection.Item1;
		var submitMessages = mockMsgClientProviderWithMessageInspection.Item2;
		var submittedPayload = mockMsgClientProviderWithMessageInspection.Item3;

		return (new NACCSErrorSenderForTest(mockSettingManager.Object, msgClientProvider, mockLogger.Object), submitMessages, submittedPayload);
	}

	sealed class NACCSErrorSenderForTest : NACCSErrorSender
	{
		public NACCSErrorSenderForTest(JPNACCSClientApplicationSettingManager settingManager, IMsgClientProvider msgClientProvider, ILogger logger)
			: base(ProtocolType.SMTP, settingManager, msgClientProvider, logger)
		{
		}

		protected override string GetNotificationTime() => "2024-08-20 00:00:00";
	}
}
