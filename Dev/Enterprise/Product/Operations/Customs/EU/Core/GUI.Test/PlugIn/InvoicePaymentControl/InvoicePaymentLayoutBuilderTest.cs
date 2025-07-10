using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.Plugin;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(InvoicePaymentLayoutBuilder<JobComInvoiceHeader>))]
	class InvoicePaymentLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<InvoicePaymentLayoutBuilder<JobComInvoiceHeader>, JobComInvoiceHeader, InvoicePaymentControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override InvoicePaymentLayoutBuilder<JobComInvoiceHeader> GetColumnLayoutBuilderForTesting() => new InvoicePaymentLayoutBuilder<JobComInvoiceHeader>();
	}
}
