using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(JobDeclarationForm))]
sealed class ExportJobDeclarationFormTest : Customs.GUI.Testing.BaseJobDeclarationFormAbstractTest<JobDeclaration>
{
	public override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Export;
}
