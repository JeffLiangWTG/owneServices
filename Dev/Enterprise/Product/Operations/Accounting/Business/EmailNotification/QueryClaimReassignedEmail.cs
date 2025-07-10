using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class QueryClaimReassignedEmail
	{
		public QueryClaimReassignedEmail(AccQueryClaim queryClaim)
		{
			Argument.NotNull(queryClaim, "QueryClaim");

			Subject = GetSubjectCore(queryClaim);
			Body = GetBodyCore(queryClaim);
			Recipient = queryClaim.StaffAssignedTo != null ? queryClaim.StaffAssignedTo.GS_EmailAddress : ZString.Empty;
		}

		readonly string Subject;
		readonly string Body;
		readonly string Recipient;

		string GetSubjectCore(AccQueryClaim queryClaim)
		{
			return Res.GetString("414de432-a7c3-46b4-a0a0-80b9bb0e5668", "Claim/Query {0} Reassignment", queryClaim.AY_QueryClaimReference);
		}

		string GetBodyCore(AccQueryClaim queryClaim)
		{
			var interCompanyBranch = queryClaim.Factory.Load<GlbBranch>(queryClaim.AY_GB_TransactionBranch);
			var interCompanyOrg = queryClaim.Factory.Load<OrgHeader>(queryClaim.AY_OH_TransactionBranchOrgProxy);

			var link = ObjectFactory.Get<IShowViewFormUrlCreator>().Create(queryClaim is ARAccQueryClaim ? ControllerIDs.ARAccQueryClaim : ControllerIDs.APAccQueryClaim, queryClaim.PK.ToGuid());
			string result = string.Format((NoResString)@"<p>
{0}<br/>
{1}<br/>
{2}<br/>
{3}<br/>
{4}<br/>
{5}<br/><br/>

{6}<br/>
{7}<br/>
{8}<br/>
{9}<br/><br/>

{10}<br/>
{11}<br/>
{12}<br/>
{13}<br/>
{14}<br/><br/>

{15}<br/>
{16}<br/><br/>

{17}<br/>
{18}<br/>
{19}</p><br/><br/>
<p>
{20}
</p>",
				Res.GetString("c83824b5-2fec-4b46-a55b-47efdffebf25", "Account Details"),
				Res.GetString("348dd40d-d14f-4450-87ac-317b1be6d5c0", "Debtor: {0}", queryClaim.Debtor != null ? queryClaim.Debtor.OH_FullName : ZString.Empty),
				Res.GetString("dccf2082-78da-4e64-8e64-4af2bb8ec870", "Invoice Number: {0}", queryClaim.TransactionHeader != null ? queryClaim.TransactionHeader.AH_TransactionNum : ZString.Empty),
				Res.GetString("42feff78-4af5-46d8-b90f-a3b693a282a4", "Contact Person: {0}", queryClaim.Contact != null ? queryClaim.Contact.OC_ContactName : ZString.Empty),
				Res.GetString("e5000607-3f44-4e77-b1d1-1c9da0c3db81", "Amount Claimed: {0}", queryClaim.AY_QueryClaimAmount),
				Res.GetString("e7d5ea21-1c03-4822-b05e-936b1ef5fb98", "Claim Description: {0}", queryClaim.AY_ShortDescriptionOfClaim),
				Res.GetString("6d80d923-4534-476f-b51e-6390c8048d3d", "Claim Details"),
				Res.GetString("1657f3b5-188e-42d5-bfd0-eae01bd26147", "Claim Type: {0}", queryClaim.AY_QueryClaimType),
				Res.GetString("036c1793-ca8b-4111-b0ed-6f2d7d24b9ff", "Claim Reason: {0}", queryClaim.AY_QueryClaimReasonCode),
				Res.GetString("f80b93af-71fb-4254-bf92-6f94c94c24ce", "Claim Creator: {0}", queryClaim.AY_GS_NKCreator),
				Res.GetString("2c28352f-49ef-4378-8fa4-8e7e6d7363c2", "Claim Status"),
				Res.GetString("e93443ee-e87c-47c9-889e-fcfbb181ef0d", "Claim Status: {0}", queryClaim.AY_QueryClaimStatus),
				Res.GetString("e586c84b-b3a7-4cb1-89e7-7d48537b3675", "Staff Member: {0}", queryClaim.AY_GS_NKStaffAssignedTo),
				Res.GetString("6692310f-8606-452e-8180-e820d7220a1c", "Branch: {0}", queryClaim.Branch != null ? queryClaim.Branch.GB_Code : ZString.Empty),
				Res.GetString("dba5f12a-f27a-4f29-9e8c-01ca017ea378", "Next Follow Up: {0}", queryClaim.AY_QueryClaimNextFollowUp),
				Res.GetString("c0e9a16c-6461-4c80-ac00-b2fc262aa817", "Claim Log"),
				queryClaim.Details,
				Res.GetString("e17be62e-11b5-4e84-a5b6-ceffa44ec9bf", "Intercompany Claim Details"),
				Res.GetString("3d814c08-4ccc-4a60-9d31-e236ca20f4ae", "Branch: {0}", interCompanyBranch != null ? interCompanyBranch.GB_BranchName : ZString.Empty),
				Res.GetString("63110f49-312c-4409-b8f8-9844bc2346f5", "Organization: {0}", interCompanyOrg != null ? interCompanyOrg.OH_FullName : ZString.Empty),
				Res.GetString("e8ef3d45-3fa8-40ea-b6cf-1be9d1c36a69", "See {0} for more details", string.Format(@"<a href=""{0}"">{1}</a>", link, queryClaim.AY_QueryClaimReference)));

			return result;
		}

		public void Send()
		{
			EmailDef email = new EmailDef(GlbStaff.CurrentUser.PK.ToGuid());
			email.ContentType = EmailContentTypes.HTML;
			email.Subject = Subject;
			email.Body = Body;
			email.AddRecipientForUserCommunication(Recipient);
			Env.OutgoingMailManager.CreateAndSave(email);
		}
	}
}

