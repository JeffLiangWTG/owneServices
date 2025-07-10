using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Universal.Testing
{
	abstract class AccountingReceiptPaymentBatchDataContextManagerTest<T, U> : DataContextManagerTestCase<T, U>
		where T : AccountingReceiptPaymentBatchDataContextManager<U>, new()
		where U : BusinessObject
	{
		public void TestUseIncomingTransactionBatchData_InvalidDataTargetType()
		{
			var messageText = @"
<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<TransactionCollection>
			<Transaction>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Type>InvalidDataTargetType</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
			</Transaction>
		</TransactionCollection>
	</TransactionBatch>
</UniversalTransactionBatch>";

			var tupleResult = CreateTransactionBatch(messageText);
			var logger = new TestErrorLogger();
			var result = GetDataContextManager().UseIncomingTransactionBatchData(tupleResult.ediMessage, tupleResult.transactionBatch, logger, Factory);

			Assert("The result should be false due to the invalid DataTarget Type.", !result);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 03, 15)]
		public void TestUseIncomingTransactionBatchData_InsertMiscTransaction()
		{
			var xml = File.ReadAllText(BaseSourcePath + TestCasePath + InesrtMiscTransactionXml);
			var tupleResult = CreateTransactionBatch(xml);

			var overpaymentQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Overpayment);
			var overpaymentCountBeforeImport = Factory.Load<AROverpayment>(overpaymentQuery).Length;

			var bankFeeQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			bankFeeQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCreatedByMatching, true);
			var bankFeeCountBeforeImport = Factory.Load<ARJournal>(bankFeeQuery).Length;

			var discountQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Discount);
			var discountCountBeforeImport = Factory.Load<ARDiscount>(discountQuery).Length;

			var exchangeDifferenceQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference);
			var exchangeDifferenceCountBeforeImport = Factory.Load<ARExchangeDifference>(exchangeDifferenceQuery).Length;

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var serviceTask = new UniversalMessageProcessingManager(Factory, serviceTaskLog);

			serviceTask.Process(tupleResult.ediMessage);
			Factory.SaveAtEndOfImport(new TestErrorLogger());

			AssertEquals(@"Begin processing Transaction AR REC ABIGAS ZHSBCAUD CSH: 
  Completed Processing Transaction.", serviceTaskLog.ToString());

			var overpaymentCountAfterImport = Factory.Load<AROverpayment>(overpaymentQuery).Length;
			AssertEquals("Should insert overpayment after import.", overpaymentCountBeforeImport + 1, overpaymentCountAfterImport);

			var bankFeeCountAfterImport = Factory.Load<ARJournal>(bankFeeQuery).Length;
			AssertEquals("Should insert bank fee journal after import.", bankFeeCountBeforeImport + 1, bankFeeCountAfterImport);

			var discountCountAfterImport = Factory.Load<ARDiscount>(discountQuery).Length;
			AssertEquals("Should insert discount after import.", discountCountBeforeImport + 1, discountCountAfterImport);

			var exchangeDifferenceCountAfterImport = Factory.Load<ARExchangeDifference>(exchangeDifferenceQuery).Length;
			AssertEquals("Should insert exchangeDifference after import.", exchangeDifferenceCountBeforeImport + 1, exchangeDifferenceCountAfterImport);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 03, 15)]
		public void TestUseIncomingTransactionBatchData_ErrorWhenPaymentContainOverpayment()
		{
			var xml = File.ReadAllText(BaseSourcePath + TestCasePath + ErrorWhenPaymentContainOverpaymentXml);
			var tupleResult = CreateTransactionBatch(xml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var serviceTask = new UniversalMessageProcessingManager(Factory, serviceTaskLog);

			serviceTask.Process(tupleResult.ediMessage);
			Factory.SaveAtEndOfImport(new TestErrorLogger());

			AssertEquals(@"Begin processing Transaction AR PAY ABIGAS ZHSBCAUD CSH: 
ERROR - Overpayment transaction cannot be created for payment match group.
ERROR - Matching failed. There are no matched transaction.
  This transaction has errors and was not imported.
No Module used this Transaction Batch data.", serviceTaskLog.ToString());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 03, 15)]
		public void TestUseIncomingTransactionBatchData_ErrorWhenTransactionInfoContainsMultipleMatchingGroup()
		{
			var xml = File.ReadAllText(BaseSourcePath + TestCasePath + ErrorWhenTransactionInfoContainsMultipleMatchingGroup);
			var tupleResult = CreateTransactionBatch(xml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var serviceTask = new UniversalMessageProcessingManager(Factory, serviceTaskLog);

			serviceTask.Process(tupleResult.ediMessage);
			Factory.SaveAtEndOfImport(new TestErrorLogger());

			AssertEquals(@"ERROR - Only one match group is allowed in <MatchLineCollection>.
No Module used this Transaction Batch data.", serviceTaskLog.ToString());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 03, 15)]
		public void TestUseIncomingTransactionBatchData_ErrorWhenNoTransactionwithSameKeyWithMatchLine()
		{
			var xml = File.ReadAllText(BaseSourcePath + TestCasePath + ErrorWhenNoTransactionwithSameKeyWithMatchLine);
			var tupleResult = CreateTransactionBatch(xml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var serviceTask = new UniversalMessageProcessingManager(Factory, serviceTaskLog);

			serviceTask.Process(tupleResult.ediMessage);
			Factory.SaveAtEndOfImport(new TestErrorLogger());

			AssertEquals(@"ERROR - Each Miscellaneous Match Transaction in <MatchLine> must be supported by <Transaction> data with the same <LinkedTransactionID><Key>.
No Module used this Transaction Batch data.", serviceTaskLog.ToString());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 03, 15)]
		public void TestUseIncomingTransactionBatchData_ErrorWhenNoTransactionwithSameKeyWithMatchLine_Journal()
		{
			var xml = File.ReadAllText(BaseSourcePath + TestCasePath + ErrorWhenNoTransactionwithSameKeyWithMatchLine_JNL);
			var tupleResult = CreateTransactionBatch(xml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var serviceTask = new UniversalMessageProcessingManager(Factory, serviceTaskLog);

			serviceTask.Process(tupleResult.ediMessage);
			Factory.SaveAtEndOfImport(new TestErrorLogger());

			AssertEquals(@"Begin processing Transaction AR REC ABIGAS ZHSBCAUD CSH: 
ERROR - Paid Transaction AR JNL 00001023: Matching failed. Transaction was not found.
  This transaction has errors and was not imported.
No Module used this Transaction Batch data.", serviceTaskLog.ToString());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 03, 15)]
		public void TestUseIncomingTransactionBatchData_ErrorWhenTransactionContainsMultipleDiscount()
		{
			var xml = File.ReadAllText(BaseSourcePath + TestCasePath + ErrorWhenTransactionContainsMultipleDiscount);
			var tupleResult = CreateTransactionBatch(xml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var serviceTask = new UniversalMessageProcessingManager(Factory, serviceTaskLog);

			serviceTask.Process(tupleResult.ediMessage);
			Factory.SaveAtEndOfImport(new TestErrorLogger());

			AssertEquals(@"Begin processing Transaction AR REC ABIGAS ZHSBCAUD CSH: 
ERROR - Only one Discount transaction can be created per match group.
  This transaction has errors and was not imported.
No Module used this Transaction Batch data.", serviceTaskLog.ToString());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 03, 15)]
		public void TestUseIncomingTransactionBatchData_ErrorWhenTransactionContainsMultipleOverpayment()
		{
			var xml = File.ReadAllText(BaseSourcePath + TestCasePath + ErrorWhenTransactionContainsMultipleOverpayment);
			var tupleResult = CreateTransactionBatch(xml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var serviceTask = new UniversalMessageProcessingManager(Factory, serviceTaskLog);

			serviceTask.Process(tupleResult.ediMessage);
			Factory.SaveAtEndOfImport(new TestErrorLogger());

			AssertEquals(@"Begin processing Transaction AR REC ABIGAS ZHSBCAUD CSH: 
ERROR - Only one Overpayment transaction can be created per match group.
  This transaction has errors and was not imported.
No Module used this Transaction Batch data.", serviceTaskLog.ToString());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 03, 15)]
		public void TestUseIncomingTransactionBatchData_ErrorWhenTransactionContainsMultipleExchangeDifferences()
		{
			var xml = File.ReadAllText(BaseSourcePath + TestCasePath + ErrorWhenTransactionContainsMultipleExchangeDifferences);
			var tupleResult = CreateTransactionBatch(xml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var serviceTask = new UniversalMessageProcessingManager(Factory, serviceTaskLog);

			serviceTask.Process(tupleResult.ediMessage);
			Factory.SaveAtEndOfImport(new TestErrorLogger());

			AssertEquals(@"Begin processing Transaction AR REC ABIGAS ZHSBCAUD CSH: 
ERROR - Only one Exchange Difference transaction can be created per match group.
  This transaction has errors and was not imported.
No Module used this Transaction Batch data."
, serviceTaskLog.ToString());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 03, 15)]
		public void TestUseIncomingTransactionBatchData_ErrorWhenTransactionContainsMultipleBankFee()
		{
			var xml = File.ReadAllText(BaseSourcePath + TestCasePath + ErrorWhenTransactionContainsMultipleBankFee);
			var tupleResult = CreateTransactionBatch(xml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var serviceTask = new UniversalMessageProcessingManager(Factory, serviceTaskLog);

			serviceTask.Process(tupleResult.ediMessage);
			Factory.SaveAtEndOfImport(new TestErrorLogger());

			AssertEquals(@"Begin processing Transaction AR REC ABIGAS ZHSBCAUD CSH: 
ERROR - Only one Bank Fee Journal can be created per match group.
  This transaction has errors and was not imported.
No Module used this Transaction Batch data."
, serviceTaskLog.ToString());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 03, 15)]
		public void TestUseIncomingTransactionBatchData_ErrorWhenDiscountContainsForeignCurrency()
		{
			var xml = File.ReadAllText(BaseSourcePath + TestCasePath + ErrorWhenDiscountContainsForeignCurrency);
			var tupleResult = CreateTransactionBatch(xml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var serviceTask = new UniversalMessageProcessingManager(Factory, serviceTaskLog);

			serviceTask.Process(tupleResult.ediMessage);
			Factory.SaveAtEndOfImport(new TestErrorLogger());

			AssertEquals(@"ERROR - Exchange difference and discount transactions must be in local currency.
No Module used this Transaction Batch data."
, serviceTaskLog.ToString());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 03, 15)]
		public void TestUseIncomingTransactionBatchData_ErrorWhenExchangeRateDifferenceContainsForeignCurrency()
		{
			var xml = File.ReadAllText(BaseSourcePath + TestCasePath + ErrorWhenExchangeRateDifferenceContainsForeignCurrency);
			var tupleResult = CreateTransactionBatch(xml);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var serviceTask = new UniversalMessageProcessingManager(Factory, serviceTaskLog);

			serviceTask.Process(tupleResult.ediMessage);
			Factory.SaveAtEndOfImport(new TestErrorLogger());

			AssertEquals(@"ERROR - Exchange difference and discount transactions must be in local currency.
No Module used this Transaction Batch data."
, serviceTaskLog.ToString());
		}

		protected (TransactionBatch transactionBatch, EDIMessage ediMessage) CreateTransactionBatch(string messageText)
		{
			var message = TestFactory.New<EDIMessage>();

			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = messageText;

			return (message.GetEM_MessageTextReader().Parse<TransactionBatch>(), message);
		}

		protected ARInvoice CreateInvoiceForMatching()
		{
			var invoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", TestObjectCreator.AUD, 1M, 1000M, 100M, 1000M, 100M, TestObjectCreator.ABIGAS, TestObjectCreator.GLHeader2.PK, "FIN");
			TestFactory.Save();
			return invoice;
		}

		const string TestCasePath = @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer.Testing\Universal\PaymentReceiptUniversalBatch\TestCases\";
		const string InesrtMiscTransactionXml = "InesrtMiscTransaction.xml";
		const string ErrorWhenPaymentContainOverpaymentXml = "ErrorWhenPaymentContainOverpayment.xml";
		const string ErrorWhenTransactionInfoContainsMultipleMatchingGroup = "ErrorWhenTransactionInfoContainsMultipleMatchingGroup.xml";
		const string ErrorWhenNoTransactionwithSameKeyWithMatchLine = "ErrorWhenNoTransactionwithSameKeyWithMatchLine.xml";
		const string ErrorWhenNoTransactionwithSameKeyWithMatchLine_JNL = "ErrorWhenNoTransactionwithSameKeyWithMatchLine_JNL.xml";
		const string ErrorWhenTransactionContainsMultipleDiscount = "ErrorWhenTransactionContainsMultipleDiscount.xml";
		const string ErrorWhenTransactionContainsMultipleOverpayment = "ErrorWhenTransactionContainsMultipleOverpayment.xml";
		const string ErrorWhenTransactionContainsMultipleExchangeDifferences = "ErrorWhenTransactionContainsMultipleExchangeDifferences.xml";
		const string ErrorWhenTransactionContainsMultipleBankFee = "ErrorWhenTransactionContainsMultipleBankFee.xml";
		const string ErrorWhenDiscountContainsForeignCurrency = "ErrorWhenDiscountContainsForeignCurrency.xml";
		const string ErrorWhenExchangeRateDifferenceContainsForeignCurrency = "ErrorWhenExchangeRateDifferenceContainsForeignCurrency.xml";

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("ReceiptPaymentBase doesn't have any JobNumber", true);
		}

		protected abstract T GetDataContextManager();

		protected override void SetUp()
		{
			base.SetUp();

			TestFactory = new BusinessObjectFactory();
			TestObjectCreator = new TestObjectCreator(TestFactory);
			var bankAccount = TestObjectCreator.AUDBankAccount;
			TestObjectCreator.CreateTestPeriods(new ZDateTime(2021, 03, 01));
			TestFactory.Save();

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateARSuspenseControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateAPSuspenseControlAccount().PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateJobRevenueJournalControlAccount().PK.ToGuid());
		}

		protected TestObjectCreator TestObjectCreator;
		protected BusinessObjectFactory TestFactory;
	}
}
