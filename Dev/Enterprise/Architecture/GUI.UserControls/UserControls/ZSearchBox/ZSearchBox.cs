using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZSearchBox : ZUserControl
	{
		public ZSearchBox()
		{
			InitializeComponent();

			InnerTextBox.PlaceHolderText = SearchLabel;

			UpdateControlSizes(true);
		}

		string SearchLabel
		{
			get { return Enterprise.ZArchitecture.GUI.UserControls.Res.GetString("75737cb3-caf7-4124-a5f8-3d7b062a85a0", "Search..."); }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			InnerTextBox.SetDataBinding(dataSource, dataMember);
		}

		public event EventHandler<SearchEventArgs> SearchPerformed;

		#region Search

		public string SearchTerm
		{
			get { return InnerTextBox.Text; }
			set { InnerTextBox.Text = value; }
		}

#if DEBUG
		public
#endif
		void OnSearchPerformed(bool searchCleared)
		{
			if (SearchPerformed != null)
			{
				SearchPerformed(this, new SearchEventArgs(SearchTerm, searchCleared));
			}

			UpdateControlSizes(searchCleared);
		}

		void UpdateControlSizes(bool searchCleared)
		{
			ClearSearchPictureBox.Visible = !searchCleared;
			ControlDpiScalingHelper.SetWidth(ref InnerTextBox, this.Width - InnerTextBox.Left * 2 - (ClearSearchPictureBox.Visible ? ClearSearchPictureBox.Width : 0), false);
		}

		public void ClearSearchText()
		{
			SearchTerm = string.Empty;
			UpdateControlSizes(true);
		}

		#endregion

		#region Event Handlers

		void InnerTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			OnKeyDown(e.KeyCode);
		}

		void ClearSearchPictureBox_Click(object sender, EventArgs e)
		{
			OnSearchCleared();
		}

		#endregion

		#region Implementation

#if DEBUG
		internal
#endif
		void OnSearchCleared()
		{
			SearchTerm = string.Empty;
			try
			{
				OnSearchPerformed(true);
			}
			finally
			{
				ClearSearchText();
			}
		}

#if DEBUG
		internal
#endif
		void OnKeyDown(Keys keys)
		{
			if (keys == Keys.Enter)
			{
				var searchCleared = string.IsNullOrEmpty(SearchTerm);
				if (searchCleared)
				{
					OnSearchCleared();
				}
				else
				{
					OnSearchPerformed(searchCleared);
				}
			}
		}

		#endregion
	}

	public class SearchEventArgs : EventArgs
	{
		public SearchEventArgs(string searchTerm, bool cleared)
		{
			SearchTerm = searchTerm;
			Cleared = cleared;
		}

		public string SearchTerm { get; private set; }
		public bool Cleared { get; private set; }
	}
}
