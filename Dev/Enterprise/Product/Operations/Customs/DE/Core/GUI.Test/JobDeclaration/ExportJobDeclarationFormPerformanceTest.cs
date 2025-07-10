using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.GUI.Testing;

public class ExportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
{
	protected override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Export;
	protected override Dictionary<string, int> DEValidateAllExpectedHits
	{
		get
		{
			var result = base.DEValidateAllExpectedHits;
			result.Add("NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 7);
			return result;
		}
	}

	protected override Dictionary<string, int> DELightFormValidationAndSaveExpectedHits
	{
		get
		{
			var result = base.DELightFormValidationAndSaveExpectedHits;
			result.Add("NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 7);
			return result;
		}
	}

	protected override Dictionary<string, int> DEUniversalXMLExportExpectedHits
	{
		get
		{
			var result = base.DEUniversalXMLExportExpectedHits;
			result.Add("NonPersitentTable DBO.ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 5);
			return result;
		}
	}
}
