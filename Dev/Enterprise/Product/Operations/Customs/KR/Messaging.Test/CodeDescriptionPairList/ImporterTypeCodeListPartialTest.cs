using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class ImporterTypeCodeListPartialTest : TestCaseWithFactory
	{
		public void TestConvertPaidByCode()
		{
			AssertEquals(ImporterTypeCodeList.Codes.A, ImporterTypeCodeList.ConvertPaidByCode(PaidByCodeList.Codes.CLI));
			AssertEquals(ImporterTypeCodeList.Codes.B, ImporterTypeCodeList.ConvertPaidByCode(PaidByCodeList.Codes.OTH));
		}
	}
}
