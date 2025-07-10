using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public partial class MessagesTabUserControl : Customs.GUI.BaseMessagesTabUserControl, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public MessagesTabUserControl()
		{
			InitializeComponent();
			InterpretedMessageTextBox.TextChanged += InterpretedMessageTextBox_TextChanged;
			UserIdleWorker.QueueWorkItem(this, new MethodInvoker(RefreshDocumentText), null); // updates the web browser control's HTML with the text in the hidden text box, but only when the computer first becomes idle.

			InterpretedMessageTextWebBrowser.AllowOverlap(InterpretedMessageTextBox);
		}

		void InterpretedMessageTextBox_TextChanged(object sender, EventArgs e)
		{
			RefreshDocumentText();
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return (control.Name == "InterpretedMessageTextWebBrowser" && previousControl.Name == "InterpretedMessageTextBox")
					|| (previousControl.Name == "InterpretedMessageTextWebBrowser" && control.Name == "InterpretedMessageTextBox");
		}

		void RefreshDocumentText()
		{
			try
			{
				InterpretedMessageTextWebBrowser.DocumentText = !string.IsNullOrEmpty(InterpretedMessageTextBox.Text.Trim())
					? InterpretedMessageTextBox.Text
					: "<html/>";
			}
			catch (ArgumentException ex)
			{
				ErrorReporter.ReportOnce("CN-GUI-MessageTextRefresh", "Could not refresh web browser text.  Text was: " + InterpretedMessageTextBox.Text, ex);
			}
		}
	}
}
