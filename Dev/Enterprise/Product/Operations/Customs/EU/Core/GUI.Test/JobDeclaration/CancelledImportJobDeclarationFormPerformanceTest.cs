using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.GUI.Testing;

public class CancelledImportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
{
	protected override Dictionary<string, int> EULightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
	{
		{ RefPacksSchema.Constants.TableName, 6 }
	};

	protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
	protected override bool DeclarationIsCancelled => true;
}
