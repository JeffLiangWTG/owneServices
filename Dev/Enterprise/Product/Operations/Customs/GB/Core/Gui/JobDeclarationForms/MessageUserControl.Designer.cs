using Enterprise.Customs.GB.GUI.Plugin;

namespace Enterprise.Customs.GB.GUI
{
	partial class MessageUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.FECChallengesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.gbfecChallengeUserControl1 = new Enterprise.Customs.GB.GUI.Plugin.GBFECChallengeUserControl();
			this.AmendmentReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
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
			this.FECChallengesTabPage.SuspendLayout();
			this.gbfecChallengeUserControl1.SuspendLayout();
			this.AmendmentReasonDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// AmendmentReasonTextBox
			// 
			this.AmendmentReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 37, true);
			this.AmendmentReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 28, true);
			this.AmendmentReasonTextBox.TabIndex = 2;
			// 
			// AmendmentReasonLabel
			// 
			this.AmendmentReasonLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 0, true);
			this.AmendmentReasonLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 13, true);
			// 
			// EntriesGroupBox
			// 
			this.EntriesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 67, true);
			// 
			// EntriesBoundGrid
			// 
			this.EntriesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(567, 48, true);
			// 
			// MainHorizontalSplitContainer
			// 
			this.MainHorizontalSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(67);
			this.MainHorizontalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(67);
			// 
			// EntryLinesMessagesTabControl
			// 
			this.EntryLinesMessagesTabControl.Controls.Add(this.FECChallengesTabPage);
			this.EntryLinesMessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 481, true);
			this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.FECChallengesTabPage, 0);
			this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.EntryLinesTabPage, 0);
			this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.MessageTabPage, 0);
			// 
			// EntryLinesTabPage
			// 
			this.EntryLinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 454, true);
			// 
			// EntryLineGrid
			// 
			this.EntryLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 331, true);
			// 
			// ExtendedInfoGroupBox
			// 
			this.ExtendedInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 334, true);
			// 
			// MessageTabPage
			// 
			this.MessageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 454, true);
			// 
			// TopVerticalSplitContainer
			// 
			this.TopVerticalSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.TopVerticalSplitContainer.IsSplitterFixed = true;
			// 
			// TopVerticalSplitContainer.Panel2
			// 
			this.TopVerticalSplitContainer.Panel2.Controls.Add(this.AmendmentReasonDropEdit);
			this.TopVerticalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 67, true);
			this.TopVerticalSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.TopVerticalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(573);
			// 
			// BaseMessageUserControl
			// 
			this.BaseMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 448, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Business.Declaration.JobDeclaration);
			// 
			// FECChallengesTabPage
			// 
			this.FECChallengesTabPage.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("0f8f5705-55c6-4cae-892d-cf298e69d73a", "FEC Challenges");
			this.FECChallengesTabPage.Controls.Add(this.gbfecChallengeUserControl1);
			this.FECChallengesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FECChallengesTabPage.Name = "FECChallengesTabPage";
			this.FECChallengesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 454, true);
			this.FECChallengesTabPage.TabIndex = 2;
			// 
			// gbfecChallengeUserControl1
			// 
			this.gbfecChallengeUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.gbfecChallengeUserControl1, "CustomsEntryHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)))));
			this.gbfecChallengeUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gbfecChallengeUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.gbfecChallengeUserControl1.Name = "gbfecChallengeUserControl1";
			this.gbfecChallengeUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 454, true);
			this.gbfecChallengeUserControl1.TabIndex = 0;
			// 
			// AmendmentReasonDropEdit
			// 
			this.AmendmentReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AmendmentReasonDropEdit, "CustomsEntryHeaders.ZG_AmendmentReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).ZG_AmendmentReasonCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AddInfoLookups.AmendmentReasonCodeList)));
			this.AmendmentReasonDropEdit.BindToList = "CustomsEntryHeaders.AddInfoLookups.AmendmentReasonCodeList";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AmendmentReasonDropEdit, false);
			this.AmendmentReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 17, true);
			this.AmendmentReasonDropEdit.Name = "AmendmentReasonDropEdit";
			this.AmendmentReasonDropEdit.ShouldResizeByMaxLength = true;
			this.AmendmentReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 20, true);
			this.AmendmentReasonDropEdit.TabIndex = 1;
			// 
			// MessageUserControl
			// 
			this.Name = "MessageUserControl";
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
			this.FECChallengesTabPage.ResumeLayout(false);
			this.FECChallengesTabPage.PerformLayout();
			this.gbfecChallengeUserControl1.ResumeLayout(true);
			this.gbfecChallengeUserControl1.PerformLayout();
			this.AmendmentReasonDropEdit.ResumeLayout(true);
			this.AmendmentReasonDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private ZArchitecture.GUI.ZTabPage FECChallengesTabPage;
		private GBFECChallengeUserControl gbfecChallengeUserControl1;
		private ZArchitecture.GUI.ZDropEdit AmendmentReasonDropEdit;
	}
}
