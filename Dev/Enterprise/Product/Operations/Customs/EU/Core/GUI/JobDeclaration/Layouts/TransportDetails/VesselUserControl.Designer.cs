namespace Enterprise.Customs.EU.GUI
{
	partial class VesselUserControl
	{

		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.VesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.LloydsIMOTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.VesselCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// VesselCodeFindBox
			// 
			this.VesselCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselCodeFindBox, "JE_VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_VesselName)));
			this.VesselCodeFindBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("FB0D52C6-EAEA-4C61-9E69-757FE75479FD", "Vessel", "Vessel Name");
			this.VesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VesselCodeFindBox.Name = "VesselCodeFindBox";
			this.VesselCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.VesselCodeFindBox.ParentType = null;
			this.VesselCodeFindBox.PreBoundMaxLength = 2;
			this.VesselCodeFindBox.ShowDescriptionBox = false;
			this.VesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.VesselCodeFindBox.TabIndex = 0;
			// 
			// LloydsIMOTextBox
			// 
			this.BindingSource.SetBindingMember(this.LloydsIMOTextBox, "JE_LloydsIMO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_LloydsIMO)));
			this.LloydsIMOTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("F1DE9A8E-2771-4DC7-A45F-C363D565217D", "Lloyds", "[19 08 017 000] Lloyds / IMO Number");
			this.LloydsIMOTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 0, true);
			this.LloydsIMOTextBox.Name = "LloydsIMOTextBox";
			this.LloydsIMOTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 20, true);
			this.LloydsIMOTextBox.TabIndex = 1;
			// 
			// VesselUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.VesselCodeFindBox);
			this.Controls.Add(this.LloydsIMOTextBox);
			this.Name = "VesselUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.VesselCodeFindBox.ResumeLayout(true);
			this.VesselCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox VesselCodeFindBox;
		internal Enterprise.ZArchitecture.ZTextBox LloydsIMOTextBox;

		#endregion
	}
}
