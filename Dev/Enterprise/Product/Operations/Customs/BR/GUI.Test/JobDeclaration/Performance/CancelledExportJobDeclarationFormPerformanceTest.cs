using CargoWise.Types;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class CancelledExportJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Export;

		protected override bool DeclarationIsCancelled => true;
	}
}
