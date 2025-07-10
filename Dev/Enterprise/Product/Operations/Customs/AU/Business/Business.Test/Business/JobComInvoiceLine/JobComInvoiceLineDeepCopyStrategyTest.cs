using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobComInvoiceLineDeepCopyStrategyTest : TestCaseWithFactory
	{
		public void TestCloneInvoiceLine()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			declaration.IsAQISCertificateRequest = true;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			RFPNumber number = invoiceLine.RFPNumbers.AddNew();
			number.ZA_RFPNumber = "TEST1";
			Factory.Save();
			AssertNotNull(invoiceLine.QuarantineExDocLine);

			var templateCopiedDec = (JobDeclaration)declaration.TemplateCopy();
			AssertEquals("Cloned Invoices count", 1, templateCopiedDec.Invoices.Count);
			AssertEquals("Cloned InvoiceLines count", 1, templateCopiedDec.InvoiceLines.Count);
			AssertEquals("Cloned IsAQISCertificateRequest", declaration.IsAQISCertificateRequest, templateCopiedDec.IsAQISCertificateRequest);
			AssertEquals("Cloned RFPNumbers count", invoiceLine.RFPNumbers.Count, templateCopiedDec.InvoiceLines[0].RFPNumbers.Count);
			AssertEquals("Cloned RFPNumber", number.ZA_RFPNumber, templateCopiedDec.InvoiceLines[0].RFPNumbers[0].ZA_RFPNumber);

			var filter = new ZQuery(QuarantineExDocLineSchema.QL_JI, SQLComparisonOperator.Equal, templateCopiedDec.InvoiceLines[0].PK);
			AssertNotNull(Factory.LoadTop1<QuarantineExDocLine>(filter));

			MasterFiles.Business.GlbCompany.CurrentCompany.SetCountry("NZ");
			Customs.Business.BaseJobDeclaration countryToCountryCopiedDec = (Customs.Business.BaseJobDeclaration)new Customs.Business.JobDeclarationDeepCloneStrategy(declaration, Customs.Business.CloneType.CountryToCountryCopy).Clone();
			AssertEquals("Country To Country Copied Invoices count", 1, countryToCountryCopiedDec.Invoices.Count);
			AssertEquals("Country To Country Copied InvoiceLines count", 1, countryToCountryCopiedDec.InvoiceLines.Count);

			filter = new ZQuery(QuarantineExDocLineSchema.QL_JI, SQLComparisonOperator.Equal, countryToCountryCopiedDec.InvoiceLines[0].PK);
			AssertNull("Should not copy ExDoc for other countries' declaration", Factory.LoadTop1<QuarantineExDocLine>(filter));
		}
	}
}
