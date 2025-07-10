using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SupplementaryDeclarantProviderTest : DataProviderTestCase<SupplementaryDeclarantProvider>
	{
		public void TestNewOrNull()
		{
			var supplementaryDeclarant = Factory.NewWithValidTestData<SupplementaryDeclarant>();
			CombineAssertions(() =>
			{
				AssertNull(SupplementaryDeclarantProvider.NewOrNull(null));
				AssertNotNull(SupplementaryDeclarantProvider.NewOrNull(supplementaryDeclarant));
			});
		}

		public void TestIdentifier()
		{
			supplementaryDeclarant.CY_Data = "TestDeclarant";
			AssertEquals("Identifier", "TestDeclarant", Provider.Identifier);
		}

		public void TestType()
		{
			supplementaryDeclarant.CY_Code = "TST";
			AssertEquals("Type", "TST", Provider.Type);
		}

		protected override void SetUp()
		{
			base.SetUp();
			supplementaryDeclarant = Factory.NewWithValidTestData<SupplementaryDeclarant>();
		}
		SupplementaryDeclarant supplementaryDeclarant;

		protected override SupplementaryDeclarantProvider GetProvider()
		{
			return SupplementaryDeclarantProvider.NewOrNull(supplementaryDeclarant);
		}
	}
}
