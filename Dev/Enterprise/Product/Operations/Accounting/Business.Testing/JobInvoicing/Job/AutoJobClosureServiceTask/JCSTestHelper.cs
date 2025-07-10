using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JCSTestHelper
	{
		public JCSTestHelper(TestObjectCreator objectCreator)
		{
			testObjectCreator = Argument.NotNull(objectCreator, nameof(objectCreator));
			factory = testObjectCreator.Factory;
		}
		readonly TestObjectCreator testObjectCreator;
		readonly BusinessObjectFactory factory;

		public GlbCompany[] CreateCompany(int noOfCompany)
		{
			GlbCompany[] companies = new GlbCompany[noOfCompany];
			for (int i = 0; i < noOfCompany; i++)
			{
				companies[i] = testObjectCreator.CreateNewCompany("CO" + i.ToString());
			}
			return companies;
		}

		public GlbBranch[] CreateBranch(GlbCompany[] companies)
		{
			GlbBranch[] branches = new GlbBranch[companies.Length];
			for (int i = 0; i < companies.Length; i++)
			{
				branches[i] = testObjectCreator.CreateBranch("B" + i.ToString(), companies[i]);
			}
			return branches;
		}

		public void SetRegistryValue(Guid companyPK, params JobClosureConfiguration[] config)
		{
			var regValue = testObjectCreator.CreateJobClosureConfiguration(config);
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(companyPK, Guid.Empty, Guid.Empty, regValue);
		}

		public List<Job> CreateJobs(int numberOfJobs, params GlbCompany[] companies)
		{
			var jobs = new List<Job>();
			var random = new Random(1);
			for (int i = 1; i <= numberOfJobs; i++)
			{
				var companyIndex = random.Next(0, companies.Length - 1);
				jobs.Add(CreateSHPJob(companies[companyIndex].PK, companies[companyIndex].Branches[0].PK, Env.Time.CurrentUtcDate));
				factory.Save();
			}
			return jobs;
		}

		public Job CreateSHPJob(ZGuid companyPK, ZGuid branchPK, ZDateTime jOP)
		{
			var plugin = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment);
			var job = testObjectCreator.CreateJob(plugin, false);
			job.JH_GC = companyPK;
			job.JH_GB = branchPK;
			job.JH_A_JOP = jOP;
			return job;
		}

		public IEnumerable<(ZGuid JobPK, ZGuid CompanyPK, ZDateTime CreationTime)> GetQueuedJobs()
		{
			#region SuppressResourceStringsCheckRegion

			var queueJobs = new List<(ZGuid JobPK, ZGuid CompanyPK, ZDateTime CreationTime)>();

			var sqlText = "SELECT JHC_JH, JHC_GC, JHC_SystemCreateTimeUtc FROM dbo.JobToCloseQueue";
			var factory = new BusinessObjectFactory();
			var bizObjCollection = new DynamicBusinessObjectCollection(factory);
			bizObjCollection.Load(sqlText);

			foreach (DynamicBusinessObject bizObj in bizObjCollection)
			{
				queueJobs.Add((new ZGuid(bizObj["JHC_JH"]), new ZGuid(bizObj["JHC_GC"]), new ZDateTime(bizObj["JHC_SystemCreateTimeUtc"])));
			}

			#endregion

			return queueJobs;
		}

		public Job CreateTHJob(ZGuid branchPK, ZGuid companyPK, ZString jobNumber)
		{
			var ratingHeader = NewClientRate(testObjectCreator.AALSHI, companyPK);
			ratingHeader.TH_QuoteNumber = jobNumber;

			var spotQuoteJob = testObjectCreator.Factory.NewJobForTesting<Job>();
			spotQuoteJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			spotQuoteJob.JH_GB = branchPK;
			spotQuoteJob.JH_GC = companyPK;
			spotQuoteJob.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
			spotQuoteJob.JH_JobNum = jobNumber;
			spotQuoteJob.JH_ParentID = ratingHeader.PK;
			spotQuoteJob.JH_A_JOP = Env.Time.CurrentUtcDate.AddDays(-20);
			return spotQuoteJob;
		}

		public ClientRate NewClientRate(OrgHeader client, ZGuid companyPK)
		{
			var result = testObjectCreator.Factory.NewWithValidTestData<ClientRate>();
			if (client != null)
			{
				result.TH_OH = client.PK;
				client.OH_IsDebtor = true;
				result.TH_GC = companyPK;
			}

			var clientRateEntry = result.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.FCL, "CNSHA", "AUSYD");
			var clientRateLine1 = clientRateEntry.AddRateLine("DDOC", CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode);
			clientRateLine1.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 0.5;
			clientRateLine1.TL_Rounding = RatingRoundingTypes.Chargeable;

			return result;
		}
	}
}
