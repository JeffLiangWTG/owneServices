using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	class FREDIMessageValidationTest : TestCaseWithFactory
	{
		public void TestCheckEM_HeldUntilDateIsValidZDateTimeRange()
		{
			var message = Factory.NewWithValidTestData<FREDIMessage>();
			message.EM_HeldUntilDate = ZDateTime.UtcNow.AddYears(6);
			AssertEquals("Check of hold date range should be deactivated and thus not showing 'is more than 5 years from now and thus is not valid' error message.", false, message.EM_HeldUntilDateInfo.HasNotifications());
		}
	}
}
