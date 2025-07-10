using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class NumberField : FilterFieldValueSerialisable, IJsonSerializable
	{
		public NumberField(BusinessObjectFactory factory)
			: base(factory)
		{
			CreateParameters();
		}

		#region Constructor For IJsonSerializable

		internal NumberField(NumberFieldJsonData data)
			: base(data)
		{
			CreateParameters();

			ZValue = data.Value;
			DecimalPlaces = data.DecimalPlaces;
		}

		#endregion

		void CreateParameters()
		{
			fDecimalPlaces = new ZInt(2);
			ShowGroupSeparators = true;
			MinValue = int.MinValue;
			MaxValue = int.MaxValue;

			fParam = new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.Decimal);
			fParam.Value = DBNull.Value;
			ParameterList.Add(fParam);
		}

		public ZDecimal ZValue
		{
			get
			{
				if (fParam.Value == DBNull.Value)
				{
					fParam.Value = 0.0m;
				}
				return new ZDecimal(fParam.Value);
			}
			set
			{
				if (value != ZValue)
				{
					fParam.Value = ((IZTypeInternals)value).GetValueForLogicalDataLayer(true);
					SetHasChangesBecauseTheValueHasBeenSetByTheUser();
					ValidateZValue();
					ZValueInfo.RefreshBinding();
				}
			}
		}

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			var numberField = otherFilterField as NumberField;
			if (numberField != null)
			{
				object valueAsObject = numberField.ValueAsObject;
				if (valueAsObject != null)
				{
					ZDecimal value;
					if (valueAsObject is decimal || valueAsObject is ZDecimal || valueAsObject is int || valueAsObject is ZInt || valueAsObject is long || valueAsObject is ZLong)
					{
						value = new ZDecimal(valueAsObject);
						return IsWithinBounds(value);
					}

					if (ZDecimal.TryParse(valueAsObject.ToString(), out value))
					{
						return IsWithinBounds(value);
					}
				}
			}

			return false;
		}

		bool IsWithinBounds(ZDecimal value)
		{
			return value >= this.MinValue && value <= this.MaxValue;
		}

#if DEBUG
		public override void ClearValueForUnitTest()
		{
			fParam.Value = DBNull.Value;
		}
#endif

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateZValue();
		}

		public object Value
		{
			get { return fParam.Value; }
			set { fParam.Value = value; }
		}

		public void ValidateZValue()
		{
			ZValueInfo.ClearAllNotifications();

			if (!IsValid)
			{
				ZValueInfo.AddError(ValidationError);
			}
			else
			{
				CompareValidation.CheckGreaterThanOrEqualTo(ZValueInfo, MinValue);
				CompareValidation.CheckLessThanOrEqualTo(ZValueInfo, MaxValue);
			}
		}

		public ZPropertyInfo ZValueInfo
		{
			get
			{
				ZPropertyInfo info = GetZPropertyInfo(nameof(ZValue));
				info.HumanReadableName = DisplayName;
				return info;
			}
		}

		public override bool IsEmpty
		{
			get { return ZValue.IsEmpty; }
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.NumberUserControl; }
		}

		public override object ValueAsObject
		{
			get { return (Value == DBNull.Value || Value == null) ? null : Value; }
		}

		public ZInt DecimalPlaces
		{
			get { return fDecimalPlaces; }
			set { fDecimalPlaces = value; }
		}

		public bool ShowGroupSeparators { get; set; }
		public decimal MinValue { get; set; }
		public decimal MaxValue { get; set; }

		#region Implementation

		protected ZInt fDecimalPlaces;
		protected SqlParameter fParam;

		protected override string NonEmptyWhereClause()
		{
			return String.Format("{0} = {1}", FieldName, fParam);
		}

		#endregion

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
			if (source is NumberField)
			{
				this.Value = ((NumberField)source).Value;
			}
		}

		public override void ClearValues()
		{
			this.Value = ZDecimal.Zero;
		}

		#endregion

		protected override string ValueAsStringForSerialisationInternal
		{
			get { return ZValue.ToString(); }
			set
			{
				ZDecimal deserialised;
				if (!string.IsNullOrEmpty(value))
				{
					if (ZDecimal.TryParse(value, out deserialised))
					{
						ZValue = deserialised;
					}
				}
				else
				{
					ZValue = 0;
				}
			}
		}

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			var filterData = new NumberFilter();
			SetBaseFilterData(filterData);

			filterData.Value = ZValue;

			reportFilterData.NumberFilterCollection.Add(filterData);
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = reportFilterData.NumberFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
			if (selectedValue != null)
			{
				ZValue = selectedValue.Value;
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var result = new NumberFieldJsonData()
			{
				Value = ZValue,
				DecimalPlaces = DecimalPlaces
			};
			SetJsonData(result);
			return result;
		}

		#endregion
	}
}
