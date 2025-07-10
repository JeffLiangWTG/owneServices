using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class InvoicingLineBaseFetchStrategyTest : TestCaseWithFactory
	{
		protected TestObjectCreator TestObjectCreator;

		[SuspendCriticalValidation]
		public void TestFetchForLoad_AL_JH_WithInterdependentJobsAndInvoices_WithoutJobClosedCommissionContext()
		{
			var jobPks = CreateInterdependentJobsAndInvoiceLines();
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			factory.EnableTableHitQueryCollection(new[] { AccTransactionLines.Schema.TableName });

			// Scenario (based on JobClosedCommissionCreator):
			// 1. You have a Job
			var job = factory.Load<JobHeader>(jobPks[0]);

			// 2. Find related AR Invoices
			var jobInvoices = LoadRelatedARInvoices(job);

			// 3. Access the invoice line items
			var inv1 = jobInvoices[0];
			AssertEquals("Precondition: expect three lines (and trigger AccTransactionLine loading)", 4, inv1.Lines.Count);
			var lineSelects = factory.TableSelects.First(x => x.TableName == AccTransactionLines.Schema.TableName);
			AssertEquals("There should be one AccTransactionLine fetch for first collection of invoice lines", 1, lineSelects.Value);
			var queryForLines = lineSelects.Queries.First().Query;
			AssertEquals("SQL should contain three references to AL_AH (SELECT * 1, WHERE * 2)", 3, queryForLines.Split(new[] { "AL_AH" }, System.StringSplitOptions.None).Length - 1);
			AssertEquals("SQL should contain two references to AL_GC (SELECT * 1, WHERE * 1)", 2, queryForLines.Split(new[] { "AL_GC" }, System.StringSplitOptions.None).Length - 1);

			var inv2 = jobInvoices[1];
			AssertEquals("Precondition: expect three lines (and trigger AccTransactionLine loading)", 4, inv2.Lines.Count);
			lineSelects = factory.TableSelects.First(x => x.TableName == AccTransactionLines.Schema.TableName);
			AssertEquals("There should be three AccTransactionLine fetches for second collection of invoice lines (actual fetch + fetch hints)", 3, lineSelects.Value);
			var fetchHintQuery = lineSelects.Queries.Skip(1).First().Query;
			AssertEquals("SQL should contain five references to AL_JH (SELECT * 1, WHERE * 1 (IN clause))", 2, fetchHintQuery.Split(new[] { "AL_JH" }, System.StringSplitOptions.None).Length - 1);
			AssertEquals("SQL should contain one reference to AL_GC (SELECT * 1, WHERE * 0)", 1, fetchHintQuery.Split(new[] { "AL_GC" }, System.StringSplitOptions.None).Length - 1);
			queryForLines = lineSelects.Queries.Skip(2).First().Query;
			AssertEquals("SQL should contain three references to AL_AH (SELECT * 1, WHERE * 2)", 3, queryForLines.Split(new[] { "AL_AH" }, System.StringSplitOptions.None).Length - 1);
			AssertEquals("SQL should contain two references to AL_GC (SELECT * 1, WHERE * 1)", 2, queryForLines.Split(new[] { "AL_GC" }, System.StringSplitOptions.None).Length - 1);
		}

		[SuspendCriticalValidation]
		public void TestFetchForLoad_AL_JH_WithInterdependentJobsAndInvoices_WithJobClosedCommissionContext()
		{
			var jobPks = CreateInterdependentJobsAndInvoiceLines();
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			factory.EnableTableHitQueryCollection(new[] { AccTransactionLines.Schema.TableName });
			using (factory.SetTempContext(Enterprise.Integration.Accounting.BusinessContext.CalculateCommissionOnJobClosure))
			{
				// Scenario (based on JobClosedCommissionCreator):
				// 1. You have a Job
				var job = factory.Load<JobHeader>(jobPks[0]);

				// 2. Find related AR Invoices
				var jobInvoices = LoadRelatedARInvoices(job);

				// 3. Access the invoice line items
				var inv1 = jobInvoices[0];
				AssertEquals("Precondition: expect three lines (and trigger AccTransactionLine loading)", 4, inv1.Lines.Count);
				var lineSelects = factory.TableSelects.First(x => x.TableName == AccTransactionLines.Schema.TableName);
				AssertEquals("There should be one AccTransactionLine fetch for first collection of invoice lines", 1, lineSelects.Value);
				var queryForLines = lineSelects.Queries.First().Query;
				AssertEquals("SQL should contain three references to AL_AH (SELECT * 1, WHERE * 2)", 3, queryForLines.Split(new[] { "AL_AH" }, System.StringSplitOptions.None).Length - 1);
				AssertEquals("SQL should contain two references to AL_GC (SELECT * 1, WHERE * 1)", 2, queryForLines.Split(new[] { "AL_GC" }, System.StringSplitOptions.None).Length - 1);

				var inv2 = jobInvoices[1];
				AssertEquals("Precondition: expect three lines (and trigger AccTransactionLine loading)", 4, inv2.Lines.Count);
				lineSelects = factory.TableSelects.First(x => x.TableName == AccTransactionLines.Schema.TableName);
				AssertEquals("There should be two AccTransactionLine fetches for second collection of invoice lines", 2, lineSelects.Value);
				queryForLines = lineSelects.Queries.Skip(1).First().Query;
				AssertEquals("SQL should contain three references to AL_AH (SELECT * 1, WHERE * 2)", 3, queryForLines.Split(new[] { "AL_AH" }, System.StringSplitOptions.None).Length - 1);
				AssertEquals("SQL should contain two references to AL_GC (SELECT * 1, WHERE * 1)", 2, queryForLines.Split(new[] { "AL_GC" }, System.StringSplitOptions.None).Length - 1);
			}
		}

		ARInvoice[] LoadRelatedARInvoices(JobHeader job)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, "INV");
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, "AR");
			var lineSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
			lineSubQuery.AddToFilter(AccTransactionLinesSchema.AL_JH, job.PK);
			query.AddSubQuery(lineSubQuery, JoinCondition.And);

			return job.Factory.Load<ARInvoice>(query);
		}

		List<ZGuid> CreateInterdependentJobsAndInvoiceLines()
		{
			TestObjectCreator.CreateARInvoice<ARInvoice>("NONJOBINV", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);

			var jobs = new List<JobInvoicing.Job>();
			for (int i = 0; i < 10; i++)
			{
				jobs.Add(TestObjectCreator.CreateJob($"JOB{i}", TestObjectCreator.ABIGAS, 1m, TestObjectCreator.AALSHI, 1m));
			}

			var jobIndex = 0;
			for (int i = 0; i < 20; i++)
			{
				var inv = TestObjectCreator.CreateARInvoice<ARInvoice>($"INV{i}", TestObjectCreator.AUD, 1m, TestObjectCreator.Debtor);
				for (int j = 0; j < 4; j++)
				{
					var jobForLine = jobs[jobIndex];
					TestObjectCreator.CreateARInvoiceLine(inv, jobForLine, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "Invoice Line for " + jobForLine.JH_JobNum, 3m * (j + 1) * (i + 1));
					jobIndex = ++jobIndex % jobs.Count;
				}
			}

			Factory.Save();
			return jobs.OrderBy(j => j.PK).Select(j => j.PK).ToList();
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(Factory);
		}
	}
}
