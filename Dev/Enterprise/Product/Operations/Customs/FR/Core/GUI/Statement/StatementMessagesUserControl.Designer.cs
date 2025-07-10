namespace Enterprise.Customs.FR.GUI
{
	partial class StatementMessagesUserControl
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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.ChargesGrid = new Enterprise.ZArchitecture.ZGrid();
            this.TabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
            this.InterpretationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.MessageInterpretationWebBrowser = new Enterprise.ZArchitecture.GUI.ZWebBrowser();
            this.MessageInterpretationTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.MessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).BeginInit();
            this.ChargesGrid.SuspendLayout();
            this.TabControl.SuspendLayout();
            this.InterpretationTabPage.SuspendLayout();
            this.TextTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader);
            // 
            // ChargesGrid
            // 
            this.ChargesGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.ChargesGrid, "Messages");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).Messages)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).Messages)).SyncRoot)).EM_MessageNum)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).Messages)).SyncRoot)).EM_MessageType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).Messages)).SyncRoot)).EM_MessageSubType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).Messages)).SyncRoot)).EM_MessageDateTime)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).Messages)).SyncRoot)).EM_SystemCreateTimeUtc)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).Messages)).SyncRoot)).EM_InterchangeNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).Messages)).SyncRoot)).EM_DateTimeInterchangeSent)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).Messages)).SyncRoot)).EM_User)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).Messages)).SyncRoot)).EM_Status)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).Messages)).SyncRoot)).EM_ReceiveTransmit)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).Messages)).SyncRoot)).EM_InterchangeStatus)));
            this.ChargesGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
            zTextBoxColumnStyleInfo1.IsCustomColumn = false;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageType";
            zTextBoxColumnStyleInfo2.IsCustomColumn = false;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo3.ColumnName = "EM_MessageSubType";
            zTextBoxColumnStyleInfo3.IsCustomColumn = false;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
            zDateEditColumnStyleInfo1.IsCustomColumn = false;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
            zDateEditColumnStyleInfo2.IsCustomColumn = false;
            zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo4.ColumnName = "EM_InterchangeNumber";
            zTextBoxColumnStyleInfo4.IsCustomColumn = false;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo3.ColumnName = "EM_DateTimeInterchangeSent";
            zDateEditColumnStyleInfo3.IsCustomColumn = false;
            zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo5.ColumnName = "EM_User";
            zTextBoxColumnStyleInfo5.IsCustomColumn = false;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo6.ColumnName = "EM_Status";
            zTextBoxColumnStyleInfo6.IsCustomColumn = false;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo7.ColumnName = "EM_ReceiveTransmit";
            zTextBoxColumnStyleInfo7.IsCustomColumn = false;
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo8.ColumnName = "EM_InterchangeStatus";
            zTextBoxColumnStyleInfo8.IsCustomColumn = false;
            zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.ChargesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.ChargesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
            this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.ChargesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
            this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.ChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.ChargesGrid.Dock = System.Windows.Forms.DockStyle.Top;
            this.ChargesGrid.GridId = "a63c454d-c4c3-4158-8f9d-85bc18bf0ce9";
            this.ChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ChargesGrid.LayoutKey = "ChargesGrid";
            this.ChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ChargesGrid.Name = "ChargesGrid";
            this.ChargesGrid.ReadOnly = true;
            this.ChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 257, true);
            this.ChargesGrid.TabIndex = 3;
            // 
            // TabControl
            // 
            this.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.TabControl.Controls.Add(this.InterpretationTabPage);
            this.TabControl.Controls.Add(this.TextTabPage);
            this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 257, true);
            this.TabControl.Name = "TabControl";
            this.TabControl.SelectedIndex = 0;
            this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 267, true);
            this.TabControl.TabIndex = 13;
            // 
            // InterpretationTabPage
            // 
            this.InterpretationTabPage.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("154d13eb-cce3-4e88-9227-e059cf443a38", "Interpretation");
            this.InterpretationTabPage.Controls.Add(this.MessageInterpretationWebBrowser);
            this.InterpretationTabPage.Controls.Add(this.MessageInterpretationTextBox);
            this.InterpretationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
            this.InterpretationTabPage.Name = "InterpretationTabPage";
            this.InterpretationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.InterpretationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 243, true);
            this.InterpretationTabPage.TabIndex = 0;
            this.InterpretationTabPage.UseVisualStyleBackColor = true;
            // 
            // MessageInterpretationWebBrowser
            // 
            this.MessageInterpretationWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageInterpretationWebBrowser.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.MessageInterpretationWebBrowser.Name = "MessageInterpretationWebBrowser";
            this.MessageInterpretationWebBrowser.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 237, true);
            this.MessageInterpretationWebBrowser.TabIndex = 0;
            this.MessageInterpretationWebBrowser.Url = new System.Uri("about:blank", System.UriKind.Absolute);
            // 
            // MessageInterpretationTextBox
            // 
            this.BindingSource.SetBindingMember(this.MessageInterpretationTextBox, "Messages.EM_MessageInterpretation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).Messages)).SyncRoot)).EM_MessageInterpretation)));
            this.MessageInterpretationTextBox.CaptionResourceString = null;
            this.MessageInterpretationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.MessageInterpretationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(267, 114, true);
            this.MessageInterpretationTextBox.Name = "MessageInterpretationTextBox";
            this.MessageInterpretationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 15, true);
            this.MessageInterpretationTextBox.TabIndex = 3;
            this.MessageInterpretationTextBox.TabStop = false;
            this.MessageInterpretationTextBox.TextChanged += MessageInterpretationTextBox_TextChanged;
            // 
            // TextTabPage
            // 
            this.TextTabPage.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("88a51fcb-c318-4100-bf03-6d541b6dc950", "Text");
            this.TextTabPage.Controls.Add(this.MessageTextBox);
            this.TextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
            this.TextTabPage.Name = "TextTabPage";
            this.TextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.TextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 243, true);
            this.TextTabPage.TabIndex = 1;
            this.TextTabPage.UseVisualStyleBackColor = true;
            // 
            // MessageTextBox
            // 
            this.BindingSource.SetBindingMember(this.MessageTextBox, "Messages.EM_FormattedMessageText");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusStatement.CusStatementHeader)(null)).Messages)).SyncRoot)).EM_FormattedMessageText)));
            this.MessageTextBox.CaptionResourceString = null;
            this.MessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.MessageTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.MessageTextBox.Multiline = true;
            this.MessageTextBox.Name = "MessageTextBox";
            this.MessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.MessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 237, true);
            this.MessageTextBox.TabIndex = 0;
            // 
            // StatementMessagesUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.TabControl);
            this.Controls.Add(this.ChargesGrid);
            this.Name = "StatementMessagesUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 524, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ChargesGrid)).EndInit();
            this.ChargesGrid.ResumeLayout(false);
            this.ChargesGrid.PerformLayout();
            this.TabControl.ResumeLayout(false);
            this.TabControl.PerformLayout();
            this.InterpretationTabPage.ResumeLayout(false);
            this.InterpretationTabPage.PerformLayout();
            this.TextTabPage.ResumeLayout(false);
            this.TextTabPage.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid ChargesGrid;
		private ZArchitecture.GUI.ZTemplateTabControl TabControl;
		private ZArchitecture.GUI.ZTabPage InterpretationTabPage;
		private ZArchitecture.GUI.ZWebBrowser MessageInterpretationWebBrowser;
		private ZArchitecture.GUI.ZTabPage TextTabPage;
		private ZArchitecture.ZTextBox MessageTextBox;
		private ZArchitecture.ZTextBox MessageInterpretationTextBox;
	}
}
