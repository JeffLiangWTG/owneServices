using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using static Enterprise.Customs.CH.NCTS.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NctsDataProviderHelperTest : TestCaseWithFactory
{
	public void TestToOptionalStateOfSeals() => CombineAssertions(() =>
	{
		AssertEquals("Y", true, NctsDataProviderHelper.ToOptionalSealIsValid(YesNoList.Codes.Yes));
		AssertEquals("N", false, NctsDataProviderHelper.ToOptionalSealIsValid(YesNoList.Codes.No));
		AssertNull("empty", NctsDataProviderHelper.ToOptionalSealIsValid(ZString.Empty));
		AssertNull("unknown", NctsDataProviderHelper.ToOptionalSealIsValid("X"));
	});

	public void TestIsSendDestinationCountryAtConsignmentLevelV4()
	{
		var noCountry = ZString.Empty;
		var country1 = new ZString("C1");
		var country2 = new ZString("C2");

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		var bill1 = nctsHeader.Bills.AddNew();
		var item11 = bill1.GoodsItems.AddNew();
		var item12 = bill1.GoodsItems.AddNew();
		var bill2 = nctsHeader.Bills.AddNew();
		var item21 = bill2.GoodsItems.AddNew();

		var previousDocument = nctsHeader.PreviousDocuments.AddNew();

		AssertResult(true, country1, noCountry, noCountry, noCountry, noCountry);
		AssertResult(true, noCountry, country1, noCountry, noCountry, country1);
		AssertResult(true, noCountry, noCountry, country1, country1, country1);
		AssertResult(false, country1, country2, noCountry, noCountry, noCountry);
		AssertResult(false, country1, noCountry, country2, noCountry, noCountry);
		AssertResult(false, country1, noCountry, noCountry, noCountry, noCountry, isLinkedExport: true);

		void AssertResult(bool expectedResult, ZString headerCountry, ZString bill1Country, ZString item11Country, ZString item12Country, ZString item21Country, bool isLinkedExport = false)
		{
			nctsHeader.MovementHeader.BM_RL_NKDestinationPort = headerCountry;
			bill1.B0_RN_NKCountryOfDestination = bill1Country;
			item11.BY_RN_NKCountryOfDestination = item11Country;
			item12.BY_RN_NKCountryOfDestination = item12Country;
			item21.BY_RN_NKCountryOfDestination = item21Country;
			previousDocument.CSI_Code = isLinkedExport ? PreviousDocumentCodes.Export : string.Empty;
			AssertEquals($"header({headerCountry}) bill1({bill1Country}) item11({item11Country}) item12({item12Country}) item21({item21Country}) isLinkedExport={isLinkedExport}", expectedResult, NctsDataProviderHelper.IsSendDestinationCountryAtConsignmentLevelV4(nctsHeader));
		}
	}
}
