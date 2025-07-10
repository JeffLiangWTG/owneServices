namespace Enterprise.Customs.KR.GUI
{
	partial class OrgUserControl
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
            this.ForeignCarrierGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.DomesticCarrierGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ForeignCarrierGuidFindBox.SuspendLayout();
            this.DomesticCarrierGuidFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // ForeignCarrierGuidFindBox
            // 
            this.ForeignCarrierGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ForeignCarrierGuidFindBox, "JE_OH_ShippingLine");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_OH_ShippingLine)));
            this.ForeignCarrierGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 11, true);
            this.ForeignCarrierGuidFindBox.Name = "ForeignCarrierGuidFindBox";
            this.ForeignCarrierGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.ForeignCarrierGuidFindBox.ParentType = null;
            this.ForeignCarrierGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 17, true);
            this.ForeignCarrierGuidFindBox.TabIndex = 0;
            // 
            // DomesticCarrierGuidFindBox
            // 
            this.DomesticCarrierGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DomesticCarrierGuidFindBox, "DeliveryOrPickupCartageCoPK");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).DeliveryOrPickupCartageCoPK)));
            this.DomesticCarrierGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 32, true);
            this.DomesticCarrierGuidFindBox.Name = "DomesticCarrierGuidFindBox";
            this.DomesticCarrierGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.DomesticCarrierGuidFindBox.ParentType = null;
            this.DomesticCarrierGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 17, true);
            this.DomesticCarrierGuidFindBox.TabIndex = 1;
            // 
            // OrgUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.DomesticCarrierGuidFindBox);
            this.Controls.Add(this.ForeignCarrierGuidFindBox);
            this.Name = "OrgUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 70, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ForeignCarrierGuidFindBox.ResumeLayout(true);
            this.ForeignCarrierGuidFindBox.PerformLayout();
            this.DomesticCarrierGuidFindBox.ResumeLayout(true);
            this.DomesticCarrierGuidFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGuidFindBox ForeignCarrierGuidFindBox;
		private ZArchitecture.GUI.ZGuidFindBox DomesticCarrierGuidFindBox;
	}
}
