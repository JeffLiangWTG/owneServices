using System;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class CusEntryInstructionInlandTransportModeAssessor
{
	public CusEntryInstructionInlandTransportModeAssessor(CusEntryInstruction entryInstruction)
	{
		this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
		lazyIsMandatory = new Lazy<bool>(GetIsMandatory);
		lazyIsRequiredInCustomsMessage = new Lazy<bool>(GetIsRequiredInCustomsMessage);
		lazyOfficeOfPresentation = new Lazy<ZString>(GetOfficeOfPresentation);
	}

	public bool IsMandatory => lazyIsMandatory.Value;

	public bool IsRequiredInCustomsMessage => lazyIsRequiredInCustomsMessage.Value;

	#region Implementation

	bool GetIsMandatory()
	{
		var declaration = Declaration;
		if (declaration is null || !declaration.IsUCC6)
		{
			return false;
		}

		var presentationOfficeNotEmptyAndNotEqualToExitOffice = PresentationOfficeNotEmptyAndNotEqualToExitOffice;
		var presentationOfficeEmptyAndCustomsOfficeNotEqualToExitOffice = PresentationOfficeEmptyAndCustomsOfficeNotEqualToExitOffice;
		var isEntryStyleExportToSpecialTerritory = IsEntryStyleExportToSpecialTerritory;

		var entryStyleIsNotExportToSpecialTerritoryAndSubStyleNotInTransportModeInlandDependentList = !isEntryStyleExportToSpecialTerritory
			&& !SubStyleIsInTransportModeInlandList;

		var entryStyleIsExportToSpecialTerritoryAndProcedureCodeNotInTransportModeInlandDependentList = isEntryStyleExportToSpecialTerritory
			&& !entryInstruction.CEI_Procedure.In(transportModeInlandDependentEntryProcedureCodes);

		return (presentationOfficeNotEmptyAndNotEqualToExitOffice && entryStyleIsNotExportToSpecialTerritoryAndSubStyleNotInTransportModeInlandDependentList)
			|| (presentationOfficeNotEmptyAndNotEqualToExitOffice && entryStyleIsExportToSpecialTerritoryAndProcedureCodeNotInTransportModeInlandDependentList)
			|| (presentationOfficeEmptyAndCustomsOfficeNotEqualToExitOffice && entryStyleIsNotExportToSpecialTerritoryAndSubStyleNotInTransportModeInlandDependentList)
			|| (presentationOfficeEmptyAndCustomsOfficeNotEqualToExitOffice && entryStyleIsExportToSpecialTerritoryAndProcedureCodeNotInTransportModeInlandDependentList);
	}

	bool GetIsRequiredInCustomsMessage()
	{
		var declaration = Declaration;
		if (declaration is null || !declaration.IsUCC6)
		{
			return false;
		}

		var subStyleIsInTransportModeInlandDependentListOrIsEntryStyleIsCoAndProcedureCodeIs10 = SubStyleIsInTransportModeInlandList
			|| (IsEntryStyleExportToSpecialTerritory && entryInstruction.CEI_Procedure == UniversalReferenceConstants.RefCusProcedureCodes.FinalExport10);

		return (PresentationOfficeNotEmptyAndNotEqualToExitOffice && !subStyleIsInTransportModeInlandDependentListOrIsEntryStyleIsCoAndProcedureCodeIs10)
			|| (PresentationOfficeEmptyAndCustomsOfficeNotEqualToExitOffice && !subStyleIsInTransportModeInlandDependentListOrIsEntryStyleIsCoAndProcedureCodeIs10);
	}

	ZString GetOfficeOfPresentation()
	{
		return Declaration
			.CustomsOffices
			.GetFirstElementHaving(EuOfficeCodesTypes.Codes.OfficeOfPresentation)
			?.CY_Data ?? ZString.Empty;
	}

	JobDeclaration Declaration => entryInstruction.JobDeclaration;
	ZString OfficeOfPresentation => lazyOfficeOfPresentation.Value;
	ZString OfficeOfExit => Declaration.OfficeOfExit;
	bool PresentationOfficeNotEmptyAndNotEqualToExitOffice => !OfficeOfPresentation.IsEmpty && OfficeOfPresentation != OfficeOfExit;
	bool PresentationOfficeEmptyAndCustomsOfficeNotEqualToExitOffice => OfficeOfPresentation.IsEmpty && Declaration.JE_CustomsOffice != OfficeOfExit;
	bool IsEntryStyleExportToSpecialTerritory => Declaration.IsEntryStyleExportToSpecialTerritory;
	bool SubStyleIsInTransportModeInlandList => entryInstruction.CEI_SubStyle.In(transportModeInlandDependentEntrySubStyles);

	readonly CusEntryInstruction entryInstruction;
	readonly Lazy<bool> lazyIsMandatory;
	readonly Lazy<bool> lazyIsRequiredInCustomsMessage;
	readonly Lazy<ZString> lazyOfficeOfPresentation;

	readonly ImmutableArray<ZString> transportModeInlandDependentEntrySubStyles = new ZString[]
	{
		ITEntrySubStyleList.Codes.SimplifiedDeclarationOccasionallyB,
		ITEntrySubStyleList.Codes.SimplifiedDeclarationRegularlyC,
		ITEntrySubStyleList.Codes.PreliminaryStandardDeclarationD,
		ITEntrySubStyleList.Codes.PreliminarySimplifiedDeclarationE,
		ITEntrySubStyleList.Codes.PreliminarySimplifiedDeclarationF,
	}.ToImmutableArray();

	readonly ImmutableArray<ZString> transportModeInlandDependentEntryProcedureCodes = new ZString[]
	{
		UniversalReferenceConstants.RefCusProcedureCodes.FinalExport10,
		UniversalReferenceConstants.RefCusProcedureCodes.PlacingOfGoodsIntoCustomsWarehousingOrIntoFreeZone76,
		UniversalReferenceConstants.RefCusProcedureCodes.StoringAndPlacingOfGoodsUnderCustomsControlWithPreFinancing77,
	}.ToImmutableArray();

	#endregion
}
