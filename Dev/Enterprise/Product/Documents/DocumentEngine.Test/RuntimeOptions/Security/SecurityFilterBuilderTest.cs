namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class SecurityFilterBuilderTest : FilterBuilderTestWithTempFile
	{
		SecurityFilterBuilder builder;

		SecurityFilterBuilder Builder
		{
			get
			{
				if (builder == null)
				{
					builder = new SecurityFilterBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
				}
				return builder;
			}
		}

		public void TestCanBuild()
		{
			AssertEquals("CanBuild()", false, Builder.CanBuild(""));
			AssertEquals("CanBuild()", false, Builder.CanBuild("x"));
			AssertEquals("CanBuild()", true, Builder.CanBuild("securityright"));
			AssertEquals("CanBuild()", true, Builder.CanBuild("SecurityRight"));
		}

		public void TestGetFilterField()
		{
			AssertEquals("NewField().GetType()", typeof(SecurityFilterField), Builder.NewField().GetType());
		}
	}
}
