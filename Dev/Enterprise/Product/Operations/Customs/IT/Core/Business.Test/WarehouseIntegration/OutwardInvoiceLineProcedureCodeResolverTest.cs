using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Warehouse.Integration;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class OutwardInvoiceLineProcedureCodeResolverTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(
			"When entryInstruction is null",
			() => new OutwardInvoiceLineProcedureCodeResolver(entryInstruction: null, bondedWarehouseAttributeMock.Object));

		AssertExceptionThrown<ArgumentNullException>(
			"When bondedWarehouseAttribute is null",
			() => new OutwardInvoiceLineProcedureCodeResolver(entryInstruction, bondedWarehouseAttribute: null));
	}

	public void TestGetProcedureCode_WhenCEI_ProcedureCodeAndWB_InwardProcedureAreValid()
	{
		var procedureCodeResolver = new OutwardInvoiceLineProcedureCodeResolver(entryInstruction, bondedWarehouseAttributeMock.Object);
		CombineAssertions(() =>
		{
			entryInstruction.CEI_Procedure = "40";

			bondedWarehouseAttributeMock.Setup(x => x.WB_InwardProcedure).Returns("7100");
			AssertEquals("ProcedureCode", "4071", procedureCodeResolver.GetProcedureCode());

			bondedWarehouseAttributeMock.Setup(x => x.WB_InwardProcedure).Returns("00");
			AssertEquals("ProcedureCode", "4000", procedureCodeResolver.GetProcedureCode());
		});
	}

	public void TestGetProcedureCode_WhenCEI_ProcedureIsNotValid()
	{
		var procedureCodeResolver = new OutwardInvoiceLineProcedureCodeResolver(entryInstruction, bondedWarehouseAttributeMock.Object);

		CombineAssertions(() =>
		{
			bondedWarehouseAttributeMock.Setup(x => x.WB_InwardProcedure).Returns("7100");

			entryInstruction.CEI_Procedure = "";
			AssertEquals("When CEI_Procedure is empty, ProcedureCode", "", procedureCodeResolver.GetProcedureCode());

			entryInstruction.CEI_Procedure = "4";
			AssertEquals("When CEI_Procedure is not 2 chars length, ProcedureCode", "", procedureCodeResolver.GetProcedureCode());
		});
	}

	public void TestGetProcedureCode_WhenWB_InwardProcedureIsNotValid()
	{
		var procedureCodeResolver = new OutwardInvoiceLineProcedureCodeResolver(entryInstruction, bondedWarehouseAttributeMock.Object);

		CombineAssertions(() =>
		{
			entryInstruction.CEI_Procedure = "40";

			bondedWarehouseAttributeMock.Setup(x => x.WB_InwardProcedure).Returns("");
			AssertEquals("When WB_InwardProcedure is empty, ProcedureCode", "", procedureCodeResolver.GetProcedureCode());

			bondedWarehouseAttributeMock.Setup(x => x.WB_InwardProcedure).Returns("7");
			AssertEquals("When WB_InwardProcedure is not 2 chars length, ProcedureCode", "", procedureCodeResolver.GetProcedureCode());
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		entryInstruction = Factory.New<CusEntryInstruction>();
		bondedWarehouseAttributeMock = new Mock<IWhsBondedWarehouseAttribute>();
	}

	CusEntryInstruction entryInstruction;
	Mock<IWhsBondedWarehouseAttribute> bondedWarehouseAttributeMock;
}
