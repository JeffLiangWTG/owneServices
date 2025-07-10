using System;
using Enterprise.Customs.EU.Business.Declaration;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.Testing;

sealed class InvoiceLinePackageValidationDeciderTestContext : DeclarationValidationDeciderTestContext<IInvoiceLinePackageValidationDecider>
{
	public InvoiceLinePackageValidationDeciderTestContext(JobDeclaration declaration, bool isUCC6, params Type[] ruleDeciderInterfaceTypes)
		: base(declaration, isUCC6, ruleDeciderInterfaceTypes)
	{
		InitializeConfiguration();
	}

	protected override void SetupConfiguration(Mock<DeclarationConfiguration> configurationMock, IInvoiceLinePackageValidationDecider validationDecider)
	{
		_ = configurationMock.Protected()
			.Setup<IInvoiceLinePackageValidationDecider>("GetInvoiceLinePackageValidationDeciderCore")
			.Returns(validationDecider);
	}
}
