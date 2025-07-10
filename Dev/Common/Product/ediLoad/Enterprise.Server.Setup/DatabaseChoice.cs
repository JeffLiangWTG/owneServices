using System;

namespace Enterprise.Server.Setup
{
	public class DatabaseChoice
	{
		public DatabaseChoice(string instanceName)
		{
			toString = $"SQL Server instance '{instanceName}'";
			InstanceName = string.Equals(instanceName, "MSSQLSERVER", StringComparison.OrdinalIgnoreCase)
							? string.Empty
							: instanceName;
		}

		public string InstanceName { get; }

		public override string ToString()
		{
			return toString;
		}
		readonly string toString;
	}
}
