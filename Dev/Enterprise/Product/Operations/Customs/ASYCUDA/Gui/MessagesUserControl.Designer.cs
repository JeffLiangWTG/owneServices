using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class MessagesUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo38 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.splitContainerGridVersusMessageDetails = new CargoWise.Windows.UI.KSplitContainer();
			this.messagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MessageDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MessageInterpretationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageInterpretationWebBrowser = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
			this.MessageTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerGridVersusMessageDetails)).BeginInit();
			this.splitContainerGridVersusMessageDetails.Panel1.SuspendLayout();
			this.splitContainerGridVersusMessageDetails.Panel2.SuspendLayout();
			this.splitContainerGridVersusMessageDetails.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.messagesGrid)).BeginInit();
			this.messagesGrid.SuspendLayout();
			this.MessageDetailsTabControl.SuspendLayout();
			this.MessageInterpretationTabPage.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Messaging.Business.IEDIMessageCollectionProvider);
			// 
			// splitContainerGridVersusMessageDetails
			// 
			this.splitContainerGridVersusMessageDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerGridVersusMessageDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.splitContainerGridVersusMessageDetails.Name = "splitContainerGridVersusMessageDetails";
			this.splitContainerGridVersusMessageDetails.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainerGridVersusMessageDetails.Panel1
			// 
			this.splitContainerGridVersusMessageDetails.Panel1.Controls.Add(this.messagesGrid);
			// 
			// splitContainerGridVersusMessageDetails.Panel2
			// 
			this.splitContainerGridVersusMessageDetails.Panel2.Controls.Add(this.MessageDetailsTabControl);
			this.splitContainerGridVersusMessageDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(999, 617, true);
			this.splitContainerGridVersusMessageDetails.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(169);
			this.splitContainerGridVersusMessageDetails.TabIndex = 1;
			// 
			// messagesGrid
			// 
			this.messagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.messagesGrid, "Messages");
			this.messagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo8.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo4.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo10.ColumnName = "EM_SystemCreateUser";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo38.ColumnName = "EM_CreateUserFullName";
			zTextBoxColumnStyleInfo38.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo38.IsVisible = false;
			zTextBoxColumnStyleInfo11.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.ColumnName = "EM_ApplicationReference";
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo14.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.messagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.messagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo38);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.messagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesGrid.GridId = "96ca5084-a46f-46ad-9713-fb2f83b49ae2";
			this.messagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.messagesGrid.LayoutKey = "messagesGrid";
			this.messagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messagesGrid.Name = "messagesGrid";
			this.messagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(999, 169, true);
			this.messagesGrid.TabIndex = 3;
			// 
			// MessageDetailsTabControl
			// 
			this.MessageDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MessageDetailsTabControl.Controls.Add(this.MessageInterpretationTabPage);
			this.MessageDetailsTabControl.Controls.Add(this.MessageTextTabPage);
			this.MessageDetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageDetailsTabControl.Name = "MessageDetailsTabControl";
			this.MessageDetailsTabControl.SelectedIndex = 0;
			this.MessageDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(999, 444, true);
			this.MessageDetailsTabControl.TabIndex = 2;
			// 
			// MessageInterpretationTabPage
			// 
			this.MessageInterpretationTabPage.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("06c1f305-a060-48b8-ab79-f24676c74c59", "Interpretation");
			this.MessageInterpretationTabPage.Controls.Add(this.MessageInterpretationWebBrowser);
			this.MessageInterpretationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageInterpretationTabPage.Name = "MessageInterpretationTabPage";
			this.MessageInterpretationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageInterpretationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(991, 417, true);
			this.MessageInterpretationTabPage.TabIndex = 0;
			this.MessageInterpretationTabPage.UseVisualStyleBackColor = true;
			// 
			// MessageInterpretationWebBrowser
			// 
			this.BindingSource.SetBindingMember(this.MessageInterpretationWebBrowser, "Messages.EM_MessageInterpretation");
			this.MessageInterpretationWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageInterpretationWebBrowser.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageInterpretationWebBrowser.Name = "MessageInterpretationWebBrowser";
			this.MessageInterpretationWebBrowser.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(985, 411, true);
			this.MessageInterpretationWebBrowser.TabIndex = 0;
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("cb3ff236-98b2-4246-8b14-e29db5b93d52", "Text");
			this.MessageTextTabPage.Controls.Add(this.MessageTextTextBox);
			this.MessageTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageTextTabPage.Name = "MessageTextTabPage";
			this.MessageTextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(991, 417, true);
			this.MessageTextTabPage.TabIndex = 1;
			this.MessageTextTabPage.UseVisualStyleBackColor = true;
			// 
			// MessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "Messages.EM_FormattedMessageText");
			this.MessageTextTextBox.CaptionResourceString = null;
			this.MessageTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageTextTextBox.Multiline = true;
			this.MessageTextTextBox.Name = "MessageTextTextBox";
			this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(985, 411, true);
			this.MessageTextTextBox.TabIndex = 0;
			this.MessageTextTextBox.HideSelection = false;
			this.MessageTextTextBox.EnableFindDialog = true;
			// 
			// MessagesUserControl
			// 
			this.Controls.Add(this.splitContainerGridVersusMessageDetails);
			this.splitContainerGridVersusMessageDetails.Panel1.ResumeLayout(false);
			this.splitContainerGridVersusMessageDetails.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainerGridVersusMessageDetails)).EndInit();
			this.splitContainerGridVersusMessageDetails.ResumeLayout(false);
			this.splitContainerGridVersusMessageDetails.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.messagesGrid)).EndInit();
			this.messagesGrid.ResumeLayout(false);
			this.messagesGrid.PerformLayout();
			this.MessageDetailsTabControl.ResumeLayout(false);
			this.MessageDetailsTabControl.PerformLayout();
			this.MessageInterpretationTabPage.ResumeLayout(false);
			this.MessageInterpretationTabPage.PerformLayout();
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
		}

		#endregion

		CargoWise.Windows.UI.KSplitContainer splitContainerGridVersusMessageDetails;
		ZTabControl MessageDetailsTabControl;
		ZTabPage MessageInterpretationTabPage;
		HtmlInterpretationBox MessageInterpretationWebBrowser;
		protected ZTabPage MessageTextTabPage;
		protected ZArchitecture.ZTextBox MessageTextTextBox;
		ZGrid messagesGrid;
	}
}
