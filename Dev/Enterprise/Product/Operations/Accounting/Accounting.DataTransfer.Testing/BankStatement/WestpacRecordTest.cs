using System;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.BankStatement.Testing
{
	public class WestpacRecordTest : TestCase
	{
		[ExpectException(typeof(FormatException))]
		public void TestConstructor_WrongFieldsCount()
		{
			WestpacRecord record = new WestpacRecord("\"ABC\",\"Statement Transactions Report for Express International Logistic\",\"Printed By\",\"Dave Gotts\",\"Report Date\",5/06/2007,\"Report Time\", 7:55:09a.m.,\"Page 1 of  1\",\"Account Number\",\"03-0207-0222452-00\",\"Statement Opening Balance as at\",1/06/2007,\"$209,193.17\",\"Statement Closing Balance as at\",1/06/2007,\"$197,415.46\",\"Account Name\",\"EXPRESS LOGISTI\",\"Net Movement\",\"($11,777.71)\",\"Other Party Name\",\"MTS\",\"Particulars\",\"Analysis Code\",\"Reference\",\"TC\",\"Debit\",\"Credit\",\"Date\",\"Balance\",\"S&S WRIGHT\",\"AP\",\"SAND OSH\",\"\"", new CsvFlatFileFormat(true));
		}

		public void TestInvalidColumnsMessageContainsCorrectBankName()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			BankStatementDataAdapter adapter = new BankStatementDataAdapter();
			var bankStatement = factory.NewWithValidTestData<Business.Base.AccStatement.BankStatement>();
			bankStatement.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			bankStatement.AB_LastReconcileDate = new ZDateTime(2006, 11, 6);

			BankStatementXmlDataTransferDirector director = new BankStatementXmlDataTransferDirector(bankStatement, BankStatementFormat.StatementFileFormats.WestpacNewZealand, adapter, false);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using var resourceRetriever = new EmbeddedResourceRetriever();
			string westpacFileInvalidColumns = resourceRetriever.SaveResourceToFile("WestpacNZBankStatementINVALIDCOLUMNS.csv");
			director.Import(westpacFileInvalidColumns, new NotificationBuffer(), SourceInfo.EmptySourceInfo);

			Assert("Should report column error for WESTPAC", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Each line of Westpac file is expected to have at least 39 fields."));
		}

		public void TestConstructor_SupportedType()
		{
			CsvFlatFileFormat format = new CsvFlatFileFormat(true);

			WestpacRecord record = new WestpacRecord("\"ABC\",\"Statement Transactions Report for Express International Logistic\",\"Printed By\",\"Dave Gotts\",\"Report Date\",5/06/2007,\"Report Time\", 7:55:09a.m.,\"Page 1 of  1\",\"Account Number\",\"03-0207-0222452-00\",\"Statement Opening Balance as at\",1/06/2007,\"$209,193.17\",\"Statement Closing Balance as at\",1/06/2007,\"$197,415.46\",\"Account Name\",\"EXPRESS LOGISTI\",\"Net Movement\",\"($11,777.71)\",\"Other Party Name\",\"MTS\",\"Particulars\",\"Analysis Code\",\"Reference\",\"TC\",\"Debit\",\"Credit\",\"Date\",\"Balance\",\"S&S WRIGHT\",\"AP\",\"SAND OSH\",\"\",\"\",\"15\",112.50,", new CsvFlatFileFormat(true));
			AssertEquals("Type", ZArchitecture.Core.ReceiptTypes.EFT, record.Type);
			AssertEquals("Reference", "", record.Reference);
			AssertEquals("Amount", 112.50m, record.Amount);

			record = new WestpacRecord("\"ABC\",\"Statement Transactions Report for Express International Logistic\",\"Printed By\",\"Dave Gotts\",\"Report Date\",5/06/2007,\"Report Time\", 7:55:09a.m.,\"Page 1 of  1\",\"Account Number\",\"03-0207-0222452-00\",\"Statement Opening Balance as at\",1/06/2007,\"$209,193.17\",\"Statement Closing Balance as at\",1/06/2007,\"$197,415.46\",\"Account Name\",\"EXPRESS LOGISTI\",\"Net Movement\",\"($11,777.71)\",\"Other Party Name\",\"MTS\",\"Particulars\",\"Analysis Code\",\"Reference\",\"TC\",\"Debit\",\"Credit\",\"Date\",\"Balance\",\"S&S WRIGHT\",\"AP\",\"SAND OSH\",\"\",\"Reference\",\"15\",112.50,11", new CsvFlatFileFormat(true));
			AssertEquals("Type", ZArchitecture.Core.ReceiptTypes.EFT, record.Type);
			AssertEquals("Reference", "Reference", record.Reference);
			AssertEquals("Amount", 112.50m, record.Amount);

			record = new WestpacRecord("\"ABC\",\"Statement Transactions Report for Express International Logistic\",\"Printed By\",\"Dave Gotts\",\"Report Date\",5/06/2007,\"Report Time\", 7:55:09a.m.,\"Page 1 of  1\",\"Account Number\",\"03-0207-0222452-00\",\"Statement Opening Balance as at\",1/06/2007,\"$209,193.17\",\"Statement Closing Balance as at\",1/06/2007,\"$197,415.46\",\"Account Name\",\"EXPRESS LOGISTI\",\"Net Movement\",\"($11,777.71)\",\"Other Party Name\",\"MTS\",\"Particulars\",\"Analysis Code\",\"Reference\",\"TC\",\"Debit\",\"Credit\",\"Date\",\"Balance\",\"S&S WRIGHT\",\"AP\",\"SAND OSH\",\"\",\"\",\"15\",,112.50", new CsvFlatFileFormat(true));
			AssertEquals("Type", ZArchitecture.Core.TransactionTypes.ReceiptBatch, record.Type);
			AssertEquals("Reference", "", record.Reference);
			AssertEquals("Amount", -112.50m, record.Amount);

			record = new WestpacRecord("\"ABC\",\"Statement Transactions Report for Express International Logistic\",\"Printed By\",\"Dave Gotts\",\"Report Date\",5/06/2007,\"Report Time\", 7:55:09a.m.,\"Page 1 of  1\",\"Account Number\",\"03-0207-0222452-00\",\"Statement Opening Balance as at\",1/06/2007,\"$209,193.17\",\"Statement Closing Balance as at\",1/06/2007,\"$197,415.46\",\"Account Name\",\"EXPRESS LOGISTI\",\"Net Movement\",\"($11,777.71)\",\"Other Party Name\",\"MTS\",\"Particulars\",\"Analysis Code\",\"Reference\",\"TC\",\"Debit\",\"Credit\",\"Date\",\"Balance\",\"S&S WRIGHT\",\"CR\",\"SAND OSH\",\"\",\"\",\"15\",112.50,", new CsvFlatFileFormat(true));
			AssertEquals("Type", ZArchitecture.Core.TransactionTypes.ReceiptBatch, record.Type);
			AssertEquals("Reference", "", record.Reference);
			AssertEquals("Amount", 112.50m, record.Amount);

			record = new WestpacRecord("\"ABC\",\"Statement Transactions Report for Express International Logistic\",\"Printed By\",\"Dave Gotts\",\"Report Date\",5/06/2007,\"Report Time\", 7:55:09a.m.,\"Page 1 of  1\",\"Account Number\",\"03-0207-0222452-00\",\"Statement Opening Balance as at\",1/06/2007,\"$209,193.17\",\"Statement Closing Balance as at\",1/06/2007,\"$197,415.46\",\"Account Name\",\"EXPRESS LOGISTI\",\"Net Movement\",\"($11,777.71)\",\"Other Party Name\",\"MTS\",\"Particulars\",\"Analysis Code\",\"Reference\",\"TC\",\"Debit\",\"Credit\",\"Date\",\"Balance\",\"S&S WRIGHT\",\"CR\",\"SAND OSH\",\"\",\"\",\"15\",,112.50", new CsvFlatFileFormat(true));
			AssertEquals("Type", ZArchitecture.Core.TransactionTypes.ReceiptBatch, record.Type);
			AssertEquals("Reference", "", record.Reference);
			AssertEquals("Amount", -112.50m, record.Amount);

			record = new WestpacRecord("\"ABC\",\"Statement Transactions Report for Express International Logistic\",\"Printed By\",\"Dave Gotts\",\"Report Date\",5/06/2007,\"Report Time\", 7:55:09a.m.,\"Page 1 of  1\",\"Account Number\",\"03-0207-0222452-00\",\"Statement Opening Balance as at\",1/06/2007,\"$209,193.17\",\"Statement Closing Balance as at\",1/06/2007,\"$197,415.46\",\"Account Name\",\"EXPRESS LOGISTI\",\"Net Movement\",\"($11,777.71)\",\"Other Party Name\",\"MTS\",\"Particulars\",\"Analysis Code\",\"Reference\",\"TC\",\"Debit\",\"Credit\",\"Date\",\"Balance\",\"S&S WRIGHT\",\"DC\",\"SAND OSH\",\"\",\"\",\"15\",112.50,", new CsvFlatFileFormat(true));
			AssertEquals("Type", ZArchitecture.Core.ReceiptTypes.DirectDebit, record.Type);
			AssertEquals("Reference", "", record.Reference);
			AssertEquals("Amount", 112.50m, record.Amount);

			record = new WestpacRecord("\"ABC\",\"Statement Transactions Report for Express International Logistic\",\"Printed By\",\"Dave Gotts\",\"Report Date\",5/06/2007,\"Report Time\", 7:55:09a.m.,\"Page 1 of  1\",\"Account Number\",\"03-0207-0222452-00\",\"Statement Opening Balance as at\",1/06/2007,\"$209,193.17\",\"Statement Closing Balance as at\",1/06/2007,\"$197,415.46\",\"Account Name\",\"EXPRESS LOGISTI\",\"Net Movement\",\"($11,777.71)\",\"Other Party Name\",\"MTS\",\"Particulars\",\"Analysis Code\",\"Reference\",\"TC\",\"Debit\",\"Credit\",\"Date\",\"Balance\",\"S&S WRIGHT\",\"DC\",\"SAND OSH\",\"\",\"\",\"15\",,112.50", new CsvFlatFileFormat(true));
			AssertEquals("Type", ZArchitecture.Core.ReceiptTypes.DirectCredit, record.Type);
			AssertEquals("Reference", "", record.Reference);
			AssertEquals("Amount", -112.50m, record.Amount);

			record = new WestpacRecord("\"ABC\",\"Statement Transactions Report for Express International Logistic\",\"Printed By\",\"Dave Gotts\",\"Report Date\",5/06/2007,\"Report Time\", 7:55:09a.m.,\"Page 1 of  1\",\"Account Number\",\"03-0207-0222452-00\",\"Statement Opening Balance as at\",1/06/2007,\"$209,193.17\",\"Statement Closing Balance as at\",1/06/2007,\"$197,415.46\",\"Account Name\",\"EXPRESS LOGISTI\",\"Net Movement\",\"($11,777.71)\",\"Other Party Name\",\"MTS\",\"Particulars\",\"Analysis Code\",\"Reference\",\"TC\",\"Debit\",\"Credit\",\"Date\",\"Balance\",\"S&S WRIGHT\",\"PS\",\"SAND OSH\",\"\",\"\",\"15\",112.50,", new CsvFlatFileFormat(true));
			AssertEquals("Type", ZArchitecture.Core.ReceiptTypes.EFT, record.Type);
			AssertEquals("Reference", "", record.Reference);
			AssertEquals("Amount", 112.50m, record.Amount);

			record = new WestpacRecord("\"ABC\",\"Statement Transactions Report for Express International Logistic\",\"Printed By\",\"Dave Gotts\",\"Report Date\",5/06/2007,\"Report Time\", 7:55:09a.m.,\"Page 1 of  1\",\"Account Number\",\"03-0207-0222452-00\",\"Statement Opening Balance as at\",1/06/2007,\"$209,193.17\",\"Statement Closing Balance as at\",1/06/2007,\"$197,415.46\",\"Account Name\",\"EXPRESS LOGISTI\",\"Net Movement\",\"($11,777.71)\",\"Other Party Name\",\"MTS\",\"Particulars\",\"Analysis Code\",\"Reference\",\"TC\",\"Debit\",\"Credit\",\"Date\",\"Balance\",\"S&S WRIGHT\",\"PS\",\"SAND OSH\",\"\",\"\",\"15\",,112.50", new CsvFlatFileFormat(true));
			AssertEquals("Type", ZArchitecture.Core.TransactionTypes.ReceiptBatch, record.Type);
			AssertEquals("Reference", "", record.Reference);
			AssertEquals("Amount", -112.50m, record.Amount);

			record = new WestpacRecord("\"ABC\",\"Statement Transactions Report for Express International Logistic\",\"Printed By\",\"Dave Gotts\",\"Report Date\",5/06/2007,\"Report Time\", 7:55:09a.m.,\"Page 1 of  1\",\"Account Number\",\"03-0207-0222452-00\",\"Statement Opening Balance as at\",1/06/2007,\"$209,193.17\",\"Statement Closing Balance as at\",1/06/2007,\"$197,415.46\",\"Account Name\",\"EXPRESS LOGISTI\",\"Net Movement\",\"($11,777.71)\",\"Other Party Name\",\"MTS\",\"Particulars\",\"Analysis Code\",\"Reference\",\"TC\",\"Debit\",\"Credit\",\"Date\",\"Balance\",\"S&S WRIGHT\",\"\",\"SAND OSH\",\"\",\"\",\"15\",112.50,", new CsvFlatFileFormat(true));
			AssertEquals("Type", ZArchitecture.Core.ReceiptTypes.Cheque, record.Type);
			AssertEquals("Reference", "", record.Reference);
			AssertEquals("Amount", 112.50m, record.Amount);

			record = new WestpacRecord("\"ABC\",\"Statement Transactions Report for Express International Logistic\",\"Printed By\",\"Dave Gotts\",\"Report Date\",5/06/2007,\"Report Time\", 7:55:09a.m.,\"Page 1 of  1\",\"Account Number\",\"03-0207-0222452-00\",\"Statement Opening Balance as at\",1/06/2007,\"$209,193.17\",\"Statement Closing Balance as at\",1/06/2007,\"$197,415.46\",\"Account Name\",\"EXPRESS LOGISTI\",\"Net Movement\",\"($11,777.71)\",\"Other Party Name\",\"MTS\",\"Particulars\",\"Analysis Code\",\"Reference\",\"TC\",\"Debit\",\"Credit\",\"Date\",\"Balance\",\"S&S WRIGHT\",\"\",\"SAND OSH\",\"\",\"\",\"15\",,112.50", new CsvFlatFileFormat(true));
			AssertEquals("Type", ZArchitecture.Core.TransactionTypes.ReceiptBatch, record.Type);
			AssertEquals("Reference", "", record.Reference);
			AssertEquals("Amount", -112.50m, record.Amount);
		}

		public void TestConstructor_NotSupportedType()
		{
			CsvFlatFileFormat format = new CsvFlatFileFormat(true);

			WestpacRecord record = new WestpacRecord("\"ABC\",\"Statement Transactions Report for Express International Logistic\",\"Printed By\",\"Dave Gotts\",\"Report Date\",5/06/2007,\"Report Time\", 7:55:09a.m.,\"Page 1 of  1\",\"Account Number\",\"03-0207-0222452-00\",\"Statement Opening Balance as at\",1/06/2007,\"$209,193.17\",\"Statement Closing Balance as at\",1/06/2007,\"$197,415.46\",\"Account Name\",\"EXPRESS LOGISTI\",\"Net Movement\",\"($11,777.71)\",\"Other Party Name\",\"MTS\",\"Particulars\",\"Analysis Code\",\"Reference\",\"TC\",\"Debit\",\"Credit\",\"Date\",\"Balance\",\"<<<+-+>>>\",\"\",\"\",\"\",\"\",\"\",,,,\"\",\"Total\",33928.70,22150.99,\"Transactions were last retrieved for \",\"No transactions have been retrieved for this account.\"", new CsvFlatFileFormat(true));
			Assert("It shoud be not supported line", record.IsNotSupported);
			AssertEquals("Type", "", record.Type);
			AssertEquals("Reference", "", record.Reference);
			AssertEquals("Amount", 0m, record.Amount);

			record = new WestpacRecord(",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,", format);
			Assert("It shoud be supported line", !record.IsNotSupported);
			AssertEquals("Type", ZArchitecture.Core.TransactionTypes.ReceiptBatch, record.Type);
			AssertEquals("Reference", "", record.Reference);
			AssertEquals("Amount", 0m, record.Amount);
		}
	}
}
