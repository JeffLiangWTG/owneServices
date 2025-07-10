using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ETHeaderAgreedLocationOfGoodsWrapperTest : TestCaseWithFactory
{
	public void TestAgreedLocationOfGoodsCode()
	{
		AssertEquals(ZString.Empty, wrapper.AgreedLocationOfGoodsCode);
	}

	public void TestAgreedLocationOfGoodsDescription()
	{
		entryInstruction.ElectronicDocuments = ZBool.False;
		declaration.JE_LocationOfGoods = ZString.Empty;
		AssertEquals(ZString.Empty, wrapper.AgreedLocationOfGoodsDescription);

		declaration.JE_LocationOfGoods = GoodsLocationList.Codes.GoodsAreInCustomsAreaForInspection;
		AssertEquals(ZString.Empty, wrapper.AgreedLocationOfGoodsDescription);

		entryInstruction.ElectronicDocuments = ZBool.True;
		AssertEquals(GoodsLocationList.Codes.GoodsAreInCustomsAreaForInspection, wrapper.AgreedLocationOfGoodsDescription);

		declaration.JE_LocationOfGoods = GoodsLocationList.Codes.GoodsAreOutOfCustomsCircuit;
		AssertEquals(GoodsLocationList.Codes.GoodsAreOutOfCustomsCircuit, wrapper.AgreedLocationOfGoodsDescription);

		declaration.JE_LocationOfGoods = "T";
		AssertEquals(ZString.Empty, wrapper.AgreedLocationOfGoodsDescription);
	}

	public void TestAuthorizedLocationOfGoodsCodeAndCin()
	{
		AssertEquals(ZString.Empty, wrapper.AuthorizedLocationOfGoodsCodeAndCin);

		declaration.JE_LocationOfGoods = GoodsLocationList.Codes.GoodsAreInCustomsAreaForInspection;
		AssertEquals(ZString.Empty, wrapper.AuthorizedLocationOfGoodsCodeAndCin);

		declaration.JE_LocationOfGoods = GoodsLocationList.Codes.GoodsAreOutOfCustomsCircuit;
		AssertEquals(ZString.Empty, wrapper.AuthorizedLocationOfGoodsCodeAndCin);

		declaration.JE_LocationOfGoods = "T";
		AssertEquals("T", wrapper.AuthorizedLocationOfGoodsCodeAndCin);

		entryInstruction.ElectronicDocuments = ZBool.True;
		AssertEquals("T-FE", wrapper.AuthorizedLocationOfGoodsCodeAndCin);
	}

	public void TestCustomsSubPlace()
	{
		AssertEquals(ZString.Empty, wrapper.CustomsSubPlace);
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderAgreedLocationOfGoodsWrapper(null));

		var entryHeader = Factory.New<CusEntryHeader>();
		AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderAgreedLocationOfGoodsWrapper(entryHeader));

		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryHeaders.Add(entryHeader);
		AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderAgreedLocationOfGoodsWrapper(entryHeader));

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		AssertNoExceptionThrown(() => new ETHeaderAgreedLocationOfGoodsWrapper(entryHeader));
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		wrapper = new ETHeaderAgreedLocationOfGoodsWrapper(entryHeader);
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	CusEntryHeader entryHeader;
	ETHeaderAgreedLocationOfGoodsWrapper wrapper;
}
