using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(JobDeclarationValidation))]
class JobDeclarationValidationBaseOnlyTest : JobDeclarationValidationAbstractTest<JobDeclarationValidation>
{
	protected override string MessageType => MessageTypeList.Codes.MiscellaneousCustoms;

	protected override JobDeclarationValidation GetValidation() => new JobDeclarationValidation(jobDeclaration);
}
