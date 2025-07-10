using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.Testing
{
	public class BranchMessageProcessorTest : BaseMessageProcessorTest
	{
		#region TestUniversalDataMessageProcessingBusinessFailureExceptionRetry

		protected override string ExpectedLogTextForFailureExceptionRetry => @"Pre-Process Message #00001
Saving...
1 message pre-processed
Processing Message #00001
Exception processing message #00001 individually: \[Save Exception Thrown during ProcessMessage\(EDIMessage message\)\]
.
Processing Message #00001
Exception processing message #00001 individually: \[Save Exception Thrown during ProcessMessage\(EDIMessage message\)\]
.
Processing Message #00001
Exception processing message #00001 individually: \[Save Exception Thrown during ProcessMessage\(EDIMessage message\)\]
Error Processing Incoming EDI Message: UDM--00001
Exception occurred 3 times whilst processing a message individually. The message's status has been set to 'Failed'.
Enterprise.Messaging.Integration.MessageProcessingBusinessFailureException: Save Exception Thrown during ProcessMessage\(EDIMessage message\)
\s+at Enterprise.Messaging.Business.Testing.+
\s+at Enterprise.Messaging.Business.+";

		#endregion

		#region TestUniversalDataMessageProcessingBusinessFailureExceptionNotRetry

		protected override string ExpectedLogTextForFailureExceptionNotRetry => @"Pre-Process Message #00001
Saving...
1 message pre-processed
Processing Message #00001
Exception processing message #00001 individually: \[Cannot Save Exception Thrown during ProcessMessage\(EDIMessage message\)\]
Error Processing Incoming EDI Message: UDM--00001
Exception occurred 1 times whilst processing a message individually. The message's status has been set to 'Failed'.
Enterprise.Messaging.Integration.MessageProcessingBusinessFailureException: Cannot Save Exception Thrown during ProcessMessage\(EDIMessage message\)
\s+at Enterprise.Messaging.Business.Testing.+
\s+at Enterprise.Messaging.Business.+";

		#endregion

		#region TestExecuteProcesses101MessagesWithoutAnyProblemsWith50RowsPerFactorySave

		protected override int ExpectedFactorySaveCountFor101Messages => 6;

		protected override string ExpectedLogTextFor101Messages => @"
Pre-Process Message #00001
Pre-Process Message #00002
Pre-Process Message #00003
Pre-Process Message #00004
Pre-Process Message #00005
Pre-Process Message #00006
Pre-Process Message #00007
Pre-Process Message #00008
Pre-Process Message #00009
Pre-Process Message #00010
Pre-Process Message #00011
Pre-Process Message #00012
Pre-Process Message #00013
Pre-Process Message #00014
Pre-Process Message #00015
Pre-Process Message #00016
Pre-Process Message #00017
Pre-Process Message #00018
Pre-Process Message #00019
Pre-Process Message #00020
Pre-Process Message #00021
Pre-Process Message #00022
Pre-Process Message #00023
Pre-Process Message #00024
Pre-Process Message #00025
Pre-Process Message #00026
Pre-Process Message #00027
Pre-Process Message #00028
Pre-Process Message #00029
Pre-Process Message #00030
Pre-Process Message #00031
Pre-Process Message #00032
Pre-Process Message #00033
Pre-Process Message #00034
Pre-Process Message #00035
Pre-Process Message #00036
Pre-Process Message #00037
Pre-Process Message #00038
Pre-Process Message #00039
Pre-Process Message #00040
Pre-Process Message #00041
Pre-Process Message #00042
Pre-Process Message #00043
Pre-Process Message #00044
Pre-Process Message #00045
Pre-Process Message #00046
Pre-Process Message #00047
Pre-Process Message #00048
Pre-Process Message #00049
Pre-Process Message #00050
Saving...
50 messages pre-processed
Processing Message #00001
Processing Message #00002
Processing Message #00003
Processing Message #00004
Processing Message #00005
Processing Message #00006
Processing Message #00007
Processing Message #00008
Processing Message #00009
Processing Message #00010
Processing Message #00011
Processing Message #00012
Processing Message #00013
Processing Message #00014
Processing Message #00015
Processing Message #00016
Processing Message #00017
Processing Message #00018
Processing Message #00019
Processing Message #00020
Processing Message #00021
Processing Message #00022
Processing Message #00023
Processing Message #00024
Processing Message #00025
Processing Message #00026
Processing Message #00027
Processing Message #00028
Processing Message #00029
Processing Message #00030
Processing Message #00031
Processing Message #00032
Processing Message #00033
Processing Message #00034
Processing Message #00035
Processing Message #00036
Processing Message #00037
Processing Message #00038
Processing Message #00039
Processing Message #00040
Processing Message #00041
Processing Message #00042
Processing Message #00043
Processing Message #00044
Processing Message #00045
Processing Message #00046
Processing Message #00047
Processing Message #00048
Processing Message #00049
Processing Message #00050
Saving...
50 messages processed
Pre-Process Message #00051
Pre-Process Message #00052
Pre-Process Message #00053
Pre-Process Message #00054
Pre-Process Message #00055
Pre-Process Message #00056
Pre-Process Message #00057
Pre-Process Message #00058
Pre-Process Message #00059
Pre-Process Message #00060
Pre-Process Message #00061
Pre-Process Message #00062
Pre-Process Message #00063
Pre-Process Message #00064
Pre-Process Message #00065
Pre-Process Message #00066
Pre-Process Message #00067
Pre-Process Message #00068
Pre-Process Message #00069
Pre-Process Message #00070
Pre-Process Message #00071
Pre-Process Message #00072
Pre-Process Message #00073
Pre-Process Message #00074
Pre-Process Message #00075
Pre-Process Message #00076
Pre-Process Message #00077
Pre-Process Message #00078
Pre-Process Message #00079
Pre-Process Message #00080
Pre-Process Message #00081
Pre-Process Message #00082
Pre-Process Message #00083
Pre-Process Message #00084
Pre-Process Message #00085
Pre-Process Message #00086
Pre-Process Message #00087
Pre-Process Message #00088
Pre-Process Message #00089
Pre-Process Message #00090
Pre-Process Message #00091
Pre-Process Message #00092
Pre-Process Message #00093
Pre-Process Message #00094
Pre-Process Message #00095
Pre-Process Message #00096
Pre-Process Message #00097
Pre-Process Message #00098
Pre-Process Message #00099
Pre-Process Message #00100
Saving...
50 messages pre-processed
Processing Message #00051
Processing Message #00052
Processing Message #00053
Processing Message #00054
Processing Message #00055
Processing Message #00056
Processing Message #00057
Processing Message #00058
Processing Message #00059
Processing Message #00060
Processing Message #00061
Processing Message #00062
Processing Message #00063
Processing Message #00064
Processing Message #00065
Processing Message #00066
Processing Message #00067
Processing Message #00068
Processing Message #00069
Processing Message #00070
Processing Message #00071
Processing Message #00072
Processing Message #00073
Processing Message #00074
Processing Message #00075
Processing Message #00076
Processing Message #00077
Processing Message #00078
Processing Message #00079
Processing Message #00080
Processing Message #00081
Processing Message #00082
Processing Message #00083
Processing Message #00084
Processing Message #00085
Processing Message #00086
Processing Message #00087
Processing Message #00088
Processing Message #00089
Processing Message #00090
Processing Message #00091
Processing Message #00092
Processing Message #00093
Processing Message #00094
Processing Message #00095
Processing Message #00096
Processing Message #00097
Processing Message #00098
Processing Message #00099
Processing Message #00100
Saving...
50 messages processed
Pre-Process Message #00101
Saving...
1 message pre-processed
Processing Message #00101
Saving...
1 message processed
";

		#endregion

		#region TestExecuteProcessesReturnsToBatchesAfterProcessOneBatchIndividually

		protected override int ExpectedFactorySaveCountForReturnsToBatches => 56;

		#endregion

		#region TestFirstExceptionIsLoggedButDoesntCauseTheMessageToBeRejected

		protected override string ExpectedMessageStatusFirstExceptionLogged => EDIMessage.Status.PreProcessedOK;

		protected override string ExpectedLogTextFirstExceptionLogged => @"
Pre-Process Message #00001
Pre-Process Message #00002
Pre-Process Message #00003
Pre-Process Message #00004
Pre-Process Message #00005
Saving...
5 messages pre-processed
Processing Message #00001
Exception processing message #00001 individually: [Exception thrown during ProcessMessage(EDIMessage message)]
Processing Message #00002
Exception processing message #00002 individually: [Exception thrown during ProcessMessage(EDIMessage message)]
Processing Message #00003
Exception processing message #00003 individually: [Exception thrown during ProcessMessage(EDIMessage message)]
Processing Message #00004
Exception processing message #00004 individually: [Exception thrown during ProcessMessage(EDIMessage message)]
Processing Message #00005
Exception processing message #00005 individually: [Exception thrown during ProcessMessage(EDIMessage message)]
";

		#endregion

		#region TestRepeatedExceptionsOnAMessageDuringExecuteCauseFailure

		protected override string ExpectedLogTextForRepeatedExceptions(int retryAttempts) => @$"Pre-Process Message #00001
Saving...
1 message pre-processed
Processing Message #00001
Exception processing message #00001 individually: \[Exception Thrown during ProcessMessage\(EDIMessage message\)\]{string.Concat(Enumerable.Repeat(@"
.
Processing Message #00001
Exception processing message #00001 individually: \[Exception Thrown during ProcessMessage\(EDIMessage message\)\]", retryAttempts - 1))}
Error Processing Incoming EDI Message: TST--00001
Exception occurred {retryAttempts} times whilst processing a message individually. The message's status has been set to 'Failed'.
System.Exception: Exception Thrown during ProcessMessage\(EDIMessage message\)
\s+at Enterprise.Messaging.Business.Testing.+
\s+at Enterprise.Messaging.Business.+
.";

		#endregion

		#region TestMessagesWithoutExceptionsWillStillGetProcessed

		protected override string ExpectedStatusForMessagesWithoutExceptions => EDIMessage.Status.PreProcessedOK;
		protected override int ExpectedSaveCountForMessagesWithoutExceptions => 6;

#if NETFRAMEWORK
		protected override string ExpectedLogTextForMessagesWithoutExceptions => @"
Pre-Process Message #1
Pre-Process Message #2
Pre-Process Message #3
Saving...
3 messages pre-processed
Processing Message #1
Processing Message #2
Processing Message #3
Saving...
Exception processing a group of 3 messages: [sender cannot be null or empty.
Parameter name: sender]
Processing Message #1
Saving...
1 message processed
Processing Message #2
Saving...
Exception processing message #2 individually: [sender cannot be null or empty.
Parameter name: sender]
Processing Message #3
Saving...
1 message processed
";
#else
		protected override string ExpectedLogTextForMessagesWithoutExceptions => @"
Pre-Process Message #1
Pre-Process Message #2
Pre-Process Message #3
Saving...
3 messages pre-processed
Processing Message #1
Processing Message #2
Processing Message #3
Saving...
Exception processing a group of 3 messages: [sender cannot be null or empty. (Parameter 'sender')]
Processing Message #1
Saving...
1 message processed
Processing Message #2
Saving...
Exception processing message #2 individually: [sender cannot be null or empty. (Parameter 'sender')]
Processing Message #3
Saving...
1 message processed
";
#endif
		public void TestMessagesWithoutExceptionsWillStillGetProcessed_AcrossBranches()
		{
			var alternativeBranch = GlbCompany.CurrentCompany.Branches.FirstOrDefault(b => b.PK != GlbBranch.CurrentBranch.PK);

			EDIMessage message1 = CreateEDIMessage("1");
			EDIMessage message2 = CreateEDIMessage("2");
			EDIMessage message3 = CreateEDIMessage("3");
			message3.EM_GB = alternativeBranch.PK;
			Factory.Save();

			var messageProcessor = new BranchMessageProcessorForTest(new ZGuid[] { message2.PK }, null);
			messageProcessor.Execute();

			message1.Reload();
			message2.Reload();
			message3.Reload();

			AssertEquals("message1.EM_Status", EDIMessage.Status.Received, message1.EM_Status);
			AssertEquals("message2.EM_Status", EDIMessage.Status.PreProcessedOK, message2.EM_Status);
			AssertEquals("message3.EM_Status", EDIMessage.Status.Received, message3.EM_Status);

			AssertEquals("messageProcessor.FactorySaveCount", 6, messageProcessor.FactorySaveCount);
#if NETFRAMEWORK
			string expectedLogText = @"
Pre-Process Message #1
Pre-Process Message #2
Pre-Process Message #3
Saving...
3 messages pre-processed
Processing Message #1
Processing Message #2
Saving...
Exception processing a group of 2 messages: [sender cannot be null or empty.
Parameter name: sender]
Processing Message #1
Saving...
1 message processed
Processing Message #2
Saving...
Exception processing message #2 individually: [sender cannot be null or empty.
Parameter name: sender]
Processing Message #3
Saving...
1 message processed
";
#else
			string expectedLogText = @"
Pre-Process Message #1
Pre-Process Message #2
Pre-Process Message #3
Saving...
3 messages pre-processed
Processing Message #1
Processing Message #2
Saving...
Exception processing a group of 2 messages: [sender cannot be null or empty. (Parameter 'sender')]
Processing Message #1
Saving...
1 message processed
Processing Message #2
Saving...
Exception processing message #2 individually: [sender cannot be null or empty. (Parameter 'sender')]
Processing Message #3
Saving...
1 message processed
";
#endif
			AssertLogs(messageProcessor.Logger, expectedLogText);
		}

		public void TestMessagesWithoutExceptionsWillStillGetPreProcessed()
		{
			EDIMessage message1 = CreateEDIMessage("1");
			EDIMessage message2 = CreateEDIMessage("2");
			EDIMessage message3 = CreateEDIMessage("3");
			Factory.Save();

			var messageProcessor = new BranchMessageProcessorForTest(null, new ZGuid[] { message2.PK });
			messageProcessor.Execute();

			message1.Reload();
			message2.Reload();
			message3.Reload();

			AssertEquals("message1.EM_Status", EDIMessage.Status.Received, message1.EM_Status);
			AssertEquals("message2.EM_Status", EDIMessage.Status.Queued, message2.EM_Status);
			AssertEquals("message3.EM_Status", EDIMessage.Status.Received, message3.EM_Status);

			AssertEquals("messageProcessor.FactorySaveCount", 6, messageProcessor.FactorySaveCount);
#if NETFRAMEWORK
			string expectedLogText = @"
Pre-Process Message #1
Pre-Process Message #2
Pre-Process Message #3
Saving...
Exception processing a group of 3 messages: [sender cannot be null or empty.
Parameter name: sender]
Pre-Process Message #1
Saving...
1 message pre-processed
Pre-Process Message #2
Saving...
Exception pre-processing message #2 individually: [sender cannot be null or empty.
Parameter name: sender]
Pre-Process Message #3
Saving...
1 message pre-processed
Processing Message #1
Processing Message #3
Saving...
2 messages processed
";
#else
			string expectedLogText = @"
Pre-Process Message #1
Pre-Process Message #2
Pre-Process Message #3
Saving...
Exception processing a group of 3 messages: [sender cannot be null or empty. (Parameter 'sender')]
Pre-Process Message #1
Saving...
1 message pre-processed
Pre-Process Message #2
Saving...
Exception pre-processing message #2 individually: [sender cannot be null or empty. (Parameter 'sender')]
Pre-Process Message #3
Saving...
1 message pre-processed
Processing Message #1
Processing Message #3
Saving...
2 messages processed
";
#endif
			AssertLogs(messageProcessor.Logger, expectedLogText);
		}

		public void TestMessagesWithoutExceptionsWillStillGetPreProcessed_AcrossBranches()
		{
			var alternativeBranch = GlbCompany.CurrentCompany.Branches.FirstOrDefault(b => b.PK != GlbBranch.CurrentBranch.PK);

			EDIMessage message1 = CreateEDIMessage("1");
			EDIMessage message2 = CreateEDIMessage("2");
			EDIMessage message3 = CreateEDIMessage("3");
			message3.EM_GB = alternativeBranch.PK;
			Factory.Save();

			var messageProcessor = new BranchMessageProcessorForTest(null, new ZGuid[] { message2.PK });
			messageProcessor.Execute();

			message1.Reload();
			message2.Reload();
			message3.Reload();

			AssertEquals("message1.EM_Status", EDIMessage.Status.Received, message1.EM_Status);
			AssertEquals("message2.EM_Status", EDIMessage.Status.Queued, message2.EM_Status);
			AssertEquals("message3.EM_Status", EDIMessage.Status.Received, message3.EM_Status);

			AssertEquals("messageProcessor.FactorySaveCount", 7, messageProcessor.FactorySaveCount);
#if NETFRAMEWORK
			string expectedLogText = @"
Pre-Process Message #1
Pre-Process Message #2
Pre-Process Message #3
Saving...
Exception processing a group of 3 messages: [sender cannot be null or empty.
Parameter name: sender]
Pre-Process Message #1
Saving...
1 message pre-processed
Pre-Process Message #2
Saving...
Exception pre-processing message #2 individually: [sender cannot be null or empty.
Parameter name: sender]
Pre-Process Message #3
Saving...
1 message pre-processed
Processing Message #1
Saving...
1 message processed
Processing Message #3
Saving...
1 message processed
";
#else
			string expectedLogText = @"
Pre-Process Message #1
Pre-Process Message #2
Pre-Process Message #3
Saving...
Exception processing a group of 3 messages: [sender cannot be null or empty. (Parameter 'sender')]
Pre-Process Message #1
Saving...
1 message pre-processed
Pre-Process Message #2
Saving...
Exception pre-processing message #2 individually: [sender cannot be null or empty. (Parameter 'sender')]
Pre-Process Message #3
Saving...
1 message pre-processed
Processing Message #1
Saving...
1 message processed
Processing Message #3
Saving...
1 message processed
";
#endif
			AssertLogs(messageProcessor.Logger, expectedLogText);
		}

#endregion

		#region TestMessagesWithoutExceptionsWillStillGetProcessedInASeparateFactory

		protected override int ExpectedSaveCountForMessagesWithoutExceptionsInNewFactory => 7;

#if NETFRAMEWORK
		protected override string ExpectedLogTextForMessagesWithoutExceptionsInNewFactory => @"
Pre-Process Message #1
Saving...
1 message pre-processed
Pre-Process Message #2
Saving...
1 message pre-processed
Pre-Process Message #3
Saving...
1 message pre-processed
Processing Message #1
Saving...
1 message processed
Processing Message #2
Saving...
Exception processing message #2 individually: [sender cannot be null or empty.
Parameter name: sender]
Processing Message #3
Saving...
1 message processed
";
#else
		protected override string ExpectedLogTextForMessagesWithoutExceptionsInNewFactory => @"
Pre-Process Message #1
Saving...
1 message pre-processed
Pre-Process Message #2
Saving...
1 message pre-processed
Pre-Process Message #3
Saving...
1 message pre-processed
Processing Message #1
Saving...
1 message processed
Processing Message #2
Saving...
Exception processing message #2 individually: [sender cannot be null or empty. (Parameter 'sender')]
Processing Message #3
Saving...
1 message processed
";

#endif

		[TestDate(2024, 01, 23)]
		public void TestPreProcessMessages_MessageShouldBeProcessedInASeparateFactory()
		{
			var message1 = CreateEDIMessage("1");
			message1.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			var previousBranchPK = message1.EM_GB;
			var messageProcessor = new BranchMessageProcessorForTestPreProcessInNewFactory();
			messageProcessor.Execute();

			var messageExposed = messageProcessor.MessageExposed;
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.Error, messageExposed.EM_Status);
				AssertNotEquals("EM_GB", previousBranchPK, messageExposed.EM_GB);
				AssertEquals("EM_HeldUntilDate", new ZDateTime(2024, 01, 25), messageExposed.EM_HeldUntilDate);
			});
		}

#endregion

		#region TestExcludeBranchFilter
		public void TestExcludeBranchFilter()
		{
			var bmp = GetNewBranchMessageProcessorForFilterTest();

			AssertEquals(ExpectedExcludeBranchFilter, bmp.ExcludeBranchFilter_Exposed);
			var emptyQuery = new ZQuery();

			if (bmp.ExcludeBranchFilter_Exposed)
			{
				AssertEquals(emptyQuery, bmp.ValidBranchesForMessageFilter_Exposed);
			}
			else
			{
				AssertNotEquals(emptyQuery, bmp.ValidBranchesForMessageFilter_Exposed);
			}
		}
		protected virtual bool ExpectedExcludeBranchFilter => false;
		protected virtual BranchMessageProcessor GetNewBranchMessageProcessorForFilterTest() => new BranchMessageProcessorForTest();
		#endregion

		#region TestPerformance
#if PERFORMANCE_TESTING

		public void TestPerformance()
		{
			ExecuteAndAssertPerformanceIsWithinRange(() => base.GetNewMessageProcessorForTest(),
																() => GetNewMessageProcessorForTest());
		}

		public void TestPerformanceSwitchingBranches()
		{
			ExecuteAndAssertPerformanceIsWithinRange(() => base.GetNewMessageProcessorForTest(),
																() => new BranchMessageProcessorForTest(null, null, true));
		}

		public void TestPerformanceFailedMessage()
		{
			ExecuteAndAssertPerformanceIsWithinRange(() =>
			{
				var msg53_base = FindMessage("00053");
				return base.GetNewMessageProcessorForTest(new ZGuid[] { msg53_base.PK });
			}, () =>
			{
				var msg53_branch = FindMessage("00053");
				return GetNewMessageProcessorForTest(new ZGuid[] { msg53_branch.PK });
			});
		}

		public void TestPerformanceProcessInSeparateFactory()
		{
			ExecuteAndAssertPerformanceIsWithinRange(() => base.GetMessageProcessorForTestProcessInNewFactory(),
																() => GetMessageProcessorForTestProcessInNewFactory());
		}

		public void TestPerformanceLoadingLinkedObjectFromMessage()
		{
			try
			{
				HasLinkedObject = true;
				var bizos = CreateAndSaveBizos(MessageCount);

				ExecuteAndAssertPerformanceIsWithinRange(() => base.GetNewMessageProcessorForTest(),
														 () => GetNewMessageProcessorForTest());
			}
			finally
			{
				HasLinkedObject = false;
			}
		}

		const int Cycles = 10;
		const int MessageCount = 1000;
		bool HasLinkedObject = false;

		EDIMessage FindMessage(ZString messageNumber)
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, "TST");
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
			query.AddToFilter(EDIMessageSchema.EM_MessageNum, messageNumber);
			return Factory.LoadTop1<EDIMessage>(query);
		}

		void ExecuteAndAssertPerformanceIsWithinRange(Func<IMessageProcessorForTest> getBaseProc, Func<IMessageProcessorForTest> getBranchProc)
		{
			var measurements = new System.Text.StringBuilder();
			var cr = "<br/>";
			string baseLog = null;
			string branchLog = null;

			for (int i = 0; i < Cycles; i++)
			{
				var baseProc = getBaseProc();
				var durationBase = MeasureDurationForProcessor(baseProc);
				var branchProc = getBranchProc();
				var durationBranch = MeasureDurationForProcessor(branchProc);
				var diffMs = durationBranch.TotalMilliseconds - durationBase.TotalMilliseconds;
				var increaseAsPercentage = (diffMs / durationBase.TotalMilliseconds) * 100.0;
				var diffPerMessage = diffMs / (double)MessageCount;

				measurements.Append(cr + "base: " + durationBase);
				measurements.AppendFormat("{0}branch: {1} &nbsp; &nbsp; increase: {2:0}% &nbsp; &nbsp; difference per message {3:0.00}ms", cr, durationBranch, increaseAsPercentage, diffPerMessage);

				if (baseLog == null)
				{
					baseLog = string.Join(System.Environment.NewLine, baseProc.Logger.DebugLogStrings.Cast<string>());
				}
				if (branchLog == null)
				{
					branchLog = string.Join(System.Environment.NewLine, branchProc.Logger.DebugLogStrings.Cast<string>());
				}

				//baseLog = string.Join(System.Environment.NewLine, baseProc.Logger.DebugLogStrings.Cast<string>());
				//branchLog = string.Join(System.Environment.NewLine, branchProc.Logger.DebugLogStrings.Cast<string>());
			}

			var message = ZString.Format("Performance Statistics. {0}{1}{0}{2}{0}{0}{3}{0}", cr, measurements.ToString(), HtmlFormatGoodValue(baseLog, x => x), HtmlFormatBadValue(branchLog, x => x));
			AssertLessThan(message, TimeSpan.MaxValue, TimeSpan.MinValue);
		}

		TimeSpan MeasureDurationForProcessor(IMessageProcessorForTest processor)
		{
			var messages = CreateAndSaveMessages(MessageCount);

			var stopwatch = Stopwatch.StartNew();
			processor.Execute();
			stopwatch.Stop();
			return stopwatch.Elapsed;
		}

		List<EDIMessage> CreateAndSaveMessages(int quantity)
		{
			var messages = new List<EDIMessage>(quantity);
			for (int i = 0; i < quantity; i++)
			{
				var message = CreateEDIMessageForCusDec(NumberFromIndex(i, quantity));
				messages.Add(message);
			}
			Factory.Save();
			return messages;
		}

		protected EDIMessage CreateEDIMessageForCusDec(string messageNumber)
		{
			EDIMessage message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageNum = messageNumber;
			message.EM_ApplicationCode = "TST";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_IsActive = true;

			if (HasLinkedObject)
			{
				message.EM_MessageText = TestConstants.HasLinkedObject;
			}

			return message;
		}

		List<DummyBusinessObject> CreateAndSaveBizos(int quantity)
		{
			var bizos = new List<DummyBusinessObject>(quantity);
			for (int i = 0; i < quantity; i++)
			{
				var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();
				bizo.Z0_Code = NumberFromIndex(i, quantity);
				bizos.Add(bizo);
			}
			Factory.Save();
			return bizos;
		}

		string NumberFromIndex(int index, int quantity) => (quantity - index).ToString().PadLeft(5, '0');

#endif
#endregion

		#region Implementation

		protected override IMessageProcessorForTest GetNewMessageProcessorForTest(ZGuid[] messagePKsToFailOn = null)
		{
			return new BranchMessageProcessorForTest(messagePKsToFailOn, null);
		}

		protected override IMessageProcessorForTest GetNewMessageProcessorForTestThrowingExceptionInProcessMessage(Exception exceptionToThrow, int maxRetries = 0)
		{
			return new BranchMessageProcessorForTestThrowingExceptionInProcessMessage(exceptionToThrow, maxRetries);
		}

		protected override IMessageProcessorForTest GetMessageProcessorForTestExceptionOnSave()
		{
			return new BranchMessageProcessorForTestExceptionOnSave();
		}

		protected override IMessageProcessorForTest GetMessageProcessorForTestProcessInNewFactory(ZGuid[] messagePKsToFailOn = null)
		{
			return new BranchMessageProcessorForTestProcessInNewFactory(messagePKsToFailOn);
		}

		class BranchMessageProcessorForTestThrowingExceptionInProcessMessage : BranchMessageProcessorForTest
		{
			public BranchMessageProcessorForTestThrowingExceptionInProcessMessage(Exception exceptionToThrow, int maxRetries = 0)
			{
				this.exceptionToThrow = exceptionToThrow;
				this.maxRetries = maxRetries;
			}

			readonly Exception exceptionToThrow;
			readonly int maxRetries;

			protected override void ProcessMessageCore(ApplicationTypeMessageProcessor processor, EDIMessage message)
			{
				Logger.Log("Processing Message #" + message.EM_MessageNum);
				throw exceptionToThrow;
			}

			protected override bool ShouldRetryOnExceptionCore(int currentExceptionsCount, EDIMessage message, Exception lastException, int retryAttempts, string additionalErrorReportMessage = null)
			{
				return maxRetries > 0
					? currentExceptionsCount < maxRetries
					: base.ShouldRetryOnExceptionCore(currentExceptionsCount, message, lastException, retryAttempts, additionalErrorReportMessage);
			}
		}

		class BranchMessageProcessorForTestExceptionOnSave : BranchMessageProcessorForTest
		{
			public BranchMessageProcessorForTestExceptionOnSave()
				: base()
			{
			}

			public const string ErrorMessage = "Some error happened during saving";
			public int SaveCount { get; set; }

			protected override void SaveAfterProcessingMessagesCore(BusinessObjectFactory factory)
			{
				SaveCount++;
				if (SaveCount == 1)
				{
					throw new Exception(ErrorMessage);
				}
				else
				{
					base.SaveAfterProcessingMessagesCore(factory);
				}
			}

			protected override bool ShouldRetryOnExceptionCore(int currentExceptionsCount, EDIMessage message, Exception lastException, int retryAttempts, string additionalErrorReportMessage = null)
			{
				return false;
			}
		}

		class BranchMessageProcessorForTestProcessInNewFactory : BranchMessageProcessorForTest
		{
			public BranchMessageProcessorForTestProcessInNewFactory(ZGuid[] messagePKsToFailProcessingOn)
				: base(messagePKsToFailProcessingOn, null)
			{
			}

			protected override bool MessageShouldBeProcessedInASeparateFactory => true;
		}

		sealed class BranchMessageProcessorForTestPreProcessInNewFactory : BranchMessageProcessorForTest
		{
			public BranchMessageProcessorForTestPreProcessInNewFactory() : base()
			{
			}

			protected override bool MessageShouldBeProcessedInASeparateFactory => true;

			public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
			{
				MessageExposed = message;
				return new PreProcessingApplicationTypeMessageProcessorForTestInNewFactory();
			}

			public EDIMessage MessageExposed { get; private set; }

			sealed class PreProcessingApplicationTypeMessageProcessorForTestInNewFactory : PreProcessingApplicationTypeMessageProcessorForTest
			{
				public PreProcessingApplicationTypeMessageProcessorForTestInNewFactory() : base(null)
				{
				}

				protected override void PreProcessMessageCore(EDIMessage message)
				{
					message.EM_Status = EDIMessage.Status.Error;
					message.EM_GB = GlbCompany.CurrentCompany.Branches.First(x => x.PK != message.EM_GB).PK;
					message.EM_HeldUntilDate = new ZDateTime(2024, 01, 25);
				}
			}
		}

		#endregion

		public void TestPreProcessMessages()
		{
			EDIMessage message1 = CreateEDIMessage("1");
			message1.EM_Status = EDIMessage.Status.Queued;
			message1.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(2);
			EDIMessage message2 = CreateEDIMessage("2");
			message2.EM_Status = EDIMessage.Status.Queued;
			message2.EM_HeldUntilDate = ZDateTime.UtcNow;
			EDIMessage message3 = CreateEDIMessage("3");
			message3.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			var messageProcessor = new BranchMessageProcessorForTest(null, new ZGuid[] { message1.PK });
			messageProcessor.Execute();

			message1.Reload();
			message2.Reload();
			message3.Reload();

			AssertEquals("message1.EM_Status", EDIMessage.Status.Queued, message1.EM_Status);
			AssertEquals("message2.EM_Status", EDIMessage.Status.Received, message2.EM_Status);
			AssertEquals("message3.EM_Status", EDIMessage.Status.Received, message3.EM_Status);
		}

		public void TestMessagesDiscardedInPreProcessAreNotPassedToProcessMessage()
		{
			AssertMessageWithAProblemInPreProcessNotPassedToProcessMessage(EDIMessage.Status.Discarded);
		}

		public void TestMessagesFailedInPreProcessAreNotPassedToProcessMessage()
		{
			AssertMessageWithAProblemInPreProcessNotPassedToProcessMessage(EDIMessage.Status.Failed);
		}

		public void TestMessagesWithErrorInPreProcessAreNotPassedToProcessMessage()
		{
			AssertMessageWithAProblemInPreProcessNotPassedToProcessMessage(EDIMessage.Status.Error);
		}

		void AssertMessageWithAProblemInPreProcessNotPassedToProcessMessage(string preProcessProblemStatus)
		{
			EDIMessage message1 = CreateEDIMessage("1");
			message1.EM_Status = EDIMessage.Status.Queued;
			EDIMessage message2 = CreateEDIMessage("2");
			message2.EM_Status = EDIMessage.Status.Queued;
			EDIMessage message3 = CreateEDIMessage("3");
			message3.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			var messageProcessor = new BranchMessageProcessorForTest(preProcessingFinalStatus: new Dictionary<ZGuid, string> { { message1.PK, preProcessProblemStatus } });
			messageProcessor.Execute();

			message1.Reload();
			message2.Reload();
			message3.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("All messages are pre-processed", 3, messageProcessor.MessagesPreProcessed);
				AssertEquals($"{preProcessProblemStatus} message is not passed to Process", 2, messageProcessor.MessagesProcessed);

				AssertEquals("message1.EM_Status", preProcessProblemStatus, message1.EM_Status);
				AssertEquals("message2.EM_Status", EDIMessage.Status.Received, message2.EM_Status);
				AssertEquals("message3.EM_Status", EDIMessage.Status.Received, message3.EM_Status);
			});
		}
	}
}
