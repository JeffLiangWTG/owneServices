using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	partial class ArrivalNotificationDetailsUserControl
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
			this.ExpectedNextCustomsProcedureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExpectedNextCustomsProcedureDropEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(NctsHeader);
			//
			// ExpectedNextCustomsProcedureDropEdit
			//
			this.ExpectedNextCustomsProcedureDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExpectedNextCustomsProcedureDropEdit, "ArrivalMovementHeader.BM_ExpectedNextCustomsProcedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).ArrivalMovementHeader.BM_ExpectedNextCustomsProcedure)));
			this.ExpectedNextCustomsProcedureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 380, true);
			this.ExpectedNextCustomsProcedureDropEdit.Name = "ExpectedNextCustomsProcedureDropEdit";
			this.ExpectedNextCustomsProcedureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.ExpectedNextCustomsProcedureDropEdit.TabIndex = 23;
			//
			// ArrivalNotificationDetailsUserControl
			//
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExpectedNextCustomsProcedureDropEdit);
			this.Name = "ArrivalNotificationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1066, 391, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExpectedNextCustomsProcedureDropEdit.ResumeLayout(true);
			this.ExpectedNextCustomsProcedureDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit ExpectedNextCustomsProcedureDropEdit;
	}
}
