using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc
{
	[TestedType(typeof(GetAddInfoValueFromCode))]
	internal class GetAddInfoValueFromCodeTest : DbCreateScriptTest
	{
		public void TestGetAddInfoValueFromCode()
		{
			const string addInfoStr = "AddInfo1=10*AddInfo2=TestAddInfoString*AddInfo3=abc";
			var valueFromAddInfo = (string)TestConnection.ExecuteScalar($"SELECT dbo.GetAddInfoValueFromCode('{addInfoStr}','AddInfo2')");
			AssertEquals("Get String Value From AddInfo", "TestAddInfoString", valueFromAddInfo);

			var wrongKeyValueFromAddInfo = (string)TestConnection.ExecuteScalar($"SELECT dbo.GetAddInfoValueFromCode('{addInfoStr}','XXX')");
			AssertEquals("Return string.Empty if key not exist", string.Empty, wrongKeyValueFromAddInfo);
		}
	}
}
