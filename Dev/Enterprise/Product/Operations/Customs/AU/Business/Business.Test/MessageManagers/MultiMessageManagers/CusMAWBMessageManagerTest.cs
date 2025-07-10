using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusMAWBMessageManagerTest : TestCaseWithFactory
	{
		public void TestSendOutturnMessage()
		{
			Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();

			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "111";
			hawb.CS_GoodsDescription = "description";
			hawb.CS_PiecesManifested = 3;
			var underbond = mawb.Underbonds.AddNew();
			underbond.C4_DestinationPremiseID = "9914N";
			underbond.C4_OriginPremiseID = "9920A";

			((ICusUnderbondNilUnderbondPerformer)mawb).PerformNilUnderbond(underbond);
			AssertEquals(1, underbond.Outturns.Count);
			var sender = new SendsMessagesToCustomsShutterUpperer();
			var manager = new CusMAWBMessageManager(() => mawb);
			manager.SendOutturnMessage(sender, underbond);
			AssertEquals("Message created", 1, underbond.Messages.Count);
		}

		public void TestAllMessageManagers()
		{
			var allMessageManagers = Manager.GetAllMessageManagers();
			AssertEquals("AllMessageManagers.Length", 1, allMessageManagers.Length);
			AssertEquals("AllMessageManagers[0]", typeof(CusHAWBAIRCRMessageManager), allMessageManagers[0].GetType());
		}

		public void TestAllMessageManagersWithNull()
		{
			var allMessageManagers = new CusMAWBMessageManagerForTest(null).GetAllMessageManagers();
			AssertEquals("AllMessageManagers.Length", 0, allMessageManagers.Length);
		}

		public void TestSendWheneverPossibleOnceMessagingActive()
		{
			AssertEquals("SendWheneverPossibleOnceMessagingActive", false, Manager.SendWheneverPossibleOnceMessagingActive);
		}

		CusMAWBMessageManagerForTest Manager
		{
			get
			{
				var mawb = Factory.New<CusMAWB>();
				var hAWB = mawb.ChildBills.AddNew();
				((ICusUnderbondDependentCollectionParent)hAWB).Underbonds.AddNew(typeof(TestHelperCusUnderbond));
				return new CusMAWBMessageManagerForTest(() => mawb);
			}
		}

		sealed class CusMAWBMessageManagerForTest : CusMAWBMessageManager
		{
			public CusMAWBMessageManagerForTest(GetCusMAWBDelegate getCusMAWBDelegate) : base(getCusMAWBDelegate)
			{
			}

			internal new bool SendWheneverPossibleOnceMessagingActive => base.SendWheneverPossibleOnceMessagingActive;

			internal new SingleMessageManager[] GetAllMessageManagers() => base.GetAllMessageManagers();
		}

		sealed class TestHelperCusUnderbond : CusUnderbond
		{
			public TestHelperCusUnderbond(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool GetCanDoOutturn() => true;
		}
	}
}
