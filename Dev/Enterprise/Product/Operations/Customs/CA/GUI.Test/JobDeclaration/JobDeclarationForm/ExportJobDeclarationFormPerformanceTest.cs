using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class ExportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Business.JobMessageTypeList.Codes.Export;

		protected override Dictionary<string, int> CALoadEditableChildObjectsExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 5 }
		};

		protected override Dictionary<string, int> CAValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 5 }
		};

		protected override Dictionary<string, int> CALightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 5 },
			{ OrgHeaderSchema.Constants.TableName, 6 }
		};

		protected override Dictionary<string, int> CAFormMergeExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 5 }
		};
	}
}
