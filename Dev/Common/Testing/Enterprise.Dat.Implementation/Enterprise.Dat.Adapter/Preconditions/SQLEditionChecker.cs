using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Dat.Implementation.Preconditions
{
	[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "This assembly has no reference to enterprise libraries")]
	class SQLEditionChecker : IPrecondition
	{
		public string ErrorMessage
		{
			get { return errorMessage; }
		}

		public bool CheckPreconditionMet()
		{
			using (var connection = LocalDBConnection.GetConnection())
			{
				connection.Open();
				var command = connection.CreateCommand();
				command.CommandText = "SELECT CONVERT(int, SERVERPROPERTY('EngineEdition'))";
				if ((int)command.ExecuteScalar() == 3)  // EnterpriseDeveloper
				{
					return true;
				}
				else
				{
					command = connection.CreateCommand();
					command.CommandText = "SELECT SERVERPROPERTY('Edition')";
					string edition = (string)command.ExecuteScalar();
					errorMessage = "SQL Edition is " + edition + ". DAT Clients must run SQL Enterprise or Developer editions";
					return false;
				}
			}
		}

		string errorMessage = "";
	}
}
