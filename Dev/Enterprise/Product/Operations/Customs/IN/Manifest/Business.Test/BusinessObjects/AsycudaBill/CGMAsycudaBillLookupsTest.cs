using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.IN.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAsycudaBillLookups))]
sealed class CGMAsycudaBillLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestSpecialCargoCodeList()
	{
		AssertSpecialCargoCodeList("AIR", Factory.GetCachedValue<ShipmentTypeList>(), "T, P, S");
		AssertSpecialCargoCodeList("SEA", Factory.GetCachedValue<ItemTypeList>(), "GC, OT, UB");
	}

	void AssertSpecialCargoCodeList(ZString transportMode, CodeDescriptionPairList expectList, string expectCodes)
	{
		var header = Factory.NewWithValidTestData<CGMAsycudaManifestHeader>();
		header.AMA_TransportMode = transportMode;
		var bill = header.Bills.AddNew();
		var cachedShipmentTypeList = Factory.GetCachedValue<ShipmentTypeList>();
		var lookedUpList = bill.Lookups.SpecialCargoCodeList;
		CombineAssertions(() =>
		{
			AssertSame($"SpecialCargoCodeList for {transportMode}", expectList, lookedUpList);
			AssertEquals($"SpecialCargoCodes for {transportMode}", expectCodes, lookedUpList.CodesAsString);
		});
	}

	public void TestCargoMovementCodeList()
	{
		var bill = Factory.NewWithValidTestData<CGMAsycudaBill>();
		var cachedList = Factory.GetCachedValue<CargoMovementList>();
		var lookedUpList = (CargoMovementList)bill.Lookups.CargoStatusList;
		CombineAssertions(() =>
		{
			AssertSame("CargoMovementList is cached", cachedList, lookedUpList);
			AssertEquals("CargoMovementList values", "LC, TC, TI", lookedUpList.CodesAsString);
		});
	}

	public void TestNatureOfCargoList()
	{
		var bill = Factory.NewWithValidTestData<CGMAsycudaBill>();
		var cachedList = Factory.GetCachedValue<NatureOfCargoList>();
		var lookedUpList = (NatureOfCargoList)bill.Lookups.NatureOfCargoList;
		CombineAssertions(() =>
		{
			AssertSame("NatureOfCargoList is cached", cachedList, lookedUpList);
			AssertEquals("NatureOfCargoList values", "C, CP, DB, LB, P", lookedUpList.CodesAsString);
		});
	}

	public void TestInlandTransportModeList()
	{
		var bill = Factory.NewWithValidTestData<CGMAsycudaBill>();
		var cachedList = Factory.GetCachedValue<ModeOfTransportList>();
		var lookedUpList = (ModeOfTransportList)bill.Lookups.InlandTransportModeList;
		CombineAssertions(() =>
		{
			AssertSame("ModeOfTransportList is cached", cachedList, lookedUpList);
			AssertEquals("ModeOfTransportList values", "R, S, T", lookedUpList.CodesAsString);
		});
	}

	public void TestDestinationCodeList()
	{
		var bill = Factory.NewWithValidTestData<CGMAsycudaBill>();
		var cachedList = Factory.GetCachedValue<DestinationCodeList>();
		var lookedUpList = (DestinationCodeList)bill.Lookups.DestinationCodeList;
		CombineAssertions(() =>
		{
			AssertSame("DestinationCodeList is cached", cachedList, lookedUpList);
			AssertEquals("DestinationCodeList values", "CUS, CFS", lookedUpList.CodesAsString);
		});
	}

	public void TestCustomsFinalDestinationPortList()
	{
		var bill = Factory.New<CGMAsycudaBill>();
		bill.ABL_LocationInformation = DestinationCodeList.Codes.CUS;

		ReferenceDataTestHelper.AssertCustomsOfficeCollection(
			() => bill.Lookups.CustomsFinalDestinationPortList as ZZRefCusCodeListCombinedCollection);

		bill.ABL_LocationInformation = DestinationCodeList.Codes.CFS;
		AssertEquals(0, bill.Lookups.CustomsFinalDestinationPortList.Count);
		AssertType<CodeDescriptionPairList>(bill.Lookups.CustomsFinalDestinationPortList);
	}

	public void TestLocalTransportCarriers()
	{
		var bill = Factory.NewWithValidTestData<CGMAsycudaBill>();
		bill.ABL_InlandTransportMode = ModeOfTransportList.Codes.Road;
		CombineAssertions(() =>
		{
			AssertLocalTransportCarriers(ModeOfTransportList.Codes.Road, OrganisationSecondaryTypes.LocalTransport);
			AssertLocalTransportCarriers(ModeOfTransportList.Codes.Ship, OrganisationSecondaryTypes.ShippingLine);
			AssertLocalTransportCarriers(ModeOfTransportList.Codes.Train, OrganisationSecondaryTypes.Rail);
		});

		void AssertLocalTransportCarriers(string modeOfTransport, string expectOrganisationSecondaryTypes)
		{
			bill.ABL_InlandTransportMode = modeOfTransport;
			var collection = bill.Lookups.LocalTransportCarriers;
			AssertEquals($"Secondary Type Property - {modeOfTransport}", expectOrganisationSecondaryTypes, (ZString)collection.FilterBusinessObjectDefaults["Secondary Type" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
			AssertEquals($"Organisation Types Property4 - {modeOfTransport}", true, (ZBool)collection.FilterBusinessObjectDefaults["Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property4"].Value);
			AssertEquals($"Organisation Types AndJoinCondition - {modeOfTransport}", true, (ZBool)collection.FilterBusinessObjectDefaults["Organisation Types" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "AndJoinCondition"].Value);
		}
	}

	public void TestCustomsStatusList()
	{
		var bill = Factory.NewWithValidTestData<CGMAsycudaBill>();
		var header = bill.Header;

		string getMessage() => $"BillMessageStatus:{bill.ABL_MessageStatus}, HeaderMessageStatus:{header.MessageStatus}, HeaderCustomsStatus:{header.RegistrationStatus}";

		CombineAssertions(getMessage(), () =>
		{
			var lookupList = bill.Lookups.CustomsStatusList;
			AssertContainsExactElementsInAnyOrder(new[] { BillActionList.Codes.Fresh }, lookupList.GetAllCodes());
			AssertSame("ActionList - cached", lookupList, bill.Lookups.CustomsStatusList);
		});

		header.AMA_MessageStatus = MessageStatusList.Codes.MessageAccepted;
		header.RegistrationStatus = RegistrationStatusList.Codes.ManifestRegistered;
		CombineAssertions(getMessage(), () =>
		{
			var lookupList = bill.Lookups.CustomsStatusList;
			AssertContainsExactElementsInAnyOrder(new[] { BillActionList.Codes.Supplementary }, bill.Lookups.CustomsStatusList.GetAllCodes());
			AssertSame("ActionList - cached", lookupList, bill.Lookups.CustomsStatusList);
		});

		header.AMA_MessageStatus = MessageStatusList.Codes.MessageQueued;
		AssertSame("ActionList - cached", Factory.GetCachedValue<BillActionList>(), bill.Lookups.CustomsStatusList);

		bill.ABL_MessageStatus = BillMessageStatusList.Codes.Accepted;
		CombineAssertions(getMessage(), () =>
		{
			var lookupList = bill.Lookups.CustomsStatusList;
			AssertContainsExactElementsInAnyOrder(new[] { BillActionList.Codes.Amendment, BillActionList.Codes.Delete }, lookupList.GetAllCodes());
			AssertSame("ActionList - cached", lookupList, bill.Lookups.CustomsStatusList);
		});

		bill.ABL_MessageStatus = BillMessageStatusList.Codes.Deleted;
		AssertSame("ActionList - cached", Factory.GetCachedValue<BillActionList>(), bill.Lookups.CustomsStatusList);
	}

	public void TestMessageStatusList()
	{
		var bill = Factory.NewWithValidTestData<CGMAsycudaBill>();
		var cachedList = Factory.GetCachedValue<BillMessageStatusList>();
		var lookedUpList = (BillMessageStatusList)bill.Lookups.MessageStatusList;
		CombineAssertions(() =>
		{
			AssertSame("MessageStatusList is cached", cachedList, lookedUpList);
			AssertEquals("MessageStatusList codes", new BillMessageStatusList().CodesAsString, lookedUpList.CodesAsString);
		});
	}
}
