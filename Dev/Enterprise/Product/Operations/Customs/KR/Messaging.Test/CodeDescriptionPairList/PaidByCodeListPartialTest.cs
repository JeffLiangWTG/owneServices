using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class PaidByCodeListPartialTest : TestCaseWithFactory
	{
		public void TestConvertImporterTypeCode()
		{
			AssertEquals(PaidByCodeList.Codes.CLI, PaidByCodeList.ConvertImporterTypeCode(ImporterTypeCodeList.Codes.A));
			AssertEquals(PaidByCodeList.Codes.OTH, PaidByCodeList.ConvertImporterTypeCode(ImporterTypeCodeList.Codes.B));
		}
	}
}
