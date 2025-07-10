namespace Enterprise.BufferManagement.GUI
{
	partial class BMBoardSlideshowForm
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
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BoardsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RunSlideShowButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EditBoardButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BoardsGrid)).BeginInit();
			this.BoardsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 370, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.EditBoardButton);
			this.MainTabPage.Controls.Add(this.RunSlideShowButton);
			this.MainTabPage.Controls.Add(this.BoardsGrid);
			this.MainTabPage.Controls.Add(this.zCheckBox1);
			this.MainTabPage.Controls.Add(this.zTextBox1);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 347, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 347, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(779, 347, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 370, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 30, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMBoardSlideshow);
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "MD_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMBoardSlideshow)(null)).MD_Name)));
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 11, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 19, true);
			this.zTextBox1.TabIndex = 0;
			// 
			// zCheckBox1
			// 
			this.zCheckBox1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox1, "MD_IsPublished");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.BMBoardSlideshow)(null)).MD_IsPublished)));
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(563, 14, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 17, true);
			this.zCheckBox1.TabIndex = 1;
			this.zCheckBox1.UseVisualStyleBackColor = true;
			// 
			// boardsGrid
			// 
			this.BoardsGrid.AllowNavigation = false;
			this.BoardsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BoardsGrid, "BoardPivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoardSlideshow)(null)).BoardPivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMBoardSlideshowPivot)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoardSlideshow)(null)).BoardPivots)).SyncRoot)).MC_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.BMBoardSlideshowPivot)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoardSlideshow)(null)).BoardPivots)).SyncRoot)).SystemPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMBoardSlideshowPivot)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoardSlideshow)(null)).BoardPivots)).SyncRoot)).System.FS_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.BMBoardSlideshowPivot)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoardSlideshow)(null)).BoardPivots)).SyncRoot)).MC_MB_Board)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMBoardSlideshowPivot)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoardSlideshow)(null)).BoardPivots)).SyncRoot)).MC_DurationInSeconds)));
			this.BoardsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "MC_Sequence";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("09da7970-1129-45e4-964a-1436b7d3095a", "System", "Buffer Management System", "");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "SystemPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("64590e2f-7dd1-404d-bf7e-4ca45eba1ebd", "System Description");
			zTextBoxColumnStyleInfo1.ColumnName = "System+FS_Description";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zGuidDropEditColumnStyleInfo1.ColumnName = "MC_MB_Board";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "MC_DurationInSeconds";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.BoardsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.BoardsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.BoardsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.BoardsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.BoardsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.BoardsGrid.GridId = "f78197dd-7028-4852-aba7-5e74fbc962f9";
			this.BoardsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BoardsGrid.LayoutKey = "zGrid1";
			this.BoardsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 40, true);
			this.BoardsGrid.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 245, true);
			this.BoardsGrid.Name = "boardsGrid";
			this.BoardsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(762, 270, true);
			this.BoardsGrid.TabIndex = 3;
			// 
			// RunSlideShowButton
			// 
			this.RunSlideShowButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.RunSlideShowButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("52aa8312-334c-4275-810f-de24a0397a7c", "Run Slide Show");
			this.RunSlideShowButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(659, 6, true);
			this.RunSlideShowButton.Name = "RunSlideShowButton";
			this.RunSlideShowButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 28, true);
			this.RunSlideShowButton.TabIndex = 2;
			this.RunSlideShowButton.UseVisualStyleBackColor = true;
			this.RunSlideShowButton.Click += new System.EventHandler(this.RunSlideShowButton_Click);
			this.RunSlideShowButton.EditableInViewMode = true;
			// 
			// EditBoardButton
			// 
			this.EditBoardButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.EditBoardButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("6c9a6b7b-b484-4900-bf40-b4ce6a31b132", "Edit Selected Board");
			this.EditBoardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 314, true);
			this.EditBoardButton.Name = "EditBoardButton";
			this.EditBoardButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 28, true);
			this.EditBoardButton.TabIndex = 4;
			this.EditBoardButton.UseVisualStyleBackColor = true;
			this.EditBoardButton.Click += new System.EventHandler(this.EditBoard_Click);
			// 
			// BMBoardSlideshowForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("f9562b08-918f-4cf5-8d2e-d50291ce611c", "Buffer Management Visual Board Slide Show");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 426, true);
			this.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMBoardSlideshow);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 464, true);
			this.Name = "BMBoardSlideshowForm";
			this.ShouldSerializeTabPageMethods = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Buffer Management Visual Board Slide Show";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BoardsGrid)).EndInit();
			this.BoardsGrid.ResumeLayout(false);
			this.BoardsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox zTextBox1;
		private ZArchitecture.GUI.ZCheckBox zCheckBox1;
		private ZArchitecture.ZGrid BoardsGrid;
		private ZArchitecture.GUI.ZButton RunSlideShowButton;
		private ZArchitecture.GUI.ZButton EditBoardButton;
	}
}