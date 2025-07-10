using System;
using System.Data.Common;
using System.IO;
using System.Text;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Export.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ResourceStrings.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.DataTransfer.AccBatchRequest.Testing
{
	sealed class AccountingTransactionEAdaptorExporterTest : TestCaseWithFactory
	{
		public void TestRollBackOnZombieTransaction()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForRollBackTest(dataAccess);

			AssertNotNull("Pre-condition", Transaction.Connection);

			var response = exporter.CreateBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			AssertNull("Transaction should be rolled back, so connection should be null.", Transaction.Connection);
			AssertContains("Real error message can be catched.", "Test Error for RollBack.", response.ErrorMessage);

			AssertNoExceptionThrown("No exception should be thrown when trying to rollback on a zombie transaction.", () =>
			{
				exporter.CreateBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			});
		}

		public void TestCreateBatch()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
			var batchResponse = exporter.CreateBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("Nothing to be batched.", batchResponse.InfoMessage);

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, Factory.Load<EDIMessage>(new ZQuery()).Length);

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"), false);
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice.Lines[0].AL_JH = job.PK;
			TestObjectCreator.CreateCharge(invoice.Lines[0]).JR_OSSellAmt = 0;
			Factory.Save();

			batchResponse = exporter.CreateBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("New batch 1 is created", batchResponse.InfoMessage);
			AssertEquals(1, batchResponse.CreatedBatchNumber);
			var batch = batchResponse.DataObject as TransactionBatch;
			AssertNotNull(batch);
			AssertBatch(batch, "AccBatchRequests_CRE1.xml");

			ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("No EDI Messages should be created", 0, ediMessages.Length);

			batchResponse = exporter.CreateBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("Nothing to be batched.", batchResponse.InfoMessage);
		}

		public void TestCreateBatch_ShouldUpdateHighWaterMark_WhenSomeTransactions()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
			AssertEquals("Precondition: no high water mark", ZDateTime.Empty, LoadStmDateValue("AccountingTransactionExportServiceHighWaterMark"));
			AssertEquals(0, Factory.Load<EDIMessage>(new ZQuery()).Length);

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"), false);
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice.Lines[0].AL_JH = job.PK;
			TestObjectCreator.CreateCharge(invoice.Lines[0]).JR_OSSellAmt = 0;
			Factory.Save();

			var batchResponse = exporter.CreateBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("New batch 1 is created", batchResponse.InfoMessage);
			AssertEquals(1, batchResponse.CreatedBatchNumber);
			var batch = batchResponse.DataObject as TransactionBatch;
			AssertNotNull(batch);
			AssertBatch(batch, "AccBatchRequests_CRE1.xml");
			var actualHighwaterMark = LoadStmDateValue("AccountingTransactionExportServiceHighWaterMark");
			AssertGreaterThan("High water mark should updated whenever CreateBatch() succeeds", actualHighwaterMark, ZDateTime.Empty);

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("No EDI Messages should be created", 0, ediMessages.Length);
		}

		public void TestCreateBatch_ShouldUpdateHighWaterMark_WhenNoTransactions()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
			AssertEquals("Precondition: no high water mark", ZDateTime.Empty, LoadStmDateValue("AccountingTransactionExportServiceHighWaterMark"));
			AssertEquals(0, Factory.Load<EDIMessage>(new ZQuery()).Length);

			var batchResponse = exporter.CreateBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("Nothing to be batched.", batchResponse.InfoMessage);
			var actualHighwaterMark = LoadStmDateValue("AccountingTransactionExportServiceHighWaterMark");
			AssertGreaterThan("High water mark should updated whenever CreateBatch() succeeds", actualHighwaterMark, ZDateTime.Empty);

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("No EDI Messages should be created", 0, ediMessages.Length);
		}

		public void TestCreateBatch_ShouldNotIncrementBatchNumber_WhenNoTransactions()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
			AssertEquals(0, Factory.Load<EDIMessage>(new ZQuery()).Length);

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"), false);
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice.Lines[0].AL_JH = job.PK;
			TestObjectCreator.CreateCharge(invoice.Lines[0]).JR_OSSellAmt = 0;
			Factory.Save();

			var batchResponse = exporter.CreateBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("New batch 1 is created", batchResponse.InfoMessage);
			AssertEquals(1, batchResponse.CreatedBatchNumber);
			var batch = batchResponse.DataObject as TransactionBatch;
			AssertNotNull(batch);
			AssertBatch(batch, "AccBatchRequests_CRE1.xml");

			var batchResponse2 = exporter.CreateBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse2.ErrorMessage));
			AssertEquals("Nothing to be batched.", batchResponse2.InfoMessage);

			job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002002"), false);
			invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("112", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice.Lines[0].AL_JH = job.PK;
			TestObjectCreator.CreateCharge(invoice.Lines[0]).JR_OSSellAmt = 0;
			Factory.Save();

			var batchResponse3 = exporter.CreateBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse3.ErrorMessage));
			AssertEquals("New batch 2 is created", batchResponse3.InfoMessage);
			AssertEquals(2, batchResponse3.CreatedBatchNumber);
			batch = batchResponse3.DataObject as TransactionBatch;
			AssertNotNull(batch);
			AssertBatch(batch, "AccBatchRequests_CRE2.xml");

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("No EDI Messages should be created", 0, ediMessages.Length);
		}

		[TestDate(2016, 2, 9, 0, 0, 0)]
		public void TestCreateBatchSqlExceptionStackTrace()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForStackTraceTest(dataAccess);
			AssertEquals("Precondition: no high water mark", ZDateTime.Empty, LoadStmDateValue("AccountingTransactionExportServiceHighWaterMark"));
			var batchResponse = exporter.CreateBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			AssertEquals(false, batchResponse.CanContinue);

			var stackTraces = new string[]
			{
				"Exception Type:",
						"System.IO.FileNotFoundException",
				"Exception Message:",
						"This is the outter exception.",
				"Exception Stack Trace:",
						"at Enterprise.Accounting.DataTransfer.AccBatchRequest.Testing.AccountingTransactionEAdaptorExporterForStackTraceTest.GetUtcNow(DbTransaction transaction)",
						"at Enterprise.Accounting.DataTransfer.AccBatchRequest.AccountingTransactionEAdaptorExporter.CreateBatchCore(String companyCode)",

				"Inner Exception Type:",
						"System.Data.SqlClient.SqlException",
				"Inner Exception Message:",
						"Incorrect syntax near the keyword 'is'.",
				"Inner Exception Stack Trace:",
						"at System.Data.SqlClient.SqlCommand.ExecuteScalar()",
						"at Enterprise.Accounting.DataTransfer.AccBatchRequest.Testing.AccountingTransactionEAdaptorExporterForStackTraceTest.GetUtcNow(DbTransaction transaction)",
			};

			foreach (var item in stackTraces)
			{
				AssertContains(item, item, batchResponse.ErrorMessage);
			}

			var actualHighwaterMark = LoadStmDateValue("AccountingTransactionExportServiceHighWaterMark");
			AssertEquals("High water mark should not be updated if CreateBatch() fails", ZDateTime.Empty, actualHighwaterMark);
		}

		[TestDate(2016, 2, 9, 0, 0, 0)]
		public void TestExportBatch()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
			var transactionBatch = new TransactionBatch(GetWriterStrategy());
			var batchResponse = exporter.ExportBatch(GetWriterStrategy(), loginCompanyCode, 1, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("Nothing to export.", batchResponse.InfoMessage);

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, Factory.Load<EDIMessage>(new ZQuery()).Length);

			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 1, 1));

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"));
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice.Lines[0].AL_JH = job.PK;
			invoice.Lines[0].AL_SupplyType = "DSB";

			var country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var language = GlbCompany.CurrentCompany.OrgProxy.OH_Language;
			var localGLAccountDescriptor = TestObjectCreator.CreateAccountDescriptor(invoice.Lines[0].GLHeader, "999.888.777", "COA", "YYY", language, "Test Local GL Account matching Country and Language", country, Constants.DebitCredit.Debit);
			localGLAccountDescriptor = TestObjectCreator.CreateAccountDescriptor(invoice.Lines[0].GLHeader, "999.888.888", "COA", "YYY", "XXX", "Test Local GL Account matching Country", country, Constants.DebitCredit.Debit);
			localGLAccountDescriptor = TestObjectCreator.CreateAccountDescriptor(invoice.Lines[0].GLHeader, "999.888.999", "COA", "YYY", language, "Test Local GL Account matching Language", "XX", Constants.DebitCredit.Debit);
			localGLAccountDescriptor = TestObjectCreator.CreateAccountDescriptor(invoice.Lines[0].GLHeader, "999.999.999", "COA", "YYY", "XXX", "Test Local GL Account not matching Country or Language", "XX", Constants.DebitCredit.Debit);
			TestObjectCreator.CreateCharge(invoice.Lines[0]).JR_OSSellAmt = 0;
			Factory.Save();

			var batchSequence = TestObjectCreator.CreateGenExportBatchSequencePostLine(100, invoice.Lines[0].PK, 1);
			batchSequence.XB_Type = "ARV";
			Factory.Save();

			batchResponse = exporter.ExportBatch(GetWriterStrategy(), loginCompanyCode, 100, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("Batch 100 is exported", batchResponse.InfoMessage);
			var batch = batchResponse.DataObject as TransactionBatch;
			AssertNotNull(batch);
			AssertBatch(batch, "AccBatchRequests_EXP.xml");

			ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("No EDI Messages should be created", 0, ediMessages.Length);
		}

		[TestDate(2016, 2, 9, 0, 0, 0)]
		public void TestExportBatch_TaxBranch()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var taxOrgProxy = TestObjectCreator.CreateOrgHeader("", true, false, "AUSYD");
			var taxBranchAddress = TestObjectCreator.CreateAddress(taxOrgProxy, OrgAddressType.Office, true);
			taxBranchAddress.OA_Address1 = "Test Address Line 1";
			taxBranchAddress.OA_Address2 = "Test Address Line 2";
			taxBranchAddress.OA_Code = "Test Short Code";
			taxBranchAddress.OA_City = "TestCity";
			taxBranchAddress.OA_State = "NSW";
			taxBranchAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			taxBranchAddress.Header.OH_Code = "TAXORGCODE";
			var taxBranch = TestObjectCreator.CreateBranch("B1", "TaxBranch", GlbCompany.CurrentCompany, taxOrgProxy);
			Factory.Save();

			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
			var transactionBatch = new TransactionBatch(GetWriterStrategy());
			var batchResponse = exporter.ExportBatch(GetWriterStrategy(), loginCompanyCode, 1, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("Nothing to export.", batchResponse.InfoMessage);

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, Factory.Load<EDIMessage>(new ZQuery()).Length);

			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 1, 1));

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"));
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice.AH_GB_TaxBranch = taxBranch.PK;
			invoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice.Lines[0].AL_JH = job.PK;
			invoice.Lines[0].AL_GB_TaxBranch = taxBranch.PK;

			var country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var language = GlbCompany.CurrentCompany.OrgProxy.OH_Language;
			var localGLAccountDescriptor = TestObjectCreator.CreateAccountDescriptor(invoice.Lines[0].GLHeader, "999.888.777", "COA", "YYY", language, "Test Local GL Account matching Country and Language", country, Constants.DebitCredit.Debit);
			localGLAccountDescriptor = TestObjectCreator.CreateAccountDescriptor(invoice.Lines[0].GLHeader, "999.888.888", "COA", "YYY", "XXX", "Test Local GL Account matching Country", country, Constants.DebitCredit.Debit);
			localGLAccountDescriptor = TestObjectCreator.CreateAccountDescriptor(invoice.Lines[0].GLHeader, "999.888.999", "COA", "YYY", language, "Test Local GL Account matching Language", "XX", Constants.DebitCredit.Debit);
			localGLAccountDescriptor = TestObjectCreator.CreateAccountDescriptor(invoice.Lines[0].GLHeader, "999.999.999", "COA", "YYY", "XXX", "Test Local GL Account not matching Country or Language", "XX", Constants.DebitCredit.Debit);
			var charge = TestObjectCreator.CreateCharge(invoice.Lines[0]);
			charge.JR_OSSellAmt = 0;
			charge.JR_GB_CostTaxBranch = taxBranch.PK;
			Factory.Save();

			var batchSequence = TestObjectCreator.CreateGenExportBatchSequencePostLine(100, invoice.Lines[0].PK, 1);
			batchSequence.XB_Type = "ARV";
			Factory.Save();

			batchResponse = exporter.ExportBatch(GetWriterStrategy(), loginCompanyCode, 100, nameSpace);
			Assert(batchResponse.ErrorMessage, string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("Batch 100 is exported", batchResponse.InfoMessage);
			var batch = batchResponse.DataObject as TransactionBatch;
			AssertNotNull(batch);
			AssertBatch(batch, "AccBatchRequests_EXP_TaxBranch.xml");

			ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("No EDI Messages should be created", 0, ediMessages.Length);
		}

		[TestDate(2016, 2, 9, 0, 0, 0)]
		public void TestExportBatch_DisableSupplyType()
		{
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
			var transactionBatch = new TransactionBatch(GetWriterStrategy());
			var batchResponse = exporter.ExportBatch(GetWriterStrategy(), loginCompanyCode, 1, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("Nothing to export.", batchResponse.InfoMessage);

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, Factory.Load<EDIMessage>(new ZQuery()).Length);

			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 1, 1));

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"));
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice.Lines[0].AL_JH = job.PK;
			invoice.Lines[0].AL_SupplyType = "DSB";

			var country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var language = GlbCompany.CurrentCompany.OrgProxy.OH_Language;
			var localGLAccountDescriptor = TestObjectCreator.CreateAccountDescriptor(invoice.Lines[0].GLHeader, "999.888.777", "COA", "YYY", language, "Test Local GL Account matching Country and Language", country, Constants.DebitCredit.Debit);
			localGLAccountDescriptor = TestObjectCreator.CreateAccountDescriptor(invoice.Lines[0].GLHeader, "999.888.888", "COA", "YYY", "XXX", "Test Local GL Account matching Country", country, Constants.DebitCredit.Debit);
			localGLAccountDescriptor = TestObjectCreator.CreateAccountDescriptor(invoice.Lines[0].GLHeader, "999.888.999", "COA", "YYY", language, "Test Local GL Account matching Language", "XX", Constants.DebitCredit.Debit);
			localGLAccountDescriptor = TestObjectCreator.CreateAccountDescriptor(invoice.Lines[0].GLHeader, "999.999.999", "COA", "YYY", "XXX", "Test Local GL Account not matching Country or Language", "XX", Constants.DebitCredit.Debit);
			TestObjectCreator.CreateCharge(invoice.Lines[0]).JR_OSSellAmt = 0;
			Factory.Save();

			var batchSequence = TestObjectCreator.CreateGenExportBatchSequencePostLine(100, invoice.Lines[0].PK, 1);
			batchSequence.XB_Type = "ARV";
			Factory.Save();

			batchResponse = exporter.ExportBatch(GetWriterStrategy(), loginCompanyCode, 100, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("Batch 100 is exported", batchResponse.InfoMessage);
			var batch = batchResponse.DataObject as TransactionBatch;
			AssertNotNull(batch);
			AssertBatch(batch, "AccBatchRequests_EXP_Original.xml");

			ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("No EDI Messages should be created", 0, ediMessages.Length);
		}

		[TestDate(2018, 4, 25, 0, 0, 0)]
		public void TestExportBatchWithNullUserContext()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
			var transactionBatch = new TransactionBatch(GetWriterStrategy());

			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 1, 1));
			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"), false);
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice.Lines[0].AL_JH = job.PK;
			TestObjectCreator.CreateCharge(invoice.Lines[0]).JR_OSSellAmt = 0;
			Factory.Save();

			var batchSequence = TestObjectCreator.CreateGenExportBatchSequencePostLine(100, invoice.Lines[0].PK, 1);
			batchSequence.XB_Type = "ARV";
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(null))
			{
				var batchResponse = exporter.ExportBatch(GetWriterStrategy(), loginCompanyCode, 100, nameSpace);
				Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
				AssertEquals("Batch 100 is exported", batchResponse.InfoMessage);
			}
		}

		[TestDate(2017, 7, 26, 0, 0, 0)]
		public void TestExportBatchWIthWIPACR()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
			var transactionBatch = new TransactionBatch(GetWriterStrategy());
			var batchResponse = exporter.ExportBatch(GetWriterStrategy(), loginCompanyCode, 1, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("Nothing to export.", batchResponse.InfoMessage);

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, Factory.Load<EDIMessage>(new ZQuery()).Length);

			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 1, 1));

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"));
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 100m);
			Factory.Save();
			charge1.WIP.AL_PostDate = ZDateTime.Today.AddDays(-10);
			charge1.Accrual.AL_PostDate = ZDateTime.Today.AddDays(-9);
			charge1.Accrual.AL_SupplyType = "DSB";
			Factory.Save();

			batchResponse = exporter.CreateBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("New batch 1 is created", batchResponse.InfoMessage);
			AssertEquals(1, batchResponse.CreatedBatchNumber);
			var batch = batchResponse.DataObject as TransactionBatch;
			AssertNotNull(batch);
			AssertBatch(batch, "AccBatchRequests_CRE1.xml");

			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			apInvoice.AH_PostDate = ZDateTime.Today.AddDays(-8);
			apInvoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			apInvoice.Lines[0].AL_JH = job.PK;
			charge1.ReverseAccrual(ZDateTime.Today.AddDays(-8));
			charge1.JR_AL_APLine = apInvoice.Lines[0].PK;
			charge1.SetChargeValuesFromLinkedAPLineForTests();

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("11111", TestObjectCreator.AUD, 1.0m, TestObjectCreator.AALSHI);
			arInvoice.AH_PostDate = ZDateTime.Today.AddDays(-7);
			TestObjectCreator.CreateARInvoiceLine(arInvoice, job, TestObjectCreator.CC2, TestObjectCreator.AUD, 1.0m, "AR Line", 100m);
			arInvoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			arInvoice.Lines[0].AL_JH = job.PK;
			charge1.ReverseWIP(ZDateTime.Today.AddDays(-7));
			charge1.JR_AL_ARLine = arInvoice.Lines[0].PK;
			charge1.SetChargeValuesFromLinkedARLineForTests();
			Factory.Save();

			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, 200m, 200m);
			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, 300m, 300m);
			Factory.Save();

			charge2.WIP.AL_PostDate = ZDateTime.Today.AddDays(-6);
			charge2.Accrual.AL_PostDate = ZDateTime.Today.AddDays(-5);
			charge2.ReverseAccrual(ZDateTime.Today.AddDays(-4));
			charge2.ReverseWIP(ZDateTime.Today.AddDays(-3));
			charge2.JR_OSCostAmt = 0m;
			charge2.JR_OSSellAmt = 0m;

			charge3.WIP.AL_PostDate = ZDateTime.Today.AddDays(-2);
			charge3.Accrual.AL_PostDate = ZDateTime.Today.AddDays(-1);
			Factory.Save();

			batchResponse = exporter.CreateBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("New batch 2 is created", batchResponse.InfoMessage);
			AssertEquals(2, batchResponse.CreatedBatchNumber);
			batch = batchResponse.DataObject as TransactionBatch;
			AssertNotNull(batch);
			AssertBatch(batch, "AccBatchRequests_CRE2.xml");

			charge3.ReverseAccrual(ZDateTime.Today);
			charge3.ReverseWIP(ZDateTime.Today.AddDays(1));
			charge3.JR_OSCostAmt = 0m;
			charge3.JR_OSSellAmt = 0m;
			Factory.Save();

			batchResponse = exporter.CreateBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("New batch 3 is created", batchResponse.InfoMessage);
			AssertEquals(3, batchResponse.CreatedBatchNumber);
			batch = batchResponse.DataObject as TransactionBatch;
			AssertNotNull(batch);
			AssertBatch(batch, "AccBatchRequests_CRE3.xml");

			batchResponse = exporter.ExportBatch(GetWriterStrategy(), loginCompanyCode, 1, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("Batch 1 is exported", batchResponse.InfoMessage);
			batch = batchResponse.DataObject as TransactionBatch;
			AssertNotNull(batch);
			AssertBatch(batch, "AccBatchRequests_EXP1.xml");

			batchResponse = exporter.ExportBatch(GetWriterStrategy(), loginCompanyCode, 2, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("Batch 2 is exported", batchResponse.InfoMessage);
			batch = batchResponse.DataObject as TransactionBatch;
			AssertNotNull(batch);
			AssertBatch(batch, "AccBatchRequests_EXP2.xml");

			batchResponse = exporter.ExportBatch(GetWriterStrategy(), loginCompanyCode, 3, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("Batch 3 is exported", batchResponse.InfoMessage);
			batch = batchResponse.DataObject as TransactionBatch;
			AssertNotNull(batch);
			AssertBatch(batch, "AccBatchRequests_EXP3.xml");

			ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("No EDI Messages should be created", 0, ediMessages.Length);
		}

		[TestDate(2016, 2, 9, 0, 0, 0)]
		public void TestExportBatchWithCanceledTransaction()
		{
			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 1, 1));

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"));
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice.Lines[0].AL_JH = job.PK;

			var country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var language = GlbCompany.CurrentCompany.OrgProxy.OH_Language;
			var localGLAccountDescriptor = TestObjectCreator.CreateAccountDescriptor(invoice.Lines[0].GLHeader, "999.888.777", "COA", "YYY", language, "Test Local GL Account matching Country and Language", country, Constants.DebitCredit.Debit);
			localGLAccountDescriptor = TestObjectCreator.CreateAccountDescriptor(invoice.Lines[0].GLHeader, "999.888.888", "COA", "YYY", "XXX", "Test Local GL Account matching Country", country, Constants.DebitCredit.Debit);
			localGLAccountDescriptor = TestObjectCreator.CreateAccountDescriptor(invoice.Lines[0].GLHeader, "999.888.999", "COA", "YYY", language, "Test Local GL Account matching Language", "XX", Constants.DebitCredit.Debit);
			localGLAccountDescriptor = TestObjectCreator.CreateAccountDescriptor(invoice.Lines[0].GLHeader, "999.999.999", "COA", "YYY", "XXX", "Test Local GL Account not matching Country or Language", "XX", Constants.DebitCredit.Debit);
			TestObjectCreator.CreateCharge(invoice.Lines[0]).JR_OSSellAmt = 0;
			Factory.Save();

			var batchSequence = TestObjectCreator.CreateGenExportBatchSequencePostLine(100, invoice.Lines[0].PK, 1);
			batchSequence.XB_Type = "ARV";
			Factory.Save();

			var reversalCodes = new CodeDescriptionPairList(AccountingMasterFilesRegistry.Instance.ReversalReasonCodesList.DefaultValue);
			reversalCodes.Add(new CodeDescriptionPair("LNG", new string('a', 100)));  // Max length in XSD is 80 characters; expect it is truncated in XML
			using (AccountingMasterFilesRegistry.Instance.ReversalReasonCodesList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, reversalCodes))
			{
				var reverser = new ReversingFactory().NewReversing(invoice);
				reverser.Reverse();
				invoice.ReverseInvoice.AH_TransactionNum = "REV001";
				invoice.ReverseInvoice.AH_ReceiptType = "LNG";
				Factory.Save();

				var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
				var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
				var dataAccess = new BatchExportDataAccess(Connection, Transaction);
				var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
				var batchResponse = exporter.ExportBatch(GetWriterStrategy(), loginCompanyCode, 100, nameSpace);
				Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
				AssertEquals("Batch 100 is exported", batchResponse.InfoMessage);
				var batch = batchResponse.DataObject as TransactionBatch;
				AssertNotNull(batch);
				AssertBatch(batch, "AccBatchRequests_CanceledTransaction.xml");
			}
		}

		[TestDate(2016, 2, 9, 0, 0, 0)]
		public void TestExportBatch_TransactionBelongsToGroup_MLI()
		{
			using (AccountingMasterFilesRegistry.Instance.IncludeRelatedJournalsInUniversalXMLTransaction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
				var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
				var dataAccess = new BatchExportDataAccess(Connection, Transaction);
				var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
				var batchResponse = exporter.ExportBatch(GetWriterStrategy(), loginCompanyCode, 1, nameSpace);
				Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
				AssertEquals("Nothing to export.", batchResponse.InfoMessage);
				AssertEquals(0, Factory.Load<EDIMessage>(new ZQuery()).Length);

				var today = ZDate.Today;
				TestObjectCreator.CreateTestPeriods(new ZDateTime(today.Year, 1, 1));

				var arTerms = TestObjectCreator.AALSHI.CompanyData.ARTerms;
				var arTermMLI = arTerms.AddNew();
				arTermMLI.PY_JobType = "ALL";
				arTermMLI.PY_GB_Branch = GlbBranch.CurrentBranch.PK;
				arTermMLI.PY_GE_Department = GlbDepartment.CurrentDepartment.PK;
				arTermMLI.PY_Direction = "ALL";
				arTermMLI.PY_TransportMode = "ALL";
				arTermMLI.PY_InvoiceClass = InvoiceTypesList.Codes.FinalInvoice;
				arTermMLI.PY_InvoiceTerm = InvoiceTerms.FromInvoiceDate;
				var arTermMLInstalments = arTermMLI.ARTermsInstallments;

				var installment1 = arTermMLInstalments.AddNew();
				installment1.ML_SequenceNumber = 1;
				installment1.ML_SplitPercentage = 50.00;
				installment1.ML_DaysFromInvoiceDate = 30;
				installment1.ML_AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest;

				var installment2 = arTermMLInstalments.AddNew();
				installment2.ML_SequenceNumber = 2;
				installment2.ML_SplitPercentage = 50.00;
				installment2.ML_DaysFromInvoiceDate = 60;
				installment2.ML_AgreedPaymentMethod = OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest;

				var invoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 20, 0M, 20, 0M, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK, today, ZDateTime.Empty, today, false);
				invoice.AH_InvoiceTerm = InvoiceTerms.FromInvoiceDate;

				Factory.Save();

				var batchSequence = TestObjectCreator.CreateGenExportBatchSequencePostLine(100, invoice.Lines[0].PK, 1);
				batchSequence.XB_Type = "APS";
				Factory.Save();

				batchResponse = exporter.ExportBatch(GetWriterStrategy(), loginCompanyCode, 100, nameSpace);
				Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
				AssertEquals("Batch 100 is exported", batchResponse.InfoMessage);
				var batch = batchResponse.DataObject as TransactionBatch;
				AssertNotNull(batch);
				AssertBatch(batch, "AccBatchRequests_TransactionBelongsToGroup_MLI.xml");

				var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("No EDI Messages should be created", 0, ediMessages.Length);
			}
		}

		[TestDate(2016, 2, 9, 0, 0, 0)]
		public void TestExportBatch_TransactionBelongsToGroup_Apportionments()
		{
			using (AccountingMasterFilesRegistry.Instance.IncludeRelatedJournalsInUniversalXMLTransaction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
				var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
				var dataAccess = new BatchExportDataAccess(Connection, Transaction);
				var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
				var batchResponse = exporter.ExportBatch(GetWriterStrategy(), loginCompanyCode, 1, nameSpace);
				Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
				AssertEquals("Nothing to export.", batchResponse.InfoMessage);
				AssertEquals(0, Factory.Load<EDIMessage>(new ZQuery()).Length);

				var today = ZDate.Today;
				TestObjectCreator.CreateTestPeriods(new ZDateTime(today.Year, 1, 1));

				var invoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 20, 0M, 20, 0M, TestObjectCreator.AALSHI, TestObjectCreator.CC1.PK, today, ZDateTime.Empty, today, false);

				invoice.Lines[0].PeriodStartDate = today;
				invoice.Lines[0].PeriodEndDate = today.AddMonths(3);
				invoice.Lines[0].PeriodApportionmentMethod = "DAY";
				invoice.Lines[0].PeriodClearingGLAccountPK = TestObjectCreator.GLJournalClearingAccount.PK;
				var journals = invoice.MultiPeriodApportionmentJournals;

				Factory.Save();

				var batchSequence = TestObjectCreator.CreateGenExportBatchSequencePostLine(100, invoice.Lines[0].PK, 1);
				batchSequence.XB_Type = "APS";
				Factory.Save();

				batchResponse = exporter.ExportBatch(GetWriterStrategy(), loginCompanyCode, 100, nameSpace);
				Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
				AssertEquals("Batch 100 is exported", batchResponse.InfoMessage);
				var batch = batchResponse.DataObject as TransactionBatch;
				AssertNotNull(batch);
				AssertBatch(batch, "AccBatchRequests_TransactionBelongsToGroup_Apportionments.xml");

				var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("No EDI Messages should be created", 0, ediMessages.Length);
			}
		}

		public void TestEmptyEDIMessage()
		{
			AssertEquals(0, Factory.Load<EDIMessage>(new ZQuery()).Length);
		}

		public void TestEmptyBatchExporter()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
			var batchResponse = exporter.ExportBatch(GetWriterStrategy(), loginCompanyCode, 1, nameSpace);

			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("Nothing to export.", batchResponse.InfoMessage);
		}

		[TestDate(2016, 2, 9, 0, 0, 0)]
		public void TestExportBatch_ZeroTax()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			Factory.Save();

			const string nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var exporter = new AccountingTransactionEAdaptorExporterForTest(
				new BatchExportDataAccess(Connection, Transaction)
			);

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "111", TestObjectCreator.AUD, 1.0m, 0m, 0m, 0m, 0m);
			apInvoice.AH_OH = TestObjectCreator.AALSHI.PK;

			AssertEquals("Precondition", 1, apInvoice.Lines.Count);
			var apLine = apInvoice.Lines[0];

			apLine.AL_AT = TestObjectCreator.GSTFREE1.PK;
			apLine.AL_RX_NKTransactionCurrency = TestObjectCreator.CNY.Code;
			apLine.AL_ExchangeRate = 0.005634m;
			apLine.AL_OSExTaxAmount = 3080329.28m;

			Factory.Save();

			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var batchResponse = exporter.CreateBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("New batch 1 is created", batchResponse.InfoMessage);
			AssertEquals(1, batchResponse.CreatedBatchNumber);
			var batchResponseExport = exporter.ExportBatch(GetWriterStrategy(), loginCompanyCode, 1, nameSpace);

			AssertBatch(batchResponseExport.DataObject as TransactionBatch, "AccBatchRequests_ZeroTax.xml");
		}

		[TestDate(2016, 2, 9, 0, 0, 0)]
		public void TestCreateAndExportBatch()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
			var transactionBatch = new TransactionBatch(GetWriterStrategy());

			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 1, 1));

			TestObjectCreator.AALSHI.SetLocalCustomsCode("ABC", "123");
			TestObjectCreator.AALSHI.SetLocalCustomsCode("DEF", "456");

			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode("MNO", "789");
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode("XYZ", "012");

			GlbCompany.CurrentCompany.OrgProxy.Factory.Save();

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"), false);
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice.Lines[0].AL_JH = job.PK;
			TestObjectCreator.CreateCharge(invoice.Lines[0]).JR_OSSellAmt = 0;
			Factory.Save();

			var batchResponse = exporter.CreateAndExportBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("New batch 1 is created and exported", batchResponse.InfoMessage);
			var batch = batchResponse.DataObject as TransactionBatch;
			AssertNotNull(batch);
			AssertBatch(batch, "AccBatchRequests_CEX.xml");

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("No EDI Messages should be created", 0, ediMessages.Length);
		}

		[TestDate(2016, 2, 9, 0, 0, 0)]
		public void TestCreateAndExportBatch_ShouldUpdateHighWaterMark_WhenSomeTransactions()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
			var transactionBatch = new TransactionBatch(GetWriterStrategy());
			AssertEquals("Precondition: no high water mark", ZDateTime.Empty, LoadStmDateValue("AccountingTransactionExportServiceHighWaterMark"));

			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 1, 1));

			TestObjectCreator.AALSHI.SetLocalCustomsCode("ABC", "123");
			TestObjectCreator.AALSHI.SetLocalCustomsCode("DEF", "456");

			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode("MNO", "789");
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode("XYZ", "012");

			GlbCompany.CurrentCompany.OrgProxy.Factory.Save();

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"), false);
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice.Lines[0].AL_JH = job.PK;
			TestObjectCreator.CreateCharge(invoice.Lines[0]).JR_OSSellAmt = 0;
			Factory.Save();

			var batchResponse = exporter.CreateAndExportBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("New batch 1 is created and exported", batchResponse.InfoMessage);
			var batch = batchResponse.DataObject as TransactionBatch;
			AssertNotNull(batch);
			AssertBatch(batch, "AccBatchRequests_CEX.xml");
			var actualHighwaterMark = LoadStmDateValue("AccountingTransactionExportServiceHighWaterMark");
			AssertGreaterThan("High water mark should updated whenever CreateAndExportBatch() succeeds", actualHighwaterMark, ZDateTime.Empty);

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("No EDI Messages should be created", 0, ediMessages.Length);
		}

		[TestDate(2016, 2, 9, 0, 0, 0)]
		public void TestCreateAndExportBatch_ShouldUpdateHighWaterMark_WhenNoTransactions()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
			var transactionBatch = new TransactionBatch(GetWriterStrategy());
			AssertEquals("Precondition: no high water mark", ZDateTime.Empty, LoadStmDateValue("AccountingTransactionExportServiceHighWaterMark"));

			var batchResponse = exporter.CreateAndExportBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("Nothing to be batched.", batchResponse.InfoMessage);
			var actualHighwaterMark = LoadStmDateValue("AccountingTransactionExportServiceHighWaterMark");
			AssertGreaterThan("High water mark should updated whenever CreateAndExportBatch() succeeds", actualHighwaterMark, ZDateTime.Empty);

			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("No EDI Messages should be created", 0, ediMessages.Length);
		}

		[TestDate(2019, 11, 20, 0, 0, 0)]
		public void TestCreateAndExportBatch_StartPostDate()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);

			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 1, 1));

			TestObjectCreator.AALSHI.SetLocalCustomsCode("ABC", "123");
			TestObjectCreator.AALSHI.SetLocalCustomsCode("DEF", "456");

			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode("MNO", "789");
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode("XYZ", "012");

			GlbCompany.CurrentCompany.OrgProxy.Factory.Save();

			var job1 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"), false);
			var invoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice1.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice1.Lines[0].AL_JH = job1.PK;
			TestObjectCreator.CreateCharge(invoice1.Lines[0]).JR_OSSellAmt = 0;
			Factory.Save();

			var job2 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00003001"), false);
			var invoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("222", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice2.AH_PostDate = new ZDateTime(2019, 11, 1);
			invoice2.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice2.Lines[0].AL_JH = job2.PK;
			TestObjectCreator.CreateCharge(invoice2.Lines[0]).JR_OSSellAmt = 0;
			Factory.Save();

			SetStmDateValue("ExportTransactionBatchStartPostDate", new ZDateTime(2019, 11, 15));
			SetStmDateValue("AccountingTransactionExportServiceHighWaterMark", new ZDateTime(2019, 10, 20));
			Factory.Save();

			var batchResponse = exporter.CreateAndExportBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("New batch 1 is created and exported", batchResponse.InfoMessage);
			var batch = batchResponse.DataObject as TransactionBatch;
			AssertNotNull(batch);
			AssertEquals("Batch should contain one transaction", 1, batch.TransactionCollection.Count);
		}

		[TestDate(2016, 2, 9, 0, 0, 0)]
		public void TestLegacyToUniveralExport_HighWatermarkFallback()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
			var transactionBatch = new TransactionBatch(GetWriterStrategy());

			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 1, 1));

			TestObjectCreator.AALSHI.SetLocalCustomsCode("ABC", "123");
			TestObjectCreator.AALSHI.SetLocalCustomsCode("DEF", "456");

			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode("MNO", "789");
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode("XYZ", "012");

			GlbCompany.CurrentCompany.OrgProxy.Factory.Save();

			var job1 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"), false);
			var invoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice1.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice1.Lines[0].AL_JH = job1.PK;
			TestObjectCreator.CreateCharge(invoice1.Lines[0]).JR_OSSellAmt = 0;
			Factory.Save();

			new TestDateAttribute(2016, 2, 4, 0, 0, 0);
			var job2 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00003001"), false);
			var invoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("222", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice2.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice2.Lines[0].AL_JH = job2.PK;
			TestObjectCreator.CreateCharge(invoice2.Lines[0]).JR_OSSellAmt = 0;
			Factory.Save();

			var highWaterMark = new ZDateTime(TestDateAttribute.Date.AddDays(2));
			SetStmDateValue("AccountingTransactionsExportHighWaterMark", highWaterMark);
			Factory.Save();

			AssertEquals("Precondition: legacy high water mark", highWaterMark, LoadStmDateValue("AccountingTransactionsExportHighWaterMark"));
			AssertEquals("Precondition: no high water mark", ZDateTime.Empty, LoadStmDateValue("AccountingTransactionExportServiceHighWaterMark"));
			var batchResponse = exporter.CreateAndExportBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("New batch 1 is created and exported", batchResponse.InfoMessage);
			var batch = batchResponse.DataObject as TransactionBatch;
			AssertNotNull(batch);
			AssertEquals("Batch should contain one transaction", 1, batch.TransactionCollection.Count);
			var actualHighwaterMark = LoadStmDateValue("AccountingTransactionExportServiceHighWaterMark");
			AssertGreaterThan("High water mark should be set whenever CreateAndExportBatch() succeeds", actualHighwaterMark, ZDateTime.Empty);
			var actualLegacyHighwaterMark = LoadStmDateValue("AccountingTransactionsExportHighWaterMark");
			AssertEquals("Legacy high water mark should not change. Ever.", actualLegacyHighwaterMark, highWaterMark);
		}

		[TestDate(2016, 2, 9, 0, 0, 0)]
		public void TestLegacyToUniveralExport_DetectDuplicateTransactionLines()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
			var transactionBatch = new TransactionBatch(GetWriterStrategy());

			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 1, 1));

			TestObjectCreator.AALSHI.SetLocalCustomsCode("ABC", "123");
			TestObjectCreator.AALSHI.SetLocalCustomsCode("DEF", "456");

			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode("MNO", "789");
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode("XYZ", "012");

			GlbCompany.CurrentCompany.OrgProxy.Factory.Save();

			SetStmDateValue("AccountingTransactionExportServiceHighWaterMark", new ZDateTime(TestDateAttribute.Date.AddDays(-2)));

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("00002001"), false);
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice.Lines[0].AL_JH = job.PK;
			TestObjectCreator.CreateCharge(invoice.Lines[0]).JR_OSSellAmt = 0;

			var genExportBatchSequenceData = Factory.New<GenExportBatchSequence>();
			genExportBatchSequenceData.XB_ParentID = invoice.PK;
			genExportBatchSequenceData.XB_ParentTableCode = "AH";
			genExportBatchSequenceData.XB_Sequence = 0;
			genExportBatchSequenceData.XB_BatchNumber = 1;
			genExportBatchSequenceData.XB_Type = "HEX";

			Factory.Save();

			var batchResponse = exporter.CreateAndExportBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("Nothing to be batched.", batchResponse.InfoMessage);
			var batch = batchResponse.DataObject as TransactionBatch;
			AssertNull(batch);
		}

		[TestDate(2016, 2, 9, 0, 0, 0)]
		public void TestLegacyToUniveralExport_DetectDuplicateWIPAccruals()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
			var transactionBatch = new TransactionBatch(GetWriterStrategy());

			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 1, 1));

			TestObjectCreator.AALSHI.SetLocalCustomsCode("ABC", "123");
			TestObjectCreator.AALSHI.SetLocalCustomsCode("DEF", "456");

			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode("MNO", "789");
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode("XYZ", "012");

			GlbCompany.CurrentCompany.OrgProxy.Factory.Save();

			var highWaterMark = new ZDateTime(TestDateAttribute.Date.AddDays(-2));
			SetStmDateValue("AccountingTransactionExportServiceHighWaterMark", highWaterMark);

			var job = TestObjectCreator.CreateJobHeader();

			var accrual = TestObjectCreator.CreateAccrual(job);
			accrual.Reverse();
			accrual.AL_ReverseDate = highWaterMark;

			var wip = TestObjectCreator.CreateWIP(job);
			wip.Reverse();
			wip.AL_ReverseDate = highWaterMark;

			var genExportBatchSequenceData1 = Factory.New<GenExportBatchSequence>();
			genExportBatchSequenceData1.XB_ParentID = accrual.PK;
			genExportBatchSequenceData1.XB_ParentTableCode = "AL";
			genExportBatchSequenceData1.XB_Sequence = 0;
			genExportBatchSequenceData1.XB_BatchNumber = 1;
			genExportBatchSequenceData1.XB_Type = "LEX";

			var genExportBatchSequenceData2 = Factory.New<GenExportBatchSequence>();
			genExportBatchSequenceData2.XB_ParentID = accrual.PK;
			genExportBatchSequenceData2.XB_ParentTableCode = "AL";
			genExportBatchSequenceData2.XB_Sequence = 0;
			genExportBatchSequenceData2.XB_BatchNumber = 1;
			genExportBatchSequenceData2.XB_Type = "LRX";

			var genExportBatchSequenceData3 = Factory.New<GenExportBatchSequence>();
			genExportBatchSequenceData3.XB_ParentID = wip.PK;
			genExportBatchSequenceData3.XB_ParentTableCode = "AL";
			genExportBatchSequenceData3.XB_Sequence = 0;
			genExportBatchSequenceData3.XB_BatchNumber = 1;
			genExportBatchSequenceData3.XB_Type = "LEX";

			var genExportBatchSequenceData4 = Factory.New<GenExportBatchSequence>();
			genExportBatchSequenceData4.XB_ParentID = wip.PK;
			genExportBatchSequenceData4.XB_ParentTableCode = "AL";
			genExportBatchSequenceData4.XB_Sequence = 0;
			genExportBatchSequenceData4.XB_BatchNumber = 1;
			genExportBatchSequenceData4.XB_Type = "LRX";

			Factory.Save();

			var batchResponse = exporter.CreateAndExportBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("Nothing to be batched.", batchResponse.InfoMessage);
			var batch = batchResponse.DataObject as TransactionBatch;
			AssertNull(batch);
		}

		[TestDate(2016, 2, 9, 0, 0, 0)]
		public void TestLegacyToUniveralExport_ForWIPAccrualsReversedAfterLegacyExport()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);
			var transactionBatch = new TransactionBatch(GetWriterStrategy());

			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 1, 1));

			TestObjectCreator.AALSHI.SetLocalCustomsCode("ABC", "123");
			TestObjectCreator.AALSHI.SetLocalCustomsCode("DEF", "456");

			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode("MNO", "789");
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode("XYZ", "012");

			GlbCompany.CurrentCompany.OrgProxy.Factory.Save();

			var highWaterMark = new ZDateTime(TestDateAttribute.Date.AddDays(-2));
			SetStmDateValue("AccountingTransactionExportServiceHighWaterMark", highWaterMark);

			var job = TestObjectCreator.CreateJobHeader();
			var accrual = TestObjectCreator.CreateAccrual(job);
			var wip = TestObjectCreator.CreateWIP(job);

			var genExportBatchSequenceData1 = Factory.New<GenExportBatchSequence>();
			genExportBatchSequenceData1.XB_ParentID = accrual.PK;
			genExportBatchSequenceData1.XB_ParentTableCode = "AL";
			genExportBatchSequenceData1.XB_Sequence = 0;
			genExportBatchSequenceData1.XB_BatchNumber = 1;
			genExportBatchSequenceData1.XB_Type = "LEX";

			var genExportBatchSequenceData2 = Factory.New<GenExportBatchSequence>();
			genExportBatchSequenceData2.XB_ParentID = wip.PK;
			genExportBatchSequenceData2.XB_ParentTableCode = "AL";
			genExportBatchSequenceData2.XB_Sequence = 0;
			genExportBatchSequenceData2.XB_BatchNumber = 1;
			genExportBatchSequenceData2.XB_Type = "LEX";

			accrual.Reverse();
			accrual.AL_ReverseDate = highWaterMark;

			wip.Reverse();
			wip.AL_ReverseDate = highWaterMark;

			Factory.Save();

			var batchResponse = exporter.CreateAndExportBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("New batch 1 is created and exported", batchResponse.InfoMessage);
			var batch = batchResponse.DataObject as TransactionBatch;
			AssertNotNull(batch);
			var loaded = Factory.Load<GenExportBatchSequence>(new ZQuery(GenExportBatchSequenceSchema.XB_ParentID, accrual.PK)
				.AddToFilter(GenExportBatchSequenceSchema.XB_Type, "ARV").AddToFilter(GenExportBatchSequenceSchema.XB_BatchNumber, (int)batchResponse.CreatedBatchNumber));
			AssertEquals("Accrual exported after reversal", 1, loaded.Length);
			loaded = Factory.Load<GenExportBatchSequence>(new ZQuery(GenExportBatchSequenceSchema.XB_ParentID, wip.PK)
				.AddToFilter(GenExportBatchSequenceSchema.XB_Type, "ARV").AddToFilter(GenExportBatchSequenceSchema.XB_BatchNumber, (int)batchResponse.CreatedBatchNumber));
			AssertEquals("WIP exported after reversal", 1, loaded.Length);
		}

		void AssertBatch(TransactionBatch batch, string xmlFile)
		{
			using (var batchStream = (SubStreamableStream)new MemoryStream())
			{
				new XmlWriter().WriteXML(batch, batchStream, false);
				batchStream.Position = 0;

				var file = embeddedResourceRetriever.SaveResourceToFile(xmlFile);
				using (var streamReader = File.OpenText(file))
				using (var batchReader = new StreamReader(batchStream))
				{
					var expectedXml = streamReader.ReadToEnd();
					streamReader.Close();
					var actualXml = batchReader.ReadToEnd();
			
					this.AssertXMLEqualsIgnoreChildOrder("Exported XML should match XML file", expectedXml, actualXml);
				}
			}
		}

		#region MatchStatus

		public void TestMatchStatusAndReasonWithDefaultValue()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			const string nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";

			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);

			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 1, 1));

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("1001" + DateTime.Now.Second), false);
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("11" + DateTime.Now.Second, TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice.Lines[0].AL_JH = job.PK;
			invoice.AH_MatchStatus = "UAC";
			invoice.AH_MatchStatusReasonCode = "ADV";

			TestObjectCreator.CreateCharge(invoice.Lines[0]).JR_OSSellAmt = 0;
			Factory.Save();

			var batchResponse = exporter.CreateAndExportBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("New batch 1 is created and exported", batchResponse.InfoMessage);
			var batch = batchResponse.DataObject as TransactionBatch;

			AssertBatchContainsMatchStatus(batch, "UAC", "Unallocated");
			AssertBatchContainsMatchStatus(batch, "ADV", "Receipt/Payment in advance");
		}

		public void TestMatchStatusAndReason()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			const string nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";

			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);

			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 1, 1));

			var statuses1 = new Enterprise.Registry.Business.SystemDefinableCodeDescriptionBoolCollection();
			var status1 = statuses1.AddNew();
			status1.Code = "abc";
			status1.Description = (NoResString)"abc desc";

			var status2 = statuses1.AddNew();
			status2.Code = "xyz";
			status2.Description = (NoResString)"xyz desc";

			var reasons1 = new Enterprise.Registry.Business.SystemDefinableCodeDescriptionBoolCollection();
			var reason1 = reasons1.AddNew();
			reason1.Code = "def";
			reason1.Description = (NoResString)"def desc";

			var reason2 = reasons1.AddNew();
			reason2.Code = "zyx";
			reason2.Description = (NoResString)"zyx desc";

			AccountingConfigurationRegistry.Instance.MatchStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, statuses1);
			AccountingConfigurationRegistry.Instance.MatchStatusReason.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reasons1);

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("1001" + DateTime.Now.Second), false);
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("11" + DateTime.Now.Second, TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice.Lines[0].AL_JH = job.PK;
			invoice.AH_MatchStatus = "abc";
			invoice.AH_MatchStatusReasonCode = "def";

			TestObjectCreator.CreateCharge(invoice.Lines[0]).JR_OSSellAmt = 0;
			Factory.Save();

			var batchResponse = exporter.CreateAndExportBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("New batch 1 is created and exported", batchResponse.InfoMessage);
			var batch = batchResponse.DataObject as TransactionBatch;

			AssertBatchContainsMatchStatus(batch, status1.Code, status1.Description);
			AssertBatchContainsMatchStatus(batch, reason1.Code, reason1.Description);
		}

		public void TestMatchStatusAndReasonOverride()
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			const string nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";

			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);

			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 1, 1));

			var statuses1 = new Enterprise.Registry.Business.SystemDefinableCodeDescriptionBoolCollection();
			var status1 = statuses1.AddNew();
			status1.Code = "abc";
			status1.Description = (NoResString)"abc desc";

			var reasons1 = new Enterprise.Registry.Business.SystemDefinableCodeDescriptionBoolCollection();
			var reason1 = reasons1.AddNew();
			reason1.Code = "def";
			reason1.Description = (NoResString)"def desc";

			AccountingConfigurationRegistry.Instance.MatchStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, statuses1);
			AccountingConfigurationRegistry.Instance.MatchStatusReason.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reasons1);

			var statuses2 = new Enterprise.Registry.Business.SystemDefinableCodeDescriptionBoolCollection();
			var status2 = statuses2.AddNew();
			status2.Code = "abc";
			status2.Description = (NoResString)"abc desc override";

			var reasons2 = new Enterprise.Registry.Business.SystemDefinableCodeDescriptionBoolCollection();
			var reason2 = reasons2.AddNew();
			reason2.Code = "def";
			reason2.Description = (NoResString)"def desc override";

			AccountingConfigurationRegistry.Instance.MatchStatus.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, statuses2);
			AccountingConfigurationRegistry.Instance.MatchStatusReason.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, reasons2);

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("1001" + DateTime.Now.Second), false);
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("11" + DateTime.Now.Second, TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice.Lines[0].AL_JH = job.PK;
			invoice.AH_MatchStatus = "abc";
			invoice.AH_MatchStatusReasonCode = "def";

			TestObjectCreator.CreateCharge(invoice.Lines[0]).JR_OSSellAmt = 0;
			Factory.Save();

			var batchResponse = exporter.CreateAndExportBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("New batch 1 is created and exported", batchResponse.InfoMessage);
			var batch = batchResponse.DataObject as TransactionBatch;

			AssertBatchContainsMatchStatus(batch, status2.Code, status2.Description);
			AssertBatchContainsMatchStatus(batch, reason2.Code, reason2.Description);
		}

		void AssertBatchContainsMatchStatus(TransactionBatch batch, string code, string description)
		{
			using (var batchStream = (SubStreamableStream)new MemoryStream())
			{
				new XmlWriter().WriteXML(batch, batchStream, false);
				using (var batchReader = new StreamReader(batchStream))
				{
					var actualXML = batchReader.ReadToEnd();
					Assert(actualXML.Contains(code));
					Assert(actualXML.Contains(description));
				}
			}
		}

		#endregion

		public void TestGovernmentChargeCodeElement()
		{
			var dataAccess = new BatchExportDataAccess(Connection, Transaction);
			var exporter = new AccountingTransactionEAdaptorExporterForTest(dataAccess);

			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 1, 1));

			var batch = CreateExportBatch(exporter);
			AssertNotNull(batch);
			AssertBatchContainsGovtChargeCode(batch, false);

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.India))
			{
				batch = CreateExportBatch(exporter);
				AssertNotNull(batch);
				AssertBatchContainsGovtChargeCode(batch, true);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.India))
			{
				AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				batch = CreateExportBatch(exporter);
				AssertNotNull(batch);
				AssertBatchContainsGovtChargeCode(batch, false);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Mexico))
			{
				batch = CreateExportBatch(exporter);
				AssertNotNull(batch);
				AssertBatchContainsGovtChargeCode(batch, true);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Mexico))
			{
				AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				batch = CreateExportBatch(exporter);
				AssertNotNull(batch);
				AssertBatchContainsGovtChargeCode(batch, false);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Brazil))
			{
				batch = CreateExportBatch(exporter);
				AssertNotNull(batch);
				AssertBatchContainsGovtChargeCode(batch, false);
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Constants.CountryCodes.Brazil))
			{
				AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				batch = CreateExportBatch(exporter);
				AssertNotNull(batch);
				AssertBatchContainsGovtChargeCode(batch, true);
			}
		}

		TransactionBatch CreateExportBatch(AccountingTransactionEAdaptorExporter exporter)
		{
			var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			const string nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("1001" + DateTime.Now.Second), false);
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("11" + DateTime.Now.Second, TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			invoice.Lines[0].AL_JH = job.PK;
			invoice.Lines[0].AL_GovtChargeCode = "RNDMTEXT";

			TestObjectCreator.CreateCharge(invoice.Lines[0]).JR_OSSellAmt = 0;
			Factory.Save();

			var batchResponse = exporter.CreateAndExportBatch(GetWriterStrategy(), loginCompanyCode, nameSpace);
			Assert(string.IsNullOrEmpty(batchResponse.ErrorMessage));
			AssertEquals("New batch 1 is created and exported", batchResponse.InfoMessage);
			var batch = batchResponse.DataObject as TransactionBatch;

			return batch;
		}

		void AssertBatchContainsGovtChargeCode(TransactionBatch batch, bool shouldContainChargeCode)
		{
			using (var batchStream = (SubStreamableStream)new MemoryStream())
			{
				new XmlWriter().WriteXML(batch, batchStream, false);
				using (var batchReader = new StreamReader(batchStream))
				{
					var actualXML = batchReader.ReadToEnd();
					AssertEquals(shouldContainChargeCode, actualXML.Contains("RNDMTEXT"));
				}
			}
		}

		void SetStmDateValue(ZString name, ZDateTime value)
		{
			var stmData = Factory.New<StmData>();
			stmData.SD_Name = name;
			stmData.SD_Owner = GlbCompany.CurrentCompany.PK;
			var dateTime = value;
			stmData.SD_BinaryValue = Encoding.Unicode.GetBytes(dateTime.SqlFormat.ToString());
		}

		ZDateTime LoadStmDateValue(ZString name)
		{
			Factory.ClearQueryCache(StmDataSchema.Constants.TableName);
			var query = new ZQuery()
				.AddToFilter(StmDataSchema.SD_Name, name)
				.AddToFilter(StmDataSchema.SD_Owner, GlbCompany.CurrentCompany.PK);
			var stmData = Factory.LoadTop1<StmData>(query);
			return stmData == null ? ZDateTime.Empty : SqlFormatInfo.FromSqlDateTime(Encoding.Unicode.GetString(stmData.SD_BinaryValue));
		}

		AccountingTransactionDataObjectWriterStrategy GetWriterStrategy() => new AccountingTransactionDataObjectWriterStrategy(context: "eAdaptor");

		protected override void SetUp()
		{
			base.SetUp();

			Connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			Transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;
			TestObjectCreator = new TestObjectCreator(Factory);

			var glHeader = TestObjectCreator.GLHeader1;
			glHeader.AG_AccountNum = "1234.56.78";
			glHeader.AG_Description = "Test GL Header";
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid());
			AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			embeddedResourceRetriever = new();
		}
		protected override void TearDown()
		{
			base.TearDown();

			embeddedResourceRetriever?.Dispose();
			embeddedResourceRetriever = null;
		}

		System.Data.Common.DbConnection Connection;
		DbTransaction Transaction;
		TestObjectCreator TestObjectCreator;

		EmbeddedResourceRetriever embeddedResourceRetriever;
	}

	[UseSnapshotProtection]
	public class TestNonTransactionedAccountingEAdaptorExporter : TestCase
	{
		public void TestCreateBatch_NoInvalidOperationException_WhenResourceStringsAreLoaded()
		{
			ResourcesDeltaSource.UseDatabaseStrings.Value = true;
			using (ObjectFactory.Get<IResourceStrings>().TemporarilySwitchLanguage("ZH-CN"))
			{
				var connection = ((IDbConnectionInternals)Db.Connection).ADOConnection;

				var loginCompanyCode = GlbCompany.CurrentCompany.GC_Code;
				var nameSpace = "http://www.cargowise.com/Schemas/Universal/2011/11";
				var dataAccess = new BatchExportDataAccess(connection, null);
				var exporter = new AccountingTransactionEAdaptorExporter(dataAccess);

				AssertNoExceptionThrown("No exception should occur while loading resource string data from the database.", () =>
				{
					var result = exporter.CreateBatch(new AccountingTransactionDataObjectWriterStrategy(context: "eAdaptor"), loginCompanyCode, nameSpace);
					AssertNullOrEmpty("Expected no error message", result.ErrorMessage);
				});
			}
		}
	}

	class AccountingTransactionEAdaptorExporterForTest : AccountingTransactionEAdaptorExporter
	{
		public AccountingTransactionEAdaptorExporterForTest(BatchExportDataAccess dataAccess)
			: base(dataAccess)
		{
		}

		protected override DateTime GetUtcNow(DbTransaction transaction)
		{
			return (TestDateAttribute.IsActive) ? TestDateAttribute.Date : DateTime.UtcNow;
		}

		protected override DbTransaction GetTransaction()
		{
			return DataAccess.Transaction;
		}

		protected override void CommitTransaction(DbTransaction transaction)
		{
			//do nothing
		}

		protected override void DisposeTransaction(DbTransaction transaction)
		{
			//do nothing
		}

		protected override void RollBackTransaction(DbTransaction transaction)
		{
			//do nothing
		}
	}

	class AccountingTransactionEAdaptorExporterForStackTraceTest : AccountingTransactionEAdaptorExporterForTest
	{
		public AccountingTransactionEAdaptorExporterForStackTraceTest(BatchExportDataAccess dataAccess)
			: base(dataAccess)
		{
		}

		protected override DateTime GetUtcNow(DbTransaction transaction)
		{
			try
			{
				using (var cmd = DataAccess.GetCommandWithTransaction("This is the inner exception.", transaction))
				{
					cmd.ExecuteScalar();
				}
			}
			catch (Exception innerEx)
			{
				try
				{
					var openLog = File.Open("FileDoesNotExist", FileMode.Open);
				}
				catch
				{
					throw new FileNotFoundException("This is the outter exception.", innerEx);
				}
			}

			return DateTime.Now;
		}
	}

	class AccountingTransactionEAdaptorExporterForRollBackTest : AccountingTransactionEAdaptorExporter
	{
		public AccountingTransactionEAdaptorExporterForRollBackTest(BatchExportDataAccess dataAccess)
			: base(dataAccess)
		{
		}

		protected override DbTransaction GetTransaction()
		{
			return DataAccess.Transaction;
		}

		protected override void CommitTransaction(DbTransaction transaction)
		{
			throw new Exception("Test Error for RollBack.");
		}
	}
}
