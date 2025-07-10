namespace Enterprise.Customs.EU.GUI
{
	partial class ValueIndicatorsUserControl
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
            this.PartyRelationShipCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.RestrictionsShipCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.SaleConditionsShipCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.DisposalAccrualShipCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
            // 
            // PartyRelationShipCheckBox
            // 
            this.BindingSource.SetBindingMember(this.PartyRelationShipCheckBox, "Invoices.RelatedIndicator");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).RelatedIndicator)));
            this.PartyRelationShipCheckBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("49421b4a-a842-4c9c-b93b-6b28ae263c0f", "Party relationship, whether there is price influence or not");
            this.PartyRelationShipCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.PartyRelationShipCheckBox.Name = "PartyRelationShipCheckBox";
            this.PartyRelationShipCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 24, true);
            this.PartyRelationShipCheckBox.TabIndex = 0;
            // 
            // RestrictionsShipCheckBox
            // 
            this.BindingSource.SetBindingMember(this.RestrictionsShipCheckBox, "Invoices.RelatedIndicator2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).RelatedIndicator2)));
            this.RestrictionsShipCheckBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("9df30e5b-ac03-42e4-8e58-df46abd218e4", "Restrictions as to the disposal or use of the goods by the buyer in accordance wi" +
        "th Article 70(3)(a) of the Code");
            this.RestrictionsShipCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 33, true);
            this.RestrictionsShipCheckBox.Name = "RestrictionsShipCheckBox";
            this.RestrictionsShipCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 24, true);
            this.RestrictionsShipCheckBox.TabIndex = 1;
            // 
            // SaleConditionsShipCheckBox
            // 
            this.BindingSource.SetBindingMember(this.SaleConditionsShipCheckBox, "Invoices.RelatedIndicator3");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).RelatedIndicator3)));
            this.SaleConditionsShipCheckBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("d06d6304-f6d6-472d-8e35-131c845cd9a2", "Sale or price is subject to some condition or consideration in accordance with Ar" +
        "ticle 70(3)(b) of the Code");
            this.SaleConditionsShipCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 63, true);
            this.SaleConditionsShipCheckBox.Name = "SaleConditionsShipCheckBox";
            this.SaleConditionsShipCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 24, true);
            this.SaleConditionsShipCheckBox.TabIndex = 2;
            // 
            // DisposalAccrualShipCheckBox
            // 
            this.BindingSource.SetBindingMember(this.DisposalAccrualShipCheckBox, "Invoices.RelatedIndicator4");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).RelatedIndicator4)));
            this.DisposalAccrualShipCheckBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("cfb578c3-b212-47a8-a2fc-665c168188c1", "The sale is subject to an arrangement under which part of the proceeds of any sub" +
        "sequent resale, disposal or use accrues directly or indirectly to the seller");
            this.DisposalAccrualShipCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 93, true);
            this.DisposalAccrualShipCheckBox.Name = "DisposalAccrualShipCheckBox";
            this.DisposalAccrualShipCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 24, true);
            this.DisposalAccrualShipCheckBox.TabIndex = 3;
            // 
            // ValueIndicatorsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.PartyRelationShipCheckBox);
            this.Controls.Add(this.RestrictionsShipCheckBox);
            this.Controls.Add(this.SaleConditionsShipCheckBox);
            this.Controls.Add(this.DisposalAccrualShipCheckBox);
            this.Name = "ValueIndicatorsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 300, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZCheckBox PartyRelationShipCheckBox;
		protected ZArchitecture.GUI.ZCheckBox RestrictionsShipCheckBox;
		protected ZArchitecture.GUI.ZCheckBox SaleConditionsShipCheckBox;
		protected ZArchitecture.GUI.ZCheckBox DisposalAccrualShipCheckBox;
	}
}
