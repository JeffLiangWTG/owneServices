using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using CargoWise.Common.Collections;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Responsible for saving and retrieving layouts of a grid
	/// </summary>
	internal class DataGridLayoutDataAccessor
	{
		#region Get User Layout Setting

		public DataSet GetUserLayoutSetting(ZGrid grid)
		{
			var result = GetLayoutDataSetFromCache(grid);

			if (result == null || result == DBNull.Value)
			{
				var storage = grid.CurrentColumnLayout;

				if (storage != null)
				{
					using (var layoutDataStream = new MemoryStream(storage.ColumnLayoutData))
					{
						result = new DataGridLayoutDataSetSerialiser().GetColumnSettingDataSet(layoutDataStream);
					}

					UpdateColumnSettingsCache(result, grid);
				}
			}

			if (result == null || result == DBNull.Value)
			{
				result = DBNull.Value;

				UpdateColumnSettingsCache(result, grid);
			}

			return result == DBNull.Value ? null : (DataSet)result;
		}

		public IGridLayoutStorage GetLastSavedLayoutStorage(BusinessObjectFactory factory, ZGrid grid)
		{
			IGridLayoutStorage result = null;
			var contextKeys = GetAllGridIDsForStmData(grid);
			var layouts = new StmData.Loader(factory).Load(contextKeys, EnvProxy.Instance.CurrentUser.PK, grid.LayoutCategoryPK);

			foreach (var nonPreConfiguredLayout in layouts)
			{
				if (nonPreConfiguredLayout != null)
				{
					if (nonPreConfiguredLayout.SD_GuidValue.IsValid)
					{
						var preConfiguredLayout = factory.Load<StmModuleFilter>(nonPreConfiguredLayout.SD_GuidValue);
						if (preConfiguredLayout != null && preConfiguredLayout.S9_SaveColumnLayout)
						{
							result = preConfiguredLayout;
						}
					}

					if (result == null)
					{
						result = StmDataGridLayoutStorage.New(nonPreConfiguredLayout);
					}

					break;
				}
			}
			return result;
		}

		public GridColourScheme GetLastSavedGridColourStorage(BusinessObjectFactory factory, ZGuid gridColourPk)
		{
			return factory.Load<GridColourScheme>(gridColourPk);
		}

		public IGridLayoutStorage GetDefaultLayoutSettingsStorage(BusinessObjectFactory factory, ZGrid grid)
		{
			IGridLayoutStorage result = null;
			var contextKeys = GetAllGridIDsForStmData(grid);
			var layouts = new StmData.Loader(factory).Load(contextKeys, EnvProxy.Instance.CurrentUser.PK, grid.LayoutCategoryPK);
			foreach (var defaultLayout in layouts)
			{
				if (defaultLayout != null)
				{
					result = StmDataGridLayoutStorage.New(defaultLayout);
					break;
				}
			}
			return result;
		}

		/// <summary>
		/// This is to return all available layouts of a grid.
		/// </summary>
		public IEnumerable<IGridLayoutStorage> GetSortedAllLayoutsForFormGrid(BusinessObjectFactory factory, ZGrid grid)
		{
			if (grid.IsGridLayoutConfigurable)
			{
				var helper = new GridLayoutContextKeyProviderHelper();

				foreach (var storage in GetSortedAllLayoutsForFormGrid(factory, helper.GetAllGridIDsForStmModuleFilter(grid, false), helper.GetAllGridIDsForStmData(grid, false), grid.LayoutCategoryPK))
				{
					yield return storage;
				}
			}
			else
			{
				throw new NotSupportedException("gridIDForStmData depends on a current filter name that is selected. There is no way to retrieve all grid layout storages for module grids");
			}
		}

		/// <summary>
		/// This is to return all available layouts for the passed ids of a grid.
		/// 
		/// This method should not be used for module grids where legacy key generated depends on a currently selected filter.
		/// </summary>
		public IEnumerable<IGridLayoutStorage> GetSortedAllLayoutsForFormGrid(BusinessObjectFactory factory, string[] gridIDsForStmModuleFilter, string[] gridIDsForStmData, ZGuid layoutContextPK)
		{
			var result = new List<IGridLayoutStorage>();

			foreach (var gridIDForStmModuleFilter in gridIDsForStmModuleFilter)
			{
				result.AddRange(new StmModuleFilter.Loader(factory).FindByID(gridIDForStmModuleFilter));
			}

			result.Sort(new GridLayoutComparer());

			var layouts = new StmData.Loader(factory).Load(gridIDsForStmData, EnvProxy.Instance.CurrentUser.PK, layoutContextPK);
			foreach (var defaultLayout in layouts)
			{
				if (defaultLayout != null && !defaultLayout.SD_BinaryValue.IsEmpty) // If SD_GuidValue is not empty, StmData functions as a pointer to a current StmModuleFilter layout
				{
					result.Add(StmDataGridLayoutStorage.New(defaultLayout));
				}
			}

			return result;
		}

		class GridLayoutComparer : IComparer<IGridLayoutStorage>
		{
			#region IComparer<StmModuleFilter> Members

			public int Compare(IGridLayoutStorage x, IGridLayoutStorage y)
			{
				int result;

				if (x.IsPublished == y.IsPublished)
				{
					result = x.ColumnLayoutName.CompareTo(y.ColumnLayoutName);
				}
				else
				{
					result = x.IsPublished && !y.IsPublished ? -1 : 1;
				}

				return result;
			}

			#endregion
		}

		#endregion

		#region Save Column Layout

		/// <summary>
		/// Update the currently selected grid layouts for normal grids and module grids
		/// StmData to reflect currently selected layout
		/// StmModuleData to store grid layout data and StmData pointing to StmModuleFilter
		/// 
		/// Should use a different factory to grid's one
		/// </summary>
		public void SaveDataGridDefaultColumnLayout(ZGrid grid)
		{
			ZExceptionReporting.ProcessWithSaveExceptionHandling(() => SaveDataGridDefaultColumnLayoutUnsafe(grid), null, true);
		}

		public void SaveDataGridDefaultColumnLayoutUnsafe(ZGrid grid)
		{
			var factory = new BusinessObjectFactory();

			var currentlySelectedLayout = GetOrCreateCurrentlySelectedUserLayout(factory, grid);

			SaveDataGridColumnLayoutCore(grid, currentlySelectedLayout);

			factory.Save();
		}

		StmData GetOrCreateCurrentlySelectedUserLayout(BusinessObjectFactory factory, ZGrid grid)
		{
			StmData result = null;

			var gridIDs = GetAllGridIDsForStmData(grid);
			var layouts = new StmData.Loader(factory).Load(gridIDs, EnvProxy.Instance.CurrentUser.PK, grid.LayoutCategoryPK);

			foreach (var layout in layouts)
			{
				result = layout;
				if (result != null)
				{
					break;
				}
			}

			if (result == null)
			{
				result = factory.New<StmData>();
				result.SD_Owner = EnvProxy.Instance.CurrentUser.PK;
				result.SD_DepartmentGuid = grid.LayoutCategoryPK;
			}

			//This convert a key from a legacy to a new context key for an existing StmData, too.
			if (string.IsNullOrEmpty(grid.GridId))
			{
				result.SD_Name = new LegacyDataGridLayoutContextKeyProvider(grid).ContextKeyForStmData;
			}
			else
			{
				result.SD_Name = new DataGridLayoutContextKeyProvider(grid, true).ContextKeyForStmData;
			}

			return result;
		}

		/// <summary>
		/// Update layout data for normal grids and module grids
		/// if StmModuleFilter is to store the layout data, then StmData should point to StmModuleFilter
		/// otherwise, StmData does not point to any StmModuleFilter
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "Baseline")]
		void SaveDataGridColumnLayoutCore(ZGrid grid, StmData currentlySelectedLayout)
		{
			var dataGridLayoutTool = new DataGridLayoutDataSetSerialiser();
			var gridLayoutSettings = dataGridLayoutTool.SerialiseAndGetCurrentLayoutAsDataset(grid);

			using (var gridLayoutData = dataGridLayoutTool.GetLayoutStreamFromDataSet(gridLayoutSettings))
			{
				var preConfiguredLayout = grid.CurrentColumnLayout as StmModuleFilter;

				if (preConfiguredLayout != null && !preConfiguredLayout.IsDeleted && preConfiguredLayout.S9_SaveColumnLayout)
				{
					currentlySelectedLayout.SD_GuidValue = preConfiguredLayout.PK;
				}
				else
				{
					currentlySelectedLayout.SD_BinaryValue = gridLayoutData.ToArray();
					currentlySelectedLayout.SD_GuidValue = ZGuid.Empty;
				}

				if (preConfiguredLayout != null && !preConfiguredLayout.IsDeleted)
				{
					if (grid.IsGridLayoutConfigurable) // normal grids. S9_ColumnLayoutData should not change here for configured layouts
					{
						ConvertFromLegacyKeyToNewContextKeyIfRequired(preConfiguredLayout, grid);
					}
					else // for module grids where column layout is attached to a filter layout
					{
						var lastUsedColourScheme = grid.GetLastUsedColourSchemeForCurrentUser;
						preConfiguredLayout.S9_ColumnLayoutData = preConfiguredLayout.S9_SaveColumnLayout ? gridLayoutData.ToArray() : Array.Empty<byte>();
						preConfiguredLayout.S9_GridColourLayoutID = preConfiguredLayout.S9_SaveGridColourLayout && lastUsedColourScheme != null ? lastUsedColourScheme.PK : ZGuid.Empty;
						preConfiguredLayout.Factory.Save();
					}
				}

				UpdateColumnSettingsCache(gridLayoutSettings, grid);
			}
		}

		void ConvertFromLegacyKeyToNewContextKeyIfRequired(StmModuleFilter layoutStorage, ZGrid grid)
		{
			if (layoutStorage != null)
			{
				if (!string.IsNullOrEmpty(grid.GridId) &&
					layoutStorage.S9_ModuleID == new LegacyDataGridLayoutContextKeyProvider(grid).ContextKeyForStmModuleFilter)
				{
					layoutStorage.S9_ModuleID = new DataGridLayoutContextKeyProvider(grid, true).ContextKeyForStmModuleFilter;
				}
			}
		}

		public StmModuleFilter SavePreconfiguredLayout(IModifyModuleAndGridLayout layoutManageable, ZString layoutName, ZBool publish, ZBool publishGlobal, SaveColumnLayout saveColumnLayout, SaveGridColourLayout saveGridColourLayout = SaveGridColourLayout.No, bool isUserDefinedFilter = false)
		{
			StmModuleFilter layoutStorage;

			if (!Globals.IsWeb)
			{
				layoutStorage = GetLayoutForSaving(layoutManageable, layoutName, publish, saveColumnLayout, saveGridColourLayout, isUserDefinedFilter, publishGlobal ? ZGuid.Empty : EnvProxy.Instance.CurrentCompany.PK);
				layoutStorage.S9_GC = publishGlobal ? ZGuid.Empty : EnvProxy.Instance.CurrentCompany.PK;
				SaveLayout(layoutStorage, layoutManageable);
			}
			else
			{
				layoutStorage = SavePreconfiguredLayoutForWeb(layoutManageable, layoutName, publish, publishGlobal, saveColumnLayout, saveGridColourLayout, isUserDefinedFilter, null);
			}

			return layoutStorage;
		}

		internal StmModuleFilter SavePreconfiguredLayoutForWeb(IModifyModuleAndGridLayout layoutManageable, ZString layoutName, ZBool publish, ZBool publishCompany, SaveColumnLayout saveColumnLayout, SaveGridColourLayout saveGridColourLayout, bool isUserDefinedFilter = false, ZGuid? gcPk = null)
		{
			var layoutStorage = GetLayoutForSaving(layoutManageable, layoutName, publish, saveColumnLayout, saveGridColourLayout, isUserDefinedFilter, gcPk);
			layoutStorage.S9_GC = EnvProxy.Instance.CurrentCompany.PK;

			if (publish)
			{
				if (publishCompany)
				{
					//publish for Company
					layoutStorage.S9_RelatedEntityID = ZGuid.Empty;
				}
				else
				{
					//publish for Organisation
					layoutStorage.S9_RelatedEntityID = (layoutManageable as FilterStripBusinessObject)?.LayoutsHelper.CurrentOrganisationPk ?? ZGuid.Empty;
				}
			}
			else
			{
				//private
				layoutStorage.S9_IsPublished = false;
			}

			SaveLayout(layoutStorage, layoutManageable);

			return layoutStorage;
		}

		static StmModuleFilter GetLayoutForSaving(IModifyModuleAndGridLayout layoutManageable, ZString layoutName, ZBool publish, SaveColumnLayout saveColumnLayout, SaveGridColourLayout saveGridColourLayout, ZBool isUserDefinedFilter, ZGuid? gcPk = null)
		{
			var layoutStorage = layoutManageable.FindLayout(layoutName, publish, gcPk) as StmModuleFilter ?? layoutManageable.AddNewLayoutStorage();
			layoutStorage.S9_FilterName = layoutName;
			var layoutId = layoutManageable.LayoutSetIdentifierToSaveANewLayoutWith;

			if (layoutStorage.S9_ModuleID != layoutId)
			{
				layoutStorage.S9_ModuleID = layoutId;
			}

			if (publish) // allow publishing but not unpublishing of an existing layout
			{
				layoutStorage.S9_IsPublished = true;
			}

			if (saveColumnLayout != SaveColumnLayout.Ignore)
			{
				layoutStorage.S9_SaveColumnLayout = saveColumnLayout == SaveColumnLayout.Yes;
			}

			if (isUserDefinedFilter)
			{
				layoutStorage.S9_FilterType = StmModuleFilterTypes.Codes.UserDefined;
			}

			if (saveGridColourLayout != SaveGridColourLayout.Ignore)
			{
				layoutStorage.S9_SaveGridColourLayout = saveGridColourLayout == SaveGridColourLayout.Yes;
			}

			return layoutStorage;
		}

		void SaveLayout(StmModuleFilter layoutStorage, IModifyModuleAndGridLayout layoutManageable)
		{
			layoutManageable.SerialiseLayoutAndWriteTo(layoutStorage);
			layoutManageable.Factory.Save();
			layoutStorage.Factory.Save();
			GridColumnSettingsCache.Remove(layoutStorage.PK);
			//We just potentially modified a UDF or part of a UDF, so clear the FSBO cache.
			ModuleFilter.ClearSelectedFiltersCache();
		}

		#endregion

		string[] GetAllGridIDsForStmData(ZGrid grid)
		{
			string[] contextKeys;
			if (grid?.DataSource != null)
			{
				contextKeys = new GridLayoutContextKeyProviderHelper().GetAllGridIDsForStmData(grid);

				if (contextKeys != null)
				{
					for (var i = 0; i < contextKeys.Length; i++)
					{
						if (contextKeys[i] == null)
						{
							contextKeys[i] = string.Empty;
						}
					}
				}
			}
			else
			{
				contextKeys = Array.Empty<string>();
			}
			return contextKeys;
		}

		#region Implementation

		internal void UpdateColumnSettingsCache(object columnSettings, ZGrid grid)
		{
			if (grid.CurrentColumnLayout != null)
			{
				var cachedColumnSettings = GridColumnSettingsCache[grid.CurrentColumnLayout.PK];

				if (cachedColumnSettings == null || cachedColumnSettings != columnSettings)
				{
					GridColumnSettingsCache.Remove(grid.CurrentColumnLayout.PK);
					GridColumnSettingsCache.Add(grid.CurrentColumnLayout.PK, columnSettings);
				}
			}
		}

		object GetLayoutDataSetFromCache(ZGrid grid)
		{
			object result = null;
			if (grid.CurrentColumnLayout != null)
			{
				GridColumnSettingsCache.TryGetValue(grid.CurrentColumnLayout.PK, out result);
			}
			return result;
		}

		[SuppressThreadStaticFieldMessage]
		static readonly LRUCache<ZGuid, object> GridColumnSettingsCache = new LRUCache<ZGuid, object>(20);//20 instances held in memory, more than this goes to WeakReferences

#if DEBUG
		internal void ClearGridColumnSettingsCacheForTesting()
		{
			GridColumnSettingsCache.Clear();
		}
#endif
		#endregion
	}
}
