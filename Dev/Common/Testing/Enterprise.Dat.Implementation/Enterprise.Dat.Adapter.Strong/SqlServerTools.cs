using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Dat.Implementation
{
	public sealed class SqlServerTools
	{
		[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "This assembly has no reference to enterprise libraries")]
		public static void Reconfigure(System.Data.Common.DbConnection connection)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = "reconfigure with override";
				cmd.ExecuteNonQuery();
			}
		}

		public static string SqlServerServiceName
		{
			get
			{
				var serviceName = LocalDBConnection.GetServiceName();
				if (!string.IsNullOrEmpty(serviceName) && serviceName != "MSSQLSERVER")
				{
					return "MSSQL$" + serviceName;
				}
				else
				{
					return "MSSQLSERVER";
				}
			}
		}
	}
}
