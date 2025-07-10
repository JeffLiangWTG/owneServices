using CargoWise.Types;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class CancelledImportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;

		protected override bool DeclarationIsCancelled => true;
	}
}
