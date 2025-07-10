using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IMHeaderLocationOfGoodWrapperTest : TestCaseWithFactory
{
	public void TestCodeAndCinPlaceOfExamination()
	{
		AssertEquals(ZString.Empty, locationOfGoodWrapper.CodeAndCinPlaceOfExamination);
		jobDeclaration.JE_LocationOfGoods = "30LOC";
		entryInstruction.ElectronicDocuments = ZBool.True;
		AssertEquals("30LOC-FE", locationOfGoodWrapper.CodeAndCinPlaceOfExamination);
		entryInstruction.ElectronicDocuments = ZBool.False;
		AssertEquals("30LOC", locationOfGoodWrapper.CodeAndCinPlaceOfExamination);
		jobDeclaration.JE_LocationOfGoods = ZString.Empty;
		entryInstruction.ElectronicDocuments = ZBool.True;
		AssertEquals("FE", locationOfGoodWrapper.CodeAndCinPlaceOfExamination);
		jobDeclaration.JE_LocationOfGoods = " ";
		entryInstruction.ElectronicDocuments = ZBool.True;
		AssertEquals("FE", locationOfGoodWrapper.CodeAndCinPlaceOfExamination);
	}

	public void TestCodeAndCinPlaceOfUnloading()
	{
		AssertEquals(ZString.Empty, locationOfGoodWrapper.CodeAndCinPlaceOfUnloading);
		jobDeclaration.JE_SubLocationOfGoods = "30.2UNLOAD";
		AssertEquals("30.2UNLOAD", locationOfGoodWrapper.CodeAndCinPlaceOfUnloading);
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new IMHeaderLocationOfGoodWrapper(null));
		AssertNoExceptionThrown(() => new IMHeaderLocationOfGoodWrapper(entryHeader));
	}

	protected override void SetUp()
	{
		base.SetUp();

		jobDeclaration = Factory.New<JobDeclaration>();
		entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		locationOfGoodWrapper = new IMHeaderLocationOfGoodWrapper(entryHeader);
	}
	CusEntryHeader entryHeader;
	JobDeclaration jobDeclaration;
	CusEntryInstruction entryInstruction;
	IMHeaderLocationOfGoodWrapper locationOfGoodWrapper;
}
