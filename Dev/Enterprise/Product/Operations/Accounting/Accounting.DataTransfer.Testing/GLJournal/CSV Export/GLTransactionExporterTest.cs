using System;
using System.IO;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	sealed class GLTransactionExporterTest : TestCaseWithFactory
	{
		#region Direct Payment/Receipt

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestDirectPayment()
		{
			AssertExportData(testObjectCreator => testObjectCreator.CreateDirectPayment(ZDateTime.Now, 100, 10, 50, 5));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestDirectPayment_VATRecoverable()
		{
			AssertExportData(testObjectCreator =>
				{
					var transaction = testObjectCreator.CreateDirectPayment(ZDateTime.Now, 100, 10, 50, 5);
					transaction.Lines[0].AL_InputGSTVATRecoverable = 0.70m;
					transaction.Lines[1].AL_InputGSTVATRecoverable = 0.60m;

					return transaction;
				}, "_VATRecoverable");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestDirectReceipt()
		{
			AssertExportData(testObjectCreator => testObjectCreator.CreateDirectReceipt(ZDateTime.Now, 100, 10, 50, 5));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestDirectReceipt_VATRecoverableNotApplicable()
		{
			AssertExportData(testObjectCreator =>
				{
					var transaction = testObjectCreator.CreateDirectReceipt(ZDateTime.Now, 100, 10, 50, 5);
					transaction.Lines[0].AL_InputGSTVATRecoverable = 0.70m;
					transaction.Lines[1].AL_InputGSTVATRecoverable = 0.60m;

					return transaction;
				});
		}

		#endregion

		#region Invoices

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestAPInvoice()
		{
			AssertExportData(testObjectCreator =>
			{
				var transaction = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, 1);
				var line = testObjectCreator.CreateInvoiceLine(transaction, testObjectCreator.AUD, 1, 100, 10, 0, testObjectCreator.GLHeader1.PK);

				return transaction;
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestARInvoice()
		{
			AssertExportData(testObjectCreator =>
			{
				var transaction = testObjectCreator.CreateInvoice(typeof(ARInvoice), testObjectCreator.AUD, 1);
				var line = testObjectCreator.CreateInvoiceLine(transaction, testObjectCreator.AUD, 1, 100, 10, 0, testObjectCreator.GLHeader1.PK);

				return transaction;
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestAPCreditNote()
		{
			AssertExportData(testObjectCreator =>
			{
				var transaction = testObjectCreator.CreateInvoice(typeof(APCreditNote), testObjectCreator.AUD, 1);
				var line = testObjectCreator.CreateInvoiceLine(transaction, testObjectCreator.AUD, 1, 100, 10, 0, testObjectCreator.GLHeader1.PK);

				return transaction;
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestARCreditNote()
		{
			AssertExportData(testObjectCreator =>
			{
				var transaction = testObjectCreator.CreateInvoice(typeof(ARCreditNote), testObjectCreator.AUD, 1);
				var line = testObjectCreator.CreateInvoiceLine(transaction, testObjectCreator.AUD, 1, 100, 10, 0, testObjectCreator.GLHeader1.PK);

				return transaction;
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestAPInvoice_VATRecoverable()
		{
			AssertExportData(testObjectCreator =>
			{
				var transaction = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, 1);
				var line = testObjectCreator.CreateInvoiceLine(transaction, testObjectCreator.AUD, 1, 100, 10, 0, testObjectCreator.GLHeader1.PK);
				line.AL_InputGSTVATRecoverable = 0.80m;

				return transaction;
			}, "_VATRecoverable");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestARInvoice_VATRecoverableNotApplicable()
		{
			AssertExportData(testObjectCreator =>
			{
				var transaction = testObjectCreator.CreateInvoice(typeof(ARInvoice), testObjectCreator.AUD, 1);
				var line = testObjectCreator.CreateInvoiceLine(transaction, testObjectCreator.AUD, 1, 100, 10, 0, testObjectCreator.GLHeader1.PK);
				line.AL_InputGSTVATRecoverable = 0.80m;

				return transaction;
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestAPCreditNote_VATRecoverable()
		{
			AssertExportData(testObjectCreator =>
			{
				var transaction = testObjectCreator.CreateInvoice(typeof(APCreditNote), testObjectCreator.AUD, 1);
				var line = testObjectCreator.CreateInvoiceLine(transaction, testObjectCreator.AUD, 1, 100, 10, 0, testObjectCreator.GLHeader1.PK);
				line.AL_InputGSTVATRecoverable = 0.80m;

				return transaction;
			}, "_VATRecoverable");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestARCreditNote_VATRecoverableNotApplicable()
		{
			AssertExportData(testObjectCreator =>
			{
				var transaction = testObjectCreator.CreateInvoice(typeof(ARCreditNote), testObjectCreator.AUD, 1);
				var line = testObjectCreator.CreateInvoiceLine(transaction, testObjectCreator.AUD, 1, 100, 10, 0, testObjectCreator.GLHeader1.PK);
				line.AL_InputGSTVATRecoverable = 0.80m;

				return transaction;
			});
		}

		#endregion

		#region CashBasisVAT

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExportDataWithCashBasisVAT_ARInvoice()
		{
			AssertExportDataWithCashBasisVAT(typeof(ARInvoice));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExportDataWithCashBasisVAT_ARCreditNote()
		{
			AssertExportDataWithCashBasisVAT(typeof(ARCreditNote));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExportDataWithCashBasisVAT_APInvoice()
		{
			AssertExportDataWithCashBasisVAT(typeof(APInvoice));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExportDataWithCashBasisVAT_APCreditNote()
		{
			AssertExportDataWithCashBasisVAT(typeof(APCreditNote));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExportDataWithCashBasisVAT_ARInvoice_VATRecoverableNotApplicable()
		{
			AssertExportDataWithCashBasisVAT(typeof(ARInvoice), true, false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExportDataWithCashBasisVAT_ARCreditNote_VATRecoverableNotApplicable()
		{
			AssertExportDataWithCashBasisVAT(typeof(ARCreditNote), true, false);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExportDataWithCashBasisVAT_APInvoice_VATRecoverable()
		{
			AssertExportDataWithCashBasisVAT(typeof(APInvoice), true, true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExportDataWithCashBasisVAT_APCreditNote_VATRecoverable()
		{
			AssertExportDataWithCashBasisVAT(typeof(APCreditNote), true, true);
		}

		void AssertExportDataWithCashBasisVAT(Type invoiceType, bool useVATRecoverable = false, bool vATRecoverableApplicable = false)
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			PeriodTestHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));
			ZGuid glAccountFrom = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, "1000.00.00")).PK;
			ZGuid glAccountTo = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, "9999.00.00")).PK;
			ZGuid glAccount = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, SQLComparisonOperator.Equal, "4710.00.00")).PK;
			AccGLHeader apSuspenseControl = testObjectCreator.CreateAccGLHeader("6999.10.10", "TS", "AP Suspense", "BSH", "CR");
			AccGLHeader arSuspenseControl = testObjectCreator.CreateAccGLHeader("8999.10.10", "TS", "AR Suspense", "BSH", "DR");
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apSuspenseControl.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arSuspenseControl.PK.ToGuid());
			var pendingInputTaxAccount = testObjectCreator.CreateInputTaxReceivablePendingAccount();
			var pendingOutputTaxAccount = testObjectCreator.CreateOutputTaxPayablePendingAccount();
			AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingInputTaxAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingOutputTaxAccount.PK.ToGuid());

			testObjectCreator.GLHeader1.AG_Description = "GL Header 1";
			testObjectCreator.GLHeader1.AG_AccountNum = "GL 1 Num";
			var invoice = testObjectCreator.CreateInvoiceWithCashVATLine(invoiceType, 100, 10, departmentCode: "FIP");
			invoice.AH_PostToGL = "Y";
			invoice.AH_TransactionNum = "Export Test";

			if (useVATRecoverable)
			{
				invoice.Lines[0].AL_InputGSTVATRecoverable = 0.80m;
			}

			Factory.Save();

			GLTransactionBusinessObject bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.FromPeriod = 200603;
			bizObj.ToPeriod = 200603;
			bizObj.StartGLAccountPK = glAccountFrom;
			bizObj.EndGLAccountPK = glAccountTo;
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.DescriptionDisplay = "H";
			bizObj.CreateAndExportBatch = true;
			bizObj.ExportExistingBatch = false;
			bizObj.BatchNumber = 7;

			var expectedFileName = string.Format("CashBasisVATBatch{0}{1}", invoice.AH_Ledger, invoice.AH_TransactionType);
			AssertExportWithCashBasisVAT(bizObj, expectedFileName, "");

			var exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
			bizObj.CreateAndExportBatch = true;
			bizObj.ExportExistingBatch = false;
			bizObj.BatchNumber = 8;
			Assert("Nothing to export here", !exporter.ExportData());

			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = true;
			bizObj.BatchNumber = 7;
			AssertExportWithCashBasisVAT(bizObj, expectedFileName, "");

			if ((invoice.AH_Ledger == "AP" && invoice.AH_TransactionType != "CRD") || (invoice.AH_Ledger == "AR" && invoice.AH_TransactionType == "CRD"))
			{
				testObjectCreator.CreateMatchLinkToPayAPInvoice(invoice, ZDateTime.Today.AddDays(-3), -40, "M001");
			}
			else
			{
				testObjectCreator.CreateMatchLinkToPayARInvoice(invoice, ZDateTime.Today.AddDays(-3), 40, "M001");
			}

			Factory.Save();

			bizObj.CreateAndExportBatch = true;
			bizObj.ExportExistingBatch = false;
			bizObj.BatchNumber = 8;
			Assert("Nothing to export here", !exporter.ExportData());

			TestConnection.ExecuteNonQuery("DELETE FROM dbo.AccCashBasisVATQueue");
			string expectedFileName2 = string.Format("CashBasisVATBatchAfterMatching{0}{1}", invoice.AH_Ledger, invoice.AH_TransactionType);
			var vatRecoverableFileNameSuffix = "";
			if (useVATRecoverable && vATRecoverableApplicable)
			{
				vatRecoverableFileNameSuffix = "_VATRecoverable"; //a difference in exported data with VATRecoverable set should be only after matching
			}
			AssertExportWithCashBasisVAT(bizObj, expectedFileName2, vatRecoverableFileNameSuffix);

			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = true;
			bizObj.BatchNumber = 7;
			AssertExportWithCashBasisVAT(bizObj, expectedFileName, "");

			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = true;
			bizObj.BatchNumber = 8;
			AssertExportWithCashBasisVAT(bizObj, expectedFileName2, vatRecoverableFileNameSuffix);

			bizObj.BatchNumber = 0;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = false;

			expectedFileName = string.Format("CashBasisVAT{0}{1}", invoice.AH_Ledger, invoice.AH_TransactionType);
			AssertExportWithCashBasisVAT(bizObj, expectedFileName, vatRecoverableFileNameSuffix);

			testObjectCreator.CreateMatchLinkToPayARInvoice(invoice, ZDateTime.Today.AddDays(-2), 30, "M002");
			Factory.Save();
			AssertExportWithCashBasisVAT(bizObj, expectedFileName, vatRecoverableFileNameSuffix);

			TestConnection.ExecuteNonQuery("DELETE FROM dbo.AccCashBasisVATQueue");
			AssertExportWithCashBasisVAT(bizObj, string.Format("CashBasisVATAfterMatching{0}{1}", invoice.AH_Ledger, invoice.AH_TransactionType), vatRecoverableFileNameSuffix);

			bizObj.DescriptionDisplay = "L";
			AssertExportWithCashBasisVAT(bizObj, string.Format("CashBasisVATForLines{0}{1}", invoice.AH_Ledger, invoice.AH_TransactionType), vatRecoverableFileNameSuffix);
		}

		void AssertExportWithCashBasisVAT(GLTransactionBusinessObject bizObj, string expectedFileName, string expectedFileNameSuffix)
		{
			AssertExport(bizObj, expectedFileName, expectedFileNameSuffix, "CashBasisVAT");
		}

		#endregion

		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExportData()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			PeriodTestHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));
			ZGuid gLAccountFrom = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, "1000.00.00")).PK;
			ZGuid gLAccountTo = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, "9999.00.00")).PK;
			AccGLHeader aPSuspenseControl = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "7200.40.50"));
			AccGLHeader glAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, SQLComparisonOperator.Equal, "4710.00.00"));

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			Invoice invoice = (Invoice)testObjectCreator.CreateInvoice(typeof(APInvoice), GlbCompany.CurrentCompany.LocalCurrency, GlbCompany.CurrentCompany.LocalCurrency.CurrentBuyRate);

			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControl.PK.ToGuid());

			invoice.AH_InvoiceAmount = 333m;
			invoice.AH_AG = glAccount.PK;
			invoice.AH_PostDate = new ZDateTime(2006, 3, 10);
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";
			invoice.AH_PostToGL = "Y";

			InvoiceLine invoiceLine = (InvoiceLine)testObjectCreator.CreateInvoiceLine(invoice, GlbCompany.CurrentCompany.LocalCurrency, GlbCompany.CurrentCompany.LocalCurrency.CurrentBuyRate, 333m);

			invoiceLine.AL_AG = glAccount.PK;
			invoiceLine.AL_LineAmount = 333m;
			invoiceLine.AL_OSAmount = 333m;
			invoiceLine.AL_PostDate = new ZDateTime(2006, 3, 10);
			AssertNotNull("reverse should be set when post is set due to new reve recognition", invoiceLine.AL_ReverseDate);

			testObjectCreator.Factory.Save();

			GLTransactionBusinessObject bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.FromPeriod = 200603;
			bizObj.ToPeriod = 200603;
			bizObj.StartGLAccountPK = gLAccountFrom;
			bizObj.EndGLAccountPK = gLAccountTo;
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;

			string exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942.csv");

			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));

				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Expected export file should exist", true, File.Exists(exportFileName));

				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("\"TransactionType\",\"InvoiceDate\",\"PostDate\",\"DueDate\",\"Branch\",\"Department\",\"Ledger\",\"TransactionNum\",\"TransactionDesc\",\"GLAccount\",\"GLAccountDesc\",\"Job\",\"Account\",\"ChargeCode\",\"Period\",\"ReversePeriod\",\"Amount\",\"GSTAmount\",\"Debit\",\"Credit\",\"Balance\",\"IsLine\",\"TRPK\",\"OpeningPeriodDate\",\"ClosingPeriodDate\",\"OpeningBalance\",\"ClosingBalance\",\"IsControlTotal\",\"BankCode\",\"TaxID\",\"TaxIDType\",\"TaxIDRate\",\"TransactionDebtorOrCreditorGroup\",\"JobLocalClient\",\"JobLocalClientDebtorGroup\",\"TransactionDebtorExternalCreditRatingCode\",\"OriginalTransactionNumberForReversals\",\"OriginalTransactionNumberConsolidatedInvoiceRef\",\"TransactionNumberConsolidatedInvoiceRef\",\"Unused1\",\"Unused2\",\"TransactionHeaderBranch\",\"TransactionHeaderDepartment\",\"TransactionLineBranchOrgProxyCode\",\"TransactionHeaderBranchOrgProxyCode\",\"WasImportedFromExternalSystem\",\"PaymentReferenceNumber\",\"Currency\",\"ExRate\",\"OsAmount\",\"OsDebit\",\"OsCredit\",\"SubAccounts\",\"OrganisationSubAccount\",\"SalesExpenseGroupsSubAccount\",\"StaffAndResourcesSubAccount\",\"StaffGroupSubAccount\",\"Units\"\r\n\"\",\"\",\"\",\"\",\"\",\"\",\"AP \",\"\",\"AP Control Account\",\"8210.00.00\",\"TRADE CREDITORS CONTROL\",\"\",\"\",\"\",\"200603\",\"\",\"333.0000\",\"0.0000\",\"333.0000\",\"\",\"0.0000\",\"False\",\"\",\"1/03/2006 12:00:00 AM\",\"31/03/2006 11:59:00 PM\",\"0.0000\",\"0.0000\",\"True\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"AUD\",\"1.000000\",\"0.0000\",\"0.0000\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n\"INV\",\"29/03/2006 3:39:00 PM\",\"10/03/2006 12:00:00 AM\",\"29/03/2006 3:39:00 PM\",\"BNE\",\"BRN\",\"AP \",\"" + invoice.AH_TransactionNum + "\",\"Test Invoice\",\"6310.00.00\",\"INPUT TAX RECEIVABLE\",\"\",\"\",\"\",\"200603\",\"\",\"0.0000\",\"0.0000\",\"\",\"\",\"0.0000\",\"False\",\"@PK\",\"1/03/2006 12:00:00 AM\",\"31/03/2006 11:59:00 PM\",\"0.0000\",\"0.0000\",\"False\",\"          \",\"ZZGST1    \",\"RAT\",\"10\",\"\",\"\",\"\",\"\",\"\",\"\",\"00001000\",\"0\",\"0\",\"BNE\",\"BRN\",\"EDICUS      \",\"EDICUS      \",\"N\",\"\",\"AUD\",\"1.000000\",\"0.0000\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n\"INV\",\"29/03/2006 3:39:00 PM\",\"10/03/2006 12:00:00 AM\",\"29/03/2006 3:39:00 PM\",\"BNE\",\"BRN\",\"AP \",\"" + invoice.AH_TransactionNum + "\",\"Test Invoice\",\"@AccountNum\",\"@AccountDescription\",\"\",\"\",\"\",\"200603\",\"\",\"-333.0000\",\"0.0000\",\"\",\"333.0000\",\"0.0000\",\"False\",\"\",\"1/03/2006 12:00:00 AM\",\"31/03/2006 11:59:00 PM\",\"0.0000\",\"0.0000\",\"False\",\"          \",\"ZZGST1    \",\"RAT\",\"10\",\"\",\"\",\"\",\"\",\"\",\"\",\"00001000\",\"0\",\"0\",\"BNE\",\"BRN\",\"EDICUS      \",\"EDICUS      \",\"N\",\"\",\"AUD\",\"0.000000\",\"-333.0000\",\"\",\"333.0000\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n");
				stringBuilder.Replace("@AccountDescription", aPSuspenseControl.AG_Description);
				stringBuilder.Replace("@AccountNum", aPSuspenseControl.AG_AccountNum);
				stringBuilder.Replace("@PK", invoiceLine.PK.ToString());
				string expectedExportData = stringBuilder.ToString();
				string actualExportData = File.ReadAllText(exportFileName);

				AssertMultilineASCIIEquals("Exporter is not exporting what is expected", expectedExportData, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}
		}

		[TestDate(2020, 03, 23, 16, 38, 41)]
		public void TestExportDataForMultiSubAccounts()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			PeriodTestHelper.SetupSinglePeriod(202003, new ZDateTime(2020, 3, 1), new ZDateTime(2020, 3, 31, 23, 59, 00));
			ZGuid gLAccountFrom = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, "1000.00.00")).PK;
			ZGuid gLAccountTo = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, "9999.00.00")).PK;
			AccGLHeader glAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, SQLComparisonOperator.Equal, "4710.00.00"));

			var arInvoice = testObjectCreator.CreateInvoice(typeof(ARInvoice), "ARINV1", testObjectCreator.AUD, 1, testObjectCreator.ABIGAS);
			arInvoice.AH_PostToGL = "Y";
			var line = testObjectCreator.CreateInvoiceLine(arInvoice, testObjectCreator.AUD, 1, 10, 0, 0, glAccount.PK);
			line.AL_ReverseToGL = "Y";

			testObjectCreator.Factory.Save();

			CreateMutilSubAccounts();

			GLTransactionBusinessObject bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.FromPeriod = 202003;
			bizObj.ToPeriod = 202003;
			bizObj.StartGLAccountPK = gLAccountFrom;
			bizObj.EndGLAccountPK = gLAccountTo;
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;

			string exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20200323163841.csv");

			using (new DisposableAction(() => { DeleteIfExists(exportFileName); }))
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));

				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Expected export file should exist", true, File.Exists(exportFileName));

				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("\"TransactionType\",\"InvoiceDate\",\"PostDate\",\"DueDate\",\"Branch\",\"Department\",\"Ledger\",\"TransactionNum\",\"TransactionDesc\",\"GLAccount\",\"GLAccountDesc\",\"Job\",\"Account\",\"ChargeCode\",\"Period\",\"ReversePeriod\",\"Amount\",\"GSTAmount\",\"Debit\",\"Credit\",\"Balance\",\"IsLine\",\"TRPK\",\"OpeningPeriodDate\",\"ClosingPeriodDate\",\"OpeningBalance\",\"ClosingBalance\",\"IsControlTotal\",\"BankCode\",\"TaxID\",\"TaxIDType\",\"TaxIDRate\",\"TransactionDebtorOrCreditorGroup\",\"JobLocalClient\",\"JobLocalClientDebtorGroup\",\"TransactionDebtorExternalCreditRatingCode\",\"OriginalTransactionNumberForReversals\",\"OriginalTransactionNumberConsolidatedInvoiceRef\",\"TransactionNumberConsolidatedInvoiceRef\",\"Unused1\",\"Unused2\",\"TransactionHeaderBranch\",\"TransactionHeaderDepartment\",\"TransactionLineBranchOrgProxyCode\",\"TransactionHeaderBranchOrgProxyCode\",\"WasImportedFromExternalSystem\",\"PaymentReferenceNumber\",\"Currency\",\"ExRate\",\"OsAmount\",\"OsDebit\",\"OsCredit\",\"SubAccounts\",\"OrganisationSubAccount\",\"SalesExpenseGroupsSubAccount\",\"StaffAndResourcesSubAccount\",\"StaffGroupSubAccount\",\"Units\"\r\n\"INV\",\"23/03/2020 4:38:00 PM\",\"23/03/2020 4:38:00 PM\",\"23/03/2020 4:38:00 PM\",\"BNE\",\"BRN\",\"AR \",\"00001000\",\"Test Invoice\",\"4710.00.00\",\"EXTRAORDINARY ITEMS AFTER TAX\",\"\",\"ABIGAS      \",\"\",\"202003\",\"\",\"-10.0000\",\"0.0000\",\"\",\"10.0000\",\"0.0000\",\"False\",\"\",\"1/03/2020 12:00:00 AM\",\"31/03/2020 11:59:00 PM\",\"0.0000\",\"0.0000\",\"False\",\"          \",\"          \",\"   \",\"0\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"0\",\"0\",\"BNE\",\"BRN\",\"EDICUS      \",\"EDICUS      \",\"N\",\"\",\"AUD\",\"1.000000\",\"-10.0000\",\"\",\"10.0000\",\"ORG: AALSHI, SEG: SG1, STR: E, SGP: STF\",\"AALSHI\",\"SG1\",\"E\",\"STF\",\"\"\r\n");

				string expectedExportData = stringBuilder.ToString();
				string actualExportData = File.ReadAllText(exportFileName);

				AssertMultilineASCIIEquals("Exporter is not exporting what is expected", expectedExportData, actualExportData);
			}

			void CreateMutilSubAccounts()
			{
				var salesGroup = testObjectCreator.CreateSalesGroup("SG1");
				var staff = GlbStaff.CurrentUser;
				var staffGroup = testObjectCreator.CreateStaffGroup("STF");

				testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line.PK, Core.Constants.SubAccountType.Organization, testObjectCreator.AALSHI.PK);
				testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line.PK, Core.Constants.SubAccountType.SalesGroup, salesGroup.PK);
				testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line.PK, Core.Constants.SubAccountType.StaffAndResources, staff.PK);
				testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line.PK, Core.Constants.SubAccountType.StaffGroup, staffGroup.PK);
				Factory.Save();
			}
		}

		public void TestHeaderLineForExportCSV()
		{
			var exporter = new GLTransactionExporterForTest(new GLTransactionBusinessObject(Factory), new NotificationBuffer());
			var expectedString = "\"TransactionType\",\"InvoiceDate\",\"PostDate\",\"DueDate\",\"Branch\",\"Department\",\"Ledger\",\"TransactionNum\",\"TransactionDesc\",\"GLAccount\",\"GLAccountDesc\",\"Job\",\"Account\",\"ChargeCode\",\"Period\",\"ReversePeriod\",\"Amount\",\"GSTAmount\",\"Debit\",\"Credit\",\"Balance\",\"IsLine\",\"TRPK\",\"OpeningPeriodDate\",\"ClosingPeriodDate\",\"OpeningBalance\",\"ClosingBalance\",\"IsControlTotal\",\"BankCode\",\"TaxID\",\"TaxIDType\",\"TaxIDRate\",\"TransactionDebtorOrCreditorGroup\",\"JobLocalClient\",\"JobLocalClientDebtorGroup\",\"TransactionDebtorExternalCreditRatingCode\",\"OriginalTransactionNumberForReversals\",\"OriginalTransactionNumberConsolidatedInvoiceRef\",\"TransactionNumberConsolidatedInvoiceRef\",\"Unused1\",\"Unused2\",\"TransactionHeaderBranch\",\"TransactionHeaderDepartment\",\"TransactionLineBranchOrgProxyCode\",\"TransactionHeaderBranchOrgProxyCode\",\"WasImportedFromExternalSystem\",\"PaymentReferenceNumber\",\"Currency\",\"ExRate\",\"OsAmount\",\"OsDebit\",\"OsCredit\",\"SubAccounts\",\"OrganisationSubAccount\",\"SalesExpenseGroupsSubAccount\",\"StaffAndResourcesSubAccount\",\"StaffGroupSubAccount\",\"Units\"";
			AssertEquals(expectedString, exporter.HeadingLineExposed);
		}

		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestDSCIncludedInBatchExportReExportofExistingBatchAndNonBatchExport()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			PeriodTestHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));
			AccGLHeader aPSuspenseControl = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "7200.40.50"));
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControl.PK.ToGuid());

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			ARDiscount discount = Factory.New<ARDiscount>();
			discount.AH_PostDate = new DateTime(2006, 3, 15);
			discount.AH_PostToGL = "Y";
			discount.AH_InvoiceAmount = 100m;
			discount.AH_OSTotal = 100m;
			discount.AH_OutstandingAmount = 100m;
			discount.Validation.ValidateAll();
			AssertNoErrors(discount);

			PrepareMiscellaneousTransactionForSaving(discount);

			testObjectCreator.Factory.Save();

			string exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942 EDI batch 1000.csv");
			TestCaseHelper.ClearTable(GenExportBatchSequenceSchema.Constants.TableName);
			GLTransactionBusinessObject bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 1000;
			bizObj.CreateAndExportBatch = true;
			string expectedExportData = "";
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Something should have been exported but wasn't", true, File.Exists(exportFileName));
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("\"TransactionType\",\"InvoiceDate\",\"PostDate\",\"DueDate\",\"Branch\",\"Department\",\"Ledger\",\"TransactionNum\",\"TransactionDesc\",\"GLAccount\",\"GLAccountDesc\",\"Job\",\"Account\",\"ChargeCode\",\"Period\",\"ReversePeriod\",\"Amount\",\"GSTAmount\",\"Debit\",\"Credit\",\"Balance\",\"IsLine\",\"TRPK\",\"OpeningPeriodDate\",\"ClosingPeriodDate\",\"OpeningBalance\",\"ClosingBalance\",\"IsControlTotal\",\"BankCode\",\"TaxID\",\"TaxIDType\",\"TaxIDRate\",\"TransactionDebtorOrCreditorGroup\",\"JobLocalClient\",\"JobLocalClientDebtorGroup\",\"TransactionDebtorExternalCreditRatingCode\",\"OriginalTransactionNumberForReversals\",\"OriginalTransactionNumberConsolidatedInvoiceRef\",\"TransactionNumberConsolidatedInvoiceRef\",\"Unused1\",\"Unused2\",\"TransactionHeaderBranch\",\"TransactionHeaderDepartment\",\"TransactionLineBranchOrgProxyCode\",\"TransactionHeaderBranchOrgProxyCode\",\"WasImportedFromExternalSystem\",\"PaymentReferenceNumber\",\"Currency\",\"ExRate\",\"OsAmount\",\"OsDebit\",\"OsCredit\",\"SubAccounts\",\"OrganisationSubAccount\",\"SalesExpenseGroupsSubAccount\",\"StaffAndResourcesSubAccount\",\"StaffGroupSubAccount\",\"Units\"\r\n\"\",\"\",\"\",\"\",\"\",\"\",\"AR \",\"\",\"AR Control Account\",\"6210.00.00\",\"TRADE DEBTORS CONTROL\",\"\",\"\",\"\",\"200603\",\"\",\"100.0000\",\"0.0000\",\"100.0000\",\"\",\"0.0000\",\"False\",\"\",\"\",\"\",\"\",\"\",\"True\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"AUD\",\"1.000000\",\"0.0000\",\"0.0000\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n\"DSC\",\"29/03/2006 3:39:00 PM\",\"15/03/2006 12:00:00 AM\",\"\",\"BNE\",\"BRN\",\"AR \",\"00001000\",\"MATCH NO.\",\"3960.00.00\",\"DISCOUNT ALLOWED\",\"\",\"\",\"\",\"200603\",\"\",\"-100.0000\",\"0.0000\",\"\",\"100.0000\",\"0.0000\",\"False\",\"{0}\",\"\",\"\",\"\",\"\",\"False\",\"          \",\"          \",\"   \",\"0\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"0\",\"0\",\"BNE\",\"BRN\",\"\",\"EDICUS      \",\"N\",\"\",\"AUD\",\"1.000000\",\"100.0000\",\"\",\"-100.0000\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n");
				stringBuilder.Replace("{0}", discount.PK.ToString());
				expectedExportData = stringBuilder.ToString();
				string actualExportData = File.ReadAllText(exportFileName);
				AssertMultilineASCIIEquals("Exporter is not exporting what is expected", expectedExportData, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 1000;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = true;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert("Something should have been exported but wasn't", exporter.ExportData());
				AssertEquals("Expected export file should exist", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				AssertMultilineASCIIEquals("Exporter is not exporting what is expected", expectedExportData, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 0;
			bizObj.ExportExistingBatch = false;
			bizObj.CreateAndExportBatch = false;
			bizObj.FromPeriod = 200603;
			bizObj.ToPeriod = 200603;
			exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942.csv");
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("\"TransactionType\",\"InvoiceDate\",\"PostDate\",\"DueDate\",\"Branch\",\"Department\",\"Ledger\",\"TransactionNum\",\"TransactionDesc\",\"GLAccount\",\"GLAccountDesc\",\"Job\",\"Account\",\"ChargeCode\",\"Period\",\"ReversePeriod\",\"Amount\",\"GSTAmount\",\"Debit\",\"Credit\",\"Balance\",\"IsLine\",\"TRPK\",\"OpeningPeriodDate\",\"ClosingPeriodDate\",\"OpeningBalance\",\"ClosingBalance\",\"IsControlTotal\",\"BankCode\",\"TaxID\",\"TaxIDType\",\"TaxIDRate\",\"TransactionDebtorOrCreditorGroup\",\"JobLocalClient\",\"JobLocalClientDebtorGroup\",\"TransactionDebtorExternalCreditRatingCode\",\"OriginalTransactionNumberForReversals\",\"OriginalTransactionNumberConsolidatedInvoiceRef\",\"TransactionNumberConsolidatedInvoiceRef\",\"Unused1\",\"Unused2\",\"TransactionHeaderBranch\",\"TransactionHeaderDepartment\",\"TransactionLineBranchOrgProxyCode\",\"TransactionHeaderBranchOrgProxyCode\",\"WasImportedFromExternalSystem\",\"PaymentReferenceNumber\",\"Currency\",\"ExRate\",\"OsAmount\",\"OsDebit\",\"OsCredit\",\"SubAccounts\",\"OrganisationSubAccount\",\"SalesExpenseGroupsSubAccount\",\"StaffAndResourcesSubAccount\",\"StaffGroupSubAccount\",\"Units\"\r\n\"\",\"\",\"\",\"\",\"\",\"\",\"AR \",\"\",\"AR Control Account\",\"6210.00.00\",\"TRADE DEBTORS CONTROL\",\"\",\"\",\"\",\"200603\",\"\",\"100.0000\",\"0.0000\",\"100.0000\",\"\",\"0.0000\",\"False\",\"\",\"1/03/2006 12:00:00 AM\",\"31/03/2006 11:59:00 PM\",\"0.0000\",\"0.0000\",\"True\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"AUD\",\"1.000000\",\"0.0000\",\"0.0000\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n\"DSC\",\"29/03/2006 3:39:00 PM\",\"15/03/2006 12:00:00 AM\",\"\",\"BNE\",\"BRN\",\"AR \",\"00001000\",\"MATCH NO.\",\"3960.00.00\",\"DISCOUNT ALLOWED\",\"\",\"\",\"\",\"200603\",\"\",\"-100.0000\",\"0.0000\",\"\",\"100.0000\",\"0.0000\",\"False\",\"{0}\",\"1/03/2006 12:00:00 AM\",\"31/03/2006 11:59:00 PM\",\"0.0000\",\"0.0000\",\"False\",\"          \",\"          \",\"   \",\"0\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"0\",\"0\",\"BNE\",\"BRN\",\"\",\"EDICUS      \",\"N\",\"\",\"AUD\",\"1.000000\",\"100.0000\",\"\",\"-100.0000\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n");
				stringBuilder.Replace("{0}", discount.PK.ToString());
				expectedExportData = stringBuilder.ToString();
				Assert("Something should have been exported but wasn't", exporter.ExportData());
				AssertEquals("Expected export file should exist", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				AssertMultilineASCIIEquals("Exporter is not exporting what is expected", expectedExportData, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 0;
			bizObj.ExportExistingBatch = false;
			bizObj.CreateAndExportBatch = false;
			bizObj.FromPeriod = 200603;
			bizObj.ToPeriod = 200603;
			bizObj.FromPeriod = 200604;
			bizObj.ToPeriod = 200604;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(!exporter.ExportData());
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}
		}

		AccPeriodManagement PriorPeriod;
		AccPeriodManagement FirstPeriod;

		void CreateExportBatchTestData()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			PriorPeriod = PeriodTestHelper.SetupSinglePeriod(200602, new ZDateTime(2006, 2, 1), new ZDateTime(2006, 3, 1).AddMinutes(-1));
			FirstPeriod = PeriodTestHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));
			// middle period
			_ = PeriodTestHelper.SetupSinglePeriod(200604, new ZDateTime(2006, 4, 1), new ZDateTime(2006, 5, 1, 23, 59, 00).AddDays(-1));
			// last period
			_ = PeriodTestHelper.SetupSinglePeriod(200605, new ZDateTime(2006, 5, 1), new ZDateTime(2006, 6, 1, 23, 59, 00).AddDays(-1));

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.GLHeader1.AG_Description = "GL Header 1";
			testObjectCreator.GLHeader1.AG_AccountNum = "GL 1 Num";
			testObjectCreator.GLHeader2.AG_Description = "GL Header 2";
			testObjectCreator.GLHeader2.AG_AccountNum = "GL 2 Num";
			AccGLHeader aPSuspenseControl = testObjectCreator.CreateAccGLHeader("6999.10.10", "TS", "AP Suspense", "BSH", "CR");
			AccGLHeader aRSuspenseControl = testObjectCreator.CreateAccGLHeader("8999.10.10", "TS", "AR Suspense", "BSH", "DR");
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControl.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aRSuspenseControl.PK.ToGuid());
			AccGLHeader generalPLAccount = testObjectCreator.CreateAccGLHeader("9999.10.10", "TS", "General P&L Account", "P&L", "DR");
			AccGLHeader exchangeReserveAccount = testObjectCreator.CreateAccGLHeader("9999.20.10", "TS", "Exchange Reserve ", "BSH", "DR");
			AccGLHeader unusedGLAccountWithAggregatedBalance = testObjectCreator.CreateAccGLHeader("9999.50.50", "TS", "Some P&L with balance", "P&L", "DR");
			AccGLHeader jpBankGLAccount = testObjectCreator.CreateAccGLHeader("9999.55.55",
				AccGLHeader.Constants.SectionTypes.Codes.TradingStatement,
				"JPY Bank Acccount",
				Constants.AccountType.BalanceSheetAccount,
				Constants.DebitCredit.Debit);

			AccBankAccount jPYBank = Factory.NewWithValidTestData<AccBankAccount>();
			jPYBank.AB_RX_NKAccountCurrency = Constants.CurrencyCodes.Japan;
			jPYBank.AB_AG = jpBankGLAccount.PK;
			jPYBank.AB_Desc = string.Empty;
			RefExchangeRate exRate = Factory.NewWithValidTestData<RefExchangeRate>();
			exRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			exRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			exRate.RE_SellRate = 1.234m;
			exRate.RE_RX_NKExCurrency = Constants.CurrencyCodes.Japan;
			exRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;

			DirectReceipt directReceiptInJPY = Factory.NewWithValidTestData<DirectReceipt>();
			directReceiptInJPY.AH_AB = jPYBank.PK;
			directReceiptInJPY.AH_ExchangeRate = 1.235;
			directReceiptInJPY.Lines.AddNew().FillWithValidTestData();
			directReceiptInJPY.Lines[0].AL_AG = generalPLAccount.PK;
			directReceiptInJPY.Lines[0].AL_OSExTaxAmount = 100m;
			Factory.Save();

			ARInvoice aRInvoice = Factory.NewWithValidTestData<ARInvoice>();
			SetHeaderDefaults(aRInvoice);
			aRInvoice.AH_PostDate = AddMonthToDateToSimulateCSTREVRecognisedAMonthLaterOrWIPACRReversedAMonthLater(ZDateTime.Now);
			aRInvoice.Lines.AddNew().FillWithValidTestData();
			aRInvoice.Lines[0].AL_AG = generalPLAccount.PK;
			aRInvoice.Lines[0].AL_OSAmount = 100m;
			aRInvoice.Lines[0].AL_LineAmount = 100m;
			aRInvoice.Lines[0].AL_ReverseToGL = "Y";

			ARCreditNote aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			SetHeaderDefaults(aRCreditNote);
			aRCreditNote.AH_PostDate = AddMonthToDateToSimulateCSTREVRecognisedAMonthLaterOrWIPACRReversedAMonthLater(ZDateTime.Now);
			aRCreditNote.Lines.AddNew().FillWithValidTestData();
			aRCreditNote.Lines[0].AL_AG = generalPLAccount.PK;
			aRCreditNote.Lines[0].AL_OSAmount = 100m;
			aRCreditNote.Lines[0].AL_LineAmount = 100m;
			aRCreditNote.Lines[0].AL_ReverseToGL = "Y";

			ARAdjustmentNote aRAdjustmentNote = Factory.NewWithValidTestData<ARAdjustmentNote>();
			SetHeaderDefaults(aRAdjustmentNote);
			aRAdjustmentNote.AH_PostDate = AddMonthToDateToSimulateCSTREVRecognisedAMonthLaterOrWIPACRReversedAMonthLater(ZDateTime.Now);
			aRAdjustmentNote.Lines.AddNew().FillWithValidTestData();
			aRAdjustmentNote.Lines[0].AL_AG = generalPLAccount.PK;
			aRAdjustmentNote.Lines[0].AL_OSAmount = 100m;
			aRAdjustmentNote.Lines[0].AL_LineAmount = 100m;
			aRAdjustmentNote.Lines[0].AL_ReverseToGL = "Y";

			ARPayment aRPayment = Factory.NewWithValidTestData<ARPayment>();
			aRPayment.AH_AB = testObjectCreator.AUDBankAccount.PK;
			SetHeaderDefaults(aRPayment);

			ARReceipt aRReceipt = Factory.NewWithValidTestData<ARReceipt>();
			aRReceipt.AH_AB = testObjectCreator.AUDBankAccount.PK;
			SetHeaderDefaults(aRReceipt);

			ARExchangeDifference aRExchangeDifference = Factory.NewWithValidTestData<ARExchangeDifference>();

			AROverpayment aROverpayment = Factory.NewWithValidTestData<AROverpayment>();

			ARDiscount aRDiscount = Factory.NewWithValidTestData<ARDiscount>();

			ARJournal aRJournal = Factory.NewWithValidTestData<ARJournal>();
			SetHeaderDefaults(aRJournal);

			ARTransfer aRTransfer = (ARTransfer)ARTransfer.New(typeof(ARTransfer), Factory);
			aRTransfer.AH_InvoiceAmount = 100;

			APInvoice aPInvoice = Factory.NewWithValidTestData<APInvoice>();
			SetHeaderDefaults(aPInvoice);
			aPInvoice.AH_PostDate = AddMonthToDateToSimulateCSTREVRecognisedAMonthLaterOrWIPACRReversedAMonthLater(ZDateTime.Now);
			aPInvoice.Lines.AddNew().FillWithValidTestData();
			aPInvoice.AH_TransactionNum = "Export Test";
			aPInvoice.Lines[0].AL_AG = generalPLAccount.PK;
			aPInvoice.Lines[0].AL_OSAmount = -100m;
			aPInvoice.Lines[0].AL_LineAmount = -100m;
			aPInvoice.Lines[0].AL_ReverseToGL = "Y";

			APCreditNote aPCreditNote = Factory.NewWithValidTestData<APCreditNote>();
			SetHeaderDefaults(aPCreditNote);
			aPCreditNote.AH_PostDate = AddMonthToDateToSimulateCSTREVRecognisedAMonthLaterOrWIPACRReversedAMonthLater(ZDateTime.Now);
			aPCreditNote.Lines.AddNew().FillWithValidTestData();
			aPCreditNote.AH_TransactionNum = "Export Test";
			aPCreditNote.Lines[0].AL_AG = generalPLAccount.PK;
			aPCreditNote.Lines[0].AL_OSAmount = -100m;
			aPCreditNote.Lines[0].AL_LineAmount = -100m;
			aPCreditNote.Lines[0].AL_ReverseToGL = "Y";

			APAdjustmentNote aPAdjustmentNote = Factory.NewWithValidTestData<APAdjustmentNote>();
			SetHeaderDefaults(aPAdjustmentNote);
			aPAdjustmentNote.AH_PostDate = AddMonthToDateToSimulateCSTREVRecognisedAMonthLaterOrWIPACRReversedAMonthLater(ZDateTime.Now);
			aPAdjustmentNote.Lines.AddNew().FillWithValidTestData();
			aPAdjustmentNote.AH_TransactionNum = "Export Test";
			aPAdjustmentNote.Lines[0].AL_AG = generalPLAccount.PK;
			aPAdjustmentNote.Lines[0].AL_OSAmount = -100m;
			aPAdjustmentNote.Lines[0].AL_LineAmount = -100m;
			aPAdjustmentNote.Lines[0].AL_ReverseToGL = "Y";

			APPayment aPPayment = Factory.NewWithValidTestData<APPayment>();
			aPPayment.AH_AB = testObjectCreator.AUDBankAccount.PK;
			SetHeaderDefaults(aPPayment);

			APReceipt aPReceipt = Factory.NewWithValidTestData<APReceipt>();
			aPReceipt.AH_AB = testObjectCreator.AUDBankAccount.PK;
			SetHeaderDefaults(aPReceipt);

			APExchangeDifference aPExchangeDifference = Factory.NewWithValidTestData<APExchangeDifference>();

			APOverpayment aPOverpayment = Factory.NewWithValidTestData<APOverpayment>();

			APDiscount aPDiscount = Factory.NewWithValidTestData<APDiscount>();

			APJournal aPJournal = Factory.NewWithValidTestData<APJournal>();
			SetHeaderDefaults(aPJournal);

			APTransfer aPTransfer = (APTransfer)APTransfer.New(typeof(APTransfer), Factory);
			aPTransfer.AH_InvoiceAmount = -100;

			Contra contra = Contra.New(Factory);
			contra.APRow.AH_AG = generalPLAccount.PK;
			contra.APRow.AH_InvoiceAmount = -100m;
			contra.APRow.AH_OSTotal = -100m;
			contra.APRow.AH_OutstandingAmount = -100m;
			contra.APRow.AH_PostToGL = "Y";
			contra.ARRow.AH_AG = generalPLAccount.PK;
			contra.ARRow.AH_InvoiceAmount = 100m;
			contra.ARRow.AH_OSTotal = 100m;
			contra.ARRow.AH_OutstandingAmount = 100m;
			contra.ARRow.AH_PostToGL = "Y";

			DirectPayment directPayment = Factory.NewWithValidTestData<DirectPayment>();
			directPayment.AH_TransactionNum = ZString.Empty;
			directPayment.AH_AB = testObjectCreator.AUDBankAccount.PK;
			directPayment.Lines.AddNew().FillWithValidTestData();
			directPayment.Lines[0].AL_AG = generalPLAccount.PK;
			directPayment.Lines[0].AL_OSExTaxAmount = 100m;

			DirectReceipt directReceipt = Factory.NewWithValidTestData<DirectReceipt>();
			directReceipt.AH_AB = testObjectCreator.AUDBankAccount.PK;
			directReceipt.Lines.AddNew().FillWithValidTestData();
			directReceipt.Lines[0].AL_AG = generalPLAccount.PK;
			directReceipt.Lines[0].AL_OSExTaxAmount = 100m;

			BankTransfer bankTransfer = new BankTransfer(Factory, null);
			bankTransfer.TransferRowFrom.AH_AB = testObjectCreator.AUDBankAccount.PK;
			bankTransfer.TransferRowFrom.AH_InvoiceAmount = bankTransfer.TransferRowFrom.AH_OSTotal = bankTransfer.TransferRowFrom.AH_OutstandingAmount = 100m;
			bankTransfer.TransferRowTo.AH_AB = testObjectCreator.AUDBankAccount2.PK;
			bankTransfer.TransferRowTo.AH_LocalExTaxAmount = bankTransfer.TransferRowTo.AH_OSExTaxAmount = bankTransfer.TransferRowTo.AH_OutstandingAmount = -100m;

			AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.ExchangeGainLossControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.ExchangeGainLossControlAccount.PK.ToGuid());
			CashbookExchangeDiff cashbookExchangeDiff = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			cashbookExchangeDiff.AH_AB = jPYBank.PK;
			cashbookExchangeDiff.AH_ExchangeRate = 1.5;

			GLJournal gJL = Factory.NewWithValidTestData<GLJournal>();
			gJL.AH_TransactionType = TransactionTypes.GLStandardJournal;
			gJL.AH_TransactionNum = "Test GJL";
			gJL.AH_Desc = "Test GJL";
			gJL.Lines.AddNew().FillWithValidTestData();
			gJL.Lines[0].AL_AG = testObjectCreator.GLHeader1.PK;
			gJL.Lines[0].AL_OSExTaxAmount = 100m;
			gJL.Lines.AddNew().FillWithValidTestData();
			gJL.Lines[1].AL_AG = testObjectCreator.GLHeader2.PK;
			gJL.Lines[1].AL_OSExTaxAmount = -100m;

			GLJournal rJL = Factory.NewWithValidTestData<GLJournal>();
			rJL.AH_TransactionNum = "Test RJL";
			rJL.AH_Desc = "Test RJL";
			rJL.AH_DueDate = ZDateTime.Now.AddMonths(1);
			rJL.AH_TransactionType = TransactionTypes.GLReversingJournal;
			rJL.Lines.AddNew().FillWithValidTestData();
			rJL.Lines.AddNew().FillWithValidTestData();
			rJL.Lines[0].AL_AG = testObjectCreator.GLHeader1.PK;
			rJL.Lines[0].AL_OSExTaxAmount = 100m;
			rJL.Lines[1].AL_AG = testObjectCreator.GLHeader2.PK;
			rJL.Lines[1].AL_OSExTaxAmount = -100m;

			GLJournal aJL = Factory.NewWithValidTestData<GLJournal>();
			aJL.AH_TransactionNum = "Test AJL";
			aJL.AH_Desc = "Test AJL";
			aJL.AH_DueDate = ZDateTime.Now.AddMonths(2);
			aJL.AH_TransactionType = TransactionTypes.GLAutoJournal;
			aJL.Lines.AddNew().FillWithValidTestData();
			aJL.Lines.AddNew().FillWithValidTestData();
			aJL.Lines[0].AL_AG = testObjectCreator.GLHeader1.PK;
			aJL.Lines[0].AL_OSExTaxAmount = 100m;
			aJL.Lines[1].AL_AG = testObjectCreator.GLHeader2.PK;
			aJL.Lines[1].AL_OSExTaxAmount = -100m;

			testObjectCreator.CC1.AC_AG_AccrualAccount = testObjectCreator.GLHeader1.PK;
			testObjectCreator.CC1.AC_AG_WIPAccount = testObjectCreator.GLHeader2.PK;
			testObjectCreator.CC1.AC_AG_CostAccount = testObjectCreator.GLHeader1.PK;
			testObjectCreator.CC1.AC_AG_RevenueAccount = testObjectCreator.GLHeader2.PK;

			JCJournalHeader jCJournalHeader = Factory.NewWithValidTestData<JCJournalHeader>();
			jCJournalHeader.AH_TransactionNum = "JCJournal";
			jCJournalHeader.AH_Desc = "JCJournal";
			jCJournalHeader.Lines.AddNew().FillWithValidTestData();
			jCJournalHeader.Lines[0].AL_ReverseDate = ZDateTime.Now;
			jCJournalHeader.Lines[0].AL_AC = testObjectCreator.CC1.PK;
			jCJournalHeader.Lines[0].AL_AG = exchangeReserveAccount.PK;
			jCJournalHeader.Lines[0].AL_PostToGL = "Y";
			jCJournalHeader.Lines[0].AL_ReverseToGL = "Y";
			jCJournalHeader.Lines[0].AL_OSExTaxAmount = 100m;

			WIP wIP = Factory.NewWithValidTestData<WIP>();
			wIP.AL_AC = testObjectCreator.CC1.PK;
			wIP.AL_ReverseDate = ZDateTime.Now;
			wIP.AL_PostToGL = "Y";
			wIP.AL_ReverseToGL = "Y";
			wIP.AL_OSAmount = 100m;
			wIP.AL_LineAmount = 100m;

			Accrual aCR = Factory.NewWithValidTestData<Accrual>();
			aCR.AL_AC = testObjectCreator.CC1.PK;
			aCR.AL_ReverseDate = ZDateTime.Now;
			aCR.AL_PostToGL = "Y";
			aCR.AL_ReverseToGL = "Y";
			aCR.AL_OSAmount = 100m;
			aCR.AL_LineAmount = 100m;

			AccGLAggregate aggregation = Factory.New<AccGLAggregate>();
			aggregation.AA_AG = unusedGLAccountWithAggregatedBalance.PK;
			aggregation.AA_Amount = 2000m;
			aggregation.AA_Period = PriorPeriod.AM_Period;
			aggregation.AA_GB = GlbBranch.CurrentBranch.PK;
			aggregation.AA_GC = GlbCompany.CurrentCompany.PK;
			aggregation.AA_GE = GlbDepartment.CurrentDepartment.PK;

			var glHeader1 = testObjectCreator.CreateGLHeader();
			glHeader1.AG_AccountNum = "999.124.11";
			glHeader1.AG_AccountType = Constants.AccountType.Note;
			glHeader1.AG_Description = "GL Header 1";

			var glHeader2 = testObjectCreator.CreateGLHeader();
			glHeader2.AG_AccountNum = "999.124.12";
			glHeader2.AG_AccountType = Constants.AccountType.Note;
			glHeader2.AG_Description = "GL Header 2";

			var noteJournal = testObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLNoteJournal, ZDateTime.Today, ZDateTime.Today);
			var line1 = noteJournal.GLJournalLines.AddNew();
			var line2 = noteJournal.GLJournalLines.AddNew();
			line1.AL_AG = glHeader1.PK;
			line1.UnitQuantity = 100m;
			line2.AL_AG = glHeader2.PK;
			line2.UnitQuantity = 200m;

			Factory.Save();

			BusinessObjectFactory newFactoryToAvoidMultipleObjectsAroundRow = new BusinessObjectFactory();
			foreach (AccTransactionHeader header in newFactoryToAvoidMultipleObjectsAroundRow.Load<AccTransactionHeader>(new ZQuery()))
			{
				header.AH_PostToGL = "Y";
				header.AH_PostDate = ZDateTime.Now;
				header.AH_Desc = header.AH_Ledger + header.AH_TransactionType;
			}

			foreach (AccTransactionLines line in newFactoryToAvoidMultipleObjectsAroundRow.Load<AccTransactionLines>(new ZQuery()))
			{
				if (line.TransactionHeader != null)
				{
					if (line.TransactionHeader.AH_Ledger == LedgerTypes.AccountsPayable || line.TransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable)
					{
						line.AL_PostDate = ZDateTime.Now;
					}
				}
				line.AL_Desc = line.AL_LineType;
			}

			using (ServiceContainerSuspenderHelper.FunctionalitySuspender<ServiceContainerSuspenderHelper.PostDateCriticalValidationSuspenderService>.GetSuspender(newFactoryToAvoidMultipleObjectsAroundRow))
			{
				newFactoryToAvoidMultipleObjectsAroundRow.Save();
			}

			void SetHeaderDefaults(AccTransactionHeader header)
			{
				header.AH_PostToGL = "Y";
				header.AH_PostDate = ZDateTime.Now;
				header.AH_InvoiceAmount = header.AH_Ledger == LedgerTypes.AccountsPayable ? -100 : 100m;
				header.AH_OutstandingAmount = header.AH_InvoiceAmount;
				header.AH_Desc = header.AH_Ledger + header.AH_TransactionType;
			}
		}

		ZDateTime AddMonthToDateToSimulateCSTREVRecognisedAMonthLaterOrWIPACRReversedAMonthLater(ZDateTime date)
		{
			return date.AddMonths(1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[SuspendCriticalValidation]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestUnfilteredBatchingAndAllPeriodsFilteredExport()
		{
			CreateExportBatchTestData();

			string exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942 EDI batch 1000.csv");
			TestCaseHelper.ClearTable(GenExportBatchSequenceSchema.Constants.TableName);
			GLTransactionBusinessObject bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 1000;
			bizObj.CreateAndExportBatch = true;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Something should have been exported but wasn't", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				AssertMultilineASCIIEquals("Expected output should match actual", FullBatchExportCSV, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert("ReExporting Should not produce anything", !exporter.ExportData());
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 1000;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = true;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert("Something should have been exported but wasn't", exporter.ExportData());
				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				AssertMultilineASCIIEquals("Expected output should match previous batch", FullBatchExportCSV, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 1000;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = true;
			bizObj.FromPeriod = 200605;
			bizObj.FromPeriod = 200605;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert("Something should have been exported but wasn't", exporter.ExportData());
				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				AssertMultilineASCIIEquals("Expected output should match previous batch Despite filter specificatoin", FullBatchExportCSV, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = false;
			bizObj.FromPeriod = 200603;
			bizObj.ToPeriod = 200605;
			exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942.csv");
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert("Something should have been exported but wasn't", exporter.ExportData());
				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				actualExportData = ReplaceInsignificantOpeningPeriodDates(actualExportData);
				AssertMultilineASCIIEquals("Expected output should match original batch", FullFilteredExportCSV, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[SuspendCriticalValidation]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		[ExpectNoExceptions]
		public void TestTransactionDescPaymentReferenceNumberMaxLength()
		{
			CreateExportBatchTestData();

			var string1024 = "".PadRight(512, 'a').PadRight(1024, 'b');
			var string35 = "".PadRight(17, 'c').PadRight(35, 'd');
			AssertEquals(1024, string1024.Length);
			AssertEquals(35, string35.Length);

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.AccTransactionLines SET AL_Desc = '{0}', AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST';", string1024));
			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.AccTransactionHeader SET AH_ChequeOrReference = '{0}', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST';", string35));

			string exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942 EDI batch 1000.csv");
			TestCaseHelper.ClearTable(GenExportBatchSequenceSchema.Constants.TableName);
			GLTransactionBusinessObject bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 1000;
			bizObj.CreateAndExportBatch = true;
			bizObj.DescriptionDisplay = "L";

			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Something should have been exported but wasn't", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				AssertMultilineASCIIEquals("Expected output should match actual", FullBatchExportCSVWithTransactionDescPaymentReferenceNumberMaxLength, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}
		}

		#region Expected data

		string FullBatchExportCSV
		{
			get
			{
				if (fFullBatchExportCSV == null)
				{
					fFullBatchExportCSV = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\FullBatchExport.csv");
				}
				return fFullBatchExportCSV;
			}
		}
		string fFullBatchExportCSV;

		string FullBatchExportCSVWithTransactionDescPaymentReferenceNumberMaxLength
		{
			get
			{
				if (fullBatchExportCSVWithTransactionDescPaymentReferenceNumberMaxLength == null)
				{
					fullBatchExportCSVWithTransactionDescPaymentReferenceNumberMaxLength = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\FullBatchExportWithDescMaxLen.csv");
				}
				return fullBatchExportCSVWithTransactionDescPaymentReferenceNumberMaxLength;
			}
		}
		string fullBatchExportCSVWithTransactionDescPaymentReferenceNumberMaxLength;

		string Period1ExportCSV
		{
			get
			{
				if (fPeriod1ExportCSV == null)
				{
					fPeriod1ExportCSV = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\Period1Export.csv");
				}
				return fPeriod1ExportCSV;
			}
		}
		string fPeriod1ExportCSV;

		string Period2ExportCSV
		{
			get
			{
				if (fPeriod2ExportCSV == null)
				{
					fPeriod2ExportCSV = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\Period2Export.csv");
				}
				return fPeriod2ExportCSV;
			}
		}
		string fPeriod2ExportCSV;

		string Period1DatesCSV
		{
			get
			{
				if (fPeriod1DatesCSV == null)
				{
					fPeriod1DatesCSV = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\Period1Dates.csv");
				}
				return fPeriod1DatesCSV;
			}
		}
		string fPeriod1DatesCSV;

		string Period2DatesCSV
		{
			get
			{
				if (fPeriod2DatesCSV == null)
				{
					fPeriod2DatesCSV = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\Period2Dates.csv");
				}
				return fPeriod2DatesCSV;
			}
		}
		string fPeriod2DatesCSV;

		string FullFilteredExportCSV
		{
			get
			{
				if (fFullFilteredExportCSV == null)
				{
					fFullFilteredExportCSV = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\FullFilteredExport.csv");
				}
				return fFullFilteredExportCSV;
			}
		}
		string fFullFilteredExportCSV;

		string AllPeriodsFilteredCSV
		{
			get
			{
				if (fAllPeriodsFilteredCSV == null)
				{
					fAllPeriodsFilteredCSV = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\AllPeriodsFiltered.csv");
				}
				return fAllPeriodsFilteredCSV;
			}
		}
		string fAllPeriodsFilteredCSV;

		string AllPeriodsFilteredForLinesCSV
		{
			get
			{
				if (fAllPeriodsFilteredForLinesCSV == null)
				{
					fAllPeriodsFilteredForLinesCSV = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\AllPeriodsFilteredForLines.csv");
				}
				return fAllPeriodsFilteredForLinesCSV;
			}
		}
		string fAllPeriodsFilteredForLinesCSV;

		string FiltereExportFor8230GLAccount
		{
			get
			{
				if (fFiltereExportFor8230GLAccount == null)
				{
					fFiltereExportFor8230GLAccount = File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\FiltereExportFor8230GLAccount.csv");
				}
				return fFiltereExportFor8230GLAccount;
			}
		}
		string fFiltereExportFor8230GLAccount;

		#endregion

		[DatCapabilityRequirement("SOURCE_CODE")]
		[SuspendCriticalValidation]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestFiltering()
		{
			CreateExportBatchTestData();

			string exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942.csv");
			GLTransactionBusinessObject bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = false;
			bizObj.DescriptionDisplay = "H";
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Something should have been exported but wasn't", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				AssertMultilineASCIIEquals("Should look like a normal full bacth", AllPeriodsFilteredCSV, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = false;
			bizObj.DescriptionDisplay = "L";
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Something should have been exported but wasn't", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				AssertMultilineASCIIEquals("Should look like a normal full bacth", AllPeriodsFilteredForLinesCSV, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = false;
			bizObj.DepartmentPK = GlbDepartment.CurrentDepartment.PK;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Something should have been exported but wasn't", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				AssertMultilineASCIIEquals("Should look like a normal full bacth", AllPeriodsFilteredCSV, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = false;
			bizObj.DepartmentPK = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, GlbDepartment.CurrentDepartment.PK.ToGuid())).PK;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert("Nothign should be exported for this department", !exporter.ExportData());
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = false;
			bizObj.BranchPK = GlbBranch.CurrentBranch.PK;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Something should have been exported but wasn't", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				AssertMultilineASCIIEquals("Should look like a normal full bacth", AllPeriodsFilteredCSV, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = false;
			bizObj.BranchPK = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK.ToGuid())).PK;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert("Nothign should be exported for this branch", !exporter.ExportData());
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = false;
			bizObj.StartGLAccountPK = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "8230.00.00")).PK;
			bizObj.EndGLAccountPK = bizObj.StartGLAccountPK;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert("Should Get somethign in this export", exporter.ExportData());
				AssertEquals("Something should have been exported but wasn't", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				AssertMultilineASCIIEquals("Should only contain OVP transactions", FiltereExportFor8230GLAccount, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = false;
			bizObj.FromDate = new ZDateTime(2006, 3, 1);
			bizObj.ToDate = FirstPeriod.AM_EndDate;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert("Should Get somethign in this export", exporter.ExportData());
				AssertEquals("Something should have been exported but wasn't", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				AssertMultilineASCIIEquals("Should contains same values as Period 1", Period1DatesCSV, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}
		}

		public void TestHighWaterMarkNotification()
		{
			using (SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.DataType.SuspendValidation())
			{
				SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.ToDateTime());
			}
			var message = string.Format(@"To aid performance of the export, the system will only search for un-batched transactions that were created or edited since {0}.
If you need to export transactions from before this date, please use the date filtering and this will search for all un-batched transactions.", SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.Value);

			BizObj.CreateAndExportBatch = true;

			Exporter.ExportData();
			if (Exporter.IsHighWaterMarkEnabled)
			{
				AssertContains("Notification should include high water mark message", message, NotificationBuffer.AsString);
			}
			else
			{
				AssertNotContains("Notification should not include high water mark message", message, NotificationBuffer.AsString);
			}

			SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.Inner.DeleteValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			NotificationBuffer.Clear();

			Exporter.ExportData();
			AssertNotContains("Notification should not include high water mark message", message, NotificationBuffer.AsString);
		}

		public void TestHighWaterMarkIsUsed()
		{
			BizObj.CreateAndExportBatch = true;
			BizObj.BatchNumber = 1000;

			// @HighWaterMark does not have a default value and must be always provided
			AssertExceptionThrown("@HighWaterMark parameter", typeof(IndexOutOfRangeException), () => Exporter.CreateGLTransactionsBatchDbCommand().GetParameter("@HighWaterMark"));

			var highWaterMark = ZDateTime.UtcNow.ToDateTime().Subtract(new TimeSpan(48, 0, 0));
			using (SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.DataType.SuspendValidation())
			{
				SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, highWaterMark);
			}

			if (Exporter.UsingHighWaterMark)
			{
				var param = Exporter.CreateGLTransactionsBatchDbCommand().GetParameter("@HighWaterMark");
				AssertNotNull("@HighWaterMark parameter", param);
				AssertEquals("@HighWaterMark parameter value", highWaterMark, param.Value);
			}
			else
			{
				AssertExceptionThrown("@HighWaterMark parameter", typeof(IndexOutOfRangeException), () => Exporter.CreateGLTransactionsBatchDbCommand().GetParameter("@HighWaterMark"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[SuspendCriticalValidation]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestPeriodBasedBatching()
		{
			CreateExportBatchTestData();

			string exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942 EDI batch 1000.csv");
			TestCaseHelper.ClearTable(GenExportBatchSequenceSchema.Constants.TableName);
			GLTransactionBusinessObject bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 1000;
			bizObj.CreateAndExportBatch = true;
			bizObj.FromPeriod = 200603;
			bizObj.ToPeriod = 200603;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Something should have been exported but wasn't", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				AssertMultilineASCIIEquals("Should Export and batch Period 1 transactions only, i.e. all posting no reversal or recognition", Period1ExportCSV, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 1000;
			bizObj.CreateAndExportBatch = true;
			bizObj.FromPeriod = 200603;
			bizObj.ToPeriod = 200603;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert("Should not try to create same batcah again", !exporter.ExportData());
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 1000;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = true;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Something should have been exported but wasn't", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				AssertMultilineASCIIEquals("Should reproduce batch exactly", Period1ExportCSV, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 1000;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = true;
			bizObj.FromPeriod = 200604;
			bizObj.ToPeriod = 200604;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Something should have been exported but wasn't", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				AssertMultilineASCIIEquals("Should reproduce first batch exactly, despite period 2 filter specification", Period1ExportCSV, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 1001;
			exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942 EDI batch 1001.csv");
			bizObj.CreateAndExportBatch = true;
			bizObj.FromPeriod = 200604;
			bizObj.ToPeriod = 200604;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Something should have been exported but wasn't", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				AssertMultilineASCIIEquals("Should Export Period 2 Transactoins Only, i.e. WIP/ACR reversal and CST/REV recognition", Period2ExportCSV, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 1001;
			exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942 EDI batch 1001.csv");
			bizObj.CreateAndExportBatch = true;
			bizObj.FromPeriod = 200604;
			bizObj.ToPeriod = 200604;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert("Should not try to create same batch again", !exporter.ExportData());
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 1001;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = true;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Something should have been exported but wasn't", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				AssertMultilineASCIIEquals("Should match batch 2 exaclty", Period2ExportCSV, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 1001;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = true;
			bizObj.FromPeriod = 200603;
			bizObj.ToPeriod = 200603;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Something should have been exported but wasn't", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				AssertMultilineASCIIEquals("Should match batch 2 exaclty , despite period 1 filter specification", Period2ExportCSV, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = false;
			bizObj.FromPeriod = 200603;
			bizObj.ToPeriod = 200603;
			exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942.csv");
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Something should have been exported but wasn't", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				actualExportData = ReplaceInsignificantOpeningPeriodDates(actualExportData);
				AssertMultilineASCIIEquals("Should match period 1 batch as dates are the same", Period1DatesCSV, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = false;
			bizObj.FromPeriod = 200604;
			bizObj.ToPeriod = 200604;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Something should have been exported but wasn't", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				actualExportData = ReplaceInsignificantOpeningPeriodDates(actualExportData);
				AssertMultilineASCIIEquals("Should match period 2 batch as dates are the", Period2DatesCSV, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}
		}

		string ReplaceInsignificantOpeningPeriodDates(string data)
		{
			string valueToReplace = "\"1/03/2006 12:00:00 AM\",\"31/05/2006 11:59:00 PM\",\"0.0000\",\"0.0000\"";
			string replacement = "\"\",\"\",\"\",\"\"";
			return data.Replace(valueToReplace, replacement);
		}

		string ReplacePKs(string data, BusinessObjectFactory factory)
		{
			foreach (BusinessObject @object in factory.Load<AccTransactionHeader>(new ZQuery()))
			{
				data = data.Replace(@object.PK.ToString(), "!!PK!!");
			}
			foreach (BusinessObject @object in factory.Load<AccTransactionLines>(new ZQuery()))
			{
				data = data.Replace(@object.PK.ToString(), "!!PK!!");
			}
			return data;
		}

		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExxIncludedInBatchExportReExportofExistingBatchAndNonBatchExport()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			PeriodTestHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AccGLHeader aPSuspenseControl = testObjectCreator.CreateAccGLHeader("9991.10.10", "TS", "AP Suspense", "BSH", "CR");
			AccGLHeader aRSuspenseControl = testObjectCreator.CreateAccGLHeader("9992.10.10", "TS", "AR Suspense", "BSH", "DR");
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControl.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aRSuspenseControl.PK.ToGuid());

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			ARExchangeDifference eXX = Factory.New<ARExchangeDifference>();
			eXX.AH_PostDate = new DateTime(2006, 3, 15);
			eXX.AH_PostToGL = "Y";
			eXX.AH_InvoiceAmount = 100m;
			eXX.AH_OSTotal = 100m;
			eXX.AH_OutstandingAmount = 100m;
			eXX.Validation.ValidateAll();
			AssertNoErrors(eXX);

			PrepareMiscellaneousTransactionForSaving(eXX);

			testObjectCreator.Factory.Save();

			string exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942 EDI batch 1000.csv");
			TestCaseHelper.ClearTable(GenExportBatchSequenceSchema.Constants.TableName);
			GLTransactionBusinessObject bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 1000;
			bizObj.CreateAndExportBatch = true;
			string expectedExportData = "";
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Something should have been exported but wasn't", true, File.Exists(exportFileName));
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("\"TransactionType\",\"InvoiceDate\",\"PostDate\",\"DueDate\",\"Branch\",\"Department\",\"Ledger\",\"TransactionNum\",\"TransactionDesc\",\"GLAccount\",\"GLAccountDesc\",\"Job\",\"Account\",\"ChargeCode\",\"Period\",\"ReversePeriod\",\"Amount\",\"GSTAmount\",\"Debit\",\"Credit\",\"Balance\",\"IsLine\",\"TRPK\",\"OpeningPeriodDate\",\"ClosingPeriodDate\",\"OpeningBalance\",\"ClosingBalance\",\"IsControlTotal\",\"BankCode\",\"TaxID\",\"TaxIDType\",\"TaxIDRate\",\"TransactionDebtorOrCreditorGroup\",\"JobLocalClient\",\"JobLocalClientDebtorGroup\",\"TransactionDebtorExternalCreditRatingCode\",\"OriginalTransactionNumberForReversals\",\"OriginalTransactionNumberConsolidatedInvoiceRef\",\"TransactionNumberConsolidatedInvoiceRef\",\"Unused1\",\"Unused2\",\"TransactionHeaderBranch\",\"TransactionHeaderDepartment\",\"TransactionLineBranchOrgProxyCode\",\"TransactionHeaderBranchOrgProxyCode\",\"WasImportedFromExternalSystem\",\"PaymentReferenceNumber\",\"Currency\",\"ExRate\",\"OsAmount\",\"OsDebit\",\"OsCredit\",\"SubAccounts\",\"OrganisationSubAccount\",\"SalesExpenseGroupsSubAccount\",\"StaffAndResourcesSubAccount\",\"StaffGroupSubAccount\",\"Units\"\r\n\"\",\"\",\"\",\"\",\"\",\"\",\"AR \",\"\",\"AR Control Account\",\"6210.00.00\",\"TRADE DEBTORS CONTROL\",\"\",\"\",\"\",\"200603\",\"\",\"100.0000\",\"0.0000\",\"100.0000\",\"\",\"0.0000\",\"False\",\"\",\"\",\"\",\"\",\"\",\"True\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"AUD\",\"1.000000\",\"0.0000\",\"0.0000\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n\"EXX\",\"29/03/2006 3:39:00 PM\",\"15/03/2006 12:00:00 AM\",\"\",\"BNE\",\"BRN\",\"AR \",\"00001000\",\"MATCH NO.\",\"2020.10.00\",\"EXCHANGE GAIN(LOSS) - CONTROL\",\"\",\"\",\"\",\"200603\",\"\",\"-100.0000\",\"0.0000\",\"\",\"100.0000\",\"0.0000\",\"False\",\"{0}\",\"\",\"\",\"\",\"\",\"False\",\"          \",\"          \",\"   \",\"0\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"0\",\"0\",\"BNE\",\"BRN\",\"\",\"EDICUS      \",\"N\",\"\",\"AUD\",\"1.000000\",\"100.0000\",\"\",\"-100.0000\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n");
				stringBuilder.Replace("{0}", eXX.PK.ToString());
				expectedExportData = stringBuilder.ToString();
				string actualExportData = File.ReadAllText(exportFileName);
				AssertMultilineASCIIEquals("Exporter is not exporting what is expected", expectedExportData, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 1000;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = true;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert("Something should have been exported but wasn't", exporter.ExportData());
				AssertEquals("Expected export file should exist", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				AssertMultilineASCIIEquals("Exporter is not exporting what is expected", expectedExportData, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 0;
			bizObj.ExportExistingBatch = false;
			bizObj.CreateAndExportBatch = false;
			bizObj.FromPeriod = 200603;
			bizObj.ToPeriod = 200603;
			exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942.csv");
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("\"TransactionType\",\"InvoiceDate\",\"PostDate\",\"DueDate\",\"Branch\",\"Department\",\"Ledger\",\"TransactionNum\",\"TransactionDesc\",\"GLAccount\",\"GLAccountDesc\",\"Job\",\"Account\",\"ChargeCode\",\"Period\",\"ReversePeriod\",\"Amount\",\"GSTAmount\",\"Debit\",\"Credit\",\"Balance\",\"IsLine\",\"TRPK\",\"OpeningPeriodDate\",\"ClosingPeriodDate\",\"OpeningBalance\",\"ClosingBalance\",\"IsControlTotal\",\"BankCode\",\"TaxID\",\"TaxIDType\",\"TaxIDRate\",\"TransactionDebtorOrCreditorGroup\",\"JobLocalClient\",\"JobLocalClientDebtorGroup\",\"TransactionDebtorExternalCreditRatingCode\",\"OriginalTransactionNumberForReversals\",\"OriginalTransactionNumberConsolidatedInvoiceRef\",\"TransactionNumberConsolidatedInvoiceRef\",\"Unused1\",\"Unused2\",\"TransactionHeaderBranch\",\"TransactionHeaderDepartment\",\"TransactionLineBranchOrgProxyCode\",\"TransactionHeaderBranchOrgProxyCode\",\"WasImportedFromExternalSystem\",\"PaymentReferenceNumber\",\"Currency\",\"ExRate\",\"OsAmount\",\"OsDebit\",\"OsCredit\",\"SubAccounts\",\"OrganisationSubAccount\",\"SalesExpenseGroupsSubAccount\",\"StaffAndResourcesSubAccount\",\"StaffGroupSubAccount\",\"Units\"\r\n\"\",\"\",\"\",\"\",\"\",\"\",\"AR \",\"\",\"AR Control Account\",\"6210.00.00\",\"TRADE DEBTORS CONTROL\",\"\",\"\",\"\",\"200603\",\"\",\"100.0000\",\"0.0000\",\"100.0000\",\"\",\"0.0000\",\"False\",\"\",\"1/03/2006 12:00:00 AM\",\"31/03/2006 11:59:00 PM\",\"0.0000\",\"0.0000\",\"True\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"AUD\",\"1.000000\",\"0.0000\",\"0.0000\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n\"EXX\",\"29/03/2006 3:39:00 PM\",\"15/03/2006 12:00:00 AM\",\"\",\"BNE\",\"BRN\",\"AR \",\"00001000\",\"MATCH NO.\",\"2020.10.00\",\"EXCHANGE GAIN(LOSS) - CONTROL\",\"\",\"\",\"\",\"200603\",\"\",\"-100.0000\",\"0.0000\",\"\",\"100.0000\",\"0.0000\",\"False\",\"{0}\",\"1/03/2006 12:00:00 AM\",\"31/03/2006 11:59:00 PM\",\"0.0000\",\"0.0000\",\"False\",\"          \",\"          \",\"   \",\"0\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"0\",\"0\",\"BNE\",\"BRN\",\"\",\"EDICUS      \",\"N\",\"\",\"AUD\",\"1.000000\",\"100.0000\",\"\",\"-100.0000\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n");
				stringBuilder.Replace("{0}", eXX.PK.ToString());
				expectedExportData = stringBuilder.ToString();
				Assert("Something should have been exported but wasn't", exporter.ExportData());
				AssertEquals("Expected export file should exist", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				AssertMultilineASCIIEquals("Exporter is not exporting what is expected", expectedExportData, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 0;
			bizObj.ExportExistingBatch = false;
			bizObj.CreateAndExportBatch = false;
			bizObj.FromPeriod = 200604;
			bizObj.ToPeriod = 200604;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(!exporter.ExportData());
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}
		}

		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestOVPIncludedInBatchExportReExportofExistingBatchAndNonBatchExport()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			PeriodTestHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));
			AccGLHeader aPSuspenseControl = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "7200.40.50"));
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AROverpayment oVP = Factory.New<AROverpayment>();
			oVP.AH_PostDate = new DateTime(2006, 3, 15);
			oVP.AH_PostToGL = "Y";
			oVP.AH_InvoiceAmount = 100m;
			oVP.AH_OSTotal = 100m;
			oVP.AH_OutstandingAmount = 100m;
			oVP.Validation.ValidateAll();
			AssertNoErrors(oVP);

			PrepareMiscellaneousTransactionForSaving(oVP);

			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControl.PK.ToGuid());

			testObjectCreator.Factory.Save();

			string exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942 EDI batch 1000.csv");
			TestCaseHelper.ClearTable(GenExportBatchSequenceSchema.Constants.TableName);
			GLTransactionBusinessObject bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 1000;
			bizObj.CreateAndExportBatch = true;
			string expectedExportData = "";
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Something should have been exported but wasn't", true, File.Exists(exportFileName));
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("\"TransactionType\",\"InvoiceDate\",\"PostDate\",\"DueDate\",\"Branch\",\"Department\",\"Ledger\",\"TransactionNum\",\"TransactionDesc\",\"GLAccount\",\"GLAccountDesc\",\"Job\",\"Account\",\"ChargeCode\",\"Period\",\"ReversePeriod\",\"Amount\",\"GSTAmount\",\"Debit\",\"Credit\",\"Balance\",\"IsLine\",\"TRPK\",\"OpeningPeriodDate\",\"ClosingPeriodDate\",\"OpeningBalance\",\"ClosingBalance\",\"IsControlTotal\",\"BankCode\",\"TaxID\",\"TaxIDType\",\"TaxIDRate\",\"TransactionDebtorOrCreditorGroup\",\"JobLocalClient\",\"JobLocalClientDebtorGroup\",\"TransactionDebtorExternalCreditRatingCode\",\"OriginalTransactionNumberForReversals\",\"OriginalTransactionNumberConsolidatedInvoiceRef\",\"TransactionNumberConsolidatedInvoiceRef\",\"Unused1\",\"Unused2\",\"TransactionHeaderBranch\",\"TransactionHeaderDepartment\",\"TransactionLineBranchOrgProxyCode\",\"TransactionHeaderBranchOrgProxyCode\",\"WasImportedFromExternalSystem\",\"PaymentReferenceNumber\",\"Currency\",\"ExRate\",\"OsAmount\",\"OsDebit\",\"OsCredit\",\"SubAccounts\",\"OrganisationSubAccount\",\"SalesExpenseGroupsSubAccount\",\"StaffAndResourcesSubAccount\",\"StaffGroupSubAccount\",\"Units\"\r\n\"\",\"\",\"\",\"\",\"\",\"\",\"AR \",\"\",\"AR Control Account\",\"6210.00.00\",\"TRADE DEBTORS CONTROL\",\"\",\"\",\"\",\"200603\",\"\",\"100.0000\",\"0.0000\",\"100.0000\",\"\",\"0.0000\",\"False\",\"\",\"\",\"\",\"\",\"\",\"True\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"AUD\",\"1.000000\",\"0.0000\",\"0.0000\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n\"OVP\",\"29/03/2006 3:39:00 PM\",\"15/03/2006 12:00:00 AM\",\"\",\"BNE\",\"BRN\",\"AR \",\"00001000\",\"MATCH NO.\",\"8230.00.00\",\"OVERPAYMENT - CUSTOMER\",\"\",\"\",\"\",\"200603\",\"\",\"-100.0000\",\"0.0000\",\"\",\"100.0000\",\"0.0000\",\"False\",\"{0}\",\"\",\"\",\"\",\"\",\"False\",\"          \",\"          \",\"   \",\"0\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"0\",\"0\",\"BNE\",\"BRN\",\"\",\"EDICUS      \",\"N\",\"\",\"AUD\",\"1.000000\",\"100.0000\",\"\",\"-100.0000\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n");
				stringBuilder.Replace("{0}", oVP.PK.ToString());
				expectedExportData = stringBuilder.ToString();
				string actualExportData = File.ReadAllText(exportFileName);
				AssertMultilineASCIIEquals("Exporter is not exporting what is expected", expectedExportData, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 1000;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = true;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert("Something should have been exported but wasn't", exporter.ExportData());
				AssertEquals("Expected export file should exist", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				AssertMultilineASCIIEquals("Exporter is not exporting what is expected", expectedExportData, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 0;
			bizObj.ExportExistingBatch = false;
			bizObj.CreateAndExportBatch = false;
			bizObj.FromPeriod = 200603;
			bizObj.ToPeriod = 200603;
			exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942.csv");
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("\"TransactionType\",\"InvoiceDate\",\"PostDate\",\"DueDate\",\"Branch\",\"Department\",\"Ledger\",\"TransactionNum\",\"TransactionDesc\",\"GLAccount\",\"GLAccountDesc\",\"Job\",\"Account\",\"ChargeCode\",\"Period\",\"ReversePeriod\",\"Amount\",\"GSTAmount\",\"Debit\",\"Credit\",\"Balance\",\"IsLine\",\"TRPK\",\"OpeningPeriodDate\",\"ClosingPeriodDate\",\"OpeningBalance\",\"ClosingBalance\",\"IsControlTotal\",\"BankCode\",\"TaxID\",\"TaxIDType\",\"TaxIDRate\",\"TransactionDebtorOrCreditorGroup\",\"JobLocalClient\",\"JobLocalClientDebtorGroup\",\"TransactionDebtorExternalCreditRatingCode\",\"OriginalTransactionNumberForReversals\",\"OriginalTransactionNumberConsolidatedInvoiceRef\",\"TransactionNumberConsolidatedInvoiceRef\",\"Unused1\",\"Unused2\",\"TransactionHeaderBranch\",\"TransactionHeaderDepartment\",\"TransactionLineBranchOrgProxyCode\",\"TransactionHeaderBranchOrgProxyCode\",\"WasImportedFromExternalSystem\",\"PaymentReferenceNumber\",\"Currency\",\"ExRate\",\"OsAmount\",\"OsDebit\",\"OsCredit\",\"SubAccounts\",\"OrganisationSubAccount\",\"SalesExpenseGroupsSubAccount\",\"StaffAndResourcesSubAccount\",\"StaffGroupSubAccount\",\"Units\"\r\n\"\",\"\",\"\",\"\",\"\",\"\",\"AR \",\"\",\"AR Control Account\",\"6210.00.00\",\"TRADE DEBTORS CONTROL\",\"\",\"\",\"\",\"200603\",\"\",\"100.0000\",\"0.0000\",\"100.0000\",\"\",\"0.0000\",\"False\",\"\",\"1/03/2006 12:00:00 AM\",\"31/03/2006 11:59:00 PM\",\"0.0000\",\"0.0000\",\"True\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"AUD\",\"1.000000\",\"0.0000\",\"0.0000\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n\"OVP\",\"29/03/2006 3:39:00 PM\",\"15/03/2006 12:00:00 AM\",\"\",\"BNE\",\"BRN\",\"AR \",\"00001000\",\"MATCH NO.\",\"8230.00.00\",\"OVERPAYMENT - CUSTOMER\",\"\",\"\",\"\",\"200603\",\"\",\"-100.0000\",\"0.0000\",\"\",\"100.0000\",\"0.0000\",\"False\",\"{0}\",\"1/03/2006 12:00:00 AM\",\"31/03/2006 11:59:00 PM\",\"0.0000\",\"0.0000\",\"False\",\"          \",\"          \",\"   \",\"0\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"0\",\"0\",\"BNE\",\"BRN\",\"\",\"EDICUS      \",\"N\",\"\",\"AUD\",\"1.000000\",\"100.0000\",\"\",\"-100.0000\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n");
				stringBuilder.Replace("{0}", oVP.PK.ToString());
				expectedExportData = stringBuilder.ToString();
				Assert("Something should have been exported but wasn't", exporter.ExportData());
				AssertEquals("Expected export file should exist", true, File.Exists(exportFileName));
				string actualExportData = File.ReadAllText(exportFileName);
				AssertMultilineASCIIEquals("Exporter is not exporting what is expected", expectedExportData, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.BatchNumber = 0;
			bizObj.ExportExistingBatch = false;
			bizObj.CreateAndExportBatch = false;
			bizObj.FromPeriod = 200604;
			bizObj.ToPeriod = 200604;
			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(!exporter.ExportData());
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}
		}

		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExportDataBatchWithoutFilter()
		{
			TestExportDataBatch(false);
		}

		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExportDataBatchWithFilter()
		{
			TestExportDataBatch(true);
		}

		void TestExportDataBatch(bool withFilter)
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			PeriodTestHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));
			ZGuid gLAccountFrom = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, "1000.00.00")).PK;
			ZGuid gLAccountTo = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, "9999.00.00")).PK;
			AccGLHeader aPSuspenseControl = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "7200.40.50"));
			AccGLHeader glAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, SQLComparisonOperator.Equal, "4710.00.00"));

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			Invoice invoice = (Invoice)testObjectCreator.CreateInvoice(typeof(APInvoice), GlbCompany.CurrentCompany.LocalCurrency, GlbCompany.CurrentCompany.LocalCurrency.CurrentBuyRate);

			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControl.PK.ToGuid());

			invoice.AH_InvoiceAmount = 333m;
			invoice.AH_AG = glAccount.PK;
			invoice.AH_PostDate = new ZDateTime(2006, 3, 10);
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";
			invoice.AH_PostToGL = "Y";

			InvoiceLine invoiceLine = (InvoiceLine)testObjectCreator.CreateInvoiceLine(invoice, GlbCompany.CurrentCompany.LocalCurrency, GlbCompany.CurrentCompany.LocalCurrency.CurrentBuyRate, 333m);

			invoiceLine.AL_AG = glAccount.PK;
			invoiceLine.AL_LineAmount = 333m;
			invoiceLine.AL_OSAmount = 333m;
			invoiceLine.AL_PostDate = new ZDateTime(2006, 3, 10);
			AssertNotNull("reverse should be set when post is set due to new reve recognition", invoiceLine.AL_ReverseDate);

			testObjectCreator.Factory.Save();

			if (withFilter)
			{
				BizObj.FromPeriod = 200603;
				BizObj.ToPeriod = 200603;
				BizObj.StartGLAccountPK = gLAccountFrom;
				BizObj.EndGLAccountPK = gLAccountTo;
			}
			BizObj.ExportDirectory = EnvProxy.Instance.TempPath;

			string exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942 " + Env.CurrentCompany.Code + " batch 1000.csv");

			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));

				BizObj.CreateAndExportBatch = true;
				BizObj.Exporter = Exporter;
				BizObj.Factory.Save();
				AssertEquals("Expected export file should exist", true, File.Exists(exportFileName));

				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("\"TransactionType\",\"InvoiceDate\",\"PostDate\",\"DueDate\",\"Branch\",\"Department\",\"Ledger\",\"TransactionNum\",\"TransactionDesc\",\"GLAccount\",\"GLAccountDesc\",\"Job\",\"Account\",\"ChargeCode\",\"Period\",\"ReversePeriod\",\"Amount\",\"GSTAmount\",\"Debit\",\"Credit\",\"Balance\",\"IsLine\",\"TRPK\",\"OpeningPeriodDate\",\"ClosingPeriodDate\",\"OpeningBalance\",\"ClosingBalance\",\"IsControlTotal\",\"BankCode\",\"TaxID\",\"TaxIDType\",\"TaxIDRate\",\"TransactionDebtorOrCreditorGroup\",\"JobLocalClient\",\"JobLocalClientDebtorGroup\",\"TransactionDebtorExternalCreditRatingCode\",\"OriginalTransactionNumberForReversals\",\"OriginalTransactionNumberConsolidatedInvoiceRef\",\"TransactionNumberConsolidatedInvoiceRef\",\"Unused1\",\"Unused2\",\"TransactionHeaderBranch\",\"TransactionHeaderDepartment\",\"TransactionLineBranchOrgProxyCode\",\"TransactionHeaderBranchOrgProxyCode\",\"WasImportedFromExternalSystem\",\"PaymentReferenceNumber\",\"Currency\",\"ExRate\",\"OsAmount\",\"OsDebit\",\"OsCredit\",\"SubAccounts\",\"OrganisationSubAccount\",\"SalesExpenseGroupsSubAccount\",\"StaffAndResourcesSubAccount\",\"StaffGroupSubAccount\",\"Units\"\r\n\"\",\"\",\"\",\"\",\"\",\"\",\"AP \",\"\",\"AP Control Account\",\"8210.00.00\",\"TRADE CREDITORS CONTROL\",\"\",\"\",\"\",\"200603\",\"\",\"333.0000\",\"0.0000\",\"333.0000\",\"\",\"0.0000\",\"False\",\"\",\"\",\"\",\"\",\"\",\"True\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"AUD\",\"1.000000\",\"0.0000\",\"0.0000\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n\"INV\",\"29/03/2006 3:39:00 PM\",\"10/03/2006 12:00:00 AM\",\"29/03/2006 3:39:00 PM\",\"BNE\",\"BRN\",\"AP \",\"" + invoice.AH_TransactionNum + "\",\"Test Invoice\",\"6310.00.00\",\"INPUT TAX RECEIVABLE\",\"\",\"\",\"\",\"200603\",\"\",\"0.0000\",\"0.0000\",\"\",\"\",\"0.0000\",\"False\",\"@PK\",\"\",\"\",\"\",\"\",\"False\",\"          \",\"ZZGST1    \",\"RAT\",\"10\",\"\",\"\",\"\",\"\",\"\",\"\",\"00001000\",\"0\",\"0\",\"BNE\",\"BRN\",\"EDICUS      \",\"EDICUS      \",\"N\",\"\",\"AUD\",\"1.000000\",\"0.0000\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n\"INV\",\"29/03/2006 3:39:00 PM\",\"10/03/2006 12:00:00 AM\",\"29/03/2006 3:39:00 PM\",\"BNE\",\"BRN\",\"AP \",\"" + invoice.AH_TransactionNum + "\",\"Test Invoice\",\"7200.40.50\",\"TYPE WRITER - ACCUM DEPN.\",\"\",\"\",\"\",\"200603\",\"\",\"-333.0000\",\"0.0000\",\"\",\"333.0000\",\"0.0000\",\"False\",\"\",\"\",\"\",\"\",\"\",\"False\",\"          \",\"ZZGST1    \",\"RAT\",\"10\",\"\",\"\",\"\",\"\",\"\",\"\",\"00001000\",\"0\",\"0\",\"BNE\",\"BRN\",\"EDICUS      \",\"EDICUS      \",\"N\",\"\",\"AUD\",\"0.000000\",\"-333.0000\",\"\",\"333.0000\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n");
				stringBuilder.Replace("@AccountDescription", aPSuspenseControl.AG_Description);
				stringBuilder.Replace("@AccountNum", aPSuspenseControl.AG_AccountNum);
				stringBuilder.Replace("@PK", invoiceLine.PK.ToString());
				string expectedExportData = stringBuilder.ToString();
				string actualExportData = File.ReadAllText(exportFileName);

				AssertMultilineASCIIEquals("Exporter is not exporting what is expected", expectedExportData, actualExportData);

				if (withFilter || !Exporter.IsHighWaterMarkEnabled)
				{
					AssertEquals("High water mark registry value should not have been set", DateTime.MinValue, SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.Value);
				}
				else
				{
					AssertEquals("High water mark registry value should have been set", ZDateTime.UtcNow.ToDateTime().Subtract(new TimeSpan(48, 0, 0)), SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.Value);
				}
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}

			exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942 " + Env.CurrentCompany.Code + " batch 1001.csv");
			AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));

			bool isSavingFailed = false;
			try
			{
				BizObj.Factory.Save();
			}
			catch (ZCannotSaveException)
			{
				isSavingFailed = true;
			}
			Assert(isSavingFailed);
			AssertEquals("There is nothing to export. Export file should not exist", false, File.Exists(exportFileName));

			try
			{
				exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942 " + Env.CurrentCompany.Code + " batch 1000.csv");
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
				SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.Inner.DeleteValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

				BizObj.ExportExistingBatch = true;
				BizObj.BatchNumber = 1000;
				Assert("Precondition: ", !BizObj.CreateAndExportBatch);
				BizObj.Factory.Save();
				AssertEquals("Expected export file should exist", true, File.Exists(exportFileName));
				if (Exporter.IsHighWaterMarkEnabled)
				{
					AssertEquals("High water mark registry value should not have been set because an existing batch was exported", DateTime.MinValue, SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.Value);
				}
				else
				{
					AssertEquals("High water mark registry value should not have been set because high water mark is not applicable", DateTime.MinValue, SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.Value);
				}

				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("\"TransactionType\",\"InvoiceDate\",\"PostDate\",\"DueDate\",\"Branch\",\"Department\",\"Ledger\",\"TransactionNum\",\"TransactionDesc\",\"GLAccount\",\"GLAccountDesc\",\"Job\",\"Account\",\"ChargeCode\",\"Period\",\"ReversePeriod\",\"Amount\",\"GSTAmount\",\"Debit\",\"Credit\",\"Balance\",\"IsLine\",\"TRPK\",\"OpeningPeriodDate\",\"ClosingPeriodDate\",\"OpeningBalance\",\"ClosingBalance\",\"IsControlTotal\",\"BankCode\",\"TaxID\",\"TaxIDType\",\"TaxIDRate\",\"TransactionDebtorOrCreditorGroup\",\"JobLocalClient\",\"JobLocalClientDebtorGroup\",\"TransactionDebtorExternalCreditRatingCode\",\"OriginalTransactionNumberForReversals\",\"OriginalTransactionNumberConsolidatedInvoiceRef\",\"TransactionNumberConsolidatedInvoiceRef\",\"Unused1\",\"Unused2\",\"TransactionHeaderBranch\",\"TransactionHeaderDepartment\",\"TransactionLineBranchOrgProxyCode\",\"TransactionHeaderBranchOrgProxyCode\",\"WasImportedFromExternalSystem\",\"PaymentReferenceNumber\",\"Currency\",\"ExRate\",\"OsAmount\",\"OsDebit\",\"OsCredit\",\"SubAccounts\",\"OrganisationSubAccount\",\"SalesExpenseGroupsSubAccount\",\"StaffAndResourcesSubAccount\",\"StaffGroupSubAccount\",\"Units\"\r\n\"\",\"\",\"\",\"\",\"\",\"\",\"AP \",\"\",\"AP Control Account\",\"8210.00.00\",\"TRADE CREDITORS CONTROL\",\"\",\"\",\"\",\"200603\",\"\",\"333.0000\",\"0.0000\",\"333.0000\",\"\",\"0.0000\",\"False\",\"\",\"\",\"\",\"\",\"\",\"True\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"AUD\",\"1.000000\",\"0.0000\",\"0.0000\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n\"INV\",\"29/03/2006 3:39:00 PM\",\"10/03/2006 12:00:00 AM\",\"29/03/2006 3:39:00 PM\",\"BNE\",\"BRN\",\"AP \",\"" + invoice.AH_TransactionNum + "\",\"Test Invoice\",\"6310.00.00\",\"INPUT TAX RECEIVABLE\",\"\",\"\",\"\",\"200603\",\"\",\"0.0000\",\"0.0000\",\"\",\"\",\"0.0000\",\"False\",\"@PK\",\"\",\"\",\"\",\"\",\"False\",\"          \",\"ZZGST1    \",\"RAT\",\"10\",\"\",\"\",\"\",\"\",\"\",\"\",\"00001000\",\"0\",\"0\",\"BNE\",\"BRN\",\"EDICUS      \",\"EDICUS      \",\"N\",\"\",\"AUD\",\"1.000000\",\"0.0000\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n\"INV\",\"29/03/2006 3:39:00 PM\",\"10/03/2006 12:00:00 AM\",\"29/03/2006 3:39:00 PM\",\"BNE\",\"BRN\",\"AP \",\"" + invoice.AH_TransactionNum + "\",\"Test Invoice\",\"7200.40.50\",\"TYPE WRITER - ACCUM DEPN.\",\"\",\"\",\"\",\"200603\",\"\",\"-333.0000\",\"0.0000\",\"\",\"333.0000\",\"0.0000\",\"False\",\"\",\"\",\"\",\"\",\"\",\"False\",\"          \",\"ZZGST1    \",\"RAT\",\"10\",\"\",\"\",\"\",\"\",\"\",\"\",\"00001000\",\"0\",\"0\",\"BNE\",\"BRN\",\"EDICUS      \",\"EDICUS      \",\"N\",\"\",\"AUD\",\"0.000000\",\"-333.0000\",\"\",\"333.0000\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n");
				stringBuilder.Replace("@AccountDescription", aPSuspenseControl.AG_Description);
				stringBuilder.Replace("@AccountNum", aPSuspenseControl.AG_AccountNum);
				stringBuilder.Replace("@PK", invoiceLine.PK.ToString());
				string expectedExportData = stringBuilder.ToString();
				string actualExportData = File.ReadAllText(exportFileName);

				AssertMultilineASCIIEquals("Exporter is not exporting what is expected", expectedExportData, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}
		}

		[TestDate(2009, 04, 06)]
		public void TestToFromDates()
		{
			GLTransactionBusinessObject bizo = new GLTransactionBusinessObject(Factory);
			PeriodTestHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));
			bizo.FromPeriod = 200603;
			bizo.ToPeriod = 200603;

			GLTransactionExporterForTest exporter = new GLTransactionExporterForTest(bizo, new NotificationBuffer());
			AssertEquals("From date", new ZDateTime(2006, 03, 1, 0, 0, 0), exporter.FromDateExposed);
			AssertEquals("To date", new ZDateTime(2006, 04, 1, 00, 00, 00), exporter.ToDateExposed);

			bizo.FromDate = new ZDateTime(2009, 4, 6, 4, 56, 0);
			bizo.ToDate = new ZDateTime(2009, 4, 6, 14, 56, 0);
			exporter = new GLTransactionExporterForTest(bizo, new NotificationBuffer());
			AssertEquals("From date", new ZDateTime(2009, 4, 6, 0, 0, 0), exporter.FromDateExposed);
			AssertEquals("To date", new ZDateTime(2009, 4, 7, 00, 00, 00), exporter.ToDateExposed);
		}

		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExportFromDifferentCompaniesWithTheSameBatchNumber()
		{
			BusinessObjectFactory factoryToCreateNewCompany = new BusinessObjectFactory();
			GlbCompany newCompany = factoryToCreateNewCompany.NewWithValidTestData<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Australia ? Constants.CountryCodes.UnitedKingdom : Constants.CountryCodes.Australia;
			GlbBranch newBranch = newCompany.Branches.AddNew();
			factoryToCreateNewCompany.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, newBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				ExportDataBatch();
			}
			ExportDataBatch();
		}

		void ExportDataBatch()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			PeriodTestHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));
			AccGLHeader aPSuspenseControl = newFactory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "7200.40.50"));
			ZGuid gLAccount = newFactory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, SQLComparisonOperator.Equal, "4710.00.00")).PK;

			TestObjectCreator testObjectCreator = new TestObjectCreator(newFactory);
			Invoice invoice = (Invoice)testObjectCreator.CreateInvoice(typeof(APInvoice), GlbCompany.CurrentCompany.LocalCurrency, GlbCompany.CurrentCompany.LocalCurrency.CurrentBuyRate);

			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControl.PK.ToGuid());

			invoice.AH_InvoiceAmount = 333m;
			invoice.AH_AG = gLAccount;
			invoice.AH_PostDate = new ZDateTime(2006, 3, 10);
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";
			invoice.AH_PostToGL = "Y";

			InvoiceLine invoiceLine = (InvoiceLine)testObjectCreator.CreateInvoiceLine(invoice, GlbCompany.CurrentCompany.LocalCurrency, GlbCompany.CurrentCompany.LocalCurrency.CurrentBuyRate, 333m);

			invoiceLine.AL_AG = gLAccount;
			invoiceLine.AL_LineAmount = 333m;
			invoiceLine.AL_OSAmount = 333m;
			invoiceLine.AL_PostDate = new ZDateTime(2006, 3, 10);
			AssertNotNull("reverse should be set when post is set due to new reve recognition", invoiceLine.AL_ReverseDate);

			testObjectCreator.Factory.Save();

			GLTransactionBusinessObject bizObj = new GLTransactionBusinessObject(newFactory);
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;

			string exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942 " + Env.CurrentCompany.Code + " batch 1000.csv");

			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));

				bizObj.CreateAndExportBatch = true;
				bizObj.Exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				bizObj.Factory.Save();
				AssertEquals("Expected export file should exist", true, File.Exists(exportFileName));
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}
		}

		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExportDataWithColumnsContainingSingleDoubleQuote()
		{
			GlbBranch branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			branch.GB_Code = "B\"E";
			GlbDepartment department = Factory.Load<GlbDepartment>(GlbDepartment.CurrentDepartment.PK);
			department.GE_Code = "B\"N";
			Factory.Save();

			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			PeriodTestHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));
			ZGuid gLAccountFrom = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, "1000.00.00")).PK;
			ZGuid gLAccountTo = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, "9999.00.00")).PK;
			AccGLHeader aPSuspenseControl = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "7200.40.50"));
			aPSuspenseControl.AG_Description = aPSuspenseControl.AG_Description + "40\" X 35\"";
			Factory.Save();
			ZGuid gLAccount = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, SQLComparisonOperator.Equal, "4710.00.00")).PK;

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			Invoice invoice = (Invoice)testObjectCreator.CreateInvoice(typeof(APInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1m);

			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControl.PK.ToGuid());

			AccChargeCode chargeCode = testObjectCreator.CC1;
			chargeCode.AC_Code = "ZZ\"CC1";

			ForwardingShipment jobShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			jobShipment.JS_UniqueConsignRef = "S000\"1000";

			Job job = new Job.Loader(Factory, jobShipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			invoice.AH_Desc = "Test 40\" X 35\"";
			invoice.AH_InvoiceAmount = 333m;
			invoice.AH_AG = gLAccount;
			invoice.AH_PostDate = new ZDateTime(2006, 3, 10);
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";
			invoice.AH_TransactionNum = "0001 40\" X 35\"";
			invoice.AH_PostToGL = "Y";
			invoice.AH_GB = branch.PK;
			invoice.AH_GE = department.PK;

			InvoiceLine invoiceLine = (InvoiceLine)testObjectCreator.CreateInvoiceLine(invoice, GlbCompany.CurrentCompany.LocalCurrency, 1m, 333m);

			invoiceLine.AL_AG = gLAccount;
			invoiceLine.AL_PostDate = new ZDateTime(2006, 3, 10);
			AssertNotNull("reverse should be set when post is set due to new reve recognition", invoiceLine.AL_ReverseDate);

			invoice.Factory.Save();

			Invoice invoice2 = (Invoice)testObjectCreator.CreateInvoice(typeof(APInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1m);

			OrgHeader orgHeader = testObjectCreator.AALSHI;
			orgHeader.OH_Code = "A\"LSHI";

			invoice2.AH_InvoiceAmount = 222m;
			invoice2.AH_PostDate = new ZDateTime(2006, 3, 10);
			invoice2.AH_Ledger = "AP";
			invoice2.AH_TransactionType = "INV";
			invoice2.AH_PostToGL = "Y";
			invoice2.AH_OH = orgHeader.PK;

			InvoiceLine invoiceLine2 = (InvoiceLine)testObjectCreator.CreateInvoiceLine(invoice2, GlbCompany.CurrentCompany.LocalCurrency, 1m, 222m);
			invoiceLine2.AL_JH = job.PK;
			invoiceLine2.AL_AC = chargeCode.PK;
			invoiceLine2.AL_PostDate = new ZDateTime(2006, 3, 10);
			invoiceLine2.AL_RevRecognitionType = "IMM";
			invoiceLine2.AL_ReverseDate = invoiceLine2.AL_PostDate;
			AssertNotNull("reverse should be set when post is set due to new revenue recognition", invoiceLine2.AL_ReverseDate);

			JobCharge jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_AL_APLine = invoiceLine2.PK;
			jobCharge.JR_AC = chargeCode.PK;
			jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge.JR_JH = job.PK;
			jobCharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
			jobCharge.JR_OSCostExRate = 1m;
			jobCharge.JR_OSCostAmt = 222m;
			jobCharge.SetAmountsFromLinkedLinesForTests();

			Factory.Save();

			GLTransactionBusinessObject bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.FromPeriod = 200603;
			bizObj.ToPeriod = 200603;
			bizObj.StartGLAccountPK = gLAccountFrom;
			bizObj.EndGLAccountPK = gLAccountTo;
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;

			string exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20060329153942.csv");

			try
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));

				GLTransactionExporter exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Expected export file should exist", true, File.Exists(exportFileName));

				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("\"TransactionType\",\"InvoiceDate\",\"PostDate\",\"DueDate\",\"Branch\",\"Department\",\"Ledger\",\"TransactionNum\",\"TransactionDesc\",\"GLAccount\",\"GLAccountDesc\",\"Job\",\"Account\",\"ChargeCode\",\"Period\",\"ReversePeriod\",\"Amount\",\"GSTAmount\",\"Debit\",\"Credit\",\"Balance\",\"IsLine\",\"TRPK\",\"OpeningPeriodDate\",\"ClosingPeriodDate\",\"OpeningBalance\",\"ClosingBalance\",\"IsControlTotal\",\"BankCode\",\"TaxID\",\"TaxIDType\",\"TaxIDRate\",\"TransactionDebtorOrCreditorGroup\",\"JobLocalClient\",\"JobLocalClientDebtorGroup\",\"TransactionDebtorExternalCreditRatingCode\",\"OriginalTransactionNumberForReversals\",\"OriginalTransactionNumberConsolidatedInvoiceRef\",\"TransactionNumberConsolidatedInvoiceRef\",\"Unused1\",\"Unused2\",\"TransactionHeaderBranch\",\"TransactionHeaderDepartment\",\"TransactionLineBranchOrgProxyCode\",\"TransactionHeaderBranchOrgProxyCode\",\"WasImportedFromExternalSystem\",\"PaymentReferenceNumber\",\"Currency\",\"ExRate\",\"OsAmount\",\"OsDebit\",\"OsCredit\",\"SubAccounts\",\"OrganisationSubAccount\",\"SalesExpenseGroupsSubAccount\",\"StaffAndResourcesSubAccount\",\"StaffGroupSubAccount\",\"Units\"\r\n");
				stringBuilder.Append("\"\",\"\",\"\",\"\",\"\",\"\",\"AP \",\"\",\"AP Control Account\",\"8210.00.00\",\"TRADE CREDITORS CONTROL\",\"\",\"\",\"\",\"200603\",\"\",\"-610.5000\",\"0.0000\",\"\",\"610.5000\",\"0.0000\",\"False\",\"\",\"1/03/2006 12:00:00 AM\",\"31/03/2006 11:59:00 PM\",\"0.0000\",\"0.0000\",\"True\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"AUD\",\"1.000000\",\"0.0000\",\"\",\"0.0000\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n");
				stringBuilder.Append("\"INV\",\"29/03/2006 3:39:00 PM\",\"10/03/2006 12:00:00 AM\",\"29/03/2006 3:39:00 PM\",\"B\"\"E\",\"B\"\"N\",\"AP \",\"0001 40\"\" X 35\"\"\",\"Test 40\"\" X 35\"\"\",\"6310.00.00\",\"INPUT TAX RECEIVABLE\",\"\",\"\",\"\",\"200603\",\"\",\"33.3000\",\"0.0000\",\"33.3000\",\"\",\"0.0000\",\"False\",\"@PK\",\"1/03/2006 12:00:00 AM\",\"31/03/2006 11:59:00 PM\",\"0.0000\",\"0.0000\",\"False\",\"          \",\"ZZGST1    \",\"RAT\",\"10\",\"\",\"\",\"\",\"\",\"\",\"\",\"00001000\",\"0\",\"0\",\"B\"E\",\"B\"N\",\"EDICUS      \",\"EDICUS      \",\"N\",\"\",\"AUD\",\"1.000000\",\"0.0000\",\"0.0000\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n");
				stringBuilder.Append("\"INV\",\"29/03/2006 3:39:00 PM\",\"10/03/2006 12:00:00 AM\",\"29/03/2006 3:39:00 PM\",\"B\"\"E\",\"B\"\"N\",\"AP \",\"" + invoice2.AH_TransactionNum + "\",\"Test Invoice\",\"6310.00.00\",\"INPUT TAX RECEIVABLE\",\"S000\"\"1000\",\"A\"\"LSHI\",\"ZZ\"\"CC1\",\"200603\",\"\",\"22.2000\",\"0.0000\",\"22.2000\",\"\",\"0.0000\",\"False\",\"@InvoiceLine2PK\",\"1/03/2006 12:00:00 AM\",\"31/03/2006 11:59:00 PM\",\"0.0000\",\"0.0000\",\"False\",\"          \",\"ZZGST1    \",\"RAT\",\"10\",\"\",\"\",\"\",\"\",\"\",\"\",\"00001001\",\"0\",\"0\",\"B\"E\",\"B\"N\",\"EDICUS      \",\"EDICUS      \",\"N\",\"\",\"AUD\",\"1.000000\",\"0.0000\",\"0.0000\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n");
				stringBuilder.Append("\"INV\",\"29/03/2006 3:39:00 PM\",\"10/03/2006 12:00:00 AM\",\"29/03/2006 3:39:00 PM\",\"B\"\"E\",\"B\"\"N\",\"AP \",\"0001 40\"\" X 35\"\"\",\"Test 40\"\" X 35\"\"\",\"@AccountNum\",\"@AccountDescription\",\"\",\"\",\"\",\"200603\",\"\",\"333.0000\",\"33.3000\",\"333.0000\",\"\",\"0.0000\",\"False\",\"\",\"1/03/2006 12:00:00 AM\",\"31/03/2006 11:59:00 PM\",\"0.0000\",\"0.0000\",\"False\",\"          \",\"ZZGST1    \",\"RAT\",\"10\",\"\",\"\",\"\",\"\",\"\",\"\",\"00001000\",\"0\",\"0\",\"B\"E\",\"B\"N\",\"EDICUS      \",\"EDICUS      \",\"N\",\"\",\"AUD\",\"1.000000\",\"366.3000\",\"366.3000\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n");
				stringBuilder.Append("\"INV\",\"29/03/2006 3:39:00 PM\",\"10/03/2006 12:00:00 AM\",\"29/03/2006 3:39:00 PM\",\"B\"\"E\",\"B\"\"N\",\"AP \",\"" + invoice2.AH_TransactionNum + "\",\"Test Invoice\",\"@AccountNum\",\"TYPE WRITER - ACCUM DEPN.40\"\" X 35\"\"\",\"S000\"\"1000\",\"A\"\"LSHI\",\"ZZ\"\"CC1\",\"200603\",\"\",\"222.0000\",\"22.2000\",\"222.0000\",\"\",\"0.0000\",\"False\",\"\",\"1/03/2006 12:00:00 AM\",\"31/03/2006 11:59:00 PM\",\"0.0000\",\"0.0000\",\"False\",\"          \",\"ZZGST1    \",\"RAT\",\"10\",\"\",\"\",\"\",\"\",\"\",\"\",\"00001001\",\"0\",\"0\",\"B\"E\",\"B\"N\",\"EDICUS      \",\"EDICUS      \",\"N\",\"\",\"AUD\",\"1.000000\",\"244.2000\",\"244.2000\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n");
				stringBuilder.Replace("@AccountDescription", aPSuspenseControl.AG_Description.Replace("\"", "\"\""));
				stringBuilder.Replace("@AccountDescription", aPSuspenseControl.AG_Description.Replace("\"", "\"\""));
				stringBuilder.Replace("@AccountNum", aPSuspenseControl.AG_AccountNum);
				stringBuilder.Replace("@PK", invoiceLine.PK.ToString());
				stringBuilder.Replace("@InvoiceLine2PK", invoiceLine2.PK.ToString());
				string expectedExportData = stringBuilder.ToString();
				string actualExportData = File.ReadAllText(exportFileName);

				AssertMultilineASCIIEquals("Exporter is not exporting what is expected", expectedExportData.Trim(), actualExportData.Trim());
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}
		}

		public void TestCanSaveAndUsingHighWaterMark()
		{
			AssertEquals("High water mark cannot be saved because exporter is not creating a batch", false, Exporter.CanSaveHighWaterMark);

			BizObj.CreateAndExportBatch = true;
			if (Exporter.IsHighWaterMarkEnabled)
			{
				AssertEquals("High water mark can be saved because exporter is creating a batch", true, Exporter.CanSaveHighWaterMark);
			}
			else
			{
				AssertEquals("High water mark cannot be saved because high water mark is not allowed", false, Exporter.CanSaveHighWaterMark);
			}

			BizObj.BranchPK = GlbBranch.CurrentBranch.PK;
			AssertEquals("High water mark cannot be saved because a restrictive filter was set", false, Exporter.CanSaveHighWaterMark);

			BizObj.BranchPK = ZGuid.Empty;

			if (Exporter.IsHighWaterMarkEnabled)
			{
				AssertEquals("High water mark can be saved", true, Exporter.CanSaveHighWaterMark);
			}
			else
			{
				AssertEquals("High water mark cannot be saved because high water mark is not allowed", false, Exporter.CanSaveHighWaterMark);
			}

			AssertEquals("High water mark cannot be used because registry is not set", false, Exporter.UsingHighWaterMark);

			using (SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.DataType.SuspendValidation())
			{
				SystemDataRegistry.Instance.GLTransactionsCSVExportHighWaterMark.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcNow.ToDateTime());
			}
			if (Exporter.IsHighWaterMarkEnabled)
			{
				AssertEquals("High water mark can be used because registry is set", true, Exporter.UsingHighWaterMark);
			}
			else
			{
				AssertEquals("High water mark cannot be saved because high water mark is not allowed", false, Exporter.CanSaveHighWaterMark);
			}
		}

		void PrepareMiscellaneousTransactionForSaving(TransactionHeader miscHeader)
		{
			AccTransactionHeader journalToMatch = miscHeader.Factory.New<AccTransactionHeader>();

			journalToMatch.AH_InvoiceDate = DateTime.Now;
			journalToMatch.AH_Ledger = LedgerTypes.AccountsReceivable;
			journalToMatch.AH_TransactionType = TransactionTypes.Journal;
			journalToMatch.AH_InvoiceAmount = -miscHeader.AH_InvoiceAmount;
			journalToMatch.AH_OutstandingAmount = -miscHeader.AH_OutstandingAmount;
			journalToMatch.AH_PostDate = miscHeader.AH_PostDate;
			journalToMatch.AH_GB = miscHeader.AH_GB;
			journalToMatch.AH_GE = miscHeader.AH_GE;
			journalToMatch.AH_TransactionNum = miscHeader.AH_TransactionNum + "J";
			journalToMatch.AH_PostToGL = "N";

			AccTransactionMatchLink link1 = miscHeader.Factory.New<AccTransactionMatchLink>();
			AccTransactionMatchLink link2 = miscHeader.Factory.New<AccTransactionMatchLink>();

			link1.AP_AH = miscHeader.PK;
			link1.AP_Amount = miscHeader.AH_OutstandingAmount;
			link2.AP_AH = journalToMatch.PK;
			link2.AP_Amount = journalToMatch.AH_OutstandingAmount;
			link1.AP_MatchDate = link2.AP_MatchDate = DateTime.Now;
			link1.AP_MatchGroupNum = link1.AP_MatchGroupNum = "100100";

			TransactionMatchLinkGroup matchlinks = new TransactionMatchLinkGroup(Factory);
			matchlinks.Add(link1);
			matchlinks.Add(link2);
			miscHeader.AH_OutstandingAmount = 0m;
			journalToMatch.AH_OutstandingAmount = 0m;
		}

		GLTransactionExporter Exporter
		{
			get
			{
				return new GLTransactionExporter(BizObj, NotificationBuffer);
			}
		}

		GLTransactionBusinessObject BizObj
		{
			get
			{
				if (fBizObj == null)
				{
					fBizObj = new GLTransactionBusinessObject(Factory);
				}
				return fBizObj;
			}
		}
		GLTransactionBusinessObject fBizObj;

		NotificationBuffer NotificationBuffer
		{
			get
			{
				if (fNotificationBuffer == null)
				{
					fNotificationBuffer = new NotificationBuffer();
				}
				return fNotificationBuffer;
			}
		}
		NotificationBuffer fNotificationBuffer;

		AccountingPeriodTestHelper PeriodTestHelper
		{
			get { return periodTestHelper ?? (periodTestHelper = new AccountingPeriodTestHelper()); }
		}
		AccountingPeriodTestHelper periodTestHelper;

		TaxFrameworkTestObjectCreator TaxFrameworkTestHelper
		{
			get { return testHelper ?? (testHelper = new TaxFrameworkTestObjectCreator(Factory)); }
		}
		TaxFrameworkTestObjectCreator testHelper;

		AccGLHeader APControlAccount;
		AccGLHeader ARControlAccount;
		AccGLHeader TaxControlAccount;
		AccGLHeader PendingTaxControlAccount;

		#region Tax Transactions

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		[SuspendCriticalValidation]
		public void TestExportTaxTransactions_ARInvoice()
		{
			ARControlAccount = Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.ARControlAccount.Value);
			TaxControlAccount = TestObjectCreator.CreatePrepaidAssetTaxControlAccount();            //"6350.00.00", "OTHER TAX - REALISED PREPAID TAX"
			PendingTaxControlAccount = TestObjectCreator.CreatePendingPrepaidTaxControlAccount();   //"6355.00.00", "OTHER TAX - PENDING PREPAID TAX"

			var period = new ZInt("200603");
			var postdate = ZDate.Today;
			var matchdate = ZDate.Today.AddDays(2);

			TestObjectCreator.Debtor.CustomsCodes.AddNew("ECR", "ECR123", GlbCompany.CurrentCompany.Country);

			var orgDebtorGroup = TestObjectCreator.CreateDebtorGroup();
			orgDebtorGroup.OJ_Code = "OG1";
			TestObjectCreator.Debtor.CompanyData.OB_OJ_ARDebtorGroup = orgDebtorGroup.PK;

			var localClientDebtorGroup = TestObjectCreator.CreateDebtorGroup();
			localClientDebtorGroup.OJ_Code = "OG2";
			TestObjectCreator.LocalClient.CompanyData.OB_OJ_ARDebtorGroup = localClientDebtorGroup.PK;

			var originalInvoice = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "Original Transaction", TestObjectCreator.AUD, 1, TestObjectCreator.Debtor);
			originalInvoice.AH_ConsolidatedInvoiceRef = "654321";

			var job = TestObjectCreator.CreateJob("S001001", TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "Export Test", TestObjectCreator.AUD, 1, TestObjectCreator.Debtor);
			invoice.AH_ConsolidatedInvoiceRef = "123456";
			invoice.AH_PostDate = ZDateTime.Today.AddDays(-30);
			invoice.AH_JH = job.PK;
			invoice.AH_TransactionBelongsToGroup = originalInvoice.PK;

			TestObjectCreator.NonCurrentBranch.GB_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;

			var taxTransaction1 = TaxFrameworkTestHelper.CreateTaxTransaction(new CreateTaxTransactionParameters { CompanyPK = GlbCompany.CurrentCompany.PK, BranchPK = TestObjectCreator.NonCurrentBranch.PK, TransactionHeaderPK = invoice.PK, OsTaxAmount = 12M, Ledger = TaxConfigurationLedgers.AccountsReceivable.Code });
			var taxTransaction2 = TaxFrameworkTestHelper.CreateTaxTransaction(new CreateTaxTransactionParameters { CompanyPK = GlbCompany.CurrentCompany.PK, TransactionHeaderPK = invoice.PK, OsTaxAmount = 24M, Ledger = TaxConfigurationLedgers.AccountsReceivable.Code });
			var taxTransaction3 = TaxFrameworkTestHelper.CreateTaxTransaction(new CreateTaxTransactionParameters { CompanyPK = GlbCompany.CurrentCompany.PK, TransactionHeaderPK = invoice.PK, OsTaxAmount = 36M, Ledger = TaxConfigurationLedgers.AccountsReceivable.Code });

			TaxFrameworkTestHelper.CreateAccTaxGLMovement(taxTransaction1.PK, ARControlAccount.PK, TaxControlAccount.PK, 10M, period, postdate);
			TaxFrameworkTestHelper.CreateAccTaxGLMovement(taxTransaction2.PK, TaxControlAccount.PK, ARControlAccount.PK, 20M, period, matchdate);
			TaxFrameworkTestHelper.CreateAccTaxGLMovement(taxTransaction3.PK, ARControlAccount.PK, PendingTaxControlAccount.PK, 30M, period, postdate, "PND");
			TaxFrameworkTestHelper.CreateAccTaxGLMovement(taxTransaction3.PK, PendingTaxControlAccount.PK, TaxControlAccount.PK, 30M, period, postdate, "RLS");

			Factory.Save();

			AssertExportData_TaxFramework(period, postdate, matchdate, invoice);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		[SuspendCriticalValidation]
		public void TestExportTaxTransactions_APInvoice()
		{
			APControlAccount = Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.APControlAccount.Value);
			TaxControlAccount = TestObjectCreator.CreatePrepaidAssetTaxControlAccount();            //"6350.00.00", "OTHER TAX - REALISED PREPAID TAX"
			PendingTaxControlAccount = TestObjectCreator.CreatePendingPrepaidTaxControlAccount();   //"6355.00.00", "OTHER TAX - PENDING PREPAID TAX"

			var period = new ZInt("200606");
			var postdate = ZDate.Today;
			var matchdate = ZDate.Today.AddDays(2);

			TestObjectCreator.Creditor1.CustomsCodes.AddNew("ECR", "ECR123", GlbCompany.CurrentCompany.Country);

			var orgCreditorGroup = TestObjectCreator.CreateCreditorGroup();
			orgCreditorGroup.OG_Code = "OG1";
			TestObjectCreator.Creditor1.CompanyData.OB_OG_APCreditorGroup = orgCreditorGroup.PK;

			var originalInvoice = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "Original Transaction", TestObjectCreator.AUD, 1, TestObjectCreator.Creditor1);
			originalInvoice.AH_ConsolidatedInvoiceRef = "654321";

			var job = TestObjectCreator.CreateJob("S001001", TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "Export Test", TestObjectCreator.AUD, 1, TestObjectCreator.Creditor1);
			invoice.AH_ConsolidatedInvoiceRef = "123456";
			invoice.AH_PostDate = ZDateTime.Today.AddDays(-30);
			invoice.AH_JH = job.PK;
			invoice.AH_TransactionBelongsToGroup = originalInvoice.PK;

			TestObjectCreator.NonCurrentBranch.GB_OH_OrgProxy = TestObjectCreator.ABIGAS.PK;

			var journal = TestObjectCreator.CreateJournal<APJournal>(5M, matchdate, TestObjectCreator.Creditor1.PK);

			var taxTransaction1 = TaxFrameworkTestHelper.CreateTaxTransaction(new CreateTaxTransactionParameters { CompanyPK = GlbCompany.CurrentCompany.PK, BranchPK = TestObjectCreator.NonCurrentBranch.PK, TransactionHeaderPK = invoice.PK, OsTaxAmount = 12M });
			var taxTransaction2 = TaxFrameworkTestHelper.CreateTaxTransaction(new CreateTaxTransactionParameters { CompanyPK = GlbCompany.CurrentCompany.PK, TransactionHeaderPK = invoice.PK, OsTaxAmount = 24M, MatchTransaction = journal });
			var taxTransaction3 = TaxFrameworkTestHelper.CreateTaxTransaction(new CreateTaxTransactionParameters { CompanyPK = GlbCompany.CurrentCompany.PK, TransactionHeaderPK = invoice.PK, OsTaxAmount = 36M });

			TaxFrameworkTestHelper.CreateAccTaxGLMovement(taxTransaction1.PK, APControlAccount.PK, TaxControlAccount.PK, 10M, period, postdate);
			TaxFrameworkTestHelper.CreateAccTaxGLMovement(taxTransaction2.PK, TaxControlAccount.PK, APControlAccount.PK, 20M, period, matchdate);
			TaxFrameworkTestHelper.CreateAccTaxGLMovement(taxTransaction3.PK, APControlAccount.PK, PendingTaxControlAccount.PK, 30M, period, postdate, "PND");
			TaxFrameworkTestHelper.CreateAccTaxGLMovement(taxTransaction3.PK, PendingTaxControlAccount.PK, TaxControlAccount.PK, 30M, period, postdate, "RLS");

			Factory.Save();

			AssertExportData_TaxFramework(period, postdate, matchdate, invoice);
		}

		void AssertExportData_TaxFramework(ZInt period, ZDate postdate, ZDate matchdate, InvoicingBase invoice)
		{
			var firstGLAccount = Factory.LoadFromUniqueKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, new ZString("1000.00.00")).PK;
			var lastGLAccount = Factory.LoadFromUniqueKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, new ZString("9999.00.00")).PK;

			TestConnection.ExecuteNonQuery("DELETE FROM dbo.AccTaxGLMovementQueue");

			PeriodTestHelper.SetupSinglePeriod(period, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));

			GLTransactionBusinessObject bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.FromDate = postdate;
			bizObj.ToDate = postdate;
			bizObj.StartGLAccountPK = firstGLAccount;
			bizObj.EndGLAccountPK = lastGLAccount;
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.DescriptionDisplay = "H";
			bizObj.CreateAndExportBatch = true;
			bizObj.ExportExistingBatch = false;
			bizObj.BatchNumber = 1001;

			var subFolder = invoice.AH_Ledger + invoice.AH_TransactionType;
			var expectedFileName = "TF_BatchOnPostDate";
			var fileNameSuffix = "";
			AssertExport(bizObj, expectedFileName, fileNameSuffix, subFolder);

			var exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
			bizObj.CreateAndExportBatch = true;
			bizObj.ExportExistingBatch = false;
			bizObj.BatchNumber = 1002;
			Assert("Nothing to export here", !exporter.ExportData());

			expectedFileName = "TF_BatchOnMatchDate";
			bizObj.FromDate = matchdate;
			bizObj.ToDate = matchdate;
			bizObj.CreateAndExportBatch = true;
			bizObj.ExportExistingBatch = false;
			bizObj.BatchNumber = 1002;
			AssertExport(bizObj, expectedFileName, fileNameSuffix, subFolder);

			expectedFileName = "TF_BatchOnPostDate";
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = true;
			bizObj.BatchNumber = 1001;
			AssertExport(bizObj, expectedFileName, fileNameSuffix, subFolder);

			expectedFileName = "TF_BatchOnMatchDate";
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = true;
			bizObj.BatchNumber = 1002;
			AssertExport(bizObj, expectedFileName, fileNameSuffix, subFolder);

			expectedFileName = "TF_WithLineDescription";
			bizObj.BatchNumber = 0;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = false;
			bizObj.DescriptionDisplay = "L";
			AssertExport(bizObj, expectedFileName, fileNameSuffix, subFolder);
		}

		#endregion

		void AssertExportData(Func<TestObjectCreator, TransactionHeader> getTransaction, string fileNameSuffix = "")
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			PeriodTestHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));
			ZGuid glAccountFrom = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, "1000.00.00")).PK;
			ZGuid glAccountTo = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, "9999.00.00")).PK;
			ZGuid glAccount = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, SQLComparisonOperator.Equal, "4710.00.00")).PK;
			AccGLHeader apSuspenseControl = testObjectCreator.CreateAccGLHeader("6999.10.10", "TS", "AP Suspense", "BSH", "CR");
			AccGLHeader arSuspenseControl = testObjectCreator.CreateAccGLHeader("8999.10.10", "TS", "AR Suspense", "BSH", "DR");
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apSuspenseControl.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arSuspenseControl.PK.ToGuid());

			testObjectCreator.GLHeader1.AG_Description = "GL Header 1";
			testObjectCreator.GLHeader1.AG_AccountNum = "GL 1 Num";
			testObjectCreator.GLHeader2.AG_Description = "GL Header 2";
			testObjectCreator.GLHeader2.AG_AccountNum = "GL 2 Num";

			var transaction = getTransaction(testObjectCreator);
			transaction.AH_PostToGL = "Y";
			transaction.AH_TransactionNum = "Export Test";
			Factory.Save();
			var transactionWithLines = transaction as TransactionHeaderWithLines;
			if (transactionWithLines != null)
			{
				foreach (TransactionLine line in transactionWithLines.Lines)
				{
					if (!line.AL_ReverseDate.IsEmpty)
					{
						line.AL_ReverseToGL = "Y";
					}
				}
			}
			Factory.Save();

			GLTransactionBusinessObject bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.FromPeriod = 200603;
			bizObj.ToPeriod = 200603;
			bizObj.StartGLAccountPK = glAccountFrom;
			bizObj.EndGLAccountPK = glAccountTo;
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;
			bizObj.DescriptionDisplay = "H";
			bizObj.CreateAndExportBatch = true;
			bizObj.ExportExistingBatch = false;
			bizObj.BatchNumber = 7;

			var subFolder = transaction.AH_Ledger + transaction.AH_TransactionType;
			var expectedFileName = "Batch";
			AssertExport(bizObj, expectedFileName, fileNameSuffix, subFolder);

			var exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
			bizObj.CreateAndExportBatch = true;
			bizObj.ExportExistingBatch = false;
			bizObj.BatchNumber = 8;
			Assert("Nothing to export here", !exporter.ExportData());

			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = true;
			bizObj.BatchNumber = 7;
			AssertExport(bizObj, expectedFileName, fileNameSuffix, subFolder);

			bizObj.BatchNumber = 0;
			bizObj.CreateAndExportBatch = false;
			bizObj.ExportExistingBatch = false;

			AssertExport(bizObj, "NonBatch", fileNameSuffix, subFolder);

			bizObj.DescriptionDisplay = "L";
			AssertExport(bizObj, "WithLineDesc", fileNameSuffix, subFolder);
		}

		void AssertExport(GLTransactionBusinessObject bizObj, string expectedFileName, string expectedFileNameSuffix, string subFolderName)
		{
			string batchNumber = null;
			if (bizObj.CreateAndExportBatch || bizObj.ExportExistingBatch)
			{
				batchNumber = bizObj.BatchNumber.ToString();
			}
			string exportFileName = Path.Combine(EnvProxy.Instance.TempPath, string.Format("GLTransactions20060329153942{0}.csv", batchNumber == null ? "" : string.Format(" {0} batch {1}", Env.CurrentCompany.Code, batchNumber)));
			try
			{
				AssertEquals(string.Format("Expected export file should not exist. {0}", exportFileName), false, File.Exists(exportFileName));

				var exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals(string.Format("Expected export file should exist. {0}", exportFileName), true, File.Exists(exportFileName));

				var expectedFilePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing", subFolderName, expectedFileName + expectedFileNameSuffix + ".csv");
				string expectedExportData = File.ReadAllText(expectedFilePath);

				string actualExportData = File.ReadAllText(exportFileName);
				actualExportData = ReplacePKs(actualExportData, Factory);
				AssertMultilineASCIIEquals(string.Format("Exporter is not exporting what is expected. {0}", expectedFileName), expectedExportData, actualExportData);
			}
			finally
			{
				DeleteIfExists(exportFileName);
			}
		}

		[TestDate(2020, 03, 23, 16, 38, 41)]
		public void TestExportDataForNoteJournal()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			PeriodTestHelper.SetupSinglePeriod(202003, new ZDateTime(2020, 3, 1), new ZDateTime(2020, 3, 31, 23, 59, 00));
			ZGuid gLAccountFrom = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, "1000.00.00")).PK;
			ZGuid gLAccountTo = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, "9999.00.00")).PK;

			var glHeader1 = TestObjectCreator.CreateGLHeader();
			glHeader1.AG_AccountNum = "999.124.11";
			glHeader1.AG_AccountType = Constants.AccountType.Note;
			glHeader1.AG_Description = "GL Header 1";

			var noteJournal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLNoteJournal, ZDateTime.Today, ZDateTime.Today);
			var line1 = noteJournal.GLJournalLines.AddNew();
			line1.AL_AG = glHeader1.PK;
			line1.UnitQuantity = 100m;
			Factory.Save();

			var bizObj = new GLTransactionBusinessObject(Factory);
			bizObj.FromPeriod = 202003;
			bizObj.ToPeriod = 202003;
			bizObj.StartGLAccountPK = gLAccountFrom;
			bizObj.EndGLAccountPK = gLAccountTo;
			bizObj.ExportDirectory = EnvProxy.Instance.TempPath;

			string exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GLTransactions20200323163841.csv");

			using (new DisposableAction(() => { DeleteIfExists(exportFileName); }))
			{
				AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));

				var exporter = new GLTransactionExporter(bizObj, new NotificationBuffer());
				Assert(exporter.ExportData());
				AssertEquals("Expected export file should exist", true, File.Exists(exportFileName));

				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("\"TransactionType\",\"InvoiceDate\",\"PostDate\",\"DueDate\",\"Branch\",\"Department\",\"Ledger\",\"TransactionNum\",\"TransactionDesc\",\"GLAccount\",\"GLAccountDesc\",\"Job\",\"Account\",\"ChargeCode\",\"Period\",\"ReversePeriod\",\"Amount\",\"GSTAmount\",\"Debit\",\"Credit\",\"Balance\",\"IsLine\",\"TRPK\",\"OpeningPeriodDate\",\"ClosingPeriodDate\",\"OpeningBalance\",\"ClosingBalance\",\"IsControlTotal\",\"BankCode\",\"TaxID\",\"TaxIDType\",\"TaxIDRate\",\"TransactionDebtorOrCreditorGroup\",\"JobLocalClient\",\"JobLocalClientDebtorGroup\",\"TransactionDebtorExternalCreditRatingCode\",\"OriginalTransactionNumberForReversals\",\"OriginalTransactionNumberConsolidatedInvoiceRef\",\"TransactionNumberConsolidatedInvoiceRef\",\"Unused1\",\"Unused2\",\"TransactionHeaderBranch\",\"TransactionHeaderDepartment\",\"TransactionLineBranchOrgProxyCode\",\"TransactionHeaderBranchOrgProxyCode\",\"WasImportedFromExternalSystem\",\"PaymentReferenceNumber\",\"Currency\",\"ExRate\",\"OsAmount\",\"OsDebit\",\"OsCredit\",\"SubAccounts\",\"OrganisationSubAccount\",\"SalesExpenseGroupsSubAccount\",\"StaffAndResourcesSubAccount\",\"StaffGroupSubAccount\",\"Units\"\r\n\"NJL\",\"23/03/2020 4:38:00 PM\",\"31/03/2020 11:59:00 PM\",\"\",\"BNE\",\"BRN\",\"GL \",\"00001000\",\"NOTE JOURNAL\",\"999.124.11\",\"GL Header 1\",\"\",\"\",\"\",\"202003\",\"0\",\"100.0000\",\"0.0000\",\"100.0000\",\"\",\"0.0000\",\"False\",\"\",\"1/03/2020 12:00:00 AM\",\"31/03/2020 11:59:00 PM\",\"0.0000\",\"0.0000\",\"False\",\"          \",\"          \",\"   \",\"0\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"0\",\"0\",\"BNE\",\"BRN\",\"EDICUS      \",\"EDICUS      \",\"N\",\"\",\"AUD\",\"1.000000\",\"100.0000\",\"100.0000\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"\r\n");

				string expectedExportData = stringBuilder.ToString();
				string actualExportData = File.ReadAllText(exportFileName);

				AssertMultilineASCIIEquals("Exporter is not exporting what is expected", expectedExportData, actualExportData);
			}
		}

		TestObjectCreator TestObjectCreator => fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator fTestObjectCreator;

		class GLTransactionExporterForTest : GLTransactionExporter
		{
			public GLTransactionExporterForTest(GLTransactionBusinessObject bizObj, NotificationBuffer notification) : base(bizObj, notification) { }

			public ZDateTime FromDateExposed
			{
				get { return base.FromDate; }
			}

			public ZDateTime ToDateExposed
			{
				get { return base.ToDate; }
			}

			public ZString HeadingLineExposed
			{
				get { return base.HeadingLine; }
			}
		}
	}
}
