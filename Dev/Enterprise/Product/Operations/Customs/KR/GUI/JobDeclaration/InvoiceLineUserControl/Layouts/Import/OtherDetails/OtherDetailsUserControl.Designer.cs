
namespace Enterprise.Customs.KR.GUI
{
	partial class OtherDetailsUserControl
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
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.LeftPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.InspectionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.InspectionPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            this.AgencyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.AgencyPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            this.MaterialTaxGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.MaterialTaxPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            this.ImmediateDeliveryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ImmediateDeliveryGrid = new Enterprise.ZArchitecture.ZGrid();
            this.HSExtensionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.HSExtensionGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.LeftPanel.SuspendLayout();
            this.InspectionGroupBox.SuspendLayout();
            this.AgencyGroupBox.SuspendLayout();
            this.MaterialTaxGroupBox.SuspendLayout();
            this.ImmediateDeliveryGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImmediateDeliveryGrid)).BeginInit();
            this.ImmediateDeliveryGrid.SuspendLayout();
            this.HSExtensionGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HSExtensionGrid)).BeginInit();
            this.HSExtensionGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // LeftPanel
            // 
            this.LeftPanel.Controls.Add(this.InspectionGroupBox);
            this.LeftPanel.Controls.Add(this.AgencyGroupBox);
            this.LeftPanel.Controls.Add(this.MaterialTaxGroupBox);
            this.LeftPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.LeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.LeftPanel.Name = "LeftPanel";
            this.LeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 278, true);
            this.LeftPanel.TabIndex = 0;
            // 
            // InspectionGroupBox
            // 
            this.InspectionGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("72106358-bd65-4227-8619-960e37214859", "Inspection");
            this.InspectionGroupBox.Controls.Add(this.InspectionPanel);
            this.InspectionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InspectionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 194, true);
            this.InspectionGroupBox.Name = "InspectionGroupBox";
            this.InspectionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 84, true);
            this.InspectionGroupBox.TabIndex = 2;
            this.InspectionGroupBox.TabStop = false;
            // 
            // InspectionPanel
            // 
            this.InspectionPanel.AllowDrop = true;
            this.InspectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InspectionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
            this.InspectionPanel.Name = "InspectionPanel";
            this.InspectionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 59, true);
            this.InspectionPanel.TabIndex = 2;
            // 
            // AgencyGroupBox
            // 
            this.AgencyGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("0c9371ee-6427-492d-bfd1-e29d418eab42", "Post Clearance Agency");
            this.AgencyGroupBox.Controls.Add(this.AgencyPanel);
            this.AgencyGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.AgencyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 85, true);
            this.AgencyGroupBox.Name = "AgencyGroupBox";
            this.AgencyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 109, true);
            this.AgencyGroupBox.TabIndex = 1;
            this.AgencyGroupBox.TabStop = false;
            // 
            // AgencyPanel
            // 
            this.AgencyPanel.AllowDrop = true;
            this.AgencyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AgencyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
            this.AgencyPanel.Name = "AgencyPanel";
            this.AgencyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 84, true);
            this.AgencyPanel.TabIndex = 2;
            // 
            // MaterialTaxGroupBox
            // 
            this.MaterialTaxGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("0c247049-13e6-4a3e-a8d6-8db797520601", "Material Tax");
            this.MaterialTaxGroupBox.Controls.Add(this.MaterialTaxPanel);
            this.MaterialTaxGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.MaterialTaxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.MaterialTaxGroupBox.Name = "MaterialTaxGroupBox";
            this.MaterialTaxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 85, true);
            this.MaterialTaxGroupBox.TabIndex = 0;
            this.MaterialTaxGroupBox.TabStop = false;
            // 
            // MaterialTaxPanel
            // 
            this.MaterialTaxPanel.AllowDrop = true;
            this.MaterialTaxPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MaterialTaxPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
            this.MaterialTaxPanel.Name = "MaterialTaxPanel";
            this.MaterialTaxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 60, true);
            this.MaterialTaxPanel.TabIndex = 1;
            // 
            // ImmediateDeliveryGroupBox
            // 
            this.ImmediateDeliveryGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("ab5ee6bf-8400-46d7-a703-081284df8253", "Immediate Delivery");
            this.ImmediateDeliveryGroupBox.Controls.Add(this.ImmediateDeliveryGrid);
            this.ImmediateDeliveryGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.ImmediateDeliveryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(437, 0, true);
            this.ImmediateDeliveryGroupBox.Name = "ImmediateDeliveryGroupBox";
            this.ImmediateDeliveryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 278, true);
            this.ImmediateDeliveryGroupBox.TabIndex = 1;
            this.ImmediateDeliveryGroupBox.TabStop = false;
            // 
            // ImmediateDeliveryGrid
            // 
            this.ImmediateDeliveryGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.ImmediateDeliveryGrid, "FilteredInvoiceLines.ImmediateDeliveries");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ImmediateDeliveries)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.ImmediateDelivery)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ImmediateDeliveries)).SyncRoot)).CY_Order)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.ImmediateDelivery)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ImmediateDeliveries)).SyncRoot)).CY_Data)));
            this.ImmediateDeliveryGrid.CaptionVisible = false;
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "CY_Order";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.IsReadOnly = true;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
            zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
            this.ImmediateDeliveryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.ImmediateDeliveryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.ImmediateDeliveryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ImmediateDeliveryGrid.GridId = "f6084e49-fb4d-4904-8ed2-10037e5c395b";
            this.ImmediateDeliveryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ImmediateDeliveryGrid.LayoutKey = "ImmediateDeliveryGrid";
            this.ImmediateDeliveryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.ImmediateDeliveryGrid.Name = "ImmediateDeliveryGrid";
            this.ImmediateDeliveryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 261, true);
            this.ImmediateDeliveryGrid.TabIndex = 0;
            // 
            // HSExtensionGroupBox
            // 
            this.HSExtensionGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("a3e553ca-0465-4dab-afbf-59974f4024be", "HS Extension Codes");
            this.HSExtensionGroupBox.Controls.Add(this.HSExtensionGrid);
            this.HSExtensionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HSExtensionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(686, 0, true);
            this.HSExtensionGroupBox.Name = "HSExtensionGroupBox";
            this.HSExtensionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 278, true);
            this.HSExtensionGroupBox.TabIndex = 2;
            this.HSExtensionGroupBox.TabStop = false;
            // 
            // HSExtensionGrid
            // 
            this.HSExtensionGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.HSExtensionGrid, "FilteredInvoiceLines.HSExtensionCodeCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).HSExtensionCodeCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.HSExtensionCode)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).HSExtensionCodeCollection)).SyncRoot)).ClassificationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.HSExtensionCode)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).HSExtensionCodeCollection)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.HSExtensionCode)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).HSExtensionCodeCollection)).SyncRoot)).CategoryDescription)));
			this.HSExtensionGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo2.ColumnName = "ClassificationType";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo3.ColumnName = "CategoryDescription";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            this.HSExtensionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.HSExtensionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.HSExtensionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.HSExtensionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HSExtensionGrid.GridId = "74de96e5-d813-483d-9c53-129eded93719";
            this.HSExtensionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.HSExtensionGrid.LayoutKey = "HSExtensionGridGrid";
            this.HSExtensionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.HSExtensionGrid.Name = "HSExtensionGrid";
            this.HSExtensionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 261, true);
            this.HSExtensionGrid.TabIndex = 0;
            // 
            // OtherDetailsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.HSExtensionGroupBox);
            this.Controls.Add(this.ImmediateDeliveryGroupBox);
            this.Controls.Add(this.LeftPanel);
            this.Name = "OtherDetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1053, 278, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.LeftPanel.ResumeLayout(false);
            this.LeftPanel.PerformLayout();
            this.InspectionGroupBox.ResumeLayout(false);
            this.InspectionGroupBox.PerformLayout();
            this.AgencyGroupBox.ResumeLayout(false);
            this.AgencyGroupBox.PerformLayout();
            this.MaterialTaxGroupBox.ResumeLayout(false);
            this.MaterialTaxGroupBox.PerformLayout();
            this.ImmediateDeliveryGroupBox.ResumeLayout(false);
            this.ImmediateDeliveryGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImmediateDeliveryGrid)).EndInit();
            this.ImmediateDeliveryGrid.ResumeLayout(false);
            this.ImmediateDeliveryGrid.PerformLayout();
            this.HSExtensionGroupBox.ResumeLayout(false);
            this.HSExtensionGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HSExtensionGrid)).EndInit();
            this.HSExtensionGrid.ResumeLayout(false);
            this.HSExtensionGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel LeftPanel;
		private ZArchitecture.GUI.ZGroupBox AgencyGroupBox;
		private ZArchitecture.GUI.ZGroupBox MaterialTaxGroupBox;
		private ZArchitecture.GUI.ZGroupBox InspectionGroupBox;
		private ZArchitecture.GUI.ZGroupBox ImmediateDeliveryGroupBox;
		private ZArchitecture.ZGrid ImmediateDeliveryGrid;
		private ZArchitecture.GUI.DynamicLayoutPanel MaterialTaxPanel;
		private ZArchitecture.GUI.DynamicLayoutPanel InspectionPanel;
		private ZArchitecture.GUI.DynamicLayoutPanel AgencyPanel;
		private ZArchitecture.GUI.ZGroupBox HSExtensionGroupBox;
		private ZArchitecture.ZGrid HSExtensionGrid;
	}
}
