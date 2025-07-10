namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class LocalExportTransactionNatureCodeListTest : NUnit.Framework.TestCase
	{
		public void TestIs5DP()
		{
			AssertEquals(true, LocalExportTransactionNatureCodeList.Is5DP(LocalExportTransactionNatureCodeList.Codes._01));
			AssertEquals(true, LocalExportTransactionNatureCodeList.Is5DP(LocalExportTransactionNatureCodeList.Codes._02));
			AssertEquals(true, LocalExportTransactionNatureCodeList.Is5DP(LocalExportTransactionNatureCodeList.Codes._03));
			AssertEquals(true, LocalExportTransactionNatureCodeList.Is5DP(LocalExportTransactionNatureCodeList.Codes._04));
			AssertEquals(true, LocalExportTransactionNatureCodeList.Is5DP(LocalExportTransactionNatureCodeList.Codes._06));

			AssertEquals(false, LocalExportTransactionNatureCodeList.Is5DP(LocalExportTransactionNatureCodeList.Codes._07));
			AssertEquals(false, LocalExportTransactionNatureCodeList.Is5DP(LocalExportTransactionNatureCodeList.Codes._08));
			AssertEquals(false, LocalExportTransactionNatureCodeList.Is5DP(LocalExportTransactionNatureCodeList.Codes._09));
			AssertEquals(false, LocalExportTransactionNatureCodeList.Is5DP(LocalExportTransactionNatureCodeList.Codes._17));
			AssertEquals(false, LocalExportTransactionNatureCodeList.Is5DP(LocalExportTransactionNatureCodeList.Codes._18));
		}
		public void TestIs5DQ()
		{
			AssertEquals(false, LocalExportTransactionNatureCodeList.Is5DQ(LocalExportTransactionNatureCodeList.Codes._01));
			AssertEquals(false, LocalExportTransactionNatureCodeList.Is5DQ(LocalExportTransactionNatureCodeList.Codes._02));
			AssertEquals(false, LocalExportTransactionNatureCodeList.Is5DQ(LocalExportTransactionNatureCodeList.Codes._03));
			AssertEquals(false, LocalExportTransactionNatureCodeList.Is5DQ(LocalExportTransactionNatureCodeList.Codes._04));
			AssertEquals(false, LocalExportTransactionNatureCodeList.Is5DQ(LocalExportTransactionNatureCodeList.Codes._06));

			AssertEquals(true, LocalExportTransactionNatureCodeList.Is5DQ(LocalExportTransactionNatureCodeList.Codes._07));
			AssertEquals(true, LocalExportTransactionNatureCodeList.Is5DQ(LocalExportTransactionNatureCodeList.Codes._08));
			AssertEquals(true, LocalExportTransactionNatureCodeList.Is5DQ(LocalExportTransactionNatureCodeList.Codes._09));
			AssertEquals(true, LocalExportTransactionNatureCodeList.Is5DQ(LocalExportTransactionNatureCodeList.Codes._17));
			AssertEquals(true, LocalExportTransactionNatureCodeList.Is5DQ(LocalExportTransactionNatureCodeList.Codes._18));
		}
	}
}
