using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusMAWBBaseBaseOnlyTest : TestCaseWithFactory
	{
		public void TestDischargePortManuallyChanged()
		{
			var mawb = Factory.New<CusMAWB>();
			AssertEquals("AUBNE", mawb.CM_RL_NKDischargePort);
			Assert(!mawb.CM_RL_NKDischargePortInfo.HasChanges);

			Factory.Save();
			mawb.CM_RL_NKDischargePort = "AUSYD";
			Assert(mawb.CM_RL_NKDischargePortInfo.HasChanges);

			mawb.CM_RL_NKDischargePort = "AUBNE";
			Assert(!mawb.CM_RL_NKDischargePortInfo.HasChanges);
		}

		public void TestIsAutoLogged()
		{
			var bizO = Factory.New<CusMAWBForTest>();
			Assert("IsAutoLogged", bizO.IsAutoLogged);
		}

		sealed class CusMAWBForTest : CusMAWB
		{
			public CusMAWBForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			new public bool IsAutoLogged => base.IsAutoLogged;
		}
	}
}
