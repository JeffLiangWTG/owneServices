using CargoWise.Types;
using Enterprise.Customs.AE.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

[TestedType(typeof(JobDeclarationForm))]
sealed class ImportJobDeclarationFormTest : Customs.GUI.Testing.BaseJobDeclarationFormAbstractTest<JobDeclaration>
{
	public override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
}
