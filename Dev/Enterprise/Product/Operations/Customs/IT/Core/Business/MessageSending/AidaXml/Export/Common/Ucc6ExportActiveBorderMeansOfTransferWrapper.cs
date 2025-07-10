using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

static class Ucc6ExportActiveBorderMeansOfTransferWrapper
{
	internal static IMeansOfTransport GetNewOrNull(JobDeclaration declaration, CusEntryInstruction entryInstruction)
	{
		Argument.NotNull(declaration, nameof(declaration));
		Argument.NotNull(entryInstruction, nameof(entryInstruction));

		if (declaration.IsTransitionPeriodAES30 || entryInstruction.CEI_SubStyle.In(subStylesToBeIgnored))
		{
			return null;
		}

		var ucc6ActiveBorderGroup = new Ucc6ExportActiveBorderGroupWithEntryInstruction(declaration, entryInstruction);
		var hasExportNormalWithAllowedProcedureCode = ucc6ActiveBorderGroup.HasEntryStyleExportNormalAndHasAllowedProcedureCodes();
		var hasExportToSpecialTerritoryWithAllowedProcedureCodes = ucc6ActiveBorderGroup.HasEntryStyleExportToSpecialTerritoryAndHasAllowedProcedureCodes();
		var hasExportToSpecialTerritoryWithRestrictedTransportModes = ucc6ActiveBorderGroup.HasEntryStyleExportToSpecialAndHasNotAllowedTransportMode();

		return hasExportNormalWithAllowedProcedureCode || (hasExportToSpecialTerritoryWithAllowedProcedureCodes && !hasExportToSpecialTerritoryWithRestrictedTransportModes)
			? BorderMeansOfTransportWrapper.NewOrNull(declaration)
			: null;
	}

	static readonly ImmutableArray<ZString> subStylesToBeIgnored = new ZString[]
	{
		ITEntrySubStyleList.Codes.SimplifiedDeclarationOccasionallyB,
		ITEntrySubStyleList.Codes.SimplifiedDeclarationRegularlyC,
		ITEntrySubStyleList.Codes.PreliminarySimplifiedDeclarationE,
		ITEntrySubStyleList.Codes.PreliminarySimplifiedDeclarationF,
	}.ToImmutableArray();
}
