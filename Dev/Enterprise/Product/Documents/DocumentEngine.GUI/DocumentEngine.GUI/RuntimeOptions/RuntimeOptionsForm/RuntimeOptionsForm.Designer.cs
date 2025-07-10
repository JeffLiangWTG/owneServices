using Enterprise.ZArchitecture;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class RuntimeOptionsForm
	{
		#region Windows Form Designer generated code

		internal AutoLayoutGroupBox SortOrderGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox ErrorsGroupBox;
		internal AutoLayoutGroupBox UserDefinedFieldsGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZPanel FooterPanel;
		internal Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		internal Enterprise.ZArchitecture.GUI.ZButton ActionButton;
		ZArchitecture.GUI.ZPanel ErrorsPanel;
		ZGrid ErrorsGridView;
		internal AutoLayoutGroupBox GroupByGroupBox;
		internal Enterprise.DocumentEngine.GUI.RuntimeOptions.AutoLayoutGroupBox OptionalTemplatesGroupBox;
		internal Enterprise.DocumentEngine.GUI.RuntimeOptions.AutoLayoutGroupBoxTabControl FilterTabControl;
		Enterprise.ZArchitecture.ZLabel errorLabel;
		AutoLayoutGroupBox ColumnArrangementGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZLinkLabel CreateShortcutLinkLabel;
		internal CargoWise.Windows.UI.KContextMenuStrip CreateShortcutContextMenu;
		internal Enterprise.ZArchitecture.GUI.ZToolStripMenuItem CreateDesktopShortcutMenuItem;
		internal Enterprise.ZArchitecture.GUI.ZToolStripMenuItem CreateHyperlinkMenuItem;
		protected internal Enterprise.ZArchitecture.GUI.ZDropEdit LanguageZDropEdit;
		private System.ComponentModel.IContainer components;
		internal ZArchitecture.ZCalcEdit MaxDopEdit;

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ErrorsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ErrorsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ErrorsGridView = new ZGrid();
			this.errorLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FooterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TimeOutEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LabelReportLanguage = new Enterprise.ZArchitecture.ZLabel();
			this.LanguageZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LabelReportOrientation = new Enterprise.ZArchitecture.ZLabel();
			this.DropEditReportOrientation = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EdwDataSourceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CopyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PreviewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CreateShortcutLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.ActionButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MaxDopEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CreateShortcutContextMenu = new CargoWise.Windows.UI.KContextMenuStrip(this.components);
			this.CreateDesktopShortcutMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.CreateHyperlinkMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.FilterTabControl = new Enterprise.DocumentEngine.GUI.RuntimeOptions.AutoLayoutGroupBoxTabControl();
			this.OptionalTemplatesGroupBox = new Enterprise.DocumentEngine.GUI.RuntimeOptions.AutoLayoutGroupBox();
			this.UserDefinedFieldsGroupBox = new Enterprise.DocumentEngine.GUI.RuntimeOptions.AutoLayoutGroupBox();
			this.GroupByGroupBox = new Enterprise.DocumentEngine.GUI.RuntimeOptions.AutoLayoutGroupBox();
			this.SortOrderGroupBox = new Enterprise.DocumentEngine.GUI.RuntimeOptions.AutoLayoutGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ErrorsGroupBox.SuspendLayout();
			this.ErrorsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ErrorsGridView)).BeginInit();
			this.ErrorsGridView.SuspendLayout();
			this.FooterPanel.SuspendLayout();
			this.LanguageZDropEdit.SuspendLayout();
			this.DropEditReportOrientation.SuspendLayout();
			this.EdwDataSourceCheckBox.SuspendLayout();
			this.CreateShortcutContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 670, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 22, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(292);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.Report);
			// 
			// ErrorsGroupBox
			// 
			this.ErrorsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ErrorsGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("RuntimeOptionsForm|e159d4f1-e38a-4db3-a592-9b8ede96def6", "Errors were found in the template!");
			this.ErrorsGroupBox.Controls.Add(this.ErrorsPanel);
			this.ErrorsGroupBox.Controls.Add(this.errorLabel);
			this.ErrorsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.ErrorsGroupBox.Name = "ErrorsGroupBox";
			this.ErrorsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 290, true);
			this.ErrorsGroupBox.TabIndex = 0;
			this.ErrorsGroupBox.TabStop = false;
			// 
			// ErrorsPanel
			// 
			this.ErrorsPanel.AutoScroll = true;
			this.ErrorsPanel.Controls.Add(this.ErrorsGridView);
			this.ErrorsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ErrorsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 96, true);
			this.ErrorsPanel.Name = "ErrorsPanel";
			this.ErrorsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(932, 191, true);
			this.ErrorsPanel.TabIndex = 1;
			// 
			// ErrorsGridView
			// 
			this.ErrorsGridView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ErrorsGridView.Name = "ErrorsGridView";
			this.ErrorsGridView.ReadOnly = true;
			this.ErrorsGridView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(932, 191, true);
			this.ErrorsGridView.TabIndex = 3;
			// 
			// errorLabel
			// 
			this.errorLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("RuntimeOptionsForm|036a7f95-f1ec-4f8d-b65a-cb1b4421e704", "This text will be replaced when the form is setup to show errors.");
			this.errorLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.errorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.errorLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.errorLabel.Name = "errorLabel";
			this.errorLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.errorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(932, 80, true);
			this.errorLabel.TabIndex = 0;
			// 
			// FooterPanel
			// 
			this.FooterPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FooterPanel.Controls.Add(this.TimeOutEdit);
			this.FooterPanel.Controls.Add(this.LabelReportLanguage);
			this.FooterPanel.Controls.Add(this.LanguageZDropEdit);
			this.FooterPanel.Controls.Add(this.LabelReportOrientation);
			this.FooterPanel.Controls.Add(this.DropEditReportOrientation);
			this.FooterPanel.Controls.Add(this.EdwDataSourceCheckBox);
			this.FooterPanel.Controls.Add(this.CopyButton);
			this.FooterPanel.Controls.Add(this.PreviewButton);
			this.FooterPanel.Controls.Add(this.CreateShortcutLinkLabel);
			this.FooterPanel.Controls.Add(this.ActionButton);
			this.FooterPanel.Controls.Add(this.CloseButton);
			this.FooterPanel.Controls.Add(this.MaxDopEdit);
			this.FooterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 560, true);
			this.FooterPanel.Name = "FooterPanel";
			this.FooterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 125, true);
			this.FooterPanel.TabIndex = 6;
			// 
			// TimeOutEdit
			// 
			this.TimeOutEdit.AccessibleDescription = "Timeout";
			this.TimeOutEdit.AccessibleName = "Query Timeout Override";
			this.BindingSource.SetBindingMember(this.TimeOutEdit, "TimeOut");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Report)(null)).TimeOut)));
			this.TimeOutEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.TimeOutEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("677733e6-d8b2-43d2-bb99-b27132234457", "Query Timeout Override", "The time in seconds after which a database query on a report will timeout, overriding 'Report Command Timeout' in the registry. ");
			this.TimeOutEdit.DecimalPlaces = 0;
			this.TimeOutEdit.Decimals = 0;
			this.TimeOutEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(228, 70, true);
			this.TimeOutEdit.Name = "TimeOutEdit";
			this.TimeOutEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.TimeOutEdit.TabIndex = 7;
			this.TimeOutEdit.Text = "0";
			this.TimeOutEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LabelReportLanguage
			// 
			this.LabelReportLanguage.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("RuntimeOptionsForm|06DF5255-704C-4FE8-89D0-CE33A52FC9AB", "Print Language");
			this.LabelReportLanguage.IsFontBold = true;
			this.LabelReportLanguage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 34, true);
			this.LabelReportLanguage.Name = "LabelReportLanguage";
			this.LabelReportLanguage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.LabelReportLanguage.TabIndex = 0;
			this.LabelReportLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// LanguageZDropEdit
			// 
			this.LanguageZDropEdit.AllowDrop = true;
			this.LanguageZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 37, true);
			this.LanguageZDropEdit.Name = "LanguageZDropEdit";
			this.LanguageZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 20, true);
			this.LanguageZDropEdit.TabIndex = 0;
			// 
			// LabelReportOrientation
			// 
			this.LabelReportOrientation.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("RuntimeOptionsForm|4e831715-4c60-4a06-afcf-e76c5f147cc8", "Orientation");
			this.LabelReportOrientation.IsFontBold = true;
			this.LabelReportOrientation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 34, true);
			this.LabelReportOrientation.Name = "LabelReportOrientation";
			this.LabelReportOrientation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.LabelReportOrientation.TabIndex = 1;
			this.LabelReportOrientation.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// DropEditReportOrientation
			// 
			this.DropEditReportOrientation.AllowDrop = false;
			this.BindingSource.SetBindingMember(this.DropEditReportOrientation, "Orientation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Report)(null)).Orientation)));
			this.DropEditReportOrientation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(446, 37, true);
			this.DropEditReportOrientation.Name = "DropEditReportOrientation";
			this.DropEditReportOrientation.PreBoundMaxLength = 3;
			this.DropEditReportOrientation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 20, true);
			this.DropEditReportOrientation.TabIndex = 1;
			// 
			// EdwDataSourceCheckBox
			// 
			this.BindingSource.SetBindingMember(this.EdwDataSourceCheckBox, "IsEdwDataSource");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.Report)(null)).IsEdwDataSource)));
			this.EdwDataSourceCheckBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("RuntimeOptionsForm|5dbeeddb-dd64-4d9f-aac8-86f2b3bf5b18", "Use EDW as Report Data Source");
			this.EdwDataSourceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 8, true);
			this.EdwDataSourceCheckBox.Name = "EdwDataSourceCheckBox";
			this.EdwDataSourceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 23, true);
			this.EdwDataSourceCheckBox.TabIndex = 3;
			// 
			// CopyButton
			// 
			this.CopyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CopyButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("RuntimeOptionsForm|1f955ef5-90b2-408a-b3f0-70aecca791c2", "Copy errors to clipboard");
			this.CopyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 68, true);
			this.CopyButton.Name = "CopyButton";
			this.CopyButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CopyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 23, true);
			this.CopyButton.TabIndex = 4;
			this.CopyButton.Visible = false;
			this.CopyButton.Click += new System.EventHandler(this.CopyButton_Click);
			// 
			// PreviewButton
			// 
			this.PreviewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PreviewButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("RuntimeOptionsForm|2608472a-b75e-46bf-9ec9-0f9f7c8dda1c", "Pre&view");
			this.PreviewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(695, 100, true);
			this.PreviewButton.Name = "PreviewButton";
			this.PreviewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.PreviewButton.TabIndex = 10;
			this.PreviewButton.Visible = false;
			this.PreviewButton.Click += new System.EventHandler(this.PreviewButton_Click);
			// 
			// CreateShortcutLinkLabel
			// 
			this.CreateShortcutLinkLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("D5D70A7F-C2FE-4544-890D-8D2DA6E316B3", "Create Shortcut");
			this.CreateShortcutLinkLabel.IsFontBold = false;
			this.CreateShortcutLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 70, true);
			this.CreateShortcutLinkLabel.Name = "CreateShortcutLinkLabel";
			this.CreateShortcutLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 13, true);
			this.CreateShortcutLinkLabel.TabIndex = 2;
			// 
			// ActionButton
			// 
			this.ActionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ActionButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("RuntimeOptionsForm|f9d44ce1-aa3a-4cd1-a725-2e2cdb54993d", "OK");
			this.ActionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(776, 100, true);
			this.ActionButton.Name = "ActionButton";
			this.ActionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ActionButton.TabIndex = 12;
			this.ActionButton.Click += new System.EventHandler(this.SubmitButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("RuntimeOptionsForm|ebb3a30e-0620-49e5-bc36-42e933bfa336", "&Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(858, 100, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 13;
			this.CloseButton.Click += new System.EventHandler(this.ButtonClose_Click);
			// 
			// MaxDopEdit
			//
			this.MaxDopEdit.AccessibleDescription = "MaxDop";
			this.MaxDopEdit.AccessibleName = "MaxDop";
			this.BindingSource.SetBindingMember(this.MaxDopEdit, "MaxDop");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.DocumentEngine.Report)(null)).MaxDop)));
			this.MaxDopEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("8f1f622a-f82c-40b3-a7e4-bb9a7bd6e7dc", "MAXDOP Override", "Specifies how many processors are used to execute a single SQL query.");
			this.MaxDopEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.MaxDopEdit.DecimalPlaces = 0;
			this.MaxDopEdit.Decimals = 0;
			this.MaxDopEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(228, 98, true);
			this.MaxDopEdit.Name = "MaxDopEdit";
			this.MaxDopEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.MaxDopEdit.TabIndex = 9;
			this.MaxDopEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CreateShortcutContextMenu
			// 
			this.CreateShortcutContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.CreateDesktopShortcutMenuItem,
			this.CreateHyperlinkMenuItem});
			this.CreateShortcutContextMenu.Name = "CreateShortcutContextMenu";
			this.CreateShortcutContextMenu.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 48, true);
			// 
			// CreateDesktopShortcutMenuItem
			// 
			this.CreateDesktopShortcutMenuItem.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("CCB4DD02-3432-4E3C-9E0D-6E3D2037392C", "Create Desktop Shortcut");
			this.CreateDesktopShortcutMenuItem.Name = "CreateDesktopShortcutMenuItem";
			this.CreateDesktopShortcutMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 22, true);
			// 
			// CreateHyperlinkMenuItem
			// 
			this.CreateHyperlinkMenuItem.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("079C970B-DAE3-422C-897F-042613059B95", "Copy Hyperlink to Clipboard");
			this.CreateHyperlinkMenuItem.Name = "CreateHyperlinkMenuItem";
			this.CreateHyperlinkMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 22, true);
			// 
			// FilterTabControl
			// 
			this.FilterTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FilterTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 330, true);
			this.FilterTabControl.Name = "FilterTabControl";
			this.FilterTabControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 34, true);
			this.FilterTabControl.TabIndex = 1;
			// 
			// OptionalTemplatesGroupBox
			// 
			this.OptionalTemplatesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.OptionalTemplatesGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("RuntimeOptionsForm|9353cf88-bcc0-49c3-b14e-837a34e9a315", "Templates To Include (Select at least one)");
			this.OptionalTemplatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 463, true);
			this.OptionalTemplatesGroupBox.Name = "OptionalTemplatesGroupBox";
			this.OptionalTemplatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 35, true);
			this.OptionalTemplatesGroupBox.TabIndex = 4;
			this.OptionalTemplatesGroupBox.TabStop = false;
			this.OptionalTemplatesGroupBox.Visible = false;
			// 
			// UserDefinedFieldsGroupBox
			// 
			this.UserDefinedFieldsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.UserDefinedFieldsGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("RuntimeOptionsForm|4d5e1072-ae17-4e4e-adaf-3c72e5a80612", "User defined fields");
			this.UserDefinedFieldsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 515, true);
			this.UserDefinedFieldsGroupBox.Name = "UserDefinedFieldsGroupBox";
			this.UserDefinedFieldsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 35, true);
			this.UserDefinedFieldsGroupBox.TabIndex = 5;
			this.UserDefinedFieldsGroupBox.TabStop = false;
			this.UserDefinedFieldsGroupBox.Visible = false;
			// 
			// GroupByGroupBox
			// 
			this.GroupByGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.GroupByGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("RuntimeOptionsForm|b6a5d047-d2dc-4e8a-9b29-4fbfa5913aa6", "Group Bys in the report");
			this.GroupByGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 426, true);
			this.GroupByGroupBox.Name = "GroupByGroupBox";
			this.GroupByGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 35, true);
			this.GroupByGroupBox.TabIndex = 3;
			this.GroupByGroupBox.TabStop = false;
			// 
			// SortOrderGroupBox
			// 
			this.SortOrderGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.SortOrderGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("RuntimeOptionsForm|41763151-f794-4af3-81e8-6cfbde777b65", "Sort the report in this order");
			this.SortOrderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 389, true);
			this.SortOrderGroupBox.Name = "SortOrderGroupBox";
			this.SortOrderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 34, true);
			this.SortOrderGroupBox.TabIndex = 2;
			this.SortOrderGroupBox.TabStop = false;
			// 
			// RuntimeOptionsForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 685, true);
			this.Controls.Add(this.FilterTabControl);
			this.Controls.Add(this.OptionalTemplatesGroupBox);
			this.Controls.Add(this.FooterPanel);
			this.Controls.Add(this.UserDefinedFieldsGroupBox);
			this.Controls.Add(this.GroupByGroupBox);
			this.Controls.Add(this.SortOrderGroupBox);
			this.Controls.Add(this.ErrorsGroupBox);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
			this.DataSourceType = typeof(Enterprise.DocumentEngine.Report);
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Business.Design.DataSourceTypeRequiredInstructionsType";
			this.Name = "RuntimeOptionsForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Load += new System.EventHandler(this.RuntimeOptionsForm_Load);
			this.Shown += new System.EventHandler(this.RuntimeOptionsForm_Shown);
			this.ClientSizeChanged += new System.EventHandler(this.RuntimeOptionsForm_ClientSizeChanged);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ErrorsGroupBox, 0);
			this.Controls.SetChildIndex(this.SortOrderGroupBox, 0);
			this.Controls.SetChildIndex(this.GroupByGroupBox, 0);
			this.Controls.SetChildIndex(this.UserDefinedFieldsGroupBox, 0);
			this.Controls.SetChildIndex(this.FooterPanel, 0);
			this.Controls.SetChildIndex(this.OptionalTemplatesGroupBox, 0);
			this.Controls.SetChildIndex(this.FilterTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ErrorsGroupBox.ResumeLayout(false);
			this.ErrorsGroupBox.PerformLayout();
			this.ErrorsPanel.ResumeLayout(false);
			this.ErrorsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ErrorsGridView)).EndInit();
			this.ErrorsGridView.ResumeLayout(false);
			this.ErrorsGridView.PerformLayout();
			this.FooterPanel.ResumeLayout(false);
			this.FooterPanel.PerformLayout();
			this.LanguageZDropEdit.ResumeLayout(true);
			this.LanguageZDropEdit.PerformLayout();
			this.DropEditReportOrientation.ResumeLayout(true);
			this.DropEditReportOrientation.PerformLayout();
			this.CreateShortcutContextMenu.ResumeLayout(false);
			this.CreateShortcutContextMenu.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		internal Enterprise.ZArchitecture.GUI.ZButton CopyButton;
		Enterprise.ZArchitecture.GUI.ZDropEdit DropEditReportOrientation;
		Enterprise.ZArchitecture.ZLabel LabelReportOrientation;
		internal Enterprise.ZArchitecture.ZLabel LabelReportLanguage;
		internal ZArchitecture.ZCalcEdit TimeOutEdit;
		internal ZArchitecture.GUI.ZCheckBox EdwDataSourceCheckBox;
	}
}
