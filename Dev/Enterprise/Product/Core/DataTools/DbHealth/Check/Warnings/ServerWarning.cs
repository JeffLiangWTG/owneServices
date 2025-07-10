namespace Enterprise.DbHealth.Check
{
	public class ServerWarning : DbHealthWarning
	{
		public ServerWarning(string source, string warningType, string description, string action)
			: base(source, warningType, description, action)
		{
		}

		public override string SourceType
		{
			get { return DiskSourceType; }
		}

		public const string DiskSourceType = "Server";

		public const string SqlVersionWarning = "SQL Server Version";
		public const string DtcServiceWarning = "Distributed Transaction Coordinator";
		public const string ServerTraceFlagsWarning = "Server Trace Flags";
		public const string InstantFileInitializationWarning = "Instant File Initialization";
		public const string ConfiguredServerAddressWarning = "Server Address";
	}
}
