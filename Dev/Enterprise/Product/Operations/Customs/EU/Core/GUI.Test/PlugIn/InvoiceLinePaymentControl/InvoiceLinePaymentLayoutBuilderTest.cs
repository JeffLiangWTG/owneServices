using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.Plugin;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(InvoiceLinePaymentLayoutBuilder<JobComInvoiceLine>))]
	class InvoiceLinePaymentLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<InvoiceLinePaymentLayoutBuilder<JobComInvoiceLine>, JobComInvoiceLine, InvoiceLinePaymentControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override InvoiceLinePaymentLayoutBuilder<JobComInvoiceLine> GetColumnLayoutBuilderForTesting() => new InvoiceLinePaymentLayoutBuilder<JobComInvoiceLine>();
	}
}
