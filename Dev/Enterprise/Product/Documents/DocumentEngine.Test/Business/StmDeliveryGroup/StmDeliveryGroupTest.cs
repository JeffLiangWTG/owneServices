using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestedType(typeof(StmDeliveryGroup))]
	sealed class StmDeliveryGroupTest : EnterpriseBusinessObjectTestCase
	{
		public void TestContainedPrintJobs()
		{
			var group = Factory.New<StmDeliveryGroup>();
			AssertEquals(0, group.ContainedPrintJobs.Count);

			var job = Factory.New<StmPrintJob>();
			job.SP_SB_DeliveryGroup = group.PK;
			AssertEquals(1, group.ContainedPrintJobs.Count);

			job.SP_SB_DeliveryGroup = ZGuid.Empty;
			AssertEquals(0, group.ContainedPrintJobs.Count);
		}

		public void TestSB_IsZippedDocPack_DefaultValue()
		{
			var group = Factory.New<StmDeliveryGroup>();
			Assert("Should be False by default", !group.SB_IsZippedDocPack);
		}

		public void TestPrintJobs()
		{
			var group = Factory.New<StmDeliveryGroup>();
			AssertNotNull("PrintJobs property should not be null", group.PrintJobs);
			AssertEquals("PrintJobs property is empty by default - you need to specifically add/remove print jobs", 0, group.PrintJobs.Count);
		}

		[TestDate(2018, 5, 30, 14, 46, 16)]
		public void TestSystemCreateTimeUtc()
		{
			var group = Factory.New<StmDeliveryGroup>();
			AssertEquals(ZDateTime.UtcNow, group.SB_SystemCreateTimeUtc);
		}

		public void TestDescriptionCanBe256CharactersLong()
		{
			var group = Factory.New<StmDeliveryGroup>();
			AssertNoExceptionThrown(() => { group.SB_EmailSubjectLine = new string('x', 256); });
		}
	}
}
