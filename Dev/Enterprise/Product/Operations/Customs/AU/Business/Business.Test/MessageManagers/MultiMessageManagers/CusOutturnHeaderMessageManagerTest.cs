using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusOutturnHeaderMessageManagerTest : TestCaseWithFactory
	{
		public void TestGetAllMessageManagers()
		{
			var header = CusOutturnHeader.New(Factory);
			var manager = new CusOutturnHeaderMessageManagerForTest(header);
			var singleManagers = manager.GetAllMessageManagers();
			AssertEquals("length", 1, singleManagers.Length);
			AssertEquals("only one is of the right type", typeof(CusUnderbondSEAOUTManager), singleManagers[0].GetType());
		}

		class CusOutturnHeaderMessageManagerForTest : CusOutturnHeaderMessageManager
		{
			public CusOutturnHeaderMessageManagerForTest(CusOutturnHeader header) : base(header)
			{
			}

			internal new SingleMessageManager[] GetAllMessageManagers() => base.GetAllMessageManagers();
		}
	}
}
