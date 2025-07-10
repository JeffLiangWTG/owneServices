using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ETLineSpecialMentionInfoAdditionalInformationWrapperTest : TestCaseWithFactory
{
	public void TestETLineSpecialMentionInfoAdditionalInformationWrapper()
	{
		var etLineSpecialMentionInfoAdditionalInformationWrapper = new ETLineSpecialMentionInfoAdditionalInformationWrapper();
		CombineAssertions("Check ETLineSpecialMentionInfoAdditionalInformationWrapper properties must be empty", () =>
		{
			AssertEquals("AdditionalInformation", "", etLineSpecialMentionInfoAdditionalInformationWrapper.AdditionalInformation);
			AssertEquals("AdditionalInformationCoded", "", etLineSpecialMentionInfoAdditionalInformationWrapper.AdditionalInformationCoded);
			AssertEquals("IsExportFromCE", null, etLineSpecialMentionInfoAdditionalInformationWrapper.IsExportFromCE);
			AssertEquals("ExportCountry", "", etLineSpecialMentionInfoAdditionalInformationWrapper.ExportCountry);
		});
	}
}
