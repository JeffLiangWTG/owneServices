using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestsSubclassesOf(typeof(LineProvider))]
	abstract class LineProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T>
		where T : LineProvider
	{
		protected T LineProvider => lineProvider ?? (lineProvider = GetLineProvider());
		T lineProvider;

		protected abstract T GetLineProvider();

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			emcsInvoiceLine = emcsDeclaration.InvoiceHeader.InvoiceLines.AddNew();
		}
		protected EMCSJobDeclaration emcsDeclaration;
		protected EMCSJobComInvoiceLine emcsInvoiceLine;

		protected override T GetProvider() => LineProvider;
	}
}
