using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc
{
	[TestedType(typeof(csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline))]
	class csfn_GetAddInfoMinusKeyAndValuePairFromCodeInlineTest : DbCreateScriptTest
	{
		public void TestGetValueFormAddInfoMinusKeyAndValuePair()
		{
			var sql = @"select AddInfoValue, Value from csfn_GetAddInfoMinusKeyAndValuePairFromCodeInline('BatchLotNumber=201929833*Category=HC¤16*HDRProgramInd=N*IntendedUseCode=HC14*MDEProgramInd=N*VETProgramInd=Y', 'Category')";
			using (var command = CargoWise.Data.Db.Connection.Command(sql))
			using (var reader = command.ExecuteReader())
			{
				reader.Read();
				AssertEquals("BatchLotNumber=201929833*HDRProgramInd=N*IntendedUseCode=HC14*MDEProgramInd=N*VETProgramInd=Y", reader["AddInfoValue"].ToString());
				AssertEquals("HC*16", reader["Value"].ToString());
				AssertEquals(false, reader.Read());
			}
		}
	}
}
