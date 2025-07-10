using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
#if DEBUG
	public interface IValueAsStringProviderForUnitTests
	{
		string ValueAsStringForSerialisation { get; set; }
	}
#endif
	public class MultipleChoice : FilterFieldValueSerialisable, IDescriptionPairListSupportField, IJsonSerializable
#if DEBUG
, IValueAsStringProviderForUnitTests
#endif
	{
		public enum Styles { DropDown }

		public MultipleChoice(BusinessObjectFactory factory)
			: base(factory)
		{
			CreateParameters();
		}

		void DeserializeList(string deserializedList)
		{
			try
			{
				var codeAndDescriptionArray = deserializedList.Split("|".ToCharArray());
				foreach (string codeAndDescription in codeAndDescriptionArray)
				{
					if (!string.IsNullOrEmpty(codeAndDescription))
					{
						var pair = codeAndDescription.Split(";".ToCharArray());
						List.AddPair(pair[0], pair[1]);
					}
				}
			}
			catch (Exception e) when (!e.IsCriticalException()) { }
		}

		#region Constructor For IJsonSerializable

		internal MultipleChoice(MultipleChoiceJsonData data)
			: base(data)
		{
			CreateParameters();

			DeserializeList(data.List);
			ZValue = data.Value;
		}

		internal override void PreSetValueForDeserializingBeforeExchange(FilterField deserializedFilter)
		{
			base.PreSetValueForDeserializingBeforeExchange(deserializedFilter);

			if (deserializedFilter is MultipleChoice deserializedMultipleChoice)
			{
				ZValue = deserializedMultipleChoice.ZValue;
			}
		}

		#endregion

		void CreateParameters()
		{
			fStyle = Styles.DropDown;
			fList = new CodeDescriptionPairList();

			fParam = new SqlParameter(SqlParameterNameGenerator.Next(), SqlDbType.Char);
			fParam.Value = DBNull.Value;
			ParameterList.Add(fParam);
		}

		public Styles Style
		{
			get { return fStyle; }
			set { fStyle = value; }
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
					if (!IsValidationSuspended)
					{
						ValidateZValue();
					}
					ZValueInfo.RefreshBinding();
					FireChangeReadOnly();
					FireChangeDependencyValue();
				}
			}
		}

		public object Value
		{
			get { return fParam.Value; }
			set
			{
				fParam.Value = value;
				SqlDbTypeDecider.UpdateParamSqlDbTypeBasedOnStringValue(fParam);
			}
		}

		public CodeDescriptionPairList List
		{
			get { return fList; }
		}

		public void ValidateZValue()
		{
			ZValueInfo.ClearAllNotifications();

			if (!IsValid)
			{
				ZValueInfo.AddError(ValidationError);
			}
			if (!IsEmpty && !AllowInvalidCode)
			{
				ListValidation.WarnIfInvalidCode(ZValueInfo, fList);
			}
			if (Globals.IsWeb && IsEmpty && !string.IsNullOrEmpty(DefaultExpression))
			{
				this.ZValue = DefaultExpression;
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
			if (otherFilterField is MultipleChoice)
			{
				//If this MultipleChoice filter is set to allow invalid codes,
				//let's always make otherFilterField compatible with it
				//(i.e we don't care if its code is not in the list)
				if (this.AllowInvalidCode || this.List.ContainsCode(((MultipleChoice)otherFilterField).ValueAsObject))
				{
					return true;
				}
			}
			return false;
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			Value = List[0].Code;
			DefaultExpression = (string)Value;
		}

		public override void ClearValueForUnitTest()
		{
			ZValue = "";
			DefaultExpression = "";
		}
#endif

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
			get
			{
				FilterFieldSuggestedUserControlType result = FilterFieldSuggestedUserControlType.None;
				switch (Style)
				{
					case Styles.DropDown:
						result = FilterFieldSuggestedUserControlType.MultipleChoiceUserControl;
						break;
				}
				return result;
			}
		}

		public override object ValueAsObject
		{
			get { return (Value == DBNull.Value || Value == null) ? null : Value; }
		}

		#endregion

		#region ValueProviders stuff

		protected override void AddSpecialisedValueProviders()
		{
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".Description", new ValueReplacers.ReplacementProviderMethod(GetDescriptionReplacement)));
		}

		protected override void AddSpecialisedValueProviderDocumenters()
		{
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.Description>", ResString.GetMultilingualString("b983609f-db18-483a-8c02-2b3c1258a857", "Return the description of the selected value.")));
		}

		protected object GetDescriptionReplacement(string macro, Report report)
		{
			return fList.GetDescriptionFromCode(ZValue);
		}

		#endregion

		protected CodeDescriptionPairList fList;
		protected Styles fStyle;
		protected SqlParameter fParam;

		protected override string NonEmptyWhereClause()
		{
			return String.Format("{0} = {1}", FieldName, fParam);
		}

		String SerializedList()
		{
			string serializedList = "";
			foreach (ICodeDescription codeDescription in List)
			{
				serializedList += codeDescription.Code + ";" + codeDescription.Description + "|";
			}
			return serializedList;
		}

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
			if (source is MultipleChoice)
			{
				this.ZValue = ((MultipleChoice)source).ZValue;
			}
		}

		public override void ClearValues()
		{
			if (Globals.IsWeb)
			{
				this.ZValue = DefaultExpression;
			}
			else
			{
				this.ZValue = null;
			}
		}

		#endregion

		protected override string ValueAsStringForSerialisationInternal
		{
			get { return ZValue.ToString(); }
			set { ZValue = value; }
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
			var filterData = new MultipleChoiceFilter();
			SetBaseFilterData(filterData);

			filterData.Value = ZValue;
			List.CopyToCodeDescriptionList(filterData.List);

			reportFilterData.MultipleChoiceFilterCollection.Add(filterData);
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = reportFilterData.MultipleChoiceFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
			if (selectedValue != null)
			{
				ZValue = selectedValue.Value;
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var result = new MultipleChoiceJsonData()
			{
				Value = ZValue,
				List = SerializedList()
			};
			SetJsonData(result);
			return result;
		}

		#endregion
	}
}
