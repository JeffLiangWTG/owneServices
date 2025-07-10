using System;
using System.Linq.Expressions;
using Enterprise.Customs.EU.Business.Testing;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class EntryInstructionValidationDeciderTestContext : IDisposable
	{
		public EntryInstructionValidationDeciderTestContext(CusEntryInstruction entryInstruction, bool isUCC6)
		{
			validationDeciderMock = new Mock<IEntryInstructionValidationDecider>();
			var instructionConfiguration = new Mock<InstructionConfiguration>();
			instructionConfiguration.Protected()
				.Setup<EU.Business.Declaration.IEntryInstructionValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<CusEntryInstruction>())
				.Returns<CusEntryInstruction>(o => isUCC6 && o.JobDeclaration.IsImport ? validationDeciderMock.Object : null);

			cleanup = ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(entryInstruction.JobDeclaration, "GetNew" + nameof(InstructionConfiguration), instructionConfiguration?.Object), isUCC6);
		}

		public void EnableRule(Expression<Func<IEntryInstructionValidationDecider, bool>> rule)
		{
			validationDeciderMock.SetupGet(rule).Returns(true);
		}

		public void DisableRule(Expression<Func<IEntryInstructionValidationDecider, bool>> rule)
		{
			validationDeciderMock.SetupGet(rule).Returns(false);
		}

		public void Dispose() => cleanup.Dispose();

		readonly IDisposable cleanup;
		readonly Mock<IEntryInstructionValidationDecider> validationDeciderMock;
	}
}
