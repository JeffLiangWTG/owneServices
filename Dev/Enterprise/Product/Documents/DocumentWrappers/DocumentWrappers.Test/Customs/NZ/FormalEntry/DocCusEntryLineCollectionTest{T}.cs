using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.NZ.FormalEntry.Testing
{
	abstract class DocCusEntryLineCollectionTest<T> : NonPersistentBusinessObjectCollectionTestCase<T> where T : DocCusEntryLineCollection
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocCusEntryLine.New(Factory);
		}

		public void TestCollectionPadsItselfOutToOneLineAtLeast()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();

			AssertEquals("CusEntryLineCollection CusEntryLine Count", 0, declaration.CusEntryHeader.MergedLines.Count);

			DocCusEntryLineCollection docCusEntryLines = new DocCusEntryLineCollection(declaration.CusEntryHeader.MergedLines, Factory);

			AssertEquals("CusEntryLineCollection CusEntryLine Count", 0, declaration.CusEntryHeader.MergedLines.Count);

			AssertEquals("DocCusEntryLineCollection DocCusEntryLine Count", 1, docCusEntryLines.Count);
			AssertEquals("Make sure last line can be accessed", "", docCusEntryLines[0].LineNumber);
		}

		#region Implementation

		ZString CurrentCountry;
		protected override void SetUp()
		{
			base.SetUp();
			CurrentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry("NZ");
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.SetCountry(CurrentCountry);
		}

		#endregion

	}
}
