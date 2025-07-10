using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class LXVJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Business.JobMessageTypeList.Codes.LVSForConsolidation;

		protected override Dictionary<string, int> CALoadEditableChildObjectsExpectedHits => new Dictionary<string, int>
		{
			{ JobComInvoiceHeaderSchema.Constants.TableName, 5 },
			{ OrgAddressSchema.Constants.TableName, 5 },
			{ StmDataSchema.Constants.TableName, 6 },
		};

		protected override Dictionary<string, int> CAValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ JobComInvoiceHeaderSchema.Constants.TableName, 6 },
			{ OrgAddressSchema.Constants.TableName, 7 },
			{ RefDataGroupingSchema.Constants.TableName, 5 },
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 6 },
			{ OrgHeaderSchema.Constants.TableName, 5 }
		};

		protected override Dictionary<string, int> CALightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 7 },
			{ OrgHeaderSchema.Constants.TableName, 6 }
		};

		protected override Dictionary<string, int> CAFormMergeExpectedHits => new Dictionary<string, int>
		{
			{ CusEntryLineSchema.Constants.TableName, 6 },
			{ GenPivotSchema.Constants.TableName, 6 },
			{ JobComInvoiceHeaderSchema.Constants.TableName, 5 },
			{ OrgAddressSchema.Constants.TableName, 5 },
			{ StmDataSchema.Constants.TableName, 6 }
		};

		protected override Dictionary<string, int> CAUniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 8 },
			{ OrgCusCodeSchema.Constants.TableName, 8 },
			{ OrgHeaderSchema.Constants.TableName, 8 },
			{ OrgTranslatedAddressSchema.Constants.TableName, 7 },
			{ JobHeaderSchema.Constants.TableName, 5 }
		};

		protected override Dictionary<string, int> CAUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
		{
			{ GenPivotSchema.Constants.TableName, 6 },
			{ JobDocumentExclusionSchema.Constants.TableName, 7 },
			{ GenCustomAddOnRuleAckSchema.Constants.TableName, 6 },
			{ JobShipmentSchema.Constants.TableName, 6 },
			{ StmDocDataOverrideSchema.Constants.TableName, 10 },
			{ StmNoteSchema.Constants.TableName, 10 },
			{ JobDocAddressSchema.Constants.TableName, 9 },
			{ OrgHeaderSchema.Constants.TableName, 5 }
		};

		protected override Dictionary<string, int> CAUniversalXMLAddExpectedHits => new Dictionary<string, int>
		{
		};

		protected override Dictionary<string, int> CADeleteExpectedHits => new Dictionary<string, int>
		{
			{ GenPivotSchema.Constants.TableName, 7 },
			{ StmDocDataOverrideSchema.Constants.TableName, 10 },
			{ JobDocumentExclusionSchema.Constants.TableName, 8 },
			{ JobComInvoiceHeaderSchema.Constants.TableName, 10 },
			{ GenCustomAddOnRuleAckSchema.Constants.TableName, 8 },
			{ JobShipmentSchema.Constants.TableName, 6 },
			{ StmNoteSchema.Constants.TableName, 6 },
			{ OrgHeaderSchema.Constants.TableName, 10 }
		};
	}
}
