using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise
{
	partial class ZPerformanceForm
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
			makeFactoryReferencesStrong?.Dispose();
			makeFactoryReferencesStrong = null;
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZPerformanceForm));
			this.zCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit2 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.TabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.FactoriesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.zGridFactoryStatistics = new Enterprise.ZArchitecture.ZGrid();
			this.FactoriesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ConstructionStackTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConstructionStackTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ObjectDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ObjectDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FormsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OpenedFormsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DataRefreshBusTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.StaticCacheTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.StaticCacheGrid = new Enterprise.ZArchitecture.ZGrid();
			this.NudgingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.NudgingGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UncollectedTypesTagPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.UncollectedTypesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LeakTrackingEnabledCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TabControl.SuspendLayout();
			this.FactoriesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGridFactoryStatistics)).BeginInit();
			this.zGridFactoryStatistics.SuspendLayout();
			this.FactoriesTabControl.SuspendLayout();
			this.ConstructionStackTabPage.SuspendLayout();
			this.ObjectDetailsTabPage.SuspendLayout();
			this.FormsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OpenedFormsGrid)).BeginInit();
			this.OpenedFormsGrid.SuspendLayout();
			this.DataRefreshBusTabPage.SuspendLayout();
			this.StaticCacheTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.StaticCacheGrid)).BeginInit();
			this.StaticCacheGrid.SuspendLayout();
			this.NudgingTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NudgingGrid)).BeginInit();
			this.NudgingGrid.SuspendLayout();
			this.UncollectedTypesTagPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.UncollectedTypesGrid)).BeginInit();
			this.UncollectedTypesGrid.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 491, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.PerformanceStatisticWithForms);
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "SubscriptionCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.PerformanceStatisticWithForms)(null)).SubscriptionCount)));
			this.zCalcEdit1.DecimalPlaces = 2;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 19, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.zCalcEdit1.TabIndex = 0;
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit2, "CollectionSubscriptionCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.PerformanceStatisticWithForms)(null)).CollectionSubscriptionCount)));
			this.zCalcEdit2.DecimalPlaces = 2;
			this.zCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 46, true);
			this.zCalcEdit2.Name = "zCalcEdit2";
			this.zCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.zCalcEdit2.TabIndex = 1;
			this.zCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zLabel2
			// 
			this.zLabel2.AutoSize = true;
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 49, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 14, true);
			this.zLabel2.TabIndex = 2;
			this.zLabel2.Text = "Collection Subscriptions";
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 22, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 14, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.Text = "Object Subscriptions";
			// 
			// TabControl
			// 
			this.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControl.Controls.Add(this.FactoriesTabPage);
			this.TabControl.Controls.Add(this.FormsTabPage);
			this.TabControl.Controls.Add(this.DataRefreshBusTabPage);
			this.TabControl.Controls.Add(this.StaticCacheTabPage);
			this.TabControl.Controls.Add(this.NudgingTabPage);
			this.TabControl.Controls.Add(this.UncollectedTypesTagPage);
			this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 515, true);
			this.TabControl.TabIndex = 3;
			// 
			// FactoriesTabPage
			// 
			this.FactoriesTabPage.Controls.Add(this.splitContainer1);
			this.FactoriesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.FactoriesTabPage.Name = "FactoriesTabPage";
			this.FactoriesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FactoriesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 492, true);
			this.FactoriesTabPage.TabIndex = 0;
			this.FactoriesTabPage.Text = "Factories";
			this.FactoriesTabPage.UseVisualStyleBackColor = true;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.zGridFactoryStatistics);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.FactoriesTabControl);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(831, 486, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(246);
			this.splitContainer1.TabIndex = 3;
			// 
			// zGridFactoryStatistics
			// 
			this.zGridFactoryStatistics.AllowNavigation = false;
			this.zGridFactoryStatistics.AlternatingBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
			this.BindingSource.SetBindingMember(this.zGridFactoryStatistics, "FactoryStatistics");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).FactoryStatistics)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.EntityFramework.BusinessObjectFactoryStatistic)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).FactoryStatistics)).SyncRoot)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((CargoWise.EntityFramework.BusinessObjectFactoryStatistic)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).FactoryStatistics)).SyncRoot)).FactoryCreationTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CargoWise.EntityFramework.BusinessObjectFactoryStatistic)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).FactoryStatistics)).SyncRoot)).DatabaseLoadCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CargoWise.EntityFramework.BusinessObjectFactoryStatistic)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).FactoryStatistics)).SyncRoot)).BusinessObjectCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CargoWise.EntityFramework.BusinessObjectFactoryStatistic)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).FactoryStatistics)).SyncRoot)).DataRowCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CargoWise.EntityFramework.BusinessObjectFactoryStatistic)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).FactoryStatistics)).SyncRoot)).ActiveFetchHintsCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((CargoWise.EntityFramework.BusinessObjectFactoryStatistic)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).FactoryStatistics)).SyncRoot)).ChildFactoriesCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CargoWise.EntityFramework.BusinessObjectFactoryStatistic)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).FactoryStatistics)).SyncRoot)).FactoryIsDeactivated)));
			this.zGridFactoryStatistics.CaptionText = "Factories and their contents";
			this.zGridFactoryStatistics.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Name";
			zTextBoxColumnStyleInfo1.ColumnName = "Name";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zDateEditColumnStyleInfo1.Caption = "Created";
			zDateEditColumnStyleInfo1.ColumnName = "FactoryCreationTime";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Database Load";
			zCalcEditColumnStyleInfo1.ColumnName = "DatabaseLoadCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Business Objects";
			zCalcEditColumnStyleInfo2.ColumnName = "BusinessObjectCount";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Data Rows";
			zCalcEditColumnStyleInfo3.ColumnName = "DataRowCount";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = "Fetch Hints";
			zCalcEditColumnStyleInfo4.ColumnName = "ActiveFetchHintsCount";
			zCalcEditColumnStyleInfo4.Decimals = 0;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.Caption = "Child Factories";
			zCalcEditColumnStyleInfo5.ColumnName = "ChildFactoriesCount";
			zCalcEditColumnStyleInfo5.Decimals = 0;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.Caption = "Is Deactivated";
			zTextBoxColumnStyleInfo9.ColumnName = "FactoryIsDeactivated";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.zGridFactoryStatistics.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGridFactoryStatistics.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.zGridFactoryStatistics.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.zGridFactoryStatistics.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.zGridFactoryStatistics.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.zGridFactoryStatistics.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.zGridFactoryStatistics.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.zGridFactoryStatistics.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.zGridFactoryStatistics.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGridFactoryStatistics.GridId = "b202cb70-9ae1-4cd9-b33d-ca46afbaec46";
			this.zGridFactoryStatistics.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGridFactoryStatistics.LayoutKey = "zGridFactoryStatistics";
			this.zGridFactoryStatistics.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGridFactoryStatistics.Name = "zGridFactoryStatistics";
			this.zGridFactoryStatistics.ReadOnly = true;
			this.zGridFactoryStatistics.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(831, 246, true);
			this.zGridFactoryStatistics.TabIndex = 2;
			// 
			// FactoriesTabControl
			// 
			this.FactoriesTabControl.Controls.Add(this.ConstructionStackTabPage);
			this.FactoriesTabControl.Controls.Add(this.ObjectDetailsTabPage);
			this.FactoriesTabControl.ItemSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 22, true);
			this.FactoriesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.FactoriesTabControl.Name = "FactoriesTabControl";
			this.FactoriesTabControl.SelectedIndex = 0;
			this.FactoriesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 214, true);
			this.FactoriesTabControl.TabIndex = 0;
			// 
			// ConstructionStackTabPage
			// 
			this.ConstructionStackTabPage.Controls.Add(this.ConstructionStackTextBox);
			this.ConstructionStackTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 25, true);
			this.ConstructionStackTabPage.Name = "ConstructionStackTabPage";
			this.ConstructionStackTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ConstructionStackTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 186, true);
			this.ConstructionStackTabPage.TabIndex = 0;
			this.ConstructionStackTabPage.Text = "Construction Stack";
			this.ConstructionStackTabPage.UseVisualStyleBackColor = true;
			// 
			// ConstructionStackTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConstructionStackTextBox, "FactoryStatistics.AllocationPath");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.EntityFramework.BusinessObjectFactoryStatistic)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).FactoryStatistics)).SyncRoot)).AllocationPath)));
			this.ConstructionStackTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConstructionStackTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConstructionStackTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ConstructionStackTextBox.Multiline = true;
			this.ConstructionStackTextBox.Name = "ConstructionStackTextBox";
			this.ConstructionStackTextBox.ReadOnly = true;
			this.ConstructionStackTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ConstructionStackTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 180, true);
			this.ConstructionStackTextBox.TabIndex = 4;
			// 
			// ObjectDetailsTabPage
			// 
			this.ObjectDetailsTabPage.Controls.Add(this.ObjectDetailsTextBox);
			this.ObjectDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 25, true);
			this.ObjectDetailsTabPage.Name = "ObjectDetailsTabPage";
			this.ObjectDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ObjectDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 172, true);
			this.ObjectDetailsTabPage.TabIndex = 1;
			this.ObjectDetailsTabPage.Text = "Object Details";
			this.ObjectDetailsTabPage.UseVisualStyleBackColor = true;
			// 
			// ObjectDetailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.ObjectDetailsTextBox, "FactoryStatistics.BusinessObjectsInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CargoWise.EntityFramework.BusinessObjectFactoryStatistic)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).FactoryStatistics)).SyncRoot)).BusinessObjectsInformation)));
			this.ObjectDetailsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ObjectDetailsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ObjectDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ObjectDetailsTextBox.Multiline = true;
			this.ObjectDetailsTextBox.Name = "ObjectDetailsTextBox";
			this.ObjectDetailsTextBox.ReadOnly = true;
			this.ObjectDetailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ObjectDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 166, true);
			this.ObjectDetailsTextBox.TabIndex = 5;
			// 
			// FormsTabPage
			// 
			this.FormsTabPage.Controls.Add(this.OpenedFormsGrid);
			this.FormsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.FormsTabPage.Name = "FormsTabPage";
			this.FormsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FormsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 468, true);
			this.FormsTabPage.TabIndex = 1;
			this.FormsTabPage.Text = "Forms";
			this.FormsTabPage.UseVisualStyleBackColor = true;
			// 
			// OpenedFormsGrid
			// 
			this.OpenedFormsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OpenedFormsGrid, "OpenedForms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).OpenedForms)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.FormDetails)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).OpenedForms)).SyncRoot)).FormCaption)));
			this.OpenedFormsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.ColumnName = "FormCaption";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(600);
			this.OpenedFormsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OpenedFormsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OpenedFormsGrid.GridId = "ea64cded-86b0-4066-97fd-73c202da9a41";
			this.OpenedFormsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OpenedFormsGrid.LayoutKey = "OpenedFormsGrid";
			this.OpenedFormsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.OpenedFormsGrid.Name = "OpenedFormsGrid";
			this.OpenedFormsGrid.ReadOnly = true;
			this.OpenedFormsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 458, true);
			this.OpenedFormsGrid.TabIndex = 0;
			// 
			// DataRefreshBusTabPage
			// 
			this.DataRefreshBusTabPage.Controls.Add(this.zCalcEdit1);
			this.DataRefreshBusTabPage.Controls.Add(this.zLabel1);
			this.DataRefreshBusTabPage.Controls.Add(this.zCalcEdit2);
			this.DataRefreshBusTabPage.Controls.Add(this.zLabel2);
			this.DataRefreshBusTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.DataRefreshBusTabPage.Name = "DataRefreshBusTabPage";
			this.DataRefreshBusTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 468, true);
			this.DataRefreshBusTabPage.TabIndex = 2;
			this.DataRefreshBusTabPage.Text = "Data Refresh Bus";
			// 
			// StaticCacheTabPage
			// 
			this.StaticCacheTabPage.Controls.Add(this.StaticCacheGrid);
			this.StaticCacheTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.StaticCacheTabPage.Name = "StaticCacheTabPage";
			this.StaticCacheTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.StaticCacheTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 468, true);
			this.StaticCacheTabPage.TabIndex = 3;
			this.StaticCacheTabPage.Text = "Static Cache";
			this.StaticCacheTabPage.UseVisualStyleBackColor = true;
			// 
			// StaticCacheGrid
			// 
			this.StaticCacheGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.StaticCacheGrid, "StaticCacheContents");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).StaticCacheContents)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.ZArchitecture.Core.StaticCacheDetails)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).StaticCacheContents)).SyncRoot)).ConstructionTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Core.StaticCacheDetails)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).StaticCacheContents)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Core.StaticCacheDetails)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).StaticCacheContents)).SyncRoot)).CleanUpState)));
			this.StaticCacheGrid.CaptionText = "hello";
			this.StaticCacheGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo2.ColumnName = "ConstructionTime";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.ColumnName = "Description";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(600);
			zTextBoxColumnStyleInfo4.ColumnName = "CleanUpState";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.StaticCacheGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.StaticCacheGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.StaticCacheGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.StaticCacheGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StaticCacheGrid.GridId = "11aa1145-792b-4ab1-ad3e-01377b48d4a2";
			this.StaticCacheGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.StaticCacheGrid.IsWholeRowSelectedOnClick = true;
			this.StaticCacheGrid.LayoutKey = "OpenedFormsGrid";
			this.StaticCacheGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.StaticCacheGrid.Name = "StaticCacheGrid";
			this.StaticCacheGrid.ReadOnly = true;
			this.StaticCacheGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 458, true);
			this.StaticCacheGrid.TabIndex = 2;
			// 
			// NudgingTabPage
			// 
			this.NudgingTabPage.Controls.Add(this.zLabel4);
			this.NudgingTabPage.Controls.Add(this.NudgingGrid);
			this.NudgingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.NudgingTabPage.Name = "NudgingTabPage";
			this.NudgingTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.NudgingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 492, true);
			this.NudgingTabPage.TabIndex = 5;
			this.NudgingTabPage.Text = "Service Task Binding";
			// 
			// zLabel4
			// 
			this.zLabel4.AutoSize = true;
			this.zLabel4.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 22, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 14, true);
			this.zLabel4.TabIndex = 0;
			this.zLabel4.Text = "Business Object to Service Task Binding is currenty disabled.  Please raise an incident if you would like it to be enabled.";
			this.zLabel4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// NudgingGrid
			// 
			this.NudgingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.NudgingGrid, "NudgeEventsContents");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).NudgeEventsContents)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.ZArchitecture.Core.NudgeEventDetails)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).NudgeEventsContents)).SyncRoot)).EventTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Core.NudgeEventDetails)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).NudgeEventsContents)).SyncRoot)).EventType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Core.NudgeEventDetails)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).NudgeEventsContents)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Core.NudgeEventDetails)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).NudgeEventsContents)).SyncRoot)).Tasks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ZArchitecture.Core.NudgeEventDetails)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).NudgeEventsContents)).SyncRoot)).RetriesRemaining)));
			this.NudgingGrid.CaptionText = "hello";
			this.NudgingGrid.CaptionVisible = false;
			zDateEditColumnStyleInfo3.ColumnName = "EventTime";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.ColumnName = "EventType";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo6.ColumnName = "Description";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo7.ColumnName = "Tasks";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "RetriesRemaining";
			zCalcEditColumnStyleInfo6.Decimals = 0;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.NudgingGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.NudgingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.NudgingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.NudgingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.NudgingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.NudgingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NudgingGrid.GridId = "98575145-804F-44E1-A0E7-FF0149BDC8CD";
			this.NudgingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NudgingGrid.IsWholeRowSelectedOnClick = true;
			this.NudgingGrid.LayoutKey = "OpenedFormsGrid";
			this.NudgingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.NudgingGrid.Name = "NudgingGrid";
			this.NudgingGrid.ReadOnly = true;
			this.NudgingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(831, 486, true);
			this.NudgingGrid.TabIndex = 2;
			// 
			// UncollectedTypesTagPage
			// 
			this.UncollectedTypesTagPage.Controls.Add(this.UncollectedTypesGrid);
			this.UncollectedTypesTagPage.Controls.Add(this.zPanel1);
			this.UncollectedTypesTagPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.UncollectedTypesTagPage.Name = "UncollectedTypesTagPage";
			this.UncollectedTypesTagPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.UncollectedTypesTagPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 468, true);
			this.UncollectedTypesTagPage.TabIndex = 4;
			this.UncollectedTypesTagPage.Text = "Uncollected Types";
			this.UncollectedTypesTagPage.UseVisualStyleBackColor = true;
			// 
			// UncollectedTypesGrid
			// 
			this.UncollectedTypesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.UncollectedTypesGrid, "UncollectedTypes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).UncollectedTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Core.UncollectedType)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).UncollectedTypes)).SyncRoot)).TypeName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ZArchitecture.Core.UncollectedType)(((System.Collections.IList)(((Enterprise.PerformanceStatisticWithForms)(null)).UncollectedTypes)).SyncRoot)).Quantity)));
			this.UncollectedTypesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo8.ColumnName = "TypeName";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "Quantity";
			zCalcEditColumnStyleInfo7.Decimals = 0;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.UncollectedTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.UncollectedTypesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.UncollectedTypesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UncollectedTypesGrid.GridId = "3b729d67-ffae-4df0-ba63-c683e84c2ff0";
			this.UncollectedTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UncollectedTypesGrid.LayoutKey = "zGrid1";
			this.UncollectedTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 103, true);
			this.UncollectedTypesGrid.Name = "UncollectedTypesGrid";
			this.UncollectedTypesGrid.ReadOnly = true;
			this.UncollectedTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 358, true);
			this.UncollectedTypesGrid.TabIndex = 0;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.LeakTrackingEnabledCheckBox);
			this.zPanel1.Controls.Add(this.zLabel3);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 100, true);
			this.zPanel1.TabIndex = 1;
			// 
			// LeakTrackingEnabledCheckBox
			// 
			this.LeakTrackingEnabledCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.LeakTrackingEnabledCheckBox, "LeakTrackingEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.PerformanceStatisticWithForms)(null)).LeakTrackingEnabled)));
			this.LeakTrackingEnabledCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LeakTrackingEnabledCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(528, 7, true);
			this.LeakTrackingEnabledCheckBox.Name = "LeakTrackingEnabledCheckBox";
			this.LeakTrackingEnabledCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 17, true);
			this.LeakTrackingEnabledCheckBox.TabIndex = 1;
			this.LeakTrackingEnabledCheckBox.Text = "Leak Tracking Enabled";
			this.LeakTrackingEnabledCheckBox.UseVisualStyleBackColor = true;
			// 
			// zLabel3
			// 
			this.zLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 8, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(512, 77, true);
			this.zLabel3.TabIndex = 0;
			this.zLabel3.Text = resources.GetString("zLabel3.Text");
			this.zLabel3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// ZPerformanceForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(844, 515, true);
			this.Controls.Add(this.TabControl);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
			this.DataSourceType = typeof(Enterprise.PerformanceStatisticWithForms);
			this.DataSourceTypeName = "Enterprise.PerformanceStatistic";
			this.Name = "ZPerformanceForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "Application Performance Statistics";
			this.Controls.SetChildIndex(this.TabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.TabControl.PerformLayout();
			this.FactoriesTabPage.ResumeLayout(false);
			this.FactoriesTabPage.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGridFactoryStatistics)).EndInit();
			this.zGridFactoryStatistics.ResumeLayout(false);
			this.zGridFactoryStatistics.PerformLayout();
			this.FactoriesTabControl.ResumeLayout(false);
			this.FactoriesTabControl.PerformLayout();
			this.ConstructionStackTabPage.ResumeLayout(false);
			this.ConstructionStackTabPage.PerformLayout();
			this.ObjectDetailsTabPage.ResumeLayout(false);
			this.ObjectDetailsTabPage.PerformLayout();
			this.FormsTabPage.ResumeLayout(false);
			this.FormsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OpenedFormsGrid)).EndInit();
			this.OpenedFormsGrid.ResumeLayout(false);
			this.OpenedFormsGrid.PerformLayout();
			this.DataRefreshBusTabPage.ResumeLayout(false);
			this.DataRefreshBusTabPage.PerformLayout();
			this.StaticCacheTabPage.ResumeLayout(false);
			this.StaticCacheTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.StaticCacheGrid)).EndInit();
			this.StaticCacheGrid.ResumeLayout(false);
			this.StaticCacheGrid.PerformLayout();
			this.NudgingTabPage.ResumeLayout(false);
			this.NudgingTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NudgingGrid)).EndInit();
			this.NudgingGrid.ResumeLayout(false);
			this.NudgingGrid.PerformLayout();
			this.UncollectedTypesTagPage.ResumeLayout(false);
			this.UncollectedTypesTagPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.UncollectedTypesGrid)).EndInit();
			this.UncollectedTypesGrid.ResumeLayout(false);
			this.UncollectedTypesGrid.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZLabel zLabel2;
		private ZLabel zLabel1;
		private ZCalcEdit zCalcEdit1;
		private ZCalcEdit zCalcEdit2;
		private ZTabControl TabControl;
		private ZTabPage FactoriesTabPage;
		private ZTabPage FormsTabPage;
		private ZTabPage DataRefreshBusTabPage;
		private ZTabPage StaticCacheTabPage;
		private ZTabPage NudgingTabPage;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private ZTabPage UncollectedTypesTagPage;
		private ZPanel zPanel1;
		private ZLabel zLabel3;
		private ZLabel zLabel4;
		private ZCheckBox LeakTrackingEnabledCheckBox;
		private ZTabControl FactoriesTabControl;
		private ZTabPage ConstructionStackTabPage;
		private ZTextBox ConstructionStackTextBox;
		private ZTabPage ObjectDetailsTabPage;
		private ZTextBox ObjectDetailsTextBox;
		private ZGrid zGridFactoryStatistics;
		private ZGrid OpenedFormsGrid;
		private ZGrid StaticCacheGrid;
		private ZGrid NudgingGrid;
		private ZGrid UncollectedTypesGrid;
	}
}
