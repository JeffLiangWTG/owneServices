using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Startup.Testing
{
	sealed class LoginLocationBusinessObjectForMainFormValidationTestCase : LoginLocationBusinessObjectValidationTestCase
	{
		protected override LoginLocationBusinessObject CreateNewLoginObject()
		{
			return new LoginLocationBusinessObjectForMainForm(Factory);
		}

		protected override void AssertDepartmentCode(LoginLocationBusinessObject loginObj)
		{
			AssertNoErrors(loginObj.DepartmentCodeInfo);
		}
	}
}
