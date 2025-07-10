using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.PeriodManagement
{
	public class PeriodValidation : AccPeriodManagementValidation
	{
		public const int MaximumNumberOfDaysInFinancialYear = 400;

		public static string EndDateLessThanStartDateErrorMessage
		{
			get { return Res.GetString("5f724e71-22ba-4dfa-b663-a8ec4e03c85f", "End date must be equal to or later than the start date"); }
		}

		public static string EndDateEqualOrMoreThanNextPeriodEndDateErrorMessage
		{
			get { return Res.GetString("e3fe9f5e-7fd5-495a-8da8-df121d6a698b", "End date must be earlier than the end date of the next period"); }
		}

		public static string ThereAreNotPeriodsSetupForTheFolowingYear
		{
			get { return Res.GetString("080986E9-251E-44d3-BC87-6C7E831DAF07", "The periods for the following year MUST be setup if you are changing the end date of the final period in a year."); }
		}

		public static string YearMustAroundCurrentFinancialYear
		{
			get { return Res.GetString("0F955B4C-C29C-41E9-A35B-54BB93572135", "Entered year must directly lead, trail or equal to current Financial Year."); }
		}

		public static string YearCanNotBeLessThanPreviousRecordYear
		{
			get { return Res.GetString("88D346C9-2F78-40FF-929B-D0B920E67F83", "Entered Year cannot be earlier than previous Period record."); }
		}

		public static string YearOfPeriodMustBeEqualToYearField
		{
			get { return Res.GetString("817685E1-545B-48F7-9FFD-650B246A305E", "Entered Period must belong to the same year."); }
		}

		public static string PeriodMustBeOneMoreThanPreviousPeriod
		{
			get { return Res.GetString("6B286890-9570-4B73-97E2-1E8C91E1EB49", "Entered period must follow the previous Period record."); }
		}

		public static string PeriodMustBeUnique
		{
			get { return Res.GetString("5F8F2131-AA11-45A2-91B6-68AFCAAF4143", "Entered period already exists in System."); }
		}

		public static string PeriodMustStartFromOne
		{
			get { return Res.GetString("F12659D5-335F-400B-98E1-E158C25A9C97", "That the FIRST PERIOD of a FY must start from 01."); }
		}

		public static string StartEndDateMustBeUnique
		{
			get { return Res.GetString("8205FB83-783C-4EF5-A8A6-8F48FDC271E2", "Entered Period cannot have same Start/End Date as previous Period record"); }
		}

		Period Period => (Period)Parent;

		public PeriodValidation(Period period)
			: base(period)
		{
		}

		protected override void CheckAM_EndDate()
		{
			base.CheckAM_EndDate();
			var endDate = Period.AM_EndDate;
			var endDateInfo = Period.AM_EndDateInfo;

			if (!endDateInfo.HasErrors() && endDate.Date < Period.AM_StartDate.Date)
			{
				endDateInfo.AddError(EndDateLessThanStartDateErrorMessage);
			}
			if (!endDateInfo.HasErrors() && Period.NextPeriod != null && endDate.AddDays(1).Date > Period.NextPeriod?.AM_EndDate.Date)
			{
				endDateInfo.AddError(EndDateEqualOrMoreThanNextPeriodEndDateErrorMessage);
			}

			var firstEndDate = (ZDateTime)endDateInfo.OriginalValue;
			if (!endDateInfo.HasErrors() && Period.LastPeriodOfCurrentYear?.PK == Period.PK)
			{
				var nextYear1stPeriod = Period.FirstPeriodOfNextYear;
				if (nextYear1stPeriod != null)
				{
					if (endDate.AddDays(1).Date > nextYear1stPeriod.AM_EndDate.Date)
					{
						endDateInfo.AddError(EndDateEqualOrMoreThanNextPeriodEndDateErrorMessage);
					}
				}
				else if (endDate != firstEndDate)
				{
					endDateInfo.AddError(ThereAreNotPeriodsSetupForTheFolowingYear);
				}
			}

			if (firstEndDate != endDate)
			{
				bool shrink = false;
				if (firstEndDate > endDate)
				{
					var tempDate = firstEndDate;
					firstEndDate = endDate;
					endDate = tempDate;
					shrink = true;
				}
				if (!endDateInfo.HasErrors())
				{
					if (IsTransactionLineExist(firstEndDate, endDate, shrink))
					{
						endDateInfo.AddError(Res.GetString("2dc46bec-15b2-43aa-b0d4-ce658dc2cdd2", "There are transactions posted between {0} and {1}", firstEndDate.ToShortDateString(), endDate.ToShortDateString()));
					}
					else if (IsTaxGLMovementExist(firstEndDate, endDate))
					{
						endDateInfo.AddError(Res.GetString("910c6e1e-c4f9-4616-9e34-e6f4cc63a73c", "There are tax GL movements between {0} and {1}", firstEndDate.ToShortDateString(), endDate.ToShortDateString()));
					}
					else if (IsOnlyTransactionHeaderExist(firstEndDate, endDate))
					{
						endDateInfo.AddError(Res.GetString("9522F06A-589A-4A4D-9756-1F877119F8E7", "There are transaction headers between {0} and {1}", firstEndDate.ToShortDateString(), endDate.ToShortDateString()));
					}
					else if (IsCashBasisVATExist(firstEndDate, endDate))
					{
						endDateInfo.AddError(Res.GetString("FE6980EE-107B-4ED1-8655-EE52E928AE7D", "There are Cash Basis VATs between {0} and {1}", firstEndDate.ToShortDateString(), endDate.ToShortDateString()));
					}
				}
			}
		}

		protected override void CheckAM_Year()
		{
			base.CheckAM_Year();
			var year = Period.AM_Year;
			var yearInfo = Period.AM_YearInfo;

			MandatoryValidation.CheckEntered(yearInfo);

			var financialYear = Period.PeriodManager?.FinancialYear;
			if (financialYear != null)
			{
				var yearIsAroundFinancialYear = Math.Abs(financialYear.Value - year) <= 1;
				if (!yearInfo.HasErrors() && !yearIsAroundFinancialYear)
				{
					yearInfo.AddError(YearMustAroundCurrentFinancialYear);
				}
			}

			var previousPeriod = GetPreviousPeriod();
			var yearIsNotLessThanPreviousPeriod = previousPeriod == null || year >= previousPeriod.AM_Year;
			if (!yearInfo.HasErrors() && !yearIsNotLessThanPreviousPeriod)
			{
				yearInfo.AddError(YearCanNotBeLessThanPreviousRecordYear);
			}
		}

		protected override void CheckAM_Period()
		{
			base.CheckAM_Period();
			var period = Period.AM_Period;
			var periodInfo = Period.AM_PeriodInfo;

			MandatoryValidation.CheckEntered(periodInfo);

			var year = Period.AM_Year;
			if (!periodInfo.HasErrors() && period / 100 != year)
			{
				periodInfo.AddError(YearOfPeriodMustBeEqualToYearField);
			}

			var previousPeriod = GetPreviousPeriod();
			var periodIsStartFromOne = (period % 10 != 1) && (previousPeriod == null || previousPeriod.AM_Period / 100 != year);
			if (!periodInfo.HasErrors() && periodIsStartFromOne)
			{
				periodInfo.AddError(PeriodMustStartFromOne);
			}

			var periods = Period.PeriodManager?.Periods;
			if (!periodInfo.HasErrors() && periods != null && periods.Cast<Period>().Any(x => Period.PK != x.PK && period == x.AM_Period))
			{
				periodInfo.AddError(PeriodMustBeUnique);
			}

			var periodIsNotOneMoreThanPrevious = previousPeriod != null && previousPeriod.AM_Period / 100 == year && period != previousPeriod.AM_Period + 1;
			if (!periodInfo.HasErrors() && periodIsNotOneMoreThanPrevious)
			{
				periodInfo.AddError(PeriodMustBeOneMoreThanPreviousPeriod);
			}

			if (!periodInfo.HasErrors())
			{
				var query = new ZQuery();
				query.AddToFilter(AccPeriodManagementSchema.AM_StartDate, Parent.AM_StartDate);
				query.AddToFilter(AccPeriodManagementSchema.AM_EndDate, Parent.AM_EndDate);
				query.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, Parent.AM_GC_Company);
				query.AddToFilter(AccPeriodManagementSchema.AM_Period, SQLComparisonOperator.NotEqual, Parent.AM_Period);
				query.AddToFilter(AccPeriodManagementSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.ExistsInDatabase(AccPeriodManagementSchema.Constants.TableName, query))
				{
					periodInfo.AddError(StartEndDateMustBeUnique);
				}
			}
		}

		Period GetPreviousPeriod()
		{
			var periods = Period?.PeriodManager?.Periods;
			if (periods == null)
			{
				return null;
			}

			Period prePeriod = null;
			var sortedPeriods = periods.Cast<Period>().OrderBy(p => p.AM_Period).ToList();
			var periodIndex = sortedPeriods.IndexOf(Period);
			if (periodIndex < 0)
			{
				return null;
			}
			else if (periodIndex == 0)
			{
				prePeriod = Period.GetLastPeriodOfPreviousFinancialYear();
			}
			else
			{
				prePeriod = sortedPeriods[periodIndex - 1];
			}

			return prePeriod;
		}

		bool IsTransactionLineExist(ZDateTime startRange, ZDateTime endRange, bool isPeriodShrunk)
		{
			bool result;
			var excludedTypes = new string[] { TransactionTypes.GLAutoJournal };

			if (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.GetValueWithoutFallback(Period.AM_GC_Company.ToGuid(), Guid.Empty, Guid.Empty))
			{
				result = IsTransactionLineExistCore(startRange, endRange, isPeriodShrunk, null, excludedTypes);
			}
			else
			{
				var journalTypes = new string[] { TransactionTypes.GLStandardJournal, TransactionTypes.GLReversingJournal, TransactionTypes.GLNoteJournal };

				result = IsTransactionLineExistCore(startRange, endRange, isPeriodShrunk, null, excludedTypes.Concat(journalTypes).ToArray());
				if (!result)
				{
					result = IsTransactionLineExistCore(startRange, endRange, isPeriodShrunk, journalTypes, null, excludeDefaultDates: true);
				}
			}
			return result;
		}

		bool IsTransactionLineExistCore(ZDateTime startRange, ZDateTime endRange, bool isPeriodShrunk, string[] includedTypes, string[] excludedTypes, bool excludeDefaultDates = false)
		{
			var postDateQuery = new ZQuery(AccTransactionLinesSchema.AL_PostDate, SQLComparisonOperator.GreaterThan, startRange);
			postDateQuery.AddToFilter(AccTransactionLinesSchema.AL_PostDate, SQLComparisonOperator.LessThanOrEqualTo, excludeDefaultDates && isPeriodShrunk ? endRange.AddDays(-1) : endRange);

			var reverseDateQuery = new ZQuery(AccTransactionLinesSchema.AL_ReverseDate, SQLComparisonOperator.GreaterThan, excludeDefaultDates && !isPeriodShrunk ? startRange.AddDays(1) : startRange);
			reverseDateQuery.AddToFilter(AccTransactionLinesSchema.AL_ReverseDate, SQLComparisonOperator.LessThanOrEqualTo, endRange);

			var dateQuery = new ZQuery();
			dateQuery.AddToFilter(postDateQuery, JoinCondition.Or);
			dateQuery.AddToFilter(reverseDateQuery, JoinCondition.Or);

			var query = new ZDBOnlyQuery(typeof(AccTransactionLines));
			query.AddToFilter(dateQuery);
			query.AddToFilter(AccTransactionLinesSchema.AL_GC, Period.AM_GC_Company);
			if (includedTypes != null)
			{
				query.AddToFilter(AccTransactionLinesSchema.AL_LineType, includedTypes);
			}
			if (excludedTypes != null)
			{
				query.AddToFilter(AccTransactionLinesSchema.AL_LineType, SQLComparisonOperator.NotEqual, excludedTypes);
			}

			return Parent.Factory.Exists(typeof(AccTransactionLines), query);
		}

		bool IsTaxGLMovementExist(ZDateTime startDate, ZDateTime endDate)
		{
			return IsDataExistBetweenDate(AccTaxGLMovementSchema.Constants.TableName, AccTaxGLMovementSchema.ATM_Date, startDate, endDate);
		}

		bool IsOnlyTransactionHeaderExist(ZDateTime startDate, ZDateTime endDate)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			query.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.GreaterThan, startDate);
			query.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.LessThanOrEqualTo, endDate);
			var withoutLineSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH, true);
			query.AddSubQuery(withoutLineSubQuery, JoinCondition.And);

			return Parent.Factory.ExistsInDatabase(AccTransactionHeaderSchema.Constants.TableName, query);
		}

		bool IsCashBasisVATExist(ZDateTime startDate, ZDateTime endDate)
		{
			return IsDataExistBetweenDate(AccCashBasisVATSchema.Constants.TableName, AccCashBasisVATSchema.YC_PostDate, startDate, endDate);
		}

		bool IsDataExistBetweenDate(string tableName, SchemaColumn schemaColumn, ZDateTime startDate, ZDateTime endDate)
		{
			var query = new ZQuery();
			query.AddToFilter(schemaColumn, SQLComparisonOperator.GreaterThan, startDate);
			query.AddToFilter(schemaColumn, SQLComparisonOperator.LessThanOrEqualTo, endDate);
			return Parent.Factory.ExistsInDatabase(tableName, query);
		}

		protected override void CheckAM_EndDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckAM_StartDateIsValidZDateTimeRange()
		{
		}
	}
}
