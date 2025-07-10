using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ImportLicenseJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => BRJobMessageTypeList.Codes.ImportLicense;

		protected override Dictionary<string, int> BRBaseUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>()
		{
			{ StmDocDataOverrideSchema.Constants.TableName, 6 }
		};
	}
}
