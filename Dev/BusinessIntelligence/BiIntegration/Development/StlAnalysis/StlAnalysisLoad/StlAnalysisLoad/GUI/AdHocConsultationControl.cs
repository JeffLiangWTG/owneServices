using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Enterprise.StlAnalysis.Load
{
	public partial class AdHocConsultationControl : UserControl // This is a standalone tool.
	{
		public AdHocConsultationControl()
		{
			InitializeComponent();
			InitialiseListViewContextMenu();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1100:DoNotUseMenuItemOrKMenuItem", Justification = "Baseline")]

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a standalone tool.")]
		void InitialiseListViewContextMenu()
		{
			var copyToClipboardMenuHandler = new EventHandler((sender, e) => OnCopyToClipboardMenuClick());
			mnuModuleListMenu.MenuItems.Add(new MenuItem("Copy Contents to Clipboard", copyToClipboardMenuHandler));
			var reloadSelectedModuleMenuHandler = new EventHandler((sender, e) => OnReloadSelectedModule());
			mnuModuleListMenu.MenuItems.Add(new MenuItem("Reload Selected Module", reloadSelectedModuleMenuHandler));
		}

		readonly ContextMenu mnuModuleListMenu = new ContextMenu();
		HostedClient currentClient;
		int year;
		int month;

		public void RefreshControls()
		{
			RefreshClientComboBox();
			RefreshFeatureListView();
		}

		void RefreshClientComboBox()
		{
			cbxHostedClientComboBox.BeginUpdate();

			var databaseBindingList = new BindingList<HostedClient>(HostedClientCollection.Instance.ToArray());
			cbxHostedClientComboBox.DataSource = databaseBindingList;
			cbxHostedClientComboBox.SelectedIndex = -1;
			cbxHostedClientComboBox.Text = cbxHostedClientComboBox.Tag.ToString();

			cbxHostedClientComboBox.EndUpdate();

			if (cbxHostedClientComboBox.Enabled)
			{
				cbxHostedClientComboBox.Focus();
			}
		}

		void RefreshFeatureListView()
		{
			ltvModuleTransactionsListView.BeginUpdate();

			ltvModuleTransactionsListView.Items.Clear();
			ltvModuleTransactionsListView.Columns[0].Width = ListViewExporter.CalculateColumnWidth(ltvModuleTransactionsListView, 0);

			foreach (var feature in LicensedFeatureCollection.Instance)
			{
				var item = new ListViewItem(feature.Name);
				item.Tag = feature;
				item.Name = STLAnalysisForm.ListViewModuleColumnKey;

				item.SubItems.Add(feature.BillingBasis).Name = STLAnalysisForm.ListViewStlBasisColumnKey;
				item.SubItems.Add(feature.BillingEntity).Name = STLAnalysisForm.ListViewStlEntityColumnKey;
				item.SubItems.Add(feature.UnitCountText).Name = STLAnalysisForm.ListViewUnitsColumnKey;

				if (feature.LoadBillingDataException == null)
				{
					item.ToolTipText = feature.Name;
					if (!String.IsNullOrWhiteSpace(feature.BillingQuery))
					{
						item.BackColor = Color.FromArgb(0, 210, 230, 225);
					}
				}
				else
				{
					item.ForeColor = Color.Maroon;
					item.ToolTipText = feature.LoadBillingDataException.Message;
				}

				ltvModuleTransactionsListView.Items.Add(item);
			}

			ltvModuleTransactionsListView.EndUpdate();
		}

		void ResetModuleBillingData()
		{
			LicensedFeatureCollection.Instance.ResetBillingPeriodData();
			RefreshFeatureListView();
		}

		bool IsBillingContextValid()
		{
			return (currentClient != null && year > 0 && month > 0);
		}

		void ReloadAndDisplaySelectedModuleBillingInfo()
		{
			if (ltvModuleTransactionsListView.SelectedItems.Count == 1)
			{
				if (IsBillingContextValid() || SetCurrentBillingContext())
				{
					DateTime startDateInclusive = new DateTime(year, month, 1);
					DateTime endDateExclusive = startDateInclusive.AddMonths(1);

					using (SuspendUserInterface())
					{
						var selectedItem = ltvModuleTransactionsListView.SelectedItems[0];

						if (selectedItem.Tag is LicensedFeature)
						{
							var feature = (LicensedFeature)selectedItem.Tag;
							feature.LoadTransactionCountPerPeriod(currentClient, startDateInclusive, endDateExclusive);
							selectedItem.SubItems[STLAnalysisForm.ListViewUnitsColumnKey].Text = feature.UnitCountText;

							if (feature.LoadBillingDataException == null)
							{
								selectedItem.ForeColor = SystemColors.WindowText;
								selectedItem.ForeColor = String.IsNullOrWhiteSpace(feature.BillingQuery) ? SystemColors.WindowText : Color.DarkBlue;
								selectedItem.ToolTipText = feature.Name;
							}
							else
							{
								selectedItem.ForeColor = Color.Maroon;
								selectedItem.ToolTipText = feature.LoadBillingDataException.Message;
							}
						}
					}
				}
			}
		}

		void ClearCurrentBillingContext()
		{
			currentClient = null;
			year = 0;
			month = 0;
		}

		void ClearBillingInfo()
		{
			if (IsBillingContextValid())
			{
				ClearCurrentBillingContext();
				ResetModuleBillingData();
			}
		}

		bool SetCurrentBillingContext()
		{
			currentClient = cbxHostedClientComboBox.SelectedItem as HostedClient;
			year = GetIntValueFromTextBox(txbYearTextBox, DateTime.UtcNow.Year);
			month = GetIntValueFromTextBox(txbMonthTextBox, (year == DateTime.UtcNow.Year) ? DateTime.UtcNow.Month : 12);
			return IsBillingContextValid();
		}

		void CollectAndDisplayBillingInfo()
		{
			using (SuspendUserInterface())
			{
				DateTime startDateInclusive = new DateTime(year, month, 1);
				DateTime endDateExclusive = startDateInclusive.AddMonths(1);
				LicensedFeatureCollection.Instance.CollectBillingDataPerPeriod(currentClient, startDateInclusive, endDateExclusive);
				RefreshFeatureListView();
			}
		}

		int GetIntValueFromTextBox(TextBox textBoxControl, int maxValue)
		{
			int result = 0;
			if (!Int32.TryParse(textBoxControl.Text, out result) || result > maxValue)
			{
				result = 0;
			}
			return result;
		}

		IDisposable SuspendUserInterface()
		{
			var controlToSuspend = this.Parent ?? this;
			return new UserInterfaceSuspender(controlToSuspend);
		}

		void cbxHostedClientComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			ClearBillingInfo();
		}

		void txbYearTextBox_Enter(object sender, EventArgs e)
		{
			OnTextBoxEnter(sender);
		}

		void txbYearTextBox_Leave(object sender, EventArgs e)
		{
			OnTextBoxLeave(sender);
		}

		void txbYearTextBox_TextChanged(object sender, EventArgs e)
		{
			ClearBillingInfo();
		}

		void txbMonthTextBox_Enter(object sender, EventArgs e)
		{
			OnTextBoxEnter(sender);
		}

		void txbMonthTextBox_Leave(object sender, EventArgs e)
		{
			OnTextBoxLeave(sender);
		}

		void txbMonthTextBox_TextChanged(object sender, EventArgs e)
		{
			ClearBillingInfo();
		}

		void OnTextBoxEnter(object sender)
		{
			var textBox = sender as TextBox;

			if (textBox.Text.Trim() == textBox.Tag.ToString())
			{
				textBox.Text = "";
			}

			textBox.SelectAll();
		}

		protected void OnTextBoxLeave(object sender)
		{
			var textBox = sender as TextBox;

			if (String.IsNullOrWhiteSpace(textBox.Text))
			{
				textBox.Text = textBox.Tag.ToString();
			}
		}

		void ltvModuleTransactionsListView_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				mnuModuleListMenu.Show(ltvModuleTransactionsListView, ltvModuleTransactionsListView.PointToClient(Cursor.Position));
			}
		}

		void OnCopyToClipboardMenuClick()
		{
			new ListViewExporter().CopyToClipboard(ltvModuleTransactionsListView);
		}

		void OnReloadSelectedModule()
		{
			ReloadAndDisplaySelectedModuleBillingInfo();
		}

		void btnCollectDataButton_Click(object sender, EventArgs e)
		{
			if (SetCurrentBillingContext())
			{
				CollectAndDisplayBillingInfo();
			}
			else
			{
				MessageBox.Show("Please select a client and a valid year/month!"); // This is a standalone tool.
			}
		}
	}
}
