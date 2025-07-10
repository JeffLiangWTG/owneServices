using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class NctsSupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestTypeCodeList()
	{
		new RefDataTestHelper(Factory).CreateTypeCodeList();

		((NctsDepartureMovementHeader)SupportingDocument.Header.MovementHeader).BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.NationalTransitSwitzerland;
		SupportingDocument.Lookups.TypeCodeList.Load();
		AssertContainsExactElementsInAnyOrder("National Transit - DC44E", new[] { "CHE1", "CHE2" }, SupportingDocument.Lookups.TypeCodeList.Select(c => c.ZZD_Code));
		AssertSame("TypeCodeList cached", SupportingDocument.Lookups.TypeCodeList, SupportingDocument.Lookups.TypeCodeList);

		((NctsDepartureMovementHeader)SupportingDocument.Header.MovementHeader).BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
		SupportingDocument.Lookups.TypeCodeList.Load();
		AssertContainsExactElementsInAnyOrder("International Transit - DC44N", new[] { "INT1", "INT2" }, SupportingDocument.Lookups.TypeCodeList.Select(c => c.ZZD_Code));
		AssertSame("TypeCodeList cached", SupportingDocument.Lookups.TypeCodeList, SupportingDocument.Lookups.TypeCodeList);
	}

	NctsSupportingDocument SupportingDocument => supportingDocument ?? (supportingDocument = CreateNctsSupportingDocument(Factory));
	NctsSupportingDocument supportingDocument;

	NctsSupportingDocument CreateNctsSupportingDocument(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var bill = nctsHeader.Bills.AddNew();
		var goodsItem = bill.GoodsItems.AddNew();
		return goodsItem.SupportingDocuments.AddNew();
	}
}
