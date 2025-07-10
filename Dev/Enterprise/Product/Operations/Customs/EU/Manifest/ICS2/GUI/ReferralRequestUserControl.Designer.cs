namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	partial class ReferralRequestUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.requestHeaderSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.requestHeadersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.requestHeaderDetailsUserControl = new Enterprise.Customs.EU.Manifest.ICS2.GUI.ReferralRequestHeaderDetailsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.requestHeaderSplitContainer)).BeginInit();
			this.requestHeaderSplitContainer.Panel1.SuspendLayout();
			this.requestHeaderSplitContainer.Panel2.SuspendLayout();
			this.requestHeaderSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.requestHeadersGrid)).BeginInit();
			this.requestHeadersGrid.SuspendLayout();
			this.requestHeaderDetailsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader);
			// 
			// RequestHeaderSplitContainer
			// 
			this.requestHeaderSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.requestHeaderSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.requestHeaderSplitContainer.Name = "requestHeaderSplitContainer";
			this.requestHeaderSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// RequestHeaderSplitContainer.Panel1
			// 
			this.requestHeaderSplitContainer.Panel1.Controls.Add(this.requestHeadersGrid);
			// 
			// RequestHeaderSplitContainer.Panel2
			// 
			this.requestHeaderSplitContainer.Panel2.Controls.Add(this.requestHeaderDetailsUserControl);
			this.requestHeaderSplitContainer.TabIndex = 0;
			// 
			// RequestHeadersGrid
			// 
			this.requestHeadersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.requestHeadersGrid, "RequestHeaders");
			this.requestHeadersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "EUS_Identifier";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.ColumnName = "EUS_Type";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "RequestTypeDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.ColumnName = "EUS_ScreeningMethod";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo4.ColumnName = "EUS_MemberState";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo2.ColumnName = "EUS_TransportDocumentType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo5.ColumnName = "EUS_Status";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.requestHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.requestHeadersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.requestHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.requestHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.requestHeadersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.requestHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.requestHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.requestHeadersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.requestHeadersGrid.GridId = "84a9dcd0-7a55-477b-b40c-a223c35c41b8";
			this.requestHeadersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.requestHeadersGrid.LayoutKey = "RequestHeadersGrid";
			this.requestHeadersGrid.Name = "RequestHeadersGrid";
			this.requestHeadersGrid.TabIndex = 1;
			this.requestHeadersGrid.ReadOnly = true;
			// 
			// RequestHeaderDetailsUserControl
			// 
			this.requestHeaderDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.requestHeaderDetailsUserControl, "RequestHeaders");
			this.requestHeaderDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.requestHeaderDetailsUserControl.Name = "EUICS2ReferralRequestHeaderDetailsUserControl";
			// 
			// EUICS2ReferralRequestUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.requestHeaderSplitContainer);
			this.Name = "EUICS2ReferralRequestUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.requestHeaderSplitContainer.Panel1.ResumeLayout(false);
			this.requestHeaderSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.requestHeaderSplitContainer)).EndInit();
			this.requestHeaderSplitContainer.ResumeLayout(false);
			this.requestHeaderSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.requestHeadersGrid)).EndInit();
			this.requestHeadersGrid.ResumeLayout(false);
			this.requestHeadersGrid.PerformLayout();
			this.requestHeaderDetailsUserControl.ResumeLayout(false);
			this.requestHeaderDetailsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		CargoWise.Windows.UI.KSplitContainer requestHeaderSplitContainer;
		Enterprise.ZArchitecture.ZGrid requestHeadersGrid;
		Enterprise.Customs.EU.Manifest.ICS2.GUI.ReferralRequestHeaderDetailsUserControl requestHeaderDetailsUserControl;
	}
}
