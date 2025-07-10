using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	class FinancialInvoiceMessageActionTest : TestCaseWithFactory
	{
		public void TestDataTransferDirector()
		{
			var action = new FinancialInvoiceMessageAction(new BusinessObjectFactoryProvider(Factory));
			AssertEquals(typeof(MultipleInvoiceXmlDataTransferDirector), action.GetNewDataTransferDirector().GetType());
		}

		public void TestImportingFinancialTransactionThroughXMSServiceTask()
		{
			TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);
			AssertEquals("Precondition: EDIMessage table should be empty.", 0, Factory.GetDatabaseCount(typeof(EDIMessage)));

			var message = Factory.New<Messaging.Business.XmlMessaging.XmlEDIMessage>();
			message.EM_MessageType = EDIMessageTypeList.Codes.XMS;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.XMS;
			message.EM_ReceiveTransmit = EDIMessage.Status.Received;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageSubType = "FTR";
			message.EM_MessageText = "<FinancialTransactions><ns0:FinancialInvoice xmlns:ns0=\"http://www.edi.com.au/EnterpriseService/\">          <ns0:Ledger>AP</ns0:Ledger>          <ns0:DebtorOrCreditor EDICode=\"SDFALV\">          </ns0:DebtorOrCreditor>          <ns0:TxnType>INV</ns0:TxnType>          <ns0:TxnNumber>YSGJENtest_0</ns0:TxnNumber>          <ns0:InvoiceDate>2011-06-22T00:00:00.0000000</ns0:InvoiceDate>          <ns0:PostDate>2011-06-22T00:00:00.0000000</ns0:PostDate>          <ns0:Branch>          </ns0:Branch>          <ns0:OsInvoiceAmtExclTax CurrencyCode=\"AUD\">-400000</ns0:OsInvoiceAmtExclTax>          <ns0:OsInvoiceAmtInclTax CurrencyCode=\"AUD\">-400000</ns0:OsInvoiceAmtInclTax>          <ns0:BankCode>CBA2</ns0:BankCode>          <ns0:ReceiptPaymentType>EFT</ns0:ReceiptPaymentType>          <ns0:TxnLines>              <ns0:TxnLine>                  <ns0:ChargeCode>CUSDSB</ns0:ChargeCode>                  <ns0:Branch></ns0:Branch>                  <ns0:ConsolOrJobNo></ns0:ConsolOrJobNo>                  <ns0:OsInvoiceAmtExclTax CurrencyCode=\"AUD\">-400000</ns0:OsInvoiceAmtExclTax>                  <ns0:OsInvoiceAmtInclTax CurrencyCode=\"AUD\">-400000</ns0:OsInvoiceAmtInclTax>              </ns0:TxnLine>          </ns0:TxnLines>      </ns0:FinancialInvoice></FinancialTransactions>";
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;

			Factory.Save();
			AssertEquals("Precondition: EDIMessage table shoulb have 1 message.", 1, Factory.GetDatabaseCount(typeof(EDIMessage)));

			var notifications = new NotificationBuffer();
			var processor = new ServiceManager.Tasks.StandardXMLProcessor.StandardXMLMessageProcessor();

			processor.Process(notifications);
			Factory.ReloadAll<Messaging.Business.XmlMessaging.XmlEDIMessage>();
			Assert("Importing should have ERROR", notifications.HasErrors);
			Assert("Importing should have ErrorType.Error", notifications.ContainsNotificationType(ErrorType.Error));
			Assert("Importing should not have ErrorType.XmlSchemaValidation", !notifications.ContainsNotificationType(ErrorType.XmlSchemaValidation));
			Assert("Importing should not habe ErrorType.DataErrorPreventSave", !notifications.ContainsNotificationType(ErrorType.DataErrorPreventSave));
			AssertEquals("FinancialInvoice should fail to import due to the error", "ERR", message.EM_Status);
		}
	}
}
