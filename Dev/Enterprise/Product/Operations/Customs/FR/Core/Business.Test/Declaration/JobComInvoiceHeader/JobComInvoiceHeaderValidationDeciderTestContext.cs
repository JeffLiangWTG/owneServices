using System;
using System.Linq.Expressions;
using Enterprise.Customs.EU.Business.Testing;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	sealed class JobComInvoiceHeaderValidationDeciderTestContext : IDisposable
	{
		public JobComInvoiceHeaderValidationDeciderTestContext(JobComInvoiceHeader invoiceHeader, bool isUCC6)
		{
			validationDeciderMock = new Mock<IInvoiceHeaderValidationDecider>();
			var invoiceHeaderConfiguration = new Mock<InvoiceHeaderConfiguration>();
			invoiceHeaderConfiguration.Protected()
				.Setup<EU.Business.Declaration.IInvoiceHeaderValidationDecider>("GetValidationDeciderCore", ItExpr.IsAny<JobComInvoiceHeader>())
				.Returns<JobComInvoiceHeader>(o => isUCC6 && o.JobDeclaration.IsImport ? validationDeciderMock.Object : null);

			cleanup = ConfigurationTestHelper.TemporarySetupUCC6(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(invoiceHeader.JobDeclaration, "GetNew" + nameof(InvoiceHeaderConfiguration), invoiceHeaderConfiguration?.Object), isUCC6);
		}

		public void EnableRule(Expression<Func<IInvoiceHeaderValidationDecider, bool>> rule)
		{
			validationDeciderMock.SetupGet(rule).Returns(true);
		}

		public void DisableRule(Expression<Func<IInvoiceHeaderValidationDecider, bool>> rule)
		{
			validationDeciderMock.SetupGet(rule).Returns(false);
		}

		public void Dispose() => cleanup.Dispose();

		readonly IDisposable cleanup;
		readonly Mock<IInvoiceHeaderValidationDecider> validationDeciderMock;
	}
}
