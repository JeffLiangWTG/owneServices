namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class NEXDOCJobDocAddressUserControl
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
			this.ApprovalNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NJobDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NJobDocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.AUJobDocAddress);
			// 
			// ApprovalNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ApprovalNumberTextBox, "ApprovalNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.AUJobDocAddress)(null)).ApprovalNumber)));
			this.ApprovalNumberTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("9d3bb587-0e99-4e5d-b8f6-55ea875a6728", "Approval Number");
			this.ApprovalNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 190, true);
			this.ApprovalNumberTextBox.Name = "ApprovalNumberTextBox";
			this.ApprovalNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 17, true);
			this.ApprovalNumberTextBox.TabIndex = 2;
			// 
			// NJobDocAddressControl
			// 
			this.NJobDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NJobDocAddressControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.AU.Declaration.Business.AUJobDocAddress)(null)))));
			this.NJobDocAddressControl.BindToOrganisations = "Lookups.AllOrganisations";
			this.NJobDocAddressControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("83d36e42-d789-4f5d-9530-3541a723a2b3", "Address");
			this.NJobDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NJobDocAddressControl.Name = "NJobDocAddressControl";
			this.NJobDocAddressControl.ReadOnly = false;
			this.NJobDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.NJobDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.NJobDocAddressControl.TabIndex = 1;
			this.NJobDocAddressControl.ValidationJustForced = false;
			// 
			// NEXDOCJobDocAddressUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ApprovalNumberTextBox);
			this.Controls.Add(this.NJobDocAddressControl);
			this.Name = "NEXDOCJobDocAddressUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 219, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NJobDocAddressControl.ResumeLayout(true);
			this.NJobDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.ZTextBox ApprovalNumberTextBox;
		private MasterFiles.GUI.ZDocAddressControl NJobDocAddressControl;
	}
}
