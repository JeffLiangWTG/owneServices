using Enterprise.Customs.Common.CH;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(JobDeclarationForm))]
sealed class ExportJobDeclarationFormTest : JobDeclarationFormTest
{
	public override CargoWise.Types.ZString MessageTypeForFormBashing => CHJobMessageTypeList.Codes.Export;
}
