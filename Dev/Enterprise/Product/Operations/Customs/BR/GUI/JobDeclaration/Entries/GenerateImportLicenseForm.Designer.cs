namespace Enterprise.Customs.BR.GUI
{
	partial class GenerateImportLicenseForm
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
		new void InitializeComponent()
		{
			this.SelectAnILLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GuidFindBox.SuspendLayout();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 100, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 24, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.GenerateImportLicenseObject);
			//
			// SelectAnILLabel
			//
			this.SelectAnILLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectAnILLabel.AutoSize = true;
			this.SelectAnILLabel.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("F4E1B84A-1350-4960-8353-1E03C46C32EA", "", "Please select an Import License to add the License");
			this.SelectAnILLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.SelectAnILLabel.IsFontBold = true;
			this.SelectAnILLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 15, true);
			this.SelectAnILLabel.Name = "SelectAnILLabel";
			this.SelectAnILLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 13, true);
			this.SelectAnILLabel.TabIndex = 1;
			//
			// CloseButton
			//
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("GenerateImportLicenseForm|79CDECBD-3317-446F-A22A-1D911216A038", "Cancel");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(396, 67, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 4;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.OnCloseButton_Click);
			//
			// OkButton
			//
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OkButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("GenerateImportLicenseForm|F45C0A70-318C-4023-83CF-66F6CB9B61F5", "OK");
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 67, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OkButton.TabIndex = 3;
			this.OkButton.ToolTipCaption = null;
			this.OkButton.UseVisualStyleBackColor = true;
			this.OkButton.Click += new System.EventHandler(this.OnOkButton_Click);
			//
			// GuidFindBox
			//
			this.GuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GuidFindBox, "ImportLicenseDeclarationPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.BR.Business.GenerateImportLicenseObject)(null)).ImportLicenseDeclarationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.GenerateImportLicenseObject)(null)).PossibleImportLicenseDeclarationForGenerate_List)));
			this.GuidFindBox.BindToList = "PossibleImportLicenseDeclarationForGenerate_List";
			this.GuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 41, true);
			this.GuidFindBox.Name = "GuidFindBox";
			this.GuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.BR.License;
			this.GuidFindBox.ParentType = null;
			this.GuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 20, true);
			this.GuidFindBox.TabIndex = 2;
			//
			// GenerateImportLicenseForm
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("GenerateImportLicenseForm|0E6E9EE3-DD86-4498-A1BD-EEC9E8633336", "Select Import License");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 124, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 172, true);
			this.Controls.Add(this.GuidFindBox);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.SelectAnILLabel);
			this.DataSourceType = typeof(Enterprise.Customs.BR.Business.GenerateImportLicenseObject);
			this.Name = "GenerateImportLicenseForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SelectAnILLabel, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.GuidFindBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GuidFindBox.ResumeLayout(true);
			this.GuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZLabel SelectAnILLabel;
		internal ZArchitecture.GUI.ZButton OkButton;
		internal ZArchitecture.GUI.ZButton CloseButton;
		internal ZArchitecture.GUI.ZGuidFindBox GuidFindBox;
	}
}
