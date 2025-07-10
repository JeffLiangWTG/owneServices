using Enterprise.Customs.IE.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.GUI
{
	partial class QueryOnGuaranteeForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.ClearAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.SelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.QueryIdentifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.PeriodFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.PeriodToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.GuaranteesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.GuaranteesGrid = new Enterprise.ZArchitecture.ZGrid();
            this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.messageSendingObjectsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
            this.MessageSendingObjectsGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ButtonsPanel.SuspendLayout();
            this.QueryIdentifierDropEdit.SuspendLayout();
            this.PeriodFromDateEdit.SuspendLayout();
            this.PeriodToDateEdit.SuspendLayout();
            this.GuaranteesGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GuaranteesGrid)).BeginInit();
            this.GuaranteesGrid.SuspendLayout();
            this.MainGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // SendButton
            // 
            this.SendButton.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("661D1AA9-0651-4D46-91C6-143B24246B3E", "OK");
            this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 2, true);
            this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 21, true);
            this.SendButton.TabIndex = 9;
            // 
            // CancelButton2
            // 
            this.CancelButton2.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("0033E17A-347B-4CE8-85CF-2F9C476669E0", "Cancel");
            this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 2, true);
            this.CancelButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 21, true);
            this.CancelButton2.TabIndex = 10;
            // 
            // messageSendingObjectsGroupBox
            // 
            this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 224, true);
            this.messageSendingObjectsGroupBox.Visible = false;
            // 
            // MessageSendingObjectsGrid
            // 
            this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(697, 207, true);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 233, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 23, true);
            this.MainStatusBar.TabIndex = 1;
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingActionParent);
            // 
            // ButtonsPanel
            // 
            this.ButtonsPanel.Controls.Add(this.ClearAllButton);
            this.ButtonsPanel.Controls.Add(this.CancelButton2);
            this.ButtonsPanel.Controls.Add(this.SelectAllButton);
            this.ButtonsPanel.Controls.Add(this.SendButton);
            this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 202, true);
            this.ButtonsPanel.Name = "ButtonsPanel";
            this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 29, true);
            this.ButtonsPanel.TabIndex = 3;
            this.ButtonsPanel.Controls.SetChildIndex(this.SendButton, 0);
            this.ButtonsPanel.Controls.SetChildIndex(this.SelectAllButton, 0);
            this.ButtonsPanel.Controls.SetChildIndex(this.CancelButton2, 0);
            this.ButtonsPanel.Controls.SetChildIndex(this.ClearAllButton, 0);
            // 
            // ClearAllButton
            // 
            this.ClearAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ClearAllButton.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("B6000E11-22AA-44DC-B8B1-8D1C60F90529", "Clear Selection");
            this.ClearAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 2, true);
            this.ClearAllButton.Name = "ClearAllButton";
            this.ClearAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 21, true);
            this.ClearAllButton.TabIndex = 7;
            this.ClearAllButton.ToolTipCaption = null;
            this.ClearAllButton.Click += new System.EventHandler(this.ClearAllButton_Click);
            // 
            // SelectAllButton
            // 
            this.SelectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectAllButton.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("42B4D759-5F32-4273-A173-D20840EDF526", "Select All");
            this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 2, true);
            this.SelectAllButton.Name = "SelectAllButton";
            this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 21, true);
            this.SelectAllButton.TabIndex = 8;
            this.SelectAllButton.ToolTipCaption = null;
            this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
            // 
            // QueryIdentifierDropEdit
            // 
            this.QueryIdentifierDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.QueryIdentifierDropEdit, "SendingObjectsCollection.QueryIdentifier");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).QueryIdentifier)));
            this.QueryIdentifierDropEdit.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("66d118d5-a996-4e9a-b1a0-57737c3a7593", "Query Identifier");
            this.QueryIdentifierDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.QueryIdentifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 17, true);
            this.QueryIdentifierDropEdit.Name = "QueryIdentifierDropEdit";
            this.QueryIdentifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.QueryIdentifierDropEdit.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);

			this.QueryIdentifierDropEdit.TabIndex = 2;
            // 
            // PeriodFromDateEdit
            // 
            this.PeriodFromDateEdit.AllowDrop = true;
            this.PeriodFromDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PeriodFromDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.PeriodFromDateEdit, "SendingObjectsCollection.PeriodFrom");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).PeriodFrom)));
            this.PeriodFromDateEdit.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("192c7929-a803-4508-8761-8fee18164899", "Period From");
			this.PeriodFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(343, 17, true);
            this.PeriodFromDateEdit.Name = "PeriodFromDateEdit";
            this.PeriodFromDateEdit.TabIndex = 3;
            // 
            // PeriodToDateEdit
            // 
            this.PeriodToDateEdit.AllowDrop = true;
            this.PeriodToDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PeriodToDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.PeriodToDateEdit, "SendingObjectsCollection.PeriodTo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).PeriodTo)));
            this.PeriodToDateEdit.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("72b8d5c1-5c89-4886-b6ed-f68080f632b5", "Period To");
			this.PeriodToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(485, 17, true);
			this.PeriodToDateEdit.Name = "PeriodToDateEdit";
            this.PeriodToDateEdit.TabIndex = 4;
            // 
            // GuaranteesGroupBox
            // 
            this.GuaranteesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GuaranteesGroupBox.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("7A4AFDE7-631C-4F4C-9C0B-6CE181C09308", "Guarantees");
            this.GuaranteesGroupBox.Controls.Add(this.GuaranteesGrid);
            this.GuaranteesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 47, true);
            this.GuaranteesGroupBox.Name = "GuaranteesGroupBox";
            this.GuaranteesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 151, true);
            this.GuaranteesGroupBox.TabIndex = 5;
            this.GuaranteesGroupBox.TabStop = false;
            // 
            // GuaranteesGrid
            // 
            this.GuaranteesGrid.AccessibleName = "";
            this.GuaranteesGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.GuaranteesGrid, "SendingObjectsCollection.AllGuarantees");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AllGuarantees)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((QueryOnGuaranteeSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AllGuarantees)).SyncRoot)).GuaranteeType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((QueryOnGuaranteeSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AllGuarantees)).SyncRoot)).GuaranteeReferenceNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((QueryOnGuaranteeSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AllGuarantees)).SyncRoot)).AccessCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((QueryOnGuaranteeSendingObject)(((System.Collections.IList)(((Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingAction)(((System.Collections.IList)(((Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).AllGuarantees)).SyncRoot)).ShouldSend)));
            this.GuaranteesGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("e1b5a3f1-fa03-4d64-a10c-1f985d3f321f", "Type");
            zTextBoxColumnStyleInfo1.ColumnName = "GuaranteeType";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(67);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("1b7a85ef-a83a-47aa-8f2f-0984347cf339", "GRN");
            zTextBoxColumnStyleInfo2.ColumnName = "GuaranteeReferenceNumber";
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(147);
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("cc99a75d-df6e-4dd9-b628-7b8cf97cf04c", "Access Code");
            zTextBoxColumnStyleInfo3.ColumnName = "AccessCode";
			zTextBoxColumnStyleInfo3.PasswordChar = '*';
			zTextBoxColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
            zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.IE.NCTS.GUI.Res.GetData("6b7d959f-32d7-47a5-8a5d-89ab5a04e1dd", "Select");
            zCheckBoxColumnStyleInfo1.ColumnName = "ShouldSend";
            zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(67);
            this.GuaranteesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.GuaranteesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.GuaranteesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.GuaranteesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
            this.GuaranteesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GuaranteesGrid.GridId = "533984d3-13c8-4990-bd20-1bef1f932482";
            this.GuaranteesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.GuaranteesGrid.LayoutKey = "GuaranteesGrid";
            this.GuaranteesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.GuaranteesGrid.Name = "GuaranteesGrid";
            this.GuaranteesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 134, true);
            this.GuaranteesGrid.TabIndex = 6;
            // 
            // MainGroupBox
            // 
            this.MainGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainGroupBox.Controls.Add(this.QueryIdentifierDropEdit);
            this.MainGroupBox.Controls.Add(this.PeriodFromDateEdit);
            this.MainGroupBox.Controls.Add(this.PeriodToDateEdit);
            this.MainGroupBox.Controls.Add(this.GuaranteesGroupBox);
            this.MainGroupBox.Controls.Add(this.ButtonsPanel);
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MainGroupBox, false);
            this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.MainGroupBox.Name = "MainGroupBox";
            this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 233, true);
            this.MainGroupBox.TabIndex = 11;
            this.MainGroupBox.TabStop = false;
            // 
            // QueryOnGuaranteeForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 256, true);
            this.Controls.Add(this.MainGroupBox);
            this.DataSourceType = typeof(Enterprise.Customs.IE.NCTS.Business.QueryOnGuaranteeSendingActionParent);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 293, true);
            this.Name = "QueryOnGuaranteeForm";
            this.Text = "Query on Guarantee";
            this.Controls.SetChildIndex(this.messageSendingObjectsGroupBox, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.MainGroupBox, 0);
            this.messageSendingObjectsGroupBox.ResumeLayout(false);
            this.messageSendingObjectsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).EndInit();
            this.MessageSendingObjectsGrid.ResumeLayout(false);
            this.MessageSendingObjectsGrid.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ButtonsPanel.ResumeLayout(false);
            this.ButtonsPanel.PerformLayout();
            this.QueryIdentifierDropEdit.ResumeLayout(true);
            this.QueryIdentifierDropEdit.PerformLayout();
            this.PeriodFromDateEdit.ResumeLayout(true);
            this.PeriodFromDateEdit.PerformLayout();
            this.PeriodToDateEdit.ResumeLayout(true);
            this.PeriodToDateEdit.PerformLayout();
            this.GuaranteesGroupBox.ResumeLayout(false);
            this.GuaranteesGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GuaranteesGrid)).EndInit();
            this.GuaranteesGrid.ResumeLayout(false);
            this.GuaranteesGrid.PerformLayout();
            this.MainGroupBox.ResumeLayout(false);
            this.MainGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MainGroupBox;
		internal ZArchitecture.GUI.ZDropEditWithFixedWidth QueryIdentifierDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit PeriodFromDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit PeriodToDateEdit;
		internal ZArchitecture.ZGrid GuaranteesGrid;
		internal ZArchitecture.GUI.ZGroupBox GuaranteesGroupBox;
		protected ZArchitecture.GUI.ZButton ClearAllButton;
		protected ZArchitecture.GUI.ZButton SelectAllButton;
		private ZArchitecture.GUI.ZPanel ButtonsPanel;
	}
}
