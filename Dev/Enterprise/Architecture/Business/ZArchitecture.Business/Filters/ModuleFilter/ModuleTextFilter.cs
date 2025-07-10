using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Business
{
	public delegate ZQuery GetTextQueryWithOperator(SQLComparisonOperator comparisonOperator, ZString value);
	public delegate ZQuery GetTextQuery(ZString value);

	// this class is split so that the ModuleTextAndNkFilter can pass its typed delegate to the base (as you cannot inherit delegates).

	#region class ModuleTextFilter

	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ModuleTextFilter : ModuleTextBaseFilter
	{
		#region Construction

		protected ModuleTextFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ModuleTextFilter(ZString description, SchemaStringColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		public ModuleTextFilter(ZString description, SchemaStringColumn filterColumn, ComparisonOptions options)
			: base(description, filterColumn, options)
		{
		}

		public ModuleTextFilter(ZString description, SchemaStringColumn filterColumn, IList list)
			: base(description, filterColumn, list)
		{
		}

		public ModuleTextFilter(ZString description, SchemaStringColumn filterColumn, GetList listDelegate)
			: base(description, filterColumn, listDelegate)
		{
		}

		public ModuleTextFilter(ZString description, GetTextQueryWithOperator queryDelegate)
			: base(description, queryDelegate)
		{
		}

		public ModuleTextFilter(ZString description, GetTextQuery queryDelegate, IList list)
			: base(description, queryDelegate, list)
		{
		}

		public ModuleTextFilter(ZString description, Delegate queryDelegate, GetList listDelegate)
			: base(description, queryDelegate, listDelegate)
		{
		}

		public ModuleTextFilter(ZString description, GetTextQueryWithOperator queryDelegate, IList list)
			: base(description, queryDelegate, list)
		{
		}

		public ModuleTextFilter(ZString description, GetTextQuery queryDelegate, GetList listDelegate)
			: base(description, queryDelegate, listDelegate)
		{
		}

		public ModuleTextFilter(ZString description, GetTextQueryWithOperator queryDelegate, GetList listDelegate)
			: base(description, queryDelegate, listDelegate)
		{
		}

		public ModuleTextFilter(ZString description, Delegate queryDelegate)
			: base(description, queryDelegate)
		{
		}

		public ModuleTextFilter(ZString description, Delegate queryDelegate, IList list)
			: base(description, queryDelegate, list)
		{
		}

		#endregion

		#region GetNewCommonModuleFilter

		protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new ModuleTextFilter(category, parentCollection);
		}

		#endregion

		#region SetValueFromInitialCode

		public override bool SetValueFromInitialCode(ZString initialProperty, ZString initialCode)
		{
			if (ShouldSetValueFromInitialCode_Prefix(initialCode))
			{
				var values = initialCode.Split(':');
				Property = values[1];
				Visibility = FilterVisibility.AlwaysVisible;
				return true;
			}
			else if (ShouldSetValueFromInitialCode_FilterColumnName(initialProperty))
			{
				Property = initialCode;
				Visibility = FilterVisibility.AlwaysVisible;
				return true;
			}

			return false;
		}

		public override bool ShouldSetValueFromInitialCode(ZString initialProperty, ZString initialCode)
		{
			return ShouldSetValueFromInitialCode_Prefix(initialCode) || ShouldSetValueFromInitialCode_FilterColumnName(initialProperty);
		}

		public override ZString GetFormattedInitialCode_FilterColumnName()
		{
			return Property;
		}

		public override ZString GetFormattedInitialCode_Prefix()
		{
			return Prefix + ":" + Property;
		}
		#endregion

		#region Overrides

		const int MaxEntries = 100;

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			if (UseMultiSearch)
			{
				var result = new ZQuery();
				result.AddToFilter_PossiblyCommaSeparated(FilterColumn, SqlComparisonOperator, Property, ComparisonOptions);
				return result;
			}
			return base.GetQueryUsingFilterColumns();
		}

		protected override ZQuery GetQuery()
		{
			var result = base.GetQuery();
			if ((FilterColumn?.IsSparse ?? false) && SqlComparisonOperator.IsNegativeSQLOperator())
			{
				result.AddToFilter(JoinCondition.Or, FilterColumn, SQLComparisonOperator.Equal, null);
			}
			return result;
		}

		public static int MultiplyMaxLength(int maxLength)
		{
			return Math.Min(ModuleFilter.MaxMaximumLength, maxLength * MaxEntries + (MaxEntries - 1));
		}

		public override int MaxLength
		{
			get
			{
				var maxLength = base.MaxLength;
				if (UseMultiSearch && maxLength > 0 && maxLength < 1024)
				{
					return MultiplyMaxLength(maxLength);
				}
				return maxLength;
			}
			set
			{
				base.MaxLength = value;
			}
		}

		protected override int Property_MaxLength
		{
			get
			{
				var maxLength = base.Property_MaxLength;
				if (UseMultiSearch && maxLength > 0 && maxLength < 1024)
				{
					return MultiplyMaxLength(maxLength);
				}
				return maxLength;
			}
		}

		protected override bool ShouldContainsOrStartsWithOrEndsWithBeConsideredExact(SchemaColumn schemaColumn)
		{
			return schemaColumn != null && (!UseMultiSearch || !Property.Contains(EnvProxy.Instance.Registry.MultiSearchSeparator, StringComparison.Ordinal)) && schemaColumn.MaxLength == Property.ToString().Trim().Length;
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			var filler = (List != null && List.Count > 0) ? List[0].ToString() : RandomInt(99).ToString(CultureInfo.InvariantCulture);
			Property = filler.Length <= MaxLength ? filler : filler.Substring(0, MaxLength);
		}

#endif
		#endregion
	}
	#endregion

	#region ModuleFilterWithListAndComparisonOperators

	public interface IModuleFilterWithSqlComparisonParameter
	{
		SQLComparisonOperator SqlComparisonOperator { get; }
		bool HasComparisonOperator { get; }
	}

	public interface IModuleFilterWithComparisonOperator : IModuleFilterWithSqlComparisonParameter
	{
		ZString ComparisonOperator { get; set; }
		IReadOnlyList<string> AllowedComparisonOperators { get; }
		ZString GetComparisonOperatorDefault();
	}

	public abstract class ModuleFilterWithListAndComparisonOperators<T> : ModuleFilterWithList, IModuleFilterWithComparisonOperator, ISupportMultiValuesFilter where T : IZType
	{
		#region Construction

		protected ModuleFilterWithListAndComparisonOperators(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ModuleFilterWithListAndComparisonOperators(ZString description, SchemaStringColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		public ModuleFilterWithListAndComparisonOperators(ZString description, SchemaStringColumn filterColumn, ComparisonOptions options)
			: base(description, filterColumn, options)
		{
		}

		public ModuleFilterWithListAndComparisonOperators(ZString description, SchemaStringColumn filterColumn, IList list)
			: base(description, filterColumn, list)
		{
		}

		public ModuleFilterWithListAndComparisonOperators(ZString description, SchemaStringColumn filterColumn, GetList listDelegate)
			: base(description, filterColumn, listDelegate)
		{
		}

		public ModuleFilterWithListAndComparisonOperators(ZString description, Delegate queryDelegate)
			: base(description, queryDelegate)
		{
		}

		public ModuleFilterWithListAndComparisonOperators(ZString description, Delegate queryDelegate, IList list)
			: base(description, queryDelegate, list)
		{
		}

		public ModuleFilterWithListAndComparisonOperators(ZString description, Delegate queryDelegate, GetList listDelegate)
			: base(description, queryDelegate, listDelegate)
		{
		}

		public ModuleFilterWithListAndComparisonOperators(ZString description, SchemaColumn filterColumn, GetListUsingCurrentModuleFilter listUsingCurrentModuleFilterDelegate)
			: base(description, filterColumn, listUsingCurrentModuleFilterDelegate)
		{
		}

		public ModuleFilterWithListAndComparisonOperators(ZString description, Delegate queryDelegate, GetListUsingCurrentModuleFilter listUsingCurrentModuleFilterDelegate)
			: base(description, queryDelegate, listUsingCurrentModuleFilterDelegate)
		{
		}

		public ModuleFilterWithListAndComparisonOperators(ZString description, SchemaGuidColumn filterColumn, IBusinessObjectCollection list)
			: base(description, filterColumn, list)
		{
		}

		public ModuleFilterWithListAndComparisonOperators(ZString description, SchemaGuidColumn filterColumn, GetList listDelegate)
			: base(description, filterColumn, listDelegate)
		{
		}

		#endregion

		#region Comparison Operator

		public static class ComparisonConstants
		{
			#region SuppressResourceStringsCheckRegion
			public const string Exact = "exact";
			public const string StartsWith = "starts with";
			public const string EndsWith = "ends with";
			public const string Contains = "contains";
			public const string NotEqual = "not equal";
			public const string NotStartsWith = "not starting";
			public const string NotContain = "not contain";
			public const string IsBlank = "is blank";
			public const string IsNotBlank = "is not blank";
			public const string CurrentUser = "current user";
			public const string FiltersMatch = "filters match";
			public const string AllMatch = "all match";
			public const string AnyMatch = "any match";
			public const string NoneMatch = "none match";
			[SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Following existing pattern.")]
			public const string NotFound = "not found";
			#endregion

			public const string Default = StartsWith;

			[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "There are simply a large number of options, sadly.")]
			public static CodeDescriptionPair GetComparisonOperatorPair(string code)
			{
				switch (code)
				{
					case Exact:
						return new CodeDescriptionPair(ResString.GetMultilingualString("ComparisonConstants|Exact", "exact"), ResString.GetMultilingualString("TextFilter|ComparisonConstraintDescription|Exact", "Search for an Exact match"));
					case Contains:
						return new CodeDescriptionPair(ResString.GetMultilingualString("ComparisonConstants|Contains", "contains"), ResString.GetMultilingualString("TextFilter|ComparisonConstraintDescription|Contains", "Search for fields that Contain the supplied text (this may be slow)"));
					case NotEqual:
						return new CodeDescriptionPair(ResString.GetMultilingualString("ComparisonConstants|NotEqual", "not equal"), ResString.GetMultilingualString("TextFilter|ComparisonConstraintDescription|NotEqual", "Searches for fields where the value is not equal to the supplied text"));
					case NotStartsWith:
						return new CodeDescriptionPair(ResString.GetMultilingualString("ComparisonConstants|NotStarting", "not starting"), ResString.GetMultilingualString("TextFilter|ComparisonConstraintDescription|NotStartsWith", "Searches for fields that do Not Start With the supplied text"));
					case NotContain:
						return new CodeDescriptionPair(ResString.GetMultilingualString("ComparisonConstants|NotContain", "not contain"), ResString.GetMultilingualString("TextFilter|ComparisonConstraintDescription|NotContain", "Search for fields that do Not Contain the supplied text (this may be slow)"));
					case IsBlank:
						return new CodeDescriptionPair(ResString.GetMultilingualString("ComparisonConstants|IsBlank", "is blank"), ResString.GetMultilingualString("TextFilter|ComparisonConstraintDescription|IsBlank", "Search for fields that are blank"));
					case IsNotBlank:
						return new CodeDescriptionPair(ResString.GetMultilingualString("ComparisonConstants|IsNotBlank", "is not blank"), ResString.GetMultilingualString("TextFilter|ComparisonConstraintDescription|IsNotBlank", "Search for fields that are not blank"));
					case CurrentUser:
						return new CodeDescriptionPair(ResString.GetMultilingualString("ComparisonConstants|CurrentUser", "current user"), ResString.GetMultilingualString("TextFilter|ComparisonConstraintDescription|CurrentUser", "Searches for fields where the value is equal to the current user's staff code"));
					case FiltersMatch:
						return new CodeDescriptionPair(ResString.GetMultilingualString("ComparisonConstants|FiltersMatch", "filters match"), ResString.GetMultilingualString("TextFilter|ComparisonConstraintDescription|FiltersMatch", "Searches for any field that matches the specified filters"));
					case AllMatch:
						return new CodeDescriptionPair(ResString.GetMultilingualString("ComparisonConstants|AllMatch", "all match"), ResString.GetMultilingualString("TextFilter|ComparisonConstraintDescription|AllMatch", "Searches for records where all the items in this relationship match the specified filters"));
					case AnyMatch:
						return new CodeDescriptionPair(ResString.GetMultilingualString("ComparisonConstants|AnyMatch", "any match"), ResString.GetMultilingualString("TextFilter|ComparisonConstraintDescription|AnyMatch", "Searches for records where any item in this relationship matches the specified filters"));
					case NoneMatch:
						return new CodeDescriptionPair(ResString.GetMultilingualString("ComparisonConstants|NoneMatch", "none match"), ResString.GetMultilingualString("TextFilter|ComparisonConstraintDescription|NoneMatch", "Searches for records where no items in this relationship match the specified filters"));
					case NotFound:
						return new CodeDescriptionPair(ResString.GetMultilingualString("ComparisonConstants|NotFound", "not found"), ResString.GetMultilingualString("TextFilter|ComparisonConstraintDescription|NotFound", "Searches for records that do not have a specified relationship"));
					case EndsWith:
						return new CodeDescriptionPair(ResString.GetMultilingualString("ComparisonConstants|EndsWith", "ends with"), ResString.GetMultilingualString("TextFilter|ComparisonConstraintDescription|EndsWith", "Search for fields that End With the supplied text"));
					case StartsWith:
					default:
						return new CodeDescriptionPair(ResString.GetMultilingualString("ComparisonConstants|StartsWith", "starts with"), ResString.GetMultilingualString("TextFilter|ComparisonConstraintDescription|StartsWith", "Search for fields that Start With the supplied text"));
				}
			}

			[SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Methode called once")]
			public static CodeDescriptionPairList GetAllComparisonOperators()
			{
				var list = new CodeDescriptionPairList();
				list.Add(GetComparisonOperatorPair(Exact));
				list.Add(GetComparisonOperatorPair(StartsWith));
				list.Add(GetComparisonOperatorPair(EndsWith));
				list.Add(GetComparisonOperatorPair(Contains));
				list.Add(GetComparisonOperatorPair(NotEqual));
				list.Add(GetComparisonOperatorPair(NotStartsWith));
				list.Add(GetComparisonOperatorPair(NotContain));
				list.Add(GetComparisonOperatorPair(IsBlank));
				list.Add(GetComparisonOperatorPair(IsNotBlank));
				list.Add(GetComparisonOperatorPair(CurrentUser));
				list.Add(GetComparisonOperatorPair(FiltersMatch));
				list.Add(GetComparisonOperatorPair(AnyMatch));
				list.Add(GetComparisonOperatorPair(AllMatch));
				list.Add(GetComparisonOperatorPair(NoneMatch));
				list.Add(GetComparisonOperatorPair(NotFound));
				return list;
			}
		}

		[BusinessObjectTestExclude]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual ZString ComparisonOperator // this exists for binding
		{
			get
			{
				if (fComparisonOperator.IsEmpty)
				{
					fComparisonOperator = GetComparisonOperatorDefault();
				}
				return fComparisonOperator;
			}
			set
			{
				EnsureSupportsComparisonOperatorSet();

				if (fComparisonOperator != value && CheckIsAllowedComparisonOperator(value))
				{
					fComparisonOperator = value;

					ComparisonOperatorInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateComparisonOperator();
					}

					ComparisonOperatorChanged?.Invoke(this, EventArgs.Empty);
					OnComparisonOperatorChanged();
					InvalidateCachedQuery();
				}
			}
		}

		#region

		public new ModuleFilterWithListAndComparisonOperatorsValidation Validation
		{
			get { return (ModuleFilterWithListAndComparisonOperatorsValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleFilterWithListAndComparisonOperatorsValidation(this);
		}

		public bool ContainsBanned { get; set; }

		public class ModuleFilterWithListAndComparisonOperatorsValidation : ModuleFilterValidation
		{
			public ModuleFilterWithListAndComparisonOperatorsValidation(ModuleFilterWithListAndComparisonOperators<T> parent)
				: base(parent)
			{
				Parent = parent;
			}

			public override void ValidateAll()
			{
				ValidateComparisonOperator();
			}

			#region ValidateComparisonOperator

			public void ValidateComparisonOperator()
			{
				ValidateCalculatedProperty(Parent.ComparisonOperatorInfo);
			}

			protected virtual void CheckComparisonOperator()
			{
				if (Parent.ContainsBanned)
				{
					if (Parent.SqlComparisonOperator == SQLComparisonOperator.Contains || Parent.SqlComparisonOperator == SQLComparisonOperator.NotContains)
					{
						Parent.ComparisonOperatorInfo.AddError(EnvProxy.Instance.Security.ContainsInNumbersAndReferences.ErrorMessageForNotAllowed);
					}
				}
			}

			#endregion

			public override Type AutoValidationType => GetType();

			protected readonly ModuleFilterWithListAndComparisonOperators<T> Parent;
		}

		#endregion

		ZString ComparisonOperatorForGrouping
		{
			get
			{
				var comparisonOperator = ComparisonOperator;

				switch (comparisonOperator)
				{
					case ComparisonConstants.Contains:
					case ComparisonConstants.StartsWith:
					case ComparisonConstants.EndsWith:
						if (ShouldContainsOrStartsWithOrEndsWithBeConsideredExact(FilterColumn))
						{
							comparisonOperator = ComparisonConstants.Exact;
						}
						break;
					case ComparisonConstants.CurrentUser:
						comparisonOperator = ComparisonConstants.Exact;
						break;
				}

				return comparisonOperator;
			}
		}

		protected virtual bool ShouldContainsOrStartsWithOrEndsWithBeConsideredExact(SchemaColumn schemaColumn)
		{
			return schemaColumn != null && Property != null && schemaColumn.MaxLength == Property.ToString().Trim().Length;
		}

		protected virtual void OnComparisonOperatorChanged()
		{
		}

		public event EventHandler ComparisonOperatorChanged;

		public ZString GetComparisonOperatorDefault()
		{
			var result = ZString.Empty;
			if (ComparisonOperator_List.DefaultCode != null && ComparisonOperator_List.ContainsCode(ComparisonOperator_List.DefaultCode))
			{
				result = ComparisonOperator_List.DefaultCode;
			}
			else if (ComparisonOperator_List.ContainsCode(ComparisonConstants.Default) || ComparisonOperator_List.Count == 0)
			{
				result = ComparisonConstants.Default;
			}
			else
			{
				result = ComparisonOperator_List[0].Code;
			}
			return result;
		}

		bool CheckIsAllowedComparisonOperator(ZString value)
		{
			foreach (var allowed in AllowedComparisonOperators)
			{
				if (value.EqualsIgnoringCase(allowed))
				{
					return true;
				}
			}
			return false;
		}

		public virtual IReadOnlyList<string> AllowedComparisonOperators
		{
			get
			{
				var result = new List<string>
				{
					string.Empty,
					ComparisonConstants.Exact,
					ComparisonConstants.StartsWith,
					ComparisonConstants.Contains,
					ComparisonConstants.NotEqual,
					ComparisonConstants.NotStartsWith,
					ComparisonConstants.NotContain
				};

				if (SupportsEndsWithComparisonOperator)
				{
					result.Add(ComparisonConstants.EndsWith);
				}

				if (!IsCommonModuleFilter && SupportsBlankComparisonOperators)
				{
					result.Add(ComparisonConstants.IsBlank);
					result.Add(ComparisonConstants.IsNotBlank);
				}

				return result;
			}
		}

		public bool SupportsBlankComparisonOperators { get; set; } = true;
		public bool SupportsEndsWithComparisonOperator { get; set; }

		public virtual SQLComparisonOperator SqlComparisonOperator
		{
			get
			{
				var comparator = GetSqlComparisonOperator(ComparisonOperator, DefaultSqlComparisonOperator);

				if (comparator.In(SQLComparisonOperator.Contains, SQLComparisonOperator.StartsWith, SQLComparisonOperator.EndsWith)
					&& ShouldContainsOrStartsWithOrEndsWithBeConsideredExact(FilterColumn))
				{
					comparator = SQLComparisonOperator.Equal;
				}

				return comparator;
			}
			set
			{
				EnsureSupportsComparisonOperatorSet();
				ComparisonOperator = GetComparisonOperatorFromSql(value);
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "There are simply a large number of options, sadly.")]
		public static SQLComparisonOperator GetSqlComparisonOperator(ZString comparisonOperator, SQLComparisonOperator defaultSqlComparisonOperator)
		{
			switch (comparisonOperator)
			{
				case ComparisonConstants.Exact:
				case ComparisonConstants.CurrentUser:
					return SQLComparisonOperator.Equal;

				case ComparisonConstants.StartsWith:
					return SQLComparisonOperator.StartsWith;
				case ComparisonConstants.EndsWith:
					return SQLComparisonOperator.EndsWith;
				case ComparisonConstants.Contains:
					return SQLComparisonOperator.Contains;
				case ComparisonConstants.NotEqual:
					return SQLComparisonOperator.NotEqual;
				case ComparisonConstants.NotContain:
					return SQLComparisonOperator.NotContains;
				case ComparisonConstants.NotStartsWith:
					return SQLComparisonOperator.DoesNotStartWith;
				case ComparisonConstants.IsBlank:
					return SpecialComparisonOperator.IsBlank;
				case ComparisonConstants.IsNotBlank:
					return SpecialComparisonOperator.IsNotBlank;

				case ComparisonConstants.FiltersMatch:
				case ComparisonConstants.AllMatch:
				case ComparisonConstants.AnyMatch:
				case ComparisonConstants.NoneMatch:
				case ComparisonConstants.NotFound:
					return SQLComparisonOperator.NotSpecified;

				default:
					return defaultSqlComparisonOperator;
			}
		}

		public static ZString GetComparisonOperatorFromSql(SQLComparisonOperator sqlComparisonOperator)
		{
			if (sqlComparisonOperator == SQLComparisonOperator.Equal)
			{
				return ComparisonConstants.Exact;
			}

			if (sqlComparisonOperator == SQLComparisonOperator.StartsWith)
			{
				return ComparisonConstants.StartsWith;
			}

			if (sqlComparisonOperator == SQLComparisonOperator.EndsWith)
			{
				return ComparisonConstants.EndsWith;
			}

			if (sqlComparisonOperator == SQLComparisonOperator.Contains)
			{
				return ComparisonConstants.Contains;
			}

			if (sqlComparisonOperator == SQLComparisonOperator.NotEqual)
			{
				return ComparisonConstants.NotEqual;
			}

			if (sqlComparisonOperator == SQLComparisonOperator.NotContains)
			{
				return ComparisonConstants.NotContain;
			}

			if (sqlComparisonOperator == SQLComparisonOperator.DoesNotStartWith)
			{
				return ComparisonConstants.NotStartsWith;
			}

			if (sqlComparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				return ComparisonConstants.IsNotBlank;
			}

			if (sqlComparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				return ComparisonConstants.IsBlank;
			}

			throw new ArgumentException("Only Equal, StartsWith, EndsWith, Contains, NotEqual, NotContains, DoesNotStartWith, IsBlank, IsNotBlank are supported.");
		}

		protected virtual SQLComparisonOperator DefaultSqlComparisonOperator
		{
			get { return SQLComparisonOperator.StartsWith; }
		}

		public ZPropertyInfo ComparisonOperatorInfo
		{
			get { return GetZPropertyInfo(nameof(ComparisonOperator)); }
		}

		protected bool ComparisonOperator_ReadOnly
		{
			get { return ComparisonOperator_List.Count <= 1; }
		}

		public void RemoveComparisonOperatorsLeavingOne(ZString onlyOperator)
		{
			var codes = ComparisonConstants.GetAllComparisonOperators();
			if (codes.ContainsCode(onlyOperator))
			{
				ComparisonOperator_List.Clear();
				ComparisonOperator_List.Add(ComparisonConstants.GetComparisonOperatorPair(onlyOperator));
				ComparisonOperator = onlyOperator;
			}
		}

		virtual public CodeDescriptionPairList ComparisonOperator_List
		{
			get
			{
				if (fComparisonOperator_List == null)
				{
					fComparisonOperator_List = new CodeDescriptionPairList();
					foreach (CodeDescriptionPair pair in ComparisonConstants.GetAllComparisonOperators())
					{
						AddComparisonOperatorForBindingIfAllowed(pair);
					}
				}
				return fComparisonOperator_List;
			}
		}

		void AddComparisonOperatorForBindingIfAllowed(CodeDescriptionPair comparisonOperatorPair)
		{
			if (CheckIsAllowedComparisonOperator(comparisonOperatorPair.MultilingualCode.GetUnresolvedString()))
			{
				ComparisonOperator_List.Add(comparisonOperatorPair);
			}
		}

		protected virtual bool SupportsComparisonOperatorSet
		{
			get { return true; }
		}

		void EnsureSupportsComparisonOperatorSet()
		{
			if (!SupportsComparisonOperatorSet)
			{
				ErrorReporter.ReportOnce(GetType().Name + " - " + Description,
					"Cannot set SqlComparisonOperator on a module filter of type " + GetType().Name + ".\n" +
					"Filter Description: " + Description);
			}
		}

		public bool IsFilterCollectionComparisonOperatorSelected()
		{
			return IsFilterCollectionComparisonOperatorCore(ComparisonOperator);
		}

		protected virtual bool IsFilterCollectionComparisonOperatorCore(string comparisonOperator)
		{
			return comparisonOperator == ComparisonConstants.FiltersMatch || comparisonOperator == ComparisonConstants.AllMatch || comparisonOperator == ComparisonConstants.AnyMatch || comparisonOperator == ComparisonConstants.NoneMatch;
		}

		ZString fComparisonOperator;
		CodeDescriptionPairList fComparisonOperator_List;

		#endregion

		#region HasComparisonOperator

		public virtual bool HasComparisonOperator
		{
			get { return QueryDelegateType == null; }
		}

		#endregion

		#region Property

		[BusinessObjectTestExclude] // don't want the max length
		public virtual T Property
		{
			get
			{
				if (ComparisonOperator == ComparisonConstants.IsBlank || ComparisonOperator == ComparisonConstants.IsNotBlank)
				{
					return GetEmptyPropertyValue();
				}
				return propertyCore;
			}
			set
			{
				if (!propertyCore.Equals(value))
				{
					InvalidateCachedQuery();
				}

				propertyCore = value;
				HasChanges = true;
			}
		}
		T propertyCore;

		public ZPropertyInfo PropertyInfo
		{
			get { return GetZPropertyInfo(nameof(Property)); }
		}

		protected abstract T GetEmptyPropertyValue();

		protected virtual bool Property_ReadOnly => ShouldComparisonOperatorCauseReadOnly();

		protected bool ShouldComparisonOperatorCauseReadOnly()
		{
			return ComparisonOperator == ComparisonConstants.IsBlank || ComparisonOperator == ComparisonConstants.IsNotBlank || ComparisonOperator == ComparisonConstants.CurrentUser;
		}

		#endregion

		#region Query

		protected override ZQuery GetQuery()
		{
			if (SqlComparisonOperator == SpecialComparisonOperator.IsBlank && IsBlankFilterQueryDelegate != null)
			{
				return IsBlankFilterQueryDelegate.Invoke();
			}
			return base.GetQuery();
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var property = (object)Property;
			if (typeof(T) == typeof(ZString) && FilterColumn.MaxLength > 0)
			{
				property = ((ZString)property).Left(FilterColumn.MaxLength);
			}
			return new ZQuery(FilterColumn, SqlComparisonOperator, property, ComparisonOptions);
		}

		protected override bool IsSubGroupEnabled => IsBlankFilterQueryDelegate == null || SqlComparisonOperator != SpecialComparisonOperator.IsBlank;

		#endregion

		#region ISupportMultiValuesFilter

		bool ISupportMultiValuesFilter.CanGroup
		{
			get
			{
				return
					!IsCommonModuleFilter && !IsEmpty &&
					((!HasQueryDelegate && IsFilterColumnTableNameSameWithProvider) || HasMultiValueQueryDelegate) &&
					(
						OrCategory != FilterOrCategory.None && SqlComparisonOperator == SQLComparisonOperator.Equal ||
						OrCategory == FilterOrCategory.None && SqlComparisonOperator == SQLComparisonOperator.NotEqual
					);
			}
		}

		bool IsFilterColumnTableNameSameWithProvider
		{
			get
			{
				var provider = ActiveModuleFiltersProvider as FilterStripBusinessObject;
				if (provider != null && provider.ActiveStatusFilterColumn != null)
				{
					if (FilterColumn == null)
					{
						return false;
					}
					return provider.ActiveStatusFilterColumn.TableName == FilterColumn.TableName;
				}
				return true;
			}
		}

		string ISupportMultiValuesFilter.GroupKey
		{
			get { return ComparisonOperatorForGrouping; }
		}

		object ISupportMultiValuesFilter.GetCombinedValue(IEnumerable<ModuleFilter> filters)
		{
			var values = new List<T>();
			foreach (var filter in filters)
			{
				var typedFilter = filter as ModuleFilterWithListAndComparisonOperators<T>
					?? throw new InvalidOperationException("Cannot combine filters of incompatible types. Expected type: " + GetType().FullName + ", but was: " + filter.GetType().FullName);
				var value = typedFilter.Property;
				if (value.IsValid && !values.Contains(value))
				{
					values.Add(value);
				}
			}
			return values;
		}

		ZQuery ISupportMultiValuesFilter.GetCombinedQuery(object combinedValue)
		{
			if (SqlComparisonOperator != SQLComparisonOperator.Equal && SqlComparisonOperator != SQLComparisonOperator.NotEqual)
			{
				throw new InvalidOperationException("Can only combine filters with Equal or NotEqual operators, but was: " + SqlComparisonOperator);
			}

			if (HasQueryDelegate)
			{
				if (!HasMultiValueQueryDelegate)
				{
					throw new InvalidOperationException("MultiValueQueryDelegate should be set together with QueryDelegate for ISupportMultiValuesFilter to work.");
				}

				return MultiValueQueryDelegate(combinedValue, SqlComparisonOperator);
			}
			else
			{
				return new ZQuery(FilterColumn, SqlComparisonOperator, combinedValue, ComparisonOptions);
			}
		}

		#region Multi-value Query Delegate

		bool HasMultiValueQueryDelegate
		{
			get { return MultiValueQueryDelegate != null; }
		}

		[BusinessObjectTestExclude]
		public Func<object, SQLComparisonOperator, ZQuery> MultiValueQueryDelegate { get; set; }

		#endregion

		#endregion
	}

	#endregion

	#region class ModuleTextBaseFilter

	public abstract class ModuleTextBaseFilter : ModuleFilterWithSelectedFilters<ZString>
	{
		#region Construction

		protected ModuleTextBaseFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		protected ModuleTextBaseFilter(ZString description, SchemaStringColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		protected ModuleTextBaseFilter(ZString description, SchemaStringColumn filterColumn, ComparisonOptions options)
			: base(description, filterColumn, options)
		{
		}

		protected ModuleTextBaseFilter(ZString description, SchemaStringColumn filterColumn, IList list)
			: base(description, filterColumn, list)
		{
		}

		protected ModuleTextBaseFilter(ZString description, SchemaStringColumn filterColumn, GetList listDelegate)
			: base(description, filterColumn, listDelegate)
		{
		}

		protected ModuleTextBaseFilter(ZString description, Delegate queryDelegate)
			: base(description, queryDelegate)
		{
		}

		protected ModuleTextBaseFilter(ZString description, Delegate queryDelegate, IList list)
			: base(description, queryDelegate, list)
		{
		}

		protected ModuleTextBaseFilter(ZString description, Delegate queryDelegate, GetList listDelegate)
			: base(description, queryDelegate, listDelegate)
		{
		}

		protected ModuleTextBaseFilter(ZString description, Delegate queryDelegate, GetListUsingCurrentModuleFilter listUsingCurrentModuleFilterDelegate)
			: base(description, queryDelegate, listUsingCurrentModuleFilterDelegate)
		{
		}

		protected ModuleTextBaseFilter(ZString description, ModuleIdentifier id, SchemaStringColumn nkFilterColumn, IBusinessObjectCollection list)
			: base(description, id, nkFilterColumn, list)
		{
		}

		protected ModuleTextBaseFilter(ZString description, ModuleIdentifier id, SchemaStringColumn nkFilterColumn, GetList listDelegate)
			: base(description, id, nkFilterColumn, listDelegate)
		{
		}

		protected ModuleTextBaseFilter(ZString description, ModuleIdentifier id, GetNkQuery queryDelegate, IBusinessObjectCollection list)
			: base(description, id, queryDelegate, list)
		{
		}

		protected ModuleTextBaseFilter(ZString description, ModuleIdentifier id, GetNkQueryWithOperator queryDelegate, IBusinessObjectCollection list)
			: base(description, id, queryDelegate, list)
		{
		}

		protected ModuleTextBaseFilter(ZString description, ModuleIdentifier id, GetNkQueryWithOperatorSupportsFiltersMatch queryDelegate, IBusinessObjectCollection list)
			: base(description, id, queryDelegate, list)
		{
		}

		protected ModuleTextBaseFilter(ZString description, ModuleIdentifier id, Delegate queryDelegate, GetListUsingCurrentModuleFilter listUsingCurrentModuleFilterDelegate)
			: base(description, id, queryDelegate, listUsingCurrentModuleFilterDelegate)
		{
		}

		protected ModuleTextBaseFilter(ZString description, ModuleIdentifier id, GetTextAndNkQuery queryDelegate, IBusinessObjectCollection list)
			: base(description, id, queryDelegate, list)
		{
		}

		#endregion

		#region Copy Properties

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			base.CopyPersistantValuesFromFilter(filterToCopyFrom);

			var filter = (ModuleTextBaseFilter)filterToCopyFrom;

			if (SupportsComparisonOperatorSet)
			{
				ComparisonOperator = filter.ComparisonOperator;
			}

			Property = filter.Property;
		}

		public override void CopyTransientProperties(BusinessObject copy)
		{
			base.CopyTransientProperties(copy);

			var filter = copy as ModuleTextBaseFilter;
			if (filter != null)
			{
				Prefix = filter.Prefix;
			}
		}

#if DEBUG
		public void CopyPersistantValuesFromFilterForTest(ModuleFilter filterToCopyFrom)
		{
			CopyPersistantValuesFromFilter(filterToCopyFrom);
		}
#endif

		#endregion

		#region Comparison Operator

		public override bool HasComparisonOperator
		{
			get { return base.HasComparisonOperator || QueryDelegateType == typeof(GetTextQueryWithOperator); }
		}

		protected override bool UsesStandardComparisonOperators => true;

		#endregion

		#region IsExpensiveQuery

		public override bool IsExpensiveQuery
		{
			get { return !Property.IsEmpty && (SqlComparisonOperator == SQLComparisonOperator.Contains || SqlComparisonOperator == SQLComparisonOperator.NotContains); }
		}

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		#endregion

		#region Clear / IsEmpty / Default

		protected override void ClearCore()
		{
			base.ClearCore();

			Property = DefaultProperty;
		}

		protected override bool IsEmptyCore => Property.IsEmpty;

		public ZString DefaultProperty
		{
			get { return fDefaultProperty; }
			set
			{
				fDefaultProperty = value;
				Property = value;
			}
		}

		ZString fDefaultProperty;

		#endregion

		#region Property

		[BusinessObjectTestExclude] // don't want the max length
		public override ZString Property
		{
			get
			{
				return base.Property;
			}
			set
			{
				if (base.Property != value)
				{
					base.Property = value.TrimEnd(' ');
					if (!IsValidationSuspended)
					{
						Validation.ValidateProperty();
					}
					PropertyInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		protected override ZString GetEmptyPropertyValue()
		{
			return ZString.Empty;
		}

		protected virtual int Property_MaxLength
		{
			get
			{
				int retVal;
				var stringColumn = FilterColumn;

				if (stringColumn == null)
				{
					retVal = maxLength;
				}
				else if (stringColumn is SchemaStringColumn)
				{
					retVal = stringColumn.MaxLength;
				}
				else
				{
					retVal = 0;
				}

				return retVal;
			}
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			Property = RandomString(MaxLength);
		}

#endif
		#endregion

		#region PropertyValidation

		public Validation PropertyValidation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return propertyValidation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { propertyValidation = value; }
		}
		Validation propertyValidation;

		#endregion

		#region Validation

		public new ModuleTextFilterValidation Validation
		{
			get { return (ModuleTextFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleTextFilterValidation(this);
		}

		public bool ErrorOnCodeNotPresent { get; set; }

		#endregion

		#region Query

		protected override object[] QueryDelegateParameters
		{
			get
			{
				if (QueryDelegateType == typeof(GetTextQueryWithOperator))
				{
					return new object[] { SqlComparisonOperator, Property };
				}
				else
				{
					return new object[] { Property };
				}
			}
		}

		#endregion

		#region Template Filter Query

		protected override ZQuery XQueryCore => SqlComparisonOperator != SQLComparisonOperator.NotSpecified && XQueryInfo != null && (!Property.IsEmpty || IsBlankOrNotBlankFilterWithoutQueryDelegate)
			? XQueryFilterHelper.GenerateXQuery((column) => new ZQuery(column, SqlComparisonOperator, Property), XQueryInfo) : base.XQueryCore;

		#endregion

		#region Serialization

		protected override ZString GetPropertyValueFromDeserializedString(string deserializedValue)
		{
			return deserializedValue;
		}

		#endregion
	}

	#region class ModuleTextFilterValidation

	public class ModuleTextFilterValidation : ModuleFilterWithSelectedFilters<ZString>.ModuleFilterWithSelectedFiltersValidation
	{
		public ModuleTextFilterValidation(ModuleTextBaseFilter parent)
			: base(parent)
		{
		}

		#region ValidateProperty

		public void ValidateProperty()
		{
			ValidateCalculatedProperty(Parent.PropertyInfo);
		}

		protected virtual void CheckProperty()
		{
			var parent = GetParent();
			var codeDescPairList = Parent.List as ICodeDescriptionPairList;
			if (parent.UseMultiSearch && parent.Property.Contains(',') && !(
				parent.SqlComparisonOperator == SQLComparisonOperator.Equal
				|| parent.SqlComparisonOperator == SQLComparisonOperator.NotEqual
				|| parent.SqlComparisonOperator == SQLComparisonOperator.StartsWith
				|| parent.SqlComparisonOperator == SQLComparisonOperator.EndsWith
				|| parent.SqlComparisonOperator == SQLComparisonOperator.Contains
				))
			{
				Parent.PropertyInfo.AddWarning(Res.GetString("f15fd8a2-d089-46e6-aa66-1f816e74458f", "Multiple values cannot be used in conjunction with 'not start' or 'not contains'."));
			}

			if (codeDescPairList != null)
			{
				if (parent.ErrorOnCodeNotPresent)
				{
					ListValidation.ErrorIfInvalidCode(Parent.PropertyInfo, codeDescPairList);
				}
				else
				{
					ListValidation.WarnIfInvalidCode(Parent.PropertyInfo, codeDescPairList);
				}
			}
			else
			{
				var collection = Parent.List as IBusinessObjectCollection;

				if (collection != null)
				{
					ListValidation.ErrorIfInvalidCode(Parent.PropertyInfo, collection);
				}
			}

			parent.PropertyValidation?.Invoke(Parent.PropertyInfo);
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateProperty();
		}

		public override Type AutoValidationType => GetType();

		protected ModuleTextBaseFilter GetParent() => (ModuleTextBaseFilter)Parent;
	}

	#endregion

	#endregion

	#region class ModuleTextRangeFilter

	public delegate ZQuery GetTextRangeQuery(ZString value1, ZString value2);

	public class ModuleTextRangeFilter : ModuleFilter
	{
		#region Construction

		protected ModuleTextRangeFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ModuleTextRangeFilter(ZString description, SchemaStringColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		public ModuleTextRangeFilter(ZString description, GetTextRangeQuery queryDelegate)
			: base(description, queryDelegate)
		{
		}

		#endregion

		#region GetNewCommonModuleFilter, CopyPropertiesToFilter

		protected internal override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new ModuleTextRangeFilter(category, parentCollection);
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (ModuleTextRangeFilter)filterToCopyFrom;
			Property1 = filter.Property1;
			Property2 = filter.Property2;
		}

		#endregion

		#region IsExpensiveQuery

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		#endregion

		#region Clear / IsEmpty / Default

		protected override void ClearCore()
		{
			Property1 = DefaultProperty1;
			Property2 = DefaultProperty2;
		}

		protected override bool IsEmptyCore => Property1 == ZString.Empty && Property2 == ZString.Empty;

		public ZString DefaultProperty1
		{
			get { return fDefaultProperty1; }
			set
			{
				fDefaultProperty1 = value;
				Property1 = value;
				InvalidateCachedQuery();
			}
		}

		public ZString DefaultProperty2
		{
			get { return fDefaultProperty2; }
			set
			{
				fDefaultProperty2 = value;
				Property2 = value;
				InvalidateCachedQuery();
			}
		}

		ZString fDefaultProperty1;
		ZString fDefaultProperty2;

		#endregion

		#region Property1

		[BusinessObjectTestExclude] // don't want the max length
		public ZString Property1
		{
			get { return fProperty1; }
			set
			{
				if (Property1 != value)
				{
					InvalidateCachedQuery();
				}

				SetNonPersistentPropertyValue(Property1Info, ref fProperty1, value.TrimEnd(' '));
				if (!IsValidationSuspended)
				{
					Validation.ValidateProperty1();
				}
				Property1Info.RefreshBinding();
			}
		}

		public ZPropertyInfo Property1Info
		{
			get { return GetZPropertyInfo(nameof(Property1)); }
		}

		ZString fProperty1;

		#endregion

		#region Property1Validation

		public Validation Property1Validation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return property1Validation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { property1Validation = value; }
		}
		Validation property1Validation;

		#endregion

		#region Property2

		[BusinessObjectTestExclude] // don't want the max length
		public ZString Property2
		{
			get { return fProperty2; }
			set
			{
				if (Property2 != value)
				{
					InvalidateCachedQuery();
				}

				SetNonPersistentPropertyValue(Property2Info, ref fProperty2, value.TrimEnd(' '));
				if (!IsValidationSuspended)
				{
					Validation.ValidateProperty2();
				}
				Property2Info.RefreshBinding();
			}
		}

		public ZPropertyInfo Property2Info
		{
			get { return GetZPropertyInfo(nameof(Property2)); }
		}

		ZString fProperty2;

		#endregion

		#region Property2Validation

		public Validation Property2Validation
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return property2Validation; }
			[System.Diagnostics.DebuggerStepThrough]
			set { property2Validation = value; }
		}
		Validation property2Validation;

		#endregion

		#region Validation

		public new ModuleTextRangeFilterValidation Validation
		{
			get { return (ModuleTextRangeFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleTextRangeFilterValidation(this);
		}

		#endregion

		#region Query

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { Property1, Property2 }; }
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var result = new ZQuery();
			if (Property1 != ZString.Empty)
			{
				result.AddToFilter(FilterColumn, SQLComparisonOperator.GreaterThanOrEqualTo, Property1, ComparisonOptions);
			}

			if (Property2 != ZString.Empty)
			{
				result.AddToFilter(FilterColumn, SQLComparisonOperator.LessThanOrEqualTo, Property2, ComparisonOptions);
			}

			return result;
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			Property1 = RandomString(MaxLength);
		}

#endif
		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString("Property1", Property1);
			writer.WriteElementString("Property2", Property2);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == "Property1")
			{
				Property1 = reader.ReadElementString("Property1");
			}

			if (reader.Name == "Property2")
			{
				Property2 = reader.ReadElementString("Property2");
			}
		}

		#endregion
	}

	#region class ModuleTextRangeFilterValidation

	public class ModuleTextRangeFilterValidation : ModuleFilterValidation
	{
		public ModuleTextRangeFilterValidation(ModuleTextRangeFilter parent)
			: base(parent)
		{
			Parent = parent;
		}

		#region ValidateProperty1

		public void ValidateProperty1()
		{
			ValidateCalculatedProperty(Parent.Property1Info);
			ValidateCalculatedProperty(Parent.Property2Info);
		}

		protected virtual void CheckProperty1()
		{
			if (Parent.Property1.CompareTo(Parent.Property2) > 0 && !Parent.Property2.IsEmpty)
			{
				Parent.Property1Info.AddError(Res.GetString("1ce3778e-229a-4e1a-ad05-35bb789b125f", "The 'From' value is greater than the 'To' value."));
			}

			if (Parent.Property1Validation != null)
			{
				Parent.Property1Validation(Parent.Property1Info);
			}
		}

		#endregion

		#region ValidateProperty2

		public void ValidateProperty2()
		{
			ValidateCalculatedProperty(Parent.Property1Info);
			ValidateCalculatedProperty(Parent.Property2Info);
		}

		protected virtual void CheckProperty2()
		{
			if (Parent.Property2.CompareTo(Parent.Property1) < 0 && !Parent.Property2.IsEmpty)
			{
				Parent.Property2Info.AddError(Res.GetString("b728dcbb-3085-474d-ba3b-789db639fd69", "The 'To' value is less than the 'From' value."));
			}

			if (Parent.Property2Validation != null)
			{
				Parent.Property2Validation(Parent.Property2Info);
			}
		}

		#endregion

		public override void ValidateAll()
		{
			ValidateProperty1();
			ValidateProperty2();
		}

		public override Type AutoValidationType
		{
			get { return GetType(); }
		}

		protected readonly ModuleTextRangeFilter Parent;
	}

	#endregion

	#endregion

	#region class ModuleTextFilterForMultipleColumns

	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ModuleTextFilterForMultipleColumns : ModuleTextFilter
	{
		#region Constructor

		public ModuleTextFilterForMultipleColumns(ZString description, SchemaStringColumn filterColumn, params SchemaStringColumn[] additionalColumns)
			: base(description, filterColumn)
		{
			this.additionalColumns = additionalColumns;
		}

		#endregion

		#region MaxLengthProperty

		public override int MaxLength
		{
			get
			{
				return FindMaxLength();
			}
		}

		protected override int Property_MaxLength => FindMaxLength();

		int FindMaxLength()
		{
			var maxLength = FilterColumn.MaxLength;
			foreach (var additionalColumn in additionalColumns)
			{
				var currentMaxLength = additionalColumn.MaxLength;
				if (currentMaxLength > maxLength)
				{
					maxLength = currentMaxLength;
				}
			}
			return maxLength < MaxMaximumLength ? maxLength : MaxMaximumLength;
		}

		#endregion

		#region Query

		[SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var query = new ZQuery();
			if (Property.Length <= FilterColumn.MaxLength || !FilterColumn.HasMaxLength)
			{
				query = base.GetQueryUsingFilterColumns();
			}
			foreach (var additionalColumn in additionalColumns) // Different columns are used here
			{
				var sqlComparisonOperator = ShouldContainsOrStartsWithOrEndsWithBeConsideredExact(additionalColumn)
					? SQLComparisonOperator.Equal
					: GetSqlComparisonOperator(ComparisonOperator, DefaultSqlComparisonOperator);
				if (Property.Length <= additionalColumn.MaxLength || !additionalColumn.HasMaxLength)
				{
					query.AddToFilter(JoinCondition.Or, additionalColumn, sqlComparisonOperator, Property, ComparisonOptions);
				}
			}
			return query;
		}

		#endregion

		#region SetValueFromInitialCode

		public override bool SetValueFromInitialCode(ZString initialProperty, ZString initialCode)
		{
			var result = base.SetValueFromInitialCode(initialProperty, initialCode);

			if (!result)
			{
				var additionalColumn = additionalColumns.FirstOrDefault(x => x.Name == initialProperty);
				if (additionalColumn != null)
				{
					Property = initialCode;
					Visibility = FilterVisibility.AlwaysVisible;
					result = true;
				}
			}

			return result;
		}

		public override bool ShouldSetValueFromInitialCode(ZString initialProperty, ZString initialCode)
		{
			return base.ShouldSetValueFromInitialCode(initialProperty, initialCode) || ShouldSetValueFromInitialCode_AdditionalColumns(initialProperty);
		}

		public bool ShouldSetValueFromInitialCode_AdditionalColumns(ZString initialProperty)
		{
			return additionalColumns.Any(x => x.Name == initialProperty);
		}
		#endregion

		public int AdditionalColumnsLength => additionalColumns?.Length ?? 0;

		#region Implementation

		readonly SchemaStringColumn[] additionalColumns;

		#endregion
	}

	#endregion
}
