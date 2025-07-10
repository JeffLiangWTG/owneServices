using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class OpportunitySourceCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new OpportunitySourceCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			foreach (ICodeDescriptionBool item in OrganisationsDataRegistry.Instance.OpportunitySource.Value)
			{
				list.Add(item);
			}
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), list);
		}
	}
}
