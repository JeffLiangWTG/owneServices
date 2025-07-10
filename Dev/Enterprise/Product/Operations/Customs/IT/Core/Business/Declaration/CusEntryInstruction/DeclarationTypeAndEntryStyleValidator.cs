using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class DeclarationTypeAndEntryStyleValidator
{
	public DeclarationTypeAndEntryStyleValidator(CusEntryInstruction entryInstruction)
	{
		this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
	}

	internal void Validate(ZPropertyInfo targetPropertyInfo)
	{
		var declaration = entryInstruction.JobDeclaration;
		if (declaration == null)
		{
			return;
		}

		var entryStyle = declaration.JE_EntryStyle;
		var declarationType = entryInstruction.CEI_Style;
		if (entryStyle.IsEmpty || declarationType.IsEmpty)
		{
			return;
		}

		if (declaration.IsImport)
		{
			ValidateForImport(entryStyle, declarationType, targetPropertyInfo);
		}
		else if (declaration.IsUCC6AndIsExport)
		{
			ValidateForUcc6Export(entryStyle, declarationType, targetPropertyInfo);
		}
	}

	#region Implementation

	void ValidateForImport(string entryStyle, string declarationType, ZPropertyInfo targetPropertyInfo)
		=> IsEntryStyleAllowedForDeclarationType(declarationTypeAndEntryStyleMapForImport, entryStyle, declarationType, targetPropertyInfo);

	void ValidateForUcc6Export(string entryStyle, string declarationType, ZPropertyInfo targetPropertyInfo)
		=> IsEntryStyleAllowedForDeclarationType(declarationTypeAndEntryStyleMapForUcc6Export, entryStyle, declarationType, targetPropertyInfo);

	void IsEntryStyleAllowedForDeclarationType(ImmutableDictionary<string, string[]> map, string entryStyle, string ceiStyle, ZPropertyInfo targetPropertyInfo)
	{
		if (!map.ContainsKey(ceiStyle))
		{
			return;
		}

		var allowedEntryStyles = map[ceiStyle];
		if (entryStyle.In(allowedEntryStyles))
		{
			return;
		}

		var messageError = ValidationCaptions.EntryInstruction.GetEntryStyleNotAllowedForADeclarationType(entryStyle);
		targetPropertyInfo.AddMessageError(messageError);
	}

	#endregion

	readonly ImmutableDictionary<string, string[]> declarationTypeAndEntryStyleMapForUcc6Export = new Dictionary<string, string[]>()
	{
		{ ExportUCC6DeclarationTypeList.Codes.DichiarazioneRiesportazioneB1, new [] { EntryStyleListExport.Codes.ExportNormal } },
		{ ExportUCC6DeclarationTypeList.Codes.RegimeSpecialeDichiarazioneB2, new [] { EntryStyleListExport.Codes.ExportNormal } },
		{ ExportUCC6DeclarationTypeList.Codes.DichiarazionePerTerritoriFiscaliSpecialiB4, new [] { EntryStyleListExport.Codes.ExportToSpecialTerritory } },
		{ ExportUCC6DeclarationTypeList.Codes.DichiarazioneSemplificataC1, new [] { EntryStyleListExport.Codes.ExportNormal } },
	}.ToImmutableDictionary();

	readonly ImmutableDictionary<string, string[]> declarationTypeAndEntryStyleMapForImport = new Dictionary<string, string[]>()
	{
		{ ImportUCC6DeclarationTypeList.Codes.ImmissioneLiberaPraticaH1, new [] {  EntryStyleListImport.Codes.ImportNormal  } },
		{ ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeDepositoDoganaleH2, new [] {  EntryStyleListImport.Codes.ImportNormal  } },
		{ ImportUCC6DeclarationTypeList.Codes.RegimeSpecialeAmmissioneTemporaneaH3, new [] {  EntryStyleListImport.Codes.ImportNormal  } },
		{ ImportUCC6DeclarationTypeList.Codes.RegimeSpecialePerfezionamentoAttivoH4, new [] {  EntryStyleListImport.Codes.ImportNormal  } },
		{ ImportUCC6DeclarationTypeList.Codes.DichiarazioneScambiTerritoriFiscaliSpecialiH5, new [] {  EntryStyleListImport.Codes.ImportFromSpecialTerritory  } },
		{ ImportUCC6DeclarationTypeList.Codes.DichiarazioneImportazioneSemplificataI1, new [] {  EntryStyleListImport.Codes.ImportNormal } },
	}.ToImmutableDictionary();

	readonly CusEntryInstruction entryInstruction;
}
