using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	partial class MeursingResultUserControl
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

		private void InitializeComponent()
		{
			this.MeursingResultDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MeursingResultDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.GuidedDecisionMakingBasic);
			// 
			// MeursingResultDropEdit
			// 
			this.MeursingResultDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MeursingResultDropEdit, "MeursingResult");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingBasic)(null)).MeursingResult)));
			this.MeursingResultDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 5, true);
			this.MeursingResultDropEdit.Name = "MeursingResultDropEdit";
			this.MeursingResultDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 20, true);
			this.MeursingResultDropEdit.TabIndex = 0;
			// 
			// MeursingResultUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MeursingResultDropEdit);
			this.Name = "MeursingResultUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, 0, 10, 0, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 29, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MeursingResultDropEdit.ResumeLayout(true);
			this.MeursingResultDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZDropEdit MeursingResultDropEdit;
	}
}
