using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocAccQueryClaim))]
	sealed class DocAccQueryClaimTest : DocumentWrapperTestCase
	{
		public void TestDocumentTitle()
		{
			AssertEquals("AR CLAIM LOG", ARClaimWrapper.DocumentTitle);
			AssertEquals("AP CLAIM LOG", APClaimWrapper.DocumentTitle);
		}

		public void TestAccountType()
		{
			AssertEquals("DEBTOR", ARClaimWrapper.AccountType);
			AssertEquals("CREDITOR", APClaimWrapper.AccountType);
		}

		public void TestReference()
		{
			AssertEquals("Reference", "REF", ARClaimWrapper.Reference);
		}

		public void TestInvoiceNo()
		{
			AssertEquals(ZString.Empty, ARClaimWrapper.InvoiceNo);
			CreateInvoice<ARInvoice>(ARClaim);
			AssertEquals("ABCDEF", ARClaimWrapper.InvoiceNo);
		}

		public void TestAmount()
		{
			AssertEquals("100.34", ARClaimWrapper.Amount);
			CreateInvoice<ARInvoice>(ARClaim);
			AssertEquals("BD100.340 BHD", ARClaimWrapper.Amount);
		}

		public void TestTypeCode()
		{
			ARClaim.AY_QueryClaimType = "QYI";
			AssertEquals("QYI", ARClaimWrapper.TypeCode);
		}

		public void TestTypeDescription()
		{
			ARClaim.AY_QueryClaimType = "QYI";
			AssertEquals("Query about invoice basis", ARClaimWrapper.TypeDescription);
		}

		public void TestReasonCode()
		{
			ARClaim.AY_QueryClaimReasonCode = "DMG";
			AssertEquals("DMG", ARClaimWrapper.ReasonCode);
		}

		public void TestReasonDescription()
		{
			ARClaim.AY_QueryClaimReasonCode = "DMG";
			AssertEquals("Damage related", ARClaimWrapper.ReasonDescription);
		}

		public void TestStatusCode()
		{
			ARClaim.AY_QueryClaimStatus = "CRD";
			AssertEquals("CRD", ARClaimWrapper.StatusCode);
		}

		public void TestStatusDescription()
		{
			ARClaim.AY_QueryClaimStatus = "CRD";
			AssertEquals("Claim Accepted, Credit note Issued and Claim Closed", ARClaimWrapper.StatusDescription);
		}

		public void TestShortDescription()
		{
			AssertEquals("ShortDescription", "SHORTDESC", ARClaimWrapper.ShortDescription);
		}

		public void TestDetails()
		{
			AssertEquals("Details", "DETAILS", ARClaimWrapper.Details);
		}

		public void TestNextFollowup()
		{
			ARClaim.AY_QueryClaimNextFollowUp = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, ARClaimWrapper.NextFollowUp);
		}

		public void TestAccountOrg()
		{
			Assert(!ARClaim.AY_OH_Debtor.IsEmpty);
			AssertEquals(ARClaimWrapper.AccountOrg.Code, ARClaim.Debtor.OH_Code);
		}

		public void TestContact()
		{
			Assert(!ARClaim.AY_OC.IsEmpty);
			AssertEquals(ARClaimWrapper.Contact.ContactName, ARClaim.Contact.OC_ContactName);
		}

		public void TestCreator()
		{
			Assert(!ARClaim.AY_GS_NKCreator.IsEmpty);
			AssertEquals(ARClaimWrapper.Creator.FullName, Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ARClaim.AY_GS_NKCreator).GS_FullName);
		}

		public void TestAssignedTo()
		{
			Assert(!ARClaim.AY_GS_NKStaffAssignedTo.IsEmpty);
			AssertEquals(ARClaimWrapper.AssignedTo.FullName, Factory.Load<GlbStaff>(ARClaim.StaffAssignedTo.PK).GS_FullName);
		}

		public void TestBranch()
		{
			Assert(!ARClaim.AY_GB.IsEmpty);
			AssertEquals(ARClaimWrapper.Branch.Code, ARClaim.Branch.GB_Code);
		}

		#region Implementation

		T CreateInvoice<T>(AccQueryClaimBase claim) where T : Invoice
		{
			T result = Factory.NewWithValidTestData<T>();
			result.AH_RX_NKTransactionCurrency = "BHD";
			result.AH_TransactionNum = "ABCDEF";
			claim.AY_AH = result.PK;
			return result;
		}

		T CreateClaim<T>() where T : AccQueryClaimBase
		{
			T result = Factory.New<T>();
			result.AY_OH_Debtor = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			result.AY_QueryClaimAmount = 100.34m;
			result.AY_QueryClaimReference = "REF";
			result.AY_ShortDescriptionOfClaim = "SHORTDESC";
			result.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			result.Details = "DETAILS";
			return result;
		}

		ARAccQueryClaim fARClaim;
		ARAccQueryClaim ARClaim
		{
			get { return fARClaim ?? (fARClaim = CreateClaim<ARAccQueryClaim>()); }
		}

		DocAccQueryClaim fARClaimWrapper;
		DocAccQueryClaim ARClaimWrapper
		{
			get { return fARClaimWrapper ?? (fARClaimWrapper = DocAccQueryClaim.New(ARClaim, Factory)); }
		}
		APAccQueryClaim fAPClaim;
		APAccQueryClaim APClaim
		{
			get { return fAPClaim ?? (fAPClaim = CreateClaim<APAccQueryClaim>()); }
		}

		DocAccQueryClaim fAPClaimWrapper;
		DocAccQueryClaim APClaimWrapper
		{
			get { return fAPClaimWrapper ?? (fAPClaimWrapper = DocAccQueryClaim.New(APClaim, Factory)); }
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new[] { ARClaimWrapper };
		}

		#endregion
	}
}
