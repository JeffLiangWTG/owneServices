namespace Enterprise.DbHealth.Check
{
	sealed class ServerWarningTest : DbHealthWarningTest
	{
		protected override DbHealthWarning GetObject(string source, string warningType, string description, string action)
		{
			return new ServerWarning(source, warningType, description, action);
		}

		protected override string DbSouceType
		{
			get { return ServerWarning.DiskSourceType; }
		}
	}
}
