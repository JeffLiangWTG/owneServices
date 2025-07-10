using Enterprise.ZArchitecture.GUI.Grid;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public partial class GridRowFinderForm
	{
		ZButton CancelButtonX;
		ZLabel zLabel1;
		ZTextBox zTextBox1;
		ZCheckedListBox zCheckedListBox1;
		private ZButton toggleColumnsButton;
		ZButton PreviousButton;
		ZButton NextButton;
		ZCheckBox MatchCaseCheckBox;
		ZCheckBox SelectedRowCheckBox;
		ZCheckBox SelectAllRowsCheckBox;
		ZLabel XOfYLabel;
		internal GridRowFinderBusinessObject RowFinder;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		new void InitializeComponent()
		{
			this.CancelButtonX = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zCheckedListBox1 = new Enterprise.ZArchitecture.GUI.ZCheckedListBox();
			this.toggleColumnsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PreviousButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NextButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MatchCaseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SelectedRowCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SelectAllRowsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.XOfYLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 230, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.GUI.Grid.GridRowFinderBusinessObject);
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.CancelButtonX.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("GridRowFinderForm|12a19abc-dd4c-40e1-ac99-210b9a171809", "Cancel");
			this.CancelButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButtonX.IsCaptionOverridden = false;
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 194, true);
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButtonX.TabIndex = 99;
			this.CancelButtonX.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelButtonX.ToolTipCaption = null;
			this.CancelButtonX.UseVisualStyleBackColor = true;
			this.CancelButtonX.Click += new System.EventHandler(this.CancelButtonX_Click);
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("GridRowFinderForm|efa9556f-6889-48c8-81e3-8dd51acbd2e7", "Columns to Search");
			this.zLabel1.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold);
			this.zLabel1.IsFontBold = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 13, true);
			this.zLabel1.TabIndex = 0;
			// 
			// zTextBox1
			// 
			this.zTextBox1.AcceptsReturn = true;
			this.zTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.zTextBox1, "TextToSearchFor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.GUI.Grid.GridRowFinderBusinessObject)(null)).TextToSearchFor);
			this.zTextBox1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("GridRowFinderForm|d478ac44-80b0-469d-9099-e07741a91194", "Text to Search for");
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.zTextBox1, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 29, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 20, true);
			this.zTextBox1.TabIndex = 3;
			// 
			// zCheckedListBox1
			// 
			this.zCheckedListBox1.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left);
			this.zCheckedListBox1.BindingItems = null;
			this.BindingSource.SetBindingMember(this.zCheckedListBox1, "ColumnsToSearch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.GUI.Grid.GridRowFinderBusinessObject)(null)).ColumnsToSearch);
			this.zCheckedListBox1.FormattingEnabled = true;
			this.zCheckedListBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 29, true);
			this.zCheckedListBox1.Name = "zCheckedListBox1";
			this.zCheckedListBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 154, true);
			this.zCheckedListBox1.TabIndex = 2;
			// 
			// toggleColumnsButton
			// 
			this.toggleColumnsButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.toggleColumnsButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("990fa98b-c0c8-43a0-8f8a-8d9539ebc330", "Toggle All");
			this.toggleColumnsButton.IsCaptionOverridden = false;
			this.toggleColumnsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 194, true);
			this.toggleColumnsButton.Name = "toggleColumnsButton";
			this.toggleColumnsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.toggleColumnsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.toggleColumnsButton.TabIndex = 98;
			this.toggleColumnsButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.toggleColumnsButton.ToolTipCaption = null;
			this.toggleColumnsButton.UseVisualStyleBackColor = true;
			this.toggleColumnsButton.Click += new System.EventHandler(this.toggleColumnsButton_Click);
			// 
			// PreviousButton
			// 
			this.PreviousButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("0e92f178-3599-40a2-8af6-10d437a06381", "Previous");
			this.PreviousButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.PreviousButton.IsCaptionOverridden = false;
			this.PreviousButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 55, true);
			this.PreviousButton.Name = "PreviousButton";
			this.PreviousButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.PreviousButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.PreviousButton.TabIndex = 8;
			this.PreviousButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.PreviousButton.ToolTipCaption = null;
			this.PreviousButton.UseVisualStyleBackColor = true;
			this.PreviousButton.Click += new System.EventHandler(this.previousButton_Click);
			// 
			// NextButton
			// 
			this.NextButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.NextButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("23dad5a8-5b49-46a1-9d88-966ae7f8d0c8", "Next");
			this.NextButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.NextButton.IsCaptionOverridden = false;
			this.NextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 55, true);
			this.NextButton.Name = "NextButton";
			this.NextButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.NextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.NextButton.TabIndex = 9;
			this.NextButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.NextButton.ToolTipCaption = null;
			this.NextButton.UseVisualStyleBackColor = true;
			this.NextButton.Click += new System.EventHandler(this.nextButton_Click);
			// 
			// MatchCaseCheckBox
			// 
			this.MatchCaseCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.MatchCaseCheckBox, "MatchCase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.GUI.Grid.GridRowFinderBusinessObject)(null)).MatchCase);
			this.MatchCaseCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("39f72f02-4de6-421c-8b75-fc5acb1d7bb7", "Match Case");
			this.MatchCaseCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MatchCaseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 85, true);
			this.MatchCaseCheckBox.Name = "MatchCaseCheckBox";
			this.MatchCaseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 17, true);
			this.MatchCaseCheckBox.TabIndex = 10;
			this.MatchCaseCheckBox.UseVisualStyleBackColor = true;
			// 
			// SelectedRowCheckBox
			// 
			this.SelectedRowCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SelectedRowCheckBox, "SearchFromSelectedRow");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.GUI.Grid.GridRowFinderBusinessObject)(null)).SearchFromSelectedRow);
			this.SelectedRowCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("2b547f6e-c40c-4b62-a484-4c10631c54ca", "Search From Selected Row");
			this.SelectedRowCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SelectedRowCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 108, true);
			this.SelectedRowCheckBox.Name = "SelectedRowCheckBox";
			this.SelectedRowCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 17, true);
			this.SelectedRowCheckBox.TabIndex = 11;
			this.SelectedRowCheckBox.UseVisualStyleBackColor = true;
			// 
			// SelectAllRowsCheckBox
			// 
			this.SelectAllRowsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SelectAllRowsCheckBox, "SelectAllRows");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.GUI.Grid.GridRowFinderBusinessObject)(null)).SelectAllRows);
			this.SelectAllRowsCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("133c9b37-417b-46dd-894d-00c25e3ca88c", "Select All Rows");
			this.SelectAllRowsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SelectAllRowsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 131, true);
			this.SelectAllRowsCheckBox.Name = "SelectAllRowsCheckBox";
			this.SelectAllRowsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.SelectAllRowsCheckBox.TabIndex = 12;
			this.SelectAllRowsCheckBox.UseVisualStyleBackColor = true;
			// 
			// XOfYLabel
			// 
			this.XOfYLabel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.XOfYLabel, "SearchLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.GUI.Grid.GridRowFinderBusinessObject)(null)).SearchLabel);
			this.XOfYLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ed1c50cd-ae1a-4806-ad7a-7ed2afd0c5ea", "{0} out of {1}");
			this.XOfYLabel.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif);
			this.XOfYLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 155, true);
			this.XOfYLabel.Name = "XOfYLabel";
			this.XOfYLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 23, true);
			this.XOfYLabel.TabIndex = 13;
			this.XOfYLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// GridRowFinderForm
			// 
			this.CancelButton = this.CancelButtonX;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("GridRowFinderForm|e9efd78d-5f7f-48a1-8571-0604c7991a8f", "Find (Ctrl-F)");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 254, true);
			this.Controls.Add(this.XOfYLabel);
			this.Controls.Add(this.SelectAllRowsCheckBox);
			this.Controls.Add(this.SelectedRowCheckBox);
			this.Controls.Add(this.MatchCaseCheckBox);
			this.Controls.Add(this.NextButton);
			this.Controls.Add(this.PreviousButton);
			this.Controls.Add(this.toggleColumnsButton);
			this.Controls.Add(this.zTextBox1);
			this.Controls.Add(this.CancelButtonX);
			this.Controls.Add(this.zCheckedListBox1);
			this.Controls.Add(this.zLabel1);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.GUI.Grid.GridRowFinderBusinessObject);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 282, true);
			this.Name = "GridRowFinderForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.zCheckedListBox1, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(this.zTextBox1, 0);
			this.Controls.SetChildIndex(this.toggleColumnsButton, 0);
			this.Controls.SetChildIndex(this.PreviousButton, 0);
			this.Controls.SetChildIndex(this.NextButton, 0);
			this.Controls.SetChildIndex(this.MatchCaseCheckBox, 0);
			this.Controls.SetChildIndex(this.SelectedRowCheckBox, 0);
			this.Controls.SetChildIndex(this.SelectAllRowsCheckBox, 0);
			this.Controls.SetChildIndex(this.XOfYLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
