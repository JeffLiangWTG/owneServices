using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Moq;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class TransactionBatchMessageProcessorProviderTest : TestCaseWithFactory
	{
		public void TestGetProcessor_DN()
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
			<Description>Daily Notices</Description>
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

			var mock = new Mock<IXmlSessionTracker>();
			var transactionBatch = message.GetEM_MessageTextReader().Parse<TransactionBatch>();
			var processor = TransactionBatchMessageProcessorProvider.GetProcessor(message, transactionBatch, mock.Object);
			AssertType(typeof(ARLDailyNoticeMessageProcessor), processor);
		}

		public void TestGetProcessor_SoA()
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
			<Code>SOA</Code>
			<Description>Statement of Account</Description>
		</BatchType>
		<TransactionCollection>
			<Transaction>
				<PostingJournalCollection>
					<PostingJournal>
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

			var mock = new Mock<IXmlSessionTracker>();
			var transactionBatch = message.GetEM_MessageTextReader().Parse<TransactionBatch>();
			var processor = TransactionBatchMessageProcessorProvider.GetProcessor(message, transactionBatch, mock.Object);
			AssertType(typeof(ARLStatementOfAccountMessageProcessor), processor);
		}
	}
}
