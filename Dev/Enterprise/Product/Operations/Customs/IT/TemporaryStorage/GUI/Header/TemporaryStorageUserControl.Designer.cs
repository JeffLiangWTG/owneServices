namespace Enterprise.Customs.IT.TemporaryStorage.GUI
{
	partial class TemporaryStorageUserControl
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
			this.components = new System.ComponentModel.Container();
			this.AccountNameDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RepresentativeQualificationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AccountNameDropEdit.SuspendLayout();
			this.RepresentativeQualificationDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageHeader);
			// 
			// AccountDropEdit
			// 
			this.AccountNameDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AccountNameDropEdit, "AMA_CustomsProfile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageHeader)(null)).AMA_CustomsProfile)));
			this.AccountNameDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 197, true);
			this.AccountNameDropEdit.Name = "AccountNameDropEdit";
			this.AccountNameDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 18, true);
			this.AccountNameDropEdit.TabIndex = 20;
			// 
			// RepresentativeQualificationDropEdit
			// 
			this.RepresentativeQualificationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RepresentativeQualificationDropEdit, "AMA_AgentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageHeader)(null)).AMA_AgentType)));
			this.RepresentativeQualificationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 227, true);
			this.RepresentativeQualificationDropEdit.Name = "RepresentativeQualificationDropEdit";
			this.RepresentativeQualificationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 18, true);
			this.RepresentativeQualificationDropEdit.TabIndex = 21;
			// 
			// UCC6TemporaryStorageUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AccountNameDropEdit);
			this.Controls.Add(this.RepresentativeQualificationDropEdit);
			this.Name = "UCC6TemporaryStorageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1360, 567, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AccountNameDropEdit.ResumeLayout(true);
			this.AccountNameDropEdit.PerformLayout();
			this.RepresentativeQualificationDropEdit.ResumeLayout(true);
			this.RepresentativeQualificationDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit AccountNameDropEdit;
		internal ZArchitecture.GUI.ZDropEdit RepresentativeQualificationDropEdit;
	}
}
