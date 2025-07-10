using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	public partial class EDIInterchangeModificationForm : ZChildForm
	{
		public EDIInterchangeModificationForm(EDIInterchange interchange)
			: base(interchange)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, SaveButton, CloseButton);
		}

		public new EDIInterchange BusinessEntity
		{
			get { return (EDIInterchange)base.BusinessEntity; }
		}
	}
}
