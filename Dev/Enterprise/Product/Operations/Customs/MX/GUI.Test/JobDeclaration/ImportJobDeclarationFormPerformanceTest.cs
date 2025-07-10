using CargoWise.Types;

namespace Enterprise.Customs.MX.GUI.Testing
{
	sealed class ImportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
	}
}
