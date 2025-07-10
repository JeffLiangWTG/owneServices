namespace Enterprise.Customs.IE.EMCS.GUI
{
	partial class DeclarationOrganizationUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CertificateIdentifierGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CertificateIdentifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CertificateIdentifierGroupBox.SuspendLayout();
			this.CertificateIdentifierDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.EMCS.Business.EMCSJobDeclaration);
			// 
			// CertificateIdentifierGroupBox
			// 
			this.CertificateIdentifierGroupBox.CaptionResourceString = Enterprise.Customs.IE.EMCS.GUI.Res.GetData("fcbb847b-b2f6-4c09-94d7-95f567f40a46", "Certificate Identifier");
			this.CertificateIdentifierGroupBox.Controls.Add(this.CertificateIdentifierDropEdit);
			this.CertificateIdentifierGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 12, true);
			this.CertificateIdentifierGroupBox.Name = "CertificateIdentifierGroupBox";
			this.CertificateIdentifierGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 37, true);
			this.CertificateIdentifierGroupBox.TabIndex = 0;
			this.CertificateIdentifierGroupBox.TabStop = false;
			// 
			// CertificateIdentifierDropEdit
			// 
			this.CertificateIdentifierDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateIdentifierDropEdit, "JE_CustomsProfile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.EMCS.Business.EMCSJobDeclaration)(null)).JE_CustomsProfile)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.EMCS.Business.EMCSJobDeclaration)(null)).Lookups.CertificateIdentifierList)));
			this.CertificateIdentifierDropEdit.BindToList = "Lookups.CertificateIdentifierList";
			this.CertificateIdentifierDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CertificateIdentifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.CertificateIdentifierDropEdit.Name = "CertificateIdentifierDropEdit";
			this.CertificateIdentifierDropEdit.ShowDescriptionBox = false;
			this.CertificateIdentifierDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.CertificateIdentifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
			this.CertificateIdentifierDropEdit.TabIndex = 0;
			// 
			// DeclarationOrganizationUserControl
			// 
			this.Controls.Add(this.CertificateIdentifierGroupBox);
			this.Name = "DeclarationOrganizationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 73, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CertificateIdentifierGroupBox.ResumeLayout(false);
			this.CertificateIdentifierGroupBox.PerformLayout();
			this.CertificateIdentifierDropEdit.ResumeLayout(true);
			this.CertificateIdentifierDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox CertificateIdentifierGroupBox;
		internal ZArchitecture.GUI.ZDropEdit CertificateIdentifierDropEdit;
	}
}
