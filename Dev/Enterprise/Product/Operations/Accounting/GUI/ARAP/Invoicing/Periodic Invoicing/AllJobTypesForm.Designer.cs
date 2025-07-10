using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Accounting.GUI
{
	partial class AllJobTypesForm : ZChildForm		
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JobTypeCheckedListBox = new Enterprise.ZArchitecture.GUI.ZCheckedListBox();
			this.btnSelect = new Enterprise.ZArchitecture.GUI.ZButton();
			this.btnCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.btnSelectAll = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 485, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.JobTypePicker);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5ccfb244-1f31-47a2-9c1e-7748a6b31ce3", "Job Types");
			this.zGroupBox1.Controls.Add(this.JobTypeCheckedListBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 445, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// JobTypeCheckedListBox
			// 
			this.JobTypeCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.JobTypeCheckedListBox.BindingItems = null;
			this.BindingSource.SetBindingMember(this.JobTypeCheckedListBox, "JobTypeList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZBoolDescriptionPairList)(((Enterprise.Accounting.Business.JobTypePicker)(null)).JobTypeList)));
			this.JobTypeCheckedListBox.CheckOnClick = true;
			this.JobTypeCheckedListBox.FormattingEnabled = true;
			this.JobTypeCheckedListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 27, true);
			this.JobTypeCheckedListBox.Name = "JobTypeCheckedListBox";
			this.JobTypeCheckedListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 409, true);
			this.JobTypeCheckedListBox.TabIndex = 0;
			this.JobTypeCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.JobTypeCheckedListBox_ItemCheck);
			// 
			// btnSelect
			// 
			this.btnSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSelect.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6d706688-7fc8-4d09-8fbb-4336aba30790", "OK");
			this.btnSelect.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(182, 454, true);
			this.btnSelect.Name = "btnSelect";
			this.btnSelect.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 26, true);
			this.btnSelect.TabIndex = 2;
			this.btnSelect.UseVisualStyleBackColor = true;
			this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("56312864-63fe-49c1-b766-6f868b6e5c09", "Cancel");
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(261, 454, true);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 26, true);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnSelectAll
			// 
			this.btnSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSelectAll.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("50bdbe00-e627-4dc9-a9e6-1b6cca560bde", "Select All");
			this.btnSelectAll.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 454, true);
			this.btnSelectAll.Name = "btnSelectAll";
			this.btnSelectAll.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 26, true);
			this.btnSelectAll.TabIndex = 1;
			this.btnSelectAll.UseVisualStyleBackColor = true;
			this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
			// 
			// AllJobTypesForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ecb9dc6a-a7fe-4e05-b3fa-80cc5c3e7c94", "Job Types");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 509, true);
			this.Controls.Add(this.btnSelectAll);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnSelect);
			this.Controls.Add(this.zGroupBox1);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.JobTypePicker);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "AllJobTypesForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.btnSelect, 0);
			this.Controls.SetChildIndex(this.btnCancel, 0);
			this.Controls.SetChildIndex(this.btnSelectAll, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.GUI.ZButton btnSelect;
		private ZArchitecture.GUI.ZButton btnCancel;
		private ZArchitecture.GUI.ZCheckedListBox JobTypeCheckedListBox;
		private ZButton btnSelectAll;
	}
}