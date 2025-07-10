using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Business
{
	public delegate void Validation(ZPropertyInfo info);

	[CodeProperty("Description")]
	[DescriptionProperty("Description")]
	public abstract class ModuleFilter : NonPersistentBusinessObject, IXmlSerializable, ICodeDescription, IModuleFilterForStrategyInternal, IModuleFilter, IObsoleteValidation
	{
		#region Construction

		public ModuleFilter(ZString description, SchemaColumn filterColumn)
			: this(description, filterColumn, ComparisonOptions.Default)
		{
		}

		public ModuleFilter(ZString description, SchemaColumn filterColumn, ComparisonOptions options)
			: this(description)
		{
			EnsureFilterColumnIsNotNull(filterColumn);
			FilterColumn = filterColumn;
			ComparisonOptions = options;
		}

		public ModuleFilter(ZString description, Delegate queryDelegate)
			: this(description)
		{
			EnsureQueryDelegateIsNotNull(queryDelegate);
			QueryDelegate = queryDelegate;
		}

		/// <summary>
		/// When this constructor is used, this filter represents the "Common" filter, eg. "Common Numbers".
		/// </summary>
		protected ModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: this(CommonFilterDescription(category))
		{
			EnsureParentCollectionsIsNotNull(parentCollection);
			fCategory = category;
			ParentCollection = parentCollection;
			MultilingualDescription = ResString.GetMultilingualString("ZArchitecture|ModuleFilter|CommonFilterOfCategory", "Common {0}", category.Description);
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected ModuleFilter(ZString description, BusinessObjectFactory factory = null)
			: base(factory)
		{
			EnsureDescriptionIsNotEmpty(description);
			fDescription = description;
			fCategory = DefaultCategory;
			fVisibility = FilterVisibility.Visible;
			failedFilterStrips = new List<string>();
		}

		public event EventHandler ModuleFilterChanged;

		protected void OnModuleFilterChanged()
		{
			if (ModuleFilterChanged != null)
			{
				ModuleFilterChanged(this, EventArgs.Empty);
			}
		}

		#region Ensure Not Null methods

		void EnsureParentCollectionsIsNotNull(ModuleFilterCollection parentCollection)
		{
			if (parentCollection == null)
			{
				throw new NullReferenceException(GetType().Name + " (" + Description + ") parentCollection cannot be null.");
			}
		}

		protected void EnsureDescriptionIsNotEmpty(ZString description)
		{
			if (description.IsEmpty)
			{
				throw new ArgumentException(GetType().Name + " description cannot be empty.");
			}
		}

		protected void EnsureFilterColumnIsNotNull(params SchemaColumn[] filterColumns)
		{
			foreach (var currentFilterColumn in filterColumns)
			{
				if (currentFilterColumn == null)
				{
					throw new NullReferenceException(GetType().Name + " (" + Description + ") filterColumn cannot be null.");
				}
			}
		}

		protected void EnsureQueryDelegateIsNotNull(params Delegate[] queryDelegates)
		{
			foreach (var currentQueryDelegate in queryDelegates)
			{
				if (currentQueryDelegate == null)
				{
					throw new NullReferenceException(GetType().Name + " (" + Description + ") queryDelegate cannot be null.");
				}
			}
		}

		protected void EnsureListIsNotNull(params IList[] lists)
		{
			foreach (var list in lists)
			{
				if (list == null)
				{
					throw new NullReferenceException(GetType().Name + " " + Description + " list cannot be null.");
				}
			}
		}

		protected void EnsureListDelegateIsNotNull(params GetList[] listDelegates)
		{
			foreach (var listDelegate in listDelegates)
			{
				if (listDelegate == null)
				{
					throw new NullReferenceException(GetType().Name + " " + Description + " listDelegate cannot be null.");
				}
			}
		}

		protected void EnsureListDelegateIsNotNull(params GetListUsingCurrentModuleFilter[] listDelegates)
		{
			foreach (var listDelegate in listDelegates)
			{
				if (listDelegate == null)
				{
					throw new NullReferenceException(GetType().Name + " " + Description + " listDelegate cannot be null.");
				}
			}
		}

		#endregion

		#endregion

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter description used as key")]
		public static ZString CommonFilterDescription(FilterCategory category)
		{
			return string.Format("Common {0}", category.Description.GetUnresolvedString());
		}

		public void Clear()
		{
			if (ShouldClear)
			{
				ClearCore();
			}
		}

		protected virtual bool ShouldClear => !ReadOnly;

		protected abstract void ClearCore();

		public bool IsEmpty => IsEmptyForSelectedFilters && IsEmptyCore;

		protected virtual bool IsEmptyForSelectedFilters => true;

		protected abstract bool IsEmptyCore { get; }

		protected internal abstract ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection);

		public ModuleFilter NewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection) => GetNewCommonModuleFilter(category, parentCollection);

		protected abstract void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom);

		public override void CopyTransientProperties(BusinessObject copy)
		{
			var copyOfModuleFilter = copy as ModuleFilter;
			if (copyOfModuleFilter != null)
			{
				CopyPersistantValuesFromFilter(copyOfModuleFilter);
				OrCategory = copyOfModuleFilter.OrCategory;
				IsActive = copyOfModuleFilter.IsActive;
			}
		}

		internal IActiveModuleFiltersProvider ActiveModuleFiltersProvider { get; set; }

		public IActiveModuleFiltersProvider ActiveModuleFiltersProviderHelper
		{
			get => ActiveModuleFiltersProvider;
			set { ActiveModuleFiltersProvider = value; }
		}

		[DebuggerDisplay("{FilterData.Length}, {FilterDataValues.Length}, {ModuleId}, {IsInFilterRuleMode}")]
		public struct SelectedFiltersCacheKey
		{
			public SelectedFiltersCacheKey(ZBlob filterData, ZBlob filterDataValues, ModuleIdentifier moduleId, bool isInFilterRuleMode)
			{
				this.FilterData = filterData;
				this.FilterDataValues = filterDataValues;
				this.ModuleId = moduleId;
				this.IsInFilterRuleMode = isInFilterRuleMode;
			}

			public ZBlob FilterData;
			public ZBlob FilterDataValues;
			public ModuleIdentifier ModuleId;
			public bool IsInFilterRuleMode;

			public override bool Equals(object obj)
			{
				return obj is SelectedFiltersCacheKey key
					&& key.FilterData.Equals(this.FilterData)
					&& key.FilterDataValues.Equals(this.FilterDataValues)
					&& key.ModuleId.Equals(this.ModuleId)
					&& key.IsInFilterRuleMode.Equals(this.IsInFilterRuleMode);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return FilterData.GetHashCode()
					+ FilterDataValues.GetHashCode() * 2647
					+ ModuleId?.GetHashCode() * 31 ?? 0
					+ IsInFilterRuleMode.GetHashCode() * 486187739;
				}
			}
		}

		static readonly ThreadLocalOverridable<MRUCache<SelectedFiltersCacheKey, Tuple<FilterStripBusinessObject, ZQuery>>> selectedFiltersCache = new ThreadLocalOverridable<MRUCache<SelectedFiltersCacheKey, Tuple<FilterStripBusinessObject, ZQuery>>>();

		public static MRUCache<SelectedFiltersCacheKey, Tuple<FilterStripBusinessObject, ZQuery>> SelectedFiltersCache
		{
			get
			{
				if (Globals.IsWebServiceOrWeb)
				{
					throw new DeveloperNotificationException("Do not use SelectedFiltersCache when in a web service or a web environment, since SelectedFiltersCache is ThreadLocal and web environments keep the thread in the thread pool to be reused, causing many Factory instances to also be kept."); // DeveloperNotificationException
				}

				object value = null;
				try
				{
					value = selectedFiltersCache.Value;
				}
				catch (ObjectDisposedException)
				{
				}
				if (value == null)
				{
					selectedFiltersCache.Value = new MRUCache<SelectedFiltersCacheKey, Tuple<FilterStripBusinessObject, ZQuery>>(20);
				}

				return selectedFiltersCache.Value;
			}
		}

		public static void ClearSelectedFiltersCache()
		{
			selectedFiltersCache.ResetValue();
		}

		#region MaxLength
		readonly int DefaultMaximumLength = 255;
		public const int MaxMaximumLength = 3998;

		public virtual int MaxLength
		{
			get
			{
				var result = DefaultMaximumLength;

				if (IsCommonModuleFilter)
				{
					var maxLengthForCommonFilter = 0;
					if (FilterColumn != null)
					{
						maxLengthForCommonFilter = FilterColumn.MaxLength;
					}
					foreach (var filter in ParentCollection)
					{
						if (filter.IsCommon && filter.Category == Category)
						{
							var currentMaxLength = filter.MaxLength;
							if (currentMaxLength > maxLengthForCommonFilter)
							{
								maxLengthForCommonFilter = currentMaxLength;
							}
						}
					}
					result = maxLengthForCommonFilter;
				}
				else if (FilterColumn != null)
				{
					if (FilterColumn.MaxLength != 0)
					{
						result = FilterColumn.MaxLength;
					}
					else
					{
						result = DefaultMaximumLength;
					}
				}
				else if (maxLength != 0)
				{
					result = maxLength;
				}

				return result < MaxMaximumLength ? result : MaxMaximumLength;
			}
			set
			{
				maxLength = value;
			}
		}
		internal int maxLength;

		public TFilter WithMaxLengthOf<TFilter>(SchemaColumn column) where TFilter : ModuleFilter
		{
			MaxLength = column.MaxLength;
			return (TFilter)this;
		}

		#endregion

		#region WasClearedWhenSettingDefault

		internal bool WasClearedWhenSettingDefault
		{
			get { return fWasClearedWhenSettingDefault; }
			set { fWasClearedWhenSettingDefault = value; }
		}

		bool fWasClearedWhenSettingDefault;

		#endregion

		#region SubGroup

		public BlueprintModuleFilterSubGroup SubGroup
		{
			get { return IsSubGroupEnabled ? fSubGroup : null; }

			set { fSubGroup = value; }
		}

		BlueprintModuleFilterSubGroup fSubGroup;

		protected virtual bool IsSubGroupEnabled => true;

		#endregion

		#region IsExclusive

		internal bool IsExclusive
		{
			get { return fIsExclusive; }
			set { fIsExclusive = value; }
		}

		bool fIsExclusive;

		public bool IsExclusiveHelper
		{
			get => IsExclusive;
			set { IsExclusive = value; }
		}

		#endregion

		public bool IsMandatorySecurityFilter { get; set; }

		public bool IsSingleInstanceOnly { get; set; }

		#region IsCommon

		public bool IsCommon
		{
			get { return fIsCommon; }
			set
			{
				if (!SupportsCommon)
				{
					throw new NotSupportedException(GetType().Name + " does not support IsCommon.");
				}
				fIsCommon = value;
			}
		}

		protected virtual bool SupportsCommon
		{
			get { return true; }
		}

		bool fIsCommon;

		#endregion

		#region IsActive

		public bool IsActive
		{
			get { return fIsActive; }
			set
			{
				if (fIsActive != value)
				{
					fIsActive = value;
					if (!fIsActive)
					{
						Clear();
						if (!this.IsOrCategoryReadOnly && this.OrCategory != FilterOrCategory.MandatoryFilterOrCategory)
						{
							//When deactivating a filter that's also in AlwaysAppliedModuleFilters, it erroneously maintains its colour despite no longer being visible. Prevent this edge case.
							//(Except for filters that intentionally force a non-None OrCategory, such as OSMG filters.)
							this.OrCategory = FilterOrCategory.None;
						}
					}
					OnIsActiveChanged();
				}
			}
		}

		public event EventHandler<IsActiveChangedEventArgs> IsActiveChanged;

		public class IsActiveChangedEventArgs : EventArgs
		{
			public IsActiveChangedEventArgs(ModuleFilter moduleFilter)
			{
				ModuleFilter = moduleFilter;
			}

			public readonly ModuleFilter ModuleFilter;
		}

		protected virtual void OnIsActiveChanged()
		{
			if (IsActiveChanged != null)
			{
				IsActiveChanged(this, new IsActiveChangedEventArgs(this));
			}
		}

		bool fIsActive;

		#endregion

		#region IsExpensiveQuery

		public abstract bool IsExpensiveQuery
		{
			get;
		}

		#endregion

		#region IsPublishedOnWeb

		public bool IsPublishedOnWeb
		{
			get { return fIsPublishedOnWeb; }
			set { fIsPublishedOnWeb = value; }
		}

		bool fIsPublishedOnWeb = true;

		#endregion

		#region Visibility

		public FilterVisibility Visibility
		{
			get { return fVisibility; }
			set { fVisibility = value; }
		}

		FilterVisibility fVisibility;

		#endregion

		#region Category

		public FilterCategory Category
		{
			get { return fCategory; }
			set
			{
				if (IsCommonModuleFilter)
				{
					throw new NotSupportedException("Cannot change category for the common filter '" + Description + "'.");
				}

				if (fCategory != value)
				{
					OnCategoryChanging();
					fCategory = value;
					OnCategoryChanged();
				}
			}
		}

		protected abstract FilterCategory DefaultCategory
		{
			get;
		}

		FilterCategory fCategory;

		#region Category Change events

		public event EventHandler<CategoryChangeEventArgs> CategoryChanging;
		public event EventHandler<CategoryChangeEventArgs> CategoryChanged;

		void OnCategoryChanging()
		{
			if (CategoryChanging != null)
			{
				CategoryChanging(this, new CategoryChangeEventArgs(this));
			}
		}

		void OnCategoryChanged()
		{
			if (CategoryChanged != null)
			{
				CategoryChanged(this, new CategoryChangeEventArgs(this));
			}
		}

		public class CategoryChangeEventArgs : EventArgs
		{
			public CategoryChangeEventArgs(ModuleFilter filter)
			{
				Filter = filter;
			}

			public readonly ModuleFilter Filter;
		}

		#endregion

		#endregion

		#region OrCategory

		public FilterOrCategory OrCategory
		{
			get { return fOrCategory; }
			set
			{
				if (OrCategory != value)
				{
					InvalidateCachedQuery();
				}

				fOrCategory = value;
			}
		}

		FilterOrCategory fOrCategory;

		public bool IsOrCategoryReadOnly { get; set; }

		#endregion

		#region FilterGroupName

		public ZString GroupName
		{
			get { return groupName; }
			set
			{
				if (GroupName != value)
				{
					InvalidateCachedQuery();
				}

				groupName = value;
			}
		}

		ZString groupName;

		#endregion

		#region GroupOrCategory

		public FilterOrCategory GroupOrCategory
		{
			get { return groupOrCategory; }
			set
			{
				if (GroupOrCategory != value)
				{
					InvalidateCachedQuery();
				}

				groupOrCategory = value;
			}
		}

		FilterOrCategory groupOrCategory;

		public bool IsGroupOrCategoryReadOnly { get; set; }

		#endregion

		#region IsDefaultDuplicate

		public bool IsDuplicateDefault { get; set; }

		#endregion

		#region FilterPriority

		FilterPriority filterPriority = FilterPriority.Default;
		public FilterPriority FilterPriority
		{
			get { return filterPriority; }
			set { filterPriority = value; }
		}

		#endregion

		#region Validation

		public ModuleFilterValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected abstract ModuleFilterValidation GetNewValidation();

		#endregion

		#region Query

		public bool IsQueryStale => CachedQuery == null || ShouldReevaluateQuery();

		public ZQuery Query
		{
			get
			{
				if (IsQueryStale)
				{
					try
					{
					CachedQuery = GetQuery();
					CachedQueryCreationTimeUtc = ZDateTime.UtcNow;
				}
					catch(TargetParameterCountException e)
					{
						ErrorReporter.ReportOnce("ModuleFilter.Query.ParameterCountMismatch", GetErrorReportMessage(), e);
						CachedQuery = new ZQuery();
					}
				}

				if (CachedQuery == null)
				{
					ErrorReporter.ReportOnce("ModuleFilter.Query", GetErrorReportMessage());
					CachedQuery = new ZQuery();
				}

				return CachedQuery.DeepClone();
			}
		}

		string GetErrorReportMessage()
		{
			return FormattableString.Invariant($@"CachedQuery is Null
Description: {Description}
Is Common Module Filter: {IsCommonModuleFilter}
Filter Type: {GetType()}
Has Query Delegate: {HasQueryDelegate}
Query Delegate Name: {QueryDelegate?.Method.Name}
Query Delegate Parameter Count: {QueryDelegateParameters?.Length}
Query Delegate Parameters: {string.Join(", ", QueryDelegateParameters?.Select(x => x.ToString()))}
Search Type: {FilterBusinessObject?.SearchType}
Other Filters: {string.Join(", ", FilterBusinessObject?.ModuleFilters.Select(f => f.Description))}
Other Index Filters: {string.Join(", ", FilterBusinessObject?.ModuleFilters.Where(v => v is IIndexSearchModuleFilter).Select(f => f.Description))}
Other Active Filters: {string.Join(", ", FilterBusinessObject?.ActiveModuleFilters.Select(f => f.Description))}
");
		}

		protected ZQuery CachedQuery { get; private set; }

		internal ZDateTime CachedQueryCreationTimeUtc { get; private set; }

		protected virtual bool ShouldReevaluateQuery()
		{
			// Query delegates can contain anything, e.g. Factory.Load and we have no control over it, so best to not use caching for those.
			// This can be overridden if the filter designer wants to use caching.
			return HasQueryDelegate || ShouldUseParentBusinessObjectsForQuery;
		}

		public virtual void InvalidateCachedQuery()
		{
			CachedQuery = null;
			OnCachedQueryInvalidated();
		}

		protected virtual void OnCachedQueryInvalidated()
		{
		}

		protected virtual ZQuery GetQuery()
		{
			ZQuery result;

			if (IsCommonModuleFilter)
			{
				result = CommonModuleFilterQuery;
			}
			else if (IsEmpty && Visibility != FilterVisibility.AlwaysApplied && Visibility != FilterVisibility.AlwaysAppliedAndHidden && !IsBlankOrNotBlankFilterWithoutQueryDelegate)
			{
				result = new ZQuery();
			}
			else if (HasQueryDelegate)
			{
				if (FilterBusinessObject != null && FilterBusinessObject.SearchType == SearchType.Index)
				{
					result = FilterBusinessObject.DoGlowQuery();
				}
				else
				{
					result = RunQueryDelegate();
				}
			}
			else
			{
				result = GetQueryUsingFilterColumns();
			}

			return result;
		}

		protected bool IsBlankOrNotBlankFilterWithoutQueryDelegate
		{
			get
			{
				var filter = this as IModuleFilterWithSqlComparisonParameter;
				bool result;
				if (filter == null || !filter.HasComparisonOperator)
				{
					result = false;
				}
				else
				{
					result = filter.SqlComparisonOperator == SpecialComparisonOperator.IsBlank || filter.SqlComparisonOperator == SpecialComparisonOperator.IsNotBlank;
				}
				return result;
			}
		}

		protected virtual ZQuery RunQueryDelegate()
		{
			var parameters = QueryDelegateParameters;
			return (ZQuery)QueryDelegate.DynamicInvoke(parameters);
		}

		protected virtual bool HasQueryDelegate
		{
			get { return QueryDelegate != null; }
		}

		public SchemaColumn FilterColumn { get; protected set; }

		public readonly ComparisonOptions ComparisonOptions;

		protected Type QueryDelegateType => QueryDelegate?.GetType();

		public Delegate QueryDelegate { get; protected set; }

		public delegate ZQuery GetIsBlankQuery();
		public GetIsBlankQuery IsBlankFilterQueryDelegate { get; set; }

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Will be handled by WI00886030")]
		protected abstract object[] QueryDelegateParameters { get; }
		protected abstract ZQuery GetQueryUsingFilterColumns();

		#endregion

		#region ForceProcessingGroup

		protected internal virtual bool ForceProcessingGroup => false;

		#endregion

		#region ShallowClone

		IDisposable ClearNotificationsForClone()
		{
			var errors = HasNotifications();
			if (errors)
			{
				ClearAllNotifications();
			}

			return new DisposableAction(() =>
			{
				if (errors)
				{
					Validation?.ValidateAll();
				}
			});
		}

		public ModuleFilter ShallowCloneAndClearValues(string newDescription, FilterVisibility visibility = FilterVisibility.Visible)
		{
			var result = ShallowClone();
			result.fDescription = newDescription;
			result.Visibility = visibility;
			result.Clear();

			return result;
		}

		public ModuleFilter ShallowClone()
		{
			return ShallowCloneCore();
		}

		protected virtual ModuleFilter ShallowCloneCore()
		{
			using (ClearNotificationsForClone())
			{
				return (ModuleFilter)this.MemberwiseClone();
			}
		}

		#endregion

		#region Common Module Filter

		[SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		ZQuery CommonModuleFilterQuery
		{
			get
			{
				var result = new ZQuery();

				if (!IsEmpty)
				{
					foreach (var filter in ParentCollection)
					{
						if (filter.IsCommon && filter.Category == Category)
						{
							filter.CopyPersistantValuesFromFilter(this);
							result.AddToFilter(filter.Query, JoinCondition.Or);
						}
					}
				}

				return result;
			}
		}

		internal bool IsCommonModuleFilter
		{
			get { return ParentCollection != null; }
		}

		readonly ModuleFilterCollection ParentCollection;

		#endregion

		#region Template Filter Query

		public ZQuery XQuery => GetXQuery != null ? GetXQuery(this) : XQueryCore;
		protected virtual ZQuery XQueryCore => new ZQuery();

		public XQueryFilterInfo XQueryInfo { get; set; }

		[BusinessObjectTestExclude]
		public Func<ModuleFilter, ZQuery> GetXQuery { get; set; }

		public ZBool Visible { get; set; } = true;

		public ZBool SupportsXQuery
		{
			get
			{
				return supportsXQuery || GetXQuery != null || XQueryInfo != null;
			}
			set
			{
				supportsXQuery = value;
			}
		}
		ZBool supportsXQuery;

		#endregion

		#region IXmlSerializable Members

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			try
			{
				DeserializePropertiesFromXml(reader);
			}
			catch (ZTypeValueException)
			{
				failedFilterStrips.Add(Description);
			}
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			SerializePropertiesToXml(writer);
		}

		protected abstract void SerializePropertiesToXml(XmlWriter writer);
		protected abstract void DeserializePropertiesFromXml(XmlReader reader);

		public void DeserializeProperties(XmlReader reader) => DeserializePropertiesFromXml(reader);

		#endregion

		#region Failed Filter Strips

		readonly List<string> failedFilterStrips;
		protected internal IEnumerable<string> FailedFilterStrips
		{
			get { return failedFilterStrips; }
		}

		#endregion

		#region ICodeDescription Members

		object ICodeDescription.PK
		{
			get { return null; }
		}

		string ICodeDescription.Code
		{
			get { return Code; }
		}

		public ZString Code
		{
			get { return Description; }
		}

		public ZString OriginalCode
		{
			get { return originalCode.IsEmpty ? Code : originalCode; }
			internal set { originalCode = value; }
		}
		ZString originalCode;

		string ICodeDescription.Description
		{
			get { return Description; }
		}

		string IModuleFilterForStrategyInternal.Description
		{
			get { return fDescription; }
			set { fDescription = value; }
		}

		public ZString Description
		{
			get { return fDescription; }
		}

		ZString fDescription;

		public MultilingualString MultilingualDescription { get; set; }

		internal bool HasMultilingualDescription
		{
			get { return MultilingualDescription != null; }
		}

		public MultilingualString LocalizedDescription
		{
			get
			{
				var result = HasMultilingualDescription ? (string.IsNullOrEmpty(Prefix) ? MultilingualDescription : ResString.GetMultilingualString("1D2DEC6A-1496-44F7-ACE2-55FA302B51EB", "{0} [{1}]", MultilingualDescription, Prefix)) : (NoResString)Description;

				return string.IsNullOrEmpty(LocalizedDescriptionSuffix) ? result : (NoResString)string.Format(CultureInfo.InvariantCulture, "{0} {1}", result, LocalizedDescriptionSuffix);
			}
		}

		public ZString DescriptionWithoutInstanceNumber
		{
			get
			{
				if (!IsDuplicateDefault)
				{
					return Description;
				}

				var suffix = Regex.Match(Description, @" \(\d+\)", RegexOptions.RightToLeft)?.Value ?? string.Empty;
				return Description.Substring(0, Description.Length - suffix.Length);
			}
		}

		public bool IsAttributeFilter { get; set; }

		#endregion

		#region Prefix / Suffix

		public string Prefix { get; set; }
		public string LocalizedDescriptionSuffix { get; set; }

		#endregion

		public virtual bool SetValueFromInitialCode(ZString initialProperty, ZString initialCode)
		{
			return false;
		}

		public virtual bool ShouldSetValueFromInitialCode(ZString initialProperty, ZString initialCode)
		{
			return false;
		}

		public virtual ZString GetFormattedInitialCode_FilterColumnName()
		{
			return ZString.Empty;
		}

		public virtual ZString GetFormattedInitialCode_Prefix()
		{
			return ZString.Empty;
		}

		public virtual ZQuery GetQueryForParentBusinessObjects(IEnumerable<BusinessObject> parents)
		{
			using (SetParentBusinessObjectsForQuery(parents))
			{
				return Query;
			}
		}

		protected virtual bool ShouldSetValueFromInitialCode_Prefix(ZString initialCode)
		{
			return initialCode.Contains(":", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(Prefix) && (initialCode.Split(':').First() == Prefix);
		}

		protected virtual bool ShouldSetValueFromInitialCode_FilterColumnName(ZString initialProperty)
		{
			return FilterColumn != null && FilterColumn.Name == initialProperty;
		}

		public FilterStripBusinessObject FilterBusinessObject { get; protected internal set; }

		public void SetFilterBusinessObject(ModuleUserDefinedFilter filter, FilterStripBusinessObject filterBizO)
		{
			filter.FilterBusinessObject = filterBizO;
		}

		protected IEnumerable<BusinessObject> ParentBusinessObjectsForQuery => parentBusinessObjectsForQuery ?? Enumerable.Empty<BusinessObject>();

		protected ZBool ShouldUseParentBusinessObjectsForQuery => parentBusinessObjectsForQuery != null;

		IDisposable SetParentBusinessObjectsForQuery(IEnumerable<BusinessObject> parentBusinessObjectsForQuery)
		{
			this.parentBusinessObjectsForQuery = parentBusinessObjectsForQuery;
			return new DisposableAction(() => parentBusinessObjectsForQuery = null);
		}

		IEnumerable<BusinessObject> parentBusinessObjectsForQuery;

		#region Test Data Setup
#if DEBUG

		public void FillWithValidTestFilterValue()
		{
			FillWithValidTestFilterValueCore();
		}

		protected abstract void FillWithValidTestFilterValueCore();

		static Random Random => random ?? (random = new Random());
		[ThreadStatic]
		static Random random;

		protected static string RandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(Enumerable.Repeat(chars, length)
			  .Select(s => s[Random.Next(s.Length)]).ToArray());
		}

		protected static int RandomInt(int maxValue)
		{
			return Random.Next(maxValue);
		}

		public ZQuery CachedQuery_ForTest
		{
			get { return CachedQuery; }
			set { CachedQuery = value; }
		}

#endif
		#endregion

		internal void OnAddToActiveFiltersQuery()
		{
			OnAddToActiveFiltersQueryCore();
		}

		protected virtual void OnAddToActiveFiltersQueryCore()
		{
		}
	}

	public interface IActiveModuleFiltersProvider
	{
		IEnumerable<ModuleFilter> ActiveModuleFilters { get; }
		IEnumerable<ModuleFilter> AlwaysAppliedModuleFilters { get; }
	}

	#region class Validation

	public abstract class ModuleFilterValidation : ZValidation
	{
		public ModuleFilterValidation(ModuleFilter parent)
			: base(parent)
		{
		}

		protected void ErrorIfInvalidCode(ZPropertyInfo info, IList list)
		{
			var collection = list as IBusinessObjectCollection;
			if (collection != null)
			{
				ListValidation.ErrorIfInvalidCode(info, collection);
			}
			else
			{
				var codeDescPairList = list as ICodeDescriptionPairList;
				if (codeDescPairList != null)
				{
					ListValidation.ErrorIfInvalidCode(info, codeDescPairList);
				}
			}
		}

		protected void ErrorIfInvalidPK(ZPropertyInfo info, IList list)
		{
			var collection = list as IBusinessObjectCollection;
			if (collection != null)
			{
				ListValidation.ErrorIfInvalidPK(info, collection);
			}
			else
			{
				var codeDescPairList = list as ICodeDescriptionPairList;
				if (codeDescPairList != null)
				{
					ListValidation.ErrorIfInvalidPK(info, codeDescPairList);
				}
			}
		}

		bool IsTemplateRecordFilter(ModuleFilter activeModuleFilter) => (
			activeModuleFilter.Description == FilterStripBusinessObject.TemplateRecordsDescription ||
			activeModuleFilter.Description == FilterStripBusinessObject.TemplateRecordsActive ||
			activeModuleFilter.Description == FilterStripBusinessObject.TemplateRecordsTemplateName
		);

		protected override void RunAdditionalValidationOnValidationObject(ZPropertyInfo propertyInfo)
		{
			base.RunAdditionalValidationOnValidationObject(propertyInfo);

			var moduleFilter = (ModuleFilter)propertyInfo.BizObj;
			if (moduleFilter.ActiveModuleFiltersProvider != null && moduleFilter.IsActive && !moduleFilter.IsEmpty)
			{
				foreach (var activeModuleFilter in moduleFilter.ActiveModuleFiltersProvider.ActiveModuleFilters)
				{
					if (activeModuleFilter != moduleFilter && activeModuleFilter.IsExclusive && !activeModuleFilter.IsEmpty)
					{
						if (moduleFilter.IsExclusive)
						{
							if (moduleFilter.OrCategory == FilterOrCategory.None || moduleFilter.OrCategory != activeModuleFilter.OrCategory)
							{
								propertyInfo.AddError(Res.GetString("0C073E47-600B-42FB-B077-C7B31B1A6948", "Only one exclusive filter should be selected or all exclusive filters should have same OR category."));
							}
						}
						else if (!moduleFilter.IsMandatorySecurityFilter)
						{
							propertyInfo.AddWarning(Res.GetString("BF76FAFF-AB02-4A0D-B145-CB1260E3202F", "An exclusive filter is selected, so this filter will be ignored."));
						}
					}
					else if (activeModuleFilter != moduleFilter
						&& IsTemplateRecordFilter(activeModuleFilter)
						&& ((ModuleTextFilter)activeModuleFilter).Property != FilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesExcluded)
					{
						if (!moduleFilter.SupportsXQuery)
						{
							propertyInfo.AddWarning(Res.GetString("d4adaa5b-df94-4910-88ac-664e35a554a4", "This filter does not support template records."));
						}
					}
				}
			}
		}
	}

	#endregion
}
