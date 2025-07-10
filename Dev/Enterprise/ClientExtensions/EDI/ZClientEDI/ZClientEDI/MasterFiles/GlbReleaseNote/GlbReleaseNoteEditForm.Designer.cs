#if DEBUG
namespace Enterprise.Client.EDI.Gui
{
	partial class GlbReleaseNoteEditForm
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.OptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.toDateFilter = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.fromDateFilter = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CheckInButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CheckoutButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UndoCheckoutButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SummaryTextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			((System.ComponentModel.ISupportInitialize)(this.ReleaseNotesGrid)).BeginInit();
			this.ReleaseNotesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OptionsGroupBox.SuspendLayout();
			this.toDateFilter.SuspendLayout();
			this.fromDateFilter.SuspendLayout();
			this.SuspendLayout();
			//
			// ReleaseNotesGrid
			//
			zDropEditColumnStyleInfo1.BindToList = "Lookups.SectionList";
			zDropEditColumnStyleInfo1.ColumnName = "GF_Section";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.ColumnName = "GF_ReleaseNoteDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "GF_RN_NKCountryForReleaseNote";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo3.ColumnName = "GF_Category";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("GlbReleaseNoteEditForm|1939c91e-98df-4db2-a010-26f8e406f112", "Title");
			zTextBoxColumnStyleInfo1.ColumnName = "Title";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.ColumnName = "GF_URL";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(380);
			this.ReleaseNotesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ReleaseNotesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ReleaseNotesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ReleaseNotesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ReleaseNotesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReleaseNotesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ReleaseNotesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 60, true);
			this.ReleaseNotesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 198, true);
			//
			// CloseButton
			//
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(730, 429, true);
			this.CloseButton.TabIndex = 7;
			//
			// ViewButton
			//
			this.ViewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(650, 429, true);
			this.ViewButton.TabIndex = 6;
			//
			// SummaryTextBox
			//
			this.SummaryTextBox.AcceptsReturn = true;
			this.SummaryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SummaryTextBox, "ReleaseNotes.GF_Summary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbReleaseNote)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbReleaseNoteManager)(null)).ReleaseNotes)).SyncRoot)).GF_Summary)));
			this.SummaryTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SummaryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 277, true);
			this.SummaryTextBox.IsMultiLine = true;
			this.SummaryTextBox.Name = "SummaryTextBox";
			//this.SummaryTextBox. = System.Windows.Forms.ScrollBars.Vertical;
			this.SummaryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 144, true);
			this.SummaryTextBox.TabIndex = 2;
			//
			// SummaryLabel
			//
			this.SummaryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 260, true);
			this.SummaryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 457, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 26, true);
			this.MainStatusBar.TabIndex = 8;
			//
			// OptionsGroupBox
			//
			this.OptionsGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("GlbReleaseNoteEditForm|7FB579D2-E413-4C8F-9990-C711CC0B9ECA", "Search Option");
			this.OptionsGroupBox.Controls.Add(this.toDateFilter);
			this.OptionsGroupBox.Controls.Add(this.fromDateFilter);
			this.OptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 6, true);
			this.OptionsGroupBox.Name = "OptionsGroupBox";
			this.OptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 43, true);
			this.OptionsGroupBox.TabIndex = 4;
			this.OptionsGroupBox.TabStop = false;
			this.OptionsGroupBox.Text = "Search Option";
			//
			// toDateFilter
			//
			this.toDateFilter.AllowDrop = true;
			this.toDateFilter.AutoCompleteMonthThreshold = 1;
			this.toDateFilter.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.toDateFilter, "DateToFilterBefore");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbReleaseNoteManager)(null)).DateToFilterBefore)));
			this.toDateFilter.CaptionResourceString = ZClientEDI.Res.GetData("9cb6c744-889a-4a15-8399-2c524cb23d72", "To");
			this.toDateFilter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 21, true);
			this.toDateFilter.Name = "toDateFilter";
			this.toDateFilter.TabIndex = 17;
			//
			// fromDateFilter
			//
			this.fromDateFilter.AllowDrop = true;
			this.fromDateFilter.AutoCompleteMonthThreshold = 1;
			this.fromDateFilter.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.fromDateFilter, "DateToFilterAfter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbReleaseNoteManager)(null)).DateToFilterAfter)));
			this.fromDateFilter.CaptionResourceString = ZClientEDI.Res.GetData("866ac989-cfcc-4892-bb94-7bf08f5b46ad", "Show Notes From");
			this.fromDateFilter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 21, true);
			this.fromDateFilter.Name = "fromDateFilter";
			this.fromDateFilter.TabIndex = 16;
			//
			// CheckInButton
			//
			this.CheckInButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CheckInButton.CaptionResourceString = ZClientEDI.Res.GetData("GlbReleaseNoteEditForm|24f7d6b6-63d8-494e-9eb3-4e1e8974c298", "Check In");
			this.CheckInButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 427, true);
			this.CheckInButton.Name = "CheckInButton";
			this.CheckInButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CheckInButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.CheckInButton.TabIndex = 5;
			this.CheckInButton.ToolTipCaption = null;
			this.CheckInButton.Click += new System.EventHandler(this.CheckInButton_Click);
			//
			// CheckoutButton
			//
			this.CheckoutButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CheckoutButton.CaptionResourceString = ZClientEDI.Res.GetData("GlbReleaseNoteEditForm|bcb3ab13-de5c-43d5-840e-d9a0ae6595e9", "Checkout");
			this.CheckoutButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 427, true);
			this.CheckoutButton.Name = "CheckoutButton";
			this.CheckoutButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CheckoutButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.CheckoutButton.TabIndex = 4;
			this.CheckoutButton.ToolTipCaption = null;
			this.CheckoutButton.Click += new System.EventHandler(this.CheckoutButton_Click);
			//
			// UndoCheckoutButton
			//
			this.UndoCheckoutButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.UndoCheckoutButton.CaptionResourceString = ZClientEDI.Res.GetData("GlbReleaseNoteEditForm|e5d2b159-17f1-4810-809a-481b2b1a4874", "Undo Checkout");
			this.UndoCheckoutButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 429, true);
			this.UndoCheckoutButton.Name = "UndoCheckoutButton";
			this.UndoCheckoutButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UndoCheckoutButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 24, true);
			this.UndoCheckoutButton.TabIndex = 3;
			this.UndoCheckoutButton.ToolTipCaption = null;
			this.UndoCheckoutButton.Click += new System.EventHandler(this.UndoCheckoutButton_Click);
			//
			// GlbReleaseNoteEditForm
			//
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 483, true);
			this.Controls.Add(this.CheckInButton);
			this.Controls.Add(this.CheckoutButton);
			this.Controls.Add(this.UndoCheckoutButton);
			this.Controls.Add(this.OptionsGroupBox);
			this.Controls.Add(this.SummaryTextBox);

			this.Name = "GlbReleaseNoteEditForm";
			this.Controls.SetChildIndex(this.OptionsGroupBox, 0);
			this.Controls.SetChildIndex(this.ViewButton, 0);
			this.Controls.SetChildIndex(this.SummaryLabel, 0);
			this.Controls.SetChildIndex(this.UndoCheckoutButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ReleaseNotesGrid, 0);
			this.Controls.SetChildIndex(this.SummaryTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CheckoutButton, 0);
			this.Controls.SetChildIndex(this.CheckInButton, 0);
			this.Controls.SetChildIndex(this.SummaryTextBox, 0);

			((System.ComponentModel.ISupportInitialize)(this.ReleaseNotesGrid)).EndInit();
			this.ReleaseNotesGrid.ResumeLayout(false);
			this.ReleaseNotesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OptionsGroupBox.ResumeLayout(false);
			this.OptionsGroupBox.PerformLayout();
			this.toDateFilter.ResumeLayout(true);
			this.toDateFilter.PerformLayout();
			this.fromDateFilter.ResumeLayout(true);
			this.fromDateFilter.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZButton CheckInButton;
		internal Enterprise.ZArchitecture.GUI.ZButton UndoCheckoutButton;
		internal Enterprise.ZArchitecture.GUI.ZButton CheckoutButton;
		Enterprise.ZArchitecture.GUI.ZGroupBox OptionsGroupBox;
		protected Enterprise.ZArchitecture.ZTranslatableTextControl SummaryTextBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit fromDateFilter;
		Enterprise.ZArchitecture.GUI.ZDateEdit toDateFilter;
	}
}
#endif
