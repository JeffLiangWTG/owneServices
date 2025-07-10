using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Startup.Login
{
	partial class LoginLocationControl
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
		void InitializeComponent()
		{
			this.loginLabel = new Enterprise.ZArchitecture.ZHeaderLabel();
			this.companyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.branchDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.departmentDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.toolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.errorLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.panel1 = new CargoWise.Windows.UI.KPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Startup.LoginLocationBusinessObjectForMainForm);
			// 
			// loginLabel
			// 
			this.loginLabel.CaptionResourceString = CargoWise.Main.Res.GetData("9B834CA0-128D-4D34-A670-9FF96E809F33", "Company, Branch and Department");
			this.loginLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(52)))), ((int)(((byte)(121)))));
			this.loginLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 5, true);
			this.loginLabel.Name = "loginLabel";
			this.loginLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 41, true);
			this.loginLabel.TabIndex = 1;
			// 
			// companyDropEdit
			// 
			this.companyDropEdit.AllowDrop = true;
			this.companyDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.companyDropEdit, "CompanyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Startup.LoginLocationBusinessObjectForMainForm)(null)).CompanyCode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.companyDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.companyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 25, true);
			this.companyDropEdit.Name = "companyDropEdit";
			this.companyDropEdit.PreBoundMaxLength = 3;
			this.companyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.companyDropEdit.TabIndex = 2;
			// 
			// branchDropEdit
			// 
			this.branchDropEdit.AllowDrop = true;
			this.branchDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.branchDropEdit, "BranchCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Startup.LoginLocationBusinessObjectForMainForm)(null)).BranchCode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.branchDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.branchDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 72, true);
			this.branchDropEdit.Name = "branchDropEdit";
			this.branchDropEdit.PreBoundMaxLength = 3;
			this.branchDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.branchDropEdit.TabIndex = 3;
			// 
			// departmentDropEdit
			// 
			this.departmentDropEdit.AllowDrop = true;
			this.departmentDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.departmentDropEdit, "DepartmentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Startup.LoginLocationBusinessObjectForMainForm)(null)).DepartmentCode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.departmentDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.departmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 120, true);
			this.departmentDropEdit.Name = "departmentDropEdit";
			this.departmentDropEdit.PreBoundMaxLength = 3;
			this.departmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.departmentDropEdit.TabIndex = 4;
			// 
			// toolStrip
			// 
			this.toolStrip.AlwaysSelectLast = true;
			this.toolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.toolStrip.BackColor = System.Drawing.Color.Transparent;
			this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 161, true);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 25, true);
			this.toolStrip.TabIndex = 5;
			this.toolStrip.TabStop = true;
			// 
			// errorLabel
			// 
			this.errorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.errorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(0)))), ((int)(((byte)(5)))));
			this.errorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 245, true);
			this.errorLabel.Name = "errorLabel";
			this.errorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 272, true);
			this.errorLabel.TabIndex = 6;
			this.errorLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// PictureBox
			// 
			this.PictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 529, true);
			this.PictureBox.Name = "PictureBox";
			this.PictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 94, true);
			this.PictureBox.TabIndex = 9;
			this.PictureBox.TabStop = false;
			this.PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Controls.Add(this.toolStrip);
			this.panel1.Controls.Add(this.companyDropEdit);
			this.panel1.Controls.Add(this.branchDropEdit);
			this.panel1.Controls.Add(this.departmentDropEdit);
			this.panel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 50, true);
			this.panel1.Name = "panel1";
			this.panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 192, true);
			this.panel1.TabIndex = 10;
			// 
			// LoginLocationControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.PictureBox);
			this.Controls.Add(this.errorLabel);
			this.Controls.Add(this.loginLabel);
			this.Name = "LoginLocationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(237, 626, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.ZHeaderLabel loginLabel;
		internal ZArchitecture.GUI.ZDropEditWithFixedWidth companyDropEdit;
		internal ZArchitecture.GUI.ZDropEditWithFixedWidth branchDropEdit;
		internal ZArchitecture.GUI.ZDropEditWithFixedWidth departmentDropEdit;
		internal ZArchitecture.GUI.ZToolStrip toolStrip;
		internal ZArchitecture.ZLabel errorLabel;
		private Enterprise.ZArchitecture.GUI.ZPictureBox PictureBox;
		private CargoWise.Windows.UI.KPanel panel1;
	}
}
