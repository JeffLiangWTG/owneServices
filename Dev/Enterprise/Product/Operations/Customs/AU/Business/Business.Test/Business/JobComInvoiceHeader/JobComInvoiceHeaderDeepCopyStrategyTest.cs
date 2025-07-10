using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobComInvoiceHeaderDeepCopyStrategyTest : TestCaseWithFactory
	{
		public void TestCloneInvoice()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AQISResponsiblePerson.OrganisationPK = org.PK;
			invoiceHeader.AQISTransitDestination.OrganisationPK = org.PK;
			AssertNotNull(invoiceHeader.QuarantineExDocHeader);
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			JobDeclaration templateCopiedDec = (JobDeclaration)declaration.TemplateCopy();
			AssertEquals(1, templateCopiedDec.Invoices.Count);
			var clonedInvoice = templateCopiedDec.Invoices[0];
			AssertEquals(org.PK, clonedInvoice.AQISResponsiblePerson.OrganisationPK);
			AssertEquals(org.PK, clonedInvoice.AQISTransitDestination.OrganisationPK);
			AssertEquals(1, templateCopiedDec.InvoiceLines.Count);

			ZQuery filter = new ZQuery(QuarantineExDocHeaderSchema.QH_JZ, SQLComparisonOperator.Equal, templateCopiedDec.Invoices[0].PK);
			AssertNotNull(Factory.LoadTop1<QuarantineExDocHeader>(filter));

			MasterFiles.Business.GlbCompany.CurrentCompany.SetCountry("NZ");
			Customs.Business.BaseJobDeclaration countryToCountryCopiedDec = (Customs.Business.BaseJobDeclaration)new Customs.Business.JobDeclarationDeepCloneStrategy(declaration, Enterprise.Customs.Business.CloneType.CountryToCountryCopy).Clone();
			AssertEquals(1, countryToCountryCopiedDec.Invoices.Count);
			AssertEquals(1, countryToCountryCopiedDec.InvoiceLines.Count);

			filter = new ZQuery(QuarantineExDocHeaderSchema.QH_JZ, SQLComparisonOperator.Equal, countryToCountryCopiedDec.Invoices[0].PK);
			AssertNull("Should not copy ExDoc for other countries' declaration", Factory.LoadTop1<QuarantineExDocHeader>(filter));
		}
	}
}
