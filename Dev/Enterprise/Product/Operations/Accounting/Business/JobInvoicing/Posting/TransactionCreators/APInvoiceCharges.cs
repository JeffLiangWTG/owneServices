using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class APInvoiceCharges : NonPersistentBusinessObject, ITransactionForApproval, IObsoleteValidation, ITransactionBranchCalculationDataProviderFromJobCharge
	{
		public abstract class Schema
		{
			public const string IsExcludedFromPosting = "IsExcludedFromPosting";
			public const string ApprovingRequestDescription = "ApprovingRequestDescription";
			public const string RequisitionStatus = "RequisitionStatus";
			public const string RequisitionDate = "RequisitionDate";
		}

		public APInvoiceCharges(ZString creditor, ZString invoiceNumber, ZGuid jobPK, string jobParentTableCode, IPostingJobTransactionsApprovalGUIProvider parentPostingGUIProvider, ZString placeOfSupply = default)
		{
			this.parentPostingGUIProvider = parentPostingGUIProvider;
			Creditor = creditor;
			InvoiceNumber = invoiceNumber;
			JobPK = jobPK;
			JobParentTableCode = jobParentTableCode;
			PlaceOfSupply = placeOfSupply;
		}

		public APInvoiceCharges(APInvoiceChargesApprovalRequest canceledRequest)
		{
			Argument.NotNull(canceledRequest, nameof(canceledRequest));

			this.canceledRequest = canceledRequest;
			Creditor = canceledRequest.PostingDetails.Creditor;
			InvoiceNumber = canceledRequest.PostingDetails.TransactionNumber;
			RequisitionStatus = canceledRequest.RequisitionStatus;
			RequisitionDate = canceledRequest.RequisitionDate;
		}

		[ResourceStringData("Creditor", Caption = "Creditor")]
		public ZString Creditor { get; }

		[ResourceStringData("InvoiceNumber", Caption = "Invoice Number", ShortCaption = "Inv. Num.", MediumCaption = "Invoice Num.")]
		public ZString InvoiceNumber { get; }

		public ZString PlaceOfSupply { get; }

		#region RequisitionStatus

		[List(nameof(PaymentCriticalityList))]
		[ResourceStringData("RequisitionStatus", Caption = "Requisition Status", ShortCaption = "Req. Stat.", MediumCaption = "Req. Status")]
		[MaxLength(nameof(RequisitionStatusMaxLength))]
		public ZString RequisitionStatus
		{
			get
			{
				return IsTransactionIncludedInApprovingRequest ? ApprovingRequest.RequisitionStatus : requisitionStatus;
			}
			set
			{
				SetNonPersistentPropertyValue(RequisitionStatusInfo, ref requisitionStatus, value);
				if (ApprovingRequest != null)
				{
					ApprovingRequest.RequisitionStatus = requisitionStatus;
				}
				ValidateRequisitionStatus();
			}
		}
		ZString requisitionStatus;
		int RequisitionStatusMaxLength => AccTransactionHeaderSchema.AH_RequisitionStatus.MaxLength;

		public ZPropertyInfo RequisitionStatusInfo
		{
			get { return GetZPropertyInfo(Schema.RequisitionStatus); }
		}

		public bool RequisitionStatus_ReadOnly
		{
			get { return !IsTransactionIncludedInApprovingRequest || !AccountingConfigurationRegistry.Instance.AllowPaymentRequisitionStatusOverride.Value; }
		}

		public ICodeDescriptionPairListWithDefaultCodeAndExtraBool PaymentCriticalityList
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.PaymentRequisitionStatuses.Value;
			}
		}

		[ResourceStringData("RequisitionDate", Caption = "Requisition Date", ShortCaption = "Req. Date", MediumCaption = "Req. Date")]
		public ZDateTime RequisitionDate
		{
			get
			{
				return IsTransactionIncludedInApprovingRequest ? ApprovingRequest.RequisitionDate : requisitionDate;
			}
			set
			{
				SetNonPersistentPropertyValue(RequisitionDateInfo, ref requisitionDate, value);
				if (ApprovingRequest != null)
				{
					ApprovingRequest.RequisitionDate = requisitionDate;
				}
				ValidateRequisitionDate();
			}
		}
		ZDateTime requisitionDate;

		public ZPropertyInfo RequisitionDateInfo
		{
			get { return GetZPropertyInfo(Schema.RequisitionDate); }
		}

		public bool RequisitionDate_ReadOnly
		{
			get { return !IsTransactionIncludedInApprovingRequest || !AccountingConfigurationRegistry.Instance.AllowPaymentRequisitionStatusOverride.Value; }
		}

		#endregion

		[ResourceStringData("ReferenceNumber", Caption = "Reference Number", ShortCaption = "Ref. Num.", MediumCaption = "Ref. Number")]
		public ZString ReferenceNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (canceledRequest != null)
				{
					result = canceledRequest.JobNumber;
				}
				else
				{
					var parentIdAndTableCode = GetParentIdAndTableCodeForJobPostingAction();
					var parentId = parentIdAndTableCode.Item1;
					var parentTableCode = parentIdAndTableCode.Item2;
					if (parentTableCode == JobHeaderSchema.Constants.Prefix)
					{
						var job = FactoryForAPInvoiceApprovalRequests.Load<JobHeader>(parentId);
						if (job != null)
						{
							result = job.JH_JobNum;
						}
					}
					else
					{
						var consol = GenericConsol.GenericConsol.GetIJobCostingPlugInByPK(FactoryForAPInvoiceApprovalRequests, parentId, parentTableCode);
						if (consol != null)
						{
							result = consol.JK_UniqueConsignRef;
						}
					}
				}

				return result;
			}
		}

		#region IsExcludedFromPosting

		[ReadOnlyMember(nameof(IsExcludedFromPosting_ReadOnly))]
		[ResourceStringData("IsExcludedFromPosting", Caption = "Exclude From Posting", ShortCaption = "Exclude")]
		public ZBool IsExcludedFromPosting
		{
			get { return !ContinueProcessing || isExcludedFromPosting; }
			set
			{
				UnregisterApprovingRequestAsEditableChildObject();

				SetNonPersistentPropertyValue(IsExcludedFromPostingInfo, ref isExcludedFromPosting, value);

				RegisterApprovingRequestAsEditableChildObjectWhenNotExcluded();
			}
		}
		ZBool isExcludedFromPosting;

		public ZPropertyInfo IsExcludedFromPostingInfo
		{
			get { return GetZPropertyInfo(Schema.IsExcludedFromPosting); }
		}

		bool IsExcludedFromPosting_ReadOnly
		{
			get { return !ContinueProcessing || canceledRequest != null; }
		}

		#endregion

		#region AH_LocalTotalAmount

		[ResourceStringData("APInvoiceCharges|AH_LocalTotalAmount", Caption = "Authorization Amount", ShortCaption = "Amount")]
		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal AH_LocalTotalAmount
		{
			get { return canceledRequest != null ? canceledRequest.PostingDetails.MaxAmountToApprove : (ZDecimal)Charges.Sum(x => x.JR_Calc_LocalCostAmtWithGST); }
		}

		public int LocalCurrencyDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		public ZDecimal AH_OSTotalAmount
		{
			get { return canceledRequest != null ? -canceledRequest.InvoiceOSTotalAmount : Charges.Sum(x => x.JR_Calc_OSCostAmtWithGST); }
		}

		public ZDecimal AH_LocalExTaxAmount
		{
			get { return canceledRequest != null ? -canceledRequest.InvoiceLocalExTaxAmount : Charges.Sum(x => x.JR_LocalCostAmt); }
		}

		public ZDecimal AH_LocalTaxAmount
		{
			get { return canceledRequest != null ? -canceledRequest.InvoiceLocalTaxAmount : Charges.Sum(x => x.JR_Cost_LocalGSTAmount); }
		}

		#endregion

		#region ApprovingRequestDescription

		[ReadOnlyMember(nameof(ApprovingRequestDescription_ReadOnly))]
		[ResourceStringData("ApprovingRequestDescription", Caption = "New Request Description", ShortCaption = "Request Desc.", MediumCaption = "New Request Desc.")]
		public ZString ApprovingRequestDescription
		{
			get { return IsTransactionIncludedInApprovingRequest ? ApprovingRequest.XP_ReasonDescription : ZString.Empty; }
			set
			{
				if (ApprovingRequest != null)
				{
					ApprovingRequest.XP_ReasonDescription = value;
				}
			}
		}

		public ZPropertyInfo ApprovingRequestDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ApprovingRequestDescription, x => IsTransactionIncludedInApprovingRequest ? ApprovingRequest.XP_ReasonDescriptionInfo : null); }
		}

		bool ApprovingRequestDescription_ReadOnly
		{
			get { return !IsTransactionIncludedInApprovingRequest; }
		}

		bool IsTransactionIncludedInApprovingRequest
		{
			get { return !IsExcludedFromPosting && ApprovingRequest != null; }
		}

		#endregion

		[ResourceStringData("APInvoiceCharges|Summary", Caption = "Action Summary", ShortCaption = "Summary")]
		public ZString Summary
		{
			get
			{
				ZStringBuilder summary = new ZStringBuilder();

				if (IsInvoiceAllowedToBeCreated)
				{
					summary.Append(Res.GetString("4d25df38-bcaa-4d6f-945e-ffae7d5e6836", "Invoice is created."));
				}
				if (IsActionDone_PostApprovedRequestForThisPostingDetails)
				{
					summary.Append(Res.GetString("ece8e826-5cce-42aa-a006-fac4a6e61928", "Approved request is posted."));
				}
				if (IsActionDone_CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus)
				{
					summary.Append(Res.GetString("6a34b5a4-b83b-43e1-9545-051cb3f5d9d1", "Previous request is canceled."));
				}
				if (IsActionDone_CancelApprovedRequestForThisPostingActionButAnotherPostingDetails)
				{
					summary.Append(Res.GetString("8067eb29-31b2-49ce-8ac3-69166812cf04", "Previous approved request with different data is canceled."));
				}
				if (ApprovingRequest != null)
				{
					summary.Append(Res.GetString("91306a84-2da8-42e5-b3cc-5c93cb67f0ab", "New request is created."));
				}
				if (canceledRequest != null)
				{
					summary.Append(Res.GetString("dfba8261-d419-4b3b-b7ac-db47743ee601", "Previous request is canceled as invoice is not in this posting."));
				}

				return summary.ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		public List<Charge> Charges
		{
			get { return charges ?? (charges = new List<Charge>()); }
		}
		List<Charge> charges;

		internal ZDateTime PostDate;

		ZGuid ITransactionBranchCalculationDataProviderFromJobCharge.JobBranchPK => Charges[0].InvoicingJob.JH_GB;

		HashSet<ZGuid> ITransactionBranchCalculationDataProviderFromJobCharge.LineBranchPKs => Charges.Select(x => x.JR_GB).ToHashSet();

		bool ITransactionBranchCalculationDataProviderFromJobCharge.AnyCharges => Charges.Any();

		#region Flags and objects for authorization purposes and to store authorization results

		public bool ContinueProcessing
		{
			get { return continueProcessing; }
			set { continueProcessing = value; }
		}
		bool continueProcessing = true;

		internal bool IsInvoiceAllowedToBeCreated
		{
			get { return canceledRequest == null && isInvoiceAllowedToBeCreated; }
			set { isInvoiceAllowedToBeCreated = value; }
		}
		bool isInvoiceAllowedToBeCreated = true;

		internal bool IsUserConfirmationRequired
		{
			get { return canceledRequest != null || ApprovingRequest != null || isUserConfirmationRequired; }
			set { isUserConfirmationRequired = value; }
		}
		bool isUserConfirmationRequired;

		internal ZGuid ApprovingUserPK { get; set; }

		internal bool IsActionDone_PostApprovedRequestForThisPostingDetails { get; set; }
		internal bool IsActionDone_CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus { get; set; }
		internal bool IsActionDone_CancelApprovedRequestForThisPostingActionButAnotherPostingDetails { get; set; }

		readonly APInvoiceChargesApprovalRequest canceledRequest;
		readonly IPostingJobTransactionsApprovalGUIProvider parentPostingGUIProvider;

		public IPostingJobTransactionsApprovalGUIProvider PostingGUIProvider
		{
			get { return postingGUIProvider ?? (postingGUIProvider = new ApprovalGUIProvider(this, parentPostingGUIProvider)); }
		}
		IPostingJobTransactionsApprovalGUIProvider postingGUIProvider;

#if DEBUG
		public void SetMockGUIProviderForTest(IPostingJobTransactionsApprovalGUIProvider guiProvider)
		{
			postingGUIProvider = guiProvider;
		}

		public ZGuid ApprovingUserPKForTest { get { return ApprovingUserPK; } }
#endif

		internal BusinessObjectFactory FactoryForAPInvoiceApprovalRequests
		{
			get { return canceledRequest != null ? canceledRequest.Factory : PostingGUIProvider.FactoryForApprovalRequests; }
		}

		internal ISecurityOverrideProviderWithApprovalRequest SecurityProviderOverride { get; set; }

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("d5608150-bc11-4ddf-b66c-e8cad7e2558a", "Accounts Payable Invoice"); }
		}

		#endregion

		#region ApprovingRequest

		public APInvoiceChargesApprovalRequest ApprovingRequest
		{
			get { return approvingRequest; }
			private set
			{
				UnregisterApprovingRequestAsEditableChildObject();

				approvingRequest = value;

				RegisterApprovingRequestAsEditableChildObjectWhenNotExcluded();
				SetDefaultRequisitionStatusAndDate();
			}
		}
		APInvoiceChargesApprovalRequest approvingRequest;

		void SetDefaultRequisitionStatusAndDate()
		{
			if (RequisitionStatus.IsEmpty)
			{
				RequisitionStatus = PaymentCriticalityList.DefaultCode;
			}
			if (RequisitionDate.IsEmpty)
			{
				RequisitionDate = Charges.Count > 0 ? Charges[0].JR_PaymentDate : ZDateTime.Empty;
			}
		}

		void UnregisterApprovingRequestAsEditableChildObject()
		{
			if (approvingRequest != null)
			{
				UnRegisterEditableChildObject(approvingRequest);
			}
		}

		void RegisterApprovingRequestAsEditableChildObjectWhenNotExcluded()
		{
			if (approvingRequest != null && !IsExcludedFromPosting)
			{
				RegisterEditableChildObject(approvingRequest);
			}
			ApprovingRequestDescriptionInfo.RefreshBinding();
		}

		#endregion

		internal APInvoiceChargesApprovalRequest PostedRequest { get; set; }

		#region GetParentIdAndTableCodeForJobPostingAction

		ZGuid JobPK { get; set; }
		ZString JobParentTableCode { get; set; }

#if DEBUG
		public void SetJobForTest(Job job)
		{
			JobPK = job.PK;
			JobParentTableCode = job.TablePrefix;
		}
#endif
		Tuple<ZGuid, ZString> GetParentIdAndTableCodeForJobPostingAction()
		{
			return JobPK.IsEmpty && parentPostingGUIProvider != null ? parentPostingGUIProvider.GetParentIdAndTableCodeForJobPostingAction() : Tuple.Create(JobPK, JobParentTableCode);
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateRequisitionStatus();
			ValidateRequisitionDate();
		}

		void ValidateRequisitionStatus()
		{
			RequisitionStatusInfo.ClearAllNotifications();
			if (IsTransactionIncludedInApprovingRequest && AccountingConfigurationRegistry.Instance.AllowPaymentRequisitionStatusOverride.Value)
			{
				MandatoryValidation.CheckEntered(RequisitionStatusInfo);
				if (!RequisitionStatus.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(RequisitionStatusInfo, PaymentCriticalityList, ResString.GetMultilingualString("1c3a4889-ede5-40b6-bb11-8030cf740078", "Please enter a valid requisition status."));
				}
			}
		}

		void ValidateRequisitionDate()
		{
			RequisitionDateInfo.ClearAllNotifications();
			if (IsTransactionIncludedInApprovingRequest && AccountingConfigurationRegistry.Instance.AllowPaymentRequisitionStatusOverride.Value)
			{
				MandatoryValidation.CheckEntered(RequisitionDateInfo);
				if (!RequisitionDate.IsEmpty)
				{
					TypeValidation.CheckValidSmallDateTime(RequisitionDateInfo);
				}
			}
		}

		#endregion

		class ApprovalGUIProvider : IPostingJobTransactionsApprovalGUIProvider
		{
			public ApprovalGUIProvider(APInvoiceCharges invoiceCharges, IPostingJobTransactionsApprovalGUIProvider parentPostingGUIProvider)
			{
				this.invoiceCharges = invoiceCharges;
				this.parentPostingGUIProvider = parentPostingGUIProvider;
			}

			readonly APInvoiceCharges invoiceCharges;
			readonly IPostingJobTransactionsApprovalGUIProvider parentPostingGUIProvider;

			public BusinessObjectFactory FactoryForApprovalRequests
			{
				get { return factoryForApprovalRequests ?? (factoryForApprovalRequests = new BusinessObjectFactory()); }
			}
			BusinessObjectFactory factoryForApprovalRequests;

			void ResetFactoryForApprovalRequests()
			{
				factoryForApprovalRequests = null;
			}

			public ISecurityOverrideProviderWithApprovalRequest GetNewSecurityOverrideProviderForARCreditNote(bool showApprovalRequestButton, bool supportMultipleApprover, ARCreditNoteApprovalRequest[] approvalRequests)
			{
				throw new NotImplementedException();
			}
			#region IPostingTransactionApprovalGUIProvider

#if DEBUG
			bool IPostingTransactionApprovalGUIProvider.ShowLoginFormForTest
			{
				get { return parentPostingGUIProvider.ShowLoginFormForTest; }
			}

			string IPostingTransactionApprovalGUIProvider.SecurityItemForTest
			{
				get { return parentPostingGUIProvider.SecurityItemForTest; }
			}
#endif

			bool IPostingTransactionApprovalGUIProvider.IsBulkPosting
			{
				get { return parentPostingGUIProvider.IsBulkPosting; }
			}

			ZDialogResult IPostingTransactionApprovalGUIProvider.ShowMessage(string messageText, string messageCaption, ZMessageBoxButtons messageBoxButtons, ZMessageBoxIcon messageBoxIcon, ZDialogResult dialogResult)
			{
				return parentPostingGUIProvider.ShowMessage(messageText, messageCaption, messageBoxButtons, messageBoxIcon, dialogResult);
			}

			ZDialogResult IPostingTransactionApprovalGUIProvider.ShowApprovalFormToSetDescription(GenApprovalRequest approvingRequest)
			{
				invoiceCharges.ApprovingRequest = (APInvoiceChargesApprovalRequest)approvingRequest;
				return ZDialogResult.OK;
			}

			void IPostingTransactionApprovalGUIProvider.NotifyBulkPostingIsNotAuthorized(string message)
			{
				var messageHeader = Res.GetString("54461c9a-6731-4b43-9b64-d84e14db9166", "Creditor: {0}, Number: {1}, Ref. Number:{2}, Amount:{3}.", invoiceCharges.Creditor, invoiceCharges.InvoiceNumber, invoiceCharges.ReferenceNumber, invoiceCharges.AH_LocalTotalAmount);
				var messageFooter = "-----------";
				parentPostingGUIProvider.NotifyBulkPostingIsNotAuthorized(new ZStringBuilder(new[] { messageHeader, message, messageFooter }).ToStringWithNewLineBetweenAppends());
			}

			BusinessObjectFactory IPostingTransactionApprovalGUIProvider.FactoryForApprovalRequests
			{
				get { return FactoryForApprovalRequests; }
			}

			void IPostingTransactionApprovalGUIProvider.ResetFactoryForApprovalRequests()
			{
				ResetFactoryForApprovalRequests();
			}

			Tuple<ZGuid, ZString> IPostingTransactionApprovalGUIProvider.GetParentIdAndTableCodeForJobPostingAction()
			{
				return invoiceCharges.GetParentIdAndTableCodeForJobPostingAction();
			}

			void IPostingTransactionApprovalGUIProvider.RollbackPosting()
			{
				invoiceCharges.IsInvoiceAllowedToBeCreated = false;
			}

			ISecurityOverrideProviderWithApprovalRequest IPostingTransactionApprovalGUIProvider.GetNewSecurityOverrideProvider(bool showApprovalRequestButton, bool supportMultipleApprover)
			{
				return invoiceCharges.SecurityProviderOverride ?? parentPostingGUIProvider.GetNewSecurityOverrideProvider(showApprovalRequestButton, supportMultipleApprover);
			}

			JobInvoicingPostingOption IPostingTransactionApprovalGUIProvider.PostingOption
			{
				get { return parentPostingGUIProvider.PostingOption; }
			}

			bool IPostingJobTransactionsApprovalGUIProvider.IsForPreviewOnly
			{
				get { return parentPostingGUIProvider.IsForPreviewOnly; }
			}

			APInvoiceChargesApprovalRequest IPostingJobTransactionsApprovalGUIProvider.RequestToCompare
			{
				get { return parentPostingGUIProvider.RequestToCompare; }
			}

			ZDialogResult IPostingJobTransactionsApprovalGUIProvider.ShowPostingConfirmationForm(APInvoiceCharges[] apInvoiceCharges)
			{
				return ZDialogResult.None;
			}

			public ISecurityOverrideProviderWithApprovalRequest GetNewSecurityOverrideProviderForARCreditNote(bool showApprovalRequestButton, bool supportMultipleApprover, InvoicingBaseApprovalRequest<ARCreditNoteApprovalRequestDetails>[] approvalRequests)
			{
				throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Not Support for APInvoiceCharges {0}, {1}, {2}", showApprovalRequestButton, supportMultipleApprover, approvalRequests.Length));
			}

			public Tuple<ZString, ZString> ShowCreditNoteReversalReasonForm(string existingReasonCode)
			{
				throw new NotImplementedException();
			}

			#endregion
		}
	}
}
