namespace Enterprise.DbHealth.Check
{
	public class DiskWarning : DbHealthWarning
	{
		public DiskWarning(string source, string warningType, string description, string action)
			: base(source, warningType, description, action)
		{
		}

		public override string SourceType
		{
			get { return DiskSourceType; }
		}

		public const string DiskSourceType = "Disk";
		public const string DiskSpaceWarning = "Disk Space";
	}
}
