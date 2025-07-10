using CargoWise.EntityFramework.Testing;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class UnlocoMappingUtilsTest : TestCaseWithFactory
	{
		public void TestGetPortCodeFromAustralianState()
		{
			string testPortCode = UnlocoMappingUtils.GetPortCodeFromAustralianState("NSW");
			AssertEquals("NSW", "AUSYD", testPortCode);

			testPortCode = UnlocoMappingUtils.GetPortCodeFromAustralianState("WESTERN AUSTRALIA");
			AssertEquals("WESTERN AUSTRALIA", "AUPER", testPortCode);

			testPortCode = UnlocoMappingUtils.GetPortCodeFromAustralianState("TAS");
			AssertEquals("TAS", "AUHBA", testPortCode);

			testPortCode = UnlocoMappingUtils.GetPortCodeFromAustralianState("NonExistingAustralianState");
			AssertEquals("NonExistingAustralianState", "ZZZZZ", testPortCode);

			testPortCode = UnlocoMappingUtils.GetPortCodeFromAustralianState("N.S.W.");
			AssertEquals("N.S.W.", "AUSYD", testPortCode);
			testPortCode = UnlocoMappingUtils.GetPortCodeFromAustralianState("TAS.");
			AssertEquals("TAS.", "AUHBA", testPortCode);
			testPortCode = UnlocoMappingUtils.GetPortCodeFromAustralianState("Vic");
			AssertEquals("Vic", "AUMEL", testPortCode);
		}

		public void TestGetPortCodeFromCountry()
		{
			string testPortCode = UnlocoMappingUtils.GetPortCodeFromCountry("AU");
			AssertEquals("AU", "AUZZZ", testPortCode);

			testPortCode = UnlocoMappingUtils.GetPortCodeFromCountry("NZ123456");
			AssertEquals("NZ123456", "NZZZZ", testPortCode);

			testPortCode = UnlocoMappingUtils.GetPortCodeFromCountry("G");
			AssertEquals("G", "GZZZZ", testPortCode);
		}
	}
}
