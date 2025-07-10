using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ComplianceReport.FEC;
using Enterprise.Accounting.Business.ComplianceReport.HMRC;
using Enterprise.Accounting.Business.ComplianceReport.IDEA;
using Enterprise.Accounting.Business.ComplianceReport.JPKV7M;
using Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA;
using Enterprise.Accounting.Registry.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	[CodeProperty(AutoAccComplianceReport.Schema.ACR_ReportType), DescriptionProperty(AutoAccComplianceReport.Schema.ACR_Description)]
	[UniversalDataContext(DataContextType.ComplianceReport)]
	public partial class AccComplianceReport : AutoAccComplianceReport, IDocumentSupportable, IDocManagerSupport, IWorkflowProvider, IEDocsParsingSupport
	{
		public AccComplianceReport(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			CommandTimeout = AccountingMasterFilesRegistry.Instance.ComplianceReportQueryTimeoutInMinutes.Value * 60;
			controlAccountAndReportSubCodeMapping = new ControlAccountAndReportSubCodeMapping();
		}
		readonly ControlAccountAndReportSubCodeMapping controlAccountAndReportSubCodeMapping;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ACR_GC_Company = GlbCompany.CurrentCompany.PK;
			ACR_Status = Status.ReportCreated;

			var complianceReportDefaultValueProvider = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Company.GC_RN_NKCountryCode) as IInstanceProvider<IComplianceReportDefaultValue>)?.Get();
			if (complianceReportDefaultValueProvider != null)
			{
				ACR_ReferenceNumber = complianceReportDefaultValueProvider.GetReferenceNumber();
			}
		}

		#region Schema

		public new abstract class Schema : AutoAccComplianceReport.Schema
		{
			public const string ACR_IsFinalised = "ACR_IsFinalised";
			public const string GeneralLedgerAmountDR = "GeneralLedgerAmountDR";
			public const string GeneralLedgerAmountCR = "GeneralLedgerAmountCR";
			public const string MaxLineNum = "MaxLineNum";
			public const string NextProcessingStepFromDate = "NextProcessingStepFromDate";

			public const string APVatSummaryPageNumberFrom = "APVatSummaryPageNumberFrom";
			public const string APVatSummaryPageNumberTo = "APVatSummaryPageNumberTo";
			public const string ARVatSummaryPageNumberFrom = "ARVatSummaryPageNumberFrom";
			public const string ARVatSummaryPageNumberTo = "ARVatSummaryPageNumberTo";
			public const string LiquidazioneIvaPageNumberFrom = "LiquidazioneIvaPageNumberFrom";
			public const string LiquidazioneIvaPageNumberTo = "LiquidazioneIvaPageNumberTo";
		}

		#endregion

		public static readonly AccComplianceReportTypeDecider TypeDecider = new AccComplianceReportTypeDecider();

		public ZString UniqueReportID
		{
			get
			{
				return Company.GC_Code + "-" + (Branch != null ? Branch.GB_Code : ZString.Empty) + "-" +
					Company.GC_RN_NKCountryCode + "-" + ACR_ReportType + "-" + ACR_DateFrom.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "-" + ACR_DateTo.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			}
		}

		readonly int CommandTimeout;

		public ZBool ApplyLinesLoadingLimit
		{
			get => applyLinesLoadingLimit;
			set
			{
				if (applyLinesLoadingLimit != value)
				{
					ResetLinesAndDependingCollections();
				}
				applyLinesLoadingLimit = value;
			}
		}
		ZBool applyLinesLoadingLimit;

		public static int MaxReportLinesToLoad =>
#if DEBUG
		Globals.IsTest ? 100 :
#endif
		1000;

		void ResetLinesAndDependingCollections()
		{
			reportLines = null;
			reportLinesCurrentPeriod = null;
			reportLinesPreviousPeriod = null;

			glMovementDetails = null;
			glMovementDetailsView = null;

			glClosingBalanceDetails = null;
			glClosingBalanceDetailsView = null;
		}

		public ZBool CheckExeedsMaxReportLinesToLoad()
		{
			var count = ZInt.Zero;

			if (IsInDatabase && ReportConfiguration != null)
			{
				var getLinesTVF = GetComplianceReportLinesBasedOnGroupByAndTablePrefix();

				var sql = $@"SELECT
COUNT(*) AS CountReportLines
FROM (SELECT TOP {MaxReportLinesToLoad + 1} 1 AS TestCol FROM {getLinesTVF} (@PK, @ReportTablePrefix, @ReportDateFrom, @ReportDateTo_PlusOneDay, @RegType, @ReportCountry, @RepCountryRegistrationCodeType, @SubCodeToGLAccountMapping, @GS, @RoundingType, @Rounding, @ReverseSign)) AS TestTab
";

				using (var command = GetLoadLinesCommand(sql))
				{
					count = new ZInt(command.ExecuteScalar());
				}
			}

			return count > MaxReportLinesToLoad;
		}

		public event EventHandler<BoolResponseEventArgs> ConfirmLinesLoadingLimit;

		#region BoolResponseEventArgs

		public class BoolResponseEventArgs : EventArgs
		{
			public BoolResponseEventArgs(bool response = false)
			{
				Response = response;
			}

			public bool Response { get; set; }
		}

		#endregion

		protected override ZString HumanReadableNameCore => Res.GetString("2e549784-7e7d-4d4f-96ab-e76cb514eed9", "Compliance Report");

		void CheckAndReportErrorOnIncreasedTimeout()
		{
			int commandTimeoutInMinutes = CommandTimeout / 60;
			if (commandTimeoutInMinutes > AccountingMasterFilesRegistry.Instance.ComplianceReportQueryTimeoutInMinutes.DefaultValue)
			{
				ErrorReporter.ReportOnce(AccountingMasterFilesRegistry.Instance.ComplianceReportQueryTimeoutInMinutes.Name,
					$"The Account Compliance Report '{UniqueReportID}' is using {commandTimeoutInMinutes} minutes(s) as ComplianceReportQueryTimeoutInMinutes value!");
			}
		}

		#region ReadOnly

		public override bool ReadOnly
		{
			get { return base.ReadOnly || ACR_IsFinalised; }
			set { base.ReadOnly = value; }
		}

		#endregion

		#region Delete

		public override bool CanDelete
		{
			get
			{
				return base.CanDelete && !ACR_IsFinalised;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return ACR_IsFinalised ?
					ResString.GetMultilingualString("a03d165e-35a3-4392-93fe-44519ec2a3b1", "Finalized Compliance Report cannot be deleted.")
					: base.ReasonForNotAbleToDelete;
			}
		}

		public override void Delete()
		{
			var isLiquidazioneIvaType = ACR_ReportType == ReportTypes.LiquidazioneIVA;

			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();

			base.Delete();

			if (IsInDatabase)
			{
				DeleteTransactionPivots();
				if (isLiquidazioneIvaType)
				{
					TaxReturns.DeleteAll();
				}
			}
		}

		AccTaxReturn[] TaxReturns => Factory.Load<AccTaxReturn>(new ZQuery(AccTaxReturnSchema.ATR_ACR_ComplianceReport, PK.ToGuid()));

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void DeleteTransactionPivots()
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"	DELETE {0} WHERE {1} = @ReportPK",
				AccComplianceReportTransactionPivotSchema.Constants.TableName,
				AccComplianceReportTransactionPivotSchema.Constants.ACL_ACR_Report);

			CheckAndReportErrorOnIncreasedTimeout();
			using (var command = ((IDbConnected)Factory).Connection.Command(sql, CommandTimeout))   // No BizO generated for this table. There is no sense to generate business objects for queue entry and pivot as they are not bound to GUI
			{
				command.AddParameterBasedOnDbColumn("@ReportPK", PK.ToGuid(), AccComplianceReportTransactionPivotSchema.ACL_ACR_Report);
				command.ExecuteNonQuery();
			}
		}

		#endregion

		#region OnFactorySavingBeforeTransactionCore

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			inFactorySaving = true;
		}

		bool inFactorySaving;

		#endregion

		public override void OnSaving()
		{
			base.OnSaving();

			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(ACR_ReportType), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(ACR_DateFrom), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(ACR_DateTo), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(ACR_Status), ConcurrencyPolicy.Strict);
		}

		public int LocalDecimals => Company.GetLocalDecimals();

		internal enum BusinessContext
		{
			PreSavingValidation
		}

		protected override void RunPreSaveValidationCore()
		{
			using (new DisposableAction(
				() => this.SetContext(BusinessContext.PreSavingValidation),
				() => this.RemoveContext(BusinessContext.PreSavingValidation)))
			{
				base.RunPreSaveValidationCore();
			}
		}

		#region OnFactorySaved

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			inFactorySaving = false;
		}

		#endregion

		#region ACR_ReportType

		[List("Lookups.ReportTypeList")]
		[ReadOnly(true)]
		public override ZString ACR_ReportType
		{
			get { return base.ACR_ReportType; }
			set
			{
				var hasChanges = ACR_ReportType != value;
				base.ACR_ReportType = value;
				if (hasChanges)
				{
					reportLines = null;
					reportTotals = null;

					reportTotalsCurrentPeriod = null;
					reportTotalsPreviousPeriod = null;

					if (ReportConfiguration != null)
					{
						ACR_Periodicity = ReportConfiguration.ReportPeriodicity;
					}
				}
			}
		}

		public static class ReportTypes
		{
			public const string SAFT = "SAF";
			public const string SAFTOnlyTransactions = "SAT";
			public const string Esterometro = "EST";
			public const string LiquidazioneIVA = "LIQ";
			public const string ZMGermany = "ZMD";
			public const string IDEA = "IDE";
			public const string FEC = "FEC";
			public const string JPKV7M = "JPK";
			public const string OpenFormatOnlyTransactions = "OFS";
		}

		#endregion

		#region ACR_Periodicity

		[List("Lookups.PeriodicityList")]
		[ReadOnly(true)]
		public override ZString ACR_Periodicity
		{
			get { return base.ACR_Periodicity; }
			set
			{
				var hasChanges = ACR_Periodicity != value;
				base.ACR_Periodicity = value;
				if (hasChanges)
				{
					accountingPeriod = null;
					ACR_DateFrom = ZDate.Empty;
					ACR_DateFromInfo.RefreshBinding();
					ACR_DateTo = ZDate.Empty;
					ACR_DateToInfo.RefreshBinding();
				}
			}
		}

		IComplianceFinancialYear GetComplianceFinancialYear() =>
			(ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Company.GC_RN_NKCountryCode) as IInstanceProvider<IComplianceFinancialYear>)?.Get();

		public ZString PeriodicityDescription
		{
			get { return Lookups.PeriodicityList.ContainsCode(ACR_Periodicity) ? Lookups.PeriodicityList[ACR_Periodicity, StringComparison.OrdinalIgnoreCase].Description : string.Empty; }
		}

		internal bool UseAccountingPeriod => ACR_Periodicity == ReportPeriodicityCodes.AccountingPeriod;

		bool AreDatesAndPeriodEditable => ACR_Status == AccComplianceReport.Status.ReportCreated
			|| ACR_Status == AccComplianceReport.Status.ReportPendingQueueing
			|| ACR_Status == AccComplianceReport.Status.ReportDataQueued;

		bool AccountingPeriod_ReadOnly => (!UseAccountingPeriod && !UseFinancialYear && !UseComplianceFinancialYear) || !AreDatesAndPeriodEditable;

		public bool ACR_Date_ReadOnly => UseAccountingPeriod || !AreDatesAndPeriodEditable || UseFinancialYear || UseComplianceFinancialYear;

		bool UseMonth => ACR_Periodicity == ReportPeriodicityCodes.CalendarMonth;

		internal bool UseFinancialYear => ACR_Periodicity == ReportPeriodicityCodes.FinancialYear;

		internal bool UseComplianceFinancialYear => ACR_Periodicity == ReportPeriodicityCodes.ComplianceFinancialYear;

		bool UseMonthlyQuarterlyYearly => ACR_Periodicity == ReportPeriodicityCodes.MonthlyQuarterlyYearly;

		#endregion

		#region AccountingPeriod

		[ReadOnlyMember(nameof(AccountingPeriod_ReadOnly))]
		public ZInt AccountingPeriod
		{
			get
			{
				return accountingPeriod.HasValue && accountingPeriod != 0 ? accountingPeriod.Value : (accountingPeriod = CalculatePeriodFromDates()).Value;
			}
			set
			{
				var hasChanges = !accountingPeriod.HasValue || accountingPeriod.Value != value;
				SetNonPersistentPropertyValue(AccountingPeriodInfo, ref accountingPeriod, value);

				if (hasChanges)
				{
					Validation.ValidateAccountingPeriod();

					if (!AccountingPeriodInfo.HasErrors())
					{
						var dates = GetPeriodDates();
						if (dates.Any())
						{
							using (GetValidationSuspender())
							{
								ACR_DateFrom = new ZDate(dates.Min(x => x.Start));
								ACR_DateTo = new ZDate(dates.Max(x => x.End));
							}
							Validation.ValidateACR_DateFrom();
							Validation.ValidateACR_DateTo();

							ACR_DateFromInfo.RefreshBinding();
							ACR_DateToInfo.RefreshBinding();
						}
					}
				}
			}
		}
		ZInt? accountingPeriod;

		IEnumerable<DateRange> GetPeriodDates()
		{
			switch (ACR_Periodicity)
			{
				case ReportPeriodicityCodes.FinancialYear:
					return GetFinancialYearDates((ZShort)AccountingPeriod);
				
				case ReportPeriodicityCodes.ComplianceFinancialYear:
					return GetComplianceFinancialYearDates(AccountingPeriod);

				default:
					return GetAccountingPeriodDates(AccountingPeriod);
			}
		}

		IEnumerable<DateRange> GetFinancialYearDates(ZShort accountingPeriod)
		{
			var query = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, ACR_GC_Company);
			query.AddToFilter(AccPeriodManagementSchema.AM_Year, accountingPeriod);
			return Factory
				.Load<AccPeriodManagement>(query)
				.Select(AccPeriodManagementDateTimeRange)
				.ToImmutableList();
		}

		IEnumerable<DateRange> GetComplianceFinancialYearDates(ZInt accountingPeriod)
		{
			var complianceFinancialYear = GetComplianceFinancialYear() ?? throw new InvalidOperationException($"Missing implementation of ComplianceFinancialYear for {ReportCountryCode}.");

			return ImmutableList.Create(complianceFinancialYear.FinancialYearRange(accountingPeriod));
		}

		IEnumerable<DateRange> GetAccountingPeriodDates(ZInt accountPeriod)
		{
			var query = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, ACR_GC_Company);
			query.AddToFilter(AccPeriodManagementSchema.AM_Period, accountingPeriod);
			return Factory
				.Load<AccPeriodManagement>(query)
				.Select(AccPeriodManagementDateTimeRange)
				.ToImmutableList();
		}

		DateRange AccPeriodManagementDateTimeRange(AccPeriodManagement accPeriodManagement) =>
			new DateRange(accPeriodManagement.AM_StartDate.ToDateTime(), accPeriodManagement.AM_EndDate.ToDateTime());

		public ResourceStringData AccountingPeriodCaptionResString
		{
			get
			{
				switch (ACR_Periodicity)
				{
					case ReportPeriodicityCodes.FinancialYear:
					case ReportPeriodicityCodes.ComplianceFinancialYear:
						return Res.GetData("C6023486-D10D-44D4-826E-3BC9D1EFAF46", "Financial Year");

					default:
						return Res.GetData("DD2FC82E-3C07-42D8-BE49-E72F86FBB4A9", "Period", "Accounting Period derived from the post date range of the report");
				}
			}
		}

		ZInt CalculatePeriodFromDates()
		{
			var result = ZInt.Zero;

			if (!ACR_DateFrom.IsEmpty && !ACR_DateTo.IsEmpty)
			{
				var complianceFinancialYear = GetComplianceFinancialYear();

				if (ACR_Periodicity == ReportPeriodicityCodes.ComplianceFinancialYear && complianceFinancialYear != null)
				{
					return complianceFinancialYear.FinancialYear(ACR_DateTo.ToDateTime());
				}
				else
				{
					var periodFilter = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, ACR_GC_Company);
					periodFilter.AddToFilter(AccPeriodManagementSchema.AM_StartDate, SQLComparisonOperator.EqualToDatePartOnly, ACR_DateFrom);
					if (UseAccountingPeriod)
					{
						periodFilter.AddToFilter(AccPeriodManagementSchema.AM_EndDate, SQLComparisonOperator.EqualToDatePartOnly, ACR_DateTo);
					}
					var period = Factory.LoadTop1<AccPeriodManagement>(periodFilter);
					if (period != null)
					{
						result = UseFinancialYear ? period.AM_Year : period.AM_Period;
					}
				}
			}
			return result;
		}

		public ZPropertyInfo AccountingPeriodInfo
		{
			get { return GetZPropertyInfo(nameof(AccountingPeriod)); }
		}

		#endregion

		#region ACR_DateFrom

		[ResourceStringData("AccComplianceReport|ACR_DateFrom", Caption = "Date From", FullDescription= "First post date in the report period")]
		[ReadOnlyMember(nameof(ACR_Date_ReadOnly))]
		public override ZDate ACR_DateFrom
		{
			get { return base.ACR_DateFrom; }
			set
			{
				if (UseMonth && ACR_DateFrom != value && !value.IsEmpty)
				{
					base.ACR_DateFrom = FirstDayOfMonth(value);
					ACR_DateFromInfo.RefreshBinding();

					if (ACR_DateTo != LastDayOfMonth(value))
					{
						ACR_DateTo = LastDayOfMonth(value);
						ACR_DateToInfo.RefreshBinding();
					}
				}
				else
				{
					if (UseMonthlyQuarterlyYearly && ACR_DateFrom != value && !value.IsEmpty)
					{
						AdjustDateFromAndToForMonthlyQuarterlyYearly(value, ACR_DateTo);
					}
					else
					{
						base.ACR_DateFrom = value;
					}
				}
			}
		}

		ZDate FirstDayOfMonth(ZDate date)
		{
			return !date.IsEmpty ? new ZDate(date.Year, date.Month, 1) : ZDate.Empty;
		}

		ZDate LastDayOfMonth(ZDate date)
		{
			return !date.IsEmpty ? new ZDate(date.Year, date.Month, 1).AddMonths(1).AddDays(-1) : ZDate.Empty;
		}

		ZDate FirstDayOfQuarter(ZDate date)
		{
			return !date.IsEmpty ? new ZDate(date.Year, 3 * ((date.Month + 2) / 3), 1).AddMonths(-2) : ZDate.Empty;
		}

		ZDate LastDayOfQuarter(ZDate date)
		{
			return !date.IsEmpty ? new ZDate(date.Year, 3 * ((date.Month + 2) / 3), 1).AddMonths(1).AddDays(-1) : ZDate.Empty;
		}

		ZDate FirstDayOfYear(ZDate date)
		{
			return !date.IsEmpty ? new ZDate(date.Year, 1, 1) : ZDate.Empty;
		}

		ZDate LastDayOfYear(ZDate date)
		{
			return !date.IsEmpty ? new ZDate(date.Year, 12, 31) : ZDate.Empty;
		}

		internal void AdjustDateFromAndToForMonthlyQuarterlyYearly(ZDate dateFrom, ZDate dateTo)
		{
			if (dateFrom.IsEmpty)
			{
				if (!dateTo.IsEmpty)
				{
					base.ACR_DateTo = LastDayOfMonth(dateTo);
					ACR_DateToInfo.RefreshBinding();
				}
			}
			else
			{
				if (dateTo.IsEmpty)
				{
					base.ACR_DateFrom = FirstDayOfMonth(dateFrom);
				}
				else
				{
					if (dateTo < dateFrom || (dateTo.Month == dateFrom.Month && dateTo.Year == dateFrom.Year))
					{
						base.ACR_DateFrom = FirstDayOfMonth(dateFrom);
						base.ACR_DateTo = LastDayOfMonth(dateFrom);
					}
					else
					{
						if (dateTo.Month < dateFrom.Month + 3 && dateTo.Year == dateFrom.Year)
						{
							base.ACR_DateFrom = FirstDayOfQuarter(dateFrom);
							base.ACR_DateTo = LastDayOfQuarter(dateFrom);
						}
						else
						{
							base.ACR_DateFrom = FirstDayOfYear(dateFrom);
							base.ACR_DateTo = LastDayOfYear(dateFrom);
						}
					}
					ACR_DateToInfo.RefreshBinding();
				}
				ACR_DateFromInfo.RefreshBinding();
			}
		}

		#endregion

		#region ACR_DateTo

		[ResourceStringData("AccComplianceReport|ACR_DateTo", Caption = "Date To", FullDescription = "Last post date in the report period")]
		[ReadOnlyMember(nameof(ACR_Date_ReadOnly))]
		public override ZDate ACR_DateTo
		{
			get { return base.ACR_DateTo; }
			set
			{
				if (UseMonth && ACR_DateTo != value && !value.IsEmpty)
				{
					base.ACR_DateTo = LastDayOfMonth(value);
					ACR_DateToInfo.RefreshBinding();

					if (ACR_DateFrom != FirstDayOfMonth(value))
					{
						ACR_DateFrom = FirstDayOfMonth(value);
						ACR_DateFromInfo.RefreshBinding();
					}
				}
				else
				{
					if (UseMonthlyQuarterlyYearly && ACR_DateTo != value && !value.IsEmpty)
					{
						AdjustDateFromAndToForMonthlyQuarterlyYearly(ACR_DateFrom, value);
					}
					else
					{
						base.ACR_DateTo = value;
					}
				}
			}
		}

		#endregion

		#region Report Status

		[List("Lookups.ReportStatusList")]
		[ReadOnly(true)]
		public override ZString ACR_Status
		{
			get { return base.ACR_Status; }
			set
			{
				if (base.ACR_Status != value)
				{
					ACR_StatusMessage = string.Empty;
				}
				base.ACR_Status = value;
			}
		}

		public ZString ReportStatusDescription => Lookups.ReportStatusList.GetDescriptionFromCode(ACR_Status) ?? string.Empty;

		public static class Status
		{
			public const string ReportCreated = "ADD";
			public const string ReportPendingQueueing = "PQU";
			public const string ReportDataQueued = "QUE";
			public const string ReportGenerated = "GEN";
			public const string ReportOutputGenerated = "OUT";
			public const string ReportInvalidated = "INV";
			public const string ReportFinalised = "FIN";
			public const string ReportError = "ERR";
		}

		public const string StatusKey = "TYP=";

		[BusinessObjectEmptyStringTestExclude]
		public override ZString ACR_StatusMessage
		{
			get
			{
				return string.IsNullOrEmpty(base.ACR_StatusMessage)
					? ReportStatusMessage
					: base.ACR_StatusMessage;
			}
			set { base.ACR_StatusMessage = value; }
		}

		ZString ReportStatusMessage
		{
			get
			{
				var statusMessagesList = GetStatusMessageList().Where(x => x.ReportStatus == ACR_Status);
				var result = statusMessagesList.Where(x => (Category & x.Category) == x.Category).OrderByDescending(y => y.Category).FirstOrDefault()?.StatusMessage;
				result ??= string.Empty;
				return result;
			}
		}

		static IEnumerable<AccComplianceReportStatusMessage> GetStatusMessageList() =>
			new List<AccComplianceReportStatusMessage>()
			{
				// automatically "queued" when transaction is saved, and generated by the user: LIQ, ...
				new AccComplianceReportStatusMessage(Status.ReportCreated, category: AccComplianceReportCategory.Standard,
					Res.GetString("06CB16AB-8355-4491-4E86-99214E5ED7B6", "Report is waiting to be generated. Please click Generate to proceed.")),
				new AccComplianceReportStatusMessage(Status.ReportGenerated, category: AccComplianceReportCategory.Standard,
					Res.GetString("2b9d7334-b817-44dc-95f7-f11b216ca510", "Report has been generated.")),
				new AccComplianceReportStatusMessage(Status.ReportInvalidated, category: AccComplianceReportCategory.Standard,
					Res.GetString("e93a6586-f158-458a-ac10-6ef51f4f334a", "Report Invalidated by Transactions updated.")),
				// Status QUE and PQU should not appear for these types of reports

				// queued by CRQ service task and generated by the user: LBG, ZMD, ...
				new AccComplianceReportStatusMessage(Status.ReportCreated, category: AccComplianceReportCategory.QueuedByServiceTask,
					Res.GetString("423edacd-b599-4191-a0e0-4e024eb3627b", "Report is waiting to be queued. Please wait.")),
				new AccComplianceReportStatusMessage(Status.ReportPendingQueueing, category: AccComplianceReportCategory.QueuedByServiceTask,
					Res.GetString("0db879ef-9aa4-4532-b049-5140c6fa1308", "Report is being re-queued. Please wait.")),
				new AccComplianceReportStatusMessage(Status.ReportDataQueued, category: AccComplianceReportCategory.QueuedByServiceTask,
					Res.GetString("f523d6fb-f2a3-4eff-80db-98bfaf61c103", "Report data has been queued. Please click Generate to proceed.")),
				new AccComplianceReportStatusMessage(Status.ReportGenerated, category: AccComplianceReportCategory.QueuedByServiceTask,
					Res.GetString("2b9d7334-b817-44dc-95f7-f11b216ca510", "Report has been generated.")),
				new AccComplianceReportStatusMessage(Status.ReportInvalidated, category: AccComplianceReportCategory.QueuedByServiceTask,
					Res.GetString("e93a6586-f158-458a-ac10-6ef51f4f334a", "Report Invalidated by Transactions updated.")),

				// queued and generated by CRQ service task: IDE, FEC
				new AccComplianceReportStatusMessage(Status.ReportCreated, category: AccComplianceReportCategory.QueuedByServiceTask | AccComplianceReportCategory.OutputGeneratedByServiceTask,
					Res.GetString("423edacd-b599-4191-a0e0-4e024eb3627b", "Report is waiting to be queued. Please wait.")),
				new AccComplianceReportStatusMessage(Status.ReportDataQueued, category: AccComplianceReportCategory.QueuedByServiceTask | AccComplianceReportCategory.OutputGeneratedByServiceTask,
					Res.GetString("d4f984bf-b8c2-48c4-98d1-c1cc9928deb3", "Report is being generated. Please wait.")),
				new AccComplianceReportStatusMessage(Status.ReportPendingQueueing, category: AccComplianceReportCategory.QueuedByServiceTask | AccComplianceReportCategory.OutputGeneratedByServiceTask,
					Res.GetString("0db879ef-9aa4-4532-b049-5140c6fa1308", "Report is being re-queued. Please wait.")),
				new AccComplianceReportStatusMessage(Status.ReportGenerated, category: AccComplianceReportCategory.QueuedByServiceTask | AccComplianceReportCategory.OutputGeneratedByServiceTask,
					Res.GetString("4b379407-234b-4269-86d4-813090ca220a", "Report has been generated. Please check eDocs.")),

				// automatically "queued" when transaction is saved, and generated by the user and output generated by CRQ service task: JPK
				new AccComplianceReportStatusMessage(Status.ReportCreated, category: AccComplianceReportCategory.OutputGeneratedByServiceTask,
					Res.GetString("06CB16AB-8355-4491-4E86-99214E5ED7B6", "Report is waiting to be generated. Please click Generate to proceed.")),
				new AccComplianceReportStatusMessage(Status.ReportGenerated, category: AccComplianceReportCategory.OutputGeneratedByServiceTask,
					Res.GetString("6A340DF7-B0BE-4254-9252-13405B6C40C5", "Report has been generated. Please wait for output to be generated.")),
				new AccComplianceReportStatusMessage(Status.ReportOutputGenerated, category: AccComplianceReportCategory.OutputGeneratedByServiceTask,
					Res.GetString("279C2758-801C-40FA-94B4-24688F7FCBD2", "Output has been generated for this Report. Please check eDocs.")),
				// Status QUE and PQU should not appear for these types of reports
			};

		void ChangeAndApplyStatusCore(ZString status, Action<DbConnection> dataAction, Action beforeFactorySave = null, AccComplianceReportUsageCollectorContextData usageDataContext = null)
		{
			if (HasChanges)
			{
				ErrorReporter.ReportOnce("StatusOfUnsavedComplianceReport", "Cannot update Status on unsaved Compliance Report.");
				return;
			}

			if (!Lookups.ReportStatusList.ContainsCode(ACR_Status))
			{
				throw new ZCannotSaveException(GetComplianceReportIsNotValidErrorMessage(ACR_Status), "Cannot Save Compliance Report");
			}

			var connection = ((IDbConnected)Factory).Connection;
			var oldStatus = ACR_Status;
			ACR_Status = status;    // Need to set ACR_Status right here to protect with the Developer Error above

			beforeFactorySave?.Invoke();

			if (Factory.IsInSaveTransaction)
			{
				DoDataAction();
			}
			else
			{
				var action = new SaveInTransactionDelegateAction(connection, () =>
				{
					DoDataAction();
					return ChangedTableNames.Empty;
				});
				BusinessObjectFactory.SaveTogether(action, Factory);
			}

			void DoDataAction()
			{
				using (var accComplianceReportUsageCollector = ObjectFactory.Get<IAccComplianceReportUsageCollectorFactory>().GetAccComplianceReportUsageCollector(this))
				{
					dataAction(connection);
					Logs.AddNew(Events.StatusUpdated, StatusKey + ACR_Status);
					LoadReportTotals();    // Movement Amounts in GenAddOnColumns should be updated for Report types supporting GL Balance;

					accComplianceReportUsageCollector.AddChangedStatus(oldStatus, ACR_Status);
					accComplianceReportUsageCollector.AddLogonUser(Env.CurrentUser);
					if (usageDataContext != null)
					{
						accComplianceReportUsageCollector.AddContext(usageDataContext.Context);
						accComplianceReportUsageCollector.AddSessionId(usageDataContext.SessionId);
						accComplianceReportUsageCollector.AddAction(usageDataContext.Action);
					}
					else
					{
						accComplianceReportUsageCollector.AddContext(AccComplianceReportUsageCollectorContext.Cargowise);
					}
				}
			}
		}

		#endregion

		#region ACR_IsFinalised

		public ZBool ACR_IsFinalised
		{
			get
			{
				if (ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.GeneralLedgerData && ACR_Status == Status.ReportInvalidated)
				{
					return false;
				}
				var lastFinalised = Logs.MostRecentLogByEventTime(Events.StatusUpdated);
				return lastFinalised != null && lastFinalised.SL_Reference.Contains(StatusKey + Status.ReportFinalised, StringComparison.OrdinalIgnoreCase);
			}
		}

		public ZPropertyInfo ACR_IsFinalisedInfo
		{
			get { return GetZPropertyInfo(Schema.ACR_IsFinalised); }
		}

		#endregion

		#region LiquidazioneIvaPageNumberFrom

		public ZInt LiquidazioneIvaPageNumberFrom
		{
			get
			{
				LoadSummaryPageNumbers();
				return liquidazioneIvaPageNumberFrom;
			}
			set { liquidazioneIvaPageNumberFrom = value; }
		}
		ZInt liquidazioneIvaPageNumberFrom;

		#endregion

		#region LiquidazioneIvaPageNumberTo

		public ZInt LiquidazioneIvaPageNumberTo
		{
			get
			{
				LoadSummaryPageNumbers();
				return liquidazioneIvaPageNumberTo;
			}
			set { liquidazioneIvaPageNumberTo = value; }
		}
		ZInt liquidazioneIvaPageNumberTo;

		#endregion

		#region APVatSummaryPageNumberFrom

		public ZInt APVatSummaryPageNumberFrom
		{
			get
			{
				LoadSummaryPageNumbers();
				return apVatSummaryPageNumberFrom;
			}
			set { apVatSummaryPageNumberFrom = value; }
		}
		ZInt apVatSummaryPageNumberFrom;

		#endregion

		#region APVatSummaryPageNumberTo

		public ZInt APVatSummaryPageNumberTo
		{
			get
			{
				LoadSummaryPageNumbers();
				return apVatSummaryPageNumberTo;
			}
			set { apVatSummaryPageNumberTo = value; }
		}
		ZInt apVatSummaryPageNumberTo;

		#endregion

		#region ARVatSummaryPageNumberFrom

		public ZInt ARVatSummaryPageNumberFrom
		{
			get
			{
				LoadSummaryPageNumbers();
				return arVatSummaryPageNumberFrom;
			}
			set { arVatSummaryPageNumberFrom = value; }
		}
		ZInt arVatSummaryPageNumberFrom;

		#endregion

		#region ARVatSummaryPageNumberTo

		public ZInt ARVatSummaryPageNumberTo
		{
			get
			{
				LoadSummaryPageNumbers();
				return arVatSummaryPageNumberTo;
			}
			set { arVatSummaryPageNumberTo = value; }
		}
		ZInt arVatSummaryPageNumberTo;

		#endregion

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void LoadSummaryPageNumbers()
		{
			if (!LoadedSummaryPageNumbers)
			{
				var sql = string.Format(CultureInfo.InvariantCulture, @"SELECT
	ATR_PageFromAP AS {0}, ATR_PageToAP AS {1},
	ATR_PageFromAR AS {2}, ATR_PageToAR AS {3},
	ATR_PageFromLiq AS {4}, ATR_PageToLiq AS {5}
	FROM dbo.AccTaxReturn
	WHERE ATR_ACR_ComplianceReport = @ComplianceReportPK "
				, Schema.APVatSummaryPageNumberFrom, Schema.APVatSummaryPageNumberTo
				, Schema.ARVatSummaryPageNumberFrom, Schema.ARVatSummaryPageNumberTo
				, Schema.LiquidazioneIvaPageNumberFrom, Schema.LiquidazioneIvaPageNumberTo);

				using (var command = ((IDbConnected)Factory).Connection.Command(sql))
				{
					command.AddParameterBasedOnDbColumn("@ComplianceReportPK", PK.ToGuid(), AccComplianceReportSchema.PK);
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							APVatSummaryPageNumberFrom = new ZInt(reader[Schema.APVatSummaryPageNumberFrom]);
							APVatSummaryPageNumberTo = new ZInt(reader[Schema.APVatSummaryPageNumberTo]);
							ARVatSummaryPageNumberFrom = new ZInt(reader[Schema.ARVatSummaryPageNumberFrom]);
							ARVatSummaryPageNumberTo = new ZInt(reader[Schema.ARVatSummaryPageNumberTo]);
							LiquidazioneIvaPageNumberFrom = new ZInt(reader[Schema.LiquidazioneIvaPageNumberFrom]);
							LiquidazioneIvaPageNumberTo = new ZInt(reader[Schema.LiquidazioneIvaPageNumberTo]);

							LoadedSummaryPageNumbers = true;
						}
					}
				}
			}
		}
		bool LoadedSummaryPageNumbers;

		public AccComplianceReportLineCollectionBase<AccComplianceReportSummaryLine> ReportSummaryLines
		{
			get
			{
				if (reportSummaryLines == null)
				{
					LoadSummaryReportLines();
				}
				return reportSummaryLines;
			}
		}
		AccComplianceReportLineCollectionBase<AccComplianceReportSummaryLine> reportSummaryLines;

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void LoadSummaryReportLines()
		{
			LIQSubmissionData computedByCW1 = new LIQSubmissionData(Factory);

			var helper = new LIQSubmissionDataHelper(this);
			helper.SetValues(computedByCW1);

			reportSummaryLines = new AccComplianceReportLineCollectionBase<AccComplianceReportSummaryLine>(this);

			var sql = string.Format(CultureInfo.InvariantCulture, @"SELECT A.ATC_ColumnName AS {0}
	,A.ATC_Amount AS {1}
	,A.ATC_Comment AS {2}
	,A.ATC_ReasonCode AS {3}
	FROM dbo.AccTaxReturn
	LEFT JOIN dbo.AccTaxReturnColumn AS A ON A.ATC_ATR_AccTaxReturn = AccTaxReturn.ATR_PK and A.ATC_GroupCode = '{4}'
	where 
	AccTaxReturn.ATR_ACR_ComplianceReport = @ComplianceReportPK "
			, AccComplianceReportSummaryLine.Schema.ColumnName
			, AccComplianceReportSummaryLine.Schema.Adjustment
			, AccComplianceReportSummaryLine.Schema.Comment
			, AccComplianceReportSummaryLine.Schema.ReasonCode
			, LIQSubmissionDataColumns.AdjustedAmountsGroup
			);

			using (var command = ((IDbConnected)Factory).Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@ComplianceReportPK", PK.ToGuid(), AccComplianceReportSchema.PK);
				using (var reader = command.ExecuteReader())
				{
					var table = ((INeedTable)reportSummaryLines).Table;
					while (reader.Read())
					{
						var row = table.NewRow();

						var columnName = (string)(reader[AccComplianceReportSummaryLine.Schema.ColumnName]);
						var adjustment = (decimal)(reader[AccComplianceReportSummaryLine.Schema.Adjustment]);
						var reasonCode = (string)(reader[AccComplianceReportSummaryLine.Schema.ReasonCode]);
						var comment = (string)(reader[AccComplianceReportSummaryLine.Schema.Comment]);

						(decimal value, string columnDescription, int columnNumber) = DecodeValues(columnName, computedByCW1);

						row[AccComplianceReportSummaryLine.Schema.ColumnNumber] = columnNumber;
						row[AccComplianceReportSummaryLine.Schema.ColumnName] = columnName;
						row[AccComplianceReportSummaryLine.Schema.ColumnDescription] = columnDescription;
						row[AccComplianceReportSummaryLine.Schema.Adjustment] = adjustment;
						row[AccComplianceReportSummaryLine.Schema.ReasonCode] = reasonCode;
						row[AccComplianceReportSummaryLine.Schema.Comment] = comment;
						row[AccComplianceReportSummaryLine.Schema.Value] = value;
						row[AccComplianceReportSummaryLine.Schema.Total] = value + adjustment;

						table.Rows.Add(row);
						row.AcceptChanges();

						var result = new AccComplianceReportSummaryLine(Factory, row);
						reportSummaryLines.Add(result);
					}
				}
			}
		}

		(decimal, string, int) DecodeValues(string row, LIQSubmissionData computedByCW1)
		{
			decimal returnValue = 0m;
			string returnDescription = "";
			int returnNumber = 0;

			switch (row)
			{
				case LIQSubmissionDataColumns.TotalVatBaseReceivables:
					returnValue = computedByCW1.Box1_TotalVatBaseReceivables;
					returnDescription = ResString.GetMultilingualString("BA7DB7BD-548D-4342-939B-1213D47CF9C4", "Total Vat Base Receivables");
					returnNumber = 1;
					break;
				case LIQSubmissionDataColumns.TotalVatReceivables:
					returnValue = computedByCW1.Box2_TotalVatReceivables;
					returnDescription = ResString.GetMultilingualString("BAD17434-73CD-458D-AE11-E9CC0DE1AA91", "Total Vat Receivables");
					returnNumber = 2;
					break;
				case LIQSubmissionDataColumns.TotalVatBasePayables:
					returnValue = computedByCW1.Box3_TotalVatBasePayables;
					returnDescription = ResString.GetMultilingualString("0B2CDA9B-0834-46D2-91B9-F686577F8563", "Total Vat Base Payables");
					returnNumber = 3;
					break;
				case LIQSubmissionDataColumns.TotalVatPayablesRecoverable:
					returnValue = computedByCW1.Box4_TotalVatPayablesRecoverable;
					returnDescription = ResString.GetMultilingualString("E4CC06B8-5348-4954-B1FC-6DFC77B4E797", "Total Vat Payables Recoverable");
					returnNumber = 4;
					break;
				case LIQSubmissionDataColumns.TotalVatPayablesNotRecoverable:
					returnValue = computedByCW1.Box5_TotalVatPayablesNotRecoverable;
					returnDescription = ResString.GetMultilingualString("12E267D3-8FB7-4629-80FC-5798690EC875", "Total Vat Payables Not Recoverable");
					returnNumber = 5;
					break;
				case LIQSubmissionDataColumns.VatBalanceReceivablesAndPayables:
					returnValue = computedByCW1.Box6_VatBalanceReceivablesAndPayables;
					returnDescription = ResString.GetMultilingualString("68D77942-149F-4594-862C-658AB4ED7A0F", "Vat Balance Receivables and Payables (Box 2 + Box 4)");
					returnNumber = 6;
					break;
				case LIQSubmissionDataColumns.BalancePreviousPeriod:
					returnValue = computedByCW1.Box7_BalancePreviousPeriod;
					returnDescription = ResString.GetMultilingualString("307822D9-3AE3-4433-9C12-92844D3AE02A", "Balance Previous Period");
					returnNumber = 7;
					break;
				case LIQSubmissionDataColumns.TotalBalance:
					returnValue = computedByCW1.Box8_TotalBalance;
					returnDescription = ResString.GetMultilingualString("9808CE94-C68F-4E0B-A385-52285536762E", "Total Balance (Box 6 + Box 7)");
					returnNumber = 8;
					break;
			}

			return (returnValue, returnDescription, returnNumber);
		}

		#region SupportsReQueue

		public ZBool SupportsReQueue => (ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.AllTransactions
				|| ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.GeneralLedgerData
				|| ReportLineGrouping == ReportLineGroupingListCodes.TransactionPayments
				|| ReportLineGrouping == ReportLineGroupingListCodes.PaymentTimesSmallBusinessReportable
				|| ReportLineGrouping == ReportLineGroupingListCodes.PaymentTimesAll
				|| ReportLineGrouping == ReportLineGroupingListCodes.TransactionHeaderAndReportSubCode
			) && !ACR_IsFinalised;

		#endregion

		#region ReportConfiguration

		public ZString ReportCountryCode => ReportConfiguration?.Country ?? ZString.Empty;

		public ZString ReportTypeDescription => ReportConfiguration?.ReportTitle ?? ZString.Empty;

		public ZString ReportBaseTablePrefix => ReportConfiguration?.ReportBaseTablePrefix ?? ZString.Empty;

		public ZString ReportLineGrouping => ReportConfiguration?.ReportLineGrouping ?? ZString.Empty;

		public ZString ReportLineOrdering => ReportConfiguration?.ReportLineOrdering ?? ZString.Empty;

		public ZString GoodsServiceType => ReportConfiguration?.GoodsServiceType ?? ZString.Empty;

		public ZString TaxRegistrationType => ReportConfiguration?.TaxRegistrationType ?? ZString.Empty;

		public CodeDescriptionPairList TaxRegistrationTypeList => ReportConfiguration?.Lookups.TaxRegistrationTypeList ?? new CodeDescriptionPairList();

		public bool IsUsingGLDTablePrefix => ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.GeneralLedgerData;

		internal ZBool IncludeQueuedForPreviousPeriodForGenerating => IncludeQueuedForPreviousPeriod && !IsFirstReport();
		internal ZBool IncludeQueuedForPreviousPeriodForDeleting => IncludeQueuedForPreviousPeriod;
		public ZBool HasPreviousPeriodData => IncludeQueuedForPreviousPeriod && ReportLinesPreviousPeriod.Any();

		DataTable SetParameters(DbCommand command)
		{
			command.AddParameterBasedOnDbColumn("@PK", PK.ToGuid(), AccComplianceReportSchema.PK);
			command.AddParameterBasedOnDbColumn("@ReportTablePrefix", ReportConfiguration?.ReportBaseTablePrefix.ToString() ?? string.Empty, CargoWise.Schema.Schema.GenericStringSchemaColumn);
			command.AddParameterBasedOnDbColumn("@ReportDateFrom", ACR_DateFrom.ToDateTime(), CargoWise.Schema.Schema.GenericDateTimeColumn);
			command.AddParameterBasedOnDbColumn("@ReportDateTo_PlusOneDay", ACR_DateTo.AddDays(1).ToDateTime(), CargoWise.Schema.Schema.GenericDateTimeColumn);
			command.AddParameterBasedOnDbColumn("@ReportCompanyPK", ACR_GC_Company.ToGuid(), AccComplianceReportSchema.ACR_GC_Company);
			command.AddParameterBasedOnDbColumn("@RegType", ReportConfiguration?.TaxRegistrationType.ToString() ?? string.Empty, CargoWise.Schema.Schema.GenericStringSchemaColumn);
			command.AddParameterBasedOnDbColumn("@ReportCountry", ReportConfiguration?.Country.ToString() ?? string.Empty, CargoWise.Schema.Schema.GenericStringSchemaColumn);
			command.AddParameterBasedOnDbColumn("@RepCountryRegistrationCodeType", ReportConfiguration?.RepCountryRegistrationCode.ToString() ?? string.Empty, CargoWise.Schema.Schema.GenericStringSchemaColumn);
			command.AddParameterBasedOnDbColumn("@GS", ReportConfiguration?.GoodsServiceType.ToString() ?? string.Empty, CargoWise.Schema.Schema.GenericStringSchemaColumn);
			command.AddParameterBasedOnDbColumn("@RoundingType", ReportConfiguration?.ReportAmountsRoundingType.ToString() ?? string.Empty, CargoWise.Schema.Schema.GenericStringSchemaColumn);
			command.AddParameterBasedOnDbColumn("@Rounding", ReportConfiguration?.ReportAmountsRoundingTruncating.ToString() ?? string.Empty, CargoWise.Schema.Schema.GenericIntSchemaColumn);

			var settings = ReportConfiguration?.Settings?.Cast<ComplianceReportConfigurationSetting>();
			var isCostOnly = !settings.IsNullOrEmpty() && settings.All(s => s.LedgerType == LedgerTypes.AccountsPayable);
			command.AddParameterBasedOnDbColumn("@ReverseSign", isCostOnly ? 1 : 0, CargoWise.Schema.Schema.GenericIntSchemaColumn);

			var mappingTable = controlAccountAndReportSubCodeMapping.AccountPkToSubCodesTable;

			if (((ICurrentDbControl)command.DbConnection).InitialDatabase == Db.DatabaseName)
			{
				command.AddTableValuedParameter("@SubCodeToGLAccountMapping", "dbo.TVP_CodeToGuidMapping", mappingTable);
			}

			return mappingTable;
		}

		internal ZBool IncludeQueuedForPreviousPeriod => ReportConfiguration?.IncludeQueuedForPreviousPeriod ?? ZBool.False;
		internal ZString ReportCode => ReportConfiguration?.ReportCode ?? ZString.Empty;
		internal ZString ReportAmountsRoundingType => ReportConfiguration?.ReportAmountsRoundingType ?? ZString.Empty;
		internal ZString ReportPeriodicity => ReportConfiguration?.ReportPeriodicity ?? ZString.Empty;
		internal ZString ReportCountryRegistrationCode => ReportConfiguration?.RepCountryRegistrationCode ?? ZString.Empty;
		internal ZInt ReportAmountsRoundingTruncating => ReportConfiguration?.ReportAmountsRoundingTruncating ?? ZInt.Zero;
		internal ZString ReportAmountThresholdLevel => ReportConfiguration?.AmountThresholdLevel ?? ZString.Empty;
		internal ZDecimal ReportExTaxAmountThreshold => ReportConfiguration?.ExTaxAmountThreshold ?? ZDecimal.Zero;
		internal ZDecimal ReportTaxAmountThreshold => ReportConfiguration?.TaxAmountThreshold ?? ZDecimal.Zero;
		internal ZGuid ReportRecipientOrgPK => ReportConfiguration?.RecipientOrgPK ?? ZGuid.Empty;

		internal bool IsFirstReport() => GetPreviousReport() == null;

		public bool IsPreviousReportFinalized()
		{
			var previousReport = GetPreviousReport();
			return previousReport == null || previousReport.ACR_Status == AccComplianceReport.Status.ReportFinalised;
		}

		public AccComplianceReport GetPreviousReport()
		{
			AccComplianceReport result = null;

			if (!ACR_GC_Company.IsEmpty && !ACR_ReportType.IsEmpty && !ACR_DateFrom.IsEmpty)
			{
				var filter = new ZQuery(AccComplianceReportSchema.ACR_GC_Company, ACR_GC_Company);
				filter.AddToFilter(AccComplianceReportSchema.ACR_ReportType, ACR_ReportType);
				filter.AddToFilter(AccComplianceReportSchema.ACR_DateTo, SQLComparisonOperator.LessThan, ACR_DateFrom);
				filter.OrderBy = AccComplianceReportSchema.ACR_DateTo.Name + OrderByClause.Descending;

				result = Factory.LoadTop1<AccComplianceReport>(filter);
			}

			return result;
		}

		public ZString[] GetComplianceSubTypes() => ReportConfiguration?.Settings.Cast<ComplianceReportConfigurationSetting>().Select(x => x.ComplianceSubType).ToArray() ?? Array.Empty<ZString>();

		internal ComplianceReportConfiguration ReportConfiguration
		{
			get
			{
				return AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration
					.GetValueWithoutFallback(ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty).Cast<ComplianceReportConfiguration>()
					.FirstOrDefault(x => x.ReportCode == ACR_ReportType && x.Country == Company.GC_RN_NKCountryCode);
			}
		}

		#endregion

		#region Report Audit Details

		public ZDateTime ACR_Calc_CreatedTime
		{
			get
			{
				var lastEvent = Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem);
				return lastEvent != null ? lastEvent.SL_EventTime : ZDateTime.Empty;
			}
		}

		public ZString ACR_Calc_CreatingUser
		{
			get
			{
				var lastEvent = Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem);
				return lastEvent != null ? lastEvent.SL_GS_NKUser : ZString.Empty;
			}
		}

		public ZDateTime ACR_Calc_LastEditedTime
		{
			get
			{
				var lastEvent = Logs.MostRecentLogByEventTime(Events.EditedARecord);
				return lastEvent != null ? lastEvent.SL_EventTime : ZDateTime.Empty;
			}
		}

		public ZString ACR_Calc_LastEditUser
		{
			get
			{
				var lastEvent = Logs.MostRecentLogByEventTime(Events.EditedARecord);
				return lastEvent != null ? lastEvent.SL_GS_NKUser : ZString.Empty;
			}
		}

		#endregion

		#region Loading NonPersistentBusinessObjectCollection

		static internal void LoadNonPersistentBusinessObjectCollection<T>(ref AccComplianceReportLineCollectionBase<T> collection, DbCommand loadCommand, Action<DataTable> additionalAction = null) where T : NonPersistentBusinessObject, new()
		{
			var ctor = typeof(T).GetConstructor(new[] { typeof(BusinessObjectFactory), typeof(DataRow) })
					   ?? throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Type {0} must have a constructor taking BusinessObjectFactory and DataRow arguments.", typeof(T).FullName));

			const string pk = "PK";
			const string columnNameConst = "ColumnName";
			const string dataTypeConst = "DataType";

			DataTable temp = null;

			try
			{
				using (var reader = loadCommand.ExecuteReader())
				{
					var schema = reader.GetSchemaTable();
					temp = new DataTable();
					temp.Locale = CultureInfo.InvariantCulture;

					if (schema != null)
					{
						temp.Columns.Add(pk, typeof(Guid));
						foreach (DataRow schemaRow in schema.Rows)
						{
							var columnName = (string)schemaRow[columnNameConst];
							if (!temp.Columns.Contains(columnName))
							{
								Type dataType = (Type)schemaRow[dataTypeConst];
								temp.Columns.Add(columnName, dataType);
							}
						}
					}

					while (reader.Read())
					{
						var row = temp.NewRow();
						row[pk] = Guid.NewGuid();

						for (int i = 0; i < reader.FieldCount; i++)
						{
							var columnName = reader.GetName(i);
							var data = reader[i];
							row[columnName] = data == null ? DBNull.Value : GetDataTrimmedIfNeeded(data);
						}

						temp.Rows.Add(row);
						row.AcceptChanges();

						var item = (T)ctor.Invoke(new object[] { collection.Factory, row });
						collection.Add(item);
					}
				}

				if (additionalAction != null)
				{
					additionalAction(temp);
				}

				temp = null;
			}
			finally
			{
				if (temp != null)
				{
					temp.Dispose();
				}
			}
		}

		#endregion

		#region Logs

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);

				var taxReturn = MTDSubmissionDataColumnsAdapter.LoadAccTaxReturn(this);
				if (taxReturn != null)
				{
					result.Add(taxReturn);
				}

				return result.ToArray();
			}
		}

		#endregion

		#region ReportLines

		protected void CheckIfLoadingLimitApplied()
		{
			if (ApplyLinesLoadingLimit && ConfirmLinesLoadingLimit != null)
			{
				var confirm = new BoolResponseEventArgs(false);
				ConfirmLinesLoadingLimit.Invoke(this, confirm);
				ApplyLinesLoadingLimit = confirm.Response;
			}
		}

		public AccComplianceReportLineCollectionBase<AccComplianceReportLine> ReportLines
		{
			get
			{
				CheckIfLoadingLimitApplied();

				if (reportLines == null)
				{
					LoadReportLines(ref reportLines, GetReportLinesLoadSql, ((IDbConnected)Factory).Connection);
				}

				return reportLines;
			}
		}
		AccComplianceReportLineCollectionBase<AccComplianceReportLine> reportLines;

		protected void LoadReportLines(ref AccComplianceReportLineCollectionBase<AccComplianceReportLine> reportLines, Func<string> getSql, DbConnection connection)
		{
			LoadLines(ref reportLines, getSql, connection,
				(dataTable) =>
				{
					LoadOpeningBalancesAndLineNumOffset();

					if (dataTable != null && LoadedOpeningBalancesAndLineNumOffset && !LineNumOffset.IsEmpty)
					{
						foreach (DataRow row in dataTable.Rows)
						{
							row[AccComplianceReportLine.Schema.ACL_ReportSequence] = LineNumOffset + new ZInt(row[AccComplianceReportLine.Schema.ACL_ReportSequence]);
							row.AcceptChanges();
						}
					}
				});
		}

		string GetReportLinesLoadSql()
		{
			var columnNames = GetReportLinesColumnNames(reportLines);
			var getLinesTVF = GetComplianceReportLinesBasedOnGroupByAndTablePrefix();

			return FormattableString.Invariant($@"SELECT {(ApplyLinesLoadingLimit ? $"TOP {MaxReportLinesToLoad} " : string.Empty)}{columnNames}
FROM {getLinesTVF} (@PK, @ReportTablePrefix, @ReportDateFrom, @ReportDateTo_PlusOneDay, @RegType, @ReportCountry, @RepCountryRegistrationCodeType, @SubCodeToGLAccountMapping, @GS, @RoundingType, @Rounding, @ReverseSign)
ORDER BY ACL_ReportSequence, AH_TransactionType, AG_AccountNum, GB_Code, GE_Code, SIGN(GeneralLedgerAmount) OPTION(RECOMPILE)");
		}

		void LoadLines<T>(ref AccComplianceReportLineCollectionBase<T> lines, Func<string> getSql, DbConnection connection, Action<DataTable> additionalAction = null) where T : AccComplianceReportLineBase, new()
		{
			if (lines == null)
			{
				lines = new AccComplianceReportLineCollectionBase<T>(this);
			}
			else
			{
				lines.RemoveAndDeleteAll();
			}

			if (IsInDatabase && ReportConfiguration != null)
			{
				using (var command = GetLoadLinesCommand(getSql(), connection))
				{
					LoadNonPersistentBusinessObjectCollection(ref lines, command, additionalAction);
				}
			}
		}

		public virtual AccComplianceReportLineCollectionBase<AccComplianceReportLine> ReportLinesExport => ReportLines;

		protected string GetReportLinesColumnNames(AccComplianceReportLineCollectionBase<AccComplianceReportLine> reportLines)
		{
			var columnsListBuilder = new ZStringBuilder();
			reportLines.GetColumnNames().ForEach(x => columnsListBuilder.Append(GetSqlForColumnName(x)));
			var columnNames = columnsListBuilder.ToStringWithDelimiterBetweenAppends(",");
			return columnNames;
		}

		public ReportLinesCollectionView ReportLinesPreviousPeriod => reportLinesPreviousPeriod ?? (reportLinesPreviousPeriod =
			new ReportLinesCollectionView(ReportLines, IncludeQueuedForPreviousPeriod ?
				(reportLine) => reportLine.PostDate < ACR_DateFrom : null));

		ReportLinesCollectionView reportLinesPreviousPeriod;

		public ReportLinesCollectionView ReportLinesCurrentPeriod => reportLinesCurrentPeriod ?? (reportLinesCurrentPeriod =
			new ReportLinesCollectionView(ReportLines, IncludeQueuedForPreviousPeriod ?
				(reportLine) => reportLine.PostDate >= ACR_DateFrom : (reportLine) => true));

		ReportLinesCollectionView reportLinesCurrentPeriod;

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		DbCommand GetLoadLinesCommand(string sql, DbConnection connection = null)
		{
			DbCommand temp = null;
			DbCommand result = null;
			DataTable mappingTable = null;

			try
			{
				CheckAndReportErrorOnIncreasedTimeout();
				temp = (connection ?? ((IDbConnected)Factory).Connection).Command(sql, CommandTimeout);

				mappingTable = SetParameters(temp);

				result = temp;
				temp = null;
				mappingTable = null;
			}
			finally
			{
				if (mappingTable != null)
				{
					mappingTable.Dispose();
				}
				if (temp != null)
				{
					temp.Dispose();
				}
			}
			return result;
		}

		static object GetDataTrimmedIfNeeded(object data)
		{
			var result = data as string;
			return result != null ? result.TrimEnd(' ') : data;
		}

		#endregion

		#region MaxLineNum

		public ZInt MaxLineNum
		{
			get
			{
				var addOn = AddOnColumns.FirstOrDefault(x => x.XA_Name == Schema.MaxLineNum);
				return addOn != null && !addOn.XA_Data.IsEmpty ? new ZInt(addOn.XA_Data) : ZInt.Zero;
			}
			private set
			{
				CreateOrAmendAddOnColumn(Schema.MaxLineNum, AddOnColumnDataType.Codes.Integer, value);
			}
		}

		#endregion

		#region NextProcessingStepFromDate

		public ZDate NextProcessingStepFromDate
		{
			get
			{
				var addOn = AddOnColumns.FirstOrDefault(x => x.XA_Name == Schema.NextProcessingStepFromDate);
				return addOn != null && !addOn.XA_Data.IsEmpty ? new ZDate(addOn.XA_Data) : ACR_DateFrom;
			}
			set
			{
				CreateOrAmendAddOnColumn(Schema.NextProcessingStepFromDate, AddOnColumnDataType.Codes.Date, value);
			}
		}

		#endregion

		#region LineNumOffset

		public ZInt LineNumOffset
		{
			get
			{
				LoadOpeningBalancesAndLineNumOffset();
				return lineNumOffset;
			}
			private set { lineNumOffset = value; }
		}
		ZInt lineNumOffset;

		#endregion

		#region SupportsGLBalance

		public ZBool SupportsGLBalance => ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.AllTransactions || ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.GeneralLedgerData;

		#endregion

		#region SupportsGLBalanceDetails

		public ZBool SupportsGLBalanceDetails => SupportsDayBook
			&& ReportConfiguration.ReportPeriodicity == ReportPeriodicityCodes.AccountingPeriod;

		#endregion

		#region SpecialEffects

		public ZBool DoesNotShowReportLines => SupportsIDEA || SupportsFEC;
		public ZBool GeneratedByCRQServiceTask => SupportsIDEA || SupportsFEC;
		public bool IsServiceTaskQueuedReport => ReportConfiguration?.IsServiceTaskQueuingConfig() ?? false;
		public ZBool IsServiceTaskOutputGeneratingReport => ReportConfiguration?.IsServiceTaskOutputGeneratingConfig() ?? false;

		public AccComplianceReportCategory Category {
			get
			{
				var combinedCategories = (IsServiceTaskQueuedReport ? AccComplianceReportCategory.QueuedByServiceTask : AccComplianceReportCategory.Undefined)
						| (IsServiceTaskOutputGeneratingReport ? AccComplianceReportCategory.OutputGeneratedByServiceTask : AccComplianceReportCategory.Undefined);
				if (combinedCategories == AccComplianceReportCategory.Undefined)
				{
					combinedCategories = AccComplianceReportCategory.Standard;
				}

				return combinedCategories;
			}
		}

		#endregion

		#region SupportsDayBook

		public ZBool SupportsDayBook => (ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.AllTransactions || IsUsingGLDTablePrefix)
			&& (ReportLineGrouping == ReportLineGroupingListCodes.DayBook
			|| ReportLineGrouping == ReportLineGroupingListCodes.DayBookWithoutGrouping
			|| ReportLineGrouping == ReportLineGroupingListCodes.DayBookWithPresentation);

		#endregion

		#region SAFT XML

		public ZBool SupportsSAFT => (ACR_ReportType == ReportTypes.SAFT || ACR_ReportType == ReportTypes.SAFTOnlyTransactions) && AccountingUtils.DoesCountrySupportSAFTGeneration;

		public ZBool IsTransactionLinesBased => ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.TransactionLine;

		public ZBool SupportsSAFTXmlGeneration => SupportsSAFT
		&& (ACR_Status == Status.ReportGenerated || ACR_Status == Status.ReportFinalised);

		#endregion

		#region Esterometro XML

		public ZBool SupportsEsterometro => ACR_ReportType == ReportTypes.Esterometro
			&& Company.GC_RN_NKCountryCode == CountryCodes.Italy;

		public ZBool SupportsEsterometroXmlGeneration => SupportsEsterometro
			&& (ACR_Status == Status.ReportGenerated || ACR_Status == Status.ReportFinalised);

		#endregion

		#region Liquidazione IVA

		public ZBool SupportsLiquidazioneIVA => ACR_ReportType == ReportTypes.LiquidazioneIVA
			&& Company.GC_RN_NKCountryCode == CountryCodes.Italy
			&& ACR_Status == Status.ReportGenerated;

		#endregion

		#region MTD

		public ZBool SupportsMTD => Company.GC_RN_NKCountryCode == CountryCodes.UnitedKingdom
			&& ACR_ReportType == ComplianceReportTypes.MakeTaxDigitalReportType
			&& (ACR_Status == Status.ReportGenerated || ACR_Status == Status.ReportFinalised)
			&& Environment.Env.Security.FinalizeComplianceReport.IsAllowed;

		#endregion

		#region TPAR

		public ZBool SupportsTPAR => Company.GC_RN_NKCountryCode == CountryCodes.Australia
			&& ACR_ReportType == ComplianceReportTypes.TaxablePaymentsAnnualReportType
			&& (ACR_Status == Status.ReportGenerated || ACR_Status == Status.ReportFinalised)
			&& Environment.Env.Security.FinalizeComplianceReport.IsAllowed;

		#endregion

		#region ZMGermany

		public ZBool SupportsZMGermany => (ReportConfiguration?.IsZMGermanyConfig() ?? false)
			&& (ACR_Status == Status.ReportGenerated || ACR_Status == Status.ReportFinalised);

		#endregion

		#region IDEA

		public ZBool SupportsIDEA => ACR_ReportType == ReportTypes.IDEA
			&& Company.GC_RN_NKCountryCode == CountryCodes.Germany;

		#endregion

		#region FEC

		public ZBool SupportsFEC => ACR_ReportType == ReportTypes.FEC
			&& Company.GC_RN_NKCountryCode == CountryCodes.France;

		#endregion

		#region UsageCollector

		public class AccComplianceReportUsageCollectorContextData
		{
			public AccComplianceReportUsageCollectorContext Context { get; set; }
			public Guid SessionId { get; set; }
			public AccComplianceReportUsageCollectorAction Action { get; set; }
		}

		#endregion

		#region DeleteEDocs

		void DeleteEDocsIfRequired()
		{
			if (GeneratedByCRQServiceTask)
			{
				var docManager = this.DocManagerInfo();
				docManager.SetupEDocsFactoryToBeSavedWithMainFactory(true);
				var documentsToDelete = new List<StorageDocsBase>(docManager.AllEDocs.Cast<StorageDocsBase>().Where(d => d.SC_IsSystemGenerated).ToList());     // SC_IsSystemGenerated is set in AttachFileToEdoc()

				foreach (var edoc in documentsToDelete)
				{
					var reference = DocumentLogReferenceHelper.GetReference(edoc, Events.DocumentDeletedPermanently);
					var isInDatabase = edoc.IsInDatabase;
					var parentMain = edoc.ParentMain;
					edoc.Delete();
					AddLogForDocument(reference, isInDatabase, parentMain, Events.DocumentDeletedPermanently);
				}
			}
		}

		void AddLogForDocument(string reference, bool isInDatabase, StorageMain parentMain, Event logEvent)
		{
			if (isInDatabase)
			{
				parentMain.Logs.AddNew(logEvent, reference);
				var parent = parentMain.DocumentOwner as EnterpriseBusinessObject;
				if (parent != null)
				{
					parent.Logs.AddNew(logEvent, reference);
				}
				var storageMain = this.DocManagerInfo().MasterFactory.RetrieveExistingOrCreateStorageMain(this, DocManagerCodes.ComplianceReport);
				if (parentMain != storageMain)
				{
					parentMain.EnableAddEditedARecordEventLogForDocumentOwner();
				}
			}
		}

		#endregion

		#region ImportABNs

		public ZBool SupportsImportABNs => Company.GC_RN_NKCountryCode == CountryCodes.Australia
			&& (ACR_ReportType == ComplianceReportTypes.PaymentTimesSmallBusinessReportType || ACR_ReportType == ComplianceReportTypes.PaymentTimesSmallBusiness2024ReportType)
			&& IsInDatabase
			&& (ACR_Status == Status.ReportCreated || ACR_Status == Status.ReportDataQueued || ACR_Status == Status.ReportPendingQueueing);

		#endregion

		#region VAT File

		public ZBool SupportsExportVATFile => ReportBaseTablePrefix == AccComplianceDocumentHeaderSchema.Constants.Prefix
			&& Company.GC_RN_NKCountryCode == CountryCodes.Taiwan
			&& (ACR_Status == Status.ReportGenerated || ACR_Status == Status.ReportFinalised);

		#endregion

		#region OpenFormatOnlyTransactions

		public ZBool SupportsExportOpenFormatFile => ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.TransactionLine
			&& Company.GC_RN_NKCountryCode == CountryCodes.Israel
			&& (ACR_Status == Status.ReportGenerated || ACR_Status == Status.ReportFinalised);

		#endregion

		#region PTRS

		public ZBool SupportsPTRSSmallBusiness => SupportsReportTypeForCountry(CountryCodes.Australia, ComplianceReportTypes.PaymentTimesSmallBusinessReportType);

		public ZBool SupportsPTRSAllPayments => SupportsReportTypeForCountry(CountryCodes.Australia, ComplianceReportTypes.PaymentTimesAllPaymentsReportType);

		public ZBool SupportsPTRS2024SmallBusiness => SupportsReportTypeForCountry(CountryCodes.Australia, ComplianceReportTypes.PaymentTimesSmallBusiness2024ReportType);

		public ZBool SupportsPTRS2024AllPayments => SupportsReportTypeForCountry(CountryCodes.Australia, ComplianceReportTypes.PaymentTimesAllPayments2024ReportType);

		public ZBool IsPTRS2024SmallBusiness => IsReportTypeForCountry(CountryCodes.Australia, ComplianceReportTypes.PaymentTimesSmallBusiness2024ReportType);

		public ZBool IsPTRS2024AllPayments => IsReportTypeForCountry(CountryCodes.Australia, ComplianceReportTypes.PaymentTimesAllPayments2024ReportType);

		ZBool IsReportTypeForCountry(string countryCode, params string[] reportTypeCodes)
			 => Company.GC_RN_NKCountryCode == countryCode
			&& reportTypeCodes.Contains(ACR_ReportType.ToString());

		ZBool SupportsReportTypeForCountry(string countryCode, params string[] reportTypeCodes)
			=> IsReportTypeForCountry(countryCode, reportTypeCodes)
			&& (ACR_Status == Status.ReportGenerated || ACR_Status == Status.ReportFinalised)
			&& Environment.Env.Security.FinalizeComplianceReport.IsAllowed;

		#endregion

		#region Pre-Calculated columns packed in ACL_ReportSubCode

		internal bool HasPreCalculatedAmount => SupportsPTRSSmallBusiness || SupportsPTRSAllPayments
			|| SupportsPTRS2024SmallBusiness || SupportsPTRS2024AllPayments;

		#endregion

		#region GL Opening Balance

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal GLOpeningBalanceDR
		{
			get
			{
				LoadOpeningBalancesAndLineNumOffset();
				return glOpeningBalanceDR;
			}
			private set { glOpeningBalanceDR = value; }
		}
		ZDecimal glOpeningBalanceDR;

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal GLOpeningBalanceCR
		{
			get
			{
				LoadOpeningBalancesAndLineNumOffset();
				return glOpeningBalanceCR;
			}
			private set { glOpeningBalanceCR = value; }
		}
		ZDecimal glOpeningBalanceCR;

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void LoadOpeningBalancesAndLineNumOffset()
		{
			if (IsInDatabase && SupportsGLBalance && !LoadedOpeningBalancesAndLineNumOffset)
			{
				var sql = string.Format(CultureInfo.InvariantCulture, @"SELECT 
	{0} = SUM(CASE WHEN XA_Name = '{0}' THEN CONVERT(INT, XA_Data) ELSE 0 END),
	{1} = SUM(CASE WHEN XA_Name = '{1}' THEN CONVERT(MONEY, REPLACE(XA_Data,',','.')) ELSE 0 END),
	{2} = SUM(CASE WHEN XA_Name = '{2}' THEN CONVERT(MONEY, REPLACE(XA_Data,',','.')) ELSE 0 END)
FROM dbo.GenAddOnColumn WHERE XA_ParentID IN (
	SELECT ACR_PK 
	FROM dbo.AccComplianceReport
	WHERE ACR_GC_Company = @CompanyPK AND
	(@BranchPK IS NULL AND ACR_GB_Branch IS NULL OR ACR_GB_Branch = @BranchPK) AND 
	ACR_ReportType = @ReportType AND 
	ACR_DateFrom >= (
		SELECT StartDate = MIN(AM_StartDate) 
		FROM dbo.AccPeriodManagement
		WHERE AM_GC_Company = @CompanyPK
		GROUP BY AM_Year
		HAVING MIN(AM_StartDate) <= @ReportDateFrom AND 
			MAX(AM_EndDate) > @ReportDateFrom) AND
	ACR_DateTo < @ReportDateFrom)", Schema.MaxLineNum, Schema.GeneralLedgerAmountDR, Schema.GeneralLedgerAmountCR);
				using (var command = ((IDbConnected)Factory).Connection.Command(sql))
				{
					command.AddParameterBasedOnDbColumn("@CompanyPK", ACR_GC_Company.ToGuid(), AccComplianceReportSchema.ACR_GC_Company);
					command.AddParameterBasedOnDbColumn("@BranchPK", ACR_GB_Branch.IsEmpty ? DBNull.Value : ACR_GB_Branch.ToGuid(), AccComplianceReportSchema.ACR_GB_Branch);
					command.AddParameterBasedOnDbColumn("@ReportType", ACR_ReportType.ToString(), AccComplianceReportSchema.ACR_ReportType);
					command.AddParameterBasedOnDbColumn("@ReportDateFrom", ACR_DateFrom.ToDateTime(), AccComplianceReportSchema.ACR_DateFrom);

					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
						{
							LineNumOffset = new ZInt(reader[Schema.MaxLineNum]);
							GLOpeningBalanceDR = new ZDecimal(reader[Schema.GeneralLedgerAmountDR]);
							GLOpeningBalanceCR = new ZDecimal(reader[Schema.GeneralLedgerAmountCR]);

							LoadedOpeningBalancesAndLineNumOffset = true;
						}
					}
				}

				if (SupportsGLBalanceDetails)
				{
					var totals = CalculateGLTotals(GLOpeningBalanceDetailsView.Cast<GeneralLedgerBalanceLine>());
					GLOpeningBalanceDR = totals.Debit;
					GLOpeningBalanceCR = totals.Credit;
				}
			}
		}

		bool LoadedOpeningBalancesAndLineNumOffset;

		(ZDecimal Debit, ZDecimal Credit) CalculateGLTotals(IEnumerable<GeneralLedgerBalanceLine> balanceLines)
		{
			var debit = ZDecimal.Zero;
			var credit = ZDecimal.Zero;
			foreach (var glAccountBalance in balanceLines)
			{
				debit += glAccountBalance.GeneralLedgerAmountDR;
				credit += glAccountBalance.GeneralLedgerAmountDR;
			}
			return (debit, credit);
		}

		public GeneralLedgerBalanceLineCollectionView GLOpeningBalanceDetailsView =>
			glOpeningBalanceDetailsView ?? (glOpeningBalanceDetailsView = new GeneralLedgerBalanceLineCollectionView(GLOpeningBalanceDetails));
		GeneralLedgerBalanceLineCollectionView glOpeningBalanceDetailsView;

		public AccComplianceReportLineCollectionBase<GeneralLedgerBalanceLine> GLOpeningBalanceDetails
		{
			get
			{
				if (glOpeningBalanceDetails == null)
				{
					LoadOpeningBalanceDetailsIfApplicable(ref glOpeningBalanceDetails);
				}
				return glOpeningBalanceDetails;
			}
		}
		AccComplianceReportLineCollectionBase<GeneralLedgerBalanceLine> glOpeningBalanceDetails;

		void LoadOpeningBalanceDetailsIfApplicable(ref AccComplianceReportLineCollectionBase<GeneralLedgerBalanceLine> collection)
		{
			if (collection == null)
			{
				collection = new AccComplianceReportLineCollectionBase<GeneralLedgerBalanceLine>(this);
			}
			else
			{
				collection.RemoveAndDeleteAll();
			}

			if (SupportsGLBalanceDetails)
			{
				using (var command = GetLoadBalanceDetailsCommand())
				{
					if (command != null)
					{
						var castCollection = collection;
						LoadNonPersistentBusinessObjectCollection(ref castCollection, command);
					}
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		DbCommand GetLoadBalanceDetailsCommand()
		{
			DbCommand temp = null;
			DbCommand result = null;

			try
			{
				if (LoadDbConnection == null)
				{
					return null;
				}

				CheckAndReportErrorOnIncreasedTimeout();

				var sql = GetLoadBalanceDetailsSql();
				temp = LoadDbConnection.Command(sql, CommandTimeout);
				AddParametersForLoadBalanceDetails(temp);
				result = temp;
				temp = null;
			}
			finally
			{
				if (temp != null)
				{
					temp.Dispose();
				}
			}
			return result;
		}

		protected virtual DbConnection LoadDbConnection => ((IDbConnected)Factory).Connection;

		protected virtual void AddParametersForLoadBalanceDetails(DbCommand dbCommand)
		{
			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var periodManagement = periodCalculator.GetPeriodManagementFromDate(ACR_DateFrom, ACR_GC_Company);
			var currentPeriod = periodManagement != null ? periodManagement.AM_Period : ZInt.Zero;
			var firstPeriod = periodManagement != null ? periodCalculator.GetFirstPeriodForYear(periodManagement.AM_Year) : ZInt.Zero;

			dbCommand.AddParameterBasedOnDbColumn("@CompanyPK", ACR_GC_Company.ToGuid(), AccComplianceReportSchema.ACR_GC_Company);
			dbCommand.AddParameterBasedOnDbColumn("@PLAppropriationAccountPK", AccountingConfigurationRegistry.Instance.PLAppropriationAccount.Value, AccGLHeaderSchema.PK);
			dbCommand.AddParameterBasedOnDbColumn("@FirstPeriodInTheYear", (int)firstPeriod, AccGLAggregateSchema.AA_Period);
			dbCommand.AddParameterBasedOnDbColumn("@CurrentPeriod", (int)currentPeriod, AccGLAggregateSchema.AA_Period);
		}

		#endregion

		#region LoadSQL

		protected virtual string GetLoadBalanceDetailsSql()
		{
			return string.Format(LoadBalanceDetails, string.Empty, AggregateQuery, HeaderDetailsQuery);
		}

		const string AggregateQuery = @"
			SELECT
				AG_PK,
				AA_Amount,
				AG_AccountType,
				AA_Period
			FROM 
				dbo.AccGLAggregate
				INNER JOIN dbo.AccGLHeader ON AA_AG = AG_PK
			WHERE
				AA_GC = @CompanyPK";

		protected const string HeaderDetailsQuery = @"
			SELECT 
				accHeader.AG_AccountNum, 
				accHeader.AG_AccountType, 
				accHeader.AG_DebitCredit, 
				accHeader.AG_PK, 
				accHeader.AG_Description,
				accHeader.AG_AG_ConsolidationNum,
				consolidationHeader.AG_AccountNum AS AG_ConsolidationAccountNum,
				accHeader.AG_AccountType AS AG_ConsolidationAccountType, 
				accHeader.AG_Description AS AG_ConsolidationDescription
			FROM 
				dbo.AccGLHeader accHeader
				LEFT JOIN dbo.AccGLHeader consolidationHeader ON consolidationHeader.AG_PK = accHeader.AG_AG_ConsolidationNum
";

		protected const string LoadBalanceDetails = @"{0}
DECLARE @PAndLTotalAmount MONEY
SET @PAndLTotalAmount =	ISNULL
							(
								(
									SELECT
										SUM(AA_Amount)
									FROM
										({1}) aggregate
									WHERE
										AG_AccountType = 'P&L' AND
										AA_Period < @FirstPeriodInTheYear
								), 0
							)

;WITH
	openingBalance AS (
	SELECT AG_PK, SUM(Balance) Balance  FROM
	(
		SELECT 
			AG_PK,
			Balance = SUM(AA_Amount) 
		FROM
			({1}) aggregate 
		WHERE
			AG_AccountType NOT IN ('HDR', 'ALT', 'TTL')	AND
			(AG_AccountType <> 'P&L' OR AG_PK = @PLAppropriationAccountPK OR AA_Period IS NULL OR AA_Period >= @FirstPeriodInTheYear) AND
			AA_Period < @CurrentPeriod
		GROUP BY AG_PK

		UNION ALL

		SELECT @PLAppropriationAccountPK AG_PK, Balance = @PAndLTotalAmount
	) PL GROUP BY AG_PK
)

SELECT
	header.AG_AccountNum, header.AG_AccountType, header.AG_DebitCredit, header.AG_PK, header.AG_Description,
	header.AG_AG_ConsolidationNum,
	header.AG_ConsolidationAccountNum,
	header.AG_ConsolidationAccountType,
	header.AG_ConsolidationDescription,
	IIF(ISNULL(Balance, 0) > 0, ISNULL(Balance, 0), 0) AS GeneralLedgerAmountDR, 
	IIF(ISNULL(Balance, 0) < 0, -ISNULL(Balance, 0), 0) AS GeneralLedgerAmountCR 
FROM
	({2}) header
	LEFT JOIN openingBalance ON openingBalance.AG_PK = header.AG_PK
ORDER BY header.AG_AccountNum";

		#endregion

		#region GL Movements

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal GeneralLedgerAmountDR
		{
			get { return GetAddOnDecimal(Schema.GeneralLedgerAmountDR); }
			private set { SetAddOnColumnDecimal(Schema.GeneralLedgerAmountDR, value); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal GeneralLedgerAmountCR
		{
			get { return GetAddOnDecimal(Schema.GeneralLedgerAmountCR); }
			private set { SetAddOnColumnDecimal(Schema.GeneralLedgerAmountCR, value); }
		}

		// C# implementation for calculation of GL amount of database view dbo.ViewComplianceReportLine (SQL comments will be removed during implementation)
		[SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Baseline")]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		public ZDecimal CalculateGeneralLedgerAmountViaReportSubCode(ZDecimal netAmountLocal, ZDecimal vatAmountLocal, ZString reportSubCode,
			ZString lineType, ZString gstVatBasis, ZDecimal inputGstVatRecoverable, ZString currencyCode, ZInt currencyDecimals)
		{
			// Performance note: ZString.Split and ZString.Right have bad performance. If overall performance is too bad, then consider replacing ZString with string here.
			var subCodeElements = reportSubCode.Split('*');  // sub codes look like: "*AR*INV*ARCtrl*Total"
			var accountCode = subCodeElements.Length > 3 ? subCodeElements[3] : ZString.Empty;
			var modifierCode = subCodeElements.Length > 4 ? subCodeElements[4] : ZString.Empty;
			ZDecimal result;

			if ((accountCode.Right(5) == "GSTIn" || accountCode.Right(6) == "GSTOut") && modifierCode == "-")  // WHEN ACL_ReportSubCode LIKE '*%GSTIn*-' OR ACL_ReportSubCode LIKE '*%GSTOut*-' THEN
			{
				if (((lineType == "CST" && gstVatBasis == "A") || lineType == "DPY") && inputGstVatRecoverable != 1)  // WHEN((AL_LineType = 'CST' AND AL_GSTVATBasis = 'A') OR AL_LineType = 'DPY') AND AL_InputGSTVATRecoverable != 1
				{
					result = Math.Round(vatAmountLocal * inputGstVatRecoverable, currencyDecimals, MidpointRounding.AwayFromZero);  // THEN GetRecoverableTaxAmounts.GSTVATRecoverable-- Recoverable Tax
				}
				else
				{
					result = vatAmountLocal;  // ELSE AL_GSTVAT  --Tax Amount
				}
			}
			else
			{
				if (accountCode == "" && modifierCode == "GSTNotRec-")  // WHEN ACL_ReportSubCode LIKE '*%**GSTNotRec-' THEN
				{
					if (lineType == "CST" && gstVatBasis == "A" && inputGstVatRecoverable != 1)  // WHEN AL_LineType = 'CST' AND AL_GSTVATBasis = 'A' AND AL_InputGSTVATRecoverable != 1
					{
						result = vatAmountLocal - Math.Round(vatAmountLocal * inputGstVatRecoverable, currencyDecimals, MidpointRounding.AwayFromZero); // THEN GetRecoverableTaxAmounts.GSTVATNotRecoverable-- Not Recoverable Tax
					}
					else
					{
						result = 0;  // ELSE 0-- Should not have such records
					}
				}
				else
				{
					if (modifierCode == "Total")  // WHEN ACL_ReportSubCode LIKE '*%*Total'
					{
						result = netAmountLocal + vatAmountLocal;  // THEN AL_LineAmount +AL_GSTVAT-- Currently* Total is used only for combinations of '*ARCtrl*Total' / '*APCtrl*Total/*CB*DPY*Bank*Total/*CB*DRC*Bank*Total' for AR / AP INv / CRD / ADJ, CB DRC / DPY without reversing sign
					}
					else
					{
						result = netAmountLocal;  // ELSE AL_LineAmount
					}
				}
			}

			if (modifierCode.Right(1) == "-")
			{
				result *= -1;  // WHEN ACL_ReportSubCode LIKE '*%-' THEN - 1 ELSE 1-- Suffix '-' is used to indicate reversing of the sign
			}

			return result;
		}

#if DEBUG
		public
#endif
		ZDecimal GetAddOnDecimal(string columnName)
		{
			var addOn = AddOnColumns.FirstOrDefault(x => x.XA_Name == columnName);
			var decimalValue = addOn != null && !addOn.XA_Data.IsEmpty && Decimal.TryParse(addOn.XA_Data, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var outValue) ? outValue : Decimal.Zero;
			return new ZDecimal(decimalValue);
		}

#if DEBUG
		public
#endif
		void SetAddOnColumnDecimal(string columnName, ZDecimal value)
		{
			CreateOrAmendAddOnColumn(columnName, AddOnColumnDataType.Codes.Decimal, value);
		}

#if DEBUG
		[ChildEditable]
		public GenAddOnColumnCollection AddOnColumnsForTestOnly => AddOnColumns;
#endif
		GenAddOnColumnCollection AddOnColumns
		{
			get { return addOnColumns ?? (addOnColumns = GetAddOnColumns()); }
		}
		GenAddOnColumnCollection addOnColumns;

		GenAddOnColumnCollection GetAddOnColumns()
		{
			var result = new GenAddOnColumnCollection(this);
			RegisterEditableChildObject(result);
			return result;
		}

		void CreateOrAmendAddOnColumn(string addOnColumnSchemaName, ZString addOnColumnType, IZType value)
		{
			var addOn = AddOnColumns.FirstOrDefault(x => x.XA_Name == addOnColumnSchemaName);
			if (addOn == null && !value.IsDefault)
			{
				addOn = AddOnColumns.AddNew();
				addOn.XA_Name = addOnColumnSchemaName;
				addOn.XA_Type = addOnColumnType;
			}
			if (addOn != null)
			{
				var isNotDecimal = addOnColumnType != AddOnColumnDataType.Codes.Decimal;
				addOn.XA_Data = value.IsEmpty ? string.Empty : isNotDecimal ? value.ToString() : string.Format(Culture.Invariant, "{0:0.00000000}", value);
			}
		}

		#endregion

		#region GL Movement Details

		public GeneralLedgerBalanceLineCollectionView GLMovementDetailsView =>
			glMovementDetailsView ?? (glMovementDetailsView = new GeneralLedgerBalanceLineCollectionView(GLMovementDetails));
		GeneralLedgerBalanceLineCollectionView glMovementDetailsView;

		public AccComplianceReportLineCollectionBase<GeneralLedgerBalanceLine> GLMovementDetails
		{
			get
			{
				CheckIfLoadingLimitApplied();

				if (glMovementDetails == null)
				{
					CalculateGlMovementDetailsIfApplicable(ref glMovementDetails, ReportLines);
				}
				return glMovementDetails;
			}
		}
		AccComplianceReportLineCollectionBase<GeneralLedgerBalanceLine> glMovementDetails;

		public virtual AccComplianceReportLineCollectionBase<GeneralLedgerBalanceLine> GLMovementDetailsExport => GLMovementDetails;

		protected void CalculateGlMovementDetailsIfApplicable(ref AccComplianceReportLineCollectionBase<GeneralLedgerBalanceLine> details, AccComplianceReportLineCollectionBase<AccComplianceReportLine> reportLines, bool addAmount = false)
		{
			if (details == null)
			{
				details = new AccComplianceReportLineCollectionBase<GeneralLedgerBalanceLine>(this);
			}
			else
			{
				details.RemoveAndDeleteAll();
			}

			if (SupportsGLBalanceDetails)
			{
				var movementTotals = reportLines.Cast<AccComplianceReportLine>().GroupBy(x => x.AG_AccountNum).Select(g => new
				{
					GLAccountNum = g.Key,
					GLAmountDR = g.Sum(dr => dr.GeneralLedgerAmountDR),
					GLAmountCR = g.Sum(cr => cr.GeneralLedgerAmountCR)
				}).ToDictionary(x => x.GLAccountNum);

				foreach (var openingBalanceLine in GLOpeningBalanceDetails.Cast<GeneralLedgerBalanceLine>())
				{
					var glMovementDR = ZDecimal.Zero;
					var glMovementCR = ZDecimal.Zero;
					if (movementTotals.ContainsKey(openingBalanceLine.AG_AccountNum))
					{
						glMovementDR = movementTotals[openingBalanceLine.AG_AccountNum].GLAmountDR;
						glMovementCR = movementTotals[openingBalanceLine.AG_AccountNum].GLAmountCR;
					}
					AddGLBalanceLine(details, openingBalanceLine, glMovementDR, glMovementCR, addAmount);
				}
			}
		}

		void AddGLBalanceLine(AccComplianceReportLineCollectionBase<GeneralLedgerBalanceLine> balanceLines, GeneralLedgerBalanceLine openingBalanceLine, ZDecimal glMovementDR, ZDecimal glMovementCR, bool addAmount = false)
		{
			var sourceRow = ((INeedRow)openingBalanceLine).Row;

			var table = ((INeedTable)balanceLines).Table;
			var row = table.NewRow();

			var totalDR = (decimal)sourceRow[GeneralLedgerBalanceLine.Schema.GeneralLedgerAmountDR] + glMovementDR;
			var totalCR = (decimal)sourceRow[GeneralLedgerBalanceLine.Schema.GeneralLedgerAmountCR] + glMovementCR;

			foreach (DataColumn column in table.Columns)
			{
				switch (column.ColumnName)
				{
					case GeneralLedgerBalanceLine.Schema.GeneralLedgerAmountDR:
						row[column.ColumnName] = !addAmount ? (decimal)glMovementDR : totalDR > totalCR ? totalDR - totalCR : decimal.Zero;
						break;
					case GeneralLedgerBalanceLine.Schema.GeneralLedgerAmountCR:
						row[column.ColumnName] = !addAmount ? (decimal)glMovementCR : totalCR > totalDR ? totalCR - totalDR : decimal.Zero;
						break;
					default:
						row[column.ColumnName] = sourceRow[column.ColumnName];
						break;
				}
			}
			table.Rows.Add(row);
			row.AcceptChanges();

			var line = new GeneralLedgerBalanceLine(Factory, row);
			balanceLines.Add(line);
		}

		#endregion

		#region GL Closing Balance

		public GeneralLedgerBalanceLineCollectionView GLClosingBalanceDetailsView =>
			glClosingBalanceDetailsView ?? (glClosingBalanceDetailsView = new GeneralLedgerBalanceLineCollectionView(GLClosingBalanceDetails));
		GeneralLedgerBalanceLineCollectionView glClosingBalanceDetailsView;

		public AccComplianceReportLineCollectionBase<GeneralLedgerBalanceLine> GLClosingBalanceDetails
		{
			get
			{
				CheckIfLoadingLimitApplied();

				if (glClosingBalanceDetails == null)
				{
					CalculateGlMovementDetailsIfApplicable(ref glClosingBalanceDetails, ReportLines, addAmount: true);
				}
				return glClosingBalanceDetails;
			}
		}
		AccComplianceReportLineCollectionBase<GeneralLedgerBalanceLine> glClosingBalanceDetails;

		#endregion

		#region ReportTotals

		public AccComplianceReportLineCollectionBase<AccComplianceReportLineBase> ReportTotals
		{
			get
			{
				if (reportTotals == null)
				{
					LoadReportTotals();
				}
				return reportTotals;
			}
		}
		AccComplianceReportLineCollectionBase<AccComplianceReportLineBase> reportTotals;

		public AccComplianceReportLineCollectionBase<AccComplianceReportLineBase> ReportTotalsPreviousPeriod
		{
			get
			{
				if (reportTotalsPreviousPeriod == null)
				{
					LoadReportTotals();
				}
				return reportTotalsPreviousPeriod;
			}
		}
		AccComplianceReportLineCollectionBase<AccComplianceReportLineBase> reportTotalsPreviousPeriod;

		public AccComplianceReportLineCollectionBase<AccComplianceReportLineBase> ReportTotalsCurrentPeriod
		{
			get
			{
				if (!HasPreviousPeriodData)
				{
					return ReportTotals;
				}
				else
				{
					if (reportTotalsCurrentPeriod == null)
					{
						LoadReportTotals();
					}
					return reportTotalsCurrentPeriod;
				}
			}
		}
		AccComplianceReportLineCollectionBase<AccComplianceReportLineBase> reportTotalsCurrentPeriod;

		void LoadReportTotals()
		{
			CreateOrClearTotalsCollection(ref reportTotalsPreviousPeriod);
			CreateOrClearTotalsCollection(ref reportTotals);

			if (SupportsGLBalance)
			{
				LoadReportTotalsWithGLBalance();
			}
			else
			{
				TotalsData movements;

				var listMovements = GetMovementsTotals();
				if (listMovements.Count > 1)
				{
					CreateOrClearTotalsCollection(ref reportTotalsCurrentPeriod);

					var movementsPreviousPeriod = listMovements.FirstOrDefault(x => x.TotalsPeriod == 0);
					var movementsCurrentPeriod = listMovements.FirstOrDefault(x => x.TotalsPeriod == 1);

					AddTotalsLine(reportTotalsPreviousPeriod, Res.GetString("00f611ef-fad5-4442-ac39-1e40f3b2d0ee", "Report Totals Previous Period"), movementsPreviousPeriod);
					AddTotalsLine(reportTotalsCurrentPeriod, Res.GetString("0555c12b-b5f2-4721-be9d-43f07d4c7be5", "Report Totals Current Period"), movementsCurrentPeriod);

					movements = CombineMovementsData(movementsPreviousPeriod, movementsCurrentPeriod);
				}
				else
				{
					movements = listMovements.FirstOrDefault(x => x.TotalsPeriod == 1);
				}

				AddTotalsLine(reportTotals, Res.GetString("15daaf85-d75c-4635-bf79-85ae4a364293", "Report Totals"), movements);
			}
		}

		void CreateOrClearTotalsCollection(ref AccComplianceReportLineCollectionBase<AccComplianceReportLineBase> collection)
		{
			if (collection == null)
			{
				collection = new AccComplianceReportLineCollectionBase<AccComplianceReportLineBase>(this);
			}
			else
			{
				collection.RemoveAndDeleteAll();
			}
		}

		void LoadReportTotalsWithGLBalance()
		{
			var movements = new TotalsData();

			LoadOpeningBalancesAndLineNumOffset();
			AddTotalsLine(reportTotals, "  " + Res.GetString("b45c63b7-ec06-4948-8013-f9d436e360ee", "Opening Balance"),
				new TotalsData() { GeneralLedgerAmountDR = GLOpeningBalanceDR, GeneralLedgerAmountCR = GLOpeningBalanceCR });

			if (SupportsGLBalanceDetails)
			{
				var glTotals = CalculateGLTotals(GLMovementDetailsView.Cast<GeneralLedgerBalanceLine>());
				movements.GeneralLedgerAmountDR = glTotals.Debit;
				movements.GeneralLedgerAmountCR = glTotals.Credit;
				var maxSequence = ReportLines.Any() ? ReportLines.Cast<AccComplianceReportLine>().Max(x => x.ACL_ReportSequence) : ZInt.Zero;
				movements.LineNum = maxSequence.IsEmpty ? ZInt.Zero : (ZInt)(maxSequence - LineNumOffset);
			}
			else
			{
				movements = GetMovementsTotals().FirstOrDefault(x => x.TotalsPeriod == 1);
			}

			AddTotalsLine(reportTotals, " " + Res.GetString("66318eec-73a9-447b-99e1-940ee1e805db", "Balance Movements"), movements);

			MaxLineNum = movements.LineNum;
			GeneralLedgerAmountDR = movements.GeneralLedgerAmountDR;
			GeneralLedgerAmountCR = movements.GeneralLedgerAmountCR;

			var closingTotals = new TotalsData();
			if (SupportsGLBalanceDetails)
			{
				var glTotals = CalculateGLTotals(GLClosingBalanceDetailsView.Cast<GeneralLedgerBalanceLine>());
				closingTotals.GeneralLedgerAmountDR = glTotals.Debit;
				closingTotals.GeneralLedgerAmountCR = glTotals.Credit;
			}
			else
			{
				closingTotals.GeneralLedgerAmountDR = GLOpeningBalanceDR + GeneralLedgerAmountDR;
				closingTotals.GeneralLedgerAmountCR = GLOpeningBalanceCR + GeneralLedgerAmountCR;
			}

			AddTotalsLine(reportTotals, Res.GetString("815a1a44-faab-4b28-88c7-54e781b86309", "Closing Balance"), closingTotals);
			reportTotals.Sort(AccComplianceReportLineBase.Schema.Comment, ListSortDirection.Ascending);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Must issue DB command to database directly as we have to pass a custom TVP.")]
		List<TotalsData> GetMovementsTotals()
		{
			var listMovements = new List<TotalsData>();

			if (IsInDatabase && ReportConfiguration != null)
			{
				var hasPreCalculatedAmount = HasPreCalculatedAmount;

				var sqlPreCalculatedAmount = hasPreCalculatedAmount
					? @"SUM(
CAST(
	CASE ISNUMERIC(
		CASE CHARINDEX('|', ReportSubCode) WHEN 0 THEN ReportSubCode ELSE LEFT ( ReportSubCode , CHARINDEX('|', ReportSubCode)-1 ) END
		)
	WHEN 1
	THEN
		CASE CHARINDEX('|', ReportSubCode) WHEN 0 THEN ReportSubCode ELSE LEFT ( ReportSubCode , CHARINDEX('|', ReportSubCode)-1 ) END
	ELSE
		'0.0'
	END
AS DECIMAL(24,9))
) AS SUM_PreCalculatedAmount,"
					: string.Empty;

				var sqlPeriod = IncludeQueuedForPreviousPeriod
					? "CASE WHEN PostDate < @ReportDateFrom THEN 0 ELSE 1 END"
					: "1";

				var sqlGroupBy = IncludeQueuedForPreviousPeriod
					? "GROUP BY CASE WHEN PostDate < @ReportDateFrom THEN 0 ELSE 1 END"
					: string.Empty;

				var getLinesTVF = GetComplianceReportLinesBasedOnGroupByAndTablePrefix();

				var sql = FormattableString.Invariant($@"SELECT
MAX(ACL_ReportSequence) AS MAX_ACL_ReportSequence, 
SUM(GoodsExTaxAmount) AS SUM_GoodsExTaxAmount, 
SUM(GoodsTaxAmount) AS SUM_GoodsTaxAmount, 
SUM(ServiceExTaxAmount) AS SUM_ServiceExTaxAmount, 
SUM(ServiceTaxAmount) AS SUM_ServiceTaxAmount, 
SUM(TotalExTaxAmount) AS SUM_TotalExTaxAmount, 
SUM(TotalTaxAmount) AS SUM_TotalTaxAmount, 
SUM(TaxRecoverableAmount) AS SUM_TaxRecoverableAmount, 
SUM(TaxNotRecoverableAmount) AS SUM_TaxNotRecoverableAmount, 
SUM(TaxReverseChargeAmount) AS SUM_TaxReverseChargeAmount, 
SUM(CASE WHEN GeneralLedgerAmount < 0 THEN - GeneralLedgerAmount ELSE 0 END) AS SUM_GeneralLedgerAmountCR,
SUM(CASE WHEN GeneralLedgerAmount > 0 THEN GeneralLedgerAmount ELSE 0 END) AS SUM_GeneralLedgerAmountDR,
{sqlPreCalculatedAmount}
{sqlPeriod} AS Period
FROM {getLinesTVF}(@PK, @ReportTablePrefix, @ReportDateFrom, @ReportDateTo_PlusOneDay, @RegType, @ReportCountry, @RepCountryRegistrationCodeType, @SubCodeToGLAccountMapping, @GS, @RoundingType, @Rounding, @ReverseSign)
{sqlGroupBy}
OPTION(RECOMPILE)");

				using (var command = GetLoadLinesCommand(sql))
				{
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var movements = new TotalsData();

							movements.GoodsExTaxAmount = new ZDecimal(reader["SUM_GoodsExTaxAmount"]);
							movements.GoodsTaxAmount = new ZDecimal(reader["SUM_GoodsTaxAmount"]);
							movements.ServiceExTaxAmount = new ZDecimal(reader["SUM_ServiceExTaxAmount"]);
							movements.ServiceTaxAmount = new ZDecimal(reader["SUM_ServiceTaxAmount"]);
							movements.TotalExTaxAmount = new ZDecimal(reader["SUM_TotalExTaxAmount"]);
							movements.TotalTaxAmount = new ZDecimal(reader["SUM_TotalTaxAmount"]);
							movements.TaxRecoverableAmount = new ZDecimal(reader["SUM_TaxRecoverableAmount"]);
							movements.TaxNotRecoverableAmount = new ZDecimal(reader["SUM_TaxNotRecoverableAmount"]);
							movements.TaxReverseChargeAmount = new ZDecimal(reader["SUM_TaxReverseChargeAmount"]);
							movements.GeneralLedgerAmountCR = new ZDecimal(reader["SUM_GeneralLedgerAmountCR"]);
							movements.GeneralLedgerAmountDR = new ZDecimal(reader["SUM_GeneralLedgerAmountDR"]);
							movements.LineNum = new ZInt(reader["MAX_ACL_ReportSequence"]);
							movements.TotalsPeriod = new ZInt(reader["Period"]);
							if (hasPreCalculatedAmount)
							{
								movements.PreCalculatedAmount = new ZDecimal(reader["SUM_PreCalculatedAmount"]);
							}

							listMovements.Add(movements);
						}
					}
				}
			}

			if (!listMovements.Any(x => x.TotalsPeriod == 1))
			{
				var movements = new TotalsData() { TotalsPeriod = 1 };
				listMovements.Add(movements);
			}

			return listMovements;
		}

		TotalsData CombineMovementsData(TotalsData data1, TotalsData data2)
		{
			var result = new TotalsData();

			result.GoodsExTaxAmount = data1.GoodsExTaxAmount + data2.GoodsExTaxAmount;
			result.GoodsTaxAmount = data1.GoodsTaxAmount + data2.GoodsTaxAmount;
			result.ServiceExTaxAmount = data1.ServiceExTaxAmount + data2.ServiceExTaxAmount;
			result.ServiceTaxAmount = data1.ServiceTaxAmount + data2.ServiceTaxAmount;
			result.TotalExTaxAmount = data1.TotalExTaxAmount + data2.TotalExTaxAmount;
			result.TotalTaxAmount = data1.TotalTaxAmount + data2.TotalTaxAmount;
			result.TaxRecoverableAmount = data1.TaxRecoverableAmount + data2.TaxRecoverableAmount;
			result.TaxNotRecoverableAmount = data1.TaxNotRecoverableAmount + data2.TaxNotRecoverableAmount;
			result.TaxReverseChargeAmount = data1.TaxReverseChargeAmount + data2.TaxReverseChargeAmount;
			result.GeneralLedgerAmountCR = data1.GeneralLedgerAmountCR + data2.GeneralLedgerAmountCR;
			result.GeneralLedgerAmountDR = data1.GeneralLedgerAmountDR + data2.GeneralLedgerAmountDR;
			result.PreCalculatedAmount = data1.PreCalculatedAmount + data2.PreCalculatedAmount;
			result.LineNum = Math.Max(data1.LineNum, data2.LineNum);
			result.LineNum -= result.LineNum.IsEmpty ? ZInt.Zero : LineNumOffset;

			return result;
		}

		struct TotalsData
		{
			public ZDecimal GoodsExTaxAmount { get; set; }
			public ZDecimal GoodsTaxAmount { get; set; }
			public ZDecimal ServiceExTaxAmount { get; set; }
			public ZDecimal ServiceTaxAmount { get; set; }
			public ZDecimal TotalExTaxAmount { get; set; }
			public ZDecimal TotalTaxAmount { get; set; }
			public ZDecimal TaxRecoverableAmount { get; set; }
			public ZDecimal TaxNotRecoverableAmount { get; set; }
			public ZDecimal TaxReverseChargeAmount { get; set; }
			public ZDecimal GeneralLedgerAmountCR { get; set; }
			public ZDecimal GeneralLedgerAmountDR { get; set; }
			public ZDecimal PreCalculatedAmount { get; set; }
			public ZInt LineNum { get; set; }
			public ZInt TotalsPeriod { get; set; }
		}

		AccComplianceReportLineBase AddTotalsLine(AccComplianceReportLineCollectionBase<AccComplianceReportLineBase> totals, string comment, TotalsData data)
		{
			var table = ((INeedTable)totals).Table;
			var row = table.NewRow();

			if (SupportsGLBalance)
			{
				PopulateGLTotalsRow(row, table.Columns.Cast<DataColumn>(), comment, data);
			}
			else
			{
				PopulateTotalsRow(row, table.Columns.Cast<DataColumn>(), comment, data);
			}
			table.Rows.Add(row);
			row.AcceptChanges();

			var result = new AccComplianceReportLineBase(Factory, row);
			totals.Add(result);
			return result;
		}

		void PopulateGLTotalsRow(DataRow row, IEnumerable<DataColumn> columns, string comment, TotalsData data)
		{
			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case AccComplianceReportLineBase.Schema.Comment:
						row[column.ColumnName] = comment;
						break;
					case AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR:
						row[column.ColumnName] = (decimal)data.GeneralLedgerAmountDR;
						break;
					case AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR:
						row[column.ColumnName] = (decimal)data.GeneralLedgerAmountCR;
						break;
					case AccComplianceReportLineBase.Schema.LineNum:
						row[column.ColumnName] = (int)data.LineNum;
						break;
					default:
						row[column.ColumnName] = DBNull.Value;
						break;
				}
			}
		}

		void PopulateTotalsRow(DataRow row, IEnumerable<DataColumn> columns, string comment, TotalsData data)
		{
			foreach (var column in columns)
			{
				PopulateTotalsColumn(row, column, comment, data);
			}
		}

		void PopulateTotalsColumn(DataRow row, DataColumn column, string comment, TotalsData data)
		{
			switch (column.ColumnName)
			{
				case AccComplianceReportLineBase.Schema.Comment:
					row[column.ColumnName] = comment;
					break;
				case AccComplianceReportLineBase.Schema.GoodsExTaxAmount:
					row[column.ColumnName] = (decimal)data.GoodsExTaxAmount;
					break;
				case AccComplianceReportLineBase.Schema.GoodsTaxAmount:
					row[column.ColumnName] = (decimal)data.GoodsTaxAmount;
					break;
				case AccComplianceReportLineBase.Schema.ServiceExTaxAmount:
					row[column.ColumnName] = (decimal)data.ServiceExTaxAmount;
					break;
				case AccComplianceReportLineBase.Schema.ServiceTaxAmount:
					row[column.ColumnName] = (decimal)data.ServiceTaxAmount;
					break;
				case AccComplianceReportLineBase.Schema.TotalExTaxAmount:
					row[column.ColumnName] = (decimal)data.TotalExTaxAmount;
					break;
				case AccComplianceReportLineBase.Schema.TotalTaxAmount:
					row[column.ColumnName] = (decimal)data.TotalTaxAmount;
					break;
				case AccComplianceReportLineBase.Schema.TaxRecoverableAmount:
					row[column.ColumnName] = (decimal)data.TaxRecoverableAmount;
					break;
				case AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount:
					row[column.ColumnName] = (decimal)data.TaxNotRecoverableAmount;
					break;
				case AccComplianceReportLineBase.Schema.TaxReverseChargeAmount:
					row[column.ColumnName] = (decimal)data.TaxReverseChargeAmount;
					break;
				case AccComplianceReportLineBase.Schema.PreCalculatedAmount:
					row[column.ColumnName] = (decimal)data.PreCalculatedAmount;
					break;
				default:
					row[column.ColumnName] = DBNull.Value;
					break;
			}
		}

		#endregion

		#region Generating Report Data

		public void GenerateFromQueue()
		{
			ChangeAndApplyStatus(Status.ReportGenerated);
		}

		public void GenerateIDEAFromQueue(ILogger serviceTaskLogger, AccComplianceReportUsageCollectorContextData usageData)
		{
			string error = null;
			var orgProxy = Company.OrgProxy;
			if (orgProxy == null)
			{
				error = $"No organization proxy is found for the reporting company {Company.HumanReadableShortcutName}";
			}
			else if (AddressHelper.GetReportAddress(orgProxy) == null)
			{
				error = $"Please define a single office address as main address for the organization proxy of the reporting company {Company.HumanReadableShortcutName}. This address will be used for the compliance report.";
			}
			if (!string.IsNullOrEmpty(error))
			{
				ChangeAndApplyStatusCore(Status.ReportError, connection => { DeleteGenAddOnColumns(connection, Schema.NextProcessingStepFromDate); }, () => { ACR_StatusMessage = error; }, usageDataContext: usageData);
				throw new InvalidOperationException(error);
			}

			var ideaTaxAuditExport = Factory.New<IDEATaxAuditExport>();
			var (isAllDataExported, statusMessage) = ideaTaxAuditExport.ExportData(this, serviceTaskLogger);
			if (isAllDataExported)
			{
				ChangeAndApplyStatusCore(Status.ReportGenerated, connection => { DeleteGenAddOnColumns(connection, Schema.NextProcessingStepFromDate); }, () => { ACR_StatusMessage = statusMessage; }, usageDataContext: usageData);
			}
			else
			{
				Factory.Save();
			}
		}

		public void GenerateFECFromQueue(ILogger serviceTaskLogger, AccComplianceReportUsageCollectorContextData usageData)
		{
			var fecReport = new FECReport();
			var newStatus = fecReport.ExportData(this, serviceTaskLogger);

			if (newStatus != ACR_Status)
			{
				ChangeAndApplyStatusCore(newStatus, connection => { }, usageDataContext: usageData);
			}
		}

		public void GenerateJPKV7MFile(IJPKV7MReport report, ILogger serviceTaskLogger, AccComplianceReportUsageCollectorContextData usageData)
		{
			if (ACR_ReportType != ReportTypes.JPKV7M ||
				ACR_Status != Status.ReportGenerated ||
				Company.GC_RN_NKCountryCode != CountryCodes.Poland)
			{
				return;
			}

			ChangeAndApplyStatusCore(Status.ReportOutputGenerated, connection => { }, () => report.ExportXmlToEDocs(this, serviceTaskLogger), usageDataContext: usageData);
		}

		public T SubscribeForDispose<T>(T obj) where T : IDisposable
		{
			Argument.NotNull(obj, nameof(obj));

			return this.Factory.SubscribeForDispose(obj);
		}

		#endregion

		#region Invalidate Report

		internal void Invalidate()
		{
			ChangeAndApplyStatus(Status.ReportInvalidated);
		}

		#endregion

		#region ReQueue Report

		public void ReQueue()
		{
			ChangeAndApplyStatus(Status.ReportPendingQueueing);
		}

		#endregion

		#region Finalise Report

		public void Finalise()
		{
			ChangeAndApplyStatus(Status.ReportFinalised);
		}

		ComplianceReportComplianceDocumentFinaliser Finaliser
		{
			get
			{
				if (fFinaliser == null)
				{
					fFinaliser = new ComplianceReportComplianceDocumentFinaliser(Factory);
				}
				return fFinaliser;
			}
		}
		ComplianceReportComplianceDocumentFinaliser fFinaliser;

		#endregion

		#region Change Status implementation

		static string GetComplianceReportIsNotValidErrorMessage(string status) =>
			string.Format(CultureInfo.InvariantCulture, (NoResString)"'{0}' is not a valid Compliance Report status.", status);

		void ChangeAndApplyStatus(ZString status, string statusMessage = "")
		{
			if (!Lookups.ReportStatusList.ContainsCode(status))
			{
				throw new ArgumentException(GetComplianceReportIsNotValidErrorMessage(status));
			}

			if (!inFactorySaving)
			{
				switch (status)
				{
					case Status.ReportPendingQueueing:
						ChangeAndApplyStatusCore(status, connection => { DeleteLinesCore(connection); DeleteQueueCore(connection); }, () => { NextProcessingStepFromDate = ZDate.Empty; DeleteEDocsIfRequired(); });
						break;
					case Status.ReportDataQueued:
					case Status.ReportInvalidated:
						ChangeAndApplyStatusCore(status, connection => { DeleteLinesCore(connection); });
						break;
					case Status.ReportGenerated:
						ChangeAndApplyStatusCore(status, connection => { GenerateCore(connection); }, () => { ACR_StatusMessage = statusMessage; });
						break;
					case Status.ReportFinalised:
						ChangeAndApplyStatusCore(status, connection => { DeleteQueueCore(connection); Finaliser.FinaliseComplianceDocuments(this); });
						break;
				}
			}
			else
			{
				ACR_Status = status;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No BizO generated for this table. This is the only way we can work with it")]
		void GenerateCore(DbConnection connection)
		{
			var generatedFromGLD = ReportConfiguration.ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.GeneralLedgerData;
			var sql = generatedFromGLD
				? "EXEC GenerateComplianceReport_GLD @ReportPK = @PK, @ReportGroupBy = @GroupBy, @ReportOrderBy = @OrderBy, @OpeningCategory = @OpeningCode, @ClosingCategory = @ClosingCode WITH RECOMPILE"
				: "EXEC GenerateComplianceReport @ReportPK = @PK, @ReportGroupBy = @GroupBy, @ReportOrderBy = @OrderBy, @GoodsServiceType = @GS, @ThresholdLevel = @ThresholdAt, @ExTaxThresholdAmount = @ExTax, @TaxThresholdAmount = @Tax, @OpeningCategory = @OpeningCode, @ClosingCategory = @ClosingCode, @IncludeQueuedForPreviousPeriod = @ConfigIncludeQueuedForPreviousPeriod WITH RECOMPILE";

			CheckAndReportErrorOnIncreasedTimeout();
			using (var command = connection.Command(sql, CommandTimeout))
			{
				command.AddParameterBasedOnDbColumn("@PK", PK.ToGuid(), AccComplianceReportSchema.PK);
				command.AddParameterBasedOnDbColumn("@GroupBy", ReportConfiguration?.ReportLineGrouping.ToString() ?? string.Empty, CargoWise.Schema.Schema.GenericStringSchemaColumn);
				command.AddParameterBasedOnDbColumn("@OrderBy", ReportConfiguration?.ReportLineOrdering.ToString() ?? string.Empty, CargoWise.Schema.Schema.GenericStringSchemaColumn);
				if (!generatedFromGLD)
				{
					command.AddParameterBasedOnDbColumn("@GS", ReportConfiguration?.GoodsServiceType.ToString() ?? string.Empty, CargoWise.Schema.Schema.GenericStringSchemaColumn);
					command.AddParameterBasedOnDbColumn("@ThresholdAt", ReportConfiguration?.AmountThresholdLevel.ToString() ?? string.Empty, CargoWise.Schema.Schema.GenericStringSchemaColumn);
					command.AddParameter("@ExTax", SqlDbType.Money, (decimal)(ReportConfiguration?.ExTaxAmountThreshold ?? 0m));
					command.AddParameter("@Tax", SqlDbType.Money, (decimal)(ReportConfiguration?.TaxAmountThreshold ?? 0m));
					command.AddParameter("@ConfigIncludeQueuedForPreviousPeriod", SqlDbType.Bit, (bool)IncludeQueuedForPreviousPeriodForGenerating);
				}

				var openingCategoryCode = string.Empty;
				var closingCategoryCode = string.Empty;
				if (SupportsGLBalance)
				{
					var openingCategory = AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.GetFallBackValueAtAllLevels(ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty)?.OpeningCategory;
					openingCategoryCode = openingCategory?.Code ?? string.Empty;
					var closingCategory = AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.GetFallBackValueAtAllLevels(ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty)?.ClosingCategory;
					closingCategoryCode = closingCategory?.Code ?? string.Empty;
				}
				command.AddParameter("@OpeningCode", SqlDbType.VarChar, openingCategoryCode);
				command.AddParameter("@ClosingCode", SqlDbType.VarChar, closingCategoryCode);
				command.ExecuteNonQuery();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void DeleteGenAddOnColumns(DbConnection connection, string columnName)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, "DELETE {0} WHERE {1} = @PK AND {2} = @ColumnName",
				GenAddOnColumnSchema.Constants.TableName,
				GenAddOnColumnSchema.Constants.XA_ParentID,
				GenAddOnColumnSchema.Constants.XA_Name);

			CheckAndReportErrorOnIncreasedTimeout();
			using (var command = connection.Command(sql, CommandTimeout))
			{
				command.AddParameterBasedOnDbColumn("@PK", PK.ToGuid(), GenAddOnColumnSchema.XA_ParentID);
				command.AddParameterBasedOnDbColumn("@ColumnName", columnName, GenAddOnColumnSchema.XA_Name);

				command.ExecuteNonQuery();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void DeleteLinesCore(DbConnection connection)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, "DELETE {0} WHERE {1}  = @PK",
				AccComplianceReportTransactionPivotSchema.Constants.TableName,
				AccComplianceReportTransactionPivotSchema.Constants.ACL_ACR_Report);

			CheckAndReportErrorOnIncreasedTimeout();
			using (var command = connection.Command(sql, CommandTimeout))
			{
				command.AddParameterBasedOnDbColumn("@PK", PK.ToGuid(), AccComplianceReportTransactionPivotSchema.ACL_ACR_Report);

				command.ExecuteNonQuery();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void DeleteQueueCore(DbConnection connection)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"DELETE {0} 
WHERE {1} = @ReportType
	AND {2} = @CompanyPK
	AND (@BranchPK IS NULL OR {3} = @BranchPK)
	AND {4} < @DateToPlusDay
	{5}",
				AccTransactionComplianceReportQueueSchema.Constants.TableName,
				AccTransactionComplianceReportQueueSchema.Constants.ACQ_ReportType,
				AccTransactionComplianceReportQueueSchema.Constants.ACQ_GC_Company,
				AccTransactionComplianceReportQueueSchema.Constants.ACQ_GB_Branch,
				AccTransactionComplianceReportQueueSchema.Constants.ACQ_Date,
				IncludeQueuedForPreviousPeriod ? string.Empty : (NoResString)"AND " + AccTransactionComplianceReportQueueSchema.Constants.ACQ_Date + (NoResString)" >= @DateFrom");

			CheckAndReportErrorOnIncreasedTimeout();
			using (var command = connection.Command(sql, CommandTimeout))
			{
				command.AddParameterBasedOnDbColumn("@ReportType", ACR_ReportType.ToString(), AccTransactionComplianceReportQueueSchema.ACQ_ReportType);
				command.AddParameterBasedOnDbColumn("@CompanyPK", ACR_GC_Company.ToGuid(), AccTransactionComplianceReportQueueSchema.ACQ_GC_Company);
				command.AddParameterBasedOnDbColumn("@BranchPK", ACR_GB_Branch.IsEmpty ? DBNull.Value : ACR_GB_Branch.ToGuid(), AccComplianceReportSchema.ACR_GB_Branch);
				command.AddParameterBasedOnDbColumn("@DateToPlusDay", ACR_DateTo.AddDays(1).ToDateTime(), AccTransactionComplianceReportQueueSchema.ACQ_Date);

				if (!IncludeQueuedForPreviousPeriodForDeleting)
				{
					command.AddParameterBasedOnDbColumn("@DateFrom", ACR_DateFrom.ToDateTime(), AccTransactionComplianceReportQueueSchema.ACQ_Date);
				}

				command.ExecuteNonQuery();
			}
		}

		#endregion

		#region IDocumentSupportable

		public DocumentSupporter DocumentSupporter
		{
			get { return AccComplianceReportDocumentSupporter.New(this); }
		}

		#endregion

		#region IDocManagerSupport

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.ComplianceReport));
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		#region EDoc Handling

		public IeDoc AttachFileToEdoc(string filename, string fileDescription)
		{
			Argument.NotNullOrEmpty(filename, nameof(filename));
			Argument.NotNullOrEmpty(fileDescription, nameof(fileDescription));

			var fileStream = SubscribeForDispose(File.OpenRead(filename));

			return AttachFileToEdoc(fileStream, fileDescription);
		}

		public IeDoc AttachFileToEdoc(FileStream fileStream, string fileDescription)
		{
			Argument.NotNull(fileStream, nameof(fileStream));
			Argument.NotNullOrEmpty(fileDescription, nameof(fileDescription));

			var docManager = this.DocManagerInfo();
			docManager.SetupEDocsFactoryToBeSavedWithMainFactory(true);

			var newFile = (StorageDocsBase)docManager.AddFileOrDocument((SubStreamableStream)fileStream, Path.GetFileName(fileStream.Name), Core.Constants.RefDocTypes.ComplianceReport, description: fileDescription);
			newFile.SC_IsPublished = true;
			newFile.SC_IsSystemGenerated = true;

			docManager.UseBusinessEntityFactoryAsInternal = true;

			return newFile;
		}
		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region IWorkflowProvider

		public ZString WorkflowType
		{
			get { return WorkflowDescriptors.AccComplianceReportCode; }
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, ACR_ReportType, ZString.Empty);
			return result;
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable]
		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new AccComplianceReportProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		AccComplianceReportProcessTaskCollection workflowItems;

		#endregion

		#region HMRC OAuth Authorisation

		public event EventHandler<OAuthClientAuthorisationEventArgs> OAuthClientAuthorisation;

		public class OAuthClientAuthorisationEventArgs : EventArgs
		{
			public string AuthorisationCode { get; set; }
		}

		internal string InvokeOAuthClientAuthorisationEvent()
		{
			var authorisationEventArgs = new OAuthClientAuthorisationEventArgs();
			OAuthClientAuthorisation?.Invoke(this, authorisationEventArgs);
			return authorisationEventArgs.AuthorisationCode;
		}

		#endregion

		protected string GetComplianceReportLinesBasedOnGroupByAndTablePrefix()
		{
			switch (ReportConfiguration.ReportBaseTablePrefix.ToString())
			{
				case ReportBaseTablePrefixListCodes.AllTransactions:
				case ReportBaseTablePrefixListCodes.GeneralLedgerData:
					return GetAllTransactionsReport();
				case ReportBaseTablePrefixListCodes.ComplianceDocumentHeader:
					return "GetComplianceReportLines_ADH";
				case ReportBaseTablePrefixListCodes.TransactionHeader:
				case ReportBaseTablePrefixListCodes.TransactionLine:
					return GetHeaderAndLineTransactionsReport();
				default:
					throw new NotSupportedException(GetNotSupportExceptionMessage());
			}

			string GetAllTransactionsReport()
			{
				switch (ReportConfiguration.ReportLineGrouping.ToString())
				{
					case ReportLineGroupingListCodes.DayBookWithPresentation:
					case ReportLineGroupingListCodes.DayBook:
						return "GetComplianceReportLines_DAB";
					case ReportLineGroupingListCodes.DayBookWithoutGrouping:
						return "GetComplianceReportLines_DBW";
					case ReportLineGroupingListCodes.NoGrouping:
						return "GetComplianceReportLines_NoGroupHDL";
					default:
						throw new NotSupportedException(GetNotSupportExceptionMessage());
				}
			}

			string GetHeaderAndLineTransactionsReport()
			{
				switch (ReportConfiguration.ReportLineGrouping.ToString())
				{
					case ReportLineGroupingListCodes.NoGrouping:
					case ReportLineGroupingListCodes.TransactionHeaderWithLines:
						return "GetComplianceReportLines_NoGroupHDL";
					case ReportLineGroupingListCodes.TaxReporting:
						return "GetComplianceReportLines_TXR";
					case ReportLineGroupingListCodes.TransactionHeader:
						return "GetComplianceReportLines_HDR";
					case ReportLineGroupingListCodes.Organisation:
						return "GetComplianceReportLines_ORG";
					case ReportLineGroupingListCodes.OrganisationBLCode:
						return "GetComplianceReportLines_OBL";
					case ReportLineGroupingListCodes.OrganisationSubCode:
						return "GetComplianceReportLines_ORS";
					case ReportLineGroupingListCodes.TransactionPayments:
						return "GetComplianceReportLines_TPA";
					case ReportLineGroupingListCodes.PaymentTimesAll:
					case ReportLineGroupingListCodes.PaymentTimesSmallBusinessReportable:
						return "GetComplianceReportLines_PTRPTA";
					case ReportLineGroupingListCodes.TransactionHeaderAndReportSubCode:
					case ReportLineGroupingListCodes.ReportSubCodeAndTransactionHeader:
						return "GetComplianceReportLines_HRSRSH";
					default:
						throw new NotSupportedException(GetNotSupportExceptionMessage());
				}
			}

			string GetNotSupportExceptionMessage()
			{
				return $"Compliance Report does not support such report configuration. Report base table prefix: {ReportConfiguration.ReportBaseTablePrefix.ToString()}, report grouping code: {ReportConfiguration.ReportLineGrouping.ToString()}";
			}
		}

		public virtual string SAFTSingleXmlExportedEventLog => FormattableString.Invariant($"Purpose: SAFT Single XML Exported");

		public virtual string SAFTAnnualXmlExportedEventLog => FormattableString.Invariant($"Purpose: SAFT Annual XML Exported");
	}
}
