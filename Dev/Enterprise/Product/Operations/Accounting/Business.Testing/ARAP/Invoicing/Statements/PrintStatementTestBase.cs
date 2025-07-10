using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class PrintStatementTestBase : NonPersistentBusinessObjectTestCase
	{
		public void TestOrganisation()
		{
			PrintStatement statement = (PrintStatement)GetNewBusinessObject();
			statement.OrganisationPK = Creator.AALSHI.PK;
			AssertEquals("Organisation should be loaded correctly", Creator.AALSHI, statement.Organisation);
		}

		public void TestOrganisationPK()
		{
			PrintStatement statement = (PrintStatement)GetNewBusinessObject();
			ZGuid organisationGuid = ZGuid.NewZGuid();
			statement.OrganisationPK = organisationGuid;
			AssertEquals("Organisation PK", organisationGuid, statement.OrganisationPK);
		}

		public void TestGlbCompanyPK()
		{
			GlbBranch testBranch = Factory.New<GlbBranch>();
			GlbCompany testCompany = Factory.New<GlbCompany>();
			testBranch.GB_GC = testCompany.PK;
			PrintStatement statement = new PrintStatement(Factory, testBranch);
			AssertSame("GlbBranch", testBranch, statement.Branch);
			AssertSame("GlbCompany", testCompany, statement.Company);
		}

		public abstract void TestTransactionHeaderCollectionCommonFilters();

		protected abstract PrintStatement GetNewPrintStatement(ZGuid organisationPK, ZString currencyNK, ZString documentToPrint);

		#region Implementation

		protected Invoice CreateInvoice(OrgHeader orgToSetup)
		{
			return CreateInvoice<ARInvoice>(orgToSetup, 100M, 1.0M);
		}

		protected T CreateInvoice<T>(OrgHeader orgToSetup) where T : Invoice
		{
			return CreateInvoice<T>(orgToSetup, 100M, 1.0M);
		}

		protected Invoice CreateInvoice(OrgHeader orgToSetup, decimal invoiceAmount)
		{
			return CreateInvoice<ARInvoice>(orgToSetup, invoiceAmount, 1.0M);
		}

		protected Invoice CreateInvoice(OrgHeader orgToSetup, decimal invoiceAmount, decimal exchangeRate)
		{
			return CreateInvoice<ARInvoice>(orgToSetup, invoiceAmount, exchangeRate);
		}

		protected T CreateInvoice<T>(OrgHeader orgToSetup, decimal invoiceAmount, decimal exchangeRate) where T : Invoice
		{
			orgToSetup.CompanyData.OB_IsDebtor = true;
			orgToSetup.CompanyData.SetARTaxApplicable(false);

			T invoice = (T)Creator.CreateInvoice(typeof(T), GlbCompany.CurrentCompany.LocalCurrency, exchangeRate);
			Creator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, 1.0M, invoiceAmount);
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			invoice.AH_OH = orgToSetup.PK;
			return invoice;
		}

		protected TestObjectCreator Creator
		{
			get
			{
				if (fCreator == null)
				{
					fCreator = new TestObjectCreator(Factory);
				}

				return fCreator;
			}
		}

		TestObjectCreator fCreator;

		#endregion
	}
}
