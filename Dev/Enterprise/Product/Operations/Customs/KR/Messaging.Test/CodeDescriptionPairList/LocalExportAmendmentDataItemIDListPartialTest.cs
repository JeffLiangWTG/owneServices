namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class LocalExportAmendmentDataItemIDListPartialTest : NUnit.Framework.TestCase
	{
		public void TestDataItemIDDescription()
		{
			AssertEquals(nameof(ILocalExportEntryHeader), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._11A));
			AssertEquals(nameof(ILocalExportEntryHeader), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._12));
			AssertEquals(nameof(ILocalExportEntryHeader), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._13));
			AssertEquals(nameof(ILocalExportEntryHeader), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._14));
			AssertEquals(nameof(ILocalExportEntryHeader), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._15));
			AssertEquals(nameof(ILocalExportEntryHeader), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._16));
			AssertEquals(nameof(ILocalExportEntryHeader), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._18));
			AssertEquals(nameof(ILocalExportEntryHeader), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._38));
			AssertEquals(nameof(ILocalExportEntryHeader), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._39));

			AssertEquals(nameof(ILocalExportOtherTransportMeans), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._11B));
			AssertEquals(nameof(ILocalExportOtherTransportMeans), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._11C));
			AssertEquals(nameof(ILocalExportOtherTransportMeans), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._11D));

			AssertEquals(nameof(ILocalExportEntryLine), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._21));
			AssertEquals(nameof(ILocalExportEntryLine), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._22));
			AssertEquals(nameof(ILocalExportEntryLine), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._23));
			AssertEquals(nameof(ILocalExportEntryLine), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._24A));
			AssertEquals(nameof(ILocalExportEntryLine), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._24B));
			AssertEquals(nameof(ILocalExportEntryLine), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._25));
			AssertEquals(nameof(ILocalExportEntryLine), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._26));
			AssertEquals(nameof(ILocalExportEntryLine), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._27));
			AssertEquals(nameof(ILocalExportEntryLine), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._28));
			AssertEquals(nameof(ILocalExportEntryLine), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._29));
			AssertEquals(nameof(ILocalExportEntryLine), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._30));
			AssertEquals(nameof(ILocalExportEntryLine), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._31));
			AssertEquals(nameof(ILocalExportEntryLine), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._32A));
			AssertEquals(nameof(ILocalExportEntryLine), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._32B));
			AssertEquals(nameof(ILocalExportEntryLine), LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(LocalExportAmendmentDataItemIDList.Codes._37));
		}
	}
}
