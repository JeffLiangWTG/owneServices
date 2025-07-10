namespace Enterprise.DbHealth.Check
{
	sealed class DiskWarningTest : DbHealthWarningTest
	{
		protected override DbHealthWarning GetObject(string source, string warningType, string description, string action)
		{
			return new DiskWarning(source, warningType, description, action);
		}

		protected override string DbSouceType
		{
			get { return DiskWarning.DiskSourceType; }
		}
	}
}
