using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Accounting.GUI
{
	partial class JobTypeSelectionControl : ZUserControl		
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
			this.JobTypeCheckedListBox = new Enterprise.ZArchitecture.GUI.ZCheckedListBox();
			this.btnSelect = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.JobTypePicker);
			// 
			// JobTypeCheckedListBox
			// 
			this.JobTypeCheckedListBox.BindingItems = null;
			this.BindingSource.SetBindingMember(this.JobTypeCheckedListBox, "SelectedJobTypeList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZBoolDescriptionPairList)(((Enterprise.Accounting.Business.JobTypePicker)(null)).SelectedJobTypeList)));
			this.JobTypeCheckedListBox.CheckOnClick = true;
			this.JobTypeCheckedListBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobTypeCheckedListBox.HorizontalScrollbar = true;
			this.JobTypeCheckedListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobTypeCheckedListBox.Name = "JobTypeCheckedListBox";
			this.JobTypeCheckedListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 64, true);
			this.JobTypeCheckedListBox.TabIndex = 14;
			// 
			// btnSelect
			// 
			this.btnSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSelect.AutoSize = true;
			this.btnSelect.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4b577747-75b9-4ad1-b36e-5080e2f26df9", "Add Job Types");
			this.btnSelect.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 68, true);
			this.btnSelect.Name = "btnSelect";
			this.btnSelect.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 25, true);
			this.btnSelect.TabIndex = 15;
			this.btnSelect.UseVisualStyleBackColor = true;
			this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
			// 
			// zPanel1
			// 
			this.zPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zPanel1.Controls.Add(this.JobTypeCheckedListBox);
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 64, true);
			this.zPanel1.TabIndex = 16;
			// 
			// JobTypeSelectionControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.btnSelect);
			this.DoubleBuffered = true;
			this.Name = "JobTypeSelectionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 93, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZCheckedListBox JobTypeCheckedListBox;
		private ZButton btnSelect;
		private ZPanel zPanel1;
	}
}
