using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class CostPosterCreatorTest : TestCaseWithFactory
	{
		public void TestCostPosterCreatorForShipment()
		{
			var shipment = Creator.CreateShipment("S001", "AUSYD", "USLAX");

			var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_OA_AgentCollectAddr = Creator.Agent.MainAddress.PK;

			var charge = Creator.CreateCharge(job, Creator.CC3, "Desc", Creator.AUD, 100M, Creator.AALSHI, Creator.AUD, 120M, Creator.Agent);

			job.RunPreSaveValidation();
			AssertNoErrors("Precondition: job shouldn't contain errors.", job);

			Factory.Save();

			var poster = new CostPosterCreator().CreateCostPoster(shipment) as JobPostingWorkflowProcessor;
			AssertNotNull(poster);
			AssertType(typeof(JobCostPoster), poster);
		}

		public void TestCostPosterCreatorForConsol()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupPeriods();

			var consol = Creator.CreateConsol("AUSYD", "USLAX", "C0001");
			var shipment1 = Creator.CreateShipment("S001", consol);
			var shipment2 = Creator.CreateShipment("S002", consol);

			var job1 = new Job.Loader(Factory, shipment1).TryCreateWithoutMutexForTestOnly();
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job1.JH_OA_AgentCollectAddr = Creator.Agent.MainAddress.PK;

			var charge1 = Creator.CreateCharge(job1, Creator.CC3, "Desc", Creator.AUD, 100M, Creator.AALSHI, Creator.AUD, 120M, Creator.Agent);

			job1.RunPreSaveValidation();
			AssertNoErrors("Precondition: job shouldn't contain errors.", job1);

			var job2 = new Job.Loader(Factory, shipment2).TryCreateWithoutMutexForTestOnly();
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job2.JH_OA_AgentCollectAddr = Creator.Agent.MainAddress.PK;

			var charge2 = Creator.CreateCharge(job2, Creator.CC3, "Desc", Creator.AUD, 200M, Creator.AALSHI, Creator.AUD, 240M, Creator.Agent);

			job2.RunPreSaveValidation();
			AssertNoErrors("Precondition: job shouldn't contain errors.", job2);

			Factory.Save();

			var poster = new CostPosterCreator().CreateCostPoster(consol);
			AssertNotNull("Poster must be created", poster);
			AssertType(typeof(ConsolCostPoster), poster);
		}

		TestObjectCreator Creator
		{
			get { return creator_cached ?? (creator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator creator_cached;
	}
}
