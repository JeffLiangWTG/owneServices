using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocInvoiceHasChargeCodeCollection))]
	sealed class DocInvoiceHasChargeCodeCollectionTests : GenericWrapperCollectionTest<DocInvoiceHasChargeCodeCollection>
	{
		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			InvoiceHasChargeCode invoiceHasChargeCode = new InvoiceHasChargeCode("FRT", ZBool.True);
			return DocInvoiceHasChargeCode.New(invoiceHasChargeCode, Factory);
		}

		protected override DocInvoiceHasChargeCodeCollection GetNewDocumentWrapperCollection()
		{
			DocARInvoiceLineCollection linesForInvoice = null;
			return new DocInvoiceHasChargeCodeCollection(linesForInvoice, Factory);
		}

		public override void TestTypedStringIndexer()
		{
			var chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "AAA";
			var chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Code = "BBB";
			ARInvoice invoice = Factory.New<ARInvoice>();
			var line1 = (ARInvoiceLine)invoice.Lines.AddNew();
			line1.AL_AC = chargeCode1.PK;
			var line2 = (ARInvoiceLine)invoice.Lines.AddNew();
			line2.AL_AC = chargeCode2.PK;
			var docInvoice = DocARInvoice.New(invoice, Factory);
			DocInvoiceHasChargeCodeCollection collection = new DocInvoiceHasChargeCodeCollection(docInvoice.LinesForInvoice, Factory);
			Assert(collection["AAA"].Exists);
			Assert(collection["BBB"].Exists);
			Assert(!collection["CCC"].Exists);
		}
	}
}
