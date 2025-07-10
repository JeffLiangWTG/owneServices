using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public partial class MessagesUserControl : ZUserControl
	{
		public MessagesUserControl()
		{
			InitializeComponent();
			InterpretedMessageTextBox.TextChanged += new EventHandler(InterpretedMessageTextBox_TextChanged);
			if (!DesignModeFinder.IsDesigning)
			{
				UserIdleWorker.QueueWorkItem(this, new MethodInvoker(RefreshMessageInterpretationWebBrowser), null); // updates the web browser control's HTML with the text in the hidden text box, but only when the computer first becomes idle.
			}

			InterpretedMessageWebBrowser.AllowOverlap(InterpretedMessageTextBox);
		}

		void InterpretedMessageTextBox_TextChanged(object sender, EventArgs e)
		{
			RefreshMessageInterpretationWebBrowser();
		}

		void RefreshMessageInterpretationWebBrowser()
		{
			try
			{
				var webPageDocument = InterpretedMessageWebBrowser.Document?.OpenNew(true);
				if (webPageDocument != null)
				{
					webPageDocument.Write(InterpretedMessageTextBox.Text.Trim());
					InterpretedMessageWebBrowser.Refresh();
				}
			}
			catch (ArgumentException ex)
			{
				ErrorReporter.ReportOnce("EU-EMCS-GUI-MessageTextRefresh", "Could not refresh web browser text.  Text was: " + InterpretedMessageTextBox.Text, ex);
			}
		}
	}
}
