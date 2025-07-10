using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.CodeDescriptionPairLists.Testing
{
	class CusAuthorizationHeaderTypeListTest : TestCaseWithFactory
	{
		public void TestOTOAndOPOCodes()
		{
			var list = new CusAuthorizationHeaderTypeList();
			AssertContains("OTO", list.CodesAsString);
			AssertContains("TEE", list.CodesAsString);
			AssertEquals("Other Than OPO", list.GetDescriptionFromCode("OTO"));
			AssertEquals("Temporary Exportation", list.GetDescriptionFromCode("TEE"));
		}
	}
}
