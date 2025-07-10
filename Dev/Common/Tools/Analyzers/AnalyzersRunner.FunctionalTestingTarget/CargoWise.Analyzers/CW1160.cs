namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1160
	{
		public void BadCode()
		{
			// CW1160 Sql objects must be fully qualified with the schema
			_ = "SELECT * FROM Users;";
			_ = "SELECT * FROM Users LEFT JOIN Orders ON Users.UserId = Orders.UserId;";
		}
	}
}
