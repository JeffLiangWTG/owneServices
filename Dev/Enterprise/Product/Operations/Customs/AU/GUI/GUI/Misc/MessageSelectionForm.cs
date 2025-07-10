using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class MessageSelectionForm : ZChildForm
	{
		public MessageSelectionForm(MessageAttacheeSelectionCollection collection) : base(collection)
		{
			this.collection = collection;
		}

		public override string FormCaption
		{
			get { return "Entry Selection for " + collection.MessageTypeString; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		public bool IsOKToSend;

		void OKBoundButton_Click(object sender, System.EventArgs e)
		{
			IsOKToSend = collection.GetMessageAttacheesToSendMessagesFor().Length > 0;
			if (collection.GetMessageAttacheesToSendMessagesFor().Length == 0)
			{
				Globals.Message.ShowWarning("There are no entries selected to send a message.", "Cannot send a message");
			}
			else
			{
				Close();
			}
		}

		void CancelBoundButton_Click(object sender, System.EventArgs e)
		{
			IsOKToSend = false;
			Close();
		}

		readonly MessageAttacheeSelectionCollection collection;
	}
}
