namespace Enterprise.Customs.KR.GUI
{
	partial class CarnetDeclarationUserControl
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
            this.SupplierOrganisationControl = new Enterprise.Customs.GUI.ZOrganisationControlWithMiscellaneous();
            this.ImporterOrganisationControl = new Enterprise.Customs.GUI.ZOrganisationControlWithMiscellaneous();
            this.CarnetDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CarnetDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            this.CustomsAreaDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CustomsAreaDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            this.MiscellaneousOptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.MiscellaneousOptionsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SupplierOrganisationControl.SuspendLayout();
            this.ImporterOrganisationControl.SuspendLayout();
            this.CarnetDetailsGroupBox.SuspendLayout();
            this.CustomsAreaDetailsGroupBox.SuspendLayout();
            this.MiscellaneousOptionsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // SupplierOrganisationControl
            // 
            this.SupplierOrganisationControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SupplierOrganisationControl, "JE_OH_Supplier");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_OH_Supplier)));
            this.SupplierOrganisationControl.BindToMiscellaneousFields = "JE_SupplierMiscFields";
            this.SupplierOrganisationControl.BindToOrganisations = "Lookups.SuppliersList";
            this.SupplierOrganisationControl.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("58f462bd-280f-491a-a9b7-ceaffe8ed26f", "Supplier");
            this.SupplierOrganisationControl.Captions = new string[] {
        "Supplier"};
            this.SupplierOrganisationControl.IsCaptionOverridden = false;
            this.SupplierOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.SupplierOrganisationControl.Name = "SupplierOrganisationControl";
            this.SupplierOrganisationControl.OrgAddressFormatter = null;
            this.SupplierOrganisationControl.PopupCaption = "";
            this.SupplierOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 151, true);
            this.SupplierOrganisationControl.TabIndex = 0;
            // 
            // ImporterOrganisationControl
            // 
            this.ImporterOrganisationControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ImporterOrganisationControl, "JE_OH_Importer");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_OH_Importer)));
            this.ImporterOrganisationControl.BindToMiscellaneousFields = "JE_ImporterMiscFields";
            this.ImporterOrganisationControl.BindToOrganisations = "Lookups.ImportersList";
            this.ImporterOrganisationControl.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("a3749417-9820-4aee-b370-e276dad57c22", "Importer");
            this.ImporterOrganisationControl.Captions = new string[] {
        "Importer"};
            this.ImporterOrganisationControl.IsCaptionOverridden = false;
            this.ImporterOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 161, true);
            this.ImporterOrganisationControl.Name = "ImporterOrganisationControl";
            this.ImporterOrganisationControl.OrgAddressFormatter = null;
            this.ImporterOrganisationControl.PopupCaption = "";
            this.ImporterOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 151, true);
            this.ImporterOrganisationControl.TabIndex = 1;
            // 
            // CarnetDetailsGroupBox
            // 
            this.CarnetDetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("8a51ba77-78ef-4302-95ae-a2dc820f8c09", "Carnet Details");
            this.CarnetDetailsGroupBox.Controls.Add(this.CarnetDetailsPanel);
            this.CarnetDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 3, true);
            this.CarnetDetailsGroupBox.Name = "CarnetDetailsGroupBox";
            this.CarnetDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 199, true);
            this.CarnetDetailsGroupBox.TabIndex = 2;
            this.CarnetDetailsGroupBox.TabStop = false;
            // 
            // CarnetDetailsPanel
            // 
            this.CarnetDetailsPanel.AllowDrop = true;
            this.CarnetDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CarnetDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.CarnetDetailsPanel.Name = "CarnetDetailsPanel";
            this.CarnetDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 182, true);
            this.CarnetDetailsPanel.TabIndex = 0;
            // 
            // CustomsAreaDetailsGroupBox
            // 
            this.CustomsAreaDetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("0079feeb-3053-46b5-9e9d-12ec29c84fbc", "Customs Area Details");
            this.CustomsAreaDetailsGroupBox.Controls.Add(this.CustomsAreaDetailsPanel);
            this.CustomsAreaDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 205, true);
            this.CustomsAreaDetailsGroupBox.Name = "CustomsAreaDetailsGroupBox";
            this.CustomsAreaDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 72, true);
            this.CustomsAreaDetailsGroupBox.TabIndex = 3;
            this.CustomsAreaDetailsGroupBox.TabStop = false;
            // 
            // CustomsAreaDetailsPanel
            // 
            this.CustomsAreaDetailsPanel.AllowDrop = true;
            this.CustomsAreaDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CustomsAreaDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.CustomsAreaDetailsPanel.Name = "CustomsAreaDetailsPanel";
            this.CustomsAreaDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 55, true);
            this.CustomsAreaDetailsPanel.TabIndex = 1;
            // 
            // MiscellaneousOptionsGroupBox
            // 
            this.MiscellaneousOptionsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("d0d64f13-ff6c-44a0-96a3-9ed8dae85b71", "Miscellaneous Options");
            this.MiscellaneousOptionsGroupBox.Controls.Add(this.MiscellaneousOptionsPanel);
            this.MiscellaneousOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 283, true);
            this.MiscellaneousOptionsGroupBox.Name = "MiscellaneousOptionsGroupBox";
            this.MiscellaneousOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 56, true);
            this.MiscellaneousOptionsGroupBox.TabIndex = 4;
            this.MiscellaneousOptionsGroupBox.TabStop = false;
            // 
            // MiscellaneousOptionsPanel
            // 
            this.MiscellaneousOptionsPanel.AllowDrop = true;
            this.MiscellaneousOptionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MiscellaneousOptionsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.MiscellaneousOptionsPanel.Name = "MiscellaneousOptionsPanel";
            this.MiscellaneousOptionsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 39, true);
            this.MiscellaneousOptionsPanel.TabIndex = 1;
            // 
            // CarnetUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.MiscellaneousOptionsGroupBox);
            this.Controls.Add(this.CustomsAreaDetailsGroupBox);
            this.Controls.Add(this.CarnetDetailsGroupBox);
            this.Controls.Add(this.ImporterOrganisationControl);
            this.Controls.Add(this.SupplierOrganisationControl);
            this.Name = "CarnetUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1372, 686, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.SupplierOrganisationControl.ResumeLayout(true);
            this.SupplierOrganisationControl.PerformLayout();
            this.ImporterOrganisationControl.ResumeLayout(true);
            this.ImporterOrganisationControl.PerformLayout();
            this.CarnetDetailsGroupBox.ResumeLayout(false);
            this.CarnetDetailsGroupBox.PerformLayout();
            this.CustomsAreaDetailsGroupBox.ResumeLayout(false);
            this.CustomsAreaDetailsGroupBox.PerformLayout();
            this.MiscellaneousOptionsGroupBox.ResumeLayout(false);
            this.MiscellaneousOptionsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private Customs.GUI.ZOrganisationControlWithMiscellaneous SupplierOrganisationControl;
		private Customs.GUI.ZOrganisationControlWithMiscellaneous ImporterOrganisationControl;
		private ZArchitecture.GUI.ZGroupBox CarnetDetailsGroupBox;
		private ZArchitecture.GUI.ZGroupBox CustomsAreaDetailsGroupBox;
		private ZArchitecture.GUI.ZGroupBox MiscellaneousOptionsGroupBox;
		private ZArchitecture.GUI.DynamicLayoutPanel CarnetDetailsPanel;
		private ZArchitecture.GUI.DynamicLayoutPanel CustomsAreaDetailsPanel;
		private ZArchitecture.GUI.DynamicLayoutPanel MiscellaneousOptionsPanel;
	}
}
