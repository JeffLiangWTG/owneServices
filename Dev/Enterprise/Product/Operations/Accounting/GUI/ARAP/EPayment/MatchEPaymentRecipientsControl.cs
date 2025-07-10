using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class MatchEPaymentRecipientsControl : ZUserControl
	{
		public MatchEPaymentRecipientsControl()
		{
			InitializeComponent();
			FilteredRecipientsGridPanel.AllowOverlap(FilteredRecipientsFilterPanel);
		}

		protected ZTextBox ErrorDescriptionTextBox;
		protected ZTextBox LastResponseTextBox;
		protected ZTextBox StatusTextBox;
		protected ZTextBox RequestedByTextBox;
		protected ZTextBox LastRequestedTextBox;
		protected ZTextBox ProviderCodeTextBox;
		ZButton RefreshButton;

		MatchEPaymentRecipients MatchEPaymentRecipients
		{
			get
			{
				MatchEPaymentRecipients result = (MatchEPaymentRecipients)BindingSource.Current;
				if (MatchEPaymentRecipientsForDispose == null)
				{
					MatchEPaymentRecipientsForDispose = result;
				}
				return result;
			}
		}
		MatchEPaymentRecipients MatchEPaymentRecipientsForDispose;

		public IEnumerable<AccEPaymentBeneficiary> SelectedBeneficiaries => FilteredRecipientsGrid.SelectedElements.OfType<AccEPaymentBeneficiary>();

		public ZGrid FilteredRecipientsGrid_ForTestOnly => FilteredRecipientsGrid;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!this.IsDesignMode())
			{
				AllRecipientsFilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
				FilteredRecipientsFilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
				FilteredRecipientsGridPanel.Dock = System.Windows.Forms.DockStyle.None;
			}
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);

			if (MatchEPaymentRecipientsFilterControl != null)
			{
				FilteredRecipientsFilterPanel.Controls.Remove(MatchEPaymentRecipientsFilterControl);
				MatchEPaymentRecipientsFilterControl.PerformSearch -= FilteredRecipientsFilterControl_PerformSearch;
				MatchEPaymentRecipientsFilterControl.FiltersCleared -= FilteredRecipientsFilterControl_ClearButtonClicked;
			}

			if (MatchEPaymentRecipients != null)
			{
				MatchEPaymentRecipientsFilterBuisnessObject = MatchEPaymentRecipients.RecipientsFilter;
				MatchEPaymentRecipientsFilterControl = new AccountingOnFormFilterControl(new AccEPaymentBeneficiaryCollection(MatchEPaymentRecipients.Factory), MatchEPaymentRecipientsFilterBuisnessObject);
				MatchEPaymentRecipientsFilterControl.SetMaxFilterStripPanelHeight(250);
				MatchEPaymentRecipientsFilterControl.Size = FilteredRecipientsFilterPanel.ClientSize;
				MatchEPaymentRecipientsFilterControl.BackColor = BackColor;
				MatchEPaymentRecipientsFilterControl.Dock = System.Windows.Forms.DockStyle.Fill;

				FilteredRecipientsFilterPanel.Controls.Add(MatchEPaymentRecipientsFilterControl);
				MatchEPaymentRecipientsFilterControl.PerformSearch += FilteredRecipientsFilterControl_PerformSearch;
				MatchEPaymentRecipientsFilterControl.FiltersCleared += FilteredRecipientsFilterControl_ClearButtonClicked;
				MatchEPaymentRecipientsFilterControl.FilteredGrid.SizeChanged += FilteredRecipientsFilteredGrid_BoundsChanged;
				MatchEPaymentRecipientsFilterControl.FilteredGrid.LocationChanged += FilteredRecipientsFilteredGrid_BoundsChanged;
				FilteredRecipientsFilteredGrid_BoundsChanged(MatchEPaymentRecipientsFilterControl.FilteredGrid, EventArgs.Empty);
				MatchEPaymentRecipientsFilterControl.FilteredGrid.ColumnStyles.Clear();

				MatchEPaymentRecipients.OnChangeFilteredRecipients -= MatchEPaymentRecipientsBase_OnChangeReceipts;
				MatchEPaymentRecipients.OnChangeFilteredRecipients += MatchEPaymentRecipientsBase_OnChangeReceipts;
			}
		}

		void FilteredRecipientsFilteredGrid_BoundsChanged(object sender, EventArgs e)
		{
			FilteredRecipientsGridPanel.Bounds = MatchEPaymentRecipientsFilterControl.FilteredGrid.Bounds;
		}

		void MatchEPaymentRecipientsBase_OnChangeReceipts(object sender, EventArgs e)
		{
			FilteredRecipientsInfoLabel.Text = GetFilteredRecipientsInfoLableText();
		}

		protected virtual string GetFilteredRecipientsInfoLableText()
		{
			return Res.GetString("D7B43F12-51B8-4e7e-A87B-5AD374C19DBD", "Found {0} records that match your criteria.", MatchEPaymentRecipients.FilteredRecipients.Count);
		}

		internal void FilteredRecipientsFilterControl_PerformSearch(object sender, EventArgs e)
		{
			MatchEPaymentRecipients.ValidateBeforeFindingFilteredRecipients();

			if (MatchEPaymentRecipients.HasErrors)
			{
				ShowErrorMessageToUser();
			}
			else
			{
				MatchEPaymentRecipients.LoadFilteredRecipients();
			}
		}

		void ShowErrorMessageToUser()
		{
			using (var form = new ZErrorMessageBox(MatchEPaymentRecipients, Res.GetString("a25831cc-81d5-4582-a082-b0624f7ac1d0", "search"), Res.GetString("b48329d8-e65c-4bb7-90b2-5c0710f75719", "perform"), Res.GetString("a28d06f7-7f7c-42c3-a250-0901158cdf28", "performed")))
			{
				ZFormModaliser.ShowMessageBoxWithoutDispose(form);
			}
		}

		internal void FilteredRecipientsFilterControl_ClearButtonClicked(object sender, EventArgs e)
		{
			MatchEPaymentRecipients.ClearFilteredRecipients();
		}

		void MatchEPaymentRecipientsControl_BackColorChanged(object sender, EventArgs e)
		{
			SyncRecipientTabPage.BackColor = BackColor;
			FilteredRecipientsTabPage.BackColor = BackColor;
			FilteredRecipientsNotificationPanel.BackColor = BackColor;

			if (MatchEPaymentRecipientsFilterControl != null)
			{
				MatchEPaymentRecipientsFilterControl.BackColor = BackColor;
			}
		}

		internal void RefreshButton_Click(object sender, EventArgs e)
		{
			MatchEPaymentRecipients.RefreshRequests();
			MatchEPaymentRecipients.RefreshBinding();
			(ParentForm as MatchEPaymentRecipientsForm)?.MatchEPaymentRecipients.RefreshBinding();
		}

		void FilteredRecipientsGrid_AfterBind(object sender, EventArgs e)
		{
			if (FilteredRecipientsGrid.ListManager != null)
			{
				FilteredRecipientsGrid.ListManager.CurrentChanged += FilteredRecipientsGrid_CurrentChanged;
				FilteredRecipientsGrid_CurrentChanged(FilteredRecipientsGrid.ListManager, EventArgs.Empty);
			}
		}

		void FilteredRecipientsGrid_CurrentChanged(object sender, EventArgs e)
		{
			FilteredRecipientsGridCurrentItemChanged?.Invoke(sender, e);
		}

		public EventHandler FilteredRecipientsGridCurrentItemChanged;
	}
}

