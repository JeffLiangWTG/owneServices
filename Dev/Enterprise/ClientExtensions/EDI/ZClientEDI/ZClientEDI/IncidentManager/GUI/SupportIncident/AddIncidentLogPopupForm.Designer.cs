namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class AddIncidentLogPopupForm : BaseIncidentPopupForm
	{
		public readonly Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentLogCommentAction SupportIncidentLogCommentAction;
		public readonly SupportIncidentForm RelatedSupportIncidentForm;
		protected ZArchitecture.ZTextBox zTextBox1;
		private ZArchitecture.GUI.ZCheckBox notifyInternalSubscribersCheckBox;

		protected override void InitializeComponent()
		{
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.notifyInternalSubscribersCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// MessageLabel
			//
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 28, true);
			this.MessageLabel.Text = "Please specify a internal comment to the incident. All emails and file attachments should be added through the eDocs tab.";
			//
			// CloseButton
			//
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 216, true);
			this.CloseButton.TabIndex = 3;
			//
			// CancelButtonX
			//
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(305, 216, true);
			this.CancelButtonX.TabIndex = 4;
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 245, true);
			this.MainStatusBar.TabIndex = 5;
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(221);
			//
			// ErrorStatusBarPanel
			//
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(221);
			//
			// zTextBox1
			//
			this.zTextBox1.AcceptsReturn = true;
			this.zTextBox1.AcceptsTab = true;
			this.zTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zTextBox1, "Comment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentLogCommentAction)(null)).Comment)));
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 50, true);
			this.zTextBox1.Multiline = true;
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 145, true);
			this.zTextBox1.TabIndex = 1;
			//
			// notifyInternalSubscribersCheckBox
			//
			this.notifyInternalSubscribersCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.notifyInternalSubscribersCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.notifyInternalSubscribersCheckBox, "NeedNotifyInternal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentLogCommentAction)(null)).NeedNotifyInternal)));
			this.notifyInternalSubscribersCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.notifyInternalSubscribersCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 198, true);
			this.notifyInternalSubscribersCheckBox.Name = "notifyInternalSubscribersCheckBox";
			this.notifyInternalSubscribersCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 14, true);
			this.notifyInternalSubscribersCheckBox.TabIndex = 2;
			this.notifyInternalSubscribersCheckBox.Text = "Notify internal subscribers";
			this.notifyInternalSubscribersCheckBox.UseVisualStyleBackColor = true;
			//
			// AddIncidentLogPopupForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 269, true);
			this.Controls.Add(this.notifyInternalSubscribersCheckBox);
			this.Controls.Add(this.zTextBox1);
			this.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentLogCommentAction);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 200, true);
			this.Name = "AddIncidentLogPopupForm";
			this.Text = "Add Internal Log";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(this.zTextBox1, 0);
			this.Controls.SetChildIndex(this.notifyInternalSubscribersCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}
