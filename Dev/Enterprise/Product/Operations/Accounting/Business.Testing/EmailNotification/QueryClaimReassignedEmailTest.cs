using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business.Accounting;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.EmailNotification.Testing
{
	public class QueryClaimReassignedEmailTest : MasterFiles.Business.Testing.AccountingEmailDefTest
	{
		protected override Type EmailDefType
		{
			get
			{
				return typeof(QueryClaimReassignedEmail);
			}
		}

		public override void TestSend()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var user = creator.CreateContact(creator.AALSHI, "Park");
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "DP";
			staff.GS_EmailAddress = "Test@DP.COM";
			var queryClaim = Factory.NewWithValidTestData<ARAccQueryClaim>();
			queryClaim.AY_OH_Debtor = creator.AALSHI.PK;
			queryClaim.AY_AH = Factory.NewWithValidTestData<ARInvoice>().PK;
			queryClaim.AY_OC = user.PK;
			queryClaim.AY_QueryClaimAmount = 1000m;
			queryClaim.AY_ShortDescriptionOfClaim = "Claim Description";
			queryClaim.AY_QueryClaimType = QueryClaimTypeCodeList.Codes.QCType1;
			queryClaim.AY_QueryClaimReasonCode = QueryClaimReasonCodeList.Codes.QCReason1;
			queryClaim.AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus1Open;
			queryClaim.AY_GS_NKStaffAssignedTo = staff.GS_Code;
			queryClaim.AY_QueryClaimNextFollowUp = ZDateTime.Today;
			queryClaim.Details = "Claim Query Details";
			Factory.Save();
			AssertEquals("No email should be sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			QueryClaimReassignedEmail email = new QueryClaimReassignedEmail(queryClaim);
			email.Send();
			string expectedSubject = string.Format("Claim/Query {0} Reassignment", queryClaim.AY_QueryClaimReference);
			string expectedLink = ShowViewFormUrlHandler.Instance.Create(ControllerIDs.ARAccQueryClaim, queryClaim.PK);
			string expectedBody = string.Format(@"<p>
Account Details<br/>
Debtor: {0}<br/>
Invoice Number: {1}<br/>
Contact Person: {2}<br/>
Amount Claimed: {3}<br/>
Claim Description: {4}<br/><br/>

Claim Details<br/>
Claim Type: {5}<br/>
Claim Reason: {6}<br/>
Claim Creator: {7}<br/><br/>

Claim Status<br/>
Claim Status: {8}<br/>
Staff Member: {9}<br/>
Branch: {10}<br/>
Next Follow Up: {11}<br/><br/>

Claim Log<br/>
{12}<br/><br/>

Intercompany Claim Details<br/>
Branch: {13}<br/>
Organization: {14}</p><br/><br/>
<p>
See <a href=""{15}"">{16}</a> for more details
</p>", queryClaim.Debtor.OH_FullName, queryClaim.TransactionHeader.AH_TransactionNum, queryClaim.Contact.OC_ContactName, queryClaim.AY_QueryClaimAmount, queryClaim.AY_ShortDescriptionOfClaim, queryClaim.AY_QueryClaimType, queryClaim.AY_QueryClaimReasonCode, queryClaim.AY_GS_NKCreator, queryClaim.AY_QueryClaimStatus, queryClaim.AY_GS_NKStaffAssignedTo, queryClaim.Branch.GB_Code, queryClaim.AY_QueryClaimNextFollowUp, queryClaim.Details, Factory.Load<GlbBranch>(queryClaim.AY_GB_TransactionBranch).GB_BranchName, Factory.Load<OrgHeader>(queryClaim.AY_OH_TransactionBranchOrgProxy).OH_FullName, expectedLink, queryClaim.AY_QueryClaimReference);
			AssertEquals("One new email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Mail subject", expectedSubject, Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertMultilineASCIIEquals("Email Body", expectedBody, Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertEquals("Recipient", 1, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			AssertEquals("Recipient", "Test@DP.COM", Env.OutgoingMailManager.EmailsCreated[0].Recipients[0]);
		}
	}
}
