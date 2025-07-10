using CargoWise.Types;

namespace Enterprise.Customs.EU.GUI.Testing;

public class ImportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
{
	protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
}
