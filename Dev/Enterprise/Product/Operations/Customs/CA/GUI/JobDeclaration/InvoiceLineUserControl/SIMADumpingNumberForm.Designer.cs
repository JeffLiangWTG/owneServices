namespace Enterprise.Customs.CA.GUI
{
	partial class SIMADumpingNumberForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SIMAMeasureLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SIMAMeasureItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SelectButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SIMAMeasureItemsGrid)).BeginInit();
			this.SIMAMeasureItemsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 297, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.SIMADumpingNumberCollection);
			// 
			// SIMAMeasureLabel
			// 
			this.SIMAMeasureLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SIMAMeasureLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SIMAMeasureLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SIMAMeasureLabel.Name = "SIMAMeasureLabel";
			this.SIMAMeasureLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 26, true);
			this.SIMAMeasureLabel.TabIndex = 1;
			this.SIMAMeasureLabel.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b9c4dc1c-b282-4eae-9cd4-c917d0f6bc39", "There are multiple SIMA measure that might apply to this tariff, please select one from the following list.");
			// 
			// SIMAMeasureItemsGrid
			// 
			this.SIMAMeasureItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SIMAMeasureItemsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.SIMADumpingNumber)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.SIMADumpingNumber)(null)).CA_DumpingNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.SIMADumpingNumber)(null)).CA_DumpingDescription)));
			this.SIMAMeasureItemsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CA_DumpingNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(144);
			zTextBoxColumnStyleInfo2.ColumnName = "CA_DumpingDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(280);
			this.SIMAMeasureItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SIMAMeasureItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SIMAMeasureItemsGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.SIMAMeasureItemsGrid.GridId = "1ecc7a74-1699-4bca-8fc4-e89e7f71c63a";
			this.SIMAMeasureItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SIMAMeasureItemsGrid.LayoutKey = "SIMAMeasureItemsGrid";
			this.SIMAMeasureItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 26, true);
			this.SIMAMeasureItemsGrid.Name = "SIMAMeasureItemsGrid";
			this.SIMAMeasureItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 244, true);
			this.SIMAMeasureItemsGrid.TabIndex = 2;
			// 
			// SelectButton
			// 
			this.SelectButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("16f5ff84-8791-4253-a382-6cba7572091f", "OK");
			this.SelectButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(460, 272, true);
			this.SelectButton.Name = "SelectButton";
			this.SelectButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SelectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 22, true);
			this.SelectButton.TabIndex = 3;
			this.SelectButton.ToolTipCaption = null;
			this.SelectButton.Click += new System.EventHandler(this.SelectButton_Click);
			// 
			// SIMADumpingNumberForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 321, true);
			this.Controls.Add(this.SIMAMeasureItemsGrid);
			this.Controls.Add(this.SelectButton);
			this.Controls.Add(this.SIMAMeasureLabel);
			this.DataSourceType = typeof(Enterprise.Customs.CA.Business.SIMADumpingNumberCollection);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "SIMADumpingNumberForm";
			this.Text = "SIMA Measure Selection";
			this.Controls.SetChildIndex(this.SIMAMeasureLabel, 0);
			this.Controls.SetChildIndex(this.SelectButton, 0);
			this.Controls.SetChildIndex(this.SIMAMeasureItemsGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SIMAMeasureItemsGrid)).EndInit();
			this.SIMAMeasureItemsGrid.ResumeLayout(false);
			this.SIMAMeasureItemsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel SIMAMeasureLabel;
		private ZArchitecture.ZGrid SIMAMeasureItemsGrid;
		private ZArchitecture.GUI.ZButton SelectButton;
	}
}
