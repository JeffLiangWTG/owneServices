using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Accounting.DataTransfer.Res;

namespace Enterprise.BatchProcessor.Accounting
{
	[Serializable]
	public class UnapprovedTransactionSubscriber : LogSubscriber
	{
		public override string[] EventTypes
		{
			get { return new string[] { Events.AddedARecordToTheSystem.Code }; }
		}

		readonly string name = "UnapprovedTransactionsConverter";

		public override string Name
		{
			get { return name; }
		}

		public override string[] TableNames
		{
			get { return new string[] { AccTransactionHeaderSchema.Constants.TableName }; }
		}

		protected override LogSubscriberResult ProcessBatch(IQueuedLog[] queuedLogs)
		{
			var queuedLog = queuedLogs.FirstOrDefault();
			if (queuedLog.SJ_ParentTableCode == AccTransactionHeaderSchema.Constants.Prefix)
			{
				var header = queuedLog.Factory.Load<TransactionHeader>(queuedLog.SJ_ParentID) as InvoicingBase;
				Process(queuedLog, header);
			}

			var processedLogs = queuedLog.SJ_IsDelayFired ? Array.Empty<IQueuedLog>() : queuedLogs;

			return new LogSubscriberResult(processedLogs, Array.Empty<IQueuedLog>());
		}

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule")]
		void Process(IQueuedLog queuedLog, InvoicingBase header)
		{
			if (header == null)
			{
				return;
			}

			if (requestPKsToSkip.ContainsKey(header.PK))
			{
				if (!string.IsNullOrEmpty(requestPKsToSkip[header.PK]))
				{
					DefaultLogger.Log(LogType.Error, requestPKsToSkip[header.PK]);
				}
				return;
			}

			InvoicingBase convertedHeader = null;

			var converter = new UnapprovedTransactionConverter(queuedLog.Factory);

			if (header.ShowError == null)
			{
				header.ShowError = (string message, string caption) =>
				{
					DefaultLogger.Log(LogType.Warning, "Caption : " + caption + ", Message: " + message);
				};
			}

			convertedHeader = converter.ConvertToAP(
				transaction: header,
				releaseJobHeaderMutex: false,
				revalidateLines: true,
				generateInvoicePdf: true,
				isAutoImport: true);

			var logDelayer = queuedLog as IQueuedLogDelayer;
			if (convertedHeader.Factory.HasContext(BusinessContext.JobLockedByAnotherUser) && logDelayer != null)
			{
				convertedHeader.Factory.RemoveContext(BusinessContext.JobLockedByAnotherUser);
				logDelayer.EventTime = ZDateTime.UtcNow.AddMinutes(AccountingConfigurationRegistry.Instance.JobLockedRetryIntervalInMinutes.Value);
				if (!queuedLog.IsRetry)
				{
					logDelayer.IsDelayFired = true;
				}

				DefaultLogger.Log(LogType.Error, Res.GetString("7c348f52-5df6-47ff-be0d-04002a45d530", "[Times:{0}] Retrying an attempt to post an Intercompany Imported since the job is locked by another user.", queuedLog.SJ_RetryCount));
			}

#if DEBUG
			UpdateConvertedInvoice_ForTestOnly?.Invoke(convertedHeader);
#endif
			if (IsInvoiceLineJobHeaderBranchEmpty(convertedHeader))
			{
				convertedHeader.ReleaseAllMutexOnInvoice();
				return;
			}

			var logsToSearch = new List<Logs>();
			var registryValue = AccountingConfigurationRegistry.Instance.AutoImportIntercompanyEventConfiguration.Value;
			var logEntryFoundWithLaterDate = false;
			if (registryValue.EnableEventConfiguration)
			{
				if (header.IsConsolInvoice)
				{
					logsToSearch.Add(((IStmALogProvider)header.Consol).Logs);
				}
				else
				{
					logsToSearch.Add(((IStmALogProvider)header.Job?.Parent)?.Logs);
					logsToSearch.Add(header.Job?.Logs);
				}

				foreach (var log in logsToSearch)
				{
					logEntryFoundWithLaterDate = (log != null
						&& log.Find(logEntry => registryValue.IntercompanyEventSettingCollection.Cast<IntercompanyEventSetting>().Any
							(entry => entry.StmEventCode == logEntry.SL_SE_NKEvent && logEntry.SL_EventTime >= entry.StartDate)).Any());
					if (logEntryFoundWithLaterDate)
					{
						break;
					}
				}
			}

			if (GetOrdinal(convertedHeader.AuthorisationLevel) > GetOrdinal(GetMaxAuthorisationLevel(header.Company)))
			{
				convertedHeader.ReleaseAllMutexOnInvoice();

				DefaultLogger.Log(LogType.Error, Res.GetString("7AFCAE19-BBC0-4F88-804E-DC1E0B620616", "Intercompany transaction {0} cannot be imported because the difference between the accrual and the transaction to be imported exceeds the cost variance approval level as defined in the Intercompany Posting Configuration registry.\r\nThis transaction will either need to be manually imported into the job using the Job Invoicing > Import AP Invoices Issued by Other Group Companies option, or imported via the Intercompany Transaction Approval module where appropriate approval can be sought.", convertedHeader.AH_TransactionNum));
			}
			else if (registryValue.EnableEventConfiguration && !logEntryFoundWithLaterDate)
			{
				convertedHeader.ReleaseAllMutexOnInvoice();

				DefaultLogger.Log(LogType.Error, Res.GetString("645D1399-765E-4426-BF81-3D106FD9FEF5", "Intercompany transaction {0} cannot be imported because a required event as specified in the Auto Import Intercompany Event Configuration registry is not recorded on the job.\r\nThis transaction will either need to be manually imported into the job using the Job Invoicing > Import AP Invoices Issued by Other Group Companies option, or imported via the Intercompany Transaction Approval module.", convertedHeader.AH_TransactionNum));
			}
			else
			{
				if (AccountingConfigurationRegistry.Instance.EnableValidationWhenAutoImportIntercompanyInvoices.Value)
				{
					convertedHeader.RunPreSaveValidation();
				}

				if (!convertedHeader.HasErrors)
				{
					queuedLog.Factory.ChildFactories.Add(convertedHeader.Factory);
					if (logDelayer?.IsDelayFired ?? false)
					{
						logDelayer.IsDelayFired = false;
					}
					convertedHeader.OnNegativeCompliancesFailedToCreate += new EventHandler(NegativeComplianceFailedToCreate);
					disposableActions.Add(new DisposableAction(() => convertedHeader.OnNegativeCompliancesFailedToCreate -= new EventHandler(NegativeComplianceFailedToCreate)));
				}
				else
				{
					convertedHeader.ReleaseAllMutexOnInvoice();
					DefaultLogger.Log(LogType.Error, GetErrorMessages(convertedHeader));
				}
			}
		}

#if DEBUG
		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		public static Action<InvoicingBase> UpdateConvertedInvoice_ForTestOnly;
#endif

		void NegativeComplianceFailedToCreate(object sender, EventArgs e)
		{
			if (sender is InvoicingBase convertedHeader)
			{
				DefaultLogger.Log(LogType.Warning, AccountingConstants.GetComplianceDocumentNegativeMessageWithInfo(convertedHeader.AH_Ledger, convertedHeader.AH_TransactionType, convertedHeader.AH_TransactionNum).GetUnresolvedString());
			}
		}

		bool IsInvoiceLineJobHeaderBranchEmpty(InvoicingBase convertedHeader)
		{
			var lineJobHeaderPKs = convertedHeader.Lines.OfType<InvoicingLineBase>().Select(x => x.AL_JH).Distinct();

			var errorMessage = new StringBuilder();

			foreach (var pk in lineJobHeaderPKs)
			{
				if (!pk.IsEmpty)
				{
					var foundedJob = convertedHeader.Factory.Load<JobHeader>(pk);

					if (foundedJob != null && foundedJob.JH_GB.IsEmpty)
					{
						errorMessage.AppendLine(Res.GetString("7AE63CC9-BEB3-4DB3-9797-6BC27C1D7FA9", "Unable to post Intercompany Transaction. Job {0}'s Branch cannot be empty. Please check branch defaulting configuration.", foundedJob.JH_JobNum));
					}
				}
			}
			
			if (!string.IsNullOrEmpty(errorMessage.ToString()))
			{
				DefaultLogger.Log(LogType.Error, errorMessage.ToString());
				return true;
			}

			return false;
		}

		static string GetErrorMessages(InvoicingBase importedTransaction)
		{
			var result = new StringBuilder();
			result.AppendLine(Res.GetString("1512bfc8-ef9b-4e5c-8b47-3513a5e1ee8d", "An attempt to post an Intercompany Imported {0} {1} {2} for {3} {4} has failed because of the following validation errors:",
				importedTransaction.AH_Ledger,
				importedTransaction.AH_TransactionType,
				importedTransaction.AH_TransactionNum,
				importedTransaction.AH_RX_NKTransactionCurrency,
				importedTransaction.AH_OSTotalAmount.ToString(string.Format(CultureInfo.CurrentCulture, "N{0}", importedTransaction.TransactionCurrency.Decimals), CultureInfo.CurrentCulture)));

			result.AppendLine(string.Empty);
			result.AppendLine(string.Join("\r\n", new ZNotificationCollector(importedTransaction, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors().GetUniqueMessageList()));
			result.AppendLine(Res.GetString("94b454ef-8bf8-4920-a777-8ea360ac252a", "Please try to do manual import."));

			return result.ToString();
		}

		static ZString GetMaxAuthorisationLevel(GlbCompany company)
		{
			var result = ZString.Empty;
			if (company != null)
			{
				result = TransactionHeader.GetMaxCompanyAuthorizationLevel(company.GC_Code);
			}
			return result;
		}

		int GetOrdinal(ZString authorisationLevel)
		{
			return AuthorisationRequirementForLevelComparison.GetAuthorisationRequirementWeight(authorisationLevel);
		}

		[NonSerialized]
		CostVarianceApprovalAuthorisationRequirement fAuthorisationRequirementForLevelComparison;
		CostVarianceApprovalAuthorisationRequirement AuthorisationRequirementForLevelComparison
		{
			get
			{
				return fAuthorisationRequirementForLevelComparison ??
					(fAuthorisationRequirementForLevelComparison = new CostVarianceApprovalAuthorisationRequirement());
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		static bool IsValidForConvert(InvoicingBase header, GlbCompany company, ILogger logger)
		{
			var isValid = false;
			if (header.Company != company)
			{
				if (company.FirstActiveBranch != null)
				{
					using (new TemporaryUserContext { BranchPK = company.FirstActiveBranch.PK.ToGuid() }.Set())
					{
						var collection = new UnapprovedTransactionCandidateCollection(header.Factory);
						collection.SuspendValidation();
						collection.Load(new ZQuery(AccTransactionHeaderSchema.PK, header.PK));
						isValid = collection.Count > 0;
					}
				}
				else
				{
					logger.Log(LogType.Error, $@"Company {company.GC_Code} does not have any active branch. Transactions from the company will not be processed."); // Just a log string
				}
			}

			return isValid;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Sevice task logs are English-Only")]
		static (UserContextInfo? ContextInfo, string ErrorMessage) GetUserContextIfEligibileForHandling(InvoicingBase header, ILogger logger)
		{
			UserContextInfo? contextInfo = null;
			var errorMessage = string.Empty;
			if (header != null &&
				(header.AH_Ledger == LedgerTypes.AccountsReceivable &&
				(header.AH_TransactionType == TransactionTypes.Invoice || header.AH_TransactionType == TransactionTypes.CreditNote)))
			{
				var branchForLoginResult = IntercompanyTransactionImportHelper.GetAPBranchForAutoImport(header, x => IsValidForConvert(header, x, logger));
				if (branchForLoginResult.IsIntercompanyInvoice)
				{
					var branchForLogin = branchForLoginResult.BranchForIntercompanyImport;
					if (branchForLogin != null)
					{
						var departmentForLogin = IntercompanyTransactionImportHelper.GetDepartmentToLogIntoAPCompany(branchForLogin, GlbDepartment.CurrentDepartment);
						if (AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.GetFallBackValueAtAllLevels(branchForLogin.Company.PK.ToGuid(), Guid.Empty, Guid.Empty))
						{
							contextInfo = new UserContextInfo()
							{
								User = GlbStaff.CurrentUser,
								Branch = branchForLogin,
								Department = departmentForLogin
							};
						}
					}
					else
					{
						errorMessage = string.Format(CultureInfo.InvariantCulture, "Intercompany Transaction {0} {1} {2} debtor {3} company {4} for {5} {6} cannot be auto-imported as Transaction Branch or Company cannot be set with reference to the invoice debtor organization proxy. Please try to use manual import.",
								header.AH_Ledger,
								header.AH_TransactionType,
								header.AH_TransactionNum,
								header.Header.OH_Code,
								header.Company.GC_Code,
								header.AH_RX_NKTransactionCurrency,
								header.AH_OSTotalAmount.ToString(string.Format(CultureInfo.InvariantCulture, "N{0}", header.TransactionCurrency.Decimals), CultureInfo.InvariantCulture));
					}
				}
			}
			return (contextInfo, errorMessage);
		}

		protected override ILogBatcher GetLogBatcher() => new UnapprovedTransactionLogBatcher(requestPKsToSkip, DefaultLogger);

		protected override void Dispose(bool disposing)
		{
			foreach (var action in disposableActions)
			{
				action.Dispose();
			}
			base.Dispose(disposing);
		}

		readonly Dictionary<ZGuid, string> requestPKsToSkip = new Dictionary<ZGuid, string>();
		readonly List<DisposableAction> disposableActions = new List<DisposableAction>();

		class UnapprovedTransactionLogBatcher : LogBatcher<ZGuid>
		{
			internal UnapprovedTransactionLogBatcher(Dictionary<ZGuid, string> requestPKsToSkip, ILogger logger)
			{
				this.requestPKsToSkip = requestPKsToSkip;
				this.logger = logger;
			}

			readonly Dictionary<ZGuid, string> requestPKsToSkip;
			readonly ILogger logger;

			protected override void AddGroupingFetchHints(IEnumerable<IQueuedLog> enumberable)
			{
			}

			protected override ZGuid GetGroupLogKey(IQueuedLog log) => log.SJ_ParentID;

			protected override LogsGroupContext SetContextForLogsGroup(ZGuid groupKey, IEnumerable<IQueuedLog> queuedLogs)
			{
				var result = new LogsGroupContext(false);
				var firstLog = queuedLogs.First();
				if (firstLog.SJ_ParentTableCode == AccTransactionHeaderSchema.Constants.Prefix)
				{
					var header = firstLog.Factory.Load<TransactionHeader>(firstLog.SJ_ParentID) as InvoicingBase;
					var (userContextInfo, message) = GetUserContextIfEligibileForHandling(header, logger);
					if (userContextInfo != null)
					{
						result = new LogsGroupContext(false, Env.SetTemporaryUserContext(userContextInfo.Value.User.PK.ToGuid(), userContextInfo.Value.Branch.PK.ToGuid(), userContextInfo.Value.Department.PK.ToGuid()));
					}
					else
					{
						if (requestPKsToSkip.ContainsKey(groupKey))
						{
							requestPKsToSkip[groupKey] = message;
						}
						else
						{
							requestPKsToSkip.Add(groupKey, message);
						}
					}
				}
				return result;
			}
		}

		struct UserContextInfo
		{
			public GlbStaff User { get; set; }
			public GlbBranch Branch { get; set; }
			public GlbDepartment Department { get; set; }
		}
	}
}
