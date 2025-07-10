namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class RequirementTypeCodeListTest : NUnit.Framework.TestCase
	{
		public void TestNonGAApprovals()
		{
			AssertEquals(false, RequirementTypeCodeList.IsGADeclarationRequired(RequirementTypeCodeList.Codes._1));
			AssertEquals(false, RequirementTypeCodeList.IsGADeclarationRequired(RequirementTypeCodeList.Codes._2));
			AssertEquals(true, RequirementTypeCodeList.IsGADeclarationRequired(RequirementTypeCodeList.Codes._3));
			AssertEquals(false, RequirementTypeCodeList.IsGADeclarationRequired(RequirementTypeCodeList.Codes._9));
		}
	}
}
