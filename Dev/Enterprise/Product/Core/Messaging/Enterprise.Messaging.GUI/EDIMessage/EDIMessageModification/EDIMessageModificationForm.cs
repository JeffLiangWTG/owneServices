using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	public partial class EDIMessageModificationForm : ZChildForm
	{
		public EDIMessageModificationForm(EDIMessage message)
			: base(message)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, SaveButton, CloseButton);
		}

		public new EDIMessage BusinessEntity
		{
			get { return (EDIMessage)base.BusinessEntity; }
		}
	}
}
