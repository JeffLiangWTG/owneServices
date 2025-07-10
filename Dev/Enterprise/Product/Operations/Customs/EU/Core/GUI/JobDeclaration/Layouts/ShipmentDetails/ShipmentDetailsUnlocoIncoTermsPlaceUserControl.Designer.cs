namespace Enterprise.Customs.EU.GUI
{
	partial class ShipmentDetailsUnlocoIncoTermsPlaceUserControl
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
            this.ShipmentIncoTermPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AgreedPlaceCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.AgreedPlaceCodeFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
            // 
            // ShipmentIncoTermPlaceTextBox
            // 
            this.BindingSource.SetBindingMember(this.ShipmentIncoTermPlaceTextBox, "JE_ShipmentIncoTermPlace");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_ShipmentIncoTermPlace)));
            this.ShipmentIncoTermPlaceTextBox.CaptionResourceString = null;
            this.ShipmentIncoTermPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 0, true);
            this.ShipmentIncoTermPlaceTextBox.Name = "ShipmentIncoTermPlaceTextBox";
            this.ShipmentIncoTermPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 20, true);
            this.ShipmentIncoTermPlaceTextBox.TabIndex = 19;
            // 
            // AgreedPlaceCodeFindBox
            // 
            this.AgreedPlaceCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.AgreedPlaceCodeFindBox, "EUD_AgreedPlaceCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).EUD_AgreedPlaceCode)));
            this.AgreedPlaceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.AgreedPlaceCodeFindBox.Name = "AgreedPlaceCodeFindBox";
            this.AgreedPlaceCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.AgreedPlaceCodeFindBox.ParentType = null;
            this.AgreedPlaceCodeFindBox.PreBoundMaxLength = 1;
            this.AgreedPlaceCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 20, true);
            this.AgreedPlaceCodeFindBox.TabIndex = 18;
            // 
            // ShipmentDetailsUnlocoIncoTermsPlaceUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ShipmentIncoTermPlaceTextBox);
            this.Controls.Add(this.AgreedPlaceCodeFindBox);
            this.Name = "ShipmentDetailsUnlocoIncoTermsPlaceUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.AgreedPlaceCodeFindBox.ResumeLayout(true);
            this.AgreedPlaceCodeFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox ShipmentIncoTermPlaceTextBox;
		internal ZArchitecture.GUI.ZCodeFindBox AgreedPlaceCodeFindBox;
	}
}
