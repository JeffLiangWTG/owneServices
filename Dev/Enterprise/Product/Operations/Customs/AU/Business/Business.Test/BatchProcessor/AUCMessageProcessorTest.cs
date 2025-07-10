using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using DisposableEnvironment = Enterprise.Environment.DisposableEnvironment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUCMessageProcessorTest : DeclarationsAndShipmentsCreatedCancelledTestCase
	{
		public void TestProcessCMRMessageFromOtherBranch()
		{
			var testCompany = Factory.New<GlbCompany>();
			testCompany.GC_Code = "TC1";
			var melbourneBranch = Factory.New<GlbBranch>();
			melbourneBranch.GB_GC = testCompany.PK;
			melbourneBranch.GB_Code = "TB1";
			melbourneBranch.GB_RL_NKHomePort = "AUMEL";
			var perthBranch = Factory.New<GlbBranch>();
			perthBranch.GB_GC = testCompany.PK;
			perthBranch.GB_Code = "TB2";
			perthBranch.GB_RL_NKHomePort = "AUPER";
			Factory.Save();

			using (DisposableEnvironment.ForBranch(melbourneBranch.PK.ToGuid()))
			{
				var mawb = Factory.New<CusMAWB>();
				mawb.CM_MessageReference = "08156997662";
				var hawb = mawb.ChildBills.AddNew();
				hawb.CS_MessageReference = "S00002158";
				hawb.CS_IsResponsePending = true;
				hawb.CS_IsPrealerted = false;

				var outgoingMessage = (CMRAIRCRMessage)hawb.Messages.AddNew(typeof(CMRAIRCRMessage));
				outgoingMessage.EM_MessageSubType = CMRMessage.MessageSubTypes.Original;
				outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				outgoingMessage.EM_Status = EDIMessage.Status.Sent;
				outgoingMessage.EM_MessageNum = "1";
				outgoingMessage.EM_MessageText = OriginalAIRCRMessage;
				Factory.Save();

				AssertEquals("Messages", 1, hawb.Messages.Count);
				AssertNull("StatusChange not logged", hawb.Logs.MostRecentLogByEventTime(ZArchitecture.Business.Events.StatusChange));

				EDIMessage processedMessage = null;
				var melbourneBranchTime = ZDateTime.Now;
				using (DisposableEnvironment.ForBranch(perthBranch.PK.ToGuid()))
				{
					AssertEquals("MAWB branch is Melbourne", "AUMEL", hawb.MAWB.Branch.HomePort.Code);
					AssertGreaterThan("No longer in East Coast timezone", melbourneBranchTime, ZDateTime.Now);

					var incomingMessage = Factory.New<CMRAIRCRRMessage>();
					incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
					incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					incomingMessage.EM_Status = EDIMessage.Status.Queued;
					incomingMessage.EM_MessageText = ClearAIRCRRMessage;
					Factory.Save();

					var processor = new AUCMessageProcessor();
					processor.ExecuteBatch();

					processedMessage = new BusinessObjectFactory().Load<EDIMessage>(incomingMessage.PK);
				}

				AssertEquals("EM_Status", EDIMessage.Status.Received, processedMessage.EM_Status);
				AssertEquals("EM_MessageSubType", CMRMessage.ManifestResponseSubTypes.Clear, processedMessage.EM_MessageSubType);

				var log = hawb.Logs.MostRecentLogByEventTime(ZArchitecture.Business.Events.StatusChange);
				AssertNotNull("StatusChange logged", log);
				AssertEquals("CusHAWB Message Status - ACO", log.SL_Reference);
				AssertGreaterThan("log should have MAWB branch timestamp even though logged in Perth timezone", log.SL_EventTime, melbourneBranchTime);
			}
		}

		public void TestMessageSetToFailedIfExceptionWhilstProcessing()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			ExceptionThrowingBatchAUCMessageProcessor processor = new ExceptionThrowingBatchAUCMessageProcessor();
			processor.ExecuteBatch();
			AssertEquals("LastDevError", typeof(DivideByZeroException), ErrorReporter.LastExceptionReported.GetType());
			ErrorReporter.Clear();
			message.Reload();
			AssertEquals("Status", EDIMessage.Status.Failed, message.EM_Status);
		}

		[ExpectNoExceptions]
		public void TestDontProcessMessageOwnedByADifferentCompany()
		{
			var query = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_GB = Factory.LoadTop1<GlbBranch>(query).PK;
			Factory.Save();

			ExceptionThrowingBatchAUCMessageProcessor processor = new ExceptionThrowingBatchAUCMessageProcessor();
			processor.ExecuteBatch();
		}

		public void TestNextMessageProcessedIfMessageBreaksFactory()
		{
			var message1 = Factory.New<EDIMessage>();
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message1.EM_Status = EDIMessage.Status.Queued;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var message2 = Factory.New<EDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message2.EM_Status = EDIMessage.Status.Queued;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			BadRecordInsertingAUCMessageProcessor processor = new();
			processor.MessagePKsToBarfOn.Add(message1.PK);
			processor.ExecuteBatch();
#if NETFRAMEWORK
			var expectedMessage = $"sender cannot be null or empty.{System.Environment.NewLine}Parameter name: sender";
#else
			var expectedMessage = "sender cannot be null or empty. (Parameter 'sender')";
#endif
			AssertEquals("LastDevError", expectedMessage, ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
			message1.Reload();
			AssertEquals("Status", EDIMessage.Status.Failed, message1.EM_Status);
			message2.Reload();
			AssertEquals("Status", EDIMessage.Status.Received, message2.EM_Status);
			processor.ExecuteBatch();
		}

		public void TestOrderAndHint()
		{
			var processor = new AUCMessageProcessorForTest();
			var query = processor.GetMessageProcessorQueryExposed();
			var hint = query.TableIndexHints.Single();

			AssertEquals("EM_SystemCreateTimeUtc, EM_MessageNum", query.OrderBy);
			AssertEquals("NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_SystemCreateTimeUtc_EM_MessageNum", hint.IndexName);
		}

		public void TestGetMessageProcessors()
		{
			var messageProcessors = new AUCMessageProcessorForTest().GetMessageProcessorsExposed();

			Assert("CMRAllMessageProcessor", messageProcessors.Any(x => x is CMRAllMessageProcessor));
			Assert("MessageProcessorFactory", messageProcessors.Any(x => x is MessageProcessorFactory));
			Assert("EXDOCApplicationTypeMessageProcessor", messageProcessors.Any(x => x is EXDOCApplicationTypeMessageProcessor));
			Assert("NEXDOCApplicationTypeMessageProcessor", messageProcessors.Any(x => x is NEXDOCApplicationTypeMessageProcessor));
			Assert("COLSApplicationTypeMessageProcessor", messageProcessors.Any(x => x is COLSApplicationTypeMessageProcessor));
			AssertEquals("Processor Count", 5, messageProcessors.Count);
		}

		const string OriginalAIRCRMessage = @"UNH+1+CUSCAR:D:99B:UN'BGM+933:::AIRCR+S00002158/1:1+9'RFF+HWB:1'RFF+MWB:08156997662'NAD+CN+++MY TEST CONSIGNOR+CONSIGOR ADRESS+SYDNEY++2000+AU'NAD+CZ+++MY TEST CONSIGNEE+CONSIGEE ADRESS+AUKLAND+++NZ'NAD+VW+51001191402::95'TDT+20+569++6+QF::3'LOC+8+AUSYD::6'LOC+76+NZAKL::6'LOC+12+AUSYD::6'LOC+91+NZAKL::6'DTM+178:20041210:102'CNI+1'RFF+UCN:S00002158'MOA+96:NDV'GID+1'PAC+10'FTX+AAA+++STUFF'MEA+AAE+G+KG:100.00'UNT+21+1'";
		const string ClearAIRCRRMessage = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIRCRR+490F DAGH CDGE:001+11'NAD+MR+AAA374M:110:95'RFF+ACW:AIRCR'RFF+AFM:9'RFF+ABO:S00002158/1::001'DTM+310:20041216010409:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5203:6:95'FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'CNT+55:000'UNT+13+000001'";

		sealed class AUCMessageProcessorForTest : AUCMessageProcessor
		{
			public List<ApplicationTypeMessageProcessor> GetMessageProcessorsExposed() => GetMessageProcessors();

			public ZQuery GetMessageProcessorQueryExposed() => GetMessageProcessorQuery();
		}

		sealed class ExceptionThrowingBatchAUCMessageProcessor : AUCMessageProcessor
		{
			protected override bool ShouldRetryOnExceptionCore(int currentExceptionsCount, EDIMessage message, Exception lastException, int retryAttempts, string additionalErrorReportMessage = null) => false;

			protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
			{
				return new List<ApplicationTypeMessageProcessor>() { new ExceptionThrowingMessageProcessor() };
			}

			sealed class ExceptionThrowingMessageProcessor : ApplicationTypeMessageProcessor
			{
				public ExceptionThrowingMessageProcessor()
					: base(new LoggingInformation())
				{
				}

				protected override string ApplicationCodeCore => "CMR";

				protected override string MessageFriendlyNameCore => "";

				protected override void ProcessMessageCore(EDIMessage message)
				{
					int x = 0;
					int i = x / x;
				}
			}
		}

		sealed class BadRecordInsertingAUCMessageProcessor : AUCMessageProcessor
		{
			protected override bool ShouldRetryOnExceptionCore(int currentExceptionsCount, EDIMessage message, Exception lastException, int retryAttempts, string additionalErrorReportMessage = null) => false;

			protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
			{
				return new List<ApplicationTypeMessageProcessor>() { new BadRecordInsertingMessageProcessor(this) };
			}

			sealed class BadRecordInsertingMessageProcessor : ApplicationTypeMessageProcessor
			{
				protected override string ApplicationCodeCore => "CMR";

				protected override string MessageFriendlyNameCore => "test";

				public BadRecordInsertingMessageProcessor(BadRecordInsertingAUCMessageProcessor parent)
					: base(new LoggingInformation())
				{
					this.parent = parent;
				}

				readonly BadRecordInsertingAUCMessageProcessor parent;

				protected override void ProcessMessageCore(EDIMessage message)
				{
					if (parent.MessagePKsToBarfOn.Contains(message.PK))
					{
						//insert ediinterchange without populating the essential fields EI_To and EI_From
						EDIInterchange interchange1 = message.Factory.New<EDIInterchange>();
					}
					message.EM_Status = EDIMessage.Status.Received;
				}
			}

			public ArrayList MessagePKsToBarfOn = new ArrayList();
		}
	}
}
