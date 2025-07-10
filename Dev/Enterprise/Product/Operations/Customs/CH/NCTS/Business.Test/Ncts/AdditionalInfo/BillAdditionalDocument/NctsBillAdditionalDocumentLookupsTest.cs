using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NctsBillAdditionalDocumentLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestSubTypeList()
	{
		NctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;
		AssertCollectionContains("SubTypeList contains INF", "INF", AdditionalDocument.Lookups.SubTypeList.GetAllCodes());
	}

	public void TestSubTypeList_NationalTransitSwitzerland() => CombineAssertions(() =>
	{
		NctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		AssertCollectionNotContains("SubTypeList doesn't contain REF", "REF", AdditionalDocument.Lookups.SubTypeList.GetAllCodes());
		AssertCollectionContains("SubTypeList contains INF", "INF", AdditionalDocument.Lookups.SubTypeList.GetAllCodes());
	});

	public void TestTypeCodeList() => CombineAssertions(() =>
	{
		new RefDataTestHelper(Factory).CreateTypeCodeList();

		NctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;
		AdditionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		AdditionalDocument.Lookups.TypeCodeList.Load();
		AssertContainsExactElementsInAnyOrder("T1 - SubType TD44N, ", new[] { "N235H" }, AdditionalDocument.Lookups.TypeCodeList.Select(c => c.ZZD_Code));
		AssertSame("TypeCodeList cached (TRA)", AdditionalDocument.Lookups.TypeCodeList, AdditionalDocument.Lookups.TypeCodeList);

		AdditionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		AdditionalDocument.Lookups.TypeCodeList.Load();
		AssertContainsExactElementsInAnyOrder("T1 - SubType AR44N, ", new[] { "Y900" }, AdditionalDocument.Lookups.TypeCodeList.Select(c => c.ZZD_Code));
		AssertSame("TypeCodeList cached (REF)", AdditionalDocument.Lookups.TypeCodeList, AdditionalDocument.Lookups.TypeCodeList);

		AdditionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		AdditionalDocument.Lookups.TypeCodeList.Load();
		AssertContainsExactElementsInAnyOrder("T1 - SubType AI44N, ", new[] { "V1013" }, AdditionalDocument.Lookups.TypeCodeList.Select(c => c.ZZD_Code));
		AssertSame("TypeCodeList cached (INF)", AdditionalDocument.Lookups.TypeCodeList, AdditionalDocument.Lookups.TypeCodeList);
	});

	public void TestTypeCodeList_NationalTransitSwitzerland() => CombineAssertions(() =>
	{
		new RefDataTestHelper(Factory).CreateTypeCodeList();

		NctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		AdditionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		AdditionalDocument.Lookups.TypeCodeList.Load();
		AssertContainsExactElementsInAnyOrder("NationalTransit - SubType TD44N, ", new[] { "N235H" }, AdditionalDocument.Lookups.TypeCodeList.Select(c => c.ZZD_Code));
		AssertSame("TypeCodeList cached (TRA)", AdditionalDocument.Lookups.TypeCodeList, AdditionalDocument.Lookups.TypeCodeList);

		AdditionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		AdditionalDocument.Lookups.TypeCodeList.Load();
		AssertContainsExactElementsInAnyOrder("NationalTransit - SubType AI44E, ", new[] { "A1100" }, AdditionalDocument.Lookups.TypeCodeList.Select(c => c.ZZD_Code));
		AssertSame("TypeCodeList cached (INF)", AdditionalDocument.Lookups.TypeCodeList, AdditionalDocument.Lookups.TypeCodeList);
	});

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader;
	}

	NctsHeader NctsHeader => nctsHeader ??= CreateNctsHeader();
	NctsHeader nctsHeader;

	NctsBill NctsBill => nctsBill ??= NctsHeader.Bills.AddNew();
	NctsBill nctsBill;

	NctsBillAdditionalDocument AdditionalDocument => additionalDocument ??= NctsBill.AdditionalDocuments.AddNew();
	NctsBillAdditionalDocument additionalDocument;
}
