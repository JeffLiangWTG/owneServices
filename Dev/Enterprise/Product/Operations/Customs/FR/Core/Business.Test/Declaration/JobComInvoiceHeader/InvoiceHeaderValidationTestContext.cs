using System;
using Enterprise.Customs.EU.Business.Testing;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class InvoiceHeaderValidationTestContext : DeclarationValidationDeciderTestContext<IInvoiceHeaderValidationDecider>
	{
		public InvoiceHeaderValidationTestContext(JobComInvoiceHeader jobComInvoiceHeader, params Type[] ruleDeciderInterfaceTypes) : base(jobComInvoiceHeader.JobDeclaration, false, ruleDeciderInterfaceTypes)
		{
			InitializeConfiguration();
		}

		protected override void SetupConfiguration(Mock<EU.Business.DeclarationConfiguration> configurationMock, IInvoiceHeaderValidationDecider validationDecider)
		{
			base.SetupConfiguration(configurationMock, validationDecider);

			var invoiceHeaderConfigurationMock = new Mock<InvoiceHeaderConfiguration>();

			_ = invoiceHeaderConfigurationMock.Protected()
				.Setup<EU.Business.Declaration.IInvoiceHeaderValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<JobComInvoiceHeader>())
				.Returns(validationDecider);

			_ = configurationMock.Protected()
				.Setup<EU.Business.InvoiceHeaderConfiguration>("GetNewInvoiceHeaderConfiguration")
				.Returns(invoiceHeaderConfigurationMock.Object);
		}
	}
}
