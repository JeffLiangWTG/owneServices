using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusMAWBBaseTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad_CTOCusMAWB()
		{
			AssertEquals("Type", typeof(CTOCusMAWB), Decider.GetTypeForLoad(((INeedRow)Factory.New<CTOCusMAWB>()).Row, Factory));
		}

		public void TestGetTypeForLoad_CusMAWB()
		{
			AssertEquals("Type", typeof(CusMAWB), Decider.GetTypeForLoad(((INeedRow)Factory.New<CusMAWB>()).Row, Factory));
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals("Type", null, Decider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			AssertEquals("Type", null, Decider.GetTypeForNew());
		}

		CusMAWBBaseTypeDecider Decider => new CusMAWBBaseTypeDecider();
	}
}
