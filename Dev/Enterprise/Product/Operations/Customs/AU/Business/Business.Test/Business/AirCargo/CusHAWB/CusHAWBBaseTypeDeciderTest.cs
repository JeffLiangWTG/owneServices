using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusHAWBBaseTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad_CTOCusHAWB()
		{
			var mawb = Factory.New<CTOCusMAWB>();
			var ctoHawb = mawb.ChildBills.AddNew();

			AssertEquals("Type", typeof(CTOCusHAWB), Decider.GetTypeForLoad(((INeedRow)ctoHawb).Row, Factory));
		}

		public void TestGetTypeForLoad_CusHAWB()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();

			AssertEquals("Type", typeof(CusHAWB), Decider.GetTypeForLoad(((INeedRow)hawb).Row, Factory));
		}

		public void TestGetTypeForLoad_StandAlone()
		{
			var hawb = Factory.New<CusHAWB>();
			hawb.CS_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			AssertEquals("Type", typeof(CusHAWB), Decider.GetTypeForLoad(((INeedRow)hawb).Row, Factory));
		}

		public void TestGetTypeForLoad_Unknown()
		{
			var mawb = Factory.New<Customs.Business.CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();

			AssertEquals("Type", typeof(Customs.Business.CusHAWB), Decider.GetTypeForLoad(((INeedRow)hawb).Row, Factory));
		}

		public void TestGetTypeForLoad_Undefined()
		{
			var hawb = Factory.New<Customs.Business.CusHAWB>();
			hawb.CS_ApplicationCode = "";

			AssertEquals("Type", typeof(Customs.Business.CusHAWB), Decider.GetTypeForLoad(((INeedRow)hawb).Row, Factory));
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals("Type", null, Decider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			AssertEquals("Type", null, Decider.GetTypeForNew());
		}

		CusHAWBBaseTypeDecider Decider => new CusHAWBBaseTypeDecider();
	}
}
