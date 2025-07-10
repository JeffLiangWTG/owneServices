using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ImportSiscomexJobDeclarationFormPerformanceTest : JobDeclarationFormPerformanceTest
	{
		protected override ZString MessageTypeForFormBashing => BRJobMessageTypeList.Codes.ImportSiscomex;
	}
}
