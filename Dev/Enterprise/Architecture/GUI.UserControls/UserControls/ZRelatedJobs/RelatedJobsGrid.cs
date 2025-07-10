using System;
using System.Windows.Forms;

using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	internal class RelatedJobsGrid : ZGrid
	{
		public RelatedJobsGrid()
		{
			IsWholeRowSelectedOnClick = true;
			AfterBind += new EventHandler(RelatedJobsGrid_AfterBind);
			MouseDown += new MouseEventHandler(RelatedJobsGrid_MouseDown);
		}

		#region RowChanged

		void RelatedJobsGrid_AfterBind(object sender, EventArgs e)
		{
			ListManager.CurrentChanged += new EventHandler(ListManager_CurrentChanged);
			OnRowChanged();
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			OnRowChanged();
		}

		void OnRowChanged()
		{
			if (RowChanged != null)
			{
				RowChanged(this, EventArgs.Empty);
			}
		}

		public event EventHandler RowChanged;

		#endregion

		#region RowDoubleClick

		void RelatedJobsGrid_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && e.Clicks > 1)
			{
				HitTestInfo hitInfo = HitTest(e.X, e.Y);
				bool validRowClickedOn = (hitInfo.Row >= 0 && hitInfo.Row < ListManager.List.Count);

				if (validRowClickedOn)
				{
					OnRowDoubleClick();
				}
			}
		}

		void OnRowDoubleClick()
		{
			if (RowDoubleClick != null)
			{
				RowDoubleClick(this, EventArgs.Empty);
			}
		}

		public event EventHandler RowDoubleClick;

		#endregion

		#region SelectedJob

		public IRelatedJob SelectedJob
		{
			get { return (IRelatedJob)ListManager.List[CurrentRowIndex]; }
		}

		#endregion
	}
}
