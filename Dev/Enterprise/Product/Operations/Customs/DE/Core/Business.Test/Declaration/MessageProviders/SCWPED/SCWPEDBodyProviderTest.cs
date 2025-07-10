using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SCWPEDBodyProvider))]
	sealed class SCWPEDBodyProviderTest : MonthlyClosingDecBodyProviderAbstractTest<SCWPEDBodyProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new SCWPEDBodyProvider(null, isModificationMessage: false));
		}

		public void TestLines()
		{
			// rejected with snapshot should be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "REJ";
			reconEntryLine.CusReconSnapshots.AddNew().CRS_Type = "CUR";

			// rejected no snapshot should not be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "REJ";

			// status blank should be in result
			AddInvoiceWithInvoiceLine();

			AssertEquals(2, Provider.Lines.Count);
		}

		public void TestLinesEntryHasSnapshot()
		{
			entry.CusReconSnapshots.AddNew().CRS_Type = "CUR";

			// rejected should be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "REJ";

			// status ERR should not be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "ERR";

			// status blank should be in result
			AddInvoiceWithInvoiceLine();

			AssertEquals(2, Provider.Lines.Count);
		}

		public void TestModification()
		{
			isModificationMessage = true;

			// RC2 with snapshot should be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "RC2";
			reconEntryLine.CusReconSnapshots.AddNew().CRS_Type = "CUR";

			// TX4 with snapshot should be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "TX4";
			reconEntryLine.CusReconSnapshots.AddNew().CRS_Type = "CUR";

			// ERR with snapshot should be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "ERR";
			reconEntryLine.CusReconSnapshots.AddNew().CRS_Type = "CUR";

			// TX5 with snapshot should be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "TX5";
			reconEntryLine.CusReconSnapshots.AddNew().CRS_Type = "CUR";

			// REJ with snapshot should not be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "REJ";
			reconEntryLine.CusReconSnapshots.AddNew().CRS_Type = "CUR";

			// status blank should not be in result
			AddInvoiceWithInvoiceLine();

			AssertEquals(4, Provider.Lines.Count);
		}

		public void TestModificationEntryHasSnapshot()
		{
			isModificationMessage = true;
			entry.CusReconSnapshots.AddNew().CRS_Type = "CUR";

			// RC2 with snapshot should be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "RC2";

			// TX4 with snapshot should be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "TX4";

			// ERR with snapshot should be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "ERR";

			// TX5 with snapshot should be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "TX5";

			// REJ with snapshot should not be in result
			AddInvoiceWithInvoiceLine();
			reconEntryLine.CRL_CustomsStatus = "REJ";

			// status blank should not be in result
			AddInvoiceWithInvoiceLine();

			AssertEquals(4, Provider.Lines.Count);
		}

		protected override SCWPEDBodyProvider GetProvider() => new SCWPEDBodyProvider(entry, isModificationMessage);

		bool isModificationMessage;

		new ISCWPEDBody Provider => base.Provider;
	}
}
