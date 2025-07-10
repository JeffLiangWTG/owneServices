using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.UniversalReference;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business;

public class RefCusIntoProcedureWrapper
{
	RefCusIntoProcedureWrapper(RefCusProcedure refCusProcedure)
	{
		this.refCusProcedure = Argument.NotNull(refCusProcedure, nameof(refCusProcedure));
	}
	readonly RefCusProcedure refCusProcedure;

	public static RefCusIntoProcedureWrapper GetNewIfProcedureCodeIsValid(IProcedureCodeProvider procedureCodeProvider, BusinessObjectFactory factory)
	{
		Argument.NotNull(procedureCodeProvider, nameof(procedureCodeProvider));
		Argument.NotNull(factory, nameof(factory));

		var procedureCode = procedureCodeProvider.ProcedureCode;
		var countryCode = procedureCodeProvider.CountryCode;

		RefCusIntoProcedureWrapper intoProcedureWrapper = null;
		if (!procedureCode.IsEmpty && !countryCode.IsEmpty)
		{
			var cusProcedure = new RefCusProcedure.Loader(factory).LoadTop1FromFullCodeCurrentPlusPreviousPlusConcession(procedureCode, countryCode, ZDateTime.Today);
			if (cusProcedure != null)
			{
				intoProcedureWrapper = new RefCusIntoProcedureWrapper(cusProcedure);
			}
		}
		return intoProcedureWrapper;
	}

	public ZString ProcedureCode => refCusProcedure.ZZ6_ProcedureCode;

	public ZBool IsIntoWarehouse => (isIntoWarehouse ?? (isIntoWarehouse = refCusProcedure.IsIntoWarehouse())).Value;
	ZBool? isIntoWarehouse;
	public ZBool IsIntoInwardProcessing => (isIntoInwardProcessing ?? (isIntoInwardProcessing = refCusProcedure.IsIntoInwardProcessing())).Value;
	ZBool? isIntoInwardProcessing;
	public ZBool IsIntoOutwardProcessing => (isIntoOutwardProcessing ?? (isIntoOutwardProcessing = refCusProcedure.IsIntoOutwardProcessing())).Value;
	ZBool? isIntoOutwardProcessing;
	public ZBool IsReimportProcedure => (isReimportProcedure ?? (isReimportProcedure = refCusProcedure.IsReimportProcedure())).Value;
	ZBool? isReimportProcedure;
	public ZBool IsIntoTemporaryProcedure => (isIntoTemporaryProcedure ?? (isIntoTemporaryProcedure = refCusProcedure.IsIntoTemporaryProcedure())).Value;
	ZBool? isIntoTemporaryProcedure;
	public ZBool IsIntoTemporaryExportProcedure => (isIntoTemporaryExportProcedure ?? (isIntoTemporaryExportProcedure = refCusProcedure.IsIntoTemporaryExport())).Value;
	ZBool? isIntoTemporaryExportProcedure;
	public ZBool IsIntoTemporaryImportProcedure => (isIntoTemporaryImportProcedure ?? (isIntoTemporaryImportProcedure = refCusProcedure.IsIntoTemporaryImport())).Value;
	ZBool? isIntoTemporaryImportProcedure;
	public ZBool IsIntoWarehouseForReExport => (isIntoWarehouseForReExport ?? (isIntoWarehouseForReExport = GetIsIntoWarehouseForReExport())).Value;
	ZBool? isIntoWarehouseForReExport;
	public ZBool IsCalculateVAT => refCusProcedure.ZZ6_CalculateVAT;

	ZBool GetIsIntoWarehouseForReExport()
	{
		var procedureCode = refCusProcedure.ZZ6_ProcedureCode;
		return procedureCode == IntoWarehouseForReExportAsIs || procedureCode == IntoWarehouseForReExportAfterBeenWorked;
	}

	const string IntoWarehouseForReExportAsIs = "76";
	const string IntoWarehouseForReExportAfterBeenWorked = "77";
}

public interface IProcedureCodeProvider
{
	ZString ProcedureCode { get; }
	ZString CountryCode { get; }
}
