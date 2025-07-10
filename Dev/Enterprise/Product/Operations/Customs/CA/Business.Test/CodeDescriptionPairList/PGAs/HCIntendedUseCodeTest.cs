namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class HCIntendedUseCodeTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestGetIntendedCodeForProgram_HDR()
		{
			AssertEquals("HC01, HC02, HC05, HC07, HC29, HC31", HCIntendedUseCode.GetIntendedCodeForProgram(HCPGADepartmentCodes.Codes.HDR).CodesAsString);
		}
	}
}
