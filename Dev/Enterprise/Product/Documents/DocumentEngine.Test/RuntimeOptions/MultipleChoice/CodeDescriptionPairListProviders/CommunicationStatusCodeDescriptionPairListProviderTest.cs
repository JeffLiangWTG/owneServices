using Enterprise.Registry.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class CommunicationStatusCodeDescriptionPairListProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new CommunicationStatusCodeDescriptionPairListProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), OrganisationsDataRegistry.Instance.CommunicationStatusList.Value.GetCodeDescriptionPairList());
		}
	}
}
