using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FI.Business.Testing;

[TestedType(typeof(ImportJobDeclarationValidation))]
class ImportJobDeclarationValidationTest : JobDeclarationValidationAbstractTest<ImportJobDeclarationValidation>
{
	protected override string MessageType => MessageTypeList.Codes.Import;

	protected override ImportJobDeclarationValidation GetValidation() => new ImportJobDeclarationValidation(jobDeclaration);
}
