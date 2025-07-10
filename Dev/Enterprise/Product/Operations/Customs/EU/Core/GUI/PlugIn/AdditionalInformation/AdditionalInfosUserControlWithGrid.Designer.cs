using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	partial class AdditionalInfosUserControlWithGrid
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.AdditionalInfosGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AdditionalInfosGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DetailsLayoutControl = new Enterprise.Customs.EU.GUI.PlugIn.AdditionalInformationDetailsLayoutControl();
			this.AdditionalInfosPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.GridAndDetailsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInfosGrid)).BeginInit();
			this.AdditionalInfosGrid.SuspendLayout();
			this.AdditionalInfosGroupBox.SuspendLayout();
			this.DetailsLayoutControl.SuspendLayout();
			this.AdditionalInfosPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GridAndDetailsSplitContainer)).BeginInit();
			this.GridAndDetailsSplitContainer.Panel1.SuspendLayout();
			this.GridAndDetailsSplitContainer.Panel2.SuspendLayout();
			this.GridAndDetailsSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// AdditionalInfosGrid
			// 
			this.AdditionalInfosGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalInfosGrid, "FilteredInvoiceLines.AdditionalInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AdditionalInfos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AdditionalInfos)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AdditionalInfos)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AdditionalInfos)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AdditionalInfos)).SyncRoot)).CSI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AdditionalInfos)).SyncRoot)).CSI_ReferenceNumber2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AdditionalInfos)).SyncRoot)).CSI_RX_NKCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AdditionalInfos)).SyncRoot)).CSI_Value)));
			this.AdditionalInfosGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_SubType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Code";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(131);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(530);
			zTextBoxColumnStyleInfo3.ColumnName = "CSI_ReferenceNumber2";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(86);
			zDropEditColumnStyleInfo2.ColumnName = "CSI_RX_NKCurrency";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_Value";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(147);
			this.AdditionalInfosGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AdditionalInfosGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AdditionalInfosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalInfosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AdditionalInfosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AdditionalInfosGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.AdditionalInfosGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.AdditionalInfosGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInfosGrid.GridId = "97634e6a-c7d0-48d3-a892-5aa308d1826a";
			this.AdditionalInfosGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalInfosGrid.LayoutKey = "AdditionalInfosGrid";
			this.AdditionalInfosGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInfosGrid.Name = "AdditionalInfosGrid";
			this.AdditionalInfosGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1209, 167, true);
			this.AdditionalInfosGrid.TabIndex = 6;
			// 
			// AdditionalInfosGroupBox
			// 
			this.AdditionalInfosGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("B9ED7A51-CE6E-41A7-A0AB-3DC477F2CC20", "[44] Additional Documents");
			this.AdditionalInfosGroupBox.Controls.Add(this.DetailsLayoutControl);
			this.AdditionalInfosGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInfosGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInfosGroupBox.Name = "AdditionalInfosGroupBox";
			this.AdditionalInfosGroupBox.TabIndex = 0;
			this.AdditionalInfosGroupBox.TabStop = false;
			// 
			// DetailsLayoutControl
			// 
			this.DetailsLayoutControl.AllowDrop = true;
			this.DetailsLayoutControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.DetailsLayoutControl, "FilteredInvoiceLines.AdditionalInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AdditionalInfos)).SyncRoot)))));
			this.DetailsLayoutControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsLayoutControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsLayoutControl.Name = "DetailsLayoutControl";
			this.DetailsLayoutControl.TabIndex = 6;
			// 
			// AdditionalInfosPanel
			// 
			this.AdditionalInfosPanel.Controls.Add(this.AdditionalInfosGroupBox);
			this.AdditionalInfosPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInfosPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInfosPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 120, true);
			this.AdditionalInfosPanel.Name = "AdditionalInfosPanel";
			this.AdditionalInfosPanel.TabIndex = 5;
			// 
			// GridAndDetailsSplitContainer
			// 
			this.GridAndDetailsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GridAndDetailsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GridAndDetailsSplitContainer.Name = "GridAndDetailsSplitContainer";
			this.GridAndDetailsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// GridAndDetailsSplitContainer.Panel1
			// 
			this.GridAndDetailsSplitContainer.Panel1.Controls.Add(this.AdditionalInfosGrid);
			// 
			// GridAndDetailsSplitContainer.Panel2
			// 
			this.GridAndDetailsSplitContainer.Panel2.Controls.Add(this.AdditionalInfosPanel);
			this.GridAndDetailsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1209, 393, true);
			this.GridAndDetailsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(167);
			this.GridAndDetailsSplitContainer.SplitterWidth = 6;
			this.GridAndDetailsSplitContainer.TabIndex = 7;
			// 
			// AdditionalInfosUserControlWithGrid
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GridAndDetailsSplitContainer);
			this.Name = "AdditionalInfosUserControlWithGrid";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1209, 393, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInfosGrid)).EndInit();
			this.AdditionalInfosGrid.ResumeLayout(false);
			this.AdditionalInfosGrid.PerformLayout();
			this.AdditionalInfosGroupBox.ResumeLayout(false);
			this.AdditionalInfosGroupBox.PerformLayout();
			this.DetailsLayoutControl.ResumeLayout(true);
			this.DetailsLayoutControl.PerformLayout();
			this.AdditionalInfosPanel.ResumeLayout(false);
			this.AdditionalInfosPanel.PerformLayout();
			this.GridAndDetailsSplitContainer.Panel1.ResumeLayout(false);
			this.GridAndDetailsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.GridAndDetailsSplitContainer)).EndInit();
			this.GridAndDetailsSplitContainer.ResumeLayout(false);
			this.GridAndDetailsSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		protected AdditionalInformationDetailsLayoutControl DetailsLayoutControl;
		public ZArchitecture.GUI.ZGroupBox AdditionalInfosGroupBox;
		protected ZArchitecture.ZGrid AdditionalInfosGrid;
		protected ZArchitecture.GUI.ZPanel AdditionalInfosPanel;
		private KSplitContainer GridAndDetailsSplitContainer;
	}
}
