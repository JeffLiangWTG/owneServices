using CargoWise.Application;
using NUnit.Framework;

namespace Enterprise.MailManager.MailFilters.Testing
{
	public static class MailFilterLocatorTestHelper
	{
		public static void SetApplication(IMailItem mail, string code, bool shouldSucceed = true)
		{
			var locator = ObjectFactory.Get<IMailFilterProvider>();

			Assertion.Assert("Code does not exist", locator.TryGetFilter(code, out var filter));
			var success = filter.CanProcess(mail);
			if (success)
			{
				mail.MI_Application = code;
			}
			else if (shouldSucceed)
			{
				Assertion.Assert("Mail does not match filter", filter.CanProcess(mail));
			}
		}
	}
}
