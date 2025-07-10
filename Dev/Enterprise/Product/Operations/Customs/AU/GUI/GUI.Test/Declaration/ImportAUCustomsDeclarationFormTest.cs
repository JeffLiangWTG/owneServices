using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing;

[TestedType(typeof(ZAUCustomsDeclarationForm))]
public class ImportAUCustomsDeclarationFormTest : AUCustomsDeclarationFormTest
{
	public override ZString MessageTypeForFormBashing => Common.Shared.SharedJobMessageTypeList.Codes.Import;
}
