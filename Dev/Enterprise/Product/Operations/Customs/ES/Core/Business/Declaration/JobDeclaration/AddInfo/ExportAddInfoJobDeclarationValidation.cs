using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.Declaration;

public partial class ExportJobDeclarationValidation
{
	protected JobDeclaration JobDeclaration => Parent;

	protected override void CheckJE_AgreedPlaceCode()
	{
		base.CheckJE_AgreedPlaceCode();
		if (Parent.ZG_AgreedPlaceCodeValidationSupport)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_AgreedPlaceCodeInfo);
		}
	}

	protected override void CheckJE_IsSecurityDeclaration()
	{
		base.CheckJE_IsSecurityDeclaration();

		var hasUcc6ExportEntryHeader = JobDeclaration.CustomsEntryHeaders.Any(x => x.IsExportUCC6 && (x.EntryInstruction?.IsSubStyleAOrBOrCOrYOrZ ?? false));

		var exnoseguList = Universal.RefCusCodeListTypes.GetCachedList(JobDeclaration.Factory, Core.Constants.CountryCodes.Spain, UniversalReferenceConstants.RefCusCodeListTypes.EXNOSEGU, ZDate.Today);
		if (!Parent.ZG_IsSecurityDeclaration && hasUcc6ExportEntryHeader && !exnoseguList.ContainsCode(JobDeclaration.JE_GoodsDestination))
		{
			Parent.ZG_IsSecurityDeclarationInfo.AddWarning(Res.GetString("A9E9BB7E-653A-4CBB-83F9-BFB6F61E04F2", "Export AES: If country of destination is not in EXNOSEGU, Security data (EXS) must be submitted. Security checkbox should be ticked."));
		}
	}

	protected override void CheckJE_CTStatusID()
	{
		base.CheckJE_CTStatusID();

		var hasUcc6ExportEntryHeader = JobDeclaration.CustomsEntryHeaders.Any(x => x.IsExportUCC6);

		if (hasUcc6ExportEntryHeader && Parent.ZG_CTStatusID == ExportCommunityTransitStatusList.Codes.T2L && (JobDeclaration.JE_GoodsDestination != Core.Constants.CountryCodes.Andorra || JobDeclaration.HasAnyCPCNotStartWithList(new List<ZString> { "10", "21" })))
		{
			Parent.ZG_CTStatusIDInfo.AddWarning(Res.GetString("54FD4F60-7A67-442A-9958-71FA77013FA7", "For Export AES declarations, if T2L value is selected, country of destination must be Andorra and all Procedure Code in entry lines must start with 10 or 21."));
		}
		else if (hasUcc6ExportEntryHeader && Parent.ZG_CTStatusID == ExportCommunityTransitStatusList.Codes.T2LF && (JobDeclaration.JE_GoodsDestination != Core.Constants.CountryCodes.Spain || JobDeclaration.JE_EntryStyle != EntryStyleListExport.Codes.ExportToSpecialTerritory || JobDeclaration.HasAnyCPCNotStartWithList(new List<ZString> { "10", "21", "23" })))
		{
			Parent.ZG_CTStatusIDInfo.AddWarning(Res.GetString("B550C2D4-F3C9-45F4-8250-0C8100B5FB5D", "For Export AES declarations, if T2LF value is selected, country of destination must be Spain (Canary Islands) and all Procedure Code in entry lines must start with 10, 21 or 23 and Entry Style must be CO."));
		}

		if (Parent.ZG_CTStatusID.IsEmpty && MessageVersionRegistryProvider.IsT2LAndAnyVersionPOUS() && HasAnyT2lInEntryInstructions)
		{
			Parent.ZG_CTStatusIDInfo.AddMessageError(Res.GetString("61441317-5EB2-45A9-BB11-6285EB28F48D", "CT Status is required for T2L request"));
		}
	}

	protected override void CheckJE_LCPDepart()
	{
		base.CheckJE_LCPDepart();

		if (JobDeclaration.CustomsEntryHeaders.Any(x => x.IsExportUCC6 && (IsCusEntryHeaderSubStyleYOrZ(x) || IsCusEntryHeaderSubStyleCAndMRNNotEmpty(x))))
		{
			if (JobDeclaration.ZG_LCPDepart.IsEmpty)
			{
				JobDeclaration.ZG_LCPDepartInfo.AddMessageError(Res.GetString("AAA4AEB3-C24A-4CE4-9885-7DE65E6B2C79", "If exists any declaration with sub style Y or Z, EIDR date is mandatory."));
			}
			else if (JobDeclaration.ZG_LCPDepart.IsInTheFuture())
			{
				JobDeclaration.ZG_LCPDepartInfo.AddMessageError(Res.GetString("BD0E4F0F-0FA9-45A6-AB45-EAE15AC3AB93", "EIDR date cannot be higher to current date."));
			}
		}

		bool IsCusEntryHeaderSubStyleYOrZ(CusEntryHeader x) => x.EntryInstruction?.IsSubStyleYOrZ ?? false;

		bool IsCusEntryHeaderSubStyleCAndMRNNotEmpty(CusEntryHeader x)
		{
			return (x.EntryInstruction?.CEI_SubStyle ?? ZString.Empty) == EntrySubStyleList.Codes.C && !x.MovementReferenceNumber.IsEmpty;
		}
	}

	bool HasAnyT2lInEntryInstructions => JobDeclaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x => x.IsT2L);
}
