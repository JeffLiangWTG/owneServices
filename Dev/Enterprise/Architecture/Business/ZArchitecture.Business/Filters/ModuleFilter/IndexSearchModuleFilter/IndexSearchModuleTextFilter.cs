using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using GlowIndexQueryService.Business;

namespace Enterprise.ZArchitecture.Business
{
	public class IndexSearchModuleTextFilter : ModuleTextFilter, IIndexSearchModuleFilter
	{
		public IndexSearchModuleTextFilter(SearchField searchField, string description = null, FilterCategory filterCategory = null) : base(searchField.FieldName, (o, s) => new ZQuery())
		{
			InitializeFilter(searchField, filterCategory, description);
		}

		public IndexSearchModuleTextFilter(string description, SearchField searchField, Func<SearchField, ZString, IGlowQuery> getGlowIndexQueryDelegate, IList list, FilterCategory filterCategory = null) : base(searchField.FieldName, () => new ZQuery(), list)
		{
			InitializeFilter(searchField, filterCategory, description);
			this.getGlowIndexQueryDelegate = getGlowIndexQueryDelegate;
		}

		public IndexSearchModuleTextFilter(SearchField searchField, IList list, FilterCategory filterCategory = null) : base(searchField.FieldName, () => new ZQuery(), list)
		{
			InitializeFilter(searchField, filterCategory);
		}

		public IndexSearchModuleTextFilter(SearchField searchField, GetList listDelegate, FilterCategory filterCategory = null) : base(searchField.FieldName, (p) => new ZQuery(), listDelegate)
		{
			InitializeFilter(searchField, filterCategory);
		}

		void InitializeFilter(SearchField searchField, FilterCategory filterCategory = null, string description = null)
		{
			((IModuleFilterForStrategyInternal)this).Description = description ?? searchField.FieldName;
			MultilingualDescription = (NoResString)searchField.Description;
			SearchField = searchField ?? throw new ArgumentNullException(nameof(searchField));
			if (filterCategory != null)
			{
				Category = filterCategory;
			}
		}

		SearchField SearchField { get; set; }

		public string SearchFieldName => SearchField.FieldName;

		public override bool HasComparisonOperator => getGlowIndexQueryDelegate == null;

		public override CodeDescriptionPairList ComparisonOperator_List => fComparisonOperator_List ??= IndexSearchTextFilterConstants.GetAllComparisonOperators();
		CodeDescriptionPairList fComparisonOperator_List;

		public override IReadOnlyList<string> AllowedComparisonOperators => allowedComparisonOperators ??= ComparisonOperator_List.Cast<ICodeDescription>().Select(op => op.Code).ToArray();
		string[] allowedComparisonOperators;
		readonly Func<SearchField, ZString, IGlowQuery> getGlowIndexQueryDelegate;

		public virtual IGlowQuery GetGlowIndexQuery()
		{
			if (getGlowIndexQueryDelegate != null)
			{
				return getGlowIndexQueryDelegate(SearchField, Property);
			}

			if (ComparisonOperator != IndexSearchTextFilterConstants.IsBlank
				&& ComparisonOperator != IndexSearchTextFilterConstants.NotBlank
				&& string.IsNullOrEmpty(Property))
			{
				return new EmptyQuery();
			}
			var term = new Term(SearchField.FieldName, Property);
			return (string)ComparisonOperator switch
			{
				IndexSearchTextFilterConstants.AnyStartsWith => new PrefixQuery(term),
				IndexSearchTextFilterConstants.AnyExact => new EqualQuery(term),
				IndexSearchTextFilterConstants.AllExact => new EqualQuery(term, useQuotes: true, allExact: true),
				IndexSearchTextFilterConstants.NoneExact => new NotEqualQuery(term),
				IndexSearchTextFilterConstants.NoStartWith => new NotQuery(new PrefixQuery(term)),
				IndexSearchTextFilterConstants.IsBlank => new IsBlankQuery(term),
				IndexSearchTextFilterConstants.NotBlank => new IsNotBlankQuery(term),
				_ => new EmptyQuery(),
			};
		}

		public override bool SetValueFromInitialCode(ZString initialProperty, ZString initialCode)
		{
			Property = initialCode;
			Visibility = FilterVisibility.AlwaysVisible;
			return true;
		}

		public override bool ShouldSetValueFromInitialCode(ZString initialProperty, ZString initialCode)
		{
			return SearchField.FieldName == IndexSearchFilterHelper.CommonField;
		}

		public static class IndexSearchTextFilterConstants
		{
#pragma warning disable CW1161 // Res.GetString Analyzer
			public const string AnyStartsWith = "any starts with";
			public const string AnyExact = "any exact";
			public const string AllExact = "all exact";
			public const string NoneExact = "none exact";
			public const string NoStartWith = "no starts with";
			public const string IsBlank = "is blank";
			public const string NotBlank = "is not blank";
#pragma warning restore CW1161 // Res.GetString Analyzer

			public static CodeDescriptionPair GetComparisonOperatorPair(string code)
			{
				return code switch
				{
					AnyStartsWith => new CodeDescriptionPair(AnyStartsWith, ResString.GetMultilingualString("IndexSearchTextFilter|ComparisonConstraintDescription|AnyStartsWith", "has ANY word STARTING with")),
					AnyExact => new CodeDescriptionPair(AnyExact, ResString.GetMultilingualString("IndexSearchTextFilter|ComparisonConstraintDescription|AnyExact", "has ANY of these EXACT words")),
					AllExact => new CodeDescriptionPair(AllExact, ResString.GetMultilingualString("IndexSearchTextFilter|ComparisonConstraintDescription|AllExact", "has ALL of these EXACT words")),
					NoneExact => new CodeDescriptionPair(NoneExact, ResString.GetMultilingualString("IndexSearchTextFilter|ComparisonConstraintDescription|NoneExact", "has NONE of these EXACT words")),
					NoStartWith => new CodeDescriptionPair(NoStartWith, ResString.GetMultilingualString("IndexSearchTextFilter|ComparisonConstraintDescription|NoStartWith", "has NO word STARTING with")),
					IsBlank => new CodeDescriptionPair(IsBlank, ResString.GetMultilingualString("IndexSearchTextFilter|ComparisonConstraintDescription|IsBlank", "is blank")),
					NotBlank => new CodeDescriptionPair(NotBlank, ResString.GetMultilingualString("IndexSearchTextFilter|ComparisonConstraintDescription|NotBlank", "is not blank")),
					_ => new CodeDescriptionPair(AnyStartsWith, ResString.GetMultilingualString("IndexSearchTextFilter|ComparisonConstraintDescription|AnyStartsWith", "has ANY word STARTING with"))
				};
			}

			public static CodeDescriptionPairList GetAllComparisonOperators()
			{
				var list = new CodeDescriptionPairList();
				list.Add(GetComparisonOperatorPair(AnyStartsWith));
				list.Add(GetComparisonOperatorPair(AnyExact));
				list.Add(GetComparisonOperatorPair(AllExact));
				list.Add(GetComparisonOperatorPair(NoneExact));
				list.Add(GetComparisonOperatorPair(NoStartWith));
				list.Add(GetComparisonOperatorPair(IsBlank));
				list.Add(GetComparisonOperatorPair(NotBlank));
				return list;
			}
		}
	}
}
