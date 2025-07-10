using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	interface IValueProviderListProvider
	{
		ValueProviderList ValueProviders { get; }
	}
#if DEBUG
	public interface IFilterFieldForUT
	{
		bool IsRequired { get; }
	}
#endif

	[System.Diagnostics.DebuggerDisplay("DisplayName = {DisplayName}")]
	public abstract class FilterField : NonPersistentBusinessObject, IValueProviderListProvider, IFilter, IObsoleteValidation
#if DEBUG
, IFilterFieldForUT
#endif
	{
		protected FilterField(BusinessObjectFactory factory)
			: base(factory)
		{
			Validators = new List<FilterCollectionValidator>();
			ParameterList = new SqlParameterList();
		}
		readonly public List<FilterCollectionValidator> Validators;
		[NonSerialized]
		protected readonly SqlParameterList ParameterList;

		protected FilterField(BaseFieldJsonData data) : this(new BusinessObjectFactory())
		{
			DisplayName = data.DisplayName ?? string.Empty;
			FieldName = data.FieldName ?? string.Empty;
			IsDeserialized = true;
		}

		internal readonly bool IsDeserialized;

		protected void SetJsonData(BaseFieldJsonData data)
		{
			data.DisplayName = DisplayName;
			data.FieldName = FieldName;
		}

		internal void SetNecessaryPropertiesForDeserializing(FilterField origin, CollectionOfIFilter filterCollection)
		{
			SetNecessaryPropertiesForDeserializingCore(origin, filterCollection);
		}

		internal virtual void PreSetValueForDeserializingBeforeExchange(FilterField deserializedFilter)
		{
			// To be implemented in children
		}

		protected virtual void SetNecessaryPropertiesForDeserializingCore(FilterField origin, CollectionOfIFilter filterCollection)
		{
			GroupName = origin.GroupName;
			GroupDescription = origin.GroupDescription;

			foreach (var validator in origin.Validators)
			{
				Validators.Add(validator);
			}

			if (filterCollection != null)
			{
				foreach (FilterField existingField in filterCollection)
				{
					foreach (var validator in existingField.Validators)
					{
						for (var j = 0; j < validator.Filters.Count; j++)
						{
							if (validator.Filters[j].DisplayName == DisplayName)
							{
								validator.Filters[j] = this;
							}
						}
					}
				}
			}
		}

		#region User control related members
		protected string fName = "";
		protected string fNameLocalized = "";
		protected ResourceStringData fNameLocalizedData;
		DataContextValue fDataContextValue;
		StringCollection fTabName = new StringCollection();
		string fDefaultExpression = "";
		int fLeft = fUnspecified;
		int fTop = fUnspecified;
		int fHeight = fUnspecified;
		int fWidth = fUnspecified;
		const int fUnspecified = -1;
		string fDependencyValue = string.Empty;
		bool disableReadOnlyIfDependency;

		public abstract FilterFieldSuggestedUserControlType SuggestedUserControlType
		{
			get;
		}

		public string DisplayName
		{
			get { return fName; }
			set { fName = value; }
		}

		public string DisplayNameLocalized
		{
			get
			{
				return DocBuilderResourceStrings.GetTranslationString(DisplayNameLocalizedData, DisplayName);
			}
		}

		public ResourceStringData DisplayNameLocalizedData
		{
			get { return fNameLocalizedData; }
			set { fNameLocalizedData = value; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return DisplayNameLocalized; }
		}

		public DataContextValue DataContextValue
		{
			get { return fDataContextValue; }
			set { fDataContextValue = value; }
		}

		public StringCollection TabNames
		{
			get { return fTabName; }
			set { fTabName = value; }
		}

		public abstract bool IsEmpty
		{
			get;
		}

		public virtual string DefaultExpression
		{
			get { return fDefaultExpression; }
			set { fDefaultExpression = value; }
		}

		#region DependentFilter

		public string DependencyValue
		{
			get
			{
				return fDependencyValue;
			}
			set
			{
				if (!string.Equals(DependencyValue, value, StringComparison.InvariantCultureIgnoreCase))
				{
					fDependencyValue = value;
					ClearValues();
					SetDependencyValue(fDependencyValue);
				}
			}
		}

		protected virtual void SetDependencyValue(string value)
		{
		}

		public string DependentFilter { get; set; }

		internal string DependentFilterReadonlyIfValue { private get; set; }

		internal bool HasDependentFilter
		{
			get { return !string.IsNullOrWhiteSpace(DependentFilter); }
		}

		internal bool DisableReadOnlyIfDependency
		{
			get { return disableReadOnlyIfDependency; }
			set { disableReadOnlyIfDependency = value; }
		}

		internal void SetupDependentFilterRelation(CollectionOfIFilter filters)
		{
			if (HasDependentFilter)
			{
				foreach (var dependentFilter in DependentFilter.Split(','))
				{
					var filterField = FindFilterFieldByName(filters, dependentFilter.Trim());
					ChangeDependencyValue += () => filterField.DependencyValue = ValueAsObject?.ToString();
					if (!filterField.DisableReadOnlyIfDependency)
					{
						filterField.ReadOnlyIfFilter = DisplayName;
					}
					filterField.FireChangeDependencyValue();
				}
			}
		}

		protected void FireChangeDependencyValue()
		{
			if (ChangeDependencyValue != null)
			{
				ChangeDependencyValue();
			}
		}

		FilterField FindFilterFieldByName(CollectionOfIFilter filters, string name)
		{
			foreach (FilterField filter in filters)
			{
				if (filter.DisplayName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
				{
					return filter;
				}
			}

			throw new DocumentEngineException(string.Format("Filter '{0}' could not be found in master-detail relation.", name));
		}

		event Action ChangeDependencyValue;

		#endregion DependentFilter

		public int Left
		{
			get { return fLeft; }
			set { fLeft = value; }
		}

		public int Top
		{
			get { return fTop; }
			set { fTop = value; }
		}

		public int Height
		{
			get { return fHeight; }
			set { fHeight = value; }
		}

		public int Width
		{
			get { return fWidth; }
			set { fWidth = value; }
		}

		public int Unspecified
		{
			get { return fUnspecified; }
		}
		#endregion

		#region ReadOnlyIf

		internal string ReadOnlyIfFilter { get; set; }

		internal bool HasReadOnlyIfFilter
		{
			get { return !string.IsNullOrWhiteSpace(ReadOnlyIfFilter); }
		}

		internal void SetupReadOnlyIfRelation(CollectionOfIFilter filters)
		{
			if (HasReadOnlyIfFilter)
			{
				var filterField = FindFilterFieldByName(filters, ReadOnlyIfFilter);
				filterField.IsUsedToDetermineReadOnlyOfRelatedFilter = true;
				filterField.ChangeReadOnly += () => ReadOnly = filterField.IsEmpty || filterField.HasErrors || filterField.ValueAsObject?.ToString() == filterField.DependentFilterReadonlyIfValue;
				filterField.FireChangeReadOnly();
			}
		}

		protected void FireChangeReadOnly()
		{
			if (ChangeReadOnly != null)
			{
				ChangeReadOnly();
			}
		}

		event Action ChangeReadOnly;

		public bool IsUsedToDetermineReadOnlyOfRelatedFilter { get; private set; }

		#endregion

		protected virtual object GetUserFriendlyValueReplacement(string macro, Report report)
		{
			object result = null;
			if (IsEmpty)
			{
				result = (NoResString)"All";
			}
			else
			{
				if (this is FilterFieldValueSerialisable)
				{
					((FilterFieldValueSerialisable)this).UpdateValueIfNotOverriddenByUser(report);
				}
				result = ValueAsObject;
			}
			return result;
		}

		protected virtual object GetReplacement(string macro, Report report)
		{
			var serialisableFilterField = this as FilterFieldValueSerialisable;
			if (serialisableFilterField != null)
			{
				if (report.DisableFixedValueCache)
				{
					// We should not change ValueAsStringForSerialisation, otherwise the cache fixed value might be changed next time we preview.
					using (serialisableFilterField.DoNotChangeValueAsStringForSerialisation())
					{
						serialisableFilterField.EvaluateValueForDisableFixedValueCacheReport(report);
						return ValueAsObject;
					}
				}
				serialisableFilterField.UpdateValueIfNotOverriddenByUser(report);
			}
			return ValueAsObject;
		}

		#region IFilter Members

		public string WhereClause()
		{
			return (IsEmpty || string.IsNullOrEmpty(FieldName)) ? "" : NonEmptyWhereClause();
		}

		public SqlParameterList SqlParameters()
		{
			return GetSqlParametersCore();
		}

		protected virtual SqlParameterList GetSqlParametersCore()
		{
			return IsEmpty ? new SqlParameterList() : ParameterList;
		}

		public abstract void SafeCopyValuesFrom(IFilter source);
		public abstract void ClearValues();

		#endregion

		public string GroupName
		{
			get { return fGroupName; }
			set { fGroupName = value; }
		}
		string fGroupName = "";

		public string GroupDescription
		{
			get { return fGroupDescription; }
			set { fGroupDescription = value; }
		}
		string fGroupDescription = "";

		public string FieldName
		{
			get { return fFieldName; }
			set { fFieldName = value; }
		}
		string fFieldName = "";

		public bool IsFilterValueExcluded
		{
			get { return isFilterValueExcluded; }
			set { isFilterValueExcluded = value; }
		}
		bool isFilterValueExcluded;

		internal bool IsValid
		{
			get { return ValidationError.Length == 0; }
		}

		public string ValidationError
		{
			get
			{
				string result = "";
				foreach (FilterCollectionValidator validator in Validators)
				{
					if (!validator.IsValid(this))
					{
						result = validator.GetErrorMessage(this);
						break;
					}
				}
				return result;
			}
		}

		internal bool IsOverriddenInDocData { get; set; }

		public ValueProviderList ValueProviders
		{
			get
			{
				if (fValueProviders == null)
				{
					fValueProviders = new ValueProviderList();
					fValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName, GetReplacement));
					fValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".UserFriendlyValue", GetUserFriendlyValueReplacement));
					fValueProviders.Add(new ValueReplacers.DelegateValueProvider(DisplayName + ".IsEmpty", GetIsEmptyReplacement));
					AddSpecialisedValueProviders();
				}
				return fValueProviders;
			}
		}

		List<ValueProviderDocumenter> valueProviderDocumenters;
		public List<ValueProviderDocumenter> ValueProviderDocumenters
		{
			get
			{
				if (valueProviderDocumenters == null)
				{
					valueProviderDocumenters = new List<ValueProviderDocumenter>();
					valueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName>", ResString.GetMultilingualString("4d9ef93d-5681-414d-86d4-d98ec3aec672", "Returns the value of the current filter.")));
					valueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.UserFriendlyValue>", ResString.GetMultilingualString("529f97b2-6916-4a28-9432-998f78e0c0d6", "Return the user friendly value of the current filter.")));
					valueProviderDocumenters.Add(new ValueProviderDocumenter("<DisplayName.IsEmpty>", ResString.GetMultilingualString("dbc46289-645f-4e8d-a4bd-a373f3263cab", "Returns a boolean set to true if the current filter has no data, false otherwise.")));

					AddSpecialisedValueProviderDocumenters();
				}
				return valueProviderDocumenters;
			}
		}

		protected virtual void AddSpecialisedValueProviderDocumenters()
		{
		}

		[NonSerialized]
		ValueProviderList fValueProviders;
		public abstract object ValueAsObject { get; }

		protected virtual void AddSpecialisedValueProviders()
		{
		}

		object GetIsEmptyReplacement(string macro, Report report)
		{
			return IsEmpty;
		}

		protected abstract string NonEmptyWhereClause();

#if DEBUG
		bool IFilterFieldForUT.IsRequired
		{
			get
			{
				bool result = false;
				foreach (FilterCollectionValidator validator in Validators)
				{
					if (validator is RequiredFilterValidator)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}
#endif

		protected abstract bool FieldSpecificsIsCompatibleWith(FilterField otherFilterField);

		internal bool IsCompatibleWith(FilterField otherFilterField)
		{
			if (otherFilterField == null)
			{
				throw new ArgumentNullException(nameof(otherFilterField), "Can not check the compatibility with a null filter field");
			}

			if (this.DisplayName == otherFilterField.DisplayName && this.GetType().FullName == otherFilterField.GetType().FullName && this.FieldName == otherFilterField.FieldName)
			{
				return FieldSpecificsIsCompatibleWith(otherFilterField);
			}
			else
			{
				return false;
			}
		}

		internal string TemplateName { get; set; }

		public string Language { get; set; }

		#region Report Data

		internal protected void SetBaseFilterData(BaseFilterData filterData)
		{
			filterData.DisplayName = DisplayName;
			filterData.Tab = GroupName;
			filterData.IsRequired = Validators.Any(validator => validator.GetType().IsAssignableFrom(typeof(RequiredFilterValidator)));
			filterData.DependentFilter = DependentFilter;
			filterData.ReadOnlyIfFilter = ReadOnlyIfFilter;
		}

		public abstract void FillFilterData(ReportFilterData reportFilterData);
		public abstract void SetFilterValue(ReportFilterData reportFilterData);

		#endregion
	}
}
