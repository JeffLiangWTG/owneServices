using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JobFetchStrategyTest : TestCaseWithFactory
	{
		public void TestNoExceptionWhenLotsOfDistinctAddresses()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			int numberOfRecordsToCreate = 500;
			for (int index = 0; index < numberOfRecordsToCreate; index++)
			{
				OrgHeader localClientOrg = Factory.NewWithValidTestData<OrgHeader>();
				OrgHeader overseasAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
				JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job.JH_JobNum = "SHP" + index.ToString();
				job.JH_OA_LocalChargesAddr = localClientOrg.MainAddress.PK;
				job.JH_OA_AgentCollectAddr = overseasAgentOrg.MainAddress.PK;
			}

			Factory.RefreshEnabled = false;

			Factory.Save();

			BusinessObjectFactory factoryForReload = new BusinessObjectFactory();
			JobCollection jobs = new JobCollection(factoryForReload);
			jobs.Load();
			try
			{
				bool isGSTRegistered = jobs[0].LocalCharges.CompanyData.IsAPTaxApplicable;
			}
			catch
			{
				Fail("Should not throw exception when accessing MiscServ");
			}
			Assert(true);
		}

		public void TestParentFetchHintInMatchingReadOnlyContext()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentID = ZGuid.NewZGuid();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Factory.Save();
			AssertEquals(0, Factory.ActiveFetchHintsForTable(JobShipmentSchema.Constants.TableName));

			var factory1 = new BusinessObjectFactory();
			var job1 = factory1.Load<Job>(job.PK);
			AssertEquals(0, factory1.ActiveFetchHintsForTable(JobShipmentSchema.Constants.TableName));

			var factory2 = new BusinessObjectFactory();
			factory2.SetContext(BusinessContext.MatchingReadOnly);
			Assert(factory2.HasAnyOfContexts(BusinessContext.MatchingReadOnly));
			try
			{
				var job2 = factory2.Load<Job>(job.PK);
				AssertEquals(1, factory2.ActiveFetchHintsForTable(JobShipmentSchema.Constants.TableName));
			}
			finally
			{
				factory2.RemoveContext(BusinessContext.MatchingReadOnly);
			}
		}

		#region TestFetchForView

		public void TestFetchForView_ConsolNo()
		{
			var columnNamesToView = new[] { "JH_ConsolNo" };

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(ViewGenericJobSchema.Constants.TableName, 1);
			expectedDbHits.Add(ViewShipmentConsolAndMasterBillNumbersSchema.Constants.TableName, 1);

			AssertFetchForViewDbHits(columnNamesToView, expectedDbHits, true);
		}

		public void TestFetchForView_MasterBillNo()
		{
			var columnNamesToView = new[] { "JH_MasterBillNo" };

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(ViewGenericJobSchema.Constants.TableName, 1);
			expectedDbHits.Add(ViewShipmentConsolAndMasterBillNumbersSchema.Constants.TableName, 1);

			AssertFetchForViewDbHits(columnNamesToView, expectedDbHits, true);
		}

		public void TestFetchForView_HouseBillNo()
		{
			var columnNames = new[] { "JH_HouseBillNo" };

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(ViewGenericJobSchema.Constants.TableName, 1);
			expectedDbHits.Add(ViewShipmentConsolAndMasterBillNumbersSchema.Constants.TableName, 1);

			AssertFetchForViewDbHits(columnNames, expectedDbHits, true);
		}

		public void TestFetchForViewWhenColumnsNotVisible()
		{
			var columnNamesToView = new[] { "JH_ConsolNo", "JH_MasterBillNo", "JH_HouseBillNo" };

			var expectedDbHits = new Dictionary<string, int>();
			AssertFetchForViewDbHits(columnNamesToView, expectedDbHits, false);
		}

		void AssertFetchForViewDbHits(string[] viewColumnNames, Dictionary<string, int> expectedDbHits, bool areColumnsVisible)
		{
			var viewFactory = new BusinessObjectFactory();
			var testJobCollection = viewFactory.Load<Job>(new ZQuery(JobHeaderSchema.PK, CreateTestJobCollection()));
			viewFactory.ResetDatabaseLoadCount();

			foreach (var job in testJobCollection)
			{
				job.FetchStrategy.FetchForView(viewColumnNames.Select(x => new TableColumn("", x)).ToArray());
			}

			if (areColumnsVisible)
			{
				foreach (var job in testJobCollection)
				{
					object hitProperty;
					foreach (var columnName in viewColumnNames)
					{
						hitProperty = job[columnName];
					}
				}
			}

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			AssertDbHits(expectedDbHits, viewFactory);
		}

		List<ZGuid> CreateTestJobCollection()
		{
			var result = new List<ZGuid>();
			var testObjectCreator = new TestObjectCreator(Factory);

			for (int i = 1; i < 11; i++)
			{
				var consolNo = string.Format("C000{0}", i);
				var consol = testObjectCreator.CreateConsol("ABC", "DEF", consolNo);
				consol.JK_MasterBillNum = string.Format("123{0}", i);

				var shipmentNo = string.Format("S000{0}", i);
				var shipment = testObjectCreator.CreateShipment(shipmentNo, consol);
				shipment.JS_HouseBill = string.Format("456{0}", i);

				var job = testObjectCreator.CreateJob(shipment);
				testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "", testObjectCreator.AUD, 10m, testObjectCreator.Creditor1, testObjectCreator.AUD, 10m, testObjectCreator.Debtor);

				result.Add(job.PK);
			}

			Factory.Save();
			return result;
		}

		#endregion
	}
}
