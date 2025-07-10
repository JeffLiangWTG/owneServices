using System.Runtime.InteropServices;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI
{
	partial class H7ManifestUserControl
	{
		void InitializeComponent()
		{
			this.CSPDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SupervisingOfficeAddressControl = new ZArchitecture.GUI.ZAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CSPDropEdit.SuspendLayout();
			this.SupervisingOfficeAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.H7.Business.AsycudaManifestHeader);
			// 
			// CSPDropEdit
			// 
			this.CSPDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CSPDropEdit, "CSP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.H7.Business.AsycudaManifestHeader)(null)).CSP)));
			this.CSPDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 11, true);
			this.CSPDropEdit.Name = "CSPDropEdit";
			this.CSPDropEdit.PreBoundMaxLength = 5;
			this.CSPDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 15, true);
			this.CSPDropEdit.TabIndex = 1;
			// 
			// SupervisingOfficeAddressControl
			// 
			this.SupervisingOfficeAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupervisingOfficeAddressControl, "SupervisingOfficeAddressPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.H7.Business.AsycudaManifestHeader)(null)).SupervisingOfficeAddressPK)));
			this.SupervisingOfficeAddressControl.Name = "SupervisingOfficeAddressControl";
			this.SupervisingOfficeAddressControl.ShowAddress = false;
			this.SupervisingOfficeAddressControl.TabIndex = 2;
			// 
			// H7ManifestUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CSPDropEdit);
			this.Controls.Add(this.SupervisingOfficeAddressControl);
			this.Name = "H7ManifestUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 40, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CSPDropEdit.ResumeLayout(true);
			this.CSPDropEdit.PerformLayout();
			this.SupervisingOfficeAddressControl.ResumeLayout(true);
			this.SupervisingOfficeAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZDropEdit CSPDropEdit;
		internal ZArchitecture.GUI.ZAddressControl SupervisingOfficeAddressControl;
	}
}
