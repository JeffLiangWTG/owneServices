using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(CusGoodsLocationAddress))]
sealed class CusGoodsLocationAddressTest : EnterpriseBusinessObjectTestCase
{
	public void TestLookups()
	{
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.Address;
		goodsLocationAddress = goodsLocation.Address;

		AssertType<IT.Business.Declaration.CusGoodsLocationAddressLookups>("Lookups", goodsLocationAddress.Lookups);
	}

	public void TestValidation()
	{
		var cusGoodsLocationAddress = Factory.New<CusGoodsLocationAddress>();
		AssertType<CusGoodsLocationAddressValidation>("Validation", cusGoodsLocationAddress.Validation);
	}

	public void TestE2_ValidationStatusIsSetToManuallyVerifiedWhenAddressIsOverride()
	{
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_Address1 = "MG Road";
		orgAddress.OA_Address2 = "4th Cross";
		goodsLocationAddress.E2_OA_Address = orgAddress.PK;

		AssertEquals("[PRE-CONDITION] E2_AddressOverride", false, goodsLocationAddress.E2_AddressOverride);

		goodsLocationAddress.E2_AddressOverride = true;
		AssertEquals("When Address is override, E2_ValidationStatus", AddressValidationStatus.ManuallyVerified, goodsLocationAddress.E2_ValidationStatus);

		goodsLocationAddress.E2_AddressOverride = false;
		AssertEquals("When Address is not override, E2_ValidationStatus matches linked address OA_ValidationStatus", orgAddress.OA_ValidationStatus, goodsLocationAddress.E2_ValidationStatus);
	}

	public void TestE2_Address1AndE2_Address2_Truncate()
	{
		using (SetTransitionPeriod(true))
		{
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.Address1 = new ZString('1', 50);
			orgAddress.Address2 = new ZString('2', 50);

			goodsLocationAddress.E2_OA_Address = orgAddress.PK;
			var expectedAddress = new ZString('1', 50) + new ZString('2', 50);
			AssertEquals("Transition Period ON - When Address1 + Address2 more than max length, no truncation expected", expectedAddress, goodsLocationAddress.E2_Address1AndE2_Address2);
		}

		using (SetTransitionPeriod(false))
		{
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.Address1 = new ZString('1', 50);
			orgAddress.Address2 = new ZString('2', 50);

			goodsLocationAddress.E2_OA_Address = orgAddress.PK;
			var expectedAddress = new ZString('1', 50) + new ZString('2', 50);
			AssertEquals("Transition Period OFF - When Address1 + Address2 more than max length, no truncation expected", expectedAddress, goodsLocationAddress.E2_Address1AndE2_Address2);
		}
	}

	public void TestE2_City_Truncate()
	{
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.City = new ZString('1', 50);

		goodsLocationAddress.E2_OA_Address = orgAddress.PK;
		var expectedAddress = new ZString('1', 35);

		using (SetTransitionPeriod(true))
		{
			AssertEquals("When City more than max length", expectedAddress, goodsLocationAddress.E2_City);
		}

		using (SetTransitionPeriod(false))
		{
			AssertEquals("When City more than max length", expectedAddress, goodsLocationAddress.E2_City);
		}
	}

	#region Implementation

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		goodsLocation = nctsHeader.MovementHeader.GoodsLocation;
		goodsLocation.CGL_LocationUse = "DEP";
		goodsLocationAddress = goodsLocation.Address;
	}

	NctsHeader nctsHeader;
	CusGoodsLocation goodsLocation;
	CusGoodsLocationAddress goodsLocationAddress;

	protected override BusinessObject GetNewBusinessObject() => goodsLocationAddress;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => goodsLocationAddress;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => goodsLocationAddress;

	IDisposable SetTransitionPeriod(bool isActive)
		=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDateTime.Today, isActive);

	#endregion
}
