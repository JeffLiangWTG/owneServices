using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.MY.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class ImportJobDeclarationFormTest : Customs.GUI.Testing.BaseJobDeclarationFormAbstractTest<Business.JobDeclaration>
	{
		public override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
	}

	[TestedType(typeof(JobDeclarationForm))]
	sealed class ImportJobDeclarationFormTest_ForWhenDeclarationCancelled : Customs.GUI.Testing.BaseCustomsDeclarationFormTest_ForWhenDeclarationCancelled<Business.JobDeclaration>
	{
		public override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
	}
}
