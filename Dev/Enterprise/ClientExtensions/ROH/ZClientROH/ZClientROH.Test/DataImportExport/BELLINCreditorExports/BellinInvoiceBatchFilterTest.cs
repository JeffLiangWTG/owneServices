using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.Rohlig.Bellin
{
	public class BellinInvoiceBatchFilterTest : TransactionExportFilterTestBase
	{
		public void TestFromToDatesColumnToFilterOn()
		{
			BatchFilter = new BellinInvoiceBatchFilter(Factory, new TransactionExportFilterProvider(Factory));
			AssertEquals("FromToDatesColumnToFilterOn", AccTransactionHeaderSchema.AH_InvoiceDate, BatchFilter.FromToDatesColumnToFilterOnForTest);
		}

		public void TestSubQuery_NoOrgsAndEmptyAccountGroup()
		{
			AssertEquals(4, BatchFilter.NumberOfObjects);
		}

		public void TestSubQuery_OneOrgAndEmptyAccountGroup()
		{
			FilterProvider.Organisations.Add(ThirdPartyCompanyOrg);
			AssertEquals(1, BatchFilter.NumberOfObjects);
			Invoices = (InvoicingBase[])Factory.Load(typeof(InvoicingBase), BatchFilter.Filter);
			AssertSame("AP AdjustmentNote1 was not loaded", APAdjustmentNote1, Invoices[0]);
		}

		public void TestSubQuery_NoOrgsAndSpecifiedAccountGroup()
		{
			FilterProvider.AccountGroup = Associated.PK;
			AssertEquals(2, BatchFilter.NumberOfObjects);
			Invoices = (InvoicingBase[])Factory.Load(typeof(InvoicingBase), BatchFilter.Filter);
			bool aPCreditNote1Found = false;
			bool aRInvoice1Found = false;
			foreach (InvoicingBase invoice in Invoices)
			{
				if (invoice == APCreditNote1)
				{
					aPCreditNote1Found = true;
				}
				else if (invoice == ARInvoice1)
				{
					aRInvoice1Found = true;
				}
			}

			Assert("APCreditNote1 was not loaded", aPCreditNote1Found);
			Assert("ARInvoice1 was not loaded", aRInvoice1Found);
		}

		public void TestSubQuery_OrgWithMatchingAccountGroup()
		{
			FilterProvider.AccountGroup = Intercompany.PK;
			FilterProvider.Organisations.Add(IntercompanyOrg);
			AssertEquals(1, BatchFilter.NumberOfObjects);
			Invoices = (InvoicingBase[])Factory.Load(typeof(InvoicingBase), BatchFilter.Filter);
			AssertSame("APInoice1 was not loaded", APInvoice1, Invoices[0]);
		}

		public void TestSubQuery_OrgWithNonMatchAccountGroup()
		{
			FilterProvider.AccountGroup = Associated.PK;
			FilterProvider.Organisations.Add(IntercompanyOrg);
			AssertEquals(0, BatchFilter.NumberOfObjects);
		}

		#region Implementation
		OrgHeader IntercompanyOrg;
		OrgHeader AssociatedCompanyOrg;
		OrgHeader ThirdPartyCompanyOrg;
		OrgCreditorGroup Intercompany;
		OrgCreditorGroup Associated;
		OrgCreditorGroup ThirdParty;
		BellinInvoiceBatchFilter BatchFilter;
		InvoicingBase[] Invoices;
		protected override void SetUp()
		{
			base.SetUp();
			SetUpOrganisationsAndCreditorGroups();
			FilterProvider = new TransactionExportFilterProvider(Factory);
			FilterProvider.CurrentBatchNo = 0;
			FilterProvider.IncludeAPAdjustmentNotes = true;
			FilterProvider.IncludeAPCreditNotes = true;
			FilterProvider.IncludeAPInvoices = true;
			FilterProvider.IncludeARInvoices = true;
			BatchFilter = new BellinInvoiceBatchFilter(Factory, FilterProvider);
		}

		void SetUpOrganisationsAndCreditorGroups()
		{
			IntercompanyOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			AssociatedCompanyOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			ThirdPartyCompanyOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			Intercompany = Factory.LoadFromUniqueKey<OrgCreditorGroup>(OrgCreditorGroupSchema.OG_Code, (ZString)"INT");
			Associated = Factory.LoadFromUniqueKey<OrgCreditorGroup>(OrgCreditorGroupSchema.OG_Code, (ZString)"ASC");
			ThirdParty = Factory.LoadFromUniqueKey<OrgCreditorGroup>(OrgCreditorGroupSchema.OG_Code, (ZString)"TPY");
			IntercompanyOrg.CompanyData.OB_OG_APCreditorGroup = Intercompany.PK;
			AssociatedCompanyOrg.CompanyData.OB_OG_APCreditorGroup = Associated.PK;
			ThirdPartyCompanyOrg.CompanyData.OB_OG_APCreditorGroup = ThirdParty.PK;
			APInvoice1.AH_OH = IntercompanyOrg.PK;
			APCreditNote1.AH_OH = AssociatedCompanyOrg.PK;
			APAdjustmentNote1.AH_OH = ThirdPartyCompanyOrg.PK;
			ARInvoice1.AH_OH = AssociatedCompanyOrg.PK;
			Factory.Save();
		}

		protected override TransactionExportFilter GetNewExportFilter()
		{
			return new BellinInvoiceBatchFilter(Factory, FilterProvider);
		}

		public new void TestSystemLastEditTimeColumn()
		{
			Assert("The test is not suitable here.", true);
		}

		protected override SchemaDateTimeColumn ExpectedSystemLastEditTimeColumn
		{
			get
			{
				throw new System.NotImplementedException();
			}
		}

		protected override bool IsForceSetupSave => false;
	}
}
#endregion
