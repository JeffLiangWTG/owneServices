using CargoWise.Types;
using Enterprise.Customs.AE.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.GUI.Testing;

[TestedType(typeof(JobDeclarationForm))]
public class ImportJobDeclarationFormTest_ForWhenDeclarationCancelled : Customs.GUI.Testing.BaseCustomsDeclarationFormTest_ForWhenDeclarationCancelled<JobDeclaration>
{
	public override ZString MessageTypeForFormBashing => Customs.Business.JobMessageTypeList.Codes.Import;
}
