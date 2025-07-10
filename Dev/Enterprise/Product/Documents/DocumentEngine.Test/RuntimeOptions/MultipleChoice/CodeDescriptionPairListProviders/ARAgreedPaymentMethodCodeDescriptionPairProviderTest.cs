using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class ARAgreedPaymentMethodCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new ARAgreedPaymentMethodCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("Undefined", "Undefined");
			result.AddRange(OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value.GetCodeDescriptionPairList());

			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), result);
		}
	}
}
