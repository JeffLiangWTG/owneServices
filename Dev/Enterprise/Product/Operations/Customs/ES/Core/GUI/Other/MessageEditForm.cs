using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class MessageEditForm : ZChildForm
	{
		public MessageEditForm()
		{
			InitializeComponent();
		}

		public virtual (ZString, ZBool) EditMessage(ZString messageText)
		{
			ZBool continueWithSend = false;
			ZString result = messageText;
			zTextBoxMessage.Text = messageText;
			if (ZFormModaliser.ShowDialogWithoutDispose(this) == DialogResult.OK)
			{
				result = zTextBoxMessage.Text;
				continueWithSend = true;
			}
			return (result, continueWithSend);
		}
	}
}
