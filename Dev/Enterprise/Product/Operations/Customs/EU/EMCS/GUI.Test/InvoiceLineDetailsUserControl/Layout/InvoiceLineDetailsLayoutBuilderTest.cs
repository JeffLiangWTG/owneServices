using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	[TestedType(typeof(InvoiceLineDetailsLayoutBuilder<EMCSJobComInvoiceLine>))]
	sealed class InvoiceLineDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<InvoiceLineDetailsLayoutBuilder<EMCSJobComInvoiceLine>, EMCSJobComInvoiceLine, InvoiceLineDetailsControlBag>
	{
		protected override InvoiceLineDetailsLayoutBuilder<EMCSJobComInvoiceLine> GetColumnLayoutBuilderForTesting() => new InvoiceLineDetailsLayoutBuilder<EMCSJobComInvoiceLine>();

		protected override bool ExpectedNarrowColumnForMediumControls => true;

		protected override int ExpectedMaxColumns => 3;
	}
}
