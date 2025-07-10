namespace Enterprise.ComplianceRisk.GUI
{
	partial class CommodityRiskStatusLogUserControl
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
			this.CommodityRiskStatus = new Enterprise.ZArchitecture.ZTextBox();
			this.AssessmentNotesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AssessmentNotesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AssessmentNotesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ComplianceRisk.Business.ComplianceCommodityRiskLog);
			// 
			// CommodityRiskStatus
			// 
			this.BindingSource.SetBindingMember(this.CommodityRiskStatus, "RiskStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityRiskLog)(null)).RiskStatusDescription)));
			this.CommodityRiskStatus.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CommodityRiskStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 14, true);
			this.CommodityRiskStatus.Name = "CommodityRiskStatus";
			this.CommodityRiskStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 35, true);
			this.CommodityRiskStatus.TabIndex = 1;
			this.CommodityRiskStatus.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// AssessmentNotesTextBox
			// 
			this.BindingSource.SetBindingMember(this.AssessmentNotesTextBox, "Notes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityRiskLog)(null)).Notes)));
			this.AssessmentNotesTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AssessmentNotesTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssessmentNotesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.AssessmentNotesTextBox.Multiline = true;
			this.AssessmentNotesTextBox.Name = "AssessmentNotesTextBox";
			this.AssessmentNotesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.AssessmentNotesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(413, 46, true);
			this.AssessmentNotesTextBox.TabIndex = 4;
			// 
			// AssessmentNotesGroupBox
			// 
			this.AssessmentNotesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.AssessmentNotesGroupBox.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("bafc645e-a1e5-4d05-b199-f83db573ac14", "Notes");
			this.AssessmentNotesGroupBox.Controls.Add(this.AssessmentNotesTextBox);
			this.AssessmentNotesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 45, true);
			this.AssessmentNotesGroupBox.Name = "AssessmentNotesGroupBox";
			this.AssessmentNotesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 80, true);
			this.AssessmentNotesGroupBox.TabIndex = 5;
			this.AssessmentNotesGroupBox.TabStop = false;
			// 
			// CommodityRiskStatusLogUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AssessmentNotesGroupBox);
			this.Controls.Add(this.CommodityRiskStatus);
			this.Name = "CommodityRiskStatusLogUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 130, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AssessmentNotesGroupBox.ResumeLayout(false);
			this.AssessmentNotesGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox CommodityRiskStatus;
		private ZArchitecture.ZTextBox AssessmentNotesTextBox;
		private ZArchitecture.GUI.ZGroupBox AssessmentNotesGroupBox;
	}
}
