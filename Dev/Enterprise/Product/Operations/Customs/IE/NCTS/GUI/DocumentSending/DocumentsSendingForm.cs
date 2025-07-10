using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.NCTS.GUI
{
	public partial class DocumentsSendingForm : MessageSendingObjectForm
	{
		public DocumentsSendingForm(DocumentSendingActionParent parent) : base(parent)
		{
			this.parent = parent;
			this.header = parent.Header;
		}
		readonly NctsHeader header;
		readonly DocumentSendingActionParent parent;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormHeading => Res.GetString("15C3B4A8-0EA5-4B0D-90A4-1DD97E034DC3", "Upload Supporting Documents");

		public static void ShowForm(NctsHeader header)
		{
			var parent = new DocumentSendingActionParent(header);
			using (var messageSendingForm = new DocumentsSendingForm(parent))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm) == DialogResult.OK)
				{
					messageSendingForm.SendToCustoms();
				}
			}
		}

		void SendToCustoms()
		{
			var messageSent = 0;
			parent.SendingObjectsCollection.Cast<DocumentSendingAction>().Where(action => action.ShouldSend).ForEach(
				action =>
				{
					var sender = action.CreateSender();
					if (sender.Send() != null)
					{
						messageSent++;
					}
				});
			if (messageSent > 0)
			{
				TrySaveAndShowMessage(header.Factory, messageSent);
			}
		}

		void TrySaveAndShowMessage(BusinessObjectFactory factory, int messagesCreated)
		{
			try
			{
				factory.Save();
				Globals.Message.ShowInformation(GetMessageSentText(messagesCreated));
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		string GetMessageSentText(int count)
		{
			return Res.GetString("0B267F92-45B0-4935-9639-A96ABB43BC47", "{0} message(s) queued for sending.", count);
		}
	}
}
