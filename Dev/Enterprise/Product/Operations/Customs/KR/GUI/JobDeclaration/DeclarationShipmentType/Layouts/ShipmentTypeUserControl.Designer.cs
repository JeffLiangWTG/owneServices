namespace Enterprise.Customs.KR.GUI
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
            this.TransactionTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.DeclarationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ExporterTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.TransactionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PaymentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PlanTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ImporterTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.TransactionTypeDropEdit.SuspendLayout();
            this.DeclarationTypeDropEdit.SuspendLayout();
            this.ExporterTypeDropEdit.SuspendLayout();
            this.TransactionDropEdit.SuspendLayout();
            this.PaymentTypeDropEdit.SuspendLayout();
            this.PlanTypeDropEdit.SuspendLayout();
            this.ImporterTypeDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // TransactionTypeDropEdit
            // 
            this.TransactionTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TransactionTypeDropEdit, "JE_ExportGoodsType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_ExportGoodsType)));
            this.TransactionTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 21, true);
            this.TransactionTypeDropEdit.Name = "TransactionTypeDropEdit";
            this.TransactionTypeDropEdit.PreBoundMaxLength = 3;
            this.TransactionTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
            this.TransactionTypeDropEdit.TabIndex = 1;
            // 
            // DeclarationTypeDropEdit
            // 
            this.DeclarationTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DeclarationTypeDropEdit, "JE_ProcedureType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_ProcedureType)));
            this.DeclarationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 44, true);
            this.DeclarationTypeDropEdit.Name = "DeclarationTypeDropEdit";
            this.DeclarationTypeDropEdit.PreBoundMaxLength = 3;
            this.DeclarationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
            this.DeclarationTypeDropEdit.TabIndex = 3;
            // 
            // ExporterTypeDropEdit
            // 
            this.ExporterTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ExporterTypeDropEdit, "JE_ExporterType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_ExporterType)));
            this.ExporterTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 67, true);
            this.ExporterTypeDropEdit.Name = "ExporterTypeDropEdit";
            this.ExporterTypeDropEdit.PreBoundMaxLength = 3;
            this.ExporterTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
            this.ExporterTypeDropEdit.TabIndex = 5;
            // 
            // TransactionDropEdit
            // 
            this.TransactionDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TransactionDropEdit, "JE_TradeType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_TradeType)));
            this.TransactionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 93, true);
            this.TransactionDropEdit.Name = "TransactionDropEdit";
            this.TransactionDropEdit.PreBoundMaxLength = 3;
            this.TransactionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
            this.TransactionDropEdit.TabIndex = 6;
            // 
            // PaymentTypeDropEdit
            // 
            this.PaymentTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PaymentTypeDropEdit, "JE_PaymentMethod");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_PaymentMethod)));
            this.PaymentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 119, true);
            this.PaymentTypeDropEdit.Name = "PaymentTypeDropEdit";
            this.PaymentTypeDropEdit.PreBoundMaxLength = 3;
            this.PaymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
            this.PaymentTypeDropEdit.TabIndex = 7;
            // 
            // PlanTypeDropEdit
            // 
            this.PlanTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PlanTypeDropEdit, "JE_DeclarationPlan");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_DeclarationPlan)));
            this.PlanTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 145, true);
            this.PlanTypeDropEdit.Name = "PlanTypeDropEdit";
            this.PlanTypeDropEdit.PreBoundMaxLength = 3;
            this.PlanTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
            this.PlanTypeDropEdit.TabIndex = 8;
            // 
            // ImporterTypeDropEdit
            // 
            this.ImporterTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ImporterTypeDropEdit, "ImporterType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).ImporterType)));
            this.ImporterTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 171, true);
            this.ImporterTypeDropEdit.Name = "ImporterTypeDropEdit";
            this.ImporterTypeDropEdit.PreBoundMaxLength = 3;
            this.ImporterTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
            this.ImporterTypeDropEdit.TabIndex = 9;
            // 
            // ShipmentTypeUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.ImporterTypeDropEdit);
            this.Controls.Add(this.PlanTypeDropEdit);
            this.Controls.Add(this.PaymentTypeDropEdit);
            this.Controls.Add(this.TransactionDropEdit);
            this.Controls.Add(this.ExporterTypeDropEdit);
            this.Controls.Add(this.DeclarationTypeDropEdit);
            this.Controls.Add(this.TransactionTypeDropEdit);
            this.Name = "ShipmentTypeUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 227, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.TransactionTypeDropEdit.ResumeLayout(true);
            this.TransactionTypeDropEdit.PerformLayout();
            this.DeclarationTypeDropEdit.ResumeLayout(true);
            this.DeclarationTypeDropEdit.PerformLayout();
            this.ExporterTypeDropEdit.ResumeLayout(true);
            this.ExporterTypeDropEdit.PerformLayout();
            this.TransactionDropEdit.ResumeLayout(true);
            this.TransactionDropEdit.PerformLayout();
            this.PaymentTypeDropEdit.ResumeLayout(true);
            this.PaymentTypeDropEdit.PerformLayout();
            this.PlanTypeDropEdit.ResumeLayout(true);
            this.PlanTypeDropEdit.PerformLayout();
            this.ImporterTypeDropEdit.ResumeLayout(true);
            this.ImporterTypeDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		public ZArchitecture.GUI.ZDropEdit TransactionTypeDropEdit;
		public ZArchitecture.GUI.ZDropEdit DeclarationTypeDropEdit;
		public ZArchitecture.GUI.ZDropEdit ExporterTypeDropEdit;
		public ZArchitecture.GUI.ZDropEdit TransactionDropEdit;
		public ZArchitecture.GUI.ZDropEdit PaymentTypeDropEdit;
		public ZArchitecture.GUI.ZDropEdit PlanTypeDropEdit;
		public ZArchitecture.GUI.ZDropEdit ImporterTypeDropEdit;
	}
}
