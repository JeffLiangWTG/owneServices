using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CA.DataTransfer.Testing
{
	[TestedType(typeof(ARLTransactionBatchDataContextManager))]
	sealed class ARLTransactionBatchDataContextManagerTest : DataContextManagerTestCase<ARLTransactionBatchDataContextManager, CusStatementHeader>
	{
		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("CusStatementHeader doesn't have any JobNumber", true);
		}

		public void TestGetProcessor()
		{
			var messageText = @"
<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CAAccountsReceivableLedger</Type>
					<Key></Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<BatchType>
			<Code>DN</Code>
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

			var message = Factory.New<EDIMessage>();

			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = messageText;

			var transactionBatch = message.GetEM_MessageTextReader().Parse<TransactionBatch>();

			Assert(new ARLTransactionBatchDataContextManagerForTest(true).UseIncomingTransactionBatchData(message, transactionBatch, null, null));
			Assert(!new ARLTransactionBatchDataContextManagerForTest(false).UseIncomingTransactionBatchData(message, transactionBatch, null, null));
		}

		sealed class ARLTransactionBatchDataContextManagerForTest : ARLTransactionBatchDataContextManager
		{
			public ARLTransactionBatchDataContextManagerForTest(bool dummyProcessResult)
			{
				this.dummyProcessResult = dummyProcessResult;
			}

			readonly bool dummyProcessResult;

			protected override ITransactionBatchMessageProcessor GetProcessor(EDIMessage message, TransactionBatch transactionBatch, IXmlSessionTracker logger)
				=> new DummyTransactionBatchMessageProcessor(dummyProcessResult);
		}

		sealed class DummyTransactionBatchMessageProcessor : ITransactionBatchMessageProcessor
		{
			public DummyTransactionBatchMessageProcessor(bool processResult)
			{
				this.processResult = processResult;
			}

			public bool Process() => processResult;

			readonly bool processResult;

			public IKeysResult GetKeysForBlockingParallelImport() => new KeysResult { IsMatch = processResult };

			sealed class KeysResult : IKeysResult
			{
				public IEnumerable<(string KeyValue, string KeySource)> KeysInfo => Enumerable.Empty<(string KeyValue, string KeySource)>();

				public bool IsMatch { get; set; }

				public IEnumerable<string> Keys => KeysInfo.Select(k => k.KeyValue);
			}
		}
	}
}
