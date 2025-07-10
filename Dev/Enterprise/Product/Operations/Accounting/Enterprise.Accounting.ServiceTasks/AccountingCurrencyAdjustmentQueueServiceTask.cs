using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.ServiceTasks;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FileFormatUtilities;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedService(
	AccountingCurrencyAdjustmentQueueServiceTask.Code,
	"Currency Adjustment Queue Service Task",
	"ACC",
	typeof(AccountingCurrencyAdjustmentQueueServiceTask),
	IsMandatory = false,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1day")
]

namespace Enterprise.Accounting.ServiceTasks
{
	public class AccountingCurrencyAdjustmentQueueServiceTask : ServiceProviderImpl
	{
		public const string Code = "CAQ";

		public override void RunTask(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			ServiceLogger.Log(LogType.Debug, "AR and AP Outstanding Balance Currency Adjustment automated process starting.");

			RunTaskCore();

			ServiceLogger.Log(LogType.Debug, "AR and AP Outstanding Balance Currency Adjustment automated process completed.");
		}

		#region RunTaskCore

		void RunTaskCore()
		{
			var factory = new BusinessObjectFactory();

			var queuedPeriods = GetQueuedPeriod(factory);
			if (queuedPeriods.Count == 0)
			{
				ServiceLogger.Log(LogType.Warning, "No AR and AP Outstanding Balance Currency Adjustment queue was found.");
				return;
			}

			if (!ValidateControlAccount(factory))
			{
				return;
			}

			foreach (var queuedPeriod in queuedPeriods)
			{
				try
				{
					ProcessQueuedPeriod(queuedPeriod);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ServiceLogger.Log(LogType.Error, FormattableString.Invariant($"Period PK: {queuedPeriod.PeriodPK}, Company PK: {queuedPeriod.CompanyPK}{System.Environment.NewLine}{ex.Message}"));
				}
			}

			if (queuedPeriods.Any())
			{
				ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, "AR and AP Outstanding Balance Currency Adjustment automated process completed for {0} periods.", queuedPeriods.Count));
			}
		}

		void ProcessQueuedPeriod(QueuedPeriod queuedPeriod)
		{
			var newFactory = new BusinessObjectFactory();

			var queuedPeriodModel = new QueuedPeriodModel(newFactory, ServiceLogger, queuedPeriod);
			var period = queuedPeriodModel.Period;
			var company = queuedPeriodModel.Company;

			try
			{
				var validateResult = queuedPeriodModel.ValidateAll();

				if (!validateResult)
				{
					return;
				}

				ServiceLogger.Log(LogType.Debug, FormattableString.Invariant($"Company: {company.GC_Code}, Period {period.AM_Period} Calculation of Currency Adjustment Values starting."));

				using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, company.FirstActiveBranch.PK.ToGuid(), queuedPeriodModel.Department.PK.ToGuid()))
				{
					var result = CreateJournal(queuedPeriodModel);
					ServiceLogger.Log(result.Item1 ? LogType.Debug : LogType.Error, result.Item2);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ServiceLogger.Log(LogType.Error, FormattableString.Invariant($"Company: {company.GC_Code}, Period {period.AM_Period} Calculation of Currency Adjustment Values finished abnormally. {System.Environment.NewLine}{ex.Message}"));
			}
		}

		#endregion

		#region Create Journal

		const string DefaultDepartmentCode = "BRN";

		string HeaderDescription => AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.OutstandingBalanceCurrencyAdjustmentJournal, Res.GetString("D701D73E-D9B7-427A-A321-D6BE138DE4C7", "A/R and A/P Outstanding Balance Currency Adjustment Journal."));

		readonly IEnumerable<ZString> CountriesToCreateGJL = new List<ZString> { CountryCodes.China, CountryCodes.Taiwan };

		(bool, string) CreateJournal(QueuedPeriodModel queuedPeriodModel)
		{
			var newFactory = queuedPeriodModel.Factory;
			var period = queuedPeriodModel.Period;
			var groupedBalancesRevaluation = queuedPeriodModel.GroupedBalancesRevaluation;
			var company = queuedPeriodModel.Company;

			var factoryList = new List<ITransactionParticipant>();
			factoryList.Add(newFactory);

			string message = null;
			string periodLog = null;
			if (groupedBalancesRevaluation.All(x => x.GainLossAmount == 0))
			{
				periodLog = (NoResString)"Automated journal(s) was not created.";
				message = (NoResString)"Automated journal(s) was not created due to Gain and Loss both equal to 0.";
			}
			else
			{
				string[] reportErrors;
				var journals = CreateAutomatedJournals(groupedBalancesRevaluation, period, company, out reportErrors);

				if (reportErrors != null)
				{
					var errorsStringBuilder = new ZStringBuilder($"Automated journal(s) was not created due to {ReportName} report has following validation errors.");
					errorsStringBuilder.Append(new ZStringBuilder(reportErrors));

					return (false, errorsStringBuilder.ToStringWithNewLineBetweenAppends());
				}

				foreach (var journal in journals)
				{
					var aggregator = new AggregateWrapper(journal, journal);
					factoryList.Add(aggregator);
				}

				periodLog = (NoResString)"Automated journal(s) was created.";
				message = (NoResString)"Automated journal(s) was created successfully.";
			}

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			period.Logs.AddNew(Events.EditedARecord, periodLog);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			newFactory.Saving += periodFactory => AccountingUtils.DeleteQueuePeriod(periodFactory, period.PK);

			BusinessObjectFactory.SaveTogether(factoryList.ToArray());

			return (true, message);
		}

		List<GLJournal> CreateAutomatedJournals(IReadOnlyCollection<BalancesRevaluationDetail> balanceRevaluations, AccPeriodManagement period, GlbCompany company, out string[] reportErrors)
		{
			var journals = new List<GLJournal>();
			reportErrors = null;

			var postPeriod = period.AM_Period;
			var agePeriod = new AccountingPeriodCalculator(period.Factory, company).GetNextPeriod(period.AM_Period);

			if (CountriesToCreateGJL.Contains(company.GC_RN_NKCountryCode))
			{
				journals.AddRange(CreateGeneralJournals(period.Factory, postPeriod, agePeriod));
			}
			else
			{
				journals.Add(CreateReverseJournal(period.Factory, postPeriod, agePeriod));
			}

			foreach (var journal in journals)
			{
				foreach (var balanceRevaluation in balanceRevaluations.SkipWhile(x => x.GainLossAmount == 0))
				{
					CreateJournalLines(balanceRevaluation, journal);
				}

				AttachReportsToeDocs(journal, period.AM_Period, out reportErrors);
				if (reportErrors != null)
				{
					return null;
				}

				journal.AH_Desc = HeaderDescription;
				journal.PeriodPK = period.PK;
				period.ReverseJournalPK = journal.PK;
			}

			var reverseJournal = journals.FirstOrDefault(x => x.PostPeriod == agePeriod);
			if (reverseJournal != null)
			{
				foreach (GLJournalLine journalLine in reverseJournal.Lines)
				{
					journalLine.DebitCreditSign = (journalLine.DebitCreditSign == DebitCreditDataEntry.DR ? DebitCreditDataEntry.CR : DebitCreditDataEntry.DR);
				}
			}

			return journals;
		}

		GLJournal CreateReverseJournal(BusinessObjectFactory factory, ZInt postPeriod, ZInt agePeriod)
		{
			var journal = factory.New<GLJournal>();
			journal.AH_TransactionType = TransactionTypes.GLReversingJournal;
			journal.PostPeriod = postPeriod;
			journal.AgePeriod = agePeriod;
			return journal;
		}

		IReadOnlyCollection<GLJournal> CreateGeneralJournals(BusinessObjectFactory factory, ZInt postPeriod, ZInt agePeriod)
		{
			var journals = new List<GLJournal>();
			var postJournal = factory.New<GLJournal>();
			postJournal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			postJournal.PostPeriod = postPeriod;

			var reverseJournal = factory.New<GLJournal>();
			reverseJournal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			reverseJournal.PostPeriod = agePeriod;

			postJournal.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			reverseJournal.AH_TransactionBelongsToGroup = postJournal.AH_TransactionBelongsToGroup;

			journals.Add(postJournal);
			journals.Add(reverseJournal);
			return journals;
		}

		void CreateJournalLines(BalancesRevaluationDetail balanceRevaluation, GLJournal journal)
		{
			var arAdjustment = AccountingConfigurationRegistry.Instance.ARControlAccountAdjustment.Value;
			var apAdjustment = AccountingConfigurationRegistry.Instance.APControlAccountAdjustment.Value;
			var exchangeGain = AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Value;
			var exchangeLoss = AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Value;
			var branch = journal.Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, balanceRevaluation.BranchCode);
			var company = journal.Company;

			var exchangeRateDecimals = company.ExchangeRateDecimalPlaces;
			var localCurrencyDecimals = company.GetLocalDecimals();
			var foreignCurrencyDecimals = journal.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, balanceRevaluation.CurrencyCode).Decimals;
			var transExchangeRate = 0M;
			if (balanceRevaluation.BalanceInLocal != 0M && balanceRevaluation.Balance != 0M)
			{
				if (company.GC_IsReciprocal)
				{
					transExchangeRate = decimal.Round(balanceRevaluation.BalanceInLocal / balanceRevaluation.Balance, exchangeRateDecimals);
				}
				else
				{
					transExchangeRate = decimal.Round(balanceRevaluation.Balance / balanceRevaluation.BalanceInLocal, exchangeRateDecimals);
				}
			}

			var journalLineDesc = Res.GetString("FDEFD44E-97A1-4A1E-A794-B4127536C31D", "{0} Unrealized {1} based on {2} {3}, Original Local {4} {5} (Ex.Rate {6}), Adjusted Local {7} {8} (Ex.Rate {9})",
				balanceRevaluation.Ledger, // {0} Ledger
				balanceRevaluation.GainLossSign == 1 ? Res.GetString("147EC190-4447-4567-97BD-467C168AD711", "Gain") : Res.GetString("80F392E2-C006-414F-8D7C-503266B4361D", "Loss"), // {1} Gain or Loss
				balanceRevaluation.CurrencyCode, decimal.Round(balanceRevaluation.Balance, foreignCurrencyDecimals),
				company.GC_RX_NKLocalCurrency, decimal.Round(balanceRevaluation.BalanceInLocal, localCurrencyDecimals),
				transExchangeRate.ToString("0.000000", CultureInfo.InvariantCulture), // {4} Exchange Rate
				company.GC_RX_NKLocalCurrency, decimal.Round(balanceRevaluation.RevaluedLocalEquivalent, localCurrencyDecimals),
				balanceRevaluation.PeriodEndExchangeRate.ToString("0.000000", CultureInfo.InvariantCulture));

			if (balanceRevaluation.Ledger == LedgerTypes.AccountsReceivable)
			{
				CreateGLJournalLine(DebitCreditDataEntry.DR, balanceRevaluation.GainLossAmount > 0 ? arAdjustment : exchangeLoss, balanceRevaluation.GainLossAmount, journalLineDesc);
				CreateGLJournalLine(DebitCreditDataEntry.CR, balanceRevaluation.GainLossAmount > 0 ? exchangeGain : arAdjustment, balanceRevaluation.GainLossAmount, journalLineDesc);
			}
			else if (balanceRevaluation.Ledger == LedgerTypes.AccountsPayable)
			{
				CreateGLJournalLine(DebitCreditDataEntry.DR, balanceRevaluation.GainLossAmount > 0 ? exchangeLoss : apAdjustment, balanceRevaluation.GainLossAmount, journalLineDesc);
				CreateGLJournalLine(DebitCreditDataEntry.CR, balanceRevaluation.GainLossAmount > 0 ? apAdjustment : exchangeGain, balanceRevaluation.GainLossAmount, journalLineDesc);
			}

			#region Create Reverse Journal Lines

			void CreateGLJournalLine(string debitOrCredit, Guid lineAccount, decimal gainLossAmount, string description)
			{
				var journalLine = journal.GLJournalLines.AddNew();
				journalLine.DebitCreditSign = debitOrCredit;
				journalLine.UnsignedOSLineAmount = gainLossAmount;
				journalLine.AL_AG = lineAccount;
				journalLine.AL_Desc = description;
				journalLine.AL_GB = branch.PK;
			}

			#endregion
		}

		#endregion

		#region Create Report

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "report name")]
		const string ReportName = "Outstanding Balances Currency Revaluation at Period End Rates";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "report group by name")]
		const string ReportGroupByName = "Transaction Currency, Transaction Branch then Organization";

		void AttachReportsToeDocs(GLJournal journal, ZInt period, out string[] reportErrors)
		{
			var reportsDict = new Dictionary<string, ReportCommand>()
			{
				{ LedgerTypes.AccountsReceivable, GetRevaluationReport(nameof(BusinessContext.RepReceivReports)) },
				{ LedgerTypes.AccountsPayable, GetRevaluationReport(nameof(BusinessContext.RepPayablesReports)) }
			};

			AttachReportsToeDocsCore(journal, reportsDict, period, out reportErrors);

			ReportCommand GetRevaluationReport(string context)
			{
				var query = new ZQuery(StmMenuItemSchema.SU_MenuName, ReportName);
				query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, context);

				return journal.Factory.GetCachedValue(context, () => journal.Factory.LoadTop1<ReportCommand>(query));
			}
		}

		void AttachReportsToeDocsCore(GLJournal journal, Dictionary<string, ReportCommand> reportCommandDict, ZInt period, out string[] reportErrors)
		{
			reportErrors = null;

			foreach (var reportCommandMember in reportCommandDict)
			{
				var reportCommand = reportCommandMember.Value;
				var ledger = reportCommandMember.Key;

				using (var report = reportCommand.GetReport())
				{
					((SingleAccountingPeriodField)report.FilterCollection["Period"]).SinglePeriod = period;

					report.GroupByCollection.ToList().OfType<GroupBy>().FirstOrDefault(x => x.DisplayName == ReportGroupByName).Selected = true;
					report.OptionalTemplateSheetCollection.ToList().OfType<OptionalTemplateSheet>().ForEach(x => x.Selected = true);
#if DEBUG
					if (ShouldEnableInvoicePaymentWebService)
					{
						AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.SetTemporaryValue(journal.AH_GC.ToGuid(), Guid.Empty, Guid.Empty, true);
					}
#endif
					report.RunPreSaveValidation();
					if (report.HasErrors)
					{
						reportErrors = report.GetErrors().GetFatalNotifications().GetUniqueMessageList();
						return;
					}

					using (var stream = new MemoryStream())
					{
						try
						{
							report.Save(stream);
						}
						catch (Exception e) when (!e.IsCriticalException())
						{
							throw new ReportServiceException(new[] { e.Message }, ReportServiceErrorType.RunningError);
						}
						var binaryData = DocumentConverter.ConvertFromExcel(stream.ToArray(), OutputFormatType.PDF, ZArchitecture.Environment.ColourDepth.BlackAndWhite);
						journal.DocManagerInfo.AddFileOrDocument(binaryData, ledger + " " + ReportName + ".pdf", "PDF", description: Core.Constants.DocManagerCodes.GLJournal);
					}
				}
			}
		}

#if DEBUG
		public ZBool ShouldEnableInvoicePaymentWebService;
#endif

		#endregion

		#region Validate Control Account

		bool ValidateControlAccount(BusinessObjectFactory factory)
		{
			var result = true;
			var controlAccounts = new List<GuidRegistryItem>
			{
				AccountingConfigurationRegistry.Instance.ARControlAccountAdjustment,
				AccountingConfigurationRegistry.Instance.APControlAccountAdjustment,
				AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount,
				AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount,
			};

			var glHeaders = factory.Load<AccGLHeader>(new ZQuery(AccGLHeaderSchema.PK, controlAccounts.Where(x => x.Value != Guid.Empty).Select(x => x.Value).Distinct())).ToList();
			controlAccounts.ForEach(x => result = result & ValidateGLHeader(x, glHeaders));
			return result;
		}

		bool ValidateGLHeader(GuidRegistryItem registry, List<AccGLHeader> glHeaders)
		{
			var result = true;
			if (registry.Value == Guid.Empty || !glHeaders.Select(x => x.PK).Contains(registry.Value))
			{
				ServiceLogger.Log(LogType.Error, $"Registry '{registry.HumanReadableRegistryPath()}' has not been setup.");
				result = false;
			}
			return result;
		}

		#endregion

		IReadOnlyCollection<QueuedPeriod> GetQueuedPeriod(BusinessObjectFactory factory)
		{
			var sql = FormattableString.Invariant($@"SELECT ACA_ParentID, ACA_GC FROM dbo.AccCurrencyAdjustmentQueue ORDER BY ACA_GC, ACA_Date");
			var dynamicCollection = new DynamicBusinessObjectCollection(factory);
			dynamicCollection.Load(sql);
			var queuedPeriods = new List<QueuedPeriod>(dynamicCollection.Count);

			foreach (DynamicBusinessObject bizo in dynamicCollection)
			{
				queuedPeriods.Add(new QueuedPeriod(bizo));
			}

			return queuedPeriods;
		}

		class QueuedPeriod
		{
			public ZGuid PeriodPK { get; private set; }
			public ZGuid CompanyPK { get; private set; }

			public QueuedPeriod(DynamicBusinessObject bizo)
			{
				PeriodPK = (ZGuid)bizo["ACA_ParentID"];
				CompanyPK = (ZGuid)bizo["ACA_GC"];
			}
		}

		class QueuedPeriodModel
		{
			public QueuedPeriodModel(BusinessObjectFactory factory, ILogger parentServiceLogger, QueuedPeriod queuedPeriod)
			{
				Factory = factory;
				ParentServiceLogger = parentServiceLogger;

				Initialize(queuedPeriod.PeriodPK, queuedPeriod.CompanyPK);
			}

			readonly ILogger ParentServiceLogger;
			public BusinessObjectFactory Factory;

			ZGuid PeriodPK { get; set; }
			public AccPeriodManagement Period { get; private set; }
			public GlbCompany Company { get; private set; }
			public GlbDepartment Department { get; private set; }
			public IReadOnlyCollection<BalancesRevaluationDetail> GroupedBalancesRevaluation { get; private set; }

			void Initialize(ZGuid periodPK, ZGuid companyPK)
			{
				PeriodPK = periodPK;
				Period = Factory.Load<AccPeriodManagement>(periodPK);
				Company = Factory.Load<GlbCompany>(companyPK);
				Department = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, DefaultDepartmentCode);
			}

			public bool ValidateAll()
			{
				var result = true;
				if (!CheckNullPeriod(Period, Company.GC_Code))
				{
					AccountingUtils.DeleteQueuePeriod(Factory, PeriodPK);
					result = false;
				}
				else if (!ValidateInvoicePaymentWebService(Company.PK))
				{
					AccountingUtils.DeleteQueuePeriod(Factory, Period.PK);
					result = false;
				}
				else if (!ValidatePeriod(Period, Company))
				{
					result = false;
				}
				else
				{
					GroupedBalancesRevaluation = GetGroupedBalancesRevaluation(Period, Company);
					var transactionCurrency = GroupedBalancesRevaluation.Where(x => x.CurrencyCode != Company.GC_RX_NKLocalCurrency).Select(x => x.CurrencyCode).Distinct().ToArray();
					if (!ValidateCurrencyExchangeRate(Period, transactionCurrency, Company.GC_Code))
					{
						result = false;
					}
				}

				return result;
			}

			#region Validate

			bool CheckNullPeriod(AccPeriodManagement period, ZString companyCode)
			{
				if (period == null)
				{
					ParentServiceLogger.Log(LogType.Warning, FormattableString.Invariant($"Invalid queue period is found in Company {companyCode}."));
					return false;
				}
				return true;
			}

			bool ValidateInvoicePaymentWebService(ZGuid companyPK)
			{
				if (AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.GetValueWithoutFallback(companyPK.ToGuid(), Guid.Empty, Guid.Empty))
				{
					ParentServiceLogger.Log(LogType.Warning, FormattableString.Invariant($"The Invoice Payment Web Service is enabled. Queue deleted."));
					return false;
				}
				return true;
			}

			bool ValidatePeriod(AccPeriodManagement period, GlbCompany company)
			{
				var periodCalculator = new AccountingPeriodCalculator(period.Factory, company);

				if (period.AM_IsGeneralLedgerClosed)
				{
					ParentServiceLogger.Log(LogType.Error, FormattableString.Invariant($"Period {period.AM_Period} is closed."));
					return false;
				}

				var nextPeriod = periodCalculator.GetNextPeriod(period.AM_Period);
				if (nextPeriod == AccountingPeriodCalculator.InvalidPeriod)
				{
					ParentServiceLogger.Log(LogType.Error, FormattableString.Invariant($"The Reverse Period of {period.AM_Period} has not been setup."));
					return false;
				}

				return true;
			}

			bool ValidateCurrencyExchangeRate(AccPeriodManagement period, string[] currencyCodes, ZString companyCode)
			{
				var exchangeRateType = AccountingConfigurationRegistry.Instance.ARAPOutstandingBalancesCurrencyAdjustmentExchangeRateType.GetFallBackValueAtAllLevels(period.AM_GC_Company.ToGuid(), Guid.Empty, Guid.Empty);

				var query = new ZQuery();
				query.AddToFilter(RefExchangeRateSchema.RE_ExRateType, exchangeRateType);
				query.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, currencyCodes);
				query.AddToFilter(RefExchangeRateSchema.RE_GC, period.AM_GC_Company);
				query.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, period.AM_EndDate);
				query.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, period.AM_EndDate);
				var exchangeRateCurrency = period.Factory.Load<RefExchangeRate>(query).Select(x => x.RE_RX_NKExCurrency).Distinct().ToList();
				if (currencyCodes.Length > exchangeRateCurrency.Count)
				{
					var currencyWithEmptyExchangeRate = currencyCodes.Where(x => !exchangeRateCurrency.Contains(x));
					ParentServiceLogger.Log(LogType.Error, $"The exchange rate for transaction currency {string.Join(",", currencyWithEmptyExchangeRate)} cannot be found with Rate type {exchangeRateType} in {companyCode} login company.");
					return false;
				}

				return true;
			}

			#endregion

			#region Data

			IReadOnlyCollection<BalancesRevaluationDetail> GetGroupedBalancesRevaluation(AccPeriodManagement period, GlbCompany company)
			{
				var balancesRevaluationDetails = GetBalancesRevaluationDetails(period);

				return balancesRevaluationDetails
					.GroupBy(x => new { x.Ledger, x.CurrencyCode, x.BranchCode, x.GainLossSign })
					.Select(g => new BalancesRevaluationDetail()
					{
						Ledger = g.Key.Ledger,
						CurrencyCode = g.Key.CurrencyCode,
						BranchCode = g.Key.BranchCode,
						GainLossSign = g.Key.GainLossSign,
						PeriodEndExchangeRate = g.Key.CurrencyCode == company.GC_RX_NKLocalCurrency ? 1M : g.Max(x => x.PeriodEndExchangeRate),
						Balance = g.Sum(x => x.Balance),
						BalanceInLocal = g.Sum(x => x.BalanceInLocal),
						RevaluedLocalEquivalent = g.Sum(x => x.RevaluedLocalEquivalent),
						GainLossAmount = g.Sum(x => x.GainLossAmount),
					})
					.OrderBy(x => x.Ledger).ThenByDescending(x => x.GainLossSign).ThenBy(x => x.CurrencyCode).ToList();
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
			IReadOnlyCollection<BalancesRevaluationDetail> GetBalancesRevaluationDetails(AccPeriodManagement period)
			{
				var balancesRevaluationDetailCollection = new List<BalancesRevaluationDetail>();
				var connection = ((IDbConnected)period.Factory).Connection;
				foreach (var ledgerType in new[] { LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable })
				{
					using (var transactionManager = connection.BeginTransactionWithManager())
					{
						var cmd = connection.Command("ARAPTransactionsSP");
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.AddParameter("@Period", SqlDbType.Int, (int)period.AM_Period);
						cmd.AddParameter("@Company", SqlDbType.UniqueIdentifier, period.AM_GC_Company.ToGuid());
						var branchPK = period.Company?.FirstActiveBranch?.PK.ToGuid() ?? Guid.Empty;
						cmd.AddParameter("@Branch", SqlDbType.UniqueIdentifier, branchPK);
						cmd.AddParameter("@LedgerType", SqlDbType.Char, ledgerType);
						cmd.AddParameter("@ShowInInvoicedCurrency", SqlDbType.Char, "Y");
						cmd.AddParameter("@CurrentDateTime", SqlDbType.DateTime, Env.Time.CurrentLocalDateTime);
						cmd.AddParameter("@OrgList", SqlDbType.NVarChar, string.Empty);
						cmd.AddParameter("@OrgGroupList", SqlDbType.NVarChar, string.Empty);
						cmd.AddParameter("@BranchList", SqlDbType.NVarChar, string.Empty);
						cmd.AddParameter("@CountryList", SqlDbType.NVarChar, string.Empty);
						cmd.AddParameter("@ExCountryList", SqlDbType.NVarChar, string.Empty);
						cmd.AddParameter("@SalesRepList", SqlDbType.NVarChar, string.Empty);
						cmd.AddParameter("@OverLimitOnly", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@AgeingOption", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@Day1", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@Day2", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@Day3", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@Day4", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@AccountsRelationShip", SqlDbType.NVarChar, string.Empty);
						cmd.AddParameter("@ConsolidatedCategory", SqlDbType.NVarChar, string.Empty);
						cmd.AddParameter("@SalesRepRoll", SqlDbType.NVarChar, string.Empty);
						cmd.AddParameter("@SummaryOnly", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@AgedByInvoiceDate", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@CurrencyList", SqlDbType.NVarChar, string.Empty);
						cmd.AddParameter("@IncludeDisbursement", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@DisbursementTranOnly", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@SettlementGroupList", SqlDbType.NVarChar, string.Empty);
						cmd.AddParameter("@CreditRating", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@ShowLocalEquivalentTotal", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@ShowAllTransactions", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@PaymentStatus", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@TransactionTypeList", SqlDbType.NVarChar, string.Empty);
						cmd.AddParameter("@PostDateFrom", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@PostDateTo", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@DueDateFrom", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@DueDateTo", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@InvoiceDateFrom", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@InvoiceDateTo", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@NotInActiveBatchTranOnly", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@GroupBy", SqlDbType.NVarChar, string.Empty);
						cmd.AddParameter("@OrderBy", SqlDbType.NVarChar, string.Empty);
						cmd.AddParameter("@OrgBranch", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@ShowAddUser", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@ShowOnlyAggregated", SqlDbType.Char, string.Empty);
						cmd.AddParameter("@ShowLineAmounts", SqlDbType.Char, string.Empty);

						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								var revaluationDetail = new BalancesRevaluationDetail();
								revaluationDetail.Ledger = ledgerType;
								revaluationDetail.CurrencyCode = (string)reader["CurrencyCode"];
								revaluationDetail.BranchCode = (string)reader["BranchCode"];
								revaluationDetail.PeriodEndExchangeRate = reader["PeriodEndRate"] != DBNull.Value ? (decimal)reader["PeriodEndRate"] : 0M;
								revaluationDetail.Balance = (decimal)reader["Balance"];
								revaluationDetail.BalanceInLocal = (decimal)reader["BalanceInLocal"];
								revaluationDetail.RevaluedLocalEquivalent = (decimal)reader["RevaluedLocalEquivalent"];
								revaluationDetail.GainLossAmount = (decimal)reader["GainLossOnRevaluation"];

								if (revaluationDetail.Ledger == LedgerTypes.AccountsReceivable)
								{
									revaluationDetail.GainLossSign = revaluationDetail.GainLossAmount >= 0 ? 1 : 0;
								}
								else
								{
									revaluationDetail.GainLossSign = revaluationDetail.GainLossAmount <= 0 ? 1 : 0;
								}

								balancesRevaluationDetailCollection.Add(revaluationDetail);
							}
						}
					}
				}

				return balancesRevaluationDetailCollection;
			}

			#endregion
		}

		class BalancesRevaluationDetail
		{
			public string Ledger { get; set; }
			public string CurrencyCode { get; set; }
			public string BranchCode { get; set; }
			public decimal PeriodEndExchangeRate { get; set; }
			public decimal Balance { get; set; }
			public decimal BalanceInLocal { get; set; }
			public decimal RevaluedLocalEquivalent { get; set; }
			public decimal GainLossAmount { get; set; }
			public int GainLossSign { get; set; }
		}
	}
}
