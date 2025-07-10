using System;
using System.Windows.Forms;
using Enterprise.Customs.GB.Chief.Messaging.DLU;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.DLU
{
	public partial class DLUMessageForm : ZTemplateForm, IPreviousNextControlProvider
	{
		public DLUMessageForm(DLUMessage dluMessage)
			: base(dluMessage)
		{
			InitializeComponent();
			UserIdleWorker.QueueWorkItem(this, 0, new MethodInvoker(RefreshWebBrowsers), null);
		}

		protected DLUMessage DLUMessage
		{
			get { return (DLUMessage)BusinessEntity; }
		}

		void InterpretedMessageTextBox_TextChanged(object sender, EventArgs e)
		{
			RefreshWebBrowsers();
		}

		void MessageTextTextBox_TextChanged(object sender, EventArgs e)
		{
			RefreshWebBrowsers();
		}

		void RefreshWebBrowsers()
		{
			zWebBrowser1.DocumentText = DLUMessage.EM_MessageInterpretation;
			zWebBrowser2.DocumentText = DLUMessage.LinkedMessage == null ? noResponsesForThisObjectHtmlMessage : (string)DLUMessage.LinkedMessage.EM_MessageInterpretation;
		}

		void ContrlTextBox_TextChanged(object sender, EventArgs e)
		{
			RefreshWebBrowsers();
		}

		readonly string noResponsesForThisObjectHtmlMessage = "<html> <body style='font-family: arial;'> <p> No response message was found for this DLU. </p> </body> </html>";
	}
}
