
namespace Enterprise.Customs.CA.GUI
{
	partial class MessageSendingActionForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EntriesToSendMessagesForGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntriesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageContentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageContentsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_SendMessageCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.EntriesToSendMessagesForGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntriesGrid)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.MessageContentsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 417, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(491, 381, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 3;
			this.CancelButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("50eb23ae-3787-4b91-ab95-e34025e34e04", "&Cancel");
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 381, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d6d3df14-4b59-4327-9123-0e3035005caa", "&OK");
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// EntriesToSendMessagesForGroupBox
			// 
			this.EntriesToSendMessagesForGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.EntriesToSendMessagesForGroupBox.Controls.Add(this.EntriesGrid);
			this.EntriesToSendMessagesForGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.EntriesToSendMessagesForGroupBox.Name = "EntriesToSendMessagesForGroupBox";
			this.EntriesToSendMessagesForGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 119, true);
			this.EntriesToSendMessagesForGroupBox.TabIndex = 0;
			this.EntriesToSendMessagesForGroupBox.TabStop = false;
			this.EntriesToSendMessagesForGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("73226bb1-2beb-46cf-9f72-c6db763f3289", "Entries");
			// 
			// EntriesGrid
			// 
			this.EntriesGrid.AllowNavigation = false;
			this.EntriesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.EntriesGrid.BindTo = ".";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CAMessageSendingActionCollection)(null)))));
			this.EntriesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("165de073-11c1-4447-a638-049f1e931ddc", "Message Description");
			zTextBoxColumnStyleInfo1.ColumnName = "CA_MessageDescription";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("56009581-1c03-44f6-a36d-6cadd1c038df", "Send?");
			zCheckBoxColumnStyleInfo1.ColumnName = "CA_SendMessage";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;

			#region Column Initialisation

			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("53fac848-2892-4161-8b8a-1d1a601e28f7", "Message Contents");
			zMultiLineTextBoxColumnInfo1.ColumnName = "CA_MessageContents";
			zMultiLineTextBoxColumnInfo1.IsReadOnly = true;
			zMultiLineTextBoxColumnInfo1.IsVisible = false;
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("df2f53be-252a-40c2-a09f-d0490adc5637", "Save Without Sending");
			zCheckBoxColumnStyleInfo2.ColumnName = "CA_SaveWithoutSending";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5576c33d-3a53-49ca-b078-a023062b2cbb", "Save Without Sending Reason Text");
			zTextBoxColumnStyleInfo2.ColumnName = "CA_SaveWithoutSendingReasonText";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntriesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.EntriesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.EntriesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);

			#endregion

			this.EntriesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntriesGrid.LayoutKey = "EntriesGrid";
			this.EntriesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EntriesGrid.Name = "EntriesGrid";
			this.EntriesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.EntriesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 100, true);
			this.EntriesGrid.TabIndex = 0;
			this.EntriesGrid.AfterBind += new System.EventHandler(this.EntriesGrid_AfterBind);
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.CA.Business.CAMessageSendingAction)(((object)(((Enterprise.Customs.CA.Business.CAMessageSendingActionCollection)(null)))))).CA_MessageDescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAMessageSendingAction)(((object)(((Enterprise.Customs.CA.Business.CAMessageSendingActionCollection)(null)))))).CA_MessageDescription)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.CAMessageSendingAction)(((object)(((Enterprise.Customs.CA.Business.CAMessageSendingActionCollection)(null)))))).CA_SendMessage)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.CA.Business.CAMessageSendingAction)(((object)(((Enterprise.Customs.CA.Business.CAMessageSendingActionCollection)(null)))))).CA_SendMessageInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.CA.Business.CAMessageSendingAction)(((object)(((Enterprise.Customs.CA.Business.CAMessageSendingActionCollection)(null)))))).CA_MessageContentsInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAMessageSendingAction)(((object)(((Enterprise.Customs.CA.Business.CAMessageSendingActionCollection)(null)))))).CA_MessageContents)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.CAMessageSendingAction)(((object)(((Enterprise.Customs.CA.Business.CAMessageSendingActionCollection)(null)))))).CA_SaveWithoutSending)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.CA.Business.CAMessageSendingAction)(((object)(((Enterprise.Customs.CA.Business.CAMessageSendingActionCollection)(null)))))).CA_SaveWithoutSendingInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.CA.Business.CAMessageSendingAction)(((object)(((Enterprise.Customs.CA.Business.CAMessageSendingActionCollection)(null)))))).CA_SaveWithoutSendingReasonTextInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAMessageSendingAction)(((object)(((Enterprise.Customs.CA.Business.CAMessageSendingActionCollection)(null)))))).CA_SaveWithoutSendingReasonText)));
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.DetailsGroupBox.Controls.Add(this.MessageContentsGroupBox);
			this.DetailsGroupBox.Controls.Add(this.CA_SendMessageCheckBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 134, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(551, 241, true);
			this.DetailsGroupBox.TabIndex = 1;
			this.DetailsGroupBox.TabStop = false;
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1aab76c7-2a76-409a-b9d0-bdf03f1aa9bb", "Details");
			// 
			// MessageContentsGroupBox
			// 
			this.MessageContentsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.MessageContentsGroupBox.Controls.Add(this.MessageContentsTextBox);
			this.MessageContentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 42, true);
			this.MessageContentsGroupBox.Name = "MessageContentsGroupBox";
			this.MessageContentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 193, true);
			this.MessageContentsGroupBox.TabIndex = 1;
			this.MessageContentsGroupBox.TabStop = false;
			this.MessageContentsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1e6bcdf3-91d6-4de0-ab31-97afab81b617", "Message Contents");
			// 
			// MessageContentsTextBox
			// 
			this.MessageContentsTextBox.BackColor = System.Drawing.SystemColors.Info;
			this.MessageContentsTextBox.BindTo = "CA_MessageContents";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.CA.Business.CAMessageSendingAction)(((object)(((Enterprise.Customs.CA.Business.CAMessageSendingActionCollection)(null)))))).CA_MessageContentsInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CAMessageSendingAction)(((object)(((Enterprise.Customs.CA.Business.CAMessageSendingActionCollection)(null)))))).CA_MessageContents)));
			this.MessageContentsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageContentsTextBox.Font = new System.Drawing.Font("Courier New", 8F);
			this.MessageContentsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageContentsTextBox.Multiline = true;
			this.MessageContentsTextBox.Name = "MessageContentsTextBox";
			this.MessageContentsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageContentsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 174, true);
			this.MessageContentsTextBox.TabIndex = 0;
			// 
			// CA_SendMessageCheckBox
			// 
			this.CA_SendMessageCheckBox.BackColor = System.Drawing.SystemColors.Info;
			this.CA_SendMessageCheckBox.BindTo = "CA_SendMessage";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.CAMessageSendingAction)(((object)(((Enterprise.Customs.CA.Business.CAMessageSendingActionCollection)(null)))))).CA_SendMessage)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.CA.Business.CAMessageSendingAction)(((object)(((Enterprise.Customs.CA.Business.CAMessageSendingActionCollection)(null)))))).CA_SendMessageInfo)));
			this.CA_SendMessageCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.CA_SendMessageCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CA_SendMessageCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.CA_SendMessageCheckBox.Name = "CA_SendMessageCheckBox";
			this.CA_SendMessageCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 17, true);
			this.CA_SendMessageCheckBox.TabIndex = 0;
			this.CA_SendMessageCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c1d04c62-a78d-478c-b462-2cf1312e2b4b", "Send?");
			this.CA_SendMessageCheckBox.UseVisualStyleBackColor = false;
			// 
			// MessageSendingActionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 441, true);
			this.Controls.Add(this.DetailsGroupBox);
			this.Controls.Add(this.EntriesToSendMessagesForGroupBox);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.OKButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.CA.Business";
			this.DataSourceType = typeof(Enterprise.Customs.CA.Business.CAMessageSendingActionCollection);
			this.DataSourceTypeName = "Enterprise.Customs.CA.Business.CAMessageSendingActionCollection";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 468, true);
			this.Name = "MessageSendingActionForm";
			this.ShouldSerializeTabPageMethods = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("53fd766f-b140-4195-a723-a18b0dd4a1c6", "CAMessageSendingActionForm");
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.EntriesToSendMessagesForGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.DetailsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.EntriesToSendMessagesForGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.EntriesGrid)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.MessageContentsGroupBox.ResumeLayout(false);
			this.MessageContentsGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		internal new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		internal Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox EntriesToSendMessagesForGroupBox;
		internal Enterprise.ZArchitecture.ZGrid EntriesGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox CA_SendMessageCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox MessageContentsGroupBox;
		private Enterprise.ZArchitecture.ZTextBox MessageContentsTextBox;
	}
}
