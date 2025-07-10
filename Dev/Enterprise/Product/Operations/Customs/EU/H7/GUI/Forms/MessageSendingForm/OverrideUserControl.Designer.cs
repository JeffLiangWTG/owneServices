using Enterprise.Customs.EU.H7.Business;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	partial class OverrideUserControl
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
			this.overrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.codeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.codeDropEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// overrideCheckBox
			// 
			this.overrideCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.overrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 7, true);
			this.overrideCheckBox.Name = "overrideCheckBox";
			this.overrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 21, true);
			this.overrideCheckBox.TabIndex = 0;
			this.overrideCheckBox.UseVisualStyleBackColor = true;
			// 
			// codeDropEdit
			// 
			this.codeDropEdit.AllowDrop = true;
			this.codeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.codeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 7, true);
			this.codeDropEdit.Name = "codeDropEdit";
			this.codeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 21, true);
			this.codeDropEdit.TabIndex = 1;
			this.codeDropEdit.PreBoundMaxLength = 3;
			// 
			// OverrideUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.overrideCheckBox);
			this.Controls.Add(this.codeDropEdit);
			this.Name = "OverrideUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 28, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.codeDropEdit.ResumeLayout(true);
			this.codeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		ZCheckBox overrideCheckBox;
		ZDropEdit codeDropEdit;

		#endregion
	}
}
