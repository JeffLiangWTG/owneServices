using System.Data;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Common;
using Enterprise.Build.Database.Script.Public.Freight.Common;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.ReportFunctions.Freight.Common.Testing
{
	[TestedType(typeof(ctfn_StorageClasses))]
	class ctfn_StorageClassesEDWTest : ctfn_StorageClassesTest
	{
		override public void TestCtfnStorageClasses(string expected, string sql)
		{
			AssertMultilineASCIIEquals("", expected, RunEDW(sql));
		}

		#region Implementation
		static string RunEDW(string str)
		{
			var sql = $@"select * from {Db.EdwDatabaseName}.dbo.ctfn_StorageClasses(@str)";
			const string RC_StorageClass = "RC_StorageClass";
			const string RC_StorageClassOrdinal = "RC_StorageClassOrdinal";

			var builder = new StringBuilder();
			builder.AppendLine();

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@str", SqlDbType.VarChar, str);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var storage = reader[RC_StorageClass] as string;
						var ordinal = (int)reader[RC_StorageClassOrdinal];

						if (storage == null)
						{
							builder.Append("<null> | ");
						}
						else
						{
							builder.Append(storage);
							builder.Append(' ', 7 - storage.Length);
							builder.Append("| ");
						}

						builder.Append(ordinal);
						builder.AppendLine();
					}
				}
			}

			return builder.ToString();
		}
		#endregion
	}
}

