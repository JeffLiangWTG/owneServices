using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.Billing.Integration;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.DataTransfer.BankStatement.Testing
{
	sealed class BankStatementXmlDataTransferDirectorTest : TestCaseWithFactory
	{
		public void TestFileBeingUsedbyAnotherProcess()
		{
			string fileName = Env.GetTempFileName(Env.TempPath, ".xml");

			try
			{
				using (FileStream xmlStream = File.OpenWrite(fileName))
				{
					Director.Import(fileName, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
				}
				AssertEquals("Message", "The process cannot access the file '" + fileName + "' because it is being used by another process.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("Message", "No statement imported.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				File.Delete(fileName);
			}
		}

		public void TestImportValidXmlFile_NewSchema()
		{
			var file = embeddedResourceRetriever.SaveResourceToFile(ValidXmlFile_NewSchema);
			Director.Import(file, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			AssertEquals("Message", "1 statement(s) imported successfully.\r\n0 direct transaction(s) created.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("GetStatements.Length", 1, BankStatement.GetStatements_ForTestOnly().Count);

			Statement statement = BankStatement.GetStatements_ForTestOnly()[0];
			AssertEquals("AS_DebitCredit", Statement.CREDIT, statement.AS_DebitCredit);
			AssertEquals("AS_StatementDate", new ZDateTime(2006, 11, 6), statement.AS_StatementDate);
			AssertEquals("AS_ChequeOrReference", "Reference", statement.AS_ChequeOrReference);
			AssertEquals("AS_Amount", 1250.85m, statement.AS_Amount);
			AssertEquals("AS_Type", ZArchitecture.Core.TransactionTypes.ReceiptBatch, statement.AS_Type);

			AssertNull("DirectTransaction", statement.DirectTransaction);
		}

		public void TestImportInvalidXmlFile_NewSchema()
		{
			var file = embeddedResourceRetriever.SaveResourceToFile(InvalidXmlFile_NewSchema);
			Director.Import(file, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			AssertEquals("Message", "There is more than one Bank Statement transaction in the XML file.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			AssertEquals("Message", "No statement imported.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestImportValidXmlFile()
		{
			var file = embeddedResourceRetriever.SaveResourceToFile(ValidXmlFile);
			Director.Import(file, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			AssertEquals("Message", "1 statement(s) imported successfully.\r\n0 direct transaction(s) created.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("GetStatements.Length", 1, BankStatement.GetStatements_ForTestOnly().Count);

			Statement statement = BankStatement.GetStatements_ForTestOnly()[0];
			AssertEquals("AS_DebitCredit", Statement.CREDIT, statement.AS_DebitCredit);
			AssertEquals("AS_StatementDate", new ZDateTime(2006, 11, 6), statement.AS_StatementDate);
			AssertEquals("AS_ChequeOrReference", "Reference", statement.AS_ChequeOrReference);
			AssertEquals("AS_Amount", 1250.85m, statement.AS_Amount);
			AssertEquals("AS_Type", ZArchitecture.Core.TransactionTypes.ReceiptBatch, statement.AS_Type);

			AssertNull("DirectTransaction", statement.DirectTransaction);
		}

		public void TestImportInvalidXmlFile_Format()
		{
			var file = embeddedResourceRetriever.SaveResourceToFile(InvalidXmlFile_Format);
			Director.Import(file, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			AssertEquals("Message", "The XML format of the file you tried to import was invalid.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			AssertEquals("Message", "No statement imported.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestImportInvalidXmlFile_Currency()
		{
			var file = embeddedResourceRetriever.SaveResourceToFile(InvalidXmlFile_Currency);
			Director.Import(file, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			AssertEquals("Message", "Statement currency cannot be different from the bank currency.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			AssertEquals("Message", "No statement imported.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestImportInvalidXmlFile_StatementDate()
		{
			var file = embeddedResourceRetriever.SaveResourceToFile(InvalidXmlFile_StatementDate);
			Director.Import(file, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			AssertEquals("Message", "Statement date cannot be after the bank's statement date.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			AssertEquals("Message", "No statement imported.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestImportInvalidXmlFile_Character()
		{
			var file = embeddedResourceRetriever.SaveResourceToFile(InvalidXmlFile_Character);
			Director.Import(file, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			AssertEquals("Message", "The XML format of the file you tried to import was invalid.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
			AssertEquals("Message", "No statement imported.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestImportNabFile()
		{
			Director = new BankStatementXmlDataTransferDirector(BankStatement, BankStatementFormat.StatementFileFormats.NABAustralia, Adapter, false);

			var file = embeddedResourceRetriever.SaveResourceToFile(NabFile);
			Director.Import(file, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			AssertEquals("Message", "9 statement(s) imported successfully.\r\n0 direct transaction(s) created.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("GetStatements.Length", 9, BankStatement.GetStatements_ForTestOnly().Count);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[0], Statement.CREDIT, new ZDateTime(2006, 11, 6), "REF", 1601.00m, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[1], Statement.CREDIT, new ZDateTime(2006, 11, 6), "REF", 342.54m, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[2], Statement.CREDIT, new ZDateTime(2006, 11, 6), "REF", 13790.26m, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[3], Statement.CREDIT, new ZDateTime(2006, 11, 6), "41", 28779.85m, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[4], Statement.CREDIT, new ZDateTime(2006, 11, 6), "REF", 2848.71m, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[5], Statement.CREDIT, new ZDateTime(2006, 11, 6), "REF", 2541.30m, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[6], Statement.CREDIT, new ZDateTime(2006, 11, 6), "REF", 130660.27m, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[7], Statement.DEBIT, new ZDateTime(2006, 11, 6), "123463", 10336.70m, ZArchitecture.Core.ReceiptTypes.Cheque);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[8], Statement.DEBIT, new ZDateTime(2006, 11, 6), "REF", 306005.94m, ZArchitecture.Core.ReceiptTypes.EFT);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);
		}

		public void TestImportANZFile()
		{
			Director = new BankStatementXmlDataTransferDirector(BankStatement, BankStatementFormat.StatementFileFormats.ANZNewZealand, Adapter, false);

			var file = embeddedResourceRetriever.SaveResourceToFile(ANZFile);
			Director.Import(file, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			AssertEquals("Message", "8 statement(s) imported successfully.\r\n0 direct transaction(s) created.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("GetStatements.Length", 8, BankStatement.GetStatements_ForTestOnly().Count);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[0], Statement.CREDIT, BankStatement.AB_LastStatementDate, "STRANDBAGS P", 54922.02m, ZArchitecture.Core.ReceiptTypes.DirectCredit);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[1], Statement.DEBIT, BankStatement.AB_LastStatementDate, "301125", 100.79m, ZArchitecture.Core.ReceiptTypes.Cheque);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[2], Statement.CREDIT, BankStatement.AB_LastStatementDate, "00023", 7007.83m, ZArchitecture.Core.ReceiptTypes.DirectCredit);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[3], Statement.CREDIT, BankStatement.AB_LastStatementDate, "000000000000", 437103.22m, ZArchitecture.Core.TransactionTypes.ReceiptBatch);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[4], Statement.DEBIT, BankStatement.AB_LastStatementDate, "1260", 10.00m, ZArchitecture.Core.ReceiptTypes.Cheque);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[5], Statement.DEBIT, BankStatement.AB_LastStatementDate, "INCL GST", 3038.73m, ZArchitecture.Core.ReceiptTypes.EFT);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[6], Statement.DEBIT, BankStatement.AB_LastStatementDate, "", 500000.00m, ZArchitecture.Core.ReceiptTypes.Cheque);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[7], Statement.DEBIT, BankStatement.AB_LastStatementDate, "", 266.99m, ZArchitecture.Core.ReceiptTypes.Cheque);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);
		}

		public void TestImportWestpacFile()
		{
			Director = new BankStatementXmlDataTransferDirector(BankStatement, BankStatementFormat.StatementFileFormats.WestpacNewZealand, Adapter, false);

			var file = embeddedResourceRetriever.SaveResourceToFile(WestpacFile);
			Director.Import(file, new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			AssertEquals("Message", "10 statement(s) imported successfully.\r\n0 direct transaction(s) created.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("GetStatements.Length", 10, BankStatement.GetStatements_ForTestOnly().Count);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[0], Statement.CREDIT, BankStatement.AB_LastStatementDate, "N PEN /RFB/0", 2747.90m, ZArchitecture.Core.ReceiptTypes.DirectCredit);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[1], Statement.CREDIT, BankStatement.AB_LastStatementDate, "", 6836.33m, ZArchitecture.Core.ReceiptTypes.DirectCredit);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[2], Statement.CREDIT, BankStatement.AB_LastStatementDate, "", 12566.76m, ZArchitecture.Core.ReceiptTypes.DirectCredit);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[3], Statement.DEBIT, BankStatement.AB_LastStatementDate, "", 112.50m, ZArchitecture.Core.ReceiptTypes.EFT);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[4], Statement.DEBIT, BankStatement.AB_LastStatementDate, "", 219.38m, ZArchitecture.Core.ReceiptTypes.EFT);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[5], Statement.DEBIT, BankStatement.AB_LastStatementDate, "", 225.00m, ZArchitecture.Core.ReceiptTypes.EFT);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[6], Statement.DEBIT, BankStatement.AB_LastStatementDate, "", 236.25, ZArchitecture.Core.ReceiptTypes.EFT);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[7], Statement.DEBIT, BankStatement.AB_LastStatementDate, "112510", 150.00m, ZArchitecture.Core.ReceiptTypes.Cheque);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[8], Statement.DEBIT, BankStatement.AB_LastStatementDate, "112511", 150.00m, ZArchitecture.Core.ReceiptTypes.Cheque);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);

			AssertStatement(BankStatement.GetStatements_ForTestOnly()[9], Statement.DEBIT, BankStatement.AB_LastStatementDate, "112550", 32835.57m, ZArchitecture.Core.ReceiptTypes.Cheque);
			AssertNull("DirectTransaction", BankStatement.GetStatements_ForTestOnly()[0].DirectTransaction);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BankStatement = Factory.NewWithValidTestData<Business.Base.AccStatement.BankStatement>();
			BankStatement.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			BankStatement.AB_LastReconcileDate = new ZDateTime(2006, 11, 7);
			BankStatement.AB_LastStatementDate = new ZDateTime(2006, 11, 6);
			Adapter = new BankStatementDataAdapter();
			Director = new BankStatementXmlDataTransferDirector(BankStatement, BankStatementFormat.StatementFileFormats.NativeXML, Adapter, false);

			embeddedResourceRetriever = new ();
		}

		protected override void TearDown()
		{
			base.TearDown();

			embeddedResourceRetriever?.Dispose();
			embeddedResourceRetriever = null;
		}

		void AssertStatement(Statement statement, string debitCredit, ZDateTime statementDate, string chequeOrReference, ZDecimal amount, string type)
		{
			AssertEquals("AS_DebitCredit", debitCredit, statement.AS_DebitCredit);
			AssertEquals("AS_StatementDate", statementDate, statement.AS_StatementDate);
			AssertEquals("AS_ChequeOrReference", chequeOrReference, statement.AS_ChequeOrReference);
			AssertEquals("AS_Amount", amount, statement.AS_Amount);
			AssertEquals("AS_Type", type, statement.AS_Type);
		}

		Business.Base.AccStatement.BankStatement BankStatement;
		BankStatementDataAdapter Adapter;
		BankStatementXmlDataTransferDirector Director;

		EmbeddedResourceRetriever embeddedResourceRetriever;

		const string ValidXmlFile_NewSchema = "ValidBankStatement_NewSchema.xml";
		const string ValidXmlFile = "ValidBankStatement.xml";
		const string InvalidXmlFile_NewSchema = "InvalidBankStatement_NewSchema.xml";
		const string InvalidXmlFile_Format = "InvalidBankStatement_Format.xml";
		const string InvalidXmlFile_Currency = "InvalidBankStatement_Currency.xml";
		const string InvalidXmlFile_StatementDate = "InvalidBankStatement_StatementDate.xml";
		const string InvalidXmlFile_Character = "InvalidBankStatement_Character.xml";
		const string NabFile = "NabBankStatement.csv";
		const string ANZFile = "ANZ_NZ_BankStatement_ForTest.csv";
		const string WestpacFile = "WestpacNZBankStatement.csv";

		#endregion
	}
}
