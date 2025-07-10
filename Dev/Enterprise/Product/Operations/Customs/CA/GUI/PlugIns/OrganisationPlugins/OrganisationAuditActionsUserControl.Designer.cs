namespace Enterprise.Customs.CA.GUI
{
	partial class OrganisationAuditActionsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ACROSSHighValueProductAuditDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ACROSSLowValueProductAuditDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.B3HighValueProductAuditDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.B3LowValueProductAuditDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.ACROSSHighValueProductAuditDropEdit.SuspendLayout();
			this.ACROSSLowValueProductAuditDropEdit.SuspendLayout();
			this.B3HighValueProductAuditDropEdit.SuspendLayout();
			this.B3LowValueProductAuditDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.OrgImpAddInfo);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.ACROSSHighValueProductAuditDropEdit);
			this.DetailsGroupBox.Controls.Add(this.ACROSSLowValueProductAuditDropEdit);
			this.DetailsGroupBox.Controls.Add(this.B3HighValueProductAuditDropEdit);
			this.DetailsGroupBox.Controls.Add(this.B3LowValueProductAuditDropEdit);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 210, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DetailsGroupBox, false);
			// 
			// ACROSSHighValueProductAuditDropEdit
			// 
			this.ACROSSHighValueProductAuditDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ACROSSHighValueProductAuditDropEdit, "ZO_ACROSSHighValueProductAuditAction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_ACROSSHighValueProductAuditAction)));
			this.ACROSSHighValueProductAuditDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("bfbdc929-1517-4821-87c5-3dddcb7141b4", "Release High Value Product Audit");
			this.ACROSSHighValueProductAuditDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 19, true);
			this.ACROSSHighValueProductAuditDropEdit.Name = "ACROSSHighValueProductAuditDropEdit";
			this.ACROSSHighValueProductAuditDropEdit.PreBoundMaxLength = 3;
			this.ACROSSHighValueProductAuditDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.ACROSSHighValueProductAuditDropEdit.TabIndex = 5;
			// 
			// ACROSSLowValueProductAuditDropEdit
			// 
			this.ACROSSLowValueProductAuditDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ACROSSLowValueProductAuditDropEdit, "ZO_ACROSSLowValueProductAuditAction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_ACROSSLowValueProductAuditAction)));
			this.ACROSSLowValueProductAuditDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("565924e4-bf58-41ae-9ff5-b06e62a0d23f", "Release Low Value Product Audit");
			this.ACROSSLowValueProductAuditDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 42, true);
			this.ACROSSLowValueProductAuditDropEdit.Name = "ACROSSLowValueProductAuditDropEdit";
			this.ACROSSLowValueProductAuditDropEdit.PreBoundMaxLength = 3;
			this.ACROSSLowValueProductAuditDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.ACROSSLowValueProductAuditDropEdit.TabIndex = 5;
			// 
			// B3HighValueProductAuditDropEdit
			// 
			this.B3HighValueProductAuditDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.B3HighValueProductAuditDropEdit, "ZO_B3HighValueProductAuditAction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_B3HighValueProductAuditAction)));
			this.B3HighValueProductAuditDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d5b1f59a-88f7-411e-9f9c-82017b9bcd93", "Entry High Value Product Audit");
			this.B3HighValueProductAuditDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 67, true);
			this.B3HighValueProductAuditDropEdit.Name = "B3HighValueProductAuditDropEdit";
			this.B3HighValueProductAuditDropEdit.PreBoundMaxLength = 3;
			this.B3HighValueProductAuditDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.B3HighValueProductAuditDropEdit.TabIndex = 5;
			// 
			// B3LowValueProductAuditDropEdit
			// 
			this.B3LowValueProductAuditDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.B3LowValueProductAuditDropEdit, "ZO_B3LowValueProductAuditAction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_B3LowValueProductAuditAction)));
			this.B3LowValueProductAuditDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9d4b7c83-b02c-46e6-b9b3-fcc6d7fa4118", "Entry Low Value Product Audit");
			this.B3LowValueProductAuditDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 90, true);
			this.B3LowValueProductAuditDropEdit.Name = "B3LowValueProductAuditDropEdit";
			this.B3LowValueProductAuditDropEdit.PreBoundMaxLength = 3;
			this.B3LowValueProductAuditDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.B3LowValueProductAuditDropEdit.TabIndex = 5;
			// 
			// OrganisationAuditActionsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailsGroupBox);
			this.Name = "OrganisationAuditActionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 210, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ACROSSHighValueProductAuditDropEdit.ResumeLayout(true);
			this.ACROSSHighValueProductAuditDropEdit.PerformLayout();
			this.ACROSSLowValueProductAuditDropEdit.ResumeLayout(true);
			this.ACROSSLowValueProductAuditDropEdit.PerformLayout();
			this.B3HighValueProductAuditDropEdit.ResumeLayout(true);
			this.B3HighValueProductAuditDropEdit.PerformLayout();
			this.B3LowValueProductAuditDropEdit.ResumeLayout(true);
			this.B3LowValueProductAuditDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.GUI.ZDropEdit ACROSSHighValueProductAuditDropEdit;
		private ZArchitecture.GUI.ZDropEdit ACROSSLowValueProductAuditDropEdit;
		private ZArchitecture.GUI.ZDropEdit B3HighValueProductAuditDropEdit;
		private ZArchitecture.GUI.ZDropEdit B3LowValueProductAuditDropEdit;
	}
}
