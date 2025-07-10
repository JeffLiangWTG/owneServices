namespace Enterprise.Customs.EU.GUI
{
	partial class InvoiceLineValuationIndicatorDropEditsUserControl
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
            this.OverrideValuationIndicatorsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.PartyRelationShipDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.RestrictionsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.SaleConditionsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.DisposalAccrualDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.OverrideValuationIndicatorsGroupBox.SuspendLayout();
            this.PartyRelationShipDropEdit.SuspendLayout();
            this.RestrictionsDropEdit.SuspendLayout();
            this.SaleConditionsDropEdit.SuspendLayout();
            this.DisposalAccrualDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
            // 
            // OverrideValuationIndicatorsGroupBox
            // 
            this.OverrideValuationIndicatorsGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("a3755f0e-0e9f-4681-a16f-c853d4be63ba", "Override Valuation Indicators");
            this.OverrideValuationIndicatorsGroupBox.Controls.Add(this.PartyRelationShipDropEdit);
            this.OverrideValuationIndicatorsGroupBox.Controls.Add(this.RestrictionsDropEdit);
            this.OverrideValuationIndicatorsGroupBox.Controls.Add(this.SaleConditionsDropEdit);
            this.OverrideValuationIndicatorsGroupBox.Controls.Add(this.DisposalAccrualDropEdit);
            this.OverrideValuationIndicatorsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.OverrideValuationIndicatorsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.OverrideValuationIndicatorsGroupBox.Name = "OverrideValuationIndicatorsGroupBox";
            this.OverrideValuationIndicatorsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 190, true);
            this.OverrideValuationIndicatorsGroupBox.TabIndex = 0;
            this.OverrideValuationIndicatorsGroupBox.TabStop = false;
            // 
            // PartyRelationShipDropEdit
            // 
            this.PartyRelationShipDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PartyRelationShipDropEdit, "FilteredInvoiceLines.JI_RelatedIndicator");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_RelatedIndicator)));
            this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.PartyRelationShipDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
            this.PartyRelationShipDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 33, true);
            this.PartyRelationShipDropEdit.Name = "PartyRelationShipDropEdit";
            this.PartyRelationShipDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
            this.PartyRelationShipDropEdit.TabIndex = 1;
            // 
            // RestrictionsDropEdit
            // 
            this.RestrictionsDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.RestrictionsDropEdit, "FilteredInvoiceLines.ZG_RelatedIndicator2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZG_RelatedIndicator2)));
            this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.RestrictionsDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
            this.RestrictionsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 73, true);
            this.RestrictionsDropEdit.Name = "RestrictionsDropEdit";
            this.RestrictionsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
            this.RestrictionsDropEdit.TabIndex = 2;
            // 
            // SaleConditionsDropEdit
            // 
            this.SaleConditionsDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SaleConditionsDropEdit, "FilteredInvoiceLines.ZG_RelatedIndicator3");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZG_RelatedIndicator3)));
            this.SaleConditionsDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("d06d6304-f6d6-472d-8e35-131c845cd9a2", "Sale or price is subject to some condition or consideration in accordance with Ar" +
        "ticle 70(3)(b) of the Code");
            this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.SaleConditionsDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
            this.SaleConditionsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 113, true);
            this.SaleConditionsDropEdit.Name = "SaleConditionsDropEdit";
            this.SaleConditionsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
            this.SaleConditionsDropEdit.TabIndex = 3;
            // 
            // DisposalAccrualDropEdit
            // 
            this.DisposalAccrualDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DisposalAccrualDropEdit, "FilteredInvoiceLines.ZG_RelatedIndicator4");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZG_RelatedIndicator4)));
            this.DisposalAccrualDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("cfb578c3-b212-47a8-a2fc-665c168188c1", "The sale is subject to an arrangement under which part of the proceeds of any sub" +
        "sequent resale, disposal or use accrues directly or indirectly to the seller");
            this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.DisposalAccrualDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
            this.DisposalAccrualDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 153, true);
            this.DisposalAccrualDropEdit.Name = "DisposalAccrualDropEdit";
            this.DisposalAccrualDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
            this.DisposalAccrualDropEdit.TabIndex = 4;
            // 
            // InvoiceLineValuationIndicatorDropEditsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.OverrideValuationIndicatorsGroupBox);
            this.Name = "InvoiceLineValuationIndicatorDropEditsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 300, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.PartyRelationShipDropEdit.ResumeLayout(true);
            this.PartyRelationShipDropEdit.PerformLayout();
            this.RestrictionsDropEdit.ResumeLayout(true);
            this.RestrictionsDropEdit.PerformLayout();
            this.SaleConditionsDropEdit.ResumeLayout(true);
            this.SaleConditionsDropEdit.PerformLayout();
            this.DisposalAccrualDropEdit.ResumeLayout(true);
            this.DisposalAccrualDropEdit.PerformLayout();
            this.OverrideValuationIndicatorsGroupBox.ResumeLayout(false);
            this.OverrideValuationIndicatorsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZGroupBox OverrideValuationIndicatorsGroupBox;
		protected ZArchitecture.GUI.ZDropEdit PartyRelationShipDropEdit;
		protected ZArchitecture.GUI.ZDropEdit RestrictionsDropEdit;
		protected ZArchitecture.GUI.ZDropEdit SaleConditionsDropEdit;
		protected ZArchitecture.GUI.ZDropEdit DisposalAccrualDropEdit;
	}
}
