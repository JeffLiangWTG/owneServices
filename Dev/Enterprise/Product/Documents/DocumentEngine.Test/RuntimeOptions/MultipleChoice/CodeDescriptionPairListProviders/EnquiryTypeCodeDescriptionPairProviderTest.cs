using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class EnquiryTypeCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new EnquiryTypeCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			var actualList = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			var expectedList = SalesEnquiryLookups.GetAllEnquiryTypes();

			AssertEquals(expectedList.Count, actualList.Count);
			for (int i = 0; i < expectedList.Count; i++)
			{
				AssertEquals(expectedList[i].Code, actualList[i].Code);
				AssertEquals(expectedList[i].Description, actualList[i].Description);
			}
		}
	}
}
