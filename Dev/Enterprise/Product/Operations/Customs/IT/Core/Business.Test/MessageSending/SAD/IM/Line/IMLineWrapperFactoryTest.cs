using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class IMLineWrapperFactoryTest : TestCaseWithFactory
{
	public void TestFactoryNullArgument()
	{
		AssertExceptionThrown<ArgumentNullException>("GetIMLineWrapper with null value should throw an ArgumentNullException", () => IMLineWrapperFactory.GetIMLineWrapper(null));
	}

	public void TestFactoryHasIntoWarehouseProcedure()
	{
		entryInstruction.CEI_Procedure = "71";
		AssertType<IMWarehouseLineProcedureCodeDependentFieldWrapper>("IMLineWrapperFactory should return type of IMWarehouseLineProcedureCodeDependentFieldWrapper", IMLineWrapperFactory.GetIMLineWrapper(entryLine));
	}

	public void TestFactoryHasNotIntoWarehouseProcedure()
	{
		entryInstruction.CEI_Procedure = "40";
		AssertType<IMNonWarehouseLineProcedureCodeDependentFieldWrapper>("IMLineWrapperFactory should return type of IMNonWarehouseLineProcedureCodeDependentFieldWrapper", IMLineWrapperFactory.GetIMLineWrapper(entryLine));
	}

	protected override void SetUp()
	{
		jobDeclaration = Factory.New<JobDeclaration>();
		entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryLine = entryHeader.MergedLines.AddNew();

		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure40And71ForCurrentCountry();
		Factory.Save();
	}

	JobDeclaration jobDeclaration;
	CusEntryHeader entryHeader;
	CusEntryInstruction entryInstruction;
	CusEntryLine entryLine;
}
