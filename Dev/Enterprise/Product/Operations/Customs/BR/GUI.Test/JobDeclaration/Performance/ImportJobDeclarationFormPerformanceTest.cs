using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ImportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;

		protected override void DecorateDeclaration(BaseJobDeclaration declaration)
		{
			BRCustomsDataRegistry.Instance.EnableForeignOperator.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			base.DecorateDeclaration(declaration);
		}

		protected override Dictionary<string, int> BRUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>()
		{
			{ CusBRForeignOperatorSchema.Constants.TableName, 7 }
		};

		protected override Dictionary<string, int> BRUniversalXMLAddExpectedHits => new Dictionary<string, int>()
		{
			{ CusBRForeignOperatorSchema.Constants.TableName, 7 }
		};
	}
}
