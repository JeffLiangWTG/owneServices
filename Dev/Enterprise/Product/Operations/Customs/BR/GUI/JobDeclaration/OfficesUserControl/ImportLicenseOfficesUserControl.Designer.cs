
namespace Enterprise.Customs.BR.GUI
{
	partial class ImportLicenseOfficesUserControl
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
			this.ClearanceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ClearanceOfficeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EntranceLocationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntranceOfficeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ClearanceGroupBox.SuspendLayout();
			this.ClearanceOfficeDropEdit.SuspendLayout();
			this.EntranceLocationGroupBox.SuspendLayout();
			this.EntranceOfficeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// ClearanceGroupBox
			// 
			this.ClearanceGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ClearanceGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("d9836381-f0b8-43dc-a5ad-724f788a5382", "Clearance Local");
			this.ClearanceGroupBox.Controls.Add(this.ClearanceOfficeDropEdit);
			this.ClearanceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ClearanceGroupBox.Name = "ClearanceGroupBox";
			this.ClearanceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 58, true);
			this.ClearanceGroupBox.TabIndex = 0;
			this.ClearanceGroupBox.TabStop = false;
			// 
			// ClearanceOfficeDropEdit
			// 
			this.ClearanceOfficeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClearanceOfficeDropEdit, "JE_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).JE_CustomsOffice)));
			this.ClearanceOfficeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 22, true);
			this.ClearanceOfficeDropEdit.Name = "ClearanceOfficeDropEdit";
			this.ClearanceOfficeDropEdit.ShouldResizeByMaxLength = true;
			this.ClearanceOfficeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 20, true);
			this.ClearanceOfficeDropEdit.TabIndex = 1;
			// 
			// EntranceLocationGroupBox
			// 
			this.EntranceLocationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.EntranceLocationGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("bce51ec3-88a2-4b72-a121-097ce3bc8d26", "Entrance Location");
			this.EntranceLocationGroupBox.Controls.Add(this.EntranceOfficeDropEdit);
			this.EntranceLocationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 65, true);
			this.EntranceLocationGroupBox.Name = "EntranceLocationGroupBox";
			this.EntranceLocationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 53, true);
			this.EntranceLocationGroupBox.TabIndex = 1;
			this.EntranceLocationGroupBox.TabStop = false;
			// 
			// EntranceOfficeDropEdit
			// 
			this.EntranceOfficeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntranceOfficeDropEdit, "EntranceOfficeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).EntranceOfficeCode)));
			this.EntranceOfficeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 22, true);
			this.EntranceOfficeDropEdit.Name = "EntranceOfficeDropEdit";
			this.EntranceOfficeDropEdit.ShouldResizeByMaxLength = true;
			this.EntranceOfficeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 20, true);
			this.EntranceOfficeDropEdit.TabIndex = 0;
			// 
			// ImportLicenseOfficesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EntranceLocationGroupBox);
			this.Controls.Add(this.ClearanceGroupBox);
			this.Name = "ImportLicenseOfficesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(479, 120, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ClearanceGroupBox.ResumeLayout(false);
			this.ClearanceGroupBox.PerformLayout();
			this.ClearanceOfficeDropEdit.ResumeLayout(true);
			this.ClearanceOfficeDropEdit.PerformLayout();
			this.EntranceLocationGroupBox.ResumeLayout(false);
			this.EntranceLocationGroupBox.PerformLayout();
			this.EntranceOfficeDropEdit.ResumeLayout(true);
			this.EntranceOfficeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox ClearanceGroupBox;
		internal ZArchitecture.GUI.ZDropEdit ClearanceOfficeDropEdit;
		internal ZArchitecture.GUI.ZGroupBox EntranceLocationGroupBox;
		internal ZArchitecture.GUI.ZDropEdit EntranceOfficeDropEdit;
	}
}
