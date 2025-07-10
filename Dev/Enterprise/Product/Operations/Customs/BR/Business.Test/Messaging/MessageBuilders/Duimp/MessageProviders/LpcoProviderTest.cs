using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class LpcoProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(LpcoProvider.New(null));
			AssertType<LpcoProvider>(LpcoProvider.New(Factory.New<Permit>()));
		}

		public void TestNumber()
		{
			var permit = Factory.New<Permit>();
			permit.CSI_ReferenceNumber = "123";
			var dataProvider = LpcoProvider.New(permit);
			AssertEquals("123", dataProvider.Number);
		}
	}
}
