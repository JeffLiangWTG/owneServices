namespace Enterprise.Customs.CA.GUI
{
	partial class ReleaseStatusPrintForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.InvoicesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CancelButton1 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PrintButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoicesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 276, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(614, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobDeclaration);
			// 
			// InvoicesGrid
			// 
			this.InvoicesGrid.AllowBeginDrag = false;
			this.InvoicesGrid.AllowCopyToNewRowMenuItem = false;
			this.InvoicesGrid.AllowDragDropWithChanges = false;
			this.InvoicesGrid.AllowNavigation = false;
			this.InvoicesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.InvoicesGrid, "ReleaseStatusesToPrint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ReleaseStatusesToPrint)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ReleaseStatus)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ReleaseStatusesToPrint)).SyncRoot)).RL_CargoControlNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ReleaseStatus)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ReleaseStatusesToPrint)).SyncRoot)).RL_ReleaseStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ReleaseStatus)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ReleaseStatusesToPrint)).SyncRoot)).RL_ReleaseStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.CA.Business.ReleaseStatus)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ReleaseStatusesToPrint)).SyncRoot)).RL_ReleaseDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.ReleaseStatus)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).ReleaseStatusesToPrint)).SyncRoot)).RL_ShouldBePrinted)));
			this.InvoicesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "RL_CargoControlNumber";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.ColumnName = "RL_ReleaseStatus";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.ColumnName = "RL_ReleaseStatusDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo1.ColumnName = "RL_ReleaseDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.ColumnName = "RL_ShouldBePrinted";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.InvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.InvoicesGrid.GridId = "449bf323-ea18-409f-a4de-d9b8bd3db9ad";
			this.InvoicesGrid.CopySelectedRowsAllowed = false;
			this.InvoicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoicesGrid.IsDataVersionLogsMenuItemVisible = true;
			this.InvoicesGrid.LayoutKey = "InvoicesGrid";
			this.InvoicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 3, true);
			this.InvoicesGrid.Name = "InvoicesGrid";
			this.InvoicesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.InvoicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 238, true);
			this.InvoicesGrid.TabIndex = 0;
			// 
			// CancelButton1
			// 
			this.CancelButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ReleaseStatusPrintForm|2fef8fc5-22de-4345-a16f-f710e204a8ac", "Cancel");
			this.CancelButton1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(536, 247, true);
			this.CancelButton1.Name = "CancelButton1";
			this.CancelButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton1.TabIndex = 2;
			this.CancelButton1.UseVisualStyleBackColor = true;
			// 
			// PrintButton
			// 
			this.PrintButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ReleaseStatusPrintForm|804e6aba-8893-44eb-80d9-ba699492c606", "Print");
			this.PrintButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.PrintButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 247, true);
			this.PrintButton.Name = "PrintButton";
			this.PrintButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.PrintButton.TabIndex = 1;
			this.PrintButton.UseVisualStyleBackColor = true;
			// 
			// ReleaseStatusPrintForm
			// 
			this.AcceptButton = this.PrintButton;
			this.AutoAddPreviousNextButtons = false;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelButton1;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(614, 300, true);
			this.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ReleaseStatusPrintForm|7d400a5d-202d-4191-8da6-db75e4fea346", "Release Status Print");
			this.Controls.Add(this.InvoicesGrid);
			this.Controls.Add(this.PrintButton);
			this.Controls.Add(this.CancelButton1);
			this.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobDeclaration);
			this.DisplayErrorsInMessagePanel = false;
			this.MinimizeBox = false;
			this.Name = "ReleaseStatusPrintForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.CancelButton1, 0);
			this.Controls.SetChildIndex(this.PrintButton, 0);
			this.Controls.SetChildIndex(this.InvoicesGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoicesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid InvoicesGrid;
		private ZArchitecture.GUI.ZButton CancelButton1;
		private ZArchitecture.GUI.ZButton PrintButton;
	}
}
