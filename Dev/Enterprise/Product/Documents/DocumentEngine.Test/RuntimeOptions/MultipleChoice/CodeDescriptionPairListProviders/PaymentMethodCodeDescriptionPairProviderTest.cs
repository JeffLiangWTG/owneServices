using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class PaymentMethodCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new PaymentMethodCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			var generatedList = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			CodeDescriptionPairList expectedList = new CodeDescriptionPairList(OLookUpEditType.PaymentMethod);
			AssertEquals(expectedList.CodesAsString, generatedList.CodesAsString);
		}
	}
}
