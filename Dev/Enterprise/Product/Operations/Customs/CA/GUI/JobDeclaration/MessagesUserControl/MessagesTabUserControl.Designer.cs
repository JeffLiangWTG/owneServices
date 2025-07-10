namespace Enterprise.Customs.CA.GUI
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.HtmlInterpretationBox = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
			this.MessageDetailsEDITabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EDIHtmlInterpretationBox = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
			this.MessageTextEDITabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageTextEDITextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.MessagesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BottomVerticalSplitContainer)).BeginInit();
			this.BottomVerticalSplitContainer.Panel1.SuspendLayout();
			this.BottomVerticalSplitContainer.Panel2.SuspendLayout();
			this.BottomVerticalSplitContainer.SuspendLayout();
			this.MessageTabControl.SuspendLayout();
			this.MessageDetailsTabPage.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageDetailsEDITabPage.SuspendLayout();
			this.MessageTextEDITabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.AfterBind += new System.EventHandler(this.MessagesGrid_AfterBind);
			// 
			// BottomVerticalSplitContainer
			// 
			// 
			// MessageTabControl
			// 
			this.MessageTabControl.Controls.Add(this.MessageDetailsEDITabPage);
			this.MessageTabControl.Controls.Add(this.MessageTextEDITabPage);
			this.MessageTabControl.Controls.SetChildIndex(this.MessageTextEDITabPage, 0);
			this.MessageTabControl.Controls.SetChildIndex(this.MessageDetailsEDITabPage, 0);
			this.MessageTabControl.Controls.SetChildIndex(this.MessageTextTabPage, 0);
			this.MessageTabControl.Controls.SetChildIndex(this.MessageDetailsTabPage, 0);
			// 
			// MessageDetailsTabPage
			// 
			this.MessageDetailsTabPage.Controls.Add(this.HtmlInterpretationBox);
			this.MessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 539, true);
			this.MessageDetailsTabPage.Controls.SetChildIndex(this.InterpretedMessageTextBox, 0);
			this.MessageDetailsTabPage.Controls.SetChildIndex(this.HtmlInterpretationBox, 0);
			// 
			// InterpretedMessageTextBox
			// 
			this.InterpretedMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 533, true);
			this.InterpretedMessageTextBox.Visible = false;
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 539, true);
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 533, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.EDIMessageCollection);
			// 
			// HtmlInterpretationBox
			// 
			this.HtmlInterpretationBox.AllowWebBrowserDrop = false;
			this.BindingSource.SetBindingMember(this.HtmlInterpretationBox, "EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.EDIMessage)(null)).EM_MessageInterpretation)));
			this.HtmlInterpretationBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HtmlInterpretationBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HtmlInterpretationBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.HtmlInterpretationBox.Name = "HtmlInterpretationBox";
			this.HtmlInterpretationBox.ScriptErrorsSuppressed = true;
			this.HtmlInterpretationBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 533, true);
			this.HtmlInterpretationBox.TabIndex = 1;
			// 
			// MessageDetailsEDITabPage
			// 
			this.MessageDetailsEDITabPage.Controls.Add(this.EDIHtmlInterpretationBox);
			this.MessageDetailsEDITabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageDetailsEDITabPage.Name = "MessageDetailsEDITabPage";
			this.MessageDetailsEDITabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageDetailsEDITabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 539, true);
			this.MessageDetailsEDITabPage.TabIndex = 2;
			this.MessageDetailsEDITabPage.Text = "Message Details EDI";
			// 
			// EDIHtmlInterpretationBox
			// 
			this.EDIHtmlInterpretationBox.AllowWebBrowserDrop = false;
			this.BindingSource.SetBindingMember(this.EDIHtmlInterpretationBox, "RawMessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.EDIMessage)(null)).RawMessageInterpretation)));
			this.EDIHtmlInterpretationBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EDIHtmlInterpretationBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EDIHtmlInterpretationBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.EDIHtmlInterpretationBox.Name = "EDIHtmlInterpretationBox";
			this.EDIHtmlInterpretationBox.ScriptErrorsSuppressed = true;
			this.EDIHtmlInterpretationBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 533, true);
			this.EDIHtmlInterpretationBox.TabIndex = 1;
			// 
			// MessageTextEDITabPage
			// 
			this.MessageTextEDITabPage.Controls.Add(this.MessageTextEDITextBox);
			this.MessageTextEDITabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageTextEDITabPage.Name = "MessageTextEDITabPage";
			this.MessageTextEDITabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageTextEDITabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 539, true);
			this.MessageTextEDITabPage.TabIndex = 3;
			this.MessageTextEDITabPage.Text = "Message Text EDI";
			// 
			// MessageTextEDITextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTextEDITextBox, "RawMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.EDIMessage)(null)).RawMessage)));
			this.MessageTextEDITextBox.CaptionResourceString = null;
			this.MessageTextEDITextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageTextEDITextBox, false);
			this.MessageTextEDITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageTextEDITextBox.Multiline = true;
			this.MessageTextEDITextBox.Name = "MessageTextEDITextBox";
			this.MessageTextEDITextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MessageTextEDITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 533, true);
			this.MessageTextEDITextBox.TabIndex = 0;
			// 
			// MessagesTabUserControl
			// 
			this.Name = "MessagesTabUserControl";
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.MessagesGrid.ResumeLayout(false);
			this.MessagesGrid.PerformLayout();
			this.BottomVerticalSplitContainer.Panel1.ResumeLayout(false);
			this.BottomVerticalSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BottomVerticalSplitContainer)).EndInit();
			this.BottomVerticalSplitContainer.ResumeLayout(false);
			this.BottomVerticalSplitContainer.PerformLayout();
			this.MessageTabControl.ResumeLayout(false);
			this.MessageTabControl.PerformLayout();
			this.MessageDetailsTabPage.ResumeLayout(false);
			this.MessageDetailsTabPage.PerformLayout();
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageDetailsEDITabPage.ResumeLayout(false);
			this.MessageDetailsEDITabPage.PerformLayout();
			this.MessageTextEDITabPage.ResumeLayout(false);
			this.MessageTextEDITabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.Messaging.GUI.HtmlInterpretationBox HtmlInterpretationBox;
		protected Enterprise.Messaging.GUI.HtmlInterpretationBox EDIHtmlInterpretationBox;
		protected Enterprise.ZArchitecture.ZTextBox MessageTextEDITextBox;
		internal Enterprise.ZArchitecture.GUI.ZTabPage MessageDetailsEDITabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage MessageTextEDITabPage;
	}
}
