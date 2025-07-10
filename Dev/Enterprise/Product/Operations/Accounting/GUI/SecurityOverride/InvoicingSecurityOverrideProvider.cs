using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Accounting.GUI.InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD;

namespace Enterprise.Accounting.GUI
{
	public partial class InvoicingSecurityOverrideProvider : SecurityOverrideProviderWithJobReopenSupport, ISupportMixedLevelAuthorization, ISecurityOverrideProviderSupportTwoApproverLogin
	{
		public InvoicingSecurityOverrideProvider(bool showApprovalRequestButton = false, bool alwaysCreateApprovalRequest = false, bool keepLoginFormResultAfterFirstUserAnswer = false, bool supportMultipleApprover = false, ARCreditNoteApprovalRequest[] aRCreditNoteApprovalRequests = null)
			: base(showApprovalRequestButton, alwaysCreateApprovalRequest, keepLoginFormResultAfterFirstUserAnswer)
		{
			if (aRCreditNoteApprovalRequests != null)
			{
				foreach (ARCreditNoteApprovalRequest creditNoteApprovalRequest in aRCreditNoteApprovalRequests)
				{
					var transaction = creditNoteApprovalRequest.CreateARCreditNoteFromRequest();
					var branchDepartmentPair = new BranchDepartmentPair(transaction.AH_GB, transaction.AH_GE);

					if (transaction.IsLevel1AuthorisationRequired)
					{
						AddBranchDepartmentPairToLevelRequiredList(FirstLevelSecurityRequiredBranchDepartment, branchDepartmentPair);
					}
					else if (transaction.IsLevel2AuthorisationRequired)
					{
						AddBranchDepartmentPairToLevelRequiredList(SecondLevelSecurityRequiredBranchDepartment, branchDepartmentPair);
					}
					else if (transaction.IsLevel3AuthorisationRequired)
					{
						AddBranchDepartmentPairToLevelRequiredList(ThirdLevelSecurityRequiredBranchDepartment, branchDepartmentPair);
					}
					else if (transaction.IsLevel4AuthorisationRequired)
					{
						AddBranchDepartmentPairToLevelRequiredList(FourthLevelSecurityRequiredBranchDepartment, branchDepartmentPair);
					}
					else if (transaction.IsLevel5AuthorisationRequired)
					{
						AddBranchDepartmentPairToLevelRequiredList(FifthLevelSecurityRequiredBranchDepartment, branchDepartmentPair);
					}
					else if (transaction.IsLevel6AuthorisationRequired)
					{
						AddBranchDepartmentPairToLevelRequiredList(SixthLevelSecurityRequiredBranchDepartment, branchDepartmentPair);
					}

					if (supportMultipleApprover)
					{
						RequiresTwoApprovers = RequiresTwoApprovers || transaction.EnforceTwoApproversWhenPostingARCredit;
						RequiresSequentialApprovals = RequiresSequentialApprovals || transaction.EnforceSequentialApproversWhenPostingARCredit;
					}
				}
			}
		}

		void AddBranchDepartmentPairToLevelRequiredList(List<BranchDepartmentPair> branchDepartmentList, BranchDepartmentPair branchDepartmentPair)
		{
			if (!branchDepartmentList.Contains(branchDepartmentPair))
			{
				branchDepartmentList.Add(branchDepartmentPair);
			}
		}

		public InvoicingSecurityOverrideProvider(InvoicingBase invoicingBase)
			: this()
		{
			InvoicingBase = invoicingBase;
			if (InvoicingBase is ARCreditNote || InvoicingBase is ARAdjustmentNote)
			{
				RequiresTwoApprovers = InvoicingBase.EnforceTwoApproversWhenPostingARCredit;
				RequiresSequentialApprovals = InvoicingBase.EnforceSequentialApproversWhenPostingARCredit;
			}
		}

		public InvoicingSecurityOverrideProvider(MultipleReversingProviderForHeader reversingProvider)
			: this()
		{
			ReversingProvider = reversingProvider;
		}
		#region SecurityOverrideProvider Overrides

		public List<BranchDepartmentPair> FirstLevelSecurityRequiredBranchDepartment
		{
			get
			{
				if (firstLevelSecurityRequiredBranchDepartment == null)
				{
					firstLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>();
				}

				return firstLevelSecurityRequiredBranchDepartment;
			}
			set
			{
				firstLevelSecurityRequiredBranchDepartment = value;
			}
		}
		List<BranchDepartmentPair> firstLevelSecurityRequiredBranchDepartment;
		public List<BranchDepartmentPair> SecondLevelSecurityRequiredBranchDepartment
		{
			get
			{
				if (secondLevelSecurityRequiredBranchDepartment == null)
				{
					secondLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>();
				}

				return secondLevelSecurityRequiredBranchDepartment;
			}
			set
			{
				secondLevelSecurityRequiredBranchDepartment = value;
			}
		}
		List<BranchDepartmentPair> secondLevelSecurityRequiredBranchDepartment;
		public List<BranchDepartmentPair> ThirdLevelSecurityRequiredBranchDepartment
		{
			get
			{
				if (thirdLevelSecurityRequiredBranchDepartment == null)
				{
					thirdLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>();
				}

				return thirdLevelSecurityRequiredBranchDepartment;
			}
			set
			{
				thirdLevelSecurityRequiredBranchDepartment = value;
			}
		}
		List<BranchDepartmentPair> thirdLevelSecurityRequiredBranchDepartment;
		public List<BranchDepartmentPair> FourthLevelSecurityRequiredBranchDepartment
		{
			get
			{
				if (fourthLevelSecurityRequiredBranchDepartment == null)
				{
					fourthLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>();
				}

				return fourthLevelSecurityRequiredBranchDepartment;
			}
			set
			{
				fourthLevelSecurityRequiredBranchDepartment = value;
			}
		}
		List<BranchDepartmentPair> fourthLevelSecurityRequiredBranchDepartment;
		public List<BranchDepartmentPair> FifthLevelSecurityRequiredBranchDepartment
		{
			get
			{
				if (fifthLevelSecurityRequiredBranchDepartment == null)
				{
					fifthLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>();
				}

				return fifthLevelSecurityRequiredBranchDepartment;
			}
			set
			{
				fifthLevelSecurityRequiredBranchDepartment = value;
			}
		}
		List<BranchDepartmentPair> fifthLevelSecurityRequiredBranchDepartment;
		public List<BranchDepartmentPair> SixthLevelSecurityRequiredBranchDepartment
		{
			get
			{
				if (sixthLevelSecurityRequiredBranchDepartment == null)
				{
					sixthLevelSecurityRequiredBranchDepartment = new List<BranchDepartmentPair>();
				}

				return sixthLevelSecurityRequiredBranchDepartment;
			}
			set
			{
				sixthLevelSecurityRequiredBranchDepartment = value;
			}
		}
		List<BranchDepartmentPair> sixthLevelSecurityRequiredBranchDepartment;
		protected override LoginForm CreateNewLoginForm()
		{
			if (showApprovalRequestButton)
			{
				return new LoginFormWithRequest(FirstLevelSecurityRequiredBranchDepartment, SecondLevelSecurityRequiredBranchDepartment,
												ThirdLevelSecurityRequiredBranchDepartment, FourthLevelSecurityRequiredBranchDepartment,
												FifthLevelSecurityRequiredBranchDepartment, SixthLevelSecurityRequiredBranchDepartment);
			}
			else
			{
				if (InvoicingBase is ARCreditNote || InvoicingBase is ARAdjustmentNote)
				{
					PopulateAllLevelsSecurityRequiredBranchDepartment();
					return new LoginFormForARCreditNoteApprovalOverride(FirstLevelSecurityRequiredBranchDepartment, SecondLevelSecurityRequiredBranchDepartment,
																	ThirdLevelSecurityRequiredBranchDepartment, FourthLevelSecurityRequiredBranchDepartment,
																	FifthLevelSecurityRequiredBranchDepartment, SixthLevelSecurityRequiredBranchDepartment);
				}
				else
				{
					return base.CreateNewLoginForm();
				}
			}
		}

		public override SecurityCertificate PromptForTemporaryAccessCore(SecurityCheckpoint checkPoint)
		{
			SecurityCertificate result = SecurityCertificate.Denied;
			SecurityCore[] securityCores = null;

			if (RequiresSingleApprover || reopenJobCheckpoints.Contains(checkPoint))
			{
				securityCores = RequestSecurityCoresForOneLoginCredential(checkPoint);
			}
			else   // if require more than one approver, e.g. posting AR CRD
			{
				securityCores = RequestSecurityCoresForMultipleLoginCredentials(checkPoint);
			}
			if (securityCores != null)
			{
				foreach (SecurityCore securityCore in securityCores)
				{
					result = GetSecurityCertificate(checkPoint, securityCore);
					if (!result.IsAllowed)
					{
						break;
					}
				}
			}
			return result;
		}

		protected virtual SecurityCore[] RequestSecurityCoresForOneLoginCredential(SecurityCheckpoint checkPoint)
		{
			if (!keepLoginFormResultAfterFirstUserAnswer || LastLoginFormResult == DialogResult.None || EnforceToCheckNextLevelOfAuthorization)
			{
				SecurityCore[] result = System.Array.Empty<SecurityCore>();

				using (var form = CreateNewLoginForm())
				{
					form.Message = GetSecurityOverrideMessage(checkPoint);

					while (true)
					{
						LastLoginFormResult = ZFormModaliser.ShowDialogWithoutDispose(form);

						if (LastLoginFormResult != DialogResult.OK)
						{
							break;
						}
						else
						{
							if (form.Credentials is ARCreditNoteApprovalAlternativeCredentials && !reopenJobCheckpoints.Contains(checkPoint))
							{
								if (form.Credentials == null || ((form.Credentials) as ARCreditNoteApprovalAlternativeCredentials).GetUserSecuritiesCheckForBranchDepartment(checkPoint).Length == 0)
								{
									if (form.Credentials != null && form.Credentials.LoginAuthentication.State == ZArchitecture.Core.LoginAuthenticationInfo.Status.UserInactive)
									{
										Globals.Message.Show(form.Credentials.LoginAuthentication.FailureMessage);
									}
									else
									{
										Globals.Message.Show(InvalidLoginErrorMsg);
									}
								}

								result = ((form.Credentials) as ARCreditNoteApprovalAlternativeCredentials)?.GetUserSecuritiesCheckForBranchDepartment(checkPoint);
								if (result.Length > 0)
								{
									UserSecurityOverride = result[0];
								}
							}
							else
							{
								if (form.Credentials == null || form.Credentials.UserSecurity == null)
								{
									if (form.Credentials != null && form.Credentials.LoginAuthentication.State == ZArchitecture.Core.LoginAuthenticationInfo.Status.UserInactive)
									{
										Globals.Message.Show(form.Credentials.LoginAuthentication.FailureMessage);
									}
									else
									{
										Globals.Message.Show(InvalidLoginErrorMsg);
									}
								}

								UserSecurityOverride = form.Credentials?.UserSecurity;
								result = new SecurityCore[] { form.Credentials?.UserSecurity };
							}
						}
						if (Globals.IsTest || (result.Length > 0 && result[0] != null))
						{
							break;
						}
					}
				}

				return result;
			}

			return new SecurityCore[] { UserSecurityOverride };
		}

		SecurityCore[] RequestSecurityCoresForMultipleLoginCredentials(SecurityCheckpoint checkPoint)
		{
			SecurityCore[] result = null;
			if (RequiresSequentialApprovals)
			{
				if (IsApprovalRequestButtonVisible)      // handle post AR CRD from billing tab
				{
					using (var form = new LoginFormWithRequestAndNoCredentials())
					{
						form.Message = Res.GetString("c41a3756-8729-4100-800c-884385d8b80c", @"Credit notes of this value require approval by multiple authorized users.
Please queue a request for approval by pressing the 'Approval Request' button below.
If you are an authorized approver, your request will be queued for additional approvals.");
						LastLoginFormResult = ZFormModaliser.ShowDialogWithoutDispose(form);
					}
				}
				else  // when mode is sequential, make it as same as two approvers.
				{
					result = ExtractSecurityCoresForTwoApprovers(checkPoint);
				}
			}
			else if (RequiresTwoApprovers)
			{
				if (IsApprovalRequestButtonVisible)      // handle post AR CRD from billing tab
				{
					using (var form = new LoginFormWithRequestAndNoCredentials())
					{
						form.Message = Res.GetString("9cf8d610-298c-48ce-9512-9bb1cf516df3", @"Credit notes of this value require approval by two authorized users.
Please queue a request for approval by pressing the 'Approval Request' button below.
If you are an authorized approver, your request will be queued for 2nd approval.");
						LastLoginFormResult = ZFormModaliser.ShowDialogWithoutDispose(form);
					}
				}
				else  // handle login form with two set of credential entry fields - for posting new AR CRD from the AR module
				{
					result = ExtractSecurityCoresForTwoApprovers(checkPoint);
				}
			}
			return result;
		}

		SecurityCore[] ExtractSecurityCoresForTwoApprovers(SecurityCheckpoint checkPoint)
		{
			SecurityCore[] result;
			if (InvoicingBase is ARCreditNote || InvoicingBase is ARAdjustmentNote)
			{
				PopulateAllLevelsSecurityRequiredBranchDepartment();
			}
			result = GetSecurityCoresFromLoginFormWithTwoCredentials(checkPoint);
			return result;
		}

		void AddBranchDepartmentPairToLevelSecurityRequiredList(InvoicingBase invoice, List<BranchDepartmentPair> levelSecurityRequiredList)
		{
			var branchDepartmentPair = new BranchDepartmentPair(invoice.AH_GB, invoice.AH_GE);
			if (!levelSecurityRequiredList.Contains(branchDepartmentPair))
			{
				levelSecurityRequiredList.Add(branchDepartmentPair);
			}
		}

		void PopulateAllLevelsSecurityRequiredBranchDepartment()
		{
			var branchDepartmentPair = new BranchDepartmentPair(InvoicingBase.AH_GB, InvoicingBase.AH_GE);

			FirstLevelSecurityRequiredBranchDepartment.Clear();
			SecondLevelSecurityRequiredBranchDepartment.Clear();
			ThirdLevelSecurityRequiredBranchDepartment.Clear();
			FourthLevelSecurityRequiredBranchDepartment.Clear();
			FifthLevelSecurityRequiredBranchDepartment.Clear();
			SixthLevelSecurityRequiredBranchDepartment.Clear();

			if (InvoicingBase.IsLevel1AuthorisationRequired)
			{
				AddBranchDepartmentPairToLevelSecurityRequiredList(InvoicingBase, FirstLevelSecurityRequiredBranchDepartment);
			}
			else if (InvoicingBase.IsLevel2AuthorisationRequired)
			{
				AddBranchDepartmentPairToLevelSecurityRequiredList(InvoicingBase, SecondLevelSecurityRequiredBranchDepartment);
			}
			else if (InvoicingBase.IsLevel3AuthorisationRequired)
			{
				AddBranchDepartmentPairToLevelSecurityRequiredList(InvoicingBase, ThirdLevelSecurityRequiredBranchDepartment);
			}
			else if (InvoicingBase.IsLevel4AuthorisationRequired)
			{
				AddBranchDepartmentPairToLevelSecurityRequiredList(InvoicingBase, FourthLevelSecurityRequiredBranchDepartment);
			}
			else if (InvoicingBase.IsLevel5AuthorisationRequired)
			{
				AddBranchDepartmentPairToLevelSecurityRequiredList(InvoicingBase, FifthLevelSecurityRequiredBranchDepartment);
			}
			else if (InvoicingBase.IsLevel6AuthorisationRequired)
			{
				AddBranchDepartmentPairToLevelSecurityRequiredList(InvoicingBase, SixthLevelSecurityRequiredBranchDepartment);
			}

			foreach (var transaction in InvoicingBase.TransactionsWithUniqueBranchDepartmentForAuthorisationCalculation)
			{
				if (transaction.IsLevel1AuthorisationRequired)
				{
					AddBranchDepartmentPairToLevelSecurityRequiredList(transaction, FirstLevelSecurityRequiredBranchDepartment);
				}
				else if (transaction.IsLevel2AuthorisationRequired)
				{
					AddBranchDepartmentPairToLevelSecurityRequiredList(transaction, SecondLevelSecurityRequiredBranchDepartment);
				}
				else if (transaction.IsLevel3AuthorisationRequired)
				{
					AddBranchDepartmentPairToLevelSecurityRequiredList(transaction, ThirdLevelSecurityRequiredBranchDepartment);
				}
				else if (transaction.IsLevel4AuthorisationRequired)
				{
					AddBranchDepartmentPairToLevelSecurityRequiredList(transaction, FourthLevelSecurityRequiredBranchDepartment);
				}
				else if (transaction.IsLevel5AuthorisationRequired)
				{
					AddBranchDepartmentPairToLevelSecurityRequiredList(transaction, FifthLevelSecurityRequiredBranchDepartment);
				}
				else if (transaction.IsLevel6AuthorisationRequired)
				{
					AddBranchDepartmentPairToLevelSecurityRequiredList(transaction, SixthLevelSecurityRequiredBranchDepartment);
				}
			}
		}

		protected virtual LoginFormWithTwoCredentialSupportBranchDepartmentLevel CreateNewLoginFormWithTwoCredential()
		{
			return new LoginFormWithTwoCredentialSupportBranchDepartmentLevel(FirstLevelSecurityRequiredBranchDepartment, SecondLevelSecurityRequiredBranchDepartment,
																			ThirdLevelSecurityRequiredBranchDepartment, FourthLevelSecurityRequiredBranchDepartment,
																			FifthLevelSecurityRequiredBranchDepartment, SixthLevelSecurityRequiredBranchDepartment);
		}

		SecurityCore[] GetSecurityCoresFromLoginFormWithTwoCredentials(SecurityCheckpoint checkPoint)
		{
			SecurityCore[] result = null;
			using (var form = CreateNewLoginFormWithTwoCredential())
			{
				form.Message = Res.GetString("bde8836c-d303-4ce3-8a53-a434e7f0c7c9", @"You do not have security rights [{0}] to post a credit note/adjustment note for this amount.
Two users with security rights to post a credit note/adjustment note can authorize this transaction.
To post this transaction, please have two authorized users enter their username and password below.", checkPoint.HumanReadableName);

				var isCurrentLoginUserAuthorized = CheckCurrentLoginUserAuthorised(checkPoint);
				if (isCurrentLoginUserAuthorized)
				{
					form.PopulateFirstCredentialWithCurrentStaff();
				}

				while (true)
				{
					LastLoginFormResult = ZFormModaliser.ShowDialogWithoutDispose(form);

					if (LastLoginFormResult != DialogResult.OK)
					{
						break;
					}

					SecurityCore[] securityCoresForUser1 = null;
					SecurityCore[] securityCoresForUser2 = null;

					if (!isCurrentLoginUserAuthorized)
					{
						if (CheckCredentialOneIsValidUser(form))
						{
							securityCoresForUser1 = ((form.Credentials) as ARCreditNoteApprovalAlternativeCredentials)?.GetUserSecuritiesCheckForBranchDepartment(checkPoint);
							if (CheckCredentialTwoIsValidUser(form))
							{
								securityCoresForUser2 = ((form.Credentials2) as ARCreditNoteApprovalAlternativeCredentials)?.GetUserSecuritiesCheckForBranchDepartment(checkPoint);
							}
						}
					}
					else
					{
						var securityCoreForLoginUser = new SecurityCore(GlbStaff.CurrentUser.StaffSecurityPermissionsCollection, GlbStaff.CurrentUser, InvoicingBase.AH_GB.ToGuid(), InvoicingBase.AH_GE.ToGuid(), Env.CurrentCompany.PK, false);
						securityCoresForUser1 = new SecurityCore[1] { securityCoreForLoginUser };
						if (CheckCredentialTwoIsValidUser(form))
						{
							securityCoresForUser2 = ((form.Credentials2) as ARCreditNoteApprovalAlternativeCredentials)?.GetUserSecuritiesCheckForBranchDepartment(checkPoint);
						}
					}
					if (securityCoresForUser1 != null && securityCoresForUser1.Length > 0 && securityCoresForUser2 != null && securityCoresForUser2.Length > 0)
					{
						UserPKsForTwoCredentialLogin = new List<ZGuid> { securityCoresForUser1[0].UserPK, securityCoresForUser2[0].UserPK };
						result = securityCoresForUser1.Concat(securityCoresForUser2).ToArray();
					}
					if (Globals.IsTest || (result != null && result.Length >= 2))
					{
						break;
					}
				}
			}
			return result;
		}

		bool CheckCurrentLoginUserAuthorised(SecurityCheckpoint checkPoint)
		{
			var branchdepartmentList = new List<BranchDepartmentPair>();
			var isFirstLevelCheckPoint = false;
			var isSecondLevelCheckPoint = false;
			var isThirdLevelCheckPoint = false;
			var isFourthLevelCheckPoint = false;
			var isFifthLevelCheckPoint = false;
			var isSixthLevelCheckPoint = false;
			var result = true;
			if (checkPoint.Code == Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code)
			{
				branchdepartmentList = FirstLevelSecurityRequiredBranchDepartment;
				isFirstLevelCheckPoint = true;
			}
			else if (checkPoint.Code == Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.Code)
			{
				branchdepartmentList = SecondLevelSecurityRequiredBranchDepartment;
				isSecondLevelCheckPoint = true;
			}
			else if (checkPoint.Code == Env.Security.CreditAdjustmentNotePostingApprovalThirdLevelApproval.Code)
			{
				branchdepartmentList = ThirdLevelSecurityRequiredBranchDepartment;
				isThirdLevelCheckPoint = true;
			}
			else if (checkPoint.Code == Env.Security.CreditAdjustmentNotePostingApprovalFourthLevelApproval.Code)
			{
				branchdepartmentList = FourthLevelSecurityRequiredBranchDepartment;
				isFourthLevelCheckPoint = true;
			}
			else if (checkPoint.Code == Env.Security.CreditAdjustmentNotePostingApprovalFifthLevelApproval.Code)
			{
				branchdepartmentList = FifthLevelSecurityRequiredBranchDepartment;
				isFifthLevelCheckPoint = true;
			}
			else if (checkPoint.Code == Env.Security.CreditAdjustmentNotePostingApprovalSixthLevelApproval.Code)
			{
				branchdepartmentList = SixthLevelSecurityRequiredBranchDepartment;
				isSixthLevelCheckPoint = true;
			}
			var loginController = new UserLoginController();
			foreach (var branchdepartment in branchdepartmentList)
			{
				var userSecurity = loginController.GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, branchdepartment.Branch.ToGuid(), branchdepartment.Department.ToGuid());

				if (isFirstLevelCheckPoint && !userSecurity.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed
				  || isSecondLevelCheckPoint && !userSecurity.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed
				  || isThirdLevelCheckPoint && !userSecurity.CreditAdjustmentNotePostingApprovalThirdLevelApproval.IsAllowed
				  || isFourthLevelCheckPoint && !userSecurity.CreditAdjustmentNotePostingApprovalFourthLevelApproval.IsAllowed
				  || isFifthLevelCheckPoint && !userSecurity.CreditAdjustmentNotePostingApprovalFifthLevelApproval.IsAllowed
				  || isSixthLevelCheckPoint && !userSecurity.CreditAdjustmentNotePostingApprovalSixthLevelApproval.IsAllowed)
				{
					result = false;
					break;
				}
			}
			return result;
		}

		ZBool CheckCredentialOneIsValidUser(LoginFormWithTwoCredentialSupportBranchDepartmentLevel form)
		{
			var result = ZBool.True;
			if (form.Credentials == null || form.Credentials.UserSecurity == null)
			{
				result = ZBool.False;
				var errorMessage = Res.GetString("990116a4-a416-4b03-8f8b-6fcd980c76cc", "1st user error: ");
				if (form.Credentials != null && form.Credentials.LoginAuthentication.State == ZArchitecture.Core.LoginAuthenticationInfo.Status.UserInactive)
				{
					errorMessage += form.Credentials.LoginAuthentication.FailureMessage;
				}
				else
				{
					errorMessage += InvalidLoginErrorMsg;
				}
				Globals.Message.Show(errorMessage);
			}
			return result;
		}

		ZBool CheckCredentialTwoIsValidUser(LoginFormWithTwoCredentialSupportBranchDepartmentLevel form)
		{
			var result = ZBool.True;
			if (form.Credentials2 == null || form.Credentials2.UserSecurity == null)
			{
				result = ZBool.False;
				var errorMessage = Res.GetString("a159a63d-df0e-40e1-a1f4-efb49e96979d", "2nd user error: ");
				if (form.Credentials2 != null && form.Credentials2.LoginAuthentication.State == ZArchitecture.Core.LoginAuthenticationInfo.Status.UserInactive)
				{
					errorMessage += form.Credentials2.LoginAuthentication.FailureMessage;
				}
				else
				{
					errorMessage += InvalidLoginErrorMsg;
				}
				Globals.Message.Show(errorMessage);
			}
			return result;
		}

		protected override string GetReopenClosedJobSecurityGrantedMessage()
		{
			return Res.GetString("Accounting|SecurityOverrideProvider|ClosedJobsPrefix", "Closed Job(s) :") + GetClosedJobs() + "\r\n" + SecurityOverrideProviderWithJobReopenSupport.ReopenClosedJobSecurityGrantedMessage;
		}

		protected override string GetReopenClosedJobSecurityOverrideMessage()
		{
			return Res.GetString("Accounting|SecurityOverrideProvider|ClosedJobsPrefix", "Closed Job(s) :") + GetClosedJobs() + "\r\n" + SecurityOverrideProviderWithJobReopenSupport.ReopenClosedJobSecurityOverrideMessageMoreThanOneJobs;
		}

		protected override string GetReopenRestrictedClosedJobSecurityOverrideMessage()
		{
			return Res.GetString("CA474D15-8C42-4B7F-9FDB-A7180DF4E97A", "Closed Job(s) :") + GetClosedJobs() + "\r\n" + SecurityOverrideProviderWithJobReopenSupport.ReopenRestrictedClosedJobSecurityOverrideMessageMoreThanOneJobs;
		}

		protected override string GetSecurityOverrideMessageCore(SecurityCheckpoint checkPoint)
		{
			string result = "";

			if (checkPoint.Code == Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code ||
				checkPoint.Code == Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.Code)
			{
				result = InvoiceLevelsSecurityOverrideMessage;
			}
			var unapprovedInvoiceSecurityRights = new[] { Env.Security.APUnapprovedInvoicesFirstApproval, Env.Security.APUnapprovedInvoicesSecondApproval, Env.Security.APUnapprovedInvoicesThirdApproval,
														Env.Security.APInvoiceApproval_FirstApproval, Env.Security.APInvoiceApproval_SecondApproval, Env.Security.APInvoiceApproval_ThirdApproval };
			if (unapprovedInvoiceSecurityRights.Contains(checkPoint))
			{
				result = Res.GetString("923e78c1-a4d8-4106-82fe-a10076f76884", "This transaction must be approved by a user with {0} authority.\r\nIf a user with this level of authority enters their username and password below you may continue. Otherwise hit cancel to continue without approving this transaction.", checkPoint.DisplayText);
			}

			if (string.IsNullOrEmpty(result))
			{
				result = base.GetSecurityOverrideMessageCore(checkPoint);
			}

			return result;
		}

		protected override bool UserInitiatorAndNotAllowedToApprove
		{
			get
			{
				return RequiresTwoApprovers || RequiresSequentialApprovals;
			}
		}

		#endregion

		#region Implementation

		bool hasReversingProvider
		{
			get { return ReversingProvider != null; }
		}

		string InvoiceLevelsSecurityOverrideMessage
		{
			get
			{
				return
					Res.GetString("acaa8f4b-aa93-4d28-ae15-8f31ee316543",
@"You do not have security rights to post a credit note/adjustment note for this amount.
A user with security rights to post a credit note/adjustment note can authorize this transaction.
To post this transaction, please have an authorized user enter their username and password below.");
			}
		}

		public bool EnforceToCheckNextLevelOfAuthorization { get; set; }

		protected string GetClosedJobs()
		{
			List<InvoicingBase> invoices = new List<InvoicingBase>();

			if (hasReversingProvider)
			{
				invoices = (from IReversingImplicitlyImplementedWrapperForBinding item in ReversingProvider.TransactionsAlreadyReversedAsITransactionCollection
							where item.WrappedBusinessEntity is InvoicingBase
							select (InvoicingBase)item.WrappedBusinessEntity).ToList();
			}
			else if (InvoicingBase != null)
			{
				invoices.Add(InvoicingBase);
			}

			var jobNumbers = (from InvoicingBase invoice in invoices
							  from Job job in invoice.RelatedJobsForReversing
							  where job.JH_Status == JobHeaderStatus.Closed.Code
							  select job.JH_JobNum).Distinct();

			return new ZStringBuilder(jobNumbers).ToStringWithDelimiterBetweenAppends(", ");
		}

		readonly InvoicingBase InvoicingBase;
		protected readonly MultipleReversingProviderForHeader ReversingProvider;

		readonly SecurityCheckpoint[] reopenJobCheckpoints = new SecurityCheckpoint[]
		{
			Env.Security.ReopenJob,
			Env.Security.ReopenJobPastAllowedReOpenPeriod,
		};

		public List<ZGuid> UserPKsForTwoCredentialLogin { get; set; }
		public bool RequiresSingleApprover => !RequiresSequentialApprovals && !RequiresTwoApprovers;
		public bool RequiresTwoApprovers { get; set; }
		public bool RequiresSequentialApprovals { get; set; }

		#endregion
	}
}
