using Enterprise.ZArchitecture;

namespace Enterprise.BufferManagement.Module
{
	partial class ModuleDurationFilterControl
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			if (disposing)
			{
				if (scopeDropEdit != null)
				{
					scopeDropEdit.SelectedIndexChanged -= ScopeDropEdit_SelectedValueChanged;
				}
			}

			base.Dispose(disposing);
		}

		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.scopeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.minTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.maxTimeEdit = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
			this.andLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.scopeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Module.ModuleDurationFilter);
			// 
			// scopeDropEdit
			// 
			this.scopeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.scopeDropEdit, "Scope");
			this.scopeDropEdit.BindToList = "ScopeList";
			this.scopeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.scopeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.scopeDropEdit.Name = "scopeDropEdit";
			this.scopeDropEdit.PreBoundMaxLength = 17;
			this.scopeDropEdit.ShowDescriptionBox = false;
			this.scopeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.scopeDropEdit.TabIndex = 1;
			//
			// minTimeEdit
			// 
			this.minTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.minTimeEdit, "MinDuration");
			this.minTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(scopeDropEdit.Right + CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(20), 0, false);
			this.minTimeEdit.Name = "minTimeEdit";
			this.minTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.minTimeEdit.TabIndex = 2;
			// 
			// maxTimeEdit
			// 
			this.maxTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.maxTimeEdit, "MaxDuration");
			this.maxTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(minTimeEdit.Right + CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(20), 0, false);
			this.maxTimeEdit.Name = "maxTimeEdit";
			this.maxTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.maxTimeEdit.TabIndex = 3;
			//
			//andLabel
			//
			andLabel.Text = "and";
			andLabel.AutoSize = true;
			andLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			andLabel.Visible = false;
			this.Controls.Add(andLabel);
			//
			// ModuleDurationFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.scopeDropEdit);
			this.Controls.Add(this.minTimeEdit);
			this.Controls.Add(this.maxTimeEdit);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "ModuleDurationFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 25, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.scopeDropEdit.ResumeLayout(false);
			this.scopeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit scopeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZTimeEditEx minTimeEdit;
		private Enterprise.ZArchitecture.GUI.ZTimeEditEx maxTimeEdit;
		private ZLabel andLabel;
	}
}
