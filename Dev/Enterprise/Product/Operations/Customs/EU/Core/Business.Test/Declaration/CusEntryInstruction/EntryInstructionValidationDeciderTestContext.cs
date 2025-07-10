using System;
using Enterprise.Customs.EU.Business.Declaration;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class EntryInstructionValidationDeciderTestContext : DeclarationValidationDeciderTestContext<IEntryInstructionValidationDecider>
	{
		public EntryInstructionValidationDeciderTestContext(JobDeclaration declaration, bool isUCC6, params Type[] ruleDeciderInterfaceTypes)
			: base(declaration, isUCC6, ruleDeciderInterfaceTypes)
		{
			InitializeConfiguration();
		}

		protected override void SetupConfiguration(Mock<DeclarationConfiguration> configurationMock, IEntryInstructionValidationDecider validationDecider)
		{
			base.SetupConfiguration(configurationMock, validationDecider);

			var instructionConfigurationMock = new Mock<InstructionConfiguration>();

			_ = instructionConfigurationMock.Protected()
				.Setup<IEntryInstructionValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<CusEntryInstruction>())
				.Returns(validationDecider);

			_ = configurationMock.Protected()
				.Setup<InstructionConfiguration>("GetNewInstructionConfiguration")
				.Returns(instructionConfigurationMock.Object);
		}
	}
}
