using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc
{
	[TestedType(typeof(GetAddInfoValueFromCodeAsBoolean))]
	internal class GetAddInfoValueFromCodeAsBooleanTest : DbCreateScriptTest
	{
		public void TestGetAddInfoValueFromCodeAsBoolean()
		{
			const string addInfoStr = "AddInfo1=10*AddInfo2=Y*AddInfo3=abc";
			var boolFromAddInfo = Convert.ToBoolean(TestConnection.ExecuteScalar($"SELECT dbo.GetAddInfoValueFromCodeAsBoolean('{addInfoStr}','AddInfo2')"));
			AssertEquals("Get Bool From AddInfo", true, boolFromAddInfo);

			var wrongKeyValueFromAddInfo = Convert.ToBoolean(TestConnection.ExecuteScalar($"SELECT dbo.GetAddInfoValueFromCodeAsBoolean('{addInfoStr}','XXX')"));
			AssertEquals("Return false if key not exist", false, wrongKeyValueFromAddInfo);

			var wrongAddInfoValue = Convert.ToBoolean(TestConnection.ExecuteScalar($"SELECT dbo.GetAddInfoValueFromCodeAsBoolean('AddInfo2=XXXX','AddInfo2')"));
			AssertEquals("Return false if value cast failed", false, wrongAddInfoValue);
		}
	}
}
