using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	public partial class CustomsMessagingControl
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

		void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.HorizontalSplitter = new CargoWise.Windows.UI.KSplitter();
            this.XmlInterpretedMessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.XmlInterpretedMessageTextWebBrowser = new Enterprise.ZArchitecture.GUI.ZWebBrowser();
            this.InnerMessageDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
            this.HtmlTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.XmlTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MessagesGroupBox.SuspendLayout();
            this.MessageTabControl.SuspendLayout();
            this.MessageDetailsTabPage.SuspendLayout();
            this.MessageTextTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessagesBoundGrid)).BeginInit();
            this.MessagesBoundGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.InnerMessageDetailsTabControl.SuspendLayout();
            this.HtmlTabPage.SuspendLayout();
            this.XmlTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // MessagesGroupBox
            // 
            this.MessagesGroupBox.Controls.Add(this.HorizontalSplitter);
            this.MessagesGroupBox.Controls.SetChildIndex(this.MessageTabControl, 0);
            this.MessagesGroupBox.Controls.SetChildIndex(this.VerticalSplitter, 0);
            this.MessagesGroupBox.Controls.SetChildIndex(this.HorizontalSplitter, 0);
            this.MessagesGroupBox.Controls.SetChildIndex(this.MessagesBoundGrid, 0);
            // 
            // VerticalSplitter
            // 
            this.VerticalSplitter.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.VerticalSplitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.VerticalSplitter.Enabled = false;
            this.VerticalSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(870, 16, true);
            this.VerticalSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 125, true);
            this.VerticalSplitter.Visible = false;
            // 
            // MessageTabControl
            // 
            this.MessageTabControl.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.MessageTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 141, true);
            this.MessageTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(870, 365, true);
            // 
            // MessageDetailsTabPage
            // 
            this.MessageDetailsTabPage.Controls.Add(this.InnerMessageDetailsTabControl);
            this.MessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 338, true);
			this.MessageDetailsTabPage.Controls.SetChildIndex(this.InnerMessageDetailsTabControl, 0);
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 338, true);
            // 
            // MessageTextTextBox
            // 
            this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(856, 332, true);
            // 
            // MessagesBoundGrid
            // 
            this.MessagesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(867, 122, true);
            // 
            // HorizontalSplitter
            // 
            this.HorizontalSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.HorizontalSplitter.DoNotSaveSplitterLayout = false;
            this.HorizontalSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 138, true);
            this.HorizontalSplitter.Name = "HorizontalSplitter";
            this.HorizontalSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(867, 3, true);
            this.HorizontalSplitter.TabIndex = 1;
            this.HorizontalSplitter.TabStop = false;
            // 
            // XmlInterpretedMessageTextBox
            // 
            this.BindingSource.SetBindingMember(this.XmlInterpretedMessageTextBox, "Messages.EM_FormattedMessageText");
            this.XmlInterpretedMessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.XmlInterpretedMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.XmlInterpretedMessageTextBox.Name = "XmlInterpretedMessageTextBox";
            this.XmlInterpretedMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1, 20, true);
            this.XmlInterpretedMessageTextBox.TabIndex = 2;
            // 
            // XmlInterpretedMessageTextWebBrowser
            // 
            this.XmlInterpretedMessageTextWebBrowser.AllowWebBrowserDrop = false;
            this.XmlInterpretedMessageTextWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.XmlInterpretedMessageTextWebBrowser.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.XmlInterpretedMessageTextWebBrowser.Name = "XmlInterpretedMessageTextWebBrowser";
            this.XmlInterpretedMessageTextWebBrowser.ScriptErrorsSuppressed = true;
            this.XmlInterpretedMessageTextWebBrowser.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 305, true);
            this.XmlInterpretedMessageTextWebBrowser.TabIndex = 1;
            // 
            // InnerMessageDetailsTabControl
            // 
            this.InnerMessageDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.InnerMessageDetailsTabControl.Controls.Add(this.HtmlTabPage);
            this.InnerMessageDetailsTabControl.Controls.Add(this.XmlTabPage);
            this.InnerMessageDetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InnerMessageDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.InnerMessageDetailsTabControl.Name = "InnerMessageDetailsTabControl";
            this.InnerMessageDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(856, 332, true);
            this.InnerMessageDetailsTabControl.TabIndex = 3;
			// 
			// HtmlTabPage
			// 
			this.HtmlTabPage.Controls.Add(base.HtmlInterpretationBox);
			this.HtmlTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 22, true);
			this.HtmlTabPage.Name = "HtmlTabPage";
			this.HtmlTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.HtmlTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 312, true);
			this.HtmlTabPage.TabIndex = 0;
			this.HtmlTabPage.Text = "HtmlTabPage";
			this.HtmlTabPage.UseVisualStyleBackColor = true;
			// 
			// XmlTabPage
			// 
			this.XmlTabPage.Controls.Add(this.XmlInterpretedMessageTextBox);
            this.XmlTabPage.Controls.Add(this.XmlInterpretedMessageTextWebBrowser);
            this.XmlTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.XmlTabPage.Name = "XmlTabPage";
            this.XmlTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.XmlTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(854, 311, true);
            this.XmlTabPage.TabIndex = 1;
            this.XmlTabPage.Text = "XmlTabPage";
            this.XmlTabPage.UseVisualStyleBackColor = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // CustomsMessagingControl
            // 
            this.Name = "CustomsMessagingControl";
            this.Controls.SetChildIndex(this.MessagesGroupBox, 0);
            this.MessagesGroupBox.ResumeLayout(false);
            this.MessagesGroupBox.PerformLayout();
            this.MessageTabControl.ResumeLayout(false);
            this.MessageTabControl.PerformLayout();
            this.MessageDetailsTabPage.ResumeLayout(false);
            this.MessageDetailsTabPage.PerformLayout();
            this.MessageTextTabPage.ResumeLayout(false);
            this.MessageTextTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessagesBoundGrid)).EndInit();
            this.MessagesBoundGrid.ResumeLayout(false);
            this.MessagesBoundGrid.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.InnerMessageDetailsTabControl.ResumeLayout(false);
            this.InnerMessageDetailsTabControl.PerformLayout();
            this.HtmlTabPage.ResumeLayout(false);
            this.HtmlTabPage.PerformLayout();
            this.XmlTabPage.ResumeLayout(false);
            this.XmlTabPage.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private KSplitter HorizontalSplitter;
		protected Enterprise.ZArchitecture.GUI.ZWebBrowser XmlInterpretedMessageTextWebBrowser;
		protected Enterprise.ZArchitecture.ZTextBox XmlInterpretedMessageTextBox;
		private ContextMenuStrip contextMenuStrip1;
		private ZTabControl InnerMessageDetailsTabControl;
		private ZTabPage HtmlTabPage;
		private ZTabPage XmlTabPage;
	}
}
