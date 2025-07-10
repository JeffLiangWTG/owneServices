using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Messaging.Testing
{
	[TestedType(typeof(EDIInterchange))]
	public class EDIInterchangeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAuditColumnLookups()
		{
			var interchange = Factory.New<EDIInterchange>();
			Assert(interchange.Lookups.GlbStaffs.Any(g => g.PK == GlbStaff.CurrentUser.PK));
			AssertEquals("Last Edit By", interchange.EI_SystemLastEditUserInfo.Description);
			AssertEquals("Created By", interchange.EI_SystemCreateUserInfo.Description);
			AssertEquals("Last Edit Time (UTC)", interchange.EI_SystemLastEditTimeUtcInfo.Description);
			AssertEquals("Create Time (UTC)", interchange.EI_SystemCreateTimeUtcInfo.Description);
		}

		public void TestLoggedWhenStausSetToFail()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			interchange.ForceDeprecatedNTextUsageForTesting = true;
			interchange.EI_BodyNText = LargeMessageTestHelper.BigChunkOfXML;
			interchange.EI_HeaderText = "HEADER";
			interchange.EI_BodyText = "BODY";
			interchange.EI_FooterText = "FOOTER";
			interchange.EI_Status = "QUE";
			Factory.Save();
			interchange.EI_Status = "FAL";
			Factory.Save();
			var edtLog = interchange.Logs.GetAllLogs();
			AssertCollectionContains(new ZQuery(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code), new ZQuery(StmALogSchema.SL_Reference, "|NEW=FAL|OLD=QUE")), edtLog);
		}

		public void TestNTextFieldGetsBlankedOutWhenUpodatedViaEI_BodyText()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			interchange.ForceDeprecatedNTextUsageForTesting = true;
			interchange.EI_BodyNText = LargeMessageTestHelper.BigChunkOfXML;
			Factory.Save();

			AssertEquals("interchange.EI_BodyData.dbdatalength()", 0, LargeMessageTestHelper.GetLengthStoredInDB(interchange, EDIInterchangeSchema.EI_BodyData));
			AssertEquals("interchange.EI_BodyText.dbdatalength()", 0, LargeMessageTestHelper.GetLengthStoredInDB(interchange, EDIInterchangeSchema.EI_BodyText));
			AssertNotEquals("interchange.EI_BodyNText.dbdatalength()", 0, LargeMessageTestHelper.GetLengthStoredInDB(interchange, EDIInterchangeSchema.EI_BodyNText));

			var reloadingFactory = new BusinessObjectFactory();
			var reloadedInterchange = reloadingFactory.Load<EDIInterchange>(interchange.PK);
			AssertEquals("reloadedInterchange.EI_BodyText", LargeMessageTestHelper.BigChunkOfXML, reloadedInterchange.EI_BodyText);
			reloadedInterchange.EI_BodyText = LargeMessageTestHelper.BigChunkOfXML + ".";
			reloadingFactory.Save();

			AssertNotEquals("reloadedInterchange.EI_BodyData.dbdatalength()", 0, LargeMessageTestHelper.GetLengthStoredInDB(reloadedInterchange, EDIInterchangeSchema.EI_BodyData));
			AssertEquals("reloadedInterchange.EI_BodyText.dbdatalength()", 0, LargeMessageTestHelper.GetLengthStoredInDB(reloadedInterchange, EDIInterchangeSchema.EI_BodyText));
			AssertEquals("reloadedInterchange.EI_BodyNText.dbdatalength()", 0, LargeMessageTestHelper.GetLengthStoredInDB(reloadedInterchange, EDIInterchangeSchema.EI_BodyNText));

			AssertEquals("reloadedInterchange.EI_BodyText", LargeMessageTestHelper.BigChunkOfXML + ".", reloadedInterchange.EI_BodyText);
		}

		public void TestXMLInterchangeTextUsingPropertyGoesToCompressedFormInInterchangeData()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.NativeDataMessaging;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			AssertEquals("EI_BodyData.Length", 0, interchange.EI_BodyData.Length);
			interchange.EI_BodyText = LargeMessageTestHelper.BigChunkOfXML;
			AssertNotEquals("EI_BodyData.Length", 0, interchange.EI_BodyData.Length);
			Factory.Save();

			var dataLengthInDB = LargeMessageTestHelper.GetLengthStoredInDB(interchange, EDIInterchangeSchema.EI_BodyData);
			Assert("interchange.EI_BodyData.dbdatalength() > 0", dataLengthInDB > 0);
			Assert("interchange.EI_BodyData.dbdatalength() < (bigChunkOfXML.Length / 5) - ie: Compression kicked in.", dataLengthInDB < (LargeMessageTestHelper.BigChunkOfXML.Length / 5));
			var textLengthInDB = LargeMessageTestHelper.GetLengthStoredInDB(interchange, EDIInterchangeSchema.EI_BodyText);
			AssertEquals("interchange.EI_BodyText.dbdatalength()", 0, textLengthInDB);
			var ntextLengthInDB = LargeMessageTestHelper.GetLengthStoredInDB(interchange, EDIInterchangeSchema.EI_BodyNText);
			AssertEquals("interchange.EI_BodyNText.dbdatalength()", 0, ntextLengthInDB);

			var reloadingFactory = new BusinessObjectFactory();
			var reloadedInterchange = reloadingFactory.Load<EDIInterchange>(interchange.PK);
			AssertEquals("reloadedInterchange.EI_BodyText", LargeMessageTestHelper.BigChunkOfXML, reloadedInterchange.EI_BodyText);
		}

		public void TestXMLInterchangeTextUsingTextReaderGoesToCompressedFormInInterchangeData()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			long xmlLength;
			using (var textSource = new MemoryStream(MessageEncoding.UTF8WithoutBOM.GetBytes(LargeMessageTestHelper.BiggerChunkOfXML)))
			{
				xmlLength = textSource.Length;
				interchange.SetEI_BodyTextSource(new TextReaderSource(textSource));
				Factory.Save();
			}

			var dataLengthInDB = LargeMessageTestHelper.GetLengthStoredInDB(interchange, EDIInterchangeSchema.EI_BodyData);
			Assert("interchange.EI_BodyData.dbdatalength() > 0", dataLengthInDB > 0);
			Assert("interchange.EI_BodyData.dbdatalength() < (bigChunkOfXML.Length / 5) - ie: Compression kicked in.", dataLengthInDB < (xmlLength / 5));
			var textLengthInDB = LargeMessageTestHelper.GetLengthStoredInDB(interchange, EDIInterchangeSchema.EI_BodyText);
			AssertEquals("interchange.EI_BodyText.dbdatalength()", 0, textLengthInDB);
			var ntextLengthInDB = LargeMessageTestHelper.GetLengthStoredInDB(interchange, EDIInterchangeSchema.EI_BodyNText);
			AssertEquals("interchange.EI_BodyNText.dbdatalength()", 0, ntextLengthInDB);

			var reloadingFactory = new BusinessObjectFactory();
			var reloadedInterchange = reloadingFactory.Load<EDIInterchange>(interchange.PK);
			using (var textReader = reloadedInterchange.GetEI_BodyTextReader())
			{
				var interchangeText = textReader.ReadToEnd();
				AssertEquals("reloadedInterchange.GetEI_BodyTextReader().ReadToEnd()", LargeMessageTestHelper.BiggerChunkOfXML, interchangeText);
			}
		}

		public void TestParameterisationOfEI_Status()
		{
			var query = new ZQuery(EDIInterchangeSchema.EI_Status, new string[] { "PND", "SNT" });
			AssertEquals("(EI_Status in ('PND', 'SNT'))", query.ParameterisedText.ParameterisedQueryText);
		}

		public void TestDiagnosticDetails()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_Status = EDIInterchange.Status.eHubQueued;
			interchange.EI_TransportType = EDIInterchange.TransportType.eHub;
			interchange.EI_ReceiveTransmit = "TRX";
			interchange.EI_SessionGUID = interchange.PK;
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.XMS;
			interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.XMS;

			Factory.Save();

			var expected = string.Format("{{Application Code: XMS, Body Data: System.Byte[], Communication Party Config: {5}, From: From, EI_GB: {3}, EI_GP: {5}, Interchange Num: 1, Interchange Type: XMS, Is Active: Y, Receive Transmit: TRX, Retry Count: 0, Server ID: 0, Session GUID: {0}, Status: HQU, System Create Time Utc: {2}, System Create User: E, System Last Edit Time Utc: {1}, System Last Edit User: E, EI_To: To, Transport Type: {4}, XT Internal Msg ID: 0}}", interchange.EI_SessionGUID, interchange.EI_SystemLastEditTimeUtc, interchange.EI_SystemCreateTimeUtc, interchange.EI_GB, EDIInterchange.TransportType.eHub, ZGuid.Empty);
			AssertEquals("Diagnostic Details: ", expected, interchange.DiagnosticDetails);
		}

		public void TestSendViaeHubOrNot()
		{
			var interchangeDirect = Factory.New<EDIInterchange>();
			interchangeDirect.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchangeDirect.EI_Status = EDIInterchange.Status.Sent;
			AssertEquals(EDIInterchange.Status.Sent, interchangeDirect.EI_Status);

			interchangeDirect.EI_Status = EDIInterchange.Status.Queued;
			AssertEquals(EDIInterchange.Status.Queued, interchangeDirect.EI_Status);
			AssertEquals(Guid.Empty, interchangeDirect.EI_SessionGUID);

			var interchangeViaeHub = Factory.New<EDIInterchangeSendViaeHub>();

			interchangeViaeHub.EI_Status = EDIInterchange.Status.Queued;
			AssertEquals(EDIInterchange.Status.Queued, interchangeViaeHub.EI_Status);
			AssertEquals(Guid.Empty, interchangeViaeHub.EI_SessionGUID);

			interchangeViaeHub.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchangeViaeHub.EI_Status = EDIInterchange.Status.Queued;
			AssertEquals(EDIInterchange.Status.eHubQueued, interchangeViaeHub.EI_Status);
			AssertEquals(EDIInterchange.TransportType.eHub, interchangeViaeHub.EI_TransportType);
			AssertEquals(false, interchangeViaeHub.EI_SessionGUID.IsEmpty);
			AssertNotNull(interchangeViaeHub.EI_SessionGUID);

			interchangeViaeHub.EI_Status = EDIInterchange.Status.Sent;
			AssertEquals(EDIInterchange.Status.Sent, interchangeViaeHub.EI_Status);

			interchangeViaeHub.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchangeViaeHub.EI_Status = EDIInterchange.Status.Queued;
			AssertEquals(EDIInterchange.Status.Queued, interchangeViaeHub.EI_Status);

			interchangeViaeHub.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchangeViaeHub.EI_Status = EDIInterchange.Status.eHubQueued;
			AssertEquals(EDIInterchange.Status.Queued, interchangeViaeHub.EI_Status);
			AssertEquals(EDIInterchange.TransportType.eHub, interchangeViaeHub.EI_TransportType);

			interchangeViaeHub.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchangeViaeHub.EI_Status = EDIInterchange.Status.Queued;
			AssertEquals(EDIInterchange.Status.eHubQueued, interchangeViaeHub.EI_Status);
			AssertEquals(EDIInterchange.TransportType.eHub, interchangeViaeHub.EI_TransportType);
			AssertEquals(false, interchangeViaeHub.EI_SessionGUID.IsEmpty);
			AssertNotNull(interchangeViaeHub.EI_SessionGUID);

			interchangeViaeHub.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			AssertEquals(EDIInterchange.Status.Queued, interchangeViaeHub.EI_Status);
			AssertEquals(ZGuid.Empty, interchangeViaeHub.EI_SessionGUID);
		}

		#region

		class EDIInterchangeForTesting : EDIInterchange
		{
			public EDIInterchangeForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override bool ShouldBatchNumberBeByInterchange
			{
				get { return true; }
			}
		}

		#endregion

		#region Send via eHub Test Class

		class EDIInterchangeSendViaeHub : EDIInterchange
		{
			public EDIInterchangeSendViaeHub(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override bool ShouldSendViaEHubCore
			{
				get { return true; }
			}
		}

		#endregion

		public void TestSpawnWithoutCreatingMessagesSetsQUEandRCV()
		{
			var interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, "UNA:+.? 'UNB+UNOA:2+CUSSWT:ZZZ+00009908C:ZZZ+121109:1318+358'UNH+483+CUSRES:D:96B:UN+BAKLDNZ00158862'BGM+963+34472678'GIS+841:120:143'ERP+001::44'ERC+618::143'UNT+6+483'UNZ+1+358'", "XXX", true, true);
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals(EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
			AssertEquals("UNA:+.? 'UNB+UNOA:2+CUSSWT:ZZZ+00009908C:ZZZ+121109:1318+358'", interchange.EI_HeaderText);
			AssertEquals("UNH+483+CUSRES:D:96B:UN+BAKLDNZ00158862'BGM+963+34472678'GIS+841:120:143'ERP+001::44'ERC+618::143'UNT+6+483'", interchange.EI_BodyText);
			AssertEquals("UNZ+1+358'", interchange.EI_FooterText);
		}

		public void TestIsEhubMessage()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_Status = EDIInterchange.Status.eHubQueued;
			interchange.EI_SessionGUID = ZGuid.NewZGuid();
			Factory.Save();

			AssertEquals("interchange.IsEHubMessage", true, interchange.IsEHubMessage);

			var otherFactory = new BusinessObjectFactory();
			var interchangeFromDB = otherFactory.Load<EDIInterchange>(interchange.PK);

			AssertEquals("interchangeFromDB.IsEHubMessage", true, interchangeFromDB.IsEHubMessage);
		}

		#region Reset to queued tests

		public void TestTransmitResetStatusToQueuedWhenFailed()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			interchange.EI_Status = EDIMessage.Status.Queued;
			Factory.Save();

			interchange.EI_Status = EDIMessage.Status.Failed;
			Factory.Save();

			interchange.ResetToQueuedStatus();

			AssertEquals("EI_Status should be Queued", EDIInterchange.Status.Queued, interchange.EI_Status);
		}

		public void TestTransmitResetStatusToQueuedForEHubWhenSent()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_Status = EDIInterchange.Status.eHubQueued;
			Factory.Save();

			interchange.EI_Status = EDIInterchange.Status.Sent;
			Factory.Save();

			interchange.ResetToQueuedStatus();

			CombineAssertions(() =>
			{
				AssertEquals("EI_Status should be eHubQueued", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
				AssertEquals("EI_TransportType should be eHub", EDIInterchange.TransportType.eHub, interchange.EI_TransportType);
			});
		}

		public void TestTransmitResetStatusToQueuedForEAdaptorWhenSent()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_Status = EDIInterchange.Status.eAdaptorQueued;
			Factory.Save();

			interchange.EI_Status = EDIInterchange.Status.Sent;
			Factory.Save();

			interchange.ResetToQueuedStatus();

			CombineAssertions(() =>
			{
				AssertEquals("EI_Status should be eAdaptorQueued", EDIInterchange.Status.eAdaptorQueued, interchange.EI_Status);
				AssertEquals("EI_TransportType should be eAdaptor", EDIInterchange.TransportType.eAdaptor, interchange.EI_TransportType);
			});
		}

		public void TestTransmitResetStatusToQueuedForInterchangeMessageThatHasBeenSentToEHub()
		{
			EDIInterchange interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_HeaderText = "HEADER";
			interchange.EI_BodyText = "BODY";
			interchange.EI_FooterText = "FOOTER";
			interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.XMS;
			interchange.EI_Status = EDIInterchange.Status.eHubQueued;
			interchange.EI_SessionGUID = ZGuid.NewZGuid();
			Factory.Save();

			interchange.EI_Status = EDIInterchange.Status.Failed;
			interchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Factory.Save();

			interchange.ResetToQueuedStatus();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("EI_Status should be eHub Queued", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
				AssertEquals("EI_TransportType should be eHub", EDIInterchange.TransportType.eHub, interchange.EI_TransportType);
				AssertEquals($"Looking for 'Updated status: FAL->{EDIInterchange.Status.eHubQueued}'", true, interchange.Logs.GetAllLogs().ToArray<StmALog>().ToList().Exists(x => x.SL_Reference.Contains("|NEW=HQU|OLD=FAL")));
				AssertEquals("IsEHubMessage should be true", interchange.IsEHubMessage, true);
			});
		}

		public void TestTransmitResetStatusToQueuedForInterchangeMessageThatHasBeenSentToEAdaptor()
		{
			EDIInterchange interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_HeaderText = "HEADER";
			interchange.EI_BodyText = "BODY";
			interchange.EI_FooterText = "FOOTER";
			interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.XMS;
			interchange.EI_Status = EDIInterchange.Status.eAdaptorQueued;
			Factory.Save();

			interchange.EI_Status = EDIInterchange.Status.Failed;
			interchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Factory.Save();

			interchange.ResetToQueuedStatus();

			CombineAssertions(() =>
			{
				AssertEquals("EI_Status should be eAdaptor Queued", EDIInterchange.Status.eAdaptorQueued, interchange.EI_Status);
				AssertEquals("EI_TransportType should be eAdaptor", EDIInterchange.TransportType.eAdaptor, interchange.EI_TransportType);
			});
		}

		public void TestReceiveResetStatusToQueuedDoesNotEditReceivedInterchanges()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIMessage.Direction.Receive;
			interchange.EI_Status = EDIMessage.Status.Queued;
			Factory.Save();

			interchange.EI_Status = EDIMessage.Status.Failed;
			Factory.Save();

			interchange.ResetToQueuedStatus();

			AssertEquals("EI_Status should be reset to QUE - we do now allow reset of any received interchanges", EDIInterchange.Status.Queued, interchange.EI_Status);
		}

		public void TestReceiveResetStatusToQueued()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Cancelled;

			interchange.ResetToQueuedStatus();

			AssertEquals("EI_Status should be Queued", EDIInterchange.Status.Queued, interchange.EI_Status);
		}

		public void TestResetToQueForAnyReceivedInterchange()
		{
			var eHubPendingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			eHubPendingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			eHubPendingInterchange.EI_Status = EDIInterchange.Status.eHubPending;
			eHubPendingInterchange.ResetToQueuedStatus();

			var errorInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			errorInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			errorInterchange.EI_Status = EDIInterchange.Status.Error;
			errorInterchange.ResetToQueuedStatus();

			var failedInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			failedInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			failedInterchange.EI_Status = EDIInterchange.Status.Failed;
			failedInterchange.ResetToQueuedStatus();

			var recievedInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			recievedInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			recievedInterchange.EI_Status = EDIInterchange.Status.Received;
			recievedInterchange.ResetToQueuedStatus();

			var pendingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			pendingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			pendingInterchange.EI_Status = EDIInterchange.Status.SendPending;
			pendingInterchange.ResetToQueuedStatus();

			var sentInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			sentInterchange.EI_Status = EDIInterchange.Status.Sent;
			sentInterchange.ResetToQueuedStatus();

			AssertEquals("EI_Status should be reset to Queued", EDIInterchange.Status.Queued, eHubPendingInterchange.EI_Status);
			AssertEquals("EI_Status should be reset to Queued", EDIInterchange.Status.Queued, errorInterchange.EI_Status);
			AssertEquals("EI_Status should be reset to Queued", EDIInterchange.Status.Queued, failedInterchange.EI_Status);
			AssertEquals("EI_Status should be reset to Queued", EDIInterchange.Status.Queued, recievedInterchange.EI_Status);
			AssertEquals("EI_Status should be reset to Queued", EDIInterchange.Status.Queued, pendingInterchange.EI_Status);
			AssertEquals("EI_Status should be reset to Queued", EDIInterchange.Status.Queued, sentInterchange.EI_Status);
		}

		public void TestStatusChangeWithoutSaveIsNotLogged()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_Status = EDIInterchange.Status.Sent;
			interchange.EI_Status = EDIInterchange.Status.Failed;
			AssertEquals("Count of 'Updated status:' in interchange.Logs", 0, interchange.Logs.GetAllLogs().Cast<StmALog>().Count(x => x.SL_SE_NKEvent == Events.StatusUpdatedCode));
		}

		public void TestStatusChangeDoesNotLogIntermediateValues()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_Status = EDIInterchange.Status.Queued;
			Factory.Save();

			interchange.EI_Status = EDIInterchange.Status.Sent;
			interchange.EI_Status = EDIInterchange.Status.Failed;
			interchange.EI_Status = EDIInterchange.Status.Acknowledged;
			interchange.EI_Status = EDIInterchange.Status.Error;
			interchange.EI_Status = EDIInterchange.Status.Received;
			interchange.EI_Status = EDIInterchange.Status.Cancelled;
			Factory.Save();

			var actualLogs = interchange.Logs.GetAllLogs().Cast<StmALog>()
				.Where(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode && x.SL_Reference.Contains("|NEW=") && x.SL_Reference.Contains("|OLD="))
				.OrderBy(x => x.SL_EventTime)
				.Select(x => x.SL_Reference.ToString())
				.ToArray();

			var expectedLogs = new[]
			{
				"|NEW=CAN|OLD=QUE",
			};

			AssertArrayEqualsByElements(expectedLogs, actualLogs);
		}

		#endregion

		public void TestCompanyThrowsNullReferenceExceptionWhrenBranchPkInvalid()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_GB = ZGuid.Invalid;
			AssertNoExceptionThrown("Object reference not set to an instance of an object.", () => { var var = interchange.Company; });
		}

		public void TestShouldUseNText()
		{
			CombineAssertions(() =>
			{
				var interchange = Factory.New<EDIInterchange>();
				AssertEquals("Empty ApplicationCode", expected: false, interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.AUCOLS;
				AssertEquals("ApplicationCode AUCOLS", expected: true, interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
				interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.UniversalDataMessaging;
				AssertEquals("ApplicationCode UniversalDataMessaging", expected: true, interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
				interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.USCustomsExport;
				AssertEquals("ApplicationCode USCustomsExport", expected: false, interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
				interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.XMS;
				AssertEquals("ApplicationCode XMS", expected: true, interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.NativeDataMessaging;
				AssertEquals("ApplicationCode NativeDataMessaging", expected: true, interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.SYS;
				AssertEquals("ApplicationCode SYS", expected: true, interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.CustomsWare;
				AssertEquals("ApplicationCode CustomsWare", expected: true, interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.Telematics;
				AssertEquals("ApplicationCode Telematics", expected: true, interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.GlobalElectronicInvoice;
				AssertEquals("ApplicationCode GlobalElectronicInvoice", expected: true, interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.ChinaInterfaceMapping;
				AssertEquals("ApplicationCode ChinaInterfaceMapping", expected: true, interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.TWCustoms;
				AssertEquals("ApplicationCode TWCustoms", expected: true, interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
				AssertEquals("ApplicationCode GenericMessageDelivery", expected: true, interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.TRCustoms;
				AssertEquals("ApplicationCode TRCustoms", expected: true, interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.KRCustoms;
				AssertEquals("ApplicationCode KRCustoms", expected: true, interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.ILCustoms;
				AssertEquals("ApplicationCode ILCustoms", expected: true, interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.JPCustoms;
				AssertEquals("ApplicationCode JPCustoms", expected: true, interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
				AssertEquals("ApplicationCode INCustoms", expected: true, interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.NOCustomsEmma;
				AssertEquals("ApplicationCode NOCustomsEmma", expected: true, interchange.ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed);
			});
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestInterchangeTextRelatedFields()
		{
			var interchangeMock = Factory.NewMoq<EDIInterchange>();
			var interchange = interchangeMock.Object;
			interchange.EI_HeaderText = "HEADER";
			interchange.EI_BodyText = "BODY";
			interchange.EI_FooterText = "FOOTER";

			AssertEquals("HEADERBODYFOOTER", interchange.EI_InterchangeText);
			AssertEquals("HEADERBODYFOOTER", interchange.EI_InterchangeTextShort);
			AssertEquals("HEADERBODYFOOTER", interchange.EI_InterchangeTextDetail);

			interchangeMock.Protected()
						   .Setup<LargeMessageHelper.TextPadder>("GetTextPadderForEI_InterchangeText")
						   .Returns(new LargeMessageHelper.TextPadder((x, y) => y == LargeMessageHelper.TextPadderDataType.Header ? "123" + x : y == LargeMessageHelper.TextPadderDataType.Body ? "456" + x : "789" + x));
			AssertEquals(interchange.EI_HeaderText + interchange.EI_BodyText + interchange.EI_FooterText, interchange.EI_InterchangeText);
			AssertEquals("123" + interchange.EI_HeaderText + "456" + interchange.EI_BodyText + "789" + interchange.EI_FooterText, interchange.EI_InterchangeTextShort);
			AssertEquals("123" + interchange.EI_HeaderText + "456" + interchange.EI_BodyText + "789" + interchange.EI_FooterText, interchange.EI_InterchangeTextDetail);

			interchange.EI_HeaderText = "HEADER".PadRight(LargeMessageHelper.ShortTextSizeLimit - 1, '!');
			interchange.EI_FooterText = "FOOTER".PadRight(LargeMessageHelper.DetailTextSizeLimit, '@');

			AssertEquals(interchange.EI_HeaderText + interchange.EI_BodyText + interchange.EI_FooterText, interchange.EI_InterchangeText);
			AssertEquals("123" + interchange.EI_HeaderText.Left(LargeMessageHelper.ShortTextSizeLimit - 3), interchange.EI_InterchangeTextShort);
			AssertEquals("123" + interchange.EI_HeaderText + "456" + interchange.EI_BodyText + "789" + interchange.EI_FooterText.Left(LargeMessageHelper.DetailTextSizeLimit - interchange.EI_HeaderText.Length - interchange.EI_BodyText.Length - 9), interchange.EI_InterchangeTextDetail);

			interchangeMock.Reset();

			AssertEquals(interchange.EI_HeaderText + interchange.EI_BodyText + interchange.EI_FooterText, interchange.EI_InterchangeText);
			AssertEquals(interchange.EI_HeaderText + interchange.EI_BodyText.Left(1), interchange.EI_InterchangeTextShort);
			AssertEquals(interchange.EI_HeaderText + interchange.EI_BodyText + interchange.EI_FooterText.Left(LargeMessageHelper.DetailTextSizeLimit - interchange.EI_HeaderText.Length - interchange.EI_BodyText.Length), interchange.EI_InterchangeTextDetail);
		}

		public void TestInterchangeTextDetailFormatted()
		{
			var interchangeMock = Factory.NewMoq<EDIInterchange>();
			var interchange = interchangeMock.Object;
			interchange.EI_HeaderText = "HEADER";
			interchange.EI_BodyText = "BODY";
			interchange.EI_FooterText = "FOOTER";

			AssertMultilineASCIIEquals("Interchange should have headings applied", @"Interchange Header:
HEADER

Interchange Body:
BODY

Interchange Footer:
FOOTER", interchange.EI_InterchangeTextDetailFormatted);

			interchangeMock = Factory.NewMoq<EDIInterchange>();
			interchange = interchangeMock.Object;
			interchange.EI_BodyText = "BODY";
			interchange.EI_FooterText = "FOOTER";

			AssertMultilineASCIIEquals("Interchange should have headings applied when there is a footer", @"Interchange Body:
BODY

Interchange Footer:
FOOTER", interchange.EI_InterchangeTextDetailFormatted);

			interchangeMock = Factory.NewMoq<EDIInterchange>();
			interchange = interchangeMock.Object;
			interchange.EI_HeaderText = "HEADER";
			interchange.EI_BodyText = "BODY";

			AssertMultilineASCIIEquals("Interchange should have headings applied when there is a header", @"Interchange Header:
HEADER

Interchange Body:
BODY", interchange.EI_InterchangeTextDetailFormatted);

			interchangeMock = Factory.NewMoq<EDIInterchange>();
			interchange = interchangeMock.Object;
			interchange.EI_BodyText = "BODY";

			AssertEquals("Interchange should not have headings when there is no header or footer", "BODY", interchange.EI_InterchangeTextDetailFormatted);
		}

		public void TestInterchangeTextDetailFormattedTruncates()
		{
			var interchangeMock = Factory.NewMoq<EDIInterchange>();
			var interchange = interchangeMock.Object;
			interchange.EI_HeaderText = "HEADER";
			interchange.EI_BodyText = new string('A', LargeMessageHelper.DetailTextSizeLimit);
			interchange.EI_FooterText = "FOOTER";
			var interchangeText = @"Interchange Header:
HEADER

Interchange Body:
";

			AssertEquals(LargeMessageHelper.DetailTextSizeLimit, interchange.EI_InterchangeTextDetailFormatted.Length);

			AssertMultilineASCIIEquals("Interchange should properly truncate with headings", string.Format("{0}{1}", interchangeText, interchange.EI_BodyText.Substring(0, LargeMessageHelper.DetailTextSizeLimit - interchangeText.Length)), interchange.EI_InterchangeTextDetailFormatted);
		}

		public void TestSplitMultipleInterchanges()
		{
			ZString multipleInterchanges =
				"UNA:+.? 'UNB+UNOC:3+1STOP+GSL:ZZ+080220:1411+2051769'UNH+1+APERAK:D:99A:UN:ANZ23'BGM+7+RESP42265+9+RE'UNT+10+1'UNZ+1+2051769'" +
				"UNA:+.? 'UNB+UNOC:3+1STOP+GSL:ZZ+080220:1411+2051770'UNH+1+APERAK:D:99A:UN:ANZ23'BGM+7+RESP42265+9+RE'UNT+10+1'UNZ+1+2051770'";
			bool found69 = false;
			bool found70 = false;
			int count = 0;
			foreach (ZString splitInterchange in EDIInterchange.SplitMultipleInterchanges(multipleInterchanges))
			{
				count++;
				if (splitInterchange == "UNA:+.? 'UNB+UNOC:3+1STOP+GSL:ZZ+080220:1411+2051769'UNH+1+APERAK:D:99A:UN:ANZ23'BGM+7+RESP42265+9+RE'UNT+10+1'UNZ+1+2051769'")
				{
					found69 = true;
				}

				if (splitInterchange == "UNA:+.? 'UNB+UNOC:3+1STOP+GSL:ZZ+080220:1411+2051770'UNH+1+APERAK:D:99A:UN:ANZ23'BGM+7+RESP42265+9+RE'UNT+10+1'UNZ+1+2051770'")
				{
					found70 = true;
				}
			}
			AssertEquals("2 split interchanges", 2, count);
			Assert("Found 1st interchange as split string", found69);
			Assert("Found 2nd interchange as split string", found70);
		}

		public void TestEI_InterchangeText()
		{
			Interchange.EI_HeaderText = "HEADER";
			Interchange.EI_BodyText = "BODY";
			Interchange.EI_FooterText = "FOOTER";
			AssertEquals("InterchangeText", "HEADERBODYFOOTER", Interchange.EI_InterchangeText);
		}

		[TestDate(2009, 1, 2, 13, 20, 20)]
		[TestUtcOffset(8, 0, 0)]
		public void TestEI_InterchangeDateTime()
		{
			EDIInterchange interchange = Factory.NewWithValidTestData<EDIInterchange>();
			Factory.Save();
			ZDateTime time = interchange.EI_InterchangeDateTime;
			ZDateTime utcTime = interchange.EI_SystemCreateTimeUtc;
			AssertEquals(utcTime.AddHours(8), time);
		}

		public void TestEI_SizeInKBDoesNotHitDBWhenDataIsAlreadyLoaded()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.CACustoms;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			interchange.EI_HeaderText = "TestHeader";
			interchange.EI_BodyText = "TestBody";
			interchange.EI_FooterText = "TestFooter";
			Factory.Save();

			var reloadingFactory = new BusinessObjectFactory();
			var reloadedInterchange = reloadingFactory.Load<EDIInterchange>(interchange.PK);
			AssertEquals("Precondition: Blobs fields already loaded because the text is small", true, reloadedInterchange.BlobFieldsAlreadyLoaded);
			int dbLoadCount = Db.Connection.ExecutedCommandCount;

			AssertEquals("Size in KB", (ZDecimal)(10 + 8 + 10) / 1024, reloadedInterchange.EI_SizeInKB);
			AssertEquals("DB Load Count has not changed when hitting EI_SizeInKB", dbLoadCount, Db.Connection.ExecutedCommandCount);
		}

		public void TestNTextEI_SizeInKBWithForceDeprecatedNTextUsage()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			interchange.ForceDeprecatedNTextUsageForTesting = true;
			interchange.EI_BodyNText = "<ns0:Test xmlns:ns0=\"http://www.edi.com.au/EnterpriseService/\"><A><a1>a1</a1><a2>a2</a2></A><B><b1>b1</b1><b2>b2</b2></B></ns0:Test>";
			Factory.Save();

			var reloadingFactory = new BusinessObjectFactory();
			var reloadedInterchange = reloadingFactory.Load<EDIInterchange>(interchange.PK);

			AssertEquals("Blobs fields already loaded", true, reloadedInterchange.BlobFieldsAlreadyLoaded);
			AssertEquals("Size in KB", (ZDecimal)(interchange.EI_BodyNText.Length) / 1024, reloadedInterchange.EI_SizeInKB);
		}

		public void TestEI_SizeInKBWhenStoredInEI_BodyData()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.NativeDataMessaging;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			interchange.EI_BodyText = "<ns0:Test xmlns:ns0=\"http://www.edi.com.au/EnterpriseService/\"><A><a1>a1</a1><a2>a2</a2></A><B><b1>b1</b1><b2>b2</b2></B></ns0:Test>";
			AssertNotEquals("Precondition: EI_BodyData.Length", 0, interchange.EI_BodyData.Length);
			Factory.Save();

			var reloadingFactory = new BusinessObjectFactory();
			var reloadedInterchange = reloadingFactory.Load<EDIInterchange>(interchange.PK);

			AssertEquals("Precondition: Blobs fields already loaded because the text is small", true, reloadedInterchange.BlobFieldsAlreadyLoaded);
			int dbLoadCount = Db.Connection.ExecutedCommandCount;

			AssertEquals("Size in KB", (ZDecimal)(interchange.EI_BodyText.Length) / 1024, reloadedInterchange.EI_SizeInKB);
			AssertEquals("DB Load Count has not changed when hitting EI_SizeInKB", dbLoadCount, Db.Connection.ExecutedCommandCount);
		}

		public void TestLargeEI_BodyTextEI_SizeInKB()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.CACustoms;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			interchange.EI_BodyText = LargeMessageTestHelper.BigChunkOfXML;
			Factory.Save();

			var reloadingFactory = new BusinessObjectFactory();
			var reloadedInterchange = reloadingFactory.Load<EDIInterchange>(interchange.PK);
			AssertEquals("Precondition: Blob fields not yet loaded", false, reloadedInterchange.BlobFieldsAlreadyLoaded);

			AssertEquals("Size in KB", (ZDecimal)(LargeMessageTestHelper.BigChunkOfXML.Length) / 1024, reloadedInterchange.EI_SizeInKB);
			AssertEquals("Blob fields not yet loaded", false, reloadedInterchange.BlobFieldsAlreadyLoaded);
		}

		public void TestLargeValuesDontHitTheDataBaseMultipleTimesWhenEI_SizeInKBIsHitRepeatedly()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.CACustoms;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			interchange.EI_BodyText = LargeMessageTestHelper.BigChunkOfXML;
			Factory.Save();

			var reloadingFactory = new BusinessObjectFactory();
			var reloadedInterchange = reloadingFactory.Load<EDIInterchange>(interchange.PK);
			AssertEquals("Precondition: Blob fields not yet loaded", false, reloadedInterchange.BlobFieldsAlreadyLoaded);

			int dbLoadCount = Db.Connection.ExecutedCommandCount;
			AssertEquals("Size in KB", (ZDecimal)(LargeMessageTestHelper.BigChunkOfXML.Length) / 1024, reloadedInterchange.EI_SizeInKB);
			AssertEquals("DB Load Count has gone up by one", dbLoadCount += 1, Db.Connection.ExecutedCommandCount);
			AssertEquals("Blob fields not yet loaded", false, reloadedInterchange.BlobFieldsAlreadyLoaded);

			AssertEquals("Size in KB", (ZDecimal)(LargeMessageTestHelper.BigChunkOfXML.Length) / 1024, reloadedInterchange.EI_SizeInKB);
			AssertEquals("DB Load Count has not changed when hitting EI_SizeInKB again", dbLoadCount, Db.Connection.ExecutedCommandCount);
			AssertEquals("Blob fields not yet loaded", false, reloadedInterchange.BlobFieldsAlreadyLoaded);

			AssertEquals("Size in KB", (ZDecimal)(LargeMessageTestHelper.BigChunkOfXML.Length) / 1024, reloadedInterchange.EI_SizeInKB);
			AssertEquals("DB Load Count has not changed when hitting EI_SizeInKB again and again", dbLoadCount, Db.Connection.ExecutedCommandCount);
			AssertEquals("Blob fields not yet loaded", false, reloadedInterchange.BlobFieldsAlreadyLoaded);

			AssertEquals("Size in KB", (ZDecimal)(LargeMessageTestHelper.BigChunkOfXML.Length) / 1024, reloadedInterchange.EI_SizeInKB);
			AssertEquals("DB Load Count has not changed when hitting EI_SizeInKB again and again and again", dbLoadCount, Db.Connection.ExecutedCommandCount);
			AssertEquals("Blob fields not yet loaded", false, reloadedInterchange.BlobFieldsAlreadyLoaded);
		}

		public void TestBlobFieldsAlreadyLoaded()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.CACustoms;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			interchange.EI_BodyText = LargeMessageTestHelper.BigChunkOfXML;
			Factory.Save();

			var reloadingFactory = new BusinessObjectFactory();
			var reloadedInterchange = reloadingFactory.Load<EDIInterchange>(interchange.PK);

			AssertEquals("Blob fields not yet loaded", false, reloadedInterchange.BlobFieldsAlreadyLoaded);
			AssertEquals(reloadedInterchange.EI_InterchangeText, LargeMessageTestHelper.BigChunkOfXML);
			AssertEquals("Blobs fields already loaded", true, reloadedInterchange.BlobFieldsAlreadyLoaded);
		}

		internal const string LargeXml = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Header>
		<SenderID />
		<RecipientID />
	</Header>
	<Body>
		<UniversalEvent version = ""1.1"" >
			< Event >
				< EventType > ACD </ EventType >
				< EventReference > FAC = CY | LOC = USLUI </ EventReference >
				< EventTime > 2016 - 05 - 12T00:00:00</EventTime>
				<IsEstimate>false</IsEstimate>
				<ContextCollection>
					<Context>
						<Type>CarrierCode</Type>
						<Value>HLCU</Value>
					</Context>
					<Context>
						<Type>ContainerNumber</Type>
						<Value>GESU6315627</Value>
					</Context>
					<Context>
						<Type>LegOriginUNLOCO</Type>
						<Value>FRBOD</Value>
					</Context>
					<Context>
						<Type>PortOfLoadingUNLOCO</Type>
						<Value>FRLEH</Value>
					</Context>
					<Context>
						<Type>VoyageNumber</Type>
						<Value>074W</Value>
					</Context>
					<Context>
						<Type>LegDestinationUNLOCO</Type>
						<Value>USORF</Value>
					</Context>
					<Context>
						<Type>ContainerFinalDestinationUNLOCO</Type>
						<Value>USLUI</Value>
					</Context>
					<Context>
						<Type>ProviderReference</Type>
						<Value>213650</Value>
					</Context>
					<Context>
						<Type>MBOLNumber</Type>
						<Value>LE1160317018</Value>
					</Context>
				</ContextCollection>
			</Event>
		</UniversalEvent>
	</Body>
</UniversalInterchange>";

		public void TestEI_LargeNTextEI_SizeInKBWithForceDeprecatedNTextUsage()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			interchange.ForceDeprecatedNTextUsageForTesting = true;
			interchange.EI_BodyNText = LargeXml;
			Factory.Save();

			var reloadingFactory = new BusinessObjectFactory();
			var reloadedInterchange = reloadingFactory.Load<EDIInterchange>(interchange.PK);
			AssertEquals("Precondition: Blobs fields not loaded", false, reloadedInterchange.BlobFieldsAlreadyLoaded);

			AssertEquals("Size in KB", (ZDecimal)(interchange.EI_BodyNText.Length) / 1024, reloadedInterchange.EI_SizeInKB);
			AssertEquals("Blobs fields still not loaded", false, reloadedInterchange.BlobFieldsAlreadyLoaded);
		}

		public void TestLargeBodyDataEI_SizeInKB()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.NativeDataMessaging;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			interchange.EI_From = "From";
			interchange.EI_To = "To";
			interchange.EI_BodyText = LargeMessageTestHelper.BigChunkOfXML;
			AssertNotEquals("Precondition: EI_BodyData.Length", 0, interchange.EI_BodyData.Length);
			Factory.Save();

			var reloadingFactory = new BusinessObjectFactory();
			var reloadedInterchange = reloadingFactory.Load<EDIInterchange>(interchange.PK);

			AssertEquals("Blobs not yet loaded", false, reloadedInterchange.BlobFieldsAlreadyLoaded);
			var dataLengthInDB = LargeMessageTestHelper.GetLengthStoredInDB(interchange, EDIInterchangeSchema.EI_BodyData);
			AssertEquals("Size in KB", (ZDecimal)(dataLengthInDB) / 1024, reloadedInterchange.EI_SizeInKB);
			AssertEquals("Blob fields not yet loaded", false, reloadedInterchange.BlobFieldsAlreadyLoaded);
			AssertEquals(reloadedInterchange.EI_InterchangeText, LargeMessageTestHelper.BigChunkOfXML);
			AssertEquals("Blobs already loaded", true, reloadedInterchange.BlobFieldsAlreadyLoaded);
		}

		public void TestUploadLargeInterchange()
		{
			string intputFilePath = LargeMessageTestHelper.CreateTestFile(LargeMessageTestHelper.FileType.Text, 100 * 1024 * 1024);
			long inputFileLength = (new FileInfo(intputFilePath)).Length;

			LargeFileHolder tester = new LargeFileHolder(intputFilePath);

			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.SetEI_BodyTextSource(tester);
			interchange.EI_HeaderText = "HEADER";
			interchange.EI_FooterText = "FOOTER";
			interchange.EI_To = "test";
			interchange.EI_From = "test";

			Factory.Save();
			interchange.Refresh();

			AssertEquals(LargeMessageHelper.ShortTextSizeLimit, interchange.EI_BodyTextShort.Length);
			AssertEquals(LargeMessageHelper.DetailTextSizeLimit, interchange.EI_BodyTextDetail.Length);

			AssertEquals(LargeMessageHelper.ShortTextSizeLimit, interchange.EI_InterchangeTextShort.Length);
			AssertEquals(LargeMessageHelper.DetailTextSizeLimit, interchange.EI_InterchangeTextDetail.Length);

			string outputBodyPath = Temp.GetTempFileName();
			using (StreamWriter writer = new StreamWriter(outputBodyPath))
			{
				using (TextReader reader = interchange.GetEI_BodyTextReader())
				{
					writer.AddStream(reader);
				}
			}
			AssertEquals(inputFileLength, new FileInfo(outputBodyPath).Length);
			File.Delete(intputFilePath);
			File.Delete(outputBodyPath);
		}

		public void TestUploadLargeNTextInterchange()
		{
			string intputFilePath = LargeMessageTestHelper.CreateTestFile(LargeMessageTestHelper.FileType.NText, 1 * 1024 * 1024);
			try
			{
				long inputFileLength = (new FileInfo(intputFilePath)).Length;
				using (var inputFile = new FileStream(intputFilePath, FileMode.Open))
				{
					var fileSource = new TextReaderSource(inputFile);

					EDIInterchange interchange = Factory.New<EDIInterchange>();
					interchange.EI_ApplicationCode = ApplicationCodeList.Codes.XMS;
					interchange.EI_HeaderText = "HEADER";
					interchange.EI_FooterText = "FOOTER";
					interchange.EI_To = "test";
					interchange.EI_From = "test";
					interchange.SetEI_BodyTextSource(fileSource);

					Factory.Save();
					interchange.Reload();

					string outputBodyPath = Temp.GetTempFileName();
					try
					{
						using (StreamWriter writer = new StreamWriter(outputBodyPath))
						{
							using (TextReader reader = interchange.GetEI_BodyTextReader())
							{
								writer.AddStream(reader);
							}
						}
						AssertEquals(inputFileLength, new FileInfo(outputBodyPath).Length);
					}
					finally
					{
						File.Delete(outputBodyPath);
					}
				}
			}
			finally
			{
				File.Delete(intputFilePath);
			}
		}

		#region SetEX1StyleInterchange
		public void TestSetEI_InterchangeTextEX1Good()
		{
			string interchangeString = "UNA:+.? 'UNB+UNOA:2+CUSSWT:ZZZ+00009917B:ZZZ+030509:1203+407'UNH+1320+CUSRES:D:96B:UN+SSYD00001093'BGM+963+00000000'GIS+841:120:143'ERP+001::530'ERC+148::143'ERP+001::515'ERC+491::143'UNT+8+1320'UNZ+1+407'";
			Interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString);
			Assert(Interchange.ContainedMessages.Count == 1);
			Assert(true);
		}

		public void TestEscapedDelimiter()
		{
			string interchangeString = "UNB+UNOA:2+CUSSWT:ZZZ+00009917B:ZZZ+030509:1203+407'UNH+1320+CUSRES:D:96B:UN+SSYD0?'0001093'BGM+963+00000000'GIS+841:120:143'ERP+001::530'ERC+148::143'ERP+001::515'ERC+491::143'UNT+8+1320'UNZ+1+407'";

			Interchange.EI_ApplicationCode = "EX1";
			Interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString);
			Assert(Interchange.EI_BodyText.Contains("SSYD0?'0001093"));
		}

		public void TestSetEI_InterchangeTextEX1GoodMultipleMessage()
		{
			string interchangeString = "UNB+UNOB:2+CUSSWT:ZZZ+00009917B:ZZZ+030509:1203+407'UNH+1320+CUSRES:D:96B:UN+SSYD00001093'BGM+963+00000000'GIS+841:120:143'ERP+001::530'ERC+148::143'ERP+001::515'ERC+491::143'UNT+8+1320'UNH+1320+CUSRES:D:96B:UN+SSYD00001093'BGM+963+00000000'GIS+841:120:143'ERP+001::530'ERC+148::143'ERP+001::515'ERC+491::143'UNT+8+1320'UNZ+1+407'";
			interchangeString = interchangeString
				.Replace("'", System.Text.Encoding.ASCII.GetString(new byte[1] { 28 }))
				.Replace("+", System.Text.Encoding.ASCII.GetString(new byte[1] { 29 }))
				.Replace(":", System.Text.Encoding.ASCII.GetString(new byte[1] { 31 }));

			Interchange.EI_ApplicationCode = "EX1";
			Interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString);
			Assert(true);
		}

		public void TestSetEI_InterchangeTextEX1MismatchedUNHUNT1()
		{
			string interchangeString = "UNB+UNOB:2+CUSSWT:ZZZ+00009917B:ZZZ+030509:1203+407'UNH+1320+CUSRES:D:96B:UN+SSYD00001093'BGM+963+00000000'GIS+841:120:143'ERP+001::530'ERC+148::143'ERP+001::515'ERC+491::143'UNZ+1+407'";
			interchangeString = interchangeString
				.Replace("'", System.Text.Encoding.ASCII.GetString(new byte[1] { 28 }))
				.Replace("+", System.Text.Encoding.ASCII.GetString(new byte[1] { 29 }))
				.Replace(":", System.Text.Encoding.ASCII.GetString(new byte[1] { 31 }));

			Interchange.EI_ApplicationCode = "EX1";
			Interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString);
			Assert(Interchange.EI_HeaderText.Contains("030509"));
			Assert(Interchange.EI_BodyText.Contains("SSYD00001093"));
			Assert(Interchange.EI_FooterText.Contains("407"));
		}

		public void TestSetEI_InterchangeTextEX1MismatchedUNHUNT2()
		{
			string interchangeString = "UNB+UNOB:2+CUSSWT:ZZZ+00009917B:ZZZ+030509:1203+407'BGM+963+00000000'GIS+841:120:143'ERP+001::530'ERC+148::143'ERP+001::515'ERC+491::143'UNT+8+1320'UNZ+1+407'";
			interchangeString = interchangeString
				.Replace("'", System.Text.Encoding.ASCII.GetString(new byte[1] { 28 }))
				.Replace("+", System.Text.Encoding.ASCII.GetString(new byte[1] { 29 }))
				.Replace(":", System.Text.Encoding.ASCII.GetString(new byte[1] { 31 }));

			Interchange.EI_ApplicationCode = "EX1";
			Interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString);
			Assert(Interchange.EI_HeaderText.Contains("030509"));
			Assert(Interchange.EI_BodyText.Contains("148"));
			Assert(Interchange.EI_FooterText.Contains("407"));
		}

		public void TestSetEI_InterchangeTextEX1MismatchedUNHUNT4()
		{
			string interchangeString = "UNB+UNOB:2+CUSSWT:ZZZ+00009917B:ZZZ+030509:1203+407'UNH+1320+CUSRES:D:96B:UN+SSYD00001093'BGM+963+00000000'GIS+841:120:143'ERP+001::530'ERC+148::143'ERP+001::515'ERC+491::143'UNT+8+1320'UNT+8+1320'UNZ+1+407'";
			interchangeString = interchangeString
				.Replace("'", System.Text.Encoding.ASCII.GetString(new byte[1] { 28 }))
				.Replace("+", System.Text.Encoding.ASCII.GetString(new byte[1] { 29 }))
				.Replace(":", System.Text.Encoding.ASCII.GetString(new byte[1] { 31 }));

			Interchange.EI_ApplicationCode = "EX1";
			Interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString);
			Assert(Interchange.EI_HeaderText.Contains("030509"));
			Assert(Interchange.EI_BodyText.Contains("841"));
			Assert(Interchange.EI_FooterText.Contains("407"));
		}

		[ExpectException(typeof(Exception))]
		public void TestSetEI_InterchangeTextEX1MissingUNB()
		{
			string interchangeString = "UNH+1320+CUSRES:D:96B:UN+SSYD00001093'BGM+963+00000000'GIS+841:120:143'ERP+001::530'ERC+148::143'ERP+001::515'ERC+491::143'UNT+8+1320'UNZ+1+407'";
			interchangeString = interchangeString
				.Replace("'", System.Text.Encoding.ASCII.GetString(new byte[1] { 28 }))
				.Replace("+", System.Text.Encoding.ASCII.GetString(new byte[1] { 29 }))
				.Replace(":", System.Text.Encoding.ASCII.GetString(new byte[1] { 31 }));

			Interchange.EI_ApplicationCode = "EX1";
			Interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString);
		}

		[ExpectException(typeof(Exception))]
		public void TestSetEI_InterchangeTextEX1MissingUNZ()
		{
			string interchangeString = "UNB+UNOA:2+CUSSWT:ZZZ+00009917B:ZZZ+030509:1203+407'UNH+1320+CUSRES:D:96B:UN+SSYD00001093'BGM+963+00000000'GIS+841:120:143'ERP+001::530'ERC+148::143'ERP+001::515'ERC+491::143'UNT+8+1320'";
			interchangeString = interchangeString
				.Replace("'", System.Text.Encoding.ASCII.GetString(new byte[1] { 28 }))
				.Replace("+", System.Text.Encoding.ASCII.GetString(new byte[1] { 29 }))
				.Replace(":", System.Text.Encoding.ASCII.GetString(new byte[1] { 31 }));

			Interchange.EI_ApplicationCode = "EX1";
			Interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString);
		}

		#endregion

		public void TestForceApplicationCode()
		{
			const string interchangeText =
				"UNA:+.? '" +
				"UNB+UNOC:3+1STOP+GSL:ZZ+080220:1411+2051769'" +
				"UNH+1+APERAK:D:99A:UN:ANZ23'" +
				"BGM+7+RESP42265+9+RE'" +
				"DTM+137:20080220141119:204'" +
				"DOC+640+6'" +
				"DTM+137:20080220124400:204'" +
				"NAD+MS+1STOP'" +
				"NAD+MR+GSL'" +
				"ERC+COM025'" +
				"FTX+AAO+++RECEIVER CODE UNKNOWN'" +
				"UNT+10+1'" +
				"UNZ+1+2051769'" +
				"";

			EDIInterchange interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeText, EDIInterchange.ApplicationCodes.EIDO);
			AssertEquals("Should use the passed in application code", EDIInterchange.ApplicationCodes.EIDO, interchange.EI_ApplicationCode);
		}

		public void TestFunctionalGroup()
		{
			const string interchangeText =
				"UNA:+.? '" +
				"UNB+UNOC:3+1STOP+GSL:ZZ+080220:1411+2051769'" +
				"UNG+GSMCAR+24681012+SRP+040121:0930+246810+UN+D:00A:SUPRPT'" +
				"UNH+1+APERAK:D:99A:UN:ANZ23'" +
				"BGM+7+RESP42265+9+RE'" +
				"DTM+137:20080220141119:204'" +
				"DOC+640+6'" +
				"DTM+137:20080220124400:204'" +
				"NAD+MS+1STOP'" +
				"NAD+MR+GSL'" +
				"ERC+COM025'" +
				"FTX+AAO+++RECEIVER CODE UNKNOWN'" +
				"UNT+10+1'" +
				"UNE+1+246810'" +
				"UNZ+1+2051769'" +
				"";

			EDIInterchange interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeText, EDIInterchange.ApplicationCodes.EIDO, true, false);
			Assert("Should not contain UNG", !interchange.EI_InterchangeText.Contains("UNG+"));
			Assert("Should not contain UNE", !interchange.EI_InterchangeText.Contains("UNE+"));
			AssertEquals("Should contain spawned message", 1, interchange.ContainedMessages.Count);
			AssertEquals("Direction should be receive", EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);

			interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeText, EDIInterchange.ApplicationCodes.EIDO, false, false);
			Assert("Should contain UNG", interchange.EI_InterchangeText.Contains("UNG+"));
			Assert("Should contain UNE", interchange.EI_InterchangeText.Contains("UNE+"));
			AssertEquals("Should contain spawned message", 1, interchange.ContainedMessages.Count);
			AssertEquals("Direction should be receive", EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);

			interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeText, EDIInterchange.ApplicationCodes.EIDO, false, true);
			AssertEquals("Header should contain UNG", "UNA:+.? 'UNB+UNOC:3+1STOP+GSL:ZZ+080220:1411+2051769'UNG+GSMCAR+24681012+SRP+040121:0930+246810+UN+D:00A:SUPRPT'", interchange.EI_HeaderText);
			Assert("Body should not contain UNG", !interchange.EI_BodyText.Contains("UNG+"));
			Assert("Interchange should contain UNG", interchange.EI_InterchangeText.Contains("UNG+"));
			AssertEquals("Footer should contain UNE", "UNE+1+246810'UNZ+1+2051769'", interchange.EI_FooterText);
			Assert("Body should not contain UNE", !interchange.EI_BodyText.Contains("UNE+"));
			Assert("Interchange should not contain UNE", interchange.EI_InterchangeText.Contains("UNE+"));
			AssertEquals("Should not contain spawned message", 0, interchange.ContainedMessages.Count);

			interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeText, EDIInterchange.ApplicationCodes.EIDO, true, true);
			Assert("Should not contain UNG", !interchange.EI_InterchangeText.Contains("UNG+"));
			Assert("Should not contain UNE", !interchange.EI_InterchangeText.Contains("UNE+"));
			AssertEquals("Should not contain spawned message", 0, interchange.ContainedMessages.Count);
		}

		public void TestReplacePlaceHolders()
		{
			string header = "UNA:+.? 'UNB+UNOB:2+EDICUS:ZZ+JJMAUS-IFCSUM:ZZ+040401:1339+<<INTERCHANGENUMBERPLACEHOLDER>>++IFCSUM++++1'UNG+GSMCAR+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>+SRP+040121:0930+246810+UN+D:00A:SUPRPT'";
			string body = "UNH+00000000000001+IFCSUM:D:94B:UN'UNT+22+00000000000001'";
			string footer = "UNE+1+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>'UNZ+13+<<INTERCHANGENUMBERPLACEHOLDER>>'";

			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_HeaderText = header;
			interchange.EI_BodyText = body;
			interchange.EI_FooterText = footer;
			interchange.EI_From = "FROM";
			interchange.EI_To = "TO";
			interchange.EI_InterchangeNum = ZString.Empty;
			Factory.Save();

			AssertEquals("Header", "UNA:+.? 'UNB+UNOB:2+EDICUS:ZZ+JJMAUS-IFCSUM:ZZ+040401:1339+1++IFCSUM++++1'UNG+GSMCAR+1+SRP+040121:0930+246810+UN+D:00A:SUPRPT'", interchange.EI_HeaderText);
			AssertEquals("Body", body, interchange.EI_BodyText);
			AssertEquals("Footer", "UNE+1+1'UNZ+13+1'", interchange.EI_FooterText);
		}

		public void TestReplaceUniqueBatchNumberPlaceHolder()
		{
			var message = Factory.New<TestEdiMessage>();
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + EDIMessage.UniqueBatchNumberPlaceHolder;
			message.ShouldReplaceUniqueBatchNumberExposed = false;
			Factory.Save();
			Assert("Precondition:MessageTextNotContainsBatchNumber", message.EM_MessageText.Contains(EDIMessage.UniqueBatchNumberPlaceHolder));

			EDIInterchangeForTesting interchange = Factory.New<EDIInterchangeForTesting>();
			interchange.EI_BodyText = message.EM_MessageText;
			interchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			interchange.EI_From = "FROM";
			interchange.EI_To = "TO";
			interchange.ContainedMessages.Add(message);
			Factory.Save();
			Assert("MessageTextContainsBatchNumber", message.EM_MessageText.Contains("GetBatchNumberResult"));
			Assert("InterchangeTextContainsBatchNumber", interchange.EI_BodyText.Contains("GetBatchNumberResult"));
		}

		public void TestIsTestInterchange()
		{
			string interchangeString = "UNA:+.? 'UNB+UNOA:2+CUSSWT:ZZZ+00009917B:ZZZ+030509:1203+407'UNH+1320+CUSRES:D:96B:UN+SSYD00001093'BGM+963+00000000'GIS+841:120:143'ERP+001::530'ERC+148::143'ERP+001::515'ERC+491::143'UNT+8+1320'UNZ+1+407'";
			EDIInterchange interchange1 = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString);
			Assert("Is not test intechnage", !interchange1.IsTestInterchange);
			interchangeString = "UNA:+.? 'UNB+UNOA:2+CUSSWT:ZZZ+00009917B:ZZZ+030509:1203+407++++++1'UNH+1320+CUSRES:D:96B:UN+SSYD00001093'BGM+963+00000000'GIS+841:120:143'ERP+001::530'ERC+148::143'ERP+001::515'ERC+491::143'UNT+8+1320'UNZ+1+407'";
			EDIInterchange interchange2 = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString);
			Assert("Is  test intechnage", interchange2.IsTestInterchange);
		}

		public void TestSpawnMessagesFromInterchageText()
		{
			var interchangeString = "UNA:+.? 'UNB+UNOA:2+CUSSWT:ZZZ+00009917B:ZZZ+030509:1203+407'UNH+1320+CUSRES:D:96B:UN+SSYD00001093'BGM+963+00000000'GIS+841:120:143'ERP+001::530'ERC+148::143'ERP+001::515'ERC+491::143'UNT+8+1320'UNZ+1+407'";
			Interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString);
			AssertEquals("EDIMessageNumber", 1, Interchange.ContainedMessages.Count);
			AssertEquals("EDIMessageText", "UNH+1320+CUSRES:D:96B:UN+SSYD00001093'BGM+963+00000000'GIS+841:120:143'ERP+001::530'ERC+148::143'ERP+001::515'ERC+491::143'UNT+8+1320'", Interchange.ContainedMessages[0].EM_MessageText);

			interchangeString = "UNB+UNOA:3+INETCECPT+YUSAIRXPN+100721:0718+40++++++1'UNG+CUSRES+QRCLASS/TAR+U10207V1+100721:0718+40+UN+S:99B'RA1022042910'RA11DC942311 CLASS NUMBER NOT NUMERIC'UNE+1+40'UNZ+1+40'";
			Interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString, "", true, false);
			AssertEquals("EDIMessageNumber", 1, Interchange.ContainedMessages.Count);
			AssertEquals("CADEX wrapped text", "RA1022042910'RA11DC942311 CLASS NUMBER NOT NUMERIC'", Interchange.ContainedMessages[0].EM_MessageText);
		}

		[ExpectNoExceptions]
		public void TestPostingLargeInterchangeUsingInsert()
		{
			EDIInterchange interchange = EDIInterchange.New(Factory);
			interchange.EI_From = "Santa";
			interchange.EI_To = "Elves";
			LoadLargePackage(interchange);
			Factory.Save();
		}

		void LoadLargePackage(EDIInterchange interchange)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < 100000; i++)
			{
				builder.Append("FTX+A07+++FULL DUTY LESS THE DUTY PAID ON ENTRY: : '");
			}
			interchange.EI_BodyText = builder.ToString();
		}

		[ExpectNoExceptions]
		public void TestPostingLargeInterchangeUsingUpdate()
		{
			EDIInterchange interchange = EDIInterchange.New(Factory);
			interchange.EI_From = "Santa";
			interchange.EI_To = "Elves";
			Factory.Save();
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < 100000; i++)
			{
				builder.Append("FTX+A07+++FULL DUTY LESS THE DUTY PAID ON ENTRY: : '");
			}
			interchange.EI_BodyText = builder.ToString();
			Factory.Save();
		}

		public void TestUNOACharacterSet()
		{
			Interchange.EI_HeaderText = "UNA:+.? 'UNB+UNOA:2+CUSSWT:ZZZ+00009917B:ZZZ+030509:1203+407'";
			UNCharacterSet uNOA = new UNOACharacterSet();
			AssertEquals("ElementDelimiter", uNOA.ElementDelimiter, Interchange.CharacterSet.ElementDelimiter);
		}

		public void TestUNOBCharacterSet()
		{
			Interchange.EI_HeaderText = "UNA.  UNBUNOB";
			UNCharacterSet uNOB = new UNOBCharacterSet();
			AssertEquals("ElementDelimiter", uNOB.ElementDelimiter, Interchange.CharacterSet.ElementDelimiter);
		}

		public void TestSetTestFlagNZCIsTrue()
		{
			string interchangeString = "UNA:+.? 'UNB+UNOA:2+CUSSWT:ZZZ+00009917B:ZZZ+030509:1203+407'UNH+1320+CUSRES:D:96B:UN+SSYD00001093'BGM+963+00000000'GIS+841:120:143'ERP+001::530'ERC+148::143'ERP+001::515'ERC+491::143'UNT+8+1320'UNZ+1+407'";

			Interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString);

			AssertEquals("EDIMessageNumber", 1, Interchange.ContainedMessages.Count);

			var myMessage = Interchange.ContainedMessages[0];
			Assert("NotTestMode", myMessage.EM_IsTestMessage);
		}

		public void TestSetTestFlagNZCIsFalse()
		{
			string interchangeString = "UNA:+.? 'UNB+UNOA:2+CUSMOD:ZZZ+00009917B:ZZZ+030509:1203+407'UNH+1320+CUSRES:D:96B:UN+SSYD00001093'BGM+963+00000000'GIS+841:120:143'ERP+001::530'ERC+148::143'ERP+001::515'ERC+491::143'UNT+8+1320'UNZ+1+407'";

			Interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString);

			AssertEquals("EDIMessageNumber", 1, Interchange.ContainedMessages.Count);

			var myMessage = Interchange.ContainedMessages[0];
			Assert("NotTestMode", !myMessage.EM_IsTestMessage);
		}

		public void TestSetTestFlagOneStopIsFalse()
		{
			string interchangeString = "UNB+UNOC:3+1STOP:ZZ+EDIAL+040715:1044+0025'UNH+10071+APERAK:D:00A:UN:ANZ23'BGM+7+10071+9+RE'DTM+137:20040715104416:204'DOC+ERA+EDISRW'DTM+137:20040715103640:204'RFF+ERN:10407'NAD+MS+1STOP'NAD+MR+EDISRW'ERC+ERA0019'FTX+AAO+++ISO SIZE/TYPE NOT DEFINED- NOR'ERC+ERA0028'FTX+AAO+++TEMPERATURE NOT SUPPLIED FOR REEFER CARGO'RFF+EQD:POCU2819798'UNT+14+10071'UNZ+1+0025'";

			Interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString);

			AssertEquals("EDIMessageNumber", 1, Interchange.ContainedMessages.Count);

			var myMessage = Interchange.ContainedMessages[0];
			Assert("NotTestMode", !myMessage.EM_IsTestMessage);
		}

		public void TestSetTestFlagCSXIsFalse()
		{
			string interchangeString = "UNB+UNOC:3+CSXWTADL:ZZ+EDIALT+040715:1044+0025'UNH+10071+APERAK:D:00A:UN:ANZ23'BGM+7+10071+9+RE'DTM+137:20040715104416:204'DOC+ERA+EDISRW'DTM+137:20040715103640:204'RFF+ERN:10407'NAD+MS+1STOP'NAD+MR+EDISRW'ERC+ERA0019'FTX+AAO+++ISO SIZE/TYPE NOT DEFINED- NOR'ERC+ERA0028'FTX+AAO+++TEMPERATURE NOT SUPPLIED FOR REEFER CARGO'RFF+EQD:POCU2819798'UNT+14+10071'UNZ+1+0025'";

			Interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString);

			AssertEquals("EDIMessageNumber", 1, Interchange.ContainedMessages.Count);

			var myMessage = Interchange.ContainedMessages[0];
			Assert("NotTestMode", !myMessage.EM_IsTestMessage);
		}

		public void TestPreparationDateTimeCMR()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(typeof(EDIInterchangeTest).Assembly))
			{
				var testFilePath = resourceRetriever.SaveResourceToFile("CMRHeader.txt");
				Interchange.EI_ApplicationCode = "CMR";
				Interchange.EI_HeaderText = File.ReadAllText(testFilePath);
				var prepTime = Interchange.PreparationDateTime.ToDateTime();
				AssertEquals("PreperationTime", new DateTime(2003, 10, 13, 10, 33, 0), prepTime);
			}
		}

		public void TestHasBeenAcknowledgedByERouter()
		{
			AssertEquals("HasBeenAcknowledgedByERouter", false, Interchange.HasBeenAcknowledgedByERouter);
			EDIMessage message = Interchange.InterchangeAcknowledgementMessages.AddNew();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.ERouter;
			AssertEquals("HasBeenAcknowledgedByERouter", true, Interchange.HasBeenAcknowledgedByERouter);
		}

		public void TestDelete()
		{
			EDIMessage message_1 = Interchange.ContainedMessages.AddNew();
			EDIMessage message_2 = Interchange.ContainedMessages.AddNew();
			Interchange.Delete();
			AssertEquals("MessageDeleted", true, message_1.IsDeleted);
			AssertEquals("MessageDeleted", true, message_2.IsDeleted);
		}

		#region IFCSUM and IFTSTA Interchange

		public void TestCreateNewIfcsumInterchange()
		{
			var header = "UNA:+.? 'UNB+UNOB:2+EDICUS:ZZ+JJMAUS-IFCSUM:ZZ+040401:1339+00000000000001++IFCSUM++++1'";
			var body = "UNH+00000000000001+IFCSUM:D:94B:UN'UNT+22+00000000000001'";
			var footer = "UNZ+13+00000000000001'";

			var interchangeString = header + body + footer;
			Interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString);

			AssertEquals("Application Code", EDIInterchange.ApplicationCodes.ForwarderEdifact, Interchange.EI_ApplicationCode);

			AssertEquals("Number", "00000000000001", Interchange.EI_InterchangeNum);
			AssertEquals("From", "EDICUS", Interchange.EI_From);
			AssertEquals("To", "JJMAUS-IFCSUM", Interchange.EI_To);

			AssertEquals("Header", header, Interchange.EI_HeaderText);
			AssertEquals("Body", body, Interchange.EI_BodyText);
			AssertEquals("Footer", footer, Interchange.EI_FooterText);
		}

		public void TestCreateNewIftstaInterchange()
		{
			var header = "UNA:+.? 'UNB+UNOB:2+EDICUS:ZZ+JJMAUS-IFTSTA:ZZ+040401:1340+00000000000001++IFTSTA++++1'";
			var body = "UNH+00000000000001+IFTSTA:D:98A:UN'UNT+14+00000000000001'";
			var footer = "UNZ+59+00000000000001'";

			var interchangeString = header + body + footer;
			Interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString);

			AssertEquals("Application Code", EDIInterchange.ApplicationCodes.ForwarderEdifact, Interchange.EI_ApplicationCode);

			AssertEquals("Number", "00000000000001", Interchange.EI_InterchangeNum);
			AssertEquals("From", "EDICUS", Interchange.EI_From);
			AssertEquals("To", "JJMAUS-IFTSTA", Interchange.EI_To);

			AssertEquals("Header", header, Interchange.EI_HeaderText);
			AssertEquals("Body", body, Interchange.EI_BodyText);
			AssertEquals("Footer", footer, Interchange.EI_FooterText);
		}

		#endregion

		public void TestXMSInterchange()
		{
			//Application Code is XMS
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.XMS;
			interchange.EI_From = "EDIEDIDAT";
			interchange.EI_To = "EDIEDIDAT";
			interchange.EI_HeaderText = "你HEADER";
			interchange.EI_BodyText = "好BODY";
			interchange.EI_FooterText = "么FOOTER";

			interchange.Validation.ValidateEI_HeaderText();
			interchange.Validation.ValidateEI_BodyText();
			interchange.Validation.ValidateEI_FooterText();

			var errorMessage = " only accepts Western European languages characters.";
			Assert("HeaderText is Western European validation succeeded.", !interchange.EI_HeaderTextInfo.HasError("Header" + errorMessage));
			Assert("BodyText is Western European validation succeeded.", !interchange.EI_BodyTextInfo.HasError("Body" + errorMessage));
			Assert("FooterText is Western European validation succeeded.", !interchange.EI_FooterTextInfo.HasError("Footer" + errorMessage));

			Factory.Save();

			var loadedInterchange = new BusinessObjectFactory().Load<EDIInterchange>(interchange.PK);
			AssertEquals("InterchangeText", "你HEADER好BODY么FOOTER", loadedInterchange.EI_InterchangeText);
			AssertEquals("HeaderNText", "你HEADER", loadedInterchange.EI_HeaderNText);
			AssertEquals("EI_BodyDataAsText", "好BODY", loadedInterchange.EI_BodyDataAsText);
			AssertEquals("FooterNText", "么FOOTER", loadedInterchange.EI_FooterNText);

			//Application Code is something else
			var interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_ApplicationCode = EDIInterchange.ApplicationCodes.AirCargo;
			interchange1.EI_From = "EDIEDIDAT";
			interchange1.EI_To = "EDIEDIDAT";
			interchange1.EI_HeaderText = "你HEADER";
			interchange1.EI_BodyText = "好BODY";
			interchange1.EI_FooterText = "么FOOTER";

			interchange1.Validation.ValidateEI_HeaderText();
			interchange1.Validation.ValidateEI_BodyText();
			interchange1.Validation.ValidateEI_FooterText();

			Assert("Error expected - HeaderText is Western European validation failed.", interchange1.EI_HeaderTextInfo.HasError("Header" + errorMessage));
			Assert("Error expected - BodyText is Western European validation failed.", interchange1.EI_BodyTextInfo.HasError("Body" + errorMessage));
			Assert("Error expected - FooterText is Western European validation failed.", interchange1.EI_FooterTextInfo.HasError("Footer" + errorMessage));

			interchange1.EI_HeaderText = "HEADER";
			interchange1.EI_BodyText = "BODY";
			interchange1.EI_FooterText = "FOOTER";

			Factory.Save();

			var loadedinterchange1 = new BusinessObjectFactory().Load<EDIInterchange>(interchange1.PK);
			AssertEquals("interchangeText", "HEADERBODYFOOTER", loadedinterchange1.EI_InterchangeText);
			AssertEquals("HeaderNText", "", loadedinterchange1.EI_HeaderNText);
			AssertEquals("EI_BodyDataAsText", "", loadedinterchange1.EI_BodyDataAsText);
			AssertEquals("FooterNText", "", loadedinterchange1.EI_FooterNText);
		}

		public void TestReadTextGEIInterchange()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.Unknown;
			interchange.EI_From = "EDIEDIDAT";
			interchange.EI_To = "EDIEDIDAT";
			interchange.EI_HeaderText = "HEADER";
			interchange.EI_BodyText = "BODY";
			interchange.EI_FooterText = "FOOTER";
			Factory.Save();

			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GlobalElectronicInvoice;
			Factory.Save();

			var loadedInterchange = new BusinessObjectFactory().Load<EDIInterchange>(interchange.PK);
			AssertEquals("InterchangeText", "HEADERBODYFOOTER", loadedInterchange.EI_InterchangeText);
			AssertEquals("HeaderText", "HEADER", loadedInterchange.EI_HeaderText);
			AssertEquals("BodyText", "BODY", loadedInterchange.EI_BodyText);
			AssertEquals("FooterText", "FOOTER", loadedInterchange.EI_FooterText);
		}

		public void TestReadBlobGEIInterchange()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GlobalElectronicInvoice;
			interchange.EI_From = "EDIEDIDAT";
			interchange.EI_To = "EDIEDIDAT";
			interchange.EI_HeaderText = "你HEADER";
			interchange.EI_BodyText = "好BODY";
			interchange.EI_FooterText = "么FOOTER";
			Factory.Save();

			var loadedInterchange = new BusinessObjectFactory().Load<EDIInterchange>(interchange.PK);
			AssertEquals("InterchangeText", "你HEADER好BODY么FOOTER", loadedInterchange.EI_InterchangeText);
			AssertEquals("HeaderText", "你HEADER", loadedInterchange.EI_HeaderText);
			AssertEquals("BodyText", "好BODY", loadedInterchange.EI_BodyText);
			AssertEquals("FooterText", "么FOOTER", loadedInterchange.EI_FooterText);
		}

		public void TestSaveToDiskShouldNotCreateAdditionalDataReader()
		{
			var reallyBigString = new string('*', 5000000);
			Factory.RefreshEnabled = false;

			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.XMS;
			interchange.EI_BodyData = ZBlob.FromUTF8(reallyBigString);

			Factory.Save();

			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			var reloadInterchange = factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.PK, interchange.PK));

			var shouldSaveToDisk = reloadInterchange.SaveToDisk;
			Assert("Should not throw exception", reloadInterchange.SaveToDisk);
			Factory.RefreshEnabled = true;
		}

		public void TestOnSavingEventIsCalled()
		{
			var mock = Factory.NewMoq<EDIInterchange>();
			mock.Protected()
				.Setup<ZString>("GetInterchangeNumber")
				.Returns("1234");
			var interchange = mock.Object;
			var interchange_SavingCalled = 0;
			interchange.Saving += new SavingEventHandler<EDIInterchange>((e) => interchange_SavingCalled++);
			Factory.Save();
			AssertEquals(1, interchange_SavingCalled);

			Factory.Save();
			AssertEquals(1, interchange_SavingCalled);

			interchange.EI_BodyText = "H";
			Factory.Save();
			AssertEquals(2, interchange_SavingCalled);
			mock.VerifyAll();
		}

		public void TestRemoveAIMFromApplicationCodes()
		{
			var applicationCodesType = typeof(EDIInterchange).GetNestedType("ApplicationCodes", BindingFlags.Public | BindingFlags.Static);
			var aimField = applicationCodesType.GetField("AIM", BindingFlags.Public | BindingFlags.Static);
			AssertEquals(null, aimField);
		}

		public void TestLosslessNumberFountain_InterchangeNumber_Released_On_Save_Failure()
		{
			var factory1 = new BusinessObjectFactory();
			var interchangeMock1 = factory1.NewMoq<EDIInterchange>();
			var interchange1 = FillInterchange(interchangeMock1.Object, "AAA");

			interchangeMock1.Setup(c => c.OnSaved(false)).Throws(new InvalidOperationException());

			AssertExceptionThrown<ZSaveException>(factory1.Save);
			ErrorReporter.Clear();

			var factory2 = new BusinessObjectFactory();
			var interchangeMock2 = factory2.NewMoq<EDIInterchange>();
			var interchange2 = FillInterchange(interchangeMock2.Object);

			interchangeMock2.Setup(c => c.OnSaved(false)).Throws(new InvalidOperationException());

			factory2.Save();
			AssertEquals("The previously failed interchange number should be released and a new number should be correctly assigned", "2", interchange2.EI_InterchangeNum);

			interchange1.EI_TransportType = "";
			factory1.Save();
			AssertEquals("The interchange number should continue to increment, indicating that it is not reused", "3", interchange1.EI_InterchangeNum);
		}

		public void TestLossyNumberFountain_InterchangeNumber_Released_On_Save_Failure()
		{
			var factory1 = new BusinessObjectFactory();
			var interchangeMock1 = factory1.NewMoq<EDIInterchange>();
			var interchange1 = FillInterchange(interchangeMock1.Object, "AAA");

			var interchangeNumber = 0;

			interchangeMock1.Protected()
				.Setup<ZString>("GetInterchangeNumber")
				.Returns(() => (++interchangeNumber).ToString());

			AssertExceptionThrown<ZSaveException>(factory1.Save);
			ErrorReporter.Clear();

			var factory2 = new BusinessObjectFactory();
			var interchangeMock2 = factory2.NewMoq<EDIInterchange>();
			var interchange2 = FillInterchange(interchangeMock2.Object);
			interchangeMock2.Protected()
				.Setup<ZString>("GetInterchangeNumber")
				.Returns(() => (++interchangeNumber).ToString());

			factory2.Save();
			AssertEquals("The previously failed interchange number should be released and a new number should be correctly assigned", "2", interchange2.EI_InterchangeNum);

			interchange1.EI_TransportType = "";
			factory1.Save();
			AssertEquals("The interchange number should continue to increment, indicating that it is not reused", "3", interchange1.EI_InterchangeNum);
		}

		public void TestNumberFountain_Save_Succeed_Despite_OnSaved_Failure()
		{
			var factory1 = new BusinessObjectFactory();
			var interchangeMock1 = factory1.NewMoq<EDIInterchange>();
			var interchange1 = FillInterchange(interchangeMock1.Object, "AAA");

			AssertExceptionThrown<ZSaveException>(factory1.Save);
			ErrorReporter.Clear();

			var factory2 = new BusinessObjectFactory();
			var interchangeMock2 = factory2.NewMoq<EDIInterchange>();
			var interchange2 = FillInterchange(interchangeMock2.Object);

			interchangeMock2.Setup(c => c.OnSaved(It.IsAny<bool>())).Throws(new InvalidOperationException());

			AssertExceptionThrown<InvalidOperationException>(factory2.Save);
			AssertEquals("Even though OnSaved fails with an exception, the interchange should be saved and the interchange number should still be incremented as expected in the number fountain mechanism", "2", interchange2.EI_InterchangeNum);

			interchange1.EI_TransportType = "";
			factory1.Save();
			AssertEquals("After handling the OnSaved exception, the interchange number should continue to increment without being affected by the exception, ensuring continuity", "3", interchange1.EI_InterchangeNum);
		}

		public void TestUpdateEDIMessageTransportType()
		{
			var message = Factory.New<TestEdiMessage>();
			Factory.Save();
			Assert("Precheck: Message TransportType is empty", message.EM_TransportType.IsEmpty);

			var interchange = Factory.New<EDIInterchangeForTesting>();
			interchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			interchange.EI_From = "FROM";
			interchange.EI_To = "TO";
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			interchange.ContainedMessages.Add(message);
			Factory.Save();
			AssertEquals("Message TransportType equals Interchange", interchange.EI_TransportType, message.EM_TransportType);
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;
			Factory.Save();
			AssertEquals("Message TransportType equals Interchange", interchange.EI_TransportType, message.EM_TransportType);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var myFactory = new BusinessObjectFactory();
			Interchange = myFactory.New<EDIInterchange>();
		}

		protected EDIInterchange Interchange;

		EDIInterchange FillInterchange(EDIInterchange interchange, string transportType = "")
		{
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.XMS;
			interchange.EI_From = "EDIEDIDAT";
			interchange.EI_To = "EDIEDIDAT";
			interchange.EI_HeaderText = "你HEADER";
			interchange.EI_BodyText = "好BODY";
			interchange.EI_FooterText = "么FOOTER";
			interchange.EI_TransportType = transportType;
			return interchange;
		}

		#endregion

	}
}
