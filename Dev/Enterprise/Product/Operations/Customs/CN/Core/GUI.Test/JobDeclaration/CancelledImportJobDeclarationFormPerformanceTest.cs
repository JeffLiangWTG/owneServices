#if !WINZOR
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.GUI.Testing
{
	class CancelledImportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Import;

		protected override bool DeclarationIsCancelled => true;

		protected override Dictionary<string, int> CNValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 7 }
		};

		protected override Dictionary<string, int> CNLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 7 },
		};

		protected override Dictionary<string, int> CNDeleteExpectedHits => new Dictionary<string, int>
		{
			{ StmNoteSchema.Constants.TableName, 11 },
			{ StmDocDataOverrideSchema.Constants.TableName, 142 },
		};
	}
}
#endif
