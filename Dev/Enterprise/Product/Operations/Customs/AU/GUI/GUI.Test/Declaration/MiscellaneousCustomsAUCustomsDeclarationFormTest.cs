using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing;

[TestedType(typeof(ZAUCustomsDeclarationForm))]
public class MiscellaneousCustomsAUCustomsDeclarationFormTest : AUCustomsDeclarationFormTest
{
	public override ZString MessageTypeForFormBashing => Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
}
