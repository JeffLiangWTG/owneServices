namespace Enterprise.Customs.DE.GUI
{
	partial class PRLCONDeclarationUserControl
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ConsolidationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DeclarationsAndLinesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.DeclarationsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LinesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.LinesToConsolidateTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LinesToConsolidateUserControl = new Enterprise.Customs.DE.GUI.LineToConsolidateDynamicUserControl();
			this.NewConsolidatedLineTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.NewConsolidatedLineUserControl = new Enterprise.Customs.DE.GUI.NewConsolidatedLineUserControl();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessagesUserControl = new Enterprise.Customs.DE.GUI.MessagesUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConsolidationPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DeclarationsAndLinesSplitContainer)).BeginInit();
			this.DeclarationsAndLinesSplitContainer.Panel1.SuspendLayout();
			this.DeclarationsAndLinesSplitContainer.Panel2.SuspendLayout();
			this.DeclarationsAndLinesSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DeclarationsGrid)).BeginInit();
			this.DeclarationsGrid.SuspendLayout();
			this.LinesTabControl.SuspendLayout();
			this.LinesToConsolidateTabPage.SuspendLayout();
			this.LinesToConsolidateUserControl.SuspendLayout();
			this.NewConsolidatedLineTabPage.SuspendLayout();
			this.NewConsolidatedLineUserControl.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.MessagesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader);
			// 
			// ConsolidationPanel
			// 
			this.ConsolidationPanel.Controls.Add(this.DeclarationsAndLinesSplitContainer);
			this.ConsolidationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolidationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsolidationPanel.Name = "ConsolidationPanel";
			this.ConsolidationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1064, 515, true);
			this.ConsolidationPanel.TabIndex = 0;
			// 
			// DeclarationsAndLinesSplitContainer
			// 
			this.DeclarationsAndLinesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeclarationsAndLinesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeclarationsAndLinesSplitContainer.Name = "DeclarationsAndLinesSplitContainer";
			this.DeclarationsAndLinesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// DeclarationsAndLinesSplitContainer.Panel1
			// 
			this.DeclarationsAndLinesSplitContainer.Panel1.Controls.Add(this.DeclarationsGrid);
			// 
			// DeclarationsAndLinesSplitContainer.Panel2
			// 
			this.DeclarationsAndLinesSplitContainer.Panel2.Controls.Add(this.LinesTabControl);
			this.DeclarationsAndLinesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1064, 515, true);
			this.DeclarationsAndLinesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(196);
			this.DeclarationsAndLinesSplitContainer.TabIndex = 0;
			// 
			// DeclarationsGrid
			// 
			this.DeclarationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DeclarationsGrid, "PRLCONCusTempStorageDecs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).PRLCONCusTempStorageDecs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).PRLCONCusTempStorageDecs)).SyncRoot)).STH_IdentificationIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).PRLCONCusTempStorageDecs)).SyncRoot)).STH_AdditionalInformation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).PRLCONCusTempStorageDecs)).SyncRoot)).ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).PRLCONCusTempStorageDecs)).SyncRoot)).STH_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).PRLCONCusTempStorageDecs)).SyncRoot)).STH_MessageStatus)));
			this.DeclarationsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "STH_IdentificationIndicator";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(61);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo1.ColumnName = "STH_AdditionalInformation";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(280);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("738c4af4-d0b3-46e4-adea-d17ccb3a774a", "New Reference");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "ReferenceNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(167);
			zDateEditColumnStyleInfo1.ColumnName = "STH_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "STH_MessageStatus";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DeclarationsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DeclarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DeclarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DeclarationsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.DeclarationsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DeclarationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeclarationsGrid.GridId = "2cee73ba-51c8-46ca-8d40-f152cbb8fb11";
			this.DeclarationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DeclarationsGrid.LayoutKey = "DeclarationsGrid";
			this.DeclarationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeclarationsGrid.Name = "DeclarationsGrid";
			this.DeclarationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1064, 196, true);
			this.DeclarationsGrid.TabIndex = 0;
			// 
			// LinesTabControl
			// 
			this.LinesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.LinesTabControl.Controls.Add(this.LinesToConsolidateTabPage);
			this.LinesTabControl.Controls.Add(this.NewConsolidatedLineTabPage);
			this.LinesTabControl.Controls.Add(this.MessagesTabPage);
			this.LinesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LinesTabControl.Name = "LinesTabControl";
			this.LinesTabControl.SelectedIndex = 0;
			this.LinesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1064, 315, true);
			this.LinesTabControl.TabIndex = 0;
			// 
			// LinesToConsolidateTabPage
			// 
			this.LinesToConsolidateTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("17ebd3cc-de0c-4f26-b1dd-48ee688c6e8d", "Lines to Consolidate");
			this.LinesToConsolidateTabPage.Controls.Add(this.LinesToConsolidateUserControl);
			this.LinesToConsolidateTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinesToConsolidateTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LinesToConsolidateTabPage.Name = "LinesToConsolidateTabPage";
			this.LinesToConsolidateTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.LinesToConsolidateTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1056, 288, true);
			this.LinesToConsolidateTabPage.TabIndex = 0;
			this.LinesToConsolidateTabPage.UseVisualStyleBackColor = true;
			// 
			// LinesToConsolidateUserControl
			// 
			this.LinesToConsolidateUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LinesToConsolidateUserControl, "PRLCONCusTempStorageDecs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).PRLCONCusTempStorageDecs)).SyncRoot)))));
			this.LinesToConsolidateUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinesToConsolidateUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LinesToConsolidateUserControl.Name = "LinesToConsolidateUserControl";
			this.LinesToConsolidateUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 282, true);
			this.LinesToConsolidateUserControl.TabIndex = 1;
			// 
			// NewConsolidatedLineTabPage
			// 
			this.NewConsolidatedLineTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("37c3a83d-70d0-4570-87a9-024bac456709", "New Consolidated Line");
			this.NewConsolidatedLineTabPage.Controls.Add(this.NewConsolidatedLineUserControl);
			this.NewConsolidatedLineTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NewConsolidatedLineTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NewConsolidatedLineTabPage.Name = "NewConsolidatedLineTabPage";
			this.NewConsolidatedLineTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.NewConsolidatedLineTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1056, 288, true);
			this.NewConsolidatedLineTabPage.TabIndex = 0;
			this.NewConsolidatedLineTabPage.UseVisualStyleBackColor = true;
			// 
			// NewConsolidatedLineUserControl
			// 
			this.NewConsolidatedLineUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NewConsolidatedLineUserControl, "PRLCONCusTempStorageDecs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).PRLCONCusTempStorageDecs)).SyncRoot)))));
			this.NewConsolidatedLineUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NewConsolidatedLineUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.NewConsolidatedLineUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(874, 290, true);
			this.NewConsolidatedLineUserControl.Name = "NewConsolidatedLineUserControl";
			this.NewConsolidatedLineUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1050, 290, true);
			this.NewConsolidatedLineUserControl.TabIndex = 1;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("7a156854-92a6-4fd5-918f-eabba9b17381", "Messages");
			this.MessagesTabPage.Controls.Add(this.MessagesUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1056, 288, true);
			this.MessagesTabPage.TabIndex = 1;
			// 
			// MessagesUserControl
			// 
			this.MessagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessagesUserControl, "PRLCONCusTempStorageDecs.Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.EDIMessageCollection)(((Enterprise.Customs.DE.Business.CusTempStorage.PRLCONCusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).PRLCONCusTempStorageDecs)).SyncRoot)).Messages)));
			this.MessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesUserControl.Name = "MessagesUserControl";
			this.MessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1056, 288, true);
			this.MessagesUserControl.TabIndex = 0;
			// 
			// PRLCONDeclarationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ConsolidationPanel);
			this.Name = "PRLCONDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1064, 515, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConsolidationPanel.ResumeLayout(false);
			this.ConsolidationPanel.PerformLayout();
			this.DeclarationsAndLinesSplitContainer.Panel1.ResumeLayout(false);
			this.DeclarationsAndLinesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.DeclarationsAndLinesSplitContainer)).EndInit();
			this.DeclarationsAndLinesSplitContainer.ResumeLayout(false);
			this.DeclarationsAndLinesSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DeclarationsGrid)).EndInit();
			this.DeclarationsGrid.ResumeLayout(false);
			this.DeclarationsGrid.PerformLayout();
			this.LinesTabControl.ResumeLayout(false);
			this.LinesTabControl.PerformLayout();
			this.LinesToConsolidateTabPage.ResumeLayout(false);
			this.LinesToConsolidateTabPage.PerformLayout();
			this.LinesToConsolidateUserControl.ResumeLayout(true);
			this.LinesToConsolidateUserControl.PerformLayout();
			this.NewConsolidatedLineTabPage.ResumeLayout(false);
			this.NewConsolidatedLineTabPage.PerformLayout();
			this.NewConsolidatedLineUserControl.ResumeLayout(true);
			this.NewConsolidatedLineUserControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.MessagesUserControl.ResumeLayout(true);
			this.MessagesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel ConsolidationPanel;
		private CargoWise.Windows.UI.KSplitContainer DeclarationsAndLinesSplitContainer;
		private ZArchitecture.ZGrid DeclarationsGrid;
		private ZArchitecture.GUI.ZTabControl LinesTabControl;
		internal ZArchitecture.GUI.ZTabPage LinesToConsolidateTabPage;
		internal ZArchitecture.GUI.ZTabPage NewConsolidatedLineTabPage;
		private LineToConsolidateDynamicUserControl LinesToConsolidateUserControl;
		private NewConsolidatedLineUserControl NewConsolidatedLineUserControl;
		private ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private MessagesUserControl MessagesUserControl;
	}
}
