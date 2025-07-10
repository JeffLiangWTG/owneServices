using NUnit.Framework;

namespace Enterprise.Environment.Testing
{
	internal static class BaseEnvironmentExtension
	{
		public static void ResetSetupDataAfterTestIfNeeded(this BaseEnvironment baseEnvironment)
		{
			if (baseEnvironment.userContextManager.ContextForReadOnly != baseEnvironment.userContextBeforeTest)
			{
				using (baseEnvironment.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
				{
					baseEnvironment.SetUserContext(baseEnvironment.userContextBeforeTest);
				}
			}

			if (baseEnvironment.userContextManager.CurrentThreadContextIsOverriden)
			{
				baseEnvironment.userContextManager.ClearCurrentThread();
				TestCase.HtmlFail("You must dispose any temporary environments you created in your test");
			}
		}
	}
}
