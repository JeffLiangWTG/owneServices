namespace Enterprise.Customs.BR.GUI
{
	partial class DownloadGoodsCatalogForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.ParametersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DownloadButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DownloadCatalogCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DownloadForeignOperatorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DownloadDeactivatedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConsigneeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CredentialsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ParametersGroupBox.SuspendLayout();
			this.ConsigneeFindBox.SuspendLayout();
			this.CredentialsGroupBox.SuspendLayout();
			this.BrokerCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 189, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.GoodsCatalogDownloadObject);
			// 
			// ParametersGroupBox
			// 
			this.ParametersGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("185787b9-e773-46ee-af30-81b006610c0b", "Enter Parameters");
			this.ParametersGroupBox.Controls.Add(this.DownloadButton);
			this.ParametersGroupBox.Controls.Add(this.CancelButton);
			this.ParametersGroupBox.Controls.Add(this.DownloadCatalogCheckBox);
			this.ParametersGroupBox.Controls.Add(this.DownloadForeignOperatorCheckBox);
			this.ParametersGroupBox.Controls.Add(this.DownloadDeactivatedCheckBox);
			this.ParametersGroupBox.Controls.Add(this.ConsigneeFindBox);
			this.ParametersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ParametersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 52, true);
			this.ParametersGroupBox.Name = "ParametersGroupBox";
			this.ParametersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 150, true);
			this.ParametersGroupBox.TabIndex = 0;
			this.ParametersGroupBox.TabStop = false;
			// 
			// DownloadButton
			// 
			this.DownloadButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("4c0bf40a-1e42-4709-a965-75d4f9034032", "Download");
			this.DownloadButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 145, true);
			this.DownloadButton.Name = "DownloadButton";
			this.DownloadButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 23, true);
			this.DownloadButton.TabIndex = 6;
			this.DownloadButton.ToolTipCaption = null;
			this.DownloadButton.UseVisualStyleBackColor = true;
			this.DownloadButton.Click += new System.EventHandler(this.DownloadButton_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("fcfbc309-83f0-4ef4-9dee-c86604a0e28f", "Cancel");
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 145, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 7;
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// DownloadCatalogCheckBox
			// 
			this.BindingSource.SetBindingMember(this.DownloadCatalogCheckBox, "DownloadCatalog");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.GoodsCatalogDownloadObject)(null)).DownloadCatalog)));
			this.DownloadCatalogCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 63, true);
			this.DownloadCatalogCheckBox.Name = "DownloadCatalogCheckBox";
			this.DownloadCatalogCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(214, 23, true);
			this.DownloadCatalogCheckBox.TabIndex = 3;
			this.DownloadCatalogCheckBox.UseCompatibleTextRendering = true;
			this.DownloadCatalogCheckBox.UseVisualStyleBackColor = true;
			// 
			// DownloadForeignOperatorCheckBox
			// 
			this.BindingSource.SetBindingMember(this.DownloadForeignOperatorCheckBox, "DownloadForeignOperator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.GoodsCatalogDownloadObject)(null)).DownloadForeignOperator)));
			this.DownloadForeignOperatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 117, true);
			this.DownloadForeignOperatorCheckBox.Name = "DownloadForeignOperatorCheckBox";
			this.DownloadForeignOperatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(214, 23, true);
			this.DownloadForeignOperatorCheckBox.TabIndex = 5;
			this.DownloadForeignOperatorCheckBox.UseCompatibleTextRendering = true;
			this.DownloadForeignOperatorCheckBox.UseVisualStyleBackColor = true;
			// 
			// DownloadDeactivatedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.DownloadDeactivatedCheckBox, "DownloadDeactivated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.GoodsCatalogDownloadObject)(null)).DownloadDeactivated)));
			this.DownloadDeactivatedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 90, true);
			this.DownloadDeactivatedCheckBox.Name = "DownloadDeactivatedCheckBox";
			this.DownloadDeactivatedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(214, 23, true);
			this.DownloadDeactivatedCheckBox.TabIndex = 4;
			this.DownloadDeactivatedCheckBox.UseCompatibleTextRendering = true;
			this.DownloadDeactivatedCheckBox.UseVisualStyleBackColor = true;
			// 
			// ConsigneeFindBox
			// 
			this.ConsigneeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeFindBox, "OwnerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.GoodsCatalogDownloadObject)(null)).OwnerCode)));
			this.ConsigneeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 36, true);
			this.ConsigneeFindBox.Name = "ConsigneeFindBox";
			this.ConsigneeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ConsigneeFindBox.ParentType = null;
			this.ConsigneeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ConsigneeFindBox.TabIndex = 2;
			// 
			// CredentialsGroupBox
			// 
			this.CredentialsGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("41ad3bf7-d0de-46d3-bb67-dbdbd5e05e4c", "Credentials");
			this.CredentialsGroupBox.Controls.Add(this.BrokerCodeFindBox);
			this.CredentialsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.CredentialsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CredentialsGroupBox.Name = "CredentialsGroupBox";
			this.CredentialsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 52, true);
			this.CredentialsGroupBox.TabIndex = 0;
			this.CredentialsGroupBox.TabStop = false;
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "BrokerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.GoodsCatalogDownloadObject)(null)).BrokerCode)));
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 19, true);
			this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
			this.BrokerCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BrokerCodeFindBox.ParentType = null;
			this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.BrokerCodeFindBox.TabIndex = 1;
			// 
			// DownloadGoodsCatalogForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("243185b9-fc7f-4488-a615-c570a29d2565", "Download Goods Catalog");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 250, true);
			this.Controls.Add(this.ParametersGroupBox);
			this.Controls.Add(this.CredentialsGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.BR.Business.GoodsCatalogDownloadObject);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "DownloadGoodsCatalogForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CredentialsGroupBox, 0);
			this.Controls.SetChildIndex(this.ParametersGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ParametersGroupBox.ResumeLayout(false);
			this.ParametersGroupBox.PerformLayout();
			this.ConsigneeFindBox.ResumeLayout(true);
			this.ConsigneeFindBox.PerformLayout();
			this.CredentialsGroupBox.ResumeLayout(false);
			this.CredentialsGroupBox.PerformLayout();
			this.BrokerCodeFindBox.ResumeLayout(true);
			this.BrokerCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ParametersGroupBox;
		internal ZArchitecture.GUI.ZCheckBox DownloadCatalogCheckBox;
		internal ZArchitecture.GUI.ZCheckBox DownloadForeignOperatorCheckBox;
		internal ZArchitecture.GUI.ZCodeFindBox ConsigneeFindBox;
		internal ZArchitecture.GUI.ZCheckBox DownloadDeactivatedCheckBox;
		private new ZArchitecture.GUI.ZButton CancelButton;
		internal ZArchitecture.GUI.ZButton DownloadButton;
		private ZArchitecture.GUI.ZGroupBox CredentialsGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox BrokerCodeFindBox;
	}
}
