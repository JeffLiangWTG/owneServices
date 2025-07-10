using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	partial class FSimplifiedDeclarationFilterStripControl
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
			if (grid != null)
			{
				grid.RowsDeleting -= SimplifiedDeclarationsGrid_RowDeleting;
				grid.Deleted -= SimplifiedDeclarationsGrid_Deleted;
				grid.MouseDown -= SimplifiedDeclarationsGrid_MouseDown;
			}

			if (LinesGrid != null)
			{
				LinesGrid.RowsDeleting -= LinesGrid_RowDeleting;
				LinesGrid.RowsDeleted -= LinesGrid_RowsDeleted;
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.HeaderAndLinesGridSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
            this.SimplifiedDeclarationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.LinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LinesGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
			this.AddStripButton.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.HeaderAndLinesGridSplitContainer)).BeginInit();
            this.HeaderAndLinesGridSplitContainer.Panel1.SuspendLayout();
            this.HeaderAndLinesGridSplitContainer.Panel2.SuspendLayout();
            this.HeaderAndLinesGridSplitContainer.SuspendLayout();
            this.SimplifiedDeclarationsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.LinesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
            this.LinesGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusReconDeclaration);
			// 
			// HeaderAndLinesGridSplitContainer
			//
			this.HeaderAndLinesGridSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.HeaderAndLinesGridSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderAndLinesGridSplitContainer.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 250, true);
            this.HeaderAndLinesGridSplitContainer.Name = "HeaderAndLinesGridSplitContainer";
            this.HeaderAndLinesGridSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // HeaderAndLinesGridSplitContainer.Panel1
            // 
            this.HeaderAndLinesGridSplitContainer.Panel1.Controls.Add(this.SimplifiedDeclarationsGroupBox);
            this.HeaderAndLinesGridSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1257, 250, true);
			this.HeaderAndLinesGridSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(5);
            // 
            // HeaderAndLinesGridSplitContainer.Panel2
            // 
            this.HeaderAndLinesGridSplitContainer.Panel2.Controls.Add(this.LinesGroupBox);
            this.HeaderAndLinesGridSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(5);
            this.HeaderAndLinesGridSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(184);
            this.HeaderAndLinesGridSplitContainer.TabIndex = 1;
			// 
			// SimplifiedDeclarationsGroupBox
			// 
			this.SimplifiedDeclarationsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("F55AE536-745C-41AD-B6B0-5B4BD5E20646", "Simplified Declarations (Select one row and double click to open Declaration).");
			this.SimplifiedDeclarationsGroupBox.Controls.Add(this.grid);
			this.SimplifiedDeclarationsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SimplifiedDeclarationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SimplifiedDeclarationsGroupBox.Name = "SimplifiedDeclarationsGroupBox";
			this.SimplifiedDeclarationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1257, 184, true);
			this.SimplifiedDeclarationsGroupBox.TabIndex = 0;
			this.SimplifiedDeclarationsGroupBox.TabStop = false;
			// 
			// grid
			// 
			this.grid.AllowReadOnlyRowsToBeDeleted = true;
			this.BindingSource.SetBindingMember(this.grid, "CusReconEntries");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CusReconEntries)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusReconEntry)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CusReconEntries)).SyncRoot)).JobNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusReconEntry)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CusReconEntries)).SyncRoot)).OwnerRef)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusReconEntry)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CusReconEntries)).SyncRoot)).DeclarantCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusReconEntry)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CusReconEntries)).SyncRoot)).ImporterCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusReconEntry)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CusReconEntries)).SyncRoot)).RepresentativeCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusReconEntry)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CusReconEntries)).SyncRoot)).BuyingAgentCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.DE.Business.CusReconEntry)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CusReconEntries)).SyncRoot)).CRE_EntryDate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusReconEntry)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CusReconEntries)).SyncRoot)).CRE_OriginalEntryNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusReconEntry)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CusReconEntries)).SyncRoot)).CRE_EntryType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusReconEntry)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CusReconEntries)).SyncRoot)).RepresentationType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusReconEntry)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CusReconEntries)).SyncRoot)).EntryHasChanges)));
            zTextBoxColumnStyleInfo1.ColumnName = "JobNumber";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.ColumnName = "OwnerRef";
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo3.ColumnName = "DeclarantCode";
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
            zTextBoxColumnStyleInfo4.ColumnName = "ImporterCode";
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
            zTextBoxColumnStyleInfo5.ColumnName = "RepresentativeCode";
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
            zTextBoxColumnStyleInfo6.ColumnName = "BuyingAgentCode";
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
            zDateEditColumnStyleInfo1.ColumnName = "CRE_EntryDate";
            zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo7.ColumnName = "CRE_OriginalEntryNumber";
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(123);
            zTextBoxColumnStyleInfo8.ColumnName = "CRE_EntryType";
            zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(76);
            zTextBoxColumnStyleInfo9.ColumnName = "RepresentationType";
            zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
            zTextBoxColumnStyleInfo10.ColumnName = "EntryHasChanges";
            zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grid.GridId = "46FC094B-D2F5-4B72-94CB-71A9F9C3D4B0";
			this.grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.LayoutKey = "SimplifiedDeclarationsGrid";
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.grid.Name = "SimplifiedDeclarationsGrid";
			this.grid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1251, 165, true);
			this.grid.TabIndex = 0;
			// 
			// LinesGroupBox
			// 
			this.LinesGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("635A7450-5106-4AF7-BD3A-D49492094036", "Lines");
			this.LinesGroupBox.Controls.Add(this.LinesGrid);
			this.LinesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LinesGroupBox.Name = "LinesGroupBox";
			this.LinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1257, 180, true);
			this.LinesGroupBox.TabIndex = 0;
			this.LinesGroupBox.TabStop = false;
			// 
			// LinesGrid
			// 
			this.LinesGrid.AllowNavigation = false;
            this.LinesGrid.AllowReadOnlyRowsToBeDeleted = true;
            this.BindingSource.SetBindingMember(this.LinesGrid, "CusReconEntries.CusReconEntryLines");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconEntry)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CusReconEntries)).SyncRoot)).CusReconEntryLines)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.DE.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconEntry)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CusReconEntries)).SyncRoot)).CusReconEntryLines)).SyncRoot)).CRL_LineNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.DE.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconEntry)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CusReconEntries)).SyncRoot)).CusReconEntryLines)).SyncRoot)).CRL_OriginalEntryLineNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconEntry)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CusReconEntries)).SyncRoot)).CusReconEntryLines)).SyncRoot)).CRL_Description)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconEntry)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CusReconEntries)).SyncRoot)).CusReconEntryLines)).SyncRoot)).CRL_CustomsStatus)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconEntry)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CusReconEntries)).SyncRoot)).CusReconEntryLines)).SyncRoot)).CustomsStatusDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconEntry)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusReconDeclaration)(null)).CusReconEntries)).SyncRoot)).CusReconEntryLines)).SyncRoot)).EntryLineHasChanges)));
            this.LinesGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo11.ColumnName = "CRL_LineNumber";
            zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo12.ColumnName = "CRL_OriginalEntryLineNumber";
            zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo13.ColumnName = "CRL_Description";
            zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo14.ColumnName = "CRL_CustomsStatus";
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("0CB25911-B6EC-4994-B2CF-43CE4D8DED41", "Customs Status");
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo15.ColumnName = "CustomsStatusDescription";
            zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo16.ColumnName = "EntryLineHasChanges";
            zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
            this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
            this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
            this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
            this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
            this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
            this.LinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LinesGrid.GridId = "47421161-1D2F-4881-9A85-982A89E32AEB";
            this.LinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.LinesGrid.LayoutKey = "LinesGrid";
            this.LinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.LinesGrid.Name = "LinesGrid";
            this.LinesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
            this.LinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1251, 161, true);
            this.LinesGrid.TabIndex = 0;
            // 
            // FSimplifiedDeclarationFilterStripControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.HeaderAndLinesGridSplitContainer);
            this.Name = "FSimplifiedDeclarationFilterStripControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1257, 626, true);
            this.Controls.SetChildIndex(this.HeaderAndLinesGridSplitContainer, 0);
            this.Controls.SetChildIndex(this.ToolStripPermissionsLabel, 0);
            this.Controls.SetChildIndex(this.FilterStripsPanel, 0);
            this.Controls.SetChildIndex(this.AddStripButton, 0);
            this.Controls.SetChildIndex(this.ToolStripRecordsFoundLabel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.HeaderAndLinesGridSplitContainer.Panel1.ResumeLayout(false);
            this.HeaderAndLinesGridSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.HeaderAndLinesGridSplitContainer)).EndInit();
            this.SimplifiedDeclarationsGroupBox.ResumeLayout(false);
            this.SimplifiedDeclarationsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
            this.grid.PerformLayout();
            this.LinesGroupBox.ResumeLayout(false);
            this.LinesGroupBox.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
            this.AddStripButton.PerformLayout();
            this.HeaderAndLinesGridSplitContainer.ResumeLayout(false);
            this.HeaderAndLinesGridSplitContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
            this.LinesGrid.ResumeLayout(false);
            this.LinesGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		CargoWise.Windows.UI.KSplitContainer HeaderAndLinesGridSplitContainer;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox SimplifiedDeclarationsGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox LinesGroupBox;
		internal ZDisplayGrid LinesGrid;
	}
}
