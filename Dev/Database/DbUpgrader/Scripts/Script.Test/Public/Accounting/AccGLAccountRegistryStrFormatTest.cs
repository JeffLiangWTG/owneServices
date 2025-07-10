using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(AccGLAccountRegistryStrFormat))]
	class AccGLAccountRegistryStrFormatTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var expectedResults = new[]
			{
				new { format = "1-2-3-4", len = 4, expected = (object)"000000" },
				new { format = "1-2-01-4", len = 3, expected = (object)"0000" },
				new { format = "5-4-10-14", len = 5, expected = (object)"000000" },
				new { format = "7-123-3-14", len = 11, expected = (object)"0" },
				new { format = "5--1-1-", len = 5, expected = (object)"00" },
				new { format = "213411", len = 5, expected = (object)System.DBNull.Value },
				new { format = string.Empty, len = 0, expected = (object)string.Empty },
				new { format = "123", len = -2, expected = (object)"000" },
			};

			using (var cmd = TestConnection.Command("SELECT * FROM [dbo].[AccGLAccountRegistryStrFormat](@NumberFormat,@NumberFormatLen)"))
			{
				cmd.AddParameter("@NumberFormat", SqlDbType.VarChar, 30, string.Empty);
				cmd.AddParameter("@NumberFormatLen", SqlDbType.Int, 0);

				foreach (var r in expectedResults)
				{
					cmd.SetParameterValue("@NumberFormat", r.format);
					cmd.SetParameterValue("@NumberFormatLen", r.len);
					AssertEquals($"(format = {r.format}, len = {r.len})", r.expected, cmd.ExecuteScalar());
				}
			}
		}
	}
}

