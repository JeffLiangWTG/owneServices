
namespace Enterprise.BufferManagement.Module
{
	partial class OpenTaskEstimateRangeFilterControl
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
			this.scopeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.minTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.maxTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.scopeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Module.OpenTaskEstimateRangeFilter);
			// 
			// scopeDropEdit
			// 
			this.scopeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.scopeDropEdit, "Scope");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Module.OpenTaskEstimateRangeFilter)(null)).Scope)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Module.OpenTaskEstimateRangeFilter)(null)).ScopeList)));
			this.scopeDropEdit.BindToList = "ScopeList";
			this.scopeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.scopeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.scopeDropEdit.Name = "scopeDropEdit";
			this.scopeDropEdit.PreBoundMaxLength = 7;
			this.scopeDropEdit.ShowDescriptionBox = false;
			this.scopeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.scopeDropEdit.TabIndex = 1;
			// 
			// minTimeEdit
			// 
			this.minTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.minTimeEdit, "MinStdEstimate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Module.OpenTaskEstimateRangeFilter)(null)).MinStdEstimate)));
			this.minTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(scopeDropEdit.Right + CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133), 0, false);
			this.minTimeEdit.Name = "minTimeEdit";
			this.minTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.minTimeEdit.TabIndex = 2;
			// 
			// maxTimeEdit
			// 
			this.maxTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.maxTimeEdit, "MaxStdEstimate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Module.OpenTaskEstimateRangeFilter)(null)).MaxStdEstimate)));
			this.maxTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(minTimeEdit.Right + CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(103), 0, false);
			this.maxTimeEdit.Name = "maxTimeEdit";
			this.maxTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.maxTimeEdit.TabIndex = 3;
			// 
			// OpenTaskEstimateRangeFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.scopeDropEdit);
			this.Controls.Add(this.minTimeEdit);
			this.Controls.Add(this.maxTimeEdit);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "OpenTaskEstimateRangeFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 25, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.scopeDropEdit.ResumeLayout(true);
			this.scopeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit scopeDropEdit;
		private ZArchitecture.GUI.ZTimeEditEx minTimeEdit;
		private ZArchitecture.GUI.ZTimeEditEx maxTimeEdit;
	}
}
