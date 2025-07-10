using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusUnderbondMessageManagerTest : TestCaseWithFactory
	{
		public void TestAllMessageManagers()
		{
			GlbCompany.GetCurrentCompany(Factory).OrgProxy.Addresses[0].LocalControlledPremisesID = "1234";
			Underbond.C4_DestinationPremiseID = GlbCompany.GetCurrentCompany(Factory).OrgProxy.Addresses[0].LocalControlledPremisesID;
			Underbond.Outturns.AddNew();
			var allMessageManagers = Manager.GetAllMessageManagers();
			AssertEquals("AllMessageManagers.Length", 1, allMessageManagers.Length);
			AssertEquals("AllMessageManagers[0]", typeof(CusUnderbondAIROUTManager), allMessageManagers[0].GetType());
		}

		public void TestAllMessageManagersWithNull()
		{
			var allMessageManagers = new CusUnderbondMessageManagerForTest(null).GetAllMessageManagers();
			AssertEquals("Top level bizo doesn't exist", 0, allMessageManagers.Length);
		}

		public void TestSendWheneverPossibleOnceMessagingActive()
		{
			AssertEquals("SendWheneverPossibleOnceMessagingActive", false, Manager.SendWheneverPossibleOnceMessagingActive);
		}

		CusUnderbondMessageManagerForTest Manager => new CusUnderbondMessageManagerForTest(Underbond);

		CusUnderbond underbond;
		CusUnderbond Underbond => underbond ?? (underbond = Factory.New<CusUnderbond>());

		sealed class CusUnderbondMessageManagerForTest : CusUnderbondMessageManager
		{
			public CusUnderbondMessageManagerForTest(CusUnderbond masterBusinessObject) : base(masterBusinessObject)
			{
			}

			internal new bool SendWheneverPossibleOnceMessagingActive => base.SendWheneverPossibleOnceMessagingActive;

			internal new SingleMessageManager[] GetAllMessageManagers() => base.GetAllMessageManagers();
		}
	}
}
