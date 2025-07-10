using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ValidationHelperTest : TestCaseWithFactory
{
	public void TestValidateDefermentAccountNumberArguments()
	{
		AssertExceptionThrown<ArgumentNullException>("PropertyInfo is required", () => ValidationHelper.ValidateDefermentAccountNumber(null));

		var dummyBizObj = Factory.New<DummyBusinessObject>();
		AssertNoExceptionThrown(() => ValidationHelper.ValidateDefermentAccountNumber((ZPropertyInfoString)dummyBizObj.Z0_DescriptionInfo));
	}
}
