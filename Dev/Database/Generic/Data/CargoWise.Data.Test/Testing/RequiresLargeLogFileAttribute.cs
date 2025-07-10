using System;
using System.Globalization;
using System.IO;
using CargoWise.IO;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
	public sealed class RequiresLargeLogFileAttribute : UseSnapshotProtectionAttribute
	{
		public RequiresLargeLogFileAttribute(int initialSize_MB = 512, int growth_MB = 512, int maxSize_MB = 4096, string databaseNameSuffix = "")
			: base(databaseNameSuffix)
		{
			this.initialSize_MB = initialSize_MB;
			this.growth_MB = growth_MB;
			this.maxSize_MB = maxSize_MB;
			this.databaseNameSuffix = databaseNameSuffix;
		}

		readonly int initialSize_MB;
		readonly int growth_MB;
		readonly int maxSize_MB;
		readonly string databaseNameSuffix;
		TempDirectory tempLogDirectory;

		public override void SetUp(TestCase testCase)
		{
			base.SetUp(testCase);
			this.tempLogDirectory = new TempDirectory();
			SetLogGrowing(Db.DatabaseName + databaseNameSuffix, 0);
			fullFileName = AddLogFile(Db.DatabaseName + databaseNameSuffix, initialSize_MB, growth_MB, maxSize_MB);
		}
		string fullFileName;

		public override void TearDown(TestCase testCase)
		{
			base.TearDown(testCase);
			RemoveTempFile(fullFileName);

			if (tempLogDirectory != null)
			{
				tempLogDirectory.Dispose();
			}
		}

		void RemoveTempFile(string fileName)
		{
			using (var connection = Db.NewAdminConnection())
			{
				using (var cmd = connection.Command("CLRDeleteFile"))
				{
					cmd.CommandType = System.Data.CommandType.StoredProcedure;
					cmd.AddParameter("@filePath", System.Data.SqlDbType.NVarChar, fileName);
					cmd.AddParameter("@deleteReadOnly", System.Data.SqlDbType.Bit, true);
					cmd.ExecuteNonQuery();
				}
			}
		}
		#region Implementation

		void SetLogGrowing(string dbName, int growth_MB)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"
DECLARE @stmt nvarchar(max);

SELECT TOP(1)
	@stmt = 'ALTER DATABASE [' + d.name + '] MODIFY FILE ( NAME = N''' + f.name + ''', FILEGROWTH = {1} MB );'
FROM
	sys.master_files   AS f
	JOIN sys.databases AS d ON d.database_id = f.database_id
WHERE
	d.name = N'{0}'
	AND f.type = 1 -- LOG
ORDER BY
	file_id

if (@stmt <> '') EXEC(@stmt);
"
				, dbName
				, growth_MB
				);

			using (var connection = Db.NewAdminConnection())
			using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
			{
				connection.ExecuteNonQuery(sql);
			}
		}

		string AddLogFile(string dbName, int initialSize_MB, int growth_MB, int maxSize_MB)
		{
			var additionalLogFileName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_AdditionalLog", dbName, Guid.NewGuid());
			var fullFileName = Path.Combine(tempLogDirectory.DirectoryName, additionalLogFileName + ".ldf");

			using (var connection = Db.NewAdminConnection())
			using (((ICurrentDbControl)connection).UseDatabase(Db.SqlMasterDb))
			{
				var sql = string.Format(CultureInfo.InvariantCulture, @"
ALTER DATABASE [{0}] ADD LOG FILE
	(
		NAME       = N'{1}',
		FILENAME   = N'{2}',
		SIZE       = {3} MB,
		FILEGROWTH = {4} MB,
		MAXSIZE    = {5} MB
	)
"
					, dbName                // 0
					, additionalLogFileName // 1
					, fullFileName          // 2
					, initialSize_MB        // 3
					, growth_MB             // 4
					, maxSize_MB            // 5
					);

				connection.ExecuteNonQuery(sql);
			}
			return fullFileName;
		}

		#endregion // Implementation
	}
}
