using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class PeriodDateRangeField : DateRangeField
	{
		public PeriodDateRangeField(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		internal PeriodDateRangeField(PeriodDateRangeFieldJsonData filter)
			: base(filter)
		{
		}

		protected override void SetNecessaryPropertiesForDeserializingCore(FilterField origin, CollectionOfIFilter filterCollection)
		{
			base.SetNecessaryPropertiesForDeserializingCore(origin, filterCollection);

			if (origin is PeriodDateRangeField originalDateRangeField)
			{
				PeriodCalculator.Company = originalDateRangeField.PeriodCalculator.Company;
			}
		}

		protected override BaseDateRangeFieldJsonData<DateTime> CreateJsonDataCore() => new PeriodDateRangeFieldJsonData();

		protected override void ValidateValueLowCore(ZDateTime valueLow, ZDateTime valueHigh)
		{
			base.ValidateValueLowCore(valueLow, valueHigh);
			if (!ValueLowInfo.HasErrors() && !Scheduled)
			{
				if (!IsDateInValidPeriod(valueLow))
				{
					ValueLowInfo.AddError(InvalidPeriodDateErrorMessageWithCompany);
				}
				else if (!AreDatesInTheSameFinancialYear(valueLow, valueHigh))
				{
					ValueLowInfo.AddError(PeriodDateRangeDifferentYearErrorMessage);
				}
			}
		}

		protected override void ValidateValueHighCore(ZDateTime valueLow, ZDateTime valueHigh)
		{
			base.ValidateValueHighCore(valueLow, valueHigh);
			if (!ValueHighInfo.HasErrors() && !Scheduled)
			{
				if (!IsDateInValidPeriod(valueHigh))
				{
					ValueHighInfo.AddError(InvalidPeriodDateErrorMessageWithCompany);
				}
				else if (!AreDatesInTheSameFinancialYear(valueLow, valueHigh))
				{
					ValueHighInfo.AddError(PeriodDateRangeDifferentYearErrorMessage);
				}
			}
		}

		protected override void SetDependencyValue(string value)
		{
			base.SetDependencyValue(value);
			if (value != null)
			{
				PeriodCalculator.Company = GlbCompany.CurrentCompany;
				var reportingBook = Factory.Load<AccReportingBook>(ZGuid.ParseSafe(value));
				if (reportingBook != null && reportingBook.ARB_GC_CompanyOfPeriod != ZGuid.Empty)
				{
					PeriodCalculator.Company = Factory.Load<GlbCompany>(reportingBook.ARB_GC_CompanyOfPeriod);
				}
			}
		}

		AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (periodCalculator == null)
				{
					periodCalculator = new AccountingPeriodCalculator(Factory);
				}

				return periodCalculator;
			}
		}

		bool IsDateInValidPeriod(ZDateTime dateIn)
		{
			return !dateIn.IsValid || (PeriodCalculator.GetPeriodFromDate(dateIn) != 0);
		}

		bool AreDatesInTheSameFinancialYear(ZDateTime valueLow, ZDateTime valueHigh)
		{
			bool result = true;
			if (valueLow.IsValid && valueHigh.IsValid && IsDateInValidPeriod(valueLow) && IsDateInValidPeriod(valueHigh))
			{
				var periodFrom = PeriodCalculator.GetPeriodFromDate(valueLow).ToString();
				var periodTo = PeriodCalculator.GetPeriodFromDate(valueHigh).ToString();
				if (periodFrom.Substring(0, 4) != periodTo.Substring(0, 4))
				{
					result = false;
				}
			}

			return result;
		}

		AccountingPeriodCalculator periodCalculator;

		protected static string PeriodDateRangeDifferentYearErrorMessage
		{
			get
			{
				return Res.GetString("964f43d7-eddd-4375-a874-540820c3ea9f",
					"Period 'From Date' and 'To Date' must be within a same financial year");
			}
		}

		protected static string InvalidPeriodDateErrorMessage => Res.GetString("2fb0a02b-bfb7-4399-bc2e-041376d23bbe", "The date entered is not in the accounting periods");

		public string InvalidPeriodDateErrorMessageWithCompany
		{
			get
			{
				return PeriodCalculator.Company.PK == GlbCompany.CurrentCompany.PK
					? InvalidPeriodDateErrorMessage
					: Res.GetString("EF1B8941-3ABF-4EEA-9794-DB0E07D46ECB",
						"The date entered is not in the accounting periods of {0}-{1}",
						PeriodCalculator.Company.GC_Code, PeriodCalculator.Company.GC_Name);
			}
		}
	}
}
