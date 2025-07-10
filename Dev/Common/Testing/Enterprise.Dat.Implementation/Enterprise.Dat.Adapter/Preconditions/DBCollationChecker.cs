using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Enterprise.Dat.Implementation.Preconditions
{
	[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "This assembly has no reference to enterprise libraries")]
	class DBCollationChecker : IPrecondition
	{
		StringBuilder errorMessageBuilder;
		bool loginFailedExceptionThrown;

		public DBCollationChecker()
		{
		}

		bool CheckCollation()
		{
			bool collationsAreCorrect = false;
			try
			{
				using (var sqlCon = LocalDBConnection.GetConnection())
				{
					sqlCon.Open();
					if (CheckCollation(sqlCon, "model"))
					{
						collationsAreCorrect = CheckCollation(sqlCon, "tempdb");
					}
				}
			}
			catch (LoginFailedException)
			{
				loginFailedExceptionThrown = true;
			}

			return collationsAreCorrect;
		}

		bool CheckCollation(System.Data.Common.DbConnection sqlCon, string dbName)
		{
			string selectCollationCommand = "SELECT DatabasePropertyEx('{0}', 'Collation')";
			string expectedDifferentCollation = "Latin1_General_CI_AS";
			bool collationCorrect = false;

			using (var cmd = sqlCon.CreateCommand())
			{
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, selectCollationCommand, dbName);
				string collation = cmd.ExecuteScalar().ToString();
				collationCorrect = collation == expectedDifferentCollation;
				if (!collationCorrect)
				{
					ShowBadCollationMessage(expectedDifferentCollation, collation, dbName);
				}
			}

			return collationCorrect;
		}

		public bool CheckPreconditionMet()
		{
			errorMessageBuilder = new StringBuilder();
			return CheckCollation();
		}

		void ShowBadCollationMessage(string expectedCollation, string actualCollation, string dbName)
		{
			errorMessageBuilder.AppendFormat("The collation in the {0} database is {1}, but should be {2}.", dbName, actualCollation, expectedCollation);
		}

		#region IPrecondition Members

		string IPrecondition.ErrorMessage
		{
			get
			{
				string result;
				if (loginFailedExceptionThrown)
				{
					result = "Dat.Client was unable to log on to the database. The Database's sa password is not blank.";
				}
				else
				{
					errorMessageBuilder.AppendLine("Please correct the local database collations before running tests.");
					result = errorMessageBuilder.ToString();
				}

				return result;
			}
		}

		#endregion
	}
}
