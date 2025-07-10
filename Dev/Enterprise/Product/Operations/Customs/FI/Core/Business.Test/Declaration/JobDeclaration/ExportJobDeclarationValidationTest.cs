using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FI.Business.Testing;

[TestedType(typeof(ExportJobDeclarationValidation))]
class ExportJobDeclarationValidationTest : JobDeclarationValidationAbstractTest<ExportJobDeclarationValidation>
{
	protected override string MessageType => MessageTypeList.Codes.Export;

	protected override ExportJobDeclarationValidation GetValidation() => new ExportJobDeclarationValidation(jobDeclaration);
}
