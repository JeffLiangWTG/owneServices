using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZColumnsLayoutModification : IModifyModuleAndGridLayout
	{
		public ZColumnsLayoutModification(ICollection<ICustomizableColumn> currentColumns, BusinessObjectFactory factory, IGridLayoutStorage layoutToDefault, string layoutKey)
		{
			CurrentColumns = currentColumns;
			Factory = factory;
			LayoutToDefault = layoutToDefault;
			this.layoutKey = layoutKey;
		}

		public string ValidateAndGetErrorsForSavingLayout()
		{
			return CurrentColumns.Count == 0 ? NoColumnsToSave : string.Empty;
		}

		public string GetReasonLayoutNameNotAllowed(string layoutName, bool isPublished)
		{
			return GetReasonLayoutNameNotAllowedCore(layoutName, isPublished);
		}

		protected virtual string GetReasonLayoutNameNotAllowedCore(string layoutName, bool isPublished)
		{
			var result = string.Empty;

			if (layoutName.ToLower() == StmDataGridLayoutStorage.DefaultLayoutName.GetUnresolvedString().ToLower())
			{
				result = DefaultAsFilterNameIsNotAcceptable;
			}

			return result;
		}

		public BusinessObjectFactory Factory { get; private set; }

		public IGridLayoutStorage LayoutToDefault { get; private set; }

		public StmModuleFilter AddNewLayoutStorage()
		{
			var result = Factory.New<StmModuleFilter>();
			result.S9_SaveColumnLayout = true;
			return result;
		}

		public IGridLayoutStorage FindLayout(string layoutName, bool isPublished)
		{
			return FindLayout(layoutName, isPublished, null);
		}

		public IGridLayoutStorage FindLayout(string layoutName, bool isPublished, ZGuid? gcPk)
		{
			return FindLayoutByName(layoutName, isPublished, shouldIgnoreIsPublished: false);
		}

		public IGridLayoutStorage FindLayout(string layoutName)
		{
			return FindLayoutByName(layoutName, isPublished: false, shouldIgnoreIsPublished: true);
		}

		IGridLayoutStorage FindLayoutByName(string layoutName, bool isPublished, bool shouldIgnoreIsPublished)
		{
			IGridLayoutStorage result = null;

			if (layoutName == StmDataGridLayoutStorage.DefaultLayoutName)
			{
				result = GetDefaultGridLayout();
			}
			else
			{
				foreach (var key in LayoutSetIdentifiers)
				{
					var loader = new StmModuleFilter.Loader(Factory);
					result = shouldIgnoreIsPublished ? loader.FindTop1ByIDAndName(key, layoutName) : loader.FindTop1ByIDAndName(key, layoutName, isPublished);

					if (result != null)
					{
						break;
					}
				}
			}

			return result;
		}

		public IGridLayoutStorage FindLayout(ZGuid layoutPK)
		{
			return Factory.Load<StmModuleFilter>(layoutPK);
		}

		public string LayoutSetIdentifierToSaveANewLayoutWith
		{
			get { return LayoutSetIdentifierToSaveANewLayoutWithCore; }
		}

		protected virtual string LayoutSetIdentifierToSaveANewLayoutWithCore
		{
			get { return layoutKey; }
		}
		readonly string layoutKey;

		public string[] LayoutSetIdentifiers
		{
			get { return LayoutSetIdentifiersCore; }
		}

		protected virtual string[] LayoutSetIdentifiersCore
		{
			get { return new[] { layoutKey }; }
		}

		public void SerialiseLayoutAndWriteTo(StmModuleFilter layoutStorage)
		{
			using (var layoutData = GetSerialisedStream())
			{
				layoutStorage.S9_ColumnLayoutData = layoutData.ToArray();
			}
		}

		protected virtual MemoryStream GetSerialisedStream()
		{
			return new DataGridLayoutDataSetSerialiser().GetLayoutStream(CurrentColumns);
		}

		public IEnumerable<StmModuleFilter> GetLayouts(bool isPublished)
		{
			return
				from contextKey in LayoutSetIdentifiers
				from layout in new StmModuleFilter.Loader(Factory).FindByID(contextKey)
				where layout.S9_IsPublished == isPublished
				select layout;
		}

		public IGridLayoutStorage GetDefaultGridLayout()
		{
			return StmDataGridLayoutStorage.New(GetDefaultStmData());
		}

		protected virtual StmData GetDefaultStmData()
		{
			return new StmData.Loader(Factory).LoadTop1(LayoutSetIdentifierToSaveANewLayoutWith, EnvProxy.Instance.CurrentUser.PK, ZGuid.Empty);
		}

		public IEnumerable<ILayoutDetailTreeNode> GetLayoutDetailTree(StmModuleFilter layout)
		{
			if (layout != null)
			{
				var serialiser = new DataGridLayoutDataSetSerialiser();

				using (var layoutData = new MemoryStream(layout.S9_ColumnLayoutData))
				{
					foreach (var columnName in serialiser.GetVisibleColumnNames(layoutData))
					{
						var column = FindColumn(columnName);
						if (column != null)
						{
							yield return column as ILayoutDetailTreeNode ?? new LayoutDetailTreeNodeImpl(columnName);
						}
					}
				}
			}
		}

		protected virtual object FindColumn(string columnName)
		{
			return CurrentColumns.FirstOrDefault(column => column.ColumnName == columnName || string.IsNullOrEmpty(column.ColumnName) && column.ToString() == columnName);
		}

		#region Implementation

		public ICollection<ICustomizableColumn> CurrentColumns { get; private set; }

		public static string NoColumnsToSave
		{
			get { return Res.GetString("dba40ef5-dc5e-409d-a53e-c420208cc59d", "There are no columns to save. Please choose and add columns."); }
		}

		public static string DefaultAsFilterNameIsNotAcceptable
		{
			get { return Res.GetString("e03d8533-c33f-406a-be4b-d2f6a7e3d8cf", "'Default' is not allowed. Please give it a different name."); }
		}

		public static string OverridingExistingLayoutNotAcceptableDueToCustomColumns
		{
			get { return Res.GetString("04d0b282-ab8b-4d16-9acf-5c48ce202ba5", "The layout already exists and you cannot overwrite the layout. The current columns to be saved contain custom ones and the existing layout does not. Please give it a different name."); }
		}

		#endregion

		#region LayoutDetailTreeNodeImpl

		class LayoutDetailTreeNodeImpl : ILayoutDetailTreeNode
		{
			public LayoutDetailTreeNodeImpl(string caption)
			{
				this.caption = caption;
			}

			readonly string caption;

			public override string ToString()
			{
				return caption;
			}

			#region Implementation of ILayoutDetailTreeNode

			/// <summary>
			/// A text that appears on a tree view node. filter strip description or column name
			/// </summary>
			string ILayoutDetailTreeNode.UniqueID
			{
				get { return caption; }
			}

			/// <summary>
			/// A text that appears on a parent node. Category description for filters or null for grids. 
			/// For filter layouts, the tree view shows two levels. Categories and then filters under each category
			/// </summary>
			string ILayoutDetailTreeNode.ParentUniqueID
			{
				get { return string.Empty; }
			}

			#endregion
		}

		#endregion
	}
}
