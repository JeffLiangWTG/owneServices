using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	internal class CodeDescriptionBoolTreeControlHelper
	{
		public static void InitializePanelGrids(ICodeDescriptionBoolTreePanelExtension registryPanel)
		{
			if (registryPanel.EditorInfo is ICodeDescriptionBoolTreeRegistryEditorInfoExtension editorInfoExtension)
			{
				var grids = registryPanel.Grids;
				var gridContainers = registryPanel.GridContainers;
				var editorInfo = registryPanel.EditorInfo;
				var captions = editorInfo.Captions.ToArray();

				var showCustomizedColumns = editorInfoExtension.AreCustomizedColumnsVisible;
				var showBoolColumns = editorInfoExtension.AreBoolColumnsVisible;
				for (var i = 0; i < gridContainers.Count; i++)
				{
					if (i + 1 <= captions.Length)
					{
						if (grids[i] is ICodeDescriptionBoolTreeNodeGridExtension gridExtension)
						{
							var previousGrid = (i <= 0) ? null : grids[i - 1];
							var nextGrid = (i >= captions.Length - 1) ? null : grids[i + 1];
							gridExtension.SetLevel(previousGrid, nextGrid);

							gridContainers[i].GetExtension<ILabelCaptionRenderer>().Caption = captions[i];
							grids[i].SetupColumns(editorInfo.BoolColumnCaption, showBoolColumns[i], editorInfo.IsCodeColumnVisible);
							if (!showCustomizedColumns[i])
							{
								RemoveAllCustomizedColumns(gridExtension, editorInfoExtension);
							}
						}
					}
					else
					{
						gridContainers[i].Visible = false;
					}
				}
			}
		}

		static void RemoveAllCustomizedColumns(
			ICodeDescriptionBoolTreeNodeGridExtension gridExtension,
			ICodeDescriptionBoolTreeRegistryEditorInfoExtension editorInfoExtension)
		{
			for (var i = gridExtension.InnerGrid.ColumnStyles.Count - 1; i >= 0; --i)
			{
				var columnInfo = (ZGridColumnInfo)gridExtension.InnerGrid.ColumnStyles[i];
				if (editorInfoExtension.CustomizedColumns.Any(x => x.Equals(columnInfo.ColumnName)))
				{
					gridExtension.InnerGrid.ColumnStyles.RemoveAt(i);
				}
			}
		}

		public static TTreeView GetTreeView<TTreeView>(ICodeDescriptionBoolTreeNodeGridExtension gridExtension, object dataSource)
			where TTreeView : CodeDescriptionBoolTreeView
		{
			TTreeView result = null;
			var allNodes = dataSource as ICodeDescriptionBoolTreeNodeCollectionExtension;
			if (allNodes != null)
			{
				var parent = gridExtension.ParentGrid;
				var parentView = parent != null ? parent.BindingSource.DataSource as TTreeView : null;
				result = (TTreeView)allNodes.CreateView(parentView == null || parentView.Count == 0 ? ZGuid.Empty : parentView[0].ID);
			}

			return result;
		}

		public static void BindDataSourceCurrentChangeEvent(ICodeDescriptionBoolTreeNodeGridExtension gridExtension, object dataSource)
		{
			if (dataSource != null && gridExtension.ChildGrid != null)
			{
				(gridExtension as CodeDescriptionBoolControl).BindingSource.BindingContext[dataSource].CurrentChanged += Grid_CurrentChanged;
			}

			void Grid_CurrentChanged(object sender, EventArgs e)
			{
				var bm = (BindingManagerBase)sender;
				var childView = gridExtension.ChildGrid.BindingSource.DataSource as CodeDescriptionBoolTreeView;
				if (childView != null && bm.Position >= 0 && bm.Position < bm.Count)
				{
					var currentNode = bm.GetCurrent() as CodeDescriptionBoolTreeNode;
					if (currentNode != null)
					{
						childView.ParentID = currentNode.ID;
					}
					else
					{
						childView.ParentID = ZGuid.Missing;
					}
				}
				else
				{
					childView.ParentID = ZGuid.Missing;
				}
			}
		}
	}
}
