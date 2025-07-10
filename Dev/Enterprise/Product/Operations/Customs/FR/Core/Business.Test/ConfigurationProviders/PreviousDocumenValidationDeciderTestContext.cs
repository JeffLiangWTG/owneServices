using System;
using Enterprise.Customs.FR.Business.Declaration;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.FR.Business.Testing;

public class PreviousDocumentValidationDeciderTestContext : EU.Business.Testing.DeclarationValidationDeciderTestContext<IPreviousDocumentValidationDecider>
{
	public PreviousDocumentValidationDeciderTestContext(EU.Business.Declaration.JobDeclaration declaration, bool isUCC6, params Type[] ruleDeciderInterfaceTypes) : base(declaration, isUCC6, ruleDeciderInterfaceTypes)
	{
	}

	protected override void SetupConfiguration(Mock<EU.Business.DeclarationConfiguration> configuration, IPreviousDocumentValidationDecider validationDecider)
	{
		base.SetupConfiguration(configuration, validationDecider);

		configuration.Protected()
			.Setup<EU.Business.Declaration.MultiLineAddInfos.IPreviousDocumentValidationDecider>("GetPreviousDocumentValidationDeciderCore", ItExpr.IsAny<JobDeclaration>())
			.Returns(validationDecider);

		var instructionConfigurationMock = new Mock<InstructionConfiguration>();
		instructionConfigurationMock.Protected()
			.Setup<EU.Business.Declaration.MultiLineAddInfos.IPreviousDocumentValidationDecider>("GetPreviousDocumentValidationDeciderCore", ItExpr.IsAny<CusEntryInstruction>())
			.Returns(validationDecider);
		configuration.Protected()
			.Setup<EU.Business.InstructionConfiguration>("GetNewInstructionConfiguration")
			.Returns(instructionConfigurationMock.Object);

		var invoiceConfigurationMock = new Mock<InvoiceHeaderConfiguration>();
		invoiceConfigurationMock.Protected()
			.Setup<EU.Business.Declaration.MultiLineAddInfos.IPreviousDocumentValidationDecider>("GetPreviousDocumentValidationDeciderCore", ItExpr.IsAny<JobComInvoiceHeader>())
			.Returns(validationDecider);
		configuration.Protected()
			.Setup<EU.Business.InvoiceHeaderConfiguration>("GetNewInvoiceHeaderConfiguration")
			.Returns(invoiceConfigurationMock.Object);

		var invoiceLineConfigurationMock = new Mock<InvoiceLineConfiguration>();
		invoiceLineConfigurationMock.Protected()
			.Setup<EU.Business.Declaration.MultiLineAddInfos.IPreviousDocumentValidationDecider>("GetPreviousDocumentValidationDeciderCore", ItExpr.IsAny<JobComInvoiceLine>())
			.Returns(validationDecider);
		configuration.Protected()
			.Setup<EU.Business.InvoiceLineConfiguration>("GetNewInvoiceLineConfiguration")
			.Returns(invoiceLineConfigurationMock.Object);
	}
}
