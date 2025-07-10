using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.GUI;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	public partial class CustomsMessagingControl : MessagesUserControl
	{
		public CustomsMessagingControl() : base()
		{
			InitializeComponent();

			MessagesBoundGrid.SetColumnVisible(false, [EDIMessage.Schema.EM_ReceiveTransmit, EDIMessage.Schema.EM_MessageSubType, EDIMessage.Schema.EM_MessageSubTypeDescription, EDIMessage.Schema.EM_SystemCreateTimeUtc, EDIMessage.Schema.EM_InterchangeNumber, EDIMessage.Schema.EM_DateTimeInterchangeSent]);
			MessagesBoundGrid.ReOrderColumns([EDIMessage.Schema.EM_MessageNum, EDIMessage.Schema.EM_MessageType, EDIMessage.Schema.EM_Status, EDIMessage.Schema.EM_User, EDIMessage.Schema.EM_MessageDateTime]);

			MessageDetailsTabPage.Text = Res.GetString("0DA8C608-787E-47D1-A293-34F387D64CEE", "Interpretation");
			HtmlTabPage.Text = Res.GetString("7DBCC4E1-EBFA-440E-90F6-AC953E285817", "Message Interpretation");
			XmlTabPage.Text = Res.GetString("00DB493D-026C-49C8-A0E4-1BB4EFCC28EA", "XML Interpretation");

			XmlInterpretedMessageTextBox.TextChanged += new EventHandler(InterpretedMessageTextBox_TextChanged);
			if (!DesignModeFinder.IsDesigning && !Globals.IsTest)
			{
				UserIdleWorker.QueueWorkItem(this, new MethodInvoker(RefreshDocumentText), null); // updates the web browser control's HTML with the text in the hidden text box, but only when the computer first becomes idle.
			}
			XmlInterpretedMessageTextBox.AllowOverlap(XmlInterpretedMessageTextWebBrowser);
		}

		protected void RefreshDocumentText()
		{
			XmlInterpretedMessageTextWebBrowser.DocumentText = "<html/>";
			var documentText = XmlInterpretedMessageTextBox.Text;
			documentText = documentText.Trim();
			if (!string.IsNullOrEmpty(documentText))
			{
				documentText = EnsureXmlDeclaration(documentText);
				XmlInterpretedMessageTextWebBrowser.DocumentText = documentText;
			}
		}

		void InterpretedMessageTextBox_TextChanged(object sender, EventArgs e)
		{
			RefreshDocumentText();
		}

		static string EnsureXmlDeclaration(string documentText)
		{
			if (!documentText.StartsWith((NoResString)"<?xml"))
			{
				try
				{
					var xmlDocument = new System.Xml.XmlDocument();
					xmlDocument.LoadXml(documentText);
					documentText = (NoResString)"<?xml version=\"1.0\" encoding=\"utf-8\"?>" + documentText;
				}
				catch (Exception ex)
				{
					ErrorReporter.ReportOnce("GUI-MessageTextRefresh", "Could not refresh web browser text. Text was: " + documentText, ex);
				}
			}

			return documentText;
		}
	}
}
