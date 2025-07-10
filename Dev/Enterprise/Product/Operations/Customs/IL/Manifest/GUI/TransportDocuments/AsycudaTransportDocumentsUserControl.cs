using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public partial class AsycudaTransportDocumentsUserControl : ZUserControl, IAdditionalTabPage
	{
		public AsycudaTransportDocumentsUserControl()
		{
			InitializeComponent();
			InitializeTransportDocumentGridAndTransportDocumenDetailTabPage();
		}

		void InitializeTransportDocumentGridAndTransportDocumenDetailTabPage()
		{
			var panelLayoutWithGrid = new AsycudaTransportDocumentsWithGridLayout();
			TransportDocumentDetailsLayoutPanel.UpdateLayout(panelLayoutWithGrid);
			var transportDocumentGridControl = (ZUserControl)Activator.CreateInstance(panelLayoutWithGrid.GridUserControlType);
			SplitContainer.Panel1.Controls.Add(transportDocumentGridControl);
			BindingSource.SetBindingMember(transportDocumentGridControl, ".");
			transportDocumentGridControl.Dock = System.Windows.Forms.DockStyle.Fill;
		}

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;
		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("2B1402A1-3991-493B-8FB6-93DAB05A8EB1", "Transport Documents");
		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);
		int IAdditionalTabPage.TabPageSequence => 1;
	}
}
