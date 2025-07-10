using System.Windows.Forms;

namespace Enterprise.BufferManagement.GUI
{
	partial class FilterTabPageControl
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
			this.filterStripWrapperControl1 = new Enterprise.BufferManagement.GUI.BMFilterStripWrapperControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMComponentSectionConfiguration);
			// 
			// filterStripWrapperControl1
			// 
			this.filterStripWrapperControl1.AllowDrop = true;
			this.filterStripWrapperControl1.AutoScroll = false;
			this.BindingSource.SetBindingMember(this.filterStripWrapperControl1, BindToProperty);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.StmModuleFilter)(((Enterprise.BufferManagement.Business.BMComponentSectionConfiguration)(null)).WorkflowFilter)));
			this.filterStripWrapperControl1.Dock = DockStyle.Fill;
			this.filterStripWrapperControl1.FilterControlIdentifier = FilterIdentifier;
			this.filterStripWrapperControl1.IsPreviewAllowed = true;
			this.filterStripWrapperControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.filterStripWrapperControl1.Name = "filterStripWrapperControl1";
			this.filterStripWrapperControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 446, true);
			this.filterStripWrapperControl1.TabIndex = 0;
			// 
			// FilterTabPageControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.Controls.Add(this.filterStripWrapperControl1);
			this.Name = "FilterTabPageControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 446, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		BMFilterStripWrapperControl filterStripWrapperControl1;
	}
}
