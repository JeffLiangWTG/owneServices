namespace Enterprise.Customs.EU.GUI
{
	partial class SpecialProceduresOthersUserControl
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
            this.OthersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.AdditionalInformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CalculateDutyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.OthersGroupBox.SuspendLayout();
            this.CalculateDutyDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
            // 
            // OthersGroupBox
            // 
            this.OthersGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("8048568e-eae3-405f-9b2d-c3d327c18abb", "Others");
            this.OthersGroupBox.Controls.Add(this.AdditionalInformationTextBox);
            this.OthersGroupBox.Controls.Add(this.CalculateDutyDropEdit);
            this.OthersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OthersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.OthersGroupBox.Name = "OthersGroupBox";
            this.OthersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(451, 85, true);
            this.OthersGroupBox.TabIndex = 1;
            this.OthersGroupBox.TabStop = false;
            // 
            // AdditionalInformationTextBox
            // 
            this.AdditionalInformationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.AdditionalInformationTextBox, "CustomsEntryInstructions.AdditionalInformation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).AdditionalInformation)));
            this.AdditionalInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 41, true);
            this.AdditionalInformationTextBox.Name = "AdditionalInformationTextBox";
            this.AdditionalInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 15, true);
            this.AdditionalInformationTextBox.TabIndex = 2;
            // 
            // CalculateDutyDropEdit
            // 
            this.CalculateDutyDropEdit.AllowDrop = true;
            this.CalculateDutyDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CalculateDutyDropEdit, "CustomsEntryInstructions.ZG_Article86_3_UCC");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ZG_Article86_3_UCC)));
            this.CalculateDutyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 16, true);
            this.CalculateDutyDropEdit.Name = "CalculateDutyDropEdit";
            this.CalculateDutyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 15, true);
            this.CalculateDutyDropEdit.TabIndex = 4;
            // 
            // SpecialProceduresOthersUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.OthersGroupBox);
            this.Name = "SpecialProceduresOthersUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(451, 85, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.OthersGroupBox.ResumeLayout(false);
            this.OthersGroupBox.PerformLayout();
            this.CalculateDutyDropEdit.ResumeLayout(true);
            this.CalculateDutyDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
		internal ZArchitecture.GUI.ZGroupBox OthersGroupBox;
		internal ZArchitecture.ZTextBox AdditionalInformationTextBox;
		internal ZArchitecture.GUI.ZDropEdit CalculateDutyDropEdit;
	}
}
