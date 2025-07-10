namespace Enterprise.DocumentScanning.GUI
{
	partial class DocumentDbManagerForm
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.StorageDatabaseGrid = new Enterprise.ZArchitecture.ZGrid();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StorageDatabaseGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 305, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentScanning.Business.DocumentDbManager);
			// 
			// StorageDatabaseGrid
			// 
			this.StorageDatabaseGrid.AllowNavigation = false;
			this.StorageDatabaseGrid.AllowSorting = false;
			this.StorageDatabaseGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StorageDatabaseGrid, "StorageDatabaseCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentDbManager)(null)).StorageDatabaseCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentScanning.Business.StorageDatabaseInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentDbManager)(null)).StorageDatabaseCollection)).SyncRoot)).DatabaseNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.StorageDatabaseInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentDbManager)(null)).StorageDatabaseCollection)).SyncRoot)).DatabaseName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentScanning.Business.StorageDatabaseInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentDbManager)(null)).StorageDatabaseCollection)).SyncRoot)).SizeInMb)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.StorageDatabaseInfo)(((System.Collections.IList)(((Enterprise.DocumentScanning.Business.DocumentDbManager)(null)).StorageDatabaseCollection)).SyncRoot)).NewReadOnly)));
			this.StorageDatabaseGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "DatabaseNumber";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(20);
			zTextBoxColumnStyleInfo1.ColumnName = "DatabaseName";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "SizeInMb";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCheckBoxColumnStyleInfo1.ColumnName = "NewReadOnly";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.StorageDatabaseGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.StorageDatabaseGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.StorageDatabaseGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.StorageDatabaseGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.StorageDatabaseGrid.GridId = "dc7f933a-430c-44d1-9719-4d859a299d51";
			this.StorageDatabaseGrid.CopySelectedRowsAllowed = false;
			this.StorageDatabaseGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.StorageDatabaseGrid.LayoutKey = "zGrid1";
			this.StorageDatabaseGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.StorageDatabaseGrid.Name = "StorageDatabaseGrid";
			this.StorageDatabaseGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.StorageDatabaseGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 258, true);
			this.StorageDatabaseGrid.TabIndex = 4;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("DocumentDbManagerForm|ebfbcc5b-65ad-4c3e-9f2c-ec25d9168071", "Save");
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 276, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 6;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("DocumentDbManagerForm|f7a5998a-6fb6-4503-a41d-cb02216dbbdf", "Cancel");
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(330, 276, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 7;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// DocumentDbManagerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 329, true);
			this.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("DocumentDbManagerForm|b9ed3569-e5ea-4d86-8d3f-ed92737a8d88", "eDocs Database Manager");
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.StorageDatabaseGrid);
			this.DataSourceType = typeof(Enterprise.DocumentScanning.Business.DocumentDbManager);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 247, true);
			this.Name = "DocumentDbManagerForm";
			this.Text = "DocumentDbManagerForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.StorageDatabaseGrid, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StorageDatabaseGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.ZGrid StorageDatabaseGrid;
		internal ZArchitecture.GUI.ZButton okButton;
		private ZArchitecture.GUI.ZButton cancelButton;
	}
}
