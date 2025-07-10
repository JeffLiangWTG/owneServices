using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	class PackageValidationTest : CusDecHouseContainerPackValidationTest
	{
		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			return declaration;
		}

		JobDeclaration Declaration => declaration ?? (declaration = (JobDeclaration)GetJobDeclaration());
		JobDeclaration declaration;

		public void TestCheckCW_MarksAndNos()
		{
			var message = "This field is recommended to be no more than 42 characters long.";
			var package = Declaration.Packages.AddNew();
			package.CW_MarksAndNos = "Test01fhskdhfs2412sfdsfs36912831219286398216498";
			AssertHasWarning("When the length of Package.CW_MarksAndNos greater than 42.", package.CW_MarksAndNosInfo, message);
			package.CW_MarksAndNos = "Test12345";
			AssertNoWarning("When the length of Package.CW_MarksAndNos less than 42.", package.CW_MarksAndNosInfo, message);
		}
	}
}
