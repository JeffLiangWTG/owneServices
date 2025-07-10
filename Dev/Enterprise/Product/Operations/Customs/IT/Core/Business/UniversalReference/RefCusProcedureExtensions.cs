using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business.UniversalReference;

public static class RefCusProcedureExtensions
{
	public static ZBool HasEmptyPreviousProcedure(this RefCusProcedure procedure) => (procedure?.ZZ6_PreviousProcedureCode ?? ZString.Empty) == NoPreviousProcedure;
	public static ZBool IsReimportProcedure(this RefCusProcedure procedure) => (procedure?.ZZ6_ProcedureCode ?? ZString.Empty) == Reimport;
	public static ZBool IsIntoTemporaryProcedure(this RefCusProcedure procedure)
	{
		var isTemporaryProcedure = ZBool.False;
		if (procedure != null)
		{
			isTemporaryProcedure = procedure.IsIntoTemporaryImport() || procedure.IsIntoTemporaryExport();
		}
		return isTemporaryProcedure;
	}

	const string NoPreviousProcedure = "00";
	const string Reimport = "61";
}
