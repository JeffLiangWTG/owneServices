using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocARComplianceDocumentLine))]
	sealed class DocARComplianceDocumentLineTest : DocumentWrapperTestCase
	{
		public void TestProperties()
		{
			AssertEquals("Description", ComplianceDocumentLine.ADL_Description, Wrapper.Description);
			AssertEquals("Amount", ComplianceDocumentLine.Amount, Wrapper.Amount);
			AssertEquals("LocalAmount", ComplianceDocumentLine.LocalAmount, Wrapper.LocalAmount);
			AssertEquals("LocalTaxAmount", ComplianceDocumentLine.LocalTaxAmount, Wrapper.LocalTaxAmount);
			AssertEquals("Sequence", ComplianceDocumentLine.ADL_Sequence, Wrapper.Sequence);
		}

		public void TestTaxCategory()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "TX1";

			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			var arInvoiceLine1 = (AccTransactionLines)arInvoice.Lines.AddNew();
			arInvoiceLine1.AL_AT = taxRate.PK;
			arInvoiceLine1.AL_GSTVAT = 0m;
			var arInvoiceLine2 = (AccTransactionLines)arInvoice.Lines.AddNew();
			arInvoiceLine2.AL_AT = taxRate.PK;
			arInvoiceLine2.AL_GSTVAT = 5m;

			var complianceDocumentHeader = TestObjectCreator.CreateComplianceDocumentHeader(LedgerTypes.AccountsReceivable, "desc", "ABC");
			var complianceDocumentLine1 = TestObjectCreator.CreateComplianceDocumentLine(complianceDocumentHeader, "Test1");
			var complianceDocumentLine2 = TestObjectCreator.CreateComplianceDocumentLine(complianceDocumentHeader, "Test2");
			TestObjectCreator.CreateComplianceDocumentPivot(complianceDocumentLine1, arInvoiceLine1);
			TestObjectCreator.CreateComplianceDocumentPivot(complianceDocumentLine2, arInvoiceLine2);
			complianceDocumentHeader.ADH_DocumentNumber = "D00001";

			var wrapper1 = DocARComplianceDocumentLine.New(complianceDocumentLine1, Factory);
			var wrapper2 = DocARComplianceDocumentLine.New(complianceDocumentLine2, Factory);

			AssertComplianceWithDifferentTaxCode(wrapper1);
			AssertComplianceWithDifferentTaxCode(wrapper2);

			void AssertComplianceWithDifferentTaxCode(DocARComplianceDocumentLine wrapper)
			{
				taxRate.AT_Code = "VAT";
				AssertEquals(true, wrapper.Taxable);
				AssertEquals(false, wrapper.ZeroRated);
				AssertEquals(false, wrapper.TaxExempt);

				taxRate.AT_Code = "CAPVAT";
				AssertEquals(true, wrapper.Taxable);
				AssertEquals(false, wrapper.ZeroRated);
				AssertEquals(false, wrapper.TaxExempt);

				taxRate.AT_Code = "FREEVAT";
				AssertEquals(false, wrapper.Taxable);
				AssertEquals(true, wrapper.ZeroRated);
				AssertEquals(false, wrapper.TaxExempt);

				taxRate.AT_Code = "EXEMPT";
				AssertEquals(false, wrapper.Taxable);
				AssertEquals(false, wrapper.ZeroRated);
				AssertEquals(true, wrapper.TaxExempt);
			}
		}

		[TestDate(2019, 1, 17)]
		public void TestINVDocumentDateForCRD()
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);
			var invoice = (ARInvoice)creator.CreateInvoiceWithLine(typeof(ARInvoice), "0001", creator.AUD, 1.0m, 100m, 0m, 100m, 0m);
			var creditNote = creator.CreateARCreditNoteWithLine("0002", creator.ABIGAS, creator.AUD, 1m, "Desc", null, creator.CC1, 100m, ZDateTime.Today, false);
			creditNote.OriginalTransactionReference = invoice.PK;
			factory.Save();

			var invoiceDocumentHeader = creator.CreateComplianceDocumentHeader(LedgerTypes.AccountsReceivable, "Des", "ABC");
			var invoicecomplianceDocumentLine = creator.CreateComplianceDocumentLine(invoiceDocumentHeader, "Test");
			creator.CreateComplianceDocumentPivot(invoicecomplianceDocumentLine, invoice.Lines[0]);
			invoiceDocumentHeader.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			invoiceDocumentHeader.ADH_DocumentNumber = "D00001";

			var creditNoteDocumentHeader = creator.CreateComplianceDocumentHeader(LedgerTypes.AccountsReceivable, "Des", "DEF");
			var creditNotecomplianceDocumentLine = creator.CreateComplianceDocumentLine(creditNoteDocumentHeader, "Test");
			creator.CreateComplianceDocumentPivot(creditNotecomplianceDocumentLine, creditNote.Lines[0]);
			creditNoteDocumentHeader.ADH_OH_Organisation = TestObjectCreator.Debtor.PK;
			creditNoteDocumentHeader.ADH_DocumentNumber = "D00001";
			factory.Save();

			var wrapper = DocARComplianceDocumentLine.New(creditNotecomplianceDocumentLine, Factory);
			AssertNotNull(creditNoteDocumentHeader.INVComplianceDocumentHeaderForCRD);
			AssertEquals("2019", wrapper.INVDocumentDateYearForCRD.ToString());
			AssertEquals("1", wrapper.INVDocumentDateMonthForCRD.ToString());
			AssertEquals("17", wrapper.INVDocumentDateDayForCRD.ToString());
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocARComplianceDocumentLine.New(ComplianceDocumentLine, Factory) };
		}

		AccComplianceDocumentHeader ComplianceDocumentHeader;
		AccComplianceDocumentLine ComplianceDocumentLine;
		DocARComplianceDocumentLine Wrapper;
		ARInvoice Invoice;

		protected override void SetUp()
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "SEU";
			Invoice = Factory.NewWithValidTestData<ARInvoice>();
			var invoiceLine = (ARInvoiceLine)Invoice.Lines.AddNew();
			invoiceLine.AL_AT = taxRate.PK;
			ComplianceDocumentHeader = TestObjectCreator.CreateComplianceDocumentHeader(LedgerTypes.AccountsReceivable, "Des", "ABC");
			ComplianceDocumentLine = TestObjectCreator.CreateComplianceDocumentLine(ComplianceDocumentHeader, "Test");
			TestObjectCreator.CreateComplianceDocumentPivot(ComplianceDocumentLine, invoiceLine);
			ComplianceDocumentHeader.ADH_DocumentNumber = "D00001";
			Wrapper = (DocARComplianceDocumentLine)GetDocumentWrappers()[0];

			base.SetUp();
		}
	}
}
