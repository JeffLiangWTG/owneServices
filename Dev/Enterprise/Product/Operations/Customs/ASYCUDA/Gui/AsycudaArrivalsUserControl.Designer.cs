using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class AsycudaArrivalsUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo = new ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			this.splitArrivalTab = new CargoWise.Windows.UI.KSplitContainer();
			this.groupboxForArrivals = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.arrivalHeadersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.arrivalDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.arrivalDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.arrivalDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.countrySpecificPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.transfersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.asycudaTransfersUserControl = new Enterprise.Customs.ASYCUDA.GUI.AsycudaTransfersUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitArrivalTab)).BeginInit();
			this.splitArrivalTab.Panel1.SuspendLayout();
			this.splitArrivalTab.Panel2.SuspendLayout();
			this.splitArrivalTab.SuspendLayout();
			this.groupboxForArrivals.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.arrivalHeadersGrid)).BeginInit();
			this.arrivalHeadersGrid.SuspendLayout();
			this.arrivalDetailsTabControl.SuspendLayout();
			this.arrivalDetailsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.arrivalDetailsGrid)).BeginInit();
			this.arrivalDetailsGrid.SuspendLayout();
			this.transfersTabPage.SuspendLayout();
			this.asycudaTransfersUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader);
			// 
			// splitArrivalTab
			// 
			this.splitArrivalTab.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitArrivalTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitArrivalTab.Name = "splitArrivalTab";
			this.splitArrivalTab.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitArrivalTab.Panel1
			// 
			this.splitArrivalTab.Panel1.Controls.Add(this.groupboxForArrivals);
			// 
			// splitArrivalTab.Panel2
			// 
			this.splitArrivalTab.Panel2.Controls.Add(this.arrivalDetailsTabControl);
			this.splitArrivalTab.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(373);
			this.splitArrivalTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 708, true);
			this.splitArrivalTab.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(180);
			this.splitArrivalTab.TabIndex = 2;
			// 
			// groupboxForArrivals
			// 
			this.groupboxForArrivals.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("1E233D93-292B-4AB4-8863-4D6CC27D29AE", "Arrivals");
			this.groupboxForArrivals.Controls.Add(this.arrivalHeadersGrid);
			this.groupboxForArrivals.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupboxForArrivals.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupboxForArrivals.Name = "groupboxForArrivals";
			this.groupboxForArrivals.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 200, true);
			this.groupboxForArrivals.TabIndex = 0;
			this.groupboxForArrivals.TabStop = false;
			// 
			// arrivalHeadersGrid
			// 
			this.arrivalHeadersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.arrivalHeadersGrid, "ArrivalHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).ArrivalHeaders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).ArrivalHeaders)).SyncRoot)).ATH_VoyageFlightNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).ArrivalHeaders)).SyncRoot)).ATH_ETAAtDischargePort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).ArrivalHeaders)).SyncRoot)).ETAAtDischargePortForShortFormat)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).ArrivalHeaders)).SyncRoot)).ATH_Reference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).ArrivalHeaders)).SyncRoot)).ATH_ReferenceIssueDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).ArrivalHeaders)).SyncRoot)).ATH_ArrivalSequence)));
			this.arrivalHeadersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "ATH_VoyageFlightNo";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.ColumnName = "ATH_ETAAtDischargePort";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo4.ColumnName = "ETAAtDischargePortForShortFormat";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "ATH_Reference";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo5.ColumnName = "ATH_ReferenceIssueDate";
			zDateEditColumnStyleInfo5.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "ATH_ArrivalSequence";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.arrivalHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.arrivalHeadersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.arrivalHeadersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.arrivalHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.arrivalHeadersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.arrivalHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.arrivalHeadersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.arrivalHeadersGrid.GridId = "54741FA3-CA00-49F6-83AD-37987B55FC6E";
			this.arrivalHeadersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.arrivalHeadersGrid.LayoutKey = "arrivalHeaderGrid";
			this.arrivalHeadersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.arrivalHeadersGrid.Name = "arrivalHeadersGrid";
			this.arrivalHeadersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 181, true);
			this.arrivalHeadersGrid.TabIndex = 1;
			// 
			// arrivalDetailsTabControl
			// 
			this.arrivalDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.arrivalDetailsTabControl.Controls.Add(this.arrivalDetailsTabPage);
			this.arrivalDetailsTabControl.Controls.Add(this.transfersTabPage);
			this.arrivalDetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.arrivalDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.arrivalDetailsTabControl.Name = "arrivalDetailsTabControl";
			this.arrivalDetailsTabControl.SelectedIndex = 0;
			this.arrivalDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 504, true);
			this.arrivalDetailsTabControl.TabIndex = 0;
			// 
			// arrivalDetailsTabPage
			// 
			this.arrivalDetailsTabPage.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("468989F9-BB8A-4960-9546-BC78F341451A", "Arrival Details");
			this.arrivalDetailsTabPage.Controls.Add(this.arrivalDetailsGrid);
			this.arrivalDetailsTabPage.Controls.Add(this.countrySpecificPanel);
			this.arrivalDetailsTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.arrivalDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.arrivalDetailsTabPage.Name = "arrivalDetailsTabPage";
			this.arrivalDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.arrivalDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 477, true);
			this.arrivalDetailsTabPage.TabIndex = 0;
			this.arrivalDetailsTabPage.UseVisualStyleBackColor = true;
			// 
			// arrivalDetailsGrid
			// 
			this.arrivalDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.arrivalDetailsGrid, "ArrivalHeaders.ArrivalDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).ArrivalHeaders)).SyncRoot)).ArrivalDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalLine)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).ArrivalHeaders)).SyncRoot)).ArrivalDetails)).SyncRoot)).ATL_BillNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalLine)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).ArrivalHeaders)).SyncRoot)).ArrivalDetails)).SyncRoot)).ATL_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalLine)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).ArrivalHeaders)).SyncRoot)).ArrivalDetails)).SyncRoot)).ATL_CargoStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalLine)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).ArrivalHeaders)).SyncRoot)).ArrivalDetails)).SyncRoot)).ATL_Reference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalLine)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).ArrivalHeaders)).SyncRoot)).ArrivalDetails)).SyncRoot)).ATL_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalLine)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).ArrivalHeaders)).SyncRoot)).ArrivalDetails)).SyncRoot)).ATL_WeightUQ)));
			this.arrivalDetailsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ATL_BillNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "ATL_Quantity";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "ATL_CargoStatus";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "ATL_Reference";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "ATL_Weight";
			zCalcEditColumnStyleInfo3.MaxValue = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "ATL_WeightUQ";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidDropEditColumnStyleInfo.ColumnName = "ATL_ABL_AsycudaBill";
			zGuidDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("327DD6AD-3E25-4928-8374-0AD962BF31BD", "Bill Number");
			zGuidDropEditColumnStyleInfo2.ColumnName = "ATL_APA_AsycudaPack";
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("994CEE99-A2DC-4A07-9811-B628DF816520", "Pack Line");

			this.arrivalDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.arrivalDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.arrivalDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.arrivalDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.arrivalDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.arrivalDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.arrivalDetailsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo);
			this.arrivalDetailsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.arrivalDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.arrivalDetailsGrid.GridId = "7225D30E-47E8-44BD-ADEF-F6A4A5A337FF";
			this.arrivalDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.arrivalDetailsGrid.LayoutKey = "arrivalDetailsGrid";
			this.arrivalDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.arrivalDetailsGrid.Name = "arrivalDetailsGrid";
			this.arrivalDetailsGrid.ReadOnly = true;
			this.arrivalDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 452, true);
			this.arrivalDetailsGrid.TabIndex = 1;
			// 
			// countrySpecificPanel
			// 
			this.countrySpecificPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.countrySpecificPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.countrySpecificPanel.Name = "countrySpecificPanel";
			this.countrySpecificPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 80, true);
			this.countrySpecificPanel.TabIndex = 0;
			// 
			// transfersTabPage
			// 
			this.transfersTabPage.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("BF7ED706-EBD3-48B4-9578-C323BB86CE1A", "Transfers");
			this.transfersTabPage.Controls.Add(this.asycudaTransfersUserControl);
			this.transfersTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.transfersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.transfersTabPage.Name = "transfersTabPage";
			this.transfersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.transfersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 477, true);
			this.transfersTabPage.TabIndex = 1;
			this.transfersTabPage.UseVisualStyleBackColor = true;
			this.transfersTabPage.TabVisible = false;
			// 
			// asycudaTransfersUserControl
			// 
			this.asycudaTransfersUserControl.AllowDrop = true;
			this.asycudaTransfersUserControl.AutoScroll = true;
			this.BindingSource.SetBindingMember(this.asycudaTransfersUserControl, "ArrivalHeaders");
			this.asycudaTransfersUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.asycudaTransfersUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.asycudaTransfersUserControl.Name = "asycudaTransfersUserControl";
			this.asycudaTransfersUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(999, 617, true);
			this.asycudaTransfersUserControl.TabIndex = 0;
			// 
			// ArrivalsUserControl
			// 
			this.Controls.Add(this.splitArrivalTab);
			this.Name = "ArrivalsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 708, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitArrivalTab.Panel1.ResumeLayout(false);
			this.splitArrivalTab.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitArrivalTab)).EndInit();
			this.splitArrivalTab.ResumeLayout(false);
			this.splitArrivalTab.PerformLayout();
			this.groupboxForArrivals.ResumeLayout(false);
			this.groupboxForArrivals.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.arrivalHeadersGrid)).EndInit();
			this.arrivalHeadersGrid.ResumeLayout(false);
			this.arrivalHeadersGrid.PerformLayout();
			this.arrivalDetailsTabControl.ResumeLayout(false);
			this.arrivalDetailsTabControl.PerformLayout();
			this.arrivalDetailsTabPage.ResumeLayout(false);
			this.arrivalDetailsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.arrivalDetailsGrid)).EndInit();
			this.arrivalDetailsGrid.ResumeLayout(false);
			this.arrivalDetailsGrid.PerformLayout();
			this.transfersTabPage.ResumeLayout(false);
			this.transfersTabPage.PerformLayout();
			this.asycudaTransfersUserControl.ResumeLayout(true);
			this.asycudaTransfersUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer splitArrivalTab;
		private ZGroupBox groupboxForArrivals;
		private ZGrid arrivalHeadersGrid;
		private ZGrid arrivalDetailsGrid;
		ZTabControl arrivalDetailsTabControl;
		ZTabPage arrivalDetailsTabPage;
		ZPanel countrySpecificPanel;
		ZTabPage transfersTabPage;
		AsycudaTransfersUserControl asycudaTransfersUserControl;
	}
}
