using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class LowValueShipmentsJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Business.JobMessageTypeList.Codes.LowValueShipments;

		protected override Dictionary<string, int> CALoadEditableChildObjectsExpectedHits => new Dictionary<string, int>
		{
			{ JobComInvoiceHeaderSchema.Constants.TableName, 6 },
			{ JobDocAddressSchema.Constants.TableName, 7 },
			{ OrgAddressCapabilitySchema.Constants.TableName, 6 },
			{ OrgAddressSchema.Constants.TableName, 14 },
			{ OrgHeaderSchema.Constants.TableName, 7 },
			{ StmDataSchema.Constants.TableName, 6 }
		};

		protected override Dictionary<string, int> CAValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ JobDocAddressSchema.Constants.TableName, 8 },
			{ JobComInvoiceHeaderSchema.Constants.TableName, 7 },
			{ OrgAddressCapabilitySchema.Constants.TableName, 7 },
			{ OrgAddressSchema.Constants.TableName, 5 },
			{ OrgCusCodeSchema.Constants.TableName, 6 },
			{ OrgHeaderSchema.Constants.TableName, 5 },
			{ StmDataSchema.Constants.TableName, 6 }
		};

		protected override Dictionary<string, int> CALightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ JobDocAddressSchema.Constants.TableName, 8 },
			{ OrgAddressCapabilitySchema.Constants.TableName, 7 },
			{ OrgAddressSchema.Constants.TableName, 5 },
			{ OrgCusCodeSchema.Constants.TableName, 6 },
			{ OrgHeaderSchema.Constants.TableName, 5 },
			{ StmDataSchema.Constants.TableName, 6 }
		};

		protected override Dictionary<string, int> CAFormMergeExpectedHits => new Dictionary<string, int>
		{
			{ JobDocAddressSchema.Constants.TableName, 7 },
			{ OrgAddressCapabilitySchema.Constants.TableName, 6 },
			{ OrgAddressSchema.Constants.TableName, 13 },
			{ OrgHeaderSchema.Constants.TableName, 6 },
			{ StmDataSchema.Constants.TableName, 6 }
		};

		protected override Dictionary<string, int> CAUniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
			{ GenPivotSchema.Constants.TableName, 5 },
			{ JobComInvoiceHeaderSchema.Constants.TableName, 5 }
		};

		protected override Dictionary<string, int> CAUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
		{
			{ OrgSupplierBuyerLinkSchema.Constants.TableName, 7 },
			{ StmDocDataOverrideSchema.Constants.TableName, 10 },
			{ JobDocumentExclusionSchema.Constants.TableName, 7 },
			{ GenPivotSchema.Constants.TableName, 9 },
			{ GenCustomAddOnRuleAckSchema.Constants.TableName, 6 },
			{ JobShipmentSchema.Constants.TableName, 6 },
			{ StmNoteSchema.Constants.TableName, 9 },
			{ JobDocAddressSchema.Constants.TableName, 9 }
		};

		protected override Dictionary<string, int> CAUniversalXMLAddExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressCapabilitySchema.Constants.TableName, 6 },
			{ OrgSupplierBuyerLinkSchema.Constants.TableName, 7 }
		};

		protected override Dictionary<string, int> CADeleteExpectedHits => new Dictionary<string, int>
		{
			{ StmDocDataOverrideSchema.Constants.TableName, 8 },
			{ JobDocumentExclusionSchema.Constants.TableName, 7 },
			{ GenPivotSchema.Constants.TableName, 9 },
			{ GenCustomAddOnRuleAckSchema.Constants.TableName, 8 },
			{ JobShipmentSchema.Constants.TableName, 6 },
			{ StmNoteSchema.Constants.TableName, 6 }
		};
	}
}
