namespace Enterprise.Customs.GB.EMCS.GUI
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
			this.CustomsProfileDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CertificateIdentifierGroupBox.SuspendLayout();
			this.CustomsProfileDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.EMCS.Business.EMCSJobDeclaration);
			// 
			// CustomsProfileGroupBox
			// 
			this.CertificateIdentifierGroupBox.CaptionResourceString = Enterprise.Customs.GB.EMCS.GUI.Res.GetData("D0696354-BF80-4DA5-8F9E-09F95E7F77D9", "Customs Profile");
			this.CertificateIdentifierGroupBox.Controls.Add(this.CustomsProfileDropEdit);
			this.CertificateIdentifierGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 12, true);
			this.CertificateIdentifierGroupBox.Name = "CertificateIdentifierGroupBox";
			this.CertificateIdentifierGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 37, true);
			this.CertificateIdentifierGroupBox.TabIndex = 0;
			this.CertificateIdentifierGroupBox.TabStop = false;
			// 
			// CustomsProfileDropEdit
			// 
			this.CustomsProfileDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsProfileDropEdit, "JE_CustomsProfile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.EMCS.Business.EMCSJobDeclaration)(null)).JE_CustomsProfile)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.CodeDescriptionPairList)(((Enterprise.Customs.GB.EMCS.Business.EMCSJobDeclaration)(null)).Lookups.Credentials)));
			this.CustomsProfileDropEdit.BindToList = "Lookups.Credentials";
			this.CustomsProfileDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CustomsProfileDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.CustomsProfileDropEdit.Name = "CertificateIdentifierDropEdit";
			this.CustomsProfileDropEdit.ShowDescriptionBox = false;
			this.CustomsProfileDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			this.CustomsProfileDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
			this.CustomsProfileDropEdit.TabIndex = 0;
			// 
			// DeclarationOrganizationUserControl
			// 
			this.Controls.Add(this.CertificateIdentifierGroupBox);
			this.Name = "DeclarationOrganizationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 73, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CertificateIdentifierGroupBox.ResumeLayout(false);
			this.CertificateIdentifierGroupBox.PerformLayout();
			this.CustomsProfileDropEdit.ResumeLayout(true);
			this.CustomsProfileDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox CertificateIdentifierGroupBox;
		internal ZArchitecture.GUI.ZDropEdit CustomsProfileDropEdit;
	}
}
