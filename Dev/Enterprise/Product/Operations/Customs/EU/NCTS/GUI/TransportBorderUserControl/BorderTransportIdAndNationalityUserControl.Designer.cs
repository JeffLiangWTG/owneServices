namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class BorderTransportIdAndNationalityUserControl
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
			this.BorderTransportIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.BorderTransportNationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.VesselCodeFindBox.SuspendLayout();
			this.BorderTransportNationalityCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader);
			// 
			// BorderTransportIdTextBox
			// 
			this.BindingSource.SetBindingMember(this.BorderTransportIdTextBox, "BM_TOLCarrierID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).BM_TOLCarrierID)));
			this.BorderTransportIdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BorderTransportIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BorderTransportIdTextBox.Name = "BorderTransportIdTextBox";
			this.BorderTransportIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.BorderTransportIdTextBox.TabIndex = 1;
			// 
			// VesselCodeFindBox
			// 
			this.VesselCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselCodeFindBox, "BM_TOLCarrierID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).BM_TOLCarrierID)));
			this.VesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VesselCodeFindBox.Name = "VesselCodeFindBox";
			this.VesselCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.VesselCodeFindBox.ParentType = null;
			this.VesselCodeFindBox.ShowDescriptionBox = false;
			this.VesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.VesselCodeFindBox.TabIndex = 1;
			// 
			// BorderTransportNationalityCodeFindBox
			// 
			this.BorderTransportNationalityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BorderTransportNationalityCodeFindBox, "BM_RN_NKTOLCarrierNationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).BM_RN_NKTOLCarrierNationality)));
			this.BorderTransportNationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 0, true);
			this.BorderTransportNationalityCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.BorderTransportNationalityCodeFindBox.Name = "BorderTransportNationalityCodeFindBox";
			this.BorderTransportNationalityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BorderTransportNationalityCodeFindBox.ParentType = null;
			this.BorderTransportNationalityCodeFindBox.PreBoundMaxLength = 2;
			this.BorderTransportNationalityCodeFindBox.ShowDescriptionBox = false;
			this.BorderTransportNationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.BorderTransportNationalityCodeFindBox.TabIndex = 2;
			// 
			// BorderTransportIdAndNationalityUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BorderTransportIdTextBox);
			this.Controls.Add(this.VesselCodeFindBox);
			this.Controls.Add(this.BorderTransportNationalityCodeFindBox);
			this.Name = "BorderTransportIdAndNationalityUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.VesselCodeFindBox.ResumeLayout(true);
			this.VesselCodeFindBox.PerformLayout();
			this.BorderTransportNationalityCodeFindBox.ResumeLayout(true);
			this.BorderTransportNationalityCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox BorderTransportIdTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox VesselCodeFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox BorderTransportNationalityCodeFindBox;
	}
}
