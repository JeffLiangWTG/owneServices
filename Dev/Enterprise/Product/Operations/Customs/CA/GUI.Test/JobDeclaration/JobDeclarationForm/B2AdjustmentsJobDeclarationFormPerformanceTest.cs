using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class B2AdjustmentsJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Business.JobMessageTypeList.Codes.B2Adjustments;

		protected override Dictionary<string, int> CALightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ OrgHeaderSchema.Constants.TableName, 5 },
			{ StmDocDataOverrideSchema.Constants.TableName, 62 }
		};

		protected override Dictionary<string, int> CAUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
		{
			{ StmDocDataOverrideSchema.Constants.TableName, 10 }
		};

		protected override Dictionary<string, int> UniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
			{ CusCALPCOSchema.Constants.TableName, 60 },
			{ CusAddInfoSchema.Constants.TableName, 12 },
			{ CusCodeDataSchema.Constants.TableName, 14 },
			{ JobComInvoiceHeaderSchema.Constants.TableName, 6 },
			{ JobComInvHeaderChargeSchema.Constants.TableName, 6 }
		};

		protected override Dictionary<string, int> CAFormMergeExpectedHits => new Dictionary<string, int>
		{
			{ JobComInvoiceHeaderSchema.Constants.TableName, 6 }
		};
	}
}
