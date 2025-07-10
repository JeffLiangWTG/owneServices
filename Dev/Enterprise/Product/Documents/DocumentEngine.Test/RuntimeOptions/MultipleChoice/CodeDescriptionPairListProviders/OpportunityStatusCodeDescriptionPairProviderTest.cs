using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class OpportunityStatusCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new OpportunityStatusCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			foreach (ICodeDescriptionBool item in OrganisationsDataRegistry.Instance.OpportunityStatus.Value)
			{
				list.Add(item);
			}

			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), list);
		}
	}
}
