using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc.Testing
{
	[TestedType(typeof(PKsByList))]
	class PKsByListTest : DbCreateScriptTest
	{
		public void TestRunFullList()
		{
			string list = @"
			,,,   , , '     '  ,
			F28BE432-47D3-4649-B239-1EDF6D41CC9B				    ,
			'68158C49-1B79-437C-B800-711F61FECA27,
		19699B40-2981-4579-85C0-F6EF9D2258DC',,'',
	'B2782142-D3B8-4CA1-8F98-D643DDA549BB',
			4273FA14-5422-40E1-A60D-6FCBB055ED39    , '' ,	

";
			string expected = @"
F28BE432-47D3-4649-B239-1EDF6D41CC9B
68158C49-1B79-437C-B800-711F61FECA27
19699B40-2981-4579-85C0-F6EF9D2258DC
B2782142-D3B8-4CA1-8F98-D643DDA549BB
4273FA14-5422-40E1-A60D-6FCBB055ED39
";

			AssertMultilineASCIIEquals("", expected, Run(list));
		}

		public void TestRunEmptyList()
		{
			string list = @"
'''	,,, ,',	,
    ,

";
			string expected = @"
";

			AssertMultilineASCIIEquals("", expected, Run(list));
		}

		public void TestRunBroken()
		{
			string list = @"
'''	,,, ,',	,
    ,
The magic smoke has escaped!!!
";
			AssertExceptionThrown("Yay!", typeof(SqlException), delegate { Run(list); });
		}

		#region Implementation

		static string Run(string list)
		{
			const string sql = "select * from PKsByList(@list)";
			const string PK = "PK";

			StringBuilder builder = new StringBuilder();
			builder.AppendLine();

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@list", SqlDbType.VarChar, list);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						Guid pk = (Guid)reader[PK];
						builder.AppendLine(pk.ToString().ToUpper());
					}
				}
			}

			return builder.ToString();
		}
		#endregion
	}
}

