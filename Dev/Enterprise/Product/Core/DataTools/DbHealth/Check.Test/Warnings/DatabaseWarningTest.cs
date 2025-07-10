namespace Enterprise.DbHealth.Check
{
	sealed class DatabaseWarningTest : DbHealthWarningTest
	{
		protected override DbHealthWarning GetObject(string source, string warningType, string description, string action)
		{
			return new DatabaseWarning(source, warningType, description, action);
		}

		protected override string DbSouceType
		{
			get { return DatabaseWarning.DatabaseSourceType; }
		}
	}
}
