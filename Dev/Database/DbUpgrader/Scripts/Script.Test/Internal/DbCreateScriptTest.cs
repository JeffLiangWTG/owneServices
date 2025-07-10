using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script
{
	[TestsSubclassesOf(typeof(DbCreateScript))]
	abstract class DbCreateScriptTest : BaseDbScriptTest
	{
		#region Implementation

		protected override string ScriptDbName
		{
			get { return Db.DatabaseName; }
		}

		protected DataRow FirstOrDefault(DataTable source, string columnName, object value) => source.AsEnumerable().Where(x => x[columnName].Equals(value)).FirstOrDefault();

		protected TestDbHelper DbHelper => dbHelper ?? (dbHelper = new TestDbHelper(TestConnection));
		TestDbHelper dbHelper;

		#endregion
	}
}

