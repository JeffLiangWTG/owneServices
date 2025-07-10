using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

[TestedType(typeof(NctsArrivalMovementHeader))]
sealed class NctsArrivalMovementHeaderTest : EU.NCTS.Business.Testing.NctsArrivalMovementHeaderAbstractTest
{
	public void TestSetDefaultValues()
	{
		var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

		AssertEquals(ZBool.True, arrivalMovement.IsSimplifiedNctsProcedure);
	}

	public void TestGoodsLocation()
	{
		var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

		AssertType<CusGoodsLocation>(arrivalMovement.GoodsLocation);
	}

	public void TestArrivalTransportInfos()
	{
		var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

		AssertType<EU.NCTS.Business.ArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>>(arrivalMovement.ArrivalTransportInfos);
	}

	public void TestDestinationTraderNotChangedOnAuthorizationOwnerChanged()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();

		var authorizationHeader = Factory.New<CusAuthorisationHeader>();
		authorizationHeader.CPH_Number = "123";
		authorizationHeader.CPH_Type = "abc";
		authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;

		var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();
		var nctsHeader = arrivalMovement.Header;

		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.DestinationTrader.OrganisationPK = orgHeader2.PK;

		CombineAssertions(() =>
		{
			arrivalMovement.AuthorizationCode = "abc";
			AssertEquals("When only code is set, owner is empty", true, arrivalMovement.AuthorizationOwner.IsEmpty);
			AssertEquals("When only code is set, DestinationTrader.OrganisationPK is not changed", orgHeader2.PK, nctsHeader.DestinationTrader.OrganisationPK);
			arrivalMovement.AuthorizationNumber = "123";
			AssertEquals("When number is set after code, owner is not empty", false, arrivalMovement.AuthorizationOwner.IsEmpty);
			AssertEquals("When number is set after code, owner is defaulted", orgHeader.PK, arrivalMovement.AuthorizationOwner);
			AssertEquals("When number is set after code and flag is false (BE), DestinationTrader.OrganisationPK is NOT defaulted", orgHeader2.PK, nctsHeader.DestinationTrader.OrganisationPK);

			nctsHeader.DestinationTrader.OrganisationPK = orgHeader.PK;
			arrivalMovement.AuthorizationOwner = orgHeader2.PK;
			AssertEquals("When owner is changed and not empty and flag is false (BE), DestinationTrader.OrganisationPK is NOT changed", orgHeader.PK, nctsHeader.DestinationTrader.OrganisationPK);

			arrivalMovement.AuthorizationCode = ZString.Empty;
			arrivalMovement.AuthorizationNumber = ZString.Empty;
			arrivalMovement.AuthorizationOwner = ZGuid.Empty;
			AssertEquals("When owner is changed and empty and flag is false (BE), DestinationTrader.OrganisationPK is NOT changed", orgHeader.PK, nctsHeader.DestinationTrader.OrganisationPK);
		});
	}

	public void TestShouldSyncDestinationTraderWithAuthorization()
	{
		var arrivalMovement = (NctsArrivalMovementHeader)GetNewBusinessObject();

		AssertEquals("Flag is false for BE", false, arrivalMovement.ShouldSyncDestinationTraderWithAuthorization);
	}

	protected override void TestBizObjectField(ZPropertyInfo info)
	{
		if (info.Name != "DestinationCustomsOfficeCodeForDeparture"
			&& info.Name != "DestinationCustomsOfficeCodeForArrival")
		{
			base.TestBizObjectField(info);
		}
	}

	public void TestTotalUnloadedGrossMassInKilograms()
	{
		var header = ((NctsArrivalMovementHeader)GetNewBusinessObject()).Header;
		var bill1 = header.Bills.AddNew();
		var goodsItem1 = bill1.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_GrossWeight = 300;
		goodsItem1.BY_GrossWeightUnit = "KG";

		var bill2 = header.Bills.AddNew();
		var goodsItem2 = bill2.ArrivalGoodsItems.AddNew();
		goodsItem2.BY_GrossWeight = 10;
		goodsItem2.BY_GrossWeightUnit = "KG";
		var goodsItem3 = bill2.ArrivalGoodsItems.AddNew();
		goodsItem3.BY_GrossWeight = 600;
		goodsItem3.BY_GrossWeightUnit = "G";

		var goodsItem4 = bill2.ArrivalGoodsItems.AddNew();
		goodsItem4.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS;
		goodsItem4.BY_GrossWeight = 60;
		goodsItem4.BY_GrossWeightUnit = "KG";

		var goodsItem5 = bill2.ArrivalGoodsItems.AddNew();
		goodsItem5.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
		goodsItem5.BY_GrossWeight = 100;
		goodsItem5.BY_GrossWeightUnit = "KG";

		AssertEquals(410.6m, header.ArrivalMovementHeader.TotalUnloadedGrossMassInKilograms);
	}

	public void TestTotalUnloadedGrossMassInKilograms_DIF()
	{
		var header = ((NctsArrivalMovementHeader)GetNewBusinessObject()).Header;
		var bill1 = header.Bills.AddNew();
		var goodsItem1 = bill1.ArrivalGoodsItems.AddNew();
		goodsItem1.BY_GrossWeight = 300;
		goodsItem1.BY_GrossWeightUnit = "KG";

		var bill2 = header.Bills.AddNew();
		var goodsItem2 = bill2.ArrivalGoodsItems.AddNew();
		goodsItem2.BY_GrossWeight = 10;
		goodsItem2.BY_GrossWeightUnit = "KG";
		var goodsItem3 = bill2.ArrivalGoodsItems.AddNew();
		goodsItem3.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;
		goodsItem3.BY_GrossWeight = 2;
		goodsItem3.BY_GrossWeightUnit = "KG";
		var unloadedGoodsItem = goodsItem3.UnloadedGoodsItem;
		unloadedGoodsItem.BY_GrossWeight = 10;
		unloadedGoodsItem.BY_GrossWeightUnit = "KG";

		var goodsItem4 = bill2.ArrivalGoodsItems.AddNew();
		goodsItem4.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS;
		goodsItem4.BY_GrossWeight = 60;
		goodsItem4.BY_GrossWeightUnit = "KG";
		AssertEquals(320m, header.ArrivalMovementHeader.TotalUnloadedGrossMassInKilograms);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	NctsArrivalMovementHeader GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		return nctsHeader.ArrivalMovementHeader;
	}
}
