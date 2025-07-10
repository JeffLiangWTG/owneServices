using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class RegistrationCodeField : FilterFieldWithUTSupport, IJsonSerializable
	{
		public RegistrationCodeField(BusinessObjectFactory factory)
			: base(factory)
		{
			SetFieldName();
			CreateParameters();
		}

		#region Constructor For IJsonSerializable

		internal RegistrationCodeField(RegistrationCodeFieldJsonData data)
			: base(data)
		{
			SetFieldName();
			CreateParameters();

			FieldNameCodeCountry = data.FieldNameCodeCountry;
			FieldNameCustomType = data.FieldNameCustomType;
			CodeCountry = data.CodeCountry;
			CustomType = data.CustomType;
		}

		#endregion

		void SetFieldName()
		{
			FieldName = "RegistrationCode";
		}

		void CreateParameters()
		{
			codeCountry = new SqlParameter(SqlParameterNameGenerator.Next(), DBNull.Value);
			customType = new SqlParameter(SqlParameterNameGenerator.Next(), DBNull.Value);
			ParameterList.Add(codeCountry);
			ParameterList.Add(customType);
		}

		public string FieldNameCodeCountry
		{
			get { return fieldNameCodeCountry; }
			set { fieldNameCodeCountry = value; }
		}
		string fieldNameCodeCountry = "";

		public string FieldNameCustomType
		{
			get { return fieldNameCustomType; }
			set { fieldNameCustomType = value; }
		}
		string fieldNameCustomType = "";

		#region CodeCountry

		[List("Countries")]
		public ZString CodeCountry
		{
			get { return new ZString(ValueIsNull(codeCountry.Value) ? "" : codeCountry.Value); }
			set
			{
				if (value != CodeCountry)
				{
					country = null;
					customTypes = null;
				}
				codeCountry.Value = ((IZTypeInternals)value).GetValueForLogicalDataLayer(true);
				if (!IsValidationSuspended)
				{
					ValidateCodeCountry();
					ValidateCustomType();
				}
				CodeCountryInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CodeCountryInfo
		{
			get { return GetZPropertyInfo(nameof(CodeCountry)); }
		}

		public void ValidateCodeCountry()
		{
			CodeCountryInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(CodeCountryInfo, Countries);
		}

		RefCountry Country
		{
			get { return country ?? (country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CodeCountry)); }
		}
		RefCountry country;

		public RefCountryCollection Countries
		{
			get { return countries ?? (countries = new RefCountryCollection(Factory)); }
		}
		RefCountryCollection countries;

		#endregion

		#region CustomType

		[List("CustomTypes")]
		public ZString CustomType
		{
			get { return new ZString(ValueIsNull(customType.Value) ? "" : customType.Value); }
			set
			{
				customType.Value = ((IZTypeInternals)value).GetValueForLogicalDataLayer(true);
				if (!IsValidationSuspended)
				{
					ValidateCustomType();
					ValidateCodeCountry();
				}
				CustomTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CustomTypeInfo
		{
			get { return GetZPropertyInfo(nameof(CustomType)); }
		}

		public void ValidateCustomType()
		{
			CustomTypeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(CustomTypeInfo, CustomTypes);
		}

		public CodeDescriptionPairList CustomTypes
		{
			get { return customTypes ?? (customTypes = new OrgCodeLists().CustomsCodes_List(Country)); }
		}
		CodeDescriptionPairList customTypes;

		#endregion

		#region Overrides

		protected override void AddSpecialisedValueProviders()
		{
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".CodeCountry", new ValueReplacers.ReplacementProviderMethod(GetCodeCountryReplacement)));
			ValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".CustomType", new ValueReplacers.ReplacementProviderMethod(GetCustomTypeReplacement)));
		}

		protected override void AddSpecialisedValueProviderDocumenters()
		{
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.CodeCountry>", ResString.GetMultilingualString("b5403e57-d71c-4c7d-bfc4-8d9fce6170a8", "Returns the selected country/region code.")));
			ValueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.CustomType>", ResString.GetMultilingualString("af0af02a-4e7e-4ca5-bc1b-2138b9d2f999", "Returns the selected customs type code.")));
		}

		protected object GetCodeCountryReplacement(string macro, Report report)
		{
			return CodeCountry;
		}

		protected object GetCustomTypeReplacement(string macro, Report report)
		{
			return CustomType;
		}

		public override bool IsEmpty
		{
			get { return ValueIsNull(codeCountry.Value) && ValueIsNull(customType.Value); }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCodeCountry();
			ValidateCustomType();
		}

		public override FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get { return FilterFieldSuggestedUserControlType.RegistrationCodedUserControl; }
		}

		public override object ValueAsObject
		{
			get
			{
				return Res.GetString("32e91d71-490d-421f-a4e1-b863222c122c", "Code Country/Region: {0}, Registration Code: {1}", CodeCountry.ToString(), CustomType.ToString());
			}
		}

		#region Debug Only
#if DEBUG
		public override void ClearValueForUnitTest()
		{
			codeCountry.Value = null;
			customType.Value = null;
		}
#endif
		#endregion

		#endregion

		#region Implementation

		SqlParameter codeCountry;
		SqlParameter customType;

		bool ValueIsNull(object value)
		{
			return (value == null || value == DBNull.Value);
		}

		protected override bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField)
		{
			return true;
		}

		protected override string NonEmptyWhereClause()
		{
			string countryCodeClause = CodeCountry.IsEmpty ? "" : FieldNameCodeCountry + " = " + codeCountry;
			string customTypeClause = CustomType.IsEmpty ? "" : FieldNameCustomType + " = " + customType;
			return countryCodeClause + ((!string.IsNullOrEmpty(countryCodeClause) && !string.IsNullOrEmpty(customTypeClause)) ? " AND " : "") + customTypeClause;
		}

		#endregion

		#region IFilter Members

		public override void SafeCopyValuesFrom(IFilter source)
		{
			if (source is RegistrationCodeField)
			{
				this.CodeCountry = ((RegistrationCodeField)source).CodeCountry;
				this.CustomType = ((RegistrationCodeField)source).CustomType;
			}
		}

		public override void ClearValues()
		{
			this.CodeCountry = ZString.Empty;
			this.CustomType = ZString.Empty;
		}

		#endregion

		public override void FillFilterData(ReportFilterData reportFilterData)
		{
			var filterData = new RegistrationCodedFilter();
			SetBaseFilterData(filterData);

			filterData.CodeCountry = CodeCountry;
			filterData.CustomType = CustomType;

			reportFilterData.RegistrationCodedFilterCollection.Add(filterData);
		}

		public override void SetFilterValue(ReportFilterData reportFilterData)
		{
			var selectedValue = reportFilterData.RegistrationCodedFilterCollection.FirstOrDefault(a => a.DisplayName == DisplayName);
			if (selectedValue != null)
			{
				CodeCountry = selectedValue.CodeCountry;
				CustomType = selectedValue.CustomType;
			}
		}

		#region IJsonSerializable Members

		public object GetJsonData()
		{
			var result = new RegistrationCodeFieldJsonData()
			{
				FieldNameCodeCountry = FieldNameCodeCountry,
				FieldNameCustomType = FieldNameCustomType,
				CodeCountry = CodeCountry,
				CustomType = CustomType
			};
			SetJsonData(result);
			return result;
		}

		#endregion
	}
}
