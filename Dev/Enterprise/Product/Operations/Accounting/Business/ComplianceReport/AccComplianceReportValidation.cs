//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccComplianceReportValidation
//
//    This class should be used for overriding validation in AutoAccComplianceReportValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport.HMRC;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class AccComplianceReportValidation : AutoAccComplianceReportValidation
	{
		public AccComplianceReportValidation(AutoAccComplianceReport parent) : base(parent)
		{
		}

		protected override void CheckACR_GC_Company()
		{
			base.CheckACR_GC_Company();
			MandatoryValidation.CheckEntered(Parent.ACR_GC_CompanyInfo);
			TypeValidation.CheckValidGuid(Parent.ACR_GC_CompanyInfo);
		}

		protected override void CheckACR_ReportType()
		{
			base.CheckACR_ReportType();
			MandatoryValidation.CheckEntered(Parent.ACR_ReportTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ACR_ReportTypeInfo, Parent.Lookups.ReportTypeList);
			if (!Parent.ACR_ReportTypeInfo.HasErrors() && CheckForOverlapingReports())
			{
				Parent.ACR_ReportTypeInfo.AddError(OverlappingReportDatesErrorMessage);
			}
			if (!Parent.ACR_ReportTypeInfo.HasErrors() && Parent.IncludeQueuedForPreviousPeriod && !Parent.IsPreviousReportFinalized())
			{
				Parent.ACR_ReportTypeInfo.AddError(Res.GetString("820a908f-5080-4d43-95bc-5fddf3c89661", "You cannot create a new report until the previous report is finalized."));
			}
		}

		public void ValidateAccountingPeriod()
		{
			ValidateCalculatedProperty(Parent.AccountingPeriodInfo);
		}

		protected void CheckAccountingPeriod()
		{
			if (!Parent.AccountingPeriodInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.AccountingPeriodInfo, Res.GetString("20FC0CB5-0CEE-4714-8947-158267B63608", "'{0}' to set 'Date From' and 'Date To' values", Parent.AccountingPeriodCaptionResString.Caption));
				if (!Parent.AccountingPeriodInfo.HasErrors() && Parent.UseFinancialYear)
				{
					if (!ZShort.TryParse(Parent.AccountingPeriod.ToString(), out _) // Value is conveted to short on passing to ZQuery filter inside of the GetFirstPeriodForYear method
						|| PeriodCalculator.GetFirstPeriodForYear(Parent.AccountingPeriod) == 0)
					{
						Parent.AccountingPeriodInfo.AddError(Res.GetString("3B74229C-D9A8-4391-8260-5FB45C212132", "Period management does not have defined period for year {0} or value is not valid.", Parent.AccountingPeriod));
					}
				}
				else if (!Parent.AccountingPeriodInfo.HasErrors() && Parent.UseComplianceFinancialYear)
				{
					if (!ZShort.TryParse(Parent.AccountingPeriod.ToString(), out _))
					{
						Parent.AccountingPeriodInfo.AddError(Res.GetString("FE18C772-BBFA-4ABE-AC7E-36C84C9EC045", "Value is not valid.", Parent.AccountingPeriod));
					}
				}
			}

			if (Parent.ReportingBook != null)
			{
				CheckPeriodWithReportingBook();
			}
		}

		void CheckPeriodWithReportingBook()
		{
			if (Parent.ACR_Periodicity == ReportPeriodicityCodes.AccountingPeriod)
			{
				var reportingBookPeriodCompany = Parent.Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, Parent.ReportingBook?.ARB_GC_CompanyOfPeriod));
				var company = reportingBookPeriodCompany ?? Parent.Company;

				if (IsExcludedByPeriod(company))
				{
					Parent.AccountingPeriodInfo.AddError(Res.GetString("78DD24C2-D082-42EF-9BBD-F71C43E1E8B9", "The Period value specified does not exist in '{0}' system company.", company.GC_Code));
				}
			}
		}

		bool IsExcludedByPeriod(GlbCompany company)
		{
			var periodCalculator = new AccountingPeriodCalculator(Parent.Factory, company);
			return !periodCalculator.IsPeriodValid(Parent.AccountingPeriod);
		}

		protected override void CheckACR_DateFrom()
		{
			base.CheckACR_DateFrom();
			if (!Parent.ACR_DateFromInfo.HasErrors())
			{
				CompareValidation.CheckDateIsNotAfterAnotherDate(Parent.ACR_DateFromInfo, Parent.ACR_DateToInfo);
			}
			if (!Parent.ACR_DateFromInfo.HasErrors() && CheckForOverlapingReports())
			{
				Parent.ACR_DateFromInfo.AddError(OverlappingReportDatesErrorMessage);
			}
			if (!Parent.ACR_DateFromInfo.HasErrors() && ShouldCheckMTDDateRange)
			{
				if (IsGroupReportingCompany)
				{
					var mtdDateRangeError = CheckMTDReportDateRange();
					if (!string.IsNullOrEmpty(mtdDateRangeError.Error))
					{
						Parent.ACR_DateFromInfo.AddError(mtdDateRangeError.Error);
					}
				}
				else if (!DoesReportExistForGroupReportingCompany())
				{
					Parent.ACR_DateFromInfo.AddError(ReportDoesNotExistForGroupReportingCompanyErrorMessage);
				}
			}

			if (!Parent.ACR_DateFromInfo.HasErrors() && ShouldCheckDateRangeMatchesAccountingPeriods)
			{
				var periodManagementOfStartDate = PeriodCalculator.GetPeriodManagementFromDate(Parent.ACR_DateFrom);
				if (periodManagementOfStartDate == null ||
				periodManagementOfStartDate.AM_StartDate != Parent.ACR_DateFrom)
				{
					Parent.ACR_DateFromInfo.AddError(Res.GetString("5CE0C036-B547-4656-ABF3-35F3DCBEB492", "The 'Date From' must be 'Start Date' of an Accounting Period."));
				}
			}

			if (!Parent.ACR_DateFromInfo.HasErrors() && Parent.ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.GeneralLedgerData && Parent.ACR_DateFrom.IsValid)
			{
				var lastProcessedDate = AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.GetValueWithoutFallback(Parent.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);	
				if (lastProcessedDate == DateTime.MinValue || Parent.ACR_DateFrom.ToDateTime() < lastProcessedDate)
				{
						Parent.ACR_DateFromInfo.AddError(Res.GetString("913C8511-6433-4D1A-A8E6-7C13A15BD8C4", "The Date From should be equal to or later than the date of the 'Journal Entries Last Processed Date' registry. Please ensure journal entries for all accounting transactions posted within the compliance reporting periods have been generated."));
				}
			}
		}

		ZBool ShouldCheckDateRangeMatchesAccountingPeriods => Parent.ReportBaseTablePrefix == ReportBaseTablePrefixListCodes.ComplianceDocumentHeader || Parent.ACR_Periodicity == ReportPeriodicityCodes.RangeAccountingPeriod;

		AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (fPeriodCalculator == null)
				{
					fPeriodCalculator = new AccountingPeriodCalculator(Parent.Factory, GlbCompany.CurrentCompany);
				}
				return fPeriodCalculator;
			}
		}
		AccountingPeriodCalculator fPeriodCalculator;

		protected override void CheckACR_DateTo()
		{
			base.CheckACR_DateTo();
			if (!Parent.ACR_DateToInfo.HasErrors() && Parent.ACR_DateFrom.IsValid)
			{
				CompareValidation.CheckDateIsNotBeforeAnotherDate(Parent.ACR_DateToInfo, Parent.ACR_DateFrom.ToZDateTime());
			}
			if (!Parent.ACR_DateToInfo.HasErrors() && CheckForOverlapingReports())
			{
				Parent.ACR_DateToInfo.AddError(OverlappingReportDatesErrorMessage);
			}
			if (!Parent.ACR_DateToInfo.HasErrors() && ShouldCheckMTDDateRange)
			{
				if (IsGroupReportingCompany)
				{
					var mtdDateRangeError = CheckMTDReportDateRange();
					if (!string.IsNullOrEmpty(mtdDateRangeError.Error))
					{
						Parent.ACR_DateToInfo.AddError(mtdDateRangeError.Error);
					}
				}
				else if (!DoesReportExistForGroupReportingCompany())
				{
					Parent.ACR_DateToInfo.AddError(ReportDoesNotExistForGroupReportingCompanyErrorMessage);
				}
			}

			if (!Parent.ACR_DateToInfo.HasErrors() && ShouldCheckDateRangeMatchesAccountingPeriods)
			{
				var periodManagementOfEndDate = PeriodCalculator.GetPeriodManagementFromDate(Parent.ACR_DateTo);
				if (periodManagementOfEndDate == null ||
				periodManagementOfEndDate.AM_EndDate.Date != Parent.ACR_DateTo)
				{
					Parent.ACR_DateToInfo.AddError(Res.GetString("E8774C5F-AF81-41B6-8A81-2CBCE7BCF0D3", "The 'Date To' must be 'End Date' of an Accounting Period."));
				}
			}
		}

		protected override void CheckACR_ARB_ReportingBook()
		{
			base.CheckACR_ARB_ReportingBook();

			if (Parent.ReportingBook != null)
			{
				CheckBackLog();
				CheckReportingBookCurrency();
				CheckReportingBookNonGlobal();
				CheckEDWServer();
			}
		}

		void CheckBackLog()
		{
			var lastProcessedDate = AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.GetValueWithoutFallback(Parent.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var sqlQuery = new ZDBOnlyQuery(typeof(AccPeriodManagement));
			sqlQuery.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK);
			sqlQuery.OrderBy = AccPeriodManagementSchema.Constants.AM_StartDate;
			var period = Parent.Factory.LoadTop1<AccPeriodManagement>(sqlQuery);

			if (lastProcessedDate == DateTime.MinValue || period.AM_StartDate < lastProcessedDate)
			{
				Parent.ACR_ARB_ReportingBookInfo.AddError(Res.GetString("ECB368A6-40E0-4203-B72E-8D7F34760A3B", "Reporting Book can only be selected after all backlog accounting transactions have been processed."));
			}
		}

		void CheckReportingBookCurrency()
		{
			if (!Parent.ReportingBook?.IsLocalCurrency ?? false)
			{
				Parent.ACR_ARB_ReportingBookInfo.AddError(Res.GetString("AA5B1BB4-3AAD-4205-A60F-BA39BAF3B4AB", "Only Reporting Book in Local Reporting Currency can be selected."));
			}
		}

		void CheckReportingBookNonGlobal()
		{
			var isGlobalReportingBook = Parent.ReportingBook?.ARB_IsGlobal ?? false;
			if (isGlobalReportingBook)
			{
				Parent.ACR_ARB_ReportingBookInfo.AddError(Res.GetString("8529C21B-22A5-40B3-98BD-D783B587E71D", "Only Non-Global Reporting Book can be selected."));
			}
		}

		void CheckEDWServer()
		{
			if (!Business.AccountingUtils.IsEDWEnabled())
			{
				Parent.ACR_ARB_ReportingBookInfo.AddError(Res.GetString("A899F91D-0BD6-49EC-8554-9EB34A5E51AC", "No Enterprise Data Warehouse server found. Please raise an eRequest."));
			}
		}

		bool ShouldCheckMTDDateRange => Parent.ACR_DateFrom.IsValid && Parent.ACR_DateTo.IsValid &&
				Parent.HasContext(AccComplianceReport.BusinessContext.PreSavingValidation) &&
				(!Parent.IsInDatabase || (Parent.ACR_DateFromInfo.HasChanges || Parent.ACR_DateToInfo.HasChanges)) &&
				Parent.Company.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedKingdom &&
				Parent.ACR_ReportType == ComplianceReportTypes.MakeTaxDigitalReportType;

		bool IsGroupReportingCompany => AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.Value == Guid.Empty;

		public (string PeriodKey, bool IsPeriodOpen, ZDateTime DueOn, ZDateTime FulfilledOn, string Error) CheckMTDReportDateRange()
		{
			var errorMessage = string.Empty;

			MTDObligation openObligation = null;
			MTDObligation fulfilledObligation = null;

			if (MTDDateRangeCheckResult.HasValue)
			{
				errorMessage = MTDDateRangeCheckResult.Value;
				MTDDateRangeCheckResult = null;
			}
			else
			{
				var dateFrom = Parent.ACR_DateFrom.ToMTDCompliantFormat();
				var dateTo = Parent.ACR_DateTo.ToMTDCompliantFormat();

				var client = new MTDClient(Parent);
				var error = string.Empty;
				var obligationRequest = new MTDGetObligationsRequest(client);
				var obligations = client.SendRequest<MTDObligations>(obligationRequest);

				openObligation = obligations?.obligations.FirstOrDefault(o => o.status == MTDObligationStatus.Open && o.start == dateFrom && o.end == dateTo);
				fulfilledObligation = obligations?.obligations.FirstOrDefault(o => o.status == MTDObligationStatus.Fulfilled && o.start == dateFrom && o.end == dateTo);

				if ((client.HasErrors() && client.LastErrorInfo.IsInvalidDateRangeError) || (!client.HasErrors() && openObligation == null))
				{
					error = Res.GetString("2c2c4870-2c07-4ad2-bc21-ab898b5ff0c5", "The dates entered are not eligible for submission of a VAT return. They should be as per your open VAT reporting period.");
				}
				else if (client.HasErrors() && client.LastErrorInfo.IsAuthorizationRelatedError)
				{
					error = Res.GetString("507d9548-3ae9-4950-a5eb-f8af85f136a3", "The entered credentials may be for an incorrect VRN or CW1 was not granted access. Please try again.");
				}
				else if (client.HasErrors())
				{
					error = client.LastErrorInfo.ToString();
				}

				errorMessage = MTDDateRangeCheckResult = error;
			}

			var result = ProcessObligations(openObligation ?? fulfilledObligation);

			return (result.PeriodKey, result.IsPeriodOpen, result.DueOn, result.FulfilledOn, errorMessage);
		}

		(string PeriodKey, bool IsPeriodOpen, ZDateTime DueOn, ZDateTime FulfilledOn) ProcessObligations(MTDObligation obligation)
		{
			var key = obligation?.periodKey ?? string.Empty;
			var isOpen = (obligation?.status ?? string.Empty) == MTDObligationStatus.Open;

			var zReceivedDate = ZDateTime.Invalid;
			if (DateTime.TryParseExact((obligation?.received ?? string.Empty), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime received))
			{
				zReceivedDate = received;
			}

			var zDueDate = ZDateTime.Invalid;
			if (DateTime.TryParseExact((obligation?.due ?? string.Empty), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime due))
			{
				zDueDate = due;
			}

			return (key, isOpen, zDueDate, zReceivedDate);
		}

		ZString? MTDDateRangeCheckResult;

		bool DoesReportExistForGroupReportingCompany()
		{
			if (!Parent.ACR_ReportType.IsEmpty && !Parent.ACR_DateFrom.IsEmpty && !Parent.ACR_DateTo.IsEmpty)
			{
				var filter = new ZQuery(AccComplianceReportSchema.ACR_GC_Company, AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.Value)
					.AddToFilter(AccComplianceReportSchema.ACR_ReportType, Parent.ACR_ReportType)
					.AddToFilter(AccComplianceReportSchema.ACR_DateFrom, SQLComparisonOperator.EqualToDatePartOnly, Parent.ACR_DateFrom)
					.AddToFilter(AccComplianceReportSchema.ACR_DateTo, SQLComparisonOperator.EqualToDatePartOnly, Parent.ACR_DateTo);
				return Parent.Factory.Exists(typeof(AccComplianceReport), filter);
			}
			return false;
		}

		bool CheckForOverlapingReports()
		{
			var result = false;

			if (!Parent.ACR_GC_Company.IsEmpty && !Parent.ACR_ReportType.IsEmpty &&
				!Parent.ACR_DateFrom.IsEmpty && !Parent.ACR_DateTo.IsEmpty)
			{
				var filter = new ZQuery(AccComplianceReportSchema.ACR_GC_Company, Parent.ACR_GC_Company);
				filter.AddToFilter(AccComplianceReportSchema.ACR_GB_Branch, Parent.ACR_GB_Branch.IsEmpty ? DBNull.Value : Parent.ACR_GB_Branch);
				filter.AddToFilter(AccComplianceReportSchema.ACR_ReportType, Parent.ACR_ReportType);
				filter.AddToFilter(AccComplianceReportSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				var dateFilter = new ZQuery();
				dateFilter.DefaultJoinCondition = JoinCondition.Or;

				var dateFromInsideFilter = new ZQuery(AccComplianceReportSchema.ACR_DateFrom, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, Parent.ACR_DateFrom);
				dateFromInsideFilter.AddToFilter(AccComplianceReportSchema.ACR_DateFrom, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, Parent.ACR_DateTo);
				dateFilter.AddToFilter(dateFromInsideFilter);

				var dateToInsideFilter = new ZQuery(AccComplianceReportSchema.ACR_DateTo, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, Parent.ACR_DateTo);
				dateToInsideFilter.AddToFilter(AccComplianceReportSchema.ACR_DateTo, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, Parent.ACR_DateFrom);
				dateFilter.AddToFilter(dateToInsideFilter);

				var includesIntervalFilter = new ZQuery(AccComplianceReportSchema.ACR_DateFrom, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, Parent.ACR_DateFrom);
				includesIntervalFilter.AddToFilter(AccComplianceReportSchema.ACR_DateTo, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, Parent.ACR_DateTo);
				dateFilter.AddToFilter(includesIntervalFilter);

				filter.AddToFilter(dateFilter);

				result = Parent.Factory.LoadTop1<AccComplianceReport>(filter) != null;
			}

			return result;
		}

		string OverlappingReportDatesErrorMessage
		{
			get { return Res.GetString("9c99c3fc-c9fd-4092-8efc-564cde324f60", "There is another Report of '{0}' Type which overlaps your date range.", Parent.ACR_ReportType); }
		}

		string ReportDoesNotExistForGroupReportingCompanyErrorMessage => Res.GetString("0c89e1b9-6c71-4646-82e0-42d93600869b", "The Start and End dates for the compliance report for a VAT group member must match the dates for the compliance report created in the VAT group reporting company.");

		public new AccComplianceReport Parent => (AccComplianceReport)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAccountingPeriod();
		}
	}
}
