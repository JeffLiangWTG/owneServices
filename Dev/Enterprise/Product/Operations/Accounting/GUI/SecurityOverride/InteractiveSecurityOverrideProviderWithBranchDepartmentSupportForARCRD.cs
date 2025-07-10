using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public class InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD : ReopenClosedJobSecurityOverrideProvider
	{
		BusinessObject[] Approvals { get; }
		public InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(BusinessObject[] businessObjects)
		{
			Approvals = businessObjects;
		}

		// I can't see a reason why we need the following override ???
		//protected override bool UserInitiatorAndNotAllowedToApprove => AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.Value.AuthorizationMode == AuthorizationMode.Codes.TwoApprovers
		//															|| AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.Value.AuthorizationMode == AuthorizationMode.Codes.SequentialApprovers;

		protected override LoginForm CreateNewLoginForm()
		{
			foreach (var approval in Approvals)
			{
				var creditNoteApprovalRequest = approval as ARCreditNoteApprovalRequest;
				if (creditNoteApprovalRequest.XP_GB_JobBranch.IsValid && creditNoteApprovalRequest.XP_GE_JobDepartment.IsValid
					|| creditNoteApprovalRequest.XP_GB_JobBranch.IsEmpty && creditNoteApprovalRequest.XP_GE_JobDepartment.IsEmpty)
				{
					var transaction = creditNoteApprovalRequest.CreateARCreditNoteFromRequest();
					var branchDepartmentPair = new BranchDepartmentPair(transaction.AH_GB, transaction.AH_GE);

					List<BranchDepartmentPair> branchDepartmentPairList = null;
					switch (creditNoteApprovalRequest.NextAuthorisationLevelRequired)
					{
						case 1:
							branchDepartmentPairList = FirstLevelSecurityRequiredBranchDepartment;
							break;
						case 2:
							branchDepartmentPairList = SecondLevelSecurityRequiredBranchDepartment;
							break;
						case 3:
							branchDepartmentPairList = ThirdLevelSecurityRequiredBranchDepartment;
							break;
						case 4:
							branchDepartmentPairList = FourthLevelSecurityRequiredBranchDepartment;
							break;
						case 5:
							branchDepartmentPairList = FifthLevelSecurityRequiredBranchDepartment;
							break;
						case 6:
							branchDepartmentPairList = SixthLevelSecurityRequiredBranchDepartment;
							break;
					}

					if (!branchDepartmentPairList?.Contains(branchDepartmentPair) ?? false)
					{
						branchDepartmentPairList.Add(branchDepartmentPair);
					}
				}
			}

			return new LoginFormForARCreditNoteApprovalOverride(FirstLevelSecurityRequiredBranchDepartment, SecondLevelSecurityRequiredBranchDepartment,
															ThirdLevelSecurityRequiredBranchDepartment, FourthLevelSecurityRequiredBranchDepartment,
															FifthLevelSecurityRequiredBranchDepartment, SixthLevelSecurityRequiredBranchDepartment);
		}

		SecurityCore[] RequestLoginCredentialsForBranchDepartment(SecurityCheckpoint checkPoint)
		{
			SecurityCore[] result = System.Array.Empty<SecurityCore>();

			using (var form = CreateNewLoginForm())
			{
				form.Message = Res.GetString("6FF5E976-6FAC-47B1-BA42-82D377F78F10", @"One or more selected credit note approval requests require {0} security rights.
To override this security, a user with the required security right must login.
Please enter username and password details below.", checkPoint.DisplayText);

				while (true)
				{
					LastLoginFormResult = ZFormModaliser.ShowDialogWithoutDispose(form);

					if (LastLoginFormResult != DialogResult.OK)
					{
						break;
					}
					else
					{
						if (reopenJobCheckpoints.Contains(checkPoint))
						{
							result = new SecurityCore[] { form.Credentials?.UserSecurity };
						}
						else
						{
							result = ((form.Credentials) as ARCreditNoteApprovalAlternativeCredentials).GetUserSecuritiesCheckForBranchDepartment(checkPoint);
						}
						
						if (form.Credentials == null || result.Length == 0)
						{
							ZString errorMessage;
							switch (form.Credentials?.LoginAuthentication.State ?? LoginAuthenticationInfo.Status.Failure)
							{
								case LoginAuthenticationInfo.Status.UserNotFound:
									errorMessage = Res.GetString("86c22833-7f55-4de2-b5e7-b34ee59a2043", "Please enter a valid username.");
									break;
								case LoginAuthenticationInfo.Status.UserInactive:
									errorMessage = Res.GetString("4e9b5837-f84f-4710-905a-f264fdbb37ca", "This user is inactive, Please enter credentials for an active user.");
									break;
								case LoginAuthenticationInfo.Status.PasswordInvalid:
								case LoginAuthenticationInfo.Status.PasswordExpired:
									errorMessage = Res.GetString("59175779-464e-4975-91d6-9102af0ea169", "The password entered is incorrect or expired. Please re-enter the password or reset the password at login.");
									break;
								default:
									errorMessage = InvalidLoginErrorMsg;
									break;
							}

							Globals.Message.ShowError(errorMessage);
						}
					}

					if (Globals.IsTest || (result != null && result.Length > 0))
					{
						break;
					}
				}
			}

			return result;
		}

		public override SecurityCertificate PromptForTemporaryAccessCore(SecurityCheckpoint checkPoint)
		{
			SecurityCertificate result = SecurityCertificate.Denied;

			var securityCores = RequestLoginCredentialsForBranchDepartment(checkPoint);
			foreach (SecurityCore securityCore in securityCores)
			{
				UserSecurityOverride = securityCore;
				result = GetSecurityCertificate(checkPoint, securityCore);

				if (!result.IsAllowed)
				{
					break;
				}
			}
			return result;
		}

		List<BranchDepartmentPair> FirstLevelSecurityRequiredBranchDepartment
		{
			get => firstLevelSecurityRequiredBranchDepartment ?? (firstLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>());
			set => firstLevelSecurityRequiredBranchDepartment = value;
		}
		List<BranchDepartmentPair> firstLevelSecurityRequiredBranchDepartment;
		List<BranchDepartmentPair> SecondLevelSecurityRequiredBranchDepartment
		{
			get => secondLevelSecurityRequiredBranchDepartment ?? (secondLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>());
			set => secondLevelSecurityRequiredBranchDepartment = value;
		}
		List<BranchDepartmentPair> secondLevelSecurityRequiredBranchDepartment;

		List<BranchDepartmentPair> ThirdLevelSecurityRequiredBranchDepartment
		{
			get => thirdLevelSecurityRequiredBranchDepartment ?? (thirdLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>());
			set => thirdLevelSecurityRequiredBranchDepartment = value;
		}
		List<BranchDepartmentPair> thirdLevelSecurityRequiredBranchDepartment;

		List<BranchDepartmentPair> FourthLevelSecurityRequiredBranchDepartment
		{
			get => fourthLevelSecurityRequiredBranchDepartment ?? (fourthLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>());
			set => fourthLevelSecurityRequiredBranchDepartment = value;
		}
		List<BranchDepartmentPair> fourthLevelSecurityRequiredBranchDepartment;

		List<BranchDepartmentPair> FifthLevelSecurityRequiredBranchDepartment
		{
			get => fifthLevelSecurityRequiredBranchDepartment ?? (fifthLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>());
			set => fifthLevelSecurityRequiredBranchDepartment = value;
		}
		List<BranchDepartmentPair> fifthLevelSecurityRequiredBranchDepartment;

		List<BranchDepartmentPair> SixthLevelSecurityRequiredBranchDepartment
		{
			get => sixthLevelSecurityRequiredBranchDepartment ?? (sixthLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>());
			set => sixthLevelSecurityRequiredBranchDepartment = value;
		}
		List<BranchDepartmentPair> sixthLevelSecurityRequiredBranchDepartment;

		public struct BranchDepartmentPair
		{
			public ZGuid Branch { get; }
			public ZGuid Department { get; }

			public override int GetHashCode()
			{
				return Branch.GetHashCode() ^ Department.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var branchDepartmentPair = (BranchDepartmentPair)obj;
				return Branch == branchDepartmentPair.Branch && Department == branchDepartmentPair.Department;
			}
			public static bool operator ==(BranchDepartmentPair obj1, BranchDepartmentPair obj2)
			{
				return obj1.Branch == obj2.Branch && obj1.Department == obj2.Department;
			}

			public static bool operator !=(BranchDepartmentPair obj1, BranchDepartmentPair obj2)
			{
				return !(obj1.Branch == obj2.Branch && obj1.Department == obj2.Department);
			}

			public BranchDepartmentPair(ZGuid branch, ZGuid department)
			{
				Branch = branch;
				Department = department;
			}
		}

		readonly SecurityCheckpoint[] reopenJobCheckpoints = new SecurityCheckpoint[]
		{
			Env.Security.ReopenJob,
			Env.Security.ReopenJobPastAllowedReOpenPeriod,
		};
	}
}
