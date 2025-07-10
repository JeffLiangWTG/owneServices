using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class AccountingNumberRangeField : FilterFieldWithUTSupport, IJsonSerializable
	{
		public string InvalidNumberErrorMessage
		{
			get { return Res.GetString("9B3E60DB-6412-4666-A742-8AFA64730AFF", "{0} is out of range.", DisplayName); }
		}

		public string NumberFromGreaterThanNumberToErrorMessage
		{
			get { return Res.GetString("05D3F475-3960-4AF8-8FB9-9BEF12C5D1D0", "The '{0} From' must be less than '{0} To'.", DisplayName); }
		}

		public string NumberToLessThanNumberFromErrorMessage
		{
			get { return Res.GetString("74322377-026A-46B7-8E2F-70AF5AE41082", "The '{0} To' must be greater than '{0} From'.", DisplayName); }
		}

		public AccountingNumberRangeField(BusinessObjectFactory factory)
			: base(factory)
		{
			CreateParameters();
		}

		#region Constructor For IJsonSerializable

		internal AccountingNumberRangeField(AccountingNumberRangeFieldJsonData data)
			: base(data)
		{
			CreateParameters();

			From = data.From;
			To = data.To;
		}

		#endregion

		void CreateParameters()
		{
			fFromParam = new SqlParameter(SqlParameterNameGenerator.Next(), DBNull.Value);
			fToParam = new SqlParameter(SqlParameterNameGenerator.Next(), DBNull.Value);
			ParameterList.Add(fFromParam);
			ParameterList.Add(fToParam);
		}

		SqlParameter fFromParam, fToParam;

		public ZString From
		{
			get { return (fFromParam.Value == DBNull.Value) ? string.Empty : fFromParam.Value as string; }
			set
			{
				if (fFromParam.Value == DBNull.Value || From != value)
				{
					string stringValue = ((IZTypeInternals)value).GetValueForLogicalDataLayer(true) as string;
					fFromParam.Value = string.IsNullOrWhiteSpace(stringValue) ? string.Empty : stringValue;
					if (!IsValidationSuspended)
					{
						ValidateFrom();
						ValidateTo();
					}
					FromInfo.RefreshBinding();
				}
			}
		}

		public ZString To
		{
			get { return (fToParam.Value == DBNull.Value) ? string.Empty : fToParam.Value as string; }
			set
			{
				if (fToParam.Value == DBNull.Value || To != value)
				{
					string stringValue = ((IZTypeInternals)value).GetValueForLogicalDataLayer(true) as string;
					fToParam.Value = string.IsNullOrWhiteSpace(stringValue) ? string.Empty : stringValue;
					if (!IsValidationSuspended)
					{
						ValidateTo();
						ValidateFrom();
					}
					ToInfo.RefreshBinding();
				}
			}
		}

		public bool IsValidNumber(string value)
		{
			return ZInt.CanParse(value);
		}

		public override bool IsEmpty
		{
			get
			{
				return From.IsEmpty && To.IsEmpty;
			}
		}

		#region Debug Only
#if DEBUG
		public override void ClearValueForUnitTest()
		{
			fFromParam.Value = string.Empty;
			fToParam.Value = string.Empty;
		}
#endif
		#endregion

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			return true;
		}

		protected override string NonEmptyWhereClause()
		{
			if (!From.IsEmpty && !To.IsEmpty)
			{
				return string.Format(CultureInfo.InvariantCulture, (NoResString)@"{0} BETWEEN {1} AND {2}", FieldName, fFromParam, fToParam);
			}
			else if (!From.IsEmpty && To.IsEmpty)
			{
				return string.Format(CultureInfo.InvariantCulture, @"{0} >= {1}", FieldName, fFromParam);
			}
			else if (From.IsEmpty && !To.IsEmpty)
			{
				return string.Format(CultureInfo.InvariantCulture, @"{0} <= {1}", FieldName, fToParam);
			}
			else
			{
				// Should never happen - only called if IsEmpty() == false
				throw new DocumentEngineException("Where clause generation logic error");
			}
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.AccountingNumberRangeUserControl; }
		}

		#region Special Value Providers

		protected override void AddSpecialisedValueProviders()
		{
			base.AddSpecialisedValueProviders();
			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".ValueFrom", new ReplacementProviderMethod(GetValueFromReplacement)));
			ValueProviders.Add(new DelegateValueProvider(DisplayName + ".ValueTo", new ReplacementProviderMethod(GetValueToReplacement)));
		}

		protected override void AddSpecialisedValueProviderDocumenters()
		{
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ValueFrom>", ResString.GetMultilingualString("ab1092ec-5e61-4301-9f2f-5c520cc1cc7c", "Returns the From value as a string.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.ValueTo>", ResString.GetMultilingualString("a499cddc-f75c-44aa-90ca-a4f55c1a2361", "Returns the To value as a string.")));
		}

		protected object GetValueFromReplacement(string macro, Report report)
		{
			return From.ToString();
		}

		protected object GetValueToReplacement(string macro, Report report)
		{
			return To.ToString();
		}

		#endregion

		#region ISingleValueProvider Members

		public override object ValueAsObject
		{
			get
			{
				return Res.GetString("29e72e41-053f-499d-a67c-7842044e24d7", "From: {0} To: {1}", fFromParam.Value, fToParam.Value);
			}
		}

		#endregion

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
			var field = source as AccountingNumberRangeField;
			if (field != null)
			{
				this.To = field.To;
				this.From = field.From;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateFrom();
			ValidateTo();
		}

		public void ValidateFrom()
		{
			FromInfo.ClearAllNotifications();

			if (!IsValid)
			{
				FromInfo.AddError(ValidationError);
			}
			else if (!From.IsEmpty)
			{
				if (!IsValidNumber(From))
				{
					FromInfo.AddError(InvalidNumberErrorMessage);
				}
				else if (IsValidNumber(To) && ZInt.Parse(From) > ZInt.Parse(To))
				{
					FromInfo.AddError(NumberFromGreaterThanNumberToErrorMessage);
				}
			}
		}

		public void ValidateTo()
		{
			ToInfo.ClearAllNotifications();

			if (!IsValid)
			{
				ToInfo.AddError(ValidationError);
			}
			else if (!To.IsEmpty)
			{
				if (!IsValidNumber(To))
				{
					ToInfo.AddError(InvalidNumberErrorMessage);
				}
				else if (IsValidNumber(From) && ZInt.Parse(From) > ZInt.Parse(To))
				{
					ToInfo.AddError(NumberToLessThanNumberFromErrorMessage);
				}
			}
		}

		public ZPropertyInfo FromInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(nameof(From));
				info.HumanReadableName = DisplayName;
				return info;
			}
		}

		public ZPropertyInfo ToInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(nameof(To));
				info.HumanReadableName = DisplayName;
				return info;
			}
		}

		public override void ClearValues()
		{
			this.To = ZString.Empty;
			this.From = ZString.Empty;
		}

		#endregion

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			var filterData = new AccountingNumberRangeFilter();
			SetBaseFilterData(filterData);

			filterData.From = From;
			filterData.To = To;

			reportFilterData.AccountingNumberRangeFilterCollection.Add(filterData);
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = reportFilterData.AccountingNumberRangeFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
			if (selectedValue != null)
			{
				From = selectedValue.From;
				To = selectedValue.To;
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var result = new AccountingNumberRangeFieldJsonData
			{
				From = From,
				To = To
			};
			SetJsonData(result);
			return result;
		}

		#endregion
	}
}
