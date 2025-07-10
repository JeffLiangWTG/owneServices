
namespace Enterprise.Customs.CN.GUI
{
	partial class CodeDescriptionOptionForm
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
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.OptionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zOKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OptionsGrid)).BeginInit();
			this.OptionsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 328, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.CodeDescriptionOptionCollectionParent);
			// 
			// OptionsGrid
			// 
			this.OptionsGrid.AllowNavigation = false;
			this.OptionsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OptionsGrid, "OptionCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CodeDescriptionOptionCollectionParent)(null)).OptionCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CN.Business.CodeDescriptionOption)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CodeDescriptionOptionCollectionParent)(null)).OptionCollection)).SyncRoot)).Selected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CodeDescriptionOption)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CodeDescriptionOptionCollectionParent)(null)).OptionCollection)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CodeDescriptionOption)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CodeDescriptionOptionCollectionParent)(null)).OptionCollection)).SyncRoot)).Description)));
			this.OptionsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("a9a43500-3dc8-4aa3-a152-88384630de24", "Selected");
			zCheckBoxColumnStyleInfo1.ColumnName = "Selected";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("9b77e62e-61a2-4258-b24b-927823dcb9d1", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("662b44df-e8a6-4ee0-8df3-e3a42f5c1fc3", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.OptionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.OptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OptionsGrid.GridId = "268a5407-0548-4ba2-a00f-07ffa686449d";
			this.OptionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OptionsGrid.LayoutKey = "OptionsGrid";
			this.OptionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OptionsGrid.Name = "OptionsGrid";
			this.OptionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 293, true);
			this.OptionsGrid.TabIndex = 1;
			// 
			// zOKButton
			// 
			this.zOKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zOKButton.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("1ab45db4-6e7f-4f30-8bfb-04101a9e978c", "OK");
			this.zOKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 299, true);
			this.zOKButton.Name = "zOKButton";
			this.zOKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zOKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zOKButton.TabIndex = 2;
			this.zOKButton.ToolTipCaption = null;
			this.zOKButton.Click += new System.EventHandler(this.zOKButton_Click);
			// 
			// zCancelButton
			// 
			this.zCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zCancelButton.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("701acd12-7582-4d2b-b80c-69dc2c047d05", "Cancel");
			this.zCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 299, true);
			this.zCancelButton.Name = "zCancelButton";
			this.zCancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zCancelButton.TabIndex = 3;
			this.zCancelButton.ToolTipCaption = null;
			this.zCancelButton.Click += new System.EventHandler(this.zCancelButton_Click);
			// 
			// CodeDescriptionOptionForm
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("fb7e6634-6115-4afb-bc1d-63ed3fdb346c", "Select Codes");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 352, true);
			this.Controls.Add(this.zCancelButton);
			this.Controls.Add(this.zOKButton);
			this.Controls.Add(this.OptionsGrid);
			this.DataSourceType = typeof(Enterprise.Customs.CN.Business.CodeDescriptionOptionCollectionParent);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 409, true);
			this.Name = "CodeDescriptionOptionForm";
			this.Text = "Select Codes";
			this.Controls.SetChildIndex(this.OptionsGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zOKButton, 0);
			this.Controls.SetChildIndex(this.zCancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OptionsGrid)).EndInit();
			this.OptionsGrid.ResumeLayout(false);
			this.OptionsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion


		internal Enterprise.ZArchitecture.ZGrid OptionsGrid;
		internal ZArchitecture.GUI.ZButton zOKButton;
		internal ZArchitecture.GUI.ZButton zCancelButton;
	}
}
