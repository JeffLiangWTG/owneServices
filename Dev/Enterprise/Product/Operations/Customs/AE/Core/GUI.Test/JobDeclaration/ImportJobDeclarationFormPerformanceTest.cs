using CargoWise.Types;

namespace Enterprise.Customs.AE.GUI.Testing;

sealed class ImportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
{
	protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
}
