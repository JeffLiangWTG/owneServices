using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class MessagesTabUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.MessagesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MessageGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MessagesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.InterpretationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageInterpretationWebBrowser = new ZWebBrowser();
			this.MessageInterpretationRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.MessageInterpretationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageEdifactTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessagesSplitContainer)).BeginInit();
			this.MessagesSplitContainer.Panel1.SuspendLayout();
			this.MessagesSplitContainer.Panel2.SuspendLayout();
			this.MessagesSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageGrid)).BeginInit();
			this.MessageGrid.SuspendLayout();
			this.MessagesTabControl.SuspendLayout();
			this.InterpretationTabPage.SuspendLayout();
			this.MessageInterpretationRichTextBox.SuspendLayout();
			this.TextTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Messaging.Business.EDIMessageCollection);
			// 
			// splitter1
			// 
			this.splitter1.DoNotSaveSplitterLayout = false;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 575, true);
			this.splitter1.TabIndex = 32;
			this.splitter1.TabStop = false;
			// 
			// MessagesSplitContainer
			// 
			this.MessagesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.MessagesSplitContainer.Name = "MessagesSplitContainer";
			this.MessagesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MessagesSplitContainer.Panel1
			// 
			this.MessagesSplitContainer.Panel1.Controls.Add(this.MessageGrid);
			// 
			// MessagesSplitContainer.Panel2
			// 
			this.MessagesSplitContainer.Panel2.Controls.Add(this.MessagesTabControl);
			this.MessagesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 575, true);
			this.MessagesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(337);
			this.MessagesSplitContainer.TabIndex = 33;
			// 
			// MessageGrid
			// 
			this.MessageGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessageGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Messaging.Business.EDIMessage)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_ReceiveTransmit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_InterchangeStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_ApplicationReference)));
			this.MessageGrid.CaptionText = "Offices";
			this.MessageGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo10.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo11.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo12.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo12.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(52);
			zTextBoxColumnStyleInfo13.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo13.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo14.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo14.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo15.ColumnName = "EM_SystemCreateUser";
			zTextBoxColumnStyleInfo15.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo4.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo16.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo16.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo17.ColumnName = "EM_InterchangeStatus";
			zTextBoxColumnStyleInfo17.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo18.ColumnName = "EM_ApplicationReference";
			zTextBoxColumnStyleInfo18.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.MessageGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.MessageGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.MessageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.MessageGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageGrid.GridId = "582a4d3d-a4fa-4344-8383-f88a8645b21e";
			this.MessageGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessageGrid.LayoutKey = "CustomsOfficesGrid";
			this.MessageGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageGrid.Name = "MessageGrid";
			this.MessageGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 337, true);
			this.MessageGrid.TabIndex = 32;
			// 
			// MessagesTabControl
			// 
			this.MessagesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MessagesTabControl.Controls.Add(this.InterpretationTabPage);
			this.MessagesTabControl.Controls.Add(this.TextTabPage);
			this.MessagesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesTabControl.Name = "MessagesTabControl";
			this.MessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 234, true);
			this.MessagesTabControl.TabIndex = 0;
			// 
			// InterpretationTabPage
			// 
			this.InterpretationTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("8a64e5bb-d32f-40d5-823a-751db63f438f", "Interpretation");
			this.InterpretationTabPage.Controls.Add(this.MessageInterpretationWebBrowser);
			this.InterpretationTabPage.Controls.Add(this.MessageInterpretationRichTextBox);
			this.InterpretationTabPage.Controls.Add(this.MessageInterpretationTextBox);
			this.InterpretationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InterpretationTabPage.Name = "InterpretationTabPage";
			this.InterpretationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.InterpretationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 207, true);
			this.InterpretationTabPage.TabIndex = 0;
			this.InterpretationTabPage.UseVisualStyleBackColor = true;
			// 
			// MessageInterpretationWebBrowser
			// 
			this.MessageInterpretationWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageInterpretationWebBrowser.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageInterpretationWebBrowser.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.MessageInterpretationWebBrowser.Name = "MessageInterpretationWebBrowser";
			this.MessageInterpretationWebBrowser.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 201, true);
			this.MessageInterpretationWebBrowser.TabIndex = 0;
			this.MessageInterpretationWebBrowser.Url = new System.Uri("about:blank", System.UriKind.Absolute);
			// 
			// MessageInterpretationRichTextBox
			//
			this.MessageInterpretationRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageInterpretationRichTextBox, false);
			this.MessageInterpretationRichTextBox.IsAttachButtonVisible = false;
			this.MessageInterpretationRichTextBox.IsInsertImageButtonVisible = false;
			this.MessageInterpretationRichTextBox.IsPopupButtonVisible = false;
			this.MessageInterpretationRichTextBox.IsToolBarVisible = false;
			this.MessageInterpretationRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageInterpretationRichTextBox.Name = "MessageInterpretationRichTextBox";
			this.MessageInterpretationRichTextBox.ParentZForm = null;
			this.MessageInterpretationRichTextBox.PopupFormCaption = null;
			this.MessageInterpretationRichTextBox.ReadOnly = true;
			this.MessageInterpretationRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
			this.MessageInterpretationRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 201, true);
			this.MessageInterpretationRichTextBox.TabIndex = 0;
			// 
			// MessageInterpretationTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageInterpretationTextBox, "EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageInterpretation)));
			this.MessageInterpretationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageInterpretationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 93, true);
			this.MessageInterpretationTextBox.Name = "MessageInterpretationTextBox";
			this.MessageInterpretationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 20, true);
			this.MessageInterpretationTextBox.TabIndex = 2;
			this.MessageInterpretationTextBox.TabStop = false;
			this.MessageInterpretationTextBox.TextChanged += new System.EventHandler(this.MessageInterpretationTextBox_TextChanged);
			// 
			// TextTabPage
			// 
			this.TextTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("7f8bce19-9faa-4926-9141-069e382591e8", "Text");
			this.TextTabPage.Controls.Add(this.MessageEdifactTextBox);
			this.TextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TextTabPage.Name = "TextTabPage";
			this.TextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 207, true);
			this.TextTabPage.TabIndex = 1;
			this.TextTabPage.UseVisualStyleBackColor = true;
			// 
			// MessageEdifactTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageEdifactTextBox, "EM_MessageTextIndentedXml");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageTextIndentedXml)));
			this.MessageEdifactTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageEdifactTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageEdifactTextBox.EnableFindDialog = true;
			this.MessageEdifactTextBox.HideSelection = false;
			this.MessageEdifactTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageEdifactTextBox.Multiline = true;
			this.MessageEdifactTextBox.Name = "MessageEdifactTextBox";
			this.MessageEdifactTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MessageEdifactTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 201, true);
			this.MessageEdifactTextBox.TabIndex = 0;
			// 
			// MessagesTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MessagesSplitContainer);
			this.Controls.Add(this.splitter1);
			this.Name = "MessagesTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 575, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessagesSplitContainer.Panel1.ResumeLayout(false);
			this.MessagesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesSplitContainer)).EndInit();
			this.MessagesSplitContainer.ResumeLayout(false);
			this.MessagesSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageGrid)).EndInit();
			this.MessageGrid.ResumeLayout(false);
			this.MessageGrid.PerformLayout();
			this.MessagesTabControl.ResumeLayout(false);
			this.MessagesTabControl.PerformLayout();
			this.InterpretationTabPage.ResumeLayout(false);
			this.InterpretationTabPage.PerformLayout();
			this.MessageInterpretationRichTextBox.ResumeLayout(true);
			this.MessageInterpretationRichTextBox.PerformLayout();
			this.TextTabPage.ResumeLayout(false);
			this.TextTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitter splitter1;
		private CargoWise.Windows.UI.KSplitContainer MessagesSplitContainer;
		protected ZArchitecture.ZGrid MessageGrid;
		private ZTabControl MessagesTabControl;
		protected ZTabPage InterpretationTabPage;
		protected ZWebBrowser MessageInterpretationWebBrowser;
		protected ZRichTextBox MessageInterpretationRichTextBox;
		private ZTabPage TextTabPage;
		protected ZArchitecture.ZTextBox MessageEdifactTextBox;
		protected ZArchitecture.ZTextBox MessageInterpretationTextBox;

	}
}
