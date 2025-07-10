namespace Enterprise.Customs.EU.GUI
{
	partial class InvoiceLineValuationIndicatorCheckboxesUserControl
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
            this.TabPartyRelationShipCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.TabRestrictionsShipCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.TabSaleConditionsShipCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.TabDisposalAccrualShipCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
            // 
            // TabPartyRelationShipCheckBox
            // 
            this.TabPartyRelationShipCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.TabPartyRelationShipCheckBox, "FilteredInvoiceLines.RelatedIndicator");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).RelatedIndicator)));
            this.TabPartyRelationShipCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.TabPartyRelationShipCheckBox.Name = "TabPartyRelationShipCheckBox";
            this.TabPartyRelationShipCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 17, true);
            this.TabPartyRelationShipCheckBox.TabIndex = 4;
            // 
            // TabRestrictionsShipCheckBox
            // 
            this.TabRestrictionsShipCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.TabRestrictionsShipCheckBox, "FilteredInvoiceLines.RelatedIndicator2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).RelatedIndicator2)));
            this.TabRestrictionsShipCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 43, true);
            this.TabRestrictionsShipCheckBox.Name = "TabRestrictionsShipCheckBox";
            this.TabRestrictionsShipCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 17, true);
            this.TabRestrictionsShipCheckBox.TabIndex = 5;
            // 
            // TabSaleConditionsShipCheckBox
            // 
            this.TabSaleConditionsShipCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.TabSaleConditionsShipCheckBox, "FilteredInvoiceLines.RelatedIndicator3");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).RelatedIndicator3)));
            this.TabSaleConditionsShipCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 83, true);
            this.TabSaleConditionsShipCheckBox.Name = "TabSaleConditionsShipCheckBox";
            this.TabSaleConditionsShipCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(518, 17, true);
            this.TabSaleConditionsShipCheckBox.TabIndex = 6;
            // 
            // TabDisposalAccrualShipCheckBox
            // 
            this.TabDisposalAccrualShipCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.TabDisposalAccrualShipCheckBox, "FilteredInvoiceLines.RelatedIndicator4");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).RelatedIndicator4)));
            this.TabDisposalAccrualShipCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 123, true);
            this.TabDisposalAccrualShipCheckBox.Name = "TabDisposalAccrualShipCheckBox";
            this.TabDisposalAccrualShipCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(749, 17, true);
            this.TabDisposalAccrualShipCheckBox.TabIndex = 7;
            // 
            // InvoiceLineValuationIndicatorCheckboxesUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.TabPartyRelationShipCheckBox);
            this.Controls.Add(this.TabRestrictionsShipCheckBox);
            this.Controls.Add(this.TabSaleConditionsShipCheckBox);
            this.Controls.Add(this.TabDisposalAccrualShipCheckBox);
            this.Name = "InvoiceLineValuationIndicatorCheckboxesUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 300, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZCheckBox TabPartyRelationShipCheckBox;
		protected ZArchitecture.GUI.ZCheckBox TabRestrictionsShipCheckBox;
		protected ZArchitecture.GUI.ZCheckBox TabSaleConditionsShipCheckBox;
		protected ZArchitecture.GUI.ZCheckBox TabDisposalAccrualShipCheckBox;
	}
}
