using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class EnquiryLeadInterestCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new EnquiryLeadInterestCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			var actualList = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			CodeDescriptionPairList expectedList = OrganisationsDataRegistry.Instance.SalesEnquiryLeadInterests.Value.GetActiveCodeDescriptionPairList();

			AssertEquals(expectedList.Count, actualList.Count);
			for (int i = 0; i < expectedList.Count; i++)
			{
				AssertEquals(expectedList[i].Code, actualList[i].Code);
				AssertEquals(expectedList[i].Description, actualList[i].Description);
			}
		}
	}
}
