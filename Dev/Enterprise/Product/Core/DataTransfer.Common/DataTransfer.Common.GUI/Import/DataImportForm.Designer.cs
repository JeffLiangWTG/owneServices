namespace Enterprise.DataTransfer.Common.GUI.Import
{
	public partial class DataImportForm<T>
	{
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.importButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.stopButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.progressTextBox = new Enterprise.DataTransfer.Common.GUI.Import.WhiteTextBox();
			this.recordImportedCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.errorRecordCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 527, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 26, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DataTransfer.Common.GUI.Import.ImportStatus);
			// 
			// importButton
			// 
			this.importButton.AutoSize = true;
			this.importButton.CaptionResourceString = Enterprise.DataTransfer.Common.GUI.Res.GetData("DataImportForm|587803b9-d99d-49d9-a362-37ed5a775a38", "Import");
			this.importButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 8, true);
			this.importButton.Name = "importButton";
			this.importButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 27, true);
			this.importButton.TabIndex = 0;
			this.importButton.Click += new System.EventHandler(this.Import_Click);
			// 
			// stopButton
			// 
			this.stopButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.stopButton.CaptionResourceString = Enterprise.DataTransfer.Common.GUI.Res.GetData("DataImportForm|46612130-cd09-45b7-aded-93b70ce7eccb", "Cancel");
			this.stopButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.stopButton.Enabled = false;
			this.stopButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 497, true);
			this.stopButton.Name = "stopButton";
			this.stopButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 27, true);
			this.stopButton.TabIndex = 12;
			this.stopButton.Click += new System.EventHandler(this.StopButton_Click);
			// 
			// progressTextBox
			// 
			this.progressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 41, true);
			this.progressTextBox.Multiline = true;
			this.progressTextBox.Name = "progressTextBox";
			this.progressTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.progressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 451, true);
			this.progressTextBox.TabIndex = 5;
			// 
			// recordImportedCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.recordImportedCalcEdit, "ImportedRecords");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DataTransfer.Common.GUI.Import.ImportStatus)(null)).ImportedRecords)));
			this.recordImportedCalcEdit.CaptionResourceString = Enterprise.DataTransfer.Common.GUI.Res.GetData("DataImportForm|936912e7-bbaa-4040-9db5-d907f9d46e5b", "Imported Records");
			this.recordImportedCalcEdit.DecimalPlaces = 2;
			this.recordImportedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 499, true);
			this.recordImportedCalcEdit.Name = "recordImportedCalcEdit";
			this.recordImportedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 24, true);
			this.recordImportedCalcEdit.TabIndex = 13;
			this.recordImportedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// errorRecordCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.errorRecordCalcEdit, "ErrorRecords");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DataTransfer.Common.GUI.Import.ImportStatus)(null)).ErrorRecords)));
			this.errorRecordCalcEdit.CaptionResourceString = Enterprise.DataTransfer.Common.GUI.Res.GetData("DataImportForm|5cc4da72-7d74-44a4-a58f-a58f65f7e20b", "Failed Records");
			this.errorRecordCalcEdit.DecimalPlaces = 2;
			this.errorRecordCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 499, true);
			this.errorRecordCalcEdit.Name = "errorRecordCalcEdit";
			this.errorRecordCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 24, true);
			this.errorRecordCalcEdit.TabIndex = 14;
			this.errorRecordCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.CaptionResourceString = Enterprise.DataTransfer.Common.GUI.Res.GetData("DataImportForm|ac8ecdea-96fd-4f94-aa45-1fe7cbe4ed78", "Close");
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(579, 497, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 27, true);
			this.closeButton.TabIndex = 15;
			this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// DataImportForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 553, true);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.errorRecordCalcEdit);
			this.Controls.Add(this.recordImportedCalcEdit);
			this.Controls.Add(this.progressTextBox);
			this.Controls.Add(this.stopButton);
			this.Controls.Add(this.importButton);
			this.DataSourceAssemblyName = "Enterprise.DataTransfer.Common.GUI";
			this.DataSourceType = typeof(Enterprise.DataTransfer.Common.GUI.Import.ImportStatus);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "DataImportForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.importButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.stopButton, 0);
			this.Controls.SetChildIndex(this.progressTextBox, 0);
			this.Controls.SetChildIndex(this.recordImportedCalcEdit, 0);
			this.Controls.SetChildIndex(this.errorRecordCalcEdit, 0);
			this.Controls.SetChildIndex(this.closeButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		Enterprise.ZArchitecture.GUI.ZButton stopButton;
		Enterprise.ZArchitecture.GUI.ZButton importButton;
		WhiteTextBox progressTextBox;
		Enterprise.ZArchitecture.ZCalcEdit errorRecordCalcEdit;
		Enterprise.ZArchitecture.GUI.ZButton closeButton;
		Enterprise.ZArchitecture.ZCalcEdit recordImportedCalcEdit;
	}
}
