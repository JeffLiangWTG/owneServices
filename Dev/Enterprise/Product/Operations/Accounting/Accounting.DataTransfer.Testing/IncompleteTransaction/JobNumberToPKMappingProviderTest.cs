using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public sealed class JobNumberToPKMappingProviderTest : TestCaseWithFactory
	{
		[SuspendToTestReportJobIsChangedByDifferentCompany]
		public void TestBatchLoadJobs()
		{
			var currentCompanyPK = Env.CurrentCompanyPK;
			var anotherCompanyPK = TestObjectCreator.NonCurrentCompany.PK;

			var jobNumber1 = "S000010";
			var jobNumber2 = "S000011";
			var jobNumber3 = "S000020";
			var jobNumber4 = "S000030";
			var inactiveJobNumber = "S000040";
			var jobInAnotherCompanyNumber = "S000050";

			var job1 = CreateJobHeader(jobNumber1, currentCompanyPK, isActive: true);
			var job2 = CreateJobHeader(jobNumber2, currentCompanyPK, isActive: true);
			var job3 = CreateJobHeader(jobNumber3, currentCompanyPK, isActive: true);
			var jobInAnotherCompany = CreateJobHeader(jobInAnotherCompanyNumber, anotherCompanyPK, isActive: true);
			var inactiveJob = CreateJobHeader(inactiveJobNumber, anotherCompanyPK, isActive: false);

			Factory.Save();

			var consolCosts = new ConsolCostCollection()
			{
				CreateConsolCost(new List<string>() { jobNumber1, jobNumber2 }),
				CreateConsolCost(new List<string>() { jobNumber1, jobNumber3, jobInAnotherCompanyNumber })
			};
			var transaction = new IncompleteTransactionHeader();
			transaction.JobRelatedLines = CreateJobRelatedLines(new List<string>() { jobNumber1, jobNumber4, inactiveJobNumber });
			transaction.ConsolCosts = consolCosts;

			var jobNumberToPKMappingProvider = new JobNumberToPKMappingProvider(Factory, new IncompleteTransactionDataAdapter<InvoicingBase>(Factory.New<APInvoice>()).LoadJobs_ForTestOnly);
			jobNumberToPKMappingProvider.BatchLoadJobs(transaction);

			var job4 = CreateJobHeader(jobNumber4, currentCompanyPK, isActive: true);
			AssertEquals("Job4 should not in Database", false, job4.IsInDatabase);

			AssertEquals("jobNumberToPKMapping should only has 3 Jobs due to one Job is NOT saved into DB.", 3, jobNumberToPKMappingProvider.jobNumberToPKMapping_ForTestOnly.Count);
			AssertEquals(false, jobNumberToPKMappingProvider.jobNumberToPKMapping_ForTestOnly.ContainsKey(jobNumber4));

			AssertEquals(3, jobNumberToPKMappingProvider.jobNumbersNotMapped_ForTestOnly.Count);
			AssertEquals(true, jobNumberToPKMappingProvider.jobNumbersNotMapped_ForTestOnly.Contains(jobNumber4));
			AssertEquals(true, jobNumberToPKMappingProvider.jobNumbersNotMapped_ForTestOnly.Contains(inactiveJobNumber));
			AssertEquals(true, jobNumberToPKMappingProvider.jobNumbersNotMapped_ForTestOnly.Contains(jobInAnotherCompanyNumber));

			AssertEquals(4, jobNumberToPKMappingProvider.JobNumberToPKMapping.Count);
			AssertEquals(job1.PK, jobNumberToPKMappingProvider.JobNumberToPKMapping[jobNumber1]);
			AssertEquals(job2.PK, jobNumberToPKMappingProvider.JobNumberToPKMapping[jobNumber2]);
			AssertEquals(job3.PK, jobNumberToPKMappingProvider.JobNumberToPKMapping[jobNumber3]);
			AssertEquals(job4.PK, jobNumberToPKMappingProvider.JobNumberToPKMapping[jobNumber4]);

			AssertEquals(true, jobNumberToPKMappingProvider.jobNumberToPKMapping_ForTestOnly.ContainsKey(jobNumber4));

			AssertEquals(2, jobNumberToPKMappingProvider.jobNumbersNotMapped_ForTestOnly.Count);
			AssertEquals(true, jobNumberToPKMappingProvider.jobNumbersNotMapped_ForTestOnly.Contains(inactiveJobNumber));
			AssertEquals(true, jobNumberToPKMappingProvider.jobNumbersNotMapped_ForTestOnly.Contains(jobInAnotherCompanyNumber));
		}

		public void TestBuildJobNumber_EmptyJobNumbers()
		{
			var transaction = new IncompleteTransactionHeader();
			transaction.ConsolCosts = null;
			transaction.JobRelatedLines = null;
			var jobNumberToPKMappingProvider = new JobNumberToPKMappingProvider(Factory, new IncompleteTransactionDataAdapter<InvoicingBase>(Factory.New<APInvoice>()).LoadJobs_ForTestOnly);
			jobNumberToPKMappingProvider.BatchLoadJobs(transaction);

			AssertEquals(0, jobNumberToPKMappingProvider.JobNumberToPKMapping.Count);
		}

		public void TestGetJobNumberToPKMapping()
		{
			var jobNumber1 = "S000010";
			var jobNumber2 = "S000011";
			var job1 = CreateJobHeader(jobNumber1, Env.CurrentCompanyPK, isActive: true);
			Factory.Save();

			var transaction = new IncompleteTransactionHeader();
			transaction.JobRelatedLines = CreateJobRelatedLines(new List<string>() { jobNumber1, jobNumber2 });

			var jobNumberToPKMappingProvider = new JobNumberToPKMappingProvider(Factory, new IncompleteTransactionDataAdapter<InvoicingBase>(Factory.New<APInvoice>()).LoadJobs_ForTestOnly);
			jobNumberToPKMappingProvider.BatchLoadJobs(transaction);

			var job2 = CreateJobHeader(jobNumber2, Env.CurrentCompanyPK, isActive: true);

			AssertEquals(1, jobNumberToPKMappingProvider.jobNumberToPKMapping_ForTestOnly.Count);
			AssertEquals(job1.PK, jobNumberToPKMappingProvider.jobNumberToPKMapping_ForTestOnly[jobNumber1]);
			AssertEquals(1, jobNumberToPKMappingProvider.jobNumbersNotMapped_ForTestOnly.Count);
			AssertCollectionContains(jobNumber2, jobNumberToPKMappingProvider.jobNumbersNotMapped_ForTestOnly);

			AssertEquals(2, jobNumberToPKMappingProvider.JobNumberToPKMapping.Count);
			AssertEquals(job1.PK, jobNumberToPKMappingProvider.JobNumberToPKMapping[jobNumber1]);
			AssertEquals(job2.PK, jobNumberToPKMappingProvider.JobNumberToPKMapping[jobNumber2]);
			AssertEquals(0, jobNumberToPKMappingProvider.jobNumbersNotMapped_ForTestOnly.Count);
		}

		public void TestJobNumbersCollectionShouldBeCleared_WhenBatchLoadJobs()
		{
			var jobNumberToPKMappingProvider = new JobNumberToPKMappingProvider(Factory, new IncompleteTransactionDataAdapter<InvoicingBase>(Factory.New<APInvoice>()).LoadJobs_ForTestOnly);

			var jobPK = ZGuid.NewZGuid();
			jobNumberToPKMappingProvider.jobNumberToPKMapping_ForTestOnly["S0001"] = jobPK;
			jobNumberToPKMappingProvider.jobNumbersNotMapped_ForTestOnly.Add("S0002");
			AssertEquals(1, jobNumberToPKMappingProvider.jobNumberToPKMapping_ForTestOnly.Count);
			AssertEquals(jobPK, jobNumberToPKMappingProvider.jobNumberToPKMapping_ForTestOnly["S0001"]);
			AssertEquals(1, jobNumberToPKMappingProvider.jobNumbersNotMapped_ForTestOnly.Count);
			AssertCollectionContains("S0002", jobNumberToPKMappingProvider.jobNumbersNotMapped_ForTestOnly);

			var transaction = new IncompleteTransactionHeader();
			jobNumberToPKMappingProvider.BatchLoadJobs(transaction);
			AssertEquals(0, jobNumberToPKMappingProvider.jobNumberToPKMapping_ForTestOnly.Count);
			AssertEquals(0, jobNumberToPKMappingProvider.jobNumbersNotMapped_ForTestOnly.Count);
		}

		Job CreateJobHeader(string jobNumber, ZGuid companyPK, bool isActive)
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_GC = companyPK;
			job.JH_JobNum = jobNumber;
			if (!isActive)
			{
				job.MarkAsInactive();
			}
			return job;
		}

		IncompleteTransactionLineCollection CreateJobRelatedLines(List<string> jobNumbers)
		{
			var jobRelatedLines = new IncompleteTransactionLineCollection();
			foreach (var jobNumber in jobNumbers)
			{
				var line = new IncompleteTransactionLine();
				line.JobNumber = jobNumber;
				jobRelatedLines.Add(line);
			}

			return jobRelatedLines;
		}

		ConsolCost CreateConsolCost(List<string> jobNumbers)
		{
			var consolCost = new ConsolCost();
			consolCost.ConsolCostCharges = CreateConsolCostCharges(jobNumbers);
			return consolCost;
		}

		ConsolCostChargeCollection CreateConsolCostCharges(List<string> jobNumbers)
		{
			var consolCostCharges = new ConsolCostChargeCollection();
			foreach (var jobNumber in jobNumbers)
			{
				var consolCostCharge = new ConsolCostCharge();
				consolCostCharge.JobNumber = jobNumber;
				consolCostCharges.Add(consolCostCharge);
			}

			return consolCostCharges;
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
