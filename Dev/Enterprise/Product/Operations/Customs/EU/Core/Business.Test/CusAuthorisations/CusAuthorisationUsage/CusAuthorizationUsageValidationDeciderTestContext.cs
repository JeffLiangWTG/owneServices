using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.Testing;

class CusAuthorizationUsageValidationDeciderTestContext : DeclarationValidationDeciderTestContext<ICusAuthorizationUsageValidationDecider>
{
	public CusAuthorizationUsageValidationDeciderTestContext(JobDeclaration declaration, bool isUCC6, bool isPopulateAuthorisationsForOfficeOfPresentationEnabled = false, params Type[] ruleDeciderInterfaceTypes)
		: base(declaration, isUCC6, ruleDeciderInterfaceTypes)
	{
		this.isPopulateAuthorisationsForOfficeOfPresentationEnabled = isPopulateAuthorisationsForOfficeOfPresentationEnabled;
		InitializeConfiguration();
	}

	protected override void SetupConfiguration(Mock<DeclarationConfiguration> configurationMock, ICusAuthorizationUsageValidationDecider validationDecider)
	{
		base.SetupConfiguration(configurationMock, validationDecider);

		var invoiceLineConfigurationMock = new Mock<InvoiceLineConfiguration>();
		_ = invoiceLineConfigurationMock.Protected()
			.Setup<ICusAuthorizationUsageValidationDecider>("GetCusAuthorizationUsageValidationDeciderCore", ItExpr.IsAny<JobComInvoiceLine>())
			.Returns(validationDecider);

		var instructionConfigurationMock = new Mock<InstructionConfiguration>();
		_ = instructionConfigurationMock.Protected()
			.Setup<ICusAuthorizationUsageValidationDecider>("GetCusAuthorizationUsageValidationDeciderCore", ItExpr.IsAny<CusEntryInstruction>())
			.Returns(validationDecider);

		_ = configurationMock.Protected()
			.Setup<InvoiceLineConfiguration>("GetNewInvoiceLineConfiguration")
			.Returns(invoiceLineConfigurationMock.Object);

		_ = configurationMock.Protected()
			.Setup<InstructionConfiguration>("GetNewInstructionConfiguration")
			.Returns(instructionConfigurationMock.Object);

		_ = configurationMock.Protected()
			.Setup<ZBool>("IsPopulateAuthorisationsForOfficeOfPresentationEnabledCore", ItExpr.IsAny<JobDeclaration>())
			.Returns(isPopulateAuthorisationsForOfficeOfPresentationEnabled);
	}

	readonly bool isPopulateAuthorisationsForOfficeOfPresentationEnabled;
}
