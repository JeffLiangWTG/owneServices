using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ExportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Export;

		protected override Dictionary<string, int> BRBaseUniversalXMLExportExpectedHits => new Dictionary<string, int>()
		{
			{ StmDocDataOverrideSchema.Constants.TableName, 6 }
		};
	}
}
