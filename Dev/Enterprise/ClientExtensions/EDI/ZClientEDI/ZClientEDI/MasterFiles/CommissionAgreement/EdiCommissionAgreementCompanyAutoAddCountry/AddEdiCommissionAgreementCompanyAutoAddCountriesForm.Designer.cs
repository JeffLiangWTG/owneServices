namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	partial class AddEdiCommissionAgreementCompanyAutoAddCountriesForm
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
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.addButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.itemsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.itemsGrid)).BeginInit();
			this.itemsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 339, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.MasterFiles.Business.AddEdiCommissionAgreementCompanyAutoAddCountriesAction);
			// 
			// addButton
			// 
			this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.addButton.CaptionResourceString = ZClientEDI.Res.GetData("5b1c4ff8-f985-48ed-a1c2-f23bf6e7c9b2", "Add");
			this.addButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 312, true);
			this.addButton.Name = "addButton";
			this.addButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 23, true);
			this.addButton.TabIndex = 1;
			this.addButton.Click += new System.EventHandler(this.AddButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cancelButton.CaptionResourceString = ZClientEDI.Res.GetData("989d68a2-a988-4492-9928-d1d4486560c1", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 312, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 23, true);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// itemsGrid
			// 
			this.itemsGrid.AllowNavigation = false;
			this.itemsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.itemsGrid, "Items");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.AddEdiCommissionAgreementCompanyAutoAddCountriesAction)(null)).Items)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.AddEdiCommissionAgreementCompanyAutoAddCountryItem)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.AddEdiCommissionAgreementCompanyAutoAddCountriesAction)(null)).Items)).SyncRoot)).CountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Client.EDI.MasterFiles.Business.AddEdiCommissionAgreementCompanyAutoAddCountryItem)(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.AddEdiCommissionAgreementCompanyAutoAddCountriesAction)(null)).Items)).SyncRoot)).Country.RN_DescMultilingual)));
			this.itemsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CountryCode";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "Country+RN_DescMultilingual";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.itemsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.itemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.itemsGrid.CopySelectedRowsAllowed = true;
			this.itemsGrid.GridId = "67333d00-2668-4316-bb56-1809be911d05";
			this.itemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.itemsGrid.LayoutKey = "itemsGrid";
			this.itemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.itemsGrid.Name = "itemsGrid";
			this.itemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 300, true);
			this.itemsGrid.TabIndex = 0;
			// 
			// AddEdiCommissionAgreementCompanyAutoAddCountriesForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = ZClientEDI.Res.GetData("8e07f268-3345-4a65-a090-f3222e15610c", "Add Countries/Regions");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 363, true);
			this.Controls.Add(this.itemsGrid);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.addButton);
			this.DataSourceType = typeof(Enterprise.Client.EDI.MasterFiles.Business.AddEdiCommissionAgreementCompanyAutoAddCountriesAction);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 400, true);
			this.Name = "AddEdiCommissionAgreementCompanyAutoAddCountriesForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.addButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.itemsGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.itemsGrid)).EndInit();
			this.itemsGrid.ResumeLayout(false);
			this.itemsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton addButton;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.ZGrid itemsGrid;
	}
}