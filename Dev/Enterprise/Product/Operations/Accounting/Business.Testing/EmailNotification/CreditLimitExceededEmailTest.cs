using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.EmailNotification
{
	//TODO: Convert to ZEmailTest
	public class CreditLimitExceededEmailTest : MasterFiles.Business.Testing.AccountingEmailDefTest
	{
		protected override Type EmailDefType
		{
			get
			{
				return typeof(CreditLimitExceededEmail);
			}
		}

		[TestDate(2009, 02, 19)]
		public void TestExceededEmail()
		{
			InvoicingBase invoice = Factory.New<APInvoice>();
			invoice.AH_OSTotal = 334.1238732m;
			invoice.AH_InvoiceDate = new ZDateTime(2009, 01, 29);
			invoice.AH_TransactionNum = "00001010";
			OrgHeader header = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
			invoice.AH_OH = header.PK;
			var mail = new CreditLimitExceededEmail(new InvoicingBase[] { invoice }, 500.001m, 200.001m);
			string expectedSubject = "Credit Limit Exceeded by AUD $200.00 (A.A.L. SHIPPING AGENCIES P/L / AALSHI)";
			AssertEquals("Mail subject", expectedSubject, GetSubject(mail));
			string expectedCollectionCallsURL = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.OrgCollectionCalls, header.CompanyData.PK);
			string expectedBody = @"<p>A.A.L. SHIPPING AGENCIES P/L / AALSHI credit limit of AUD $500.00 has been exceeded by AUD $200.00 with the creation of AP INV 00001010 dated 29-Jan-09 for AUD $334.12 posted 19-Feb-09 by " + GlbStaff.CurrentUser.GS_FullName + @".</p>
<p></p>
<p>Credit approved.</p>
<p>See <a href=""{0}"">A.A.L. SHIPPING AGENCIES P/L / AALSHI</a> for more details.</p>";
			AssertMultilineASCIIEquals("Email Body", string.Format(expectedBody, expectedCollectionCallsURL), GetBody(mail));
		}

		[TestDate(2009, 02, 19)]
		public void TestExceededEmailWithGlobalParentOrg()
		{
			var org = TestObjectCreator.CreateOrgHeaderWithGlobalCreditLimitParent("GOrgParent", TestObjectCreator.AUD.RX_Code, 500m);
			var invoice = GetARInvoice(700m, "00001010", org);
			var mail = new CreditLimitExceededEmail(new InvoicingBase[] { invoice }, 500m, 200m, false, TestObjectCreator.AUD);
			string expectedSubject = "Global Credit Limit Exceeded by AUD $200.00 (Test Company Name / ZGOrgParent)";
			AssertEquals("Mail subject", expectedSubject, GetSubject(mail));
			string expectedCollectionCallsURL = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.OrgCollectionCalls, org.CompanyData.PK);
			string expectedBody = @"<p>Test Company Name / ZGOrgParent Global credit limit of AUD $500.00 has been exceeded by AUD $200.00 with the creation of AR INV 00001010 dated 29-Jan-09 for AUD $700.00 posted 19-Feb-09 by " + GlbStaff.CurrentUser.GS_FullName + @".</p>
<p>ZGOrgParent is a Global Group.</p>
<p>Credit approved.</p>
<p>See <a href=""{0}"">Test Company Name / ZGOrgParent</a> for more details.</p>";
			AssertMultilineASCIIEquals("Email Body", string.Format(expectedBody, expectedCollectionCallsURL), GetBody(mail));
		}

		[TestDate(2009, 02, 19)]
		public void TestExceededEmailWithGlobalChildOrg()
		{
			var org = TestObjectCreator.CreateOrgHeaderWithGlobalCreditLimitChild("GOrgChild", TestObjectCreator.AUD.RX_Code, 500m);
			var invoice = GetARInvoice(700m, "00001010", org);
			var mail = new CreditLimitExceededEmail(new InvoicingBase[] { invoice }, 500m, 200m, false, TestObjectCreator.AUD);
			string expectedSubject = "Global Credit Limit Exceeded by AUD $200.00 (Test Company Name / ZGOrgChildC)";
			AssertEquals("Mail subject", expectedSubject, GetSubject(mail));
			string expectedCollectionCallsURL = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.OrgCollectionCalls, org.CompanyData.PK);
			string expectedBody = @"<p>Test Company Name / ZGOrgChildC Global credit limit of AUD $500.00 has been exceeded by AUD $200.00 with the creation of AR INV 00001010 dated 29-Jan-09 for AUD $700.00 posted 19-Feb-09 by " + GlbStaff.CurrentUser.GS_FullName + @".</p>
<p>ZGOrgChildC is configured to use the credit limit of Global Credit Group ZGOrgChild.</p>
<p>Credit approved.</p>
<p>See <a href=""{0}"">Test Company Name / ZGOrgChildC</a> for more details.</p>";
			AssertMultilineASCIIEquals("Email Body", string.Format(expectedBody, expectedCollectionCallsURL), GetBody(mail));
		}

		[TestDate(2009, 02, 19)]
		public void TestExceededEmailCreditApproved()
		{
			InvoicingBase invoice = GetARInvoice();
			OrgHeader header = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
			header.CompanyData.OB_ARCreditApproved = true;
			var mail = new CreditLimitExceededEmail(new InvoicingBase[] { invoice }, 500.001m, 200.001m);
			string expectedSubject = "Credit Limit Exceeded by AUD $200.00 (A.A.L. SHIPPING AGENCIES P/L / AALSHI)";
			AssertEquals("Mail subject", expectedSubject, GetSubject(mail));
			string expectedCollectionCallsURL = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.OrgCollectionCalls, header.CompanyData.PK);
			string expectedBody = @"<p>A.A.L. SHIPPING AGENCIES P/L / AALSHI credit limit of AUD $500.00 has been exceeded by AUD $200.00 with the creation of AR INV 00001010 dated 29-Jan-09 for AUD $334.12 posted 19-Feb-09 by " + GlbStaff.CurrentUser.GS_FullName + @".</p>
<p></p>
<p>{1}</p>
<p>See <a href=""{0}"">A.A.L. SHIPPING AGENCIES P/L / AALSHI</a> for more details.</p>";
			AssertMultilineASCIIEquals("Email Body", string.Format(expectedBody, expectedCollectionCallsURL, "Credit approved."), GetBody(mail));
			header.CompanyData.OB_ARCreditApproved = false;
			mail = new CreditLimitExceededEmail(new InvoicingBase[] { invoice }, 500.001m, 200.001m);
			AssertMultilineASCIIEquals("Email Body", string.Format(expectedBody, expectedCollectionCallsURL, "Credit pending approval."), GetBody(mail));
		}

		[TestDate(2009, 02, 19)]
		public void TestExceededEmailWithSettlementGroup()
		{
			OrgHeader header = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
			OrgHeader orgHeaderParent = Factory.NewWithValidTestData<OrgHeader>();
			OrgRelatedParty orgRelatedParty = Factory.New<OrgRelatedParty>();
			orgRelatedParty.PR_OH_Parent = orgHeaderParent.PK;
			orgRelatedParty.PR_OH_RelatedParty = header.PK;
			orgRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
			orgRelatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			InvoicingBase invoice = GetARInvoice();
			var mail = new CreditLimitExceededEmail(new InvoicingBase[] { invoice }, 500.001m, 200.001m);
			string expectedSubject = "Credit Limit Exceeded by AUD $200.00 (A.A.L. SHIPPING AGENCIES P/L / AALSHI)";
			AssertEquals("Mail subject", expectedSubject, GetSubject(mail));
			string expectedCollectionCallsURL = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.OrgCollectionCalls, header.CompanyData.PK);
			string expectedBody = @"<p>A.A.L. SHIPPING AGENCIES P/L / AALSHI credit limit of AUD $500.00 has been exceeded by AUD $200.00 with the creation of AR INV 00001010 dated 29-Jan-09 for AUD $334.12 posted 19-Feb-09 by " + GlbStaff.CurrentUser.GS_FullName + @".</p>
<p>{1}</p>
<p>Credit approved.</p>
<p>See <a href=""{0}"">A.A.L. SHIPPING AGENCIES P/L / AALSHI</a> for more details.</p>";
			AssertMultilineASCIIEquals("Email Body", string.Format(expectedBody, expectedCollectionCallsURL, "AALSHI is a settlement group."), GetBody(mail));
			// Clear existing cached value for getting updates while checking validation of Settlement Group
			Factory.ClearCachedValue<bool>(string.Format(header.PK.ToString() + "" + GlbCompany.CurrentCompany.PK));
			Factory.ClearCachedValue<bool>(string.Format(header.PK.ToString() + "AR" + GlbCompany.CurrentCompany.PK));
			Factory.ClearCachedValue<bool>(string.Format(header.PK.ToString() + "AP" + GlbCompany.CurrentCompany.PK));
			orgRelatedParty.PR_OH_RelatedParty = orgHeaderParent.PK;
			OrgHeader settlementGroup = Factory.NewWithValidTestData<OrgHeader>();
			settlementGroup.OH_Code = "XYZXYZ";
			header.ARSettlementGroupPK = settlementGroup.PK;
			header.CompanyData.OB_ARUseSettlementGroupCreditLimit = true;
			Factory.Save();
			invoice = GetARInvoice();
			mail = new CreditLimitExceededEmail(new InvoicingBase[] { invoice }, 500.001m, 200.001m);
			AssertMultilineASCIIEquals("Email Body", string.Format(expectedBody, expectedCollectionCallsURL, "AALSHI is configured to use the credit limit of settlement group XYZXYZ."), GetBody(mail));
		}

		[TestDate(2009, 02, 19)]
		public void TestExceededEmailMultipleInvoices()
		{
			InvoicingBase invoice1 = GetARInvoice(100, "00010001");
			InvoicingBase invoice2 = GetARInvoice(200, "00010002");
			InvoicingBase invoice3 = GetARInvoice(300, "00010003");
			InvoicingBase[] invoices = new InvoicingBase[] { invoice1, invoice2, invoice3 };
			var email = new CreditLimitExceededEmail(invoices, 500, 100);
			string expectedSubject = "Credit Limit Exceeded by AUD $100.00 (A.A.L. SHIPPING AGENCIES P/L / AALSHI)";
			AssertEquals("Mail subject", expectedSubject, GetSubject(email));
			string expectedBody = @"<p>A.A.L. SHIPPING AGENCIES P/L / AALSHI credit limit of AUD $500.00 has been exceeded by AUD $100.00 with the creation of 3 Invoices posted on 19-Feb-09 by " + GlbStaff.CurrentUser.GS_FullName + @". The invoices are:
<ul>
<li>AR INV 00010001 created 29-Jan-09</li>
<li>AR INV 00010002 created 29-Jan-09</li>
<li>AR INV 00010003 created 29-Jan-09</li>
</ul></p>
<p></p>
<p>Credit approved.</p>
<p>See <a href=""edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=OrgCollectionCalls&BusinessEntityPK=cdcc905e-40da-4e47-8ccb-87f4b1af7f02&VersionNumber=" + new EnterpriseInformationRetriever().VersionNumber + @"&Hash=%2bH%2fZ2Wkb72pgcMAsGCxNS6wWIr81kZuFf"">A.A.L. SHIPPING AGENCIES P/L / AALSHI</a> for more details.</p>";
			AssertMultilineASCIIEquals("Email Body", expectedBody, GetBody(email));
		}

		[TestDate(2009, 02, 19)]
		public void TestExceededEmailMultipleInvoicesWithGlobalCreditLimit()
		{
			var org = TestObjectCreator.CreateOrgHeaderWithGlobalCreditLimitParent("GOrgParent", TestObjectCreator.AUD.RX_Code, 500m);
			var invoice1 = GetARInvoice(100, "00010001", org);
			var invoice2 = GetARInvoice(200, "00010002", org);
			var invoice3 = GetARInvoice(300, "00010003", org);
			var invoices = new InvoicingBase[] { invoice1, invoice2, invoice3 };
			var email = new CreditLimitExceededEmail(invoices, 500, 100, false, TestObjectCreator.AUD);
			string expectedSubject = "Global Credit Limit Exceeded by AUD $100.00 (Test Company Name / ZGOrgParent)";
			AssertEquals("Mail subject", expectedSubject, GetSubject(email));
			string expectedBody = string.Format(@"<p>Test Company Name / ZGOrgParent Global credit limit of AUD $500.00 has been exceeded by AUD $100.00 with the creation of 3 Invoices posted on 19-Feb-09 by " + GlbStaff.CurrentUser.GS_FullName + @". The invoices are:
<ul>
<li>AR INV 00010001 created 29-Jan-09</li>
<li>AR INV 00010002 created 29-Jan-09</li>
<li>AR INV 00010003 created 29-Jan-09</li>
</ul></p>
<p>ZGOrgParent is a Global Group.</p>
<p>Credit approved.</p>
<p>See <a href=""{0}"">Test Company Name / ZGOrgParent</a> for more details.</p>", ShowEditFormUrlHandler.Instance.Create(ControllerIDs.OrgCollectionCalls, org.CompanyData.PK));
			AssertMultilineASCIIEquals("Email Body", expectedBody, GetBody(email));
		}

		InvoicingBase GetARInvoice()
		{
			return GetARInvoice(334.123, "00001010");
		}

		InvoicingBase GetARInvoice(ZDecimal amount, string transNum, OrgHeader org = null)
		{
			var header = org ?? Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_OSTotal = amount;
			invoice.AH_InvoiceDate = new ZDateTime(2009, 01, 29);
			invoice.AH_TransactionNum = transNum;
			invoice.AH_OH = header.PK;
			return invoice;
		}

		public void TestContentType()
		{
			InvoicingBase invoice = Factory.New<APInvoice>();
			OrgHeader header = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
			invoice.AH_OH = header.PK;
			AssertEquals(new CreditLimitExceededEmail(new InvoicingBase[] { invoice }, 10m, 10m).ContentType, EmailContentTypes.HTML);
		}

		[TestDate(2009, 02, 19)]
		public void TestCreditLimitGrantedEmailNotificationNote()
		{
			AccountingConfigurationRegistry.Instance.ARExceedCreditLimitGrantedEmailNotificationNote.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "TestARExceedCreditLimitGrantedEmailNotificationNote");
			var header = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
			var orgHeaderParent = Factory.NewWithValidTestData<OrgHeader>();
			var orgRelatedParty = Factory.New<OrgRelatedParty>();
			orgRelatedParty.PR_OH_Parent = orgHeaderParent.PK;
			orgRelatedParty.PR_OH_RelatedParty = header.PK;
			orgRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ARSettlementGroup;
			orgRelatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			var invoice = GetARInvoice();
			var mail = new CreditLimitExceededEmail(new InvoicingBase[] { invoice }, 500.001m, 200.001m);
			string expectedCollectionCallsURL = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.OrgCollectionCalls, header.CompanyData.PK);
			string expectedBody = @"<p>A.A.L. SHIPPING AGENCIES P/L / AALSHI credit limit of AUD $500.00 has been exceeded by AUD $200.00 with the creation of AR INV 00001010 dated 29-Jan-09 for AUD $334.12 posted 19-Feb-09 by " + GlbStaff.CurrentUser.GS_FullName + @".</p>
<p>{1}</p>
<p>Credit approved.</p>
<p>TestARExceedCreditLimitGrantedEmailNotificationNote</p>
<p>See <a href=""{0}"">A.A.L. SHIPPING AGENCIES P/L / AALSHI</a> for more details.</p>";
			AssertMultilineASCIIEquals("Email Body", string.Format(expectedBody, expectedCollectionCallsURL, "AALSHI is a settlement group."), GetBody(mail));
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
		IDisposable instanceDetailsDisposable;

		protected override void SetUp()
		{
			base.SetUp();
			instanceDetailsDisposable = InstanceDetails.SetUpCurrentForTest();
		}

		protected override void TearDown()
		{
			instanceDetailsDisposable?.Dispose();
			base.TearDown();
		}
	}
}
