namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class HCCategoriesTest : NUnit.Framework.TestCase
	{
		public void TestGetCategoryForProgram_HDR_HC31()
		{
			var list = HCCategories.GetCategoryForProgram(HCPGADepartmentCodes.Codes.HDR, HCIntendedUseCode.Codes.HC31);
			AssertEquals(2, list.Count);
			AssertEquals(HCCategories.Descriptions.HC05, list.GetDescriptionFromCode(HCCategories.Codes.HC05));
			AssertEquals(HCCategories.Descriptions.HC06, list.GetDescriptionFromCode(HCCategories.Codes.HC06));
		}

		public void TestGetCategoryForProgram_RED()
		{
			var list = HCCategories.GetCategoryForProgram(HCPGADepartmentCodes.Codes.RED, string.Empty);
			AssertEquals(6, list.Count);
			AssertEquals(HCCategories.Descriptions.HC40, list.GetDescriptionFromCode(HCCategories.Codes.HC40));
			AssertEquals(HCCategories.Descriptions.HC41, list.GetDescriptionFromCode(HCCategories.Codes.HC41));
			AssertEquals(HCCategories.Descriptions.HC42, list.GetDescriptionFromCode(HCCategories.Codes.HC42));
			AssertEquals(HCCategories.Descriptions.HC43, list.GetDescriptionFromCode(HCCategories.Codes.HC43));
			AssertEquals(HCCategories.Descriptions.HC44, list.GetDescriptionFromCode(HCCategories.Codes.HC44));
			AssertEquals(HCCategories.Descriptions.HC45, list.GetDescriptionFromCode(HCCategories.Codes.HC45));
		}
	}
}
