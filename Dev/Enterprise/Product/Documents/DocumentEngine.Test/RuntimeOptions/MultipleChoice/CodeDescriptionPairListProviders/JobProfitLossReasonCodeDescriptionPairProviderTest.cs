using CargoWise.Application;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class JobProfitLossReasonCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new JobProfitLossReasonCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), (ReadOnlyCodeDescriptionPairList)ObjectFactory.Get<IAccounting>().JobProfitLossReasonCodeDescriptionPairList);
		}
	}
}
