using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.NctsAndDeclarationIntegration;

namespace Enterprise.Customs.EU.Business.Testing
{
	public abstract class EntryNumberFormatterForNctsAndDeclarationIntegrationTestBase<TInvoiceLine, TFormatter> : TestCaseWithFactory
		where TInvoiceLine : JobComInvoiceLine
		where TFormatter : IEntryNumberFormatterForNctsAndDeclarationIntegration
	{
		public abstract void TestFormatEntryNumber();

		public void TestFormatterType()
		{
			var invoiceLine = Factory.New<TInvoiceLine>();
			AssertType<TFormatter>(invoiceLine.GetEntryNumberFormatter());
		}

		protected void AssertTupleEquals((ZString v1, ZString v2, ZString v3, ZInt? v4) expected, (ZString v1, ZString v2, ZString v3, ZInt? v4) actual) => AssertEquals(expected, actual);
	}
}
