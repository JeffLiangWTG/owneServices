using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class HouseConsignmentsTabUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.HouseConsignmentsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.HouseConsignmentTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.HouseConsignmentDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TransportDepartureGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransportDepartureDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.HouseConsignmentDetailsDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.GoodsItemsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HouseConsignmentSupportingDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HouseConsignmentSupportingDocumentsTabUserControl = new Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentSupportingDocumentsTabUserControl();
			this.HouseConsignmentAdditionalDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HouseConsignmentAdditionalDocumentsTabUserControl = new Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentAdditionalDocumentsTabUserControl();
			this.HouseConsignmentPreviousDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HouseConsignmentPreviousDocumentsTabUserControl = new Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentPreviousDocumentsTabUserControl();
			this.HouseConsignmentSupplyChainActorsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HouseConsignmentSupplyChainActorsTabUserControl = new Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentSupplyChainActorsTabUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HouseConsignmentsSplitContainer)).BeginInit();
			this.HouseConsignmentsSplitContainer.Panel1.SuspendLayout();
			this.HouseConsignmentsSplitContainer.Panel2.SuspendLayout();
			this.HouseConsignmentsSplitContainer.SuspendLayout();
			this.HouseConsignmentTabControl.SuspendLayout();
			this.HouseConsignmentDetailsTabPage.SuspendLayout();
			this.TransportDepartureGroupBox.SuspendLayout();
			this.HouseConsignmentSupportingDocumentsTabPage.SuspendLayout();
			this.HouseConsignmentSupportingDocumentsTabUserControl.SuspendLayout();
			this.HouseConsignmentAdditionalDocumentsTabPage.SuspendLayout();
			this.HouseConsignmentAdditionalDocumentsTabUserControl.SuspendLayout();
			this.HouseConsignmentPreviousDocumentsTabPage.SuspendLayout();
			this.HouseConsignmentPreviousDocumentsTabUserControl.SuspendLayout();
			this.HouseConsignmentSupplyChainActorsTabPage.SuspendLayout();
			this.HouseConsignmentSupplyChainActorsTabUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.INctsBillCollection<Enterprise.Customs.EU.NCTS.Business.NctsBill>);
			// 
			// HouseConsignmentsSplitContainer
			// 
			this.HouseConsignmentsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseConsignmentsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseConsignmentsSplitContainer.Name = "HouseConsignmentsSplitContainer";
			this.HouseConsignmentsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.HouseConsignmentsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1012, 750, true);
			// 
			// HouseConsignmentsSplitContainer.Panel1
			// 
			this.HouseConsignmentsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(75);
			// 
			// HouseConsignmentsSplitContainer.Panel2
			// 
			this.HouseConsignmentsSplitContainer.Panel2.Controls.Add(this.HouseConsignmentTabControl);
			this.HouseConsignmentsSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(350);
			this.HouseConsignmentsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(75);
			this.HouseConsignmentsSplitContainer.TabIndex = 0;
			// 
			// HouseConsignmentTabControl
			// 
			this.HouseConsignmentTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.HouseConsignmentTabControl.Controls.Add(this.HouseConsignmentDetailsTabPage);
			this.HouseConsignmentTabControl.Controls.Add(this.GoodsItemsTabPage);
			this.HouseConsignmentTabControl.Controls.Add(this.HouseConsignmentSupportingDocumentsTabPage);
			this.HouseConsignmentTabControl.Controls.Add(this.HouseConsignmentAdditionalDocumentsTabPage);
			this.HouseConsignmentTabControl.Controls.Add(this.HouseConsignmentPreviousDocumentsTabPage);
			this.HouseConsignmentTabControl.Controls.Add(this.HouseConsignmentSupplyChainActorsTabPage);
			this.HouseConsignmentTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseConsignmentTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseConsignmentTabControl.Name = "HouseConsignmentTabControl";
			this.HouseConsignmentTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1012, 671, true);
			this.HouseConsignmentTabControl.TabIndex = 0;
			// 
			// HouseConsignmentDetailsTabPage
			// 
			this.HouseConsignmentDetailsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("28F83B5C-FD94-4CCB-9203-9E84FCE9D8E1", "Details");
			this.HouseConsignmentDetailsTabPage.Controls.Add(this.TransportDepartureGroupBox);
			this.HouseConsignmentDetailsTabPage.Controls.Add(this.HouseConsignmentDetailsDynamicLayoutPanel);
			this.HouseConsignmentDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HouseConsignmentDetailsTabPage.Name = "HouseConsignmentDetailsTabPage";
			this.HouseConsignmentDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.HouseConsignmentDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 644, true);
			this.HouseConsignmentDetailsTabPage.TabIndex = 0;
			// 
			// TransportDepartureGroupBox
			// 
			this.TransportDepartureGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("bf4e625d-a72f-431c-915a-9f99219e71d1", "Transport Departure");
			this.TransportDepartureGroupBox.Controls.Add(this.TransportDepartureDynamicLayoutPanel);
			this.TransportDepartureGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 240, true);
			this.TransportDepartureGroupBox.Name = "TransportDepartureGroupBox";
			this.TransportDepartureGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 187, true);
			this.TransportDepartureGroupBox.TabIndex = 1;
			this.TransportDepartureGroupBox.TabStop = false;
			// 
			// TransportDepartureDynamicLayoutPanel
			// 
			this.TransportDepartureDynamicLayoutPanel.AllowDrop = true;
			this.TransportDepartureDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 18, true);
			this.TransportDepartureDynamicLayoutPanel.Name = "TransportDepartureDynamicLayoutPanel";
			this.TransportDepartureDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 140, true);
			this.TransportDepartureDynamicLayoutPanel.TabIndex = 0;
			// 
			// HouseConsignmentDetailsDynamicLayoutPanel
			// 
			this.HouseConsignmentDetailsDynamicLayoutPanel.AllowDrop = true;
			this.HouseConsignmentDetailsDynamicLayoutPanel.AutoScroll = true;
			this.HouseConsignmentDetailsDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.HouseConsignmentDetailsDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HouseConsignmentDetailsDynamicLayoutPanel.Name = "HouseConsignmentDetailsDynamicLayoutPanel";
			this.HouseConsignmentDetailsDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 231, true);
			this.HouseConsignmentDetailsDynamicLayoutPanel.TabIndex = 0;
			// 
			// GoodsItemsTabPage
			// 
			this.GoodsItemsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("B95644D4-67FA-4AE4-A5DB-81DC0862CA15", "Goods Items");
			this.GoodsItemsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GoodsItemsTabPage.Name = "GoodsItemsTabPage";
			this.GoodsItemsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 644, true);
			this.GoodsItemsTabPage.TabIndex = 1;
			// 
			// HouseConsignmentSupportingDocumentsTabPage
			// 
			this.HouseConsignmentSupportingDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("85d82576-d32a-45d4-a5c7-ef6f5e385b8b", "Supporting Documents");
			this.HouseConsignmentSupportingDocumentsTabPage.Controls.Add(this.HouseConsignmentSupportingDocumentsTabUserControl);
			this.HouseConsignmentSupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HouseConsignmentSupportingDocumentsTabPage.Name = "HouseConsignmentSupportingDocumentsTabPage";
			this.HouseConsignmentSupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 644, true);
			this.HouseConsignmentSupportingDocumentsTabPage.TabIndex = 2;
			// 
			// HouseConsignmentSupportingDocumentsTabUserControl
			// 
			this.HouseConsignmentSupportingDocumentsTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseConsignmentSupportingDocumentsTabUserControl, "SupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocumentCollection<Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument>)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).SupportingDocuments)));
			this.HouseConsignmentSupportingDocumentsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseConsignmentSupportingDocumentsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseConsignmentSupportingDocumentsTabUserControl.Name = "HouseConsignmentSupportingDocumentsTabUserControl";
			this.HouseConsignmentSupportingDocumentsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 644, true);
			this.HouseConsignmentSupportingDocumentsTabUserControl.TabIndex = 0;
			// 
			// HouseConsignmentAdditionalDocumentsTabPage
			// 
			this.HouseConsignmentAdditionalDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("16CC9A45-0839-4D5A-90EB-B2AF718D81ED", "Additional Documents");
			this.HouseConsignmentAdditionalDocumentsTabPage.Controls.Add(this.HouseConsignmentAdditionalDocumentsTabUserControl);
			this.HouseConsignmentAdditionalDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HouseConsignmentAdditionalDocumentsTabPage.Name = "HouseConsignmentAdditionalDocumentsTabPage";
			this.HouseConsignmentAdditionalDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 644, true);
			this.HouseConsignmentAdditionalDocumentsTabPage.TabIndex = 5;
			// 
			// HouseConsignmentAdditionalDocumentsTabUserControl
			// 
			this.HouseConsignmentAdditionalDocumentsTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseConsignmentAdditionalDocumentsTabUserControl, "AdditionalDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).AdditionalDocuments)));
			this.HouseConsignmentAdditionalDocumentsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseConsignmentAdditionalDocumentsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseConsignmentAdditionalDocumentsTabUserControl.Name = "HouseConsignmentAdditionalDocumentsTabUserControl";
			this.HouseConsignmentAdditionalDocumentsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 644, true);
			this.HouseConsignmentAdditionalDocumentsTabUserControl.TabIndex = 0;
			// 
			// HouseConsignmentPreviousDocumentsTabPage
			// 
			this.HouseConsignmentPreviousDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("d96ba4fd-3da8-4354-b565-dbb5ef22d36c", "Previous Documents");
			this.HouseConsignmentPreviousDocumentsTabPage.Controls.Add(this.HouseConsignmentPreviousDocumentsTabUserControl);
			this.HouseConsignmentPreviousDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HouseConsignmentPreviousDocumentsTabPage.Name = "HouseConsignmentPreviousDocumentsTabPage";
			this.HouseConsignmentPreviousDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 644, true);
			this.HouseConsignmentPreviousDocumentsTabPage.TabIndex = 3;
			// 
			// HouseConsignmentPreviousDocumentsTabUserControl
			// 
			this.HouseConsignmentPreviousDocumentsTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseConsignmentPreviousDocumentsTabUserControl, "PreviousDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.CommonPreviousDocumentCollection< Enterprise.Customs.EU.NCTS.Business.CommonPreviousDocument>)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).PreviousDocuments)));
			this.HouseConsignmentPreviousDocumentsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseConsignmentPreviousDocumentsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseConsignmentPreviousDocumentsTabUserControl.Name = "HouseConsignmentPreviousDocumentsTabUserControl";
			this.HouseConsignmentPreviousDocumentsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 644, true);
			this.HouseConsignmentPreviousDocumentsTabUserControl.TabIndex = 0;
			// 
			// HouseConsignmentSupplyChainActorsTabPage
			// 
			this.HouseConsignmentSupplyChainActorsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("20fcb72d-3b0f-4a69-985c-bd4ba551bb1a", "Supply Chain Actors");
			this.HouseConsignmentSupplyChainActorsTabPage.Controls.Add(this.HouseConsignmentSupplyChainActorsTabUserControl);
			this.HouseConsignmentSupplyChainActorsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HouseConsignmentSupplyChainActorsTabPage.Name = "HouseConsignmentSupplyChainActorsTabPage";
			this.HouseConsignmentSupplyChainActorsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 644, true);
			this.HouseConsignmentSupplyChainActorsTabPage.TabIndex = 4;
			// 
			// HouseConsignmentSupplyChainActorsTabUserControl
			// 
			this.HouseConsignmentSupplyChainActorsTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseConsignmentSupplyChainActorsTabUserControl, "CusSupplyChainActorReferences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ICusSupplyChainActorReferenceCollection<Enterprise.Customs.EU.NCTS.Business.CusSupplyChainActorReference>)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).CusSupplyChainActorReferences)));
			this.HouseConsignmentSupplyChainActorsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseConsignmentSupplyChainActorsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseConsignmentSupplyChainActorsTabUserControl.Name = "HouseConsignmentSupplyChainActorsTabUserControl";
			this.HouseConsignmentSupplyChainActorsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 644, true);
			this.HouseConsignmentSupplyChainActorsTabUserControl.TabIndex = 0;
			// 
			// HouseConsignmentsTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HouseConsignmentsSplitContainer);
			this.Name = "HouseConsignmentsTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1012, 750, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HouseConsignmentsSplitContainer.Panel1.ResumeLayout(false);
			this.HouseConsignmentsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.HouseConsignmentsSplitContainer)).EndInit();
			this.HouseConsignmentsSplitContainer.ResumeLayout(false);
			this.HouseConsignmentsSplitContainer.PerformLayout();
			this.HouseConsignmentTabControl.ResumeLayout(false);
			this.HouseConsignmentTabControl.PerformLayout();
			this.HouseConsignmentDetailsTabPage.ResumeLayout(false);
			this.HouseConsignmentDetailsTabPage.PerformLayout();
			this.TransportDepartureGroupBox.ResumeLayout(false);
			this.TransportDepartureGroupBox.PerformLayout();
			this.HouseConsignmentSupportingDocumentsTabPage.ResumeLayout(false);
			this.HouseConsignmentSupportingDocumentsTabPage.PerformLayout();
			this.HouseConsignmentSupportingDocumentsTabUserControl.ResumeLayout(true);
			this.HouseConsignmentSupportingDocumentsTabUserControl.PerformLayout();
			this.HouseConsignmentAdditionalDocumentsTabPage.ResumeLayout(false);
			this.HouseConsignmentAdditionalDocumentsTabPage.PerformLayout();
			this.HouseConsignmentAdditionalDocumentsTabUserControl.ResumeLayout(true);
			this.HouseConsignmentAdditionalDocumentsTabUserControl.PerformLayout();
			this.HouseConsignmentPreviousDocumentsTabPage.ResumeLayout(false);
			this.HouseConsignmentPreviousDocumentsTabPage.PerformLayout();
			this.HouseConsignmentPreviousDocumentsTabUserControl.ResumeLayout(true);
			this.HouseConsignmentPreviousDocumentsTabUserControl.PerformLayout();
			this.HouseConsignmentSupplyChainActorsTabPage.ResumeLayout(false);
			this.HouseConsignmentSupplyChainActorsTabPage.PerformLayout();
			this.HouseConsignmentSupplyChainActorsTabUserControl.ResumeLayout(true);
			this.HouseConsignmentSupplyChainActorsTabUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer HouseConsignmentsSplitContainer;
		internal ZArchitecture.GUI.ZTabControl HouseConsignmentTabControl;
		internal ZArchitecture.GUI.ZTabPage HouseConsignmentDetailsTabPage;
		internal ZArchitecture.GUI.DynamicLayoutPanel HouseConsignmentDetailsDynamicLayoutPanel;
		internal Enterprise.ZArchitecture.GUI.ZTabPage GoodsItemsTabPage;
		internal ZArchitecture.GUI.ZTabPage HouseConsignmentPreviousDocumentsTabPage;
		internal HouseConsignmentPreviousDocumentsTabUserControl HouseConsignmentPreviousDocumentsTabUserControl;
		internal ZArchitecture.GUI.ZTabPage HouseConsignmentSupportingDocumentsTabPage;
		internal Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentSupportingDocumentsTabUserControl HouseConsignmentSupportingDocumentsTabUserControl;
		protected internal ZArchitecture.GUI.ZTabPage HouseConsignmentSupplyChainActorsTabPage;
		internal Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentSupplyChainActorsTabUserControl HouseConsignmentSupplyChainActorsTabUserControl;
		internal ZArchitecture.GUI.ZTabPage HouseConsignmentAdditionalDocumentsTabPage;
		internal Enterprise.Customs.EU.NCTS.GUI.HouseConsignmentAdditionalDocumentsTabUserControl HouseConsignmentAdditionalDocumentsTabUserControl;
		internal ZArchitecture.GUI.ZGroupBox TransportDepartureGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel TransportDepartureDynamicLayoutPanel;
	}
}

