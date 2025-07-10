using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing;

[TestedType(typeof(JobDeclarationForm))]
public class ImportJobDeclarationFormTest_ForWhenDeclarationCancelled : JobDeclarationFormTest_ForWhenDeclarationCancelled<JobDeclaration>
{
	public override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
}
