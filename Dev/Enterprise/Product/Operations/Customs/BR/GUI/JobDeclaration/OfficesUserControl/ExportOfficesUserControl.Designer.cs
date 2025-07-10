using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	partial class ExportOfficesUserControl
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
			this.ClearanceOfficeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ClearanceIsCustomsEnclosureCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BoardingEnclosureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ClearanceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ClearanceEnclosureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BoardingOfficeIsCustomsEnclosureCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BoardingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BoardingOfficeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ClearanceOfficeDropEdit.SuspendLayout();
			this.BoardingEnclosureDropEdit.SuspendLayout();
			this.ClearanceGroupBox.SuspendLayout();
			this.ClearanceEnclosureDropEdit.SuspendLayout();
			this.BoardingGroupBox.SuspendLayout();
			this.BoardingOfficeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// ClearanceOfficeDropEdit
			// 
			this.ClearanceOfficeDropEdit.AllowDrop = true;
			this.ClearanceOfficeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ClearanceOfficeDropEdit, "JE_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).JE_CustomsOffice)));
			this.ClearanceOfficeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 19, true);
			this.ClearanceOfficeDropEdit.Name = "ClearanceOfficeDropEdit";
			this.ClearanceOfficeDropEdit.PreBoundMaxLength = 7;
			this.ClearanceOfficeDropEdit.ShouldResizeByMaxLength = false;
			this.ClearanceOfficeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.ClearanceOfficeDropEdit.TabIndex = 1;
			// 
			// ClearanceIsCustomsEnclosureCheckBox
			// 
			this.ClearanceIsCustomsEnclosureCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ClearanceIsCustomsEnclosureCheckBox, "ClearanceOfficeIsCustomsEnclosure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).ClearanceOfficeIsCustomsEnclosure)));
			this.ClearanceIsCustomsEnclosureCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ClearanceIsCustomsEnclosureCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 16, true);
			this.ClearanceIsCustomsEnclosureCheckBox.Name = "ClearanceIsCustomsEnclosureCheckBox";
			this.ClearanceIsCustomsEnclosureCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 24, true);
			this.ClearanceIsCustomsEnclosureCheckBox.TabIndex = 2;
			this.ClearanceIsCustomsEnclosureCheckBox.UseVisualStyleBackColor = true;
			// 
			// BoardingEnclosureDropEdit
			// 
			this.BoardingEnclosureDropEdit.AllowDrop = true;
			this.BoardingEnclosureDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BoardingEnclosureDropEdit, "BoardingEnclosureCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).BoardingEnclosureCode)));
			this.BoardingEnclosureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 45, true);
			this.BoardingEnclosureDropEdit.Name = "BoardingEnclosureDropEdit";
			this.BoardingEnclosureDropEdit.PreBoundMaxLength = 7;
			this.BoardingEnclosureDropEdit.ShouldResizeByMaxLength = true;
			this.BoardingEnclosureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.BoardingEnclosureDropEdit.TabIndex = 7;
			// 
			// ClearanceGroupBox
			// 
			this.ClearanceGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ClearanceGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("E2AE4AD0-DCBD-4B8A-82DE-C83D6140A3F5", "Clearance Local");
			this.ClearanceGroupBox.Controls.Add(this.ClearanceEnclosureDropEdit);
			this.ClearanceGroupBox.Controls.Add(this.ClearanceIsCustomsEnclosureCheckBox);
			this.ClearanceGroupBox.Controls.Add(this.ClearanceOfficeDropEdit);
			this.ClearanceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ClearanceGroupBox.Name = "ClearanceGroupBox";
			this.ClearanceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 73, true);
			this.ClearanceGroupBox.TabIndex = 0;
			this.ClearanceGroupBox.TabStop = false;
			// 
			// ClearanceEnclosureDropEdit
			// 
			this.ClearanceEnclosureDropEdit.AllowDrop = true;
			this.ClearanceEnclosureDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ClearanceEnclosureDropEdit, "JE_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).JE_LocationOfGoods)));
			this.ClearanceEnclosureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 45, true);
			this.ClearanceEnclosureDropEdit.Name = "ClearanceEnclosureDropEdit";
			this.ClearanceEnclosureDropEdit.PreBoundMaxLength = 7;
			this.ClearanceEnclosureDropEdit.ShouldResizeByMaxLength = false;
			this.ClearanceEnclosureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.ClearanceEnclosureDropEdit.TabIndex = 3;
			// 
			// BoardingOfficeIsCustomsEnclosureCheckBox
			// 
			this.BoardingOfficeIsCustomsEnclosureCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BoardingOfficeIsCustomsEnclosureCheckBox, "BoardingOfficeIsCustomsEnclosure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).BoardingOfficeIsCustomsEnclosure)));
			this.BoardingOfficeIsCustomsEnclosureCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BoardingOfficeIsCustomsEnclosureCheckBox.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.BoardingOfficeIsCustomsEnclosureCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(375, 16, true);
			this.BoardingOfficeIsCustomsEnclosureCheckBox.Name = "BoardingOfficeIsCustomsEnclosureCheckBox";
			this.BoardingOfficeIsCustomsEnclosureCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 24, true);
			this.BoardingOfficeIsCustomsEnclosureCheckBox.TabIndex = 6;
			this.BoardingOfficeIsCustomsEnclosureCheckBox.UseVisualStyleBackColor = true;
			// 
			// BoardingGroupBox
			// 
			this.BoardingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BoardingGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("47E7B428-3441-496F-B6E0-7121C26CDB82", "Boarding Local");
			this.BoardingGroupBox.Controls.Add(this.BoardingOfficeDropEdit);
			this.BoardingGroupBox.Controls.Add(this.BoardingOfficeIsCustomsEnclosureCheckBox);
			this.BoardingGroupBox.Controls.Add(this.BoardingEnclosureDropEdit);
			this.BoardingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 81, true);
			this.BoardingGroupBox.Name = "BoardingGroupBox";
			this.BoardingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 72, true);
			this.BoardingGroupBox.TabIndex = 4;
			this.BoardingGroupBox.TabStop = false;
			// 
			// BoardingOfficeDropEdit
			// 
			this.BoardingOfficeDropEdit.AllowDrop = true;
			this.BoardingOfficeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BoardingOfficeDropEdit, "BoardingOfficeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).BoardingOfficeCode)));
			this.BoardingOfficeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 19, true);
			this.BoardingOfficeDropEdit.Name = "BoardingOfficeDropEdit";
			this.BoardingOfficeDropEdit.PreBoundMaxLength = 7;
			this.BoardingOfficeDropEdit.ShouldResizeByMaxLength = false;
			this.BoardingOfficeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.BoardingOfficeDropEdit.TabIndex = 5;
			// 
			// OfficesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BoardingGroupBox);
			this.Controls.Add(this.ClearanceGroupBox);
			this.Name = "ExportOfficesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 157, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ClearanceOfficeDropEdit.ResumeLayout(true);
			this.ClearanceOfficeDropEdit.PerformLayout();
			this.BoardingEnclosureDropEdit.ResumeLayout(true);
			this.BoardingEnclosureDropEdit.PerformLayout();
			this.ClearanceGroupBox.ResumeLayout(false);
			this.ClearanceGroupBox.PerformLayout();
			this.ClearanceEnclosureDropEdit.ResumeLayout(true);
			this.ClearanceEnclosureDropEdit.PerformLayout();
			this.BoardingGroupBox.ResumeLayout(false);
			this.BoardingGroupBox.PerformLayout();
			this.BoardingOfficeDropEdit.ResumeLayout(true);
			this.BoardingOfficeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZGroupBox ClearanceGroupBox;
		internal ZDropEdit ClearanceOfficeDropEdit;
		internal ZDropEdit ClearanceEnclosureDropEdit;
		internal ZCheckBox ClearanceIsCustomsEnclosureCheckBox;
		internal ZGroupBox BoardingGroupBox;
		internal ZDropEdit BoardingOfficeDropEdit;
		internal ZDropEdit BoardingEnclosureDropEdit;
		internal ZCheckBox BoardingOfficeIsCustomsEnclosureCheckBox;
	}
}
