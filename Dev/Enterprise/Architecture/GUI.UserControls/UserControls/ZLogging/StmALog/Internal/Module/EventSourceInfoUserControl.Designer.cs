
namespace Enterprise.ZArchitecture.GUI
{
	partial class EventSourceInfoUserControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.sourceInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.showMessageButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ediMessageInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.messageSenderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.messageSubTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.messageNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.sourceInfoGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.ediMessageInfoGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.StmALog);
			// 
			// sourceInfoGroupBox
			// 
			this.sourceInfoGroupBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("EventSourceInfoUserControl|6dc4ad0d-a88c-469c-b65c-5263be19ffef", "Event Context Information", "Additional information provided along with an Event when it comes from an external source.");
			this.sourceInfoGroupBox.Controls.Add(this.zGrid1);
			this.sourceInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sourceInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.sourceInfoGroupBox.Name = "sourceInfoGroupBox";
			this.sourceInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(702, 177, true);
			this.sourceInfoGroupBox.TabIndex = 1;
			this.sourceInfoGroupBox.TabStop = false;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, "SourceInfoItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.StmALog)(null)).SourceInfoItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.KeyDataPair)(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.StmALog)(null)).SourceInfoItems)).SyncRoot)).Key)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.KeyDataPair)(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.StmALog)(null)).SourceInfoItems)).SyncRoot)).Data)));
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("EventSourceInfoUserControl|985239b6-c599-46e4-9337-8bf4f9ba2f57", "Key", "Key identifying the Event Context Information.");
			zTextBoxColumnStyleInfo1.ColumnName = "Key";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(215);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("EventSourceInfoUserControl|ab3a0584-00d3-456e-b0d5-f1f5aa970fce", "Value", "Value from the Event Context Information.");
			zTextBoxColumnStyleInfo2.ColumnName = "Data";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(450);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.DataSource = this.BindingSource;
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.GridId = "4b958a46-bf29-4d21-8bbf-4c7cae1ad9de";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.ReadOnly = true;
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 158, true);
			this.zGrid1.TabIndex = 0;
			// 
			// showMessageButton
			// 
			this.showMessageButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("EventSourceInfoUserControl|78894b5b-e933-418e-bf23-c26faf1a9821", "View", "Brings up the related EDI Message of the selected Event.");
			this.showMessageButton.EditableInViewMode = true;
			this.showMessageButton.IsCaptionOverridden = false;
			this.showMessageButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 11, true);
			this.showMessageButton.Name = "showMessageButton";
			this.showMessageButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.showMessageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 21, true);
			this.showMessageButton.TabIndex = 1;
			this.showMessageButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.showMessageButton.ToolTipCaption = null;
			this.showMessageButton.UseVisualStyleBackColor = true;
			this.showMessageButton.Click += new System.EventHandler(this.ShowMessageButton_Click);
			// 
			// ediMessageInfoGroupBox
			// 
			this.ediMessageInfoGroupBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("EventSourceInfoUserControl|3fa067e4-09b8-4173-8e10-65c356ee8364", "EDI Message Information");
			this.ediMessageInfoGroupBox.Controls.Add(this.messageSenderTextBox);
			this.ediMessageInfoGroupBox.Controls.Add(this.messageSubTypeTextBox);
			this.ediMessageInfoGroupBox.Controls.Add(this.messageNumberTextBox);
			this.ediMessageInfoGroupBox.Controls.Add(this.showMessageButton);
			this.ediMessageInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.ediMessageInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(702, 0, true);
			this.ediMessageInfoGroupBox.Name = "ediMessageInfoGroupBox";
			this.ediMessageInfoGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.ediMessageInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 177, true);
			this.ediMessageInfoGroupBox.TabIndex = 2;
			this.ediMessageInfoGroupBox.TabStop = false;
			// 
			// messageSenderTextBox
			// 
			this.BindingSource.SetBindingMember(this.messageSenderTextBox, "RelatedEDIMessage+Sender");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.StmALog)(null)).RelatedEDIMessage.Sender)));
			this.messageSenderTextBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("EventSourceInfoUserControl|1f89bf2e-62be-4973-8c1e-1ed4533b50bd", "Sender ID");
			this.messageSenderTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.messageSenderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 80, true);
			this.messageSenderTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.messageSenderTextBox.Name = "messageSenderTextBox";
			this.messageSenderTextBox.ReadOnly = true;
			this.messageSenderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 20, true);
			this.messageSenderTextBox.TabIndex = 4;
			// 
			// messageSubTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.messageSubTypeTextBox, "RelatedEDIMessage+Message+MessageSubTypeWithDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.StmALog)(null)).RelatedEDIMessage.Message.MessageSubTypeWithDescription)));
			this.messageSubTypeTextBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("EventSourceInfoUserControl|eabf29fe-2c60-4551-bf1a-e16e95895062", "Data Type");
			this.messageSubTypeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.messageSubTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 58, true);
			this.messageSubTypeTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.messageSubTypeTextBox.Name = "messageSubTypeTextBox";
			this.messageSubTypeTextBox.ReadOnly = true;
			this.messageSubTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 20, true);
			this.messageSubTypeTextBox.TabIndex = 3;
			// 
			// messageNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.messageNumberTextBox, "RelatedEDIMessage+Message+EM_MessageNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.StmALog)(null)).RelatedEDIMessage.Message.EM_MessageNum)));
			this.messageNumberTextBox.CaptionResourceString = null;
			this.messageNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.messageNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 36, true);
			this.messageNumberTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.messageNumberTextBox.Name = "messageNumberTextBox";
			this.messageNumberTextBox.ReadOnly = true;
			this.messageNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 20, true);
			this.messageNumberTextBox.TabIndex = 2;
			// 
			// EventSourceInfoUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.sourceInfoGroupBox);
			this.Controls.Add(this.ediMessageInfoGroupBox);
			this.Name = "EventSourceInfoUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(948, 177, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.sourceInfoGroupBox.ResumeLayout(false);
			this.sourceInfoGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.ediMessageInfoGroupBox.ResumeLayout(false);
			this.ediMessageInfoGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZGrid zGrid1;
		private ZGroupBox sourceInfoGroupBox;
		protected ZButton showMessageButton;
		private ZGroupBox ediMessageInfoGroupBox;
		private ZTextBox messageNumberTextBox;
		private ZTextBox messageSenderTextBox;
		private ZTextBox messageSubTypeTextBox;
	}
}
