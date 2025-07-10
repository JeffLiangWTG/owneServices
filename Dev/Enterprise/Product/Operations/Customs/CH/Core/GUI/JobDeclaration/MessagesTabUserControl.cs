using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class MessagesTabUserControl : BaseMessagesTabUserControl
{
	public MessagesTabUserControl()
	{
		InitializeComponent();
		SetupInterpretedMessageTextWebBrowser();
		InterpretedMessageTextBox.TextChanged += new EventHandler(InterpretedMessageTextBox_TextChanged);
		if (!DesignModeFinder.IsDesigning)
		{
			UserIdleWorker.QueueWorkItem(this, new MethodInvoker(RefreshDocumentText), null);
		}

		interpretedMessageTextWebBrowser.AllowOverlap(InterpretedMessageTextBox);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1147:DoNotUseWebControls",
		Justification = "To be removed")]
	void SetupInterpretedMessageTextWebBrowser()
	{
		interpretedMessageTextWebBrowser = new ZWebBrowser();

		MessageDetailsTabPage.Controls.Add(interpretedMessageTextWebBrowser);
		MessageDetailsTabPage.Controls.SetChildIndex(interpretedMessageTextWebBrowser, 0);

		interpretedMessageTextWebBrowser.AllowWebBrowserDrop = false;
		interpretedMessageTextWebBrowser.Dock = DockStyle.Fill;
		interpretedMessageTextWebBrowser.Location =
			CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
		interpretedMessageTextWebBrowser.MinimumSize =
			CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
		interpretedMessageTextWebBrowser.Name = "InterpretedMessageTextWebBrowser";
		interpretedMessageTextWebBrowser.ScriptErrorsSuppressed = true;
		interpretedMessageTextWebBrowser.Size =
			CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 533, true);
		interpretedMessageTextWebBrowser.TabIndex = 1;
	}

	void InterpretedMessageTextBox_TextChanged(object sender, EventArgs e)
	{
		RefreshDocumentText();
	}

	void RefreshDocumentText()
	{
		try
		{
			if (!string.IsNullOrEmpty(InterpretedMessageTextBox.Text.Trim()))
			{
				interpretedMessageTextWebBrowser.DocumentText =
					MessageTabUserControlHelper.GetHtmlFormattedText(InterpretedMessageTextBox.Text);
			}
			else
			{
				interpretedMessageTextWebBrowser.DocumentText = "<html/>";
			}
		}
		catch (ArgumentException ex)
		{
			ErrorReporter.ReportOnce("CH-GUI-MessageTextRefresh",
				"Could not refresh web browser text.  Text was: " + InterpretedMessageTextBox.Text, ex);
		}
	}

	protected ZWebBrowser interpretedMessageTextWebBrowser;
}
