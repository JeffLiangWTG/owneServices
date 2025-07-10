namespace Enterprise.DataTransfer.GUI
{
	public partial class DataImporterForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.ImportFromFileButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RecordsAddedCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RecordsUpdatedCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ProgressTextBox = new Enterprise.DataTransfer.GUI.DataImporterForm.WhiteTextBox();
			this.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
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
			this.BindingSource.DataSourceType = typeof(Enterprise.DataTransfer.Business.DataImporterBusinessObject);
			// 
			// ImportFromFileButton
			// 
			this.ImportFromFileButton.AutoSize = true;
			this.ImportFromFileButton.CaptionResourceString = Enterprise.DataTransfer.GUI.Res.GetData("DataImporterForm|587803b9-d99d-49d9-a362-37ed5a775a38", "Import from File");
			this.ImportFromFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 8, true);
			this.ImportFromFileButton.Name = "ImportFromFileButton";
			this.ImportFromFileButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 23, true);
			this.ImportFromFileButton.TabIndex = 0;
			this.ImportFromFileButton.Click += new System.EventHandler(this.ImportFromFile_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.DataTransfer.GUI.Res.GetData("DataImporterForm|46612130-cd09-45b7-aded-93b70ce7eccb", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 496, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.CloseButton.TabIndex = 12;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// RecordsAddedCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RecordsAddedCalcEdit, "RecordsAdded");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DataTransfer.Business.DataImporterBusinessObject)(null)).RecordsAdded)));
			this.RecordsAddedCalcEdit.CaptionResourceString = Enterprise.DataTransfer.GUI.Res.GetData("DataImporterForm|a3460e1b-3bd2-4fa1-8cab-dd8d2b10ab43", "Records Added");
			this.RecordsAddedCalcEdit.DecimalPlaces = 0;
			this.RecordsAddedCalcEdit.Decimals = 0;
			this.RecordsAddedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 497, true);
			this.RecordsAddedCalcEdit.Name = "RecordsAddedCalcEdit";
			this.RecordsAddedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.RecordsAddedCalcEdit.TabIndex = 8;
			this.RecordsAddedCalcEdit.Text = "1,000";
			this.RecordsAddedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RecordsUpdatedCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RecordsUpdatedCalcEdit, "RecordsUpdated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DataTransfer.Business.DataImporterBusinessObject)(null)).RecordsUpdated)));
			this.RecordsUpdatedCalcEdit.CaptionResourceString = Enterprise.DataTransfer.GUI.Res.GetData("DataImporterForm|91bcda33-dfd2-4fb8-b3fa-80312e6cc3a3", "Records Updated");
			this.RecordsUpdatedCalcEdit.DecimalPlaces = 0;
			this.RecordsUpdatedCalcEdit.Decimals = 0;
			this.RecordsUpdatedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 497, true);
			this.RecordsUpdatedCalcEdit.Name = "RecordsUpdatedCalcEdit";
			this.RecordsUpdatedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.RecordsUpdatedCalcEdit.TabIndex = 10;
			this.RecordsUpdatedCalcEdit.Text = "1,000";
			this.RecordsUpdatedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ProgressTextBox
			// 
			this.ProgressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 37, true);
			this.ProgressTextBox.Multiline = true;
			this.ProgressTextBox.Name = "ProgressTextBox";
			this.ProgressTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ProgressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 453, true);
			this.ProgressTextBox.TabIndex = 5;
			// 
			// OnlySaveDataWhenNoRecordsHaveErrorsCheckBox
			// 
			this.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox.AutoSize = true;
			this.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox.CaptionResourceString = Enterprise.DataTransfer.GUI.Res.GetData("DataImporterForm|4a105cbf-f955-41d3-9df2-f536334e0e43", "Only save data when no imported records have errors");
			this.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 13, true);
			this.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox.Name = "OnlySaveDataWhenNoRecordsHaveErrorsCheckBox";
			this.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox.TabIndex = 13;
			this.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox.UseVisualStyleBackColor = true;
			this.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox.Visible = false;
			// 
			// DataImporterForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 553, true);
			this.CaptionResourceString = Enterprise.DataTransfer.GUI.Res.GetData("DataImporterForm|42d09331-6c81-4b0b-ba9f-d8482a602b9c", "Data Importer");
			this.Controls.Add(this.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox);
			this.Controls.Add(this.ProgressTextBox);
			this.Controls.Add(this.RecordsUpdatedCalcEdit);
			this.Controls.Add(this.RecordsAddedCalcEdit);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ImportFromFileButton);
			this.DataSourceAssemblyName = "Enterprise.DataTransfer";
			this.DataSourceType = typeof(Enterprise.DataTransfer.Business.DataImporterBusinessObject);
			this.DataSourceTypeName = "Enterprise.DataTransfer.Business.DataImporterBusinessObject";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "DataImporterForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.ImportFromFileButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.RecordsAddedCalcEdit, 0);
			this.Controls.SetChildIndex(this.RecordsUpdatedCalcEdit, 0);
			this.Controls.SetChildIndex(this.ProgressTextBox, 0);
			this.Controls.SetChildIndex(this.OnlySaveDataWhenNoRecordsHaveErrorsCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		protected Enterprise.ZArchitecture.GUI.ZButton ImportFromFileButton;
		Enterprise.ZArchitecture.ZCalcEdit RecordsAddedCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit RecordsUpdatedCalcEdit;
		internal Enterprise.DataTransfer.GUI.DataImporterForm.WhiteTextBox ProgressTextBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OnlySaveDataWhenNoRecordsHaveErrorsCheckBox;
	}
}
