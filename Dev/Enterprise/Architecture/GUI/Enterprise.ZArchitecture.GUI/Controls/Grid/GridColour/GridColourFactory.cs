using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture
{
	public class GridColourFactory : IGridColourFactory
	{
		public GridColourFactory()
		{
		}

		public GridColourFactory(ZGrid grid)
		{
			Grid = grid;
			var collection = grid.List as IBusinessObjectCollection;
			if (collection != null && !grid.IsColourGridFactoryStandAlone)
			{
				try
				{
					fFactory = collection.Factory;
				}
				catch (NotSupportedException)
				{
					//argh
				}
			}
			schemes = new SchemeCollection(Factory);
			((ILegacyBusinessObjectCollectionInternals)schemes).SetOverriddenAdditionalFilter(new ZQuery(StmModuleFilterSchema.S9_ModuleID, SQLComparisonOperator.Contains, ColorSchemeCode));
		}

		readonly SchemeCollection schemes;

		#region Last Used Scheme

		IGridLayoutStorage lastUsedLayout;

		internal ZFilterStripCommonControl Control
		{
			get { return control; }
			set
			{
				control = value;
				if (value != null)
				{
					value.Disposed += delegate
					{ control = null; };
				}
			}
		}
		ZFilterStripCommonControl control;

		internal ZGrid Grid
		{
			get { return grid; }
			set
			{
				grid = value;
				if (value != null)
				{
					value.Disposed += delegate
					{ grid = null; };
					Control = value.GetParentFilterControl();
				}
			}
		}
		ZGrid grid;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "For invalid color scheme and valueFactory occurs recursion")]
		public GridColourScheme GetLastUsedSchemeForCurrentUser(FilterStripBusinessObject activeModuleStrip)
		{
			var found = true;
			var forceLastUsedScheme = false;
			var currentLayout = Grid?.CurrentColumnLayout;

			if (currentLayout == null && activeModuleStrip.LastUsedLayoutLoaded)
			{
				currentLayout = activeModuleStrip.LastUsedLayout;
			}

			if (lastUsedLayout != currentLayout)
			{
				lastUsedLayout = currentLayout;
				lastUsedSchemes.TryRemove(activeModuleStrip, out _);
			}

			GridColourScheme lastUsedScheme = null;
			try
			{
				lastUsedScheme = lastUsedSchemes.GetOrAdd(activeModuleStrip, key =>
				{
					found = false;
					GridColourScheme result = null;

					var layout = currentLayout as StmModuleFilter;

					//If this is a 'real' filter business object, then allow its current layout to force its colour scheme
					if (layout == null && activeModuleStrip.ParentModule is ZFilterModule zFilterModule && !zFilterModule.DoNotCheckOrSaveChanges)
					{
						layout = activeModuleStrip.LastUsedLayout;
					}

					if (layout != null && !layout.IsDeleted && layout.S9_SaveGridColourLayout)
					{
						if (!layout.S9_GridColourLayoutID.IsEmpty)
						{
							result = GetSpecificScheme(key, layout.S9_GridColourLayoutID);
						}

						forceLastUsedScheme = true;
					}

					if (result == null && !forceLastUsedScheme)
					{
						var lastUsed = activeModuleStrip.Factory.LoadTop1<StmData>(GetLastUsedSchemeQuery(key));
						if (lastUsed != null)
						{
							result = GetSpecificScheme(key, lastUsed.SD_GuidValue);
						}

						if (result == null && (lastUsed == null || !lastUsed.SD_GuidValue.IsEmpty)) // this user has no last used scheme yet or the last used scheme was removed, take the first available scheme
						{
							GetAllSchemesForActiveFilter(key, justOne: true);
							result = schemes.GetScheme(GetAllSchemesForActiveFilterQuery(key));
						}
					}

					return result;
				});
			}
			catch (InvalidOperationException ex) when (ex.Message.Contains("ValueFactory attempted to access the Value property of this instance"))
			{
				// no need to report because we know the valueFactory occurs recursion, and the color scheme is invalid
			}

			if (!found && (lastUsedScheme != null || forceLastUsedScheme))
			{
				SetLastUsedSchemeForCurrentUser(activeModuleStrip, lastUsedScheme);
			}
			return lastUsedScheme;
		}

		void AddOrUpdateLastUsedScheme(FilterStripBusinessObject activeModuleStrip, GridColourScheme result)
		{
			lastUsedSchemes.AddOrUpdate(activeModuleStrip, (key) => result, (key, old) => result);
		}

		public ZQuery GetLastUsedSchemeQuery(FilterStripBusinessObject activeModuleStrip)
		{
			var query = new ZQuery();

			query.AddToFilter(GetModuleIdFilterQuery(activeModuleStrip, StmDataSchema.SD_Name));

			query.AddToFilter(StmDataSchema.SD_Type, SQLComparisonOperator.Equal, ColorSchemeCode);
			query.AddToFilter(StmDataSchema.SD_Owner, EnvProxy.Instance.CurrentUser.PK);
			query.AddToFilter(StmDataSchema.SD_DepartmentGuid, EnvProxy.Instance.CurrentCompany.PK);

			return query;
		}

		readonly ConcurrentDictionary<FilterStripBusinessObject, Lazy<GridColourScheme>> lastUsedSchemes = new ConcurrentDictionary<FilterStripBusinessObject, Lazy<GridColourScheme>>();

		#endregion

		#region All Schemes

		bool fullyLoaded;

		GridColourScheme GetSpecificScheme(FilterStripBusinessObject activeModuleStrip, ZGuid pk)
		{
			var query = new ZQuery(StmModuleFilterSchema.PK, pk);
			query.FetchOnlyFromLocalCache = fullyLoaded;
			var scheme = activeModuleStrip.Factory.Load<GridColourScheme>(query).FirstOrDefault();
			if (scheme != null)
			{
				if (scheme.S9_IsSystem
					|| scheme.S9_RelatedEntityID == EnvProxy.Instance.CurrentUser.PK
					|| (scheme.S9_IsPublished && scheme.S9_GC == EnvProxy.Instance.CurrentCompany.PK)
					|| scheme.PublishAcrossAllCompanies)
				{
					var filterStripBusinessObject = control != null ? control.FilterBusinessObject : activeModuleStrip;

					if (!schemes.Contains(scheme))
					{
						scheme.SetStripsFromFilter(filterStripBusinessObject, grid.ElementTypeFromCollection);
						schemes.Add(scheme);
					}
				}
				else
				{
					return null;
				}
			}
			return scheme;
		}

		public GridColourScheme[] GetAllSchemesForActiveFilter(FilterStripBusinessObject activeModuleStrip, bool justOne = false)
		{
			var query = GetAllSchemesForActiveFilterQuery(activeModuleStrip);
			IEnumerable<GridColourScheme> result = activeModuleStrip.Factory.Load<GridColourScheme>(query);

			if (justOne)
			{
				result = result.Take(1);
			}
			else
			{
				fullyLoaded = true;
			}

			foreach (var scheme in result)
			{
				if (!schemes.Contains(scheme))
				{
					schemes.Add(scheme);
				}
			}

			return schemes.GetSchemes(query);
		}

#if DEBUG
		internal
#endif
		ZQuery GetAllSchemesForActiveFilterQuery(FilterStripBusinessObject activeModuleStrip)
		{
			var query = new ZQuery();
			query.AddToFilter(GetModuleIdFilterQuery(activeModuleStrip, StmModuleFilterSchema.S9_ModuleID));
			query.AddToFilter(StmModuleFilterSchema.S9_FilterType, SQLComparisonOperator.NotEqual, StmModuleFilterTypes.Codes.FilterRule);

			var companyQuery = new ZQuery(StmModuleFilterSchema.S9_GC, EnvProxy.Instance.CurrentCompany.PK);
			companyQuery.AddToFilter(JoinCondition.Or, StmModuleFilterSchema.S9_GC, DBNull.Value);
			query.AddToFilter(companyQuery);

			var currentUserOrPublishedOrSystem = new ZQuery(StmModuleFilterSchema.S9_RelatedEntityID, EnvProxy.Instance.CurrentUser.PK);
			currentUserOrPublishedOrSystem.AddToFilter(JoinCondition.Or, StmModuleFilterSchema.S9_IsPublished, true);
			currentUserOrPublishedOrSystem.AddToFilter(JoinCondition.Or, StmModuleFilterSchema.S9_IsSystem, true);
			query.AddToFilter(currentUserOrPublishedOrSystem);

			var noPseudoQuery = new ZDBOnlyQuery(typeof(GridColourScheme));
			noPseudoQuery.AddToFilter(JoinCondition.Or, StmModuleFilterSchema.S9_RelatedEntityID, null);

			var subQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IGlbStaff>(), GlbStaffSchema.PK);
			noPseudoQuery.AddSubQuery(StmModuleFilterSchema.S9_RelatedEntityID, GlbStaffSchema.PK, subQuery, JoinCondition.Or);
			query.AddToFilter(noPseudoQuery);

			query.OrderBy = StmModuleFilterSchema.S9_FilterName.Name;
			return query;
		}

		ZQuery GetModuleIdFilterQuery(FilterStripBusinessObject activeModuleStrip, SchemaColumn schemaColumn)
		{
			var moduleIdQuery = new ZQuery(schemaColumn, SQLComparisonOperator.Equal, GetLayoutContext(activeModuleStrip));
			var gridColorContext = GetGridColorContext(grid, true);
			if (!gridColorContext.IsEmpty)
			{
				moduleIdQuery.AddToFilter(JoinCondition.Or, schemaColumn, SQLComparisonOperator.Equal, gridColorContext);

				if (!grid.ShareActiveColorScheme)
				{
					moduleIdQuery.AddToFilter(JoinCondition.Or, schemaColumn, SQLComparisonOperator.Equal, GetGridColorContext(grid, false));
				}
			}
			var supporter = activeModuleStrip as IGridColourAdditionalModuleIdFilterSupporter;
			if (supporter != null)
			{
				supporter.AddAdditionalFilter(moduleIdQuery, schemaColumn);
			}
			return moduleIdQuery;
		}

		internal static ZString GetLayoutContext(FilterStripBusinessObject activeModuleStrip)
		{
			var key = ((IFilterStripBusinessObjectInternals)activeModuleStrip).LayoutContext + ColorSchemeCode;
			if (key.Length > MaxModuleIdLenght)
			{
				key = key.Substring(key.Length - MaxModuleIdLenght);
			}
			return key;
		}

		internal static ZString GetGridColorContext(ZGrid grid, bool shared)
		{
			if (grid != null && grid.List != null)
			{
				Type elementType = null;

				var alternateContext = grid.List as IUseParentGridContext;
				if (alternateContext != null)
				{
					elementType = alternateContext.ParentType;
				}
				else if (grid.List is IBusinessObjectCollection)
				{
					elementType = BusinessObjectCollection.GetElementTypeFromCollectionType(grid.List.GetType());
				}
				if (elementType == null)
				{
					elementType = grid.List.GetType();
				}

				var key = elementType.FullName + "|" + grid.ColorContextKey + ColorSchemeCode;
				if (!shared)
				{
					key += "|" + grid.GridId;
				}
				if (key.Length > MaxModuleIdLenght)
				{
					key = key.Substring(key.Length - MaxModuleIdLenght);
				}
				return key;
			}

			return ZString.Empty;
		}

		static int MaxModuleIdLenght
		{
			get { return Math.Min(StmModuleFilterSchema.S9_ModuleID.MaxLength, StmDataSchema.SD_Name.MaxLength); }
		}

		internal static ZString GetModuleId(FilterStripBusinessObject activeModuleStrip, ZGrid grid, bool shared)
		{
			var moduleId = GetGridColorContext(grid, shared);
			if (moduleId.IsEmpty)
			{
				moduleId = GetLayoutContext(activeModuleStrip);
			}
			return moduleId;
		}

		#endregion

		#region Set Last Scheme

		public void SetLastUsedSchemeForCurrentUser(FilterStripBusinessObject activeModuleStrip, GridColourScheme scheme)
		{
			var schemePK = scheme != null ? scheme.PK : ZGuid.Empty;

			if (scheme != null && !schemes.Contains(scheme))
			{
				schemes.Add(scheme);
			}

			var query = GetLastUsedSchemeQuery(activeModuleStrip);
			var stmFactory = new BusinessObjectFactory();
			var lastUsedScheme = stmFactory.LoadTop1<StmData>(query);
			var lastSchemeName = new ZString("null");
			var currentSchemeName = scheme == null ? new ZString("null") : scheme.DisplayName;

			if (lastUsedScheme != null)
			{
				var lastUsedSchemaObject = Factory.Load<GridColourScheme>(lastUsedScheme.SD_GuidValue);
				lastSchemeName = lastUsedSchemaObject == null ? new ZString("null") : lastUsedSchemaObject.DisplayName;
				lastUsedScheme.SD_GuidValue = schemePK;
			}
			else
			{
				lastUsedScheme = stmFactory.New<StmData>();

				lastUsedScheme.SD_GuidValue = schemePK;
				lastUsedScheme.SD_Name = GetModuleId(activeModuleStrip, grid, grid.ShareActiveColorScheme);
				lastUsedScheme.SD_Type = ColorSchemeCode;
				lastUsedScheme.SD_Owner = EnvProxy.Instance.CurrentUser.PK;
				lastUsedScheme.SD_DepartmentGuid = EnvProxy.Instance.CurrentCompany.PK;
			}

			if (lastSchemeName != currentSchemeName)
			{
				var colorChangeLog = stmFactory.New<StmALog>();
				using (((IUpdateFieldsLock)colorChangeLog).LockForUpdatingKeyFields())
				{
					colorChangeLog.SL_Parent = lastUsedScheme.PK;
					colorChangeLog.SL_Table = "StmData";
#pragma warning disable CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
					colorChangeLog.SL_SE_NKEvent = AutoEvents.EditedARecordCode;
#pragma warning restore CW1198 // Do Not Use StmALog Event Assignment With Audit Events Analyzer Rule.
					colorChangeLog.SL_Reference = lastSchemeName + " -> " + currentSchemeName;
				}
			}

			stmFactory.Save();
			stmFactory = null;

			AddOrUpdateLastUsedScheme(activeModuleStrip, scheme);

			if (grid != null)
			{
				grid.ColorByPK = new Dictionary<ZGuid, Color>();
				grid.RowColorsAreDataViewOptimisable = scheme != null && scheme.ColourStrips.All(gridColourStrip => gridColourStrip.Filter.IsDataViewOptimisable);
			}
		}

		#endregion

		#region Implementation

		public void ResetSchemes()
		{
			lastUsedSchemes.Clear();
			schemes.RemoveAll();
		}

		internal BusinessObjectFactory Factory
		{
			get { return fFactory ?? (fFactory = new BusinessObjectFactory { NameForDebugging = "ColourGridFactory" }); }
		}

		public string GridColorStripCode => ColorStripCode;

		BusinessObjectFactory fFactory;

		public const string ColorStripCode = StmModuleFilter.ModuleIdSuffix.GridColorStrip;

		internal const string ColorSchemeCode = StmModuleFilter.ModuleIdSuffix.GridColorScheme;
		internal const string SchemeRulesRelationCode = "_SR";

		#endregion
	}
}
