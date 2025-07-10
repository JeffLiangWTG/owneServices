using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(WineCodeDataCollection))]
	sealed class WineCodeDataCollectionTest : CusCodeDataCollectionTest<WineCodeData>
	{
		public void TestAllowNewCore()
		{
			var invoiceLine = Factory.New<EMCSJobComInvoiceLine>();
			var collection = new WineCodeDataCollection(invoiceLine);

			Assert("Should default to true when the invoice line dont have any parent.", collection.AllowNew);

			var declaration = Factory.New<EMCSJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();

			collection = new WineCodeDataCollection(invoiceLine);

			declaration.JE_MessageStatus = string.Empty;
			Factory.InvalidateCachedProperties();

			Assert("Should default to true.", collection.AllowNew);

			declaration.JE_MessageStatus = EDIMessage.Status.Sent;
			Factory.InvalidateCachedProperties();

			Assert("Should not allow new when the message status of parent is SNT.", !collection.AllowNew);

			declaration.JE_MessageStatus = EDIMessage.Status.Acknowledged;
			Factory.InvalidateCachedProperties();

			Assert("Should not allow new when the message status of parent is ACK.", !collection.AllowNew);
		}

		protected override CusCodeDataCollection<WineCodeData> GetCusCodeDataCollection()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			return new WineCodeDataCollection(invoiceLine);
		}
	}
}
