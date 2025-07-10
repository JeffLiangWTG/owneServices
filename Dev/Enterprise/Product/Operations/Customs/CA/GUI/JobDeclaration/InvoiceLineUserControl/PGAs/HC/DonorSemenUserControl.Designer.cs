namespace Enterprise.Customs.CA.GUI
{
	partial class DonorSemenUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.TitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ComplianceStatementCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SuspendLayout();
			this.DetailsGroupBox.Controls.Add(this.TitleLabel);
			this.DetailsGroupBox.Controls.Add(this.ComplianceStatementCheckBox);
			// 
			// IntendedUseCodeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.IntendedUseCodeDropEdit, "CA_IntendedUseCodeDSE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_IntendedUseCodeDSE)));
			// 
			// CategoryDropEdit
			// 
			this.BindingSource.SetBindingMember(this.CategoryDropEdit, "CA_CategoryDSE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_CategoryDSE)));
			// 
			// ComplianceStatementCheckBox
			// 
			this.ComplianceStatementCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ComplianceStatementCheckBox, "CA_ComplianceStatement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_ComplianceStatement)));
			this.ComplianceStatementCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("992a06e4-e01d-425a-9f63-aaff183823ec", "Certify");
			this.ComplianceStatementCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ComplianceStatementCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(493, 14, true);
			this.ComplianceStatementCheckBox.Name = "ComplianceStatementCheckBox";
			this.ComplianceStatementCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 14, true);
			this.ComplianceStatementCheckBox.TabIndex = 3;
			this.ComplianceStatementCheckBox.UseVisualStyleBackColor = true;
			// 
			// TitleLabel
			// 
			this.TitleLabel.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ED7722DD-B7A8-476B-A4F2-66225004D1DF",
				@"The outer shipping container in which the semen is transported displays clearly, on the outside surface of that container, a declaration, signed by the processor or an authorized agent of the processor, certifying that the semen has been processed in accordance with the Processing and Distribution of Semen for Assisted Conception Regulations and quarantined for a minimum of six months.");
			this.TitleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.TitleLabel.IsFontBold = true;
			this.TitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(453, 35, true);
			this.TitleLabel.Name = "TitleLabel";
			this.TitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 68, true);
			this.TitleLabel.TabIndex = 4;
			this.TitleLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// DonorSemenUserControl
			// 
			this.Name = "DonorSemenUserControl";
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		private ZArchitecture.ZLabel TitleLabel;
		internal ZArchitecture.GUI.ZCheckBox ComplianceStatementCheckBox;
	}
}
