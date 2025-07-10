using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class IndustryVerticalTypeCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new IndustryVerticalTypeCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), OrganisationsDataRegistry.Instance.IndustryVerticalTypes.Value.GetCodeDescriptionPairList());
		}
	}
}
