using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class LastDbRestoreRegistryItemUpdateRegistryTest : TransactionedTestCase
	{
		public void TestUpdateRegistry()
		{
			var date = new DateTime(2015, 7, 21);

			SystemDataRegistry.Instance.LastDatabaseRestore.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new LastDbRestoreInfo("Restore as is", "1234.0", date, "1232.0", "1233.0"));

			using (var cmd = Db.Connection.Command("SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name = @name"))
			{
				cmd.AddParameterBasedOnDbColumn("@name", "LastDatabaseRestore", StmDataSchema.SD_Name);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						var bytes = reader[StmDataSchema.Constants.SD_BinaryValue];

						if (bytes != DBNull.Value)
						{
							var currentValue = System.Text.Encoding.ASCII.GetString((byte[])bytes).Replace("\0", "");
							var expectedValue = "<?xml version=\"1.0\" encoding=\"utf-16\"?><LastDbRestoreInfo><Operation>Restore as is</Operation><ToolVersion>1234.0</ToolVersion><CompletionDate>21/07/2015 12:00:00 AM</CompletionDate><DbSchemaVersionBefore>1232.0</DbSchemaVersionBefore><DbSchemaVersionAfter>1233.0</DbSchemaVersionAfter></LastDbRestoreInfo>";
							AssertEquals(expectedValue, currentValue);
						}
						else
						{
							Fail("Invalid value for Last Database Restore registry");
						}
					}
					else
					{
						Fail("No entry for Last Database Restore registry in database");
					}
				}
			}
		}
	}
}
