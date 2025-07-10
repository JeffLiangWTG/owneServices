using System;
using System.Collections;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusHAWBMessageManagerTest : TestCaseWithFactory
	{
		public void TestAllMessageManagers()
		{
			var allMessageManagers = Manager.GetAllMessageManagers();
			AssertEquals("AllMessageManagers.Length", 3, allMessageManagers.Length);

			AssertContains(typeof(CusHAWBAIRCRMessageManager), allMessageManagers);
			AssertContains(typeof(CusUnderbondUBMREQManager), allMessageManagers);
			AssertContains(typeof(CusUnderbondAIROUTManager), allMessageManagers);
		}

		public void TestAllMessageManagersWithNull()
		{
			var allMessageManagers = new CusHAWBMessageManagerForTest(null).GetAllMessageManagers();
			AssertEquals("AllMessageManagers.Length", 0, allMessageManagers.Length);
		}

		public void TestSendWheneverPossibleOnceMessagingActive()
		{
			AssertEquals("SendWheneverPossibleOnceMessagingActive", false, Manager.SendWheneverPossibleOnceMessagingActive);
		}

		void AssertContains(Type expectedType, IEnumerable collection)
		{
			var found = false;
			foreach (object element in collection)
			{
				if (element.GetType() == expectedType)
				{
					found = true;
					break;
				}
			}

			Assert(found);
		}

		CusHAWBMessageManagerForTest Manager
		{
			get
			{
				var mawb = Factory.New<CusMAWB>();
				var hawb = mawb.ChildBills.AddNew();
				var underbond = (CusUnderbond)((ICusUnderbondDependentCollectionParent)hawb).Underbonds.AddNew();
				underbond.C4_DestinationPremiseID = GlbCompany.GetCurrentCompany(Factory).OrgProxy.MainAddress.LocalControlledPremisesID = "9914N";
				hawb.AllUnderbonds.Load();
				return new CusHAWBMessageManagerForTest(() => hawb);
			}
		}

		sealed class CusHAWBMessageManagerForTest : CusHAWBMessageManager
		{
			public CusHAWBMessageManagerForTest(GetCusHAWBDelegate getHAWBDelegate) : base(getHAWBDelegate)
			{
			}

			internal new bool SendWheneverPossibleOnceMessagingActive => base.SendWheneverPossibleOnceMessagingActive;

			internal new SingleMessageManager[] GetAllMessageManagers() => base.GetAllMessageManagers();
		}
	}
}
