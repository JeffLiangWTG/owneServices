using CargoWise.Types;

namespace Enterprise.Customs.AE.GUI.Testing;

sealed class CancelledImportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
{
	protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
	protected override bool DeclarationIsCancelled => true;
}
