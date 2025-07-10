using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	class ImportJobDeclarationFormTest : JobDeclarationFormTest
	{
		public override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Import;
	}
}
