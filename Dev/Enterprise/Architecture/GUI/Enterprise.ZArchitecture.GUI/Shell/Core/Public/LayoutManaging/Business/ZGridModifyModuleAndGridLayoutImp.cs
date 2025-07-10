using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public class ZGridLayoutModification : ZColumnsLayoutModification
	{
		public ZGridLayoutModification(ZGrid grid, ZGridColumns currentColumns, BusinessObjectFactory factory, IGridLayoutStorage layoutToDefault)
			: base(currentColumns.Cast<ICustomizableColumn>().ToList(), factory, layoutToDefault, string.Empty)
		{
			Grid = grid;
			CurrentColumns = currentColumns;
		}

		ZGrid Grid { get; set; }
		new ZGridColumns CurrentColumns { get; set; }

		protected override string GetReasonLayoutNameNotAllowedCore(string layoutName, bool isPublished)
		{
			var result = base.GetReasonLayoutNameNotAllowedCore(layoutName, isPublished);

			if (string.IsNullOrEmpty(result))
			{
				var existingLayout = FindLayout(layoutName, isPublished);

				if (existingLayout != null)
				{
					var layoutIDForNewLayouts = LayoutSetIdentifierToSaveANewLayoutWith;

					if (existingLayout.GridLayoutKey != layoutIDForNewLayouts && layoutIDForNewLayouts.StartsWith(existingLayout.GridLayoutKey))
					{
						result = OverridingExistingLayoutNotAcceptableDueToCustomColumns;
					}
				}
			}

			return result;
		}

		/// <summary>
		/// ID set to StmModuleFilter.S9_ModuleID for a new pre-configured layout
		/// If current columns to save contain custom ones configured against an organisation, then it should save with a key with LayoutContextPK.
		/// Otherwise it should save with a key without LayoutContextPK so that it is available to everyone
		/// </summary>
		protected override string LayoutSetIdentifierToSaveANewLayoutWithCore
		{
			get
			{
				DataGridLayoutContextKeyProvider keyProvider = null;
				if (string.IsNullOrEmpty(Grid.GridId))
				{
					keyProvider = new LegacyDataGridLayoutContextKeyProvider(Grid);
				}
				else
				{
					var hasCustomAttributes =
						Grid.LayoutCategoryPK != Guid.Empty &&
						//If CurrentColumns has custom attributes against organisation
						Grid.QueryHasCustomisedColumns != null &&
						Grid.QueryHasCustomisedColumns(CurrentColumns.GetVisibleColumnMappingNames());

					keyProvider = new DataGridLayoutContextKeyProvider(Grid, hasCustomAttributes);
				}

				return keyProvider.ContextKeyForStmModuleFilter;
			}
		}

		protected override string[] LayoutSetIdentifiersCore
		{
			get
			{
				return new GridLayoutContextKeyProviderHelper().GetAllGridIDsForStmModuleFilter(Grid);
			}
		}

		protected override MemoryStream GetSerialisedStream()
		{
			return new DataGridLayoutDataSetSerialiser().GetLayoutStream(CurrentColumns);
		}

		protected override StmData GetDefaultStmData()
		{
			return new StmData.Loader(Factory).LoadTop1(LayoutSetIdentifierToSaveANewLayoutWith, EnvProxy.Instance.CurrentUser.PK, Grid.LayoutCategoryPK);
		}

		protected override object FindColumn(string columnName)
		{
			return Grid.Columns[columnName];
		}
	}
}
