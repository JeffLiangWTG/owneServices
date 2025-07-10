using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Core.Forms.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Data grid layout will be looked up from a grid using this key matching StmData.SD_Name or StmModuleFilter.S9_ModuleID for a current user
	/// </summary>
	public class DataGridLayoutContextKeyProvider
	{
		public const string Prefix = "DataGridLayout";

		public DataGridLayoutContextKeyProvider(ZGrid grid)
			: this(grid, false)
		{
		}

		public DataGridLayoutContextKeyProvider(ZGrid grid, bool shouldAddLayoutCategoryPKIfExists)
		{
			this.Grid = grid;
			this.ShouldAddLayoutCategoryPKIfExists = shouldAddLayoutCategoryPKIfExists;
		}

		protected readonly ZGrid Grid;
		protected readonly bool ShouldAddLayoutCategoryPKIfExists;

		protected virtual string ContextKey
		{
			get
			{
				var result = Prefix + "|" + GetControlIdentifier(Grid);

				var idRootID = Grid.IdentifierRootID;
				if (!string.IsNullOrEmpty(idRootID))
				{
					result += "|" + idRootID;
				}

				if (ShouldAddLayoutCategoryPKIfExists)
				{
					result += GetLayoutCategoryPKIfRequired();
				}

				var columnLayoutContext = Grid.ColumnLayoutContext;
				if (!string.IsNullOrEmpty(columnLayoutContext))
				{
					result += "|" + columnLayoutContext;
				}

				return result;
			}
		}

		public string ContextKeyForStmData
		{
			get { return ContextKey; }
		}

		public string ContextKeyForStmModuleFilter
		{
			get
			{
				var result = "";

				var moduleGrid = Grid as ZFilterGrid;

				if (moduleGrid != null)
				{
					result = moduleGrid.ModuleIDName;
				}
				else
				{
					result = ContextKeyForStmModuleFilterForNormalGrid;
				}

				return result;
			}
		}

		protected virtual string ContextKeyForStmModuleFilterForNormalGrid
		{
			get { return ContextKey; }
		}

		string GetControlIdentifier(ZGrid grid)
			=> grid.GridId;

		protected string GetLayoutCategoryPKIfRequired()
		{
			var result = string.Empty;

			if (Grid.LayoutCategoryPK != Guid.Empty)
			{
				result += "|" + Grid.LayoutCategoryPK;
			}

			return result;
		}
	}

	/// <summary>
	/// Legacy way of generating context keys for a grid for StmData.SD_Name or StmModuleFilter.S9_ModuleID. 
	/// Will be obsolete soon when all grids have a unique identifier assigned.
	/// </summary>
#if DEBUG
	public
#endif
	class LegacyDataGridLayoutContextKeyProvider : DataGridLayoutContextKeyProvider
	{
		public LegacyDataGridLayoutContextKeyProvider(ZGrid grid)
			: this(grid, false)
		{
		}

		public LegacyDataGridLayoutContextKeyProvider(ZGrid grid, bool shouldAddLayoutCategoryPKIfExists)
			: this(grid, shouldAddLayoutCategoryPKIfExists, true)
		{
		}

		public LegacyDataGridLayoutContextKeyProvider(ZGrid grid, bool shouldAddLayoutCategoryPKIfExists, bool useFullParentsPath)
			: base(grid, shouldAddLayoutCategoryPKIfExists)
		{
			this.useFullGridParentsPath = useFullParentsPath;
		}

		readonly bool useFullGridParentsPath;

		protected override string ContextKey
		{
			get
			{
				var dataMember = Grid.DataMember;

				var displayGrid = Grid as ZDisplayGrid;

				if (displayGrid != null)
				{
					var filterBusinessObject = displayGrid.FilterBusinessObject;
					if (filterBusinessObject != null)
					{
						var layout = filterBusinessObject.LastUsedLayout;
						if (layout != null && !layout.IsDeleted && !layout.IsDeleting && layout.S9_SaveColumnLayout)
						{
							dataMember += layout.PK;
						}
					}
				}
				else
				{
					var parentControl = Grid.Parent;

					if (parentControl != null && parentControl is Enterprise.Core.Forms.CodeDescriptionListEditControl && Grid.ColumnLayoutContext != null)
					{
						dataMember += Grid.ColumnLayoutContext;
					}
				}

				if (Grid.DataSource == null)
				{
					ErrorReporter.ReportOnce("DataSource is Null", Grid.Parent.Name + ":" + Grid.Name);
					return null;
				}

				var result = new GridLayoutKeyGenerator(Grid, Grid.DataSource, dataMember, useFullGridParentsPath).GetContextKey();

				if (ShouldAddLayoutCategoryPKIfExists)
				{
					result += GetLayoutCategoryPKIfRequired();
				}

				return result;
			}
		}
	}

	/// <summary>
	/// Responsible for providing all the related context keys including a legacy key as well as organisation-specific keys
	/// </summary>
	class GridLayoutContextKeyProviderHelper
	{
		public string[] GetAllGridIDsForStmModuleFilter(ZGrid grid, bool getLegacyLayouts = true)
		{
			var result = new HashSet<string>();

			if (grid != null)
			{
				foreach (var keyProvider in GetKeyProvidersForStmModuleFilter(grid, getLegacyLayouts))
				{
					result.Add(keyProvider.ContextKeyForStmModuleFilter);
				}
			}

			return result.ToArray();
		}

		public string[] GetAllGridIDsForStmData(ZGrid grid, bool getLegacyLayouts = true)
		{
			var result = new List<string>();

			if (grid != null)
			{
				foreach (var keyProvider in GetKeyProvidersForStmData(grid, getLegacyLayouts))
				{
					var key = keyProvider.ContextKeyForStmData;

					if (!result.Contains(key))
					{
						result.Add(key);
					}
				}
			}

			return result.ToArray();
		}

#if DEBUG
		protected virtual
#endif
		IEnumerable<DataGridLayoutContextKeyProvider> GetKeyProvidersForStmModuleFilter(ZGrid grid, bool getLegacyLayouts = true)
		{
			//for general layouts
			if (!string.IsNullOrEmpty(grid.GridId))
			{
				yield return new DataGridLayoutContextKeyProvider(grid);
			}

			if (getLegacyLayouts || string.IsNullOrEmpty(grid.GridId))
			{
				yield return new LegacyDataGridLayoutContextKeyProvider(grid, false, false);
				yield return new LegacyDataGridLayoutContextKeyProvider(grid, false, true);
			}

			//for organisation-specific layouts
			if (grid.LayoutCategoryPK != Guid.Empty)
			{
				if (!string.IsNullOrEmpty(grid.GridId))
				{
					yield return new DataGridLayoutContextKeyProvider(grid, true);
				}
				else
				{
					yield return new LegacyDataGridLayoutContextKeyProvider(grid, true);
				}
			}
		}

		IEnumerable<DataGridLayoutContextKeyProvider> GetKeyProvidersForStmData(ZGrid grid, bool getLegacyLayouts = true)
		{
			if (!string.IsNullOrEmpty(grid.GridId))
			{
				yield return new DataGridLayoutContextKeyProvider(grid, true);
			}

			if (getLegacyLayouts || string.IsNullOrEmpty(grid.GridId))
			{
				yield return new LegacyDataGridLayoutContextKeyProvider(grid);
			}
		}
	}
}
