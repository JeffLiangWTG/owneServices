using System;
using Enterprise.Customs.EU.Business.Declaration;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class InvoiceHeaderValidationDeciderTestContext : DeclarationValidationDeciderTestContext<IInvoiceHeaderValidationDecider>
	{
		public InvoiceHeaderValidationDeciderTestContext(JobDeclaration declaration, bool isUCC6, params Type[] ruleDeciderInterfaceTypes)
			: base(declaration, isUCC6, ruleDeciderInterfaceTypes)
		{
		}

		protected override void SetupConfiguration(Mock<DeclarationConfiguration> configurationMock, IInvoiceHeaderValidationDecider validationDecider)
		{
			base.SetupConfiguration(configurationMock, validationDecider);

			var invoiceHeaderConfiguration = new Mock<InvoiceHeaderConfiguration>();

			_ = invoiceHeaderConfiguration.Protected()
				.Setup<IInvoiceHeaderValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<JobComInvoiceHeader>())
				.CallBase();

			_ = invoiceHeaderConfiguration.Protected()
				.SetupGet<IInvoiceHeaderValidationDecider>("UCC6ImportInvoiceHeaderValidationDecider")
				.Returns(() => validationDecider);

			_ = configurationMock.Protected()
				.Setup<InvoiceHeaderConfiguration>("GetNewInvoiceHeaderConfiguration")
				.Returns(invoiceHeaderConfiguration.Object);
		}
	}
}
