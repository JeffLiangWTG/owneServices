using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class DateBasedAccountingPeriodField : AccountingPeriodField
	{
		public DateBasedAccountingPeriodField(BusinessObjectFactory factory)
			: base(factory)
		{
			Init();
		}

		void Init()
		{
			fStartDate = new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.DateTime);
			fEndDate = new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.DateTime);
			fParameterList = new SqlParameterList();
			fParameterList.Add(fStartDate);
			fParameterList.Add(fEndDate);
		}

		protected SqlParameter fStartDate, fEndDate;
		protected SqlParameterList fParameterList;

		#region Constructor For IJsonSerializable

		internal DateBasedAccountingPeriodField(DateBasedAccountingPeriodFieldJsonData data)
			: base(data)
		{
			Init();
		}

		#endregion

		protected override AccountingPeriodFieldJsonData CreateJsonDataCore() => new DateBasedAccountingPeriodFieldJsonData();

		protected override string NonEmptyWhereClause()
		{
			if (UseSinglePeriod || UsePeriodRange || UseYearToPeriod)
			{
				return String.Format((NoResString)@"{0} >= {1} AND {0} <= {2}", FieldName, fStartDate, fEndDate);
			}
			else
			{
				return "";
			}
		}

		protected override SqlParameterList GetSqlParametersCore()
		{
			if (FromDate != ZDateTime.Empty && ToDate != ZDateTime.Empty)
			{
				fStartDate.Value = FromDate.ToDateTime();
				fEndDate.Value = ToDate.ToDateTime();

				return fParameterList;
			}
			else
			{
				return new SqlParameterList();
			}
		}

		AccountingPeriodCalculator fAccountingPeriodCalculator;
		AccountingPeriodCalculator AccountingPeriodCalculator
		{
			get
			{
				if (fAccountingPeriodCalculator == null)
				{
					fAccountingPeriodCalculator = new AccountingPeriodCalculator(Factory);
				}
				return fAccountingPeriodCalculator;
			}
		}

		ZDateTime FromDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (UseSinglePeriod || UsePeriodRange || UseYearToPeriod)
				{
					if (UseSinglePeriod)
					{
						result = AccountingPeriodCalculator.GetFirstDayForPeriod(SinglePeriod);
					}
					else if (UsePeriodRange)
					{
						result = AccountingPeriodCalculator.GetFirstDayForPeriod(FromPeriod);
					}
					else if (UseYearToPeriod)
					{
						result = AccountingPeriodCalculator.GetFirstDayForPeriod(YearStartPeriod);
					}
				}
				return result;
			}
		}

		ZDateTime ToDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (UseSinglePeriod || UsePeriodRange || UseYearToPeriod)
				{
					if (UseSinglePeriod)
					{
						result = AccountingPeriodCalculator.GetLastDayForPeriod(SinglePeriod);
					}
					else if (UsePeriodRange)
					{
						result = AccountingPeriodCalculator.GetLastDayForPeriod(ToPeriod);
					}
					else if (UseYearToPeriod)
					{
						result = AccountingPeriodCalculator.GetLastDayForPeriod(YearToPeriod);
					}
				}
				return result;
			}
		}

		#region ValueProviders stuff
		protected override void AddSpecialisedValueProviders()
		{
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".FromDate", new ValueReplacers.ReplacementProviderMethod(GetFromDateReplacement)));
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".ToDate", new ValueReplacers.ReplacementProviderMethod(GetToDateReplacement)));
		}

		protected override void AddSpecialisedValueProviderDocumenters()
		{
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.FromDate>", ResString.GetMultilingualString("417dc152-d492-4444-838c-59a46d611630", "Returns the From date of the period.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ToDate>", ResString.GetMultilingualString("22045071-5ae3-407e-bf62-e680471cad6a", "Returns the To date of the period.")));
		}

		protected object GetFromDateReplacement(string macro, Report report)
		{
			return FromDate;
		}

		protected object GetToDateReplacement(string macro, Report report)
		{
			return ToDate;
		}
		#endregion
	}
}
