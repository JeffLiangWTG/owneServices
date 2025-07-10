namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1065
	{
		public void Method()
		{
			//CW1065:Encrypt SQL Connection Rule
			var builder = new SqlConnectionStringBuilder();
			builder.UserID = "userid";
		}
	}
}
