using System.Collections.Generic;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management.Testing
{
	class UniversalTransactionBatchProcessorTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestProcess()
		{
			var message = GetQueuedUniversalTransactionBatchMessage(Message_ProcessedOK);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			message = GetQueuedUniversalTransactionBatchMessage(@"
<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<BatchType>
			<Code>XXX</Code>
		</BatchType>
		<TransactionCollection>
			<Transaction>
				<Category>SUM</Category>
				<CreateTime>2014-11-18T00:00:00</CreateTime>
				<PostDate>2014-11-17T00:00:00</PostDate>
				<PostingJournalCollection>
					<PostingJournal>
						<Description>TotalPaymentReceived</Description>
						<LocalAmount>0</LocalAmount>
					</PostingJournal>
				</PostingJournalCollection>
			</Transaction>
		</TransactionCollection>
	</TransactionBatch>
</UniversalTransactionBatch>");

			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

			AssertMultilineASCIIEquals("Service Task Log", @"
No Module used this Transaction Batch data.
".Trim(), serviceTaskLog.ToString());

			AssertMultilineASCIIEquals("Message Log Note", @"
No Module used this Transaction Batch data.
Message Discarded.
".Trim(), message.GetLogNoteText());
		}

		public void TestProcess_ImportResults()
		{
			var message = GetQueuedUniversalTransactionBatchMessage(Message_ProcessedOK);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			var importResults = (List<IImportResult>)manager.Logger.ImportResults;
			Assert("The import results of Logger should be greater than 0 after calling IndividualImportEnd.", importResults.Count > 0);
		}

		readonly string Message_ProcessedOK = @"
<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<BatchType>
			<Code>TST</Code>
		</BatchType>
		<TransactionCollection>
			<Transaction>
				<Category>SUM</Category>
				<CreateTime>2014-11-18T00:00:00</CreateTime>
				<PostDate>2014-11-17T00:00:00</PostDate>
				<PostingJournalCollection>
					<PostingJournal>
						<Description>TotalPaymentReceived</Description>
						<LocalAmount>0</LocalAmount>
					</PostingJournal>
				</PostingJournalCollection>
			</Transaction>
		</TransactionCollection>
	</TransactionBatch>
</UniversalTransactionBatch>";
	}
}
