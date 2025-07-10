using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public static class DataInterfaceUtils
	{
		public const string DOT = ".";

		public static ZString GetGLAccountNoWithNoTrailingZero(BusinessObjectFactory factory, ZGuid accountPK)
		{
			ZString result = ZString.Empty;
			AccGLHeader header = factory.Load<AccGLHeader>(accountPK);
			if (header != null)
			{
				result = RemoveTrailingZero(header.AG_AccountNum);
			}

			return result;
		}

		public static ZString GetLocalAccountNoWithNoTrailingZero(BusinessObjectFactory factory, ZGuid accountPK)
		{
			ZString accountNumber = GetLocalAccountNo(factory, accountPK);
			accountNumber = RemoveTrailingZero(accountNumber);
			return accountNumber;
		}

		public static ZString GetLocalAccountNo(BusinessObjectFactory factory, ZGuid accountPK)
		{
			ZString accountNumber = "";

			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(AccGLAccountDescriptor));
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, GetLocalLanguage());
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, SQLComparisonOperator.Equal, AccGLAccountDescriptor.ReportTypeCOA);
			if (accountPK.IsValid)
			{
				ZDBOnlySubQuery accGLDescriptorPivotSubQuery = new ZDBOnlySubQuery(typeof(AccGLDescriptorPivot), AccGLDescriptorPivotSchema.YJ_AJ);
				accGLDescriptorPivotSubQuery.AddToFilter(AccGLDescriptorPivotSchema.YJ_AG, SQLComparisonOperator.Equal, accountPK);
				filter.AddSubQuery(AccGLAccountDescriptorSchema.PK, accGLDescriptorPivotSubQuery, JoinCondition.And);

				AccGLAccountDescriptor accountDescriptor = factory.LoadTop1(typeof(AccGLAccountDescriptor), filter) as AccGLAccountDescriptor;

				if (accountDescriptor != null)
				{
					accountNumber = accountDescriptor.AJ_LocalAccountNumber;
				}
			}
			return accountNumber;
		}

		public static ZString RemoveTrailingZero(ZString number)
		{
			ZInt trailingPosition = number.LastIndexOf(DOT);
			ZDecimal tralingValue = 0m;
			if (ZDecimal.TryParse(number.Right(trailingPosition), out tralingValue))
			{
				if (tralingValue == 0m)
				{
					return number.Left(trailingPosition);
				}
			}

			return number;
		}

		public static string GetLocalLanguage()
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.China)
			{
				return Constants.Languages.ChineseSimplified;
			}

			return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Taiwan ? Constants.Languages.ChineseTraditional : Constants.Languages.English;
		}

		public static ZString GetLocalAccountDescription(BusinessObjectFactory factory, ZGuid accountPK)
		{
			ZString accountDescription = "";

			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(AccGLAccountDescriptor));
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, GetLocalLanguage());
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, SQLComparisonOperator.Equal, AccGLAccountDescriptor.ReportTypeCOA);

			ZDBOnlySubQuery accGLDescriptorPivotSubQuery = new ZDBOnlySubQuery(typeof(AccGLDescriptorPivot), AccGLDescriptorPivotSchema.YJ_AJ);

			accGLDescriptorPivotSubQuery.AddToFilter(AccGLDescriptorPivotSchema.YJ_AG, SQLComparisonOperator.Equal, accountPK);

			filter.AddSubQuery(AccGLAccountDescriptorSchema.PK, accGLDescriptorPivotSubQuery, JoinCondition.And);

			AccGLAccountDescriptor accountDescriptor = factory.LoadTop1(typeof(AccGLAccountDescriptor), filter) as AccGLAccountDescriptor;

			if (accountDescriptor != null)
			{
				accountDescription = accountDescriptor.AJ_AccountDescription;
			}
			return accountDescription;
		}

		public static ZString GetLocalAccountDescription(BusinessObjectFactory factory, ZString localAccountNo)
		{
			ZString accountDescription = "";

			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(AccGLAccountDescriptor));
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, GetLocalLanguage());
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, SQLComparisonOperator.Equal, AccGLAccountDescriptor.ReportTypeCOA);
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, SQLComparisonOperator.Equal, localAccountNo);

			AccGLAccountDescriptor accountDescriptor = factory.LoadTop1(typeof(AccGLAccountDescriptor), filter) as AccGLAccountDescriptor;

			if (accountDescriptor != null)
			{
				accountDescription = accountDescriptor.AJ_AccountDescription;
			}
			return accountDescription;
		}

		public static ZString GetAccountDescription(ZGuid accountPK)
		{
			return GetLocalAccountNoWithNoTrailingZero(new BusinessObjectFactory(), accountPK);
		}

		public static ZString GetVoucherNumberFowWIPAccrual(ZString lineType, ZInt period)
		{
			ZString voucherNumber = "";
			if (lineType == TransactionLineTypes.WIP)
			{
				voucherNumber = new ZString(period.ToString()).Right(4) + "001000";
			}
			else if (lineType == TransactionLineTypes.Accrual)
			{
				voucherNumber = new ZString(period.ToString()).Right(4) + "001000";
			}
			return voucherNumber;
		}

		public static int GetFirstPeriodOfTheYear(int year, BusinessObjectFactory factory)
		{
			AccountingPeriodCalculator periodCalc = new AccountingPeriodCalculator(factory);
			return periodCalc.GetFirstPeriodForYear(year);
		}

		public static int GetYearFromPeriod(int period, BusinessObjectFactory factory)
		{
			int year = 0;
			AccountingPeriodCalculator periodCalc = new AccountingPeriodCalculator(factory);
			ZDateTime firstDay = periodCalc.GetFirstDayForPeriod(period);
			if (firstDay.IsValid)
			{
				year = firstDay.Year;
			}
			return year;
		}

		public static AccPeriodManagement[] GetPeriodRangeExcludingCurrentPeriod(int fromPeriod, int toPeriod, BusinessObjectFactory factory)
		{
			ZQuery periodFilter = new ZQuery();
			AccountingPeriodCalculator calculator = new AccountingPeriodCalculator(factory);
			periodFilter.AddToFilter(AccPeriodManagementSchema.AM_Period, SQLComparisonOperator.GreaterThanOrEqualTo, fromPeriod);
			periodFilter.AddToFilter(AccPeriodManagementSchema.AM_Period, SQLComparisonOperator.LessThanOrEqualTo, toPeriod);
			periodFilter.AddToFilter(AccPeriodManagementSchema.AM_Period, SQLComparisonOperator.NotEqual, calculator.GetPeriodFromDate(ZDateTime.Now));
			periodFilter.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			AccPeriodManagement[] periods = factory.Load(typeof(AccPeriodManagement), periodFilter) as AccPeriodManagement[];
			return periods;
		}

		public static AccPeriodManagement[] GetPeriodRange(int fromPeriod, int toPeriod, BusinessObjectFactory factory)
		{
			ZQuery periodFilter = new ZQuery();
			periodFilter.AddToFilter(AccPeriodManagementSchema.AM_Period, SQLComparisonOperator.GreaterThanOrEqualTo, fromPeriod);
			periodFilter.AddToFilter(AccPeriodManagementSchema.AM_Period, SQLComparisonOperator.LessThanOrEqualTo, toPeriod);
			periodFilter.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			AccPeriodManagement[] periods = factory.Load(typeof(AccPeriodManagement), periodFilter) as AccPeriodManagement[];
			return periods;
		}

		public static ZString GetVoucherDescription(ZString headerDescription, ZString transactionLineDescription, ZString jobNumber,ZBool includeJobNumber)
		{
			ZString descriptionResult = headerDescription;
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China)
			{
				if (!transactionLineDescription.IsEmpty)
				{
					descriptionResult += " - " + transactionLineDescription;
				}
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan)
			{
				descriptionResult = transactionLineDescription;
			}

			if (includeJobNumber && !jobNumber.IsEmpty)
			{
				descriptionResult += " " + Res.GetString("83EE58A6-099C-453a-BA9C-2FF0A23D6BE0", "Job:") + " " + jobNumber;
			}

			return descriptionResult.Trim(' ');
		}
	}
}
