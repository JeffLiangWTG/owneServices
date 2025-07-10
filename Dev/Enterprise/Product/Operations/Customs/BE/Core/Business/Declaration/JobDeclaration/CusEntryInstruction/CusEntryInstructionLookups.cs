using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration;

public class CusEntryInstructionLookups : EU.Business.Declaration.CusEntryInstructionLookups
{
	public CusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction) : base(cusEntryInstruction)
	{
	}

	public new CusEntryInstruction Parent => (CusEntryInstruction)base.Parent;

	public override CodeDescriptionPairList EntrySubStyleList
	{
		get
		{
			var style = Parent.CEI_Style;
			return Factory.GetCachedValue($"BE.EntrySubStyleList.CodeList|Style|{style}", () => GetEntrySubStyleList(style));
		}
	}

	static CodeDescriptionPairList GetEntrySubStyleList(ZString style)
	{
		var result = new EntrySubStyleList();
		if (style == ExportCusEntryInstructionsDeclarationTypeList.Codes.ExportAndReExport
			|| style == ExportCusEntryInstructionsDeclarationTypeList.Codes.OutwardProcessing
			|| style == ExportCusEntryInstructionsDeclarationTypeList.Codes.CustomsWarehousingOfUnionGoods
			|| style == ExportCusEntryInstructionsDeclarationTypeList.Codes.DispatchOfGoodsForTradeWithSpecialTerritories
			|| style == ExportCusEntryInstructionsDeclarationTypeList.Codes.ExportSimplified
			|| style == ImportCusEntryInstructionsDeclarationTypeList.Codes.FreeCirculation
			|| style == ImportCusEntryInstructionsDeclarationTypeList.Codes.CustomsWarehousing
			|| style == ImportCusEntryInstructionsDeclarationTypeList.Codes.TemporaryAdmission
			|| style == ImportCusEntryInstructionsDeclarationTypeList.Codes.InwardProcessing
			|| style == ImportCusEntryInstructionsDeclarationTypeList.Codes.ProbablyNoControlRequired
			|| style == ImportCusEntryInstructionsDeclarationTypeList.Codes.ImportSimplified)
		{
			if (style != ExportCusEntryInstructionsDeclarationTypeList.Codes.ExportSimplified
				&& style != ImportCusEntryInstructionsDeclarationTypeList.Codes.ImportSimplified)
			{
				result.RemoveCode(Business.EntrySubStyleList.Codes.SimplifiedDeclarationOccasionalBasis);
				result.RemoveCode(Business.EntrySubStyleList.Codes.SimplifiedDeclarationRegularUse);
				result.RemoveCode(Business.EntrySubStyleList.Codes.PreliminarySimplifiedDeclarationUnderCodeB);
				result.RemoveCode(Business.EntrySubStyleList.Codes.PreliminarySimplifiedDeclarationUnderCodeC);
				result.RemoveCode(Business.EntrySubStyleList.Codes.SupplementaryDeclarationForDeclarationsBOrE);
				result.RemoveCode(Business.EntrySubStyleList.Codes.SupplementaryDeclarationForDeclarationsCOrF);
				result.RemoveCode(Business.EntrySubStyleList.Codes.SupplementaryDeclarationOfSimplifiedDeclarationsCoveredByCAndF);
			}
			else
			{
				result.RemoveCode(Business.EntrySubStyleList.Codes.StandardDeclaration);
				result.RemoveCode(Business.EntrySubStyleList.Codes.PreliminaryStandardDeclarationUnderCodeA);
				result.RemoveCode(Business.EntrySubStyleList.Codes.ComprehensiveSupplementaryDeclarationAccordingToArticle182OfTheUcc);
				result.RemoveCode(Business.EntrySubStyleList.Codes.SupplementaryDeclarationOfGeneralOrPeriodicNatureArticle182);
			}

			if (style != ExportCusEntryInstructionsDeclarationTypeList.Codes.ExportAndReExport
				&& style != ExportCusEntryInstructionsDeclarationTypeList.Codes.DispatchOfGoodsForTradeWithSpecialTerritories)
			{
				result.RemoveCode(Business.EntrySubStyleList.Codes.RetrospectiveLodgementOfAnExportReExportDeclaration);
			}
		}
		return result;
	}
}
