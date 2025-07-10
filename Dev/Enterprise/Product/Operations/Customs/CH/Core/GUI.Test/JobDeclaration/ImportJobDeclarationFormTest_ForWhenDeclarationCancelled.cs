using Enterprise.Customs.Common.CH;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(JobDeclarationForm))]
sealed class ImportJobDeclarationFormTest_ForWhenDeclarationCancelled : JobDeclarationFormTest_ForWhenDeclarationCancelled
{
	public override CargoWise.Types.ZString MessageTypeForFormBashing => CHJobMessageTypeList.Codes.Import;
}
