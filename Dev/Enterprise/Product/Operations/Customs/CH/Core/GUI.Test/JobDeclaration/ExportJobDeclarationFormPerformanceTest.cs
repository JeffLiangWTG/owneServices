using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.GUI.Testing;

sealed class ExportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
{
	protected override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Export;

	protected override Dictionary<string, int> CHDeleteExpectedHits => new Dictionary<string, int>()
		{
			{ StmDocDataOverrideSchema.Constants.TableName, 6 }
		};

	protected override Dictionary<string, int> CHBaseValidateAllExpectedHits => new Dictionary<string, int>()
		{
			{ "NonPersitentTable ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 5 }
		};

	protected override Dictionary<string, int> CHBaseLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>()
		{
			{ "NonPersitentTable ZZREFCUSCODELISTCOMBINED LEFT JOIN REFDATABASE_REFCUSCODELISTLANGUAGE ON ZXA_ZZD_CODELIST = ZZD_PK AND ZXA_ZX6_NKLANGUAGE = @LANGUAGE", 5 }
		};
}
