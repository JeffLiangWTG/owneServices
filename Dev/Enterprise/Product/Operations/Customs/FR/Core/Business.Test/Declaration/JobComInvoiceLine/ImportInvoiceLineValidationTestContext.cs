using System;
using Enterprise.Customs.EU.Business.Testing;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class ImportInvoiceLineValidationTestContext : DeclarationValidationDeciderTestContext<IImportInvoiceLineValidationDecider>
	{
		public ImportInvoiceLineValidationTestContext(JobComInvoiceLine jobComInvoiceLine, params Type[] ruleDeciderInterfaceTypes) : base(jobComInvoiceLine.Declaration, true, ruleDeciderInterfaceTypes)
		{
			InitializeConfiguration();
		}

		protected override void SetupConfiguration(Mock<EU.Business.DeclarationConfiguration> configurationMock, IImportInvoiceLineValidationDecider validationDecider)
		{
			base.SetupConfiguration(configurationMock, validationDecider);

			var invoiceLineConfigurationMock = new Mock<InvoiceLineConfiguration>();

			_ = invoiceLineConfigurationMock.Protected()
				.Setup<EU.Business.Declaration.IInvoiceLineValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<JobComInvoiceLine>())
				.Returns(validationDecider);

			_ = configurationMock.Protected()
				.Setup<EU.Business.InvoiceLineConfiguration>("GetNewInvoiceLineConfiguration")
				.Returns(invoiceLineConfigurationMock.Object);
		}
	}
}
