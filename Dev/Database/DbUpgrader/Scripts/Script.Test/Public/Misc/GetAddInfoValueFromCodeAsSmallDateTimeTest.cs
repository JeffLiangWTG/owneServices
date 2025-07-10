using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc
{
	[TestedType(typeof(GetAddInfoValueFromCodeAsSmallDateTime))]
	internal class GetAddInfoValueFromCodeAsSmallDateTimeTest : DbCreateScriptTest
	{
		public void TestGetAddInfoValueFromCodeAsSmallDateTime()
		{
			const string addInfoStr = "AddInfo1=10*AddInfo2=2022-06-24 07:40:15.000*AddInfo3=abc";
			var timeFromAddInfo = (DateTime)TestConnection.ExecuteScalar($"SELECT dbo.GetAddInfoValueFromCodeAsSmallDateTime('{addInfoStr}','AddInfo2')");
			AssertEquals("Get DateTime From AddInfo", new DateTime(2022, 06, 24, 07, 40, 00), timeFromAddInfo);

			var lessThanMinTimeFromAddInfo = (DateTime)TestConnection.ExecuteScalar($"SELECT dbo.GetAddInfoValueFromCodeAsSmallDateTime('AddInfo2=1899-12-31 23:59:59','AddInfo2')");
			AssertEquals("Return SmallDateTime.MinDate if addinfo date is before SmallDateTime.MinDate", new DateTime(1900, 01, 01), lessThanMinTimeFromAddInfo);

			var greaterThanMaxTimeFromAddInfo = TestConnection.ExecuteScalar($"SELECT dbo.GetAddInfoValueFromCodeAsSmallDateTime('AddInfo2=2080-01-01 00:00:00','AddInfo2')");
			AssertEquals("Return DBNull.Value if addinfo date is after SmallDateTime.MaxDate", DBNull.Value, greaterThanMaxTimeFromAddInfo);

			var wrongKeyValueFromAddInfo = TestConnection.ExecuteScalar($"SELECT dbo.GetAddInfoValueFromCodeAsSmallDateTime('{addInfoStr}','XXX')");
			AssertEquals("Return DBNull.Value if key not exist", DBNull.Value, wrongKeyValueFromAddInfo);

			var wrongAddInfoValue = TestConnection.ExecuteScalar($"SELECT dbo.GetAddInfoValueFromCodeAsSmallDateTime('AddInfo2=XXXX','AddInfo2')");
			AssertEquals("Return DBNull.Value if value cast failed", DBNull.Value, wrongAddInfoValue);
		}
	}
}
