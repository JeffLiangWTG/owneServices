using System;
using Enterprise.Customs.FR.Business.Declaration;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.FR.Business.Testing;

public class EntryInstructionValidationDeciderTestContext : EU.Business.Testing.DeclarationValidationDeciderTestContext<IEntryInstructionValidationDecider>
{
	public EntryInstructionValidationDeciderTestContext(EU.Business.Declaration.JobDeclaration declaration, bool isUCC6, params Type[] ruleDeciderInterfaceTypes) : base(declaration, isUCC6, ruleDeciderInterfaceTypes)
	{
	}

	protected override void SetupConfiguration(Mock<EU.Business.DeclarationConfiguration> configuration, IEntryInstructionValidationDecider validationDecider)
	{
		base.SetupConfiguration(configuration, validationDecider);

		var instructionConfigurationMock = new Mock<InstructionConfiguration>();
		instructionConfigurationMock.Protected()
			.Setup<EU.Business.Declaration.IEntryInstructionValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<CusEntryInstruction>())
			.Returns(validationDecider);
		configuration.Protected()
			.Setup<EU.Business.InstructionConfiguration>("GetNewInstructionConfiguration")
			.Returns(instructionConfigurationMock.Object);
	}
}
