using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class TextField : FilterFieldValueSerialisable, IJsonSerializable
	{
		public TextField(BusinessObjectFactory factory)
			: base(factory)
		{
			CreateParameters();
		}

		public TextField(bool showMultiLine, BusinessObjectFactory factory)
			: this(factory)
		{
			fMultiLine = showMultiLine;
		}

		#region Constructor For IJsonSerializable

		internal TextField(TextFieldJsonData data)
			: base(data)
		{
			CreateParameters();

			FilterMethod = data.FilterMethod;
			Value = data.Value;
		}

		#endregion

		/// <summary>
		/// Gets or sets the method in which the field will be filtered.
		/// Select from the TextFieldFilterMethodList.Codes.
		/// </summary>
		public string FilterMethod { get; set; }

		void CreateParameters()
		{
			fParam = new SqlParameter(SqlParameterNameGenerator.Next(), DBNull.Value);
			ParameterList.Add(fParam);
		}

		[BusinessObjectTestExclude]
		public ZString Value
		{
			get
			{
				ZString result = ZString.Empty;

				if (fParam.Value != DBNull.Value)
				{
					result = fParam.Value as string;
				}

				return result;
			}
			set
			{
				if (fParam.Value == DBNull.Value || value != Value)
				{
					SetHasChangesBecauseTheValueHasBeenSetByTheUser();
					string stringValue = FixCarriageReturns(((IZTypeInternals)value).GetValueForLogicalDataLayer(true) as string);
					fParam.Value = stringValue;
					if (!IsValidationSuspended)
					{
						ValidateValue();
					}
					ValueInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ValueInfo
		{
			get { return GetZPropertyInfo(nameof(Value)); }
		}

		public void ValidateValue()
		{
			ValueInfo.ClearAllNotifications();

			if (!IsValid)
			{
				ValueInfo.AddError(ValidationError);
			}
		}

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateValue();
		}

		public override object ValueAsObject
		{
			get { return (string)Value; }
		}

		public override bool IsEmpty
		{
			get
			{
				return Value.IsEmpty;
			}
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return fMultiLine ? FilterFieldSuggestedUserControlType.ZMultiLineTextFieldUserControl : FilterFieldSuggestedUserControlType.TextFieldUserControl; }
		}

		#endregion

		#region Implementation

		SqlParameter fParam;
		protected readonly bool fMultiLine;

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			return true;
		}

		protected override string NonEmptyWhereClause()
		{
			switch (FilterMethod)
			{
				case TextFieldFilterMethodList.Codes.Contains:
					return string.Format((NoResString)"{0} LIKE '%' + {1} + '%'", FieldName, fParam);

				default:
					return string.Format((NoResString)"{0} LIKE {1} + '%'", FieldName, fParam);
			}
		}

		protected ZString FixCarriageReturns(ZString text)
		{
			return text.Replace("\n", "\r\n").Replace("\r\r", "\r");
		}

		#endregion

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
			if (source is TextField)
			{
				this.Value = ((TextField)source).Value;
			}
		}

		public override void ClearValues()
		{
			this.Value = ZString.Empty;
		}

		#endregion

		protected override string ValueAsStringForSerialisationInternal
		{
			get { return Value; }
			set { Value = FixCarriageReturns(value); }
		}

		#region For Testing
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			Value = "Text";
		}

		public override void ClearValueForUnitTest()
		{
			fParam.Value = DBNull.Value;
		}
#endif
		#endregion

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			BaseFilterData filterData = null;
			if (SuggestedUserControlType == FilterFieldSuggestedUserControlType.TextFieldUserControl)
			{
				filterData = new TextFilter { Value = Value };
				reportFilterData.TextFilterCollection.Add(filterData as TextFilter);
			}
			else if (SuggestedUserControlType == FilterFieldSuggestedUserControlType.ZMultiLineTextFieldUserControl)
			{
				filterData = new ZMultiLineTextFilter { Value = Value };
				reportFilterData.ZMultiLineTextFilterCollection.Add(filterData as ZMultiLineTextFilter);
			}

			if (filterData != null)
			{
				SetBaseFilterData(filterData);
			}
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			BaseFilterData selectedValue = null;
			if (SuggestedUserControlType == FilterFieldSuggestedUserControlType.TextFieldUserControl)
			{
				selectedValue = reportFilterData.TextFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
				if (selectedValue != null)
				{
					Value = ((TextFilter)selectedValue).Value;
				}
			}
			else if (SuggestedUserControlType == FilterFieldSuggestedUserControlType.ZMultiLineTextFieldUserControl)
			{
				selectedValue = reportFilterData.ZMultiLineTextFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
				if (selectedValue != null)
				{
					Value = ((ZMultiLineTextFilter)selectedValue).Value;
				}
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var result = fMultiLine
				? new ZMultiLineTextFieldJsonData
				{
					Value = Value,
					FilterMethod = FilterMethod
				}
				: new TextFieldJsonData
				{
					Value = Value,
					FilterMethod = FilterMethod
				};
			SetJsonData(result);
			return result;
		}

		#endregion
	}
}
