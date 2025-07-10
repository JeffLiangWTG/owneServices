using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class CancelledImportLicenseJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => BRJobMessageTypeList.Codes.ImportLicense;

		protected override bool DeclarationIsCancelled => true;
	}
}
