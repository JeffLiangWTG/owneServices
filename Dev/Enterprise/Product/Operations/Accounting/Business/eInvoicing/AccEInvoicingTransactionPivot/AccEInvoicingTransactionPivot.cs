using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public class AccEInvoicingTransactionPivot : AutoAccEInvoicingTransactionPivot
	{
		public AccEInvoicingTransactionPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AIP_ActionType), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AIP_Status), ConcurrencyPolicy.Strict);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SetCompanyAndCountryCode(GlbCompany.CurrentCompany);
			AIP_ActionType = EInvoicingPivotActionType.Submit;
			AIP_Status = EInvoicingPivotState.Queued;
		}

		#region SuppressResourceStringsCheckRegion
		// Error description does not need to be localised as per requirements

		public static string ErrorDescriptionWhenNoLongerEligible()
			=> FormattableString.Invariant($"Transaction is no longer eligible for E-Reporting/E-Invoicing");

		public const string BaseErrorDescriptionWhenEInvoicingDisabled
			= "E-Reporting/E-Invoicing is disabled for this login company.";

		public static string ErrorDescriptionWhenEInvoicingDisabled()
			=> FormattableString.Invariant($"{BaseErrorDescriptionWhenEInvoicingDisabled} Please review the Registry: {AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.HumanReadableRegistryPath()}");

		#endregion

		#region AIP_ActionType

		[List("Lookups.ActionTypeList")]
		[ReadOnly(true)]
		public override ZString AIP_ActionType
		{
			get => base.AIP_ActionType;
			set
			{
				if (IsInDatabase && AIP_ActionType != value)
				{
					var message = FormattableString.Invariant($"Assigning a different value to {nameof(AIP_ActionType)} on saved {AccEInvoicingTransactionPivotSchema.Constants.TableName} record. Current value: {AIP_ActionType}, new value: {value}.");
					ExceptionReporter.Instance.ReportDeveloperException(GetType().Name + "." + nameof(AIP_ActionType), message, new InvalidOperationException(message));
				}
				base.AIP_ActionType = value;
			}
		}

		#endregion

		#region AIP_Status

		[List("Lookups.StatusList")]
		[ReadOnly(true)]
		public override ZString AIP_Status { get => base.AIP_Status; set => base.AIP_Status = value; }

		#endregion

		[BusinessObjectMaxLengthTestExclude]
		public override ZString AIP_ErrorDescription
		{
			get => base.AIP_ErrorDescription;
			set => DecorateErrorDescriptionByCountryCompliance(value);
		}

		void DecorateErrorDescriptionByCountryCompliance(ZString errorDescription)
		{
			var eReportingStatusMessageProvider = ObjectFactory.Get<ICountryComplianceEInvoicingExtensionFactory>().GetIEReportingStatusMessageProvider(Company?.GC_RN_NKCountryCode ?? ZString.Empty);
			if (eReportingStatusMessageProvider != null)
			{
				var tpa = (Parent as TransactionPendingAllocation);
				var request = tpa?.TransactionApprovalRequest;
				if (request != null)
				{
					errorDescription = eReportingStatusMessageProvider.GetStatusMessage(tpa.ComplianceSubType, request.XP_ApprovalStatus, AIP_Status, AIP_ActionType, errorDescription);
				}
			}

			TruncateAndSetErrorDescription(errorDescription);
		}

		void TruncateAndSetErrorDescription(ZString errorDescription)
		{
			var errorMaxLength = AccEInvoicingTransactionPivotSchema.AIP_ErrorDescription.MaxLength;
			if (errorDescription.Length > errorMaxLength)
			{
				errorDescription = errorDescription.Substring(0, errorMaxLength - 1) + (NoResString)"…";
			}

			base.AIP_ErrorDescription = errorDescription;
		}

		#region AIP_GC and AIP_RN_NKCountryCode

		[ReadOnly(true)]
		public new ZGuid AIP_GC
		{
			get => base.AIP_GC;
			private set => base.AIP_GC = value;
		}

		[ReadOnly(true)]
		public new ZString AIP_RN_NKCountryCode
		{
			get => base.AIP_RN_NKCountryCode;
			private set => base.AIP_RN_NKCountryCode = value;
		}

		public void SetCompanyAndCountryCode(GlbCompany company)
		{
			Argument.NotNull(company, nameof(company));

			base.AIP_GC = company.PK;
			base.AIP_RN_NKCountryCode = company.GC_RN_NKCountryCode;
		}

		#endregion

		#region Parent

		public BusinessObject Parent
		{
			get
			{
				switch (AIP_ParentTableCode)
				{
					case AccTransactionHeaderSchema.Constants.Prefix:
						return ParentTransactionHeader;
					case AccComplianceDocumentHeaderSchema.Constants.Prefix:
						return ParentComplianceDocumentHeader;
					default:
						return null;
				}
			}
		}

		public TransactionHeader ParentTransactionHeader => Factory.Load<TransactionHeader>(AIP_ParentID);

		public AccComplianceDocumentHeader ParentComplianceDocumentHeader => Factory.Load<AccComplianceDocumentHeader>(AIP_ParentID);

		#endregion

		public AccEInvoicingBatch Batch => Factory.Load<AccEInvoicingBatch>(AIP_AIB);

		public void Requeue(bool useQueuedStatus = false)
		{
			AIP_AIB = ZGuid.Empty;

			var status = AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.GetFallBackValueAtAllLevels(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			if (useQueuedStatus || string.IsNullOrEmpty(status))
			{
				status = EInvoicingPivotState.Queued;
			}
			AIP_Status = status;

			AIP_ErrorDescription = ZString.Empty;
			AIP_IsNotifiedByEmail = false;
			AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			AIP_LastSentTimeUtc = ZDateTime.Empty;
		}

		public void AwaitReview()
		{
			AIP_AIB = ZGuid.Empty;
			AIP_Status = EInvoicingPivotState.AwaitingReview;
			AIP_ErrorDescription = Res.GetString("0E0DD69C-62A5-4D66-9A91-E43D437B3104", "Awaiting Review status set by user {0} on day {1}. To send the invoice, click 'Authorize and Send''",
				GlbStaff.CurrentUser.GS_LoginName, ZDateTime.Now);
			AIP_IsNotifiedByEmail = false;
			AIP_LastResponseReceivedUtc = ZDateTime.Empty;
			AIP_LastSentTimeUtc = ZDateTime.Empty;
		}

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new PivotUniqueIndexFailureHandler(this); }
		}

#if DEBUG
		public IUniqueIndexFailureHandler UniqueIndexFailureHandler_ForTestOnly => UniqueIndexFailureHandlers.Single();
#endif

		class PivotUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public PivotUniqueIndexFailureHandler(AccEInvoicingTransactionPivot pivot)
			{
				Pivot = pivot;
			}

			AccEInvoicingTransactionPivot Pivot { get; }
			public IEnumerable<string> HandledUniqueIndexNames => new string[]
				{
					AccEInvoicingTransactionPivotSchema.Constants.Indexes.FK_UX__AIP_AIB_AIP_ParentID,
					AccEInvoicingTransactionPivotSchema.Constants.Indexes.NR_UX__AIP_ParentID_AIP_ActionType
				};

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				switch (indexName)
				{
					case AccEInvoicingTransactionPivotSchema.Constants.Indexes.NR_UX__AIP_ParentID_AIP_ActionType:
						notifier.ReportError(Res.GetString("a2bf5f99-9bd7-4b1c-bceb-4a2ca95c0513", "There is already an active electronic invoicing pivot for the same action type {0}.", Pivot.AIP_ActionType), Res.GetString("61121282-5A1E-4D72-BCEC-C1FDBB81EED5", "Duplicate Electronic Invoicing Pivot"));
						break;
					case AccEInvoicingTransactionPivotSchema.Constants.Indexes.FK_UX__AIP_AIB_AIP_ParentID:
						notifier.ReportError(Res.GetString("176dd5c9-83fc-4cab-bf74-2becf4e73b4e", "There is already an electronic invoicing pivot for the same parent and same electronic invoicing batch."), Res.GetString("61121282-5A1E-4D72-BCEC-C1FDBB81EED5", "Duplicate Electronic Invoicing Pivot"));
						break;
				}
			}
		}

		#endregion

		public ZBool IsSubmitPivotSucceedOrDelivered => AIP_ActionType == EInvoicingPivotActionType.Submit && (AIP_Status == EInvoicingPivotState.Succeed || AIP_Status == EInvoicingPivotState.Delivered);

		public ZBool IsCancelPivotSucceed => AIP_ActionType == EInvoicingPivotActionType.Cancel && AIP_Status == EInvoicingPivotState.Succeed;

		public ZBool IsApprovePivotSucceed => AIP_ActionType == EInvoicingPivotActionType.Approve && AIP_Status == EInvoicingPivotState.Succeed;

		public override void OnSaving()
		{
			base.OnSaving();

			if (IsInDatabase && AIP_ActionType == EInvoicingPivotActionType.Submit && AIP_Status == EInvoicingPivotState.Sent
				&& !AIP_LastSentTimeUtc.IsEmpty && !AIP_LastResponseReceivedUtc.IsEmpty
				&& AIP_LastResponseReceivedUtcInfo.HasChanges
				&& Company.GC_RN_NKCountryCode == CountryCodes.India)
			{
				var transaction = ParentTransactionHeader;
				var authorisation = transaction != null ? AccTransactionHeaderAuthorisationRecordLoader.LoadByParentID(Factory, transaction.PK, transaction.Company?.GC_RN_NKCountryCode ?? string.Empty) : null;
				ErrorReporter.ReportOnce("PivotStatusNotUpdatedOnReceivingResponse_IN",
					FormattableString.Invariant($@"Submit pivot status for the {transaction?.AH_Ledger} {transaction?.AH_TransactionType} {transaction?.AH_TransactionNum} transaction was not updated on receiving response.
Transaction company code: {transaction?.Company?.GC_Code}, post date: {transaction?.AH_PostDate}.
Pivot last sent time: {AIP_LastSentTimeUtc}, last response received original value: {AIP_LastResponseReceivedUtcInfo.OriginalValue} and new value: {AIP_LastResponseReceivedUtc}, decription: {AIP_ErrorDescription}
Authorisation record is {(authorisation != null ? (NoResString)"in database: " + (authorisation.IsInDatabase ? (NoResString)"true" : (NoResString)"false") : "null")}")
					);
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
			=> new AccEInvoicingTransactionPivotFetchStrategy(this);
	}
}
