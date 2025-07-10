using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IN.GUI.Testing;

sealed class ImportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
{
	protected override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Import;
}
