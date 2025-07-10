using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public class IMatchingCollectionFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForView()
		{
			AssertNoJobFetchHintsOnFacvtoryAndReadOnlyFactory();

			fetchStrategy.FetchForView(new BusinessObject[] { header1 }, new TableColumn[] { column1 });
			AssertNoJobFetchHintsOnFacvtoryAndReadOnlyFactory();

			fetchStrategy.FetchForView(new BusinessObject[] { header1 }, new TableColumn[] { column1, column2 });
			AssertNoJobFetchHintsOnFacvtoryAndReadOnlyFactory();

			fetchStrategy.FetchForView(new BusinessObject[] { header1, header2 }, new TableColumn[] { column1 });
			AssertNoJobFetchHintsOnFacvtoryAndReadOnlyFactory();

			fetchStrategy.FetchForView(new BusinessObject[] { header1, header2 }, new TableColumn[] { column1, column2 });
			AssertEquals("Factory should not have Fetch Hints for Job", 0, Factory.ActiveFetchHintsForTable(JobHeaderSchema.Constants.TableName));
			AssertEquals("Read Only Factory should have a fetch Hints for Job", 1, Factory.GetCachedReadOnlyFactory().ActiveFetchHintsForTable(JobHeaderSchema.Constants.TableName));
			AssertEquals("Factory is not in MatchingReadOnly context", false, Factory.HasContext(BusinessContext.MatchingReadOnly));
			AssertEquals("Read Only Factory is in MatchingReadOnly context", true, Factory.GetCachedReadOnlyFactory().HasContext(BusinessContext.MatchingReadOnly));
		}

		void AssertNoJobFetchHintsOnFacvtoryAndReadOnlyFactory()
		{
			AssertEquals("Factory should not have Fetch Hints for Job", 0, Factory.ActiveFetchHintsForTable(JobHeaderSchema.Constants.TableName));
			AssertEquals("Read Only Factory should not have Fetch Hints for Job", 0, Factory.GetCachedReadOnlyFactory().ActiveFetchHintsForTable(JobHeaderSchema.Constants.TableName));
			AssertEquals("Factory is not in MatchingReadOnly context", false, Factory.HasContext(BusinessContext.MatchingReadOnly));
			AssertEquals("Read Only Factory is not in MatchingReadOnly context", false, Factory.GetCachedReadOnlyFactory().HasContext(BusinessContext.MatchingReadOnly));
		}

		protected override void SetUp()
		{
			base.SetUp();

			header1 = Factory.NewWithValidTestData<APInvoice>();
			header2 = Factory.NewWithValidTestData<ARInvoice>();

			job = Factory.NewJobWithValidTestDataForTesting<Job>();
			header2.AH_JH = job.PK;

			Factory.Save();

			var collection = new IMatchingCollection(Factory);
			collection.Add(header1);
			collection.Add(header2);

			fetchStrategy = new IMatchingCollectionFetchStrategy(collection);

			column1 = new TableColumn("IMatching", "");
			column2 = new TableColumn("IMatching", "VoyageVesselOrFlightDate");
		}

		BusinessObjectCollectionFetchStrategy fetchStrategy;

		AccTransactionHeader header1;
		AccTransactionHeader header2;

		TableColumn column1;
		TableColumn column2;

		Job job;
	}
}