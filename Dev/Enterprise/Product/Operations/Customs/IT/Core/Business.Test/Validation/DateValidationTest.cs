using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Validation;

namespace Enterprise.Customs.IT.Business.Testing.Validation;

sealed class DateValidationTest : TestCaseWithDummy
{
	public void TestMessageErrorIfDateIsInFuture()
	{
		using (Dummy.SuspendValidationTesting())
		{
			AssertEquals("No Errors", false, Dummy.HasErrors);

			Dummy.Z0_Date = ZDateTime.Today.AddDays(1);
			DateValidation.MessageErrorIfDateIsInFuture(Dummy.Z0_DateInfo);
			AssertHasMessageErrorContaining(Dummy.Z0_DateInfo, "is greater than today's date");

			Dummy.Z0_Date = ZDateTime.Now;
			DateValidation.MessageErrorIfDateIsInFuture(Dummy.Z0_DateInfo);
			AssertNoNotifications(Dummy.Z0_DateInfo);

			Dummy.Z0_Date = ZDateTime.Today.AddDays(-2);
			DateValidation.MessageErrorIfDateIsInFuture(Dummy.Z0_DateInfo);
			AssertNoNotifications(Dummy.Z0_DateInfo);
		}
	}
}
