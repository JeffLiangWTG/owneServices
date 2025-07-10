
namespace Enterprise.Customs.BR.GUI
{
	partial class ExportOutOfEnclosureUserControl
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
			this.ClearanceLocalGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsHomeDispatchCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.InvolvedPartyDocAddress = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.BoardingLocalGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BoardingLocalDocAddress = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ClearanceLocalGroupBox.SuspendLayout();
			this.InvolvedPartyDocAddress.SuspendLayout();
			this.BoardingLocalGroupBox.SuspendLayout();
			this.BoardingLocalDocAddress.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// ClearanceLocalGroupBox
			// 
			this.ClearanceLocalGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ClearanceLocalGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("1bff94b6-5c62-440a-8664-af517070d5ef", "Clearance Local");
			this.ClearanceLocalGroupBox.Controls.Add(this.IsHomeDispatchCheckBox);
			this.ClearanceLocalGroupBox.Controls.Add(this.InvolvedPartyDocAddress);
			this.ClearanceLocalGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClearanceLocalGroupBox.Name = "ClearanceLocalGroupBox";
			this.ClearanceLocalGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 69, true);
			this.ClearanceLocalGroupBox.TabIndex = 0;
			this.ClearanceLocalGroupBox.TabStop = false;
			// 
			// IsHomeDispatchCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsHomeDispatchCheckBox, "ClearanceOfficeIsHomeDispatch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).ClearanceOfficeIsHomeDispatch)));
			this.IsHomeDispatchCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 14, true);
			this.IsHomeDispatchCheckBox.Name = "IsHomeDispatchCheckBox";
			this.IsHomeDispatchCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 26, true);
			this.IsHomeDispatchCheckBox.TabIndex = 0;
			this.IsHomeDispatchCheckBox.UseVisualStyleBackColor = true;
			// 
			// InvolvedPartyDocAddress
			// 
			this.InvolvedPartyDocAddress.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvolvedPartyDocAddress, "ClearanceLocalInvolvedParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).ClearanceLocalInvolvedParty)));
			this.InvolvedPartyDocAddress.BindToOrganisations = "Lookups.InvolvedPartyAddressOrganisations";
			this.InvolvedPartyDocAddress.CaptionResourceString = null;
			this.InvolvedPartyDocAddress.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.InvolvedPartyDocAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 43, true);
			this.InvolvedPartyDocAddress.Name = "InvolvedPartyDocAddress";
			this.InvolvedPartyDocAddress.ReadOnly = false;
			this.InvolvedPartyDocAddress.SingleLineNoGroupBoxPanelWidth = 320;
			this.InvolvedPartyDocAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.InvolvedPartyDocAddress.TabIndex = 1;
			this.InvolvedPartyDocAddress.ValidationJustForced = false;
			// 
			// BoardingLocalGroupBox
			// 
			this.BoardingLocalGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BoardingLocalGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("3a0f6c5f-57a3-4d11-949a-07fd64f244a8", "Boarding Local");
			this.BoardingLocalGroupBox.Controls.Add(this.BoardingLocalDocAddress);
			this.BoardingLocalGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 75, true);
			this.BoardingLocalGroupBox.Name = "BoardingLocalGroupBox";
			this.BoardingLocalGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 53, true);
			this.BoardingLocalGroupBox.TabIndex = 1;
			this.BoardingLocalGroupBox.TabStop = false;
			// 
			// BoardingLocalDocAddress
			// 
			this.BoardingLocalDocAddress.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BoardingLocalDocAddress, "BoardingLocalAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).BoardingLocalAddress)));
			this.BoardingLocalDocAddress.BindToOrganisations = "Lookups.BoardingLocalAddressOrganisations";
			this.BoardingLocalDocAddress.CaptionResourceString = null;
			this.BoardingLocalDocAddress.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.BoardingLocalDocAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 19, true);
			this.BoardingLocalDocAddress.Name = "BoardingLocalDocAddress";
			this.BoardingLocalDocAddress.ReadOnly = false;
			this.BoardingLocalDocAddress.SingleLineNoGroupBoxPanelWidth = 320;
			this.BoardingLocalDocAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.BoardingLocalDocAddress.TabIndex = 0;
			this.BoardingLocalDocAddress.ValidationJustForced = false;
			// 
			// ExportOutOfEnclosureUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BoardingLocalGroupBox);
			this.Controls.Add(this.ClearanceLocalGroupBox);
			this.Name = "ExportOutOfEnclosureUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 130, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ClearanceLocalGroupBox.ResumeLayout(false);
			this.ClearanceLocalGroupBox.PerformLayout();
			this.InvolvedPartyDocAddress.ResumeLayout(true);
			this.InvolvedPartyDocAddress.PerformLayout();
			this.BoardingLocalGroupBox.ResumeLayout(false);
			this.BoardingLocalGroupBox.PerformLayout();
			this.BoardingLocalDocAddress.ResumeLayout(true);
			this.BoardingLocalDocAddress.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox ClearanceLocalGroupBox;
		internal MasterFiles.GUI.ZDocAddressControl InvolvedPartyDocAddress;
		internal ZArchitecture.GUI.ZCheckBox IsHomeDispatchCheckBox;
		internal ZArchitecture.GUI.ZGroupBox BoardingLocalGroupBox;
		internal MasterFiles.GUI.ZDocAddressControl BoardingLocalDocAddress;
	}
}
