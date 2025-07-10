namespace Enterprise.Customs.ES.GUI
{
	partial class AdditionalInformationDetailsUserControl
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
			this.NKCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NKCountryCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.AdditionalInfo);
			// 
			// NKCountryCodeFindBox
			// 
			this.NKCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NKCountryCodeFindBox, "CSI_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.AdditionalInfo)(null)).CSI_RN_NKCountryCode)));
			this.NKCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 59, true);
			this.NKCountryCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.NKCountryCodeFindBox.Name = "NKCountryCodeFindBox";
			this.NKCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.NKCountryCodeFindBox.ParentType = null;
			this.NKCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 20, true);
			this.NKCountryCodeFindBox.TabIndex = 7;
			// 
			// AdditionalInformationDetailsUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NKCountryCodeFindBox);
			this.Name = "AdditionalInformationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 264, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NKCountryCodeFindBox.ResumeLayout(true);
			this.NKCountryCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZArchitecture.GUI.ZCodeFindBox NKCountryCodeFindBox;

		#endregion
	}
}
