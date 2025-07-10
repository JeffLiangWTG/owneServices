using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class TypeOfDutyProviderTest : DataProviderTestCase<TypeOfDutyProvider>
	{
		public void TestUnionCode()
		{
			AssertNull("UnionCode", Provider.UnionCode);
		}

		public void TestNationalCode()
		{
			AssertNull("NationalCode", Provider.NationalCode);
		}

		protected sealed override TypeOfDutyProvider GetProvider()
		{
			return new TypeOfDutyProvider();
		}
	}
}

