using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JCSWatermarkUpdaterTest : TestCaseWithFactory
	{
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestUpdate()
		{
			var companies = Helper.CreateCompany(1);
			Helper.CreateBranch(companies);

			Helper.SetRegistryValue(companies[0].PK.ToGuid(),
								testObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "ALL", "ARV", 10, false, false));

			var jobs = Helper.CreateJobs(4, companies);
			jobs[0].JH_SystemCreateTimeUtc = new ZDateTime(2020, 08, 01);
			jobs[1].JH_SystemCreateTimeUtc = new ZDateTime(2020, 08, 02);
			jobs[2].JH_SystemCreateTimeUtc = new ZDateTime(2020, 08, 03);
			jobs[3].JH_SystemCreateTimeUtc = new ZDateTime(2020, 08, 04);
			Factory.Save();

			var populator = new JCSQueuePopulator();
			populator.Populate(new DateTime(2020, 1, 1), new ZGuid[] { companies[0].PK });

			AssertEquals("Watermark Date", DateTime.MinValue, AccountingConfigurationRegistry.Instance.AutoJobClosureProcessingWatermark.Value);
			var updater = new JCSWatermarkUpdater();
			updater.Update();
			AssertEquals("Watermark Date", new ZDateTime(2020, 08, 04), AccountingConfigurationRegistry.Instance.AutoJobClosureProcessingWatermark.Value);
			Assert(updater.CanMoveForwardWatermark);

			TestConnection.ExecuteNonQuery("TRUNCATE TABLE JobToCloseQueue");
			updater = new JCSWatermarkUpdater();
			updater.Update();
			AssertEquals("Watermark Date", new DateTime(1900, 1, 1), AccountingConfigurationRegistry.Instance.AutoJobClosureProcessingWatermark.Value);
			AssertEquals("LastUTCDateTimeOfReachingTheHighestJCSWatermark", ZDateTime.UtcNow.Date, AccountingConfigurationRegistry.Instance.LastUTCDateTimeOfReachingTheHighestJCSWatermark.Value.Date);
			Assert(!updater.CanMoveForwardWatermark);
		}

		JCSTestHelper Helper
		{
			get { return helper ?? (helper = new JCSTestHelper(TestObjectCreator)); }
		}
		JCSTestHelper helper;

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
