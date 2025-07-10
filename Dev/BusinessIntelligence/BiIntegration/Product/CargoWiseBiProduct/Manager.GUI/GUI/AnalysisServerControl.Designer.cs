using CargoWise.Bi.Product.Manager.Business;
using System;

namespace CargoWise.Bi.Product.Manager.GUI
{
	partial class AnalysisServerControl
	{
		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.ssasInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.totalEstimatedMemoryUsageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.serverModeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.serverVersionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.serverTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zTabControl1 = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ssasCubesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ssasCubesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.refreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ssasInfoGroupBox.SuspendLayout();
			this.zTabControl1.SuspendLayout();
			this.ssasCubesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ssasCubesGrid)).BeginInit();
			this.ssasCubesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CargoWise.Bi.Product.Manager.Business.AnalysisServerInformation);
			// 
			// ssasInfoGroupBox
			// 
			this.ssasInfoGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ssasInfoGroupBox.BackColor = System.Drawing.SystemColors.Control;
			this.ssasInfoGroupBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("3FE52B8E-EC6D-484C-8494-ACA405F59C46", "Analysis Server Info");
			this.ssasInfoGroupBox.Controls.Add(this.totalEstimatedMemoryUsageTextBox);
			this.ssasInfoGroupBox.Controls.Add(this.serverModeTextBox);
			this.ssasInfoGroupBox.Controls.Add(this.serverVersionTextBox);
			this.ssasInfoGroupBox.Controls.Add(this.serverTextBox);
			this.ssasInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 8, true);
			this.ssasInfoGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(40, true);
			this.ssasInfoGroupBox.Name = "ssasInfoGroupBox";
			this.ssasInfoGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(40, true);
			this.ssasInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 94, true);
			this.ssasInfoGroupBox.TabIndex = 1;
			this.ssasInfoGroupBox.TabStop = false;
			// 
			// totalEstimatedMemoryUsageTextBox
			// 
			this.totalEstimatedMemoryUsageTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.totalEstimatedMemoryUsageTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.totalEstimatedMemoryUsageTextBox, "TotalEstimatedMemoryUsage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.AnalysisServerInformation)(null)).TotalEstimatedMemoryUsage)));
			this.totalEstimatedMemoryUsageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.totalEstimatedMemoryUsageTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("ca9f0c0b-ea28-40cb-a05c-d2958d9b14ba", "Estimated Memory Usage");
			this.totalEstimatedMemoryUsageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.totalEstimatedMemoryUsageTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.totalEstimatedMemoryUsageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 71, true);
			this.totalEstimatedMemoryUsageTextBox.Name = "totalEstimatedMemoryUsageTextBox";
			this.totalEstimatedMemoryUsageTextBox.ReadOnly = true;
			this.totalEstimatedMemoryUsageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 12, true);
			this.totalEstimatedMemoryUsageTextBox.TabIndex = 4;
			// 
			// serverModeTextBox
			// 
			this.serverModeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.serverModeTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.serverModeTextBox, "ServerMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.AnalysisServerInformation)(null)).ServerMode)));
			this.serverModeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.serverModeTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("fa901a74-7a20-417f-a393-b364ddea26dc", "Server Mode");
			this.serverModeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.serverModeTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.serverModeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 54, true);
			this.serverModeTextBox.Name = "serverModeTextBox";
			this.serverModeTextBox.ReadOnly = true;
			this.serverModeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 12, true);
			this.serverModeTextBox.TabIndex = 3;
			// 
			// serverVersionTextBox
			// 
			this.serverVersionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.serverVersionTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.serverVersionTextBox, "ServerVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.AnalysisServerInformation)(null)).ServerVersion)));
			this.serverVersionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.serverVersionTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("f8bdbbd6-0d3e-4f85-ab79-02222013adf3", "Analysis Server Version");
			this.serverVersionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.serverVersionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.serverVersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 37, true);
			this.serverVersionTextBox.Name = "serverVersionTextBox";
			this.serverVersionTextBox.ReadOnly = true;
			this.serverVersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 12, true);
			this.serverVersionTextBox.TabIndex = 2;
			// 
			// serverTextBox
			// 
			this.serverTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.serverTextBox.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.serverTextBox, "ServerName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((CargoWise.Bi.Product.Manager.Business.AnalysisServerInformation)(null)).ServerName)));
			this.serverTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.serverTextBox.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("2e98c01b-7f47-4ab6-850c-1cdb19dbf3f4", "Analysis Server Name");
			this.serverTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.serverTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.serverTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 18, true);
			this.serverTextBox.Name = "serverTextBox";
			this.serverTextBox.ReadOnly = true;
			this.serverTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 12, true);
			this.serverTextBox.TabIndex = 1;
			// 
			// zTabControl1
			// 
			this.zTabControl1.Controls.Add(this.ssasCubesTabPage);
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 122, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 494, true);
			this.zTabControl1.TabIndex = 2;
			// 
			// ssasCubesTabPage
			// 
			this.ssasCubesTabPage.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("9061c100-9d73-4aae-aa4d-fda29d1137d2", "SSAS Cubes");
			this.ssasCubesTabPage.Controls.Add(this.ssasCubesGrid);
			this.ssasCubesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.ssasCubesTabPage.Name = "ssasCubesTabPage";
			this.ssasCubesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ssasCubesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 471, true);
			this.ssasCubesTabPage.TabIndex = 0;
			this.ssasCubesTabPage.Text = "SsasCubes";
			this.ssasCubesTabPage.UseVisualStyleBackColor = true;
			// 
			// ssasCubesGrid
			// 
			this.ssasCubesGrid.AllowDrop = true;
			this.ssasCubesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ssasCubesGrid, "SsasCubes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.AnalysisServerInformation)(null)).SsasCubes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.Bi.Product.Manager.Business.SsasCube)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.AnalysisServerInformation)(null)).SsasCubes)).SyncRoot)).ModelFileName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.Bi.Product.Manager.Business.SsasCube)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.AnalysisServerInformation)(null)).SsasCubes)).SyncRoot)).ModelVersion)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CargoWise.Bi.Product.Manager.Business.SsasCube)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.AnalysisServerInformation)(null)).SsasCubes)).SyncRoot)).DeployToServer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CargoWise.Bi.Product.Manager.Business.SsasCube)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.AnalysisServerInformation)(null)).SsasCubes)).SyncRoot)).RedeployOnNextBID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CargoWise.Bi.Product.Manager.Business.SsasCube)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.AnalysisServerInformation)(null)).SsasCubes)).SyncRoot)).MemoryUsage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.Bi.Product.Manager.Business.SsasCube)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.AnalysisServerInformation)(null)).SsasCubes)).SyncRoot)).IsCubeProcessing)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((CargoWise.Bi.Product.Manager.Business.SsasCube)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.AnalysisServerInformation)(null)).SsasCubes)).SyncRoot)).LastProcessingFinishDateTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((CargoWise.Bi.Product.Manager.Business.SsasCube)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.AnalysisServerInformation)(null)).SsasCubes)).SyncRoot)).LastProcessingStartDateTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((CargoWise.Bi.Product.Manager.Business.SsasCube)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.AnalysisServerInformation)(null)).SsasCubes)).SyncRoot)).LastSourceLsnDateTimeLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((CargoWise.Bi.Product.Manager.Business.SsasCube)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.AnalysisServerInformation)(null)).SsasCubes)).SyncRoot)).LastResetModelInfoLocal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CargoWise.Bi.Product.Manager.Business.SsasCube)(((System.Collections.IList)(((CargoWise.Bi.Product.Manager.Business.AnalysisServerInformation)(null)).SsasCubes)).SyncRoot)).EnableEtl)));
			this.ssasCubesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("6b7c5b58-6e84-4357-8789-3021be5da5ba", "Model File Name");
			zTextBoxColumnStyleInfo1.ColumnName = "ModelFileName";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("a9b2b238-b87a-4f3d-905f-23b6cfa3078b", "Model Version");
			zTextBoxColumnStyleInfo2.ColumnName = "ModelVersion";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("0c0d1a47-d4a5-4d2e-b104-16ff25097387", "Deployed");
			zCheckBoxColumnStyleInfo1.ColumnName = "DeployToServer";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("96090dfb-5131-49a8-83bf-0399cf7649df", "To Redeploy");
			zCheckBoxColumnStyleInfo2.ColumnName = "RedeployOnNextBID";
			zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo2.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "MemoryUsage";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.ColumnName = "IsCubeProcessing";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("bec19489-997d-4627-8022-86494954eca4", "Last Process Finish");
			zDateEditColumnStyleInfo1.ColumnName = "LastProcessingFinishDateTimeLocal";
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("4d602769-0d91-4630-a9c8-06c35a022574", "Last Process Start");
			zDateEditColumnStyleInfo2.ColumnName = "LastProcessingStartDateTimeLocal";
			zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo3.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("7a60d458-ae9c-47db-a664-c20a2525b3d8", "Last Source LSN");
			zDateEditColumnStyleInfo3.ColumnName = "LastSourceLsnDateTimeLocal";
			zDateEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo4.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("87fde7b3-9655-4b63-8713-290a236493c7", "Last Reset");
			zDateEditColumnStyleInfo4.ColumnName = "LastResetModelInfoLocal";
			zDateEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo4.IsReadOnly = true;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("bdfac849-d6a5-410c-841b-844ba7d9134c", "Enable ETL");
			zCheckBoxColumnStyleInfo3.ColumnName = "EnableEtl";
			zCheckBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo3.IsReadOnly = true;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ssasCubesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ssasCubesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ssasCubesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ssasCubesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ssasCubesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ssasCubesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ssasCubesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ssasCubesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ssasCubesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.ssasCubesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.ssasCubesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.ssasCubesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ssasCubesGrid.GridId = "01885d58-9411-45c6-8a61-6f351c757f89";
			this.ssasCubesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ssasCubesGrid.LayoutKey = "ssasCubesGrid";
			this.ssasCubesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ssasCubesGrid.Name = "ssasCubesGrid";
			this.ssasCubesGrid.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, true);
			this.ssasCubesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 465, true);
			this.ssasCubesGrid.TabIndex = 0;
			this.ssasCubesGrid.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ssasCubesGrid_MouseClick);
			// 
			// refreshButton
			// 
			this.refreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.refreshButton.CaptionResourceString = CargoWise.Bi.Product.Manager.GUI.Res.GetData("9684b638-3d65-44c7-8462-b4103e4d0c95", "Refresh");
			this.refreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(559, 623, true);
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 23, true);
			this.refreshButton.TabIndex = 3;
			this.refreshButton.ToolTipCaption = null;
			this.refreshButton.UseVisualStyleBackColor = true;
			this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
			// 
			// AnalysisServerControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ssasInfoGroupBox);
			this.Controls.Add(this.zTabControl1);
			this.Controls.Add(this.refreshButton);
			this.Name = "AnalysisServerControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 657, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ssasInfoGroupBox.ResumeLayout(false);
			this.ssasInfoGroupBox.PerformLayout();
			this.zTabControl1.ResumeLayout(false);
			this.zTabControl1.PerformLayout();
			this.ssasCubesTabPage.ResumeLayout(false);
			this.ssasCubesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ssasCubesGrid)).EndInit();
			this.ssasCubesGrid.ResumeLayout(false);
			this.ssasCubesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox ssasInfoGroupBox;
		private Enterprise.ZArchitecture.ZTextBox serverTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton refreshButton;
		private Enterprise.ZArchitecture.GUI.ZTabControl zTabControl1;
		private Enterprise.ZArchitecture.GUI.ZTabPage ssasCubesTabPage;
		public Enterprise.ZArchitecture.ZGrid ssasCubesGrid;
		private Enterprise.ZArchitecture.ZTextBox serverModeTextBox;
		private Enterprise.ZArchitecture.ZTextBox serverVersionTextBox;
		private System.ComponentModel.IContainer components;
		private Enterprise.ZArchitecture.ZTextBox totalEstimatedMemoryUsageTextBox;
	}
}
