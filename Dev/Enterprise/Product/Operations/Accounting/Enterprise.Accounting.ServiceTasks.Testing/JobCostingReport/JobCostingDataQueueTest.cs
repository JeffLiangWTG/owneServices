using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Accounting.ServiceTasks.Testing.JobCostingReport
{
	class JobCostingDataQueueTest : TestCaseWithFactory
	{
		string InsertQuery(string postDate, string reverseDate)
		{
			return $@"INSERT INTO JobCostingDataQueue (
						JCQ_ALPK,
						JCQ_GCPK,
						JCQ_PostDate,
						JCQ_ReverseDate)
					VALUES (
						NEWID(), 
						NEWID(), 
						{postDate},
						{reverseDate})
					;";
		}

		public void TestQueueResultWithPostDate()
		{
			var itemDate = new DateTime(2020, 2, 20);
			Db.Connection.ExecuteNonQuery(InsertQuery($"'{itemDate.ToString("s")}'", "NULL"));

			IHostedServiceQueueProvider provider = new JobCostingDataQueue();
			AssertEquals(1, provider.QueueResult.QueueSize);
			var expectedAge = DateTime.UtcNow - itemDate;
			NUnit.Framework.Assert.That((int)provider.QueueResult.MaximumItemAge.TotalSeconds, NUnit.Framework.Is.EqualTo((int)expectedAge.TotalSeconds).Within(60));
		}

		//JCQ_PostDate and JCQ_ReverseDate are populated from AL_PostDate and AL_ReverseDate respectively. These are NOT utc dates.
		//Therefore a datediff in sql with getutcdate() as the endtime can result in negative value, if JCQ_ReverseDate or JCQ_PostDate is ahead in local time.
		public void TestQueueResultWithFuturePostDateInLocalTime()
		{
			var ausyd = Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, "AUSYD") as RefUNLOCO;
			var itemDate = ((ZDateTime)DateTime.UtcNow).ToLocationTime(ausyd).ToDateTime();

			Db.Connection.ExecuteNonQuery(InsertQuery($"'{itemDate.ToString("s")}'", "NULL"));

			IHostedServiceQueueProvider provider = new JobCostingDataQueue();
			AssertEquals(1, provider.QueueResult.QueueSize);
			var expectedAge = itemDate - DateTime.UtcNow;
			NUnit.Framework.Assert.That((int)provider.QueueResult.MaximumItemAge.TotalSeconds, NUnit.Framework.Is.EqualTo((int)expectedAge.TotalSeconds).Within(60));
		}

		public void TestQueueResultWithReverseDate()
		{
			var itemDate = new DateTime(2020, 2, 20);
			Db.Connection.ExecuteNonQuery(InsertQuery("NULL", $"'{itemDate.ToString("s")}'"));

			IHostedServiceQueueProvider provider = new JobCostingDataQueue();
			AssertEquals(1, provider.QueueResult.QueueSize);
			var expectedAge = DateTime.UtcNow - itemDate;
			NUnit.Framework.Assert.That((int)provider.QueueResult.MaximumItemAge.TotalSeconds, NUnit.Framework.Is.EqualTo((int)expectedAge.TotalSeconds).Within(60));
		}

		public void TestQueueResultWithNulls()
		{
			Db.Connection.ExecuteNonQuery($@"delete from JobCostingDataQueue;");

			IHostedServiceQueueProvider provider = new JobCostingDataQueue();
			AssertEquals(0, provider.QueueResult.QueueSize);
			AssertEquals(0, (int)provider.QueueResult.MaximumItemAge.TotalSeconds);
		}

		public void TestIsJCDInitialized()
		{
			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "NON");
			AssertEquals(false, string.IsNullOrEmpty(JobCostingDataPopulationServiceTask.IsJCDInitialized()));

			AccountingConfigurationRegistry.Instance.JCDServiceTaskController.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "STR");
			AssertEquals(true, string.IsNullOrEmpty(JobCostingDataPopulationServiceTask.IsJCDInitialized()));
		}
	}
}
