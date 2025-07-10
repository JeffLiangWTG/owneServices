namespace Enterprise.Customs.EU.GUI
{
	partial class ConditionsAndTermsUserControl
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
            this.ConditionsAndTermsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ProcessingProcedureCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ProcessingProcedureDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ConditionsAndTermsGroupBox.SuspendLayout();
            this.ProcessingProcedureCodeDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
            // 
            // ConditionsAndTermsGroupBox
            // 
            this.ConditionsAndTermsGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("259291c0-321c-4e84-ac8a-cb2c3c93dc07", "Conditions and Terms");
            this.ConditionsAndTermsGroupBox.Controls.Add(this.ProcessingProcedureCodeDropEdit);
            this.ConditionsAndTermsGroupBox.Controls.Add(this.ProcessingProcedureDetailsTextBox);
            this.ConditionsAndTermsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConditionsAndTermsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ConditionsAndTermsGroupBox.Name = "ConditionsAndTermsGroupBox";
            this.ConditionsAndTermsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 170, true);
            this.ConditionsAndTermsGroupBox.TabIndex = 0;
            this.ConditionsAndTermsGroupBox.TabStop = false;
            // 
            // ProcessingProcedureCodeDropEdit
            // 
            this.ProcessingProcedureCodeDropEdit.AllowDrop = true;
            this.ProcessingProcedureCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.ProcessingProcedureCodeDropEdit, "CustomsEntryInstructions.ZG_ProcessingProcedureCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ZG_ProcessingProcedureCode)));
            this.ProcessingProcedureCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(213, 19, true);
            this.ProcessingProcedureCodeDropEdit.Name = "ProcessingProcedureCodeDropEdit";
            this.ProcessingProcedureCodeDropEdit.PreBoundMaxLength = 3;
            this.ProcessingProcedureCodeDropEdit.ShouldResizeByMaxLength = false;
            this.ProcessingProcedureCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 15, true);
            this.ProcessingProcedureCodeDropEdit.TabIndex = 0;
            // 
            // ProcessingProcedureDetailsTextBox
            // 
            this.ProcessingProcedureDetailsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.ProcessingProcedureDetailsTextBox, "CustomsEntryInstructions.ProcessingProcedureDetails");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ProcessingProcedureDetails)));
            this.ProcessingProcedureDetailsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ProcessingProcedureDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 45, true);
            this.ProcessingProcedureDetailsTextBox.Multiline = true;
            this.ProcessingProcedureDetailsTextBox.Name = "ProcessingProcedureDetailsTextBox";
            this.ProcessingProcedureDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 119, true);
            this.ProcessingProcedureDetailsTextBox.TabIndex = 1;
            // 
            // ConditionsAndTermsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ConditionsAndTermsGroupBox);
            this.Name = "ConditionsAndTermsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 170, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ConditionsAndTermsGroupBox.ResumeLayout(false);
            this.ConditionsAndTermsGroupBox.PerformLayout();
            this.ProcessingProcedureCodeDropEdit.ResumeLayout(true);
            this.ProcessingProcedureCodeDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox ConditionsAndTermsGroupBox;
		internal ZArchitecture.GUI.ZDropEdit ProcessingProcedureCodeDropEdit;
		internal ZArchitecture.ZTextBox ProcessingProcedureDetailsTextBox;
	}
}
