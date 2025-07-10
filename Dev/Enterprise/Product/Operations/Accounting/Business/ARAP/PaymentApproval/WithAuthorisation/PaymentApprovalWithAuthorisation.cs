using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public abstract partial class PaymentApprovalWithAuthorisation : PaymentApprovalBase
	{
		protected PaymentApprovalWithAuthorisation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		static string PaymentIsPosted => Res.GetString("d1244547-e25d-42fa-9a8b-37d17b465a99", "This Payment is already Posted");
		static string PaymentIsFullyApproved => Res.GetString("070c39f7-b020-4902-83fe-8ab90a670169", "This Payment is already Fully Approved");
		static string PaymentIsRejected => Res.GetString("b068daab-11a6-41ef-9f88-ee949e503d07", "This Payment is already Rejected");
		static string PaymentIsCancelled => Res.GetString("74363bc5-e01f-4ad8-9ede-98d68acc653a", "This Payment is already Canceled");
		static string NoAuthorisationLevelRequired => Res.GetString("8fb5dc39-5a3a-4aaf-a12d-b8065eced372", "This Payment requires no authorization level");
		static string LevelOne => Res.GetString("75a5f366-6627-4964-b6e7-2c22c92b4925", "Level 1");
		static string LevelTwo => Res.GetString("b44042e5-db50-436d-b870-b615453fd2f3", "Level 2");
		static string LevelThree => Res.GetString("ea3bc5d6-4460-4067-8b40-c189866ac95f", "Level 3");

		public static string DealWithDraftPaymentErrorMessage
		{
			get { return ResString.GetMultilingualString("c365ddbe-d31b-484d-9b9f-6e22bc008219", "Payment in 'Draft' status. Please edit and complete the payment, in order to save as 'Awaiting Approval' or 'Fully Approved', then try again."); }
		}

		public new static readonly PaymentApprovalTypeDecider TypeDecider = new PaymentApprovalTypeDecider();

		public override bool ReadOnly
		{
			get
			{
				return IsCancelled || base.ReadOnly;
			}
		}

		#region Logs

		protected override void CreateLogsBeforeSaving()
		{
			base.CreateLogsBeforeSaving();

			CreateEditLog();

			if (!IsRejected)
			{
				CreateLogsForFirstAuthorisation();
				CreateLogsForSecondAuthorisation();
				CreateLogsForThirdAuthorisation();
				CreateLogsForCancellation();
			}
		}

		protected override void ClearCachedLogsAfterSave()
		{
			base.ClearCachedLogsAfterSave();

			CurrentEditLog = null;
			CurrentFirstAuthorisationChangedLog = null;
			CurrentFirstUnAuthorisationChangedLog = null;
			CurrentSecondAuthorisationChangedLog = null;
			CurrentSecondUnAuthorisationChangedLog = null;
			CurrentThirdAuthorisationChangedLog = null;
			CurrentThirdUnAuthorisationChangedLog = null;
			CurrentStatusChangedToCancelLog = null;
		}

		#region SuppressResourceStringsCheckRegion

		internal const string FirstAuthorisationLogText = "1st Authorization Granted";

		internal const string FirstAuthorisationWithdrawnLogText = "1st Authorization Withdrawn";

		internal const string SecondAuthorisationLogText = "2nd Authorization Granted";

		internal const string SecondAuthorisationWithdrawnLogText = "2nd Authorization Withdrawn";

		internal const string ThirdAuthorisationLogText = "3rd Authorization Granted";

		internal const string ThirdAuthorisationWithdrawnLogText = "3rd Authorization Withdrawn";

		internal const string PaymentCancelledLogText = "Payment Cancelled";

		#endregion

		#region Edit Log (EDT)

		void CreateEditLog()
		{
			if (IsInDatabase && CurrentEditLog == null)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				CurrentEditLog = Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		#endregion

		#region First Authorisation Log (ATH/ATW)

		void CreateLogsForFirstAuthorisation()
		{
			if (!IsInDatabase)
			{
				if (!AV_GS_NKApproval1st.IsEmpty)
				{
					CreateFirstAuthorisationLog();
				}
			}
			else if (AV_GS_NKApproval1stInfo.HasChanges)
			{
				if (!AV_GS_NKApproval1st.IsEmpty)
				{
					CreateFirstAuthorisationLog();
				}

				if (!AV_GS_NKApproval1stInfo.OriginalValue.IsEmpty)
				{
					CreateFirstUnAuthorisationLog();
				}
			}
		}

		void CreateFirstAuthorisationLog()
		{
			if (CurrentFirstAuthorisationChangedLog == null)
			{
				CurrentFirstAuthorisationChangedLog = Logs.AddNew(Events.Authorised, FirstAuthorisationLogText);
			}
		}

		void CreateFirstUnAuthorisationLog()
		{
			if (CurrentFirstUnAuthorisationChangedLog == null)
			{
				CurrentFirstUnAuthorisationChangedLog = Logs.AddNew(Events.AuthorisationWithdrawn, FirstAuthorisationWithdrawnLogText);
			}
		}

		#endregion

		#region Second Authorisation Log (ATH/ATW)

		void CreateLogsForSecondAuthorisation()
		{
			if (!IsInDatabase)
			{
				if (!AV_GS_NKApproval2nd.IsEmpty)
				{
					CreateSecondAuthorisationLog();
				}
			}
			else if (AV_GS_NKApproval2ndInfo.HasChanges)
			{
				if (!AV_GS_NKApproval2nd.IsEmpty)
				{
					CreateSecondAuthorisationLog();
				}

				if (!AV_GS_NKApproval2ndInfo.OriginalValue.IsEmpty)
				{
					CreateSecondUnAuthorisationLog();
				}
			}
		}

		void CreateSecondAuthorisationLog()
		{
			if (CurrentSecondAuthorisationChangedLog == null)
			{
				CurrentSecondAuthorisationChangedLog = Logs.AddNew(Events.Authorised, SecondAuthorisationLogText);
			}
		}

		void CreateSecondUnAuthorisationLog()
		{
			if (CurrentSecondUnAuthorisationChangedLog == null)
			{
				CurrentSecondUnAuthorisationChangedLog = Logs.AddNew(Events.AuthorisationWithdrawn, SecondAuthorisationWithdrawnLogText);
			}
		}

		#endregion

		#region Third Authorisation Log (ATH/ATW)

		void CreateLogsForThirdAuthorisation()
		{
			if (!IsInDatabase)
			{
				if (!AV_GS_NKApproval3rd.IsEmpty)
				{
					CreateThirdAuthorisationLog();
				}
			}
			else if (AV_GS_NKApproval3rdInfo.HasChanges)
			{
				if (!AV_GS_NKApproval3rd.IsEmpty)
				{
					CreateThirdAuthorisationLog();
				}

				if (!AV_GS_NKApproval3rdInfo.OriginalValue.IsEmpty)
				{
					CreateThirdUnAuthorisationLog();
				}
			}
		}

		void CreateThirdAuthorisationLog()
		{
			if (CurrentThirdAuthorisationChangedLog == null)
			{
				CurrentThirdAuthorisationChangedLog = Logs.AddNew(Events.Authorised, ThirdAuthorisationLogText);
			}
		}

		void CreateThirdUnAuthorisationLog()
		{
			if (CurrentThirdUnAuthorisationChangedLog == null)
			{
				CurrentThirdUnAuthorisationChangedLog = Logs.AddNew(Events.AuthorisationWithdrawn, ThirdAuthorisationWithdrawnLogText);
			}
		}

		#endregion

		#region Transaction Posted (PST)

		protected override void CreateLogForTransactionPosted()
		{
			if (IsPosted && CurrentTransactionPostedLog == null)
			{
				CurrentTransactionPostedLog = Logs.AddNew(Events.TransactionPosted, UserPostedFullyApprovedTransactionLogText);
			}
		}

		ZString UserPostedFullyApprovedTransactionLogText
		{
			get { return ZString.Format((NoResString)"Posted Payment (Authorization Requirements on Posting: {0})", AV_Calc_DescriptionOfAuthorisationRequired.GetUnresolvedString()); }
		}

		#endregion

		void CreateLogsForCancellation()
		{
			if (IsInDatabase && AV_StatusInfo.HasChanges && AV_Status == PaymentApprovalStatus.Cancelled)
			{
				CreateStatusChangedToCancelLog();
			}
		}

		void CreateStatusChangedToCancelLog()
		{
			if (CurrentStatusChangedToCancelLog == null)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				CurrentStatusChangedToCancelLog = Logs.AddNew(Events.EditedARecord, PaymentCancelledLogText);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		protected StmALog CurrentEditLog;
		protected StmALog CurrentFirstAuthorisationChangedLog;
		protected StmALog CurrentFirstUnAuthorisationChangedLog;
		protected StmALog CurrentSecondAuthorisationChangedLog;
		protected StmALog CurrentSecondUnAuthorisationChangedLog;
		protected StmALog CurrentThirdAuthorisationChangedLog;
		protected StmALog CurrentThirdUnAuthorisationChangedLog;
		protected StmALog CurrentStatusChangedToCancelLog;

		#endregion

		#region Events

		public delegate void RequiredAuthorisationChangedHandler(object sender, string message);
		public event RequiredAuthorisationChangedHandler RequiredAuthorisationChanged;

		public delegate void FirstApprovalStatusChangedHandler(object sender, string message);
		public event FirstApprovalStatusChangedHandler FirstApprovalStatusChanged;

		public delegate void SecondApprovalStatusChangedHandler(object sender, string message);
		public event SecondApprovalStatusChangedHandler SecondApprovalStatusChanged;

		public delegate void ThirdApprovalStatusChangedHandler(object sender, string message);
		public event ThirdApprovalStatusChangedHandler ThirdApprovalStatusChanged;

		#endregion

		#region Authorisation

		public PaymentAuthorisationSettings AuthorisationRequired
		{
			get { return GetAuthorisationRequired(AV_Calc_LocalAmount); }
		}

		protected virtual PaymentAuthorisationSettings GetAuthorisationRequired(ZDecimal localAmount)
		{
			PaymentAuthorisationSettings result = null;

			if (localAmount != 0)
			{
				PaymentAuthorisationSettingsCollection registryValue = AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.Value;
				ZInt currentUpToAmount = -1;

				foreach (PaymentAuthorisationSettings currentSetting in registryValue)
				{
					if (currentSetting.Range == RangeCodes.Above && localAmount > currentSetting.Amount)
					{
						return currentSetting;
					}
				}

				foreach (PaymentAuthorisationSettings currentSetting in registryValue)
				{
					if (currentSetting.Range == RangeCodes.UpTo)
					{
						if (result == null || currentSetting.Amount < result.Amount)
						{
							if (currentSetting.Amount >= localAmount)
							{
								result = currentSetting;
							}
						}
					}
				}
			}

			return result;
		}

		[ReadOnly(true)]
		public MultilingualString AV_Calc_DescriptionOfAuthorisationRequired
		{
			get { return AV_Calc_DescriptionOfAuthorisationRequiredCore; }
		}

		protected override MultilingualString AV_Calc_DescriptionOfAuthorisationRequiredCore
		{
			get
			{
				MultilingualString result;

				if (AV_Calc_LocalAmount <= 0)
				{
					result = ResString.GetMultilingualString("Accounting|PaymentApprovalWithAuthorisation|PaymentAmountMustBeGreaterThanZero", "Payment amount must be greater than zero");
				}
				else if (AuthorisationRequired == null)
				{
					result = ResString.GetMultilingualString("Accounting|PaymentApprovalWithAuthorisation|NoAuthorizationRequired", "No Authorization Required");
				}
				else
				{
					result = GetDescriptionForDisplay(AuthorisationRequired);
				}

				return result;
			}
		}

		MultilingualString GetDescriptionForDisplay(PaymentAuthorisationSettings setting)
		{
			MultilingualString result = (NoResString)string.Empty;

			if (setting != null)
			{
				ZDecimal amount = setting.Amount;
				var rangeDescription = setting.RangeMultilingual;
				var requirementDescription = setting.AuthorisationRequirementMultilingual;
				result = ResString.GetMultilingualString("B9270455-ED4D-490D-8A39-F96C9C89BB0D", "{0} ({1} {2})", requirementDescription, rangeDescription, amount);
			}

			return result;
		}

		public ZPropertyInfo AV_Calc_DescriptionOfAuthorisationRequiredInfo
		{
			get { return GetZPropertyInfo(nameof(AV_Calc_DescriptionOfAuthorisationRequired)); }
		}

		public override bool NoAuthorisationRequired
		{
			get { return AuthorisationRequired == null || AuthorisationRequired.AuthorisationRequirement == AuthorisationCodes.NoApprovalRequired; }
		}

		protected override void FireRequiredAuthorisationChanged() => RequiredAuthorisationChanged?.Invoke(this, string.Empty);

		public string GetApprovalStatus()
		{
			bool isAuthorisationSufficient = NoAuthorisationRequired || 
											((!Level1AuthorisationRequired || Approval1st != null) &&
											(!Level2AuthorisationRequired || Approval2nd != null) &&
											(!Level3AuthorisationRequired || Approval3rd != null));

			return isAuthorisationSufficient ? PaymentApprovalStatus.FullyApproved : PaymentApprovalStatus.AwaitingApproval;
		}

		void ResetApprovalStatus()
		{
			AV_Status = GetApprovalStatus();
		}

		protected override void UpdateStatus()
		{
			if (!IsPosted && !IsRejected && !IsDraft)
			{
				ResetApprovalStatus();
			}
		}

		public override void UpdateDraftStatus()
		{
			base.UpdateDraftStatus();

			if (IsInDatabase && IsDraft && !IsSavingPaymentApprovalAsDraft)
			{
				ResetApprovalStatus();
			}
		}

		void ResetAuthorisationToUnapproved(bool shouldCheckExRateTolerance = false)
		{
			if (IsRejected)
			{
				AV_GS_NKApproval1st = ZString.Empty;
			}

			if (shouldCheckExRateTolerance && CheckIsExceedExchangeRateTolerance(out bool isExceedExchangeRateTolerance))
			{
				if (isExceedExchangeRateTolerance)
				{
					if (!AV_GS_NKApproval1st.IsEmpty)
					{
						AV_GS_NKApproval1st = ZString.Empty;
					}

					if (!AV_GS_NKApproval2nd.IsEmpty)
					{
						AV_GS_NKApproval2nd = ZString.Empty;
					}

					if (!AV_GS_NKApproval3rd.IsEmpty)
					{
						AV_GS_NKApproval3rd = ZString.Empty;
					}
				}
			}
			else
			{
				if (!Level1AuthorisationRequired || AV_GS_NKApproval1st != GlbStaff.CurrentUser.GS_Code)
				{
					AV_GS_NKApproval1st = ZString.Empty;
				}

				if (!Level2AuthorisationRequired || AV_GS_NKApproval2nd != GlbStaff.CurrentUser.GS_Code)
				{
					AV_GS_NKApproval2nd = ZString.Empty;
				}

				if (!Level3AuthorisationRequired || AV_GS_NKApproval3rd != GlbStaff.CurrentUser.GS_Code)
				{
					AV_GS_NKApproval3rd = ZString.Empty;
				}
			}

			if (!IsDraft)
			{
				ResetApprovalStatus();
			}
		}

		#region Exchange Rate Tolerance

		bool CheckIsExceedExchangeRateTolerance(out bool isExceedExchangeRateTolerance)
		{
			isExceedExchangeRateTolerance = false;

			if (!IsInDatabase)
			{
				return false;
			}

			var exchangeRateTolerance = AccountingConfigurationRegistry.Instance.ExchangeRateTolerance.Value.ExchangeRateToleranceCollection.FindValueWithFallback(AV_RX_NKPaymentCurrency);
			bool isExchangeRateToleranceEnabled = exchangeRateTolerance.ExchangeRateTolerancePercentage != 0M;

			if (!isExchangeRateToleranceEnabled)
			{
				return false;
			}

			var originalLocalAmount = GetAV_Calc_LocalAmountOriginalValue();
			bool isAuthorizationLevelChanged = GetAuthorisationRequired(originalLocalAmount)?.AuthorisationRequirement != AuthorisationRequired?.AuthorisationRequirement;

			if (isAuthorizationLevelChanged)
			{
				return false;
			}

			var localAmountDiff = AV_Calc_LocalAmount - originalLocalAmount;

			if (localAmountDiff > 0)
			{
				var exchangeRateDiff = (localAmountDiff / originalLocalAmount) * 100;
				isExceedExchangeRateTolerance = exchangeRateDiff > exchangeRateTolerance.ExchangeRateTolerancePercentage;
			}

			return true;
		}

		#endregion

		public static class AuthorisationStatus
		{
			public static string NotRequired { get { return Res.GetString("Accounting|PaymentApprovalWithAuthorisation|NotRequired", "Not Required"); } }
			public static string AwaitingAuthorisation { get { return Res.GetString("Accounting|PaymentApprovalWithAuthorisation|AwaitingAuthorisation", "Awaiting Authorization"); } }
			public static string Authorised { get { return Res.GetString("Accounting|PaymentApprovalWithAuthorisation|Authorised", "Authorized"); } }
			public static string Rejected { get { return Res.GetString("Accounting|PaymentApprovalWithAuthorisation|Rejected", "Rejected"); } }
		}

		ZString GetAuthorisationStatus(bool authorisationRequired, GlbStaff authorisingPerson)
		{
			ZString result;
			if (IsRejected)
			{
				result = authorisingPerson != null ? AuthorisationStatus.Rejected : AuthorisationStatus.NotRequired;
			}
			else if (authorisationRequired)
			{
				result = authorisingPerson != null ? AuthorisationStatus.Authorised : AuthorisationStatus.AwaitingAuthorisation;
			}
			else
			{
				result = AuthorisationStatus.NotRequired;
			}

			return result;
		}

		protected override void AuthorisationChanged()
		{
			AV_Calc_DescriptionOfAuthorisationRequiredInfo.RefreshBinding();
		}

		#region Cancel

		public ZBool UserHasCancelSecurity => UserHasCancelSecurityCore;

		protected virtual ZBool UserHasCancelSecurityCore => CancelApprovalCheckpoint.IsAllowed;

		protected abstract SecurityCheckpoint CancelApprovalCheckpoint
		{
			get;
		}

		#endregion

		#region Level 1

		public bool ApproveFirstApproval()
		{
			bool result = false;
			if (!IsPosted && UserHasAuthoriseLevel1Security && Approval1st == null && Level1AuthorisationRequired)
			{
				AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
				SetNotificationEmailIfPossible(ActionType.Authorize);
				result = true;
			}

			return result;
		}

		public bool UnApproveFirstApproval()
		{
			bool result = false;
			if (!IsPosted && Approval1st != null &&
				Level1AuthorisationRequired && UserHasAuthoriseLevel1Security)
			{
				AV_GS_NKApproval1st = ZString.Empty;
				SetNotificationEmailIfPossible(ActionType.Unauthorize);
				result = true;
			}

			return result;
		}

		public ZString Level1AuthorisationStatus
		{
			get { return GetAuthorisationStatus(Level1AuthorisationRequired, Approval1st); }
		}

		public ZPropertyInfo Level1AuthorisationStatusInfo
		{
			get { return GetZPropertyInfo(nameof(Level1AuthorisationStatus)); }
		}

		public ZBool UserCanAuthoriseLevel1
		{
			get { return Level1AuthorisationRequired && UserHasAuthoriseLevel1Security && UserHasSecurityToApproveThisTransaction; }
		}

		public ZBool Level1AuthorisationRequired
		{
			get { return Level1AuthorisationRequiredCore; }
		}

		protected virtual ZBool Level1AuthorisationRequiredCore
		{
			get
			{
				ZString authorisationType = AuthorisationRequired != null ? AuthorisationRequired.AuthorisationRequirement : ZString.Empty;

				return
					authorisationType == AuthorisationCodes.FirstApprovalRequiredOnly ||
					authorisationType == AuthorisationCodes.FirstAndSecondApprovalRequired ||
					authorisationType == AuthorisationCodes.FirstAndThirdApprovalRequired ||
					authorisationType == AuthorisationCodes.AllThreeApprovalRequired;
			}
		}

		public ZBool UserHasAuthoriseLevel1Security
		{
			get { return UserHasAuthoriseLevel1SecurityCore && UserHasSecurityToApproveThisTransaction; }
		}

		protected virtual ZBool UserHasAuthoriseLevel1SecurityCore
		{
			get { return FirstApprovalCheckpoint.IsAllowed; }
		}

		protected override void FireFirstApprovalStatusChanged()
		{
			if (FirstApprovalStatusChanged != null)
			{
				FirstApprovalStatusChanged(this, string.Empty);
			}
		}

		protected abstract SecurityCheckpoint FirstApprovalCheckpoint
		{
			get;
		}

		#endregion

		#region Level 2

		public bool ApproveSecondApproval()
		{
			bool result = false;
			if (!IsPosted && UserHasAuthoriseLevel2Security && Approval2nd == null && Level2AuthorisationRequired)
			{
				AV_GS_NKApproval2nd = GlbStaff.CurrentUser.GS_Code;
				SetNotificationEmailIfPossible(ActionType.Authorize);
				result = true;
			}

			return result;
		}

		public bool UnApproveSecondApproval()
		{
			bool result = false;
			if (!IsPosted && Approval2nd != null &&
				Level2AuthorisationRequired && UserHasAuthoriseLevel2Security)
			{
				AV_GS_NKApproval2nd = ZString.Empty;
				SetNotificationEmailIfPossible(ActionType.Unauthorize);
				result = true;
			}

			return result;
		}

		public ZString Level2AuthorisationStatus
		{
			get { return GetAuthorisationStatus(Level2AuthorisationRequired, Approval2nd); }
		}

		public ZBool UserCanAuthoriseLevel2
		{
			get { return Level2AuthorisationRequired && UserHasAuthoriseLevel2Security && UserHasSecurityToApproveThisTransaction; }
		}

		public ZPropertyInfo Level2AuthorisationStatusInfo
		{
			get { return GetZPropertyInfo(nameof(Level2AuthorisationStatus)); }
		}

		public ZBool Level2AuthorisationRequired
		{
			get { return Level2AuthorisationRequiredCore; }
		}

		protected virtual ZBool Level2AuthorisationRequiredCore
		{
			get
			{
				ZString authorisationType = AuthorisationRequired != null ? AuthorisationRequired.AuthorisationRequirement : ZString.Empty;

				return
					authorisationType == AuthorisationCodes.SecondApprovalRequiredOnly ||
					authorisationType == AuthorisationCodes.FirstAndSecondApprovalRequired ||
					authorisationType == AuthorisationCodes.SecondAndThirdApprovalRequired ||
					authorisationType == AuthorisationCodes.AllThreeApprovalRequired;
			}
		}

		public ZBool UserHasAuthoriseLevel2Security
		{
			get { return UserHasAuthoriseLevel2SecurityCore; }
		}

		protected virtual ZBool UserHasAuthoriseLevel2SecurityCore
		{
			get { return SecondApprovalCheckpoint.IsAllowed; }
		}

		protected override void FireSecondApprovalStatusChanged()
		{
			if (SecondApprovalStatusChanged != null)
			{
				SecondApprovalStatusChanged(this, string.Empty);
			}
		}

		protected abstract SecurityCheckpoint SecondApprovalCheckpoint
		{
			get;
		}

		#endregion

		#region Level 3

		public bool ApproveThirdApproval()
		{
			bool result = false;
			if (!IsPosted && UserHasAuthoriseLevel3Security && Approval3rd == null && Level3AuthorisationRequired)
			{
				AV_GS_NKApproval3rd = GlbStaff.CurrentUser.GS_Code;
				SetNotificationEmailIfPossible(ActionType.Authorize);
				result = true;
			}

			return result;
		}

		public bool UnApproveThirdApproval()
		{
			bool result = false;
			if (!IsPosted && Approval3rd != null &&
				Level3AuthorisationRequired && UserHasAuthoriseLevel3Security)
			{
				AV_GS_NKApproval3rd = ZString.Empty;
				SetNotificationEmailIfPossible(ActionType.Unauthorize);
				result = true;
			}

			return result;
		}

		public ZString Level3AuthorisationStatus
		{
			get { return GetAuthorisationStatus(Level3AuthorisationRequired, Approval3rd); }
		}

		public ZBool UserCanAuthoriseLevel3
		{
			get { return Level3AuthorisationRequired && UserHasAuthoriseLevel3Security && UserHasSecurityToApproveThisTransaction; }
		}

		public ZPropertyInfo Level3AuthorisationStatusInfo
		{
			get { return GetZPropertyInfo(nameof(Level3AuthorisationStatus)); }
		}

		public ZBool Level3AuthorisationRequired
		{
			get { return Level3AuthorisationRequiredCore; }
		}

		protected virtual ZBool Level3AuthorisationRequiredCore
		{
			get
			{
				ZString authorisationType = AuthorisationRequired != null ? AuthorisationRequired.AuthorisationRequirement : ZString.Empty;

				return
					authorisationType == AuthorisationCodes.ThirdApprovalRequiredOnly ||
					authorisationType == AuthorisationCodes.FirstAndThirdApprovalRequired ||
					authorisationType == AuthorisationCodes.SecondAndThirdApprovalRequired ||
					authorisationType == AuthorisationCodes.AllThreeApprovalRequired;
			}
		}

		public ZBool UserHasAuthoriseLevel3Security
		{
			get { return UserHasAuthoriseLevel3SecurityCore; }
		}

		protected virtual ZBool UserHasAuthoriseLevel3SecurityCore
		{
			get { return ThirdApprovalCheckpoint.IsAllowed; }
		}

		protected override void FireThirdApprovalStatusChanged()
		{
			if (ThirdApprovalStatusChanged != null)
			{
				ThirdApprovalStatusChanged(this, string.Empty);
			}
		}

		protected abstract SecurityCheckpoint ThirdApprovalCheckpoint
		{
			get;
		}

		#endregion

		public bool UserIsAuthorised
		{
			get
			{
				if ((Level1AuthorisationRequired && !UserHasAuthoriseLevel1Security)
					|| (Level2AuthorisationRequired && !(UserHasAuthoriseLevel2Security && UserHasSecurityToApproveThisTransaction))
					|| (Level3AuthorisationRequired && !(UserHasAuthoriseLevel3Security && UserHasSecurityToApproveThisTransaction)))
				{
					return false;
				}
				return true;
			}
		}

		bool UserHasSecurityToApproveThisTransaction
		{
			get { return ChequeBook == null || GlbBranch.CurrentBranch.PK == ChequeBook.AK_GB || Env.Security.APPaymentProcessingApproveNonLoginBranchTransactions.IsAllowed; }
		}

		public bool FullyApprove()
		{
			bool result = false;

			if (IsAwaitingApproval)
			{
				result = ApproveFirstApproval() | ApproveSecondApproval() | ApproveThirdApproval();
			}

			return result;
		}

		#endregion

		#region Submit For Approval

		public void TrySubmitForApproval(INotifications buffer)
		{
			if (!IsDraft)
			{
				buffer.AddError(Res.GetString("44E44ACB-FBF3-4B93-BE12-1F40714A7701", "This Payment is not in Draft status"));
			}
			else
			{
				MatchingBaseObject.RunPreSaveValidation();
				if (MatchingBaseObject.HasErrors)
				{
					buffer.AddRange(MatchingBaseObject.GetErrors());
				}
				else
				{
					ResetApprovalStatus();
				}
			}
		}

		#endregion

		#region Cancel Payment

		public void TryCancelPayment(INotifications buffer)
		{
			if (HasActiveDeal)
			{
				buffer.AddError(ActiveDealErrorTextForActionItems);
			}
			else if (AV_Status == PaymentApprovalStatus.Posted)
			{
				buffer.AddError(PaymentIsPosted);
			}
			else if (IsCancelled)
			{
				buffer.AddError(PaymentIsCancelled);
			}
			else
			{
				var message = BuildMissingLevelsRequiredMessageForCancel();
				if (!message.IsEmpty)
				{
					buffer.AddError(Res.GetString("68D4229E-6E6E-409B-B312-4EEA9752AC83", "You do not have sufficient rights to cancel this payment. {0}", message));
				}
				else
				{
					CancelCore();
				}
			}
		}

		void CancelCore()
		{
			AV_Status = PaymentApprovalStatus.Cancelled;
			UnmatchAllTransactions();
			SetNotificationEmailIfPossible(ActionType.Cancel);
		}

		void UnmatchAllTransactions()
		{
			var approvalItems = new PaymentApprovalItemCollection(this);
			approvalItems.Load();
			approvalItems.RemoveAndDeleteAll();
		}

		ZString BuildMissingLevelsRequiredMessageForCancel()
		{
			var result = ZString.Empty;
			if (!UserHasCancelSecurity)
			{
				result = Res.GetString("0b5e7c7c-2714-49fb-8b52-c5ceda6da734", "You have not been granted security rights to {0}.", CancelApprovalCheckpoint.DisplayTextPathToSecurityRight);
			}
			return result;
		}

		#endregion

		#region Reject Payment

		public void TryRejectPayment(INotifications buffer)
		{
			if (HasActiveDeal)
			{
				buffer.AddError(ActiveDealErrorTextForActionItems);
			}
			else if (AV_Status == PaymentApprovalStatus.Posted)
			{
				buffer.AddError(PaymentIsPosted);
			}
			else if (IsCancelled)
			{
				buffer.AddError(PaymentIsCancelled);
			}
			else if (IsRejected)
			{
				buffer.AddError(PaymentIsRejected);
			}
			else if (IsDraft)
			{
				buffer.AddError(DealWithDraftPaymentErrorMessage);
			}
			else if (NoAuthorisationRequired)
			{
				RejectCore();
			}
			else
			{
				var message = BuildMissingLevelsRequiredMessageForReject();
				if (!message.IsEmpty)
				{
					buffer.AddError(Res.GetString("a605f393-96e8-4677-857a-be592857c0d7", "You do not have sufficient rights to reject this payment. {0}", message));
				}
				else
				{
					RejectCore();
				}
			}
		}

		void RejectCore()
		{
			AV_Status = PaymentApprovalStatus.Rejected;
			AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
			AV_GS_NKApproval2nd = ZString.Empty;
			AV_GS_NKApproval3rd = ZString.Empty;
			FireRequiredAuthorisationChanged();
			SetNotificationEmailIfPossible(ActionType.Reject);
		}

		ZString BuildMissingLevelsRequiredMessageForReject()
		{
			var result = ZString.Empty;
			if (!UserHasRejectSecurity)
			{
				var requiredLevelsBuilder = new ZStringBuilder();
				if (Level1AuthorisationRequired)
				{
					requiredLevelsBuilder.Append(LevelOne);
				}
				if (Level2AuthorisationRequired)
				{
					requiredLevelsBuilder.Append(LevelTwo);
				}
				if (Level3AuthorisationRequired)
				{
					requiredLevelsBuilder.Append(LevelThree);
				}
				if (requiredLevelsBuilder.Length > 0)
				{
					result = Res.GetString("c3abe93f-255e-4aae-bc69-70d50f41f2fd", "Required authorization level: {0}.", requiredLevelsBuilder.ToStringWithDelimiterBetweenAppends((NoResString)" and "));
				}
			}
			return result;
		}

		public ZBool UserHasRejectSecurity => (Approval1st == null || UserCanAuthoriseLevel1)
			&& (Approval2nd == null || UserCanAuthoriseLevel2)
			&& (Approval3rd == null || UserCanAuthoriseLevel3);

		#endregion

		#region Authorise Payment

		public void TryAuthorisePayment(INotifications buffer)
		{
			if (IsPosted)
			{
				buffer.AddError(PaymentIsPosted);
			}
			else if (IsFullyApproved)
			{
				buffer.AddError(PaymentIsFullyApproved);
			}
			else if (IsCancelled)
			{
				buffer.AddError(PaymentIsCancelled);
			}
			else if (IsRejected)
			{
				buffer.AddError(PaymentIsRejected);
			}
			else if (IsDraft)
			{
				buffer.AddError(DealWithDraftPaymentErrorMessage);
			}
			else // is awaiting approval
			{
				IsLoadedFromGUI = false;
				if (NoAuthorisationRequired)
				{
					AV_Status = PaymentApprovalStatus.FullyApproved;
					SetNotificationEmailIfPossible(ActionType.Authorize);
				}
				else
				{
					bool approvedAtLeastOneLevel = ApproveFirstApproval() | ApproveSecondApproval() | ApproveThirdApproval();
					if (!approvedAtLeastOneLevel)
					{
						var message = BuildMissingLevelsRequiredMessageForAuthorise();
						if (!message.IsEmpty)
						{
							buffer.AddError(Res.GetString("6339c69f-76f0-4eb6-bdee-8b6a347db724", "You do not have sufficient rights to authorize this payment. {0}", message));
						}
					}
				}
			}
		}

		ZString BuildMissingLevelsRequiredMessageForAuthorise()
		{
			var result = ZString.Empty;

			if (!(UserCanAuthoriseLevel1 && Approval1st == null
				|| UserCanAuthoriseLevel2 && Approval2nd == null
				|| UserCanAuthoriseLevel3 && Approval3rd == null))
			{
				var requiredLevelsBuilder = new ZStringBuilder();
				if (Level1AuthorisationRequired && Approval1st == null)
				{
					requiredLevelsBuilder.Append(LevelOne);
				}
				if (Level2AuthorisationRequired && Approval2nd == null)
				{
					requiredLevelsBuilder.Append(LevelTwo);
				}
				if (Level3AuthorisationRequired && Approval3rd == null)
				{
					requiredLevelsBuilder.Append(LevelThree);
				}
				if (requiredLevelsBuilder.Length > 0)
				{
					result = Res.GetString("c3abe93f-255e-4aae-bc69-70d50f41f2fd", "Required authorization level: {0}.", requiredLevelsBuilder.ToStringWithDelimiterBetweenAppends((NoResString)" and "));
				}
			}
			return result;
		}

		#endregion

		#region Unauthorise Payment

		public void TryUnauthorisePayment(INotifications buffer)
		{
			if (HasActiveDeal)
			{
				buffer.AddError(ActiveDealErrorTextForActionItems);
			}
			else if (IsPosted)
			{
				buffer.AddError(PaymentIsPosted);
			}
			else if (IsCancelled)
			{
				buffer.AddError(PaymentIsCancelled);
			}
			else if (IsRejected)
			{
				buffer.AddError(PaymentIsRejected);
			}
			else if (IsDraft)
			{
				buffer.AddError(DealWithDraftPaymentErrorMessage);
			}
			else if (IsFullyApproved && NoAuthorisationRequired)
			{
				buffer.AddError(NoAuthorisationLevelRequired);
			}
			else
			{
				IsLoadedFromGUI = false;

				bool unapprovedAtLeastOneLevel = UnApproveFirstApproval() | UnApproveSecondApproval() | UnApproveThirdApproval();
				if (!unapprovedAtLeastOneLevel)
				{
					var message = BuildMissingLevelsRequiredMessageForUnauthorise();
					if (!message.IsEmpty)
					{
						buffer.AddError(Res.GetString("ce1111ee-1d08-477b-a596-82912f872247", "You do not have sufficient rights to unauthorize this payment. {0}", message));
					}
				}
				else
				{
					SetNotificationEmailIfPossible(ActionType.Unauthorize);
				}
			}
		}

		ZString BuildMissingLevelsRequiredMessageForUnauthorise()
		{
			ZString result = ZString.Empty;
			if (!(UserCanAuthoriseLevel1 && Approval1st != null
				|| UserCanAuthoriseLevel2 && Approval2nd != null
				|| UserCanAuthoriseLevel3 && Approval3rd != null))
			{
				var requiredLevelsBuilder = new ZStringBuilder();
				if (Level1AuthorisationRequired && Approval1st != null)
				{
					requiredLevelsBuilder.Append(LevelOne);
				}
				if (Level2AuthorisationRequired && Approval2nd != null)
				{
					requiredLevelsBuilder.Append(LevelTwo);
				}
				if (Level3AuthorisationRequired && Approval3rd != null)
				{
					requiredLevelsBuilder.Append(LevelThree);
				}
				if (requiredLevelsBuilder.Length > 0)
				{
					result = Res.GetString("c3abe93f-255e-4aae-bc69-70d50f41f2fd", "Required authorization level: {0}.", requiredLevelsBuilder.ToStringWithDelimiterBetweenAppends((NoResString)" and "));
				}
			}
			return result;
		}

		[ResourceStringData("E7FD1636-079A-453E-A78F-F8E350325337", Caption = "Rejection Reason")]
		public ZString Reason
		{
			get
			{
				var sb = new ZStringBuilder();
				sb.AppendIfNotEmpty(AV_RejectionReasonCode);
				sb.AppendIfNotEmpty(AccountingMasterFilesRegistry.Instance.PaymentRejectionReasonCodesList.Value.GetDescriptionFromCode(AV_RejectionReasonCode));
				return sb.ToStringWithDelimiterBetweenAppends(" - ");
			}
		}

		public ZPropertyInfo ReasonInfo => GetZPropertyInfo(nameof(Reason));

		#endregion

		#region Property Overrides

		[List("BranchCollection")]
		public ZGuid AV_AK_GB
		{
			get { return ChequeBook == null ? ZGuid.Empty : ChequeBook.AK_GB; }
		}

		public ZPropertyInfo AV_AK_GBInfo
		{
			get { return GetZPropertyInfo(nameof(AV_AK_GB)); }
		}

		public override ZString AV_Status
		{
			get { return base.AV_Status; }
			set
			{
				var originalValue = base.AV_Status;
				base.AV_Status = value;

				if (originalValue != value)
				{
					FireRequiredAuthorisationChanged();
					if (originalValue == PaymentApprovalStatus.Rejected)
					{
						AV_RejectionReasonCode = ZString.Empty;
						AV_RejectionReasonDetails = ZString.Empty;
					}
				}
			}
		}

		public override ZDateTime AV_PaymentDate
		{
			get { return base.AV_PaymentDate; }
			set
			{
				ZDateTime originalValue = AV_PaymentDate;

				base.AV_PaymentDate = value;

				if (originalValue != AV_PaymentDate)
				{
					ResetAuthorisationToUnapproved();
				}
			}
		}

		public override ZDateTime AV_PostDate
		{
			get { return base.AV_PostDate; }
			set
			{
				ZDateTime originalValue = AV_PostDate;

				base.AV_PostDate = value;

				if (originalValue != AV_PostDate)
				{
					ResetAuthorisationToUnapproved();
				}
			}
		}

		public override ZGuid AV_OH
		{
			get { return base.AV_OH; }
			set
			{
				ZGuid originalValue = AV_OH;

				base.AV_OH = value;

				if (originalValue != AV_OH)
				{
					ResetAuthorisationToUnapproved();
				}
			}
		}

		public override ZString AV_PaymentType
		{
			get { return base.AV_PaymentType; }
			set
			{
				ZString originalValue = AV_PaymentType;

				base.AV_PaymentType = value;

				if (originalValue != AV_PaymentType)
				{
					ResetAuthorisationToUnapproved();
				}
			}
		}

		public override ZGuid AV_AB
		{
			get { return base.AV_AB; }
			set
			{
				ZGuid originalValue = AV_AB;

				base.AV_AB = value;

				if (originalValue.IsValid && originalValue != AV_AB)
				{
					ResetAuthorisationToUnapproved();
				}
			}
		}

		public override ZGuid AV_AK
		{
			get { return base.AV_AK; }
			set
			{
				ZGuid originalValue = AV_AK;

				base.AV_AK = value;

				if (originalValue.IsValid && originalValue != AV_AK)
				{
					ResetAuthorisationToUnapproved();
				}
			}
		}

		public override ZDecimal AV_Amount
		{
			get { return base.AV_Amount; }
			set
			{
				ZDecimal originalValue = AV_Amount;

				base.AV_Amount = value;
				FireRequiredAuthorisationChanged();
				AuthorisationChanged();

				if (originalValue != AV_Amount)
				{
					ResetAuthorisationToUnapproved();
				}
			}
		}

		public override ZString AV_GS_NKApproval1st
		{
			get { return base.AV_GS_NKApproval1st; }
			set
			{
				base.AV_GS_NKApproval1st = value;
				UpdateStatus();
				FireFirstApprovalStatusChanged();
			}
		}

		public override ZString AV_GS_NKApproval2nd
		{
			get { return base.AV_GS_NKApproval2nd; }
			set
			{
				base.AV_GS_NKApproval2nd = value;
				UpdateStatus();
				FireSecondApprovalStatusChanged();
			}
		}

		public override ZString AV_GS_NKApproval3rd
		{
			get { return base.AV_GS_NKApproval3rd; }
			set
			{
				base.AV_GS_NKApproval3rd = value;
				UpdateStatus();
				FireThirdApprovalStatusChanged();
			}
		}

		public ReadOnlyCodeDescriptionPairList RejectionReasonCodesList => AccountingMasterFilesRegistry.Instance.PaymentRejectionReasonCodesList.Value;

		[MaxLength(3)]
		[List(nameof(RejectionReasonCodesList))]
		public override ZString AV_RejectionReasonCode
		{
			get => base.AV_RejectionReasonCode;

			set
			{
				if (AV_RejectionReasonCode != value)
				{
					AV_RejectionReasonDetails = RejectionReasonCodesList.GetDescriptionFromCode(value);
				}
				base.AV_RejectionReasonCode = value;
			}
		}

		[MaxLength(128)]
		public override ZString AV_RejectionReasonDetails { get => base.AV_RejectionReasonDetails; set => base.AV_RejectionReasonDetails = value; }

		public override ZDecimal AV_PayExRate
		{
			get { return base.AV_PayExRate; }
			set
			{
				ZDecimal originalValue = AV_PayExRate;

				base.AV_PayExRate = value;

				if (originalValue != AV_PayExRate)
				{
					ResetAuthorisationToUnapproved(!AV_AmountInfo.HasChanges);
				}
			}
		}

		protected override void TransactionsSelectedChanged()
		{
			base.TransactionsSelectedChanged();

			ResetAuthorisationToUnapproved();
		}

		#endregion

		#region Cancel E-Payment

		public bool IsCancelEPaymentAllowed => CancelEPaymentSecurityCheckPoint.IsAllowed;

		public abstract SecurityCheckpoint CancelEPaymentSecurityCheckPoint
		{
			get;
		}

		public bool IsCancelEPaymentPossible
		{
			get
			{
				return CurrentDeal != null && EPaymentStatusCodes.Deal.StatusCodesAllowedToCancel.Contains<string>(CurrentDeal.AED_Status);
			}
		}

		public void CancelEPayment()
		{
			if (CurrentDeal != null)
			{
				CurrentDeal.CancelEPayment();
				CurrentDeal.Factory.Save();
			}
		}

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (AV_Status != PaymentApprovalStatus.Rejected)
			{
				AV_RejectionReasonCode = ZString.Empty;
				AV_RejectionReasonDetails = ZString.Empty;
			}

			if (!AV_StatusInfo.HasChanges
				&& !AV_GS_NKApproval1stInfo.HasChanges
				&& !AV_GS_NKApproval2ndInfo.HasChanges
				&& !AV_GS_NKApproval3rdInfo.HasChanges)
			{
				QueuedNotificationEmail = null;
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				QueuedNotificationEmail?.Send();
				QueuedNotificationEmail = null;
			}
		}

		PaymentApprovalAuthorizationNotificationEmail QueuedNotificationEmail;

#if DEBUG
		internal
#endif
		bool CanSendNotificationEmail() => Logs.CreatedByUserInitials != GlbStaff.CurrentUser.GS_Code
			&& !GlbStaff.GetEmailAddressFromUserCode(Factory, Logs.CreatedByUserInitials).IsNullOrEmpty();

		void SetNotificationEmailIfPossible(ActionType actionType)
		{
			if (CanSendNotificationEmail())
			{
				QueuedNotificationEmail = new PaymentApprovalAuthorizationNotificationEmail(actionType, this);
			}
		}

		protected override bool PostsOnSaveCore
		{
			get { return false; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			if (AccountingConfigurationRegistry.Instance.PayableAuthorizationSettings.Value.Count == 0)
			{
				AV_Status = PaymentApprovalStatus.FullyApproved;
			}
			else
			{
				AV_Status = PaymentApprovalStatus.AwaitingApproval;
			}
		}

		public enum ActionType
		{
			Authorize,
			Unauthorize,
			Reject,
			Cancel,
			SubmitForApproval,
			ApproveForPosting
		}
	}
}
