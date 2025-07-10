using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.FR;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.FR.Testing
{
	[TestedType(typeof(CusFrEntryStatus))]
	class CusFrEntryStatusTest : DbCreateScriptTest
	{
		public void TestGetDescriptionGivenCode()
		{
			void AssertDescriptionGivenCode(string code, string description, bool expectedResult = true)
			{
				var querySql = "SELECT StatusDescription FROM dbo.CusFrEntryStatus WHERE StatusCode = @StatusCode";
				using (var command = Db.Connection.Command(querySql))
				{
					command.AddParameter("@StatusCode", SqlDbType.VarChar, code);

					using (var reader = command.ExecuteReader())
					{
						AssertEquals("At least 1 row is expected", expectedResult, reader.Read());
						if (expectedResult)
						{
							AssertEquals("Description", description, reader.GetString(0));
							AssertEquals("Expected only 1 row", false, reader.Read());
						}
					}
				}
			}

			AssertDescriptionGivenCode(code: "100", description: "BAE");
			AssertDescriptionGivenCode(code: "010", description: "AWAITINGRESPONSE");

			AssertDescriptionGivenCode(code: "XXX", description: null, expectedResult: false);
		}
	}
}

