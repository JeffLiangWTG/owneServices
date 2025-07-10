using Enterprise.Accounting.Business;
using Enterprise.Messaging.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class LinkedeNettEDIMessageForm : EDIMessageForm
	{
		public LinkedeNettEDIMessageForm(LinkedeNettEDIMessage message)
			: base(message)
		{
			InitializeComponent();
		}

		protected override EDIMessageStandAloneUserControl GetNewMessageDetailUserControl()
		{
			return new LinkedeNettEDIMessageStandAloneUserControl();
		}
	}
}

