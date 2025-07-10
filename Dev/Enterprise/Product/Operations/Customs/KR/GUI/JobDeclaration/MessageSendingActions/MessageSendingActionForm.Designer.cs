
namespace Enterprise.Customs.KR.GUI
{
	partial class MessageSendingActionForm
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
            this.ValidationErrorsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ValidationErrorsTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
            this.ItemsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.messageSendingObjectsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
            this.MessageSendingObjectsGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ValidationErrorsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
            this.SplitContainer.Panel1.SuspendLayout();
            this.SplitContainer.Panel2.SuspendLayout();
            this.SplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // SendButton
            // 
            this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(684, 514, true);
            this.SendButton.TabIndex = 3;
            // 
            // CancelButton2
            // 
            this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(785, 514, true);
            this.CancelButton2.TabIndex = 4;
            // 
            // messageSendingObjectsGroupBox
            // 
            this.messageSendingObjectsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.messageSendingObjectsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.messageSendingObjectsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 126, true);
            // 
            // MessageSendingObjectsGrid
            // 
            this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(875, 107, true);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 540, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 23, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclarationMessageSendingObjectParent);
            // 
            // ValidationErrorsGroupBox
            // 
            this.ValidationErrorsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("70accbe8-3b1c-4be6-91df-1e449a91c93c", "Validation Errors");
            this.ValidationErrorsGroupBox.Controls.Add(this.ValidationErrorsTextBox);
            this.ValidationErrorsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ValidationErrorsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ValidationErrorsGroupBox.Name = "ValidationErrorsGroupBox";
            this.ValidationErrorsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 241, true);
            this.ValidationErrorsGroupBox.TabIndex = 4;
            this.ValidationErrorsGroupBox.TabStop = false;
            // 
            // ValidationErrorsTextBox
            // 
            this.BindingSource.SetBindingMember(this.ValidationErrorsTextBox, "SendingObjectsCollection.BizObjValidationMessageErrors");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclarationMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).BizObjValidationMessageErrors)));
            this.ValidationErrorsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ValidationErrorsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ValidationErrorsTextBox, false);
            this.ValidationErrorsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.ValidationErrorsTextBox.Multiline = true;
            this.ValidationErrorsTextBox.Name = "ValidationErrorsTextBox";
            this.ValidationErrorsTextBox.ReadOnly = true;
            this.ValidationErrorsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.ValidationErrorsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(875, 222, true);
            this.ValidationErrorsTextBox.TabIndex = 2;
            // 
            // SplitContainer
            // 
            this.SplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SplitContainer.Name = "SplitContainer";
            this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // SplitContainer.Panel1
            // 
            this.SplitContainer.Panel1.Controls.Add(this.ItemsGroupBox);
            this.SplitContainer.Panel1.Controls.Add(this.messageSendingObjectsGroupBox);
            this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 512, true);
            this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(67);
            // 
            // SplitContainer.Panel2
            // 
            this.SplitContainer.Panel2.Controls.Add(this.ValidationErrorsGroupBox);
            this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(67);
            this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(268);
            this.SplitContainer.SplitterWidth = 3;
            this.SplitContainer.TabIndex = 0;
            // 
            // ItemsGroupBox
            // 
            this.ItemsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ItemsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 126, true);
            this.ItemsGroupBox.Name = "ItemsGroupBox";
            this.ItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 142, true);
            this.ItemsGroupBox.TabIndex = 2;
            this.ItemsGroupBox.TabStop = false;
            this.ItemsGroupBox.Visible = false;
            // 
            // MessageSendingActionForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 563, true);
            this.Controls.Add(this.SplitContainer);
            this.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclarationMessageSendingObjectParent);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(895, 601, true);
            this.Name = "MessageSendingActionForm";
            this.Text = "MessageSendingActionForm";
            this.Controls.SetChildIndex(this.SplitContainer, 0);
            this.Controls.SetChildIndex(this.SendButton, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.CancelButton2, 0);
            this.messageSendingObjectsGroupBox.ResumeLayout(false);
            this.messageSendingObjectsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).EndInit();
            this.MessageSendingObjectsGrid.ResumeLayout(false);
            this.MessageSendingObjectsGrid.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ValidationErrorsGroupBox.ResumeLayout(false);
            this.ValidationErrorsGroupBox.PerformLayout();
            this.SplitContainer.Panel1.ResumeLayout(false);
            this.SplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
            this.SplitContainer.ResumeLayout(false);
            this.SplitContainer.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		protected CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private ZArchitecture.GUI.ZGroupBox ValidationErrorsGroupBox;
		private ZArchitecture.ZTextBox ValidationErrorsTextBox;
		public ZArchitecture.GUI.ZGroupBox ItemsGroupBox;
	}
}
