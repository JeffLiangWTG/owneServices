using System;
using Enterprise.Customs.FR.Business.Declaration;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.FR.Business.Testing;

public class AdditionalInfoValidationDeciderTestContext : EU.Business.Testing.DeclarationValidationDeciderTestContext<IAdditionalInfoValidationDecider>
{
	public AdditionalInfoValidationDeciderTestContext(EU.Business.Declaration.JobDeclaration declaration, bool isUCC6, params Type[] ruleDeciderInterfaceTypes) : base(declaration, isUCC6, ruleDeciderInterfaceTypes)
	{
	}

	protected override void SetupConfiguration(Mock<EU.Business.DeclarationConfiguration> configuration, IAdditionalInfoValidationDecider validationDecider)
	{
		base.SetupConfiguration(configuration, validationDecider);

		var invoiceLineConfigurationMock = new Mock<InvoiceLineConfiguration>();
		invoiceLineConfigurationMock.Protected()
			.Setup<EU.Business.Declaration.MultiLineAddInfos.IAdditionalInfoValidationDecider>("GetAdditionalInfoValidationDeciderCore", ItExpr.IsAny<JobComInvoiceLine>())
			.Returns(validationDecider);

		configuration.Protected()
			.Setup<EU.Business.InvoiceLineConfiguration>("GetNewInvoiceLineConfiguration")
			.Returns(invoiceLineConfigurationMock.Object);

		var invoiceConfigurationMock = new Mock<InvoiceHeaderConfiguration>();
		invoiceConfigurationMock.Protected()
			.Setup<EU.Business.Declaration.MultiLineAddInfos.IAdditionalInfoValidationDecider>("GetAdditionalInfoValidationDeciderCore", ItExpr.IsAny<JobComInvoiceHeader>())
			.Returns(validationDecider);
		configuration.Protected()
			.Setup<EU.Business.InvoiceHeaderConfiguration>("GetNewInvoiceHeaderConfiguration")
			.Returns(invoiceConfigurationMock.Object);

		configuration.Protected()
			.Setup<EU.Business.Declaration.MultiLineAddInfos.IAdditionalInfoValidationDecider>("GetAdditionalInfoValidationDeciderCore", ItExpr.IsAny<JobDeclaration>())
			.Returns(validationDecider);

		var instructionConfigurationMock = new Mock<InstructionConfiguration>();
		instructionConfigurationMock.Protected()
			.Setup<EU.Business.Declaration.MultiLineAddInfos.IAdditionalInfoValidationDecider>("GetAdditionalInfoValidationDeciderCore", ItExpr.IsAny<CusEntryInstruction>())
			.Returns(validationDecider);
		configuration.Protected()
			.Setup<EU.Business.InstructionConfiguration>("GetNewInstructionConfiguration")
			.Returns(instructionConfigurationMock.Object);
	}
}
