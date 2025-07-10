using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(BillRecipient))]
	internal class BillRecipientTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOrganisationInformation()
		{
			var organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var billRecipient = new BillRecipient(Factory, Env.CurrentBranch.PK, organisation.PK, "USD", ZDateTime.Now);

			AssertEquals(organisation, billRecipient.Organisation);
			AssertEquals(organisation.LicCompany, billRecipient.LicCompany);

			AssertEquals(organisation.OH_Code, billRecipient.OrganisationCode);
			AssertEquals(organisation.OH_FullName, billRecipient.OrganisationName);
			AssertEquals(organisation.OH_RL_NKClosestPort, billRecipient.OrganisationUNLOCO);
			AssertEquals(organisation.LicEnterprise.LE_EnterpriseCode, billRecipient.EnterpriseCode);
			AssertEquals("Invoice currency", "USD", billRecipient.InvoiceCurrencyCode);
		}

		public void TestPartner()
		{
			EDIOrgHeader partner = BillingTestHelper.CreateOrganisation(Factory, "PAR");
			partner.LicCompany.SelfBilling.L4_IsPartner = true;

			LicenceHeader licHeader = BillingTestHelper.CreateLicence(Factory, "AAA");
			var billRecipient = new BillRecipient(Factory, Env.CurrentBranch.PK, licHeader.Company.LC_OH, "AUD", ZDateTime.Now);
			AssertEquals(false, billRecipient.IsInvoicedByPartner);
			Assert(billRecipient.PartnerOrgPK.IsEmpty);
			AssertNull(billRecipient.Partner);

			var delivery1 = licHeader.Company.InvoiceDeliveries.AddNew();
			delivery1.L9_IsBilled = false;
			var delivery2 = licHeader.Company.InvoiceDeliveries.AddNew();
			delivery2.L9_OH_InvoiceTo = partner.PK;

			billRecipient = new BillRecipient(Factory, Env.CurrentBranch.PK, licHeader.Company.LC_OH, "AUD", ZDateTime.Now);
			AssertEquals(true, billRecipient.IsInvoicedByPartner);
			AssertEquals(partner.PK, billRecipient.PartnerOrgPK);
			AssertEquals(partner.PK, billRecipient.Partner.PK);
		}

		public void TestValidateAll()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			organisation.CompanyData.OB_IsDebtor = true;
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var billRecipient = new BillRecipient(Factory, Env.CurrentBranch.PK, ZGuid.Empty, "USD", new ZDateTime(2016, 6, 30));
			AssertNoNotifications(billRecipient);

			billRecipient.AddRowNotifications(billRecipient);
			AssertHasRowError(billRecipient, BillRecipient.ValidationMessages.NoLicence);

			billRecipient = CreateBillRecipient(organisation.PK, Env.CurrentBranch.PK, "AUD");
			AssertNoNotifications(billRecipient);

			billRecipient = CreateBillRecipient(organisation.PK, ZGuid.Empty, "");
			AssertHasRowError(billRecipient, BillRecipient.ValidationMessages.NoBranch);

			billRecipient = CreateBillRecipient(organisation.PK, Env.CurrentBranch.PK, "");
			AssertHasRowError(billRecipient, BillRecipient.ValidationMessages.NoCurrency);

			billRecipient = CreateBillRecipient(organisation.PK, Env.CurrentBranch.PK, "AUD");
			AssertNoNotifications(billRecipient);

			billRecipient = CreateBillRecipient(organisation.PK, Env.CurrentBranch.PK, "SGD", new ZDateTime(2016, 6, 30));
			AssertHasRowError(billRecipient, "Login company EDI: exchange rate for 30-Jun-16 not found for SGD to AUD.");

			billRecipient = CreateBillRecipient(organisation.PK, Env.CurrentBranch.PK, "AUD");
			AssertNoNotifications(billRecipient);

			organisation.CompanyData.OB_IsDebtor = false;
			billRecipient = CreateBillRecipient(organisation.PK, Env.CurrentBranch.PK, "AUD");
			AssertHasRowError(billRecipient, BillRecipient.ValidationMessages.NotDebtor);

			organisation.CompanyData.OB_IsDebtor = true;
			billRecipient = CreateBillRecipient(organisation.PK, Env.CurrentBranch.PK, "AUD");
			AssertNoNotifications(billRecipient);

			organisation.OH_IsActive = false;
			billRecipient = CreateBillRecipient(organisation.PK, Env.CurrentBranch.PK, "AUD");
			AssertHasRowError(billRecipient, BillRecipient.ValidationMessages.InactiveOrg);

			organisation.OH_IsActive = true;
			billRecipient = CreateBillRecipient(organisation.PK, Env.CurrentBranch.PK, "AUD");
			AssertNoNotifications(billRecipient);

			InvalidOperationException expected = null;
			try
			{
				billRecipient = CreateBillRecipient(organisation.PK, branch.PK, "AUD");
			}
			catch (InvalidOperationException ex)
			{
				expected = ex;
			}
			AssertNotNull("wrong branch exception", expected);
			AssertEquals("Current branch must be set to invoicing branch", expected.Message);

			using (BillingInvoicingHelper.BranchContext(branch.PK))
			{
				billRecipient = CreateBillRecipient(organisation.PK, branch.PK, "AUD");
				AssertHasRowError(billRecipient, BillRecipient.ValidationMessages.NotDebtor);
			}
		}

		public void TestInvoiceComment()
		{
			EDIOrgHeader organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			organisation.CompanyData.OB_IsDebtor = true;
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			EDIDataRegistry.Instance.MonthlyUsageInvoiceComment.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, "Other branch comment");
			EDIDataRegistry.Instance.MonthlyUsageInvoiceComment.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "This company comment");

			var billRecipient = new BillRecipient(Factory, Env.CurrentBranch.PK, organisation.PK, "USD", ZDateTime.Now);
			AssertEquals("This company comment", billRecipient.MonthlyUsageInvoiceComment);

			using (BillingInvoicingHelper.BranchContext(branch.PK))
			{
				billRecipient = new BillRecipient(Factory, Env.CurrentBranch.PK, organisation.PK, "USD", ZDateTime.Now);
				AssertEquals("registry comment is branch specific", "Other branch comment", billRecipient.MonthlyUsageInvoiceComment);
			}

			organisation.LicCompany.SelfBilling.L4_InvoiceComment = "Org comment";
			billRecipient = new BillRecipient(Factory, Env.CurrentBranch.PK, organisation.PK, "USD", ZDateTime.Now);
			AssertEquals("org comment override registry", "Org comment", billRecipient.MonthlyUsageInvoiceComment);
		}

		public void TestARContact()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "BBB", "AAA", "AAA");
			var org = licence.Company.Header;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Chewbacca";
			contact.OC_Email = "Chewbacc@MileniumFalcon.com.au";
			var doc = contact.Documents.AddNew();
			doc.OD_DocumentGroup = ContactType.Receivables.Code;
			doc.OD_DefaultContact = true;

			licence.LA_AgreedLiveDate = new ZDateTime(2010, 1, 1);
			licence.LA_ContractExpiryDate = new ZDateTime(2010, 12, 31);

			licence.Billing.L0_RenewalMonths = 12;
			licence.Billing.L0_NextMaintenancePercent = 25;
			licence.LA_ContractExpiryDate = ZDateTime.Now;
			licence.Company.Header.CompanyData.OB_IsDebtor = true;
			var core = licence.GetCoreModule();
			core.LM_LicenceType = LicenceTypes.Codes.PUR;
			core.LM_UserCount = 5;
			if (licence.Company.InvoiceDeliveries.Count == 0)
			{
				ClientInvoiceDelivery delivery = licence.Company.InvoiceDeliveries.AddNew();
				delivery.L9_SystemCode = BillingConstants.BillingSystem.All;
				delivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;
				delivery.L9_RX_NKInvoiceCurrency = "AUD";
			}
			Factory.Save();

			var billRecipient = new BillRecipient(Factory, Env.CurrentBranch.PK, licence.Company.LC_OH, "AUD", ZDateTime.Now); //NewRecipient(licence);

			AssertEquals("contact name", contact.OC_ContactName, billRecipient.ContactName);
			AssertEquals("contact email", contact.OC_Email, billRecipient.ContactEmail);

			doc.OD_DocumentGroup = ContactType.All.Code;
			doc.OD_DefaultContact = true;
			Factory.Save();

			billRecipient = new BillRecipient(Factory, Env.CurrentBranch.PK, licence.Company.LC_OH, "AUD", ZDateTime.Now);

			AssertEquals("contact name", contact.OC_ContactName, billRecipient.ContactName);
			AssertEquals("contact email", contact.OC_Email, billRecipient.ContactEmail);

			doc.Delete();
			Factory.Save();

			billRecipient = new BillRecipient(Factory, Env.CurrentBranch.PK, licence.Company.LC_OH, "AUD", ZDateTime.Now);

			AssertEquals("contact name", ZString.Empty, billRecipient.ContactName);
			AssertEquals("contact email", ZString.Empty, billRecipient.ContactEmail);
		}

		BillRecipient CreateBillRecipient(ZGuid orgPk, ZGuid branchPk, ZString currencyCode)
		{
			var billRecipient = new BillRecipient(Factory, branchPk, orgPk, currencyCode, ZDateTime.Now);
			billRecipient.AddRowNotifications(billRecipient);
			return billRecipient;
		}

		BillRecipient CreateBillRecipient(ZGuid orgPk, ZGuid branchPk, ZString currencyCode, ZDateTime dateForExchangeRate)
		{
			var billRecipient = new BillRecipient(Factory, branchPk, orgPk, currencyCode, dateForExchangeRate);
			billRecipient.AddRowNotifications(billRecipient);
			return billRecipient;
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateBillRecipient(ZGuid.Empty, ZGuid.Empty, "");
		}

		#endregion
	}
}
