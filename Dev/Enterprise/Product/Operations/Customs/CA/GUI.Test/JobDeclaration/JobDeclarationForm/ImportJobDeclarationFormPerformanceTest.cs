using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class ImportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Business.JobMessageTypeList.Codes.Import;

		protected override Dictionary<string, int> CALoadEditableChildObjectsExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 8 },
			{ JobComInvoiceHeaderSchema.Constants.TableName, 5 }
		};

		protected override Dictionary<string, int> CAValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ JobComInvoiceHeaderSchema.Constants.TableName, 6 },
			{ OrgAddressSchema.Constants.TableName, 10 },
			{ OrgCusCodeSchema.Constants.TableName, 8 }
		};

		protected override Dictionary<string, int> CALightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 7 },
			{ OrgHeaderSchema.Constants.TableName, 5 }
		};

		protected override Dictionary<string, int> CAFormMergeExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 7 },
			{ CusHouseContPackInvoiceHeaderPivotSchema.Constants.TableName, 6 },
			{ CusHouseContPackInvoiceLinePivotSchema.Constants.TableName, 60 } //Pacakges tab no visible for IID. INC to create WI to fix
		};

		protected override Dictionary<string, int> CAUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
		{
			{ StmDocDataOverrideSchema.Constants.TableName, 10 },
			{ JobDocumentExclusionSchema.Constants.TableName, 7 },
			{ GenCustomAddOnRuleAckSchema.Constants.TableName, 6 },
			{ JobShipmentSchema.Constants.TableName, 6 },
			{ StmNoteSchema.Constants.TableName, 9 },
			{ JobDocAddressSchema.Constants.TableName, 9 }
		};

		protected override Dictionary<string, int> CAUniversalXMLAddExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressCapabilitySchema.Constants.TableName, 6 },
		};

		protected override Dictionary<string, int> CADeleteExpectedHits => new Dictionary<string, int>
		{
			{ JobDocumentExclusionSchema.Constants.TableName, 7 },
			{ GenCustomAddOnRuleAckSchema.Constants.TableName, 8 },
			{ JobShipmentSchema.Constants.TableName, 6 },
			{ StmDocDataOverrideSchema.Constants.TableName, 8 },
			{ StmNoteSchema.Constants.TableName, 6 }
		};
	}
}
