using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Schema;
using Grpc.Core;
using Xware.Xt.Grpc.Application;

namespace Enterprise.xTMessaging.Shared.Test
{
	sealed class BasicReceiveHandlerTest : TestCaseWithFactory
	{
		public void TestSuccessfulSave()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();

			Factory.Save();

			var logger = new TestUtils.TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			var testMsgTrackingId = ZGuid.NewZGuid();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingId.ToString();
			testMsgMetaData[Constants.CustomMsgAttributes.MessageType] = "MST";

			var oppositeInterchange = Factory.New<EDIInterchange>();
			oppositeInterchange.EI_ApplicationCode = "TST";
			oppositeInterchange.EI_InterchangeType = "MST";
			oppositeInterchange.EI_SessionGUID = testMsgTrackingId;
			oppositeInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			oppositeInterchange.EI_GB = branch.PK;
			oppositeInterchange.EI_From = "TSTRECIPIENT";
			oppositeInterchange.EI_To = "TSTSENDER";
			oppositeInterchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			oppositeInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;

			Factory.Save();

			var handler = new BasicReceiveHandlerForTest_True(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri> { new() { Msgid = 1u } },
				_ => testMsgMetaData,
				_ => new AsyncServerStreamingCall<ResultByteChunk>(new TestUtils.ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				_ => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedEdiInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive));
			AssertEquals(1, reloadedEdiInterchanges.Length);
			var rld = reloadedEdiInterchanges[0];
			AssertEquals("TST", rld.EI_ApplicationCode);
			AssertEquals("TSTSENDER", rld.EI_From);
			AssertEquals("TSTRECIPIENT", rld.EI_To);
			AssertEquals(testMsgTrackingId, rld.EI_SessionGUID);
			AssertEquals("MST", rld.EI_InterchangeType);
			AssertEquals("Test Message", rld.EI_BodyText);
			AssertEquals("Should contain expected xT MsgId", 1L, rld.EI_XTInternalMsgID);

			AssertEquals(true, logger.InfoLogs.Contains("1 message(s) received."));
		}

		public void TestError_GetMetadata()
		{
			var logger = new TestUtils.TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";

			var handler = new BasicReceiveHandlerForTest_True(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri> { new() { Msgid = 1u } },
				_ => throw new Exception($"get metadata failed on message"),
				_ => new AsyncServerStreamingCall<ResultByteChunk>(new TestUtils.ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				_ => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedEdiInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
			AssertEquals(0, reloadedEdiInterchanges.Length);

			AssertArrayEqualsByElements(new[] { "Error on reading metadata: get metadata failed on message - MsgId:1" }, logger.ErrorLogs.Select(x => x.Trim()).ToArray());
		}

		public void TestFail_MissingKeyInformation()
		{
			var logger = new TestUtils.TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";

			var handler = new BasicReceiveHandlerForTest_True(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri> { new() { Msgid = 1u } },
				_ => testMsgMetaData,
				_ => new AsyncServerStreamingCall<ResultByteChunk>(new TestUtils.ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				_ => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedEdiInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
			AssertEquals(0, reloadedEdiInterchanges.Length);

			AssertArrayEqualsByElements(new[] { @"Error on creating metadata helper: Missing Key Message Information - MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID:" }, logger.ErrorLogs.Select(x => x.Trim()).ToArray());
		}

		public void TestFail_EmptySessionID()
		{
			var logger = new TestUtils.TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			var testMsgTrackingId = ZGuid.Empty;
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingId.ToString();

			var handler = new BasicReceiveHandlerForTest_True(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri> { new() { Msgid = 1u } },
				_ => testMsgMetaData,
				_ => new AsyncServerStreamingCall<ResultByteChunk>(new TestUtils.ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				_ => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedEdiInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
			AssertEquals(0, reloadedEdiInterchanges.Length);

			AssertArrayEqualsByElements(new[] { @"Error on creating metadata helper: Missing Key Message Information - MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID: 00000000-0000-0000-0000-000000000000" }, logger.ErrorLogs.Select(x => x.Trim()).ToArray());
		}

		public void TestFail_GetMsgData()
		{
			var logger = new TestUtils.TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			var testMsgTrackingId = ZGuid.Empty;
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingId.ToString();

			var handler = new BasicReceiveHandlerForTest_True(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri> { new() { Msgid = 1u } },
				_ => testMsgMetaData,
				_ => throw new Exception("mock exception in GetMsgData"),
				_ => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedEdiInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
			AssertEquals(0, reloadedEdiInterchanges.Length);

			AssertArrayEqualsByElements(new[] { @"Error on reading message body: mock exception in GetMsgData - MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID: 00000000-0000-0000-0000-000000000000" }, logger.ErrorLogs.Select(x => x.Trim()).ToArray());
		}

		public void TestFail_LoadReplyIntoMemory()
		{
			var logger = new TestUtils.TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			var testMsgTrackingId = ZGuid.Empty;
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingId.ToString();

			var handler = new BasicReceiveHandlerForTest_True(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri> { new() { Msgid = 1u } },
				_ => testMsgMetaData,
				_ => new AsyncServerStreamingCall<ResultByteChunk>(new TestUtils.ResultByteChunkReaderForTest(""),
					null, null, null, () => { }),
				_ => throw new Exception("mock exception in LoadReplyIntoMemory"));

			var assertFactory = new BusinessObjectFactory();
			var reloadedEdiInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
			AssertEquals(0, reloadedEdiInterchanges.Length);

			AssertArrayEqualsByElements(new[] { @"Error on reading message body: mock exception in LoadReplyIntoMemory - MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID: 00000000-0000-0000-0000-000000000000" }, logger.ErrorLogs.Select(x => x.Trim()).ToArray());
		}

		public void TestFail_LoadReplyIntoMemory_Retrying()
		{
			var logger = new TestUtils.TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);
			var testMsgMetaData = new Dictionary<string, string>();
			var testMsgTrackingId = ZGuid.NewZGuid();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingId.ToString();

			var handler = new BasicReceiveHandlerForTest_Retrying(logger, 1);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri> { new() { Msgid = 1u } },
				_ => testMsgMetaData,
				_ => new AsyncServerStreamingCall<ResultByteChunk>(new TestUtils.ResultByteChunkReaderForTest(""),
					null, null, null, () => { }),
				_ => throw new Exception("mock exception in LoadReplyIntoMemory"));
			handler.HandleReceivedMessageBatch(new List<MsgIdUri> { new() { Msgid = 1u } },
				_ => testMsgMetaData,
				_ => new AsyncServerStreamingCall<ResultByteChunk>(new TestUtils.ResultByteChunkReaderForTest(""),
					null, null, null, () => { }),
				_ => testMsgBody);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri> { new() { Msgid = 1u } },
				_ => testMsgMetaData,
				_ => new AsyncServerStreamingCall<ResultByteChunk>(new TestUtils.ResultByteChunkReaderForTest(""),
					null, null, null, () => { }),
				_ => testMsgBody);

			AssertEquals(1, logger.ErrorLogs.Count(l => l == $@"Error on reading message body: mock exception in LoadReplyIntoMemory - MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID: {testMsgTrackingId}"));

			AssertEquals(1, logger.ErrorLogs.Count(l => l == $@"Message handling error and will retry in next run: fake error - MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID: {testMsgTrackingId}"));
			AssertEquals(1, logger.InfoLogs.Count(l => l == "Message 1 received successfully."));
			AssertEquals(1, logger.WarningLogs.Count(l => l == "1 message(s) handled: 0 received, 1 failed."));
		}

		public void TestSaveExceptionHandling()
		{
			var logger = new TestUtils.TestLogger();
			var testMsgBody = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			testMsgBody.Write(testMsgBytes, 0, testMsgBytes.Length);

			var testMsgMetaData = new Dictionary<string, string>();
			var testMsgTrackingId = ZGuid.NewZGuid();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingId.ToString();
			testMsgMetaData[Constants.CustomMsgAttributes.MessageType] = "MST";

			var handler = new BasicReceiveHandlerForTestWithException(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri> { new() { Msgid = 1u } },
				_ => testMsgMetaData,
				_ => new AsyncServerStreamingCall<ResultByteChunk>(new TestUtils.ResultByteChunkReaderForTest(""), null, null, null, () => { }),
				_ => testMsgBody);

			var assertFactory = new BusinessObjectFactory();
			var reloadedEdiInterchanges = assertFactory.Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
			CombineAssertions(() =>
			{
				AssertEquals(0, reloadedEdiInterchanges.Length);
				AssertEquals(1, logger.ErrorLogs.Count);

				AssertEquals($@"Message handling error and will not retry more: The method or operation is not implemented. - MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID: {testMsgTrackingId}", logger.ErrorLogs.Single().Trim());

				AssertEquals(1, logger.WarningLogs.Count);
				AssertEquals("1 message(s) handled: 0 received, 1 failed.", logger.WarningLogs.FirstOrDefault());
			});
		}

		public void TestSaveReturnFalse()
		{
			var logger = new TestUtils.TestLogger();

			var testMsgMetaData = new Dictionary<string, string>();
			var testMsgTrackingId = ZGuid.NewZGuid();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingId.ToString();
			testMsgMetaData[Constants.CustomMsgAttributes.MessageType] = "MST";

			var testMsgData =
				new AsyncServerStreamingCall<ResultByteChunk>(new TestUtils.ResultByteChunkReaderForTest(""), null,
					null, null, () => { });

			var handler = (BasicReceiveHandler)new BasicReceiveHandlerForTest_False(logger);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri> { new() { Msgid = 1u } },
				_ => testMsgMetaData,
				_ => testMsgData,
				_ => testMsgBody());

			CombineAssertions(() =>
			{
				var assertFactory = new BusinessObjectFactory();
				var reloadedEdiInterchanges = assertFactory.Load<EDIInterchange>(
					new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
				AssertEquals(0, reloadedEdiInterchanges.Length);
				AssertEquals(1, logger.ErrorLogs.Count);

				AssertEquals($@"Message handling error and will retry in next run: fake error - MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID: {testMsgTrackingId}", logger.ErrorLogs.Single().Trim());

				AssertEquals(1, logger.WarningLogs.Count);
				AssertEquals("1 message(s) handled: 0 received, 1 failed.", logger.WarningLogs.FirstOrDefault());

				logger = new TestUtils.TestLogger();
				handler = new BasicReceiveHandlerForTest_True(logger);
				handler.HandleReceivedMessageBatch(new List<MsgIdUri> { new() { Msgid = 1u } },
					_ => testMsgMetaData,
					_ => testMsgData,
					_ => testMsgBody());

				assertFactory = new BusinessObjectFactory();
				reloadedEdiInterchanges = assertFactory.Load<EDIInterchange>(
					new ZQuery(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT));
				AssertEquals(1, reloadedEdiInterchanges.Length);
				AssertEquals(0, logger.ErrorLogs.Count);
				AssertEquals(0, logger.WarningLogs.Count);
				AssertEquals(true, logger.InfoLogs.Contains("1 message(s) received."));
			});
		}

		public void TestSaveLog_RetryingLog()
		{
			var logger = new TestUtils.TestLogger();

			var testMsgMetaData = new Dictionary<string, string>();
			var testMsgTrackingId = ZGuid.NewZGuid();
			testMsgMetaData[Constants.CustomMsgAttributes.ApplicationCode] = "TST";
			testMsgMetaData[Constants.CustomMsgAttributes.SourceParty] = "TSTSENDER";
			testMsgMetaData[Constants.CustomMsgAttributes.DestinationParty] = "TSTRECIPIENT";
			testMsgMetaData[Constants.CustomMsgAttributes.MessageTrackingID] = testMsgTrackingId.ToString();
			testMsgMetaData[Constants.CustomMsgAttributes.MessageType] = "MST";

			var testMsgData =
				new AsyncServerStreamingCall<ResultByteChunk>(new TestUtils.ResultByteChunkReaderForTest(""), null,
					null, null, () => { });

			var handler = (BasicReceiveHandler)new BasicReceiveHandlerForTest_Retrying(logger, 2);
			handler.HandleReceivedMessageBatch(new List<MsgIdUri> { new() { Msgid = 1u } },
				_ => testMsgMetaData,
				_ => testMsgData,
				_ => testMsgBody());
			handler.HandleReceivedMessageBatch(new List<MsgIdUri> { new() { Msgid = 1u } },
				_ => testMsgMetaData,
				_ => testMsgData,
				_ => testMsgBody());
			handler.HandleReceivedMessageBatch(new List<MsgIdUri> { new() { Msgid = 1u } },
				_ => testMsgMetaData,
				_ => testMsgData,
				_ => testMsgBody());

			CombineAssertions(() =>
			{
				AssertEquals(2, logger.ErrorLogs.Count(l => l == $@"Message handling error and will retry in next run: fake error - MsgId: 1
AppCode: TST
Sender: TSTSENDER
Recipient: TSTRECIPIENT
SessionID: {testMsgTrackingId}"));
				AssertEquals(1, logger.InfoLogs.Count(l => l == "Message 1 received successfully."));
				AssertEquals(1, logger.InfoLogs.Count(l => l == "1 message(s) received."));
			});
		}

		Stream testMsgBody()
		{
			var stream = new MemoryStream();
			var testMsgBytes = Encoding.UTF8.GetBytes("Test Message");
			stream.Write(testMsgBytes, 0, testMsgBytes.Length);
			return stream;
		}
	}

	class BasicReceiveHandlerForTest_True : BasicReceiveHandler
	{
		public BasicReceiveHandlerForTest_True(ILogger logger) : base(logger)
		{
		}

		protected override (bool Success, string ErrorMessage) HandleMessage(MetaDataHelper metaDataHelper,
			Stream payload, long xTInternalMsgID)
		{
			var factory = new BusinessObjectFactory();
			var interchange = factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = metaDataHelper.ApplicationCode;
			interchange.EI_SessionGUID = new ZGuid(metaDataHelper.MessageTrackingId);
			interchange.EI_From = metaDataHelper.SourceParty;
			interchange.EI_To = metaDataHelper.DestinationParty;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.EI_InterchangeType = metaDataHelper.MessageType;
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			interchange.EI_XTInternalMsgID = xTInternalMsgID;
			interchange.SetHeaderTextWithAttributeDictionary(metaDataHelper.MetaData);
			interchange.SetEI_BodyTextOrDataSource(payload);
			factory.Save();

			return (true, string.Empty);
		}
	}

	class BasicReceiveHandlerForTest_False : BasicReceiveHandler
	{
		public BasicReceiveHandlerForTest_False(ILogger logger) : base(logger)
		{
		}

		protected override (bool Success, string ErrorMessage) HandleMessage(MetaDataHelper metaDataHelper, Stream payload, long xTInternalMsgID)
		{
			return (false, "fake error");
		}
	}

	class BasicReceiveHandlerForTestWithException : BasicReceiveHandler
	{
		public BasicReceiveHandlerForTestWithException(ILogger logger) : base(logger)
		{
		}

		protected override (bool Success, string ErrorMessage) HandleMessage(MetaDataHelper metaDataHelper, Stream payload, long xTInternalMsgID)
		{
			throw new NotImplementedException();
		}
	}

	class BasicReceiveHandlerForTest_Retrying : BasicReceiveHandler
	{
		public BasicReceiveHandlerForTest_Retrying(ILogger logger, int successOnRetryCount) : base(logger)
		{
			this.successOnRetryCount = successOnRetryCount;
		}

		public Dictionary<long, int> RetryingMsgIDs = new();
		readonly int successOnRetryCount;
		protected override (bool Success, string ErrorMessage) HandleMessage(MetaDataHelper metaDataHelper, Stream payload, long xTInternalMsgID)
		{
			if (RetryingMsgIDs.ContainsKey(xTInternalMsgID))
			{
				if (RetryingMsgIDs[xTInternalMsgID] >= successOnRetryCount)
				{
					RetryingMsgIDs.Remove(xTInternalMsgID);
					return (true, string.Empty);
				}

				RetryingMsgIDs[xTInternalMsgID]++;
				return (false, "fake error");
			}

			RetryingMsgIDs.Add(xTInternalMsgID, 1);
			return (false, "fake error");
		}
	}
}
