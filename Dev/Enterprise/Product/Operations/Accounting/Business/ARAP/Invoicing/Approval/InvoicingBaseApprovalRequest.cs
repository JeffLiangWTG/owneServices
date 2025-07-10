using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract class InvoicingBaseApprovalRequest<DetailsType> : TransactionApprovalRequest<DetailsType>
		where DetailsType : ApprovalRequestDetails
	{
		public InvoicingBaseApprovalRequest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		public override ZGuid XP_ParentID
		{
			get { return base.XP_ParentID; }
			set
			{
				base.XP_ParentID = value;
				ResetJobNumber();
			}
		}

		public override ZString XP_ParentTableCode
		{
			get { return base.XP_ParentTableCode; }
			set
			{
				base.XP_ParentTableCode = value;
				ResetJobNumber();
			}
		}

		protected override ZString ReferenceIDCore
		{
			get { return JobNumber; }
		}

		#region JobNumber

		[ResourceStringData("JobNumber", Caption = "Job Number", ShortCaption = "Job Num.")]
		public virtual ZString JobNumber
		{
			get
			{
				if (!JobNumber_cached.HasValue)
				{
					switch (XP_ParentTableCode)
					{
						case JobHeaderSchema.Constants.Prefix:
							var job = Factory.Load<JobHeader>(XP_ParentID);
							if (job != null)
							{
								JobNumber_cached = job.JH_JobNum;
							}
							break;
						case AccTransactionHeaderSchema.Constants.Prefix:
							var invoice = Factory.Load<TransactionHeader>(XP_ParentID);
							if (invoice != null)
							{
								JobNumber_cached = invoice.AH_TransactionNum;
							}
							break;
						case GenApprovalRequestSchema.Constants.Prefix:
							var parentRequest = Factory.Load<ARCreditNoteApprovalRequest>(XP_ParentID);
							if (parentRequest != null)
							{
								JobNumber_cached = parentRequest.JobNumber;
							}
							break;
						default:
							var consol = GenericConsol.GenericConsol.GetIJobCostingPlugInByPK(Factory, XP_ParentID, XP_ParentTableCode);
							if (consol != null)
							{
								JobNumber_cached = consol.JK_UniqueConsignRef;
							}
							break;
					}
				}
				return JobNumber_cached ?? ZString.Empty;
			}
		}
		protected ZString? JobNumber_cached;

		void ResetJobNumber()
		{
			JobNumber_cached = null;
		}

		#endregion

		[ResourceStringData("PostingOptionForDisplay", Caption = "Posting Option", ShortCaption = "Post Opt.")]
		public ZString PostingOptionForDisplay
		{
			get
			{
				JobInvoicingPostingOption option;
				if (Enum.TryParse(PostingOptionCore, out option))
				{
					return AccountingUtils.ConvertPostingOptionToHumanReadableName(option, IsConsolRelated);
				}
				else
				{
					return PostingOptionCore;
				}
			}
		}

		protected abstract ZString PostingOptionCore
		{
			get;
		}

		public bool IsTransactionRelated
		{
			get { return XP_ParentTableCode == AccTransactionHeaderSchema.Constants.Prefix; }
		}

		public bool IsJobRelated
		{
			get { return XP_ParentTableCode == JobHeaderSchema.Constants.Prefix; }
		}

		public bool IsApprovalRequestRelated => XP_ParentTableCode == GenApprovalRequestSchema.Constants.Prefix;

		public bool IsConsolRelated => !IsJobRelated && !IsTransactionRelated && !IsApprovalRequestRelated;

		#endregion

		public override void OnSaving()
		{
			base.OnSaving();
			AddAdditionalLogsOnLinkedTransaction();
		}

		void AddAdditionalLogsOnLinkedTransaction()
		{
			if (ShouldAddAdditionalLogs && IsInDatabase && XP_ApprovalStatusInfo.HasChanges)
			{
				if (LinkedTransactionToAddLog != null)
				{
					if (!RequestApprovedReference.IsEmpty && XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Approved)
					{
						AddAdditionalLog(LinkedTransactionToAddLog, RequestApprovedReference);
					}
					else if (ShouldAddLogsWhenApprovalRequestCancelled && base.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Cancelled)
					{
						var allCancelApprovalRequestContexts = new List<BusinessContext> { BusinessContext.CancelApprovalRequestByUser, BusinessContext.CancelApprovalRequestDueToUpdatingLinkedTransaction, BusinessContext.CancelApprovalRequestAsTransactionAlreadyCancelledOrPosted };
						var contexts = this.GetContexts<BusinessContext>().Where(x => x.In(allCancelApprovalRequestContexts)).ToHashSet();
						if (contexts.Count > 1)
						{
							ErrorReporter.ReportOnce("ApprovalRequestCancelledHasMoreThanOneContext", "Approval request cannot be cancelled with more than one context responsible for cancelling. Please investigate why we have added theses contexts at the same time.");
						}
						else if (contexts.Count == 0)
						{
							var referenceTypeAndValue = CreateReferenceTypeAndValueForLog();
							var collectedRequestInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfoSafe(PK, CriticalValidationInfoCollectorServiceKeyType.ApprovalRequestCancelledWithoutAnyBusinessContext);
							var errorMessage = string.Format(CultureInfo.InvariantCulture, (NoResString)@"We set at least one Business Context when cancelling Approval request. For some reason no context was set while cancelling this approval request.
Please check if we missed to add one of the Business context when its valid to cancel an approval request.
Or if in the unit test we are setting the Approval Status to CAN directly then we might need to set one of the contexts to the Approval Request.
Request type and value:
{0}
Request details:
{1}", referenceTypeAndValue, collectedRequestInfo);

							ErrorReporter.ReportOnce("ApprovalRequestCancelledWithoutAnyBusinessContext", errorMessage);
						}

						if (!RequestCancelledForEditReference.IsEmpty && this.HasContext(BusinessContext.CancelApprovalRequestDueToUpdatingLinkedTransaction))
						{
							AddAdditionalLog(LinkedTransactionToAddLog, RequestCancelledForEditReference);
						}
						else if (!RequestCancelledByUserReference.IsEmpty && this.HasContext(BusinessContext.CancelApprovalRequestByUser))
						{
							AddAdditionalLog(LinkedTransactionToAddLog, RequestCancelledByUserReference);
						}
						else if (!RequestCancelledAsTransactionAlreadyCancelledOrPostedReference.IsEmpty && this.HasContext(BusinessContext.CancelApprovalRequestAsTransactionAlreadyCancelledOrPosted))
						{
							AddAdditionalLog(LinkedTransactionToAddLog, RequestCancelledAsTransactionAlreadyCancelledOrPostedReference);
						}
					}
					else if (!RequestRejectedReference.IsEmpty && XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Rejected)
					{
						AddAdditionalLog(LinkedTransactionToAddLog, RequestRejectedReference);
					}
				}
			}
		}

		protected void CancelAndIncrementTransactionCount(InvoicingBase invoice)
		{
			if (invoice != null && !invoice.AH_TransactionNum.IsEmpty)
			{
				invoice.IsCancelled = true;
				var maxTransactionCount = GetMaximumTransactionCountFromDb(invoice);
				if (maxTransactionCount == byte.MaxValue)
				{
					ErrorReporter.ReportOnce("APInvoiceApprovalRequestCancelling",
						string.Format(CultureInfo.InvariantCulture, @"Invoice AH_TransactionCount has reached a maximum value. As such invoice parameters won't be unique, no more invoices can be created with: 
Transaction Ledger: '{0}', Type: '{1}', Number: '{2}', Organisation: '{3}', Company: '{4}'.

Approach with incrementing AH_TransactionCount on approval requests cancelling may require reconsideration.",
						invoice.AH_Ledger, invoice.AH_TransactionType, invoice.AH_TransactionNum, invoice.Header != null ? (string)invoice.Header.OH_Code : invoice.AH_OH.ToString(), invoice.Company.GC_Code));
				}
				invoice.AH_TransactionCount = (ZByte)(maxTransactionCount < byte.MaxValue ? maxTransactionCount + 1 : TransactionCountDefaultValue);
			}
		}

		ZByte GetMaximumTransactionCountFromDb(InvoicingBase invoice)
		{
			var parameters = new ZSqlParameterCollection();
			var selectClause = "MAX(AH_TransactionCount) AS MaxTransactionCount";
			var sql = AccountingUtils.GetQueryForUniqueTransactionNumber(invoice.AH_Ledger, invoice.AH_TransactionType, invoice.AH_TransactionNum, invoice.AH_OH, ZGuid.Empty,
				new AccountingUtils.UniqueTransactionNumberQueryGeneratorParameters(selectClause, parameters));

			var sqlResults = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			sqlResults.Load(sql, parameters);

			return (ZByte)sqlResults[0]["MaxTransactionCount"];
		}

		protected static byte TransactionCountDefaultValue
		{
			get { return (byte)AccTransactionHeaderSchema.AH_TransactionCount.SqlDbDefault; }
		}

		protected virtual ZString RequestApprovedReference => ZString.Empty;
		protected virtual ZString RequestCancelledByUserReference => ZString.Empty;
		protected virtual ZString RequestCancelledForEditReference => ZString.Empty;
		protected virtual ZString RequestCancelledAsTransactionAlreadyCancelledOrPostedReference => ZString.Empty;
		protected virtual ZString RequestRejectedReference => ZString.Empty;

		bool ShouldAddLogsWhenApprovalRequestCancelled => !RequestCancelledForEditReference.IsEmpty || !RequestCancelledByUserReference.IsEmpty || !RequestCancelledAsTransactionAlreadyCancelledOrPostedReference.IsEmpty;
		bool ShouldAddAdditionalLogs => !RequestApprovedReference.IsEmpty || ShouldAddLogsWhenApprovalRequestCancelled || !RequestRejectedReference.IsEmpty;
#if DEBUG
		public bool ShouldAddAdditionalLogs_ForTestOnly => ShouldAddAdditionalLogs;
#endif
		StmALog AddAdditionalLog(InvoicingBase linkedTransactionToAddLog, ZString reference)
		{
			return linkedTransactionToAddLog.Logs.AddNew(Events.TransactionApprovalActioned, string.Format(CultureInfo.InvariantCulture, "{0}|{1}|{2}"
											, linkedTransactionToAddLog.AH_Ledger, linkedTransactionToAddLog.AH_TransactionType, reference));
		}

		string CreateReferenceTypeAndValueForLog()
		{
			if (!RequestCancelledForEditReference.IsEmpty)
			{
				return "RequestCancelledForEditReference: " + RequestCancelledForEditReference;
			}
			if (!RequestCancelledByUserReference.IsEmpty)
			{
				return "RequestCancelledByUserReference: " + RequestCancelledByUserReference;
			}
			if (!RequestCancelledAsTransactionAlreadyCancelledOrPostedReference.IsEmpty)
			{
				return "RequestCancelledAsTransactionAlreadyCancelledOrPostedReference: " + RequestCancelledAsTransactionAlreadyCancelledOrPostedReference;
			}
			return string.Empty;
		}

		public InvoicingBase LinkedTransactionToAddLog
		{
			get
			{
				if (fLinkedTransactionToAddLog == null)
				{
					var factory = Factory.ServiceContainer.GetService<ParentFactoryService>()?.ParentFactory ?? Factory;
					fLinkedTransactionToAddLog = factory.Load<InvoicingBase>(XP_ParentID);
				}
				return fLinkedTransactionToAddLog;
			}
		}
		protected InvoicingBase fLinkedTransactionToAddLog;

		public string GetApprovalRequestInfo()
		{
			var result = new ZStringBuilder();
			result.AppendLine((NoResString)"Linked transaction Ledger: " + LinkedTransactionToAddLog.AH_Ledger);
			result.AppendLine((NoResString)"Linked transaction Type: " + LinkedTransactionToAddLog.AH_TransactionType);
			result.AppendLine((NoResString)"XP_ParentTableCode: " + XP_ParentTableCode);
			result.AppendLine((NoResString)"JobNumber/TransactionNum: " + ReferenceIDCore);
			result.AppendLine(System.Environment.StackTrace);
			return result.ToString();
		}
	}

	class ParentFactoryService : IService
	{
		internal ParentFactoryService(BusinessObjectFactory factory)
		{
			ParentFactory = factory;
		}

		internal BusinessObjectFactory ParentFactory { get; }
	}
}
