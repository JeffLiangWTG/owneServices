using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing;

[TestedType(typeof(ZAUCustomsDeclarationForm))]
public class DrawbackAUCustomsDeclarationFormTest : AUCustomsDeclarationFormTest
{
	public override ZString MessageTypeForFormBashing => Common.Shared.SharedJobMessageTypeList.Codes.Drawback;
}
