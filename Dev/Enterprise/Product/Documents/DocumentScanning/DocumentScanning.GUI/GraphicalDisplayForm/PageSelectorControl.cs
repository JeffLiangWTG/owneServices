using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class PageSelectorControl : ZUserControl, IDisposable
	{
		public PageSelectorControl()
		{
			InitializeComponent();
			SyncTotalPages(0);
		}

		public int TotalPages { get; private set; }

		public EventHandler CurrentPageChanged;
		public int CurrentPage => Convert.ToInt32(PageNumericUpDown.Value);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1040", Justification = " Code analysis is high")]
		public int CurrentPageIndex => CurrentPage - 1; // make zero based.

		protected internal virtual void ChangeCurrentPage()
		{
			if (Enabled && !IsDisposed)
			{
				CurrentPageChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public void SyncTotalPages(int requiredTotalPages)
		{
			Enabled = false;
			TotalPages = Math.Max(0, requiredTotalPages);

			var hasPages = TotalPages > 0;
			if (hasPages)
			{
				PageNumericUpDown.Minimum = 1;
				PageNumericUpDown.Maximum = TotalPages;
				PageNumericUpDown.Value = 1;
				TotalPagesLabel.Text = Res.GetString("a7999e3b-2dac-4690-9de5-f52927cdc821", "of {0}", TotalPages);
			}
			else
			{
				PageNumericUpDown.Minimum = 0;
				PageNumericUpDown.Maximum = 0;
				PageNumericUpDown.Value = 0;
				TotalPagesLabel.Text = string.Empty;
			}

			Enabled = hasPages;
		}

		public void SetPageNumbericUpDownValue(int page) => PageNumericUpDown.Value = page;

		public bool SetCurrentPageIndex(int pageIndex)
		{
			var pageNum = pageIndex + 1; // make 1 based.
			var validPageIndex = (pageNum >= PageNumericUpDown.Minimum) && (pageIndex <= PageNumericUpDown.Maximum);

			if (validPageIndex)
			{
				Enabled = false;
				PageNumericUpDown.Value = pageNum;
				Enabled = true;
			}

			return validPageIndex;
		}

		void PageNumericUpDown_ValueChanged(object sender, EventArgs e) => ChangeCurrentPage();
	}
}
