namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class TraderDetailsUserControl
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
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.NctsHeader);
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "MovementHeader.BM_GS_NKCusAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_GS_NKCusAgent)));
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 20, true);
			this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
			this.BrokerCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BrokerCodeFindBox.ParentType = null;
			this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			this.BrokerCodeFindBox.TabIndex = 1;
			// 
			// CertificateDropEdit
			// 
			this.CertificateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateDropEdit, "BH_CustomsProfile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).BH_CustomsProfile)));
			this.CertificateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 53, true);
			this.CertificateDropEdit.Name = "CertificateDropEdit";
			this.CertificateDropEdit.ShouldResizeByMaxLength = false;
			this.CertificateDropEdit.ShowDescriptionBox = false;
			this.CertificateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			this.CertificateDropEdit.TabIndex = 13;
			this.CertificateDropEdit.UseFullWidthForCodeBox = true;
			// 
			// TrainingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.TrainingCheckBox, "TrainingEntry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).TrainingEntry)));
			this.TrainingCheckBox.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("7909DDDA-E16E-4264-9E77-D48FCA0790EA", "Training Entry", "Training Entry", "Training Entry", "When checked the declaration will be sent to Test");
			this.TrainingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 3, true);
			this.TrainingCheckBox.Name = "TrainingCheckBox";
			this.TrainingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 19, true);
			this.TrainingCheckBox.TabIndex = 4;
			this.TrainingCheckBox.UseVisualStyleBackColor = true;
			// 
			// TraderDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BrokerCodeFindBox);
			this.Controls.Add(this.CertificateDropEdit);
			this.Controls.Add(this.TrainingCheckBox);
			this.Name = "TraderDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 95, true);
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
		internal ZArchitecture.GUI.ZDropEdit CertificateDropEdit;
		internal ZArchitecture.GUI.ZCheckBox TrainingCheckBox;
	}
}

