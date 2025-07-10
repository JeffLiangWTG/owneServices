using CargoWise.Types;

namespace Enterprise.Customs.MX.GUI.Testing
{
	sealed class ExportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Export;
	}
}
