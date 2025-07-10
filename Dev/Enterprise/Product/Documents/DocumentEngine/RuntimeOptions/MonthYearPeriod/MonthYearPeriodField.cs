using System;
using System.Linq;
using System.Runtime.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class MonthYearPeriodField : FilterFieldValueSerialisable, ISerializable, IJsonSerializable
	{
		public MonthYearPeriodField(BusinessObjectFactory factory)
			: base(factory)
		{
			CreateParameters();
		}

		#region Constructor For IJsonSerializable

		internal MonthYearPeriodField(MonthYearPeriodJsonData data)
			: base(data)
		{
			CreateParameters();

			Month = data.Month;
			Year = data.Year;
		}

		#endregion

		void CreateParameters()
		{
			fMonth = new SqlParameter(SqlParameterNameGenerator.Next(), System.Data.SqlDbType.Int);
			fYear = new SqlParameter(SqlParameterNameGenerator.Next(), System.Data.SqlDbType.Int);
			ParameterList.Add(fMonth);
			ParameterList.Add(fYear);
		}

		SqlParameter fMonth, fYear;

		public ZInt Month
		{
			get
			{
				return ValueIsNull(fMonth.Value) ? ZInt.Zero : new ZInt(fMonth.Value);
			}
			set
			{
				SetHasChangesBecauseTheValueHasBeenSetByTheUser();
				fMonth.Value = ((IZTypeInternals)value).GetValueForLogicalDataLayer(true);
				if (!IsValidationSuspended)
				{
					ValidateMonthAndYear();
				}
				MonthInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo MonthInfo
		{
			get { return GetZPropertyInfo(nameof(Month)); }
		}

		public ZInt Year
		{
			get
			{
				return ValueIsNull(fYear.Value) ? ZInt.Zero : new ZInt(fYear.Value);
			}
			set
			{
				SetHasChangesBecauseTheValueHasBeenSetByTheUser();
				fYear.Value = ((IZTypeInternals)value).GetValueForLogicalDataLayer(true);
				if (!IsValidationSuspended)
				{
					ValidateMonthAndYear();
				}
				YearInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo YearInfo
		{
			get { return GetZPropertyInfo(nameof(Year)); }
		}

		bool ValueIsNull(object value)
		{
			return (value == null || value == DBNull.Value);
		}

		void ValidateMonthAndYear()
		{
			MonthInfo.ClearAllNotifications();
			YearInfo.ClearAllNotifications();

			if (!IsValid)
			{
				MonthInfo.AddError(ValidationError);
				YearInfo.AddError(ValidationError);
			}
			else
			{
				if (!Month.IsEmpty)
				{
					if (Month < 1 || Month > 12)
					{
						MonthInfo.AddError(Res.GetString("b288d51f-6d06-48bd-8f32-6ecee5d0bc8d", "Invalid value for month."));
					}
					if (Year.IsEmpty)
					{
						YearInfo.AddError(Res.GetString("c3d06152-80ca-45aa-8e5a-5bc4a8d72cb9", "Please enter year."));
					}
				}
				if (!Year.IsEmpty)
				{
					if (Year < ZDateTime.MinSmallDateTimeValue.Year || Year > ZDateTime.MaxSmallDateTime.Year)
					{
						YearInfo.AddError(Res.GetString("6e6de846-596f-4493-a18a-2409c60ec3b2", "Invalid value for year. Please use value between {0} and {1}.", ZDateTime.MinSmallDateTimeValue.Year, ZDateTime.MaxSmallDateTime.Year));
					}
					if (Month.IsEmpty)
					{
						MonthInfo.AddError(Res.GetString("131ae6d4-bc4e-49c9-84fc-c7d559721856", "Please enter month."));
					}
				}
			}
		}

		#region Implementation

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType => FilterFieldSuggestedUserControlType.MonthYearPeriodUserControl;

		public override bool IsEmpty => Month == ZInt.Zero || Year == ZInt.Zero;

		public override object ValueAsObject => Res.GetString("25d2df6b-9e6c-4666-95be-8580b710c04d", "{0:00}/{1:0000}", Month, Year);

		protected override string ValueAsStringForSerialisationInternal
		{
			get
			{
				return string.Format("{0:00}/{1:0000}", Month, Year);
			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					Month = Year = 0;
				}
				else
				{
					try
					{
						var values = value.Split('/');
						Month = ZInt.Parse(values[0]);
						Year = ZInt.Parse(values[1]);
					}
					catch
					{
						Month = Year = 0;
					}
				}
			}
		}

#if DEBUG

		public override void ClearValueForUnitTest()
		{
			ClearValues();
		}

#endif

		public override void ClearValues()
		{
			Month = Year = 0;
		}

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			var filterData = new MonthYearPeriodFilter();
			SetBaseFilterData(filterData);

			filterData.Year = Year;
			filterData.Month = Month;

			reportFilterData.MonthYearPeriodFilterCollection.Add(filterData);
		}

		public override void SafeCopyValuesFrom(IFilter source)
		{
			if (source is MonthYearPeriodField periodField)
			{
				this.Month = periodField.Month;
				this.Year = periodField.Year;
			}
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = reportFilterData.MonthYearPeriodFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
			if (selectedValue != null)
			{
				Month = selectedValue.Month;
				Year = selectedValue.Year;
			}
		}

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			return true;
		}

		protected override string NonEmptyWhereClause()
		{
			return $"{FilterBuilderPropertyCodeDescriptionList.Codes.Month} = {fMonth} AND {FilterBuilderPropertyCodeDescriptionList.Codes.Year} = {fYear}";
		}

		protected override void AddSpecialisedValueProviders()
		{
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".Month", new ValueReplacers.ReplacementProviderMethod(GetMonthValue)));
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".Year", new ValueReplacers.ReplacementProviderMethod(GetYearValue)));
		}

		protected override void AddSpecialisedValueProviderDocumenters()
		{
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.Month>", ResString.GetMultilingualString("5b0c9a4d-bf7e-4a52-b565-4750d2ca35b2", "Returns the Month value.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.Year>", ResString.GetMultilingualString("4f8bdc82-0ddc-4f6e-8f01-ebeb8cfc1092", "Returns the Year value.")));
		}

		protected object GetMonthValue(string macro, Report report)
		{
			return Month;
		}

		protected object GetYearValue(string macro, Report report)
		{
			return Year;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateMonthAndYear();
		}

		#endregion

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var result = new MonthYearPeriodJsonData()
			{
				Month = Month,
				Year = Year
			};
			SetJsonData(result);
			return result;
		}

		#endregion

		#region ISerializable Members

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("DisplayName", DisplayName);
			info.AddValue((NoResString)"Month", Month);
			info.AddValue((NoResString)"Year", Year);
		}

		#endregion
	}
}
