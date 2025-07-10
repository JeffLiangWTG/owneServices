using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public partial class ReasonForShortageMessageSendingForm : EMCSMessageSendingForm<ReasonForShortageSendingAction>
	{
		public ReasonForShortageMessageSendingForm(ReasonForShortageSendingActionParent parent) : base(parent)
		{
			InitializeComponent();
		}

		protected override ZUserControl GetBottomSectionUserControl() => new ReasonForShortageBottomSectionUserControl();
	}
}
