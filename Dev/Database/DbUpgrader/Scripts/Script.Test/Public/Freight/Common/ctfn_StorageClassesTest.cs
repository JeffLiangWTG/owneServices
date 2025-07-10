using System.Data;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Common;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Common
{
	[TestedType(typeof(ctfn_StorageClasses))]
	class ctfn_StorageClassesTest : DbCreateScriptTest
	{
		public void TestRun()
		{
			string sql = "20F, 20R, 20H, 40F, 40R, 40H, 45F, GEN";

			string expected = @"
20F    | 1
20R    | 2
20H    | 3
40F    | 4
40R    | 5
40H    | 6
45F    | 7
GEN    | 8
<null> | 9
<null> | 10
<null> | 11
<null> | 12
<null> | 13
<null> | 14
<null> | 15
";

			//AssertMultilineASCIIEquals("", expected, Run(sql));
			TestCtfnStorageClasses(expected, sql);
		}

		public void TestRunOverflow()
		{
			string sql = "A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z";

			string expected = @"
A      | 1
B      | 2
C      | 3
D      | 4
E      | 5
F      | 6
G      | 7
H      | 8
I      | 9
J      | 10
K      | 11
L      | 12
M      | 13
N      | 14
O      | 15
";

			//AssertMultilineASCIIEquals("", expected, Run(sql));
			TestCtfnStorageClasses(expected, sql);
		}

		virtual public void TestCtfnStorageClasses(string expected, string sql)
		{
			AssertMultilineASCIIEquals("", expected, Run(sql));
		}

		#region Implementation

		static string Run(string str)
		{
			const string sql = "select * from ctfn_StorageClasses(@str)";
			const string RC_StorageClass = "RC_StorageClass";
			const string RC_StorageClassOrdinal = "RC_StorageClassOrdinal";

			StringBuilder builder = new StringBuilder();
			builder.AppendLine();

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@str", SqlDbType.VarChar, str);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						string storage = reader[RC_StorageClass] as string;
						int ordinal = (int)reader[RC_StorageClassOrdinal];

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

