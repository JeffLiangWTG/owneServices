using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.TaxFramework
{
	class AccountingTransactionExportGetTaxTransactionsTest : ScriptTest
	{
		public void TestSingleTransactionExport_GetTaxTransactions_WithoutDuplicatedRecordsByGlMovement()
		{
			SetupCommonData();
			var taxTransactionPER = GetTaxTransaction(Invoice, TaxSystemPER, TaxConfigPER, TaxBasisList.Matching.Code, 200M, 200M, 4M, 4M);
			TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxTransactionPER.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(InvoiceLine));
			CreateGLMovement(taxTransactionPER.PK, 4M, TaxGLMovementTypeList.Pending.Code);
			CreateGLMovement(taxTransactionPER.PK, 5M, TaxGLMovementTypeList.Realised.Code);
			Factory.Save();

			AssertGreaterThan("Precondition: GL Movement per tax transaction", GetGLMovements(taxTransactionPER.PK).Length, 1);

			AssertAccountingTransactionExportGetTaxTransactions(Invoice.PK, 1, new[] { taxTransactionPER.PK });
		}

		public void TestSingleTransactionExport_GetTaxTransactions_WithoutDuplicatedRecordsByTransactionHeader()
		{
			SetupCommonData();
			var taxTransactionPER = GetTaxTransaction(Invoice, TaxSystemPER, TaxConfigPER, TaxBasisList.Posting.Code, 200M, 200M, 4M, 4M);
			var taxTransactionSPR = GetTaxTransaction(Invoice, TaxSystemSPR, TaxConfigSPR, TaxBasisList.PostingOnMatching.Code, 300M, 300M, -45M, -45M);
			TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxTransactionSPR.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(InvoiceLine));
			TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxTransactionPER.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(InvoiceLine));
			CreateGLMovement(taxTransactionPER.PK, 5M, TaxGLMovementTypeList.Normal.Code);
			TestObjectCreator.CreateInvoiceLine(TestObjectCreator.CreateInvoice(typeof(APInvoice), "10010011"), 200M);
			TestObjectCreator.CreateInvoiceLine(TestObjectCreator.CreateInvoice(typeof(APInvoice), "10010012"), 300M);
			Factory.Save();

			AssertEquals("Precondition: GL Movement for PER tax transaction", 1, GetGLMovements(taxTransactionPER.PK).Length);
			AssertEquals("Precondition: GL Movement for SPR transaction", 0, GetGLMovements(taxTransactionSPR.PK).Length);
			AssertGreaterThan("Precondition: Number of Transactions in db to test duplications", Factory.GetDatabaseCount(typeof(AccTransactionHeader)), 1);

			AssertAccountingTransactionExportGetTaxTransactions(Invoice.PK, 2, new[] { taxTransactionPER.PK, taxTransactionSPR.PK });
		}

		public void TestSingleTransactionExport_GetTaxTransactions_TransactionWithGLMovements()
		{
			SetupCommonData();
			var taxTransactionPER = GetTaxTransaction(Invoice, TaxSystemPER, TaxConfigPER, TaxBasisList.Posting.Code, 200M, 200M, 4M, 4M);
			TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxTransactionPER.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(InvoiceLine));
			CreateGLMovement(taxTransactionPER.PK, 4M, TaxGLMovementTypeList.Normal.Code);
			Factory.Save();

			AssertEquals("Precondition: GL Movements for a tax transaction", 1, GetGLMovements(taxTransactionPER.PK).Length);

			AssertAccountingTransactionExportGetTaxTransactions(Invoice.PK, 1, new[] { taxTransactionPER.PK });
		}

		public void TestSingleTransactionExport_GetTaxTransactions_TransactionWithoutGLMovements()
		{
			SetupCommonData();
			var taxTransactionSPR = GetTaxTransaction(Invoice, TaxSystemSPR, TaxConfigSPR, TaxBasisList.PostingOnMatching.Code, 300M, 300M, -45M, -45M);
			TaxFrameworkTestObjectCreator.CreateTaxTransactionLinePivot(taxTransactionSPR.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(InvoiceLine));
			Factory.Save();

			AssertEquals("Precondition: Number of GL Movements in db", 0, Factory.GetDatabaseCount(typeof(AccTaxGLMovement)));

			AssertAccountingTransactionExportGetTaxTransactions(Invoice.PK, 1, new[] { taxTransactionSPR.PK });
		}

		void AssertAccountingTransactionExportGetTaxTransactions(ZGuid invoicePK, int expectedCount, ZGuid[] expectedTaxTransactions)
		{
			var result = RunScript(invoicePK);
			AssertEquals("Tax Transaction records", expectedCount, result.Rows.Count);

			var actualTaxTransactions = result.Rows.Cast<DataRow>().Select(x => x["ATT_PK"]).Cast<Guid>().ToList();
			AssertContainsExactElementsInAnyOrder(expectedTaxTransactions, actualTaxTransactions);
		}

		DataTable RunScript(ZGuid transactionHeaderPK)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, $"EXEC AccountingTransactionExportGetTaxTransactionsSingle '{transactionHeaderPK}'");
		}

		AccTaxGLMovement[] GetGLMovements(ZGuid taxTransactionPK) => Factory.Load<AccTaxGLMovement>(new ZQuery(AccTaxGLMovementSchema.ATM_ATT_TaxTransaction, taxTransactionPK));

		void CreateGLMovement(ZGuid taxtransactionPK, ZDecimal amount, string taxGLMovementType)
		{
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement
			(
				taxTransactionPK: taxtransactionPK,
				amount: amount,
				type: taxGLMovementType
			);
		}

		AccTaxTransaction GetTaxTransaction(AccTransactionHeader invoice, TaxSystemsConfiguration taxSystem, AccTaxConfiguration taxConfig, ZString taxBasis,
											ZDecimal osTaxBaseAmount, ZDecimal localTaxBaseAmount, ZDecimal osTaxAmount, ZDecimal localTaxAmount)
		{
			return TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters
			{
				TransactionHeader = invoice,
				TaxSystem = taxSystem,
				TaxConfiguration = taxConfig,
				TaxBasis = taxBasis,
				OsTaxBaseAmount = osTaxBaseAmount,
				LocalTaxBaseAmount = localTaxBaseAmount,
				OsTaxAmount = osTaxAmount,
				LocalTaxAmount = localTaxAmount,
				AffectsSourceTransactionTotal = false,
				DoesNotCreateGLMovemetsOnSaving = true,
			});
		}

		void SetupCommonData()
		{
			company = GlbCompany.CurrentCompany;
			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N01AU");
			TaxSystemPER = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUPER", taxSuperType: TaxSuperTypeList.Perceptions.Code);
			TaxSystemSPR = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);

			TaxConfigPER = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, TaxSystemPER, TaxConfigurationLedgers.AccountsPayable.Code);
			TaxConfigSPR = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, TaxSystemSPR, TaxConfigurationLedgers.AccountsPayable.Code);

			Invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "10010010", organisation: TestObjectCreator.Creditor1);
			InvoiceLine = TestObjectCreator.CreateInvoiceLine(Invoice, 100M);
		}

		AccTaxConfiguration TaxConfigPER, TaxConfigSPR;
		TaxSystemsConfiguration TaxSystemPER, TaxSystemSPR;
		InvoicingBase Invoice;
		InvoicingLineBase InvoiceLine;
		GlbCompany company;

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator => taxFrameworkTestObjectCreator ??= new TaxFrameworkTestObjectCreator(Factory);
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;
	}
}
