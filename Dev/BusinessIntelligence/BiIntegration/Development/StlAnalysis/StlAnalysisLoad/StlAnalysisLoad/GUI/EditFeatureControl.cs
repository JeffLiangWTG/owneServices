using System;
using System.Drawing;
using System.Windows.Forms;

namespace Enterprise.StlAnalysis.Load
{
	public partial class EditFeatureControl : UserControl // Cannot use ZArchitecture because this is a standalone tool
	{
		public EditFeatureControl()
		{
			InitializeComponent();
		}

		public void RefreshControls()
		{
			RefreshFeatureListView();
		}

		void RefreshFeatureListView()
		{
			ltvFeatureListView.BeginUpdate();

			ltvFeatureListView.Items.Clear();
			ltvFeatureListView.Columns[1].Width = ListViewExporter.CalculateColumnWidth(ltvFeatureListView, 1);

			foreach (var feature in LicensedFeatureCollection.Instance)
			{
				var item = new ListViewItem(feature.FeatureCode);
				item.Name = STLAnalysisForm.ListViewCodeColumnKey;
				item.Tag = feature;
				item.ToolTipText = feature.Name;

				item.SubItems.Add(feature.Name).Name = STLAnalysisForm.ListViewModuleColumnKey;
				item.SubItems.Add(feature.BillingBasis).Name = STLAnalysisForm.ListViewStlBasisColumnKey;
				item.SubItems.Add(feature.BillingEntity).Name = STLAnalysisForm.ListViewStlEntityColumnKey;
				item.SubItems.Add(feature.BillingUnits.ToString());

				if (!String.IsNullOrWhiteSpace(feature.BillingQuery))
				{
					item.BackColor = Color.FromArgb(0, 210, 230, 225);
				}

				ltvFeatureListView.Items.Add(item);
			}

			ltvFeatureListView.EndUpdate();
		}

		LicensedFeature currentFeature;

		LicensedFeature GetSelectedFeature()
		{
			return (ltvFeatureListView.SelectedItems.Count == 1)
				? ltvFeatureListView.SelectedItems[0].Tag as LicensedFeature
				: null;
		}

		void SetStatusBox(string message)
		{
			txbStatusTextBox.Text = message;
			txbStatusTextBox.Visible = !String.IsNullOrWhiteSpace(message);
		}

		void ltvFeatureListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			currentFeature = GetSelectedFeature();

			if (currentFeature != null)
			{
				ckbIsSystemLevelCheckBox.Checked = currentFeature.IsSystemLevel;
				ckbIsCompanyLevelCheckBox.Checked = currentFeature.IsCompanyLevel;
				txbRulesTextBox.Text = System.Xml.Linq.XDocument.Parse(currentFeature.BillingRules).ToString();
				txbSummaryQueryTextBox.Text = currentFeature.BillingQuery;
				txbDetailSqlExpressionTextBox.Text = currentFeature.DetailSqlExpression;
				txbDetailCountExpressionTextBox.Text = currentFeature.DetailCountExpression;
				cbxDataSourceComboBox.Text = currentFeature.DataSource.Location.ToString();
				SetStatusBox("");
			}
		}

		void btnSaveChangesButton_Click(object sender, EventArgs e)
		{
			SetStatusBox("");

			if (currentFeature != null && currentFeature == GetSelectedFeature())
			{
				var error = currentFeature.UpdateFeature(
					ckbIsSystemLevelCheckBox.Checked,
					ckbIsCompanyLevelCheckBox.Checked,
					txbSummaryQueryTextBox.Text.Trim(),
					txbDetailSqlExpressionTextBox.Text.Trim(),
					txbDetailCountExpressionTextBox.Text.Trim(),
					txbRulesTextBox.Text.Trim(),
					cbxDataSourceComboBox.Text);
				SetStatusBox((error == null) ? "Feature updated." : error.Message);
			}
		}
	}
}
