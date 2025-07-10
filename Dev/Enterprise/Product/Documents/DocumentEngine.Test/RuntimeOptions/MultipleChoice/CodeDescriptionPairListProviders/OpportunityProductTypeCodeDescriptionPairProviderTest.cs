using Enterprise.Registry.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class OpportunityProductTypeCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new OpportunityProductTypeCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			var actualList = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			var expectedList = OrganisationsDataRegistry.Instance.ProductTypeList.Value;

			AssertEquals(expectedList.Count, actualList.Count);
			for (int i = 0; i < expectedList.Count; i++)
			{
				AssertEquals(expectedList[i].Code, actualList[i].Code);
				AssertEquals(expectedList[i].Description, actualList[i].Description);
			}
		}
	}
}
