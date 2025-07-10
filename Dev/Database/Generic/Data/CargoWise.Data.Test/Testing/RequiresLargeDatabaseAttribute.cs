using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.IO;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	public sealed class RequiresLargeDatabaseAttribute : TestSetupAttribute
	{
		public RequiresLargeDatabaseAttribute(string databaseNameSuffix = "")
		{
			this.databaseName = Db.DatabaseName + databaseNameSuffix;
		}

		public override void SetUp(TestCase testCase)
		{
			adminConnection = Db.NewAdminConnection("master");
			files = adminConnection.GetDbFiles(databaseName);

			Detach();
			try
			{
				DbConnection connection;
				if (databaseName == Db.DatabaseName)
				{
					connection = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.AuditDatabaseName);
				}
				else
				{
					connection = Db.Connection;
				}

				tempDirectory = new TempDirectory();
				foreach (var file in files)
				{
					using (var cmd = connection.Command("CLRCopyFile"))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.AddParameter("sourceFilePath", SqlDbType.NVarChar, file);
						cmd.AddParameter("destFilePath", SqlDbType.NVarChar, Path.Combine(tempDirectory.DirectoryName, Path.GetFileName(file)));
						cmd.ExecuteNonQuery();
					}
				}

				if (databaseName == Db.DatabaseName)
				{
					connection.Dispose();
				}

				Attach(files.Select(f => Path.Combine(tempDirectory.DirectoryName, Path.GetFileName(f))));
			}
			catch
			{
				Attach(files);
				throw;
			}
		}

		public override void TearDown(TestCase testCase)
		{
			DbCommitTracker.Reset(databaseName);
			if (files != null)
			{
				Detach();
				Attach(files);
				foreach (var file in files)
				{
					using (var cmd = Db.Connection.Command("CLRDeleteFile"))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.AddParameter("@filePath", SqlDbType.NVarChar, Path.Combine(tempDirectory.DirectoryName, Path.GetFileName(file)));
						cmd.AddParameter("@deleteReadOnly", SqlDbType.Bit, true);
						cmd.ExecuteNonQuery();
					}
				}
				tempDirectory.Dispose();
			}
			adminConnection?.Dispose();
		}

		void Attach(IEnumerable<string> files)
		{
			var attachSql = $"create database [{databaseName}] on\r\n"
				+ string.Join(",", files.Select(f => $"(filename = '{f}')")) + "\r\n"
				+ "for attach";
			adminConnection.ExecuteNonQuery(attachSql);
			DataUtils.SetTrustworthyOn(adminConnection, databaseName);
		}

		void Detach()
		{
			adminConnection.ExecuteNonQuery($"alter database [{databaseName}] set single_user with rollback immediate");
			adminConnection.ExecuteNonQuery($"dbo.sp_detach_db @dbname = '{databaseName}'");
		}

		string[] files;
		TempDirectory tempDirectory;
		AdminConnection adminConnection;
		readonly string databaseName;
	}
}
