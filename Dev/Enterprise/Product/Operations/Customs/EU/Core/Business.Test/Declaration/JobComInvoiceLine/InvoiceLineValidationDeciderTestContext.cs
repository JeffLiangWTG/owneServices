using System;
using Enterprise.Customs.EU.Business.Declaration;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class InvoiceLineValidationDeciderTestContext<T> : DeclarationValidationDeciderTestContext<T>
		where T : class, IInvoiceLineValidationDecider
	{
		public InvoiceLineValidationDeciderTestContext(JobDeclaration declaration, params Type[] ruleDeciderInterfaceTypes)
			: base(declaration, isUCC6: false, ruleDeciderInterfaceTypes)
		{
		}

		protected override void SetupConfiguration(Mock<DeclarationConfiguration> configurationMock, T validationDecider)
		{
			base.SetupConfiguration(configurationMock, validationDecider);

			var invoiceLineConfiguration = new Mock<InvoiceLineConfiguration>();
			_ = invoiceLineConfiguration.Protected()
				.Setup<IInvoiceLineValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<JobComInvoiceLine>())
				.Returns(validationDecider);

			_ = configurationMock.Protected()
				.Setup<InvoiceLineConfiguration>("GetNewInvoiceLineConfiguration")
				.Returns(invoiceLineConfiguration.Object);
		}
	}
}
