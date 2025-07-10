using Enterprise.Customs.IL.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class ImportJobDeclarationFormTest : Customs.GUI.Testing.BaseJobDeclarationFormAbstractTest<JobDeclaration>
	{
		public override CargoWise.Types.ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
	}
}
