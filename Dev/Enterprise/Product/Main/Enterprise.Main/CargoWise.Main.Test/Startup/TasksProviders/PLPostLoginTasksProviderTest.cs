namespace Enterprise.Startup.Testing
{
	sealed class PLPostLoginTasksProviderTest : NUnit.Framework.TestCase
	{
		public void TestGetPostLoginTasks()
		{
			var postLoginTasksProvider = new PLPostLoginTasksProvider();
			CombineAssertions(() =>
			{
				AssertEquals("Number of tasks", 1, postLoginTasksProvider.GetPostLoginTasks().Count);
				var postLoginTask = postLoginTasksProvider.GetPostLoginTasks()[0];
				AssertType<StartupCheckPLCertificateExpirationDate>("Certificate expiration check task", postLoginTask);
			});
		}
	}
}
