namespace Enterprise.Customs.KR.GUI
{
	partial class FTADetailsUserControl
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
            this.LawCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CustomsDisbursementBillDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.LawCodeDropEdit.SuspendLayout();
            this.CustomsDisbursementBillDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // LawCodeDropEdit
            // 
            this.LawCodeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.LawCodeDropEdit, "CustomsEntryInstructions.CEI_FTARelationArticleCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_FTARelationArticleCode)));
            this.LawCodeDropEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("E5890A12-74A9-4385-92CA-6C62134D69E5", "Law Code");
            this.LawCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.LawCodeDropEdit.Name = "LawCodeDropEdit";
            this.LawCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 21, true);
            this.LawCodeDropEdit.TabIndex = 1;
			// 
			// CustomsDisbursementBillDropEdit
			//
			this.CustomsDisbursementBillDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsDisbursementBillDropEdit, "CustomsEntryInstructions.CEI_StatementNumber5WN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_StatementNumber5WN)));
			this.CustomsDisbursementBillDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 34, true);
            this.CustomsDisbursementBillDropEdit.Name = "CustomsDisbursementBillDropEdit";
            this.CustomsDisbursementBillDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 21, true);
            this.CustomsDisbursementBillDropEdit.TabIndex = 2;
            // 
            // FTADetailsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.CustomsDisbursementBillDropEdit);
            this.Controls.Add(this.LawCodeDropEdit);
            this.Name = "FTADetailsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 67, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.LawCodeDropEdit.ResumeLayout(true);
            this.LawCodeDropEdit.PerformLayout();
            this.CustomsDisbursementBillDropEdit.ResumeLayout(true);
            this.CustomsDisbursementBillDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		public ZArchitecture.GUI.ZDropEdit LawCodeDropEdit;
		public ZArchitecture.GUI.ZDropEdit CustomsDisbursementBillDropEdit;
	}
}
