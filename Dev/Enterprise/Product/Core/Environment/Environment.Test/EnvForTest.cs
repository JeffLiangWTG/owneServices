namespace Enterprise.Environment.Testing
{
	public static class EnvForTest
	{
		public static void ResetSetupDataAfterTestIfNeeded()
		{
			var providerAfterTest = Env.Provider;
			if (providerAfterTest != Env.providerBeforeTest)
			{
				Env.providerBeforeTest.Enable();
				providerAfterTest.Dispose();
			}
			Env.Instance.ResetSetupDataAfterTestIfNeeded();
		}
	}
}
