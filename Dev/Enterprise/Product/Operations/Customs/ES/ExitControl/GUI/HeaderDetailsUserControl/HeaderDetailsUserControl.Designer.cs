namespace Enterprise.Customs.ES.ExitControl.GUI
{
	partial class HeaderDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CertificateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TrainingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BrokerCodeFindBox.SuspendLayout();
			this.CertificateDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.ExitControl.Business.CusExitHeader);
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "CXH_GS_NKCustomsAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.ExitControl.Business.CusExitHeader)(null)).CXH_GS_NKCustomsAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ES.ExitControl.Business.CusExitHeader)(null)).Lookups.CustomsAgents)));
			this.BrokerCodeFindBox.CaptionResourceString = Enterprise.Customs.ES.ExitControl.GUI.Res.GetData("52129951-BF06-4EA6-9186-B8CA2D0259A6", englishCaption: "Broker", englishMediumCaption: "Broker", englishShortCaption: "Broker", englishFullDescription: "The broker selected will be the responsible of declarations to Customs in this Job");
			this.BrokerCodeFindBox.BindToList = "Lookups+CustomsAgents";
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1074, 13, true);
			this.BrokerCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
			this.BrokerCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BrokerCodeFindBox.ParentType = null;
			this.BrokerCodeFindBox.PreBoundMaxLength = 3;
			this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 20, true);
			this.BrokerCodeFindBox.TabIndex = 3;
			// 
			// CertificateDropEdit
			// 
			this.CertificateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateDropEdit, "CXH_CustomsProfile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.ExitControl.Business.CusExitHeader)(null)).CXH_CustomsProfile)));
			this.CertificateDropEdit.CaptionResourceString = Enterprise.Customs.ES.ExitControl.GUI.Res.GetData("10F8A428-84B1-4898-B767-72CF3E177BC0", englishCaption: "Certificate", englishMediumCaption: "Certif.", englishShortCaption: "Cert.", englishFullDescription: "The certificate selected will be used to sign and communicate with Customs to declare all entries in this Job");
			this.CertificateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1074, 39, true);
			this.CertificateDropEdit.Name = "CertificateDropEdit";
			this.CertificateDropEdit.PreBoundMaxLength = 27;
			this.CertificateDropEdit.ShouldResizeByMaxLength = true;
			this.CertificateDropEdit.ShowDescriptionBox = false;
			this.CertificateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 20, true);
			this.CertificateDropEdit.TabIndex = 4;
			// 
			// TrainingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.TrainingCheckBox, "TrainingEntry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.ExitControl.Business.CusExitHeader)(null)).TrainingEntry)));
			this.TrainingCheckBox.CaptionResourceString = Enterprise.Customs.ES.ExitControl.GUI.Res.GetData("6B93A89D-F53B-4D47-804E-737BFEAC0BDD", englishCaption: "Training Entry", englishMediumCaption: "Training Entry", englishShortCaption: "Training Entry", englishFullDescription: "When checked the declaration will be sent to Test");
			this.TrainingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TrainingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 86, true);
			this.TrainingCheckBox.Name = "TrainingCheckBox";
			this.TrainingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 19, true);
			this.TrainingCheckBox.TabIndex = 4;
			this.TrainingCheckBox.UseVisualStyleBackColor = true;
			// 
			// HeaderDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BrokerCodeFindBox);
			this.Controls.Add(this.CertificateDropEdit);
			this.Controls.Add(this.TrainingCheckBox);
			this.Name = "HeaderDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 274, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BrokerCodeFindBox.ResumeLayout(true);
			this.BrokerCodeFindBox.PerformLayout();
			this.CertificateDropEdit.ResumeLayout(true);
			this.CertificateDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCodeFindBox BrokerCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit CertificateDropEdit;
		internal ZArchitecture.GUI.ZCheckBox TrainingCheckBox;
	}
}
