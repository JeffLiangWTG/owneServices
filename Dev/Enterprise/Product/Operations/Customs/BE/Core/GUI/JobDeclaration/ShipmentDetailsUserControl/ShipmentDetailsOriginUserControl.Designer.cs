namespace Enterprise.Customs.BE.GUI
{
	partial class ShipmentDetailsOriginUserControl
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
            this.OriginFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.OriginFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BE.Business.Declaration.JobDeclaration);
            // 
            // OriginFindBox
            // 
            this.OriginFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.OriginFindBox, "JE_RL_NKOrigin");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).JE_RL_NKOrigin)));
            this.OriginFindBox.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("CF29CCA4-4B07-44FE-8F35-4BC4E1AA99EF", "[UCC 5/14] Disp.", "[UCC 5/14] Dispatch", "[UCC 5/14] Country of Dispatch");
            this.OriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.OriginFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
            this.OriginFindBox.Name = "OriginFindBox";
            this.OriginFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.OriginFindBox.ParentType = null;
            this.OriginFindBox.PreBoundMaxLength = 5;
            this.OriginFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 16, true);
            this.OriginFindBox.TabIndex = 0;
			//
            // ShipmentDetailsOriginUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.OriginFindBox);
            this.Name = "ShipmentDetailsOriginUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 20, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.OriginFindBox.ResumeLayout(true);
            this.OriginFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox OriginFindBox;
	}
}
