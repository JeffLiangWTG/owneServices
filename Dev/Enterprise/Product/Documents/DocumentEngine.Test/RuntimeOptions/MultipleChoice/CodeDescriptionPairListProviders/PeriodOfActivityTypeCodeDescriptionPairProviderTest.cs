using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.Registry.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class PeriodOfActivityTypeCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new PeriodOfActivityTypeCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), OrganisationsDataRegistry.Instance.PeriodOfActivityTypes.Value.GetCodeDescriptionPairList());
		}
	}
}
