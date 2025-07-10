namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class ImportProcessResultTypeCodeListPartialTest : NUnit.Framework.TestCase
	{
		public void TestIsReleasedFromCustomsControl()
		{
			AssertEquals(false, ImportProcessResultTypeCodeList.IsReleasedFromCustomsControl(ImportProcessResultTypeCodeList.Codes._11));
			AssertEquals(false, ImportProcessResultTypeCodeList.IsReleasedFromCustomsControl(ImportProcessResultTypeCodeList.Codes._12));
			AssertEquals(true, ImportProcessResultTypeCodeList.IsReleasedFromCustomsControl(ImportProcessResultTypeCodeList.Codes._13));
			AssertEquals(true, ImportProcessResultTypeCodeList.IsReleasedFromCustomsControl(ImportProcessResultTypeCodeList.Codes._14));
			AssertEquals(true, ImportProcessResultTypeCodeList.IsReleasedFromCustomsControl(ImportProcessResultTypeCodeList.Codes._15));
			AssertEquals(false, ImportProcessResultTypeCodeList.IsReleasedFromCustomsControl(ImportProcessResultTypeCodeList.Codes._16));
			AssertEquals(false, ImportProcessResultTypeCodeList.IsReleasedFromCustomsControl(ImportProcessResultTypeCodeList.Codes._17));
			AssertEquals(false, ImportProcessResultTypeCodeList.IsReleasedFromCustomsControl(ImportProcessResultTypeCodeList.Codes._18));
			AssertEquals(false, ImportProcessResultTypeCodeList.IsReleasedFromCustomsControl(ImportProcessResultTypeCodeList.Codes._19));
			AssertEquals(false, ImportProcessResultTypeCodeList.IsReleasedFromCustomsControl(ImportProcessResultTypeCodeList.Codes._20));
			AssertEquals(false, ImportProcessResultTypeCodeList.IsReleasedFromCustomsControl(ImportProcessResultTypeCodeList.Codes._21));
		}
	}
}
