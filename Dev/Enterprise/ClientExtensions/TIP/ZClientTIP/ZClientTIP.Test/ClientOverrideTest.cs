using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Client.TIP.Testing
{
	[TestedType(typeof(ClientOverride))]
	public class ClientOverrideTest : ZArchitecture.Modules.Testing.ClientOverrideTest
	{
		public void TestInitialiseAndUnitialise()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			CommonContainer container = factory.New<CommonContainer>();
			InvoicingLineBase line = factory.New<ARInvoiceLine>();
			InvoiceBatchHeader batch = factory.New<InvoiceBatchHeader>();
			ARInvoice transHeader = factory.New<ARInvoice>();
			ClientOverride clientOverride = ClientOverride.Instance;
			clientOverride.Uninitialise();
			AssertEquals("DocBatchARInvoiceLineTransactionLine", typeof(DocBatchARInvoiceLineTransactionLine), DocBatchARInvoiceLineTransactionLine.New(line, factory).GetType());
			AssertEquals("DocARBatchInvoice", typeof(DocARBatchInvoice), DocARBatchInvoice.New(batch, factory).GetType());
			AssertEquals("DocTransactionHeader", typeof(DocTransactionHeader), DocTransactionHeader.New(transHeader, factory).GetType());
			clientOverride.Initialise();
			AssertEquals("DocBatchARInvoiceLineTransactionLine type", typeof(DocTIPBatchARInvoiceLineTransactionLine), DocBatchARInvoiceLineTransactionLine.New(line, factory).GetType());
			AssertEquals("DocARBatchInvoice type", typeof(DocTIPARBatchInvoice), DocARBatchInvoice.New(batch, factory).GetType());
		}

		protected override Type ClientOverrideType
		{
			get
			{
				return typeof(ClientOverride);
			}
		}
	}
}
