using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.DataTransfer.BankStatement;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Testing.BankStatement
{
	[TestedType(typeof(BankStatementDataAdapter))]
	sealed class BankStatementDataAdapterTest : BaseAccountingDataAdapterTest<Business.Base.AccStatement.BankStatement, Xsd.BankStatement>
	{
		public void TestImportFromValueObjectCore()
		{
			Xsd.BankStatementBankStatementLine line = Value.BankStatementLines[0];

			Adapter.ImportFromValueObjectCore_ForTestOnly(BankStatement, Value, Context);
			AssertEquals("Statements.Count", 1, BankStatement.GetStatements_ForTestOnly().Count);

			Statement statement = BankStatement.GetStatements_ForTestOnly()[0];
			AssertEquals("AS_DebitCredit", Statement.CREDIT, statement.AS_DebitCredit);
			AssertEquals("AS_StatementDate", line.StatementDate, statement.AS_StatementDate);
			AssertEquals("AS_ChequeOrReference", line.ChequeOrReferenceNumber, statement.AS_ChequeOrReference);
			AssertEquals("AS_Amount", Math.Abs(line.TransactionAmount), statement.AS_Amount);
			AssertEquals("AS_Type", line.TransactionType, statement.AS_Type);

			DirectTransactionHeaderBase directTransaction = statement.DirectTransaction;
			AssertNotNull("DirectTransaction", directTransaction);
			AssertEquals("AH_Desc", line.TransactionDescription, directTransaction.AH_Desc);
			AssertEquals("AH_ChequeDrawer", line.PayeeName, directTransaction.AH_ChequeDrawer);
			AssertEquals("AH_InvoiceDate", line.TransactionDate, directTransaction.AH_InvoiceDate);
			AssertEquals("AH_PostDate", line.TransactionDate, directTransaction.AH_PostDate);
			AssertEquals("AH_RX_NKTransactionCurrency", BankStatement.AB_RX_NKAccountCurrency, directTransaction.AH_RX_NKTransactionCurrency);
		}

		public void TestImportFromValueObjectCore_WithBlankFields()
		{
			Value.BankStatementLines.Clear();
			SetupXmlWithOutSomeData();
			Xsd.BankStatementBankStatementLine line = Value.BankStatementLines[0];

			BankStatement.AB_LastStatementDate = ZDateTime.Now;

			Adapter.ImportFromValueObjectCore_ForTestOnly(BankStatement, Value, Context);
			AssertEquals("Statements.Count", 1, BankStatement.GetStatements_ForTestOnly().Count);

			Statement statement = BankStatement.GetStatements_ForTestOnly()[0];
			AssertEquals("AS_DebitCredit", Statement.DEBIT, statement.AS_DebitCredit);
			AssertEquals("AS_StatementDate", BankStatement.AB_LastStatementDate, statement.AS_StatementDate);
			AssertEquals("AS_ChequeOrReference", line.ChequeOrReferenceNumber, statement.AS_ChequeOrReference);
			AssertEquals("AS_Amount", Math.Abs(line.TransactionAmount), statement.AS_Amount);
			AssertEquals("AS_Type", line.TransactionType, statement.AS_Type);

			DirectTransactionHeaderBase directTransaction = statement.DirectTransaction;
			AssertNotNull("DirectTransaction", directTransaction);
			AssertEquals("AH_Desc", line.TransactionDescription, directTransaction.AH_Desc);
			AssertEquals("AH_ChequeDrawer", line.PayeeName, directTransaction.AH_ChequeDrawer);
			AssertEquals("AH_InvoiceDate", statement.AS_StatementDate, directTransaction.AH_InvoiceDate);
			AssertEquals("AH_PostDate", statement.AS_StatementDate, directTransaction.AH_PostDate);
			AssertEquals("AH_RX_NKTransactionCurrency", BankStatement.AB_RX_NKAccountCurrency, directTransaction.AH_RX_NKTransactionCurrency);
		}

		public void TestAddErrorsToNotifications()
		{
			Adapter.ImportFromValueObjectCore_ForTestOnly(BankStatement, Value, Context);
			AssertEquals("Statements.Count", 1, BankStatement.GetStatements_ForTestOnly().Count);

			Statement statement = BankStatement.GetStatements_ForTestOnly()[0];
			AssertEquals("HasErrors", false, statement.AS_TypeInfo.HasErrors());

			Value.BankStatementLines[0].TransactionType = "XXX";
			BankStatement.GetStatements_ForTestOnly().RemoveAndDeleteAll();
			Adapter.ImportFromValueObjectCore_ForTestOnly(BankStatement, Value, Context);
			AssertEquals("Statements.Count", 1, BankStatement.GetStatements_ForTestOnly().Count);

			statement = BankStatement.GetStatements_ForTestOnly()[0];
			AssertEquals("HasErrors", true, statement.AS_TypeInfo.HasErrors());
			AssertEquals("Buffer contains error", true, Buffer.AsString.Contains(statement.AS_TypeInfo.GetErrors().GetFirstMessage()));
		}

		#region Overrides

		protected override bool IsExportToValueObjectSupported
		{
			get { return false; }
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return true; }
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return Adapter.RootCollectionElementName; }
		}

		protected override string ExpectedRootElementName
		{
			get { return Adapter.RootElementName; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return null;
		}

		protected override ValueObjectDataAdapter<Business.Base.AccStatement.BankStatement, Xsd.BankStatement> GetNewBizObjXmlDataAdapter()
		{
			return new BankStatementDataAdapter();
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BankStatement = Factory.NewWithValidTestData<Business.Base.AccStatement.BankStatement>();
			Value = new Xsd.BankStatement();
			SetupXmlWithCorrectData();
			Buffer = new NotificationBuffer();
			Context = new ValueObjectImportContext(Factory, Buffer);
			Adapter = new BankStatementDataAdapter();
		}

		void SetupXmlWithCorrectData()
		{
			Xsd.BankStatementBankStatementLine line = Value.BankStatementLines.AddNew();
			line.TransactionType = ZArchitecture.Core.ReceiptTypes.AccountMaintenanceFee;
			line.ChequeOrReferenceNumber = "Reference";
			line.TransactionDescription = "Description";
			line.PayeeName = "PayeeName";
			line.TransactionDate = new ZDateTime(2006, 10, 9);
			line.StatementDate = new ZDateTime(2006, 10, 10);
			line.TransactionAmount = -1250.85m;
			line.CurrencyCode = Core.Constants.CurrencyCodes.Australia;
		}

		void SetupXmlWithOutSomeData()
		{
			Xsd.BankStatementBankStatementLine line = Value.BankStatementLines.AddNew();
			line.TransactionType = ZArchitecture.Core.ReceiptTypes.AccountMaintenanceFee;
			line.ChequeOrReferenceNumber = "";
			line.TransactionDescription = "SomeDescription";
			line.PayeeName = "";
			line.TransactionDate = ZDateTime.Empty;
			line.StatementDate = ZDateTime.Empty;
			line.TransactionAmount = 0m;
			line.CurrencyCode = "";
		}

		Business.Base.AccStatement.BankStatement BankStatement;
		Xsd.BankStatement Value;
		NotificationBuffer Buffer;
		ValueObjectImportContext Context;
		BankStatementDataAdapter Adapter;

		#endregion
	}
}
