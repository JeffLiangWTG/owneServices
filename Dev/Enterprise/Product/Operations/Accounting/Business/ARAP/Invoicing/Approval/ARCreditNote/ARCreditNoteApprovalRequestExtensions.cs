using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Security;
using static Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public static class ARCreditNoteApprovalRequestExtensions
	{
		public static SecurityCheckpoint GetSecurityCheckpointForRequest(this ARCreditNoteApprovalRequest approval, UserLoginController loginController = null)
		{
			if (loginController == null)
			{
				loginController = new UserLoginController();
			}

			var userSecurity = loginController.GetSecurityForUser(Env.CurrentUser.LoginName,
				approval.JobBranch?.PK.ToGuid() ?? Env.CurrentBranchPK,
				approval.JobDepartment?.PK.ToGuid() ?? Env.CurrentDepartmentPK);

			switch (approval.NextAuthorisationLevelRequired)
			{
				case 1:
					return userSecurity.CreditAdjustmentNotePostingApprovalFirstLevelApproval;
				case 2:
					return userSecurity.CreditAdjustmentNotePostingApprovalSecondLevelApproval;
				case 3:
					return userSecurity.CreditAdjustmentNotePostingApprovalThirdLevelApproval;
				case 4:
					return userSecurity.CreditAdjustmentNotePostingApprovalFourthLevelApproval;
				case 5:
					return userSecurity.CreditAdjustmentNotePostingApprovalFifthLevelApproval;
				case 6:
					return userSecurity.CreditAdjustmentNotePostingApprovalSixthLevelApproval;
				default:
					return null;
			}
		}

		public static ZInt GetRequiredAuthorisationLevelFromRegistry(this ARCreditNoteApprovalRequest approval)
		{
			var registryItem = approval.XP_ApprovalType == Core.Constants.GenApprovalRequestApprovalType.ARCreditNoteForReversal
				? AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings
				: AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings;

			var settings = registryItem.GetFallBackValueAtAllLevels(approval.JobBranch?.GB_GC.ToGuid() ?? Env.CurrentCompanyPK,
				approval.JobBranch?.PK.ToGuid() ?? Env.CurrentBranchPK,
				approval.JobDepartment?.PK.ToGuid() ?? Env.CurrentDepartmentPK);

			var requirement = settings?.AuthorisationSettings.Cast<AmountBasedMultiLevelAuthorisationRequirement>().GetAuthorisationRequired(approval.PostingDetails.MaxAmountToApprove);
			switch (requirement?.AuthorisationRequirement)
			{
				case AuthorisationRequirementCodes.FirstApprovalRequiredOnly:
					return 1;
				case AuthorisationRequirementCodes.SecondApprovalRequiredOnly:
					return 2;
				case AuthorisationRequirementCodes.ThirdApprovalRequiredOnly:
					return 3;
				case AuthorisationRequirementCodes.FourthApprovalRequiredOnly:
					return 4;
				case AuthorisationRequirementCodes.FifthApprovalRequiredOnly:
					return 5;
				case AuthorisationRequirementCodes.SixthApprovalRequiredOnly:
					return 6;
				default:
					return 0;
			}
		}

		public static ZInt GetNumApproversFilled(this ARCreditNoteApprovalRequest approval)
		{
			return approval.XP_GS_NKApprovingUser1.IsEmpty ? 0 :
					approval.XP_GS_NKApprovingUser2.IsEmpty ? 1 :
					approval.XP_GS_NKApprovingUser3.IsEmpty ? 2 :
					approval.XP_GS_NKApprovingUser4.IsEmpty ? 3 :
					approval.XP_GS_NKApprovingUser5.IsEmpty ? 4 :
					approval.XP_GS_NKApprovingUser6.IsEmpty ? 5 : 6;
		}
	}
}
