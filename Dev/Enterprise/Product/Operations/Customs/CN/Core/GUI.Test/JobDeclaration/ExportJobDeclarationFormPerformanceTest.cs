#if !WINZOR
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.GUI.Testing
{
	class ExportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Export;

		protected override Dictionary<string, int> CNValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 5 },
			{ OrgHeaderSchema.Constants.TableName, 5 },
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 8 },
		};

		protected override Dictionary<string, int> CNLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 5 },
			{ OrgHeaderSchema.Constants.TableName, 5 },
		};

		protected override Dictionary<string, int> CNUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
		{
			{ RefPacksSchema.Constants.TableName, 12 },
			{ StmDocDataOverrideSchema.Constants.TableName, 128 },
		};

		protected override Dictionary<string, int> CNDeleteExpectedHits => new Dictionary<string, int>
		{
			{ StmNoteSchema.Constants.TableName, 11 }
		};

		protected override Dictionary<string, int> CNUniversalXMLAddExpectedHits => new Dictionary<string, int>
		{
			{ RefPacksSchema.Constants.TableName, 12 },
		};
	}
}
#endif
