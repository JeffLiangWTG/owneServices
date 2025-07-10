namespace Enterprise.Customs.BE.GUI
{
	partial class MiscOptionsLayoutUserControl
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
			this.VATDeferTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.VATDeferTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BE.Business.Declaration.JobDeclaration);
			// 
			// VATDeferTypeDropEdit
			// 
			this.VATDeferTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VATDeferTypeDropEdit, "ZG_VATDeferType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).ZG_VATDeferType)));
			this.VATDeferTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 36, true);
			this.VATDeferTypeDropEdit.Name = "VATDeferTypeDropEdit";
			this.VATDeferTypeDropEdit.PreBoundMaxLength = 1;
			this.VATDeferTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.VATDeferTypeDropEdit.TabIndex = 1;
			// 
			// MiscOptionsLayoutUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.VATDeferTypeDropEdit);
			this.Name = "MiscOptionsLayoutUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 94, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.VATDeferTypeDropEdit.ResumeLayout(true);
			this.VATDeferTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit VATDeferTypeDropEdit;
	}
}
