namespace Enterprise.Customs.IT.GUI
{
	partial class ShipmentTypeUserControl
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
			this.AuthorisationNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageVersionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AuthorisationNumberDropEdit.SuspendLayout();
			this.MessageVersionDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobDeclaration);
			// 
			// AuthorisationNumberDropEdit
			// 
			this.AuthorisationNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorisationNumberDropEdit, "ZG_AuthorisationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).ZG_AuthorisationNumber)));
			this.AuthorisationNumberDropEdit.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("173e128c-e012-4a55-b045-e40a0bf7a040", "Authorization");
			this.AuthorisationNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 12, true);
			this.AuthorisationNumberDropEdit.Name = "AuthorisationNumberDropEdit";
			this.AuthorisationNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.AuthorisationNumberDropEdit.TabIndex = 4;
			// 
			// MessageVersionDropEdit
			// 
			this.MessageVersionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageVersionDropEdit, "MessageVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).MessageVersion)));
			this.MessageVersionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 39, true);
			this.MessageVersionDropEdit.Name = "MessageVersionDropEdit";
			this.MessageVersionDropEdit.PreBoundMaxLength = 3;
			this.MessageVersionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
			this.MessageVersionDropEdit.TabIndex = 12;
			// 
			// ShipmentTypeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MessageVersionDropEdit);
			this.Controls.Add(this.AuthorisationNumberDropEdit);
			this.Name = "ShipmentTypeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 166, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AuthorisationNumberDropEdit.ResumeLayout(true);
			this.AuthorisationNumberDropEdit.PerformLayout();
			this.MessageVersionDropEdit.ResumeLayout(true);
			this.MessageVersionDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		protected internal ZArchitecture.GUI.ZDropEdit AuthorisationNumberDropEdit;
		protected internal ZArchitecture.GUI.ZDropEdit MessageVersionDropEdit;
	}
}
