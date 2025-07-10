using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	public partial class SupportingDocumentUserControl : ZUserControl
	{
		public SupportingDocumentUserControl()
		{
			InitializeComponent();

			InitializeGridAndDetailTabPage();
		}

		protected virtual ZString UserBindingMemberCore() => "SupportingDocuments";

		void InitializeGridAndDetailTabPage()
		{
			var panelLayoutWithGrid = new SupportingDocumentsWithGridLayout();
			BindingSource.SetBindingMember(SupportingDocumentLayoutPanel, UserBindingMemberCore());
			SupportingDocumentLayoutPanel.UpdateLayout(panelLayoutWithGrid);
			var supportingDocumentGridControl = (ZUserControl)Activator.CreateInstance(panelLayoutWithGrid.GridUserControlType);
			SplitContainer.Panel1.Controls.Add(supportingDocumentGridControl);
			BindingSource.SetBindingMember(supportingDocumentGridControl, UserBindingMemberCore());
			supportingDocumentGridControl.Dock = System.Windows.Forms.DockStyle.Fill;

			var supportingDocumentMetaDataGridControl = (ZUserControl)Activator.CreateInstance(typeof(SupportingDocumentMetaDataGridControl));
			MetaDataTabPage.Controls.Add(supportingDocumentMetaDataGridControl);
			BindingSource.SetBindingMember(supportingDocumentMetaDataGridControl, UserBindingMemberCore());
			supportingDocumentMetaDataGridControl.Dock = System.Windows.Forms.DockStyle.Fill;
		}
	}
}
