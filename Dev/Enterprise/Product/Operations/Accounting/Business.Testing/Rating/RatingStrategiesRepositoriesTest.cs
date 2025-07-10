using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using static Enterprise.Accounting.Business.AutoRatingStarterCoreTest;

namespace Enterprise.Accounting.Business
{
	public class RatingStrategiesRepositoryTest : TestCaseWithFactory
	{
		public void TestShouldJobBeAutoRated_ModuleDoesNotSupportPeriodicRating_ExcludeIsFalse()
		{
			var interactor = new TestInteractor();
			var dummy1 = new DummyAutoRatingObject(Factory);
			var dummy2 = new DummyAutoRatingObject(Factory);
			var dummy3 = new DummyAutoRatingObject(Factory);

			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_ParentID = dummy1.PK;
			job1.JH_JobNum = "JOB1";
			job1.JH_ExcludeFromPeriodicRating = false;

			var job2 = Factory.NewJobForTesting<Job>();
			job2.JH_ParentID = dummy2.PK;
			job2.JH_JobNum = "JOB2";
			job2.JH_ExcludeFromPeriodicRating = false;

			var job3 = Factory.NewJobForTesting<Job>();
			job3.JH_ParentID = dummy3.PK;
			job3.JH_JobNum = "JOB3";
			job3.JH_ExcludeFromPeriodicRating = false;

			dummy1.additionalJobsExposed = new ReadOnlyCollection<IJobInvoicingPlugIn>(new[] { dummy2, dummy3 });

			RatingStrategiesRepository strategiesRepository = new RatingStrategiesRepository(interactor, null, dummy1, BillingType.Invoicing, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null);
			AssertContainsExactElementsInAnyOrder
			(
				"Actual jobs being auto rated do not match expected jobs",
				(Job x) => x.JH_JobNum,
				new Job[] { job1, job2, job3 },
				strategiesRepository.Strategies.Select(x => x.Job)
			);
		}

		public void TestShouldJobBeAutoRated_ModuleDoesNotSupportPeriodicRating_ExcludeIsTrue()
		{
			var interactor = new TestInteractor();
			var dummy1 = new DummyAutoRatingObject(Factory);
			var dummy2 = new DummyAutoRatingObject(Factory);
			var dummy3 = new DummyAutoRatingObject(Factory);

			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_ParentID = dummy1.PK;
			job1.JH_JobNum = "JOB1";
			job1.JH_ExcludeFromPeriodicRating = true;

			var job2 = Factory.NewJobForTesting<Job>();
			job2.JH_ParentID = dummy2.PK;
			job2.JH_JobNum = "JOB2";
			job2.JH_ExcludeFromPeriodicRating = true;

			var job3 = Factory.NewJobForTesting<Job>();
			job3.JH_ParentID = dummy3.PK;
			job3.JH_JobNum = "JOB3";
			job3.JH_ExcludeFromPeriodicRating = true;

			dummy1.additionalJobsExposed = new ReadOnlyCollection<IJobInvoicingPlugIn>(new[] { dummy2, dummy3 });

			RatingStrategiesRepository strategiesRepository = new RatingStrategiesRepository(interactor, null, dummy1, BillingType.Invoicing, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null);
			AssertContainsExactElementsInAnyOrder
			(
				"Actual jobs being auto rated do not match expected jobs",
				(Job x) => x.JH_JobNum,
				new Job[] { job1, job2, job3 },
				strategiesRepository.Strategies.Select(x => x.Job)
			);
		}

		public void TestShouldJobBeAutoRated_ModuleDoesSupportPeriodicRating_ExcludeIsTrue()
		{
			var interactor = new TestInteractor();
			var dummy1 = new DummyAutoRatingPeriodicObject(Factory);
			var dummy2 = new DummyAutoRatingPeriodicObject(Factory);
			var dummy3 = new DummyAutoRatingPeriodicObject(Factory);

			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_ParentID = dummy1.PK;
			job1.JH_JobNum = "JOB1";
			job1.JH_ExcludeFromPeriodicRating = true;

			var job2 = Factory.NewJobForTesting<Job>();
			job2.JH_ParentID = dummy2.PK;
			job2.JH_JobNum = "JOB2";
			job2.JH_ExcludeFromPeriodicRating = true;

			var job3 = Factory.NewJobForTesting<Job>();
			job3.JH_ParentID = dummy3.PK;
			job3.JH_JobNum = "JOB3";
			job3.JH_ExcludeFromPeriodicRating = true;

			dummy1.additionalJobsExposed = new ReadOnlyCollection<IJobInvoicingPlugIn>(new[] { dummy2, dummy3 });

			RatingStrategiesRepository strategiesRepository = new RatingStrategiesRepository(interactor, null, dummy1, BillingType.Invoicing, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null);
			/**
			 * The parent job belonging to the module (eg WhsInvoice) would always be auto rated and is not subject 
			 * to the value of the flag - JH_ExcludeFromPeriodicRating. Hence we assert to check that just the parent job
			 * has been taken for auto rating. 
			 */
			AssertContainsExactElementsInAnyOrder
			(
				"Actual jobs being auto rated do not match expected jobs",
				(Job x) => x.JH_JobNum,
				new Job[] { job1 },
				strategiesRepository.Strategies.Select(x => x.Job)
			);
		}

		public void TestShouldJobBeAutoRated_ModuleDoesSupportPeriodicRating_ExcludeIsFalse()
		{
			var interactor = new TestInteractor();
			var dummy1 = new DummyAutoRatingPeriodicObject(Factory);
			var dummy2 = new DummyAutoRatingPeriodicObject(Factory);
			var dummy3 = new DummyAutoRatingPeriodicObject(Factory);

			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_ParentID = dummy1.PK;
			job1.JH_JobNum = "JOB1";
			job1.JH_ExcludeFromPeriodicRating = false;

			var job2 = Factory.NewJobForTesting<Job>();
			job2.JH_ParentID = dummy2.PK;
			job2.JH_JobNum = "JOB2";
			job2.JH_ExcludeFromPeriodicRating = false;

			var job3 = Factory.NewJobForTesting<Job>();
			job3.JH_ParentID = dummy3.PK;
			job3.JH_JobNum = "JOB3";
			job3.JH_ExcludeFromPeriodicRating = false;

			dummy1.additionalJobsExposed = new ReadOnlyCollection<IJobInvoicingPlugIn>(new[] { dummy2, dummy3 });

			RatingStrategiesRepository strategiesRepository = new RatingStrategiesRepository(interactor, null, dummy1, BillingType.Invoicing, AdditionalJobsAction.AutoRateAdditionalInvoicingJobs, null);

			AssertContainsExactElementsInAnyOrder
			(
				"Actual jobs being auto rated do not match expected jobs",
				(Job x) => x.JH_JobNum,
				new Job[] { job1, job2, job3 },
				strategiesRepository.Strategies.Select(x => x.Job)
			);
		}

		public void TestGetRatingStrategyFromRatingSupporter_CostSupporterWithBlankPKAndBillingTypeInvoicing()
		{
			var interactor = new TestInteractor();
			var dummy1 = new DummyAutoRatingConsolObject(Factory);

			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_ParentID = dummy1.PK;
			job1.JH_JobNum = "JOB1";
			job1.JH_ExcludeFromPeriodicRating = false;

			var strategiesRepository = new RatingStrategiesRepository(interactor, null, dummy1, BillingType.Invoicing, AdditionalJobsAction.NoAction, null);
			var strategy = strategiesRepository.Strategies.Single();
			Assert("Strategy should be AutoRateInvoicingStrategy", strategy is AutoRateInvoicingStrategy);
		}

		public void TestGetRatingStrategyFromRatingSupporter_CostSupporterWithNonBlankPKAndBillingTypeInvoicing()
		{
			var interactor = new TestInteractor();
			var dummy1 = new DummyAutoRatingJobCostingPlugInObject(Factory);

			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_ParentID = dummy1.PK;
			job1.JH_JobNum = "JOB1";
			job1.JH_ExcludeFromPeriodicRating = false;

			var strategiesRepository = new RatingStrategiesRepository(interactor, null, dummy1, BillingType.Invoicing, AdditionalJobsAction.NoAction, null);
			var strategy = strategiesRepository.Strategies.Single();
			Assert("Strategy should be AutoRateInvoicingStrategy", strategy is AutoRateInvoicingStrategy);
		}

		public void TestGetRatingStrategyFromRatingSupporter_CostSupporterWithBlankPKAndBillingTypeDefault()
		{
			var interactor = new TestInteractor();
			var dummy1 = new DummyAutoRatingConsolObject(Factory);

			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_ParentID = dummy1.PK;
			job1.JH_JobNum = "JOB1";
			job1.JH_ExcludeFromPeriodicRating = false;

			var strategiesRepository = new RatingStrategiesRepository(interactor, null, dummy1, BillingType.Default, AdditionalJobsAction.NoAction, null);
			var strategy = strategiesRepository.Strategies.Single();
			Assert("Strategy should be AutoRateInvoicingStrategy", strategy is AutoRateInvoicingStrategy);
		}

		public void TestGetRatingStrategyFromRatingSupporter_CostSupporterWithNonBlankPKAndBillingTypeDefault()
		{
			var interactor = new TestInteractor();
			var dummy1 = new DummyAutoRatingJobCostingPlugInObject(Factory);

			var job1 = Factory.NewJobForTesting<Job>();
			job1.JH_ParentID = dummy1.PK;
			job1.JH_JobNum = "JOB1";
			job1.JH_ExcludeFromPeriodicRating = false;

			var strategiesRepository = new RatingStrategiesRepository(interactor, null, dummy1, BillingType.Default, AdditionalJobsAction.NoAction, null);
			var strategy = strategiesRepository.Strategies.Single();
			Assert("Strategy should be AutoRateApportionmentStrategy", strategy is AutoRateApportionmentStrategy);
		}

		class DummyAutoRatingPeriodicObject : DummyAutoRatingObject, IPeriodicRatingSupporter
		{
			public DummyAutoRatingPeriodicObject(BusinessObjectFactory factory) : base(factory)
			{
			}
		}

		class GenericJobCostSupporterWithPK : GenericJobCostSupporter
		{
			public GenericJobCostSupporterWithPK(ZGuid pk) : base()
			{
				this.pk = pk;
			}

			public override ZGuid PK => pk;
			readonly ZGuid pk;
		}

		internal class DummyAutoRatingJobCostingPlugInObject : DummyAutoRatingConsolObject
		{
			public DummyAutoRatingJobCostingPlugInObject(BusinessObjectFactory factory) : base(factory)
			{
			}

			public override IGenericJobCostSupporter CostSupporter => new GenericJobCostSupporterWithPK(PK);
		}
	}
}
