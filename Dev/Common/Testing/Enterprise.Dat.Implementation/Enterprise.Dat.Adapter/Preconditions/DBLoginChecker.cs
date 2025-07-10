
namespace Enterprise.Dat.Implementation.Preconditions
{
	class DBLoginChecker : IPrecondition
	{
		public DBLoginChecker()
		{
		}

		public string ErrorMessage
		{
			get { return "The password for the Database Server is incorrect."; }
		}

		public bool CheckPreconditionMet()
		{
			bool result = true;
			try
			{
				LocalDBConnection.GetConnection().Dispose();
			}
			catch (LoginFailedException)
			{
				result = false;
			}

			return result;
		}
	}
}
