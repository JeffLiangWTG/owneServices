using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(JobDeclarationForm))]
sealed class ExportJobDeclarationFormTest_ForWhenDeclarationCancelled : Customs.GUI.Testing.BaseCustomsDeclarationFormTest_ForWhenDeclarationCancelled<JobDeclaration>
{
	public override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Export;
}
