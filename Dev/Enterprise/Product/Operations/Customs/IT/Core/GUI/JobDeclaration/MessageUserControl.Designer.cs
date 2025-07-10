namespace Enterprise.Customs.IT.GUI
{
	partial class MessageUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;


		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.GroupedPreviousDocumentsUserControl = new Enterprise.Customs.IT.GUI.GroupedPreviousDocumentsUserControl();
			this.AllM2LinesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntriesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntriesBoundGrid)).BeginInit();
			this.EntriesBoundGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainHorizontalSplitContainer)).BeginInit();
			this.MainHorizontalSplitContainer.Panel1.SuspendLayout();
			this.MainHorizontalSplitContainer.Panel2.SuspendLayout();
			this.MainHorizontalSplitContainer.SuspendLayout();
			this.EntryLinesMessagesTabControl.SuspendLayout();
			this.EntryLinesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).BeginInit();
			this.EntryLineGrid.SuspendLayout();
			this.ExtendedInfoGroupBox.SuspendLayout();
			this.MessageTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).BeginInit();
			this.TopVerticalSplitContainer.Panel1.SuspendLayout();
			this.TopVerticalSplitContainer.Panel2.SuspendLayout();
			this.TopVerticalSplitContainer.SuspendLayout();
			this.BaseMessageUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupedPreviousDocumentsUserControl.SuspendLayout();
			this.AllM2LinesTabPage.SuspendLayout();
			this.EntryDetailsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// AmendmentReasonTextBox
			// 
			this.AmendmentReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 60, true);
			// 
			// EntriesGroupBox
			// 
			this.EntriesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1386, 85, true);
			// 
			// EntriesBoundGrid
			// 
			this.EntriesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1380, 66, true);
			this.EntriesBoundGrid.AfterBind += new System.EventHandler(this.EntriesBoundGridOnAfterBind);
			// 
			// MainHorizontalSplitContainer
			// 
			this.MainHorizontalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1438, 728, true);
			this.MainHorizontalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(85);
			// 
			// EntryLinesMessagesTabControl
			// 
			this.EntryLinesMessagesTabControl.Controls.Add(this.AllM2LinesTabPage);
			this.EntryLinesMessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1438, 640, true);
			this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.AllM2LinesTabPage, 0);
			this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.EntryLinesTabPage, 0);
			this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.MessageTabPage, 0);
			// 
			// EntryLinesTabPage
			// 
			this.EntryLinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1430, 613, true);
			// 
			// EntryLineGrid
			// 
			this.EntryLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1424, 490, true);
			// 
			// ExtendedInfoGroupBox
			// 
			this.ExtendedInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 493, true);
			this.ExtendedInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1424, 117, true);
			// 
			// MessageTabPage
			// 
			this.MessageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 457, true);
			// 
			// TopVerticalSplitContainer
			// 
			this.TopVerticalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1438, 85, true);
			this.TopVerticalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1051);
			// 
			// BaseMessageUserControl
			// 
			this.BaseMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 451, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobDeclaration);
			// 
			// GroupedPreviousDocumentsUserControl
			// 
			this.GroupedPreviousDocumentsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GroupedPreviousDocumentsUserControl, "CustomsEntryHeaders.AllGroupedPreviousDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.IT.Business.Declaration.AllGroupedPreviousDocumentCollection)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllGroupedPreviousDocuments)));
			this.GroupedPreviousDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupedPreviousDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.GroupedPreviousDocumentsUserControl.Name = "GroupedPreviousDocumentsUserControl";
			this.GroupedPreviousDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 451, true);
			this.GroupedPreviousDocumentsUserControl.TabIndex = 0;
			// 
			// AllM2LinesTabPage
			// 
			this.AllM2LinesTabPage.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("085747C5-2FC3-4378-BC5B-F80B394D0B37", "M2 Lines");
			this.AllM2LinesTabPage.Controls.Add(this.GroupedPreviousDocumentsUserControl);
			this.AllM2LinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AllM2LinesTabPage.Name = "AllM2LinesTabPage";
			this.AllM2LinesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AllM2LinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 457, true);
			this.AllM2LinesTabPage.TabIndex = 2;
			// 
			// MessageUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "MessageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1438, 728, true);
			this.CaptionRenderingEnabled = true;
			this.EntriesGroupBox.ResumeLayout(false);
			this.EntriesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntriesBoundGrid)).EndInit();
			this.EntriesBoundGrid.ResumeLayout(false);
			this.EntriesBoundGrid.PerformLayout();
			this.MainHorizontalSplitContainer.Panel1.ResumeLayout(false);
			this.MainHorizontalSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainHorizontalSplitContainer)).EndInit();
			this.MainHorizontalSplitContainer.ResumeLayout(false);
			this.MainHorizontalSplitContainer.PerformLayout();
			this.EntryLinesMessagesTabControl.ResumeLayout(false);
			this.EntryLinesMessagesTabControl.PerformLayout();
			this.EntryLinesTabPage.ResumeLayout(false);
			this.EntryLinesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineGrid)).EndInit();
			this.EntryLineGrid.ResumeLayout(false);
			this.EntryLineGrid.PerformLayout();
			this.ExtendedInfoGroupBox.ResumeLayout(false);
			this.ExtendedInfoGroupBox.PerformLayout();
			this.MessageTabPage.ResumeLayout(false);
			this.MessageTabPage.PerformLayout();
			this.TopVerticalSplitContainer.Panel1.ResumeLayout(false);
			this.TopVerticalSplitContainer.Panel2.ResumeLayout(false);
			this.TopVerticalSplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopVerticalSplitContainer)).EndInit();
			this.TopVerticalSplitContainer.ResumeLayout(false);
			this.TopVerticalSplitContainer.PerformLayout();
			this.BaseMessageUserControl.ResumeLayout(true);
			this.BaseMessageUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupedPreviousDocumentsUserControl.ResumeLayout(true);
			this.GroupedPreviousDocumentsUserControl.PerformLayout();
			this.AllM2LinesTabPage.ResumeLayout(false);
			this.AllM2LinesTabPage.PerformLayout();
			this.EntryDetailsUserControl.ResumeLayout(true);
			this.EntryDetailsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		protected Enterprise.ZArchitecture.GUI.ZTabPage AllM2LinesTabPage;
		private GroupedPreviousDocumentsUserControl GroupedPreviousDocumentsUserControl;

		#endregion
	}
}
