using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class NLEDIMessageValidationTest : TestCaseWithFactory
{
	public void TestCheckEM_HeldUntilDateIsValidZDateTimeRange()
	{
		var message = Factory.NewWithValidTestData<NLEDIMessage>();
		message.EM_HeldUntilDate = ZDateTime.MaxSmallDateTime;
		AssertEquals("Check of hold date range should be deactivated and thus not showing 'is more than 5 years from now and thus is not valid' error message.", false, message.EM_HeldUntilDateInfo.HasNotifications());
	}
}
