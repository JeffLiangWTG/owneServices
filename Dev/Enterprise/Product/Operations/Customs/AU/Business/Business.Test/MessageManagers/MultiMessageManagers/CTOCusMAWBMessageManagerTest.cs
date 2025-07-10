using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CTOCusMAWBMessageManagerTest : TestCaseWithFactory
	{
		public void TestSendWheneverPossibleOnceMessagingActive()
		{
			var mawb = Factory.New<CTOCusMAWB>();
			AssertEquals("SendWheneverPossibleOnceMessagingActive", false, new CTOCusMAWBMessageManagerForTest(() => mawb).SendWheneverPossibleOnceMessagingActive);
		}

		public void TestAllMessageManagers()
		{
			var mawb = Factory.New<CTOCusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var partShip = hawb.PartShips.AddNew();
			((ICusUnderbondDependentCollectionParent)hawb).Underbonds.AddNew();
			var allMessageManagers = new CTOCusMAWBMessageManagerForTest(() => mawb).GetAllMessageManagers();
			AssertEquals("AllMessageManagers.Length", 4, allMessageManagers.Length);
			AssertEquals("AllMessageManagers[0]", typeof(CTOCusHAWBAIRCRMessageManager), allMessageManagers[0].GetType());
			AssertEquals("AllMessageManagers[1]", typeof(CusPartShipAIRCRManager), allMessageManagers[1].GetType());
			AssertEquals("AllMessageManagers[2]", typeof(CusUnderbondUBMREQManager), allMessageManagers[2].GetType());
			AssertEquals("AllMessageManagers[3]", typeof(CusUnderbondAIROUTManager), allMessageManagers[3].GetType());
		}

		sealed class CTOCusMAWBMessageManagerForTest : CTOCusMAWBMessageManager
		{
			public CTOCusMAWBMessageManagerForTest(GetCTOCusMAWBDelegate getHAWBDelegate) : base(getHAWBDelegate)
			{
			}

			internal new bool SendWheneverPossibleOnceMessagingActive => base.SendWheneverPossibleOnceMessagingActive;

			internal new SingleMessageManager[] GetAllMessageManagers() => base.GetAllMessageManagers();
		}
	}
}
