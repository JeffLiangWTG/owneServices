using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusEntryInstructionLookups : EU.Business.Declaration.CusEntryInstructionLookups
{
	public CusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction)
		: base(cusEntryInstruction)
	{
	}

	protected override CodeDescriptionPairList GetDefinedDeclarationTypeList(EU.Business.Declaration.JobDeclaration declaration)
	{
		if (declaration is JobDeclaration itJobDeclaration)
		{
			return GetDeclarationTypeCodeDescriptionPairList(itJobDeclaration);
		}

		return base.GetDefinedDeclarationTypeList(declaration);
	}

	protected override CodeDescriptionPairList GetEntrySubstyleList(ICanBeImportOrExport parent) => GetCachedEntrySubStyleCodeDescriptionPairList();

	protected override string GetDeclarationTypeListCacheKey(EU.Business.Declaration.JobDeclaration declaration)
	{
		var key = base.GetDeclarationTypeListCacheKey(declaration);
		if (declaration != null)
		{
			key = string.Join("_", key, declaration.IsUCC6);
		}
		return key;
	}

	protected override CodeDescriptionPairList ProcedureCodeListCore
	{
		get
		{
			var declaration = Parent.JobDeclaration;
			var ceiStyle = Parent.CEI_Style;
			var messageType = declaration?.JE_MessageType ?? ZString.Empty;
			var currentCompanyCountryCode = GlbCompany.CurrentCompany.Country.Code;
			var cacheKey = FormattableString.Invariant($"{currentCompanyCountryCode}_{messageType}_{ceiStyle}_RefCusProcedures_ProcedureCodeList");
			return Factory.GetCachedValue(cacheKey, () =>
			{
				var result = new CodeDescriptionPairList();
				var procedures = new RefCusProcedureCollection(Factory, currentCompanyCountryCode, ZDateTime.Today, ceiStyle, messageType);
				procedures.ForEach(x => result.AddPairIfNotExist(x.ZZ6_ProcedureCode, x.ZZ6_Description.Split('-').First().Trim()));
				result.Sort();
				return result;
			});
		}
	}

	#region Implementation

	CodeDescriptionPairList GetDeclarationTypeCodeDescriptionPairList(JobDeclaration declaration)
	{
		if (declaration.IsImport)
		{
			return new ImportUCC6DeclarationTypeList();
		}
		if (declaration.IsUCC6AndIsExport)
		{
			return new ExportUCC6DeclarationTypeList();
		}
		if (declaration.IsExport)
		{
			return new SADDeclarationTypeList();
		}
		return new CodeDescriptionPairList();
	}

	CodeDescriptionPairList GetCachedEntrySubStyleCodeDescriptionPairList()
	{
		var declarationType = Parent.CEI_Style;
		return Factory.GetCachedValue($"Enterprise.Customs.IT.Business.Declaration.CusEntryInstructionLookups|GetEntrySubStyleCodeDescriptionPairList|{declarationType}", () =>
		{
			var subStyleCodeDescriptionPairList = new CodeDescriptionPairList();
			switch (declarationType)
			{
				case ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1:
				case ExportUCC6DeclarationTypeList.Codes.DichiarazioneRiesportazioneB1:
					subStyleCodeDescriptionPairList.AddPair(ITEntrySubStyleList.Codes.StandardDeclarationA, ITEntrySubStyleList.Descriptions.StandardDeclarationA);
					subStyleCodeDescriptionPairList.AddPair(ITEntrySubStyleList.Codes.PreliminaryStandardDeclarationD, ITEntrySubStyleList.Descriptions.PreliminaryStandardDeclarationD);
					subStyleCodeDescriptionPairList.AddPair(ITEntrySubStyleList.Codes.SupplementaryDeclarationX, ITEntrySubStyleList.Descriptions.SupplementaryDeclarationX);
					subStyleCodeDescriptionPairList.AddPair(ITEntrySubStyleList.Codes.SupplementaryDeclarationY, ITEntrySubStyleList.Descriptions.SupplementaryDeclarationY);
					subStyleCodeDescriptionPairList.AddPair(ITEntrySubStyleList.Codes.SupplementaryDeclarationZ, ITEntrySubStyleList.Descriptions.SupplementaryDeclarationZ);
					break;

				case ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeDepositoDoganaleH2:
				case ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeAmmissioneTemporaneaH3:
				case ImportUCC6DeclarationTypeList.Codes.RegimeSpecialePerfezionamentoAttivoH4:
				case ExportUCC6DeclarationTypeList.Codes.RegimeSpecialeDichiarazioneB2:
				case ExportUCC6DeclarationTypeList.Codes.DichiarazionePerTerritoriFiscaliSpecialiB4:
				case SADDeclarationTypeList.Codes.ProceduraOrdinariaCODogana:
				case SADDeclarationTypeList.Codes.ProceduraOrdinariaCOLuogo:
				case SADDeclarationTypeList.Codes.DichiarazioneSemplificata:
					subStyleCodeDescriptionPairList.AddPair(ITEntrySubStyleList.Codes.StandardDeclarationA, ITEntrySubStyleList.Descriptions.StandardDeclarationA);
					subStyleCodeDescriptionPairList.AddPair(ITEntrySubStyleList.Codes.PreliminaryStandardDeclarationD, ITEntrySubStyleList.Descriptions.PreliminaryStandardDeclarationD);
					break;

				case ExportUCC6DeclarationTypeList.Codes.DichiarazioneSemplificataC1:
				case ImportUCC6DeclarationTypeList.Codes.DichiarazioneImportazioneSemplificataI1:
					subStyleCodeDescriptionPairList.AddPair(ITEntrySubStyleList.Codes.SimplifiedDeclarationOccasionallyB, ITEntrySubStyleList.Descriptions.SimplifiedDeclarationOccasionallyB);
					subStyleCodeDescriptionPairList.AddPair(ITEntrySubStyleList.Codes.SimplifiedDeclarationRegularlyC, ITEntrySubStyleList.Descriptions.SimplifiedDeclarationRegularlyC);
					subStyleCodeDescriptionPairList.AddPair(ITEntrySubStyleList.Codes.PreliminarySimplifiedDeclarationE, ITEntrySubStyleList.Descriptions.PreliminarySimplifiedDeclarationE);
					subStyleCodeDescriptionPairList.AddPair(ITEntrySubStyleList.Codes.PreliminarySimplifiedDeclarationF, ITEntrySubStyleList.Descriptions.PreliminarySimplifiedDeclarationF);
					break;
			}
			return subStyleCodeDescriptionPairList;
		});
	}

	#endregion
}
