namespace Enterprise.Customs.JP.AFR.GUI
{
	partial class MessagesUserControl
	{
		#region Component Designer generated code
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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MessagesBottomSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MessagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.JPAFRHeaderMessageTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.JPAFRHeaderMessageDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageDetailHTMLInterpretationBox = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
			this.JPAFRHeaderMessageTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.FormattedMessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessagesBottomSplitContainer)).BeginInit();
			this.MessagesBottomSplitContainer.Panel1.SuspendLayout();
			this.MessagesBottomSplitContainer.Panel2.SuspendLayout();
			this.MessagesBottomSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.JPAFRHeaderMessageTabControl.SuspendLayout();
			this.JPAFRHeaderMessageDetailsTabPage.SuspendLayout();
			this.JPAFRHeaderMessageTextTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.AFR.Business.JPAFRHeader);
			// 
			// MessagesBottomSplitContainer
			// 
			this.MessagesBottomSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesBottomSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesBottomSplitContainer.Name = "MessagesBottomSplitContainer";
			// 
			// MessagesBottomSplitContainer.Panel1
			// 
			this.MessagesBottomSplitContainer.Panel1.Controls.Add(this.MessagesGrid);
			this.MessagesBottomSplitContainer.Panel1MinSize = 200;
			// 
			// MessagesBottomSplitContainer.Panel2
			// 
			this.MessagesBottomSplitContainer.Panel2.Controls.Add(this.JPAFRHeaderMessageTabControl);
			this.MessagesBottomSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 741, true);
			this.MessagesBottomSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(438);
			this.MessagesBottomSplitContainer.TabIndex = 4;
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessagesGrid, "AFRMessages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).AFRMessages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRMessage)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).AFRMessages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRMessage)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).AFRMessages)).SyncRoot)).EM_MessageOwner)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.JP.AFR.Business.JPAFRMessage)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).AFRMessages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.JP.AFR.Business.JPAFRMessage)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).AFRMessages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRMessage)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).AFRMessages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.JP.AFR.Business.JPAFRMessage)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).AFRMessages)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRMessage)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).AFRMessages)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRMessage)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).AFRMessages)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRMessage)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).AFRMessages)).SyncRoot)).EM_InterchangeStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRMessage)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).AFRMessages)).SyncRoot)).EM_ReceiveTransmit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRMessage)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).AFRMessages)).SyncRoot)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRMessage)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).AFRMessages)).SyncRoot)).EM_MessageSubType)));
			this.MessagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(129);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("ab1b3be7-9218-43e3-beb7-75946ba90394", "Action");
			zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageOwner";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114);
			zTextBoxColumnStyleInfo3.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(129);
			zDateEditColumnStyleInfo3.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			zTextBoxColumnStyleInfo4.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(104);
			zTextBoxColumnStyleInfo5.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo6.ColumnName = "EM_InterchangeStatus";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(84);
			zTextBoxColumnStyleInfo7.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			zTextBoxColumnStyleInfo8.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(69);
			zTextBoxColumnStyleInfo9.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(69);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.MessagesGrid.CopySelectedRowsAllowed = true;
			this.MessagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesGrid.GridId = "d98549e5-32bf-4074-a0a9-406f6c54dd53";
			this.MessagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessagesGrid.LayoutKey = "JPAFRHeaderMessagesGrid";
			this.MessagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesGrid.Name = "MessagesGrid";
			this.MessagesGrid.ReadOnly = true;
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 741, true);
			this.MessagesGrid.TabIndex = 1;
			// 
			// JPAFRHeaderMessageTabControl
			// 
			this.JPAFRHeaderMessageTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.JPAFRHeaderMessageTabControl.Controls.Add(this.JPAFRHeaderMessageDetailsTabPage);
			this.JPAFRHeaderMessageTabControl.Controls.Add(this.JPAFRHeaderMessageTextTabPage);
			this.JPAFRHeaderMessageTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JPAFRHeaderMessageTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JPAFRHeaderMessageTabControl.Name = "JPAFRHeaderMessageTabControl";
			this.JPAFRHeaderMessageTabControl.SelectedIndex = 0;
			this.JPAFRHeaderMessageTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(529, 741, true);
			this.JPAFRHeaderMessageTabControl.TabIndex = 1;
			// 
			// JPAFRHeaderMessageDetailsTabPage
			// 
			this.JPAFRHeaderMessageDetailsTabPage.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("b3031a89-bf6b-4898-b899-ce2ab5bff10f", "Message Details");
			this.JPAFRHeaderMessageDetailsTabPage.Controls.Add(this.MessageDetailHTMLInterpretationBox);
			this.JPAFRHeaderMessageDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.JPAFRHeaderMessageDetailsTabPage.Name = "JPAFRHeaderMessageDetailsTabPage";
			this.JPAFRHeaderMessageDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.JPAFRHeaderMessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(521, 714, true);
			this.JPAFRHeaderMessageDetailsTabPage.TabIndex = 0;
			// 
			// MessageDetailHTMLInterpretationBox
			// 
			this.BindingSource.SetBindingMember(this.MessageDetailHTMLInterpretationBox, "AFRMessages.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRMessage)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).AFRMessages)).SyncRoot)).EM_MessageInterpretation)));
			this.MessageDetailHTMLInterpretationBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageDetailHTMLInterpretationBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageDetailHTMLInterpretationBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.MessageDetailHTMLInterpretationBox.Name = "MessageDetailHTMLInterpretationBox";
			this.MessageDetailHTMLInterpretationBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 708, true);
			this.MessageDetailHTMLInterpretationBox.TabIndex = 1;
			// 
			// JPAFRHeaderMessageTextTabPage
			// 
			this.JPAFRHeaderMessageTextTabPage.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("0e47a367-860b-42e7-8416-79b5c73e8bba", "Message Text");
			this.JPAFRHeaderMessageTextTabPage.Controls.Add(this.FormattedMessageTextTextBox);
			this.JPAFRHeaderMessageTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.JPAFRHeaderMessageTextTabPage.Name = "JPAFRHeaderMessageTextTabPage";
			this.JPAFRHeaderMessageTextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.JPAFRHeaderMessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(521, 714, true);
			this.JPAFRHeaderMessageTextTabPage.TabIndex = 1;
			// 
			// FormattedMessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.FormattedMessageTextTextBox, "AFRMessages.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.JPAFRMessage)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.JPAFRHeader)(null)).AFRMessages)).SyncRoot)).EM_FormattedMessageText)));
			this.FormattedMessageTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FormattedMessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FormattedMessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.FormattedMessageTextTextBox.Multiline = true;
			this.FormattedMessageTextTextBox.Name = "FormattedMessageTextTextBox";
			this.FormattedMessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.FormattedMessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 708, true);
			this.FormattedMessageTextTextBox.TabIndex = 0;
			// 
			// MessagesUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MessagesBottomSplitContainer);
			this.Name = "MessagesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 741, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessagesBottomSplitContainer.Panel1.ResumeLayout(false);
			this.MessagesBottomSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesBottomSplitContainer)).EndInit();
			this.MessagesBottomSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.JPAFRHeaderMessageTabControl.ResumeLayout(false);
			this.JPAFRHeaderMessageDetailsTabPage.ResumeLayout(false);
			this.JPAFRHeaderMessageTextTabPage.ResumeLayout(false);
			this.JPAFRHeaderMessageTextTabPage.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer MessagesBottomSplitContainer;


		#endregion
		private ZArchitecture.ZGrid MessagesGrid;
		private ZArchitecture.GUI.ZTabControl JPAFRHeaderMessageTabControl;
		private ZArchitecture.GUI.ZTabPage JPAFRHeaderMessageDetailsTabPage;
		private ZArchitecture.GUI.ZTabPage JPAFRHeaderMessageTextTabPage;
		private ZArchitecture.ZTextBox FormattedMessageTextTextBox;
		private Messaging.GUI.HtmlInterpretationBox MessageDetailHTMLInterpretationBox;
	}
}
