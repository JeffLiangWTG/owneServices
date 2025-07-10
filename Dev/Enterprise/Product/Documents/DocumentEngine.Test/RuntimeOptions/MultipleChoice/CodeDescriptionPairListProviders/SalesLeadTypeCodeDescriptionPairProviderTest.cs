using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class SalesLeadTypeCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), OrganisationsDataRegistry.Instance.OpportunitySource.Value.GetCodeDescriptionPairList());
		}

		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new SalesLeadTypeCodeDescriptionPairProvider();
		}
	}
}
