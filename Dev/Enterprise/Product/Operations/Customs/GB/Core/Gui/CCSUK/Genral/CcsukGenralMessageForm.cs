using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class CcsukGenralMessageForm : ZTemplateForm, IPreviousNextControlProvider
	{
		public CcsukGenralMessageForm(GenralEdiMessage genralEdiMessage)
			: base(genralEdiMessage)
		{
			UserIdleWorker.QueueWorkItem(this, 0, new MethodInvoker(RefreshWebBrowsers), null);
			MaybeAddReplyButton();
		}

		protected GenralEdiMessage GenralEdiMessage
		{
			get { return (GenralEdiMessage)BusinessEntity; }
		}

		void MaybeAddReplyButton()
		{
			if (GenralEdiMessage.EM_ReceiveTransmit == EDIMessage.Direction.Receive)
			{
				ActionsMenuItem.MenuItems.Add(new ZMenuItem("Reply to sender", Reply_Click));
			}
		}

		void Reply_Click(object sender, EventArgs e)
		{
			var newMessage = GenralEdiMessage.GetReplyMessage();
			var manager = new NewGenralMessageManager(newMessage);
			var form = new CcsukGenralMessageFormForNew(manager);
			ZFormModaliser.ShowDialogAndDispose(form);
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
			zWebBrowser1.DocumentText = GenralEdiMessage.EM_MessageInterpretation;
			zWebBrowser2.DocumentText = GenralEdiMessage.LinkedMessage == null ? noContrlsForThisObjectHtmlMessage : (string)GenralEdiMessage.LinkedMessage.EM_MessageInterpretation;
		}

		void ContrlTextBox_TextChanged(object sender, EventArgs e)
		{
			RefreshWebBrowsers();
		}

		protected override bool SupportsEDocs
		{
			get
			{
				return false;
			}
		}

		public override Size MinimumSize
		{
			get { return CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 342); }
			set { base.MinimumSize = value; }
		}

		readonly string noContrlsForThisObjectHtmlMessage = "<html> <body style='font-family: arial;'> <p> No CONTRL messages or other linked objects were found for this GENRAL. </p> </body> </html>";
	}
}
