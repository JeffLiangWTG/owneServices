using System.Collections.Generic;
using Enterprise.Environment;
using Enterprise.Security;
using static Enterprise.Accounting.GUI.InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD;

namespace Enterprise.Accounting.GUI
{
	public class ARCreditNoteApprovalAlternativeCredentials : AlternativeCredentials
	{
		public ARCreditNoteApprovalAlternativeCredentials(string userName, string userPassword,
											List<BranchDepartmentPair> firstLevelSecurityRequiredBranchDepartment,
											List<BranchDepartmentPair> secondLevelSecurityRequiredBranchDepartment,
											List<BranchDepartmentPair> thirdLevelSecurityRequiredBranchDepartment,
											List<BranchDepartmentPair> fourthLevelSecurityRequiredBranchDepartment,
											List<BranchDepartmentPair> fifthLevelSecurityRequiredBranchDepartment,
											List<BranchDepartmentPair> sixthLevelSecurityRequiredBranchDepartment)
			: base(userName, userPassword)
		{
			dictLevelSecurityRequiredBranchDepartment = new Dictionary<string, List<BranchDepartmentPair>>();
			dictLevelSecurityRequiredBranchDepartment.Add(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code, firstLevelSecurityRequiredBranchDepartment ?? new List<BranchDepartmentPair>());
			dictLevelSecurityRequiredBranchDepartment.Add(Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.Code, secondLevelSecurityRequiredBranchDepartment ?? new List<BranchDepartmentPair>());
			dictLevelSecurityRequiredBranchDepartment.Add(Env.Security.CreditAdjustmentNotePostingApprovalThirdLevelApproval.Code, thirdLevelSecurityRequiredBranchDepartment ?? new List<BranchDepartmentPair>());
			dictLevelSecurityRequiredBranchDepartment.Add(Env.Security.CreditAdjustmentNotePostingApprovalFourthLevelApproval.Code, fourthLevelSecurityRequiredBranchDepartment ?? new List<BranchDepartmentPair>());
			dictLevelSecurityRequiredBranchDepartment.Add(Env.Security.CreditAdjustmentNotePostingApprovalFifthLevelApproval.Code, fifthLevelSecurityRequiredBranchDepartment ?? new List<BranchDepartmentPair>());
			dictLevelSecurityRequiredBranchDepartment.Add(Env.Security.CreditAdjustmentNotePostingApprovalSixthLevelApproval.Code, sixthLevelSecurityRequiredBranchDepartment ?? new List<BranchDepartmentPair>());
		}

		readonly Dictionary<string, List<BranchDepartmentPair>> dictLevelSecurityRequiredBranchDepartment;

		public SecurityCore[] GetUserSecuritiesCheckForBranchDepartment(SecurityCheckpoint checkpoint)
		{
			if (securityCores == null)
			{
				securityCores = new List<SecurityCore>();
				var loginController = new UserLoginController();
				loginAuthentication = ValidateAlternativeCredentials(loginController);

				if (loginAuthentication.LoginValidated)
				{
					var branchdepartmentList = dictLevelSecurityRequiredBranchDepartment.TryGetValue(checkpoint.Code, out var list) ? list : new List<BranchDepartmentPair>();

					foreach (BranchDepartmentPair branchdepartment in branchdepartmentList)
					{
						SecurityCore userSecurity = null;
						if (loginAuthentication.LoginValidated)
						{
							userSecurity = loginController.GetSecurityForUser(Login, branchdepartment.Branch.ToGuid(), branchdepartment.Department.ToGuid());
							securityCores.Add(userSecurity);
						}
					}
				}
			}

			return securityCores.ToArray();
		}
		List<SecurityCore> securityCores;
	}
}
