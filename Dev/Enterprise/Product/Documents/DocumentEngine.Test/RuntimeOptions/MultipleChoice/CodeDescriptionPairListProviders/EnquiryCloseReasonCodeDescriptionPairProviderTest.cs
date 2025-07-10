using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class EnquiryCloseReasonCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new EnquiryCloseReasonCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			var actualList = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			CodeDescriptionPairList expectedList = SalesEnquiryLookups.CreateEnquiryCloseReasonList();

			AssertEquals(expectedList.Count, actualList.Count);
			for (int i = 0; i < expectedList.Count; i++)
			{
				AssertEquals(expectedList[i].Code, actualList[i].Code);
				AssertEquals(expectedList[i].Description, actualList[i].Description);
			}
		}
	}
}
