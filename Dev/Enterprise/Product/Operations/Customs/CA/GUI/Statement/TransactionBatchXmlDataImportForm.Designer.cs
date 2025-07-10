namespace Enterprise.Customs.CA.GUI
{
	partial class TransactionBatchXmlDataImportForm
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
			this.ImportXmlFilesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// CloseButton
			// 
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(603, 494, true);
			// 
			// ImportFromFileButton
			// 
			this.ImportFromFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 10, true);
			this.ImportFromFileButton.Visible = false;
			// 
			// OnlySaveDataWhenNoRecordsHaveErrorsCheckBox
			// 
			this.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 14, true);
			this.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(278, 17, true);
			this.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox.Visible = false;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 520, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 26, true);
			// 
			// ImportXmlFilesButton
			// 
			this.ImportXmlFilesButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("bd7eeb59-f2d8-4a23-b8a2-bd0e9e49dbec", "Import From Files");
			this.ImportXmlFilesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.ImportXmlFilesButton.Name = "ImportXmlFilesButton";
			this.ImportXmlFilesButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ImportXmlFilesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 23, true);
			this.ImportXmlFilesButton.TabIndex = 0;
			this.ImportXmlFilesButton.ToolTipCaption = null;
			this.ImportXmlFilesButton.UseVisualStyleBackColor = true;
			this.ImportXmlFilesButton.Click += new System.EventHandler(this.ImportXmlFilesButton_Click);
			// 
			// TransactionBatchXmlDataImportForm
			// 
			this.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d8c41c69-8af9-45db-939b-8fd515911ec0", "Import Transaction Batch XML Files");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 546, true);
			this.Controls.Add(this.ImportXmlFilesButton);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 585, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 585, true);
			this.Name = "TransactionBatchXmlDataImportForm";
			this.Text = "Import Transaction XML Files";
			this.Controls.SetChildIndex(this.ImportFromFileButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox, 0);
			this.Controls.SetChildIndex(this.ImportXmlFilesButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZButton ImportXmlFilesButton;
	}
}
