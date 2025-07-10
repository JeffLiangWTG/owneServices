using System;
using System.Windows.Forms;
using Enterprise.Client.JAS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.GUI
{
	partial class JASDataExporterUserControl : ZUserControl
	{
		public JASDataExporterUserControl()
		{
			InitializeComponent();
			SetDataSourceBinding(EmailPanel, "IsVisibleForBinding", JASDataExporterBizO.Schema.IsEmailDeliveryMethod);
			SetDataSourceBinding(DirectoryPanel, "IsVisibleForBinding", JASDataExporterBizO.Schema.IsDirectoryDeliveryMethod);
			SetDataSourceBinding(EmailGroupGuidFindBox, "IsVisibleForBinding", JASDataExporterBizO.Schema.IsGroupEmailRecipient);
			SetDataSourceBinding(EmailTextBox, "IsVisibleForBinding", JASDataExporterBizO.Schema.IsIndividualEmailRecipient);
		}

		#region Event Handlers

		void BrowseButton_Click(object sender, EventArgs e)
		{
			if (ZFormModaliser.ShowCommonDialogWithoutDispose(ExportDirectoryBrowserDialog) == DialogResult.OK)
			{
				ExportDirectoryTextBox.Text = ExportDirectoryBrowserDialog.UnmappedSelectedPath;
				ExportDirectoryTextBox.Focus();
			}
		}

		#endregion
	}
}
