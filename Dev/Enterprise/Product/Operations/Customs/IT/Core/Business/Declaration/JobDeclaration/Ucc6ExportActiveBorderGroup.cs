using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

class Ucc6ExportActiveBorderGroup
{
	public Ucc6ExportActiveBorderGroup(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}

	readonly JobDeclaration declaration;

	internal bool HasEntryStyleExportNormalAndHasAllowedProcedureCodes()
		=> HasEntryStyleExportNormal && HasAnyEntryInstructionWithProcedureCodeFromList(procedureCodesToBeConsideredForExportNormal);

	internal bool HasEntryStyleExportNormalAndHasNotAllowedTransportMode()
		=> HasEntryStyleExportNormal && HasTransportModeFromList(transportModesToIgnoreForExportNormal);

	internal bool HasEntryStyleExportToSpecialTerritoryAndHasAllowedProcedureCodes()
		=> HasEntryStyleExportToSpecialTerritory && HasAnyEntryInstructionWithProcedureCodeFromList(procedureCodesToBeConsideredForExportToSpecialTerritory);

	internal bool HasEntryStyleExportToSpecialAndHasNotAllowedTransportMode()
		=> HasEntryStyleExportToSpecialTerritory && HasTransportModeFromList(transportModesToIgnoreForExportToSpecialTerritory);

	internal bool AreAllFieldsEmptyOrFilled()
	{
		var activeBorderGroupFieldsWithoutTransportMediumFields = GetAllActiveBorderGroupFieldsWithoutTransportMediumFields().ToArray();
		var transportMediumFields = GetActiveBorderTransportMediumFields().ToArray();
		var allFields = activeBorderGroupFieldsWithoutTransportMediumFields.Union(transportMediumFields);

		return allFields.All(f => f.Value.IsEmpty)
				|| (activeBorderGroupFieldsWithoutTransportMediumFields.All(f => !f.Value.IsEmpty) && transportMediumFields.Any(f => !f.Value.IsEmpty));
	}

	internal bool IsPropertyInTransportMediumGroupAndAreOtherTransportMediumFieldsFilled(string propertyName)
	{
		Argument.NotNullOrEmpty(propertyName, nameof(propertyName));

		var transportMediumFields = GetActiveBorderTransportMediumFields().ToArray();
		return transportMediumFields.Any(t => t.Name == propertyName)
				&& transportMediumFields.Length > 1
				&& transportMediumFields.Where(p => p.Name != propertyName).All(t => !t.Value.IsEmpty);
	}

	protected virtual IEnumerable<CusEntryInstruction> GetEntryInstructions()
	{
		return declaration.CustomsEntryInstructions;
	}

	#region Implementation

	bool HasEntryStyleExportNormal => declaration.JE_EntryStyle == EntryStyleListExportUCC.Codes.ExportNormal;

	bool HasEntryStyleExportToSpecialTerritory => declaration.JE_EntryStyle == EntryStyleListExportUCC.Codes.ExportToSpecialTerritory;

	bool HasAnyEntryInstructionWithProcedureCodeFromList(ImmutableArray<ZString> procedureCodes)
	{
		var entryInstructionsWithProcedureCodes = GetEntryInstructions().ToArray();
		return entryInstructionsWithProcedureCodes
			.Where(e => !e.CEI_Procedure.IsEmpty)
			.Any(e => e.CEI_Procedure.In(procedureCodes));
	}

	bool HasTransportModeFromList(ImmutableArray<ZString> transportModes)
	{
		var transportMode = declaration.JE_TransportMode;
		return !transportMode.IsEmpty && transportMode.In(transportModes);
	}

	IEnumerable<ZPropertyInfo> GetAllActiveBorderGroupFieldsWithoutTransportMediumFields()
	{
		yield return declaration.JE_RN_NKTransportNationalityInfo;
		yield return declaration.ZG_BorderTransportMeansInfo;
	}

	IEnumerable<ZPropertyInfo> GetActiveBorderTransportMediumFields()
	{
		if (!declaration.IsAir)
		{
			yield return declaration.JE_VesselNameInfo;
		}

		if (declaration.IsAir || declaration.IsSea)
		{
			yield return declaration.JE_VoyageFlightNoInfo;
		}
	}

	readonly ImmutableArray<ZString> procedureCodesToBeConsideredForExportToSpecialTerritory = new ZString[]
	{
		UniversalReferenceConstants.RefCusProcedureCodes.PlacingOfGoodsIntoCustomsWarehousingOrIntoFreeZone76,
		UniversalReferenceConstants.RefCusProcedureCodes.StoringAndPlacingOfGoodsUnderCustomsControlWithPreFinancing77,
	}.ToImmutableArray();
	readonly ImmutableArray<ZString> transportModesToIgnoreForExportToSpecialTerritory = new ZString[]
	{
		TransportTypeList.Codes.Mail,
		TransportTypeList.Codes.FixedTransportInstallations,
	}.ToImmutableArray();
	readonly ImmutableArray<ZString> procedureCodesToBeConsideredForExportNormal = new ZString[]
	{
		UniversalReferenceConstants.RefCusProcedureCodes.FinalExport10,
		UniversalReferenceConstants.RefCusProcedureCodes.CompensatingProductsExport11,
		UniversalReferenceConstants.RefCusProcedureCodes.TemporaryGoodsExportToBeReintegratedAsTheyAre23,
		UniversalReferenceConstants.RefCusProcedureCodes.GoodsReExport31,
	}.ToImmutableArray();
	readonly ImmutableArray<ZString> transportModesToIgnoreForExportNormal = new ZString[]
	{
		TransportTypeList.Codes.Rail,
		TransportTypeList.Codes.Mail,
		TransportTypeList.Codes.FixedTransportInstallations,
	}.ToImmutableArray();

	#endregion
}
