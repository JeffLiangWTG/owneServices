using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Universal.Testing
{
	public class TransactionBatchDataLoaderTest : TestCaseWithFactory
	{
		public void TestConstructor_SetsProperties()
		{
			var loader1 = new TransactionBatchDataLoader();
			AssertNotNull(loader1.Factory);

			var factory = Factory.GetCachedReadOnlyFactory();
			var loader2 = new TransactionBatchDataLoader(factory: factory);
			AssertSame(factory, loader2.Factory);
		}

		#region LoadBranchAndCompany()

		public void TestLoadBranchAndCompany_Throws_WhenNullBatch()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
				new TransactionBatchDataLoader().LoadBranchAndCompany(null)
			);
		}

		public void TestLoadBranchAndCompany_HasNotification_WhenEmptyTransactionCollection()
		{
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			var loader = new TransactionBatchDataLoader();

			var (loadedBranch, error) = loader.LoadBranchAndCompany(batch);

			AssertNull(loadedBranch);
			AssertEquals("Universal Transaction Batch contains no transactions.", error);
		}

		public void TestLoadBranchAndCompany_HasNotification_WhenMissingBranch()
		{
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			batch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance));

			var loader = new TransactionBatchDataLoader();

			var (loadedBranch, error) = loader.LoadBranchAndCompany(batch);

			AssertNull(loadedBranch);
			AssertEquals("Unable to determine Branch from Universal Transaction Batch - empty / missing Branch Code.", error);
		}

		public void TestLoadBranchAndCompany_HasNotification_WhenMissingBranchCode()
		{
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			batch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch()
			});

			var loader = new TransactionBatchDataLoader();

			var (loadedBranch, error) = loader.LoadBranchAndCompany(batch);

			AssertNull(loadedBranch);
			AssertEquals("Unable to determine Branch from Universal Transaction Batch - empty / missing Branch Code.", error);
		}

		public void TestLoadBranchAndCompany_HasNotification_WhenBranchNotInDatabase()
		{
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			batch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch() { Code = "NUL" }
			});

			var loader = new TransactionBatchDataLoader();

			var (loadedBranch, error) = loader.LoadBranchAndCompany(batch);

			AssertNull(loadedBranch);
			AssertEquals("Unable to load Branch based on Universal Transaction Batch.", error);
		}

		public void TestLoadBranchAndCompany_HasNotification_WhenManyTransactionsWithDifferentBranches()
		{
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			batch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch() { Code = "BR1" }
			});
			batch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch() { Code = "BR2" }
			});

			var loader = new TransactionBatchDataLoader();

			var (loadedBranch, error) = loader.LoadBranchAndCompany(batch);

			AssertNull(loadedBranch);
			AssertEquals("Unable to determine Branch from Universal Transaction Batch - multiple Transactions with different Branch Codes.", error);
		}

		public void TestLoadBranchAndCompany_ReturnBranch_WhenOneTransaction()
		{
			var branch = CreateBranchAndCompany();
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			batch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch() { Code = branch.GB_Code }
			});

			var loader = new TransactionBatchDataLoader();

			var (loadedBranch, error) = loader.LoadBranchAndCompany(batch);

			AssertNullOrEmpty(error);
			AssertNotNull(loadedBranch);
			AssertNotNull(loadedBranch.Company);
			AssertEquals(branch.PK, loadedBranch.PK);
		}

		#endregion

		#region LoadAccBatch()

		public void TestLoadAccBatch_HasError_MissingBatchNumber()
		{
			var company = CreateBranchAndCompany().Company;

			var loader = new TransactionBatchDataLoader();
			var (accBatch, error) = loader.LoadAccBatch(company, "");

			AssertEquals("Unable to load transaction batch: missing batch number.", error);
			AssertNull(accBatch);
		}

		public void TestLoadAccBatch_HasError_BatchNumberNotInteger()
		{
			var company = CreateBranchAndCompany().Company;

			var loader = new TransactionBatchDataLoader();
			var (accBatch, error) = loader.LoadAccBatch(company, "abczzz");

			AssertEquals("Unable to load transaction batch: batch number 'abczzz' is not a number.", error);
			AssertNull(accBatch);
		}

		public void TestLoadAccBatch_HasError_BatchNotInDatabase()
		{
			var company = CreateBranchAndCompany().Company;

			var loader = new TransactionBatchDataLoader();
			var (accBatch, error) = loader.LoadAccBatch(company, "12345");

			AssertEquals("Unable to load transaction batch: batch '12345' was not found for company 'CAU'.", error);
			AssertNull(accBatch);
		}

		public void TestLoadAccBatch_ReturnsBatch_SingleCompany()
		{
			var company = CreateBranchAndCompany().Company;
			var batch = CreateBatch(company);

			var loader = new TransactionBatchDataLoader();
			var (accBatch, error) = loader.LoadAccBatch(company, "12345");

			AssertNullOrEmpty(error);
			AssertNotNull(accBatch);
			AssertEquals(batch.PK, accBatch.PK);
		}

		public void TestLoadAccBatch_ReturnsBatch_MultipleCompaniesWithSameBatchNumber()
		{
			var company1 = CreateBranchAndCompany("AUBNE").Company;
			var company2 = CreateBranchAndCompany("NZAKL").Company;
			var batch1 = CreateBatch(company1);
			var batch2 = CreateBatch(company2);

			var loader = new TransactionBatchDataLoader();
			var (accBatch1, error1) = loader.LoadAccBatch(company1, "12345");

			AssertNullOrEmpty(error1);
			AssertNotNull(accBatch1);
			AssertEquals(batch1.PK, accBatch1.PK);

			var (accBatch2, error2) = loader.LoadAccBatch(company2, "12345");

			AssertNullOrEmpty(error2);
			AssertNotNull(accBatch2);
			AssertEquals(batch2.PK, accBatch2.PK);
		}

		#endregion

		#region LoadDepartment()

		public void TestLoadDepartment_Throws_WhenNullBatch()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
				new TransactionBatchDataLoader().LoadDepartment(null)
			);
		}

		public void TestLoadDepartment_HasNotification_WhenEmptyTransactionCollection()
		{
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			var loader = new TransactionBatchDataLoader();

			var (loadedDepartment, error) = loader.LoadDepartment(batch);

			AssertNull(loadedDepartment);
			AssertEquals("Universal Transaction Batch contains no transactions.", error);
		}

		public void TestLoadDepartment_HasNotification_WhenMissingDepartment()
		{
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			batch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance));
			var loader = new TransactionBatchDataLoader();

			var (loadedDepartment, error) = loader.LoadDepartment(batch);

			AssertNull(loadedDepartment);
			AssertEquals("Unable to determine Department from Universal Transaction Batch - empty / missing Department Code.", error);
		}

		public void TestLoadDepartment_HasNotification_WhenMissingDepartmentCode()
		{
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			batch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Department = new Department()
			});
			var loader = new TransactionBatchDataLoader();

			var (loadedDepartment, error) = loader.LoadDepartment(batch);

			AssertNull(loadedDepartment);
			AssertEquals("Unable to determine Department from Universal Transaction Batch - empty / missing Department Code.", error);
		}

		public void TestLoadDepartment_HasNotification_WhenDepartmentNotInDatabase()
		{
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			batch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Department = new Department() { Code = "XXX" }
			});

			var loader = new TransactionBatchDataLoader();

			var (loadedDepartment, error) = loader.LoadDepartment(batch);

			AssertNull(loadedDepartment);
			AssertEquals("Unable to load Department based on Universal Transaction Batch.", error);
		}

		public void TestLoadDepartment_HasNotification_WhenManyTransactionsWithDifferentDepartments()
		{
			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			batch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Department = new Department() { Code = "CIA" }
			});
			batch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Department = new Department() { Code = "FEA" }
			});

			var loader = new TransactionBatchDataLoader();

			var (loadedDepartment, error) = loader.LoadDepartment(batch);

			AssertNull(loadedDepartment);
			AssertEquals("Unable to determine Department from Universal Transaction Batch - multiple Transactions with different Department Codes.", error);
		}

		public void TestLoadDepartment_ReturnDepartment_WhenOneTransaction()
		{
			var department = TestObjectCreator.FEADepartment;

			var batch = new UniversalTransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			batch.TransactionCollection.Add(new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Department = new Department() { Code = department.GE_Code }
			});
			var loader = new TransactionBatchDataLoader();

			var (loadedDepartment, error) = loader.LoadDepartment(batch);

			AssertNullOrEmpty(error);
			AssertEquals("Department should have value", department.PK, loadedDepartment.PK);
		}
		#endregion

		#region Implementation

		GlbBranch CreateBranchAndCompany(string homePortUnloco = "AUBNE")
		{
			var branch = TestObjectCreator.CreateBranchWithCompany(homePortUnloco);
			TestObjectCreator.Factory.Save();
			return branch;
		}

		AccEInvoicingBatch CreateBatch(GlbCompany company)
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(12345, "RDY", company);
			TestObjectCreator.Factory.Save();
			return batch;
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(new BusinessObjectFactory()));

		TestObjectCreator testObjectCreator;

#endregion
	}
}
