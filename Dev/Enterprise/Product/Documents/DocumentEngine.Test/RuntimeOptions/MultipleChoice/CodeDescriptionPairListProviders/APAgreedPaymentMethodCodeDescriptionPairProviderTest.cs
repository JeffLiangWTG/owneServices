using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class APAgreedPaymentMethodCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new APAgreedPaymentMethodCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("Undefined", "Undefined");
			result.AddRange(Env.Registry.PayablesCreditAgreedPaymentMethodsList);

			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), result);
		}
	}
}
