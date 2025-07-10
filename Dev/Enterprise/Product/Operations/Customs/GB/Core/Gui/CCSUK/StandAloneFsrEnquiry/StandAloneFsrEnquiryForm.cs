using System;
using System.Windows.Forms;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class StandAloneFsrEnquiryForm : ZTemplateForm, IPreviousNextControlProvider
	{
		public StandAloneFsrEnquiryForm(StandAloneFsrEnquiry standAloneFsrEnquiry)
			: base(standAloneFsrEnquiry)
		{
			MaybeAddReplyButton();
			UserIdleWorker.QueueWorkItem(this, 0, new MethodInvoker(RefreshWebBrowsers), null);
		}

		protected StandAloneFsrEnquiry Enquiry
		{
			get { return (StandAloneFsrEnquiry)BusinessEntity; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			RefreshWebBrowsers();
		}

		void RefreshWebBrowsers()
		{
			zWebBrowser1.DocumentText = Enquiry.EM_MessageInterpretation;
			zWebBrowser2.DocumentText = Enquiry.LinkedMessage == null ? noResponsesForThisObjectHtmlMessage : (string)Enquiry.LinkedMessage.EM_MessageInterpretation;
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}

		protected override bool ShowNotesTab
		{
			get { return false; }
		}

		void MaybeAddReplyButton()
		{
			if (Enquiry.HasLinkedMessage)
			{
				ActionsMenuItem.MenuItems.Add(new ZMenuItem("Query again", QueryAgain_Click));
			}
		}

		void QueryAgain_Click(object sender, EventArgs e)
		{
			var npbo = Enquiry.GetRequeryMessage();
			var manager = new NewStandAloneFsrEnquiryManager(npbo);
			var form = new StandAloneFsrEnquiryFormForNew(manager);
			ZFormModaliser.ShowDialogAndDispose(form);
			Close();
		}

		readonly string noResponsesForThisObjectHtmlMessage = "<html> <body style='font-family: arial;'> <p> No response message was found for this FSR. </p> </body> </html>";
	}
}
