using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class IncotermsPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new IncotermsPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			CodeDescriptionPairList incotermsList = new IncoTermsCodeDescriptionPairList();
			incotermsList.AddRange(new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms));

			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), incotermsList);
		}
	}
}
