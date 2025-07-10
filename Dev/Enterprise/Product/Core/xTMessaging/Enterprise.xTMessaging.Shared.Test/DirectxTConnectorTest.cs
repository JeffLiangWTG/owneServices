using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Grpc.Core;
using NUnit.Framework;
using Xware.Xt.Grpc.Application;
using static Enterprise.xTMessaging.Shared.Test.TestUtils;

namespace Enterprise.xTMessaging.Shared.Test
{
	sealed class DirectxTConnectorTest : TestCaseWithFactory
	{
		#region Sending

		public async void TestSending_SendingFailure()
		{
			var testLogger = new TestLogger();
			var interchange = CreateInterchangeForXT(Factory);
			var sessionID = ZGuid.NewZGuid();
			interchange.EI_SessionGUID = sessionID;
			Factory.Save();

			var (msgClientProvider, _, _) = GetMockedMsgClientProviderWithMessageInspection(ErrorCode.ErrInternal);
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger);
			connector.InitializeIfNeeded();
			var (sendResult, msgId, errMessage) = await connector.SendInterchange(new XtMessageInfo(interchange, testLogger), CancellationToken.None);

			AssertEquals("msgId should be 0 if sending failed", 0, msgId);
			AssertEquals("Sending failed when not ErrorCode.OK returned", false, sendResult);
			AssertEquals("SendingResult", $"Rejected by xT Server. Error 'ErrInternal - Internal unexpected error (6)' returned.", errMessage);
		}

		public async void TestSending_CancellationToken()
		{
			var testLogger = new TestLogger();
			var cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = cancellationTokenSource.Token;
			var interchange = CreateInterchangeForXT(Factory);
			interchange.EI_BodyText = TestBodyText;
			Factory.Save();

			var msgClientProvider = GetMockedMsgClientProviderWithSubmitMsgDelay();
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger);
			connector.InitializeIfNeeded();
			cancellationTokenSource.CancelAfter(2000);

			try
			{
				await connector.SendInterchange(new XtMessageInfo(interchange, testLogger), cancellationToken);
			}
			catch (Exception ex)
			{
				AssertType<OperationCanceledException>("CancellationToken should be canceled by now", ex);
			}
		}

		public void TestSending_ExceedDeadline_SubmitMsg()
		{
			AssertSendingExceedDeadline(MsgMethod.SubmitMsgAsync, false);
		}

		public void TestSending_ExceedDeadline_WriteMsgDataStream()
		{
			AssertSendingExceedDeadline(MsgMethod.WriteMsgDataStream, true);
		}

		void AssertSendingExceedDeadline(MsgMethod msgMethod, bool isAsyncMethod)
		{
			var testLogger = new TestLogger();
			var interchange = CreateInterchangeForXT(Factory);
			Factory.Save();

			var msgClientProvider = GetMockMsgClientProviderForDeadlineTesting(msgMethod);
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger);
			connector.InitializeIfNeeded();

			var methodName = Enum.GetName(typeof(MsgMethod), msgMethod);
			var ex = AssertExceptionThrown<MsgServerConnectionException>(methodName + " MsgServerConnectionException",
				$"Direct xT Client - DeadlineExceeded Error",
				() => (_, _, _) = connector.SendInterchange(new XtMessageInfo(interchange, testLogger), CancellationToken.None).GetAwaiter().GetResult());
			AsyncHelper.WaitAllActiveTasksForTest();
			AsyncHelper.WaitAllActiveTasksForTest();
			var errorMsg = Utils.GetDeadlineExceededErrorMessage(TimeSpan.FromSeconds(DirectxTMessagingRegistry.Instance.XTServerMessageTimeoutInSeconds.Value));
			AssertEquals(methodName + " ErrorDetail", isAsyncMethod ? $"Send interchange(MessageTrackingID={interchange.EI_SessionGUID}){errorMsg}" : $"Send interchange(MessageTrackingID={interchange.EI_SessionGUID}) | {methodName} Deadline Exceeded", ex.ErrorDetail);
			AsyncHelper.WaitAllActiveTasksForTest();
		}

		public void TestIMessageAttributeProvider()
		{
			var testLogger = new TestLogger();
			var msgClientProvider = GetMockedMsgClientProviderWithMessageInspection().Item1;
			var interchange = Factory.New<EDIInterchange_WithIMessageAttributeProvider_ForTest>();
			var trackingId = Guid.NewGuid();
			interchange.EI_ApplicationCode = "APP";
			interchange.EI_InterchangeType = "TYP";
			interchange.EI_SessionGUID = trackingId;
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			interchange.EI_HeaderNText = "{\"existedKey\", \"existedValue\"}";
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger);
			var errMsg = connector.GetAndValidateOutgoingMessageMetaData(new XtMessageInfo(interchange, testLogger), out var testResult);
			AssertEquals(string.Empty, errMsg);
			AssertEquals(testResult.Count, 7);
			AssertEquals(testResult.Count(t => string.IsNullOrEmpty(t.Key)), 0);
			AssertEquals(testResult["custom.ApplicationCode"], "APP");
			AssertEquals(testResult["custom.MessageType"], "TYP");
			AssertEquals(testResult["custom.New"], "customNewValue");
			AssertEquals(testResult["existedKey"], "existedValue");
			AssertEquals(testResult["custom.SourceParty"], "From");
			AssertEquals(testResult["custom.DestinationParty"], "To");
			AssertEquals(testResult["custom.MessageTrackingID"], trackingId.ToString());
		}

		public void TestGetAndValidateOutgoingMessageMetaData_CatchesExceptionsAndReturnsErrMsg()
		{
			var testLogger = new TestLogger();
			var msgClientProvider = TestUtils.GetMockedMsgClientProviderWithMessageInspection().Item1;
			var interchange = Factory.New<EDIInterchange_WithIMessageAttributeProviderThrowsException_ForTest>();
			var trackingId = Guid.NewGuid();
			interchange.EI_ApplicationCode = "APP";
			interchange.EI_InterchangeType = "TYP";
			interchange.EI_SessionGUID = trackingId;
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger);
			var errMsg = connector.GetAndValidateOutgoingMessageMetaData(new XtMessageInfo(interchange, testLogger), out _);
			AssertEquals("Could not create Message attributes dictionary: Exception occurred during processing: Something went wrong!. ", errMsg);
		}

		public void TestGetAndValidateOutgoingMessageMetaData_EnsuresValidMessageTrackingId()
		{
			var testLogger = new TestLogger();
			var msgClientProvider = TestUtils.GetMockedMsgClientProviderWithMessageInspection().Item1;
			var interchange = Factory.New<EDIInterchange>();
			var trackingId = Guid.NewGuid();
			interchange.EI_ApplicationCode = "APP";
			interchange.EI_InterchangeType = "TYP";
			interchange.EI_SessionGUID = Guid.Empty;
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger);
			var errMsg = connector.GetAndValidateOutgoingMessageMetaData(new XtMessageInfo(interchange, testLogger), out _);
			AssertContains("Could not create Message attributes dictionary: MessageTrackingID (EI_SessionGUID) must be a valid GUID. ", errMsg);
		}

		public async void TestSendInterchangeReturnsFailureWhenGetAndValidateOutgoingMessageMetaData_ReturnsErrMsg()
		{
			var testLogger = new TestLogger();
			var msgClientProvider = TestUtils.GetMockedMsgClientProviderWithMessageInspection().Item1;
			var interchange = Factory.New<EDIInterchange_WithIMessageAttributeProviderThrowsException_ForTest>();
			var trackingId = Guid.NewGuid();
			interchange.EI_ApplicationCode = "APP";
			interchange.EI_InterchangeType = "TYP";
			interchange.EI_SessionGUID = trackingId;
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger);
			connector.InitializeIfNeeded();
			var (sendResult, _, errMsg) = await connector.SendInterchange(new XtMessageInfo(interchange, testLogger), CancellationToken.None);
			Assert(!sendResult);
			AssertEquals("Could not create Message attributes dictionary: Exception occurred during processing: Something went wrong!. ", errMsg);
		}

		public async void TestSending()
		{
			var testLogger = new TestLogger();
			var testBody = TestBodyText;
			var interchange = CreateInterchangeForXT(Factory);
			interchange.EI_BodyText = testBody;
			Factory.Save();

			var (msgClientProvider, _, submittedPayload) = GetMockedMsgClientProviderWithMessageInspection();
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger);
			connector.InitializeIfNeeded();
			var (sendResult, _, _) = await connector.SendInterchange(new XtMessageInfo(interchange, testLogger), CancellationToken.None);

			CombineAssertions(() =>
			{
				AssertEquals("SendingResult", true, sendResult);
				AssertEquals("Received SubmitMsg Count", 3, submittedPayload.Count());
				AssertMultilineEquals("MessageBody", testBody, string.Join("", submittedPayload.Select(x => x.Chunk.ToStringUtf8())), '\n', true);
			});
		}

		public async void TestChunkSizeMatchesRegistry()
		{
			List<ByteChunk> chunks = new List<ByteChunk>();
			var testLogger = new TestLogger();
			var testBody = TestBodyText;
			var interchange = CreateInterchangeForXT(Factory);
			interchange.EI_BodyText = testBody;
			Factory.Save();

			using (DirectxTMessagingRegistry.Instance.XTServerMessageChunkSizeWhenSending.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				var client = GetMockedMsgClientProviderChunkSize(chunks);
				var connector = new DirectxTConnector(client, new Cw1DirectxTMessagingConfig(), testLogger);
				connector.InitializeIfNeeded();
				var (sendResult, _, _) = await connector.SendInterchange(new XtMessageInfo(interchange, testLogger), CancellationToken.None);

				CombineAssertions(() =>
				{
					AssertEquals("SendingResult", true, sendResult);
					foreach (var chunk in chunks)
					{
						Assert("Each chunk size should be <= 32KB", chunk.CalculateSize() <= 2048);
					}
				});
			}
		}

		public async void TestSending_ExpectValidMsgIdOnSending()
		{
			var testLogger = new TestLogger();
			var testBody = TestBodyText;
			var interchange = CreateInterchangeForXT(Factory);
			interchange.EI_BodyText = testBody;
			Factory.Save();

			var (msgClientProvider, _, submittedPayload) = GetMockedMsgClientProviderWithMessageInspection();
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger);
			connector.InitializeIfNeeded();
			var (sendResult, msgId, _) = await connector.SendInterchange(new XtMessageInfo(interchange, testLogger));

			CombineAssertions(() =>
			{
				AssertEquals("SendingResult", true, sendResult);
				AssertEquals("Expected xT MsgId", msgId, 12345L);
			});
		}

		public async void TestSending_Tiny()
		{
			var testLogger = new TestLogger();
			var interchange = CreateInterchangeForXT(Factory);
			Factory.Save();

			var (msgClientProvider, _, submittedPayload) = GetMockedMsgClientProviderWithMessageInspection();
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger);
			connector.InitializeIfNeeded();
			var (sendResult, _, _) = await connector.SendInterchange(new XtMessageInfo(interchange, testLogger));

			CombineAssertions(() =>
			{
				AssertEquals("SendingResult", true, sendResult);
				AssertEquals("Received SubmitMsg Count", 1, submittedPayload.Count());
				var msg = submittedPayload.First();
				AssertArrayEqualsByElements(new byte[]
				{
					0x54, 0x53, 0x54
				}, msg.Chunk.ToArray());
				AssertEquals("TST", msg.Chunk.ToStringUtf8());
			});
		}

		public async void TestGetOutgoingMessageMetaData()
		{
			var testLogger = new TestLogger();
			var interchange = CreateInterchangeForXT(Factory);
			var sessionID = ZGuid.NewZGuid();
			interchange.EI_SessionGUID = sessionID;
			Factory.Save();

			var (msgClientProvider, submitMsgMessages, _) = GetMockedMsgClientProviderWithMessageInspection();
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger);
			connector.InitializeIfNeeded();
			var (sendResult, _, _) = await connector.SendInterchange(new XtMessageInfo(interchange, testLogger));
			AssertEquals("SendingResult", true, sendResult);
			AssertEquals("Received SubmitMsg Count", 1, submitMsgMessages.Count());
			var msg = submitMsgMessages.First();

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"custom.ApplicationCode - TST",
				"custom.MessageType - TST",
				"custom.MessageTrackingID - " + sessionID.ToString(),
				"custom.SourceParty - TSTSND",
				"custom.DestinationParty - TSTRCV",
			}, msg.Msgattr.Select(x => string.Format("{0} - {1}", x.Key, x.Value)));
		}

		public async void TestExternalPasswordModifyMetaData()
		{
			var pw = Factory.New<GlbExternalPassword_WithxTMessageAttributeProvider_ForTest>();
			var interchange = CreateInterchangeForXT(Factory);
			interchange.EI_GP = pw.PK;
			var sessionID = ZGuid.NewZGuid();
			interchange.EI_SessionGUID = sessionID;

			var testLogger = new TestLogger();
			var (msgClientProvider, submitMsgMessages, _) = GetMockedMsgClientProviderWithMessageInspection();
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger);
			connector.InitializeIfNeeded();
			connector.InitializeIfNeeded();
			var (sendResult, _, _) = await connector.SendInterchange(new XtMessageInfo(interchange, testLogger));
			var msg = submitMsgMessages.First();

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"custom.ApplicationCode - TST",
				"custom.MessageType - TST",
				"custom.MessageTrackingID - " + sessionID.ToString(),
				"custom.SourceParty - TSTSND",
				"custom.DestinationParty - TSTRCV",
				"cw1.key - Key",
				"cw1.certificate - cert",
			}, msg.Msgattr.Select(x => string.Format("{0} - {1}", x.Key, x.Value)));
		}

		public async void TestMessageWithProperHeaderText()
		{
			var testLogger = new TestLogger();
			var interchange = CreateInterchangeForXT(Factory);
			interchange.EI_HeaderText = "{" +
				"'custom.COUNTRY.EMPTY':''," +
				"'custom.COUNTRY.MULTILINE':'Line1\r\nLine2\r\n'," +
				"'custom.COUNTRY.NORMAL':'NORMAL'," +
				"'COUNTRY.PRIVATE':'PRIVATE'," +
				"'custom.COUNTRY.NONSTRING':123," +
				"'custom.ApplicationCode': 'OVD'," +
				"'custom.DUPKEY': 'VAL1'," +
				"'custom.DUPKEY': 'VAL2'," +
				"}";
			var sessionID = ZGuid.NewZGuid();
			interchange.EI_SessionGUID = sessionID;
			Factory.Save();

			var (msgClientProvider, submitMsgMessages, _) = GetMockedMsgClientProviderWithMessageInspection();
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger);
			connector.InitializeIfNeeded();
			var (sendResult, _, _) = await connector.SendInterchange(new XtMessageInfo(interchange, testLogger));
			AssertEquals("SendingResult", true, sendResult);
			AssertEquals("Received SubmitMsg Count", 1, submitMsgMessages.Count());
			var msg = submitMsgMessages.First();

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"custom.ApplicationCode - TST",
				"custom.MessageType - TST",
				"custom.MessageTrackingID - " + sessionID.ToString(),
				"custom.SourceParty - TSTSND",
				"custom.DestinationParty - TSTRCV",
				"custom.COUNTRY.MULTILINE - Line1\r\nLine2\r\n",
				"custom.COUNTRY.NORMAL - NORMAL",
				"COUNTRY.PRIVATE - PRIVATE",
				"custom.COUNTRY.NONSTRING - 123",
				"custom.DUPKEY - VAL2"
			}, msg.Msgattr.Select(x => string.Format("{0} - {1}", x.Key, x.Value)));
		}

		public async void TestMessageWithProperHeaderText_WithDoubleQuoteAndLineChange()
		{
			var testLogger = new TestLogger();
			var interchange = CreateInterchangeForXT(Factory);
			interchange.EI_HeaderText = "{" +
				"\"custom.COUNTRY.EMPTY\":\"\",\r\n" +
				"\"custom.COUNTRY.MULTILINE\":\"Line1\r\nLine2\r\n\",\r\n" +
				"\"custom.COUNTRY.NORMAL\":\"NORMAL\",\r\n" +
				"\"COUNTRY.PRIVATE\":\"PRIVATE\",\r\n" +
				"\"custom.COUNTRY.NONSTRING\":123,\r\n" +
				"\"custom.ApplicationCode\": \"OVD\",\r\n" +
				"\"custom.DUPKEY\": \"VAL1\",\r\n" +
				"\"custom.DUPKEY\": \"VAL2\",\r\n" +
				"}";
			var sessionID = ZGuid.NewZGuid();
			interchange.EI_SessionGUID = sessionID;
			Factory.Save();

			var (msgClientProvider, submitMsgMessages, _) = GetMockedMsgClientProviderWithMessageInspection();
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger);
			connector.InitializeIfNeeded();
			var (sendResult, _, _) = await connector.SendInterchange(new XtMessageInfo(interchange, testLogger));
			AssertEquals("SendingResult", true, sendResult);
			AssertEquals("Received SubmitMsg Count", 1, submitMsgMessages.Count());
			var msg = submitMsgMessages.First();

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"custom.ApplicationCode - TST",
				"custom.MessageType - TST",
				"custom.MessageTrackingID - " + sessionID.ToString(),
				"custom.SourceParty - TSTSND",
				"custom.DestinationParty - TSTRCV",
				"custom.COUNTRY.MULTILINE - Line1\r\nLine2\r\n",
				"custom.COUNTRY.NORMAL - NORMAL",
				"COUNTRY.PRIVATE - PRIVATE",
				"custom.COUNTRY.NONSTRING - 123",
				"custom.DUPKEY - VAL2"
			}, msg.Msgattr.Select(x => string.Format("{0} - {1}", x.Key, x.Value)));
		}

		public async void TestMessageWithProperHeaderText_Invalid_NonJSON()
		{
			var testLogger = new TestLogger();
			var interchange = CreateInterchangeForXT(Factory);
			interchange.EI_HeaderText = "RANDOM STRING";
			var sessionID = ZGuid.NewZGuid();
			interchange.EI_SessionGUID = sessionID;
			Factory.Save();

			var (msgClientProvider, submitMsgMessages, _) = GetMockedMsgClientProviderWithMessageInspection();
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger);
			connector.InitializeIfNeeded();
			var (sendResult, _, _) = await connector.SendInterchange(new XtMessageInfo(interchange, testLogger));
			AssertEquals("SendingResult", true, sendResult);
			AssertEquals("Received SubmitMsg Count", 1, submitMsgMessages.Count());
			var msg = submitMsgMessages.First();

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"custom.ApplicationCode - TST",
				"custom.MessageType - TST",
				"custom.MessageTrackingID - " + sessionID.ToString(),
				"custom.SourceParty - TSTSND",
				"custom.DestinationParty - TSTRCV",
			}, msg.Msgattr.Select(x => string.Format("{0} - {1}", x.Key, x.Value)));
			AssertArrayEqualsByElements("Logs", new string[] {
				"Initialization completed - connect to GRPC client",
				"Invalid Header Text for xT Message Attribute"
			}, testLogger.DebugLogs.Select(x => x.Trim()).ToArray());
		}

		public async void TestMessageWithProperHeaderText_Invalid_JSONNonDictionary()
		{
			var testLogger = new TestLogger();
			var interchange = CreateInterchangeForXT(Factory);
			interchange.EI_HeaderText = "['test1', 'test2']";
			var sessionID = ZGuid.NewZGuid();
			interchange.EI_SessionGUID = sessionID;
			Factory.Save();

			var (msgClientProvider, submitMsgMessages, _) = GetMockedMsgClientProviderWithMessageInspection();
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger);
			connector.InitializeIfNeeded();
			var (sendResult, _, _) = await connector.SendInterchange(new XtMessageInfo(interchange, testLogger));
			AssertEquals("SendingResult", true, sendResult);
			AssertEquals("Received SubmitMsg Count", 1, submitMsgMessages.Count());
			var msg = submitMsgMessages.First();

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"custom.ApplicationCode - TST",
				"custom.MessageType - TST",
				"custom.MessageTrackingID - " + sessionID.ToString(),
				"custom.SourceParty - TSTSND",
				"custom.DestinationParty - TSTRCV",
			}, msg.Msgattr.Select(x => string.Format("{0} - {1}", x.Key, x.Value)));
			AssertArrayEqualsByElements("Logs", new string[] {
				"Initialization completed - connect to GRPC client",
				"Invalid Header Text for xT Message Attribute"
			}, testLogger.DebugLogs.Select(x => x.Trim()).ToArray());
		}

		public async void TestMessageWithProperHeaderText_Invalid_JSONNested()
		{
			var testLogger = new TestLogger();
			var interchange = CreateInterchangeForXT(Factory);
			interchange.EI_HeaderText = "{" +
				"'test1':'value1', " +
				"'test2':['nested1', 'nested2']" +
				"}";
			var sessionID = ZGuid.NewZGuid();
			interchange.EI_SessionGUID = sessionID;
			Factory.Save();

			var (msgClientProvider, submitMsgMessages, _) = GetMockedMsgClientProviderWithMessageInspection();
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger);
			connector.InitializeIfNeeded();
			var (sendResult, _, _) = await connector.SendInterchange(new XtMessageInfo(interchange, testLogger));
			AssertEquals("SendingResult", true, sendResult);
			AssertEquals("Received SubmitMsg Count", 1, submitMsgMessages.Count());
			var msg = submitMsgMessages.First();

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"custom.ApplicationCode - TST",
				"custom.MessageType - TST",
				"custom.MessageTrackingID - " + sessionID.ToString(),
				"custom.SourceParty - TSTSND",
				"custom.DestinationParty - TSTRCV",
			}, msg.Msgattr.Select(x => string.Format("{0} - {1}", x.Key, x.Value)));
			AssertArrayEqualsByElements("Logs", new string[] {
				"Initialization completed - connect to GRPC client",
				"Invalid Header Text for xT Message Attribute"
			}, testLogger.DebugLogs.Select(x => x.Trim()).ToArray());
		}

		public async void TestSendMessageWithMessageDataProvider()
		{
			var testLogger = new TestLogger();

			var interchange = Factory.New<EDIInterchange_WithMessageDataProvider_ForTest>();
			interchange.EI_ApplicationCode = "TST";
			interchange.EI_InterchangeType = "TST";
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			interchange.EI_From = "Source";
			interchange.EI_To = "Target";
			interchange.EI_SessionGUID = ZGuid.NewZGuid();
			interchange.EI_BodyText = "NOT EXPECTED CONTENT";

			Factory.Save();

			var (msgClientProvider, _, submittedPayload) = GetMockedMsgClientProviderWithMessageInspection();
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger);
			connector.InitializeIfNeeded();
			var (sendResult, msgId, _) = await connector.SendInterchange(new XtMessageInfo(interchange, testLogger));

			CombineAssertions(() =>
			{
				AssertEquals("SendingResult", true, sendResult);
				AssertEquals("Expected xT MsgId", msgId, 12345L);
				AssertEquals("Received SubmitMsg Count", 3, submittedPayload.Count());

				AssertMultilineEquals("MessageBody", TestBodyText, string.Join("", submittedPayload.Select(x => x.Chunk.ToStringUtf8())), '\n', true);
			});
		}

		public async void TestSendMessageWithAttributeInExternalPassword()
		{
			var testLogger = new TestLogger();

			var interchange = Factory.New<EDIInterchange_WithMessageDataProvider_ForTest>();
			interchange.EI_ApplicationCode = "TST";
			interchange.EI_InterchangeType = "TST";
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			interchange.EI_From = "Source";
			interchange.EI_To = "Target";
			interchange.EI_SessionGUID = ZGuid.NewZGuid();
			interchange.EI_BodyText = "NOT EXPECTED CONTENT";

			Factory.Save();

			var (msgClientProvider, _, submittedPayload) = GetMockedMsgClientProviderWithMessageInspection();
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger);
			connector.InitializeIfNeeded();
			var (sendResult, msgId, _) = await connector.SendInterchange(new XtMessageInfo(interchange, testLogger));

			CombineAssertions(() =>
			{
				AssertEquals("SendingResult", true, sendResult);
				AssertEquals("Expected xT MsgId", msgId, 12345L);
				AssertEquals("Received SubmitMsg Count", 3, submittedPayload.Count());

				AssertMultilineEquals("MessageBody", TestBodyText, string.Join("", submittedPayload.Select(x => x.Chunk.ToStringUtf8())), '\n', true);
			});
		}

		#endregion

		#region Receiving

		public void TestLog_NoMessageReceived()
		{
			var incomingMessageList = new Dictionary<ulong, ITestIncomingMessage>();

			var testLogger = new TestLogger();
			var (msgClientProvider, _) = GetMockedMsgClientProviderWithIncomingMessages(incomingMessageList);
			var testHandler = new TestReceiveHandler(testLogger);
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger, null, testHandler);

			connector.InitializeIfNeeded();
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				connector.Receive();
			}
			connector.Dispose();

			var logs = testLogger.InfoLogs.ToArray();
			AssertArrayEqualsByElements("Logs", new[] {
				"Initialization completed - connect to GRPC client",
				"0 message(s) is ready to be received",
				"Logged Off the GRPC client" }, logs);
		}

		public void TestReceiving()
		{
			var incomingMessageList = new Dictionary<ulong, ITestIncomingMessage>();
			incomingMessageList[1u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body1", AckResponse = ErrorCode.ErrOk };
			incomingMessageList[3u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body2", AckResponse = ErrorCode.ErrOk };
			incomingMessageList[2u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body3", AckResponse = ErrorCode.ErrOk };

			var testLogger = new TestLogger();
			var (msgClientProvider, ackTracker) = GetMockedMsgClientProviderWithIncomingMessages(incomingMessageList);
			var testHandler = new TestReceiveHandler(testLogger);
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger, null, testHandler);
			connector.InitializeIfNeeded();

			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			{
				connector.Receive();
			}
			AssertArrayEqualsByElements("received Content", new string[] { "Body1", "Body3", "Body2" }, testHandler.ReceivedContents.ToArray());
			var logs = testLogger.AllLogs.Select(x => x.Message.Trim()).ToArray();
			AssertContainByArrayElements("Logs", new string[] {
				"Initialization completed - connect to GRPC client",
				"3 message(s) is ready to be received",
				"Msg:1 has been acknowledged with status StatusOk/0",
				"Msg:2 has been acknowledged with status StatusOk/0",
				"Msg:3 has been acknowledged with status StatusOk/0",
				"Finish Receive 3 Interchange(s). Total Time:",
				"0 message(s) is ready to be received",
				"Return metadata for message 1 from pre-cached dictionary",
				"Return metadata for message 2 from pre-cached dictionary",
				"Return metadata for message 3 from pre-cached dictionary"
			}, logs);

			AssertEquals("Receive Attempts that yielded no messages", logs.Count(a => a.Contains("0 message(s) is ready to be received")), 3);
			AssertArrayEqualsByElements("AckReceivedOnServerSide", new string[] {
				"1:StatusOk",
				"2:StatusOk",
				"3:StatusOk",
			}, ackTracker.Select(x => $"{x.Key}:{x.Value}").ToArray());
			AssertEquals(testHandler.BatchCount, 1);
		}

		public void TestReceiving_NoCaching()
		{
			var incomingMessageList = new Dictionary<ulong, ITestIncomingMessage>();
			incomingMessageList[1u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body1", AckResponse = ErrorCode.ErrOk };
			incomingMessageList[3u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body2", AckResponse = ErrorCode.ErrOk };
			incomingMessageList[2u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body3", AckResponse = ErrorCode.ErrOk };

			var testLogger = new TestLogger();
			var (msgClientProvider, ackTracker) = GetMockedMsgClientProviderWithIncomingMessages(incomingMessageList, true);
			var testHandler = new TestReceiveHandler(testLogger);
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger, null, testHandler);
			connector.InitializeIfNeeded();

			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			{
				connector.Receive();
			}
			AssertArrayEqualsByElements("received Content", new string[] { "Body1", "Body3", "Body2" }, testHandler.ReceivedContents.ToArray());
			var logs = testLogger.AllLogs.Select(x => x.Message.Trim()).ToArray();
			AssertContainByArrayElements("Logs", new string[] {
				"Initialization completed - connect to GRPC client",
				"3 message(s) is ready to be received",
				"Msg:1 has been acknowledged with status StatusOk/0",
				"Msg:2 has been acknowledged with status StatusOk/0",
				"Msg:3 has been acknowledged with status StatusOk/0",
				"Finish Receive 3 Interchange(s). Total Time:",
				"0 message(s) is ready to be received"
			}, logs);

			AssertEquals("Logs should not include", logs.Count(l => l.Contains("Return metadata for message 1 from pre-cached dictionary")), 0);
			AssertEquals("Logs should not include", logs.Count(l => l.Contains("Return metadata for message 2 from pre-cached dictionary")), 0);
			AssertEquals("Logs should not include", logs.Count(l => l.Contains("Return metadata for message 3 from pre-cached dictionary")), 0);

			AssertEquals("Receive Attempts that yielded no messages", logs.Count(a => a.Contains("0 message(s) is ready to be received")), 3);
			AssertArrayEqualsByElements("AckReceivedOnServerSide", new string[] {
				"1:StatusOk",
				"2:StatusOk",
				"3:StatusOk",
			}, ackTracker.Select(x => $"{x.Key}:{x.Value}").ToArray());
			AssertEquals(testHandler.BatchCount, 1);
		}

		public void TestReceivingNothing()
		{
			var testLogger = new TestLogger();
			var (msgClientProvider, _) = GetMockedMsgClientProviderWithIncomingMessages(new Dictionary<ulong, ITestIncomingMessage>());
			var testHandler = new TestReceiveHandler(testLogger);
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger, null, testHandler);
			connector.InitializeIfNeeded();
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			{
				connector.Receive();
			}
			var logs = testLogger.AllLogs.Select(x => x.Message.Trim()).ToArray();
			AssertContainByArrayElements("Logs", new string[] {
				"Initialization completed - connect to GRPC client",
				"0 message(s) is ready to be received"
			}, logs);
			AssertEquals("Receive Attempts that yielded no messages", logs.Count(a => a.Contains("0 message(s) is ready to be received")), 1);
			AssertEquals(testHandler.BatchCount, 0);
		}

		public void TestReceiving_Batch()
		{
			var incomingMessageList = new Dictionary<ulong, ITestIncomingMessage>();
			incomingMessageList[1u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body1", AckResponse = ErrorCode.ErrOk };
			incomingMessageList[3u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body2", AckResponse = ErrorCode.ErrOk };
			incomingMessageList[2u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body3", AckResponse = ErrorCode.ErrOk };

			var testLogger = new TestLogger();
			var (msgClientProvider, ackTracker) = GetMockedMsgClientProviderWithIncomingMessages(incomingMessageList);
			var testHandler = new TestReceiveHandler(testLogger);
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger, null, testHandler);
			connector.InitializeIfNeeded();

			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (DirectxTMessagingRegistry.Instance.InterchangeCountPerBatchOnReceiving.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				connector.Receive();
			}

			CombineAssertions(() =>
			{
				AssertArrayEqualsByElements("received Content", new string[] { "Body1", "Body3", "Body2" },
					testHandler.ReceivedContents.ToArray());
				AssertContainByArrayElements("Logs", new string[]
				{
					"Initialization completed - connect to GRPC client",
					"3 message(s) is ready to be received",
					"Msg:1 has been acknowledged with status StatusOk/0",
					"Msg:2 has been acknowledged with status StatusOk/0",
					"Finish Receive 2 Interchange(s). Total Time:",
					"Msg:3 has been acknowledged with status StatusOk/0",
					"Finish Receive 1 Interchange(s). Total Time:"
				}, testLogger.AllLogs.Select(x => x.Message.Trim()).ToArray());
				AssertArrayEqualsByElements("AckReceivedOnServerSide", new string[]
				{
					"1:StatusOk",
					"2:StatusOk",
					"3:StatusOk",
				}, ackTracker.Select(x => $"{x.Key}:{x.Value}").ToArray());
				AssertEquals(testHandler.BatchCount, 2);
			});
		}

		public void TestReceiving_MoreThan4KMessages()
		{
			var incomingMessageList = new Dictionary<ulong, ITestIncomingMessage>();
			for (ulong i = 0; i < 4100; i++)
			{
				incomingMessageList[i] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = $"Body{i}", AckResponse = ErrorCode.ErrOk };
			}

			var testLogger = new TestLogger();
			var (msgClientProvider, ackTracker) = GetMockedMsgClientProviderWithIncomingMessages(incomingMessageList);
			var testHandler = new TestReceiveHandler(testLogger);
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger, null, testHandler);
			connector.InitializeIfNeeded();

			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				connector.Receive();
			}

			AssertEquals(4100, ackTracker.Count);

			for (ulong i = 0; i < 4100; i++)
			{
				AssertEquals(MsgStatusCommand.StatusOk, ackTracker[i]);
			}
		}

		public void TestAcknowledgementAfterReceiving()
		{
			var incomingMessageList = new Dictionary<ulong, ITestIncomingMessage>();
			incomingMessageList[1u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body1", AckResponse = ErrorCode.ErrOk };
			incomingMessageList[2u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.ERROR, MsgBody = "Body2", AckResponse = ErrorCode.ErrOk };
			incomingMessageList[3u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body3", AckResponse = ErrorCode.ErrOk };

			var testLogger = new TestLogger();
			var (msgClientProvider, ackTracker) = GetMockedMsgClientProviderWithIncomingMessages(incomingMessageList);
			var testHandler = new TestReceiveHandler(testLogger);
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger, null, testHandler);
			connector.InitializeIfNeeded();

			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				connector.Receive();
			}
			AssertArrayEqualsByElements("received Content", new string[] { "Body1", "Body3" }, testHandler.ReceivedContents.ToArray());
			AssertContainByArrayElements("Logs", new string[] {
				"Initialization completed - connect to GRPC client",
				"3 message(s) is ready to be received",
				"Msg:1 has been acknowledged with status StatusOk/0",
				"Msg:2 has been acknowledged with status StatusFailed/0",
				"Msg:3 has been acknowledged with status StatusOk/0",
				"Finish Receive 3 Interchange(s). Total Time:",
				"0 message(s) is ready to be received",
			}, testLogger.AllLogs.Select(x => x.Message.Trim()).ToArray());
			AssertArrayEqualsByElements("AckReceivedOnServerSide", new string[] {
				"1:StatusOk",
				"2:StatusFailed",
				"3:StatusOk",
			}, ackTracker.Select(x => $"{x.Key}:{x.Value}").ToArray());
		}

		public void TestExceptionHandlingForLoadingReply()
		{
			var incomingMessageList = new Dictionary<ulong, ITestIncomingMessage>();
			incomingMessageList[1u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.SUCCESS, MsgBody = "Body1", AckResponse = ErrorCode.ErrOk };
			incomingMessageList[2u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.EXCEPTION, MsgBody = "Body2", AckResponse = ErrorCode.ErrOk };
			incomingMessageList[3u] = new TestIncomingMessagePreparation() { TestInstruction = TestInstructionValues.ERROR, MsgBody = "Body3", AckResponse = ErrorCode.ErrOk };

			var testLogger = new TestLogger();
			var (msgClientProvider, ackTracker) = GetMockedMsgClientProviderWithIncomingMessages(incomingMessageList);
			var testHandler = new TestReceiveHandler(testLogger);
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger, null, testHandler);
			connector.InitializeIfNeeded();

			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				connector.Receive();
			}
			AssertArrayEqualsByElements("received Content", new string[] { "Body1" }, testHandler.ReceivedContents.ToArray());
			AssertContainByArrayElements("Logs", new string[] {
				"Initialization completed - connect to GRPC client",
				"3 message(s) is ready to be received",
				"Error when downloading message MsgId:2: Mocked Error",
				"Msg:1 has been acknowledged with status StatusOk/0",
				"Msg:3 has been acknowledged with status StatusFailed/0",
				"Finish Receive 3 Interchange(s). Total Time:",
				"0 message(s) is ready to be received"
			}, testLogger.AllLogs.Select(x => x.Message.Trim()).ToArray());
			AssertArrayEqualsByElements("AckReceivedOnServerSide", new string[] {
				"1:StatusOk",
				"3:StatusFailed",
			}, ackTracker.Select(x => $"{x.Key}:{x.Value}").ToArray());
		}

		public void TestRetrieveIncomingKeyInformation()
		{
			var incomingMessageList = new Dictionary<ulong, ITestIncomingMessage>();
			incomingMessageList[20u] = new TestIncomingMessagePreparation()
			{
				MetaData = new Dictionary<string, string>
				{
					{ Constants.CustomMsgAttributes.SourceParty, "TESTSENDER" },
					{ Constants.CustomMsgAttributes.DestinationParty, "TESTRECEIVER" },
					{ Constants.CustomMsgAttributes.ApplicationCode, "TST" },
					{ Constants.CustomMsgAttributes.MessageTrackingID, "4EF4E999-E649-4971-AA19-E8D6BC605B6F" },
					{ Constants.CustomMsgAttributes.MessageType, "TST" }
				}
			};
			incomingMessageList[21u] = new TestIncomingMessagePreparation()
			{
				TestInstruction = TestInstructionValues.SUCCESS,
				MsgBody = "Body1",
				AckResponse = ErrorCode.ErrOk,
				MetaData = new Dictionary<string, string> {
					{ Constants.CustomMsgAttributes.SourceParty, "TESTCUSTOMS" },
					{ Constants.CustomMsgAttributes.DestinationParty, "TESTCW1" },
					{ Constants.CustomMsgAttributes.ApplicationCode, "TST" },
					{ Constants.CustomMsgAttributes.MessageTrackingID, "F562CE18-AF24-4149-AEA7-0A76B148C7AD" },
					{ Constants.CustomMsgAttributes.MessageType, "RES" },
					{ Constants.xTMsgAttributes.refexternal, "20" }
				}
			};

			var testLogger = new TestLogger();
			var (msgClientProvider, ackTracker) = GetMockedMsgClientProviderWithIncomingMessages(incomingMessageList);
			var testHandler = new TestReceiveHandler(testLogger);
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger, null, testHandler);
			connector.InitializeIfNeeded();

			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				connector.Receive();
			}
			AssertContainByArrayElements("Logs", new string[] {
				"Initialization completed - connect to GRPC client",
				"1 message(s) is ready to be received",
				"Msg:21 has been acknowledged with status StatusOk/0",
				"Finish Receive 1 Interchange(s). Total Time:",
				"0 message(s) is ready to be received"
			}, testLogger.AllLogs.Select(x => x.Message.Trim()).ToArray());
			AssertArrayEqualsByElements("AckReceivedOnServerSide", new string[] {
				"21:StatusOk",
			}, ackTracker.Select(x => $"{x.Key}:{x.Value}").ToArray());
			AssertEquals("Received MetaData count", 1, testHandler.ReceivedMetaData.Count);
			AssertArrayEqualsByElements("MetaData for Processing", new string[] {
				$"{Constants.CustomMsgAttributes.ApplicationCode}:TST" ,
				$"{Constants.CustomMsgAttributes.DestinationParty}:TESTCW1" ,
				$"{Constants.CustomMsgAttributes.MessageTrackingID}:F562CE18-AF24-4149-AEA7-0A76B148C7AD" ,
				$"{Constants.CustomMsgAttributes.MessageType}:RES",
				$"{Constants.CustomMsgAttributes.SourceParty}:TESTCUSTOMS",
				$"{Constants.xTMsgAttributes.refexternal}:20",
				$"{TestInstructionValues.Key}:{TestInstructionValues.SUCCESS}"
			}, testHandler.ReceivedMetaData[21u].OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value}").ToArray());
		}

		public void TestRetrieveOriginalKeyInformation()
		{
			var incomingMessageList = new Dictionary<ulong, ITestIncomingMessage>();
			incomingMessageList[20u] = new TestIncomingMessagePreparation()
			{
				MetaData = new Dictionary<string, string>
				{
					{ Constants.CustomMsgAttributes.SourceParty, "TESTSENDER" },
					{ Constants.CustomMsgAttributes.DestinationParty, "TESTRECEIVER" },
					{ Constants.CustomMsgAttributes.ApplicationCode, "TST" },
					{ Constants.CustomMsgAttributes.MessageTrackingID, "4EF4E999-E649-4971-AA19-E8D6BC605B6F" },
					{ Constants.CustomMsgAttributes.MessageType, "TST" }
				}
			};
			incomingMessageList[21u] = new TestIncomingMessagePreparation()
			{
				TestInstruction = TestInstructionValues.SUCCESS,
				MsgBody = "Body1",
				AckResponse = ErrorCode.ErrOk,
				MetaData = new Dictionary<string, string> { { Constants.xTMsgAttributes.refexternal, "20" } }
			};

			var testLogger = new TestLogger();
			var (msgClientProvider, ackTracker) = GetMockedMsgClientProviderWithIncomingMessages(incomingMessageList);
			var testHandler = new TestReceiveHandler(testLogger);
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger, null, testHandler);
			connector.InitializeIfNeeded();

			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				connector.Receive();
			}
			AssertContainByArrayElements("Logs", new string[] {
				"Initialization completed - connect to GRPC client",
				"1 message(s) is ready to be received",
				"Msg:21 has been acknowledged with status StatusOk/0",
				"Finish Receive 1 Interchange(s). Total Time:",
				"0 message(s) is ready to be received"
			}, testLogger.AllLogs.Select(x => x.Message.Trim()).ToArray());
			AssertArrayEqualsByElements("AckReceivedOnServerSide", new string[] {
				"21:StatusOk",
			}, ackTracker.Select(x => $"{x.Key}:{x.Value}").ToArray());
			AssertEquals("Received MetaData count", 1, testHandler.ReceivedMetaData.Count);
			AssertArrayEqualsByElements("MetaData for Processing", new string[] {
				$"{Constants.CustomMsgAttributes.ApplicationCode}:TST" ,
				$"{Constants.CustomMsgAttributes.DestinationParty}:TESTSENDER" ,
				$"{Constants.CustomMsgAttributes.MessageTrackingID}:4EF4E999-E649-4971-AA19-E8D6BC605B6F" ,
				$"{Constants.CustomMsgAttributes.MessageType}:TST",
				$"{Constants.CustomMsgAttributes.SourceParty}:TESTRECEIVER",
				$"{Constants.xTMsgAttributes.refexternal}:20",
				$"{TestInstructionValues.Key}:{TestInstructionValues.SUCCESS}"
			}, testHandler.ReceivedMetaData[21u].OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value}").ToArray());
		}

		public void TestRetrieveOriginalKeyInformation_WithPartialMetaData()
		{
			var incomingMessageList = new Dictionary<ulong, ITestIncomingMessage>();
			incomingMessageList[20u] = new TestIncomingMessagePreparation()
			{
				MetaData = new Dictionary<string, string>
				{
					{ Constants.CustomMsgAttributes.SourceParty, "TESTSENDER" },
					{ Constants.CustomMsgAttributes.DestinationParty, "TESTRECEIVER" },
					{ Constants.CustomMsgAttributes.ApplicationCode, "TST" },
					{ Constants.CustomMsgAttributes.MessageTrackingID, "4EF4E999-E649-4971-AA19-E8D6BC605B6F" },
					{ Constants.CustomMsgAttributes.MessageType, "TST" }
				}
			};
			incomingMessageList[21u] = new TestIncomingMessagePreparation()
			{
				TestInstruction = TestInstructionValues.SUCCESS,
				MsgBody = "Body1",
				AckResponse = ErrorCode.ErrOk,
				MetaData = new Dictionary<string, string> {
					{ Constants.CustomMsgAttributes.SourceParty, "TESTCUSTOMS" },
					{ Constants.CustomMsgAttributes.MessageTrackingID, "F562CE18-AF24-4149-AEA7-0A76B148C7AD" },
					{ Constants.CustomMsgAttributes.MessageType, "RES" },
					{ Constants.xTMsgAttributes.refexternal, "20" }
				}
			};

			var testLogger = new TestLogger();
			var (msgClientProvider, ackTracker) = GetMockedMsgClientProviderWithIncomingMessages(incomingMessageList);
			var testHandler = new TestReceiveHandler(testLogger);
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger, null, testHandler);
			connector.InitializeIfNeeded();

			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				connector.Receive();
			}
			AssertContainByArrayElements("Logs", new string[] {
				"Initialization completed - connect to GRPC client",
				"1 message(s) is ready to be received",
				"Msg:21 has been acknowledged with status StatusOk/0",
				"Finish Receive 1 Interchange(s). Total Time:",
				"0 message(s) is ready to be received"
			}, testLogger.AllLogs.Select(x => x.Message.Trim()).ToArray());
			AssertArrayEqualsByElements("AckReceivedOnServerSide", new string[] {
				"21:StatusOk",
			}, ackTracker.Select(x => $"{x.Key}:{x.Value}").ToArray());
			AssertEquals("Received MetaData count", 1, testHandler.ReceivedMetaData.Count);
			AssertArrayEqualsByElements("MetaData for Processing", new string[] {
				$"{Constants.CustomMsgAttributes.ApplicationCode}:TST" ,
				$"{Constants.CustomMsgAttributes.DestinationParty}:TESTSENDER" ,
				$"{Constants.CustomMsgAttributes.MessageTrackingID}:F562CE18-AF24-4149-AEA7-0A76B148C7AD" ,
				$"{Constants.CustomMsgAttributes.MessageType}:RES",
				$"{Constants.CustomMsgAttributes.SourceParty}:TESTCUSTOMS",
				$"{Constants.xTMsgAttributes.refexternal}:20",
				$"{TestInstructionValues.Key}:{TestInstructionValues.SUCCESS}"
			}, testHandler.ReceivedMetaData[21u].OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Value}").ToArray());
		}

		public void TestProcessorThrowsMsgServerConnectionExceptionWhenGetMsgThrowsDeadlineExceededRpcException()
		{
			// Arrange
			var incomingMessageList = new Dictionary<ulong, ITestIncomingMessage>();
			incomingMessageList[20u] = new TestIncomingMessagePreparation()
			{
				MetaData = new Dictionary<string, string>
				{
					{ Constants.CustomMsgAttributes.SourceParty, "TESTSENDER" },
					{ Constants.CustomMsgAttributes.DestinationParty, "TESTRECEIVER" },
					{ Constants.CustomMsgAttributes.ApplicationCode, "TST" },
					{ Constants.CustomMsgAttributes.MessageTrackingID, "4EF4E999-E649-4971-AA19-E8D6BC605B6F" },
					{ Constants.CustomMsgAttributes.MessageType, "TST" }
				}
			};
			incomingMessageList[21u] = new TestIncomingMessagePreparation()
			{
				TestInstruction = TestInstructionValues.SUCCESS,
				MsgBody = "Body1",
				AckResponse = ErrorCode.ErrOk,
				MetaData = new Dictionary<string, string> {
					{ Constants.CustomMsgAttributes.SourceParty, "TESTCUSTOMS" },
					{ Constants.CustomMsgAttributes.MessageTrackingID, "F562CE18-AF24-4149-AEA7-0A76B148C7AD" },
					{ Constants.CustomMsgAttributes.MessageType, "RES" },
					{ Constants.xTMsgAttributes.refexternal, "20" }
				}
			};
			var ex = new RpcException(new Status(StatusCode.DeadlineExceeded, "Deadline Exceeded"));
			var msgClientProvider = GetMoqMsgClientProviderWithRunActionWrappersForExceptionHandlingTesting(ex, MsgClientMethod.WaitMsg);

			var interchange = CreateInterchangeForXT(Factory);
			interchange.EI_HeaderText = "RANDOM STRING";
			interchange.EI_SessionGUID = ZGuid.NewZGuid();
			Factory.Save();

			var testLogger = new TestLogger();
			var testHandler = new TestReceiveHandler(testLogger);
			var connector = new DirectxTConnector(msgClientProvider, new Cw1DirectxTMessagingConfig(), testLogger, null, testHandler);
			connector.InitializeIfNeeded();

			var exception = AssertExceptionThrown<MsgServerConnectionException>(
				   "SubmitMsgMessage deadline error",
				   "Direct xT Client - DeadlineExceeded Error",
				   new AnonymousMethod(() =>
				   {
					   using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
					   {
						   connector.Receive();
					   }
				   })
			   );

			interchange.Reload();
			AssertEquals(EDIInterchangeStatusList.Codes.Queued, interchange.EI_Status);
		}

		void AssertContainByArrayElements(string message, string[] expected, string[] actual)
		{
			foreach (var exp in expected)
			{
				AssertGreaterThan(message, actual.Count(a => a.Contains(exp)), 0);
			}
		}

		public void TestReceive_WaitMsgAsyncFinishBeforeTimeout()
		{
			var clientProvider = MockMsgClientProviderForReceivingTesting(MsgBehaviors.FinishInTime);

			var testLogger = new TestLogger();
			var testHandler = new TestReceiveHandler(testLogger);
			var connector = new DirectxTConnector(clientProvider, new Cw1DirectxTMessagingConfig(), testLogger, null, testHandler);
			connector.InitializeIfNeeded();
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				connector.Receive();
			}
			AssertMultilineASCIIEquals("logMessage", @"Initialization completed - connect to GRPC client
0 message(s) is ready to be received"
				, string.Join(System.Environment.NewLine, testLogger.AllLogs.Select(x => x.Message.Trim())));
			AssertMultilineASCIIEquals("logMessage", @"Initialization completed - connect to GRPC client
0 message(s) is ready to be received"
				, string.Join(System.Environment.NewLine, testLogger.AllLogs.Select(x => x.Message.Trim())));
			AsyncHelper.WaitAllActiveTasksForTest();
		}

		public void TestReceiving_ExceedDeadline_WaitMsgAsync()
		{
			AssertReceivingExceedDeadline(MsgMethod.WaitMsgAsync, true);
		}

		public void TestReceiving_ExceedDeadline_GetMsgAttributes()
		{
			AssertReceivingExceedDeadline(MsgMethod.GetMsgAttributes, false);
		}

		public void TestReceiving_ExceedDeadline_GetMsgData()
		{
			AssertReceivingExceedDeadline(MsgMethod.GetMsgData, true);
		}

		public void TestReceiving_ExceedDeadline_MsgSetStatus()
		{
			AssertReceivingExceedDeadline(MsgMethod.MsgSetStatus, false);
		}

		void AssertReceivingExceedDeadline(MsgMethod msgMethod, bool isAsyncMethod)
		{
			using (DirectxTMessagingRegistry.Instance.XTServerMessageTimeoutInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				var testLogger = new TestLogger();

				var clientProvider = GetMockMsgClientProviderForDeadlineTesting(msgMethod);
				var testHandler = new TestReceiveHandler(testLogger);
				var connector = new DirectxTConnector(clientProvider, new Cw1DirectxTMessagingConfig(), testLogger, null, testHandler);
				connector.InitializeIfNeeded();

				var methodName = Enum.GetName(typeof(MsgMethod), msgMethod);
				using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
				{
					var ex = AssertExceptionThrown<MsgServerConnectionException>(methodName + " MsgServerConnectionException",
					"Direct xT Client - DeadlineExceeded Error",
					() => connector.Receive());

					AssertEquals(methodName + " ErrorDetail", isAsyncMethod ? "Receive 1 seconds timeout, please try it again." : $"Receive | {methodName} Deadline Exceeded", ex.ErrorDetail);
					AssertEquals(methodName + " No DeadlineExceeded Log record in connector", false, testLogger.DebugLogs.Any(x => x.Contains("DeadlineExceeded")));
					AssertEquals(methodName + " No ErrorReporter in connector", null, ErrorReporter.LastExceptionReported);
					AsyncHelper.WaitAllActiveTasksForTest();
				}
			}
		}

		[ExpectException(typeof(Exception))]
		public void TestReceive_ThrowException()
		{
			try
			{
				var clientProvider = MockMsgClientProviderForReceivingTesting(MsgBehaviors.ThrowException);
				var testLogger = new TestLogger();
				var testHandler = new TestReceiveHandler(testLogger);
				var connector = new DirectxTConnector(clientProvider, new Cw1DirectxTMessagingConfig(), testLogger, null, testHandler);
				connector.InitializeIfNeeded();
				using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
				using (DirectxTMessagingRegistry.Instance.XTServerMessageTimeoutInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
				{
					connector.Receive();
				}
			}
			finally
			{
				AsyncHelper.WaitAllActiveTasksForTest();
			}
		}

		#endregion

		string TestBodyText
		{
			get
			{
				using (var resourceRetriever = new EmbeddedResourceRetriever())
				{
					var data = resourceRetriever.GetBytes("Enterprise.xTMessaging.Shared.Test.TestFiles.testBody.txt");
					return System.Text.Encoding.UTF8.GetString(data);
				}
			}
		}

		#region Reference

		sealed class GlbExternalPassword_WithxTMessageAttributeProvider_ForTest : MasterFiles.Business.GlbExternalPassword, IxTMessageAttributeProvider
		{
			public GlbExternalPassword_WithxTMessageAttributeProvider_ForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public Dictionary<string, string> GetMessageAttrDictionary()
			{
				return new Dictionary<string, string>() {
				{ Constants.xTMsgAttributes.cw1key, "Key" },
				{ Constants.xTMsgAttributes.cw1certificate, "cert" }
			};
			}
		}

		sealed class EDIInterchange_WithMessageDataProvider_ForTest : EDIInterchange, IMessageDataProvider
		{
			public EDIInterchange_WithMessageDataProvider_ForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			BinaryReader IMessageDataProvider.GetMessageData()
			{
				using (var resourceRetriever = new EmbeddedResourceRetriever())
				{
					var bytes = resourceRetriever.GetStream("Enterprise.xTMessaging.Shared.Test.TestFiles.testBody.txt");
					return new BinaryReader(bytes);
				}
			}
		}

		sealed class EDIInterchange_WithIMessageAttributeProvider_ForTest : EDIInterchange, IxTMessageAttributeProvider
		{
			public Dictionary<string, string> GetMessageAttrDictionary()
			{
				return new Dictionary<string, string> { { "", "" }, { "existedKey", "existedValue" }, { "custom.ApplicationCode", "NewApplicationCode" }, { "custom.New", "customNewValue" } };
			}

			public EDIInterchange_WithIMessageAttributeProvider_ForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}

		sealed class EDIInterchange_WithIMessageAttributeProviderThrowsException_ForTest : EDIInterchange, IxTMessageAttributeProvider
		{
			public Dictionary<string, string> GetMessageAttrDictionary()
			{
				throw new Exception("Something went wrong!");
			}

			public EDIInterchange_WithIMessageAttributeProviderThrowsException_ForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}

		#endregion
	}
}
