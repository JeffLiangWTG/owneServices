using System.Windows.Forms;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class VehicleRegistrationAndNationalityUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
			this.VehicleRegistrationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VehicleNationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.VehicleNationalityCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader);
			// 
			// VehicleRegistrationTextBox
			// 
			this.BindingSource.SetBindingMember(this.VehicleRegistrationTextBox, "AMA_VehicleRegistration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_VehicleRegistration)));
			this.VehicleRegistrationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 0, true);
			this.VehicleRegistrationTextBox.Name = "VehicleRegistrationTextBox";
			this.VehicleRegistrationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.VehicleRegistrationTextBox.TabIndex = 0;
			// 
			// VehicleNationalityCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.VehicleNationalityCodeFindBox, "AMA_RN_NKConveyanceNationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).AMA_RN_NKConveyanceNationality)));
			this.VehicleNationalityCodeFindBox.AllowDrop = true;
			this.VehicleNationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 0, true);
			this.VehicleNationalityCodeFindBox.Name = "VehicleNationalityCodeFindBox";
			this.VehicleNationalityCodeFindBox.ParentType = null;
			this.VehicleNationalityCodeFindBox.ShowDescriptionBox = false;
			this.VehicleNationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.VehicleNationalityCodeFindBox.TabIndex = 1;
			this.VehicleNationalityCodeFindBox.Dock = DockStyle.Right;
			// 
			// VehicleRegistrationAndNationalityUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.VehicleNationalityCodeFindBox);
			this.Controls.Add(this.VehicleRegistrationTextBox);
			this.Name = "VehicleRegistrationAndNationalityUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.VehicleNationalityCodeFindBox.ResumeLayout(true);
			this.VehicleNationalityCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZTextBox VehicleRegistrationTextBox;
		public ZArchitecture.GUI.ZCodeFindBox VehicleNationalityCodeFindBox;
	}
}
