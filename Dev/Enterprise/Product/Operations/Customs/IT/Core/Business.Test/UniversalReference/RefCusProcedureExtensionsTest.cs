using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business.UniversalReference.Testing;

sealed class RefCusProcedureExtensionsTest : TestCaseWithFactory
{
	public void TestHasNoPreviousProcedure()
	{
		AssertEquals("HasNoPreviousProcedure, False when input param is null", ZBool.False, (null as RefCusProcedure).HasEmptyPreviousProcedure());

		var procedure = Factory.New<RefCusProcedure>();

		procedure.ZZ6_PreviousProcedureCode = "10";
		AssertEquals("HasNoPreviousProcedure, False when ProcedureCode is not 00", ZBool.False, procedure.HasEmptyPreviousProcedure());

		procedure.ZZ6_PreviousProcedureCode = "00";
		AssertEquals("HasNoPreviousProcedure, True when ProcedureCode is 00", ZBool.True, procedure.HasEmptyPreviousProcedure());
	}

	public void TestIsReimportProcedure()
	{
		AssertEquals("IsReimportProcedure, False when input param is null", ZBool.False, (null as RefCusProcedure).IsReimportProcedure());

		var procedure = Factory.New<RefCusProcedure>();

		procedure.ZZ6_ProcedureCode = "10";
		AssertEquals("IsReimportProcedure, False when ProcedureCode is not 63", ZBool.False, procedure.IsReimportProcedure());

		procedure.ZZ6_ProcedureCode = "61";
		AssertEquals("IsReimportProcedure, True when ProcedureCode is 63", ZBool.True, procedure.IsReimportProcedure());
	}

	public void TestIsIntoTemporaryProcedure()
	{
		AssertEquals("IntoTemporaryProcedure, False when input param is null", ZBool.False, (null as RefCusProcedure).IsIntoTemporaryProcedure());

		var procedure = Factory.New<RefCusProcedure>();

		procedure.ZZ6_IntoTemporaryImport = "N";
		AssertEquals("IsIntoTemporaryProcedure, False when both IntoTemporaryImport and IntoTemporaryExport are false", ZBool.False, procedure.IsIntoTemporaryProcedure());

		procedure.ZZ6_IntoTemporaryExport = "Y";
		AssertEquals("IsIntoTemporaryProcedure, True when one of IntoTemporaryImport or IntoTemporaryExport are true", ZBool.True, procedure.IsIntoTemporaryProcedure());

		procedure.ZZ6_IntoTemporaryImport = "Y";
		procedure.ZZ6_IntoTemporaryExport = "N";
		AssertEquals("IsIntoTemporaryProcedure, True when one of IntoTemporaryImport or IntoTemporaryExport are true", ZBool.True, procedure.IsIntoTemporaryProcedure());
	}
}
