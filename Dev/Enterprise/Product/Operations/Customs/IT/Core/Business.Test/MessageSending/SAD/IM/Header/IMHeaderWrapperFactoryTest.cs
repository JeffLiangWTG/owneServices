using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IMHeaderWrapperFactoryTest : TestCaseWithFactory
{
	public void TestFactoryNullArgument()
	{
		AssertExceptionThrown<ArgumentNullException>("GetIMHeaderWrapper with null value should throw an ArgumentNullException", () => IMHeaderWrapperFactory.GetIMHeaderWrapper(null));
	}

	public void TestFactoryHasIntoWarehouseProcedure()
	{
		entryInstruction.CEI_Procedure = "71";
		AssertType<IMWarehouseHeaderProcedureWrapper>("IMHeaderWrapperFactory should return type of IMWarehouseHeaderProcedureWrapper", IMHeaderWrapperFactory.GetIMHeaderWrapper(entryHeader));
	}

	public void TestFactoryHasNotIntoWarehouseProcedure()
	{
		entryInstruction.CEI_Procedure = "40";
		AssertType<IMNonWarehouseHeaderProcedureWrapper>("IMHeaderWrapperFactory should return type of IMNonWarehouseHeaderProcedureWrapper", IMHeaderWrapperFactory.GetIMHeaderWrapper(entryHeader));
	}

	protected override void SetUp()
	{
		jobDeclaration = Factory.New<JobDeclaration>();
		entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure40And71ForCurrentCountry();
		Factory.Save();
	}

	JobDeclaration jobDeclaration;
	CusEntryHeader entryHeader;
	CusEntryInstruction entryInstruction;
}
