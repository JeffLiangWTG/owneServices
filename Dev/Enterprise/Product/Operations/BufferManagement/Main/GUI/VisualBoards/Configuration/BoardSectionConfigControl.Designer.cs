namespace Enterprise.BufferManagement.GUI
{
	partial class BoardSectionConfigControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.SectionConfigSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.BoardSectionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SectionConfigTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ConfigTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ConfigurationControlPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SectionLayoutConfigGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ForeColorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BackColorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ColWidthPercentCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RowHeightPercentCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ColSpanCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RowSpanCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ColumnCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RowCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SectionConfigSplitContainer)).BeginInit();
			this.SectionConfigSplitContainer.Panel1.SuspendLayout();
			this.SectionConfigSplitContainer.Panel2.SuspendLayout();
			this.SectionConfigSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BoardSectionsGrid)).BeginInit();
			this.SectionConfigTabControl.SuspendLayout();
			this.ConfigTabPage.SuspendLayout();
			this.SectionLayoutConfigGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMBoard);
			// 
			// SectionConfigSplitContainer
			// 
			this.SectionConfigSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SectionConfigSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SectionConfigSplitContainer.Name = "SectionConfigSplitContainer";
			// 
			// SectionConfigSplitContainer.Panel1
			// 
			this.SectionConfigSplitContainer.Panel1.Controls.Add(this.BoardSectionsGrid);
			// 
			// SectionConfigSplitContainer.Panel2
			// 
			this.SectionConfigSplitContainer.Panel2.Controls.Add(this.SectionConfigTabControl);
			this.SectionConfigSplitContainer.Panel2MinSize = 470;
			this.SectionConfigSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 423, true);
			this.SectionConfigSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(102);
			this.SectionConfigSplitContainer.TabIndex = 6;
			// 
			// BoardSectionsGrid
			// 
			this.BoardSectionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BoardSectionsGrid, "Sections");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).MS_SectionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).MS_FC_Component)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).SectionName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).DisplaySequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).Row)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).Column)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).RowSpan)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).ColSpan)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).RowHeightPercent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).ColWidthPercent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).BackgroundColor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).ForegroundColor)));
			this.BoardSectionsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "MS_SectionType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "MS_FC_Component";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo1.ColumnName = "SectionName";
			zTextBoxColumnStyleInfo1.IsVisible = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "DisplaySequence";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "Row";
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "Column";
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "RowSpan";
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "ColSpan";
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "RowHeightPercent";
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "ColWidthPercent";
			zCalcEditColumnStyleInfo7.IsVisible = false;
			zDropEditColumnStyleInfo2.ColumnName = "BackgroundColor";
			zDropEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo3.ColumnName = "ForegroundColor";
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo3.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.BoardSectionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.BoardSectionsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.BoardSectionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.BoardSectionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.BoardSectionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.BoardSectionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.BoardSectionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.BoardSectionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.BoardSectionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.BoardSectionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.BoardSectionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.BoardSectionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.BoardSectionsGrid.CopySelectedRowsAllowed = true;
			this.BoardSectionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BoardSectionsGrid.GridId = "6dbde402-79c8-4a74-9e03-ae73109dc3c7";
			this.BoardSectionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BoardSectionsGrid.LayoutKey = "BoardSectionsGrid";
			this.BoardSectionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BoardSectionsGrid.Name = "BoardSectionsGrid";
			this.BoardSectionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 423, true);
			this.BoardSectionsGrid.TabIndex = 0;
			// 
			// SectionConfigTabControl
			// 
			this.SectionConfigTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.SectionConfigTabControl.Controls.Add(this.ConfigTabPage);
			this.SectionConfigTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SectionConfigTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SectionConfigTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 0, true);
			this.SectionConfigTabControl.Name = "SectionConfigTabControl";
			this.SectionConfigTabControl.SelectedIndex = 0;
			this.SectionConfigTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(647, 423, true);
			this.SectionConfigTabControl.TabIndex = 4;
			// 
			// ConfigTabPage
			// 
			this.ConfigTabPage.BackColor = System.Drawing.Color.Transparent;
			this.ConfigTabPage.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("4372de06-9bfa-4948-953b-3ce44ce7410f", "Configuration");
			this.ConfigTabPage.Controls.Add(this.ConfigurationControlPanel);
			this.ConfigTabPage.Controls.Add(this.SectionLayoutConfigGroupBox);
			this.ConfigTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ConfigTabPage.Name = "ConfigTabPage";
			this.ConfigTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ConfigTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(639, 396, true);
			this.ConfigTabPage.TabIndex = 0;
			this.ConfigTabPage.UseVisualStyleBackColor = true;
			// 
			// ConfigurationControlPanel
			// 
			this.ConfigurationControlPanel.AutoScroll = true;
			this.ConfigurationControlPanel.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 450);
			this.ConfigurationControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfigurationControlPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 102, true);
			this.ConfigurationControlPanel.Name = "ConfigurationControlPanel";
			this.ConfigurationControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 291, true);
			this.ConfigurationControlPanel.TabIndex = 2;
			// 
			// SectionLayoutConfigGroupBox
			// 
			this.SectionLayoutConfigGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("bb3c8da7-396b-482a-8dd5-d0ebbe1ff77e", "Section Layout");
			this.SectionLayoutConfigGroupBox.Controls.Add(this.ForeColorDropEdit);
			this.SectionLayoutConfigGroupBox.Controls.Add(this.BackColorDropEdit);
			this.SectionLayoutConfigGroupBox.Controls.Add(this.ColWidthPercentCalcEdit);
			this.SectionLayoutConfigGroupBox.Controls.Add(this.RowHeightPercentCalcEdit);
			this.SectionLayoutConfigGroupBox.Controls.Add(this.ColSpanCalcEdit);
			this.SectionLayoutConfigGroupBox.Controls.Add(this.RowSpanCalcEdit);
			this.SectionLayoutConfigGroupBox.Controls.Add(this.ColumnCalcEdit);
			this.SectionLayoutConfigGroupBox.Controls.Add(this.RowCalcEdit);
			this.SectionLayoutConfigGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.SectionLayoutConfigGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SectionLayoutConfigGroupBox.Name = "SectionLayoutConfigGroupBox";
			this.SectionLayoutConfigGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 99, true);
			this.SectionLayoutConfigGroupBox.TabIndex = 1;
			this.SectionLayoutConfigGroupBox.TabStop = false;
			// 
			// ForeColorDropEdit
			// 
			this.ForeColorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ForeColorDropEdit, "Sections.ForegroundColor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).ForegroundColor)));
			this.ForeColorDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ForeColorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(254, 69, true);
			this.ForeColorDropEdit.Name = "ForeColorDropEdit";
			this.ForeColorDropEdit.ShowDescriptionBox = false;
			this.ForeColorDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ForeColorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ForeColorDropEdit.TabIndex = 15;
			// 
			// BackColorDropEdit
			// 
			this.BackColorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BackColorDropEdit, "Sections.BackgroundColor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).BackgroundColor)));
			this.BackColorDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BackColorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 69, true);
			this.BackColorDropEdit.Name = "BackColorDropEdit";
			this.BackColorDropEdit.ShowDescriptionBox = false;
			this.BackColorDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.BackColorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.BackColorDropEdit.TabIndex = 14;
			// 
			// zCalcEdit5
			// 
			this.BindingSource.SetBindingMember(this.ColWidthPercentCalcEdit, "Sections.ColWidthPercent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).ColWidthPercent)));
			this.ColWidthPercentCalcEdit.DecimalPlaces = 2;
			this.ColWidthPercentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 43, true);
			this.ColWidthPercentCalcEdit.MaxValue = new decimal(new int[] {
						0,
						0,
						0,
						0});
			this.ColWidthPercentCalcEdit.Name = "ColWidthPercentCalcEdit";
			this.ColWidthPercentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.ColWidthPercentCalcEdit.TabIndex = 5;
			this.ColWidthPercentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit4
			// 
			this.BindingSource.SetBindingMember(this.RowHeightPercentCalcEdit, "Sections.RowHeightPercent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).RowHeightPercent)));
			this.RowHeightPercentCalcEdit.DecimalPlaces = 2;
			this.RowHeightPercentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 17, true);
			this.RowHeightPercentCalcEdit.MaxValue = new decimal(new int[] {
						0,
						0,
						0,
						0});
			this.RowHeightPercentCalcEdit.Name = "RowHeightPercentCalcEdit";
			this.RowHeightPercentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RowHeightPercentCalcEdit.TabIndex = 4;
			this.RowHeightPercentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit3
			// 
			this.BindingSource.SetBindingMember(this.ColSpanCalcEdit, "Sections.ColSpan");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).ColSpan)));
			this.ColSpanCalcEdit.DecimalPlaces = 2;
			this.ColSpanCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 43, true);
			this.ColSpanCalcEdit.MaxValue = new decimal(new int[] {
						0,
						0,
						0,
						0});
			this.ColSpanCalcEdit.Name = "ColSpanCalcEdit";
			this.ColSpanCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.ColSpanCalcEdit.TabIndex = 3;
			this.ColSpanCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.RowSpanCalcEdit, "Sections.RowSpan");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).RowSpan)));
			this.RowSpanCalcEdit.DecimalPlaces = 2;
			this.RowSpanCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 17, true);
			this.RowSpanCalcEdit.MaxValue = new decimal(new int[] {
						0,
						0,
						0,
						0});
			this.RowSpanCalcEdit.Name = "RowSpanCalcEdit";
			this.RowSpanCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RowSpanCalcEdit.TabIndex = 2;
			this.RowSpanCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.ColumnCalcEdit, "Sections.Column");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).Column)));
			this.ColumnCalcEdit.DecimalPlaces = 2;
			this.ColumnCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(54, 43, true);
			this.ColumnCalcEdit.MaxValue = new decimal(new int[] {
						0,
						0,
						0,
						0});
			this.ColumnCalcEdit.Name = "ColumnCalcEdit";
			this.ColumnCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.ColumnCalcEdit.TabIndex = 1;
			this.ColumnCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RowCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RowCalcEdit, "Sections.Row");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMBoardSection)(((System.Collections.IList)(((Enterprise.BufferManagement.Business.BMBoard)(null)).Sections)).SyncRoot)).Row)));
			this.RowCalcEdit.DecimalPlaces = 2;
			this.RowCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(54, 17, true);
			this.RowCalcEdit.MaxValue = new decimal(new int[] {
						0,
						0,
						0,
						0});
			this.RowCalcEdit.Name = "RowCalcEdit";
			this.RowCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.RowCalcEdit.TabIndex = 0;
			this.RowCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BoardSectionConfigControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SectionConfigSplitContainer);
			this.Name = "BoardSectionConfigControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(753, 423, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SectionConfigSplitContainer.Panel1.ResumeLayout(false);
			this.SectionConfigSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SectionConfigSplitContainer)).EndInit();
			this.SectionConfigSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BoardSectionsGrid)).EndInit();
			this.SectionConfigTabControl.ResumeLayout(false);
			this.ConfigTabPage.ResumeLayout(false);
			this.SectionLayoutConfigGroupBox.ResumeLayout(false);
			this.SectionLayoutConfigGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer SectionConfigSplitContainer;
#if DEBUG
		public
#else
			internal
#endif
 ZArchitecture.ZGrid BoardSectionsGrid;
		public ZArchitecture.GUI.ZTabControl SectionConfigTabControl;
		private ZArchitecture.GUI.ZTabPage ConfigTabPage;
		private ZArchitecture.GUI.ZGroupBox SectionLayoutConfigGroupBox;
		private ZArchitecture.ZCalcEdit ColWidthPercentCalcEdit;
		private ZArchitecture.ZCalcEdit RowHeightPercentCalcEdit;
		private ZArchitecture.ZCalcEdit ColSpanCalcEdit;
		private ZArchitecture.ZCalcEdit RowSpanCalcEdit;
		private ZArchitecture.ZCalcEdit ColumnCalcEdit;
		private ZArchitecture.ZCalcEdit RowCalcEdit;
		private ZArchitecture.GUI.ZDropEdit BackColorDropEdit;
		private ZArchitecture.GUI.ZDropEdit ForeColorDropEdit;
		private ZArchitecture.GUI.ZPanel ConfigurationControlPanel;
	}
}
