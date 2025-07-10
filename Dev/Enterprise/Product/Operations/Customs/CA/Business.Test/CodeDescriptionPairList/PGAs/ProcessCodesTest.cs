using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ProcessCodesTest : TestCaseWithFactory
	{
		public void TestGetProcessCodesFor_CIFA()
		{
			CombineAssertions(() =>
			{
				var result = ProcessCodes.GetProcessCodesFor(PGACodes.Codes.CFIA, Factory);
				AssertEquals("Empty List", ZString.Empty, result.CodesAsString);
				AssertSame("Cached", result, ProcessCodes.GetProcessCodesFor(PGACodes.Codes.CFIA, Factory));
			});
		}

		public void TestGetProcessCodesFor_ECCC()
		{
			var result = ProcessCodes.GetProcessCodesFor(PGACodes.Codes.ECCC, Factory);
			AssertEquals("XE01, XE02, XE03, XE04", result.CodesAsString);
		}
	}
}
