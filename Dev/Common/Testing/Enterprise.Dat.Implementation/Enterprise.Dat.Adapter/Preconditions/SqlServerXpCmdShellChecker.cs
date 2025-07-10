using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Dat.Implementation.Preconditions
{
	[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "This assembly has no reference to enterprise libraries")]
	class SqlServerXpCmdShellChecker : IPrecondition
	{
		static void CheckXpCmdShell()
		{
			using (var connection = LocalDBConnection.GetConnection())
			{
				int currentXp_CmdShellValue;
				connection.Open();

				using (var cmd = connection.CreateCommand())
				{
					cmd.CommandText = "exec sp_configure 'xp_cmdshell'";
					using (var reader = cmd.ExecuteReader())
					{
						reader.Read();
						currentXp_CmdShellValue = (int)reader["run_value"];
					}
				}

				if (currentXp_CmdShellValue < 1)
				{
					using (var cmd = connection.CreateCommand())
					{
						cmd.CommandText = "exec sp_configure 'xp_cmdshell', 1";
						cmd.ExecuteNonQuery();
					}

					SqlServerTools.Reconfigure(connection);
				}
			}
		}

		#region IPrecondition Members

		public bool CheckPreconditionMet()
		{
			CheckXpCmdShell();
			return true;
		}

		string IPrecondition.ErrorMessage
		{
			get { return "SQL Server should have xp_cmdshell option enabled. You need to reconfigure your SQL Server."; }
		}

		#endregion
	}
}
