namespace Enterprise.Customs.IT.NCTS.GUI
{
	partial class MiscellaneousOptionsUserControl
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
			this.CustomsProfileDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomsProfileDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.NCTS.Business.NctsHeader);
			// 
			// CustomsProfileDropEdit
			// 
			this.CustomsProfileDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsProfileDropEdit, "BH_CustomsProfile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).BH_CustomsProfile)));
			this.CustomsProfileDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 12, true);
			this.CustomsProfileDropEdit.Name = "CustomsProfileDropEdit";
			this.CustomsProfileDropEdit.PreBoundMaxLength = 3;
			this.CustomsProfileDropEdit.ShowDescriptionBox = false;
			this.CustomsProfileDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.CustomsProfileDropEdit.TabIndex = 2;
			// 
			// MiscellaneousOptionsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsProfileDropEdit);
			this.Name = "MiscellaneousOptionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 260, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomsProfileDropEdit.ResumeLayout(true);
			this.CustomsProfileDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit CustomsProfileDropEdit;
	}
}
