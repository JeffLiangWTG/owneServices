namespace Enterprise.Customs.EU.EMCS.GUI
{
	partial class OwnerDocAddressUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.OwnerDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OwnerDocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration);
			// 
			// OwnerDocAddressControl
			// 
			this.OwnerDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OwnerDocAddressControl, "OwnerDocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).OwnerDocumentaryAddress)));
			this.OwnerDocAddressControl.BindToOrganisations = "Lookups.OwnersList";
			this.OwnerDocAddressControl.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("0d9c63f8-c0e6-4ca3-ac8d-af0a5521b749", "Goods Owner");
			this.OwnerDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OwnerDocAddressControl.Name = "OwnerDocAddressControl";
			this.OwnerDocAddressControl.ReadOnly = false;
			this.OwnerDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.OwnerDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.OwnerDocAddressControl.TabIndex = 2;
			this.OwnerDocAddressControl.ValidationJustForced = false;
			// 
			// EMCSOwnerDocAddressUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OwnerDocAddressControl);
			this.Name = "EMCSOwnerDocAddressUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OwnerDocAddressControl.ResumeLayout(true);
			this.OwnerDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal MasterFiles.GUI.ZDocAddressControl OwnerDocAddressControl;
	}
}
