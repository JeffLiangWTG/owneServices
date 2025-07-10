using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing;

[TestedType(typeof(ZAUCustomsDeclarationForm))]
public class QuarantineAUCustomsDeclarationFormTest : AUCustomsDeclarationFormTest
{
	public override ZString MessageTypeForFormBashing => AUJobMessageTypeList.Codes.Quarantine;
}
