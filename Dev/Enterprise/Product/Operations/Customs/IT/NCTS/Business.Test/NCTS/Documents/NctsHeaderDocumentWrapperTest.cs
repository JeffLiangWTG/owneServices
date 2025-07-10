using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.DocumentWrappers;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderDocumentWrapper))]
sealed class NctsHeaderDocumentWrapperTest : NonPersistentBusinessObjectTestCase
{
	public void TestNew()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

		AssertExceptionThrown<ArgumentNullException>("Expected exception when NctsHeader parameter is null", () => NctsHeaderDocumentWrapper.New(null, Factory));
		AssertExceptionThrown<ArgumentNullException>("Expected exception when Factory parameter is null", () => NctsHeaderDocumentWrapper.New(header, null));

		AssertNotNull("Instance of NctsHeaderDocumentWrapper expected", NctsHeaderDocumentWrapper.New(header, Factory));
	}

	public void TestDocumentWrapperTypeDependingOnIsSecurityDeclaration()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.BH_FTZMove = true;
		NCTSTestHelper.CreateJobDocAddressForTest(Factory, "COS", header.SecurityConsignor);

		CombineAssertions(() =>
		{
			AssertType<SecurityNctsHeaderDocumentWrapper>("When IsSecurityDeclaration is true then returned type is SecurityNctsHeaderDocumentWrapper", NctsHeaderDocumentWrapper.New(header, header.Factory));

			header.BH_FTZMove = false;
			AssertType<NctsHeaderDocumentWrapper>("When IsSecurityDeclaration is false then returned type is NctsHeaderDocumentWrapper", NctsHeaderDocumentWrapper.New(header, header.Factory));
		});
	}

	public void TestBox35GrossMass()
	{
		CombineAssertions(() =>
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var wrapper = NctsHeaderDocumentWrapper.New(nctsHeader, Factory);
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

	public void TestBox38NetMass()
	{
		CombineAssertions(() =>
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var wrapper = NctsHeaderDocumentWrapper.New(nctsHeader, Factory);
			AssertEquals(nameof(wrapper.BOX38NETTMASS), "", wrapper.BOX38NETTMASS);

			goodsItem.BY_NetWeight = 30;
			goodsItem.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals(nameof(wrapper.BOX38NETTMASS), "30", wrapper.BOX38NETTMASS);

			goodsItem.BY_NetWeight = 123.4678m;
			goodsItem.BY_NetWeightUnit = Core.Constants.Weight.Grams;
			AssertEquals(nameof(wrapper.BOX38NETTMASS), "0.12347", wrapper.BOX38NETTMASS);

			goodsItem.BY_NetWeight = 99.123456m;
			goodsItem.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
			AssertEquals(nameof(wrapper.BOX38NETTMASS), "99.12346", wrapper.BOX38NETTMASS);
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		return NctsHeaderDocumentWrapper.New(nctsHeader, Factory);
	}

	public void TestLinesCollectionType()
	{
		var header = Factory.New<NctsHeader>();

		var wrapper = new NctsHeaderDocumentWrapperForTest(header);
		AssertType<ITNctsDepartureCargoDescWrapperCollection>("The returned lines collection is ITNctsDepartureCargoDescWrapperCollection", wrapper.GetLinesCore_Exposed());
	}

	public void TestGoodsItemsWithDeclarationItemNumber()
	{
		var header = Factory.New<NctsHeader>();

		header.SetMovementType(NctsMovementType.Codes.Departure);
		var bill1 = header.Bills.AddNew();
		bill1.B0_ReferenceID = "sbb1";
		bill1.SequenceNumber = 2;
		var bill1Item1 = bill1.GoodsItems.AddNew();
		bill1Item1.BY_LineNo = 11;
		bill1Item1.BY_DeclarationGoodsItemNumber = 40;
		bill1Item1.BY_Description = "gi4";
		var bill1Item2 = bill1.GoodsItems.AddNew();
		bill1Item2.BY_LineNo = 6;
		bill1Item2.BY_DeclarationGoodsItemNumber = 30;
		bill1Item2.BY_Description = "gi3";

		var bill2 = header.Bills.AddNew();
		bill2.B0_ReferenceID = "sbb2";
		bill2.SequenceNumber = 1;
		var bill2Item1 = bill2.GoodsItems.AddNew();
		bill2Item1.BY_LineNo = 10;
		bill2Item1.BY_DeclarationGoodsItemNumber = 20;
		bill2Item1.BY_Description = "gi2";
		var bill2Item2 = bill2.GoodsItems.AddNew();
		bill2Item2.BY_LineNo = 5;
		bill2Item2.BY_DeclarationGoodsItemNumber = 50;
		bill2Item2.BY_Description = "gi1";

		Factory.Save();
		var wrapper = NctsHeaderDocumentWrapper.New(header, Factory);
		var lines = wrapper.Lines;

		AssertEquals(4, lines.Count);
		CombineAssertions(() =>
		{
			AssertEquals("BOX32ITEM is BY_DeclarationGoodsItemNumber", "20", lines[0].BOX32ITEM);
			AssertEquals("BOX314DESCRIPTION is BY_Description", "gi2", lines[0].BOX314DESCRIPTION);
			AssertEquals("BOX32ITEM is BY_DeclarationGoodsItemNumber", "30", lines[1].BOX32ITEM);
			AssertEquals("BOX314DESCRIPTION is BY_Description", "gi3", lines[1].BOX314DESCRIPTION);
			AssertEquals("BOX32ITEM is BY_DeclarationGoodsItemNumber", "40", lines[2].BOX32ITEM);
			AssertEquals("BOX314DESCRIPTION is BY_Description", "gi4", lines[2].BOX314DESCRIPTION);
			AssertEquals("BOX32ITEM is BY_DeclarationGoodsItemNumber", "50", lines[3].BOX32ITEM);
			AssertEquals("BOX314DESCRIPTION is BY_Description", "gi1", lines[3].BOX314DESCRIPTION);
		});
	}

	public void TestGoodsItemsWithoutDeclarationItemNumber()
	{
		var header = Factory.New<NctsHeader>();

		header.SetMovementType(NctsMovementType.Codes.Departure);
		var bill1 = header.Bills.AddNew();
		bill1.B0_ReferenceID = "sbb1";
		bill1.SequenceNumber = 2;
		var bill1Item1 = bill1.GoodsItems.AddNew();
		bill1Item1.BY_LineNo = 11;
		bill1Item1.BY_DeclarationGoodsItemNumber = 40;
		bill1Item1.BY_Description = "gi4";
		var bill1Item2 = bill1.GoodsItems.AddNew();
		bill1Item2.BY_LineNo = 6;
		bill1Item2.BY_DeclarationGoodsItemNumber = 30;
		bill1Item2.BY_Description = "gi3";

		var bill2 = header.Bills.AddNew();
		bill2.B0_ReferenceID = "sbb2";
		bill2.SequenceNumber = 1;
		var bill2Item1 = bill2.GoodsItems.AddNew();
		bill2Item1.BY_LineNo = 10;
		bill2Item1.BY_DeclarationGoodsItemNumber = 20;
		bill2Item1.BY_Description = "gi2";
		var bill2Item2 = bill2.GoodsItems.AddNew();
		bill2Item2.BY_LineNo = 5;
		bill2Item2.BY_DeclarationGoodsItemNumber = 0;
		bill2Item2.BY_Description = "gi1";

		Factory.Save();
		var wrapper = NctsHeaderDocumentWrapper.New(header, Factory);
		var lines = wrapper.Lines;

		AssertEquals(4, lines.Count);
		CombineAssertions(() =>
		{
			AssertEquals("BOX32ITEM is a generated progressive number", "1", lines[0].BOX32ITEM);
			AssertEquals("BOX314DESCRIPTION is BY_Description", "gi1", lines[0].BOX314DESCRIPTION);
			AssertEquals("BOX32ITEM is a generated progressive number", "2", lines[1].BOX32ITEM);
			AssertEquals("BOX314DESCRIPTION is BY_Description", "gi2", lines[1].BOX314DESCRIPTION);
			AssertEquals("BOX32ITEM is a generated progressive number", "3", lines[2].BOX32ITEM);
			AssertEquals("BOX314DESCRIPTION is BY_Description", "gi3", lines[2].BOX314DESCRIPTION);
			AssertEquals("BOX32ITEM is a generated progressive number", "4", lines[3].BOX32ITEM);
			AssertEquals("BOX314DESCRIPTION is BY_Description", "gi4", lines[3].BOX314DESCRIPTION);
		});
	}

	public void TestBoxDResult()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		var wrapper = NctsHeaderDocumentWrapper.New(nctsHeader, Factory);

		var entryNum = CusEntryNumber.LoadOrCreate(nctsHeader, "CLR", Core.Constants.CountryCodes.Italy);
		entryNum.CE_EntryNum = "12345";

		AssertEquals("When ReleaseCode exists, BoxDResult is CODICE SVINCOLO plus CE_EntryNum", "CODICE SVINCOLO 12345", wrapper.BOXDRESULT);

		entryNum.CE_EntryNum = "";

		AssertEquals("When ReleaseCode does not exist, BoxDResult is empty", "", wrapper.BOXDRESULT);
	}

	class NctsHeaderDocumentWrapperForTest : NctsHeaderDocumentWrapper
	{
		public NctsHeaderDocumentWrapperForTest(NctsHeader nctsHeader) : base(nctsHeader, nctsHeader.Factory)
		{
		}

		public DocBaseWrapperCollection<DocumentWrappers.Customs.EU.NCTS.NctsDepartureCargoDescWrapper> GetLinesCore_Exposed() => base.GetLinesCore();
	}
}
