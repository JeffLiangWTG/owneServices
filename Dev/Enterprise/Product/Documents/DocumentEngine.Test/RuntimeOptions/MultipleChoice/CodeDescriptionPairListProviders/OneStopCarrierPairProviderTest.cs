using CargoWise.Application;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class OneStopCarrierPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new OneStopCarrierPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			CodeDescriptionPairList oneStopCarrierTestList = new CodeDescriptionPairList();
			oneStopCarrierTestList.AddRange(ObjectFactory.Get<Enterprise.Freight.Integration.IOneStopCarrierCodePairListProvider>().GetOneStopCarrierCodePairListForFilter());

			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), oneStopCarrierTestList);
		}
	}
}
