using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.DocumentWrappers;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(SecurityNctsHeaderDocumentWrapper))]
sealed class SecurityNctsHeaderDocumentWrapperTest : NonPersistentBusinessObjectTestCase
{
	public void TestNew()
	{
		AssertExceptionThrown<ArgumentNullException>("Expected exception when NctsHeader parameter is null", () => SecurityNctsHeaderDocumentWrapper.New(null, Factory));
		AssertExceptionThrown<ArgumentNullException>("Expected exception when Factory parameter is null", () => SecurityNctsHeaderDocumentWrapper.New(nctsHeader, null));
		AssertNotNull("Instance of SecurityNctsHeaderDocumentWrapper expected", SecurityNctsHeaderDocumentWrapper.New(nctsHeader, Factory));
	}

	public void TestBox35GrossMass_Phase4()
	{
		CombineAssertions(() =>
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var wrapper = SecurityNctsHeaderDocumentWrapper.New(nctsHeader, Factory);
			AssertEquals(nameof(wrapper.BOX35GROSSMASS), "", wrapper.BOX35GROSSMASS);
			goodsItem.BY_GrossWeight = 30;
			goodsItem.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals(nameof(wrapper.BOX35GROSSMASS), "30", wrapper.BOX35GROSSMASS);

			goodsItem.BY_GrossWeight = 123.4678m;
			goodsItem.BY_GrossWeightUnit = Core.Constants.Weight.Grams;
			AssertEquals(nameof(wrapper.BOX35GROSSMASS), "0.12347", wrapper.BOX35GROSSMASS);

			goodsItem.BY_GrossWeight = 99.123456m;
			goodsItem.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals(nameof(wrapper.BOX35GROSSMASS), "99.12346", wrapper.BOX35GROSSMASS);
		});
	}

	public void TestBox35GrossMass_Phase5()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		var houseBill1 = nctsHeader.Bills.AddNew();
		var item1 = houseBill1.GoodsItems.AddNew();
		item1.BY_GrossWeight = 50;
		item1.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;

		var houseBill2 = nctsHeader.Bills.AddNew();
		var item2 = houseBill2.GoodsItems.AddNew();
		item2.BY_GrossWeight = 50;
		item2.BY_GrossWeightUnit = Core.Constants.Weight.Grams;

		var wrapper = SecurityNctsHeaderDocumentWrapper.New(nctsHeader, Factory);
		AssertEquals(nameof(wrapper.BOX35GROSSMASS), "50.05", wrapper.BOX35GROSSMASS);
	}

	public void TestBox38NetMass_Phase4()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var wrapper = SecurityNctsHeaderDocumentWrapper.New(nctsHeader, Factory);
			AssertEquals("Not populated", "", wrapper.BOX38NETTMASS);

			goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem.BY_NetWeight = 30;
			goodsItem.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals("Kilograms", "30", wrapper.BOX38NETTMASS);

			goodsItem.BY_NetWeight = 123.4678m;
			goodsItem.BY_NetWeightUnit = Core.Constants.Weight.Grams;
			AssertEquals("Grams", "0.12347", wrapper.BOX38NETTMASS);

			goodsItem.BY_NetWeight = 99.123456m;
			goodsItem.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals("Switched back to Kilograms", "99.12346", wrapper.BOX38NETTMASS);
		});
	}

	public void TestBox38NetMass_Phase5()
	{
		CombineAssertions(() =>
		{
			var wrapper = SecurityNctsHeaderDocumentWrapper.New(nctsHeader, Factory);
			AssertEquals("Not populated", "", wrapper.BOX38NETTMASS);

			goodsItem.BY_NetWeight = 30;
			goodsItem.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals("Kilograms", "30", wrapper.BOX38NETTMASS);

			goodsItem.BY_NetWeight = 123.4678m;
			goodsItem.BY_NetWeightUnit = Core.Constants.Weight.Grams;
			AssertEquals("Grams", "0.12347", wrapper.BOX38NETTMASS);

			goodsItem.BY_NetWeight = 99.123456m;
			goodsItem.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals("Switched back to Kilograms", "99.12346", wrapper.BOX38NETTMASS);
		});
	}

	public void TestNOTRELEASEDWATERMARK()
	{
		var wrapper = SecurityNctsHeaderDocumentWrapper.New(nctsHeader, Factory);
		AssertEquals("NOTRELEASEDWATERMARK", "NOT RELEASED", wrapper.NOTRELEASEDWATERMARK);

		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "21FR00007411BBC885";
		wrapper = SecurityNctsHeaderDocumentWrapper.New(nctsHeader, Factory);
		AssertEquals("NOTRELEASEDWATERMARK", "NOT RELEASED", wrapper.NOTRELEASEDWATERMARK);

		var entryNum = CusEntryNumber.LoadOrCreate(nctsHeader, "CLR", Core.Constants.CountryCodes.Italy);
		entryNum.CE_EntryNum = "12345";
		AssertEquals("NOTRELEASEDWATERMARK", "", wrapper.NOTRELEASEDWATERMARK);

		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		wrapper = SecurityNctsHeaderDocumentWrapper.New(nctsHeader, Factory);
		AssertEquals("NOTRELEASEDWATERMARK", "NOT RELEASED", wrapper.NOTRELEASEDWATERMARK);
	}

	public void TestBoxDResult()
	{
		var wrapper = SecurityNctsHeaderDocumentWrapper.New(nctsHeader, Factory);

		var entryNum = CusEntryNumber.LoadOrCreate(nctsHeader, "CLR", Core.Constants.CountryCodes.Italy);
		entryNum.CE_EntryNum = "12345";

		AssertEquals("When ReleaseCode exists, BoxDResult is CODICE SVINCOLO plus CE_EntryNum", "CODICE SVINCOLO 12345", wrapper.BOXDRESULT);

		entryNum.CE_EntryNum = "";

		AssertEquals("When ReleaseCode does not exist, BoxDResult is empty", "", wrapper.BOXDRESULT);
	}

	public void TestLinesCollectionType()
	{
		var wrapper = new SecurityNctsHeaderDocumentWrapperForTest(nctsHeader);
		AssertType<ITNctsDepartureCargoDescWrapperCollection>("The returned lines collection is ITNctsDepartureCargoDescWrapperCollection", wrapper.GetLinesCore_Exposed());
	}

	protected override BusinessObject GetNewBusinessObject() => SecurityNctsHeaderDocumentWrapper.New(Factory.New<NctsHeader>(), Factory);

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
	}
	NctsHeader nctsHeader;
	NctsDepartureCargoDesc goodsItem;

	class SecurityNctsHeaderDocumentWrapperForTest : SecurityNctsHeaderDocumentWrapper
	{
		public SecurityNctsHeaderDocumentWrapperForTest(NctsHeader nctsHeader) : base(nctsHeader, nctsHeader.Factory)
		{
		}

		public DocBaseWrapperCollection<DocumentWrappers.Customs.EU.NCTS.NctsDepartureCargoDescWrapper> GetLinesCore_Exposed() => base.GetLinesCore();
	}
}
