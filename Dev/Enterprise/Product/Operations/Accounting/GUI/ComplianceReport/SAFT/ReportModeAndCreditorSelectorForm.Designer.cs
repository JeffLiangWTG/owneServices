namespace Enterprise.Accounting.GUI.ComplianceReport.SAFT
{
	partial class ReportModeAndCreditorSelectorForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.continueButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.creditorFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ownSellInvoicesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.creeditorSelfBilledInvoicesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.creditorFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ComplianceReport.SAFT.ReportModeAndCreditorSelector);
			// 
			// continueButton
			// 
			this.continueButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0219fe08-0a37-4a11-8001-913bba79f15d", "Continue");
			this.continueButton.IsCaptionOverridden = false;
			this.continueButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(279, 81, true);
			this.continueButton.Name = "continueButton";
			this.continueButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.continueButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 25, true);
			this.continueButton.TabIndex = 3;
			this.continueButton.ToolTipCaption = null;
			this.continueButton.UseVisualStyleBackColor = true;
			this.continueButton.Click += new System.EventHandler(this.ContinueButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("de720846-a1ce-44b8-a06c-ac19af2fb148", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.IsCaptionOverridden = false;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 81, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 25, true);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// creditorFindBox
			// 
			this.creditorFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.creditorFindBox, "CreditorPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ComplianceReport.SAFT.ReportModeAndCreditorSelector)(null)).CreditorPK)));
			this.creditorFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("26e9922c-baaa-4717-ab55-7b5d19a03ec1", "Creditor");
			this.creditorFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.creditorFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 47, true);
			this.creditorFindBox.Name = "creditorFindBox";
			this.creditorFindBox.ShouldResize = true;
			this.creditorFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 17, true);
			this.creditorFindBox.TabIndex = 2;
			// 
			// ownSellInvoicesRadioButton
			// 
			this.ownSellInvoicesRadioButton.AutoCheck = false;
			this.ownSellInvoicesRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ownSellInvoicesRadioButton, "GenerateSalesInvoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ComplianceReport.SAFT.ReportModeAndCreditorSelector)(null)).GenerateSalesInvoices)));
			this.ownSellInvoicesRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8b0e9e2f-f97b-4196-9a6b-52635e3d113f", "Generate Sales Invoices");
			this.ownSellInvoicesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ownSellInvoicesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 9, true);
			this.ownSellInvoicesRadioButton.Name = "ownSellInvoicesRadioButton";
			this.ownSellInvoicesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 16, true);
			this.ownSellInvoicesRadioButton.TabIndex = 0;
			this.ownSellInvoicesRadioButton.TabStop = true;
			this.ownSellInvoicesRadioButton.UseVisualStyleBackColor = true;
			// 
			// creeditorSelfBilledInvoicesRadioButton
			// 
			this.creeditorSelfBilledInvoicesRadioButton.AutoCheck = false;
			this.creeditorSelfBilledInvoicesRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.creeditorSelfBilledInvoicesRadioButton, "GenerateCreditorInvoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ComplianceReport.SAFT.ReportModeAndCreditorSelector)(null)).GenerateCreditorInvoices)));
			this.creeditorSelfBilledInvoicesRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("554e084b-ecb1-4fc7-8726-a4d786324dcc", "Generate Self Billed Creditor Invoices");
			this.creeditorSelfBilledInvoicesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.creeditorSelfBilledInvoicesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 27, true);
			this.creeditorSelfBilledInvoicesRadioButton.Name = "creeditorSelfBilledInvoicesRadioButton";
			this.creeditorSelfBilledInvoicesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 16, true);
			this.creeditorSelfBilledInvoicesRadioButton.TabIndex = 1;
			this.creeditorSelfBilledInvoicesRadioButton.TabStop = true;
			this.creeditorSelfBilledInvoicesRadioButton.UseVisualStyleBackColor = true;
			// 
			// ReportModeAndCreditorSelectorForm
			// 
			this.AcceptButton = this.continueButton;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3e4c5eda-1fda-49da-95bd-7b98d4ed2244", "Generate SAFT XML file");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 138, true);
			this.Controls.Add(this.creeditorSelfBilledInvoicesRadioButton);
			this.Controls.Add(this.ownSellInvoicesRadioButton);
			this.Controls.Add(this.creditorFindBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.continueButton);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ComplianceReport.SAFT.ReportModeAndCreditorSelector);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ComplianceReport.SAFT.CreditorSelector";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "ReportModeAndCreditorSelectorForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.continueButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.creditorFindBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ownSellInvoicesRadioButton, 0);
			this.Controls.SetChildIndex(this.creeditorSelfBilledInvoicesRadioButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.creditorFindBox.ResumeLayout(true);
			this.creditorFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton continueButton;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.GUI.ZGuidFindBox creditorFindBox;
		private ZArchitecture.GUI.ZRadioButton ownSellInvoicesRadioButton;
		private ZArchitecture.GUI.ZRadioButton creeditorSelfBilledInvoicesRadioButton;
	}
}
