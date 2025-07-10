using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.ZArchitecture.Business.Res;

namespace Enterprise.ZArchitecture.GUI
{
	public abstract class ModuleFilterWithSelectedFilters<T> : ModuleFilterWithListAndComparisonOperators<T>, IModuleFilterWithSelectedFilters
		where T : IZType
	{
		#region Constructors

		protected ModuleFilterWithSelectedFilters(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		protected ModuleFilterWithSelectedFilters(ZString description, SchemaStringColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected ModuleFilterWithSelectedFilters(ZString description, ModuleIdentifier id, SchemaStringColumn filterColumn, ComparisonOptions options)
			: base(description, filterColumn, options)
		{
			SupportsFiltersMatchComparisonOperator = true;
			moduleId = id;
			SetCategoryForInitialModuleId(id);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected ModuleFilterWithSelectedFilters(ZString description, ModuleIdentifier id, SchemaStringColumn filterColumn, IList list)
			: base(description, filterColumn, list)
		{
			SupportsFiltersMatchComparisonOperator = true;
			moduleId = id;
			SetCategoryForInitialModuleId(id);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected ModuleFilterWithSelectedFilters(ZString description, ModuleIdentifier id, SchemaStringColumn filterColumn, GetList listDelegate)
			: base(description, filterColumn, listDelegate)
		{
			SupportsFiltersMatchComparisonOperator = true;
			moduleId = id;
			SetCategoryForInitialModuleId(id);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected ModuleFilterWithSelectedFilters(ZString description, ModuleIdentifier id, Delegate queryDelegate)
			: base(description, queryDelegate)
		{
			moduleId = id;
			SetCategoryForInitialModuleId(id);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected ModuleFilterWithSelectedFilters(ZString description, ModuleIdentifier id, Delegate queryDelegate, IList list)
			: base(description, queryDelegate, list)
		{
			moduleId = id;
			SetCategoryForInitialModuleId(id);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected ModuleFilterWithSelectedFilters(ZString description, ModuleIdentifier id, Delegate queryDelegate, GetList listDelegate)
			: base(description, queryDelegate, listDelegate)
		{
			moduleId = id;
			SetCategoryForInitialModuleId(id);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected ModuleFilterWithSelectedFilters(ZString description, ModuleIdentifier id, SchemaColumn filterColumn, GetListUsingCurrentModuleFilter listUsingCurrentModuleFilterDelegate)
			: base(description, filterColumn, listUsingCurrentModuleFilterDelegate)
		{
			SupportsFiltersMatchComparisonOperator = true;
			moduleId = id;
			SetCategoryForInitialModuleId(id);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected ModuleFilterWithSelectedFilters(ZString description, ModuleIdentifier id, Delegate queryDelegate, GetListUsingCurrentModuleFilter listUsingCurrentModuleFilterDelegate)
			: base(description, queryDelegate, listUsingCurrentModuleFilterDelegate)
		{
			moduleId = id;
			SetCategoryForInitialModuleId(id);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected ModuleFilterWithSelectedFilters(ZString description, ModuleIdentifier id, SchemaGuidColumn filterColumn, IBusinessObjectCollection list)
			: base(description, filterColumn, list)
		{
			SupportsFiltersMatchComparisonOperator = true;
			moduleId = id;
			SetCategoryForInitialModuleId(id);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected ModuleFilterWithSelectedFilters(ZString description, ModuleIdentifier id, SchemaGuidColumn filterColumn, GetList listDelegate)
			: base(description, filterColumn, listDelegate)
		{
			SupportsFiltersMatchComparisonOperator = true;
			moduleId = id;
			SetCategoryForInitialModuleId(id);
		}

		protected ModuleFilterWithSelectedFilters(ZString description, SchemaStringColumn filterColumn, ComparisonOptions options)
			: base(description, filterColumn, options)
		{
		}

		protected ModuleFilterWithSelectedFilters(ZString description, SchemaStringColumn filterColumn, IList list)
			: base(description, filterColumn, list)
		{
		}

		protected ModuleFilterWithSelectedFilters(ZString description, SchemaStringColumn filterColumn, GetList listDelegate)
			: base(description, filterColumn, listDelegate)
		{
		}

		protected ModuleFilterWithSelectedFilters(ZString description, Delegate queryDelegate)
			: base(description, queryDelegate)
		{
		}

		protected ModuleFilterWithSelectedFilters(ZString description, Delegate queryDelegate, IList list)
			: base(description, queryDelegate, list)
		{
		}

		protected ModuleFilterWithSelectedFilters(ZString description, Delegate queryDelegate, GetList listDelegate)
			: base(description, queryDelegate, listDelegate)
		{
		}

		protected ModuleFilterWithSelectedFilters(ZString description, Delegate queryDelegate, GetListUsingCurrentModuleFilter listUsingCurrentModuleFilterDelegate)
			: base(description, queryDelegate, listUsingCurrentModuleFilterDelegate)
		{
		}

		#endregion

		#region ModuleFilter Overrides

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var filter = (ModuleFilterWithSelectedFilters<T>)filterToCopyFrom;
			UpdateSelectedFilters(filter.SelectedFilters);
		}

		protected override void ClearCore()
		{
			using (GetValidationSuspender())
			{
				ClearSelectedFilters();
			}
		}

		protected void ClearSelectedFilters()
		{
			selectedFilters = null;
			OnSelectedFiltersChanged();
		}

		protected sealed override bool IsEmptyForSelectedFilters => !HasComparisonOperator || !IsFilterCollectionComparisonOperatorSelected();

		public override IReadOnlyList<string> AllowedComparisonOperators
		{
			get
			{
				var operators = new List<string>();

				if (UsesStandardComparisonOperators)
				{
					operators.AddRange(base.AllowedComparisonOperators);
				}
				else
				{
					operators.Add(string.Empty);
					operators.Add(ComparisonConstants.Exact);
					operators.Add(ComparisonConstants.NotEqual);

					if (SupportsBlankComparisonOperators)
					{
						operators.Add(ComparisonConstants.IsBlank);
						operators.Add(ComparisonConstants.IsNotBlank);
					}
				}

				var additionalOperators = GetAdditionalAllowedComparisonOperators();

				if (additionalOperators != null)
				{
					operators.AddRange(additionalOperators);
				}

				if (SupportsFiltersMatchComparisonOperator)
				{
					operators.Add(ComparisonConstants.FiltersMatch);
				}

				return operators;
			}
		}

		public bool IsComparisonOperatorAutomaticallySelected => IsComparisonOperatorAutomaticallySelectedCore();

		protected virtual bool IsComparisonOperatorAutomaticallySelectedCore() => false;

		protected virtual bool UsesStandardComparisonOperators => false;

		protected virtual string[] GetAdditionalAllowedComparisonOperators()
		{
			return Array.Empty<string>();
		}

		protected override void OnComparisonOperatorChanged()
		{
			base.OnComparisonOperatorChanged();

			if (IsFilterCollectionComparisonOperatorSelected())
			{
				OnSelectedFiltersChanged();
			}
		}

		#endregion

		#region SelectedFilters

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Need to check these two specific one")]
		public virtual void UpdateSelectedFilters(FilterStripBusinessObject newSelectedFilters)
		{
			if (newSelectedFilters == null)
			{
				return;
			}

			var layout = new ReadOnlyBusinessObjectFactory { NameForDebugging = "ReplaceStripsWithOthers" }.New<StmModuleFilter>();
			((IModifyModuleAndGridLayout)newSelectedFilters).SerialiseLayoutAndWriteTo(layout);

			selectedFilters = GetNewSelectedFilters(layoutToLoad: layout);

			if (SelectedFilters != null && SelectedFilters.AlwaysVisibleModuleFilters.Count == 2)
			{
				foreach (var filter in SelectedFilters)
				{
					if (filter.Description == "Code" || filter.Description == "Description")
					{
						filter.IsActive = true;
					}
				}
			}
			SelectedFiltersDescriptionInfo.RefreshBinding();
		}

		public int SelectedFilterCount => SelectedFilters?.ActiveModuleFilters.Count ?? 0;

		public FilterStripBusinessObject SelectedFilters => selectedFilters ?? (selectedFilters = GetNewSelectedFilters(layoutToLoad: null));

		FilterStripBusinessObject selectedFilters;

		readonly Dictionary<SelectedFiltersCacheKey, Tuple<FilterStripBusinessObject, ZQuery>> localSelectedFiltersCache = new Dictionary<SelectedFiltersCacheKey, Tuple<FilterStripBusinessObject, ZQuery>>();

		protected virtual FilterStripBusinessObject GetNewSelectedFilters(StmModuleFilter layoutToLoad)
		{
			var blobKey1 = ZBlob.Empty;
			var blobKey2 = ZBlob.Empty;
			if (layoutToLoad != null)
			{
				var values = layoutToLoad.GetOrCreateLayoutUserData(selectedFilters?.LayoutsHelper ?? new FilterStripLayoutsHelper());
				blobKey1 = layoutToLoad.S9_FilterData;
				blobKey2 = values.S0_FilterDataValues;
			}
			var key = new SelectedFiltersCacheKey(blobKey1, blobKey2, ModuleId, FilterBusinessObject?.IsInFilterRuleMode ?? false);

			if (Globals.IsWebServiceOrWeb || Thread.CurrentThread.IsThreadPoolThread)
			{
				if (localSelectedFiltersCache.TryGetValue(key, out var localResult) && localResult != null)
				{
					if (!localResult.Item1.AddFilterStripUsed)
					{
						this.additionalDisplayFilters = localResult.Item2;
						return localResult.Item1;
					}
				}
			}
			else if (!IsDuplicateDefault && SelectedFiltersCache.TryGetValue(key, out var result) && result != null)
			{
				if (!result.Item1.AddFilterStripUsed)
				{
					this.additionalDisplayFilters = result.Item2;
					return result.Item1;
				}
			}

			using (var module = ObjectFactory.Get<IZFilterModuleHelper>().GetZFilterModule(ModuleId))
			{
				if (module != null)
				{
					module.OverrideModuleDecisionProvider(ObjectFactory.Get<ISQLFilterOnlyModuleDecisionProvider>());
					module.DoNotCheckOrSaveChanges = true;

					module.ShouldLoadFilterBizOIndexSearchFilter = layoutToLoad?.S9_IsIndexSearch ?? FilterBusinessObject?.SearchType == SearchType.Index;

					var filterBusinessObject = module.FilterBusinessObject;

					if (FilterBusinessObject != null)
					{
						filterBusinessObject.IsInFilterRuleMode = FilterBusinessObject.IsInFilterRuleMode;
					}

					if (layoutToLoad != null)
					{
						filterBusinessObject.LoadLayout(layoutToLoad);
					}
					else
					{
						filterBusinessObject.AddAlwaysVisibleFilters();
					}

					try
					{
						additionalDisplayFilters = module.GridCollection.CompleteFilter.DeepClone();
						module.AddAdditionalDisplayFilter?.Invoke(additionalDisplayFilters);
						additionalDisplayFilters.IsNoResultQuery = false;
					}
					catch (ModuleGuiNotSupportedException)
					{
						// No GridCollection filters to add, which is fine.
					}

					filterBusinessObject.AddFilterStripUsed = false;

					if (Globals.IsWebServiceOrWeb || Thread.CurrentThread.IsThreadPoolThread)
					{
						localSelectedFiltersCache.Add(key, new Tuple<FilterStripBusinessObject, ZQuery>(filterBusinessObject, additionalDisplayFilters));
					}
					else
					{
						SelectedFiltersCache.SetValue(key, new Tuple<FilterStripBusinessObject, ZQuery>(filterBusinessObject, additionalDisplayFilters));
					}

					return filterBusinessObject;
				}
			}

			return null;
		}

		public ZString SelectedFiltersDescription => SelectedFilterCount == 1
			? Res.GetString("5e32ef3b-d29f-4b0d-9776-08151b195b14", "1 filter applied")
			: Res.GetString("c649d2b0-8de8-48df-a211-69c06d57ebe1", "{0} filters applied", SelectedFilterCount);

		public ZPropertyInfo SelectedFiltersDescriptionInfo => GetZPropertyInfo(nameof(SelectedFiltersDescription));

		void OnSelectedFiltersChanged()
		{
			if (SelectedFiltersChanged != null && selectedFiltersChangedFiringSuspendedCount <= 0)
			{
				SelectedFiltersChanged(this, EventArgs.Empty);
			}

			Validation.ValidateSelectedFiltersDescription();
			InvalidateCachedQuery();
		}

		public event EventHandler SelectedFiltersChanged;

		public IDisposable SuspendSelectedFiltersChangedFiring()
		{
			selectedFiltersChangedFiringSuspendedCount++;
			return new DisposableAction(() => selectedFiltersChangedFiringSuspendedCount--);
		}

		int selectedFiltersChangedFiringSuspendedCount;

		protected virtual bool SelectedFilters_ReadOnly => false;

		protected void ReloadSelectedFilters(StmModuleFilter layout)
		{
			UpdateSelectedFilters(GetNewSelectedFilters(layout));
		}

		#endregion

		#region Query

		protected override bool ShouldReevaluateQuery()
		{
			if (base.ShouldReevaluateQuery())
			{
				return true;
			}

			if (IsFilterCollectionComparisonOperatorSelected())
			{
				return CachedQuery == null || SelectedFilters == null || SelectedFilters.IsFilterQueryStale || SelectedFilters.Any(x => x.CachedQueryCreationTimeUtc > CachedQueryCreationTimeUtc);
			}

			return false;
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			if (IsFilterCollectionComparisonOperatorSelected() && FilterColumn != null)
			{
				return GetQueryForSelectedFilters();
			}

			return base.GetQueryUsingFilterColumns();
		}

		protected virtual ZQuery GetQueryForSelectedFilters()
		{
			ZQuery result = null;

			if (ShouldGetQueryForSelectedFiltersFromLayout && !Equals(ModuleId, ModuleIDs.NotAssigned))
			{
				result = GetQueryForSelectedFiltersFromLayout();
			}

			return result ?? new ZQuery();
		}

		protected virtual bool ShouldGetQueryForSelectedFiltersFromLayout => ComparisonOperator == ModuleTextFilter.ComparisonConstants.FiltersMatch;

		protected ZQuery GetQueryForSelectedFiltersFromLayout()
		{
			var subModuleFilter = ((IModuleFilterWithSelectedFilters)this).GetSubFilterQueryIncludingCollectionFilters();

			return GetQueryForSelectedFiltersCore(SelectedFilters, subModuleFilter);
		}

		ZQuery IModuleFilterWithSelectedFilters.GetSubFilterQueryIncludingCollectionFilters()
		{
			if (SelectedFilters == null)
			{
				var message = FormattableString.Invariant($@"Unable to load the query for this filter's selected filters. Please ensure that the module associated with the filter is available in the current context.
Filter: {Description}
Module: {ModuleId.Name}"); // exception messages should not be translated.
				throw new InvalidFilterConfigurationException(message, MultilingualDescription, ModuleId.Name);
			}

			var query = SelectedFilters.Filter;
			query.AddToFilter(additionalDisplayFilters);

			return query;
		}

		protected IZFilterModule CreateModuleForQueryBuilding()
		{
			var module = (IZFilterModule)ObjectFactory.Get<IModuleFactory>().Create(ModuleId);

			return module;
		}

		protected virtual ZQuery GetQueryForSelectedFiltersCore(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter)
		{
			var query = GetNewQueryForSelectedFilters();
			var subQuery = GetSubModuleSubQuery(filterBusinessObject, subModuleFilter, UsesNotInQuery);
			AddSelectedFiltersSubquery(filterBusinessObject, query, subQuery);

			return query;
		}

		protected virtual ZDBOnlyQuery GetNewQueryForSelectedFilters()
		{
			var schema = FilterColumn.TableSchema;

			return new ZDBOnlyQueryForFiltersMatch(schema.TableName, schema.PK);
		}

		class ZDBOnlyQueryForFiltersMatch : ZDBOnlyQuery
		{
			internal ZDBOnlyQueryForFiltersMatch(string tableName, SchemaPKColumn pkColumn)
			{
				Initialise(tableName, pkColumn);
			}
		}

		protected virtual void AddSelectedFiltersSubquery(FilterStripBusinessObject filterBusinessObject, ZDBOnlyQuery query, ZDBOnlySubQuery subQuery)
		{
			query.AddSubQuery(subQuery, JoinCondition.And);
		}

		protected ZDBOnlySubQuery GetSubModuleSubQuery(FilterStripBusinessObject filterBusinessObject, ZQuery subModuleFilter, bool usesNotInQuery)
		{
			var subQuery = new ZDBOnlySubQuery(filterBusinessObject.QueryObjectType, SubQueryColumn, usesNotInQuery);
			subQuery.AddToFilter(subModuleFilter);

			return subQuery;
		}

		protected virtual SchemaColumn SubQueryColumn => FilterColumn;

		protected virtual bool UsesNotInQuery => false;

		public virtual bool UseMultiSearch { get; set; }

		#endregion

		#region New Properties

		public ModuleIdentifier ModuleId => GetModuleIdCore();
		public bool SupportsFiltersMatchComparisonOperator { get; set; }

		readonly ModuleIdentifier moduleId;
		ZQuery additionalDisplayFilters;

		protected virtual ModuleIdentifier GetModuleIdCore()
		{
			return moduleId;
		}

		public bool HasValidModuleInCurrentContext => HasValidModuleInCurrentContextCore;
		protected virtual bool HasValidModuleInCurrentContextCore => ModuleId != null && !Equals(ModuleId, ModuleIDs.NotAssigned);

		protected void SetCategoryForInitialModuleId(ModuleIdentifier initialModuleId)
		{
			if (Equals(initialModuleId, ModuleIDs.Organisation) && DefaultCategory != FilterCategories.UserDefined)
			{
				Category = FilterCategories.Organisations;
			}
		}

		#endregion

		#region Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			if (SupportsComparisonOperatorSet)
			{
				writer.WriteElementString(XmlComparerElement, ComparisonOperator);
			}

			writer.WriteElementString(XmlPropertyElement, Property.ToString());

			if (IsFilterCollectionComparisonOperatorSelected() && SelectedFilters != null)
			{
				var layoutData = SelectedFilters.FilterStrips.GetLayoutAsXml();
				var layoutValues = SelectedFilters.FilterStrips.GetLayoutValuesAsXml();

				writer.WriteElementString(XmlPropertySelectedFiltersDataElement, layoutData.ToUTF8());
				writer.WriteElementString(XmlPropertySelectedFiltersValuesElement, layoutValues.ToUTF8());
				writer.WriteElementString(XmlPropertySelectedFiltersModuleIdElement, ModuleId.Name);
			}
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == XmlComparerElement && SupportsComparisonOperatorSet)
			{
				ComparisonOperator = reader.ReadElementString(XmlComparerElement);
			}
			if (reader.Name == XmlPropertyElement)
			{
				var propertyValue = reader.ReadElementString(XmlPropertyElement);
				Property = GetPropertyValueFromDeserializedString(propertyValue);
			}

			if (reader.Name == XmlPropertySelectedFiltersDataElement)
			{
				var factory = new ReadOnlyBusinessObjectFactory { NameForDebugging = "Module Filter Subquery Deserialisation", RefreshEnabled = false };
				var layout = factory.New<StmModuleFilter>();

				var dataString = reader.ReadElementString(XmlPropertySelectedFiltersDataElement);
				layout.S9_FilterData = ZBlob.FromUTF8(dataString);

				if (reader.Name == XmlPropertySelectedFiltersValuesElement)
				{
					var values = layout.GetOrCreateLayoutUserData(selectedFilters?.LayoutsHelper ?? new FilterStripLayoutsHelper());
					var valuesString = reader.ReadElementString(XmlPropertySelectedFiltersValuesElement);
					values.S0_FilterDataValues = ZBlob.FromUTF8(valuesString);
				}

				if (reader.Name == XmlPropertySelectedFiltersModuleIdElement)
				{
					layout.S9_ModuleID = reader.ReadElementString(XmlPropertySelectedFiltersModuleIdElement);
				}

				selectedFilters = GetNewSelectedFilters(layout);
				SelectedFiltersDescriptionInfo.RefreshBinding();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Non-translatable XML element name")]
		protected const string XmlComparerElement = "Comparer";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Non-translatable XML element name")]
		protected const string XmlPropertyElement = "Property";
		protected const string XmlPropertySelectedFiltersDataElement = "PropertySelectedFiltersData";
		protected const string XmlPropertySelectedFiltersValuesElement = "PropertySelectedFiltersValues";
		protected const string XmlPropertySelectedFiltersModuleIdElement = "PropertySelectedFiltersModuleId";

		protected abstract T GetPropertyValueFromDeserializedString(string deserializedValue);

		#endregion

		#region Validation

		public new ModuleFilterWithSelectedFiltersValidation Validation => (ModuleFilterWithSelectedFiltersValidation)base.Validation;

		protected override ModuleFilterValidation GetNewValidation() => new ModuleFilterWithSelectedFiltersValidation(this);

		public class ModuleFilterWithSelectedFiltersValidation : ModuleFilterWithListAndComparisonOperatorsValidation
		{
			public ModuleFilterWithSelectedFiltersValidation(ModuleFilterWithSelectedFilters<T> parent)
				: base(parent)
			{
			}

			public override void ValidateAll()
			{
				base.ValidateAll();
				ValidateSelectedFiltersDescription();
			}

			public void ValidateSelectedFiltersDescription()
			{
				ValidateCalculatedProperty(Parent.SelectedFiltersDescriptionInfo);
			}

			protected virtual void CheckSelectedFiltersDescription()
			{
				if (Parent.IsFilterCollectionComparisonOperatorSelected() && Parent.HasValidModuleInCurrentContext && Parent.SelectedFilterCount > 0)
				{
					Parent.SelectedFilters.RunPreSaveValidation();

					if (Parent.SelectedFilters.ActiveModuleFilters.Any(x => x.HasErrors))
					{
						Parent.SelectedFiltersDescriptionInfo.AddError(Res.GetString("63bbcd4b-ecd6-418e-963a-4e37d61aa988", "The selected filters have one or more errors."));
					}

					CheckFilterRuleDoesNotHaveNonpublishedUserDefinedFilters();
				}
			}

			void CheckFilterRuleDoesNotHaveNonpublishedUserDefinedFilters()
			{
				if (Parent.ActiveModuleFiltersProvider is IRelatedModuleFilterBusinessObject filterBizo && filterBizo.IsInFilterRuleMode)
				{
					UserDefinedFilterHelperBusiness.CheckFilterBizoDoesNotContainNestedNonpublishedUserDefinedFilters(Parent.SelectedFilters,
						trail => Res.GetString("e814e74f-0e9f-4a15-a391-7c648d1523c0", @"The following non-published user-defined filter has been included in the selected filters of this filter strip:

{0}

Filter rules cannot contain non-published user-defined filters.", trail),
						trails => Res.GetString("e6dfc24b-93f2-49ed-979f-9f1bad75759e", @"The following non-published user-defined filters have been included in the selected filters of this filter strip:

{0}

Filter rules cannot contain non-published user-defined filters.", trails), Parent.SelectedFiltersDescriptionInfo, Parent.MultilingualDescription ?? Parent.Description);
				}
			}

			public override Type AutoValidationType => GetType();

			new ModuleFilterWithSelectedFilters<T> Parent
			{
				get
				{
					return (ModuleFilterWithSelectedFilters<T>)base.Parent;
				}
			}
		}

		#endregion
	}
}
