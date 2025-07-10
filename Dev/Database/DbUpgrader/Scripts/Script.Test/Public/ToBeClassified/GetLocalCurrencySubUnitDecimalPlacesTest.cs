using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified
{
	[TestedType(typeof(GetLocalCurrencySubUnitDecimalPlaces))]
	class GetLocalCurrencySubUnitDecimalPlacesTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestSampleCallWithException()
		{
			var result = (int)TestConnection.ExecuteScalar("SELECT LocalCurrencyDecimalPlaces FROM dbo.GetLocalCurrencySubUnitDecimalPlaces('03052ED3-2C64-49AC-97D8-C6079D5015B5')");
			AssertEquals(2, result);

			result = (int)TestConnection.ExecuteScalar("SELECT LocalCurrencySubUnitRatio FROM dbo.GetLocalCurrencySubUnitDecimalPlaces('03052ED3-2C64-49AC-97D8-C6079D5015B5')");
			AssertEquals(100, result);
		}
	}
}

