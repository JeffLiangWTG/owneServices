using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class ExportJobDeclarationFormTest : JobDeclarationFormTest
	{
		public override ZString MessageTypeForFormBashing => Business.JobMessageTypeList.Codes.Export;
	}
}
