using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsDepartureCargoDescCollection))]
sealed class NctsDepartureCargoDescCollectionTest : ActiveBusinessObjectCollectionTestCase<NctsDepartureCargoDescCollection>
{
	protected override NctsDepartureCargoDescCollection GetCollectionToTest()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		return new NctsDepartureCargoDescCollection(nctsHeader.Bills.AddNew());
	}

	public void TestDefaultAeoCertificateForConsignor()
	{
		var bill = nctsHeader.Bills.AddNew();
		var goodsItem1 = bill.GoodsItems.AddNew();
		AssertEquals("No default supporting document expected", 0, goodsItem1.SupportingDocuments.Count);

		NCTSTestHelper.CreateJobDocAddressForTest(
			Factory
			, traderId: "CO1"
			, nctsHeader.Consignor
			, "2"
			, configureOrgHeaderAction: x => x.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO022"));

		var goodsItemY022 = bill.GoodsItems.AddNew();
		AssertThatNctsSupportingDocumentContainAeoCertificate(goodsItemY022, "Y022", "AEO022");
	}

	public void TestDefaultAeoCertificateForConsignee()
	{
		var bill = nctsHeader.Bills.AddNew();
		var goodsItem1 = bill.GoodsItems.AddNew();
		AssertEquals("No default supporting document expected", 0, goodsItem1.SupportingDocuments.Count);

		NCTSTestHelper.CreateJobDocAddressForTest(
			Factory
			, traderId: "CE1"
			, nctsHeader.Consignee
			, suffix: "3"
			, configureOrgHeaderAction: x => x.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO023"));

		var goodsItemY023 = bill.GoodsItems.AddNew();
		AssertThatNctsSupportingDocumentContainAeoCertificate(goodsItemY023, "Y023", "AEO023");
	}

	public void TestDefaultAeoCertificateForDeclarant()
	{
		var bill = nctsHeader.Bills.AddNew();
		var goodsItem1 = bill.GoodsItems.AddNew();
		AssertEquals("No default supporting document expected", 0, goodsItem1.SupportingDocuments.Count);

		var declarant = Factory.New<OrgHeader>();
		declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator, "AEO024");
		nctsHeader.DeclarantAddressPK = declarant.MainAddress.PK;
		nctsHeader.RepresentationType = RepresentationTypeList.Codes._2Direct;

		var goodsItemY024 = bill.GoodsItems.AddNew();
		AssertThatNctsSupportingDocumentContainAeoCertificate(goodsItemY024, "Y024", "AEO024");
	}

	public void TestIsLastGoodsItem()
	{
		Assert("Collection has no items", !nctsHeader.MovementHeader.GoodsItems.IsLastGoodsItem(0));

		var bill = nctsHeader.Bills.AddNew();
		var goodsItem1 = bill.GoodsItems.AddNew();
		goodsItem1.BY_LineNo = 1;
		var goodsItem2 = bill.GoodsItems.AddNew();
		goodsItem2.BY_LineNo = 2;

		Assert($"{nameof(goodsItem1)} should not be the last item", !((NctsDepartureCargoDescCollection)bill.GoodsItems).IsLastGoodsItem(1));
		Assert($"{nameof(goodsItem2)} should be the last item", ((NctsDepartureCargoDescCollection)bill.GoodsItems).IsLastGoodsItem(2));
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
	}
	NctsHeader nctsHeader;

	void AssertThatNctsSupportingDocumentContainAeoCertificate(NctsDepartureCargoDesc goodItem, ZString codeExpected, ZString referenceNumberExpected)
	{
		var nctsSupportingDocuments = goodItem.SupportingDocuments.Cast<NctsSupportingDocument>();
		AssertEquals($"A {codeExpected} Supporting Document is expected, Count", 1, nctsSupportingDocuments.Count(x => x.CSI_Code == codeExpected));
		var y022Sup = nctsSupportingDocuments.SingleOrDefault(x => x.CSI_Code == codeExpected);
		CombineAssertions($"Check {codeExpected} Supporting Document", () =>
		{
			AssertEquals("Code", codeExpected, y022Sup.CSI_Code);
			AssertEquals("ReferenceNumber", referenceNumberExpected, y022Sup.CSI_ReferenceNumber);
		});
	}

	public void TestAtLeastOneOfGoodItemsCountryOfDestinationIsFilledButNotAllOfThem()
	{
		var bill = nctsHeader.Bills.AddNew();
		AssertEquals("No goods item exist", false, ((NctsDepartureCargoDescCollection)bill.GoodsItems).AtLeastOneOfGoodItemsCountryOfDestinationIsFilledButNotAllOfThem());

		var goodItem = bill.GoodsItems.AddNew();
		AssertEquals("One good item exsist but with empty Country Of Destination", false, nctsHeader.MovementHeader.GoodsItems.AtLeastOneOfGoodItemsCountryOfDestinationIsFilledButNotAllOfThem());

		goodItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Italy;
		AssertEquals("One good item exsist with filled Country Of Destination", false, nctsHeader.MovementHeader.GoodsItems.AtLeastOneOfGoodItemsCountryOfDestinationIsFilledButNotAllOfThem());

		var secondGoodItem = bill.GoodsItems.AddNew();
		AssertEquals("One good item with filled Country Of Dispatch and a good item with empty Country of Destination", true, ((NctsDepartureCargoDescCollection)bill.GoodsItems).AtLeastOneOfGoodItemsCountryOfDestinationIsFilledButNotAllOfThem());

		secondGoodItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.France;
		AssertEquals("Two good items with filled County of Destination", false, ((NctsDepartureCargoDescCollection)bill.GoodsItems).AtLeastOneOfGoodItemsCountryOfDestinationIsFilledButNotAllOfThem());
	}

	public void TestAtLeastOneOfGoodItemsCountryOfDispatchIsFilledButNotAllOfThem()
	{
		var bill = nctsHeader.Bills.AddNew();

		AssertEquals("No goods item exist", false, ((NctsDepartureCargoDescCollection)bill.GoodsItems).AtLeastOneOfGoodItemsCountryOfDispatchIsFilledButNotAllOfThem());

		var goodItem = bill.GoodsItems.AddNew();
		AssertEquals("One good item exsist but with empty Country Of Dispatch", false, ((NctsDepartureCargoDescCollection)bill.GoodsItems).AtLeastOneOfGoodItemsCountryOfDispatchIsFilledButNotAllOfThem());

		goodItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Italy;
		AssertEquals("One good item exsist with filled Country Of Dispatch", false, ((NctsDepartureCargoDescCollection)bill.GoodsItems).AtLeastOneOfGoodItemsCountryOfDispatchIsFilledButNotAllOfThem());

		var secondGoodItem = bill.GoodsItems.AddNew();
		AssertEquals("One good item with filled Country Of Dispatch and a good item with empty Country of Dispatch", true, ((NctsDepartureCargoDescCollection)bill.GoodsItems).AtLeastOneOfGoodItemsCountryOfDispatchIsFilledButNotAllOfThem());

		secondGoodItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.France;
		AssertEquals("Two good items with filled County of Disptach", false, ((NctsDepartureCargoDescCollection)bill.GoodsItems).AtLeastOneOfGoodItemsCountryOfDispatchIsFilledButNotAllOfThem());
	}

	public void TestGetItemsWithSendableGroupedPreviousDocuments()
	{
		var bill = nctsHeader.Bills.AddNew();
		AssertEquals("MergedLinesWithSendableGroupedPreviousDocuments count", 0, ((NctsDepartureCargoDescCollection)bill.GoodsItems).GetItemsWithSendableGroupedPreviousDocuments().Count());

		var goodsItem1 = bill.GoodsItems.AddNew();

		goodsItem1.PreviousDocuments.AddNew().CSI_Procedure = "A3";
		goodsItem1.PreviousDocuments.AddNew().CSI_Procedure = "MRN";
		goodsItem1.BY_Status = "";
		AssertArrayEqualsByElements("When BY_Status is empty, ItemsWithSendableGroupedPreviousDocuments", new[] { goodsItem1 }, GetExpectedItems());

		goodsItem1.BY_Status = "NBR";
		AssertArrayEqualsByElements("When BY_Status is 'NBR', ItemsWithSendableGroupedPreviousDocuments", new[] { goodsItem1 }, GetExpectedItems());

		goodsItem1.BY_Status = "NBA";
		AssertArrayEqualsByElements("When BY_Status is 'NBA', ItemsWithSendableGroupedPreviousDocuments", System.Array.Empty<object>(), GetExpectedItems());

		goodsItem1.BY_Status = "NBS";
		AssertArrayEqualsByElements("When BY_Status is 'NBS', ItemsWithSendableGroupedPreviousDocuments", new[] { goodsItem1 }, GetExpectedItems());

		goodsItem1.PreviousDocuments.RemoveAndDeleteAll();
		AssertArrayEqualsByElements("When status is allowed but there are no previous documents, ItemsWithSendableGroupedPreviousDocuments", System.Array.Empty<object>(), GetExpectedItems());

		NctsDepartureCargoDesc[] GetExpectedItems() => ((NctsDepartureCargoDescCollection)bill.GoodsItems).GetItemsWithSendableGroupedPreviousDocuments().ToArray();
	}

	public void TestHasAnyItemWithGroupedPreviousDocuments()
	{
		var bill = nctsHeader.Bills.AddNew();
		AssertEquals("When the collection has no goods items, TestHasAnyItemWithGroupedPreviousDocuments", false, ((NctsDepartureCargoDescCollection)bill.GoodsItems).HasAnyItemWithGroupedPreviousDocuments);

		var goodsItem = bill.GoodsItems.AddNew();
		AssertEquals("When the collection has 1 goods item without previous documents, TestHasAnyItemWithGroupedPreviousDocuments", false, ((NctsDepartureCargoDescCollection)bill.GoodsItems).HasAnyItemWithGroupedPreviousDocuments);

		goodsItem.PreviousDocuments.AddNew().CSI_Procedure = "A3";
		AssertEquals("When the collection has 1 goods item with 1 previous document, TestHasAnyItemWithGroupedPreviousDocuments", false, nctsHeader.MovementHeader.GoodsItems.HasAnyItemWithGroupedPreviousDocuments);

		goodsItem.PreviousDocuments.AddNew().CSI_Procedure = "MRN";
		AssertEquals("When the collection has 1 goods item with 2 PA previous documents, TestHasAnyItemWithGroupedPreviousDocuments", true, ((NctsDepartureCargoDescCollection)bill.GoodsItems).HasAnyItemWithGroupedPreviousDocuments);
	}
}
