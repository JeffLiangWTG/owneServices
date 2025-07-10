using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class ExactTextField : FilterFieldValueSerialisable, IJsonSerializable
	{
		#region Constructors

		public ExactTextField(BusinessObjectFactory factory)
			: base(factory)
		{
			CreateParameters();
		}

		#region Constructor For IJsonSerializable

		void CreateParameters()
		{
			parameter = new SqlParameter(SqlParameterNameGenerator.Next(), DBNull.Value);
			ParameterList.Add(parameter);
		}

		internal ExactTextField(ExactTextFieldJsonData data)
			: base(data)
		{
			CreateParameters();

			Value = data.Value;
		}

		#endregion

		#endregion

		SqlParameter parameter;

		protected override string NonEmptyWhereClause()
		{
			return FieldName + " = " + base.ParameterList[0] + " ";
		}

		[BusinessObjectTestExclude]
		public ZString Value
		{
			get
			{
				ZString result = ZString.Empty;

				if (parameter.Value != DBNull.Value)
				{
					result = parameter.Value as string;
				}

				return result;
			}
			set
			{
				if (parameter.Value == DBNull.Value || value != Value)
				{
					SetHasChangesBecauseTheValueHasBeenSetByTheUser();
					string stringValue = FixCarriageReturns(((IZTypeInternals)value).GetValueForLogicalDataLayer(true) as string);
					parameter.Value = stringValue;
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

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateValue();
		}

		public void ValidateValue()
		{
			ValueInfo.ClearAllNotifications();

			if (!IsValid)
			{
				ValueInfo.AddError(ValidationError);
			}
		}

		public override object ValueAsObject
		{
			get { return (string)Value; }
		}

		protected override string ValueAsStringForSerialisationInternal
		{
			get { return Value; }
			set { Value = FixCarriageReturns(value); }
		}

		ZString FixCarriageReturns(ZString text)
		{
			return text.Replace("\n", "\r\n").Replace("\r\r", "\r");
		}

		public override bool IsEmpty
		{
			get { return Value.IsEmpty; }
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.TextFieldUserControl; }
		}

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			return true;
		}

		public override void SafeCopyValuesFrom(IFilter source)
		{
			if (source is ExactTextField)
			{
				this.Value = ((ExactTextField)source).Value;
			}
		}

		public override void ClearValues()
		{
			this.Value = ZString.Empty;
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
			parameter.Value = DBNull.Value;
		}
#endif
		#endregion

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			var filterData = new TextFilter();
			SetBaseFilterData(filterData);

			filterData.Value = Value;

			reportFilterData.TextFilterCollection.Add(filterData);
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = reportFilterData.TextFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
			if (selectedValue != null)
			{
				Value = selectedValue.Value;
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var result = new ExactTextFieldJsonData() { Value = Value };
			SetJsonData(result);
			return result;
		}

		#endregion
	}
}
