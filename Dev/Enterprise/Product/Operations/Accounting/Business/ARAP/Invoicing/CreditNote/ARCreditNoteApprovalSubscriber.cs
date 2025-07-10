using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[Serializable]
	public class ARCreditNoteApprovalSubscriber : LogSubscriberWithOptionalFactorySave
	{
		public override string Name => "ARCreditNoteApprovalSubscriber";

		public override string[] EventTypes
		{
			get { return new string[] { Events.EditedARecord.Code }; }
		}

		public override string[] TableNames
		{
			get { return new string[] { GenApprovalRequestSchema.Constants.TableName }; }
		}

		ZStringBuilder ErrorBuffer => errorBuffer ?? (errorBuffer = new ZStringBuilder());

		[NonSerialized]
		ZStringBuilder errorBuffer = null;

		class ARCreditNoteApprovalLogBatcher : LogBatcher<ZGuid>
		{
			public ARCreditNoteApprovalLogBatcher(ARCreditNoteApprovalSubscriber parent)
				: base()
			{
				this.parent = parent;
			}

			readonly ARCreditNoteApprovalSubscriber parent;
			protected override void AddGroupingFetchHints(IEnumerable<IQueuedLog> enumberable) { }
			protected override ZGuid GetGroupLogKey(IQueuedLog log) => log.SJ_ParentID;
			protected override LogsGroupContext SetContextForLogsGroup(ZGuid groupKey, IEnumerable<IQueuedLog> queuedLogs)
			{
				var firstLog = queuedLogs.First();
				var request = firstLog.Factory.Load<ARCreditNoteApprovalRequest>(groupKey);
				if (IsEligibileForHandling(request))
				{
					var user = request.CreatedUser;
					var branch = request.RequestingBranch;
					var department = request.JobDepartment;
					return new LogsGroupContext(false, Env.SetTemporaryUserContext(user.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()));
				}
				else
				{
#if DEBUG
					parent.DefaultLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "GenApprovalRequest {0} EDT log received, but cannot process due to constraints on ARCreditNoteApprovalRequest.", firstLog.SJ_ParentID));
#endif
					return new LogsGroupContext(true);
				}
			}
		}

		protected override ILogBatcher GetLogBatcher() => new ARCreditNoteApprovalLogBatcher(this);

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			var firstLog = queuedLogs.First();

			if (!AccountingConfigurationRegistry.Instance.PostCreditNoteOnApproval.Value)
			{
				DefaultLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "PostCreditNoteOnApproval registry turned OFF, cannot process ARCreditNoteApprovalRequest {0} EDT logs.", firstLog.SJ_ParentID));
				return;
			}

			foreach (var log in queuedLogs)
			{
				var request = log.Factory.Load<ARCreditNoteApprovalRequest>(log.SJ_ParentID);
				var job = request.Parent as Job;
				InvoicingBase originalTransaction = null;
				if (job == null)
				{
					originalTransaction = (InvoicingBase)request.Parent;
					job = originalTransaction.InvoicingJob;
				}

				IJobInvoicingPlugIn plugin = null;
				if (job != null)
				{
					job.InitializeParentFromGenericJobWithSettingDefaults();
					plugin = job.PlugInData;
				}

				if (plugin == null)
				{
					var logMissingParentFormErrorAndContinue = false;
					if (job != null)
					{
						logMissingParentFormErrorAndContinue = true;
					}
					else
					{
						if ((request.IsEligibleToAutoPostARCreditNote || request.IsEligibleToAutoPostAmendingARCreditNote)
							&& !(originalTransaction.IsConsolInvoice || originalTransaction.IsPeriodicInvoice))  //For consol invoice or periodic invoice, we should let auto posting happen even without the plugin
						{
							logMissingParentFormErrorAndContinue = true;
						}
					}

					if (logMissingParentFormErrorAndContinue)
					{
						DefaultLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "ARCreditNoteApprovalRequest {0} EDT log received, but cannot process due to missing parent form.", request.PK));
						continue;
					}
				}

				string errorMessageToThrowExceptionDisallowingFactorySaving = null;
				bool canSaveFactory = false;

				try
				{
					if (request.PostingDetails.PostDate.Date < ZDateTime.Today
						&& AccountingConfigurationRegistry.Instance.PostCreditNoteOnApproval.Value
						&& AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.Value != AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code
						&& (!AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value || !Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed))
					{
						request.PostingDetails.UpdatePostDateToToday();
					}

					if (AccountingConfigurationRegistry.Instance.UseCurrentDateAsTransactionDateWhenAutoPosting.Value)
					{
						request.Factory.SetContext(BusinessContext.UseCurrentDateAsTransactionDateWhenAutoPosting);
						request.Factory.Saved += Factory_Saved;
					}

					if (request.IsEligibleToAutoPostARCreditNote)
					{
						canSaveFactory = HandlePostingForJob(request, plugin, job);
					}
					else if (request.IsEligibleToAutoPostAmendingARCreditNote)
					{
						canSaveFactory = HandlePostingForAmendingTransaction(request, plugin, originalTransaction, out errorMessageToThrowExceptionDisallowingFactorySaving);
					}
					else if (request.IsEligibleToAutoPostARCreditNoteReversal)
					{
						canSaveFactory = ARCreditNoteApprovalRequestInvoiceReversingHelper.HandleReversing(request, CollectErrorMessages);
					}
				}
				catch (LogSubscriberToAbortLogGroupProcessingSilentlyException)
				{
					// We catch this exception to avoid the error email being sent twice because an exception of same type is thrown later with all the error messages in the ErrorBuffer.
					canSaveFactory = false;
				}

				if (!canSaveFactory)
				{
					var aRCreditNoteApprovalRequestPostingErrorEmail = new ARCreditNoteApprovalRequestPostingErrorEmail(request, ErrorBuffer.ToStringWithNewLineBetweenAppends());
					var email = aRCreditNoteApprovalRequestPostingErrorEmail.CreateEmailDef();
					throw new LogSubscriberToAbortLogGroupProcessingSilentlyException(errorMessageToThrowExceptionDisallowingFactorySaving, email);
				}
			}
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			factory.RemoveContext(BusinessContext.UseCurrentDateAsTransactionDateWhenAutoPosting);
			factory.Saved -= Factory_Saved;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "warningMessage")]
		bool HandlePostingForAmendingTransaction(ARCreditNoteApprovalRequest request, IJobInvoicingPlugIn plugin, TransactionHeader originalTransaction, out string errorMessageToThrowExceptionDisallowingFactorySaving)
		{
			InvoicingBase transactionHeader;
			errorMessageToThrowExceptionDisallowingFactorySaving = null;
			bool shouldPostTransaction = false;

			var approvalHelper = new ARCreditNoteApprovalHelper(request);
			if (approvalHelper.ApprovedRequestsForThisParentID.Any())
			{
				CollectErrorMessages(LogType.Error, string.Format(CultureInfo.CurrentCulture, (NoResString)"Could not process Approved ARCreditNoteApprovalRequest {0} for credit note amendment because there are other approved requests exist in database for the same invoice {1}.", request.PK, originalTransaction.AH_TransactionNum));
				return shouldPostTransaction;
			}

			var securityCheckPoint = plugin == null ? Env.Security.None : plugin.InvoicingSupporter.JobInvoicingSecurity;
			var securityHelper = new JobInvoicingSecurityHelper(securityCheckPoint);
			transactionHeader = CreditNoteAmendingHelper.AmendARTransaction(TransactionTypes.CreditNote, originalTransaction, securityHelper,
							(message, caption) => CollectErrorMessages(LogType.Error, string.Format(CultureInfo.InvariantCulture, (NoResString)"ARCreditNoteApprovalRequest {0} EDT log received, but cannot process due to {1}", request.PK, securityHelper.GetErrorTextForSecurityCheckPoint(SecurityCore.AmendTransactionWCreditNote))),
							(message, caption) => CollectErrorMessages(LogType.Error, string.Format(CultureInfo.InvariantCulture, (NoResString)"ARCreditNoteApprovalRequest {0} EDT log received, but cannot process due to {1}", request.PK, message))) as InvoicingBase;

			if (transactionHeader != null)
			{
				transactionHeader.SetContext(BusinessContext.SystemCreatedAmending);
				request.InitializeTransactionLinesFromPostingDetails(transactionHeader);

				if (transactionHeader.Lines.Count == 0)
				{
					errorMessageToThrowExceptionDisallowingFactorySaving = string.Format(CultureInfo.CurrentCulture, (NoResString)"Could not process Approved ARCreditNoteApprovalRequest {0} for credit note amendment because amended AR Credit Note has no lines.", request.PK);
					CollectErrorMessages(LogType.Error, errorMessageToThrowExceptionDisallowingFactorySaving);
					return shouldPostTransaction;
				}
			}
			else
			{
				CollectErrorMessages(LogType.Error, string.Format(CultureInfo.CurrentCulture, (NoResString)"Could not process Approved ARCreditNoteApprovalRequest {0} for credit note amendment because amendment credit note could not be generated from transaction.", request.PK));
				return shouldPostTransaction;
			}

			transactionHeader.RunPreSaveValidation();

			if (!transactionHeader.HasErrors)
			{
				transactionHeader.SetContext(BusinessContext.AmendingInvoice);
				var guiProvider = new PostingGUIProviderForARCreditNoteApprovalSubscriber(request, CollectErrorMessages);
				var result = new InvoicingPreSaveHelper(DefaultLogger).PreSaveActions(transactionHeader, null, false, guiProvider, true, false, false);

				if (result.CanProceed)
				{
					shouldPostTransaction = true;
				}
				else
				{
					errorMessageToThrowExceptionDisallowingFactorySaving = string.Format(CultureInfo.CurrentCulture, (NoResString)"Approved ARCreditNoteApprovalRequest {0} could not be processed because {1}.", request.PK, result.ErrorMessage.TrimEnd(new char[] { '.', ' ' }));
					CollectErrorMessages(LogType.Error, errorMessageToThrowExceptionDisallowingFactorySaving);
				}
			}
			else
			{
				errorMessageToThrowExceptionDisallowingFactorySaving = string.Format(CultureInfo.CurrentCulture, (NoResString)"Could not process Approved ARCreditNoteApprovalRequest {0} for credit note amendment because the amending AR Credit Note has validation errors:\r\n{1}", request.PK,
					string.Join("\r\n", new ZNotificationCollector(transactionHeader, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors().GetUniqueMessageList()));
				CollectErrorMessages(LogType.Error, errorMessageToThrowExceptionDisallowingFactorySaving);
			}

			return shouldPostTransaction;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Logger Notification Text;, Logger Notification Text")]
		bool HandlePostingForJob(ARCreditNoteApprovalRequest request, IJobInvoicingPlugIn plugin, Job job)
		{
			bool canSaveFactory = false;
			if (!new[] { JobInvoicingPostingOption.Agent, JobInvoicingPostingOption.Revenue, JobInvoicingPostingOption.All, JobInvoicingPostingOption.AllSisterCompanyCharges, JobInvoicingPostingOption.LocalSisterCompanyChargesOnly }.Contains(request.PostingOption))
			{
				CollectErrorMessages(LogType.Error, string.Format(CultureInfo.InvariantCulture, (NoResString)"Credit Note on '{0}' is not posted after approval because '{1}' is not supported. This credit note must be posted by selecting '{1}' from the job.", job.JH_JobNum, request.PostingOptionForDisplay));
				canSaveFactory = true;
				return canSaveFactory;
			}

			JobPostingWorkflowProcessorSupportingARCreditNoteLevelAuthorization processor = null;
			var guiProvider = new PostingGUIProviderForARCreditNoteApprovalSubscriber(request, CollectErrorMessages);
			switch (request.PostingOption)
			{
				case JobInvoicingPostingOption.Agent: processor = new JobOverseasAgentChargesPoster(plugin, guiProvider, request.PostingDetails.InvoiceDate, request.PostingDetails.PostDate); break;
				case JobInvoicingPostingOption.Revenue:
				case JobInvoicingPostingOption.All: processor = new JobRevenuePoster(plugin, guiProvider, request.PostingDetails.InvoiceDate, request.PostingDetails.PostDate); break;
				case JobInvoicingPostingOption.AllSisterCompanyCharges: processor = new SisterCompanyChargePoster(plugin, guiProvider, request.PostingDetails.InvoiceDate, request.PostingDetails.PostDate); break;
				case JobInvoicingPostingOption.LocalSisterCompanyChargesOnly: processor = new LocalSisterCompanyChargePoster(plugin, guiProvider, request.PostingDetails.InvoiceDate, request.PostingDetails.PostDate); break;
				default: CollectErrorMessages(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Credit Note on '{0}' is not posted after approval because '{1}' is not supported. This credit note must be posted by selecting '{1}' from the job.", job.JH_JobNum, request.PostingOptionForDisplay)); break;
			}

			var notifications = new NotificationBuffer();
			try
			{
				processor.Process(notifications, new CancellationToken());
				canSaveFactory = true;
			}
			finally
			{
				if (notifications.HasErrors)
				{
					CollectErrorMessages(LogType.Error, notifications.AsString);
				}
			}

			return canSaveFactory;
		}

		static bool IsEligibileForHandling(ARCreditNoteApprovalRequest request)
		{
			return request != null && (request.IsEligibleToAutoPostARCreditNote || request.IsEligibleToAutoPostAmendingARCreditNote || request.IsEligibleToAutoPostARCreditNoteReversal);
		}

		void CollectErrorMessages(LogType notificationType, string message)
		{
			if (notificationType == LogType.Error)
			{
				ErrorBuffer.Append(message);
			}
			DefaultLogger.Log(notificationType, message);
		}

#if DEBUG
		internal void ProcessLogQueueItems_ForTestOnly(IQueuedLog[] queuedLogs) => ProcessLogQueueItems(queuedLogs);
		internal ZStringBuilder ErrorBuffer_ForTestOnly => ErrorBuffer;
#endif

	}
}

class PostingGUIProviderForARCreditNoteApprovalSubscriber : IPostingJobTransactionsApprovalGUIProvider
{
	internal PostingGUIProviderForARCreditNoteApprovalSubscriber(ARCreditNoteApprovalRequest request, Action<LogType, string> appendToNotificationLogDelegate)
	{
		this.appendToNotificationLogDelegate = appendToNotificationLogDelegate;
		this.request = request;
	}

	readonly Action<LogType, string> appendToNotificationLogDelegate;
	readonly ARCreditNoteApprovalRequest request;

	bool IPostingJobTransactionsApprovalGUIProvider.IsForPreviewOnly => false;

	APInvoiceChargesApprovalRequest IPostingJobTransactionsApprovalGUIProvider.RequestToCompare => throw new NotImplementedException();

	bool IPostingTransactionApprovalGUIProvider.IsBulkPosting => false;

	BusinessObjectFactory IPostingTransactionApprovalGUIProvider.FactoryForApprovalRequests
	{
		get { return request.Factory; }
	}

	JobInvoicingPostingOption IPostingTransactionApprovalGUIProvider.PostingOption => request.PostingOption;

#if DEBUG
	bool IPostingTransactionApprovalGUIProvider.ShowLoginFormForTest => false;

	string IPostingTransactionApprovalGUIProvider.SecurityItemForTest => null;
#endif

	ZDialogResult IPostingJobTransactionsApprovalGUIProvider.ShowPostingConfirmationForm(APInvoiceCharges[] apInvoiceCharges)
	{
		throw new NotImplementedException();
	}

	ZDialogResult IPostingTransactionApprovalGUIProvider.ShowMessage(string messageText, string messageCaption, ZMessageBoxButtons messageBoxButtons, ZMessageBoxIcon messageBoxIcon, ZDialogResult dialogResult)
	{
		appendToNotificationLogDelegate(LogType.Error, messageText);
		return ZDialogResult.None;
	}

	ZDialogResult IPostingTransactionApprovalGUIProvider.ShowApprovalFormToSetDescription(GenApprovalRequest approvingRequest)
	{
		throw new NotImplementedException();
	}

	void IPostingTransactionApprovalGUIProvider.NotifyBulkPostingIsNotAuthorized(string message)
	{
		throw new NotImplementedException();
	}

	void IPostingTransactionApprovalGUIProvider.ResetFactoryForApprovalRequests()
	{
	}

	void IPostingTransactionApprovalGUIProvider.RollbackPosting()
	{
	}

	ISecurityOverrideProviderWithApprovalRequest IPostingTransactionApprovalGUIProvider.GetNewSecurityOverrideProvider(bool showApprovalRequestButton, bool supportMultipleApprover)
	{
		throw new NotImplementedException();
	}

	ISecurityOverrideProviderWithApprovalRequest IPostingTransactionApprovalGUIProvider.GetNewSecurityOverrideProviderForARCreditNote(bool showApprovalRequestButton, bool supportMultipleApprover, ARCreditNoteApprovalRequest[] approvalRequests)
	{
		return new DefaultAccessSecurityProviderForWorkflowPosting();
	}

	Tuple<ZGuid, ZString> IPostingTransactionApprovalGUIProvider.GetParentIdAndTableCodeForJobPostingAction()
	{
		return Tuple.Create(request.XP_ParentID, request.XP_ParentTableCode);
	}

	Tuple<ZString, ZString> IPostingTransactionApprovalGUIProvider.ShowCreditNoteReversalReasonForm(string existingReasonCode)
	{
		throw new NotImplementedException();
	}
}
