using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class IncidentClosureDispositionGridControl : CodeDescriptionBoolControl
	{
		public IncidentClosureDispositionGridControl()
		{
			InitializeComponent();
		}

		IncidentClosureDispositionGridControl parentGrid;
		IncidentClosureDispositionGridControl childGrid;

		public void SetLevel(IncidentClosureDispositionGridControl parentGrid, IncidentClosureDispositionGridControl childGrid)
		{
			this.childGrid = childGrid;
			this.parentGrid = parentGrid;
		}

		public bool CodeDescriptionBoolGridReadOnly
		{
			get { return CodeDescriptionBoolGrid.ReadOnly; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var allNodes = (IncidentClosureDispositionCollection)dataSource;
			if (allNodes != null)
			{
				var parentView = parentGrid != null ? parentGrid.BindingSource.DataSource as IncidentClosureDispositionCollectionView : null;
				dataSource = new IncidentClosureDispositionCollectionView(allNodes, parentView == null || parentView.Count == 0 ? ZGuid.Empty : parentView[0].ID);
			}

			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null && childGrid != null)
			{
				BindingSource.BindingContext[dataSource].CurrentChanged += Grid_CurrentChanged;
			}
		}

		void Grid_CurrentChanged(object sender, EventArgs e)
		{
			BindingManagerBase bm = (BindingManagerBase)sender;
			var childView = childGrid.BindingSource.DataSource as IncidentClosureDispositionCollectionView;
			if (bm.Position >= 0 && bm.Position < bm.Count)
			{
				var currentNode = bm.GetCurrent() as IncidentClosureDisposition;
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

		public void RemoveIsResolutionColumn()
		{
			for (int i = CodeDescriptionBoolGrid.ColumnStyles.Count - 1; i >= 0; --i)
			{
				ZGridColumnInfo columnInfo = (ZGridColumnInfo)CodeDescriptionBoolGrid.ColumnStyles[i];
				if (columnInfo.ColumnName == "IsResolution")
				{
					CodeDescriptionBoolGrid.ColumnStyles.RemoveAt(i);
				}
			}
		}
	}
}
