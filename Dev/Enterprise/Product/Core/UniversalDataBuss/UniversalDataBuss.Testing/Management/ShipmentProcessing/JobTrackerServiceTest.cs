using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management.ShipmentProcessing;

namespace Enterprise.UniversalDataBuss.Testing.Management.ShipmentProcessing.Testing
{
	public class JobTrackerServiceTest : TestCaseWithFactory
	{
		public void TestAdd()
		{
			using (Factory.AddDisposableService())
			{
				JobTrackerService service = new JobTrackerService(Factory);

				service.Add(Factory.NewJobForTesting<JobHeader>());
				AssertEquals(1, service.JobCount);

				service.Clear();
				AssertEquals(0, service.JobCount);

				service.Add(null);
				AssertEquals(0, service.JobCount);
			}
		}

		public void TestDelete()
		{
			using (Factory.AddDisposableService())
			{
				JobTrackerService service = new JobTrackerService(Factory);

				var job = Factory.NewJobForTesting<JobHeader>();
				service.Add(job);
				service.DeleteNewJobs();

				AssertEquals(true, job.IsDeleted);
			}
		}

		public void TestDontDeleteJobInDB()
		{
			using (Factory.AddDisposableService())
			{
				JobTrackerService service = new JobTrackerService(Factory);

				var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				service.Add(job);
				Factory.Save();
				service.DeleteNewJobs();

				AssertEquals(false, job.IsDeleted);
			}
		}
	}
}
