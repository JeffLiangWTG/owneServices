using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class CodeListMultipleChoice : FilterFieldValueSerialisable, IDescriptionPairListSupportField, IJsonSerializable
#if DEBUG
, IValueAsStringProviderForUnitTests
#endif
	{
		public CodeListMultipleChoice(BusinessObjectFactory factory)
			: base(factory)
		{
			CreateParameterValues();
		}

		#region Constructor For IJsonSerializable

		internal CodeListMultipleChoice(CodeListMultipleChoiceJsonData data)
			: base(data)
		{
			CreateParameterValues();

			ZValue = data.Value;
		}

		#endregion

		internal ZString SqlDataSource { get; set; }

		void CreateParameterValues()
		{
			fParam = new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.Char);
			fParam.Value = DBNull.Value;
			ParameterList.Add(fParam);
		}

		public bool AllowInvalidCode
		{
			get { return allowInvalidCode; }
			set { allowInvalidCode = value; }
		}
		bool allowInvalidCode;

		[BusinessObjectTestExclude]
		public ZString ZValue
		{
			get
			{
				if (fParam.Value == DBNull.Value)
				{
					fParam.Value = "";
				}
				return new ZString(fParam.Value);
			}
			set
			{
				if (value != ZValue)
				{
					fParam.Value = ((IZTypeInternals)value).GetValueForLogicalDataLayer(true);
					SqlDbTypeDecider.UpdateParamSqlDbTypeBasedOnStringValue(fParam);
					SetHasChangesBecauseTheValueHasBeenSetByTheUser();
					ValidateZValue();
					ZValueInfo.RefreshBinding();
					FireChangeReadOnly();
					FireChangeDependencyValue();
				}
			}
		}
		public ZString Value
		{
			get { return fParam.Value == DBNull.Value ? ZString.Empty : new ZString(fParam.Value); }
			set
			{
				if (value != ZValue)
				{
					fParam.Value = (value.IsValid ? ((IZTypeInternals)value).GetValueForLogicalDataLayer(true) : DBNull.Value);
					SqlDbTypeDecider.UpdateParamSqlDbTypeBasedOnStringValue(fParam);
					HasChanges = true;
					ZValueInfo.RefreshBinding();
				}
			}
		}

		public ReadOnlyCodeDescriptionPairList List
		{
			get { return fList; }
		}

		public void SetPairList(ReadOnlyCodeDescriptionPairList pairList)
		{
			fList = pairList;
		}

		protected override void SetDependencyValue(string value)
		{
			if (DependenceListProvider != null)
			{
				SetPairList(DependenceListProvider.GetDependenceCodeDescriptionPairList(value));
			}
		}

		protected override void SetNecessaryPropertiesForDeserializingCore(FilterField origin, CollectionOfIFilter filterCollection)
		{
			base.SetNecessaryPropertiesForDeserializingCore(origin, filterCollection);

			if (origin is CodeListMultipleChoice originalCodeListMultipleChoice)
			{
				DependenceListProvider = originalCodeListMultipleChoice.DependenceListProvider;
				SetPairList(originalCodeListMultipleChoice.List);
			}
		}

		internal override void PreSetValueForDeserializingBeforeExchange(FilterField deserializedFilter)
		{
			base.PreSetValueForDeserializingBeforeExchange(deserializedFilter);

			if (deserializedFilter is CodeListMultipleChoice deserializedCodeListMultipleChoice)
			{
				ZValue = deserializedCodeListMultipleChoice.ZValue;
			}
		}

		internal IDependenceCodeDescriptionPairListProvider DependenceListProvider { get; set; }

		public void ValidateZValue()
		{
			ZValueInfo.ClearAllNotifications();

			if (!IsValid)
			{
				ZValueInfo.AddError(ValidationError);
			}
			if (!IsEmpty && fList != null && !AllowInvalidCode)
			{
				ListValidation.WarnIfInvalidCode(ZValueInfo, fList);
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

		#region Overrides

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			if (otherFilterField is CodeListMultipleChoice)
			{
				//If this CodeListMultipleChoice filter is set to allow invalid codes,
				//let's always make otherFilterField compatible with it
				//(i.e we don't care if its code is not in the list)
				if (AllowInvalidCode || List.ContainsCode(((CodeListMultipleChoice)otherFilterField).ValueAsObject))
				{
					return true;
				}
			}
			return false;
		}

		#region Debug Only
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			if (List.Count > 0)
			{
				Value = List[0].Code;
			}
		}

		public override void ClearValueForUnitTest()
		{
			Value = "";
		}
#endif
		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateZValue();
		}

		public override bool IsEmpty
		{
			get { return ZValue.IsEmpty; }
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.CodeListMultipleChoiceUserControl; }
		}

		public override object ValueAsObject
		{
			get { return IsEmpty ? DBNull.Value : Value; }
		}

		#endregion

		#region ValueProviders stuff
		protected override void AddSpecialisedValueProviders()
		{
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".Description", new ValueReplacers.ReplacementProviderMethod(GetDescriptionReplacement)));
		}

		protected override void AddSpecialisedValueProviderDocumenters()
		{
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.Description>", ResString.GetMultilingualString("a5c76da8-0ff6-4ce3-b71d-c554118578da", "Returns the description of the selected value.")));
		}

		protected object GetDescriptionReplacement(string macro, Report report)
		{
			return fList.GetDescriptionFromCode(ZValue);
		}

		#endregion

		protected ReadOnlyCodeDescriptionPairList fList;

		protected SqlParameter fParam;

		protected override string NonEmptyWhereClause()
		{
			return String.Format("{0} = {1}", FieldName, fParam);
		}

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
			if (source is CodeListMultipleChoice)
			{
				this.Value = ((CodeListMultipleChoice)source).Value;
			}
		}

		public override void ClearValues()
		{
			this.Value = null;
		}

		#endregion

		protected override string ValueAsStringForSerialisationInternal
		{
			get { return Value.ToString(); }
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					try
					{
						Value = new ZString(value);
					}
					catch (FormatException)
					{
						// Ignore it - the serialised value was not a string
					}
				}
			}
		}

		#region IDescriptionPairListSupportField implement

		List<string> IDescriptionPairListSupportField.DescriptionList
		{
			get
			{
				var result = new List<string>();
				if (List != null)
				{
					foreach (ICodeDescription item in List)
					{
						result.Add(item.Description);
					}
				}
				return result;
			}
		}

		#endregion

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			var filterData = new CodeListMultipleChoiceFilter();
			SetBaseFilterData(filterData);

			List.CopyToCodeDescriptionList(filterData.List);
			filterData.Value = ZValue;

			reportFilterData.CodeListMultipleChoiceFilterCollection.Add(filterData);
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = reportFilterData.CodeListMultipleChoiceFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
			if (selectedValue != null)
			{
				ZValue = selectedValue.Value;
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var result = new CodeListMultipleChoiceJsonData()
			{
				Value = Value,
			};
			SetJsonData(result);
			return result;
		}

		#endregion
	}
}
