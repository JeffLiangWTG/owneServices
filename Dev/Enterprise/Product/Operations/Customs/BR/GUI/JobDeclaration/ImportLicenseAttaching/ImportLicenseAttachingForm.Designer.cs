namespace Enterprise.Customs.BR.GUI
{
	partial class ImportLicenseAttachingForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AttachButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LicensesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LicensesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.LicensesGrid)).BeginInit();
			this.LicensesGrid.SuspendLayout();
			this.LicensesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 409, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(877, 41, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.ImportLicenseAttachingObjectParent);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("ImportLicenseAttachingForm|D10D791D-EDAF-4628-A76D-4D60DE1C41E3", "Cancel");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(791, 373, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.OnCloseButton_Click);
			// 
			// AttachButton
			// 
			this.AttachButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AttachButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("ImportLicenseAttachingForm|A5BAF66E-F0F2-40B4-809E-DB19608F828C", "Attach");
			this.AttachButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(708, 373, true);
			this.AttachButton.Name = "AttachButton";
			this.AttachButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AttachButton.TabIndex = 2;
			this.AttachButton.ToolTipCaption = null;
			this.AttachButton.UseVisualStyleBackColor = true;
			this.AttachButton.Click += new System.EventHandler(this.OnAttachButton_Click);
			// 
			// LicensesGrid
			// 
			this.LicensesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LicensesGrid, "ImportLicenses");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.ImportLicenseAttachingObjectParent)(null)).ImportLicenses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.ImportLicenseAttachingObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.ImportLicenseAttachingObjectParent)(null)).ImportLicenses)).SyncRoot)).ShouldAttach)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ImportLicenseAttachingObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.ImportLicenseAttachingObjectParent)(null)).ImportLicenses)).SyncRoot)).ImportLicenseEntryDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ImportLicenseAttachingObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.ImportLicenseAttachingObjectParent)(null)).ImportLicenses)).SyncRoot)).ImportLicenseEntryMRN)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.BR.Business.ImportLicenseAttachingObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.ImportLicenseAttachingObjectParent)(null)).ImportLicenses)).SyncRoot)).ImportLicenseEntryRegistrationDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ImportLicenseAttachingObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.ImportLicenseAttachingObjectParent)(null)).ImportLicenses)).SyncRoot)).ImportLicenseEntryStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.ImportLicenseAttachingObject)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.ImportLicenseAttachingObjectParent)(null)).ImportLicenses)).SyncRoot)).FeeType)));
			this.LicensesGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "ShouldAttach";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zTextBoxColumnStyleInfo1.ColumnName = "ImportLicenseEntryDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "ImportLicenseEntryMRN";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.ColumnName = "ImportLicenseEntryRegistrationDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo3.ColumnName = "ImportLicenseEntryStatusDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.ColumnName = "FeeType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.LicensesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.LicensesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LicensesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LicensesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.LicensesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LicensesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LicensesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LicensesGrid.GridId = "811669e2-679c-4e4e-a2b8-3b1eb7aabbb3";
			this.LicensesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LicensesGrid.LayoutKey = "LicensesGrid";
			this.LicensesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LicensesGrid.Name = "LicensesGrid";
			this.LicensesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(855, 340, true);
			this.LicensesGrid.TabIndex = 1;
			// 
			// LicensesGroupBox
			// 
			this.LicensesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LicensesGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("ImportLicenseAttachingForm|1AF1519B-5D78-47D6-8933-53A279DCFA32", "Licenses to be Attached");
			this.LicensesGroupBox.Controls.Add(this.LicensesGrid);
			this.LicensesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.LicensesGroupBox.Name = "LicensesGroupBox";
			this.LicensesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 359, true);
			this.LicensesGroupBox.TabIndex = 4;
			this.LicensesGroupBox.TabStop = false;
			// 
			// ImportLicenseAttachingForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("ImportLicenseAttachingForm|8217831E-ED31-4805-AD3E-2173C89C8588", "Attaching Licenses");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(877, 450, true);
			this.Controls.Add(this.LicensesGroupBox);
			this.Controls.Add(this.AttachButton);
			this.Controls.Add(this.CloseButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.BR.Business";
			this.DataSourceType = typeof(Enterprise.Customs.BR.Business.ImportLicenseAttachingObjectParent);
			this.DataSourceTypeName = "Enterprise.Customs.BR.Business.ImportLicenseAttachingObjectParent";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 296, true);
			this.Name = "ImportLicenseAttachingForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.AttachButton, 0);
			this.Controls.SetChildIndex(this.LicensesGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.LicensesGrid)).EndInit();
			this.LicensesGrid.ResumeLayout(false);
			this.LicensesGrid.PerformLayout();
			this.LicensesGroupBox.ResumeLayout(false);
			this.LicensesGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZButton CloseButton;
		internal ZArchitecture.GUI.ZButton AttachButton;
		internal ZArchitecture.ZGrid LicensesGrid;
		internal ZArchitecture.GUI.ZGroupBox LicensesGroupBox;
	}
}
