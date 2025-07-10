
namespace Enterprise.Dat.Implementation.Preconditions
{
	class SqlServerVersionChecker : IPrecondition
	{
		string errorMessage;

		string IPrecondition.ErrorMessage
		{
			get { return errorMessage; }
		}

		bool IPrecondition.CheckPreconditionMet()
		{
			using (var connection = LocalDBConnection.GetConnection())
			{
				connection.Open();
				return LocalDBConnection.CheckIsLatestRequiredSqlServerVersion(connection, out errorMessage);
			}
		}
	}
}
