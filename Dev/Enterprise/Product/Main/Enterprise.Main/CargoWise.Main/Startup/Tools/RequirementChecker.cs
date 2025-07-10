using CargoWise.Data;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise
{
	/// <summary>
	/// Check requirements for the Application.
	/// </summary>
	public class RequirementChecker
	{
		public RequirementChecker()
		{
		}

		public void DoCheck()
		{
			CheckForDatabaseCollation();
		}

		#region Always Checked Requirements

		protected void CheckForDatabaseCollation()
		{
			CheckForDatabaseCollation(Db.DatabaseName);
		}

		protected void CheckForDatabaseCollation(string databaseName)
		{
			string sqlText = "SELECT DatabasePropertyEx('" + databaseName + "','Collation')";
			string collation = Db.Connection.ExecuteScalar(sqlText).ToString();
			if (collation != Db.DatabaseCollation)
			{
				string message = (NoResString)"Your " + databaseName + (NoResString)" database does not have the correct Collation (" + Db.DatabaseCollation + (NoResString)"), please restore.";
				Globals.Message.ShowDeveloperErrorOnce("RequirementCheck" + databaseName + Db.DatabaseCollation, message, "Requirement Check");
			}
		}

		#endregion
	}
}
