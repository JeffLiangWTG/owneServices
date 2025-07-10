using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI
{
	public partial class CustomsPackingUserControl
	{
		ZArchitecture.GUI.ZGroupBox releaseStatusesGroupBox;
		ZArchitecture.ZGrid releaseStatusesGrid;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.releaseStatusesGroupBox = new ZArchitecture.GUI.ZGroupBox();
			this.releaseStatusesGrid = new ZArchitecture.ZGrid();
			this.PackingDetailsPanel.SuspendLayout();
			this.PackingDetailGroupBoxPanel.SuspendLayout();
			this.PackingDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingDetailsGrid)).BeginInit();
			this.PackingDetailsGrid.SuspendLayout();
			this.TotalCountPanel.SuspendLayout();
			this.HouseBillPanel.SuspendLayout();
			this.BillFilterByAndGridPanel.SuspendLayout();
			this.FilterByPanel.SuspendLayout();
			this.BillGroupBoxPanel.SuspendLayout();
			this.HouseBillsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsGrid)).BeginInit();
			this.HouseBillsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.releaseStatusesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.releaseStatusesGrid)).BeginInit();
			this.releaseStatusesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// PackingDetailsPanel
			// 
			this.PackingDetailsPanel.Controls.Add(this.releaseStatusesGroupBox);
			this.PackingDetailsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PackingDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1127, 359, true);
			this.PackingDetailsPanel.Controls.SetChildIndex(this.releaseStatusesGroupBox, 0);
			this.PackingDetailsPanel.Controls.SetChildIndex(this.TotalCountPanel, 0);
			this.PackingDetailsPanel.Controls.SetChildIndex(this.PackingDetailGroupBoxPanel, 0);
			// 
			// PackingDetailGroupBoxPanel
			// 
			this.PackingDetailGroupBoxPanel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.PackingDetailGroupBoxPanel.Dock = System.Windows.Forms.DockStyle.None;
			this.PackingDetailGroupBoxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1127, 168, true);
			// 
			// PackingDetailsGroupBox
			// 
			this.PackingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1127, 168, true);
			// 
			// PackingDetailsGrid
			// 
			this.PackingDetailsGrid.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left);
			this.PackingDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1121, 149, true);
			// 
			// TotalCountPanel
			// 
			this.TotalCountPanel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.TotalCountPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.TotalCountPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 169, true);
			this.TotalCountPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1127, 30, true);
			// 
			// Splitter
			// 
			this.Splitter.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.Splitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.Splitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 221, true);
			this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1127, 3, true);
			// 
			// HouseBillPanel
			// 
			this.HouseBillPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1127, 221, true);
			// 
			// BillFilterByAndGridPanel
			// 
			this.BillFilterByAndGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1127, 221, true);
			// 
			// FilterByPanel
			// 
			this.FilterByPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1127, 45, true);
			// 
			// BillGroupBoxPanel
			// 
			this.BillGroupBoxPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1127, 176, true);
			// 
			// HouseBillsGroupBox
			// 
			this.HouseBillsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1127, 176, true);
			// 
			// HouseBillsGrid
			// 
			this.HouseBillsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1121, 157, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobDeclaration);
			// 
			// ReleaseStatusesGroupBox
			// 
			this.releaseStatusesGroupBox.Controls.Add(this.releaseStatusesGrid);
			this.releaseStatusesGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.releaseStatusesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 199, true);
			this.releaseStatusesGroupBox.Name = "ReleaseStatusesGroupBox";
			this.releaseStatusesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1127, 160, true);
			this.releaseStatusesGroupBox.TabIndex = 5;
			this.releaseStatusesGroupBox.TabStop = false;
			this.releaseStatusesGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1ba62c69-9feb-4672-b433-3c2b6c9afa18", "Cargo Control Numbers");
			// 
			// ReleaseStatusesGrid
			// 
			this.releaseStatusesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.releaseStatusesGrid, "ReleaseStatuses");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobDeclaration)(null)).ReleaseStatuses);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ReleaseStatus)(((System.Collections.IList)(((JobDeclaration)(null)).ReleaseStatuses)).SyncRoot)).RL_CargoControlNumber);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ReleaseStatus)(((System.Collections.IList)(((JobDeclaration)(null)).ReleaseStatuses)).SyncRoot)).RL_Bill);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ReleaseStatus)(((System.Collections.IList)(((JobDeclaration)(null)).ReleaseStatuses)).SyncRoot)).Bills);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ReleaseStatus)(((System.Collections.IList)(((JobDeclaration)(null)).ReleaseStatuses)).SyncRoot)).RL_ReleaseStatus);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ReleaseStatus)(((System.Collections.IList)(((JobDeclaration)(null)).ReleaseStatuses)).SyncRoot)).RL_ReleaseStatusDescription);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ReleaseStatus)(((System.Collections.IList)(((JobDeclaration)(null)).ReleaseStatuses)).SyncRoot)).RL_ProcessingDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ReleaseStatus)(((System.Collections.IList)(((JobDeclaration)(null)).ReleaseStatuses)).SyncRoot)).RL_ReleaseDate);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ReleaseStatus)(((System.Collections.IList)(((JobDeclaration)(null)).ReleaseStatuses)).SyncRoot)).RL_ReleaseOffice);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ReleaseStatus)(((System.Collections.IList)(((JobDeclaration)(null)).ReleaseStatuses)).SyncRoot)).RL_WarehouseCode);
			this.releaseStatusesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "RL_CargoControlNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zGuidDropEditColumnStyleInfo1.BindToList = "Bills";
			zGuidDropEditColumnStyleInfo1.Caption = "Bill";
			zGuidDropEditColumnStyleInfo1.ColumnName = "RL_Bill";
			zGuidDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.ColumnName = "RL_ReleaseStatus";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "RL_ReleaseStatusDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo1.ColumnName = "RL_ProcessingDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.ColumnName = "RL_ReleaseDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "RL_ReleaseOffice";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CustomsPackingUserControl|90b50b41-d034-4a47-939b-46f123e318f0", "Sub-Location");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "RL_WarehouseCode";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.releaseStatusesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.releaseStatusesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.releaseStatusesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.releaseStatusesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.releaseStatusesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.releaseStatusesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.releaseStatusesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.releaseStatusesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.releaseStatusesGrid.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.releaseStatusesGrid.GridId = "0984351e-b7a0-4ac9-b1cf-d6da8ebbfa0e";
			this.releaseStatusesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.releaseStatusesGrid.LayoutKey = "ReleaseStatusesGrid";
			this.releaseStatusesGrid.LimitedColumns = null;
			this.releaseStatusesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.releaseStatusesGrid.Name = "ReleaseStatusesGrid";
			this.releaseStatusesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1121, 141, true);
			this.releaseStatusesGrid.TabIndex = 0;
			// 
			// CustomsPackingUserControl
			// 
			this.Name = "CustomsPackingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1127, 583, true);
			this.PackingDetailsPanel.ResumeLayout(false);
			this.PackingDetailsPanel.PerformLayout();
			this.PackingDetailGroupBoxPanel.ResumeLayout(false);
			this.PackingDetailGroupBoxPanel.PerformLayout();
			this.PackingDetailsGroupBox.ResumeLayout(false);
			this.PackingDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingDetailsGrid)).EndInit();
			this.PackingDetailsGrid.ResumeLayout(false);
			this.PackingDetailsGrid.PerformLayout();
			this.TotalCountPanel.ResumeLayout(false);
			this.TotalCountPanel.PerformLayout();
			this.HouseBillPanel.ResumeLayout(false);
			this.HouseBillPanel.PerformLayout();
			this.BillFilterByAndGridPanel.ResumeLayout(false);
			this.BillFilterByAndGridPanel.PerformLayout();
			this.FilterByPanel.ResumeLayout(false);
			this.FilterByPanel.PerformLayout();
			this.BillGroupBoxPanel.ResumeLayout(false);
			this.BillGroupBoxPanel.PerformLayout();
			this.HouseBillsGroupBox.ResumeLayout(false);
			this.HouseBillsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsGrid)).EndInit();
			this.HouseBillsGrid.ResumeLayout(false);
			this.HouseBillsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.releaseStatusesGroupBox.ResumeLayout(false);
			this.releaseStatusesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.releaseStatusesGrid)).EndInit();
			this.releaseStatusesGrid.ResumeLayout(false);
			this.releaseStatusesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		readonly System.ComponentModel.Container components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
