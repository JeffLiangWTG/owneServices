using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class InvoiceLineOverrideAdaptorTest<T> : NonPersistentBusinessObjectTestCase where T : InvoiceLineOverride
	{
		TestObjectCreator testObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}

		public void TestGetOverrides()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 100, 10, 0);
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 200, 20, 0);
			var line3 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1, 300, 30, 0);

			var adaptor = GetAdaptor(Factory, new[] { line1.PK, line2.PK });

			AssertEquals(2, adaptor.WrappedObjects.Count);

			var overrideForLine1 = adaptor.WrappedObjects.Cast<T>().First(o => o.LineForTest == line1);
			AssertNotNull(overrideForLine1);
			AssertEquals(110m, overrideForLine1.AL_OSAmount);

			var overrideForLine2 = adaptor.WrappedObjects.Cast<T>().First(o => o.LineForTest == line2);
			AssertNotNull(overrideForLine2);
			AssertEquals(220m, overrideForLine2.AL_OSAmount);
		}

		protected abstract InvoiceLineOverrideAdaptor<T> GetAdaptor(BusinessObjectFactory factory, params ZGuid[] linePks);
	}
}
