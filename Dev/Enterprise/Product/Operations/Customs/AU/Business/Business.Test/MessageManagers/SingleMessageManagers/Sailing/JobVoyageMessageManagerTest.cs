using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class JobVoyageMessageManagerTest : TestCaseWithFactory
	{
		public void TestSendWheneverPossibleOnceMessagingActive()
		{
			AssertEquals("SendWheneverPossibleOnceMessagingActive", false, Manager.SendWheneverPossibleOnceMessagingActive);
		}

		public void TestAllMessageManagers()
		{
			Voyage.Destinations.AddNew();

			var allMessageManagers = Manager.GetAllMessageManagers();
			AssertEquals("AllMessageManagers.Length", 2, allMessageManagers.Length);
			AssertEquals("AllMessageManagers[0]", typeof(JobVoyageAIRIARManager), allMessageManagers[0].GetType());
			AssertEquals("AllMessageManagers[1]", typeof(VoyageDestinationAIRAARManager), allMessageManagers[1].GetType());
		}

		public void TestAllowManualAmendments()
		{
			AssertEquals("AllowManualAmendments", true, Manager.AllowManualAmendments);
		}

		JobVoyageMessageManagerForTest manager;
		JobVoyageMessageManagerForTest Manager => manager ?? (manager = new JobVoyageMessageManagerForTest(new CustomsJobVoyageWrapper(Voyage)));

		JobVoyage voyage;
		JobVoyage Voyage => voyage ?? (voyage = Factory.New<JobVoyage>());

		sealed class JobVoyageMessageManagerForTest : JobVoyageMessageManager
		{
			public JobVoyageMessageManagerForTest(CustomsJobVoyageWrapper voyageWrapper) : base(voyageWrapper)
			{
			}

			internal new bool SendWheneverPossibleOnceMessagingActive => base.SendWheneverPossibleOnceMessagingActive;
			internal new SingleMessageManager[] GetAllMessageManagers() => base.GetAllMessageManagers();
		}
	}
}
