namespace Enterprise.ZArchitecture.GUI
{
	public partial class HyperlinkAlertForm
	{
		#region Component Designer generated code

		public ZLabel HyperlinkFormMessageLabel;
		public ZTextBox HyperlinkFormDetailMessageTextBox;
		public ZButton HyperlinkFormOKButton;
		public ZLinkLabel HyperlinkFormViewDetailLinkLabel;

		protected new void InitializeComponent()
		{
			this.HyperlinkFormMessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.HyperlinkFormViewDetailLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.HyperlinkFormOKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 163, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(636, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.GUI.IHyperlinkAlertBusinessObject);
			// 
			// HyperlinkFormMessageLabel
			// 
			this.BindingSource.SetBindingMember(this.HyperlinkFormMessageLabel, "MessageLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.GUI.IHyperlinkAlertBusinessObject)(null)).MessageLabel)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.HyperlinkFormMessageLabel, false);
			this.HyperlinkFormMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 14, true);
			this.HyperlinkFormMessageLabel.Name = "HyperlinkFormMessageLabel";
			this.HyperlinkFormMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 58, true);
			this.HyperlinkFormMessageLabel.TabIndex = 1;
			// 
			// HyperlinkFormViewDetailLinkLabel
			// 
			this.HyperlinkFormViewDetailLinkLabel.AutoSize = true;
			this.HyperlinkFormViewDetailLinkLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("HyperlinkAlertForm|e0c60287-af6a-40e7-af5f-138b8450eed4", "View Details");
			this.HyperlinkFormViewDetailLinkLabel.IsFontBold = false;
			this.HyperlinkFormViewDetailLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(565, 82, true);
			this.HyperlinkFormViewDetailLinkLabel.Name = "HyperlinkFormViewDetailLinkLabel";
			this.HyperlinkFormViewDetailLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 14, true);
			this.HyperlinkFormViewDetailLinkLabel.TabIndex = 2;
			this.HyperlinkFormViewDetailLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ShowDetailClick);
			// 
			// HyperlinkFormOKButton
			// 
			this.HyperlinkFormOKButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.HyperlinkFormOKButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("HyperlinkAlertForm|36e16c75-1789-474a-9fb7-1e100114f46a", "OK");
			this.HyperlinkFormOKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.HyperlinkFormOKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 129, true);
			this.HyperlinkFormOKButton.Name = "HyperlinkFormOKButton";
			this.HyperlinkFormOKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.HyperlinkFormOKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 23, true);
			this.HyperlinkFormOKButton.TabIndex = 4;
			this.HyperlinkFormOKButton.UseVisualStyleBackColor = true;
			this.HyperlinkFormOKButton.Click += new System.EventHandler(this.OKButtonClick);
			// 
			// HyperlinkAlertForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(636, 187, true);
			this.Controls.Add(this.HyperlinkFormOKButton);
			this.Controls.Add(this.HyperlinkFormViewDetailLinkLabel);
			this.Controls.Add(this.HyperlinkFormMessageLabel);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.GUI.IHyperlinkAlertBusinessObject);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "HyperlinkAlertForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.HyperlinkFormMessageLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.HyperlinkFormViewDetailLinkLabel, 0);
			this.Controls.SetChildIndex(this.HyperlinkFormOKButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
