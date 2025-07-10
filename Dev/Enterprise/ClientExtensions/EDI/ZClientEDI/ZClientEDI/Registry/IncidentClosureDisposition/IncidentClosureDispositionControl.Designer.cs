using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class IncidentClosureDispositionControl
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
			this.kSplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.kSplitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.Grid1 = new Enterprise.Client.EDI.Registry.GUI.IncidentClosureDispositionGridControl();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.Grid2 = new Enterprise.Client.EDI.Registry.GUI.IncidentClosureDispositionGridControl();
			this.kSplitContainer3 = new CargoWise.Windows.UI.KSplitContainer();
			this.zGroupBox3 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.Grid3 = new Enterprise.Client.EDI.Registry.GUI.IncidentClosureDispositionGridControl();
			this.zGroupBox4 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.Grid4 = new Enterprise.Client.EDI.Registry.GUI.IncidentClosureDispositionGridControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).BeginInit();
			this.kSplitContainer1.Panel1.SuspendLayout();
			this.kSplitContainer1.Panel2.SuspendLayout();
			this.kSplitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer2)).BeginInit();
			this.kSplitContainer2.Panel1.SuspendLayout();
			this.kSplitContainer2.Panel2.SuspendLayout();
			this.kSplitContainer2.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.Grid1.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.Grid2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer3)).BeginInit();
			this.kSplitContainer3.Panel1.SuspendLayout();
			this.kSplitContainer3.Panel2.SuspendLayout();
			this.kSplitContainer3.SuspendLayout();
			this.zGroupBox3.SuspendLayout();
			this.Grid3.SuspendLayout();
			this.zGroupBox4.SuspendLayout();
			this.Grid4.SuspendLayout();
			this.SuspendLayout();
			// 
			// kSplitContainer1
			// 
			this.kSplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.kSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kSplitContainer1.Name = "kSplitContainer1";
			this.kSplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// kSplitContainer1.Panel1
			// 
			this.kSplitContainer1.Panel1.Controls.Add(this.kSplitContainer2);
			// 
			// kSplitContainer1.Panel2
			// 
			this.kSplitContainer1.Panel2.Controls.Add(this.kSplitContainer3);
			this.kSplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 520, true);
			this.kSplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(222);
			this.kSplitContainer1.SplitterWidth = 10;
			this.kSplitContainer1.TabIndex = 0;
			// 
			// kSplitContainer2
			// 
			this.kSplitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 6, true);
			this.kSplitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kSplitContainer2.Name = "kSplitContainer2";
			this.kSplitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// kSplitContainer2.Panel1
			// 
			this.kSplitContainer2.Panel1.Controls.Add(this.zGroupBox1);
			// 
			// kSplitContainer2.Panel2
			// 
			this.kSplitContainer2.Panel2.Controls.Add(this.zGroupBox2);
			this.kSplitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 204, true);
			this.kSplitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(85);
			this.kSplitContainer2.SplitterWidth = 10;
			this.kSplitContainer2.TabIndex = 0;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.Grid1);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 85, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// Grid1
			// 
			this.Grid1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Grid1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			//CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(You_must_set_DataSourceType_on_the_control_binder);
			this.Grid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Grid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.Grid1.Name = "Grid1";
			this.Grid1.ReadOnly = false;
			this.Grid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 70, true);
			this.Grid1.TabIndex = 0;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Controls.Add(this.Grid2);
			this.zGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 114, true);
			this.zGroupBox2.TabIndex = 0;
			this.zGroupBox2.TabStop = false;
			// 
			// Grid2
			// 
			this.Grid2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Grid2, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			//CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(You_must_set_DataSourceType_on_the_control_binder);
			this.Grid2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Grid2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.Grid2.Name = "Grid2";
			this.Grid2.ReadOnly = false;
			this.Grid2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 99, true);
			this.Grid2.TabIndex = 0;
			// 
			// kSplitContainer3
			// 
			this.kSplitContainer3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 11, true);
			this.kSplitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kSplitContainer3.Name = "kSplitContainer3";
			this.kSplitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// kSplitContainer3.Panel1
			// 
			this.kSplitContainer3.Panel1.Controls.Add(this.zGroupBox3);
			// 
			// kSplitContainer3.Panel2
			// 
			this.kSplitContainer3.Panel2.Controls.Add(this.zGroupBox4);
			this.kSplitContainer3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 262, true);
			this.kSplitContainer3.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(116);
			this.kSplitContainer3.SplitterWidth = 10;
			this.kSplitContainer3.TabIndex = 0;
			// 
			// zGroupBox3
			// 
			this.zGroupBox3.Controls.Add(this.Grid3);
			this.zGroupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox3.Name = "zGroupBox3";
			this.zGroupBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 116, true);
			this.zGroupBox3.TabIndex = 0;
			this.zGroupBox3.TabStop = false;
			// 
			// Grid3
			// 
			this.Grid3.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Grid3, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			//CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(You_must_set_DataSourceType_on_the_control_binder);
			this.Grid3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Grid3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.Grid3.Name = "Grid3";
			this.Grid3.ReadOnly = false;
			this.Grid3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 101, true);
			this.Grid3.TabIndex = 0;
			// 
			// zGroupBox4
			// 
			this.zGroupBox4.Controls.Add(this.Grid4);
			this.zGroupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox4.Name = "zGroupBox4";
			this.zGroupBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 140, true);
			this.zGroupBox4.TabIndex = 0;
			this.zGroupBox4.TabStop = false;
			// 
			// Grid4
			// 
			this.Grid4.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Grid4, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			//CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(You_must_set_DataSourceType_on_the_control_binder);
			this.Grid4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Grid4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.Grid4.Name = "Grid4";
			this.Grid4.ReadOnly = false;
			this.Grid4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 125, true);
			this.Grid4.TabIndex = 0;
			// 
			// IncidentClosureDispositionControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.kSplitContainer1);
			this.Name = "IncidentClosureDispositionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(939, 530, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.kSplitContainer1.Panel1.ResumeLayout(false);
			this.kSplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).EndInit();
			this.kSplitContainer1.ResumeLayout(false);
			this.kSplitContainer1.PerformLayout();
			this.kSplitContainer2.Panel1.ResumeLayout(false);
			this.kSplitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer2)).EndInit();
			this.kSplitContainer2.ResumeLayout(false);
			this.kSplitContainer2.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.Grid1.ResumeLayout(true);
			this.Grid1.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.Grid2.ResumeLayout(true);
			this.Grid2.PerformLayout();
			this.kSplitContainer3.Panel1.ResumeLayout(false);
			this.kSplitContainer3.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer3)).EndInit();
			this.kSplitContainer3.ResumeLayout(false);
			this.kSplitContainer3.PerformLayout();
			this.zGroupBox3.ResumeLayout(false);
			this.zGroupBox3.PerformLayout();
			this.Grid3.ResumeLayout(true);
			this.Grid3.PerformLayout();
			this.zGroupBox4.ResumeLayout(false);
			this.zGroupBox4.PerformLayout();
			this.Grid4.ResumeLayout(true);
			this.Grid4.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private KSplitContainer kSplitContainer1;
		private KSplitContainer kSplitContainer2;
		private KSplitContainer kSplitContainer3;
		private ZGroupBox zGroupBox1;
		private ZGroupBox zGroupBox2;
		private ZGroupBox zGroupBox3;
		private ZGroupBox zGroupBox4;
		private IncidentClosureDispositionGridControl Grid1;
		private IncidentClosureDispositionGridControl Grid2;
		private IncidentClosureDispositionGridControl Grid3;
		private IncidentClosureDispositionGridControl Grid4;
	}
}
