using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	class PBNCustomsStatusListTest : TestCase
	{
		public void TestGetCodeFromEnglishDescription()
		{
			CombineAssertions(() =>
			{
				AssertEquals(PBNCustomsStatusList.Codes.CheckedIn, PBNCustomsStatusList.GetCodeFromEnglishDescription(PBNCustomsStatusList.EnglishDescriptions.CheckedIn));
				AssertEquals(PBNCustomsStatusList.Codes.Incomplete, PBNCustomsStatusList.GetCodeFromEnglishDescription(PBNCustomsStatusList.EnglishDescriptions.Incomplete));
				AssertEquals(PBNCustomsStatusList.Codes.Proceed, PBNCustomsStatusList.GetCodeFromEnglishDescription(PBNCustomsStatusList.EnglishDescriptions.Proceed));
				AssertEquals(PBNCustomsStatusList.Codes.Rejected, PBNCustomsStatusList.GetCodeFromEnglishDescription(PBNCustomsStatusList.EnglishDescriptions.Rejected));
				AssertNull(PBNCustomsStatusList.GetCodeFromEnglishDescription("Invalid"));
			});
		}
	}
}
