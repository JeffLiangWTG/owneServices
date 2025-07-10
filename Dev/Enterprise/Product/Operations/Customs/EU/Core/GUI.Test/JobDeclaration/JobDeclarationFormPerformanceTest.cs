using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.GUI.Testing;

public class JobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceAbstractTest
{
	protected override ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((JobDeclaration)bizO);

	protected virtual Dictionary<string, int> EULoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
	Dictionary<string, int> EUBaseLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();

	protected virtual Dictionary<string, int> EUValidateAllExpectedHits => new Dictionary<string, int>();
	protected virtual Dictionary<string, int> EUBaseValidateAllExpectedHits => new Dictionary<string, int>()
	{
		{ RefPacksSchema.Constants.TableName, 6 },
		{ "NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 7 },
		{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 5  } // CusEntryInstructionLookups.ReConstructCodeDescriptionPairList + ZZCustomsFunctionalityEffectiveDate.IsNewCustomsFunctionalityValid + ZZCustomsFunctionalityEffectiveDate.IsPilotFunctionalityValid + 2 in EUUniversalLookupsHelper.GetAgreedPlaceCodeList
	};

	protected virtual Dictionary<string, int> EULightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();
	Dictionary<string, int> EUBaseLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>()
	{
		{ RefPacksSchema.Constants.TableName, 6 },
		{ "NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 7 }
	};

	protected virtual Dictionary<string, int> EUFormMergeExpectedHits => new Dictionary<string, int>();
	Dictionary<string, int> EUBaseFormMergeExpectedHits => new Dictionary<string, int>();

	protected virtual Dictionary<string, int> EUUniversalXMLExportExpectedHits => new Dictionary<string, int>();
	Dictionary<string, int> EUBaseUniversalXMLExportExpectedHits => new Dictionary<string, int>()
	{
		{ "NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 7 },
		{ CusAuthorizationUsageSchema.Constants.TableName, 9 }
	};

	protected virtual Dictionary<string, int> EUUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>();
	Dictionary<string, int> EUBaseUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
	{
		{ OrgAddressSchema.Constants.TableName, 9 },
		{ OrgHeaderSchema.Constants.TableName, 11 },
		{ RefPacksSchema.Constants.TableName, 6 },
		{ StmNoteSchema.Constants.TableName, 6 },
		{ StmDocDataOverrideSchema.Constants.TableName, 6 }
	};

	protected virtual Dictionary<string, int> EUUniversalXMLAddExpectedHits => new Dictionary<string, int>();
	Dictionary<string, int> EUBaseUniversalXMLAddExpectedHits => new Dictionary<string, int>
	{
		{ OrgAddressSchema.Constants.TableName, 9 },
		{ OrgHeaderSchema.Constants.TableName, 13 },
		{ RefPacksSchema.Constants.TableName, 6 }
	};

	protected virtual Dictionary<string, int> EUDeleteExpectedHits => new Dictionary<string, int>();
	Dictionary<string, int> EUBaseDeleteExpectedHits => new Dictionary<string, int>
	{
		{ CusCodeDataSchema.Constants.TableName, 134 },
		{ CusSupportingInfoSchema.Constants.TableName, 5 },
		{ StmDocDataOverrideSchema.Constants.TableName, 6 },
		{ StmNoteSchema.Constants.TableName, 5 }
	};

	protected override Dictionary<string, int> LoadEditableChildObjectsExpectedHits => ZipDictionaries(EUBaseLoadEditableChildObjectsExpectedHits, EULoadEditableChildObjectsExpectedHits);
	protected override Dictionary<string, int> ValidateAllExpectedHits => ZipDictionaries(EUBaseValidateAllExpectedHits, EUValidateAllExpectedHits);
	protected override Dictionary<string, int> LightFormValidationAndSaveExpectedHits => ZipDictionaries(EUBaseLightFormValidationAndSaveExpectedHits, EULightFormValidationAndSaveExpectedHits);
	protected override Dictionary<string, int> FormMergeExpectedHits => ZipDictionaries(EUBaseFormMergeExpectedHits, EUFormMergeExpectedHits);
	protected override Dictionary<string, int> UniversalXMLExportExpectedHits => ZipDictionaries(EUBaseUniversalXMLExportExpectedHits, EUUniversalXMLExportExpectedHits);
	protected override Dictionary<string, int> UniversalXMLImportUpdateExpectedHits => ZipDictionaries(EUBaseUniversalXMLImportUpdateExpectedHits, EUUniversalXMLImportUpdateExpectedHits);
	protected override Dictionary<string, int> UniversalXMLAddExpectedHits => ZipDictionaries(EUBaseUniversalXMLAddExpectedHits, EUUniversalXMLAddExpectedHits);
	protected override Dictionary<string, int> DeleteExpectedHits => ZipDictionaries(EUBaseDeleteExpectedHits, EUDeleteExpectedHits);
}
