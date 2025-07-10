using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.NetworkVisualisation.Integration;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.NetworkVisualisation.GUI
{
	/// <summary>
	/// Represents a form for searching and finding items.
	/// </summary>
	/// <remarks>
	/// Inherits from <see cref="ZChildForm"/>
	/// </remarks>
	public partial class SearchFinderForm : ZChildForm
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SearchFinderForm"/> class.
		/// </summary>
		/// <param name="networkUserControl">The network user control.</param>
#pragma warning disable CS0618 // Disable the warning for obsolete usage
		public SearchFinderForm(NetworkUserControl networkUserControl)
		{
			InitializeComponent();

			this.networkUserControl = networkUserControl;
			refresher = networkUserControl.NetworkViewModel.Network.Refresher;
			refresher.Refreshed += Refresher_Refreshed;

			searchTextBox.Focus();
			searchButton.CaptionResourceString = Res.GetData("df1b702e-b74e-4a6a-9df9-4677d97749fa", "Search");
		}
#pragma warning restore CS0618 // Restore the warning for obsolete usage

#pragma warning disable CS0618 // Disable the warning for obsolete usage
		readonly NetworkUserControl networkUserControl;
#pragma warning restore CS0618 // Restore the warning for obsolete usage
		readonly INetworkRefresher refresher;
		SearchFinderViewModel viewModel;

		/// <summary>
		/// Refocuses on the search text box.
		/// </summary>
		public void ReFocus()
		{
			searchTextBox.Focus();
		}

		void ToggleSearchButton(ResourceStringData buttonText)
		{
			searchButton.CaptionResourceString = buttonText;
		}

		void SetSearchButtonToSearch()
		{
			ToggleSearchButton(Res.GetData("df1b702e-b74e-4a6a-9df9-4677d97749fa", "Search"));
		}

		#region Buttons
		void cancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void searchTextBox_TextChanged(object sender, EventArgs e)
		{
			SetSearchButtonToSearch();
			viewModel?.ResetSearchState();
		}

		void Refresher_Refreshed(object sender, RefreshArgs e)
		{
			SetSearchButtonToSearch();
		}

		void searchButton_Click(object sender, EventArgs e)
		{
			if (viewModel == null || viewModel.NetworkViewModel != networkUserControl.NetworkViewModel)
			{
				viewModel = new SearchFinderViewModel(networkUserControl.NetworkViewModel);
			}

			if (!viewModel.HasPerformedSearch)
			{
				viewModel.PerformSearch(searchTextBox.Text);
				ToggleSearchButton(Res.GetData("659ae479-54b9-4d07-88ce-0a5d4cbe9e02", "Next Result"));
			}
			else
			{
				viewModel.ShowNextResult();
			}
		}

		void searchTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				searchButton.PerformClick();
			}
			else if (e.KeyCode == Keys.Escape)
			{
				Close();
			}
		}

		protected override bool ShowStatusBar => false;

		protected override void UpdateStatusBar(string notification, INotificationType state)
		{
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			Dispose();
		}

		#region IDisposable Support

		bool disposedValue;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!disposedValue)
				{
					disposedValue = true;
					refresher.Refreshed -= Refresher_Refreshed;
					this.Close();
				}

				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#endregion
		#endregion
		#region Testing
#if DEBUG

		public void SetSearchBox_ForTest(string text)
		{
			searchTextBox.Text = text;
		}

		public void Search_PerformClick_ForTest()
		{
			searchButton.PerformClick();
		}

		public string GetSearchButtonContent_ForTest() => searchButton.CaptionResourceString.Caption;

		public bool? HasPerformedSearch_ForTest => viewModel?.HasPerformedSearch;

#endif
		#endregion
	}
}
